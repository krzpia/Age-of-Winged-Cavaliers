using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEditor.Experimental.GraphView.GraphView;

public class CardBehaviour : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IEndDragHandler, IDragHandler, IPointerClickHandler
{
    Animator cardAnimator;
    Card card;
    Canvas canvas;
    public Transform parentToSet;
    private Transform myHand;

    [Header("GRAPHICS")]
    public Sprite changeMark;
    public Sprite woundMark;
    
    [Header("LOGIC"), SerializeField]
    private bool isDragging = false;
    public int siblingInt = 0;
    [SerializeField]
    private bool active = true;
    [SerializeField]
    private bool inDeck;
    [SerializeField]
    private bool inUsedPile;
    [SerializeField]
    private bool inStartDeck;
    [SerializeField]
    private bool inHand;
    [SerializeField]
    private bool isMarkedToChange;

    public bool canInteract = true;

    // WSPOLRZEDNE KART
    private int[] cardHandXPos = {62-1100, -44-1100, -151 - 1100, -258 - 1100, -365 - 1100, -442 - 1100, -495 - 1100, -552 - 1100 };


    void Start()
    {
        // SUBSCRIBE EVENTS
        GameEvents.current.onCardDrag += TurnOffInteractivity;
        GameEvents.current.onCardEndDrag += TurnOnInteractivity;
        GameEvents.current.onStartInteraction += TurnOnInteractivity;
        GameEvents.current.onBlockInteraction += TurnOffInteractivity;
        GameEvents.current.onStartInteraction += StartInteraction;
        GameEvents.current.onBlockInteraction += BlockInteraction;
        // COMPONENTS
        cardAnimator = GetComponent<Animator>();
        card = GetComponent<Card>();
        canvas = GetComponent<Canvas>();
        myHand = GetComponent<Card>().cardSO.GetOwner().hand;
    }

    public bool CheckIfInDeck()
    {
        return inDeck;
    }

    public bool CheckIfInUsedPile()
    {
        return inUsedPile;
    }

    public bool CheckIfInStartDeck() 
    {
        return inStartDeck;
    }

    public bool CheckIfMarkedToChange()
    {
        return isMarkedToChange;
    }

    private void SetSiblingInt()
    {
        siblingInt = transform.GetSiblingIndex();
        //Debug.Log(siblingInt);
    }

    public bool ChcekIfActive()
    {
        if (active)
        {
            return true;
        }
        else
        {
            return false;
        }
            
    }

    private void StartInteraction()
    {
        canInteract = true;
    }

    private void BlockInteraction()
    {
        canInteract = false;
    }

    // POINTER CLICK - ONLY IN START DECK OR DEBUG AI MODE
    public void OnPointerClick(PointerEventData eventData)
    {
        if (inDeck) return;
        if (inUsedPile) return;
        if (!active) 
        {
            if (inStartDeck) return;
            //Debug.Log("CLICK - AI TEST DEPLOY SLOT HEATMAP");
            Debug.Log("CLICKED ON: " + GetComponent<Card>().cardSO.cardName);
        }
        else
        {
            if (inStartDeck)
            {
                Player owner = GetComponent<Card>().cardSO.GetOwner();
                if (!owner.startPanel.cardsExchanged)
                {
                    if (!isMarkedToChange)
                    {
                        PutChangeMark();
                        owner.startPanel.cardsToChange.Add(this.gameObject);
                    }
                    else
                    {
                        ClearChangeMark();
                        if (owner.startPanel.cardsToChange.Contains(this.gameObject))
                        {
                            owner.startPanel.cardsToChange.Remove(this.gameObject);
                        }
                    }
                }
                owner.startPanel.AnyCardClick();
            }
        }
        
    }

    // MARKS
    public void PutChangeMark()
    {
        Image checkMark = GetComponent<Card>().cardMarkImage;
        isMarkedToChange = true;
        checkMark.sprite = changeMark;
        checkMark.gameObject.SetActive(true);
        checkMark.color = new Color32(0, 150, 0, 220);
        // ADD TO CHANGE LIST
    }

    public void ClearChangeMark()
    {
        Image checkMark = GetComponent<Card>().cardMarkImage;
        isMarkedToChange = false;
        checkMark.gameObject.SetActive(false);
    }

    public void AImarkToChange()
    {
        isMarkedToChange= true;
    }

    public void AIClearMarkToChange()
    {
        isMarkedToChange= false;
    }

