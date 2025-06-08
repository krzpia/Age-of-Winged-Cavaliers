
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEditor.Timeline.Actions.MenuPriority;

public class UnitBehaviour : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IEndDragHandler, IDragHandler, IDropHandler
{
    Animator unitAnimator;
    [SerializeField]
    private float unitHoverTime = 1.5f;
    private float pointerTimer = 0f;
    private bool pointerHover = false;
    private DragLineController line;
    [SerializeField]
    private GameObject lateralPanel;

    public bool canInteract = true;
    public bool canZoom = true;

    void Awake()
    {
        unitAnimator = GetComponent<Animator>();
        line = GetComponent<DragLineController>();

        GameEvents.current.onShowPosibleUnitsToPutItem += HighlightUnitToPutItem; 
        GameEvents.current.onShowPosibleAttacks += HighlightUnitToAttack;
        GameEvents.current.onShowPosibleOffensiveAction += HighlightUnitToOffensiveAction;
        GameEvents.current.onEndHighlightUnit += EndHighlightUnit;
        GameEvents.current.onStartInteraction += StartInteraction;
        GameEvents.current.onBlockInteraction += BlockInteraction;
        GameEvents.current.onBlockZoom += BlockZoom;
        GameEvents.current.onUnBlockZoom += UnBlockZoom;
    }

    private void StartInteraction()
    {
        canInteract = true;
    }

    private void BlockInteraction()
    {
        canInteract = false;
    }

    private void HighlightUnitToPutItem(Card cardItem)
    {
        if (cardItem == null) return;
        if (cardItem.cardSO.cardTypeSO.cardType != CardType.Item) return;
        //if (cardItem.cardSO.GetOwner() == GetComponent<Unit>().cardSO.GetOwner()) return;
        if (cardItem.cardSO.cardCost > GameManager.instance.actPlayer.playerActGold) return;
        // PODSWIETLAM NA ZIELONO JEDNOSTKE
        ItemTypeSO itemType = cardItem.cardSO.cardTypeSO as ItemTypeSO;
        UnitTypeSO unitType = GetComponent<Unit>().cardSO.cardTypeSO as UnitTypeSO;
        if (itemType.compatibilityList.Contains(unitType.unitType))
        {
            GetComponent<Unit>().highlightImage.gameObject.SetActive(true);
            GetComponent<Unit>().activityImage.gameObject.SetActive(true);
            GetComponent<Unit>().activityImage.sprite = GameManager.instance.addItemIcon;
            GetComponent<Unit>().activityImage.color = new Color32(40, 200, 40, 200);
            GetComponent<Unit>().highlightImage.color = new Color32(0, 200, 0, 25);
        }
        
    }


    private void HighlightUnitToAttack(Unit unitAttacking)
    {
        //Debug.Log("HighLightUnit: " + GetComponent<Unit>().cardSO.cardName);
        //Debug.Log("UNIT ATTACKING: " + unitAttacking.cardSO.cardName);
        if (unitAttacking == null) return;
        if (unitAttacking == GetComponent<Unit>())  return;
        if (unitAttacking.cardSO.GetOwner() == GetComponent<Unit>().cardSO.GetOwner()) return;
        // TO DO (2 strony do ataku?)
        if (unitAttacking.CheckIfCanAttack(GetComponent<Unit>()))
        {
            GetComponent<Unit>().highlightImage.gameObject.SetActive(true);
            GetComponent<Unit>().activityImage.gameObject.SetActive(true);
            GetComponent<Unit>().activityImage.sprite = GameManager.instance.attackIcon;
            GetComponent<Unit>().activityImage.color = new Color32(220, 40, 40, 240);
            GetComponent<Unit>().highlightImage.color = new Color32(0, 0, 0, 45);
            GetComponent<Unit>().glowImage.gameObject.SetActive(true);
            GetComponent<Unit>().glowAnimator.SetBool("RedGlow", true);
            
        }
    }

