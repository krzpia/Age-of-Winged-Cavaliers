using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum AttackType
{
    MelleDefence = 0,
    MelleAttack = 1,
    SiegeDefence = 2,
    SiegeAttack = 3,
    FirstStrikeAttack = 4,
    FirstStrikeDefence = 5
}


public class UnitAI
{
    public Unit unit;
    public int unitAttackf;

    public void AddUnitToAtack(Unit unitToAdd)
    {
        unit = unitToAdd;
    }

    public void ChangeUnitf(int unitItemfToChange)
    {
        unitAttackf += unitItemfToChange;
    }
}

public class Unit : MonoBehaviour
{
    public CardSO cardSO;
    [Header("LOGIC")]
    [SerializeField]
    private bool active;
    public bool paid;
    public bool unitKilled;
    public bool isFighting;

    [Header ("ATRIBUTES")]
    public string unitName;
    //public int unitMovement;
    //public int unitMaxMovement;
    public bool siege;

    [Header("VISUALS")]
    public TMP_Text nameText;
    // public TMP_Text typeText;
    public TMP_Text unitAttackText;
    public TMP_Text unitDefenceText;
    public TMP_Text unitUpkeepText;
    public TMP_Text unitArtAuthorText;
    public Image baseImage;
    public Image upkeepCoinImage;
    public Image unitTypeIcon;
    public Image backGroundImage;
    public Image unitImage;
    public Image flagImage;
    public Image activityDot;
    public Image highlightImage;
    public Image activityImage;
    public Image unitBackImage;
    public Animator glowAnimator;
    public Image glowImage;
    public Transform specAbIconPanel;
    public Transform specAbLateralPanel;
    public GameObject specAbImagePrefab;
    public GameObject specAbLateralPanelSlotPrefab;
    public GameObject specAbPopUpPrefab;
    public Animator woundAnimator;
    public GameObject cannonBallPrefab;
    public Button upkeepButton;

    [Header("AI")]
    // SLOT MOVE AIF
    public List<SlotAI> AImoveSlots = new List<SlotAI>();
    public BattleSlot besSlotToMove = null;
    public int bestSlotToMoveAIF = -12;
    // OPPONENT UNIT AIF
    public List<UnitAI> AIattackUnits = new List<UnitAI>();
    public Unit target = null;
    public int targetAIF = -11;
    // OPPONENT HQ TENT AIF
    public bool targetHqTent = false;
    public int hqTentAIF = -10;
    //public string attackDecision = "";
    public UnitDecsion aiDecision;


    private void Awake()
    {
        GameEvents.current.onRefreshUnitVisuals += RefreshUnitAtributes;
    }

    public void CreateUnit(CardSO card)
    {
        // CARDSO
        cardSO = card;
        cardSO.SetActAtt(card.baseAtt);
        cardSO.SetActDef(card.baseDef);
        // ATTRIBUTES
        unitName = card.cardName;
        nameText.text = unitName;
        unitArtAuthorText.text = card.artAuthor;
        UnitTypeSO unitType = card.cardTypeSO as UnitTypeSO;
        //typeText.text = unitType.typeName;
        //unitMaxMovement = unitType.unitTypeMovement;
        siege = unitType.unitSiege;
        //unitMovement = 0;
        // GRAPHICS AT START
        baseImage.color = card.GetOwner().playerSO.playerColor;
        backGroundImage.color = card.GetOwner().playerSO.playerColor;
        unitImage.sprite = card.cardImage;
        flagImage.sprite = card.cardFlag;
        unitTypeIcon.sprite = unitType.unitTypeIcon;
        unitBackImage.sprite = card.GetOwner().playerSO.playerDeckImage;
        // VISUALS
        GetComponent<UnitBehaviour>().SetStantardSortingLayer();
        RefreshUnitAtributes();
        // TO DO IF HAS CHARGE SET TO ACTIVE OTHERWISE DEACTIVATE
        if (cardSO.CheckIfHasSpecAb("Charge"))
        {
            Activate();
        }
        else
        {
            Deactivate();
        }
        // UPKEEP BUTTON TO FALSE
        upkeepButton.gameObject.SetActive(false);
        paid = true;
        // REFRESH ALL OTHER UNITS (UPKEEP, SPEC ABS)
        GameEvents.current.RefreshUnitVisuals();
    }

    public void RefreshUnitAtributes()
    {
        // STANDARD ATTRIBUTES
        unitAttackText.text = cardSO.GetAtt().ToString();
        unitDefenceText.text = cardSO.GetDef().ToString();
        // UPKEEP
        if (cardSO.cardUpkeep == 0)
        {
            upkeepCoinImage.gameObject.SetActive(false);
            upkeepButton.gameObject.SetActive(false);
        }
        else
        {
            upkeepCoinImage.gameObject.SetActive(true);
            unitUpkeepText.text = cardSO.cardUpkeep.ToString();
            if (!paid)
            {
                if (cardSO.GetOwner() == GameManager.instance.actPlayer)
                {
                    upkeepButton.gameObject.SetActive(true);
                    ActivateUpkeepButton();
                }
                else
                {
                    upkeepButton.gameObject.SetActive(false);
                }
            }
            else
            {
                upkeepButton.gameObject.SetActive(false);
            }
        }
        // SET SPECIAL ABS
        foreach (Transform existingSpecAbIco in specAbIconPanel)
        {
            Destroy(existingSpecAbIco.gameObject);
        }
        foreach (Transform existingSpecAbSlot in specAbLateralPanel)
        {
            Destroy(existingSpecAbSlot.gameObject);
        }
        foreach (SpecialAbilitySO specAb in cardSO.specialAbilityList)
        {
            GameObject specAbIco = Instantiate(specAbImagePrefab, specAbIconPanel);
            specAbIco.GetComponent<Image>().sprite = specAb.specAbIcon;
            GameObject specAbSlot = Instantiate(specAbLateralPanelSlotPrefab, specAbLateralPanel);
            specAbSlot.GetComponent<UnitSpecAbSlot>().SetSpecAb(specAb);
        }
        // TENT
        cardSO.GetOwner().RefreshAttributeVisuals();
        GameManager.instance.GetOtherPlayer(cardSO.GetOwner()).RefreshAttributeVisuals();

    }

    public void PutItem(CardSO itemCardSO)
    {
        // BASE VALORS
        int baseAtt = cardSO.GetAtt();
        int baseDef = cardSO.GetDef();
        // CREATING SPEC AB FROM ITEM
        SpecialAbilitySO specAb = ScriptableObject.CreateInstance<SpecialAbilitySO>();
        specAb.SetSpecAb(itemCardSO);
        // ADDING BONUS ANIMATIONS
        if (specAb.attBonus != 0)
        {
            StartCoroutine(ItemAttackMod(baseAtt, specAb.attBonus));
        }
        if (specAb.defBonus != 0)
        {
            StartCoroutine(ItemDefenceMod(baseDef, specAb.defBonus));
        }
        // ADDING SPECABS
        cardSO.specialAbilityList.Add(specAb);
    }

