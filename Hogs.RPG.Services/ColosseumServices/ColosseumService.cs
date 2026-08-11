using Hogs.RPG.Core.Entities.ColosseumObjects;
using Hogs.RPG.Core.Enums;
using Hogs.RPG.Core.Enums.PlayerEnums;
using Hogs.RPG.Core.Registries;
using Hogs.RPG.Data.Repositories;

namespace Hogs.RPG.Services.ColosseumServices
{
    /// <summary>
    /// Orchestrates the Colosseum lifecycle: opening registration, real
    /// player signup + buy-in, build purchases (gear/pet/passive/buffs)
    /// against the AP budget, locking builds, closing registration
    /// (randomizing unfinished builds + filling bot slots + seeding the
    /// bracket), and paying out prizes once a tournament completes.
    ///
    /// Bracket resolution itself lives in ColosseumBracketService/
    /// ColosseumCombatService - this service owns everything before combat
    /// starts and the payout step after it ends.
    /// </summary>
    public class ColosseumService
    {
        private readonly ColosseumRepository _colosseumRepository;
        private readonly PlayerRepository _playerRepository;
        private readonly ColosseumBotBuilderService _botBuilderService;
        private readonly ColosseumBracketService _bracketService;

        public ColosseumService(
            ColosseumRepository colosseumRepository,
            PlayerRepository playerRepository,
            ColosseumBotBuilderService botBuilderService,
            ColosseumBracketService bracketService)
        {
            _colosseumRepository = colosseumRepository;
            _playerRepository = playerRepository;
            _botBuilderService = botBuilderService;
            _bracketService = bracketService;
        }

        // =========================
        // TOURNAMENT LIFECYCLE
        // =========================

        /// <summary>
        /// Opens a new tournament's registration window. Called once a day
        /// by ColosseumScheduler.
        /// </summary>
        public async Task<ColosseumTournament> OpenRegistrationAsync(ulong announceChannelId)
        {
            var tournament = new ColosseumTournament
            {
                Status = ColosseumTournamentStatus.Registration,
                RegistrationOpenedAt = DateTime.UtcNow,
                RegistrationEndsAt = DateTime.UtcNow.AddHours(6),
                AnnounceChannelId = announceChannelId,
                BuildBudgetAP = RollDailyApBudget()
            };

            return await _colosseumRepository.CreateTournamentAsync(tournament);
        }

        /// <summary>
        /// Closes registration: locks any real player who never finished
        /// their build (giving them a randomized one, same as bots), fills
        /// the rest of the 32 slots with bots, and seeds the bracket. Called
        /// by ColosseumScheduler once RegistrationEndsAt has passed.
        /// </summary>
        public async Task StartTournamentAsync(int tournamentId)
        {
            var tournament = await _colosseumRepository.GetTournamentAsync(tournamentId)
                ?? throw new Exception($"Colosseum: tournament {tournamentId} not found.");

            if (tournament.Status != ColosseumTournamentStatus.Registration)
                throw new Exception($"Colosseum: tournament {tournamentId} is not in Registration status (currently {tournament.Status}).");

            tournament.Status = ColosseumTournamentStatus.Locked;
            tournament.LockedAt = DateTime.UtcNow;
            await _colosseumRepository.SaveTournamentAsync(tournament);

            // Randomize any real player who signed up but never locked a build.
            foreach (var participant in tournament.Participants.Where(p => !p.IsBot && !p.BuildLocked))
            {
                var randomBuild = _botBuilderService.GenerateRandomBuild(tournament.BuildBudgetAP);
                randomBuild.ColosseumParticipantId = participant.Id;
                await _colosseumRepository.CreateBuildAsync(randomBuild);

                participant.BuildLocked = true;
                participant.BuildWasRandomized = true;
                await _colosseumRepository.SaveParticipantAsync(participant);
            }

            // Fill the remaining slots up to BracketSize with bots.
            // GenerateBotParticipant attaches an unsaved Build to the
            // participant's navigation property, so AddParticipantAsync's
            // single SaveChangesAsync call inserts both the participant and
            // its build together (EF Core cascades through populated
            // navigation properties automatically, fixing up the FK itself).
            // No separate CreateBuildAsync call needed - doing so was
            // trying to re-insert the same already-saved build a second
            // time, which is what caused the duplicate key error.
            var slotsToFill = tournament.BracketSize - tournament.Participants.Count;
            for (var i = 0; i < slotsToFill; i++)
            {
                var botParticipant = _botBuilderService.GenerateBotParticipant(tournament);
                await _colosseumRepository.AddParticipantAsync(botParticipant);
            }

            // Reload with the freshly added bot participants included, then seed.
            tournament = await _colosseumRepository.GetTournamentAsync(tournamentId)
                ?? throw new Exception($"Colosseum: tournament {tournamentId} vanished mid-start.");

            await _bracketService.SeedBracketAsync(tournament);

            tournament.Status = ColosseumTournamentStatus.InProgress;
            await _colosseumRepository.SaveTournamentAsync(tournament);
        }

