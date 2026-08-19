using Discord;
using Discord.WebSocket;
using Hogs.RPG.Core.Entities.ColosseumObjects;
using Hogs.RPG.Core.Enums;
using Hogs.RPG.Data.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Hogs.RPG.Services.ColosseumServices
{
    /// <summary>
    /// Drives the whole Colosseum lifecycle on a timer, same shape as
    /// BossScheduler: a BackgroundService ticking every 30s, resolving
    /// scoped services per-tick via IServiceScopeFactory since this class
    /// itself is a long-lived singleton.
    ///
    /// Responsibilities per tick, each independent and wrapped in its own
    /// try/catch so one failing doesn't block the others:
    ///   1. Open a new tournament once a day at OpenHourUtc (also cleans up
    ///      the previous tournament's thread first)
    ///   2. Warn the RPG feed 30 minutes before registration closes
    ///   3. Close registration + seed the bracket once RegistrationEndsAt passes
    ///   4. Resolve every currently-ready match in any InProgress tournament,
    ///      one round at a time, pausing SecondsPerRound between rounds.
    ///      Each match's combat log + individual result posts as a colored
    ///      embed in the single shared tournament thread (created lazily on
    ///      first sight of an InProgress tournament). The main Colosseum
    ///      channel instead gets ONE combined embed per round - grouped by
    ///      bracket type + round number - so it reads as a clean sequence of
    ///      stages ("Winner Bracket Round 1" -> "Loser Bracket Round 1" ->
    ///      "Winner Bracket Round 2" -> ...) rather than a flood of
    ///      individual match results.
    /// </summary>
    public class ColosseumScheduler : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly DiscordSocketClient _client;

        // Dedicated Colosseum channel - daily open/close announcements and
        // per-round result summaries post here, and this is the parent
        // channel the tournament thread gets created under.
        private readonly ulong _announceChannelId = 1536112830967709776;

        // Final results (winner + runner-up) and the 30-minute warning also
        // cross-post to the main RPG feed, since that's higher-traffic than
        // the dedicated Colosseum channel and most players won't be
        // watching the tournament thread.
        private readonly ulong _rpgFeedChannelId = 1485357755433750549;

        // Tournament opens once a day at this UTC hour. Change freely.
        private const int OpenHourUtc = 12;

        // Pause between bracket rounds once a tournament is InProgress -
        // gives each round's messages a moment to land before the next
        // wave starts, and gives spectators something closer to a real
        // event pace instead of the whole bracket resolving instantly.
        private const int SecondsPerRound = 20;

        private const int MaxDiscordRetries = 3;
        private const int DiscordRetryDelaySeconds = 5;

        // Embed descriptions cap at 4096 chars. Condensed combat logs should
        // never get close to this, but chunk into multiple embeds as a
        // fallback just in case a very long fight slips past truncation.
        private const int MaxEmbedDescriptionLength = 4000;

        private DateTime _lastOpenedDate = DateTime.MinValue;

        // Tracks which tournament ids have already gotten their 30-minute
        // warning, so the 30s tick loop doesn't re-send it repeatedly while
        // inside that window. In-memory only (same trade-off BossScheduler
        // makes with _spawnedToday/_preWarnedToday) - a bot restart inside
        // the warning window could in theory cause a duplicate send, which
        // felt acceptable for a once-a-day announcement.
        private readonly HashSet<int> _thirtyMinuteWarningsSent = new();

        public ColosseumScheduler(IServiceScopeFactory scopeFactory, DiscordSocketClient client)
        {
            _scopeFactory = scopeFactory;
            _client = client;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("🏛️ ColosseumScheduler started");

            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.UtcNow;

                try { await CheckAndOpenDailyTournamentAsync(now); }
                catch (Exception ex) { Console.WriteLine($"❌ Colosseum open-check failed: {ex.Message}"); }

                try { await CheckAndSendThirtyMinuteWarningAsync(now); }
                catch (Exception ex) { Console.WriteLine($"❌ Colosseum 30-minute warning check failed: {ex.Message}"); }

                try { await CheckAndCloseRegistrationAsync(now); }
                catch (Exception ex) { Console.WriteLine($"❌ Colosseum close-registration check failed: {ex.Message}"); }

                try { await ProcessInProgressTournamentAsync(stoppingToken); }
                catch (Exception ex) { Console.WriteLine($"❌ Colosseum bracket processing failed: {ex.Message}"); }

                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }

        // =========================
        // 1. OPEN DAILY TOURNAMENT
        // =========================
        private async Task CheckAndOpenDailyTournamentAsync(DateTime now)
        {
            if (_lastOpenedDate.Date == now.Date) return;
            if (now.Hour != OpenHourUtc || now.Minute >= 5) return;

            using var scope = _scopeFactory.CreateScope();
            var colosseumService = scope.ServiceProvider.GetRequiredService<ColosseumService>();
            var colosseumRepo = scope.ServiceProvider.GetRequiredService<ColosseumRepository>();

            // Safety net: don't open a second one if something already exists
            // in Registration/Locked/InProgress (e.g. after a restart).
            var existing = await colosseumRepo.GetActiveRegistrationAsync();
            if (existing != null)
            {
                _lastOpenedDate = now.Date;
                return;
            }

            // Clean up every previous tournament's thread before opening
            // today's - keeps the dedicated Colosseum channel from
            // accumulating stale threads day after day.
            var completedTournaments = await colosseumRepo.GetCompletedTournamentsAsync();
            foreach (var completed in completedTournaments)
                await CleanupTournamentThreadAsync(completed);

            var tournament = await colosseumService.OpenRegistrationAsync(_announceChannelId);
            _lastOpenedDate = now.Date;

            Console.WriteLine($"🏛️ Opened Colosseum tournament {tournament.Id} - registration closes {tournament.RegistrationEndsAt:u}");

            var channel = _client.GetChannel(_announceChannelId) as IMessageChannel;
            if (channel == null) return;

            var embed = new EmbedBuilder()
                .WithTitle("🏛️ The Colosseum is open!")
                .WithDescription(
                    $"Sign up with `/colosseum` — buy-in is **{tournament.BuyInGold} gold**.\n" +
                    $"You'll get a DM to build your loadout with **{tournament.BuildBudgetAP} Arena Points**.\n\n" +
                    $"Registration closes <t:{new DateTimeOffset(tournament.RegistrationEndsAt).ToUnixTimeSeconds()}:R> — " +
                    $"any unfinished build gets randomized, same as the bots filling the rest of the bracket.\n\n" +
                    $"🥇 Winner: **{tournament.WinnerPrizeGold} gold** · 🥈 Runner-up: **{tournament.RunnerUpPrizeGold} gold**")
                .WithColor(new Color(0xC0392B))
                .Build();

            await SendWithRetryAsync(() => channel.SendMessageAsync(embed: embed), "tournament-open announcement");
        }

        // =========================
        // 2. 30-MINUTE WARNING (RPG FEED)
        // =========================
        private async Task CheckAndSendThirtyMinuteWarningAsync(DateTime now)
        {
            using var scope = _scopeFactory.CreateScope();
            var colosseumRepo = scope.ServiceProvider.GetRequiredService<ColosseumRepository>();

            var tournament = await colosseumRepo.GetActiveRegistrationAsync();
            if (tournament == null) return;
            if (_thirtyMinuteWarningsSent.Contains(tournament.Id)) return;

            var warnAt = tournament.RegistrationEndsAt.AddMinutes(-30);
            if (now < warnAt || now >= tournament.RegistrationEndsAt) return;

            _thirtyMinuteWarningsSent.Add(tournament.Id);

            var realCount = tournament.Participants.Count(p => !p.IsBot);

            var channel = _client.GetChannel(_rpgFeedChannelId) as IMessageChannel;
            if (channel == null) return;

            var embed = new EmbedBuilder()
                .WithTitle("🏛️ Colosseum starting in 30 minutes!")
                .WithDescription(
                    $"{realCount}/{tournament.MaxRealPlayers} players signed up. Last chance - " +
                    $"use `/colosseum` to sign up before builds lock in.")
                .WithColor(new Color(0xE67E22))
                .Build();

            await SendWithRetryAsync(() => channel.SendMessageAsync(embed: embed), "30-minute warning (rpg feed)");
        }

        // =========================
        // 3. CLOSE REGISTRATION + SEED BRACKET
        // =========================
        private async Task CheckAndCloseRegistrationAsync(DateTime now)
        {
            using var scope = _scopeFactory.CreateScope();
            var colosseumService = scope.ServiceProvider.GetRequiredService<ColosseumService>();
            var colosseumRepo = scope.ServiceProvider.GetRequiredService<ColosseumRepository>();

            var tournament = await colosseumRepo.GetActiveRegistrationAsync();
            if (tournament == null || now < tournament.RegistrationEndsAt) return;

            Console.WriteLine($"🏛️ Closing registration for tournament {tournament.Id}...");

            await colosseumService.StartTournamentAsync(tournament.Id);

            Console.WriteLine($"🏛️ Tournament {tournament.Id} bracket seeded, now InProgress.");

            var channel = _client.GetChannel(_announceChannelId) as IMessageChannel;
            if (channel == null) return;

            var embed = new EmbedBuilder()
                .WithTitle("🏛️ The bracket is set!")
                .WithDescription("Fighters enter. Matches are resolving now — the tournament thread will appear shortly.")
                .WithColor(new Color(0xC0392B))
                .Build();

            await SendWithRetryAsync(() => channel.SendMessageAsync(embed: embed), "bracket-seeded announcement");
        }

        // =========================
        // 4. RESOLVE READY MATCHES
        // =========================
        private async Task ProcessInProgressTournamentAsync(CancellationToken stoppingToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var colosseumRepo = scope.ServiceProvider.GetRequiredService<ColosseumRepository>();
            var colosseumService = scope.ServiceProvider.GetRequiredService<ColosseumService>();
            var bracketService = scope.ServiceProvider.GetRequiredService<ColosseumBracketService>();
            var combatService = scope.ServiceProvider.GetRequiredService<ColosseumCombatService>();
            var playerRepo = scope.ServiceProvider.GetRequiredService<PlayerRepository>();

            var tournament = await colosseumRepo.GetInProgressTournamentAsync();
            if (tournament == null) return;

            var parentChannel = _client.GetChannel(tournament.AnnounceChannelId) as ITextChannel;

            // Lazily create the master thread the first time this tournament
            // is seen InProgress - covers both the normal daily flow (which
            // goes through CheckAndCloseRegistrationAsync first) and
            // /testcolosseum (which skips straight to InProgress and would
            // otherwise never get a thread at all).
            if (tournament.MasterThreadId == 0 && parentChannel != null)
            {
                try
                {
                    var newThread = await parentChannel.CreateThreadAsync(
                        name: $"🏛️ Colosseum Tournament #{tournament.Id}",
                        autoArchiveDuration: ThreadArchiveDuration.OneDay,
                        type: ThreadType.PublicThread);

                    tournament.MasterThreadId = newThread.Id;
                    await colosseumRepo.SaveTournamentAsync(tournament);

                    await SendWithRetryAsync(
                        () => newThread.SendMessageAsync($"🏛️ **{tournament.Participants.Count} fighters enter.** Matches resolve here, one round at a time."),
                        $"tournament {tournament.Id} thread intro");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Colosseum thread creation failed for tournament {tournament.Id}: {ex.Message}");
                }
            }

            var thread = tournament.MasterThreadId != 0
                ? _client.GetChannel(tournament.MasterThreadId) as IThreadChannel
                : null;

            // Resolve one round's worth of ready matches, pause
            // SecondsPerRound, then check again - gives the bracket real
            // pacing instead of instant-resolving the whole thing.
            while (true)
            {
                var readyMatches = await bracketService.GetReadyMatchesAsync(tournament.Id);
                if (!readyMatches.Any()) break;

                // Collects one summary per match resolved this wave, so a
                // single combined embed per (BracketType, RoundNumber) group
                // can be posted to the main channel after the wave finishes,
                // instead of one embed per match.
                var waveSummaries = new List<(ColosseumMatch match, string winnerName, string loserName, (int winnerId, int runnerUpId)? decided)>();

                foreach (var match in readyMatches)
                {
                    var participantA = await colosseumRepo.GetParticipantAsync(match.ParticipantAId!.Value);
                    var participantB = await colosseumRepo.GetParticipantAsync(match.ParticipantBId!.Value);

                    if (participantA?.Build == null || participantB?.Build == null)
                    {
                        Console.WriteLine($"❌ Colosseum match {match.Id} has a participant missing a build - skipping this pass.");
                        continue;
                    }

                    var nameA = await ResolveDisplayNameAsync(participantA, playerRepo);
                    var nameB = await ResolveDisplayNameAsync(participantB, playerRepo);

                    var result = combatService.ResolveMatch(participantA, nameA, participantB, nameB);

                    var winnerName = result.WinnerParticipantId == participantA.Id ? nameA : nameB;
                    var loserName = result.WinnerParticipantId == participantA.Id ? nameB : nameA;

                    var decided = await bracketService.AdvanceAfterMatchAsync(match, result.WinnerParticipantId, result.LoserParticipantId);

                    var presentation = BuildMatchPresentation(match, winnerName, loserName, decided);

                    // Detailed per-match embeds - thread only.
                    if (thread != null)
                    {
                        var logTitle = $"⚔️ {nameA} vs {nameB} ({match.BracketType})";
                        await SendCombatLogEmbedsAsync(thread, logTitle, result.CombatLog, presentation.Color, match.Id);

                        var resultEmbed = BuildAdvancementEmbed(presentation);
                        await SendWithRetryAsync(() => thread.SendMessageAsync(embed: resultEmbed), $"match {match.Id} result (thread)");
                    }

                    waveSummaries.Add((match, winnerName, loserName, decided));

                    if (decided.HasValue)
                    {
                        await colosseumService.CompleteTournamentAsync(tournament.Id, decided.Value.winnerId, decided.Value.runnerUpId);

                        // Still post this wave's section(s) to the main
                        // channel before wrapping up, so the last round
                        // (including the deciding match) shows there too.
                        if (parentChannel != null)
                            await PostWaveSectionsAsync(parentChannel, waveSummaries);

                        await AnnounceResultsAsync(tournament, decided.Value.winnerId, decided.Value.runnerUpId, playerRepo, colosseumRepo);
                        return; // tournament fully decided - nothing left to do this tick
                    }
                }

                if (parentChannel != null)
                    await PostWaveSectionsAsync(parentChannel, waveSummaries);

                await Task.Delay(TimeSpan.FromSeconds(SecondsPerRound), stoppingToken);
            }
        }

        // Groups one wave's resolved matches by (BracketType, RoundNumber)
        // and posts one combined embed per group to the main channel - this
        // is the "section" the main channel sees, e.g. "Winner Bracket
        // Round 2" listing every match that happened in that stage at once.
        private async Task PostWaveSectionsAsync(
            ITextChannel channel,
            List<(ColosseumMatch match, string winnerName, string loserName, (int winnerId, int runnerUpId)? decided)> waveSummaries)
        {
            var groups = waveSummaries
                .GroupBy(s => (s.match.BracketType, s.match.RoundNumber))
                .OrderBy(g => g.Key.BracketType)
                .ThenBy(g => g.Key.RoundNumber);

            foreach (var group in groups)
            {
                var (bracketType, roundNumber) = group.Key;
                var (title, color) = GetStageHeader(bracketType, roundNumber);

                var lines = group.Select(s =>
                {
                    var outcome = GetOutcomeSuffix(s.match.BracketType, s.decided);
                    return $"⚔️ **{s.winnerName}** defeats **{s.loserName}** {outcome}";
                });

                var embed = new EmbedBuilder()
                    .WithTitle(title)
                    .WithDescription(string.Join("\n", lines))
                    .WithColor(color)
                    .Build();

                await SendWithRetryAsync(() => channel.SendMessageAsync(embed: embed), $"round section ({title})");
            }
        }

        // Section header + color for a bracket stage - this line is the
        // "overview" of where the tournament currently is.
        private (string title, Color color) GetStageHeader(ColosseumBracketType bracketType, int roundNumber) => bracketType switch
        {
            ColosseumBracketType.WinnerBracket => ($"🟢 Winner Bracket — Round {roundNumber}", new Color(0x2ECC71)),
            ColosseumBracketType.LoserBracket => ($"🔻 Loser Bracket — Round {roundNumber}", new Color(0xE74C3C)),
            ColosseumBracketType.GrandFinal => ("🏆 Grand Final", new Color(0xF1C40F)),
            ColosseumBracketType.BracketReset => ("⚠️ Bracket Reset", new Color(0xE67E22)),
            _ => ("Colosseum", new Color(0x95A5A6))
        };

        private string GetOutcomeSuffix(ColosseumBracketType bracketType, (int winnerId, int runnerUpId)? decided) => bracketType switch
        {
            ColosseumBracketType.WinnerBracket => "→ drops to Loser Bracket",
            ColosseumBracketType.LoserBracket => "→ ☠️ eliminated",
            ColosseumBracketType.GrandFinal when decided.HasValue => "→ 🏆 tournament champion!",
            ColosseumBracketType.GrandFinal => "→ forces a Bracket Reset!",
            ColosseumBracketType.BracketReset => "→ 🏆 tournament champion!",
            _ => ""
        };

        // Everything needed to render one match's log embed + individual
        // result embed (thread only) with a shared color.
        private readonly record struct MatchPresentation(
            string Title, string AdvanceLabel, string AdvanceText,
            string OutLabel, string OutText, Color Color);

        private MatchPresentation BuildMatchPresentation(ColosseumMatch match, string winnerName, string loserName, (int winnerId, int runnerUpId)? decided)
        {
            switch (match.BracketType)
            {
                case ColosseumBracketType.WinnerBracket:
                    return new MatchPresentation(
                        "🟢 Winner Bracket Result", "Advances", $"**{winnerName}**",
                        "Drops to Loser Bracket", $"**{loserName}**", new Color(0x2ECC71));

                case ColosseumBracketType.LoserBracket:
                    return new MatchPresentation(
                        "🔻 Loser Bracket Result", "Survives", $"**{winnerName}**",
                        "☠️ Eliminated", $"**{loserName}**", new Color(0xE74C3C));

                case ColosseumBracketType.GrandFinal when decided.HasValue:
                    return new MatchPresentation(
                        "🏆 TOURNAMENT CHAMPION!", "Winner", $"**{winnerName}**",
                        "Runner-up", $"**{loserName}**", new Color(0xF1C40F));

                case ColosseumBracketType.GrandFinal:
                    return new MatchPresentation(
                        "⚠️ BRACKET RESET!", "Forces a decider match", $"**{winnerName}**",
                        "Must win the reset to survive", $"**{loserName}**", new Color(0xE67E22));

                default: // BracketReset
                    return new MatchPresentation(
                        "🏆 TOURNAMENT CHAMPION!", "Winner", $"**{winnerName}**",
                        "Runner-up", $"**{loserName}**", new Color(0xF1C40F));
            }
        }

        private Embed BuildAdvancementEmbed(MatchPresentation p)
        {
            return new EmbedBuilder()
                .WithTitle(p.Title)
                .AddField(p.AdvanceLabel, p.AdvanceText, inline: true)
                .AddField(p.OutLabel, p.OutText, inline: true)
                .WithColor(p.Color)
                .Build();
        }

        // Embed descriptions cap at 4096 chars - condensed logs should
        // always fit in one, but this chunks into multiple embeds as a
        // fallback so nothing ever gets silently dropped or fails to send.
        private async Task SendCombatLogEmbedsAsync(IThreadChannel thread, string title, string combatLog, Color color, int matchId)
        {
            var chunks = new List<string>();

            if (combatLog.Length <= MaxEmbedDescriptionLength)
            {
                chunks.Add(combatLog);
            }
            else
            {
                var lines = combatLog.Split('\n');
                var current = new System.Text.StringBuilder();

                foreach (var line in lines)
                {
                    if (current.Length + line.Length + 1 > MaxEmbedDescriptionLength && current.Length > 0)
                    {
                        chunks.Add(current.ToString());
                        current.Clear();
                    }

                    if (current.Length > 0) current.Append('\n');
                    current.Append(line);
                }

                if (current.Length > 0)
                    chunks.Add(current.ToString());
            }

            for (var i = 0; i < chunks.Count; i++)
            {
                var chunkTitle = chunks.Count == 1 ? title : $"{title} ({i + 1}/{chunks.Count})";

                var embed = new EmbedBuilder()
                    .WithTitle(chunkTitle)
                    .WithDescription(chunks[i])
                    .WithColor(color)
                    .Build();

                await SendWithRetryAsync(() => thread.SendMessageAsync(embed: embed), $"match {matchId} combat log embed {i + 1}/{chunks.Count}");
            }
        }

        // =========================
        // RESULTS ANNOUNCEMENT
        // =========================
        private async Task AnnounceResultsAsync(
            ColosseumTournament tournament, int winnerId, int runnerUpId,
            PlayerRepository playerRepo, ColosseumRepository colosseumRepo)
        {
            var winner = await colosseumRepo.GetParticipantAsync(winnerId);
            var runnerUp = await colosseumRepo.GetParticipantAsync(runnerUpId);

            var winnerName = winner == null ? "Unknown" : await ResolveDisplayNameAsync(winner, playerRepo);
            var runnerUpName = runnerUp == null ? "Unknown" : await ResolveDisplayNameAsync(runnerUp, playerRepo);

            var winnerPrizeText = winner != null && !winner.IsBot ? $"{tournament.WinnerPrizeGold} gold" : "no gold (bot)";
            var runnerUpPrizeText = runnerUp != null && !runnerUp.IsBot ? $"{tournament.RunnerUpPrizeGold} gold" : "no gold (bot)";

            var embedBuilder = new EmbedBuilder()
                .WithTitle("🏛️ The Colosseum has a champion!")
                .AddField("🥇 Winner", $"{winnerName} — {winnerPrizeText}")
                .AddField("🥈 Runner-up", $"{runnerUpName} — {runnerUpPrizeText}")
                .WithColor(new Color(0xF1C40F));

            if (tournament.MasterThreadId != 0)
                embedBuilder.AddField("📜 Full bracket", $"<#{tournament.MasterThreadId}>");

            var embed = embedBuilder.Build();

            var colosseumChannel = _client.GetChannel(tournament.AnnounceChannelId) as IMessageChannel;
            if (colosseumChannel != null)
                await SendWithRetryAsync(() => colosseumChannel.SendMessageAsync(embed: embed), "results announcement (colosseum channel)");

            // Cross-post to the main RPG feed too, unless that IS the
            // Colosseum channel (avoids a duplicate post if they're ever
            // pointed at the same place).
            if (_rpgFeedChannelId != tournament.AnnounceChannelId)
            {
                var feedChannel = _client.GetChannel(_rpgFeedChannelId) as IMessageChannel;
                if (feedChannel != null)
                    await SendWithRetryAsync(() => feedChannel.SendMessageAsync(embed: embed), "results announcement (rpg feed)");
            }

            Console.WriteLine($"🏛️ Tournament {tournament.Id} complete. Winner: {winnerName}, Runner-up: {runnerUpName}");
        }

        // =========================
        // THREAD CLEANUP
        // =========================
        // Deletes the single tournament thread. Called right before opening
        // the next daily tournament, so players get a full day to review
        // yesterday's bracket before the thread disappears.
        private async Task CleanupTournamentThreadAsync(ColosseumTournament tournament)
        {
            if (tournament.MasterThreadId == 0) return;

            Console.WriteLine($"🧹 Cleaning up Colosseum thread for tournament {tournament.Id}...");

            try
            {
                if (_client.GetChannel(tournament.MasterThreadId) is IThreadChannel thread)
                {
                    await thread.DeleteAsync();
                    Console.WriteLine($"🧹 Deleted Colosseum thread for tournament {tournament.Id}.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Failed to delete Colosseum thread for tournament {tournament.Id}: {ex.Message}");
            }
        }

        // =========================
        // HELPERS
        // =========================
        private async Task<string> ResolveDisplayNameAsync(ColosseumParticipant participant, PlayerRepository playerRepo)
        {
            if (participant.IsBot)
                return participant.BotDisplayName ?? "Arena Bot";

            var player = await playerRepo.GetByDiscordIdAsync(participant.DiscordId);
            return player?.Username ?? $"Player {participant.DiscordId}";
        }

        private async Task<bool> SendWithRetryAsync(Func<Task> sendAction, string context)
        {
            for (var attempt = 1; attempt <= MaxDiscordRetries; attempt++)
            {
                try
                {
                    await sendAction();
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Colosseum send failed ({context}), attempt {attempt}/{MaxDiscordRetries}: {ex.Message}");
                    if (attempt < MaxDiscordRetries)
                        await Task.Delay(TimeSpan.FromSeconds(DiscordRetryDelaySeconds));
                }
            }

            Console.WriteLine($"❌ Colosseum send permanently failed ({context}) after {MaxDiscordRetries} attempts.");
            return false;
        }
    }
}