    // LOGIC AND LISTS

    public Unit GetEastUnit()
    {
        BattleSlot mySlot = GetSlot();
        if (mySlot.x < 6)
        {
            BattleSlot slot = mySlot.GetSlotByXY(mySlot.x + 1, mySlot.y);
            return slot.GetUnit();
        }
        else
        {
            return null;
        }
    }

    public Unit GetWestUnit()
    {
        BattleSlot mySlot = GetSlot();
        if (mySlot.x > 0)
        {
            BattleSlot slot = mySlot.GetSlotByXY(mySlot.x - 1 , mySlot.y);
            return slot.GetUnit();
        }
        else
        {
            return null;
        }
    }

    public List<Unit> GetListOfUnitsInLeftEnemyLine()
    {
        List<Unit> list = new List<Unit>();
        List<Unit> allEnemyUnits = cardSO.GetOwner().GetListOfEnemyUnits();
        foreach (Unit unit in allEnemyUnits)
        {
            if (unit.GetSlot().x < 4)
            {
                list.Add(unit);
            }
        }
        return list;
    }

    public List<Unit> GetListOfUnitsInRightEnemyLine()
    {
        List<Unit> list = new List<Unit>();
        List<Unit> allEnemyUnits = cardSO.GetOwner().GetListOfEnemyUnits();
        foreach (Unit unit in allEnemyUnits)
        {
            if (unit.GetSlot().x >= 4)
            {
                list.Add(unit);
            }
        }
        return list;
    }

    // LISTS

    public List<Unit> GetListOfUnitsCanAttackMe()
    {
        List<Unit> list = new List<Unit>();
        List<Unit> allEnemyUnits = cardSO.GetOwner().GetListOfEnemyUnits();
        foreach (Unit unit in allEnemyUnits)
        {
            if (unit.CheckIfCanAttack(this))
            {
                list.Add(unit);
            }
        }
        return list;
    }

    public List<Unit> GetListOfMyPosibleTargets()
    {
        List<Unit> list = new List<Unit>();
        List<Unit> allEnemyUnits = cardSO.GetOwner().GetListOfEnemyUnits();
        foreach (Unit unit in allEnemyUnits)
        {
            if (CheckIfCanAttack(unit))
            {
                list.Add(unit);
            }
        }
        return list;
    }

    public List<Unit> GetListOfEnemySiegeUnits()
    {
        List<Unit> list = new List<Unit>();
        List<Unit> allEnemyUnits = cardSO.GetOwner().GetListOfEnemyUnits();
        foreach (Unit enemyUnit in allEnemyUnits)
        {
            if (enemyUnit.siege)
            {
                list.Add(enemyUnit);
            }
        }
        return list;
    }

    // UNIT BATTLE RESULTS LOGIC

    public bool CheckIfAttackerCanKillMe(Unit attacker)
    {
        bool unitKilled = false;
        int myDamage = GetMyDamageWhenDefending(attacker);
        // FIRST STRIKE not apply (I`m defenfing, so dosent matter if I have first strike or not)
        // TO DO OTHER BONUSES?
        {
            if (myDamage >= cardSO.GetDef())
            {
                unitKilled = true;
            }
        }
        return unitKilled;
    }

    public bool CheckIfAttackerWillBeKilled(Unit attacker)
    {
        if (attacker.cardSO.GetUnitType() == UnitType.Artillery)
        {
            return false;
        }
        bool attackerKilled = false;
        int myDamage = GetMyDamageWhenDefending(attacker);
        int attackerDamage = attacker.GetMyDamageWhenAttacking(this); 
        if (attacker.cardSO.GetDef() <= attackerDamage)
        {
            attackerKilled = true;
        }
        // FIRST STRIKE
        if (attacker.cardSO.CheckIfHasActiveSpecAb("First Strike"))
        {
            if (myDamage >= cardSO.GetDef())
            {
                return false;
            }
        }
        return attackerKilled;
    }
    
    public bool CheckIfDefenderCanKillMe(Unit defender)
    {
        if (cardSO.GetUnitType() == UnitType.Artillery)
        {
            return false;
        }
        bool unitKilled = false;
        int myDamage = GetMyDamageWhenAttacking(defender);
        int defenderDamage = defender.GetMyDamageWhenDefending(this);
        // FIST STRIKE - JEZELI MAM FIRST STRIKE, SPRAWDZAM NAJPIERW CzY JA GO ZABIJE, JESLI TAK< TO JA NIE ZGINE 
        if (cardSO.CheckIfHasActiveSpecAb("First Strike"))
        {
            if (defender.cardSO.GetDef() <= defenderDamage)
            {
                // RETURN FALSE - zabije jednostke, zanim ta zdazy zaatakowac, wiec mnie nie zabije!
                return false;
            }
        }
        if (myDamage >= cardSO.GetDef())
        {
            unitKilled = true;
        }
        // TO DO OTHER SPEC ABS
        return unitKilled;
    }
    
    public bool CheckIfDefenderWillBeKilled(Unit defender)
    {
        bool defenderKilled = false;
        int defenderDamage = defender.GetMyDamageWhenDefending(this);
        if (defenderDamage >= defender.cardSO.GetDef())
        {
            defenderKilled = true;
        }
        return defenderKilled;
    }

    // FUNKCJE OBLICZAJACE UNIT DAMAGE (TU ZAWRZEC WSZYSTKIE SPEC ABS!)

    public int GetMyDamageWhenEnemyCounterAttacksAfterMyFirstStrikeAttack(Unit defender)
    {
        int defenderAttackBonus = 0;
        //int attackerAttackBonus = 0;
        //int defenderDefenceBonus = 0;
        // int attackerDefenceBonus = 0;
        // SPEC ABS 
        // 1. PIKE (ADD DEFENDER ATTACK IF MY UNIT IS CAVALRY
        if (defender.cardSO.CheckIfHasActiveSpecAb("Pike"))
        {
            UnitTypeSO unitType = cardSO.cardTypeSO as UnitTypeSO;
            if (unitType.unitType == UnitType.Cavalry)
            {
                // Debug.Log("APPLY PIKE BONUS " + unitDefending.cardSO.GetSpecAbByName("Pike").cavalryDefPenalty);
                // defender.SetSpecAbPopUp(unitDefending.cardSO.GetSpecAbByName("Pike"));
                defenderAttackBonus += defender.cardSO.GetSpecAbByName("Pike").cavalryDefPenalty;
                Debug.Log("PIKE SPEC ABILITIY DETECTED: DEFENDER ATT BONUS: " + defenderAttackBonus);
            }
        }
        // 2. OTHER SPEC ABS (TO DO)
        // 2. OTHER SPEC ABS (ADD ATTACKER OR DEFENDER STATS)
        if (defender.cardSO.CheckIfHasActiveSpecAb("Fire Defense"))
        {
            defenderAttackBonus += defender.cardSO.GetSpecAbByName("Fire Defense").attBonusDefending;
            Debug.Log("FIRE DEFENSE ABILITY DETECTED: DEFENDER ATT BONUS: " + defenderAttackBonus);
        }
        return defender.cardSO.GetAtt() + defenderAttackBonus;
    }

