using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum UnitDecsion
{
    AttackUnit,
    AttackTent,
    Move,
    Skip,
    None
}

public class AI : MonoBehaviour
{
    // private float allCardsMeanCost;
    [SerializeField]
    private List<Card> allCards;
    [SerializeField]
    private List<Card> cardsInStartPanel;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("AI SCRIPT STARTS");
        Debug.Log("Attached TO: " + GetComponent<Player>().playerSO.playerName);
    }

    // SET DECK

    public void AISetDeck()
    {
        allCards = GetComponent<Player>().GetListCardInDeck();
        // allCardsMeanCost = GetAllCardsMeanCost();
        // ebug.Log("AI MEAN CARD VALUE:" + allCardsMeanCost);
    }

    public void AISetStarterCards(List<Card> startCards)
    {
        cardsInStartPanel = startCards;
    }

    // EXCHANGE CARDS AI AUX FUNCTIONS

    private float GetAllCardsMeanCost()
    {
        int i = 0;
        int sum = 0;
        foreach (Card card in allCards)
        {
            i++;
            sum += card.cardSO.cardCost;
        }
        float result = sum / (float)i;
        //Debug.Log("MEAN CARDS COST: " + sum + "/" + i + "=" + result);
        return (result);
    }

    private float GetStartCardsMeanCost()
    {
        int i = 0;
        int sum = 0;
        foreach (Card card in cardsInStartPanel)
        {
            i++;
            sum += card.cardSO.cardCost;
        }
        float result = sum / (float)i;
        return (result);
    }

    private Card GetMostExpensiveCardInStartPanel()
    {
        int maxCost = 0;
        Card mostExpCard = null;
        foreach (Card card in cardsInStartPanel) 
        { 
            if (card.cardSO.cardCost > maxCost)
            {
                maxCost = card.cardSO.cardCost;
                mostExpCard = card;
            }
        }
        return mostExpCard;
    }

    private Card GetCardHighestAIChangefInNonMarkedCards()
    {
        int maxAIChf = 0;
        Card highestAIChfCard = null;
        foreach (Card card in cardsInStartPanel)
        {
            if (card.AIchangef > maxAIChf)
            {
                if (!card.GetComponent<CardBehaviour>().IfIsMarkedToChange())
                {
                    maxAIChf = card.AIchangef;
                    highestAIChfCard = card;
                }
            }
        }
        return highestAIChfCard;
    }

    public Card GetMostExpensiveCard (List<Card> cardList)
    {
        int maxCost = 0;
        Card mostExpCard = null;
        foreach (Card card in cardList)
        {
            if (card.cardSO.cardCost > maxCost)
            {
                maxCost = card.cardSO.cardCost;
                mostExpCard = card;
            }
        }
        return mostExpCard;
    }

    public List<Card> GetUnitsInStartPanel()
    {
        List<Card> units = new List<Card>();
        foreach (Card card in cardsInStartPanel) 
        {
            if (card.cardSO.cardTypeSO.cardType == CardType.Unit)
            {
                units.Add(card);
            }
        }
        return units;
    }

    public List<Card> GetItemsInStartPanel()
    {
        List<Card> items = new List<Card>();
        foreach (Card card in cardsInStartPanel)
        {
            if (card.cardSO.cardTypeSO.cardType == CardType.Item)
            {
                items.Add(card);
            }
        }
        return items;
    }

    public List<Card> GetActionsInStartPanel()
    {
        List<Card> actions = new List<Card>();
        foreach (Card card in cardsInStartPanel)
        {
            if (card.cardSO.cardTypeSO.cardType == CardType.Action)
            {
                actions.Add(card);
            }
        }
        return actions;
    }

    private void DoCardTypeStarterCount()
    {
        int units = GetUnitsInStartPanel().Count;
        int items = GetItemsInStartPanel().Count;
        int actions = GetActionsInStartPanel().Count;
        Debug.Log("UNITS: " + units + ", ITEMS: " + items + " ,ACTIONS: " + actions);
        Debug.Log("MEAN ALL CARD COST:" + GetAllCardsMeanCost().ToString());
        Debug.Log("MEAN START CARDS COST: "+ GetStartCardsMeanCost().ToString());   
        // 0. KAZDA KARTA + koszt
        foreach (Card card in cardsInStartPanel)
        {
            card.AIchangef += card.cardSO.cardCost;
        }
        // 1. PRZEDMIOTY: JEZELI WIECEJ PRZEDMIOTOW niz 1 lub wiecej niz jednostek - przedmioty +2 lub +5 lub +9 do CHANGE CARD FACTOR
        foreach (Card card in GetItemsInStartPanel())
        {
            card.AIchangef += 2;
        }
        if (items > 1)
        {
            foreach (Card card in GetItemsInStartPanel())
            {
                card.AIchangef += 3;
            }
        }
        if (items >= units)
        {
            foreach (Card card in GetItemsInStartPanel())
            {
                card.AIchangef += 4;
            }
        }
        // 2. JEZELI WIECEK AKCJI NIZ JEDNOSTEK - akcje +5 
        if (actions >= units)
        {
            foreach (Card card in GetActionsInStartPanel())
            {
                card.AIchangef += 5;
            }
        }
        // 3. JEZELI sredni koszt na rece wiekszy niz sredni koszt calosci - najdrozsza karta +3
        Debug.Log("MOST EXPENSIVE CARD: " + GetMostExpensiveCardInStartPanel().cardSO.cardName);
        if (GetStartCardsMeanCost() > GetAllCardsMeanCost())
        {
            Card mostExpCard = GetMostExpensiveCardInStartPanel();
            mostExpCard.AIchangef += 3;
        }
        // 4. ADD SMALL RANDOM
        foreach (Card card in cardsInStartPanel)
        {
            card.AIchangef += Random.Range(0, 2);
        }
        // 5. SPECIAL CARDS... cheapest artillery, unit with maneuver.
        foreach (Card card in cardsInStartPanel)
        {
            if (card.cardSO.CheckIfHasSpecAb("Charge"))
            {
                if (card.cardSO.cardCost < GetStartCardsMeanCost())
                {
                    card.AIchangef -= 3;
                }
                else
                {
                    card.AIchangef -= 1;
                }
            }
            if (card.cardSO.cardTypeSO.cardType == CardType.Unit)
            {
                UnitTypeSO unitType = card.cardSO.cardTypeSO as UnitTypeSO;
                if (unitType.unitType == UnitType.Artillery)
                {
                    if (card != GetMostExpensiveCardInStartPanel())
                    {
                        Debug.Log("TWO ARTILLERY UNITS, CHEAPER ONE REDUCING AICHANGEF");
                        card.AIchangef -= 4;
                    }
                }
            }
        }
        // END: SPRAWDZAM AI CHANGE FACTOR:
        foreach (Card card in cardsInStartPanel)
        {
            Debug.Log("CARD: " + card.cardSO.cardName + ", AI CHANGE FACTOR: " + card.AIchangef);
        }
    }

    private void MarkCardsToChange()
    {
        float meanDif = GetStartCardsMeanCost() - GetAllCardsMeanCost();
        int noCardsToChange = 0;
        // 1. VERY LOW (less than -1) MEAN STARTS CARD  = Change items or actions
        if (meanDif < -1)
        {
            Debug.Log("Mean Dif < -1, very cheap cards");
            noCardsToChange = Random.Range(0, 2);
            if (GetItemsInStartPanel().Count >= GetUnitsInStartPanel().Count)
            {
                noCardsToChange += 1;
            }
            if (GetActionsInStartPanel().Count >= GetUnitsInStartPanel().Count)
            {
                noCardsToChange += 1;
            }
        }
        else if (meanDif >= -1 && meanDif < 0)
        {
            Debug.Log("Mean Dif <0, cheap cards");
            noCardsToChange = Random.Range(0, 3);
            if (GetItemsInStartPanel().Count >= GetUnitsInStartPanel().Count)
            {
                noCardsToChange += 1;
            }
            if (GetActionsInStartPanel().Count >= GetUnitsInStartPanel().Count)
            {
                noCardsToChange += 1;
            }
        }
        else if (meanDif >=0 && meanDif < 1)
        {
            Debug.Log("Mean Dif >=0, exp cards");
            noCardsToChange = Random.Range(1, 3);
            if (GetItemsInStartPanel().Count >= GetUnitsInStartPanel().Count)
            {
                noCardsToChange += 1;
            }
            if (GetActionsInStartPanel().Count >= GetUnitsInStartPanel().Count)
            {
                noCardsToChange += 1;
            }
        }
        else if (meanDif >= 1)
        {
            Debug.Log("Mean Dif > 1, very exp cards");
            noCardsToChange = Random.Range(1, 4);
            if (GetItemsInStartPanel().Count >= GetUnitsInStartPanel().Count)
            {
                noCardsToChange += 1;
            }
            if (GetActionsInStartPanel().Count >= GetUnitsInStartPanel().Count)
            {
                noCardsToChange += 1;
            }
        }
        Debug.Log("NO CARDS TO CHANGE: " + noCardsToChange);
        for (int i = 0; i < noCardsToChange; i++)
        {
            MarkCardToChange();
        }
        Debug.Log("CHECK CARDS MARKED TO CHANGE:");
        foreach (Card card in cardsInStartPanel)
        {
            if (card.GetComponent<CardBehaviour>().IfIsMarkedToChange())
            {
                Debug.Log(card.cardSO.cardName + " TO CHANGE");
            }
        }
        Debug.Log("END OF MarkCardsToChange()");
    }

    private void MarkCardToChange()
    {
        Card cardToMark = GetCardHighestAIChangefInNonMarkedCards();
        cardToMark.GetComponent<CardBehaviour>().AImarkToChange();
        GetComponent<Player>().startPanel.cardsToChange.Add(cardToMark.gameObject);
    }

    public void ExchangeCardsAtStart()
    {
        Debug.Log("AI - START CORUTINE EXCHANGE CARDS");
        // 1. LOGIC OF WHICH CARD TO CHANGE
        // 1a. SET CARDS AICHANGEF 
        DoCardTypeStarterCount();
        // 2. MARK CARDS TO CHANGE
        MarkCardsToChange();
        // 3. START COROUTINE ANIMATIONS and LOGIC
        StartCoroutine(ExchangeCards());
    }

    public IEnumerator ExchangeCards()
    {
        yield return new WaitForSeconds(1);
        GetComponent<Player>().startPanel.AIExchangeCards();
        yield return new WaitForSeconds(1);
        Debug.Log("<AI> - END CORUTINE EXCHANGE CARDS");
    }

    // PLACEMENT AND BATTLE AI AUX FUNCTIONS

    private Unit GetUnitWithHighestAIFactor()
    {
        Debug.Log("************AIMAINLOOP************* f() GetUnitWithHighestAIFactor()");
        Unit unitHighestF = null;
        List<Unit> unitToMakeAction = new List<Unit>();
        // 1. SPRAWDZAM CZY JEST JEDNOSTKA TO ZAGRANIA
        if (GetComponent<Player>().GetListOfPlayerUnits().Count > 0)
        {
            foreach (Unit unitToAct in GetComponent<Player>().GetListOfPlayerUnits())
            {
                unitToAct.target = null;
                if (unitToAct.CheckActive())
                {
                    //Debug.Log("UNIT: " + unitToAct.cardSO.cardName + "Is ACTIVE, adding");
                    unitToMakeAction.Add(unitToAct);
                }
                else
                {
                    if (unitToAct.cardSO.cardUpkeep > 0 && unitToAct.cardSO.cardUpkeep <= GetComponent<Player>().playerActGold)
                    {
                        if (!unitToAct.paid)
                        {
                            //Debug.Log("UNIT WITH UPKEEP IS INACTIVE, BUT DID NOT PLAY THIS TURN AND COULD BE PAID");
                            unitToMakeAction.Add(unitToAct);
                        }
                    }
                }
            }
        }
        else
        {
            Debug.Log("AI PLAYER UNITS NOT FOUND, Unit GetUnitWithHighestAIFactor() RETURN NULL");
            return null;
        }
        // 2.1. RESET VALUES
        foreach (Unit unit in unitToMakeAction)
        {
            unit.aiDecision = UnitDecsion.None;
            unit.target = null;
            unit.targetAIF = -11;
            unit.besSlotToMove = null;
        }
        // 2.2 SPRAWDZAM AI ATTACK FACTOR, AI TENT ATTACK, AI MOVE F KAZDEJ JEDNOSTKI
        int highestAIF = GetComponent<Player>().playerSO.AIUnitAttackMinimalFactor;
        foreach (Unit unit in unitToMakeAction) 
        {
            // 2.1. AI UNIT ATTACK
            foreach(UnitAI unitAIToAttack in unit.AIattackUnits)
            {
                Debug.Log("--UNIT-- GetUnitWithHighestAIFactor(): UNIT TO ATTACK: " + unitAIToAttack.unit.cardSO.cardName + "BY MY UNIT: " + unit.cardSO.cardName + " HAS ATTACK AIF: " + unitAIToAttack.unitAttackf);
                if (unitAIToAttack.unitAttackf > highestAIF)
                {
                    highestAIF = unitAIToAttack.unitAttackf;
                    unit.target = unitAIToAttack.unit;
                    unit.targetAIF = unitAIToAttack.unitAttackf;
                    unitHighestF = unit;
                    unit.aiDecision = UnitDecsion.AttackUnit;
                }
            }
            // 2.2. AI TENT ATTACK
            if (unit.hqTentAIF >= highestAIF)
            {
                if (unit.targetHqTent)
                {
                    Debug.Log("--UNIT-- GetUnitWithHighestAIFactor(): TENT TO ATTACK BY MY UNIT: " + unit.cardSO.cardName + " HAS HQTENT AIF:" + unit.hqTentAIF);
                    highestAIF = unit.hqTentAIF;
                    unitHighestF = unit;
                    unit.aiDecision = UnitDecsion.AttackTent;
                }
            }
            // 2.3. AI MOVE
            foreach (SlotAI slotToMove in unit.AImoveSlots)
            {
                //Debug.Log("--UNIT-- GetUnitWithHighestAIFactor(): UNIT TO MOVE TO SLOT: " + slotToMove.slot.x + " HAS MOVE AIF: " + slotToMove.slotMovef);
                if (slotToMove.slotMovef > highestAIF)
                {
                    if (slotToMove.slotMovef > 0)
                    {
                        highestAIF = slotToMove.slotMovef;
                        unit.besSlotToMove = slotToMove.slot;
                        unit.bestSlotToMoveAIF = slotToMove.slotMovef;
                        unitHighestF = unit;
                        unit.aiDecision = UnitDecsion.Move;
                    }
                }
            }
        }
        // 3. DEBUG INFO
        if (unitHighestF != null)
        {
            //Debug.Log("GetUnitWithHighestAIFactor(): UNITHIGHESTF.HQTENTAIF: " + unitHighestF.hqTentAIF);
            //Debug.Log("GetUnitWithHighestAIFactor(): UNITHIGHESTF.TARGETAIF: " + unitHighestF.targetAIF);
            if (unitHighestF.targetHqTent && unitHighestF.target == null)
            {
                Debug.Log("--UNIT-- GetUnitWithHighestAIFactor(): UNIT WITH HIGHEST ATTACK FACTOR IS: " + unitHighestF.cardSO.cardName + " WILL ATTACK ENEMY`s HQ TENT with AIF : " + highestAIF);
                Debug.Log("--UNIT-- GetUnitWithHighestAIFactor(): UNIT.AIDECISON = " + unitHighestF.aiDecision);
            }
            else if (unitHighestF.targetHqTent && unitHighestF.target != null)
            {
                if (unitHighestF.hqTentAIF >= unitHighestF.targetAIF)
                {
                    Debug.Log("--UNIT-- GetUnitWithHighestAIFactor(): UNIT WITH HIGHEST ATTACK FACTOR IS: " + unitHighestF.cardSO.cardName + " WILL ATTACK ENEMY`s HQ TENT with AIF : " + highestAIF);
                    Debug.Log("--UNIT-- GetUnitWithHighestAIFactor(): UNIT.AIDECISON = " + unitHighestF.aiDecision);
                }
                else
                {
                    Debug.Log("--UNIT-- GetUnitWithHighestAIFactor(): UNIT WITH HIGHEST ATTACK FACTOR IS: " + unitHighestF.cardSO.cardName + " WILL ATTACK " + unitHighestF.target.cardSO.cardName + " with AIF : " + highestAIF);
                    Debug.Log("--UNIT-- GetUnitWithHighestAIFactor(): UNIT.AIDECISON = " + unitHighestF.aiDecision);
                }
            }
            else if (!unitHighestF.targetHqTent && unitHighestF.target != null)
            {
                Debug.Log("--UNIT-- GetUnitWithHighestAIFactor(): UNIT WITH HIGHEST ATTACK FACTOR IS: " + unitHighestF.cardSO.cardName + " WILL ATTACK " + unitHighestF.target.cardSO.cardName + " with AIF : " + highestAIF);
                Debug.Log("--UNIT-- GetUnitWithHighestAIFactor(): UNIT.AIDECISON = " + unitHighestF.aiDecision);
            }
            else
            {
                Debug.Log("GetUnitWithHighestAIFactor(): UNIT.AIDECISON = " + unitHighestF.aiDecision);
            }  
        }
        else
        {
            Debug.Log("--UNIT-- UNIT WITH HIGHEST ATTACK FACTOR NOT FOUND");
        }
        // 4. RETURN
        return unitHighestF;
    }

    private Card GetCardWithHighestAIFactor()
    {
        Debug.Log("************AIMAINLOOP************* GetCardWithHighestAIFactor() ");
        Card cardHighestF = null;
        int maxF = GetComponent<Player>().playerSO.AICardDeployMinimalFactor;
        foreach (Card card in GetComponent<Player>().GetListCardInHand())
        {
            // CARD ITEM
            if (card.cardSO.cardTypeSO.cardType == CardType.Item)
            {
                foreach (ItemAI itemAI in card.AIItemPos)
                {
                    if (itemAI.unitItemf > maxF)
                    {
                        maxF = itemAI.unitItemf;
                        card.bestItemTarget = itemAI.unit;
                        card.bestItemTargetAIF = itemAI.unitItemf;
                        cardHighestF = card;
                    }
                }
            }
            // CARD UNIT
            else if (card.cardSO.cardTypeSO.cardType == CardType.Unit)
            {
                foreach (SlotAI slotAI in card.AIdeploySlots)
                {
                    if (slotAI.slotDeployf > maxF)
                    {
                        maxF = slotAI.slotDeployf;
                        card.bestTargetSlot = slotAI.slot;
                        card.bestTargetSlotAIF = slotAI.slotDeployf;
                        cardHighestF = card;
                    }
                }
            }
            // CARD ACTION (DONE WITH OTHER LOGIC, WHERE AI FACTOR DATA CONTAINER IS IN OBJECT OF CLASS ACTION AI
            else if (card.cardSO.cardTypeSO.cardType == CardType.Action && card.actionDataAIs.CheckIfAbleToPlay())
            {
                if (card.actionDataAIs.GetBestActionAIF() > maxF)
                {
                    maxF = card.actionDataAIs.GetBestActionAIF();
                    cardHighestF = card;
                }
            }
        }
        // DEBUG LOG:
        if (cardHighestF != null)
        {
            Debug.Log("-- CARD -- RETURN CARD: " + cardHighestF.cardSO.cardName + "  WITH HIGHEST FACTOR OF ALL CARDS ON HAND: " + maxF);
        }
        else
        {
            Debug.Log("-- CARD --RETURN NULL, CARD WITH HIGHEST FACTOR NOT FOUND");
        }
        return cardHighestF;
    }

    public Unit GetUnitOnTheLeftOfSlot(BattleSlot slot)
    {
        //Unit unit = null;
        BattleSlot slotOnTheLeft = slot.GetLeftNeighbour();
        if (slotOnTheLeft != null)
        {
            if (slotOnTheLeft.GetUnit() != null)
            {
                return slotOnTheLeft.GetUnit();
            }
            else
            {
                return null;
            }

        }
        else
        {
            return null;
        }
    }

    public Unit GetUnitOnTheRightOfSlot(BattleSlot slot)
    {
        BattleSlot slotOnTheLeft = slot.GetRightNeighbour();
        if (slotOnTheLeft != null)
        {
            if (slotOnTheLeft.GetUnit() != null)
            {
                return slotOnTheLeft.GetUnit();
            }
            else
            {
                return null;
            }

        }
        else
        {
            return null;
        }
    }

    public List<Unit> GetAllNeighboursOfSlot(BattleSlot slot)
    {
        List<Unit> list = new List<Unit>();
        list.Add(GetUnitOnTheRightOfSlot(slot));
        list.Add(GetUnitOnTheLeftOfSlot(slot));
        return list;
    }

    public List<Unit> GetFriendsFromNeighbourSlots(BattleSlot slot)
    {
        List<Unit> friendList = new List<Unit>();
        List<Unit> unitList = GetAllNeighboursOfSlot(slot);
        foreach (Unit unit in unitList)
        {
            if (unit.cardSO.GetOwner() == GetComponent<Player>())
            {
                friendList.Add(unit);
            }
        } 
        return friendList;
    }

    public List<Unit> GetEnemiesFromNeighbourSlots(BattleSlot slot)
    {
        List<Unit> friendList = new List<Unit>();
        List<Unit> unitList = GetAllNeighboursOfSlot(slot);
        foreach (Unit unit in unitList)
        {
            if (unit.cardSO.GetOwner() != GetComponent<Player>())
            {
                friendList.Add(unit);
            }
        }
        return friendList;
    }

    public List<Unit> GetListOfUnitsCanAttack()
    {
        List<Unit> unitsCanAttack = new List<Unit>();
        foreach (Unit playerUnit in GetComponent<Player>().GetListOfPlayerUnits())
        {
            // Debug.Log("UNIT " + playerUnit.cardSO.cardName);
            if (playerUnit.CheckActive())
            {
                // Debug.Log("CAN MOVE THIS TURN");
                unitsCanAttack.Add(playerUnit);
            }
            // ADD UPKEEP UNITS TO SEE THIS POSIBILITY
            if (playerUnit.cardSO.cardUpkeep > 0 && GetComponent<Player>().playerActGold >= playerUnit.cardSO.cardUpkeep)
            {
                if (!playerUnit.paid)
                {
                    Debug.Log("FOUND UPKEEP UNIT THAT COULD BE PLAYED");
                    unitsCanAttack.Add(playerUnit);
                }
            }
        }
        return unitsCanAttack;
    }

    public List<Unit> GetListOfEnemiesCanAttackNextTurn()
    {
        List<Unit> enemiesCanAttackNextTurn = new List<Unit>();
        foreach(Unit enemy in GetComponent<Player>().GetListOfEnemyUnits())
        {
            // TO DO FREEZE OR OTHER HANDICAP
            enemiesCanAttackNextTurn.Add(enemy);
        }
        return enemiesCanAttackNextTurn;
    }
 
    public int GetOverallEnemyTentDamage()
    {
        int overallDmg = 0;
        List<Unit> unitsCanAttack = GetListOfUnitsCanAttack();
        foreach (Unit unitCanAttack in unitsCanAttack)
        {
            if (unitCanAttack.AICheckIfCanAttackEnemyTent())
            {
                overallDmg += unitCanAttack.GetTentDamage();
            }
        }
        Debug.Log("OVERALL TENT DAMAGE: " + overallDmg);
        return overallDmg;
    }

    public int GetLeftSideMyTentDamage()
    {
        int leftDamage = 0;
        List<Unit> enemiesCanAttack = GetListOfEnemiesCanAttackNextTurn();
        foreach(Unit enemy in enemiesCanAttack)
        {
            if (enemy.CheckSiege())
            {
                leftDamage += enemy.GetTentDamage();
            }
            if (enemy.cardSO.CheckIfHasSpecAb("Maneuver"))
            {
                if (enemy.GetListOfUnitsInLeftEnemyLine().Count == 0 || enemy.GetListOfUnitsInRightEnemyLine().Count == 0)
                {
                    leftDamage += enemy.GetTentDamage();
                }
            }
            if (enemy.GetListOfUnitsInLeftEnemyLine().Count == 0 && enemy.GetSlot().x < 4)
            {
                leftDamage += enemy.GetTentDamage();
            }
        }
        return leftDamage;
    }

    public int GetRightSideMyTentDamage()
    {
        int rightDamage = 0;
        List<Unit> enemiesCanAttack = GetListOfEnemiesCanAttackNextTurn();
        foreach (Unit enemy in enemiesCanAttack)
        {
            if (enemy.CheckSiege())
            {
                rightDamage += enemy.GetTentDamage();
            }
            if (enemy.cardSO.CheckIfHasSpecAb("Maneuver"))
            {
                if (enemy.GetListOfUnitsInRightEnemyLine().Count == 0 || enemy.GetListOfUnitsInLeftEnemyLine().Count == 0)
                {
                    rightDamage += enemy.GetTentDamage();
                }
            }
            if (enemy.GetListOfUnitsInRightEnemyLine().Count == 0 && enemy.GetSlot().x >= 4)
            {
                rightDamage += enemy.GetTentDamage();
            }
        }
        return rightDamage;
    }

    public bool CheckIfMyTentCanBeAttacked()
    {
        bool enemyCanAttackMyTent = false;
        foreach (Unit enemy in GetComponent<Player>().GetListOfEnemyUnits())
        {
            UnitType attackerType = enemy.cardSO.GetUnitType();
            if (attackerType == UnitType.Artillery)
            {
                enemyCanAttackMyTent = true;
            }
            else
            {
                // MANEUVER ATTACK
                if (enemy.cardSO.CheckIfHasSpecAb("Maneuver"))
                {
                    if (enemy.GetListOfUnitsInLeftEnemyLine().Count == 0 || enemy.GetListOfUnitsInRightEnemyLine().Count == 0)
                    {
                        return true;
                    }
                }
                // STANDARD ATTACK CHECK POSITION
                if (enemy.GetSlot().x < 4)
                {
                    if (enemy.GetListOfUnitsInLeftEnemyLine().Count == 0)
                    {
                        enemyCanAttackMyTent = true;
                    }
                }
                if (enemy.GetSlot().x >= 4)
                {
                    if (enemy.GetListOfUnitsInRightEnemyLine().Count == 0)
                    {
                        enemyCanAttackMyTent = true;
                    }
                }
            }
        }
        // TO DO ADD PROTECTION SPEC ABS?
        return enemyCanAttackMyTent;
    }


    // CORE

    public void StartAITurn()
    {
        Debug.Log(".........STARTING AI......");
        StartCoroutine(AILoop());
    }

    // AI ACTION
    private void AISetCardAIAction()
    {
        Debug.Log("-----* ACTION *---- Start AI SET ACTION CARDAIF");
        // RESET AIACTION
        foreach (Card cardInHand in GetComponent<Player>().GetListCardInHand())
        {
            cardInHand.ClearAIAction();
        }
        // CHECK GOLD
        int gold = GetComponent<Player>().playerActGold;
        // 1. LIST OF ACTIONS THAT CAN AFFORD
        List<Card> actionsCanAfford = new List<Card>();
        foreach(Card cardToPlay in GetComponent<Player>().GetListCardInHand())
        {
            if (cardToPlay.cardSO.cardCost <= gold)
            {
                if (cardToPlay.cardSO.cardTypeSO.cardType == CardType.Action)
                {
                    actionsCanAfford.Add(cardToPlay);
                }
            }
        }
        // 2. SETTING AIACTION FOR EVERY ACTION CARD 
        foreach (Card actionToPlay in actionsCanAfford)
        {
            ActionTypeSO actionTypeSO = actionToPlay.cardSO.cardTypeSO as ActionTypeSO;
            // 2.1. SETTING AI ACTION FOR ACTION TO PLAY ON UNIT
            if (actionToPlay.cardSO.GetActionPlayMethod() == ActionPlayMethod.OnUnit)
            {
                if (actionTypeSO.enemyUnits)
                {
                    // 2.1.1. TO PLAY ON ENEMY UNIT
                    List<Unit> enemyUnits = GetComponent<Player>().GetListOfEnemyUnits();
                    foreach (Unit enemyUnit in enemyUnits)
                    {
                        // CHECK COMPATIBILITY
                        if (actionTypeSO.CheckTargetUnitCompatibility(enemyUnit) && actionTypeSO.CheckTargetNationCompatibility(enemyUnit))
                        {
                            // SET OBJECT ACTIONAI (setting only, posibilities is when list of UnitAI has more than 0 elements.
                            actionToPlay.actionDataAIs.SetUnitAI(enemyUnit);
                            // IF IS DAMAGE UNIT
                            if (actionTypeSO.isDamageUnit)
                            {
                                // 2.1.1.1.SET FACTOR FOR OFFENSIVE ACTIONS (DAMAGE)
                                int actionDmg = Mathf.Abs(actionToPlay.cardSO.baseDef);
                                Debug.Log("ACTION: " + actionToPlay.cardSO.cardName + ". ADDING ACTION AIF FOR ACTION DAMAGE: " + actionDmg +", ON UNIT: " + enemyUnit.cardSO.cardName);
                                // 2.1.1.1.1. ADD BASE Def
                                actionToPlay.actionDataAIs.ChangeUnitAIF(enemyUnit, actionDmg);
                                // 2.1.1.1.2. IF KILL UNIT and if equals its damage ADD bonus of real card cost
                                if (enemyUnit.cardSO.GetDef() == actionDmg)
                                {
                                    actionToPlay.actionDataAIs.ChangeUnitAIF(enemyUnit, enemyUnit.cardSO.GetRealCardCost());
                                }
                                // 2.1.1.1.3. IF DONT KILL UNIT -1
                                if (enemyUnit.cardSO.GetDef() > actionDmg)
                                {
                                    actionToPlay.actionDataAIs.ChangeUnitAIF(enemyUnit, -1);
                                }
                            }
                            // IF IS DESTROY UNIT
                            if (actionTypeSO.isDestroyUnit)
                            {
                                // 2.1.1.2. SET FACTOR FOR OFFENSIVE ACTIONS (DESTROY)
                                Debug.Log("ACTION: " + actionToPlay.cardSO.cardName + ". ADDING ACTION AIF FOR ACTION DESTROY UNIT: " + enemyUnit.cardSO.cardName);
                                // 2.1.1.2.1. ADD DESTROYED CARD COST AS AIF:
                                actionToPlay.actionDataAIs.ChangeUnitAIF(enemyUnit, enemyUnit.cardSO.GetRealCardCost());
                                if (enemyUnit.cardSO.GetDef() >= enemyUnit.cardSO.baseDef)
                                {
                                    // 2.1.1.2.2. ADD BONUS +1 if card is fully health or boosted
                                    actionToPlay.actionDataAIs.ChangeUnitAIF(enemyUnit, 3);
                                }
                            }
                        }
                    }
                }
                if (actionTypeSO.myUnits)
                {
                    // 2.1.2. TO PLAY ON OWN UNITS
                    List<Unit> ownUnits = GetComponent<Player>().GetListOfPlayerUnits();
                    foreach (Unit myUnit in ownUnits)
                    {
                        // CHECK COMPATIBILITY AND ADD!
                        if (actionTypeSO.CheckTargetUnitCompatibility(myUnit) && actionTypeSO.CheckTargetNationCompatibility(myUnit))
                        {
                            // SET ACTIONAI OBJECT
                            actionToPlay.actionDataAIs.SetUnitAI(myUnit);
                            // TO DO (FOR OWN UNITS ACTIONS (bonus actions)
                        }
                    }
                }
            }
            // 2.2. SETTING AI ACTION FOR ACTION TO PLAY ON ENEMY LINE
            if (actionToPlay.cardSO.GetActionPlayMethod() == ActionPlayMethod.OnLine)
            {
                if (actionTypeSO.enemyLineAction)
                {
                    // 2.2.1 TO PLAY ON ENEMY LINE (for example: tatar invasion)
                    actionToPlay.actionDataAIs.SetLineAI();                
                    foreach (Unit enemyUnit in GetComponent<Player>().GetListOfEnemyUnits())
                    {
                        // CHECK COMPATIBILITY AND SET ACTION AI DATA
                        if (actionTypeSO.CheckTargetUnitCompatibility(enemyUnit) && actionTypeSO.CheckTargetNationCompatibility(enemyUnit))
                        {
                            actionToPlay.actionDataAIs.canPlayOnEnemyLine = true;
                            // 2.2.1.1. CHECK ENEMY DAMAGE AND ADD +1 for every 1 Damage made
                            if (enemyUnit.cardSO.GetDef() > Mathf.Abs(actionToPlay.cardSO.baseDef))
                            {
                                actionToPlay.actionDataAIs.ChangeEnemyLineAIF(Mathf.Abs(actionToPlay.cardSO.baseDef));
                                
                            }
                            // 2.2.1.2. CHECK IF CAN KILL A UNIT ADD REAL COST
                            if (enemyUnit.cardSO.GetDef() <= Mathf.Abs(actionToPlay.cardSO.baseDef))
                            {
                                actionToPlay.actionDataAIs.ChangeEnemyLineAIF(enemyUnit.cardSO.GetRealCardCost());
                            }
                        }
                    }
                    Debug.Log("ACTION: " + actionToPlay.cardSO.cardName + ". OVERALL AIF " + actionToPlay.actionDataAIs.enemyLineAIF);
                }
                if (actionTypeSO.ownerLineAction)
                {
                    // 2.2.2. TO PLAY ON OWN LINE (bogurodzica for example)
                    // 2.2.1 TO PLAY ON ENEMY LINE (for example: tatar invasion)
                    actionToPlay.actionDataAIs.SetLineAI();
                    foreach (Unit ownUnit in GetComponent<Player>().GetListOfPlayerUnits())
                    {
                        // CHECK COMPATIBILITY AND SET ACTION AI DATA
                        if (actionTypeSO.CheckTargetUnitCompatibility(ownUnit) && actionTypeSO.CheckTargetNationCompatibility(ownUnit))
                        {
                            actionToPlay.actionDataAIs.canPlayOnOwnLine = true;
                            // TO DO LOGIC
                        }
                    }
                    // TO DO!
                }  
            }
            // 2.3. TO DO SETTING AI ACTION FOR ACTION TO PLAY ON PLAYER
            // 2.4. TO DO SETTING AI ACTION FOR GLOBAL ACTION
            // 2.5. TO DO SETTING AI ACTION FOR EVERY SLOT ACTION (If will be created)
        }
    }

    // AI ITEM

    private void AISetCardsAIItemPos()
    {
        Debug.Log("ITEM ITEM ITEM ITEM ITEM IIIIIIIiiiiii Start AISetCardsAIItemPos iiiiiiIIIIIIIIIIIIITEM ITEM ITEM ITEM ITEM");
        // RESET AIITEMPOS
        foreach (Card cardInHand in GetComponent<Player>().GetListCardInHand())
        {
            cardInHand.ClearAIItemPos();
        }
        // CHECK GOLD
        int gold = GetComponent<Player>().playerActGold;
        // 1. LIST OF ITEMS THAT I CAN AFFORD
        List<Card> itemsCanAfford = new List<Card>();
        foreach(Card cardToPlay in GetComponent<Player>().GetListCardInHand())
        {
            if (cardToPlay.cardSO.cardCost <= gold)
            {
                if (cardToPlay.cardSO.cardTypeSO.cardType == CardType.Item)
                {
                    itemsCanAfford.Add(cardToPlay);
                }
            }
        }
        // 2. SETTING AIITEMPOS FOR EVERY ITEM CARD
        foreach(Card itemToPlay in itemsCanAfford)
        {
            List<Unit> myUnits = GetComponent<Player>().GetListOfPlayerUnits();
            foreach (Unit unitToPlayItem in myUnits) 
            {
                // ONLY ON COMPATIBLE UNITS
                ItemTypeSO itemTypeSO = itemToPlay.cardSO.cardTypeSO as ItemTypeSO;
                UnitTypeSO unitTypeSO = unitToPlayItem.cardSO.cardTypeSO as UnitTypeSO;
                if (itemTypeSO.compatibilityList.Contains(unitTypeSO.unitType))
                {
                    // Debug.Log("COMPATIBLE ITEM FOUND");
                    itemToPlay.AddPlayableItemToPlay(unitToPlayItem);
                }
            }
            // 3. START ITEMPOS LOGIC
            foreach (ItemAI itemAI in itemToPlay.AIItemPos)
            {
                Debug.Log("ANALYZE CARD(ITEM):" + itemToPlay.cardSO.cardName + " to play on the:" + itemAI.unit.cardSO.cardName);
                // TO DO CHECK IF PROTECTION BY WEST OR EAST UNIT
                // Unit eastUnit = itemAI.unit.GetEastUnit();
                // Unit westUnit = itemAI.unit.GetWestUnit();
                // LISTS
                List<Unit> enemiesCanAttackMe = itemAI.unit.GetListOfUnitsCanAttackMe();
                List<Unit> enemiesICanAttack = itemAI.unit.GetListOfMyPosibleTargets();
                List<Unit> enemySiegeUnits = itemAI.unit.GetListOfEnemySiegeUnits();
                int bonusAtt = itemToPlay.cardSO.baseAtt;
                int bonusDef = itemToPlay.cardSO.baseDef;
                // 1. ADD CARD COST +?
                itemAI.ChangeUnitf(itemToPlay.cardSO.cardCost);
                // 2. ADD SOME RANDOM
                itemAI.ChangeUnitf(Random.Range(0, itemToPlay.cardSO.cardCost));
                // Debug.Log("FIRST BONUS RANDOM: " + itemAI.unitItemf);
                // 3. ITEMS TO ANALYSE WHEN PLAYER ATTACKS ME - TO DO FIRST STRIKE AND MORE SPEC ABS!
                int unitfProtectionBonus = 0;
                int enemiesInRange = 0;
                int enemiesICanKillDefendingWithItem = 0;
                bool myUnitWillDie = false;
                // 3.1 ANALYSING IF ENEMY WILL ATTACK ME (NEXT TURN)
                foreach (Unit enemy in enemiesCanAttackMe)
                {
                    // DEFINITION OF VARIABLES
                    enemiesInRange++;
                    int specAbAttBonus = 0;
                    //int specAbDefBonus = 0; // NIE UZYWANA BO JESZCZE NIE MA TAKIEJ SPECAB
                    // SET SPEC AB BONUS
                    // 1. PIKE
                    if (itemAI.unit.cardSO.CheckIfHasSpecAb("Pike"))
                    {
                        int pikeBonus = itemAI.unit.cardSO.GetSpecAbByName("Pike").cavalryDefPenalty;
                        UnitTypeSO enemyUnitTypeSO = enemy.cardSO.cardTypeSO as UnitTypeSO;
                        if (enemyUnitTypeSO.unitType == UnitType.Cavalry)
                        {
                            specAbAttBonus += pikeBonus;
                        }
                    }
                    // 2. MORE SPEC ABS IF CREATED
                    // CHECK IF CAN BE KILLED BY ENEMY
                    if (enemy.cardSO.GetAtt() >= itemAI.unit.cardSO.GetDef())
                    {
                        //Debug.Log("ANALYSING ITEM: " + itemToPlay.cardSO.cardName + ". FOUND ENEMY: "+ enemy.cardSO + " THAT CAN KILL UNIT " + itemAI.unit.cardSO.cardName);
                        if (enemy.cardSO.GetAtt() < itemAI.unit.cardSO.GetDef() + bonusDef)
                        {
                            Debug.Log("THE ENEMY CANT KILL ME WITH ITEM PLACED - ADD BONUS: " + itemAI.unit.cardSO.cardCost + " +2");
                            enemiesICanKillDefendingWithItem++;
                            unitfProtectionBonus += itemAI.unit.cardSO.cardCost + 2;
                            if (enemy.cardSO.CheckIfHasSpecAb("First Strike"))
                            {
                                unitfProtectionBonus += 2;
                            }
                        }
                        else
                        {
                            Debug.Log("THE ENEMY CAN KILL ME EVEN WITH ITEM PLACED");
                            myUnitWillDie = true;
                        }
                    }
                    else
                    {
                        // NOONE WILL KILL ME EITHER - Add simple bonus
                        unitfProtectionBonus += 1;
                    }
                    // CHECK IF I CAN KILL ENEMY ATTACKING ME
                    if (enemy.cardSO.GetDef() > itemAI.unit.cardSO.GetAtt() + specAbAttBonus)
                    {
                        if (enemy.cardSO.GetDef() <= itemAI.unit.cardSO.GetAtt() + specAbAttBonus + bonusAtt)
                        {
                            Debug.Log("Thanks To " + itemToPlay.cardSO.cardName + " I can kill " + enemy.cardSO.cardName + " when defending");
                            unitfProtectionBonus += enemy.cardSO.cardCost;
                        }
                    }
                }
                if (myUnitWillDie)
                {
                    Debug.Log("MY UNIT CAN BE KILLED. REMOVE (COST OF ITEM + 1 )FROM UNITFPROTECTION BONUS");
                    unitfProtectionBonus -= (itemToPlay.cardSO.cardCost + 1);
                    if (enemiesICanKillDefendingWithItem > 0)
                    {
                        unitfProtectionBonus += 3;
                    }
                }
                Debug.Log("ANALYSING ITEM " + itemToPlay.cardSO.cardName + " DEFENSIVE FACTOR: OVERALL BONUS: " + (unitfProtectionBonus));
                itemAI.ChangeUnitf(unitfProtectionBonus);
                // 4. OFFENSIVE WEAPONS - ADD TO UNIT TO BE ABLE TO KILL ENEMY UNIT (more expensive - better) TO DO FIRST STRIKE AND SPEC ABS!
                int unitfAttackBonus = 0;
                foreach(Unit enemy in enemiesICanAttack)
                {
                    // CHECK IF ENEMY WILL BE KILLED IF I WILL ATTACK
                    if (enemy.cardSO.GetDef() > itemAI.unit.cardSO.GetAtt())
                    {
                        if (enemy.cardSO.GetDef() <= itemAI.unit.cardSO.GetAtt() + bonusAtt)
                        {
                            Debug.Log("MY UNIT: " + itemAI.unit.cardSO.cardName + " CAN KILL ENEMY: " + enemy.cardSO.cardName + " THANKS TO ITEM:" + itemToPlay.cardSO.cardName);
                            unitfAttackBonus += enemy.cardSO.cardCost + bonusAtt;
                        }
                        else
                        {   Debug.Log("MY UNIT: " + itemAI.unit.cardSO.cardName + " CAN NOT KILL ENEMY: " + enemy.cardSO.cardName + " WITH ITEM:" + itemToPlay.cardSO.cardName);
                            unitfAttackBonus += 1;
                        }
                    }
                    else
                    {
                        Debug.Log("MY UNIT: " + itemAI.unit.cardSO.cardName + " CAN KILL ENEMY: " + enemy.cardSO.cardName + " WITH OR WITHOUT ITEM:" + itemToPlay.cardSO.cardName);
                        unitfAttackBonus -= bonusAtt;
                    }
                }
                if (!itemAI.unit.CheckActive())
                {
                    // NIE DAJE OFENSYWNEJ BRONI JEDNOSTCE NIEAKTYWNEJ
                    if (itemToPlay.cardSO.CheckOneAttackTempEffect() == 1)
                    {
                        Debug.Log("ITEM FOR ONLY 1 ATTACK, IF IS OFFENSIVE Wont play on inactive unit! (-6)");
                        if (bonusAtt > 0)
                        {
                            itemAI.ChangeUnitf(-4);
                        }
                    }
                    // NIE DAJE NIETRWALEGO (1 TURA) EFECKTU JEDNOSTCE NIEAKTYWNEJ
                    if (itemToPlay.cardSO.CheckTurnTempEffect() == 1)
                    {
                        Debug.Log("ITEM FOR ONLY 1 TURN, IF IS OFFENSIVE Wont play on inactive unit! (-8)");
                        if (bonusAtt > 0)
                        {
                            itemAI.ChangeUnitf(-8);
                        }
                    }
                }              
                Debug.Log("ANALYSING ITEM " + itemToPlay.cardSO.cardName + " OFFENSIVE FACTOR: OVERALL BONUS: " + (unitfAttackBonus));
                itemAI.ChangeUnitf(unitfAttackBonus);
                // 5.1 REMOVE SOME POINTS IF ITEM IS OFFENSIVE AND MY UNIT CANNOT ATTACK ENEMY NOR TENT, ADD SOME POINTS FOR TENT ATTACK.
                if (enemiesICanAttack.Count == 0 && !itemAI.unit.CheckActive())
                {   
                    if (bonusAtt > 0)
                    {
                        itemAI.ChangeUnitf(-3);
                        itemAI.ChangeUnitf(-bonusAtt);
                        Debug.Log("OFFENSIVE WEAPON NOT NEEDED, UNIT ALREADY ATTACKED, AND THERE IS NO OTHER UNIT!");
                    }
                }
                else if (enemiesICanAttack.Count > 0 && !itemAI.unit.CheckActive())
                {
                    if (bonusAtt > 0)
                    {
                        itemAI.ChangeUnitf(-1);
                        itemAI.ChangeUnitf(-bonusAtt);
                    }
                    if (bonusDef > 0)
                    {
                        itemAI.ChangeUnitf(bonusDef);
                    }
                }
                else if (enemiesICanAttack.Count == 0 && itemAI.unit.CheckActive())
                {
                    if (bonusAtt > 0)
                    {
                        itemAI.ChangeUnitf(bonusAtt);
                    }
                }
                else if (enemiesICanAttack.Count > 0)
                {
                    if (bonusDef > 0)
                    {
                        itemAI.ChangeUnitf(bonusDef);
                    }
                }
                // 6. REMOVE SOME POINTS IF CAN BE KILLED BY ARTILLERY WITH ITEM
                foreach (Unit enemySiege in enemySiegeUnits)
                {
                    if (enemySiege.cardSO.GetAtt() >= itemAI.unit.cardSO.GetDef() + bonusDef)
                    {
                        itemAI.ChangeUnitf(-1);
                    }
                }
                // 7. ADD SOME POINTS TO UNITS WITH SPEC ABS.: PIKE
                if (itemAI.unit.cardSO.CheckIfHasSpecAb("Pike"))
                {
                    itemAI.ChangeUnitf(1);
                }
                // 8. ADD SOME POINTS TO UNIT WITH FIRST STRIKE
                if (itemAI.unit.cardSO.CheckIfHasSpecAb("First Strike"))
                {
                    itemAI.ChangeUnitf(2);
                }
                // 9. ADD DECISIVE POINTS IF UNIT IS ACTIVE, AND WITH OFFENSIVE WEAPON CAN END GAME (+110);
                int overallTentDamage = GetOverallEnemyTentDamage();
                if ((overallTentDamage + bonusAtt) >= GameManager.instance.GetOtherPlayer(GetComponent<Player>()).playerActMorale)
                {
                    if (bonusAtt > 0)
                    {
                        itemAI.ChangeUnitf(110);
                        Debug.Log("ADDING ITEM TO MAKE A DECISIVE ATTACK!");
                    }
                }
                Debug.Log("END ANALYZE CARD(ITEM): " + itemToPlay.cardSO.cardName + " to play on the: " + itemAI.unit.cardSO.cardName + ". RESULT OF ITEM UNITF: " + itemAI.unitItemf);
            }
        }
        // Debug.Log("AISetCardsAIItemPos()");
        Debug.Log("ITEM ITEM ITEM ITEM ITEMIIIIIIIIIIIII DEBUG LIST OF AIITEMF RESULTS: IIIIIIIIIIIIIIIIITEM ITEM ITEM ITEM ITEM");
        if (itemsCanAfford.Count > 0)
        {
            foreach (Card itemToPlayDebug in itemsCanAfford)
            {
                if (itemToPlayDebug.AIItemPos.Count > 0)
                {
                    List<ItemAI> sortedAIList = itemToPlayDebug.AIItemPos.OrderByDescending(o => o.unitItemf).ToList();
                    Debug.Log("CARD:" + itemToPlayDebug.cardSO.cardName + " has Highest slotDeployf : " + sortedAIList[0].unitItemf);
                }
                else
                {
                    Debug.Log("NO ITEMS PLAYABLE!");
                }
            }
        }
        //Debug.Log("-----> END AISetCardsAIItemPos <------");
    }
    
    // AI DEPLOY LOGIC

    private void AISetCardsAIDeployf()
    {
        Debug.Log("DDDDDDDDDDddddddd Start AISETCARDSAIDEPLOYF dddddddddddDDDDDDDDDDDDD");
        // RESET AISLOTS
        foreach (Card cardInHand in GetComponent<Player>().GetListCardInHand())
        {
            cardInHand.ClearAISlots();
        }
        // CHECK GOLD
        int gold = GetComponent<Player>().playerActGold;
        // 1. UNITS = TEMP LIST OF UNIT THAT I CAN AFFORD
        List<Card> cardsCanAfford = new List<Card>();
        foreach (Card cardToPlay in GetComponent<Player>().GetListCardInHand())
        {
            if (cardToPlay.cardSO.cardCost <= gold)
            {
                if (cardToPlay.cardSO.cardTypeSO.cardType == CardType.Unit)
                {
                    cardsCanAfford.Add(cardToPlay);
                }
            }
        }
        // SETTING SLOTS OF EVERY UNIT
        foreach (Card cardToPlay in cardsCanAfford)
        {
            // Debug.Log("------------ ANALYSE " + cardToPlay.cardSO.cardName + " ---------------");
            // GET LIST OF SLOT CAN BE PLAYED CARD.
            List<BattleSlot> deploySlots = GetComponent<Player>().GetListOfDeploySlots();
            foreach (BattleSlot slot in deploySlots)
            {
                // ONLY ON FREE SLOTS!
                if (!slot.CheckIfOccupied())
                {
                    cardToPlay.AddDeployableSlotAI(slot);
                }
            }
            // FIRST LOGIC - CANOT BE ATTACKED AND KILLED NEXT TURN
            foreach (SlotAI slotAI in cardToPlay.AIdeploySlots)
            {
                // SET LOCAL VARIABLE UNITTOPLAY
                UnitTypeSO unitToPlay = cardToPlay.cardSO.cardTypeSO as UnitTypeSO;
                // TO FURTHER SPEC ABS (PROTECTION)
                Unit eastUnit = GetUnitOnTheLeftOfSlot(slotAI.slot);
                Unit westUnit = GetUnitOnTheRightOfSlot(slotAI.slot);
                // LOCAL VARIABLES RESET
                int allAttackersAtt = 0;
                int allAttackersCost = 0;
                int howManyAttackersCanAttackMe = 0;
                // 1. ADD BONUS +1 IF CARD HAS COST OF MAX GOLD
                if (cardToPlay.cardSO.cardCost == GetComponent<Player>().playerActGold)
                {
                    slotAI.ChangeSlotDeployf(1);
                }
                // 2. ADD SOME RANDOM (0 TO MAX COST - MORE LIKELY PLAY MORE EXPENSIVE CARDS)
                int rand = Random.Range(0, cardToPlay.cardSO.cardCost);
                slotAI.ChangeSlotDeployf(rand);
                // 3. IF LEFT OR RIGHT LINE NOT PROTECTED AND TENT CAN BE ATTACKED
                if (slotAI.slot.x < 4)
                {
                    if (GetComponent<Player>().GetListOfMyUnitsInRightSide().Count > 0)
                    {
                        if (GetComponent<Player>().GetListOfMyUnitsInLeftSide().Count == 0)
                        {
                            if (unitToPlay.unitType != UnitType.Artillery)
                            {
                                slotAI.ChangeSlotDeployf(1);
                                //Debug.Log(cardToPlay.cardSO.cardName + " has DEPLOY BONUS - +1, TO PROTECT LEFT SIDE");
                                if (GetLeftSideMyTentDamage() > 0)
                                {
                                    slotAI.ChangeSlotDeployf(3);
                                    //Debug.Log(cardToPlay.cardSO.cardName + "DEPLOY BONUS - +4, TO PROTECT LEFT SIDE, CAN BE ATTACKED");
                                } 
                            }
                            if (GetLeftSideMyTentDamage() >= GetComponent<Player>().playerActMorale)
                            {
                                slotAI.ChangeSlotDeployf(10);
                                //Debug.Log(cardToPlay.cardSO.cardName + "CRITICAL SITUATION DEPLOY BONUS - +10, TO PROTECT LEFT SIDE, CAN BE ATTACKED AND I WILL LOSE THE GAME");
                            }
                        }
                    }
                }
                else if (slotAI.slot.x >= 4)
                {
                    if (GetComponent<Player>().GetListOfMyUnitsInLeftSide().Count > 0)
                    {
                        if (GetComponent<Player>().GetListOfMyUnitsInRightSide().Count == 0)
                        {
                            if (unitToPlay.unitType != UnitType.Artillery)
                            {
                                slotAI.ChangeSlotDeployf(1);
                                //Debug.Log(cardToPlay.cardSO.cardName + "DEPLOY BONUS - +1, TO PROTECT RIGHT SIDE");
                                if (GetRightSideMyTentDamage() > 0)
                                {
                                    slotAI.ChangeSlotDeployf(4);
                                    //Debug.Log(cardToPlay.cardSO.cardName + "DEPLOY BONUS - +4, TO PROTECT RIGHT SIDE, CAN BE ATTACKED");
                                }
                            }
                            if (GetRightSideMyTentDamage() >= GetComponent<Player>().playerActMorale)
                            {
                                slotAI.ChangeSlotDeployf(10);
                                //Debug.Log(cardToPlay.cardSO.cardName + "CRITICAL SITUATION DEPLOY BONUS - +14, TO PROTECT RIGHT SIDE, CAN BE ATTACKED AND I WILL LOSE THE GAME");
                            }
                        }
                    }
                }
                // 4. IF HAS NEIGHBOURS (FRIENDS) - ADD SOME POINTS FOR ARTILLERY AND INFANTRY
                // 4.1. ARTILLERY - BONUS FOR FRIEND WITH PROTECTION WEST OR EAST (TO DO IF HAS PROTECTION)
                if (unitToPlay.unitType == UnitType.Artillery)
                { 
                    if (eastUnit != null)
                    {
                        if (eastUnit.cardSO.CheckIfHasSpecAb("Protection"))
                        {
                            Debug.Log("DEPLOY PROTECTION BONUS TO ARTILLERY");
                            slotAI.ChangeSlotDeployf(2);
                        }
                    }
                    if (westUnit != null)
                    {
                        if (westUnit.cardSO.CheckIfHasSpecAb("Protection"))
                        {
                            Debug.Log("DEPLOY PROTECTION BONUS TO ARTILLERY");
                            slotAI.ChangeSlotDeployf(2);
                        }
                    }
                }
                // 4.2. CAVALRY - BONUS FOR FOR FRIEND WITH PROTECTION WEST OR EAST (TO DO IF HAS PROTECTION)
                else if (unitToPlay.unitType == UnitType.Cavalry)
                {
                    if (eastUnit != null)
                    {
                        if (eastUnit.cardSO.CheckIfHasSpecAb("Protection"))
                        {
                            Debug.Log("DEPLOY PROTECTION BONUS TO CAVALRY PROTECTED BY OTHER UNIT");
                            slotAI.ChangeSlotDeployf(1);
                        }
                    }
                    if (westUnit != null)
                    {
                        if (westUnit.cardSO.CheckIfHasSpecAb("Protection"))
                        {
                            Debug.Log("DEPLOY PROTECTION BONUS TO CAVALRY PROTECTED BY OTHER UNIT");
                            slotAI.ChangeSlotDeployf(1);
                        }
                    }
                }
                // 4.3. UNIT WITH PROTECTION
                else if (cardToPlay.cardSO.CheckIfHasSpecAb("Protection"))
                {
                    if (eastUnit != null)
                    {
                        if (eastUnit.cardSO.GetUnitType() == UnitType.Artillery)
                        {
                            Debug.Log("DEPLOY UNIT WITH PROTECTION, NEAR ARTILLERY UNIT");
                            slotAI.ChangeSlotDeployf(2);
                        }
                    }
                    if (westUnit != null)
                    {
                        if (westUnit.cardSO.GetUnitType() == UnitType.Artillery)
                        {
                            Debug.Log("DEPLOY UNIT WITH PROTECTION, NEAR ARTILLERY UNIT");
                            slotAI.ChangeSlotDeployf(2);
                        }
                    }
                }
                // 5.CHECK EACH SLOT. IF CAN BE ATTACKED AND IF CAN BE KILLED - reduce points, IF CAN KILL DEFENDING - add points
                // Debug.Log("ANALYZE SLOT: " + slotAI.slot.x +","+ slotAI.slot.y);
                List<Unit> unitsCanAttackSlot = slotAI.slot.GetListOfUnitsCanAttackThisSlot();
                bool anyPlayerCanKillMe = false;
                bool anyPlayerCanBeKilledIfAttacksMe = false;
                foreach(Unit unitCanAttack in unitsCanAttackSlot)
                {
                    //Debug.Log("FOUND ENEMY UNIT CAN ATTACK THE SLOT");
                    //Debug.Log("SLOT: " + slotAI.slot.x + "," + slotAI.slot.y + "IN DANGER OF ATTACK OF " + unitCanAttack.cardSO.cardName);
                    allAttackersAtt += unitCanAttack.cardSO.GetAtt();
                    allAttackersCost += unitCanAttack.cardSO.cardCost;
                    howManyAttackersCanAttackMe++;
                    UnitTypeSO unitTypeSO = unitCanAttack.cardSO.cardTypeSO as UnitTypeSO;
                    // 5.1. CHECK IF PLAYER CAN KILL ME
                    if (cardToPlay.cardSO.CheckIfAttackerSOCanKillMe(unitCanAttack))
                    {
                        //Debug.Log("NA POLU " + slotAI.slot.x + "," + slotAI.slot.y +" ZABIJE MNIE: " + unitCanAttack.cardSO.cardName);
                        slotAI.ChangeSlotDeployf(-1);
                        anyPlayerCanKillMe = true;
                    }
                    // 5.2. CHECK IF I CAN KILL PLAYER IF ATTACKS ME  
                    if (cardToPlay.cardSO.CheckIfAttackerSOWillBeKilled(unitCanAttack))
                    {
                        //Debug.Log("NA POLU " + slotAI.slot.x + "," + slotAI.slot.y + " MOGE ZABIC: " + unitCanAttack.cardSO.cardName + " JEZELI MNIE ZAATAKUJE");
                        anyPlayerCanBeKilledIfAttacksMe = true;
                        // ADD cost pts, UNIT CAN KILL WHEN ATTACKED BY OTHER UNIT'
                        slotAI.ChangeSlotDeployf(1);
                        // IF HAS SPEC ABS BONUS - ADD MORE POINTS! ( TO DO?)
                        if (unitCanAttack.cardSO.CheckIfHasSpecAb("First Strike"))
                        {
                            slotAI.ChangeSlotDeployf(2);
                        }
                        if (unitCanAttack.cardSO.CheckIfHasSpecAb("Maneuver"))
                        {
                            slotAI.ChangeSlotDeployf(1);
                        }
                    }
                    // 5.3. FIRST STRIKE (IF POSIBLE TO PUT OTHER UNIT IN FRONT OF UNIT WITH FIRST STRIKE)
                    if (unitCanAttack.cardSO.CheckIfHasSpecAb("First Strike"))
                    {
                        int unitsInLine = slotAI.slot.GetListOfUnitsInLine().Count;
                        if (unitsInLine > 0)
                        {
                            slotAI.ChangeSlotDeployf(1);
                        }
                    }
                    // 5.4. IF HAS CHARGE AND CAN ATTACK AND KILL MORE EXPENSIVE UNIT ADD BONUS OF HIGHEST COST OF UNIT
                    if (cardToPlay.cardSO.CheckIfHasSpecAb("Charge"))
                    {
                        int highestCostOfEnemyICanKill = 0;
                        List<Unit> enemiesICanAttack = slotAI.slot.GetListOfUnitsICanAttackFromThisSlot(cardToPlay.cardSO);
                        foreach (Unit enemy in enemiesICanAttack)
                        {
                            if (enemy.cardSO.GetDef() <= cardToPlay.cardSO.GetAtt())
                            {   
                                if (highestCostOfEnemyICanKill < enemy.cardSO.GetRealCardCost())
                                {
                                    highestCostOfEnemyICanKill = enemy.cardSO.GetRealCardCost();
                                }
                            }
                        }
                        slotAI.ChangeSlotDeployf(highestCostOfEnemyICanKill);
                        // 5.5. IF HAS CHARGE AND CAN ATTACK ENEMY TENT:
                        if (slotAI.slot.CheckIfICanAttackEnemyTentFromThisSlot(cardToPlay.cardSO))
                        {
                            slotAI.ChangeSlotDeployf(cardToPlay.cardSO.baseAtt);
                        }
                    }

                }
                // 6. CHECK IF ANY ONE CAN KILL ME
                if (howManyAttackersCanAttackMe > 0 && allAttackersAtt < cardToPlay.cardSO.baseDef)
                {
                    //Debug.Log("NA POLU " + slotAI.slot.x + "," + slotAI.slot.y + "NIKT MNIE NIE ZABIJE! +1 do pola");
                    slotAI.ChangeSlotDeployf(1);
                }
                // 7. MODIFY FACTOR IF CAN BE KILLED
                if (anyPlayerCanKillMe)
                {
                    slotAI.ChangeSlotDeployf(-cardToPlay.cardSO.cardCost);
                }
                // 8. MODIFY FACTOR IF I CAN KILL OTHER UNIT
                if (anyPlayerCanBeKilledIfAttacksMe)
                {
                    slotAI.ChangeSlotDeployf(2);
                }
            }
            // DEBUG. LIST ALL SLOTS AI OF UNIT
            //Debug.Log("------------- RESUME OF UNIT DEPLOY ON SLOT :");
            foreach (SlotAI slotAI in cardToPlay.AIdeploySlots)
            {
                // Debug.Log("SLOT x:" + slotAI.slot.x + ", y:" + slotAI.slot.y+ " can be deployed a card " + cardToPlay.cardSO.cardName + "with AIDeployFactor: "+ slotAI.slotDeployf);
            }

        }
        // DEBUG. LIST OF ALL AISLOTDEPLOYF RESULTS:
        Debug.Log("DDDDDDDDDDDDDDDDDDDdddd----- DEBUG LIST OF AISLOT DEPLOYF RESULTS: -----dddddDDDDDDDDDDDDDDDDDDDD");
        if (cardsCanAfford.Count> 0)
        {
            foreach (Card cardToPlayDebug in cardsCanAfford)
            {
                List<SlotAI> sortedSlotAIList = cardToPlayDebug.AIdeploySlots.OrderByDescending(o => o.slotDeployf).ToList();
                Debug.Log("CARD:" + cardToPlayDebug.cardSO.cardName + " has Highest slotDeployf : " + sortedSlotAIList[0].slotDeployf);
            }
        }
        //Debug.Log("-----> END AISetCardsAIDeployf <------");
    }

    // AI PLACE UNIT
    public void AIPlaceUnitOnSlotWithHighestDeployf(Card cardToPlace)
    {
        BattleSlot slotOfHighestDeployfOfUnit = cardToPlace.bestTargetSlot;
        if (slotOfHighestDeployfOfUnit != null)
        {
            // CHECK IF SLOT IS OCCUPIED (nie powinien byc zajety, bo lista AISlots dla danej jednostki to tylko wolne sloty!
            if (slotOfHighestDeployfOfUnit.CheckIfOccupied())
            {
                Debug.LogWarning("ERROR - AI TRYING TO PUT UNIT ON OCCUPIED SLOT!");
            }
            // PLAY UNIT CARD ON FREE SLOT
            else
            {
                Debug.Log("PLACE UNIT:" + cardToPlace.cardSO.cardName + " ON SLOT " + slotOfHighestDeployfOfUnit.x + "," + slotOfHighestDeployfOfUnit.y);
                // PAY GOLD!
                GameManager.instance.actPlayer.playerActGold -= cardToPlace.cardSO.cardCost;
                // CREATE UNIT
                AIPlayUnitOnSlot(cardToPlace.cardSO, slotOfHighestDeployfOfUnit);
                // SEND CARD GO TO PLAYED CONTAINER
                cardToPlace.UnsubscribeEvents();
                cardToPlace.cardSO.GetOwner().SendCardToPlayedContainer(cardToPlace.gameObject);
                cardToPlace.gameObject.SetActive(false);
                // REFRESH VISUALS
                GameManager.instance.actPlayer.RefreshAttributeVisuals();
                Debug.Log("AIPlaceUnitOnSlotWithHighestDeployf... COMPLETED");
            }
        }
        else
        {
            Debug.LogWarning("SLOT TO DEPLOY NOT FOUND (GetSlotWithHighestDeployf returns null)");
        }
    }

    public void AIPlayUnitOnSlot(CardSO cardSO, BattleSlot slotToPlay)
    {
        GameObject unit = Instantiate(GameManager.instance.unitPrefab, slotToPlay.unitSlot);
        unit.GetComponent<Unit>().CreateUnit(cardSO);
    }

  
    // AI PLAY ACTION
    
    public void AIPlayActionOnBestPosibleAIF(Card actionToPlay)
    {
        Debug.Log("PLAY ACTION: " + actionToPlay.cardSO.cardName);
        // 1. Do Animation for AI players (ZOOM CARD AND WAIT 2s aprox)



        // 2. THEN CONTINUE
        if (actionToPlay.actionDataAIs.playMethod == ActionPlayMethod.OnUnit)
        {
            Debug.Log("PLAY ACTION: " + actionToPlay.cardSO.cardName + " On Unit: "+ actionToPlay.actionDataAIs.bestActionTarget.cardSO.cardName);
            // PAY GOLD!
            GameManager.instance.actPlayer.playerActGold -= actionToPlay.cardSO.cardCost;
            // PLAY ACTION!
            actionToPlay.actionDataAIs.bestActionTarget.PlayAction(actionToPlay.cardSO);
            // SEND CARD TO GRAVEYARD
            actionToPlay.UnsubscribeEvents();
            actionToPlay.cardSO.GetOwner().SendCardToPlayedContainer(actionToPlay.gameObject);
            actionToPlay.gameObject.SetActive(false);
            // REFRESH VISUALS
            GameManager.instance.actPlayer.RefreshAttributeVisuals();
            Debug.Log("AIPlayACTION On Unit... COMPLETED");

        }
        else if (actionToPlay.actionDataAIs.playMethod == ActionPlayMethod.OnLine)
        {
            Debug.Log("PLAY ACTION: " + actionToPlay.cardSO.cardName + " On LINE");
            // PAY GOLD!
            GameManager.instance.actPlayer.playerActGold -= actionToPlay.cardSO.cardCost;
            if (actionToPlay.cardSO.GetActionTypeSO().enemyLineAction)
            {
                // PLAY ACTION!
                GameManager.instance.actPlayer.enemyDeployLine.GetComponent<BattleLine>().PlayLineAction(actionToPlay);
            }
            if (actionToPlay.cardSO.GetActionTypeSO().ownerLineAction)
            {
                // PLAY ACTION!
                GameManager.instance.actPlayer.deployLine.GetComponent<BattleLine>().PlayLineAction(actionToPlay);
            } 
            // SEND CARD TO GRAVEYARD
            actionToPlay.UnsubscribeEvents();
            actionToPlay.cardSO.GetOwner().SendCardToPlayedContainer(actionToPlay.gameObject);
            actionToPlay.gameObject.SetActive(false);
            // REFRESH VISUALS
            GameManager.instance.actPlayer.RefreshAttributeVisuals();
            Debug.Log("AIPlayACTION On Line... COMPLETED");
        }
        else if (actionToPlay.actionDataAIs.playMethod == ActionPlayMethod.OnPlayer)
        {

        }
        else if (actionToPlay.actionDataAIs.playMethod == ActionPlayMethod.OnSide)
        {

        }
        else if (actionToPlay.actionDataAIs.playMethod == ActionPlayMethod.OnSlot)
        {

        }
        else if (actionToPlay.actionDataAIs.playMethod == ActionPlayMethod.Global)
        {

        }
        else
        {
            Debug.LogWarning("BAD ACTIONDATAI.PLAYMETHOD - not coded? cleared? not passed?");
        }
    }
    
    
    // AI PLAY ITEM

    public void AIPlayItemOnUnitWithHighestItemf(Card itemToPlay)
    {
        // CARD IS ITEM
        Unit unitToPlaceItem = itemToPlay.bestItemTarget;
        if (unitToPlaceItem != null)
        {
            Debug.Log("PLACE ITEM: " + itemToPlay.cardSO.cardName + ", ON UNIT:" + unitToPlaceItem.cardSO.cardName);
            // PAY GOLD!
            GameManager.instance.actPlayer.playerActGold -= itemToPlay.cardSO.cardCost;
            // PLAY ITEM
            unitToPlaceItem.PutItem(itemToPlay.cardSO);
            // SEND CARD GO TI PLAYER CONT
            itemToPlay.UnsubscribeEvents();
            itemToPlay.cardSO.GetOwner().SendCardToPlayedContainer(itemToPlay.gameObject);
            itemToPlay.gameObject.SetActive(false);
            // REFRESH VISUALS
            GameManager.instance.actPlayer.RefreshAttributeVisuals();
            Debug.Log("AIPlayItemOnUnitWithHighestItemf... COMPLETED");
        }
        else
        {
            Debug.LogWarning("ITEM TO PLAY NOT FOUND (GetUnitWithHighestItemf returns null)");
        }
    }

    // AI ATTACK UNIT

    private void AIRealizeAttack(Unit unitToPlay)
    {
        Debug.Log("UNIT SELECTED TO attack: " + unitToPlay.cardSO.cardName + " ON TARGET: " + unitToPlay.target.cardSO.cardName);
        if (unitToPlay.cardSO.cardUpkeep > 0)
        {
            unitToPlay.PayUpkeep();
            unitToPlay.AttackUnit(unitToPlay.target);
        }
        else
        {
            unitToPlay.AttackUnit(unitToPlay.target);
        }
    }

    private void AIRealizeTentAttack(Unit unitToPlay)
    {
        Debug.Log("UNIT SELECTED TO attack: " + unitToPlay.cardSO.cardName + " ON TARGET ENEMY HEADQUARTERS TENT");
        if (unitToPlay.cardSO.cardUpkeep > 0)
        {
            unitToPlay.PayUpkeep();
            GameManager.instance.GetOtherPlayer(GetComponent<Player>()).tent.GetComponent<TentSlot>().TentAttack(unitToPlay);
        }
        else
        {
            GameManager.instance.GetOtherPlayer(GetComponent<Player>()).tent.GetComponent<TentSlot>().TentAttack(unitToPlay);
        }
    }

    // AI MOVE UNIT

    private void AIRealizeMove(Unit unitToPlay)
    {
        unitToPlay.MoveUnitTo(unitToPlay.besSlotToMove);
    }

    private void AISetUnitsFlankf()
    {
        // 1. RESET ALL VALUES
        foreach (Unit unitOnBattle in GetComponent<Player>().GetListOfPlayerUnits())
        {
            unitOnBattle.AImoveSlots.Clear();
            unitOnBattle.besSlotToMove = null;
            unitOnBattle.bestSlotToMoveAIF = -12;
        }
        Player enemy = GameManager.instance.GetOtherPlayer(GetComponent<Player>());
        // 2. PICK ONLY UNITS WITH FLANK ABILITY
        List<Unit> unitCanMove = new List<Unit>();
        foreach (Unit playerUnit in GetComponent<Player>().GetListOfPlayerUnits())
        {
            // Debug.Log("UNIT " + playerUnit.cardSO.cardName);
            if (playerUnit.CheckActive())
            {
                // SPEC AB FLANK
                if (playerUnit.cardSO.CheckIfHasSpecAb("Flanking"))
                {
                    unitCanMove.Add(playerUnit);
                }
            }
        }
        if (unitCanMove.Count > 0)
        {
            Debug.Log("---------->>>>>> START AISETUNITSAIMOVEF, AT LEAST ONE UNIT CAN MOVE <<<<<<-----------");
        }
        // 3. SETTING AIF OF EVERY SLOT FOR EACH UNIT
        foreach (Unit unitToMove in unitCanMove)
        {
            unitToMove.AIAddSlotsCanMove();
            if (unitToMove.AImoveSlots.Count > 0)
            {
                foreach (SlotAI slotAI in unitToMove.AImoveSlots)
                {
                    //Debug.Log("Unit " + unitToMove.cardSO.cardName + " can move to slot: " + slotAI.slot.x + "," + slotAI.slot.y);
                    List<Unit> unitsCanAttackOnSlot = slotAI.slot.GetListOfUnitsCanAttackThisSlot();
                    // 1. SET MOVE F IF UNIT CAN OR CANT KILL ME
                    foreach (Unit unitCanAttackOnSlot in unitsCanAttackOnSlot)
                    {
                        if (!unitCanAttackOnSlot.cardSO.CheckIfHasSpecAb("Maneuver"))
                        {
                            if (!unitCanAttackOnSlot.CheckSiege())
                            {
                                //1.1.1. CHECK IF UNIT CAN ATTACK ME ON SLOT CAN KILL ME
                                if (unitToMove.CheckIfAttackerCanKillMe(unitCanAttackOnSlot))
                                {
                                    // Debug.Log(unitCanAttackOnSlot.cardSO.cardName + "is non ARTILLERY, non MANEUVER UNIT can Attack and KILL " + unitToMove.cardSO.cardName + " if I WILL MOVE TO SLOT: " + slotAI.slot.x);
                                    slotAI.ChangeSlotMovef(-unitToMove.cardSO.cardCost);
                                }
                                // 1.1.2. CHECK IF UNIT CAN ATTACK ME ON THIS SIDE WILL DIE
                                if (unitToMove.CheckIfAttackerWillBeKilled(unitCanAttackOnSlot))
                                {
                                    // Debug.Log(unitCanAttackOnSlot.cardSO.cardName + " is non ARTILLERY, non MANEUVER UNIT can Attack and will be killed by " + unitToMove.cardSO.cardName + " if I WILL MOVE TO SLOT: " + slotAI.slot.x);
                                    slotAI.ChangeSlotMovef(unitCanAttackOnSlot.cardSO.cardCost);
                                }
                            }
                        }
                    }
                    // 2. TO DO IF UNIT WITH PROTECTION SPEC AB AND CAN STAND BY ARTILLERY UNIT:
                    if (unitToMove.cardSO.CheckIfHasSpecAb("Protection"))
                    {
                        // TO DO SIMPLE FUNCTION SEARCH FOR SLOTS WITH ARTILLERY CLOSE TO 
                        List<Unit> friendlyNeigbours = slotAI.slot.GetNeighbourUnits();
                        foreach (Unit friendlyNbr in friendlyNeigbours)
                        {
                            if (friendlyNbr.CheckSiege())
                            {
                                slotAI.ChangeSlotMovef(3);
                            }
                        }
                    }
                    // 3. MOVE IF LEFT OR RIGHT LINE NOT PROTECTED AND TENT CAN BE ATTACKED
                    if (slotAI.slot.x < 4)
                    {
                        if (GetComponent<Player>().GetListOfMyUnitsInRightSide().Count > 1)
                        {
                            if (GetComponent<Player>().GetListOfMyUnitsInLeftSide().Count == 0)
                            {
                                slotAI.ChangeSlotMovef(2);
                                if (CheckIfMyTentCanBeAttacked())
                                {
                                    slotAI.ChangeSlotMovef(2);
                                    Debug.Log("MOVE BONUS - +4, TO PROTECT LEFT SIDE");
                                }
                            }
                        }
                    }
                    else if (slotAI.slot.x >= 4)
                    {
                        if (GetComponent<Player>().GetListOfMyUnitsInLeftSide().Count > 1)
                        {
                            if (GetComponent<Player>().GetListOfMyUnitsInRightSide().Count == 0)
                            {
                                slotAI.ChangeSlotMovef(2);
                                if (CheckIfMyTentCanBeAttacked())
                                {
                                    slotAI.ChangeSlotMovef(2);
                                    Debug.Log("MOVE BONUS - +4, TO PROTECT RIGHT SIDE");
                                }
                            }
                        }
                    }
                } 
            }
        }
        // Debug.Log(" -------->>> UNIT MOVE ------>>>> -- DEBUG LIST: List OF AISLOT MOVE FACTOR: ->>>- ");
        foreach (Unit debugUnitToMove in unitCanMove)
        {
            if (debugUnitToMove.AImoveSlots.Count > 0)
            {
                foreach (SlotAI debugSlotAI in debugUnitToMove.AImoveSlots)
                {
                    // Debug.Log("UNIT: " + debugUnitToMove.cardSO.cardName + " CAN MOVE ON SLOT " + debugSlotAI.slot.x + "," + debugSlotAI.slot.y + " with Movef: " + debugSlotAI.slotMovef);
                }
            }
        }
        //Debug.Log("----------> END AISETUNITSAIMOVEF <-----------");
    }

    // AI ATTACK UNIT

    private void AISetUnitsAIAttackf()
    {
        Debug.Log("xxxxATTACKxxxxxxxxx START SET UNITSAIATTACKF xxxxxxxxxATTACKxxxx");
        // 1. RESETA ATTACKF VALUES
        foreach (Unit unitOnBattle in GetComponent<Player>().GetListOfPlayerUnits())
        {
            unitOnBattle.AIattackUnits.Clear();
            unitOnBattle.target = null;
            unitOnBattle.targetAIF = -11;
        }
        // 2. GET LIST OF UNITS CAN ATTACK
        List<Unit> unitsCanAttack = GetListOfUnitsCanAttack();
        // 3. SETTING AIATTACKF OF EVERY UNIT CAN ATTACK
        foreach (Unit myUnitCanAttack in unitsCanAttack)
        {
            myUnitCanAttack.AIAddUnitsCanAttack();
            foreach (UnitAI unitToAttack in myUnitCanAttack.AIattackUnits)
            {
                Debug.Log("1. ANALYSE ATTACK UNIT: " + myUnitCanAttack.cardSO.cardName + " on ENEMY: " + unitToAttack.unit.cardSO.cardName);
                // 1. GET DAMAGE OF ATTACKED UNIT
                int defenderDmg = myUnitCanAttack.GetInflictedDamageToDefender(unitToAttack.unit);
                int defenderDef = unitToAttack.unit.cardSO.GetDef();
                Debug.Log(myUnitCanAttack.cardSO.cardName + " INFLICTS " + defenderDmg + " dmg to enemy " + unitToAttack.unit.cardSO.cardName);
                if (defenderDmg < defenderDef)
                {
                    unitToAttack.ChangeUnitf(defenderDmg + GetComponent<Player>().playerSO.playerAIAgrresivness);
                }
                else
                {
                    unitToAttack.ChangeUnitf(defenderDef + GetComponent<Player>().playerSO.playerAIAgrresivness);
                }
                // 2. CHECK IF I CAN KILL ATTACKED UNIT
                if (myUnitCanAttack.CheckIfDefenderWillBeKilled(unitToAttack.unit))
                {
                    Debug.Log("MY UNIT CAN KILL: " + unitToAttack.unit.cardSO.cardName);
                    unitToAttack.ChangeUnitf(unitToAttack.unit.cardSO.GetRealCardCost());
                }
                // 3. GET MY DAMAGE WHEN ATTACKING
                int attackerDmg = myUnitCanAttack.GetMyDamageWhenAttacking(unitToAttack.unit);
                Debug.Log(myUnitCanAttack.cardSO.cardName + " GETS "+ attackerDmg + " dmg inflicted by enemy "+ unitToAttack.unit.cardSO.cardName);
                unitToAttack.ChangeUnitf(-attackerDmg);
                // 4. CHECK IF UNIT ATTACKED (DEFENDER) WILL KILL ME
                if (myUnitCanAttack.CheckIfDefenderCanKillMe(unitToAttack.unit))
                {
                    Debug.Log("MY UNIT WILL BE KILLED IF I ATTACK: " + unitToAttack.unit.cardSO.cardName);
                    unitToAttack.ChangeUnitf(-myUnitCanAttack.cardSO.GetRealCardCost());
                }
                // 5. IF UNIT TO ATTACK IS ARTILLERY - MORE LIKELY TO ATTACK
                if (unitToAttack.unit.cardSO.GetUnitType() == UnitType.Artillery)
                {
                    unitToAttack.ChangeUnitf(2);
                }
                // 6. IF UNIT TO ATTACK HAS FIRST STRIKE - MORE LIKELY TO ATTACK
                if (unitToAttack.unit.cardSO.CheckIfHasSpecAb("First Strike"))
                {
                    Debug.Log("MY UNIT CAN ATTAK UNIT WITH FIRST STRIKE");
                    unitToAttack.ChangeUnitf(3);
                    if (unitToAttack.unit.cardSO.GetRealCardCost() >= myUnitCanAttack.cardSO.GetRealCardCost()) 
                    {
                        unitToAttack.ChangeUnitf(unitToAttack.unit.cardSO.GetRealCardCost());
                    }
                }
                // 7. IF UNIT TO ATTACK HAS PIKE, AND CAN BE ATTACKED BY MY CAVALRY = substract 5 if my unit dies, 2 if survives
                if (unitToAttack.unit.cardSO.CheckIfHasActiveSpecAb("Pike"))
                {
                    if (myUnitCanAttack.cardSO.GetUnitType() == UnitType.Cavalry && (unitToAttack.unit.CheckIfAttackerWillBeKilled(myUnitCanAttack)))
                    {
                        unitToAttack.ChangeUnitf(-3);
                        Debug.Log("MY UNIT IS CAVALRY AND DEFENDER HAS PIKE AND WILL KILL ME!");
                    }
                    else if (unitToAttack.unit.cardSO.GetUnitType() == UnitType.Cavalry)
                    {
                        unitToAttack.ChangeUnitf(-2);
                        Debug.Log("MY UNIT IS CAVALRY AND DEFENDER HAS PIKE!");
                    }
                }
                // 8. IF UNIT TO ATTACK HAS DEFENESIVE ABILITY, AND WONT KILL ME ATTACKING = substract 4
                if (unitToAttack.unit.cardSO.CheckIfHasActiveSpecAb("Fire Defense"))
                {
                    if (unitToAttack.unit.CheckIfAttackerWillBeKilled(myUnitCanAttack))
                    {
                        Debug.Log("DEFENDER HAS FIRE DEFENCE! AND WILL KILL ME!");
                        unitToAttack.ChangeUnitf(-2);
                    }
                    Debug.Log("DEFENDER HAS FIRE DEFENCE!");
                    unitToAttack.ChangeUnitf(-2);
                }
            }
        }
        Debug.Log("xxxxxxxxxxxxx UNIT ATTACK xxxx xx DEBUG LIST: List OF AIUNITS ATTACK FACTOR");
        foreach (Unit debugUnitToAttack in unitsCanAttack)
        {
            if (debugUnitToAttack.AIattackUnits.Count > 0)
            {
                foreach (UnitAI debugUnitAI in debugUnitToAttack.AIattackUnits)
                {
                    Debug.Log("UNIT: " + debugUnitToAttack.cardSO.cardName + " CAN ATTACK: " + debugUnitAI.unit.cardSO.cardName + " with Attackf: " + debugUnitAI.unitAttackf);
                }
            }
        }
    }

    // AI ATTACK HQ TENT

    private void AISetUnitsAIAttackTentF()
    {
        Debug.Log("QQQQQQQQQQQQQQQQQQQQQQ START SET UNITSAITENTATTACKF QQQQQQQQQQQQQQQQQQQQQQQQQQQQ");
        // 1. RESETA ATTACKF VALUES
        foreach (Unit unitOnBattle in GetComponent<Player>().GetListOfPlayerUnits())
        {
            unitOnBattle.hqTentAIF = -10;
            unitOnBattle.targetHqTent = false;
        }
        // 2. GET LIST OF UNITS CAN ATTACK
        List<Unit> unitsCanAttack = GetListOfUnitsCanAttack();
        // 3. CHECK IF CAN ATTACK HQ
        List<Unit> unitsCanAttackTent = new List<Unit>();
        foreach (Unit unitCanAttack in unitsCanAttack)
        {
            if (unitCanAttack.AICheckIfCanAttackEnemyTent())
            {
                unitsCanAttackTent.Add(unitCanAttack);
                unitCanAttack.targetHqTent = true;
                unitCanAttack.hqTentAIF = 0;
            }
        }
        // 3. SETTING HQTENT AI FACTOR FOR EVERY UNIT
        foreach (Unit unitCanAttackTent in unitsCanAttackTent)
        {
            // 1. ADD ATTACK
            unitCanAttackTent.hqTentAIF += unitCanAttackTent.GetTentDamage();
            // 2. IF MORALE IS LEFT ONLY AS DAMAGE (IF WINS) ADD 100!
            if (unitCanAttackTent.GetTentDamage() >= GameManager.instance.GetOtherPlayer(GetComponent<Player>()).playerActMorale)
            {
                unitCanAttackTent.hqTentAIF += 100;
                //Debug.Log("DECISIVE ATTACK!");
            }
            // 3. CHECK IF ALL MY UNITS CAN ATTACK AND KILL ME THIS TURN (Check overallTentAttack)
            if (GetOverallEnemyTentDamage() >= GameManager.instance.GetOtherPlayer(GetComponent<Player>()).playerActMorale)
            {
                unitCanAttackTent.hqTentAIF += 100;
                //Debug.Log("DECISIVE ROUND!");
            }
            // 3. IF CAN KILL UNIT WITH MORE COST - THEN REMOVE DAMAGE POINTS (more likely to attack unit than tent)
            List<Unit> enemiesInRangeOfUnit = unitCanAttackTent.GetListOfMyPosibleTargets();
            foreach(Unit enemyInRange in enemiesInRangeOfUnit)
            {
                if (unitCanAttackTent.CheckIfDefenderWillBeKilled(enemyInRange))
                {
                    unitCanAttackTent.hqTentAIF -= enemyInRange.cardSO.GetRealCardCost();
                    Debug.Log("CAN ATTACK OTHER UNIT, REMOVING POINTS FROM HQTENT AIF");
                }
            }  
        }
        // 4. RESULT (DEBUG LIST)
        foreach (Unit unitCanAttackTent in unitsCanAttackTent)
        {
            Debug.Log("UNIT: " + unitCanAttackTent.cardSO.cardName + " CAN ATTACK ENEMY HEADQUARTERS TENT WITH FACTOR: " + unitCanAttackTent.hqTentAIF);
        }
    }

    // AI MAIN LOOP (for now has name DEPLOY PHASE, but have to be sincronic with MOVE and ATTACK and ACTIONS)!

    public IEnumerator AILoop()
    {
        // LOOP - Looping trough AISLOTS 
        Debug.Log("START COROUTINE DEPLOY PHASE (NEW LOOP)");
        // USTAW FACTOR DEPLOY (Deployf) kazdej karty UNIT
        AISetCardsAIDeployf();
        // USTAW FACTOR PLACE ITEM kazdej karty ITEM
        AISetCardsAIItemPos();
        // USTAW FACTOR PLAY ACTION dla kazdej karty, linii, gracza, slotu lub globalnie
        AISetCardAIAction();
        // USTAW FACTOR MOVE UNIT kazdej jednostki
        AISetUnitsFlankf();
        // USTAW FACTOE ATTACK UNIT KAZDEJ JEDNOSTKI
        AISetUnitsAIAttackf();
        // USTAW FACTOR ATTACK ON HEADCUARTERS TENT
        AISetUnitsAIAttackTentF();
        // WYBIERZ KARTE O NAJWYZSZYM DEPLOYF
        Card cardToPlay = GetCardWithHighestAIFactor();
        Unit unitToPlay = GetUnitWithHighestAIFactor();
        // DECISION TO PLAY CARD OR MOVE/ATTACK UNIT
        string decision;
        if (cardToPlay != null && unitToPlay != null)
        {
            // DECISION BETWEEN PLAY CARD AND UNIT
            if (cardToPlay.bestTargetSlotAIF > unitToPlay.targetAIF && cardToPlay.bestTargetSlotAIF > unitToPlay.hqTentAIF)
            {
                decision = "card";
            }
            else if (cardToPlay.bestItemTargetAIF > unitToPlay.targetAIF && cardToPlay.bestItemTargetAIF > unitToPlay.hqTentAIF) 
            {
                decision = "card";
            }
            else
            {
                decision = "unit";
            }
        }
        else if (cardToPlay == null && unitToPlay != null)
        {
            decision = "unit";
        }
        else if (cardToPlay != null && unitToPlay == null)
        {
            decision = "card";
        }
        else
        {
            decision = "skip";
        }
        // DECISION MADE.
        if (decision == "card")
        {
            Debug.Log("AILoop() Coroutine execution. Decision MADE TO PLAY CARD: " + cardToPlay.cardSO.cardName);
            // IF UNIT
            if (cardToPlay.cardSO.cardTypeSO.cardType == CardType.Unit)
            {
                cardToPlay.GetComponent<Animator>().SetTrigger("FlipToDraw");
                yield return new WaitForSeconds(0.8f);
                Debug.Log("CARD: " + cardToPlay.cardSO.cardName + " TO PLAY");
                AIPlaceUnitOnSlotWithHighestDeployf(cardToPlay);
            }
            // IF ITEM
            if (cardToPlay.cardSO.cardTypeSO.cardType == CardType.Item)
            {
                cardToPlay.GetComponent<Animator>().SetTrigger("FlipToDraw");
                yield return new WaitForSeconds(0.8f);
                Debug.Log("CARD: " + cardToPlay.cardSO.cardName + " (item) TO PLAY");
                AIPlayItemOnUnitWithHighestItemf(cardToPlay);
            }
            // IF ACTION
            if (cardToPlay.cardSO.cardTypeSO.cardType == CardType.Action)
            {
                cardToPlay.GetComponent<Animator>().SetTrigger("FlipToPlayAction");
                LeanTween.move(cardToPlay.gameObject, new Vector3(984, 538), 0.4f);
                yield return new WaitForSeconds(2.2f);
                Debug.Log("CARD: " + cardToPlay.cardSO.cardName + " (action) TO PLAY");
                AIPlayActionOnBestPosibleAIF(cardToPlay);
            }
        }
        else if (decision == "unit")
        {
            Debug.Log("AILoop() Coroutine execution. Decision MADE TO PLAY UNIT: " + unitToPlay.cardSO.cardName);
            // IF TO ATTACK OTHER UNIT:
            if (unitToPlay.aiDecision == UnitDecsion.AttackTent)
            {
                yield return new WaitForSeconds(0.8f);
                Debug.Log("UNIT: " + unitToPlay.cardSO.cardName + " TO ATTACK ENEMY`s HEADQUARTERS TENT");
                AIRealizeTentAttack(unitToPlay);
            }
            else if (unitToPlay.aiDecision == UnitDecsion.AttackUnit) 
            {
                yield return new WaitForSeconds(0.8f);
                Debug.Log("UNIT: " + unitToPlay.cardSO.cardName+ " TO ATTACK ENEMY UNIT");
                AIRealizeAttack(unitToPlay);
            }
            else if (unitToPlay.aiDecision == UnitDecsion.Move)
            {
                yield return new WaitForSeconds(0.8f);
                Debug.Log("UNIT: " + unitToPlay.cardSO.cardName + " TO MOVE TO OTHER SIDE");
                AIRealizeMove(unitToPlay);
                
            }
            else
            {
                Debug.LogWarning("ATTACK DECISION ERROR");
            }
        }
        // JEZELI NIE MA CzYNNOSCI DO WYKONANIA - ZAKONCZ AI LOOP
        else 
        {
            yield return new WaitForSeconds(1f);
            Debug.Log("END OF AI LOGIC, MOVEMENT AND ATTACK - SWITCH TURN");
            Debug.Log("!!!!!!! TEST VERSION --- DEBUG END BUTTON TO END AI TURN");
            if (GameManager.instance.AIdebugMode)
            {
                GameManager.instance.debugAIEndTurnButton.interactable = true;
            }
            else
            {
                GameManager.instance.onClickEndTurn();
            }
            yield break;
        }
        // TO TRZEBA BEDZIE ZMIENIC, = AI kliknie guzik, jak skonczy grac karty i ruszac jednostki
        yield return new WaitUntil(() => !GameManager.instance.IsAnimationPlayig());
        yield return new WaitForSeconds(0.5f);
        Debug.Log(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
        Debug.Log(">> LOOP COMPLETED, GO ON OR ACTIVATE DEBUG AI GO BUTTON >>>");
        Debug.Log(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
        GameManager.instance.ActivateDebugAIGoButton();
        // StartCoroutine(AILoop());
    }

    public void NextAILoop()
    {
        StartCoroutine(AILoop());
    }

}
