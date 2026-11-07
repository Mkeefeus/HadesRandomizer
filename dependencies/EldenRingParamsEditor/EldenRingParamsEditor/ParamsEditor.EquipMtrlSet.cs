namespace EldenRingParamsEditor;

public partial class ParamsEditor
{
    /*
        columns are consecutive per family in EquipMtrlSetParam.xml, 
        slot n is the base plus n-1, each family has 6 slots
        padding sits at columns 6, 13, 20 and 27.
    */
    public const int ColIndexMtrlMaterialId = 0;
    public const int ColIndexMtrlItemNum = 7;
    public const int ColIndexMtrlMaterialCate = 14;
    public const int ColIndexMtrlIsDisableDispNum = 21;
    public const int MtrlSlotCount = 6;

    private static int GetMtrlSlotColumn(int baseCol, int slot)
    {
        if (slot < 1 || slot > MtrlSlotCount)
        {
            throw new ArgumentOutOfRangeException(nameof(slot), slot, $"Slot must be between 1 and {MtrlSlotCount}.");
        }
        return baseCol + slot - 1;
    }

    public int GetEquipMtrlSetMaterialId(int equipMtrlSetId, int slot)
    {
        return (int)GetValueAtCell(EquipMtrlSet, _idToRowIndexEquipMtrlSet, equipMtrlSetId, GetMtrlSlotColumn(ColIndexMtrlMaterialId, slot));
    }

    public void SetEquipMtrlSetMaterialId(int equipMtrlSetId, int slot, int materialId)
    {
        SetValueAtCell(EquipMtrlSet, _idToRowIndexEquipMtrlSet, equipMtrlSetId, GetMtrlSlotColumn(ColIndexMtrlMaterialId, slot), materialId);
    }

    public sbyte GetEquipMtrlSetItemNum(int equipMtrlSetId, int slot)
    {
        return (sbyte)GetValueAtCell(EquipMtrlSet, _idToRowIndexEquipMtrlSet, equipMtrlSetId, GetMtrlSlotColumn(ColIndexMtrlItemNum, slot));
    }

    // itemNum is an s8 field; the paramdef allows -1 (unused) to 99
    public void SetEquipMtrlSetItemNum(int equipMtrlSetId, int slot, int itemNum)
    {
        if (itemNum < -1 || itemNum > 99)
        {
            throw new ArgumentOutOfRangeException(nameof(itemNum), itemNum, "Item count must be between -1 and 99.");
        }
        SetValueAtCell(EquipMtrlSet, _idToRowIndexEquipMtrlSet, equipMtrlSetId, GetMtrlSlotColumn(ColIndexMtrlItemNum, slot), (sbyte)itemNum);
    }

    public byte GetEquipMtrlSetMaterialCate(int equipMtrlSetId, int slot)
    {
        return (byte)GetValueAtCell(EquipMtrlSet, _idToRowIndexEquipMtrlSet, equipMtrlSetId, GetMtrlSlotColumn(ColIndexMtrlMaterialCate, slot));
    }

    public void SetEquipMtrlSetMaterialCate(int equipMtrlSetId, int slot, byte materialCate)
    {
        SetValueAtCell(EquipMtrlSet, _idToRowIndexEquipMtrlSet, equipMtrlSetId, GetMtrlSlotColumn(ColIndexMtrlMaterialCate, slot), materialCate);
    }

    // isDisableDispNum is a 1-bit field stored in a u8: 0 or 1
    public byte GetEquipMtrlSetIsDisableDispNum(int equipMtrlSetId, int slot)
    {
        return (byte)GetValueAtCell(EquipMtrlSet, _idToRowIndexEquipMtrlSet, equipMtrlSetId, GetMtrlSlotColumn(ColIndexMtrlIsDisableDispNum, slot));
    }

    public void SetEquipMtrlSetIsDisableDispNum(int equipMtrlSetId, int slot, byte isDisableDispNum)
    {
        if (isDisableDispNum > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(isDisableDispNum), isDisableDispNum, "Value must be 0 or 1.");
        }
        SetValueAtCell(EquipMtrlSet, _idToRowIndexEquipMtrlSet, equipMtrlSetId, GetMtrlSlotColumn(ColIndexMtrlIsDisableDispNum, slot), isDisableDispNum);
    }
}