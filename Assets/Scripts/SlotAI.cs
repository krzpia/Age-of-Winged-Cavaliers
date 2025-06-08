using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlotAI
{
    public BattleSlot slot;
    public int slotDeployf;
    public int slotMovef;
    // TO DO IN FUTURE if action will be played on slots
    public int slotActionf;

    public void AddDeploySlot(BattleSlot slotToAdd)
    {
        slot = slotToAdd;
    }

    public void AddMoveSlot(BattleSlot slotToAdd)
    {
        slot = slotToAdd;
    }

    public void ChangeSlotMovef(int movef)
    {
        slotMovef += movef;
    }

    public void ChangeSlotDeployf(int deployf)
    {
        slotDeployf += deployf;
    }
}
