using Hogs.RPG.Core.Entities;
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
        private readonly PlayerRepository _playerRepository;
        private readonly PetRepository _petRepository;
        private readonly StatService _statService;
        private readonly PetService _petService;

        public LeaderboardService(
            PlayerRepository playerRepository,
            PetRepository petRepository,
            StatService statService,
            PetService petService)
        {
            _playerRepository = playerRepository;
            _petRepository = petRepository;
            _statService = statService;
            _petService = petService;
        }

        // =========================
        // 💰 GOLD
        // =========================
        public async Task<List<Player>> GetTopGold(int count = 10)
        {
            return await _playerRepository.GetTopGoldAsync(count);
        }

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
        // 💀 DEATHS
        // =========================
        public async Task<List<Player>> GetTopDeaths(int count = 5)
            => await _playerRepository.GetTopDeathsAsync(count);

        // =========================
        // 🏕️ TRAILS COMPLETED
        // =========================
        public async Task<List<Player>> GetTopTrails(int count = 5)
            => await _playerRepository.GetTopTrailsAsync(count);
    }
}