        /// <summary>
        /// Finalizes a tournament once the bracket has produced a winner and
        /// runner-up: records placements and pays out gold to real players
        /// (bots never receive gold, per design).
        /// </summary>
        public async Task CompleteTournamentAsync(int tournamentId, int winnerParticipantId, int runnerUpParticipantId)
        {
            var tournament = await _colosseumRepository.GetTournamentAsync(tournamentId)
                ?? throw new Exception($"Colosseum: tournament {tournamentId} not found.");

            var winner = await _colosseumRepository.GetParticipantAsync(winnerParticipantId)
                ?? throw new Exception($"Colosseum: winner participant {winnerParticipantId} not found.");
            var runnerUp = await _colosseumRepository.GetParticipantAsync(runnerUpParticipantId)
                ?? throw new Exception($"Colosseum: runner-up participant {runnerUpParticipantId} not found.");

            winner.FinalPlacement = 1;
            runnerUp.FinalPlacement = 2;
            await _colosseumRepository.SaveParticipantAsync(winner);
            await _colosseumRepository.SaveParticipantAsync(runnerUp);

            if (!winner.IsBot)
                await PayGoldAsync(winner.DiscordId, tournament.WinnerPrizeGold);

            if (!runnerUp.IsBot)
                await PayGoldAsync(runnerUp.DiscordId, tournament.RunnerUpPrizeGold);

            tournament.Status = ColosseumTournamentStatus.Completed;
            tournament.CompletedAt = DateTime.UtcNow;
            tournament.WinnerParticipantId = winner.Id;
            tournament.RunnerUpParticipantId = runnerUp.Id;
            await _colosseumRepository.SaveTournamentAsync(tournament);
        }

        private async Task PayGoldAsync(ulong discordId, int amount)
        {
            var player = await _playerRepository.GetByDiscordIdAsync(discordId);
            if (player == null) return; // shouldn't happen, but don't blow up prize payout over it

            player.Gold += amount;
            await _playerRepository.UpdatePlayerAsync(player);
        }

        // =========================
        // SIGNUP
        // =========================

        /// <summary>
        /// Signs a real player up for whatever tournament is currently
        /// taking registrations: validates the buy-in, the 20-real-player
        /// cap, and that they're not already signed up, then deducts gold
        /// and creates their participant + a fresh AP-budgeted build.
        /// </summary>
        public async Task<(bool success, string message, ColosseumParticipant? participant)> SignUpAsync(ulong discordId)
        {
            var tournament = await _colosseumRepository.GetActiveRegistrationAsync();
            if (tournament == null)
                return (false, "There's no Colosseum tournament open for signups right now.", null);

            var alreadySignedUp = await _colosseumRepository.GetActiveParticipantByDiscordIdAsync(discordId);
            if (alreadySignedUp != null)
                return (false, "You're already signed up for this tournament.", null);

            var realPlayerCount = tournament.Participants.Count(p => !p.IsBot);
            if (realPlayerCount >= tournament.MaxRealPlayers)
                return (false, $"The tournament is full ({tournament.MaxRealPlayers}/{tournament.MaxRealPlayers} real players).", null);

            var player = await _playerRepository.GetByDiscordIdAsync(discordId);
            if (player == null)
                return (false, "You need to create a character first.", null);

            if (player.Gold < tournament.BuyInGold)
                return (false, $"You need {tournament.BuyInGold} gold to enter - you have {player.Gold}.", null);

            player.Gold -= tournament.BuyInGold;
            await _playerRepository.UpdatePlayerAsync(player);

            var participant = await _colosseumRepository.AddParticipantAsync(new ColosseumParticipant
            {
                ColosseumTournamentId = tournament.Id,
                DiscordId = discordId,
                IsBot = false
            });

            var build = await _colosseumRepository.CreateBuildAsync(new ColosseumBuild
            {
                ColosseumParticipantId = participant.Id,
                ApBudget = tournament.BuildBudgetAP
            });

            participant.Build = build;

            return (true, "Signed up! Check your DMs to build your loadout.", participant);
        }

        // =========================
        // BUILD PURCHASES
        // Each "Set" method refunds whatever was previously purchased in
        // that category before charging for the new pick, so a player can
        // freely change their mind during the registration window without
        // needing a separate sell/undo command.
        // =========================

