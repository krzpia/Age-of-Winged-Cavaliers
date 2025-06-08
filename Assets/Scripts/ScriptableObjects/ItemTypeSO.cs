using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemType", menuName = "Types/Item")]
public class ItemTypeSO : CardTypeSO
{
    [Header("ITEM ATTRIBUTES")]
    public List<UnitType> compatibilityList;

    [Header ("TURN DURATION")]
    //public int temporaryAttacksItemEffect = 0;
    public int temporaryTurnsItemEffect = 0;

    [Header("FIREARM")]
    public bool firearm;
    public bool firearmDefence = false;
    public int maxAmmo = 0;
}
