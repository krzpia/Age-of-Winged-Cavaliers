using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    [Header ("LOGIC")]
    public bool isHuman;
    public bool firstToPlay;

    [Header("PLAYER Scriptable Object")]
    public PlayerSO playerSO;

    [Header("Attributes")]
    public int playerActGold;
    public int playerIncome;
    public int playerActMorale;
    public int playerMaxGold;

    [Header("Visuals")]
    public TMP_Text playerNameText;
    public TMP_Text playerGoldText;
    public TMP_Text playerNextTurnIncomeText;
    //public TMP_Text playerUpkeepText;
    public TMP_Text playerMoraleText;
    //public TMP_Text playerMoralePenaltyText;
    public TMP_Text playerMaxGoldText;
    public Image playerImage;
    public Image tentImage;
    public Image coatImage;

    [Header("Containers")]
    public Transform hand;
    public Transform deployLine;
    // public Transform battleLine;
    public Transform enemyDeployLine;
    public Transform deckTransform;
    public Transform graveyard;
    public Transform tent;
    public Transform cardPlayedContainer;

    [Header("Decks")]
    // LIST OF CARDSO TO COPY
    public List<CardSO> startDeck;
    // public CardSO startCity;

    [Header("In Game Panels")]
    public StartPanel startPanel;

    // GET LISTS (TO DO NEW LOGIC!)

    public List<Card> GetListOfCardsInHand()
    {
        List<Card> cards = new List<Card>();
        if (hand.childCount == 0)
        {
            Debug.LogWarning("f GetListOfCardsInHand: No CARDS");
        }
        foreach (Transform cardT in hand)
        {
            cards.Add(cardT.GetComponent<Card>());
        }
        return cards;
    }

    public List<Unit> GetListOfUnitInLine(Transform battleLine)
    {
        List<Unit> listOfUnits = new List<Unit>();
        foreach (Transform slotTransform in battleLine)
        {
            Transform unitSlot = slotTransform.GetComponent<BattleSlot>().unitSlot;
            if (unitSlot.childCount > 0)
            {
                if (unitSlot.GetChild(0).GetComponent<Unit>() != null)
                {
                    listOfUnits.Add(unitSlot.GetChild(0).GetComponent<Unit>());
                }
            }
        }
        return listOfUnits;
    }

    public List<Unit> GetListOfAllUnits()
    {
        List<Unit> listOfUnits = new List<Unit>();
        listOfUnits.AddRange(GetListOfUnitInLine(deployLine));
        // listOfUnits.AddRange(GetListOfUnitInLine(battleLine));
        listOfUnits.AddRange(GetListOfUnitInLine(enemyDeployLine));
        return listOfUnits;
    }

    public List<Unit> GetListOfPlayerUnits()
    {

        List<Unit> playerUnits = GetListOfUnitInLine(deployLine);
        return playerUnits;
    }

    public List<Unit> GetListOfEnemyUnits()
    {
        List<Unit> enemyUnits = GetListOfUnitInLine(enemyDeployLine);
        return enemyUnits;
    }

    public List<Unit> GetListOfMyUnitsInLeftSide()
    {
        List<Unit> myUnits = GetListOfPlayerUnits();
        List<Unit> leftUnits = new List<Unit>();
        foreach (Unit unit in myUnits)
        {
            if (unit.GetSlot().x < 4)
            {
                leftUnits.Add(unit);
            }
        }
        return leftUnits;
    }

    public List<Unit> GetListOfMyUnitsInRightSide()
    {
        List<Unit> myUnits = GetListOfPlayerUnits();
        List<Unit> rightUnits = new List<Unit>();
        foreach (Unit unit in myUnits)
        {
            if (unit.GetSlot().x >= 4)
            {
                rightUnits.Add(unit);
            }
        }
        return rightUnits;
    }

    public List<Unit> GetListOfEnemyUnitsInLeftSide()
    {
        List<Unit> enemyUnits = GetListOfEnemyUnits();
        List<Unit> leftUnits = new List<Unit>();
        foreach (Unit unit in enemyUnits)
        {
            if (unit.GetSlot().x < 4)
            {
                leftUnits.Add(unit);
            }
        }
        return leftUnits;
    }

    public List<Unit> GetListOfEnemyUnitsInRightSide()
    {
        List<Unit> enemyUnits = GetListOfEnemyUnits();
        List<Unit> rightUnits = new List<Unit>();
        foreach (Unit unit in enemyUnits)
        {
            if (unit.GetSlot().x >= 4)
            {
                rightUnits.Add(unit);
            }
        }
        return rightUnits;
    }

    public List<BattleSlot> GetListOfDeploySlots()
    {
        List<BattleSlot> battleSlots = new List<BattleSlot>();
        foreach (Transform slotT in deployLine)
        {
            battleSlots.Add(slotT.GetComponent<BattleSlot>());
        }
        return battleSlots;
    }

    public List<BattleSlot> GetListOfEnemySlots()
    {
        List<BattleSlot> battleSlots = new List<BattleSlot>();
        foreach (Transform slotT in enemyDeployLine)
        {
            battleSlots.Add(slotT.GetComponent<BattleSlot>());
        }
        return battleSlots;
    }

    public List<BattleSlot> GetListOfAllSlots()
    {
        List<BattleSlot> battleSlots = new List<BattleSlot>();
        battleSlots.AddRange(GetListOfDeploySlots());
        // battleSlots.AddRange(GetListOfFrontLineSlots());
        battleSlots.AddRange(GetListOfEnemySlots());
        return battleSlots;
    }


    // DECK AND HAND LOGIC

    public List<CardSO> GetDeck()
    {
        List<CardSO> deck = new List<CardSO>();
        foreach (CardSO card in playerSO.startDeck)
        {
            //// COPYING SCRIPTABLE OBJECTS!
            // SPEC ABS
            List<SpecialAbilitySO> specAbsToAdd = new List<SpecialAbilitySO>();
            foreach (SpecialAbilitySO specAb in card.specialAbilityList)
            {
                specAbsToAdd.Add(Instantiate(specAb));
            }
            // CARD INSTANTIATE
            CardSO cardToAdd = Instantiate(card);
            CardTypeSO cardTypeSOToAdd = Instantiate(card.cardTypeSO);
            cardToAdd.cardTypeSO = cardTypeSOToAdd;
            // SPEC ABS ADDING
            cardToAdd.specialAbilityList.Clear();
            foreach(SpecialAbilitySO specAb in specAbsToAdd)
            {
                cardToAdd.specialAbilityList.Add(specAb);
            }
            // ADDING COPIED CARD
            cardToAdd.SetOwner(this);
            deck.Add(cardToAdd);
        }
        return deck;
    }

    public List<CardSO> GetListCardSOInDeck()
    {
        List<CardSO> cardSOs= new List<CardSO>();
        foreach (Transform cardT in deckTransform)
        {
            cardSOs.Add(cardT.GetComponent<Card>().cardSO);
        }
        return cardSOs;
    }

    public List<Card> GetListCardInDeck()
    {
        List<Card> cards = new List<Card>();
        foreach (Transform cardT in deckTransform)
        {
            cards.Add(cardT.GetComponent<Card>());
        }
        return cards;
    }

    public List<CardSO> GetListCardSOInHand()
    {
        List<CardSO> cardSOs = new List<CardSO>();
        foreach (Transform cardT in hand)
        {
            cardSOs.Add(cardT.GetComponent<Card>().cardSO);
        }
        return cardSOs;
    }

    public List<Card> GetListCardInHand()
    {
        List<Card> cards = new List<Card>();
        foreach (Transform cardT in hand)
        {
            cards.Add(cardT.GetComponent<Card>());
        }
        return cards;
    }

    public void CreateDeck()
    {
        List<CardSO> deckCardSO = GetDeck();
        foreach (CardSO card in deckCardSO)
        {
            GameObject cardinDeck = Instantiate(GameManager.instance.cardPrefab, deckTransform);
            cardinDeck.GetComponent<Card>().SetCard(card);
            cardinDeck.GetComponent<Card>().SetRewers();
            cardinDeck.GetComponent<CardBehaviour>().SetToDeck();
        }
        // CREATE CITY
        // startCity = Instantiate(playerSO.startCity);
        // startCity.SetOwner(this);
        // IF AI
        if (!isHuman)
        {
            GetComponent<AI>().AISetDeck();
        }
    }

    public GameObject GetFirstCardGameObjectFromDeck()
    {
        if (deckTransform.childCount == 0)
        {
            Debug.LogWarning("Deck Empty and trying to draw a card!");
            return null;
        }
        return deckTransform.GetChild(0).gameObject;
    }

    public GameObject GetRandomCardFromDeck()
    {
        if (deckTransform.childCount == 0)
        {
            Debug.LogWarning("Deck Empty and trying to draw a card!");
            return null;
        }
        int randomPick = UnityEngine.Random.Range(0, deckTransform.childCount - 1);
        return deckTransform.GetChild(randomPick).gameObject;
    }

    // ATTRIBUTES AND TURN LOGIC

    public void SetStartAttributes()
    {
        // ATTS
        playerActGold = playerSO.startGold;
        playerMaxGold = playerSO.startMaxGold;
        playerActMorale = playerSO.startMorale;
        playerNameText.text = playerSO.playerName;
        // GRAPHICS
        tentImage.color = playerSO.playerColor;
        coatImage.sprite = playerSO.playerCoatImage;
        playerImage.sprite = playerSO.playerSprite;
        // GRAVEYARD
        graveyard.GetComponent<Image>().sprite = playerSO.playerDeckImage;
        // TENT
        tent.GetComponent<TentSlot>().owner = this;
        // TO DO!
    }

    public void SetNewTurn()
    {
        // INCOME
        ActualizeIncome();
        playerActGold += playerIncome;
        if(playerActGold > playerMaxGold)
        {
            playerActGold = playerMaxGold;
        }
        // SLOTS
        foreach (BattleSlot slot in GetListOfAllSlots())
        {
            if (slot.owner == this)
            {
                slot.ActivateCardDrop();
            }
        }
        // CARDS
        foreach (Card card in GetListOfCardsInHand())
        {
            card.SetAwersForTurnStart();
            card.GetComponent<CardBehaviour>().Activate();
        }
        // UNITS
        foreach (Unit unit in GetListOfPlayerUnits())
        {
            unit.NewTurn();
        }
        // VISUALS
        GameEvents.current.RefreshUnitVisuals();
        RefreshAttributeVisuals();
        playerImage.transform.parent.GetComponent<Animator>().SetBool("PlayerImagePulse", true);
        // SETING END TURN BUTTON TO ACTIVE IF HUMAN
        if (isHuman)
        {
            GameManager.instance.ActivateEndTurnButton();
        }
        else
        {
            GetComponent<AI>().StartAITurn();
        }
    }

    public void SetEndTurn()
    {
        // SLOTS
        foreach (BattleSlot slot in GetListOfAllSlots())
        {
            if (slot.owner == this)
            {
                slot.DeactivateCardDrop();
            }
        }
        // CARDS
        foreach (Card card in GetListOfCardsInHand())
        {
            card.SetRewersForTurnEnd();
            card.GetComponent<CardBehaviour>().Deactivate();
        }
        // UNITS
        foreach (Unit unit in GetListOfPlayerUnits())
        {
            unit.EndTurn();
        }
        // MORALE PENALTY
        playerActMorale -= GetMoralePenalty();
        // CHECK END GAME
        CheckEndGame();
        // VISUALS
        playerImage.transform.parent.GetComponent<Animator>().SetBool("PlayerImagePulse", false);
        RefreshAttributeVisuals();
    }

    private void ActualizeIncome()
    {
        int playerActIncome = 0;
        int playerNextTurnIncome = 0;
        //1. BASE INCOME
        if (GameManager.instance.turnNo == 0)
        {
            playerActIncome = 0;
            playerNextTurnIncome = 0;
        }
        else if (GameManager.instance.turnNo == 1)
        {
            if (firstToPlay)
            {
                //Debug.Log("FIRST TO PLAY RECIEVES 1 GOLD");
                playerActIncome = playerSO.incomeList[0] - 1;
            }
            else
            {
                playerActIncome = playerSO.incomeList[0];
            }
        }
        else
        {
            playerActIncome = playerSO.incomeList[GameManager.instance.turnNo - 1];
        }
        //Debug.Log ("BASE INCOME IN "+ GameManager.instance.turnNo + " turn: "+ playerIncome);
        // 1.1 NEXT TURN BASE INCOME
        // TO DO INCOME *how many turns...?
        playerNextTurnIncome = playerSO.incomeList[GameManager.instance.turnNo];
        //2. UNITS
        foreach (Unit unit in GetListOfAllUnits())
        {
            if (unit.cardSO.CheckIfHasSpecAb("Income"))
            {
                playerActIncome += unit.cardSO.GetSpecAbByName("Income").income;
                playerNextTurnIncome += unit.cardSO.GetSpecAbByName("Income").income;
            }
        }
        // 3. 10 OR LESS MORALE ADD 1 gold
        if (playerActMorale < 10)
        {
            playerActIncome++;
        }
        playerIncome = playerActIncome;
        playerNextTurnIncomeText.text = ("+ " + playerNextTurnIncome.ToString());
    }

    public void RefreshAttributeVisuals()
    {
        playerGoldText.text = playerActGold.ToString();
        ActualizeIncome();  
        playerMoraleText.text = playerActMorale.ToString();
        // playerMoralePenaltyText.text = GetMoralePenalty().ToString();
        playerMaxGoldText.text = "Max: " + playerMaxGold.ToString();
        tent.GetComponent<TentSlot>().RefreshShieldIcon();
        
    }

    public void NewTurnDrawPhase()
    {
        if (isHuman)
        {
            StartCoroutine(DrawCard());
            SetAllCardsInHandVisible();
        }
        else
        {
            StartCoroutine(AIDrawCard());
        }
        
    }

    public IEnumerator AIDrawCard()
    {
        AIDrawOneCardStartAnimation();
        yield return new WaitForSeconds(1.2f);
        Debug.Log("END ANIMATION, SET NEW TURN");
        SetNewTurn();
    }

    public void AIDrawOneCardStartAnimation()
    {
        GameEvents.current.BlockInteraction();
        if (hand.childCount >= 8)
        {
            Debug.Log("AI HAND FULL!");
            return;
        }
        if (deckTransform.childCount == 0)
        {
            Debug.Log("NO CARDS IN AI DECK");
            return;
        }
        GameObject cardToDraw = GetFirstCardGameObjectFromDeck();
        cardToDraw.GetComponent<Canvas>().sortingOrder = 6;
        // START ANIMATION
        cardToDraw.GetComponent<CardBehaviour>().AIDrawCardAnimation();
    }


    public IEnumerator DrawCard()
    {
        DrawOneCardStartAnimation();
        yield return new WaitForSeconds(1.2f);
        // 

        // DRAW CARD AND SET UNITS ANIMATIONS END. SET NEW TURN
        SetNewTurn();
        // ACTIVATE BUTTON AFTER DRAW ANIMATION (1.2s)
        GameManager.instance.ActivateEndTurnButton();
    }

    public void DrawOneCardStartAnimation()
    {
        GameEvents.current.BlockInteraction();
        // SKIP DRAW IF HAND FULL
        if (hand.childCount >= 8)
        {
            Debug.Log("HAND FULL!");
            // TO DO ANIMATION OF HAD FULL! ERROR najprawdopodobniej gdyz animacja z kartmi blokuje aktywnosc, 
            // gdy nie ma animacji jest blad z karta!
            GameEvents.current.StartInteraction();
            return;
        }
        // SKIP DRAW IF NO MORE CARDS IN DECK
        if (deckTransform.childCount == 0)
        {
            Debug.Log("NO MORE CARDS");
            GameEvents.current.StartInteraction();
            return;
        }
        //1. START ANIMATION (ANIMATION STARTS FURTHER FUNCTIONS
        GameObject cardToDraw = GetFirstCardGameObjectFromDeck();
        cardToDraw.GetComponent<Canvas>().sortingOrder = 6;
        cardToDraw.GetComponent<Animator>().SetTrigger("DrawCard");
    }

    private void SetAllCardsInHandVisible()
    {
        foreach (Transform cardT in hand)
        {
            cardT.GetComponent<Card>().SetAwersButNotActivate();
        }
    }

    // NOT USED
    public IEnumerator DrawCards(int number)
    {
        for (int i=0; i < number; i++)
        {
            if (hand.childCount < 8)
            {
                //1. START ANIMATION (ANIMATION STARTS FURTHER FUNCTIONS
                GameObject cardToDraw = GetFirstCardGameObjectFromDeck();
                cardToDraw.GetComponent<Canvas>().sortingOrder = 6;
                cardToDraw.GetComponent<Animator>().SetTrigger("DrawCard");
                yield return new WaitForSeconds(1.2f);
            }
            else
            {
                Debug.LogWarning("HAND FULL!");
                yield break;
            }
        }

    }

    public void SendCardToPlayedContainer(GameObject card)
    {
        card.transform.SetParent(cardPlayedContainer,false);
        card.GetComponent<CardBehaviour>().SetToUsedPile();
    }

    public int GetMoralePenalty()
    {
        int penalty = 0;
        // TO DO IF SPEC AB?
        return penalty;
    }

    public int GetAllUpkeep()
    {
        int upkeep = 0;
        List<Unit> myUnits = GetListOfPlayerUnits();
        foreach (Unit unit in myUnits)
        {
            upkeep += unit.cardSO.cardUpkeep; 
        }
        return upkeep;
    }

    public void CheckEndGame()
    {
        if (playerActMorale <= 0)
        {
            GameManager.instance.boolEndGame = true;
            Debug.Log(playerSO.playerName + "LOSES!");
            GameManager.instance.GenerateEndGameWindow(GameManager.instance.GetOtherPlayer(this));
        }
    }

}
