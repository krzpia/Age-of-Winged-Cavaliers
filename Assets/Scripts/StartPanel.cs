using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class StartPanel : MonoBehaviour
{
    public TMP_Text playerNameText;
    public Image playerImage;
    public Image playerCoatOfArms;
    public Transform cardContainer;
    public Transform temporaryCardContainer;
    public Button changeButton;
    public bool cardsExchanged;
    public bool buttonToChangeCards = false;
    public List<GameObject> cardsToChange;
    public Player owner;

    public void EndStartPanel()
    {
        // TO DO ANIMATION?
        gameObject.SetActive(false);
    }

    public void AIExchangeCards()
    {
        StartCoroutine(ExchangeMarkedCards());
    }

    public IEnumerator ExchangeMarkedCards()
    {
        int amount = 0;
        List<GameObject> tempCardsToChange = new List<GameObject>();
        // MOVING CARD AND MARKING SIBLING INDEX
        foreach (GameObject card in cardsToChange)
        {
            LeanTween.move(card, new Vector3(card.GetComponent<RectTransform>().position.x,-200,0), 0.4f).setEase(LeanTweenType.easeOutSine);
            tempCardsToChange.Add(card);
            amount++;
        }
        yield return new WaitForSeconds(0.5f);
        // CHANGE PARENT (SEND TO DECK)
        for (int i = 0; i < amount; i++)
        {
            //1. SET PARENT OF EXCHANGED CARD TO TEMP CARD CONTAINER
            GameObject card = tempCardsToChange[i];
            // Debug.Log(card.GetComponent<Card>().cardSO.cardName + " TO SEND TO DECK, BUT FIRST TO TEMP CARD CONT");
            PutCardInTempCardContAtTheSameIndex(card);

            // 2. NEW CARD FROM DECK TO TEMP CONTAINER, ANIMATING
            GameObject cardToPut = owner.GetRandomCardFromDeck();
            PutCardInTempCardContAtIndex(cardToPut, card.transform.parent.GetSiblingIndex());
            cardToPut.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
            cardToPut.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.5f);
            cardToPut.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 0, 200);
            cardToPut.GetComponent<CardBehaviour>().SetToStartDeck();
            if (owner.isHuman)
            {
                cardToPut.GetComponent<Card>().SetAwers();
            }
            else
            {
                cardToPut.GetComponent<Card>().SetRewers();
            }
            LeanTween.move(cardToPut, new Vector3(cardToPut.GetComponent<RectTransform>().position.x, 260, 0), 0.4f).setEase(LeanTweenType.easeOutSine); 
            // 3. CHANGED CARD SENDING TO DECK, positioning.
            card.GetComponent<CardBehaviour>().PutCardToDeck();
            card.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
            card.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.5f);
            card.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 0, 200);
        }
        yield return new WaitForSeconds(0.5f);
        // 4. CARDS TO PUT SET PARENT TO START PANEL (IN TEMP DECK CONT)
        int slotIndex = 0;
        foreach (Transform slot in temporaryCardContainer)
        {
            if (slot.childCount > 0)
            {
                slot.GetChild(0).SetParent(cardContainer.GetChild(slotIndex), true);
            }
            slotIndex++;
        }
        // WAIT UNTIL COMPLETE
        yield return new WaitForSeconds(0.4f);
        // OVERALL PROCEDURE LOGIC, ACTIVATE BUTTON TO PROCEED
        if (owner.isHuman)
        {
            cardsExchanged = true;
            cardsToChange.Clear();
            changeButton.interactable = true;
        }
        else
        {
            cardsExchanged = true;
            cardsToChange.Clear();
            Debug.Log("AI Exchange Animation END, proceed to Swich player");
            onClickButton();
        }
    }

    private void PutCardInTempCardContAtTheSameIndex(GameObject card)
    {
        card.transform.SetParent(temporaryCardContainer.GetChild(CheckCardIndexInStarPanel(card)));
    }

    private void PutCardInTempCardContAtIndex(GameObject card, int index)
    {
        card.transform.SetParent(temporaryCardContainer.GetChild(index));
    }

    private int CheckCardIndexInStarPanel(GameObject card)
    {
        // Debug.Log("CARD SIBLING INDEX: " + card.transform.parent.GetSiblingIndex());
        return card.transform.parent.GetSiblingIndex();
    }

    public int GetFirstFreeSlotIndexInStartPanel()
    {
        int index = 0;
        foreach (Transform slot in cardContainer)
        {
            Debug.Log("SLOT : " + slot.gameObject.name + " of index: "+ index);
            if (slot.childCount == 0)
            {
                return index;
            }
            index++;
        }
        Debug.LogWarning("NO EMPTY SLOT IN START PANEL! TRYING TO PUT THERE MORE THAN FIVE?");
        return index;
    }

    public void AISetListOfStartCards()
    {
        // AI PURPOSES 
        List<Card> list = new List<Card>();
        foreach (Transform slot in cardContainer)
        {
            if (slot.childCount != 0)
            {
                list.Add(slot.GetChild(0).GetComponent<Card>());
            }
        }
        if (!owner.isHuman) 
        {
            owner.GetComponent<AI>().AISetStarterCards(list);
        }
    }

    public void RefreshButton()
    {
        // vs AI
        if (!GameManager.instance.hotSeat)
        {
            //Debug.Log("REFRESH BUTTON, vs AI VERSION");
            if (owner.isHuman)
            {
                changeButton.interactable = true;
                if (cardsExchanged)
                {
                    changeButton.GetComponentInChildren<TMP_Text>().text = "Start Battle";
                    buttonToChangeCards = false;
                }
                else
                {
                    if (CheckIfAnySelected())
                    {
                        changeButton.GetComponentInChildren<TMP_Text>().text = "Change Cards";
                        buttonToChangeCards = true;
                    }
                    else
                    {
                        changeButton.GetComponentInChildren<TMP_Text>().text = "Start Battle";
                        buttonToChangeCards = false;
                    }
                }
            }
            else
            {
                // AI PURPOSES TO SET START CARDS (AI will change it if so)
                AISetListOfStartCards();
                changeButton.interactable = false;
                Debug.Log("PREPARED TO START CORUTINE WITH AI CHANGE CARDS");
            }

        }
        // HOT SEAT
        else
        {
            changeButton.interactable = true;
            //Debug.Log("REFRESH BUTTON, HOT SEAT VERSION");
            if (cardsExchanged)
            {
                changeButton.GetComponentInChildren<TMP_Text>().text = "Start Battle";
                buttonToChangeCards = false;
            }
            else
            {
                if (CheckIfAnySelected())
                {
                    changeButton.GetComponentInChildren<TMP_Text>().text = "Change Cards";
                    buttonToChangeCards = true;
                }
                else
                {
                    changeButton.GetComponentInChildren<TMP_Text>().text = "Start Battle";
                    buttonToChangeCards = false;
                }
            }
        }
    }

    public void ClearChangeMarks()
    {
        foreach (Transform slot in cardContainer)
        {
            Transform cardT = slot.GetChild(0);
            cardT.GetComponent<CardBehaviour>().ClearChangeMark();
        }
    }

    public bool CheckIfAnySelected()
    {
        foreach (Transform slot in cardContainer)
        {
            Transform cardT = slot.GetChild(0);
            if (cardT.GetComponent<CardBehaviour>().CheckIfMarkedToChange())
            {
                return true;
            }
        }
        return false;
    }

    public void onClickButton()
    {
        if (GameManager.instance.hotSeat)
        {
            if (buttonToChangeCards)
            {
                // CHANGE CARDS
                ClearChangeMarks();
                StartCoroutine(ExchangeMarkedCards());
                RefreshButton();
                changeButton.interactable = false;
            }
            else
            {
                // GO ON OTHER PLAYER OR START BATTLE
                cardsExchanged = true;
                RefreshButton();
                PutCardsInHand();
                GameManager.instance.SwitchDrawPhase();
                changeButton.interactable = false;
            }
        }
        else
        {
            if (buttonToChangeCards)
            {
                // CHANGE CARDS
                ClearChangeMarks();
                StartCoroutine(ExchangeMarkedCards());
                RefreshButton();
                changeButton.interactable = false;
            }
            else
            {
                // GO ON OTHER PLAYER OR START BATTLE
                cardsExchanged = true;
                RefreshButton();
                PutCardsInHand();
                GameManager.instance.AISwitchDrawPhase();
                changeButton.interactable = false;
            }
        }
        
    }

    public void AnyCardClick()
    {
        RefreshButton();
    }

    public void PutCardsInHand()
    {
        int maxCards = cardContainer.childCount;
        if (cardContainer.childCount >5) 
        {
            Debug.LogWarning("MORE THAN FIVE SLOTS IN START PILE");
        }
        else if (cardContainer.childCount < 5)
        {
            Debug.LogWarning("LESS THAN FIVE SLOTS IN START PILE");
        }
        List<GameObject> tempList = new List<GameObject>();
        for (int i = 0; i<maxCards; i++)
        {
            cardContainer.GetChild(i).GetChild(0).GetComponent<CardBehaviour>().PutCardToHand();
        }
    }

    
}