    public int GetMyDamageWhenAttacking(Unit defender)
    {
        // Debug.Log("== GetMyDamageWhenAttackUnit == (Unit " + cardSO.cardName + " attacks: " + defender.cardSO.cardName);
        // Debug.Log("== GetMyDamageWhenAttackUnit == ATTACKER ATT: " + cardSO.GetAtt());
        // Debug.Log("== GetMyDamageWhenAttackUnit == DEFENDER ATT: " + defender.cardSO.GetAtt());
        // Debug.Log("== GetMyDamageWhenAttackUnit == ATTACKER DEF: " + cardSO.GetDef());
        // Debug.Log("== GetMyDamageWhenAttackUnit == DEFENDER DEF: " + defender.cardSO.GetDef());
        int defenderAttackBonus = 0;
        int attackerAttackBonus = 0;
        int defenderDefenceBonus = 0;
        // int attackerDefenceBonus = 0;
        // SPEC ABS 
        // 1. PIKE (ADD DEFENDER ATTACK IF MY UNIT IS CAVALRY
        if (defender.cardSO.CheckIfHasActiveSpecAb("Pike"))
        {
            UnitTypeSO unitType = cardSO.cardTypeSO as UnitTypeSO;
            if (unitType.unitType == UnitType.Cavalry)
            {
                // Debug.Log("APPLY PIKE BONUS " + unitDefending.cardSO.GetSpecAbByName("Pike").cavalryDefPenalty);
                // defender.SetSpecAbPopUp(unitDefending.cardSO.GetSpecAbByName("Pike"));
                defenderAttackBonus += defender.cardSO.GetSpecAbByName("Pike").cavalryDefPenalty;
                Debug.Log("== GetMyDamageWhenAttackUnit == PIKE SPEC ABILITIY DETECTED: DEFENDER ATT BONUS: " + defenderAttackBonus);
            }
        }
        // 2. OTHER SPEC ABS (ADD ATTACKER OR DEFENDER STATS)
        if (defender.cardSO.CheckIfHasActiveSpecAb("Fire Defense"))
        {
            defenderAttackBonus += defender.cardSO.GetSpecAbByName("Fire Defense").attBonusDefending;
            Debug.Log("== GetMyDamageWhenAttackUnit == FIRE DEFENSE ABILITY DETECTED: DEFENDER ATT BONUS: " + defenderAttackBonus);
        }
        // FIRST STRIKE
        if (cardSO.CheckIfHasActiveSpecAb("First Strike"))
        {   
            if (cardSO.GetAtt() + attackerAttackBonus > defender.cardSO.GetDef() + defenderDefenceBonus)
            {
                Debug.Log("== GetMyDamageWhenAttackUnit == FIRST STRIKE:Defender DIES before deals damage to me");
                // Defender DIES before deals damage to me
                return 0;
            }
            else
            {
                Debug.Log("== GetMyDamageWhenAttackUnit MY UNIT HAS FIRST STRIKE AND RECIEVES: " + defender.cardSO.cardName + "");
                return defender.cardSO.GetAtt() + defenderAttackBonus;
                
            }
        }
        else
        {
            Debug.Log("== GetMyDamageWhenAttackUnit == (Unit " + cardSO.cardName + " WILL RECIEVE TOTAL DAMAGE: " + (defender.cardSO.GetAtt() + defenderAttackBonus));
            return defender.cardSO.GetAtt() + defenderAttackBonus;
        }
    }

    public int GetMyDamageWhenDefending(Unit attacker)
    {
        // int defenderAttackBonus = 0;
        int attackerAttackBonus = 0;
        // int defenderDefenceBonus = 0;
        // int attackerDefenceBonus = 0;
        // SPEC ABS
        // TO DO
        // RETURN RESULT
        return attacker.cardSO.GetAtt() + attackerAttackBonus;
    }

    public int GetInflictedDamageToDefender(Unit defender)
    {
        //Debug.Log("f() GetDamageOfAttackedUnit (Unit " + defender.cardSO.cardName + "");
        //Debug.Log("ATTACKER ATT: " + cardSO.actAtt);
        //Debug.Log("DEFENDER ATT: " + defender.cardSO.actAtt);
        //Debug.Log("ATTACKER DEF: " + cardSO.actDef);
        //Debug.Log("DEFENDER DEF: " + defender.cardSO.actDef);
        // int defenderAttackBonus = 0;
        // int defenderDefenceBonus = 0;
        int attackerAttackBonus = 0;
        // int attackerDefenceBonus = 0;
        // 1. TO DO SPEC ABS
        return cardSO.GetAtt() + attackerAttackBonus ;
    }

    public int GetTentDamage()
    {
        int specAbTentAttBonus = 0;
        // TO DO SPEC AB IF DEVELOPED
        int damage = cardSO.GetAtt() + specAbTentAttBonus;
        return damage;
    }

    // AI

    public void AIAddUnitsCanAttack()
    {
        //if (!CheckActive())
        //{
        //    Debug.LogWarning("AIAddUnitsCanAttack return Empty: Unit active == false");
        //    return;
        //}
        List<Unit> enemyUnitCanBeAttacked = GetListOfMyPosibleTargets();
        foreach (Unit enemy in enemyUnitCanBeAttacked)
        {
            AddUnitToAttackAI(enemy);
        }
    }

    public void AIAddSlotsCanMove()
    {
        // FOR NOW FLANK IS CHANGE SIDE.
        if (!CheckActive())
        {
            Debug.LogWarning("AIAddSlotsCanMove returns Empty: Unit active == false");
            return;
        }
        List<BattleSlot> allMySlots = cardSO.GetOwner().GetListOfDeploySlots();
        foreach (BattleSlot slot in allMySlots)
        {
            if (GetSlot().x < 4)
            {
                if (!slot.CheckIfOccupied())
                {
                    if (slot.x >= 4)
                    {
                        AddSlotToMoveAI(slot);
                    }
                }
            }
            if (GetSlot().x >= 4)
            {
                if (!slot.CheckIfOccupied())
                {
                    if (slot.x < 4)
                    {
                        AddSlotToMoveAI(slot);
                    }
                }
            }

            
        }
    }