        public async Task<(bool success, string message)> SetGearAsync(int participantId, EquipmentSlot slot, string? newItemId)
        {
            var (build, error) = await GetUnlockedBuildAsync(participantId);
            if (build == null) return (false, error!);

            var currentItemId = GetGearField(build, slot);
            var refund = string.IsNullOrEmpty(currentItemId) ? 0 : ColosseumPriceRegistry.GetGearCost(currentItemId);
            var cost = string.IsNullOrEmpty(newItemId) ? 0 : ColosseumPriceRegistry.GetGearCost(newItemId);

            var newSpent = build.ApSpent - refund + cost;
            if (newSpent > build.ApBudget)
                return (false, $"That would put you at {newSpent}/{build.ApBudget} AP - not enough budget left.");

            SetGearField(build, slot, newItemId);
            build.ApSpent = newSpent;
            await _colosseumRepository.SaveBuildAsync(build);

            return (true, $"Equipped. AP spent: {build.ApSpent}/{build.ApBudget}.");
        }

        public async Task<(bool success, string message)> SetPetAsync(int participantId, string? newPetId)
        {
            var (build, error) = await GetUnlockedBuildAsync(participantId);
            if (build == null) return (false, error!);

            var currentPetId = build.PetId;
            var refund = string.IsNullOrEmpty(currentPetId) ? 0 : ColosseumPriceRegistry.GetPetCost(currentPetId);
            var cost = string.IsNullOrEmpty(newPetId) ? 0 : ColosseumPriceRegistry.GetPetCost(newPetId);

            var newSpent = build.ApSpent - refund + cost;
            if (newSpent > build.ApBudget)
                return (false, $"That would put you at {newSpent}/{build.ApBudget} AP - not enough budget left.");

            build.PetId = newPetId;
            build.PetTier = string.IsNullOrEmpty(newPetId)
                ? 1
                : Core.GameData.Registries.PetRegistry.Get(newPetId).Tier;
            build.ApSpent = newSpent;
            await _colosseumRepository.SaveBuildAsync(build);

            return (true, $"Pet set. AP spent: {build.ApSpent}/{build.ApBudget}.");
        }

        public async Task<(bool success, string message)> SetPassiveAsync(int participantId, PetPassive? newPassive)
        {
            var (build, error) = await GetUnlockedBuildAsync(participantId);
            if (build == null) return (false, error!);

            var refund = build.PetPassive.HasValue ? ColosseumPriceRegistry.GetPassiveCost(build.PetPassive.Value) : 0;
            var cost = newPassive.HasValue ? ColosseumPriceRegistry.GetPassiveCost(newPassive.Value) : 0;

            var newSpent = build.ApSpent - refund + cost;
            if (newSpent > build.ApBudget)
                return (false, $"That would put you at {newSpent}/{build.ApBudget} AP - not enough budget left.");

            build.PetPassive = newPassive;
            build.ApSpent = newSpent;
            await _colosseumRepository.SaveBuildAsync(build);

            return (true, $"Passive set. AP spent: {build.ApSpent}/{build.ApBudget}.");
        }

        public async Task<(bool success, string message)> BuyBuffAsync(int participantId, BuffStat stat)
        {
            var (build, error) = await GetUnlockedBuildAsync(participantId);
            if (build == null) return (false, error!);

            var currentCount = GetBuffCount(build, stat);
            if (currentCount >= Core.GameData.Colosseum.ColosseumBuffShop.MaxPurchasesPerStat)
                return (false, $"You've already maxed out {stat} buffs ({currentCount}/{Core.GameData.Colosseum.ColosseumBuffShop.MaxPurchasesPerStat}).");

            var cost = ColosseumPriceRegistry.GetBuffCost(stat);
            var newSpent = build.ApSpent + cost;
            if (newSpent > build.ApBudget)
                return (false, $"That would put you at {newSpent}/{build.ApBudget} AP - not enough budget left.");

            SetBuffCount(build, stat, currentCount + 1);
            build.ApSpent = newSpent;
            await _colosseumRepository.SaveBuildAsync(build);

            return (true, $"Buff purchased. AP spent: {build.ApSpent}/{build.ApBudget}.");
        }

        public async Task<(bool success, string message)> RemoveBuffAsync(int participantId, BuffStat stat)
        {
            var (build, error) = await GetUnlockedBuildAsync(participantId);
            if (build == null) return (false, error!);

            var currentCount = GetBuffCount(build, stat);
            if (currentCount <= 0)
                return (false, $"You don't have any {stat} buffs to remove.");

            var refund = ColosseumPriceRegistry.GetBuffCost(stat);
            SetBuffCount(build, stat, currentCount - 1);
            build.ApSpent -= refund;
            await _colosseumRepository.SaveBuildAsync(build);

            return (true, $"Buff removed. AP spent: {build.ApSpent}/{build.ApBudget}.");
        }

