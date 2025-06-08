using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UnitType", menuName = "Types/Unit")]
public class UnitTypeSO : CardTypeSO
{
    public UnitType unitType;
    public string typeName;
    public Sprite unitTypeIcon;

    [Header("UNIT TYPES ATTRIBUTES")]
    //public int unitTypeMovement;
    public bool unitSiege;
}
