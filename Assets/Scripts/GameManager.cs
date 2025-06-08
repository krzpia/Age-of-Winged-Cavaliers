using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    //[HideInInspector]
    public Player actPlayer;
    public bool hotSeat;
    public bool AIdebugMode;

    [Header("Players")]
    public Player playerSouth;
    public Player playerNorth;
    private Player playerToStart;
    private Player playerSecond;

    [Header("Prefabs")]
    public GameObject cardPrefab;
    public GameObject unitPrefab;
    public GameObject slotPrefab;

    [Header("UI")]
    [SerializeField]
    private Button endTurnButton;
    public Button debugAIEndTurnButton;
    public Button debugAIGoButton;
    public PopUpWindow popUpWindow;
    public TMP_Text turnNoText;
    public Transform backGround;

    [Header("ACTION CARD PLACEMENT SHADOWS")]
    public GameObject southPlayerLineShadow;
    public GameObject northPlayerLineShadow;
    //public GameObject southPlayerLeftLineShadow;
    //public GameObject southPlayerRightLineShadow;
    //public GameObject northPlayerLeftLineShadow;
    //public GameObject northPlayerRightLineShadow;
    public GameObject globalEffectShadow;

    [Header("Logic")]
    // public int slotsNo = 6;
    public int turnNo = 0;
    public bool boolEndGame;
    // MOJE GOWNIANE ASYNCHRONICZNE SINGLETONY
    //public bool attackAnimationPlaying;
    //public bool defenceAnimationPlaying;
    //public bool actionAnimationPlaying;
    //public bool destroyAnimationPlaying;
    //public bool tentAnimationPlaying;
    // NOWA OBLSLUGA ASYNCHRONICZNYCH CORUTYN...
    public int noOfAnimationsPlaying = 0;

    // IS DRAGGING
    public bool unitIsDragging;

    [Header("SPRITES")]
    public Sprite woundIcon;
    public Sprite attackIcon;
    public Sprite moveIcon;
    public Sprite placeIcon;
    public Sprite addItemIcon;
    public Sprite addEffectIcon;
    public Sprite killIcon;
    public Sprite smallShieldIcon;

    // Start is called before the first frame update
    void Start()
    {
        // 0. PREPARE BATTLEFIELD, CHECK GAME TYPE (vs AI or HOT SEAT?)
        if (playerNorth.isHuman)
        {
            Debug.Log("HOT SEAT GAME --- STARTING Game Manager ---");
            hotSeat = true;
        }
        else
        {
            Debug.Log("Vs AI GAME --- STARTING Game Manager ---");
            hotSeat = false;
            if (AIdebugMode)
            {
                debugAIEndTurnButton.gameObject.SetActive(true);
            }
            else
            {
                debugAIEndTurnButton.gameObject.SetActive(false);
            }
        }
        southPlayerLineShadow.GetComponent<BattleLinePanel>().Hide();
        northPlayerLineShadow.GetComponent<BattleLinePanel>().Hide();
        globalEffectShadow.GetComponent<BattleLinePanel>().Hide();

        // 1. CHECK PLAYERS AND PREPARE DECK
        if (!playerSouth.isHuman)
        {
            Debug.LogError("SOUTH PLAYER HAS TO BE HUMAN!");
        }
        playerSouth.GetComponent<Player>().CreateDeck();
        playerNorth.GetComponent<Player>().CreateDeck();

        // 2. SET START ATTRIBUTES
        playerSouth.GetComponent<Player>().SetStartAttributes();
        playerNorth.GetComponent<Player>().SetStartAttributes();

        // 3. SET PLAYER VISUALS
        playerSouth.GetComponent<Player>().RefreshAttributeVisuals();
        playerNorth.GetComponent<Player>().RefreshAttributeVisuals();

        // 4. PREPARE START PANELS WITH 5 RANDOM CARDS
        PrepareStartDrawPanel(playerNorth);
        PrepareStartDrawPanel(playerSouth);

        // 5. SET FIRST TO PLAY
        playerToStart = PickRandomPlayer();
        playerToStart.GetComponent<Player>().firstToPlay = true;
        playerSecond = GetOtherPlayer(playerToStart);

        // 6a. PREPARE START PANEL
        playerToStart.GetComponent<Player>().startPanel.gameObject.SetActive(true);
        playerToStart.GetComponent<Player>().startPanel.GetComponent<StartPanel>().RefreshButton();
        playerSecond.GetComponent<Player>().startPanel.gameObject.SetActive(false);
        playerSecond.GetComponent<Player>().startPanel.GetComponent<StartPanel>().RefreshButton();
        
        // 7. REFRESH VISUALS
        playerNorth.GetComponent<Player>().RefreshAttributeVisuals();

        // 8. START CORUTINE EXCHANGE CARDS IN CASE THAT AI PLAERS IS FIRST
        if (!hotSeat && playerToStart == playerNorth)
        {
            playerToStart.GetComponent<AI>().ExchangeCardsAtStart();
        }

        // 9. END AND WAIT FOR INTERACION (BUTTON IN START PANEL)
        Debug.Log("Game Manager: END START FUNCTION");
    }

    public void StartBattle(Player PlayerToStart)
    {
        // SET ACTIVE PLAYER
        SetFirstTurn(PlayerToStart);
        // START 1st TURN (END OTHER TURN TO DEACTIVATE UNITS AND OTHER LOGIC)
        GetOtherPlayer(playerToStart).SetEndTurn();
        playerToStart.SetNewTurn();
    }

    public Player PickRandomPlayer()
    {
        int randomPick = UnityEngine.Random.Range(0, 2);
        //Debug.Log("RANDOM PICK: "+ randomPick);
        if(randomPick == 0)
        {
            return playerNorth;
        }
        else
        {
            return playerSouth;
        }
    }

    public Player GetOtherPlayer(Player thisPlayer)
    {
        if (thisPlayer == playerNorth)
        {
            return playerSouth;
        }
        else
        {
            return playerNorth;
        }
    }

    // DRAW PHASE - draw 5 cards and can change form 0 to 5

    public void AISwitchDrawPhase()
    {
        // CAN BE ACTIVATED ONLY BY HUMAN PLAYER BY CLICKING BUTTON.
        // ONLY 2 OPTIONS - AI not drawed or AI just drawed and ended corutine.
        if (playerSouth.GetComponent<Player>().startPanel.gameObject.activeInHierarchy)
        {
            // JEZELI PLAYER AI WYMIENIL KARTY
            if (playerNorth.GetComponent<Player>().startPanel.cardsExchanged)
            {
                playerSouth.GetComponent<Player>().startPanel.gameObject.SetActive(false);
                Debug.Log("START BATTLE - TO PLAY PLAYER NORTH");
                StartBattle(playerNorth);
            }
            else
            {
                playerSouth.GetComponent<Player>().startPanel.gameObject.SetActive(false);
                playerNorth.GetComponent<Player>().startPanel.gameObject.SetActive(true);
                Debug.Log("START CORUTINE AIEXCHANGE CARDS");
                playerNorth.GetComponent<AI>().ExchangeCardsAtStart();
            }
        }
        else
        {
            if (playerSouth.GetComponent<Player>().startPanel.cardsExchanged)
            {
                playerNorth.GetComponent<Player>().startPanel.gameObject.SetActive(false);
                Debug.Log("START BATTLE - TO PLAY PLAYER SOUTH");
                StartBattle(playerSouth);
            }
            else
            {
                playerNorth.GetComponent<Player>().startPanel.gameObject.SetActive(false);
                playerSouth.GetComponent<Player>().startPanel.gameObject.SetActive(true);
            }
        }
    }

    public void SwitchDrawPhase()
    {
        if (playerSouth.GetComponent<Player>().startPanel.gameObject.activeInHierarchy)
        {
            if (playerNorth.GetComponent<Player>().startPanel.cardsExchanged)
            {
                playerSouth.GetComponent<Player>().startPanel.gameObject.SetActive(false);
                Debug.Log("START BATTLE - TO PLAY PLAYER NORTH");
                StartBattle(playerNorth);
            }
            else
            {
                playerSouth.GetComponent<Player>().startPanel.gameObject.SetActive(false);
                playerNorth.GetComponent<Player>().startPanel.gameObject.SetActive(true);
            }
            
        }
        else
        {
            if (playerSouth.GetComponent<Player>().startPanel.cardsExchanged)
            {
                playerNorth.GetComponent<Player>().startPanel.gameObject.SetActive(false);
                Debug.Log("START BATTLE - TO PLAY PLAYER SOUTH");
                StartBattle(playerSouth);
            }
            else
            {
                playerNorth.GetComponent<Player>().startPanel.gameObject.SetActive(false);
                playerSouth.GetComponent<Player>().startPanel.gameObject.SetActive(true);
            }
            
        }
    }

    public void PrepareStartDrawPanel(Player player)
    {
        // HUMAN
        if (player.isHuman)
        {
            player.startPanel.GetComponent<StartPanel>().owner = player;
            player.startPanel.GetComponent<StartPanel>().playerImage.sprite = player.playerSO.playerSprite;
            player.startPanel.GetComponent<StartPanel>().playerNameText.text = player.playerSO.playerName;
            player.startPanel.GetComponent<StartPanel>().playerCoatOfArms.sprite = player.playerSO.playerCoatImage;
            // RANDOM PICK 5 CARDS AND PUT IT TO START PANEL
            for (int i = 0; i < 5; i++)
            {
                Transform cardGrid = player.startPanel.cardContainer.transform;
                GameObject cardToDraw = player.GetRandomCardFromDeck();
                cardToDraw.transform.SetParent(cardGrid.GetChild(i), false);
                cardToDraw.GetComponent<CardBehaviour>().SetToStartDeck();
                cardToDraw.GetComponent<Card>().SetAwers();
            }
        }
        // AI
        else
        {
            player.startPanel.GetComponent<StartPanel>().owner = player;
            player.startPanel.GetComponent<StartPanel>().playerImage.sprite = player.playerSO.playerSprite;
            player.startPanel.GetComponent<StartPanel>().playerNameText.text = player.playerSO.playerName;
            player.startPanel.GetComponent<StartPanel>().playerCoatOfArms.sprite = player.playerSO.playerCoatImage;
            // ORGANIZE START DECK  1.DRAW 5 CARDS, 2. DECIDE WHICH TO CHANGE.(cost above?, too much items - change?)
            Debug.Log("AI PLAYER DECK CREATE...");
            // FOR NOW ONLY DRAW 5 cards
            for (int i = 0; i < 5; i++)
            {
                Transform cardGrid = player.startPanel.cardContainer.transform;
                GameObject cardToDraw = player.GetRandomCardFromDeck();
                cardToDraw.transform.SetParent(cardGrid.GetChild(i), false);
                cardToDraw.GetComponent<CardBehaviour>().SetToStartDeck();
                cardToDraw.GetComponent<Card>().SetRewers();
            }
            Debug.Log("DONE.");
            //  
        }
        
    }

    // SET ACTIVE PLAYER

    public void SetActivePlayer(Player player)
    {
        actPlayer = player;
    }

    // ONCLICK > 1. BUTTON END TURN
    public void onClickEndTurn()
    {
        endTurnButton.interactable = false;
        if (actPlayer == null)
        {
            Debug.LogWarning("NO ACTIVE PLAYER, ERROR");    
        }
        if (actPlayer == playerSouth)
        {
            Debug.Log("End South Player Turn, Switch player to north");
            // 1. ENDING TUNR OF PLAYER
            actPlayer.SetEndTurn();
            // TO DO IF END GAME, DONT HAVE TO SWITCH PLAYER;
            if (!boolEndGame)
            {
                // 2. SWITCH ACT PLAYER
                SetActivePlayer(playerNorth);
                // 3. STARTING TURN OF PLAYER
                if (actPlayer == playerToStart)
                {
                    turnNo++;
                    turnNoText.text = turnNo.ToString();
                }
                actPlayer.NewTurnDrawPhase(); 
            }

        }
        else
        {
            Debug.Log("End North Player Turn, Switch player to south");
            // 1. ENDING TUNR OF PLAYER
            actPlayer.SetEndTurn();
            // TO DO IF END GAME, DONT HAVE TO SWITCH PLAYER;
            if (!boolEndGame)
            {
                // 2. SWITCH ACT PLAYER
                SetActivePlayer(playerSouth);
                // 3. STARTING TURN OF PLAYER
                if (actPlayer == playerToStart)
                {
                    turnNo++;
                    turnNoText.text = turnNo.ToString();
                }
                actPlayer.NewTurnDrawPhase();
            }
        }
    }

    public void ActivateEndTurnButton()
    {
        endTurnButton.interactable = true;
    }

    public void DeactivateEndTurnButton()
    {
        endTurnButton.interactable = false;
    }

    public void onClickDebugEndTurn()
    {
        onClickEndTurn();
        debugAIEndTurnButton.interactable = false;
    }

    public void ActivateDebugAIGoButton()
    {
        debugAIGoButton.interactable = true;
    }

    public void onClickGoAI()
    {
        debugAIGoButton.interactable = false;
        GameManager.instance.playerNorth.GetComponent<AI>().NextAILoop();
    }

    public void SetFirstTurn(Player player)
    {
        turnNo = 1;
        turnNoText.text = turnNo.ToString();
        // SETTING ACTPLAYER
        actPlayer = player;
    }

    // ACTIONS

    public void ShowPlayActionPosibilities(Card card)
    {
        if (instance.actPlayer.playerActGold < card.cardSO.cardCost)
        {
            return;
        }
        ActionTypeSO actionType = card.cardSO.cardTypeSO as ActionTypeSO;
        // AKCJA DO ZAGRANIA NA LINII WROGA
        if (actionType.actionPlayMethod == ActionPlayMethod.OnLine)
        {
            if (actionType.enemyLineAction)
            {
                ShowEnemyLineAction(card);
            }
            if (actionType.ownerLineAction)
            {
                ShowOwnerLineAction(card);
            }
            
        }
        else if (actionType.actionPlayMethod == ActionPlayMethod.OnUnit)
        {
            if (actionType.enemyUnits)
            {
                GameEvents.current.ShowPosibleOffensiveAction(card.cardSO);
            }
            if (actionType.myUnits)
            {
                // TO DO
                //GameEvents.current.ShowPosibleMyUnitsAction(card.cardSO);
            }
        }
        else
        {
            // TO DO MORE ACTION PLAY OPTIONS!
        }
    }

    public void ShowOwnerLineAction(Card card)
    {
        Player owner = card.cardSO.GetOwner();
        if (owner == playerSouth)
        {
            southPlayerLineShadow.GetComponent<BattleLinePanel>().Show(card);
        }
        else if (owner == playerNorth)
        {
            northPlayerLineShadow.GetComponent<BattleLinePanel>().Show(card);
        }
        else
        {
            Debug.LogWarning("SHOW PLAY ACTION POSIBILITIES - CARD OWNER ERROR");
        }
    }

    public void ShowEnemyLineAction(Card card)
    {
        Player owner = card.cardSO.GetOwner();
        if (owner == playerSouth)
        {
            northPlayerLineShadow.GetComponent<BattleLinePanel>().Show(card);
        }
        else if (owner == playerNorth)
        {
            southPlayerLineShadow.GetComponent<BattleLinePanel>().Show(card);
        }
        else
        {
            Debug.LogWarning("SHOW PLAY ACTION POSIBILITIES - CARD OWNER ERROR");
        }
    }


    public void EndShowActionPosibilities()
    {
        southPlayerLineShadow.GetComponent<BattleLinePanel>().Hide();
        northPlayerLineShadow.GetComponent<BattleLinePanel>().Hide();
        globalEffectShadow.SetActive(false);
    }

    public void GenerateEndGameWindow(Player winner)
    {
        popUpWindow.GenerateWindow(winner.playerSO.playerName + " wins!", () =>
        {
            EndGame();
        });
    }

    public void EndGame()
    {
        Debug.Log("END GAME");
        Application.Quit();
        EditorApplication.ExitPlaymode();
    }

    public bool IsAnimationPlayig()
    {
        if (noOfAnimationsPlaying > 0)
        {
            return true;
        }
        else
        {
            return false;
        }
        //if (actionAnimationPlaying) return true;
        //if (attackAnimationPlaying) return true;
        //if (defenceAnimationPlaying) return true;
        //if (destroyAnimationPlaying) return true;
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
