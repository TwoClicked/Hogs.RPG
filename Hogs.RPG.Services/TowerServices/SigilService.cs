using Hogs.RPG.Core.Entities.SigilObjects;
using Hogs.RPG.Data.Repositories;

namespace Hogs.RPG.Services.TowerServices
{
    public class SigilService
    {
        private readonly SigilRepository _sigilRepo;

        // Per-stack bonus values, matching the descriptions in SigilRegistry.
        private const float SlaughterPerStack = 0.02f;  // +2% damage
        private const float FortitudePerStack = 0.02f;  // +2% defense
        private const float VitalityPerStack = 0.02f;  // +2% max HP
        private const float LeechPerStack = 0.01f;  // +1% lifesteal
        private const float GreedPerStack = 0.02f;  // +2% gold
        private const float WisdomPerStack = 0.02f;  // +2% XP

        public SigilService(SigilRepository sigilRepo)
        {
            _sigilRepo = sigilRepo;
        }

        public async Task<SigilBonuses> GetSigilBonusesAsync(ulong discordId)
        {
            var owned = await _sigilRepo.GetSigilsAsync(discordId);
            var bonuses = new SigilBonuses();

            int Stacks(string sigilId) => owned.FirstOrDefault(s => s.SigilId == sigilId)?.Count ?? 0;

            bonuses.AttackPercent = Stacks("sigil_slaughter") * SlaughterPerStack;
            bonuses.DefensePercent = Stacks("sigil_fortitude") * FortitudePerStack;
            bonuses.MaxHpPercent = Stacks("sigil_vitality") * VitalityPerStack;
            bonuses.LifeStealPercent = Stacks("sigil_leech") * LeechPerStack;
            bonuses.BonusGoldPercent = Stacks("sigil_greed") * GreedPerStack;
            bonuses.BonusPlayerXpPercent = Stacks("sigil_wisdom") * WisdomPerStack;

            return bonuses;
        }
    }
}