using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BattleLinePanel : MonoBehaviour, IDropHandler
{
    public BattleLine battleLine;

    public void Show(Card card)
    {
        Debug.Log("TO SHOW PANEL, CARD TO DRAG: " + card.cardSO.cardName);
        if (card.cardSO.cardTypeSO.cardType == CardType.Action)
        {
            gameObject.SetActive(true);
            GetComponent<Animator>().SetBool("Glow", true);
        }
    }

    public void Hide()
    {
        GetComponent<Animator>().SetBool("Glow", false);
        gameObject.SetActive(false);
    }

    public void OnDrop(PointerEventData eventData)
    {
        //Debug.Log("Droping :" + eventData.pointerDrag.name);
        if (eventData.pointerDrag.GetComponent<Card>() == null) return;
        CardSO cardSO = eventData.pointerDrag.GetComponent<Card>().cardSO;
        // ACTION CARDS ON DROP ON BATTLE LINE
        if (eventData.pointerDrag.GetComponent<Card>().cardSO.cardTypeSO.cardType == CardType.Action)
        {
            if (GameManager.instance.actPlayer.playerActGold >= cardSO.cardCost)
            {
                Debug.Log("Drop Action on Area:" + battleLine.gameObject.name);
                // TYPES OF ACTION:
                battleLine.PlayLineAction(eventData.pointerDrag.GetComponent<Card>());  
            }
            else
            {
                Debug.Log("Not enough Gold");
            }
        }
    }
}
