using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleLine : MonoBehaviour
{
    public void PlayLineAction(Card actionCard)
    {
        ActionTypeSO actionType = actionCard.cardSO.cardTypeSO as ActionTypeSO;
        if (actionType == null)
        {
            Debug.LogWarning("ERROR ON DROP ACTION CARD: NO ACTIONTYPE CLASS ATTACHED?");
        }
        if (actionType.actionPlayMethod != ActionPlayMethod.OnLine)
        {
            Debug.LogWarning("ERROR: ON DROP IN LINE: ACTION PLAY METHOD DOES NOT MATCH");
            return;
        }
        // ACTION PLAY METHOD: ON LINE
        if (actionCard.cardSO.baseDef != 0)
        {
            // PAY GOLD
            GameManager.instance.actPlayer.playerActGold -= actionCard.cardSO.cardCost;
            // ACTION TO BE APPLIED
            ApplyDefMod(actionType, actionCard.cardSO);
            // CARD TO USED
            SendCardToPlayedContainer(actionCard.gameObject);
        }
        if (actionCard.cardSO.baseAtt != 0)
        {
            // PAY GOLD
            GameManager.instance.actPlayer.playerActGold -= actionCard.cardSO.cardCost;
            // ACTION TO BE APPLIED
            ApplyAttMod(actionType, actionCard.cardSO);
            // CARD TO USED
            SendCardToPlayedContainer(actionCard.gameObject);
        }
    }

    private void ApplyDefMod(ActionTypeSO actionType, CardSO actionCardSO)
    {
        // MY UNITS
        if (actionType.myUnits)
        {
            foreach (Unit unit in GetListOfUnitsOfPlayer(actionCardSO.GetOwner()))
            {
                Debug.Log(unit.cardSO.cardName + " GETS " + actionCardSO.baseDef + " DMG");
                if (actionCardSO.baseDef < 0)
                {
                    unit.ActionAttackUnit(Mathf.Abs(actionCardSO.baseDef));
                }
                else if (actionCardSO.baseDef > 0)
                {
                    Debug.LogWarning("TO DO: ACTION WITH BOUNS DEFENCE + ANIMATION!");
                }
            }
        }
        // ENEMY UNITS
        if (actionType.enemyUnits)
        {
            foreach (Unit unit in GetListOfUnitsOfPlayer(GameManager.instance.GetOtherPlayer(actionCardSO.GetOwner())))
            {
                Debug.Log(unit.cardSO.cardName + " GETS " + actionCardSO.baseDef + " DMG");
                if (actionCardSO.baseDef < 0)
                {
                    unit.ActionAttackUnit(Mathf.Abs(actionCardSO.baseDef));
                }
                else if (actionCardSO.baseDef > 0)
                {
                    Debug.LogWarning("TO DO: ACTION WITH BOUNS DEFENCE + ANIMATION!");
                }
            }
        }
        GameEvents.current.CardEndDrag();
        GameEvents.current.EndShowPosibleBattleSlot();
        GameEvents.current.EndHighlightUnit();
        GameManager.instance.EndShowActionPosibilities();
        // REFRESH VISUALS
        GameManager.instance.actPlayer.RefreshAttributeVisuals();

    }

    private void SendCardToPlayedContainer(GameObject cardGO)
    {
        Card card = cardGO.GetComponent<Card>();
        card.GetComponent<Card>().UnsubscribeEvents();
        card.cardSO.GetOwner().SendCardToPlayedContainer(cardGO);
        card.gameObject.SetActive(false);
    }

    private void ApplyAttMod(ActionTypeSO actionType, CardSO actionCardSO)
    {
        if (actionType.myUnits)
        {
            foreach (Unit unit in GetListOfUnitsOfPlayer(actionCardSO.GetOwner()))
            {
                Debug.LogWarning(" TO DO ACTION: " + unit.cardSO.cardName + " GETS " + actionCardSO.baseAtt + " PTS");
            }
        }
        if (actionType.enemyUnits)
        {
            foreach (Unit unit in GetListOfUnitsOfPlayer(GameManager.instance.GetOtherPlayer(actionCardSO.GetOwner())))
            {
                Debug.LogWarning(" TO DO ACTION :" + unit.cardSO.cardName + " GETS " + actionCardSO.baseAtt + " PTS");
            }
        }
        GameEvents.current.CardEndDrag();
        GameEvents.current.EndShowPosibleBattleSlot();
        GameEvents.current.EndHighlightUnit();
        GameManager.instance.EndShowActionPosibilities();
        // REFRESH VISUALS
        GameManager.instance.actPlayer.RefreshAttributeVisuals();
    }

    // LOGIC

    public int GetNumberOfSlots()
    {
        int numberOfSlots = 0;
        foreach (Transform slotTransform in transform)
        {
            numberOfSlots++;
        }
        return numberOfSlots;
    }

    public List<Unit> GetListOfUnits()
    {
        List<Unit> unitsOnLine = new List<Unit>();
        foreach (Transform slotTransform in transform)
        {
            Transform unitSlot = slotTransform.GetComponent<BattleSlot>().unitSlot;
            if (unitSlot.childCount > 0)
            {
                if (unitSlot.GetChild(0).GetComponent<Unit>() != null)
                {
                    unitsOnLine.Add(unitSlot.GetChild(0).GetComponent<Unit>());
                }
            }
        }
        return unitsOnLine;
    }

    public List<Unit> GetListOfUnitsOfPlayer(Player player)
    {
        List<Unit> playerUnits = new List<Unit>();
        foreach (Unit unit in GetListOfUnits())
        {
            if (unit.cardSO.GetOwner() == player)
            {
                playerUnits.Add(unit);
            }
        }
        return playerUnits;
    }
}
