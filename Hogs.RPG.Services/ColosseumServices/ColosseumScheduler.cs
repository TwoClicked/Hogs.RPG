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
    ///      the previous tournament's match threads first)
    ///   2. Warn the RPG feed 30 minutes before registration closes
    ///   3. Close registration + seed the bracket once RegistrationEndsAt passes
    ///   4. Resolve every currently-ready match in any InProgress tournament,
    ///      repeatedly, until the bracket is fully decided (this is what
    ///      makes the whole thing finish in well under an hour once started -
    ///      there's no reason to wait between rounds since combat needs no
    ///      live input)
    /// </summary>
    public class ColosseumScheduler : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly DiscordSocketClient _client;

        // Dedicated Colosseum channel - daily open/close announcements and
        // every match thread post here.
        private readonly ulong _announceChannelId = 1536104075253391360;

        // Final results (winner + runner-up) and the 30-minute warning also
        // cross-post to the main RPG feed, since that's higher-traffic than
        // the dedicated Colosseum channel and most players won't be
        // watching match threads.
        private readonly ulong _rpgFeedChannelId = 1485357755433750549;

        // Tournament opens once a day at this UTC hour. Change freely.
        private const int OpenHourUtc = 12;

        private const int MaxDiscordRetries = 3;
        private const int DiscordRetryDelaySeconds = 5;

        // Discord caps message content at 2000 chars. A long fight (many
        // rounds, pet passive trigger lines) can easily exceed that, so
        // combat logs get chunked - this is the target size per chunk,
        // kept comfortably under the hard cap.
        private const int MaxMessageLength = 1900;

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

                try { await ProcessInProgressTournamentAsync(); }
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

            // Clean up every previous tournament's match threads before
            // opening today's - keeps the dedicated Colosseum channel from
            // accumulating dozens of stale threads day after day.
            var completedTournaments = await colosseumRepo.GetCompletedTournamentsAsync();
            foreach (var completed in completedTournaments)
                await CleanupMatchThreadsAsync(completed);

            var tournament = await colosseumService.OpenRegistrationAsync(_announceChannelId);
            _lastOpenedDate = now.Date;

            Console.WriteLine($"🏛️ Opened Colosseum tournament {tournament.Id} - registration closes {tournament.RegistrationEndsAt:u}");

            var channel = _client.GetChannel(_announceChannelId) as IMessageChannel;
            if (channel == null) return;

            var embed = new EmbedBuilder()
                .WithTitle("🏛️ The Colosseum is open!")
                .WithDescription(
                    $"Sign up with `/colosseum signup` — buy-in is **{tournament.BuyInGold} gold**.\n" +
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
                .WithDescription("32 fighters enter. Matches are resolving now — check the threads below as they go up.")
                .WithColor(new Color(0xC0392B))
                .Build();

            await SendWithRetryAsync(() => channel.SendMessageAsync(embed: embed), "bracket-seeded announcement");
        }

        // =========================
        // 4. RESOLVE READY MATCHES
        // =========================
        private async Task ProcessInProgressTournamentAsync()
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

            // Keep resolving whatever's ready until nothing's left this pass -
            // a match resolving can immediately make its next-round match
            // ready too (both feeder matches might finish in the same pass),
            // so this can walk the entire bracket to completion in one tick.
            while (true)
            {
                var readyMatches = await bracketService.GetReadyMatchesAsync(tournament.Id);
                if (!readyMatches.Any()) break;

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

                    ulong threadId = 0;
                    if (parentChannel != null)
                    {
                        try
                        {
                            var thread = await parentChannel.CreateThreadAsync(
                                name: $"⚔️ {nameA} vs {nameB} ({match.BracketType})",
                                autoArchiveDuration: ThreadArchiveDuration.OneDay,
                                type: ThreadType.PublicThread);

                            threadId = thread.Id;
                            await SendCombatLogAsync(thread, result.CombatLog, match.Id);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"⚠️ Colosseum thread creation failed for match {match.Id}: {ex.Message}");
                        }
                    }

                    match.ThreadId = threadId;
                    await colosseumRepo.SaveMatchAsync(match);

                    var decided = await bracketService.AdvanceAfterMatchAsync(match, result.WinnerParticipantId, result.LoserParticipantId);

                    if (decided.HasValue)
                    {
                        await colosseumService.CompleteTournamentAsync(tournament.Id, decided.Value.winnerId, decided.Value.runnerUpId);
                        await AnnounceResultsAsync(tournament, decided.Value.winnerId, decided.Value.runnerUpId, playerRepo, colosseumRepo);
                        return; // tournament fully decided - nothing left to do this tick
                    }
                }
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

            var embed = new EmbedBuilder()
                .WithTitle("🏛️ The Colosseum has a champion!")
                .AddField("🥇 Winner", $"{winnerName} — {winnerPrizeText}")
                .AddField("🥈 Runner-up", $"{runnerUpName} — {runnerUpPrizeText}")
                .WithColor(new Color(0xF1C40F))
                .Build();

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
        // Deletes every match thread for a completed tournament. Each
        // deletion is independently try/caught - one already-deleted or
        // permission-denied thread shouldn't stop the rest from being
        // cleaned up. Called right before opening the next daily
        // tournament, so players get a full day to review yesterday's
        // matches before the threads disappear.
        private async Task CleanupMatchThreadsAsync(ColosseumTournament tournament)
        {
            var threadIds = tournament.Matches.Where(m => m.ThreadId != 0).Select(m => m.ThreadId).ToList();
            if (threadIds.Count == 0) return;

            Console.WriteLine($"🧹 Cleaning up {threadIds.Count} Colosseum match threads for tournament {tournament.Id}...");

            var deleted = 0;
            foreach (var threadId in threadIds)
            {
                try
                {
                    if (_client.GetChannel(threadId) is IThreadChannel thread)
                    {
                        await thread.DeleteAsync();
                        deleted++;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Failed to delete Colosseum thread {threadId}: {ex.Message}");
                }
            }

            Console.WriteLine($"🧹 Deleted {deleted}/{threadIds.Count} Colosseum match threads.");
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

        // Discord caps message content at 2000 chars, and a long fight
        // (many rounds, pet passive trigger lines) can easily exceed that.
        // Splits on line boundaries so no single line gets cut mid-sentence,
        // and sends each chunk as its own message in order.
        private async Task SendCombatLogAsync(IThreadChannel thread, string combatLog, int matchId)
        {
            var lines = combatLog.Split('\n');
            var chunks = new List<string>();
            var current = new System.Text.StringBuilder();

            foreach (var line in lines)
            {
                // +1 accounts for the newline that'll join it back in.
                if (current.Length + line.Length + 1 > MaxMessageLength && current.Length > 0)
                {
                    chunks.Add(current.ToString());
                    current.Clear();
                }

                if (current.Length > 0) current.Append('\n');
                current.Append(line);
            }

            if (current.Length > 0)
                chunks.Add(current.ToString());

            foreach (var chunk in chunks)
                await SendWithRetryAsync(() => thread.SendMessageAsync(chunk), $"match {matchId} combat log chunk");
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