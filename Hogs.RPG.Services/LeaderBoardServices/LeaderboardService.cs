using Hogs.RPG.Core.Entities;
using Hogs.RPG.Core.Entities.PlayerObjects;
using Hogs.RPG.Data.Repositories;
using Hogs.RPG.Services.GameplayServices;
using Hogs.RPG.Services.PetServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hogs.RPG.Services
{
    public class LeaderboardService
    {

        public record PlayerRanks(
            int Gold,
            int Level,
            int GearScore,
            int DungeonRuns,
            int Raids,
            int BossDamage,
            int PetGearScore,
            int Deaths,
            int Trails,
            int SmithingLevel,
            int AlchemistLevel,
            int AchievementCount,
            int SoloTowerFloor,
            int DuoTowerFloor,
            int TotalPlayers
        );

        private readonly PlayerRepository _playerRepository;
        private readonly PetRepository _petRepository;
        private readonly StatService _statService;
        private readonly PetService _petService;
        private readonly AchievementRepository _achievementRepository;

        public LeaderboardService(
            PlayerRepository playerRepository,
            PetRepository petRepository,
            StatService statService,
            PetService petService,
            AchievementRepository achievementRepository)
        {
            _playerRepository = playerRepository;
            _petRepository = petRepository;
            _statService = statService;
            _petService = petService;
            _achievementRepository = achievementRepository;
        }

        // =========================
        // 🧪 ALCHEMIST LEVEL
        // =========================
        public async Task<List<Hogs.RPG.Core.Entities.PlayerObjects.Player>> GetTopAlchemistLevel(int count = 5)
            => await _playerRepository.GetTopAlchemistLevelAsync(count);

        // =========================
        // 💰 GOLD
        // =========================
        public async Task<List<Player>> GetTopGold(int count = 10)
        {
            return await _playerRepository.GetTopGoldAsync(count);
        }

        // =========================
        // ⚒️ SMITHING LEVEL
        // =========================
        public async Task<List<Player>> GetTopSmithingLevel(int count = 5)
            => await _playerRepository.GetTopSmithingLevelAsync(count);

        // =========================
        // 📈 XP (LEVEL FIRST, THEN XP)
        // =========================
        public async Task<List<Player>> GetTopXP(int count = 10)
        {
            return await _playerRepository.GetTopXPAsync(count);
        }

        // =========================
        // 🏰 DUNGEON RUNS
        // =========================
        public async Task<List<Player>> GetTopDungeonRuns(int count = 10)
        {
            return await _playerRepository.GetTopDungeonRunsAsync(count);
        }

        // =========================
        // ⚔️ PLAYER GEAR SCORE
        // =========================
        public async Task<List<(Player player, int score)>> GetTopGearScore(int count = 10)
        {
            var players = await _playerRepository.GetTopForGearScoreAsync(100); // buffer

            return players
                .Select(p =>
                {
                    var (atk, def, hp) = _statService.CalculateStats(p);
                    return (player: p, score: atk + def + hp);
                })
                .OrderByDescending(x => x.score)
                .Take(count)
                .ToList();
        }

        // =========================
        // 🐾 PET GEAR SCORE
        // =========================
        public async Task<List<(Player player, PlayerPet pet, int score)>> GetTopPetGearScore(int count = 10)
        {
            var pets = await _petRepository.GetTopForPetGearScoreWithPlayerAsync(100); // buffer

            return pets
                .Select(p => (
                    player: p.Player,
                    pet: p,
                    score: _petService.GetPetGearScore(p)
                ))
                .OrderByDescending(x => x.score)
                .Take(count)
                .ToList();
        }

        // =========================
        // ⚔️ RAIDS COMPLETED
        // =========================
        public async Task<List<Player>> GetTopRaidsCompleted(int count = 5)
            => await _playerRepository.GetTopRaidsCompletedAsync(count);

        // =========================
        // 💥 BOSS DAMAGE
        // =========================
        public async Task<List<Player>> GetTopBossDamage(int count = 5)
            => await _playerRepository.GetTopBossDamageAsync(count);

        // =========================
        // 🏆 ACHIEVEMENTS
        // =========================
        public async Task<List<(ulong DiscordId, int Count)>> GetTopAchievements(int count = 5)
            => await _achievementRepository.GetTopByCountAsync(count);

        // =========================
        // 💀 DEATHS
        // =========================
        public async Task<List<Player>> GetTopDeaths(int count = 5)
            => await _playerRepository.GetTopDeathsAsync(count);

        // =========================
        // 🏕️ TRAILS COMPLETED
        // =========================
        public async Task<List<Player>> GetTopTrails(int count = 5)
            => await _playerRepository.GetTopTrailsAsync(count);

        // =========================
        // 🗼 TOWER OF DOOM
        // =========================
        public async Task<List<Player>> GetTopSoloTowerFloor(int count = 5)
            => await _playerRepository.GetTopSoloTowerFloorAsync(count);

        public async Task<List<Player>> GetTopDuoTowerFloor(int count = 5)
            => await _playerRepository.GetTopDuoTowerFloorAsync(count);


        //Player individual ranks across all categories
        public async Task<PlayerRanks> GetPlayerRanksAsync(ulong discordId)
        {
            var total = await _playerRepository.GetTotalPlayerCountAsync();

            var goldRank = await _playerRepository.GetRankByGoldAsync(discordId);
            var levelRank = await _playerRepository.GetRankByLevelAsync(discordId);
            var dungeonsRank = await _playerRepository.GetRankByDungeonRunsAsync(discordId);
            var raidsRank = await _playerRepository.GetRankByRaidsAsync(discordId);
            var bossDmgRank = await _playerRepository.GetRankByBossDamageAsync(discordId);
            var deathsRank = await _playerRepository.GetRankByDeathsAsync(discordId);
            var trailsRank = await _playerRepository.GetRankByTrailsAsync(discordId);
            var alchemistRank = await _playerRepository.GetRankByAlchemistLevelAsync(discordId);
            var smithingRank = await _playerRepository.GetRankBySmithingLevelAsync(discordId);
            var achievementRank = await _achievementRepository.GetRankAsync(discordId);
            var soloTowerRank = await _playerRepository.GetRankBySoloTowerFloorAsync(discordId);
            var duoTowerRank = await _playerRepository.GetRankByDuoTowerFloorAsync(discordId);

            // Gear score rank — compute from buffered list (same as top-5 display)
            var allForGear = await _playerRepository.GetTopForGearScoreAsync(1000);
            var gearScores = allForGear
                .Select(p => { var (atk, def, hp) = _statService.CalculateStats(p); return (p.DiscordId, score: atk + def + hp); })
                .OrderByDescending(x => x.score)
                .ToList();
            var myGearScore = gearScores.FirstOrDefault(x => x.DiscordId == discordId);
            int gearRank = myGearScore == default ? total : gearScores.IndexOf(myGearScore) + 1;

            // Pet gear score rank
            var allPets = await _petRepository.GetTopForPetGearScoreWithPlayerAsync(1000);
            var petScores = allPets
                .Select(p => (p.Player.DiscordId, score: _petService.GetPetGearScore(p)))
                .OrderByDescending(x => x.score)
                .ToList();
            var myPet = petScores.FirstOrDefault(x => x.DiscordId == discordId);
            int petRank = myPet == default ? total : petScores.IndexOf(myPet) + 1;

            return new PlayerRanks(goldRank, levelRank, gearRank, dungeonsRank, raidsRank, bossDmgRank, petRank, deathsRank, trailsRank, smithingRank, alchemistRank, achievementRank, soloTowerRank, duoTowerRank, total);
        }
    }
}