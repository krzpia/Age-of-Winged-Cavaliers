using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

public class HandPanelDisplay : MonoBehaviour
{

    private void Awake()
    {
        GameEvents.current.onCardEndDrag += SetCardsArray;
    }

    public float GetNextX()
    {
        float x = 0f;
        if (transform.childCount == 0)
        {
            return 0;
        }
        else
        {
            x = transform.GetChild(0).GetComponent<RectTransform>().localPosition.x;
        }
        Debug.Log("First siling x = " + x);
        return x-80;
    }

    public void SetCardsArray()
    {
        int cardsNo = transform.childCount;
        if (cardsNo <= 5)
        {
            GetComponent<HorizontalLayoutGroup>().spacing = 18;
        }
        else if (cardsNo == 5) 
        {
            GetComponent<HorizontalLayoutGroup>().spacing = 12;
        }
        else if (cardsNo == 6)
        {
            GetComponent<HorizontalLayoutGroup>().spacing = 6;
        }
        else if (cardsNo == 7)
        {
            GetComponent<HorizontalLayoutGroup>().spacing = -10;
        }
        else if (cardsNo == 8)
        {
            GetComponent<HorizontalLayoutGroup>().spacing = -22;
        }
        // SET CARDS z
        int i = 8;
        foreach (Transform cardT in transform)
        {
            cardT.position = new Vector3(cardT.position.x, cardT.position.y, i);
            i--;
        }
    }
}