    public void HighlightUnitToOffensiveAction(CardSO cardSO)
    {
        if (cardSO == null) return;
        if (cardSO.cardTypeSO.cardType != CardType.Action) return; // IS ACTION VERIFICATION
        ActionTypeSO actionCardSO = cardSO.cardTypeSO as ActionTypeSO;
        UnitTypeSO unitType = GetComponent<Unit>().cardSO.cardTypeSO as UnitTypeSO;
        Player unitOwner = GetComponent<Unit>().cardSO.GetOwner();
        Player actionOwner = cardSO.GetOwner();
        // OFFENSIVE ACTION - PLAYER OR ENEMY OR BOTH
        if (actionCardSO.enemyUnits == false && actionCardSO.myUnits == false)
        {
            Debug.LogWarning("HIGHLIGHT UNIT: ACTION WITH NO myUnits nor enemyUnits bool assigned in cardTypeSO");
            return;
        }
        else if (actionCardSO.enemyUnits == false && actionCardSO.myUnits)
        {
            if (unitOwner != actionOwner) { return; }
        }
        else if (actionCardSO.enemyUnits && actionCardSO.myUnits == false)
        {
            if (unitOwner == actionOwner) { return; }   
        }
        // CHECK COMPATIBLITY AND DO OFFENSIVE ACTION LOGIC
        if (actionCardSO.CheckTargetUnitCompatibility(GetComponent<Unit>()) && actionCardSO.CheckTargetNationCompatibility(GetComponent<Unit>())) // CHECK COMPATIBILITY
        {
            if (actionCardSO.isDestroyUnit) // KILL ENEMY UNIT
            {
                GetComponent<Unit>().highlightImage.gameObject.SetActive(true);
                GetComponent<Unit>().activityImage.gameObject.SetActive(true);
                GetComponent<Unit>().activityImage.sprite = GameManager.instance.killIcon;
                GetComponent<Unit>().activityImage.color = new Color32(220, 40, 40, 240);
                GetComponent<Unit>().highlightImage.color = new Color32(0, 0, 0, 45);
                GetComponent<Unit>().glowImage.gameObject.SetActive(true);
                GetComponent<Unit>().glowAnimator.SetBool("RedGlow", true);
            }
            else if (actionCardSO.isDamageUnit) // DAMAGE ENEMY UNIT
            {
                GetComponent<Unit>().highlightImage.gameObject.SetActive(true);
                GetComponent<Unit>().activityImage.gameObject.SetActive(true);
                GetComponent<Unit>().activityImage.sprite = GameManager.instance.attackIcon;
                GetComponent<Unit>().activityImage.color = new Color32(220, 40, 40, 240);
                GetComponent<Unit>().highlightImage.color = new Color32(0, 0, 0, 45);
                GetComponent<Unit>().glowImage.gameObject.SetActive(true);
                GetComponent<Unit>().glowAnimator.SetBool("RedGlow", true);
            }
        }
    }
    