    private void AddSlotToMoveAI(BattleSlot slot)
    {
        bool slotFound = false;
        if (AImoveSlots.Count > 0)
        {
            foreach (SlotAI slotAI in AImoveSlots)
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
            newSlotAI.AddMoveSlot(slot);
            AImoveSlots.Add(newSlotAI);
        }
        else
        {
            Debug.LogWarning("SLOT to ADD AS SLOTAI OF UNIT is already set");
        }
    }

    private void AddUnitToAttackAI(Unit enemy)
    {
        bool enemyFound = false;
        if (AIattackUnits.Count > 0)
        {
            foreach (UnitAI unitAI in AIattackUnits)
            {
                if (unitAI.unit == enemy)
                {
                    enemyFound = true;
                }
            }
        }
        if (!enemyFound)
        {
            UnitAI newUnitAI = new UnitAI();
            newUnitAI.AddUnitToAtack(enemy);
            AIattackUnits.Add(newUnitAI);
        }
        else
        {
            Debug.LogWarning("UNIT(ENEMY) TO ADD AS UNITAI OF UNIT is already set");
        }
    }

    public bool AICheckIfCanAttackEnemyTent()
    {
        bool canAttack = false;
        bool maneuver = false;
        bool leftProtected = false;
        bool rightProtected = false;
        // 1. CHECK IF ACTIVE OR NOT (POSIBLE ACTIVE WITH UPKEEP
        if (active)
        {
            canAttack = true;
        }
        else
        {
            if (cardSO.cardUpkeep > 0 && cardSO.GetOwner().playerActGold >= cardSO.cardUpkeep)
            {
                if (!paid)
                {
                    canAttack = true;
                }
            }
        }
        // 2. CHECK IF MANEUVER && SIEGE
        if (cardSO.CheckIfHasActiveSpecAb("Maneuver"))
        {
            maneuver = true;
        }
        // 3. CHECK IF TENT IS PROTECTED
        if (cardSO.GetOwner().GetListOfEnemyUnitsInLeftSide().Count > 0)
        {
            leftProtected = true;
        }
        if (cardSO.GetOwner().GetListOfEnemyUnitsInRightSide().Count > 0)
        {
            rightProtected = true;
        }
        // 4. LOGIC
        if (canAttack)
        {
            if (siege)
            {
                return true;
            }
            if (maneuver)
            {
                if (leftProtected && rightProtected)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            if (GetSlot().x < 4)
            {
                if (!leftProtected)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                if (!rightProtected)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        else
        {
            return false;
        }
    }

    // MOVE 

    public void MoveUnitTo(BattleSlot slot)
    {
        // MOVE UNIT
        transform.SetParent(slot.unitSlot.transform, false);
        // IF SPEC AB PERMITS ATTACK AFTER FLANKING?
        Deactivate();
        GameEvents.current.EndShowPosibleBattleSlot();
        GameEvents.current.RefreshUnitVisuals();
        GetComponent<UnitBehaviour>().ClearLineRenderer();
        GameManager.instance.playerNorth.RefreshAttributeVisuals();
        GameManager.instance.playerSouth.RefreshAttributeVisuals();   
    }

    public void UnitSkip()
    {
        Deactivate();
        GameEvents.current.EndShowPosibleBattleSlot();
        GameEvents.current.RefreshUnitVisuals();
        GetComponent<UnitBehaviour>().ClearLineRenderer();
        GameManager.instance.playerNorth.RefreshAttributeVisuals();
        GameManager.instance.playerSouth.RefreshAttributeVisuals();
    }

    public void PlayAction(CardSO card)
    {
        if (card.cardTypeSO.cardType != CardType.Action) return;
        Player unitOwner = cardSO.GetOwner();
        Player actionOwner = card.GetOwner();
        ActionTypeSO actionCardSO = card.cardTypeSO as ActionTypeSO;
        // ACTION LOGIC
        Debug.Log("PLAY ACTION LOGIC");
        // IF ACTION IS TO KILL UNIT
        if (actionCardSO.isDestroyUnit)
        {
            ActionAttackUnit(cardSO.GetDef());
        }
        // IF ACTION IS TO DAMAGE DEFENCE
        if (actionCardSO.isDamageUnit)
        {
            if (card.baseDef < 0)
            {
                int damage = Mathf.Abs(card.baseDef);
                ActionAttackUnit(damage);
            }
        }
        // IF ACTION IS TO ATTACK PENALTY
        if (actionCardSO.isWeakUnit)
        {
            if (card.baseAtt < 0)
            {
                Debug.LogWarning("TO DO : ACTION WITH PERMANENT ATTACK PENALTY? OR SPEC AB LIKE CONDITION?");
            }
        }
        // TO DO OTHER ACTION TYPES    
    }

    // ATTACK

    public bool CheckIfCanAttackOpponentsTent()
    {
        bool canAttack = false;
        // 1. CHECK TYPE OF UNIT
        UnitType attackerType = cardSO.GetUnitType();
        if (attackerType == UnitType.Artillery)
        {
            Debug.Log("ARTILLERY ATTACKS");
            canAttack = true;
        }
        else
        {
            // MANEUVER ATTACK
            if (cardSO.CheckIfHasActiveSpecAb("Maneuver"))
            {
                Debug.Log("UNIT WITH MANEUVER ATTACKS");
                // TO DO
                if (GetListOfUnitsInLeftEnemyLine().Count == 0 || GetListOfUnitsInRightEnemyLine().Count == 0)
                {
                    return true;
                }
            }
            // STANDARD ATTACK CHECK POSITION
            if (GetSlot().x < 4)
            {
                if (GetListOfUnitsInLeftEnemyLine().Count == 0)
                {
                    canAttack = true;
                }
            }
            if (GetSlot().x >= 4)
            {
                if (GetListOfUnitsInRightEnemyLine().Count == 0)
                {
                    canAttack = true;
                }
            }
        }
        // TO DO ADD PROTECTION SPEC ABS?
        return canAttack;
    }

    public bool CheckIfCanAttack(Unit unit)
    {
        bool canAttack = false;
        // 1. CHECK TYPE OF UNIT
        UnitType attackerType = cardSO.GetUnitType();
        // 2. CHECK ATTACK BY UNIT TYPE (ARTILLERY OR OTHER
        if (attackerType == UnitType.Artillery)
        {
            // JEZELI UPKEEP ZAPLACONY
            if (unit.CheckActive())
            {
                //Debug.Log("ARTILLERY ATTACKS");
                return true;
            }
            else
            {
                if (unit.cardSO.GetOwner() == GameManager.instance.actPlayer)
                {
                    if (!unit.paid && unit.cardSO.GetOwner().playerActGold >= unit.cardSO.cardUpkeep)
                    {
                        //Debug.Log("DURING ACTUAL TURN THE ARTILLERY: " + unit.cardSO.cardName + " CAN ATTACK, IS NOT ACTIVATED, BUT IT CAN BE PAID");
                        return true;
                    }
                }
                else
                {
                    if ((unit.cardSO.GetOwner().playerActGold + unit.cardSO.GetOwner().playerIncome) >= unit.cardSO.cardUpkeep)
                    {
                        //Debug.Log("DURING NEXT TURN THE ARTILLERY: " + unit.cardSO.cardName + " CAN ATTACK, IF WILL BE PAID");
                        return true;
                    }
                }
            }
        }
        else
        {
            if (cardSO.CheckIfHasActiveSpecAb("Maneuver"))
            {
                //Debug.Log("UNIT WITH MANEUVER ATTACKS");
                canAttack = true;
            }
            else
            {
                //Debug.Log("CAVALRY OR INFANTRY ATTACKS");
                if (GetSlot().x < 4)
                {
                    if (unit.GetSlot().x < 4)
                    {
                        {
                            canAttack = true;
                        }
                    }
                }
                if (GetSlot().x >= 4)
                {
                    if (unit.GetSlot().x >= 4)
                    {
                        canAttack = true;
                    }
                }
            }
        }
        // 3. CHECK IF UNIT TO ATTACK IS UNDER PROTECTION
        if (unit.cardSO.CheckIfHasActiveSpecAb("Protected"))
        {
            canAttack = false;
        }
        // RESULT
        return canAttack;
    }

    public void AttackUnit(Unit unit)
    {
        BattleSlot slot = unit.GetSlot();
        Transform unitSlot = slot.unitSlot;
        if (CheckSiege())
        {
            //Debug.Log("SIEGE ATTACK");
            SiegeAttack(slot.GetUnit());
            GameEvents.current.RefreshUnitVisuals();
        }
        else
        {
            //Debug.Log("MELLE ATTACK");
            MelleAttack(slot.GetUnit());
            GameEvents.current.RefreshUnitVisuals();
        }
    }

    private void SiegeAttack(Unit unitDefending)
    {
        // BLOCK INTERACTION
        GameEvents.current.BlockZoom();
        GameEvents.current.BlockInteraction();
        // int defenderDamageBonus = 0;
        int defenderDamage = unitDefending.GetMyDamageWhenDefending(this);
        StartCoroutine(unitDefending.StartDamageAnimation(this, AttackType.SiegeDefence, defenderDamage));
        // DEACTIVATE
        Deactivate();
        // DECREASE TEMPORARY SPEC ABS EFFECTS       
        cardSO.DecreaseAllAttackTemporaryEffects();
        // VISUALS
        GameEvents.current.RefreshUnitVisuals();
        GameEvents.current.EndShowPosibleBattleSlot();
        GameEvents.current.EndHighlightUnit();
        GetComponent<UnitBehaviour>().ClearLineRenderer();
    }

    private void ShowSpecAbsPopUpAtBattleStart(Unit unitDefending)
    {
        // PIKE
        if (unitDefending.cardSO.CheckIfHasActiveSpecAb("Pike"))
        {
            UnitTypeSO unitType = cardSO.cardTypeSO as UnitTypeSO;
            if (unitType.unitType == UnitType.Cavalry)
            {
                unitDefending.SetSpecAbPopUp(unitDefending.cardSO.GetSpecAbByName("Pike"));
            }
        }
        // FIRST STRIKE 
        if (cardSO.CheckIfHasActiveSpecAb("First Strike") && !unitDefending.cardSO.CheckIfHasActiveSpecAb("First Strike"))
        {
            SetSpecAbPopUp(cardSO.GetSpecAbByName("First Strike"));  
        }
        // FIRE DEFENSE
        if (unitDefending.cardSO.CheckIfHasActiveSpecAb("Fire Defense"))
        {
            unitDefending.SetSpecAbPopUp(unitDefending.cardSO.GetSpecAbByName("Fire Defense"));
        }
    }


    private void MelleAttack(Unit unitDefending)
    {
        // BLOCK INTERACTION
        isFighting = true;
        unitDefending.isFighting = true;
        GameEvents.current.BlockZoom();
        GameEvents.current.BlockInteraction();
        // int defenderDamageBonus = 0;
        // 1. SPEC ABS POP UPS
        ShowSpecAbsPopUpAtBattleStart(unitDefending);
        // 1.1. SORTING LAYER
        //Debug.Log("Seting superior sorting layer for :" + cardSO.cardName + ", and for: " + unitDefending.cardSO.cardName);
        GetComponent<UnitBehaviour>().SetSuperiorSortingLayer();
        unitDefending.GetComponent<UnitBehaviour>().SetSuperiorSortingLayer();
        // 2.2. DAMAGE COMPUTE
        int attackerDamage = GetMyDamageWhenAttacking(unitDefending);
        Debug.Log("UNIT: " + cardSO.cardName + " RECIVES " +  attackerDamage + " DAMAGE while ATTACKING");
        int defenderDamage = unitDefending.GetMyDamageWhenDefending(this);
        Debug.Log("UNIT: " + unitDefending.cardSO.cardName + " RECIEVES " + defenderDamage + " DAMAGE while DEFENDING");
        // 2. FIRST STRIKE = ODDZIELNIE BO DWIE CORUTYNY JEDNA PO DRUGIEJ !
        if (cardSO.CheckIfHasActiveSpecAb("First Strike") && !unitDefending.cardSO.CheckIfHasActiveSpecAb("First Strike"))
        {
            StartCoroutine(unitDefending.StartDamageAnimation(this, AttackType.FirstStrikeDefence, defenderDamage));
        }
        // 3. NORMAL MELLE ATTACK = OD RAZU SYMULTANICzNE DWIE ANIMACJE !
        else
        {
            StartCoroutine(StartDamageAnimation(unitDefending, AttackType.MelleAttack, attackerDamage));
            StartCoroutine(unitDefending.StartDamageAnimation(this, AttackType.MelleDefence, defenderDamage));
        }
        // 4. DEACTIVATE (TP DO SPEC ABS if needed (Two attacks, move after attack+?)
        Deactivate();
        // 5. REDUCE TEMPORARY EFFECTS DURATION DEPENDING ON ATTACK
        unitDefending.cardSO.DecreaseAllAttackTemporaryEffects();
        cardSO.DecreaseAllAttackTemporaryEffects();
        // 6. REFRESH VISUALS
        GameEvents.current.RefreshUnitVisuals();
        GameEvents.current.EndShowPosibleBattleSlot();
        GameEvents.current.EndHighlightUnit();
        GetComponent<UnitBehaviour>().ClearLineRenderer();
    }

    public void SetSpecAbPopUp(SpecialAbilitySO specAb)
    {
        GameObject newPopUp = Instantiate(specAbPopUpPrefab, transform);
        newPopUp.GetComponent<Image>().sprite = specAb.specAbIcon;
        newPopUp.SetActive(true);
        LeanTween.moveLocalY(newPopUp, 50f, 1.2f).setEase(LeanTweenType.easeInSine);
        LeanTween.scale(newPopUp, new Vector3(3f, 3f, 0), 1.2f).setEase(LeanTweenType.easeInSine);
        LeanTween.color(newPopUp.GetComponent<RectTransform>(), new Color32(0,0,0,0), 1.2f).setEase(LeanTweenType.easeInQuint);
    }

    IEnumerator ItemAttackMod(int baseAtt, int attMod)
    {
        GameEvents.current.BlockInteraction();
        GameManager.instance.noOfAnimationsPlaying++;
        // BONUS OR WOUND ANIMATION
        // TO DO some kind of icon like wound
        Debug.Log("START ATT MOD ANIMATION OF ITEM OR ACTION (WITH GENERATED SPEC AB)");
        float xPos = unitAttackText.transform.localPosition.x;
        float yPos = unitAttackText.transform.localPosition.y;
        int startAtt = baseAtt;
        if (attMod > 0)
        {
            Debug.Log("ADD ATTACK!");
            for (int i = 0; i < attMod; i++)
            {
                LeanTween.moveLocalY(unitAttackText.gameObject, 30, 0.5f);
                yield return new WaitForSeconds(0.5f);
                unitAttackText.transform.localPosition = new Vector3(xPos, yPos);
                unitAttackText.text = (startAtt + (i + 1)).ToString();
            }
        }
        if (attMod < 0)
        {
            Debug.Log("REDUCE ATTACK!");
            for (int i = 0; i > attMod; i--)
            {
                LeanTween.moveLocalY(unitAttackText.gameObject, -30, 0.5f);
                yield return new WaitForSeconds(0.5f);
                unitAttackText.transform.localPosition = new Vector3(xPos, yPos);
                unitAttackText.text = (startAtt + (i + 1)).ToString();
            }
        }
        yield return new WaitForSeconds(0.2f);
        RefreshUnitAtributes();
        GameManager.instance.noOfAnimationsPlaying--;
        ////// WAIT UNTIL ANIMATION ENDS
        yield return new WaitUntil(() => !GameManager.instance.IsAnimationPlayig());
        yield return new WaitForSeconds(0.2f);
        GameEvents.current.StartInteraction();
    }

    IEnumerator ItemDefenceMod(int baseDef, int defMod)
    {
        GameEvents.current.BlockInteraction();
        GameManager.instance.noOfAnimationsPlaying++;
        // BONUS OR WOUND ANIMATION
        // TO DO some kind of icon like wound
        Debug.Log("START DEF MOD ANIMATION");
        float xPos = unitDefenceText.transform.localPosition.x;
        float yPos = unitDefenceText.transform.localPosition.y;
        int startDef = baseDef;
        if (defMod > 0)
        {
            Debug.Log("ADD DEFENCE!");
            for (int i = 0; i < defMod; i++)
            {
                LeanTween.moveLocalY(unitDefenceText.gameObject, 30, 0.5f);
                yield return new WaitForSeconds(0.5f);
                unitDefenceText.transform.localPosition = new Vector3(xPos, yPos);
                unitDefenceText.text = (startDef + (i + 1)).ToString();
            }
        }
        if (defMod < 0)
        {
            Debug.LogWarning("USE ACTION DAMAGE!");
        }
        yield return new WaitForSeconds(0.2f);
        RefreshUnitAtributes();
        GameManager.instance.noOfAnimationsPlaying--;
        ////// WAIT UNTIL ANIMATION ENDS
        yield return new WaitUntil(() => !GameManager.instance.IsAnimationPlayig());
        yield return new WaitForSeconds(0.2f);
        GameEvents.current.StartInteraction();
    }

    public void ActionAttackUnit(int damage)
    {
        StartCoroutine(ActionDamage(damage));
    }

    IEnumerator ActionDamage(int dmg)
    {
        // BLOCK INTERACTION
        GameEvents.current.BlockInteraction();
        GameEvents.current.BlockZoom();
        GameManager.instance.noOfAnimationsPlaying++;
        // DAMAGE 
        Debug.Log("START ACTION DAMAGE ANIMATION");
        float xPos = unitDefenceText.transform.localPosition.x;
        float yPos = unitDefenceText.transform.localPosition.y;
        int startDef = cardSO.GetDef();
        int damage = dmg;
        if (damage > startDef)
        {
            damage = startDef;
        }
        // DAMAGE ANIMATION
        for (int i = 0; i < damage; i++)
        {
            LeanTween.moveLocalY(unitDefenceText.gameObject, -30, 0.5f);
            yield return new WaitForSeconds(0.5f);
            if (cardSO.GetDef() <= 0)
            {
                break;
            }
            unitDefenceText.transform.localPosition = new Vector3(xPos, yPos);
            unitDefenceText.text = (startDef - (i + 1)).ToString();
        }
        // DAMAGE
        cardSO.RemoveDef(damage);
        // DESTROY UNIT
        if (cardSO.GetDef() <= 0)
        {
            // MORALE - FOR NOW WITHOUT PENALTY
            // cardSO.GetOwner().playerActMorale -= cardSO.cardCost;
            // REFRESH VISUALS
            cardSO.GetOwner().RefreshAttributeVisuals();
            // START ANIMATION 
            StartCoroutine(DestroyUnit());
        }
        GameManager.instance.noOfAnimationsPlaying--;
        GameEvents.current.RefreshUnitVisuals();
        ////// WAIT UNTIL ANIMATION ENDS
        yield return new WaitUntil(() => !GameManager.instance.IsAnimationPlayig());   
        yield return new WaitForSeconds(0.05f);
        // SORTING LAYER
        if (!unitKilled)
        {
            isFighting = false;
            GetComponent<UnitBehaviour>().SetStantardSortingLayer();
        }
        else
        {
            isFighting = false;
            GetComponent<UnitBehaviour>().SetGraveyardSortingLayer();
        }
        // START INTERACTION
        GameEvents.current.UnBlockZoom();
        GameEvents.current.StartInteraction();
    }

    public IEnumerator StartDamageAnimation(Unit enemy, AttackType attackType, int damage)
    {
        // ANIMATION PLAYS
        GameManager.instance.noOfAnimationsPlaying++;
        // APROACH THE ATTACKER TO DEFENDER: HERE MOVE UNIT IF MELLE TYPE ATTACK
        if (attackType == AttackType.MelleAttack)
        {
            if (cardSO.GetOwner() == GameManager.instance.playerSouth)
            {
                LeanTween.move(gameObject, new Vector3(enemy.GetSlot().transform.position.x, enemy.GetSlot().transform.position.y - 280), 0.5f);
            }
            else
            {
                LeanTween.move(gameObject, new Vector3(enemy.GetSlot().transform.position.x, enemy.GetSlot().transform.position.y + 280), 0.5f);
            }

            yield return new WaitForSeconds(0.6f);
        }
        if (attackType == AttackType.FirstStrikeDefence)
        {
            if (cardSO.GetOwner() == GameManager.instance.playerSouth)
            {
                LeanTween.move(enemy.gameObject, new Vector3(GetSlot().transform.position.x, GetSlot().transform.position.y + 280), 0.5f);
            }
            else
            {
                LeanTween.move(enemy.gameObject, new Vector3(GetSlot().transform.position.x, GetSlot().transform.position.y - 280), 0.5f);
            }
            yield return new WaitForSeconds(0.6f);
        }
        // TO DO! BONUS ANIMATION
        int bonusAttWhenDefending = 0;
        int bonusAttWhenAttacking = 0;
        if (attackType == AttackType.MelleDefence)
        {
            // bonusAttWhenDefending = GetMyBonusAttackWhenDefending(enemy);
        }
        if (attackType == AttackType.MelleAttack)
        {
            //bonusAttWhenAttacking = GetMyBonusAttackWhenAttacking(enemy);
        }
        // to do - animation 

        // WOUND ANIMATION
        if (attackType == AttackType.SiegeDefence)
        {
            GameObject cannonBall = Instantiate(cannonBallPrefab, transform, true);
            cannonBall.transform.position = new Vector3(enemy.transform.position.x, enemy.transform.position.y + 20);
            LeanTween.moveLocal(cannonBall, new Vector3(0, 20),0.5f);
            yield return new WaitForSeconds(0.5f);
            woundAnimator.SetTrigger("siegeWound");
            yield return new WaitForSeconds(0.2f);
            Destroy(cannonBall);
        }
        else
        {
            woundAnimator.SetTrigger("melleWound");
            yield return new WaitForSeconds(0.3f);
        }
        Debug.Log("MY UNIT: " + cardSO.cardName + " RECIVED: " + damage + " DAMAGE, dealt by enemy: " + enemy.cardSO.cardName);
        // DAMAGE CALCULATIONS AND DAMAGE ANIMATION
        //Debug.Log("START DAMAGE ANIMATION");
        // START POSITION OF DAMAGE TEXT
        float xPos = unitDefenceText.transform.localPosition.x;
        float yPos = unitDefenceText.transform.localPosition.y;
        int startDef = cardSO.GetDef();
        if (damage > startDef)
        {
            damage = startDef;
        }
        // DAMAGE ANIMATION
        for (int i=0; i < damage; i++)
        {
            // ATTRIBUTES ANIMATION
            LeanTween.moveLocalY(unitDefenceText.gameObject, -30, 0.5f);
            yield return new WaitForSeconds(0.5f);
            unitDefenceText.transform.localPosition = new Vector3(xPos, yPos);
            unitDefenceText.text = (startDef - (i+1)).ToString();
        }
        // DAMAGE
        cardSO.RemoveDef(damage);
        // MOVE BACK UNIT IF MELLE ATTACK AND LIVES
        if (attackType == AttackType.MelleAttack && cardSO.GetDef() > 0)
        {
            LeanTween.moveLocal(gameObject, new Vector3(0, 0, 0), 0.5f);
            yield return new WaitForSeconds(0.6f);
        }
        else if (attackType == AttackType.FirstStrikeAttack && cardSO.GetDef() > 0)
        {
            LeanTween.moveLocal(gameObject, new Vector3(0, 0, 0), 0.5f);
            yield return new WaitForSeconds(0.6f);
        }
        else if (attackType == AttackType.FirstStrikeDefence && cardSO.GetDef() <= 0)
        {
            LeanTween.moveLocal(enemy.gameObject, new Vector3(0, 0, 0), 0.5f);
        }
        // DESTROY UNIT 
        if (cardSO.GetDef() <= 0)
        {
            //Debug.Log("DESTROYNIGN UNIT " + cardSO.cardName + " KILLED BY " + enemy.cardSO.cardName + " WHEN OPERATING " + attackType);
            // XP (TO FURTHER CAMPAIGNS?
            enemy.cardSO.xp += cardSO.cardCost;
            // PILAGE (GOLD) - TO DO change to fire event at kill unit.
            if (attackType == AttackType.MelleDefence || attackType == AttackType.FirstStrikeDefence)
            {
                // Debug.Log("CHECKING PILLAGE: " + cardSO.cardName + " killed an enemy: " + enemy.cardSO.cardName + " when attacking");
                if (enemy.cardSO.CheckIfHasActiveSpecAb("Pilage"))
                {
                    enemy.SetSpecAbPopUp(enemy.cardSO.GetSpecAbByName("Pilage"));
                    enemy.cardSO.GetOwner().playerActGold += cardSO.cardCost;
                }
            }
            // MORALE (FOR NOW NOT TAKE MORALE
            // cardSO.GetOwner().playerActMorale -= cardSO.cardCost;
            // REFRESH VISUALS
            cardSO.GetOwner().RefreshAttributeVisuals();
            enemy.cardSO.GetOwner().RefreshAttributeVisuals();
            // START ANIMATION 
            StartCoroutine(DestroyUnit());
        }
        // FURTHER LOGIC OF FIRST STRIKE: IF NOT KILL UNIT, GO ON WITH COUNTERSTRIKE, IF KILL UNIT REFRESH BATTLE
        if (attackType == AttackType.FirstStrikeDefence && cardSO.GetDef() > 0)
        {
            int firstStikeAttackerDamage = GetMyDamageWhenEnemyCounterAttacksAfterMyFirstStrikeAttack(enemy);
            StartCoroutine(enemy.StartDamageAnimation(this, AttackType.FirstStrikeAttack, firstStikeAttackerDamage));
        }
        else if (attackType == AttackType.FirstStrikeDefence && cardSO.GetDef() <= 0)
        {
            enemy.SetSortingLayerAfterFirstStrikeVictoriousBattle();
        }
        // DESTROY TEMPORARY ABILITIES IF SO
        cardSO.CheckAndEndTemporaryEffect();
        enemy.cardSO.CheckAndEndTemporaryEffect();
        GameEvents.current.RefreshUnitVisuals();
        ////// WAIT UNTIL ANIMATION END
        GameManager.instance.noOfAnimationsPlaying--;
        yield return new WaitUntil(() => !GameManager.instance.IsAnimationPlayig());
        // START INTERACTION DEPENDING IF DESTROY ANIMATION IS ACTIVE
        yield return new WaitForSeconds(0.02f);
        // SORTING LAYER
        if (!unitKilled)
        {
            isFighting = false;
            GetComponent<UnitBehaviour>().SetStantardSortingLayer();
        }
        else
        {
            isFighting = false;
            GetComponent<UnitBehaviour>().SetGraveyardSortingLayer();
        }
        // START INTERACTION
        Debug.Log("STARTING INTERACTION - END OF DAMAGE ANIMATIO COROUTINE OF OBJECT: "+ cardSO.cardName );
        GameEvents.current.StartInteraction();
        GameEvents.current.UnBlockZoom();
        RefreshAfterBattle(enemy);
    }

    public void SetSortingLayerAfterFirstStrikeVictoriousBattle()
    {
        // SORTING LAYER
        if (!unitKilled)
        {
            isFighting = false;
            GetComponent<UnitBehaviour>().SetStantardSortingLayer();
        }
        else
        {
            isFighting = false;
            GetComponent<UnitBehaviour>().SetGraveyardSortingLayer();
        }
    }

    public void RefreshAfterBattle(Unit unitAttacking)
    {
        // REFRESH ALL ATRIBUTES
        GameEvents.current.RefreshUnitVisuals();
        GameManager.instance.playerNorth.RefreshAttributeVisuals();
        GameManager.instance.playerSouth.RefreshAttributeVisuals();
        // CHECK END GAME
        // Debug.Log("UNIT INFLICTING DAMAGE: " + unitAttacking.cardSO.cardName + " Owner has MORALE: " + unitAttacking.cardSO.GetOwner().playerActMorale);
        // Debug.Log("UNIT RECIVING DAMAGE: " + cardSO.cardName + " Owner has MORALE: " + cardSO.GetOwner().playerActMorale);
        unitAttacking.cardSO.GetOwner().CheckEndGame();
        cardSO.GetOwner().CheckEndGame();
    }

    // TO DO ZMIENIC SPRAWDZANIE CHECK TYPE NA CHECK SIEGE W INNCYH MIEJSCA!
    public bool CheckSiege()
    {
        UnitTypeSO unitType = cardSO.cardTypeSO as UnitTypeSO;
        if (unitType.unitSiege)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public BattleSlot GetSlot()
    {
        if (transform.parent.parent.GetComponent<BattleSlot>() != null)
        {
            return transform.parent.parent.GetComponent<BattleSlot>();
        }
        else
        {
            Debug.LogWarning("NO PARENT SLOT OF UNIT: " + unitName);
            return null;
        }
        
    }

    public void DeactivateAfterTentAttack()
    {
        // TENT ATTACK IN TENTSLOT SCRIPT
        Deactivate();
    }


    private void Deactivate()
    {
        active = false;
        activityDot.color = new Color32(255, 0, 0, 200);
        GetComponent<UnitBehaviour>().EndPulse();
    }

    private void Activate()
    {
        // TO DO 
        active = true;
        activityDot.color = new Color32(0, 255, 0, 200);
        GetComponent<UnitBehaviour>().StartPulse();
    }

    public bool CheckActive()
    {
        if (active) { return true; }
        else { return false; }
    }

    // CALLED BY UPKEEP BUTTON
    public void PayUpkeep()
    {
        if (!paid)
        {
            GameManager.instance.actPlayer.playerActGold -= cardSO.cardUpkeep;
            paid = true;
            Activate();
            // SPRAWDZIC CZY INNE JEDNOSTKI MAJA NA UPKEEP i AKTUALIZOWAC VISUALS 
            GameEvents.current.RefreshUnitVisuals();
            // GAME VISUALS
            cardSO.GetOwner().RefreshAttributeVisuals();
        }
        else
        {
            Debug.LogWarning("UNIT FUNCTION PAY UPKEEP - Already paid");
        }
        
    }

    private void ActivateUpkeepButton()
    {
        if (cardSO.GetOwner().playerActGold >= cardSO.cardUpkeep)
        {
            if (cardSO.GetOwner() == GameManager.instance.actPlayer)
            {
                upkeepButton.gameObject.SetActive(true);
            }
        }
        else
        {
            upkeepButton.gameObject.SetActive(false);
        }
        
    }

    public void NewTurn()
    {
        // UNIT ACTIVATE due to UPKEEP
        if (cardSO.cardUpkeep <= 0)
        {
            Activate();
            paid = false;
        }
        else
        {
            Deactivate();
            paid = false;
            ActivateUpkeepButton();
        }
        // DECREASE TURN BASED SPEC ABS
        cardSO.DecreaseAllTurnTemporaryEffects();
        // AUTO RELOAD SPEC ABS
        cardSO.ReloadSpecAbs();
    }

    public void EndTurn()
    {
        Deactivate();
        upkeepButton.gameObject.SetActive(false);
    }
    
    public IEnumerator DestroyUnit()
    {
        // !!! START ONLY FROM COROUTINE WITH WAIT UNTIL ANIMATION ENDS WaitUntil(() => !GameManager.instance.IsAnimationPlayig()) !!! //
        // DESTROY ANIMATION PLAYING
        GameManager.instance.noOfAnimationsPlaying++;
        // KILLED
        unitKilled = true;
        // DEACTIVATE
        Deactivate();
        // UNSUBSCRIBE EVENTS
        GetComponent<UnitBehaviour>().UnsubscribeAll();
        GameEvents.current.onRefreshUnitVisuals -= RefreshUnitAtributes;
        // START ANIMATION
        GetComponent<Animator>().SetTrigger("destroy");
        yield return new WaitForSeconds(1f);
        // MOVE CARD
        LeanTween.move(gameObject, cardSO.GetOwner().graveyard, 0.5f);
        yield return new WaitForSeconds(0.6f);
        // PUT TO GRAVEYARD 
        transform.SetParent(cardSO.GetOwner().graveyard, false);
        transform.localPosition = new Vector3(0, 0);
        // PLAYER REFRESH
        cardSO.GetOwner().RefreshAttributeVisuals();
        GameManager.instance.GetOtherPlayer(cardSO.GetOwner()).RefreshAttributeVisuals();
        // UNBLOCK INTERACTIVITY
        GameManager.instance.noOfAnimationsPlaying--;
        yield return new WaitForSeconds(0.2f);
        // CHECK END GAME
        cardSO.GetOwner().CheckEndGame();

    }



}
