using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BattleSlot : MonoBehaviour, IDropHandler
{
    //public bool occupied;
    [SerializeField]
    private bool active;
    public int x;
    public int y;
    public Player owner;
    public Transform unitSlot;
    public Image actImage;



    void Awake()
    {
        // SUBSCRIBE EVENTS
        GameEvents.current.onShowPosibleBattleSlot += HighlightBattleSlotToPlayCard;
        GameEvents.current.onShowPosibleMovement += HighlightBattleSlotToMoveUnit;
        GameEvents.current.onEndShowPosibleBattleSlot += EndHighlightSlot;
    }

    // AI LOGIC 

    public BattleSlot GetSlotByXY(int x, int y)
    {
        foreach (BattleSlot slot in GameManager.instance.actPlayer.GetListOfAllSlots())
        {
            if (slot.x == x)
            {
                if (slot.y == y)
                {
                    return slot;
                }
            }
        }
        Debug.LogWarning("SLOT NOT FOUD!");
        return null;
    }

    public BattleSlot GetLeftNeighbour()
    {
        if (x > 0 & x < 4)
        {
            return GetSlotByXY(x - 1, y);
        }
        else if (x > 4 & x < 8)
        {
            return GetSlotByXY(x - 1, y);
        }
        else
        {
            return null;
        }
        
    }

    public BattleSlot GetRightNeighbour()
    {
        if (x >= 0 & x < 3)
        {
            return GetSlotByXY(x + 1, y);
        }
        else if (x >=4 & x < 7)
        {
            return GetSlotByXY(x + 1, y);
        }
        else
        {
            return null;
        }
    }

    public int GetDistanceFromSlot(BattleSlot slot)
    {
        int distance = 0;
        int xD = 0;
        int yD = 0;
        xD = Mathf.Abs(slot.x - x);
        yD = Mathf.Abs(slot.y - y);
        distance = xD + yD;
        return distance;
    }

    public List<Unit> GetListOfUnitsCanAttackThisSlot()
    {
        // TO DO PROTECTION IF SO!
        List<Unit> units = new List<Unit>();
        List<Unit> allEnemies = owner.GetListOfEnemyUnits();
        foreach (Unit unit in allEnemies)
        {
            if (unit.cardSO.CheckIfHasSpecAb("Maneuver"))
            {
                units.Add(unit);
            }
            else if (!unit.CheckSiege())
            {
                if (x < 4)
                {
                    if (unit.GetSlot().x < 4)
                    {
                        units.Add(unit);
                    }
                }
                else
                {
                    if (unit.GetSlot().x >= 4)
                    {
                        units.Add(unit);
                    }
                }
            }
            else if (unit.CheckSiege())
            {
                units.Add(unit);
            }
        }
        return units;
    }

    public List<Unit> GetListOfUnitsICanAttackFromThisSlot(CardSO myUnit)
    {
        List<Unit> unitsICanAttack = new List<Unit>();
        List<Unit> allEnemies = owner.GetListOfEnemyUnits();
        foreach (Unit unit in allEnemies)
        {
            if (myUnit.GetUnitType() == UnitType.Artillery)
            {
                unitsICanAttack.Add(unit);
            }
            if (myUnit.CheckIfHasSpecAb("Maneuver"))
            {
                // TO DO PROTECTION IF SO!
                unitsICanAttack.Add(unit);
            }
            else
            {
                if (unit.GetSlot().x < 4 && x < 4)
                {
                    unitsICanAttack.Add(unit);
                }
                if (unit.GetSlot().x >= 4 && x >= 4)
                {
                    unitsICanAttack.Add(unit);
                }
            }
        }
        return unitsICanAttack;
    }

    public bool CheckIfICanAttackEnemyTentFromThisSlot(CardSO myUnit)
    {
        bool canAttack = false;
        Player enemy = GameManager.instance.GetOtherPlayer(owner);
        bool leftProtected = enemy.tent.GetComponent<TentSlot>().CheckIfLeftSideProtected();
        bool rightProtected = enemy.tent.GetComponent<TentSlot>().CheckIfRightSideProtected();
        if (myUnit.GetUnitType() == UnitType.Artillery)
        {
            return true;
        }
        if (myUnit.CheckIfHasSpecAb("Maneuver"))
        {
            if (!leftProtected || !rightProtected)
            {
                canAttack = true;
            }
        }
        else
        {
            if (!leftProtected && x < 4)
            {
                canAttack = true;
            }
            if (!rightProtected && x >= 4)
            {
                canAttack = true;
            }
        }
        return canAttack;
    }

    public List<Unit> GetNeighbourUnits()
    {
        List<Unit> neighbours = new List<Unit>();
        Unit rightNeighbour = GetRightNeighbour().GetUnit();
        Unit leftNeighbour = GetLeftNeighbour().GetUnit();
        if (rightNeighbour != null)
        {
            neighbours.Add(rightNeighbour);
        }
        if (leftNeighbour != null)
        {
            neighbours.Add(leftNeighbour);
        }
        return neighbours;
    }

    public List<Unit> GetListOfUnitsInLine()
    {
        List<Unit> units = new List<Unit>();
        if (x < 4)
        {
            // LEFT LINE
            for (int i = 0; i <4; i++)
            {
                if (GetSlotByXY(i, y).CheckIfOccupied())
                {
                    units.Add(GetSlotByXY(i,y).GetUnit());
                }
            }
        }
        else
        {
            // RIGHT LINE
            for (int i = 4; i < 8; i++)
            {
                if (GetSlotByXY(i, y).CheckIfOccupied())
                {
                    units.Add(GetSlotByXY(i, y).GetUnit());
                }
            }
        }
        return units;
    }

    // LOGIC

    public bool CheckIfOccupied()
    {
        if (unitSlot.transform.childCount == 0)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    // HIGHLOGHT

    private void HighlightBattleSlotToPlayCard(GameObject cardToPlay)
    {
        // MOZNA ZAGRAC GDY? NIE JEST ZAJETY
        if (CheckIfOccupied()) { return; }
        // MOZNA ZAGRAC GDY? JEST TWOJ
        if (cardToPlay.GetComponent<Card>().cardSO.GetOwner() != owner) { return; }
        // MOZNA ZAGRAC GDY MASZ ZLOTO
        if (GameManager.instance.actPlayer.playerActGold < cardToPlay.GetComponent<Card>().cardSO.cardCost)
        {
            return;
        }
        // MOZNA ZAGRAC GDY KARTA TO JEDNOSTKA (akcja - zrobic oddzielna funkcje)
        if (cardToPlay.GetComponent<Card>().cardSO.cardTypeSO.cardType == CardType.Unit)
        {
            GetComponent<Image>().color = new Color32(150, 255, 70, 40);
            actImage.gameObject.SetActive(true);
            actImage.sprite = GameManager.instance.placeIcon;
            actImage.color = new Color32(150, 200, 70, 200);
        }
        
    }

    private void EndHighlightSlot() 
    {
        actImage.gameObject.SetActive(false);
        GetComponent<Image>().color = new Color32(255, 255, 255, 40);
    }

    private void HighlightBattleSlotToMoveUnit(Unit unitToMove)
    {
        // TO DO
        // 1. Jezeli zajety
        if (CheckIfOccupied()) { return; }
        // 2. TYLKO JEZELI JEDNOSTKA MA SPEC AB FLANKING MOZE ZMIENIC MIEJCE!
        if (unitToMove.cardSO.CheckIfHasSpecAb("Flanking"))
        {
            if (unitToMove.cardSO.GetOwner() == owner)
            {
                //Debug.Log("HIGHLIGHT SLOT: " + x + ", " + y);
                actImage.gameObject.SetActive(true);
                actImage.sprite = GameManager.instance.moveIcon;
                actImage.color = new Color32(150, 255, 70, 200);
                GetComponent<Image>().color = new Color32(150, 255, 70, 70);
            }  
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        // Debug.Log("ON DROP: " + eventData.pointerDrag.name);
        // ONLY WITH LEFT BUTTON!
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            // JEZELI UPUSZCzAM UNIT
            if (eventData.pointerDrag.GetComponent<Unit>() != null)
            {
                // RESET RED HIGHLIGHT
                GameEvents.current.EndHighlightUnit();
                // IF CAN INTERACT!
                if (!eventData.pointerDrag.GetComponent<UnitBehaviour>().canInteract) return;
                // RUSZAM TYLKO JEDNOSTKAMI GRACzA OBECNIE AKTYWNEGO 
                if (eventData.pointerDrag.GetComponent<Unit>().cardSO.GetOwner() != GameManager.instance.actPlayer)  return; 
                // RUSZAM TYLKO JEDNOSTKAMI AKTYWNYMI
                if (!eventData.pointerDrag.GetComponent<Unit>().CheckActive())  return;
                // RUSZAM TYLKO JEDNOSTKAMI ZE ZDOLNOSCIA FLANKIG
                if (eventData.pointerDrag.GetComponent<Unit>().cardSO.CheckIfHasSpecAb("Flanking"))
                {
                    // RUSZAM TYLKO NA SWOJE POLA
                    if (eventData.pointerDrag.GetComponent<Unit>().cardSO.GetOwner() == owner)
                    {
                        if (!CheckIfOccupied())
                        {
                            Debug.Log("MOVING UNIT");
                            //eventData.pointerDrag.GetComponent<Unit>().CalculateMovementTo(this);
                            eventData.pointerDrag.GetComponent<Unit>().MoveUnitTo(this);
                        }
                    }
                }
            }
            // JEZELI UPUSZCZAM CARD
            else if (eventData.pointerDrag.GetComponent<Card>() != null)
            {
                // MOZNA ZAGRAC GDY WLACzONA INTERACTIVITY
                if (!eventData.pointerDrag.GetComponent<CardBehaviour>().canInteract) return;
                // MOZNA ZAGRAC GDY AKTYWNY GRACZ
                if (!active) { return; }
                // MOZNA ZAGRAC GDY SLOT WOLNY
                if (CheckIfOccupied()) { return; }
                // MOZNA ZAGRAC GDY SLOT JEST TWOJ
                if (eventData.pointerDrag.GetComponent<Card>().cardSO.GetOwner() != owner) { return; }
                //Debug.Log("DROP HERE");
                //Debug.Log("GAMEOBJECT: " + eventData.pointerDrag.name);
                if (eventData.pointerDrag.GetComponent<Card>() != null)
                {
                    //Debug.Log("Droping GameObject Card");
                    CardSO card = eventData.pointerDrag.GetComponent<Card>().cardSO;
                    if (card.cardTypeSO.cardType == CardType.Unit)
                    {
                        //Debug.Log("PLAYING " + card.cardName);
                        // CHECK IF HAS GOLD
                        if (GameManager.instance.actPlayer.playerActGold >= card.cardCost)
                        {
                            // PAY GOLD!
                            GameManager.instance.actPlayer.playerActGold -= card.cardCost;
                            // CREATE UNIT
                            PlayUnit(card);
                            // VISUALS
                            GameEvents.current.EndShowPosibleBattleSlot();
                            GameEvents.current.CardEndDrag();
                            SendCardToPlayedContainer(eventData.pointerDrag);
                            // REFRESH VISUALS
                            GameManager.instance.actPlayer.RefreshAttributeVisuals();
                            //GameManager.instance.actPlayer.RefreshUpkeepText();
                        }
                        else
                        {
                            Debug.Log("NOT ENOUGH GOLD!");
                        }

                    }
                }
            }
        }
    }

    public Unit GetUnit()
    {
        if (unitSlot.childCount== 0) 
        {
            //Debug.LogWarning("NO UNIT ON SLOT, func:GetUnit()");
            return null; 
        }
        return unitSlot.GetChild(0).GetComponent<Unit>();
    }

    public void PlayUnit(CardSO cardUnit)
    {
        GameObject unit = Instantiate(GameManager.instance.unitPrefab, unitSlot);
        unit.GetComponent<Unit>().CreateUnit(cardUnit);
    }

    public void DeactivateCardDrop()
    {
        active = false;
    }

    public void ActivateCardDrop()
    {
        active = true;
    }

    private void SendCardToPlayedContainer(GameObject cardGO)
    {
        Card card = cardGO.GetComponent<Card>();
        card.GetComponent<Card>().UnsubscribeEvents();
        card.cardSO.GetOwner().SendCardToPlayedContainer(cardGO);
        card.gameObject.SetActive(false);
    }

}
