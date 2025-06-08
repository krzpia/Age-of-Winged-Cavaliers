using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class CardRarity
{
    public string cardRarity;
    public Sprite rarityIcon;
} 

public enum UnitType {NonUnit = 0, Cavalry = 1, Infantry =2, Artillery =3, OtherUnit = 4}

public enum CardType {Unit=0, Item=1, Action=2, Other=3}

// NOT USED for NOW
public enum Nationality {None = 0, Poland = 1, Lithuania =2, Ukraine = 3, Russia = 4, Sweden = 5, Crimea = 6, Ottoman = 7, Brandenbug = 8, Wallachia = 9}


[CreateAssetMenu(fileName = "CardSO", menuName = "Card")]
public class CardSO : ScriptableObject

{
    [Header("BASIC INFO")]
    public string cardName;
    // public string cardNationality;
    public Nationality nationality;
    [TextArea]
    public string cardDescription;
    //public CardType cardType;
    public CardTypeSO cardTypeSO;
    //public ItemTypeSO itemType;
    //public CityTypeSO cityType;
    //public UnitTypeSO unitType;
    //public ActionTypeSO actionType;
    public int cardId;


    [Header("ATTRIBUTES")]
    public int cardCost;
    public int cardUpkeep;
    public int baseAtt;
    public int baseDef;
    [SerializeField]
    private int actAtt;
    [SerializeField]
    private int actDef;

    public int xp;
    public int level;

    [Header("SPECIAL ABILITIES"), SerializeField]
    public List<SpecialAbilitySO> specialAbilityList;

    [Header("SPRITES")]
    public Sprite cardImage;
    public string artAuthor;
    public Sprite cardFlag;
    public Sprite cardIco;

    [Header("LOGIC"), SerializeField]
    private Player owner;


    public void SetOwner(Player player)
    {
        //Debug.Log("! SETTING OWNER OF A CARD!");
        owner = player;
    }

    public Player GetOwner()
    {
        return owner;
    }

    public int GetAtt()
    {
        return (actAtt + GetAttBonusFromSpecAbs());
    }

    public void SetActAtt(int att)
    {
        actAtt = att;
    }

    public int GetDef()
    {
        return (actDef + GetDefBonusFromSpecAbs());
    }

    public void SetActDef(int def)
    {
        actDef = def;
    }

    public void RemoveAtt(int att)
    {
        int substractAtt = att;
        if (GetAttBonusFromSpecAbs() <= 0)
        {
            actAtt -= att;
        }
        else
        {
            // PO PIERWSZE ODEJMUJEMY OD ZDOLNOSCI
            List<SpecialAbilitySO> tempListSpecAbToDelete = new List<SpecialAbilitySO>();
            foreach (SpecialAbilitySO specAb in specialAbilityList)
            {
                if (specAb.attBonus > 0)
                {
                    // JEZELI ODEJMUJEMY MNIEJ NIZ ZDOLNOSC
                    if (specAb.attBonus > substractAtt)
                    {
                        specAb.attBonus -= substractAtt;
                        substractAtt = 0;

                    }
                    // JEZELI ODEJMUJEMY DOKLADNIE TYLE ILE ZDOLNOSC
                    else if (specAb.attBonus == substractAtt)
                    {
                        specAb.attBonus -= substractAtt;
                        substractAtt = 0;
                        tempListSpecAbToDelete.Add(specAb);
                    }
                    // JEZELI ODEJMUJEMY WIECEJ NIZ ZDOLNOSC
                    else if (specAb.attBonus < substractAtt)
                    {
                        substractAtt -= specAb.attBonus;
                        specAb.attBonus = 0;
                        tempListSpecAbToDelete.Add(specAb);
                    }
                }
            }
            // JAK COS ZOSTANIE ODEJMUJEMY OD WARTOSCI NOMINALNEJ
            actAtt -= substractAtt;
            // USUWAMY ZDOLNOSCI, KTORE MAJA OBNIZONA WARTOSC DO 0
            foreach (SpecialAbilitySO tempSpecAbToDelete in tempListSpecAbToDelete)
            {
                if (specialAbilityList.Contains(tempSpecAbToDelete))
                {
                    // TO DO JEZELI ZDOLNOSC MA INNE WLASCIWOSCI NIZ BONUSATT? TO ZOSTAWIC?
                    if (tempSpecAbToDelete.attBonus <= 0 && tempSpecAbToDelete.defBonus <= 0)
                    {
                        specialAbilityList.Remove(tempSpecAbToDelete);
                    }
                }
            }
        }
    }