        /// <summary>
        /// Locks a build in - after this, no further purchases are allowed
        /// for this participant. Idempotent-safe: locking an already-locked
        /// build just returns a failure message rather than throwing.
        /// </summary>
        public async Task<(bool success, string message)> LockBuildAsync(int participantId)
        {
            var participant = await _colosseumRepository.GetParticipantAsync(participantId);
            if (participant?.Build == null)
                return (false, "Build not found.");

            if (participant.Build.LockedAt.HasValue)
                return (false, "Your build is already locked in.");

            participant.Build.LockedAt = DateTime.UtcNow;
            participant.BuildLocked = true;
            await _colosseumRepository.SaveBuildAsync(participant.Build);
            await _colosseumRepository.SaveParticipantAsync(participant);

            return (true, "Build locked in! Good luck in the arena.");
        }

        // =========================
        // HELPERS
        // =========================

        private async Task<(ColosseumBuild? build, string? error)> GetUnlockedBuildAsync(int participantId)
        {
            var participant = await _colosseumRepository.GetParticipantAsync(participantId);
            if (participant?.Build == null)
                return (null, "Build not found.");

            if (participant.Build.LockedAt.HasValue)
                return (null, "Your build is already locked in - no more changes allowed.");

            return (participant.Build, null);
        }

        private string? GetGearField(ColosseumBuild build, EquipmentSlot slot) => slot switch
        {
            EquipmentSlot.MainHand => build.GearMainHandId,
            EquipmentSlot.OffHand => build.GearOffHandId,
            EquipmentSlot.Helmet => build.GearHelmetId,
            EquipmentSlot.Body => build.GearBodyId,
            EquipmentSlot.Legs => build.GearLegsId,
            EquipmentSlot.Gloves => build.GearGlovesId,
            EquipmentSlot.Boots => build.GearBootsId,
            EquipmentSlot.Ring => build.GearRingId,
            EquipmentSlot.Amulet => build.GearAmuletId,
            _ => null
        };

        private void SetGearField(ColosseumBuild build, EquipmentSlot slot, string? itemId)
        {
            switch (slot)
            {
                case EquipmentSlot.MainHand: build.GearMainHandId = itemId; break;
                case EquipmentSlot.OffHand: build.GearOffHandId = itemId; break;
                case EquipmentSlot.Helmet: build.GearHelmetId = itemId; break;
                case EquipmentSlot.Body: build.GearBodyId = itemId; break;
                case EquipmentSlot.Legs: build.GearLegsId = itemId; break;
                case EquipmentSlot.Gloves: build.GearGlovesId = itemId; break;
                case EquipmentSlot.Boots: build.GearBootsId = itemId; break;
                case EquipmentSlot.Ring: build.GearRingId = itemId; break;
                case EquipmentSlot.Amulet: build.GearAmuletId = itemId; break;
            }
        }

        private int GetBuffCount(ColosseumBuild build, BuffStat stat) => stat switch
        {
            BuffStat.Attack => build.BuffAttackPurchases,
            BuffStat.Defense => build.BuffDefensePurchases,
            BuffStat.Health => build.BuffHealthPurchases,
            _ => 0
        };

        private void SetBuffCount(ColosseumBuild build, BuffStat stat, int count)
        {
            switch (stat)
            {
                case BuffStat.Attack: build.BuffAttackPurchases = count; break;
                case BuffStat.Defense: build.BuffDefensePurchases = count; break;
                case BuffStat.Health: build.BuffHealthPurchases = count; break;
            }
        }

        /// <summary>
        /// Creates and immediately starts an all-bot Colosseum tournament
        /// for testing - skips the registration window entirely (no real
        /// players, no buy-ins) and hands straight to StartTournamentAsync,
        /// which fills all 32 slots with bots and seeds the bracket. The
        /// regular ColosseumScheduler tick picks it up from there and
        /// resolves it exactly like a real tournament - nothing about
        /// match resolution or payout is test-specific.
        /// </summary>
        public async Task<ColosseumTournament> CreateTestTournamentAsync(ulong announceChannelId)
        {
            var tournament = new ColosseumTournament
            {
                Status = ColosseumTournamentStatus.Registration,
                RegistrationOpenedAt = DateTime.UtcNow,
                RegistrationEndsAt = DateTime.UtcNow, // already "closed" - nothing to wait out
                AnnounceChannelId = announceChannelId,
                BuildBudgetAP = RollDailyApBudget()
            };

            tournament = await _colosseumRepository.CreateTournamentAsync(tournament);
            await StartTournamentAsync(tournament.Id);

            return tournament;
        }

        private static readonly Random _apBudgetRandom = new();

        // Rolls a random build budget for a new tournament - keeps the
        // meta fresh day to day (tight budgets force hard trade-offs, loose
        // ones let more builds approach full BiS). Rounded to the nearest
        // 50 so displayed numbers look intentional rather than arbitrary.
        private static int RollDailyApBudget()
        {
            var raw = _apBudgetRandom.Next(500, 2001); // upper bound exclusive, so 2000 is reachable
            return (int)(Math.Round(raw / 50.0) * 50);
        }
    }
}