    private void EndHighlightUnit()
    {
        //Debug.Log("END SHOW HIGHLIGHT");
        GetComponent<Unit>().activityImage.gameObject.SetActive(false);
        GetComponent<Unit>().highlightImage.gameObject.SetActive(false);
        GetComponent<Unit>().activityImage.color = new Color32(255, 255, 255, 0);
        GetComponent<Unit>().highlightImage.color = new Color32(255, 255, 255, 0);
        if (GetComponent<Unit>().glowImage.gameObject.activeSelf == true)
        {
            GetComponent<Unit>().glowAnimator.SetBool("RedGlow", false);
        }
        GetComponent<Unit>().glowImage.gameObject.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (GameManager.instance.unitIsDragging) return;
        if (canZoom)
        {
            //Debug.Log("POINTER ENTER ON UNIT, timer starts");
            pointerHover = true;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (canZoom)
        {
            // Debug.Log("POINTER EXIT ON UNIT: "  + GetComponent<Unit>().cardSO.cardName + ", timer zero, End Show Big Unit");
            pointerTimer = 0f;
            pointerHover = false;
            EndShowBigUnit();
        }
        
    }

    private void BlockZoom()
    {
        canZoom = false;
    }

    private void UnBlockZoom()
    {
        canZoom = true;
    }

    private void ShowBigUnit()
    {
        unitAnimator.SetBool("unitZoomIn", true);
        SetSuperiorSortingLayer();
    }

    // CALLED BY ANIMATION ZOOM UNIT
    public void ShowSpecAbPanel()
    {
        if (GetComponent<Unit>().cardSO.specialAbilityList.Count > 0) 
        {
            if (GetComponent<Unit>().GetSlot().x == 0)
            {
                //lateralPanel.GetComponent<Image>().color = new Color32(255, 255, 255, 255);
                unitAnimator.SetTrigger("specAbShowRight");
            }
            else
            {
                //lateralPanel.GetComponent<Image>().color = new Color32(255, 255, 255, 255);
                unitAnimator.SetTrigger("specAbShow");
            }
            
        }   
    }

    private void EndShowBigUnit()
    {
        unitAnimator.SetBool("unitZoomIn", false);
        if (GetComponent<Unit>().isFighting)
        {
            SetSuperiorSortingLayer();
        }
        else
        {
            SetStantardSortingLayer();
        }
        
    }

    public void SetStantardSortingLayer()
    {
        // Debug.Log("Seting Standard Sorting Layer for: " + GetComponent<Unit>().cardSO.cardName);
        // ONLY FOR LIVING UNITS
        if (GetComponent<Unit>().unitKilled) return;
        // LAYERS
        GetComponent<Canvas>().sortingLayerName = "InGame";
        lateralPanel.GetComponent<Canvas>().sortingLayerName = "InGame";
        GetComponent<Canvas>().sortingOrder = 3;
        lateralPanel.GetComponent<Canvas>().sortingOrder = 2;
    }

    public void SetSuperiorSortingLayer()
    {
        // Debug.Log("Seting Superior Sorting Layer for: " + GetComponent<Unit>().cardSO.cardName);
        // ONLY FOR LIVING UNITS
        if (GetComponent<Unit>().unitKilled) return;
        // LAYERS
        GetComponent<Canvas>().sortingLayerName = "InGame";
        lateralPanel.GetComponent<Canvas>().sortingLayerName = "InGame";
        GetComponent<Canvas>().sortingOrder = 6;
        lateralPanel.GetComponent<Canvas>().sortingOrder = 5;
    }

    public void SetGraveyardSortingLayer()
    {
        GetComponent<Canvas>().sortingLayerName = "Default";
        lateralPanel.GetComponent<Canvas>().sortingLayerName = "Default";
        GetComponent<Canvas>().sortingOrder = -1;
        lateralPanel.GetComponent<Canvas>().sortingOrder = -2;
    }

    // DRAG UNIT
    public void OnBeginDrag(PointerEventData eventData)
    {
        // SINGLETON TO AVOID OTHER FUNCTIONALITY WHEN IS DRAGGING ANY
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (!GetComponent<Unit>().CheckActive()) { return; }
            if (!canInteract) return;
            pointerTimer = 0f;
            pointerHover = false;
            EndShowBigUnit();
            GameManager.instance.unitIsDragging = true;
            GameEvents.current.ShowPosibleMovement(GetComponent<Unit>());
            GameEvents.current.ShowPosibleAttacks(GetComponent<Unit>());
            //Debug.Log("Start Unit Drag");
            //Debug.Log("line controller:" + line);
        }

    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (!GetComponent<Unit>().CheckActive()) { return; }
            if (!canInteract) return;
            if (GameManager.instance.unitIsDragging)
            {
                line.DrawLine(transform.position, eventData.position);
            }
        } 
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        GameManager.instance.unitIsDragging = false;
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (!GetComponent<Unit>().CheckActive()) { return; }
            if (!canInteract) return;
            //Debug.Log("End Unit Drag");
            GameEvents.current.EndShowPosibleBattleSlot();
            GameEvents.current.EndHighlightUnit();
            ClearLineRenderer();
        }
    }

    // ON DROP UNIT: ATTACK, ON DROP CARD:  DROPPING ITEM OR ACTION
    public void OnDrop(PointerEventData eventData)
    {
        if (!canInteract) return;
        // Debug.Log("ON UNIT DROP");
        // ONLY WITH LEFT BUTTON!
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            // JEZELI KARTA (ITEM, ACTION)
            if (eventData.pointerDrag.GetComponent<Card>() != null)
            {
                Card card = eventData.pointerDrag.GetComponent<Card>();
                if (!card.GetComponent<CardBehaviour>().ChcekIfActive())
                {
                    // DRAGGING AND DROPPING INACTIVE CARD!
                    return;
                }
                // JEZELI KARTA PRZEDMIOT (ITEM)
                if (card.cardSO.cardTypeSO.cardType == CardType.Item)
                {
                    // SPRAWDZAM CzY MAM ZLOTO!
                    if (card.cardSO.cardCost > card.cardSO.GetOwner().playerActGold)
                    {
                        Debug.Log("Not Enough gold");
                        return;
                    }
                    else
                    {
                        // SPRAWDZAM KOMPATYBILNOSC PRZEDMIOTU
                        ItemTypeSO itemType = card.cardSO.cardTypeSO as ItemTypeSO;
                        UnitTypeSO unitType = GetComponent<Unit>().cardSO.cardTypeSO as UnitTypeSO;
                        if (itemType.compatibilityList.Contains(unitType.unitType))
                        {
                            // PLAY ITEM
                            //Debug.Log("DROP ITEM " + card.cardSO.cardName + " ON UNIT");
                            card.cardSO.GetOwner().playerActGold -= card.cardSO.cardCost;
                            GetComponent<Unit>().PutItem(card.cardSO);
                            // VISUALS
                            GameEvents.current.EndShowPosibleBattleSlot();
                            GameEvents.current.EndHighlightUnit();
                            GameEvents.current.CardEndDrag();
                            card.UnsubscribeEvents();
                            card.cardSO.GetOwner().SendCardToPlayedContainer(card.gameObject);
                            card.gameObject.SetActive(false);
                            // REFRESH VISUALS
                            GameManager.instance.actPlayer.RefreshAttributeVisuals();
                        }
                    }
                }
                // ---** ACTION **---- JEZELI KARTA AKCJA (ACTION)
                if (card.cardSO.cardTypeSO.cardType == CardType.Action)
                {
                    ActionTypeSO actionCardSO = card.cardSO.cardTypeSO as ActionTypeSO;
                    Player unitOwner = GetComponent<Unit>().cardSO.GetOwner();
                    Player actionOwner = card.cardSO.GetOwner();
                    // 0. CHECK IF ACTION PLAY METHOD IS VALID
                    if (actionCardSO.actionPlayMethod != ActionPlayMethod.OnUnit)
                    {
                        Debug.LogWarning("Action Play Method NOT VALID");
                        return;
                    }
                    // 1.CHECK PLAYER UNIT OR ENEMY UNIT OR BOTH
                    if (actionCardSO.enemyUnits == false && actionCardSO.myUnits == false)
                    {
                        Debug.LogWarning("PLAY ACTION: ACTION WITH NO myUnits NOr enemyUnits bool assigned in cardTypeSO");
                        return;
                    }
                    else if (actionCardSO.enemyUnits == false && actionCardSO.myUnits)
                    {
                        if (unitOwner != actionOwner) { return; }
                    }
                    else if (actionCardSO.enemyUnits && actionCardSO.myUnits == false)
                    {
                        if (unitOwner == actionOwner) { return; }
                    }
                    // 2. CHECK GOLD -- SPRAWDZAM CzY MAM ZLOTO!
                    if (card.cardSO.cardCost > card.cardSO.GetOwner().playerActGold)
                    {
                        Debug.Log("Not Enough gold");
                        return;
                    }
                    else
                    {
                        // 3. CHECK COMPATIBILITY -- SPRAWDZAM KOMPATYBILNOSC AKCJI
                        // UnitTypeSO unitType = GetComponent<Unit>().cardSO.cardTypeSO as UnitTypeSO;
                        if (actionCardSO.CheckTargetUnitCompatibility(GetComponent<Unit>()) && actionCardSO.CheckTargetNationCompatibility(GetComponent<Unit>())) 
                        {
                            // PLAY ACTION
                            card.cardSO.GetOwner().playerActGold -= card.cardSO.cardCost;
                            GetComponent<Unit>().PlayAction(card.cardSO);
                            // VISUALS
                            GameEvents.current.EndShowPosibleBattleSlot();
                            GameEvents.current.EndHighlightUnit();
                            GameEvents.current.CardEndDrag();
                            card.UnsubscribeEvents();
                            card.cardSO.GetOwner().SendCardToPlayedContainer(card.gameObject);
                            card.gameObject.SetActive(false);
                            // REFRESH VISUALS
                            GameManager.instance.actPlayer.RefreshAttributeVisuals();
                        }
                    }
                }
            }
            // ---xx ATAK xx--- JEZELI JEDNOSTKA TO WYKONUJE ATAK
            if (eventData.pointerDrag.GetComponent<Unit>() != null)
            {
                if (!GameManager.instance.unitIsDragging) return;
                Unit unit = eventData.pointerDrag.GetComponent<Unit>();
                if (!unit.CheckActive()) return;
                if (!unit.GetComponent<UnitBehaviour>().canInteract) return;
                if (unit.cardSO.GetOwner() != GetComponent<Unit>().cardSO.GetOwner())
                {
                    if (unit.CheckIfCanAttack(GetComponent<Unit>()))
                    {
                        unit.AttackUnit(GetComponent<Unit>());
                    }
                    else
                    {
                        Debug.Log("CANNOT ATTACK - BAD BATTLE SIDE");
                    }
                }
            }
        }  
    }

    public void ClearLineRenderer()
    {
        line.EndDrawLine();
    }

    public void UnsubscribeAll()
    {
        GameEvents.current.onShowPosibleAttacks -= HighlightUnitToAttack;
        GameEvents.current.onShowPosibleOffensiveAction -= HighlightUnitToOffensiveAction;
        GameEvents.current.onEndHighlightUnit -= EndHighlightUnit;
        GameEvents.current.onShowPosibleUnitsToPutItem -= HighlightUnitToPutItem;
        GameEvents.current.onStartInteraction -= StartInteraction;
        GameEvents.current.onBlockInteraction -= BlockInteraction;
        GameEvents.current.onBlockZoom -= BlockZoom;
        GameEvents.current.onUnBlockZoom -= UnBlockZoom;

    }

    // UPDATE
    void Update()
    {
        if (pointerHover)
        {
            pointerTimer += Time.deltaTime;
        }
        if (pointerTimer > unitHoverTime)
        {
            if (canZoom)
            {
                ShowBigUnit();
            }
            else
            {
                pointerTimer = 0;
                pointerHover = false;
            }
        } 
        if (!canZoom && !GetComponent<Unit>().isFighting && !GetComponent<Unit>().unitKilled)
        {
            // Debug.Log("Setting End Show Big Unit for: " + GetComponent<Unit>().cardSO.cardName);
            if (unitAnimator.GetBool("unitZoomIn") == false)
            {
                EndShowBigUnit();
            }
        }
    }

    public void StartPulse()
    {
        unitAnimator.SetBool("pulse", true);
    }

    public void EndPulse()
    {
        unitAnimator.SetBool("pulse", false);
    }

}


