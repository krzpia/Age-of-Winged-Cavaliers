using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class GameEvents : MonoBehaviour
{
    public static GameEvents current;

    private void Awake()
    {
        current = this;
    }

    public event Action onCardDrag;
    public event Action onCardEndDrag;
    public event Action<GameObject> onShowPosibleBattleSlot;
    public event Action<Unit> onShowPosibleMovement;
    public event Action onEndShowPosibleBattleSlot;
    public event Action<Unit> onShowPosibleAttacks;
    public event Action<CardSO> onShowPosibleOffensiveAction;
    public event Action onEndHighlightUnit;
    public event Action onRefreshUnitVisuals;
    public event Action onStartInteraction;
    public event Action onBlockInteraction;
    public event Action onBlockZoom;
    public event Action onUnBlockZoom;
    public event Action<Card> onShowPosibleUnitsToPutItem;

    public void CardDrag()
    {
        if (onCardDrag != null)
        {
            onCardDrag();
        }
    }
    
    public void CardEndDrag()
    {
        if (onCardEndDrag != null)
        {
            onCardEndDrag();
        }
    }

    public void ShowPosibleBattleSlot(GameObject cardToPlay)
    {
        if (onShowPosibleBattleSlot != null)
        {
            onShowPosibleBattleSlot(cardToPlay);
        }
    }

    public void ShowPosibleMovement(Unit unit)
    {
        if (onShowPosibleMovement != null)
        {
            onShowPosibleMovement(unit);
        }
    }

    public void ShowPosibleAttacks(Unit unit)
    {
        if (onShowPosibleAttacks != null)
        {
            onShowPosibleAttacks(unit);
        }
    }

    public void ShowPosibleOffensiveAction(CardSO cardSO)
    {
        if (onShowPosibleOffensiveAction != null)
        {
            onShowPosibleOffensiveAction(cardSO);
        }
    }

    public void EndShowPosibleBattleSlot()
    {
        if (onEndShowPosibleBattleSlot != null)
        {
            onEndShowPosibleBattleSlot();
        }
    }

    public void EndHighlightUnit()
    {
        if (onEndHighlightUnit != null)
        {
            onEndHighlightUnit();
        }
    }

    public void RefreshUnitVisuals()
    {
        if (onRefreshUnitVisuals != null)
        {
            onRefreshUnitVisuals();
        }
    }

    public void StartInteraction()
    {
        if (onStartInteraction != null)
        {
            onStartInteraction();
        }
    }

    public void BlockInteraction()
    {
        if (onBlockInteraction != null)
        {
            onBlockInteraction();
        }
    }

    public void BlockZoom()
    {
        if (onBlockZoom != null)
        {
            onBlockZoom();
        }
    }

    public void UnBlockZoom()
    {
        if (onUnBlockZoom != null)
        {
            onUnBlockZoom();
        }
    }

    public void ShowPosibleUnitToPutItem(Card card)
    {
        if (onShowPosibleUnitsToPutItem != null)
        {
            onShowPosibleUnitsToPutItem(card);
        }
    }

}
