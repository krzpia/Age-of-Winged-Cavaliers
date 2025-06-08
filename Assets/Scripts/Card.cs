using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class Card : MonoBehaviour
{
    // = CARD SCRIPTABLE OBJECT = //
    [Header("Scriptable object")]
    public CardSO cardSO;
    // = VISUALS =//
    [Header("VISUALS")]
    public TMP_Text cardBNameText;
    public TMP_Text cardBDescriptionText;
    public TMP_Text cardBTypeText;
    public TMP_Text cardBCostText;
    public TMP_Text cardBUpkeepText;
    public TMP_Text cardBAttText;
    public TMP_Text cardBDefText;
    public TMP_Text cardBArtAuthorText;
    public Image cardBImage;
    public Image cardUpkeepBcg;
    public Image cardAttIcon;
    public Image cardDefIcon;
    public Image cardFlag;
    public Image cardBack;
    public Image cardBackGround;
    public Image cardImageBackGround;
    public Image cardMarkImage;
    public Transform specAbLateralPanel;
    public Transform specAbLateralPanelContent;
    public Transform specAbIconPanel;
    public GameObject specAbSlotPrefab;
    public GameObject specAbIconPrefab;
    public Transform ammoPanel;
    public GameObject ammoDotPrefab;

    [Header("AI")]
    // AI ZMIENNE DO WYMIANY KARTY W FAZIE WSTEPNEJ GRY
    public int AIchangef;
    // AI ZMIENNE DO ZAGRANIA KARTY JEDNOSTKA
    public List<SlotAI> AIdeploySlots = new List<SlotAI>();
    public BattleSlot bestTargetSlot = null;
    public int bestTargetSlotAIF = -10;
    // AI ZMIENNE DO ZAGRANIA KARTY PRZEDMIOT
    public List<ItemAI> AIItemPos = new List<ItemAI>();
    public Unit bestItemTarget = null;
    public int bestItemTargetAIF = -10;
    // AI ZMIENNE DO ZAGRANIA KARTY ACTION (INNY SPOSOB. CLASA ACTION AI JEST JEDYNA I PRZECHOWUJE WARTOSCI WSZYSTKICH OPCJI ZAGRANIA)
    [SerializeField]
    public ActionAI actionDataAIs = new ActionAI();


    // AI - Obsluga klasy SLOTAI, ktora zachowuje sloty jako obiekty i zmienne AI do deploy, move, action.

    public void AddPlayableItemToPlay(Unit unit)
    {
        bool unitFound = false;
        if (AIItemPos.Count > 0)
        {
            foreach (ItemAI itemAI in AIItemPos)
            {
                if (itemAI.unit == unit)
                {
                    unitFound = true;
                }
            }
        }
        if (!unitFound)
        {
            ItemAI newItemAI = new ItemAI();
            newItemAI.AddUnitToPlayItem(unit);
            AIItemPos.Add(newItemAI);
        }
    }

    public void AddDeployableSlotAI(BattleSlot slot)
    {
        bool slotFound = false;
        if (AIdeploySlots.Count > 0)
        {
            foreach (SlotAI slotAI in AIdeploySlots)
            {
                if (slotAI.slot == slot)
                {
                    slotFound = true;
                }
            }
        }
        if (!slotFound)
        {
            SlotAI newSlotAI = new SlotAI();
            newSlotAI.AddDeploySlot(slot);
            AIdeploySlots.Add(newSlotAI);
        }
        else
        {
            Debug.LogWarning("SLOT to ADD AS SLOTAI OF UNIT is already set");
        }
    }

    public void ClearAISlots()
    {
        AIdeploySlots.Clear();
    }

    // AI Obsulga Listy AI ITEM

    public void ClearAIItemPos()
    {
        AIItemPos.Clear();
    }

    public void ClearAIAction()
    {
        actionDataAIs.ClearAllValues();
    }

    // SET CARD

    public void SetCard(CardSO cardSO)
    {
        this.cardSO = cardSO;
        SetCardVisuals();

    }

    public void SetCardVisuals()
    {
        ammoPanel.gameObject.SetActive(false);
        cardBack.sprite = cardSO.GetOwner().playerSO.playerDeckImage;
        cardBackGround.color = cardSO.GetOwner().playerSO.playerColor;
        cardImageBackGround.color  = cardSO.GetOwner().playerSO.playerColor;
        cardBNameText.text = cardSO.cardName;
        cardBDescriptionText.text = cardSO.cardDescription;
        if (cardSO.cardTypeSO.cardType == CardType.Unit)
        {
            cardBTypeText.text = "Unit";
        }
        else if (cardSO.cardTypeSO.cardType == CardType.Item)
        {
            cardBTypeText.text = "Item";
        }
        else if (cardSO.cardTypeSO.cardType == CardType.Action)
        {
            cardBTypeText.text = "Action";
        }
        cardBCostText.text = cardSO.cardCost.ToString();
        cardBUpkeepText.text = cardSO.cardUpkeep.ToString();
        // ATTACK AND DEFENCE
        // 1. IF ITEM
        if (cardSO.cardTypeSO.cardType == CardType.Item)
        {
            cardBAttText.text = "+" + cardSO.baseAtt.ToString();
            cardBDefText.text = "+" + cardSO.baseDef.ToString();
            // 1.1. IF ITEM WITH AMMO PANEL
            ItemTypeSO itemType = cardSO.cardTypeSO as ItemTypeSO;
            if (itemType.maxAmmo > 0)
            {
                ammoPanel.gameObject.SetActive(true);
                for (int i = 0; i < itemType.maxAmmo; i++)
                {
                    GameObject ammoDot = Instantiate(ammoDotPrefab, ammoPanel.transform.GetChild(1));
                }
            }
        }
        // 2. IF UNIT
        else if (cardSO.cardTypeSO.cardType == CardType.Unit)
        {
            cardBAttText.text = cardSO.baseAtt.ToString();
            cardBDefText.text = cardSO.baseDef.ToString();
        }
        // IF ACTION
        else if (cardSO.cardTypeSO.cardType == CardType.Action)
        {
            //Debug.Log("TO DO ACTION CARD STATS VISUALS");
            cardBAttText.text = ""; 
            cardBDefText.text = "";
            cardAttIcon.color = new Color32(0, 0, 0, 0);
            cardDefIcon.color = new Color32(0, 0, 0, 0);
        }
        // AUTHOR TEXT
        cardBArtAuthorText.text = cardSO.artAuthor;
        // IMAGE
        cardBImage.sprite = cardSO.cardImage;
        // FLAG
        if (cardSO.nationality == Nationality.None)
        {
            cardFlag.color = new Color32(0, 0, 0, 0);
        }
        else
        {
            cardFlag.sprite = cardSO.cardFlag;
        }
        // UPKEEP
        if (cardSO.cardUpkeep == 0)
        {
            cardUpkeepBcg.gameObject.SetActive(false);
        }
        else
        {
            cardUpkeepBcg.gameObject.SetActive(true);
            GameObject specAbSlotUpkeep = Instantiate(specAbSlotPrefab, specAbLateralPanelContent);
            specAbSlotUpkeep.transform.GetChild(0).GetComponent<TMP_Text>().text = "Upkeep";
            specAbSlotUpkeep.transform.GetChild(1).GetComponent<TMP_Text>().text = "To activate the unit you must pay "+ cardSO.cardUpkeep + " ducats";
            specAbSlotUpkeep.transform.GetChild(2).GetComponent<Image>().sprite = cardUpkeepBcg.sprite;
        }
        // SPEC ABS
        foreach (SpecialAbilitySO specAb in cardSO.specialAbilityList)
        {
            // ICON PANEL
            GameObject specAbIcon = Instantiate(specAbIconPrefab, specAbIconPanel);
            specAbIcon.GetComponent<Image>().sprite = specAb.specAbIcon;
            // LATERAL PANEL
            GameObject specAbSlot = Instantiate(specAbSlotPrefab, specAbLateralPanelContent);
            specAbSlot.GetComponent<CardSpecAbSlot>().SetCardSpecAbSlot(specAb);
        }
    }


    public void SetRewers()
    {
        cardBack.gameObject.SetActive(true);
        GetComponent<CardBehaviour>().SetInactive();
    }

    public void SetAwers()
    {
        // SET CARD BEHAVIOUR WHEN ISN`T IN HAND
        if (GetComponent<CardBehaviour>().CheckIfInDeck()) return;
        if (GetComponent<CardBehaviour>().CheckIfInUsedPile()) return;
        // SET AWERS
        cardBack.gameObject.SetActive(false);
        GetComponent<CardBehaviour>().SetActive();
    }

    // FOR PURPOSES TO MAKE VISIBLE DURING DRAW ANIMATION ONLY (1.2s)
    // LATER SET AWERS NORMALLY AND ACTIVATE
    public void SetAwersButNotActivate()
    {
        if (GetComponent<CardBehaviour>().CheckIfInDeck()) return;
        if (GetComponent<CardBehaviour>().CheckIfInUsedPile()) return;
        if (GetComponent<CardBehaviour>().CheckIfInStartDeck()) return;
        // SET AWERS
        cardBack.gameObject.SetActive(false);
    }

    public void SetRewersForTurnEnd()
    {
        // SET CARD BEHAVIOUR WHEN ISN`T IN HAND
        if (GetComponent<CardBehaviour>().CheckIfInDeck()) return;
        if (GetComponent<CardBehaviour>().CheckIfInUsedPile()) return;
        // SET REWERS
        SetRewers();
    }

    public void SetAwersForTurnStart()
    {
        // SET CARD BEHAVIOUR WHEN ISN`T IN HAND
        if (GetComponent<CardBehaviour>().CheckIfInDeck()) return;
        if (GetComponent<CardBehaviour>().CheckIfInUsedPile()) return;
        // ACTIVATE (ONLY IF HUMAN)
        if (cardSO.GetOwner().isHuman)
        {
            SetAwers();
        }
        else
        {
            SetRewers();
        }
    }
    
    public void UnsubscribeEvents()
    {
        GetComponent<CardBehaviour>().UnSubscribeEvents();
    }

    
}
