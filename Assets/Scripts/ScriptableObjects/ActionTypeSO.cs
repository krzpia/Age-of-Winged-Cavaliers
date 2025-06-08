using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ActionPlayMethod  {NotCoded = 0, OnUnit = 1, OnPlayer = 2, OnLine = 3, OnSide = 4, OnSlot =5, Global =6};


[CreateAssetMenu(fileName = "ActionType", menuName = "Types/Action")]
public class ActionTypeSO : CardTypeSO
{
    public string actionName;

    [Header ("PLAY METHOD")]
    public ActionPlayMethod actionPlayMethod;

    [Header ("DURATION")]
    public int actionTurnDuration = 0;
    //public int actionAttacksDuration = 0;

    [Header("FIREARM")]
    public bool firearm = false;
    public bool firearmDefence = false;
    public int maxAmmo = 0;

    // SUBTYPES
    [Header ("GLOBAL ACTION")]
    public bool globalAction;

    [Header ("UNIT ACTION")]
    // MY UNITS > ADD ATT, ADD DEF, ACTIVATE, INCREASE MOV, GET SPEC AB
    public bool myUnits;
    // ENEMY UNITS > DAMAGE, DESTROY, DEACTIVATE, GET SPEC AB
    public bool enemyUnits;
    // ACTION SUBTYPE:
    public bool isDamageUnit;
    public bool isWeakUnit;
    public bool isHealUnit;
    public bool isAddDefenceUnit;
    public bool isAddAttackUnit;
    public bool isDestroyUnit;
    public bool isActivateUnit;
    public bool isDeactivateUnit;
    public bool isPayUpkeep;
    public bool isJoinUnit;
    public bool isClaimUnit;

    [Header ("BATTLE LINE ACTION:")]
    public bool ownerLineAction;
    public bool enemyLineAction;

    [Header ("PLAYER ACTION")]
    public bool playerAction;
    public int myMoraleBouns;
    public int enemyMoraleBonus;
    public int myGoldBonus;
    public int enemyGoldBonus;
    public int myIncomeBonus;
    public int enemyIncomeBonus;
    public List<CardSO> toDrawCardFromMyDeck;
    public List<CardSO> toDrawCardFromOppDeck;

    [Header ("SLOT ACTION")]
    public bool slotAction;
    // TO DO

    [Header ("TARGET UNIT TYPE")]
    public List<UnitType> unitCompatibilityList;

    [Header("TARGET NATIONALITY")]
    public List<Nationality> nationalityList;

    public bool CheckTargetUnitCompatibility(Unit unitToCheck)
    {
        bool isCompatible = false;
        // UNIT TYPE COMPATIBILITY
        if (unitCompatibilityList.Count > 0)
        {
            if (unitCompatibilityList.Contains(unitToCheck.cardSO.GetUnitType()))
            {
                isCompatible = true;
            }
        }
        else
        {
            isCompatible = true;
        }
        return isCompatible; 
    }

    public bool CheckTargetNationCompatibility(Unit unitToCheck) 
    {
        bool isCompatible = false;
        // NATION TYPE COMPATIBILITY
        if (nationalityList.Count > 0) 
        {
            if (nationalityList.Contains(unitToCheck.cardSO.nationality))
            {
                isCompatible = true;
            }
        }
        else
        {
            isCompatible = true;
        }
        return isCompatible;
    }


}
