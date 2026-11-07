using EldenRingParamsEditor;

namespace Hades.Services;

public static class EquipmentMaterialService
{
    private static void ValidateCostDict(IReadOnlyDictionary<int, int> costDict)
    {
        foreach (var (_, cost) in costDict)
        {
            if (cost < -1 || cost > 99)
            {
                throw new ArgumentOutOfRangeException(nameof(costDict), cost, "Stone cost must be between -1 and 99");
            }
        }
    }

    public static void SetSmithingStoneUpgradeCosts(ParamsEditor editor, IReadOnlyDictionary<int, int> costDict)
    {
        ValidateCostDict(costDict);
        foreach (var (equipMtrlSetId, cost) in costDict)
        {
            // Slot 1 corresponds to stones required at that upgrade level
            editor.SetEquipMtrlSetItemNum(equipMtrlSetId, 1, cost);
        }
    }

    public static void SetSmithingStoneUpgradeCosts(ParamsEditor editor, int cost, bool includeAncient = false)
    {
        Dictionary<int, int> costDict = [];
        // IDs 1-25 are Smithing Stones, 
        // with no dict specified all upgrades are set to cost, excluding ancient dragon smithing stone unless specified
        for (int i = 1; i <= (includeAncient ? 25 : 24); i++)
        {
            costDict.Add(i, cost);
        }
        SetSmithingStoneUpgradeCosts(editor, costDict);
    }
}