    public void RemoveDef(int def)
    {
        int substractDef = def;
        if (GetDefBonusFromSpecAbs() <= 0)
        {
            actDef -= def;
        }
        else
        {
            List<SpecialAbilitySO> tempListSpecAbToDelete = new List<SpecialAbilitySO>();
            foreach (SpecialAbilitySO specAb in specialAbilityList)
            {
                if (specAb.defBonus > 0)
                {
                    // JEZELI ODEJMUJEMY MNIEJ NIZ ZDOLNOSC
                    if (specAb.defBonus > substractDef)
                    {
                        specAb.defBonus -= substractDef;
                        substractDef = 0;

                    }
                    // JEZELI ODEJMUJEMY DOKLADNIE TYLE ILE ZDOLNOSC
                    else if (specAb.defBonus == substractDef)
                    {
                        specAb.defBonus -= substractDef;
                        substractDef = 0;
                        tempListSpecAbToDelete.Add(specAb);
                    }
                    // JEZELI ODEJMUJEMY WIECEJ NIZ ZDOLNOSC
                    else if (specAb.defBonus < substractDef)
                    {
                        substractDef -= specAb.defBonus;
                        specAb.defBonus = 0;
                        tempListSpecAbToDelete.Add(specAb);
                    }
                }
            }
            // REMOVING DEF
            actDef -= substractDef;
            foreach (SpecialAbilitySO tempSpecAbToDelete in tempListSpecAbToDelete)
            {
                if (specialAbilityList.Contains(tempSpecAbToDelete))
                {
                    // TO DO JEZELI ZDOLNOSC MA INNE WLASCIWOSCI NIZ BONUSATT? TO ZOSTAWIC?
                    if (tempSpecAbToDelete.attBonus <= 0 && tempSpecAbToDelete.defBonus <= 0)
                    {
                        specialAbilityList.Remove(tempSpecAbToDelete);
                    }
                }
            }
        }
    }

    public int CheckTurnTempEffect()
    {
        if (cardTypeSO.cardType == CardType.Item)
        {
            ItemTypeSO itemTypeSO = cardTypeSO as ItemTypeSO;
            return itemTypeSO.temporaryTurnsItemEffect;
        }
        else
        {
            Debug.LogWarning("ERROR CHECK TURN TEMPORARY EFFECT IN CARDSO Class, bad cardTypeSO");
            return 0;
        }
    }