    public bool IfIsMarkedToChange()
    {
        if (isMarkedToChange)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    // GDY KARTA ZMIENIA WLASCICIELA
    public void ChangeMyHand(Transform newHand)
    {
        myHand= newHand;
    }

    // DRAG CARD - ONLY IF IN HAND
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            // SET CARD BEHAVIOUR WHEN ISN`T IN HAND
            if (inDeck) return;
            if (inUsedPile) return;
            if (inStartDeck) return;
            // CAN INTERACT
            if(!canInteract) return;
            // CHECK IF ACTIVE (actPlayer)
            if (!active) { return; }
            // ANIMATION POINTER OVER EXIT!
            cardAnimator.SetBool("CardSPointerOver", false);
            cardAnimator.SetBool("CardNPointerOver", false);
            cardAnimator.SetBool("StartZoom", false);
            // LOGIC (SET SIBLING, PARENT AND SORT ORDER)
            SetSiblingInt();
            isDragging = true;
            parentToSet = myHand;
            transform.SetParent(GameManager.instance.backGround);
            transform.position = eventData.position;
            SetToHandWhenDragging();
            // CARD DRAG - BLOCK INTERACTIVITY
            GameEvents.current.CardDrag();
            // CARD DRAG VISUALS
            GetComponent<CanvasGroup>().alpha = .8f;
            GetComponent<CanvasGroup>().blocksRaycasts = false;
            // Debug.Log("BEGIN DRAG:" + eventData.pointerDrag.name);
            // IF UNIT
            if (GetComponent<Card>().cardSO.cardTypeSO.cardType == CardType.Unit)
            {
                // Debug.Log("Unit:" + eventData.pointerDrag.GetComponent<Card>().cardSO.cardName);
                GameEvents.current.ShowPosibleBattleSlot(eventData.pointerDrag);
            }
            // IF ITEM
            else if (GetComponent<Card>().cardSO.cardTypeSO.cardType == CardType.Item)
            {
                GameEvents.current.ShowPosibleUnitToPutItem(GetComponent<Card>());
            }
            // IF ACTION
            else if (GetComponent<Card>().cardSO.cardTypeSO.cardType == CardType.Action)
            {
                GameManager.instance.ShowPlayActionPosibilities(GetComponent<Card>());
                // Debug.Log("BEGIN DRAG>>> EVENT SHOW TU PLAY ACTION SLOT or AREA");
            }


        }
        
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            // CHECK IF ACTIVE
            if (!active) { return; }
            if (inUsedPile) return;
            if (inStartDeck) return;
            // CAN INTERACT
            if (!canInteract) return;
            // SORT
            SetToHandWhenDragging();
            //Debug.Log("Dragging:" + eventData.pointerDrag.name);
            //Debug.Log("CARD POSITION: "+ eventData.pointerDrag.transform.position);
            //Debug.Log("EVENT DATA POSITION: " + eventData.position);
            transform.position = eventData.position;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            // SET CARD BEHAVIOUR WHEN ISN`T IN HAND
            if (inDeck) return;
            if (inUsedPile) return;
            if (inStartDeck) return;
            // CAN INTERACT
            // if (!canInteract) return;
            // CHECK IF ACTIVE
            // if (!active) { return; }
            // LOGIC
            GetComponent<CanvasGroup>().blocksRaycasts = true;
            GetComponent<CanvasGroup>().alpha = 1f;
            if (parentToSet == null)
            {
                transform.SetParent(myHand, false);
                transform.SetSiblingIndex(siblingInt);
                GameEvents.current.CardEndDrag();
                GameEvents.current.EndShowPosibleBattleSlot();
                GameEvents.current.EndHighlightUnit();
                GameManager.instance.EndShowActionPosibilities();
                SetToHand();
                isDragging = false;
            }
            else
            {
                transform.SetParent(parentToSet, false);
                transform.SetSiblingIndex(siblingInt);
                GameEvents.current.CardEndDrag();
                GameEvents.current.EndShowPosibleBattleSlot();
                GameEvents.current.EndHighlightUnit();
                GameManager.instance.EndShowActionPosibilities();
                SetToHand();
                isDragging = false;
            }
            //Debug.Log("END DRAG END");
        }
    }

    // AIDRAWANIMATION
    // CZAS ANIMACJI - 1s (!) BEZ ANIMATORA, COUROTINE ze skryptu PLAYER ma WaitforSeconds 1.2s.
    public void AIDrawCardAnimation()
    {
        StartCoroutine("AIAnimationCoroutine");
    }

    public IEnumerator AIAnimationCoroutine()
    {
        int yMaxPosN = -300;
        float animationPhaseOneTime = 0.5f;
        float animationPhaseTwoTime = 0.4f;
        int sibling = card.cardSO.GetOwner().hand.childCount;
        if (transform.parent != card.cardSO.GetOwner().deckTransform)
        {
            Debug.LogWarning("CARD OWNER IS NOT THE SAME THAT DECK TRANSFORM!");
        }
        // ANIMATION FOR NORTH AI PLAYER ONLY!
        // PHASE ONE
        LeanTween.moveLocal(gameObject, new Vector3(-800, yMaxPosN, 200), animationPhaseOneTime).setEaseInOutSine();
        yield return new WaitForSeconds(animationPhaseOneTime);
        // PHASE TWO
        LeanTween.moveLocal(gameObject, new Vector3(cardHandXPos[sibling], 0f, 0f), animationPhaseTwoTime).setEaseInOutSine();
        yield return new WaitForSeconds(animationPhaseTwoTime);
        // PUT CARD TO HAND
        PutCardToHand();
    }

    // DRAW ANIMATION
    // CZAS ANIMACJI - 1s (!) BO ANIMATOR KONTROLUJE CzAS i PO 1s AKTYWUJE FUNKCJE
    // takie jak SET PARENT, Bool inHAND etc.
    // ANIMACJE LEANTWEEN MUSZA MIEC RAZEM MNIEJ NIZ 1s (TUTAJ 0.5f + 0.4f = 0.9s)
    public void DrawAnimationPhaseOne()
    {
        int yMaxPosS = 300;
        int yMaxPosN = -300;
        float animationPhaseOneTime = 0.5f;
        if (transform.parent != card.cardSO.GetOwner().deckTransform)
        {
            Debug.LogWarning("CARD OWNER IS NOT THE SAME THAT DECK TRANSFORM!");
        }
        // ANIMATION FOR SOUTH AND NORTH PLAYER
        if (transform.parent == GameManager.instance.playerSouth.deckTransform)
        {
            LeanTween.moveLocal(gameObject, new Vector3(-800, yMaxPosS, 200), animationPhaseOneTime).setEaseInOutSine();
        }
        else if(transform.parent == GameManager.instance.playerNorth.deckTransform)
        {
            LeanTween.moveLocal(gameObject, new Vector3(-800, yMaxPosN, 200), animationPhaseOneTime).setEaseInOutSine();
        }
        else
        {
            Debug.LogWarning("CARD PARENT NOT PLAYERSOUTH NOR PLAYERNORTH DECK");
        }
        
    }

    public void DrawAnimationPhaseTwo()
    {
        int siblingS = GameManager.instance.playerSouth.hand.childCount;
        int siblingN = GameManager.instance.playerNorth.hand.childCount;
        float animationPhaseTwoTime = 0.4f;
        
        // SPRAWDZAM CZY KARTA JEST W DECKU POSIADACzA KARTY
        if (transform.parent != card.cardSO.GetOwner().deckTransform)
        {
            Debug.LogWarning("CARD OWNER IS NOT THE SAME THAT DECK TRANSFORM!");
        }
        // ANIMATION FOR SOUTH AND NORTH PLAYER
        if (transform.parent == GameManager.instance.playerSouth.deckTransform)
        {
            // ANOTHER WAY TO DO animation with destination point?
            //fakeCard.transform.SetParent(GameManager.instance.playerSouth.hand, false);
            //Debug.Log("POSITION OF THE TRANSFORM IN PLAYER HAND: "+ fakeCard.transform.position);
            //fakeCard.transform.SetParent(GameManager.instance.playerSouth.hand.parent, true);
            //Debug.Log("POSITION OF THE TRANSFORM IN DECK after setting parent with world position stay:" + fakeCard.transform.position);
            //Debug.Log("cardHandXPos[siblingS]: " + cardHandXPos[siblingS]);
            LeanTween.moveLocal(gameObject, new Vector3(cardHandXPos[siblingS], 0f, 0f), animationPhaseTwoTime).setEaseInOutSine();

        }
        else if (transform.parent == GameManager.instance.playerNorth.deckTransform)
        {
            // ANOTHER WAY TO DO animation with destination point?
            Debug.Log("cardHandXPos[siblingN]: " + cardHandXPos[siblingN]);
            LeanTween.moveLocal(gameObject, new Vector3(cardHandXPos[siblingN], 0f, 0f), animationPhaseTwoTime).setEaseInOutSine();
        }
        else
        {
            Debug.LogWarning("CARD PARENT NOT PLAYERSOUTH NOR PLAYERNORTH DECK");
        }
    }
    // END OF LEAN TWEEN ANIMATION
    // FURTHER LOGIC CONTROLED BY CARD ANIMATOR

    // CARD FLIP IN THE MIDDLE OF ANIMATION
    // TRIGERRED BY ANIMATOR
    public void CardFlipInDrawAnimation()
    {
        card.cardBack.gameObject.SetActive(false);
    }

    public void SetParentToSet(Transform parentToSet)
    {
        this.parentToSet = parentToSet;
    }

    public void SetInactive()
    {
        active = false;
    }

    public void SetActive()
    {
        active = true;
    }

    // LOGIC
    public void SetToDeck()
    {
        inDeck = true;
        inUsedPile = false;
        inStartDeck= false;
        SetSortingLayer();
        GetComponent<Canvas>().sortingOrder = 1;
        GetComponent<Card>().specAbLateralPanel.GetComponent<Canvas>().sortingOrder = 0;
    }

    public void SetToHand()
    {
        inDeck = false;
        inUsedPile = false;
        inStartDeck = false;
        SetSortingLayer();
        GetComponent<Canvas>().sortingOrder = 5;
        GetComponent<Card>().specAbLateralPanel.GetComponent<Canvas>().sortingOrder = 4;
    }

    private void SetToHandWhenDragging()
    {
        inDeck = false;
        inUsedPile = false;
        inStartDeck = false;
        SetSortingLayer();
        GetComponent<Canvas>().sortingOrder = 7;
        GetComponent<Card>().specAbLateralPanel.GetComponent<Canvas>().sortingOrder = 6;
    }

    public void SetToUsedPile()
    {
        inUsedPile = true;
        inDeck = false;
        inStartDeck = false;
        SetSortingLayer();
        GetComponent<Canvas>().sortingOrder = 1;
        GetComponent<Card>().specAbLateralPanel.GetComponent<Canvas>().sortingOrder = 0;
    }

    public void SetToStartDeck()
    {
        inStartDeck = true;
        inDeck = false;
        inUsedPile = false;
        SetSortingLayer();
        GetComponent<Canvas>().sortingOrder = 5;
        GetComponent<Card>().specAbLateralPanel.GetComponent<Canvas>().sortingOrder = 4;
    }

    // ANIMATION CONTROLLED CARD LOGIC (DRAW CARD)

    public void InAnimationCardGameObjectSetActive()
    {
        transform.GetChild(0).gameObject.SetActive(true);
        GameEvents.current.StartInteraction();
    }

    public void InAnimationSetParentToHand()
    {
        PutCardToHand();
    }

    // CHANGE PHYSICAL LOCATION
    public void PutCardToHand()
    {
        transform.SetParent(GetComponent<Card>().cardSO.GetOwner().hand, false);
        transform.SetSiblingIndex(0);
        transform.position = new Vector3(transform.position.x, transform.position.y, 0); 
        SetToHand();
        GetComponent<Card>().cardSO.GetOwner().hand.gameObject.GetComponent<HandPanelDisplay>().SetCardsArray();
        if (GetComponent<Card>().cardSO.GetOwner().isHuman)
        {
            GetComponent<Card>().SetAwers();
        }
        else
        {
            GetComponent<Card>().SetRewers();
        }
        SetSiblingInt();
    }

    public void PutCardToDeck()
    {
        transform.SetParent(GetComponent<Card>().cardSO.GetOwner().deckTransform, false);
        // TO DO SHUFFLE?
        SetToDeck();
        GetComponent<Card>().SetRewers();
    }

    // INTERACTIVITY
    public void OnPointerEnter(PointerEventData eventData)
    {
        // SET CARD BEHAVIOUR WHEN ISN`T IN HAND
        if (inDeck) return;
        if (inUsedPile) return;
        // CHECK IF ACTIVE
        if (!active) { return; }
        // NO ZOOM WHEN IS DRAGGING
        if (isDragging) { return; }
        // ZOOM WHEN STARTING GAME AND CHOOSING CARD TO CHANGE
        if (inStartDeck)
        {
            cardAnimator.SetBool("StartZoom", true);
            SetSortingLayer();
            canvas.sortingOrder = 7;
            card.specAbLateralPanel.GetComponent<Canvas>().sortingOrder = 6;
            return;
        }
        // IN GAME ZOOM
        if (card.cardSO.GetOwner() == GameManager.instance.playerNorth)
        {
            cardAnimator.SetBool("CardNPointerOver", true);
            SetSortingLayer();
            SetToHandWhenDragging();
        }
        else
        {
            cardAnimator.SetBool("CardSPointerOver", true);
            SetSortingLayer();
            SetToHandWhenDragging();
        }
        
    }


    private void SetSortingLayer()
    {
        if (inStartDeck)
        {
            GetComponent<Canvas>().sortingLayerName = "SuperiorPanel";
            //Debug.Log("SETTING LAYER TO SUPERIOR PANEL CARD.SPECABPANEL: "+ GetComponent<Card>().specAbLateralPanel);
            GetComponent<Card>().specAbLateralPanel.GetComponent<Canvas>().sortingLayerName = "SuperiorPanel";
    
        }
        else
        {
            GetComponent<Canvas>().sortingLayerName = "InGame";
            //Debug.Log("SETTING LAYER TO INGAME CARD.SPECABPANEL: " + GetComponent<Card>().specAbLateralPanel);
            GetComponent<Card>().specAbLateralPanel.GetComponent<Canvas>().sortingLayerName = "InGame";
        }
    }

    public void ShowSpecPanelAtStart()
    {
        if (card.cardSO.specialAbilityList.Count > 0)
        {
            //card.specAbLateralPanel.GetComponent<Image>().color = new Color32(255, 255, 255, 255);
            cardAnimator.SetTrigger("StartLateralPanel");

        }
        if (card.cardSO.cardUpkeep > 0)
        {
            //card.specAbLateralPanel.GetComponent<Image>().color = new Color32(255, 255, 255, 255);
            cardAnimator.SetTrigger("StartLateralPanel");
        }
    }


    public void ShowSpecAbPanelS()
    {
        if (card.cardSO.specialAbilityList.Count > 0) 
        {
            //card.specAbLateralPanel.GetComponent<Image>().color = new Color32(255, 255, 255, 255);
            cardAnimator.SetTrigger("CardSLateralPanel");
            
        }
        if (card.cardSO.cardUpkeep > 0)
        {
            //card.specAbLateralPanel.GetComponent<Image>().color = new Color32(255, 255, 255, 255);
            cardAnimator.SetTrigger("CardSLateralPanel");
        }
    }

    public void ShowSpecAbPanelN()
    {
        if (card.cardSO.specialAbilityList.Count > 0)
        {
            //card.specAbLateralPanel.GetComponent<Image>().color = new Color32(255, 255, 255, 255);
            cardAnimator.SetTrigger("CardNLateralPanel");

        }
        if (card.cardSO.cardUpkeep > 0)
        {
            //card.specAbLateralPanel.GetComponent<Image>().color = new Color32(255, 255, 255, 255);
            cardAnimator.SetTrigger("CardNLateralPanel");
        }
    }


    public void OnPointerExit(PointerEventData eventData)
    {
        // SET CARD BEHAVIOUR WHEN ISN`T IN HAND
        if (inDeck) return;
        if (inUsedPile) return;
        // FUNCTION
        //Debug.Log("Exit OVER CARD");
        cardAnimator.SetBool("CardSPointerOver", false);
        cardAnimator.SetBool("CardNPointerOver", false);
        cardAnimator.SetBool("StartZoom", false);
        if (inStartDeck)
        {
            canvas.sortingOrder = 5;
            card.specAbLateralPanel.GetComponent<Canvas>().sortingOrder = 4;
        }
        else
        {
            SetToHand();
        }
        card.specAbLateralPanel.GetComponent<Image>().color = new Color32(255, 255, 255, 0);
    }

    private void TurnOffInteractivity()
    {
        isDragging = true;
    }

    private void TurnOnInteractivity()
    {
        isDragging = false;
    }

    // ACTIVATE AND DEACTIVATE 

    public void Deactivate()
    {
        // SET CARD BEHAVIOUR WHEN ISN`T IN HAND
        if (inDeck) return;
        if (inUsedPile) return;
        // DEACTIVATE
        SetInactive();
    }

    public void Activate()
    {
        // SET CARD BEHAVIOUR WHEN ISN`T IN HAND
        if (inDeck) return;
        if (inUsedPile) return;
        // ACTIVATE
        if (card.cardSO.GetOwner().isHuman)
        {
            SetActive();
        }
    }

    // PUT TO GRAVEYARD

    public void PutToGraveYard(Player player)
    {
        transform.SetParent(player.graveyard, false);
        transform.position = Vector3.zero;
    }

    public void UnSubscribeEvents()
    {
        // SUBSCRIBE EVENTS
        GameEvents.current.onCardDrag -= TurnOffInteractivity;
        GameEvents.current.onCardEndDrag -= TurnOnInteractivity;
        GameEvents.current.onStartInteraction -= TurnOnInteractivity;
        GameEvents.current.onBlockInteraction -= TurnOffInteractivity;
        GameEvents.current.onStartInteraction -= StartInteraction;
        GameEvents.current.onBlockInteraction -= BlockInteraction;
    }
    
}
