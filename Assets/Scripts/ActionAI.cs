using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionAI
{
    public ActionPlayMethod playMethod;
    // SUB CLASS WHICH WILL ACT AS CONTAINER OF ALL AIFACTORS
    // 1. IF IS UNIT PLAY
    // if can play on Unit -> list has more than 0 elements
    [SerializeField] public List<UnitAI> posibleUnitsToCast;
    public Unit bestActionTarget = null;
    // 2. IF IS LINE PLAY
    public bool canPlayOnEnemyLine = false;
    public int enemyLineAIF;
    public bool canPlayOnOwnLine = false;
    public int ownLineAIF;
    // 3. IF IS PLAYER PLAY
    public bool canPlayOnEnemyPlayer = false;
    public int enemyPlayerAIF;
    public bool canPlayOnSelfPlayer = false;
    public int ownPlayerAIF;
    // 4. IF IS GLOBAL
    public bool canPlayGlobal = false;
    public int globalAIF;
    // 5. IF IS ON SLOT PLAY:
    // if can play on Slot -> list has more than 0 eleements
    [SerializeField] public List<SlotAI> posibleSlotsToCast;
    public BattleSlot bestActionSlot = null;
    // 6. TO STORE BEST RESULT:
    public int bestAIF = -10;

    // TO DO DICT WITH BEST VALUES?
    // TO DO FUNCTION RETURNING BEST PLAY?

    public bool CheckIfAbleToPlay()
    {
        // 1. IF IS UNIT PLAY
        if (posibleUnitsToCast.Count > 0)
        {
            return true;
        }
        // 2. IF IS LINE PLAY
        if (canPlayOnEnemyLine)
        {
            return true;
        }
        if (canPlayOnOwnLine)
        {
            return true;
        }
        if (canPlayOnEnemyPlayer)
        {
            return true;
        }
        if (canPlayOnSelfPlayer)
        {
            return true; 
        }
        if (canPlayGlobal) 
        { 
            return true;
        }
        if (posibleSlotsToCast.Count > 0)
        {
            return true;
        }
        return false;
    }


    public void ClearAllValues()
    {
        //ableToPlay = false;
        playMethod = ActionPlayMethod.NotCoded;
        posibleUnitsToCast = new List<UnitAI>();
        bestActionTarget = null;
        canPlayOnEnemyLine = false;
        enemyLineAIF = 0;
        canPlayOnOwnLine = false;
        ownLineAIF = 0;
        canPlayOnEnemyPlayer = false;
        enemyPlayerAIF = 0;
        canPlayOnSelfPlayer = false;
        ownPlayerAIF = 0;
        canPlayGlobal = false;
        globalAIF = 0;
        posibleSlotsToCast = new List<SlotAI>();
        bestActionSlot = null;
        bestAIF = -11;
    }

    public int GetBestActionAIF()
    {
        int maxF = -11;
        if (playMethod == ActionPlayMethod.OnUnit)
        {
            if (posibleUnitsToCast.Count > 0)
            {
                foreach (UnitAI unitAI in posibleUnitsToCast)
                {
                    if (unitAI.unitAttackf > maxF)
                    {
                        maxF = unitAI.unitAttackf;
                        bestAIF = unitAI.unitAttackf;
                        bestActionTarget = unitAI.unit;
                    }
                }
            }
        }
        else if (playMethod == ActionPlayMethod.OnLine)
        {
            if (canPlayOnEnemyLine)
            {
                if (enemyLineAIF > maxF)
                {
                    maxF = enemyLineAIF;
                    bestAIF = enemyLineAIF;
                }
            }
            if (canPlayOnOwnLine)
            {
                if (ownLineAIF > maxF)
                {
                    maxF = ownLineAIF;
                    bestAIF = ownLineAIF;
                }
            }
        }
        else if(playMethod == ActionPlayMethod.OnPlayer)
        {
            // TO DO
        }
        else if (playMethod == ActionPlayMethod.OnSide)
        {
            // TO DO
        }
        else if(playMethod == ActionPlayMethod.OnSlot)
        {
            // TO DO
        }
        else if(playMethod == ActionPlayMethod.Global)
        {
            // TO DO
        }
        // Debug.Log("BEST ACTION AIF OF ACTION CARD IS: " + bestAIF);
        return bestAIF;
    }

    public void SetUnitAI(Unit unitToCastOn)
    {
        //ableToPlay = true;
        playMethod = ActionPlayMethod.OnUnit;
        UnitAI unitAI = new UnitAI();
        unitAI.AddUnitToAtack(unitToCastOn);
        posibleUnitsToCast.Add(unitAI);
    }

    public void ChangeUnitAIF(Unit unitToCastOn, int unitAIF)
    {
        bool found = false;
        foreach (UnitAI unitAI in posibleUnitsToCast)
        {
            if (unitAI.unit == unitToCastOn)
            {
                unitAI.ChangeUnitf(unitAIF);
                found = true; break;
            }
        }
        if (!found)
        {
            Debug.LogWarning("CHANGE UNIT AIF IN ACTION POSIBLE UNITS TO CAST NOT FOUND!");
        }
    }

    public void SetLineAI()
    {
        playMethod = ActionPlayMethod.OnLine;
    }

    public void ChangeEnemyLineAIF(int enemyLineAIFToAdd)
    {
        enemyLineAIF += enemyLineAIFToAdd;
    }
}