    public int CheckOneAttackTempEffect()
    {
        if (cardTypeSO.cardType == CardType.Item)
        {
            ItemTypeSO itemTypeSO = cardTypeSO as ItemTypeSO;
            if (itemTypeSO.maxAmmo == 1)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
        else
        {
            Debug.LogWarning("ERROR CHECK TURN TEMPORARY EFFECT IN CARDSO Class, bad cardTypeSO");
            return 0;
        }
    }

    public bool CheckIfHasSpecAb(string specAbName)
    {
        foreach (SpecialAbilitySO specAbSO in specialAbilityList)
        {
            if (specAbSO.specAbName == specAbName) return true;
        }
        return false;
    }

    public bool CheckIfHasActiveSpecAb(string specAbName)
    {
        foreach (SpecialAbilitySO specAbSO in specialAbilityList)
        {
            if (specAbSO.specAbName == specAbName)
            {
                if (specAbSO.specAbIsActive)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public void ReloadSpecAbs()
    {
        foreach (SpecialAbilitySO specAbSO in specialAbilityList)
        {
            // 1. FIREARM
            if (specAbSO.firearm)
            {
                if (specAbSO.actAmmo > 0)
                {
                    specAbSO.specAbIsActive = true;
                }
            }
        }
    }

    public SpecialAbilitySO GetSpecAbByName(string specAbName)
    {
        foreach (SpecialAbilitySO specAbSO in specialAbilityList)
        {
            if (specAbSO.specAbName == specAbName)
            {
                return specAbSO;
            }
        }
        Debug.LogWarning("NO SPEC AB FOUND!");
        return null;
    }

    public bool CheckIfHasAnySpecAb()
    {
        if (specialAbilityList.Count > 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public int GetSpecAbsOverallKillValue()
    {
        int overallValue = 0;
        foreach (SpecialAbilitySO specAbSO in specialAbilityList)
        {
            overallValue += specAbSO.killValueAI;
        }
        return overallValue;
    }

    public bool CheckIfIsUnit()
    {
        if (cardTypeSO.cardType == CardType.Unit)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public ActionPlayMethod GetActionPlayMethod()
    {
        if (cardTypeSO.cardType == CardType.Action)
        {
            ActionTypeSO actionTypeSO = cardTypeSO as ActionTypeSO;
            return actionTypeSO.actionPlayMethod;
        }
        else
        {
            Debug.LogWarning("Action Type not found!");
            return ActionPlayMethod.NotCoded;
        }
    }

    public ActionTypeSO GetActionTypeSO()
    {
        return cardTypeSO as ActionTypeSO;
    }

    public UnitType GetUnitType()
    {
        if (cardTypeSO.cardType == CardType.Unit)
        {
            UnitTypeSO unitTypeSO = cardTypeSO as UnitTypeSO;
            return unitTypeSO.unitType; 
        }
        else
        {
            Debug.LogWarning("Unit Type not foud!");
            return UnitType.NonUnit;
        }
    }
    
    public int GetAttBonusFromSpecAbs()
    {
        int attBonus = 0;
        if (GameManager.instance.actPlayer == owner)
        {
            Debug.Log("OBLICZAM ATTACK BONUS JEDNOSTKI: " + cardName + ". JEDNOSTKA JEST W TRAKCIE ATAKU");
            foreach (SpecialAbilitySO specAbSO in specialAbilityList)
            {
                if (specAbSO.specAbIsActive)
                {
                    attBonus += specAbSO.attBonusAttacking;
                }
            }
        }
        else
        {
            Debug.Log("OBLICZAM ATTACK BONUS JEDNOSTKI: " + cardName + ". JEDNOSTKA JEST W TRAKCIE OBRONY, NALICzYMY SPEC ABS TYPU fire defense");
            foreach (SpecialAbilitySO specAbSO in specialAbilityList)
            {
                if (specAbSO.specAbIsActive)
                {
                    attBonus += specAbSO.attBonusDefending;
                }
            }
        }
        
        if (specialAbilityList.Count > 0) 
        {
            foreach (SpecialAbilitySO specAbSO in specialAbilityList)
            {
                if (specAbSO.specAbIsActive)
                {
                    attBonus += specAbSO.attBonus;
                } 
            }
            return attBonus;
        }
        else
        {
            return 0;
        }

        
    }

    public int GetDefBonusFromSpecAbs()
    {
        int defBonus = 0;
        if ( specialAbilityList.Count > 0)
        {
            foreach (SpecialAbilitySO specAbSO in specialAbilityList)
            {
                defBonus += specAbSO.defBonus;
            }
            return defBonus;
        }
        else
        {
            return 0;
        }
        
    }

    public void CheckAndEndTemporaryEffect()
    {
        List<SpecialAbilitySO> tempListSpecAbToDelete = new List<SpecialAbilitySO>();
        // TEMPORARY EFFECTS TO BE REMOVED
        foreach (SpecialAbilitySO specAb in specialAbilityList)
        {
            if (specAb.temporaryEffect)
            {
                if (specAb.effectTurnDuration <= 0)
                {
                    tempListSpecAbToDelete.Add(specAb);
                }
            }
            else
            {
                //Debug.LogWarning("Effect permanent! Cannot end effect!");
            }
        }
        foreach(SpecialAbilitySO tempSpecAbToDelete in tempListSpecAbToDelete)
        {
            if (specialAbilityList.Contains(tempSpecAbToDelete))
            {
                specialAbilityList.Remove(tempSpecAbToDelete);
            }
        }
        // DISABLING EFFECTS TO BE DISABLED
        foreach (SpecialAbilitySO specAbToDeactivate in specialAbilityList)
        {
            if (specAbToDeactivate.firearm)
            {
                if (specAbToDeactivate.actAmmo <= 0)
                {
                    Debug.Log("AMMO DEPLETED: " + specAbToDeactivate.specAbName);
                    specAbToDeactivate.specAbIsActive = false;
                }
            }
        }
    }

    public void DecreaseAllAttackTemporaryEffects()
    {
        foreach (SpecialAbilitySO specAb in specialAbilityList)
        {
            // DURATION ATTACK EFFECT (Narazie nie ma, bron palna przeniesiona do firearm, ammo sie redukuje)
            //if (specAb.temporaryEffect)
            //{
            //    if (specAb.effectAttackDuration > 0)
            //    {
            //        specAb.DecreaseAttackDuration();
            //    }
            //}
            
            bool thisUnitIsDefending;
            if (owner == GameManager.instance.actPlayer)
            {
                thisUnitIsDefending = false;
            }
            else
            {
                thisUnitIsDefending = true;
            }
            // DECREASE AMMO (Fire Defence)
            if (thisUnitIsDefending)
            {
                if (specAb.firearmDefence)
                {
                    if (specAb.actAmmo > 0)
                    {
                        specAb.DecreaseAmmo();
                    }
                }
            }
            // DECREASE AMMO (fire arm)
            if (specAb.firearm && !specAb.firearmDefence)
            {
                if (specAb.actAmmo > 0)
                {
                    specAb.DecreaseAmmo();
                }
            }
        }
    }

    // STILL NOT USED!(?)
    public void DecreaseAllTurnTemporaryEffects()
    {
        foreach (SpecialAbilitySO specAb in specialAbilityList)
        {
            if (specAb.temporaryEffect)
            {
                if (specAb.effectTurnDuration > 0)
                {
                    specAb.DecreaseTurnsDuration();
                }
            }
        }
    }

    // BATTLE LOGIC *AI*
    public bool CheckIfAttackerSOCanKillMe(Unit attacker)
    {
        bool unitKilled = false;
        int myDamage = GetMyCardDamageWhenDefending(attacker.cardSO);
        // FIRST STRIKE not apply (I`m defenfing, so dosent matter if I have first strike or not)
        // TO DO OTHER BONUSES?
        {
            if (myDamage >= actDef)
            {
                unitKilled = true;
            }
        }
        return unitKilled;
    }

    public bool CheckIfAttackerSOWillBeKilled(Unit attacker)
    {
        bool attackerKilled = false;
        int myDamage = GetMyCardDamageWhenDefending(attacker.cardSO);
        int attackerDamage = attacker.cardSO.GetMyCardDamageWhenAttacking(this);
        if (attacker.cardSO.actDef <= attackerDamage)
        {
            attackerKilled = true;
        }
        // FIRST STRIKE
        if (attacker.cardSO.CheckIfHasSpecAb("First Strike"))
        {
            if (myDamage >= actDef)
            {
                return false;
            }
        }
        return attackerKilled;
    }

    public bool CheckIfDefenderSOCanKillMe(Unit defender)
    {
        bool unitKilled = false;
        int myDamage = GetMyCardDamageWhenDefending(defender.cardSO);
        int defenderDamage = defender.cardSO.GetMyCardDamageWhenDefending(this);
        // FIST STRIKE - JEZELI MAM FIRST STRIKE, SPRAWDZAM NAJPIERW CzY JA GO ZABIJE, JESLI TAK< TO JA NIE ZGINE 
        if (CheckIfHasSpecAb("First Strike"))
        {
            if (defender.cardSO.actDef <= defenderDamage)
            {
                // RETURN FALSE - zabije jednostke, zanim ta zdazy zaatakowac, wiec mnie nie zabije!
                return false;
            }
        }
        if (myDamage >= actDef)
        {
            unitKilled = true;
        }
        // TO DO OTHER SPEC ABS
        return unitKilled;
    }

    public bool CheckIfDefenderSOWillBeKilled(Unit defender)
    {
        bool defenderKilled = false;
        int defenderDamage = defender.cardSO.GetMyCardDamageWhenDefending(this);
        if (defenderDamage >= defender.cardSO.actDef)
        {
            defenderKilled = true;
        }
        return defenderKilled;
    }

    public int GetMyCardDamageWhenAttacking(CardSO defender)
    {
        //Debug.Log("f() GetMyDamageWhenAttackUnit(Unit " + defender.cardSO.cardName + "");
        //Debug.Log("ATTACKER ATT: " + cardSO.actAtt);
        //Debug.Log("DEFENDER ATT: " + defender.cardSO.actAtt);
        //Debug.Log("ATTACKER DEF: " + cardSO.actDef);
        //Debug.Log("DEFENDER DEF: " + defender.cardSO.actDef);
        int defenderAttackBonus = 0;
        int attackerAttackBonus = 0;
        int defenderDefenceBonus = 0;
        // int attackerDefenceBonus = 0;

        // SPEC ABS 
        // 1. PIKE (ADD DEFENDER ATTACK IF MY UNIT IS CAVALRY
        if (defender.CheckIfHasSpecAb("Pike"))
        {
            UnitTypeSO unitType = cardTypeSO as UnitTypeSO;
            if (unitType.unitType == UnitType.Cavalry)
            {
                // Debug.Log("APPLY PIKE BONUS " + unitDefending.cardSO.GetSpecAbByName("Pike").cavalryDefPenalty);
                // defender.SetSpecAbPopUp(unitDefending.cardSO.GetSpecAbByName("Pike"));
                defenderAttackBonus += defender.GetSpecAbByName("Pike").cavalryDefPenalty;
                // Debug.Log("DEFENDER ATT BONUS: " + defenderAttackBonus);
            }
        }
        // 2. OTHER SPEC ABS (ADD ATTACKER OR DEFENDER STATS
        // FIRST STRIKE
        if (CheckIfHasSpecAb("First Strike"))
        {
            if (actAtt + attackerAttackBonus > defender.actDef + defenderDefenceBonus)
            {
                // Defender DIES before deals damage to me
                return 0;
            }
            else
            {
                return defender.actAtt + defenderAttackBonus;
            }
        }
        else
        {
            return defender.actAtt + defenderAttackBonus;
        }
    }

    public int GetMyCardDamageWhenDefending(CardSO attacker)
    {
        // int defenderAttackBonus = 0;
        int attackerAttackBonus = 0;
        // int defenderDefenceBonus = 0;
        // int attackerDefenceBonus = 0;
        // SPEC ABS
        // TO DO
        // RETURN RESULT
        return attacker.actAtt + attackerAttackBonus;
    }

    public int GetRealCardCost()
    {
        int realCost = cardCost;
        int realDef = GetDef();
        int realAtt = GetAtt();
        int halfCost = Mathf.RoundToInt(cardCost / 2);
        if (realDef < baseDef)
        {
            // WOUNDED, SUBTRACT COST
            float baseCost = (float)cardCost;
            float p = realDef / baseDef;
            float pCost = realCost * p;
            if(p > 0.6 & p <= 1)
            {
                realCost = cardCost;
            }
            if (p >0.3 & p <= 0.6)
            {
                realCost = Mathf.RoundToInt(baseCost * (p + 0.15f));
            }
            if (p > 0 & p <= 0.3)
            {
                realCost = Mathf.RoundToInt(baseCost * (p + 0.2f));
            }
        }
        if (realDef > baseDef)
        {
            // UPGRADED (ITEM OR ACTION)
            realCost += 1;
        }
        if (realAtt < baseAtt)
        {
            realCost -= (baseAtt - realAtt);
        }
        if (realAtt > baseAtt)
        {
            realCost += 1;
        }
        if (realCost < 1)
        {
            realCost = 1;
        }
        // Debug.Log("REAL COST OF :" + cardName + " is: " + realCost);
        return realCost;
    }
}