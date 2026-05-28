using Hogs.RPG.Core.GameData.Raids;
using Hogs.RPG.Services.InventoryServices;

namespace Hogs.RPG.Services.RaidServices
{
    public class MaterialConversionService
    {
        private readonly InventoryService _inventoryService;

        public MaterialConversionService(InventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        public bool IsConvertible(string materialId) =>
            MaterialConversionData.MaterialToKeyTier.ContainsKey(materialId);

        public int GetMaxKeys(int heldQty) =>
            heldQty / MaterialConversionData.MaterialsPerKey;

        public async Task<(bool success, string message)> ConvertAsync(
            ulong discordId, string materialId, int keyAmount)
        {
            if (!MaterialConversionData.MaterialToKeyTier.TryGetValue(materialId, out int tier))
                return (false, "❌ That material cannot be converted.");

            int cost = keyAmount * MaterialConversionData.MaterialsPerKey;
            int held = await _inventoryService.GetItemAmountAsync(discordId, materialId);

            if (held < cost)
            {
                int canMake = GetMaxKeys(held);
                if (canMake == 0)
                    return (false, $"❌ You need at least **{MaterialConversionData.MaterialsPerKey:N0}** to convert. You only have **{held:N0}**.");

                return (false, $"❌ Not enough materials. You can make at most **{canMake}** key(s) with your **{held:N0}** on hand.");
            }

            await _inventoryService.TakeItemAsync(discordId, materialId, cost);
            await _inventoryService.GiveItemAsync(discordId, $"raid_key_t{tier}", keyAmount);

            string keyName = MaterialConversionData.TierKeyNames[tier];
            return (true, $"✅ Converted **{cost:N0}x** materials into **{keyAmount}x 🗝️ {keyName}**!");
        }
    }
}