using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.ExceptionServices;
using UnityEngine;

[CreateAssetMenu(fileName = "SpecialAbility", menuName = "SpecialAbility")]
public class SpecialAbilitySO : ScriptableObject
{
    [Header ("GENERAL DATA")]
    public string specAbName;
    [TextArea]
    public string specAbDesc;
    public Sprite specAbIcon;
    public bool specAbIsActive = true;

    [Header ("VALUES")]
    public bool firstSrike;
    public bool protection;
    public bool charge;
    public bool pilage;
    public bool flank;
    public bool maneuver;
    public int income;
    public int cavalryDefPenalty;
    public int attBonusDefending;
    public int attBonusAttacking;
    public int attBonus;
    public int defBonus;

    [Header("FIREARMS AMMUNITION ATS")] // AMMUNITION DEPENDING SPEC AB DEACTIVATES AFTER 1 TURN, REFILL AT NEW TURN IF HAS AMMO
    public bool firearm;
    public bool firearmDefence;
    public int maxAmmo; // MAX AMMUNITION
    public int actAmmo; // ACTUAL AMMUNITION

    [Header ("DURATION")] // SPEC AB DISAPEAR AFTER DURATION
    public bool temporaryEffect;
    public int effectTurnDuration;
    //public int effectAttackDuration;

    [Header("AI values")]
    public int deployValueAI;
    public int killValueAI;

    public bool CheckName(string specAbNameToCheck)
    {
        if (specAbName == specAbNameToCheck)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    //public void DecreaseAttackDuration()
    //{
    //    if (!temporaryEffect)
    //    {
    //        Debug.LogWarning("Effect permanent! Cannot decrease duration!");
    //    }
    //    else
    //    {
    //        effectAttackDuration--;
    //    }
    //}

    public void DecreaseAmmo()
    {
        if (!firearm)
        {
            Debug.LogWarning("Effect NOT FIREARM! Cannot decrease ammo!");
        }
        else
        {
            actAmmo--;
        }
    }

    public void DecreaseTurnsDuration()
    {
        if (!temporaryEffect)
        {
            Debug.LogWarning("Effect permanent! Cannot decrease duration!");
        }
        else
        {
            effectTurnDuration--;
        }
    }

    private void CopySpecAbs(CardSO cardToSetSpecAb)
    {
        Debug.Log("SET SPEC AB");
        specAbName = cardToSetSpecAb.cardName;
        specAbDesc = cardToSetSpecAb.cardDescription;
        specAbIcon = cardToSetSpecAb.cardIco;
        if (cardToSetSpecAb.CheckIfHasSpecAb("First Strike"))
        {
            firstSrike = true;
        }
        else if (cardToSetSpecAb.CheckIfHasSpecAb("Protection"))
        {
            protection = true;
        }
        else if(cardToSetSpecAb.CheckIfHasSpecAb("Pilage"))
        {
            pilage = true;
        }
        else if(cardToSetSpecAb.CheckIfHasSpecAb("Income"))
        {
            income = cardToSetSpecAb.GetSpecAbByName("Income").income;
        }
        else if(cardToSetSpecAb.CheckIfHasSpecAb("Pike"))
        {
            cavalryDefPenalty = cardToSetSpecAb.GetSpecAbByName("Pike").cavalryDefPenalty;
        }
        else if(cardToSetSpecAb.CheckIfHasSpecAb("Flanking"))
        {
            flank = true;
        }
        else if(cardToSetSpecAb.CheckIfHasSpecAb("Maneuver"))
        {
            maneuver = true;
        }
        else if (cardToSetSpecAb.CheckIfHasSpecAb("Fire Defense"))
        {
            attBonusDefending = cardToSetSpecAb.GetSpecAbByName("Fire Defense").attBonusDefending;
        }
        //else
        //{
        //    Debug.LogWarning("COPY SPEC AB ERROR - NOT RECOGNIZED");
        //}
    }

    public void SetSpecAb(CardSO cardToSetSpecAb)
    {
        if (cardToSetSpecAb.cardTypeSO.cardType == CardType.Action)
        {
            // COPY ABILITIES
            CopySpecAbs(cardToSetSpecAb);
            // ACTION - ADDS BONUS FROM BASIC CARD ATTS
            attBonus = cardToSetSpecAb.baseAtt;
            defBonus = cardToSetSpecAb.baseDef;
            // EFFECT WITH DURATION
            ActionTypeSO actionType = cardToSetSpecAb.cardTypeSO as ActionTypeSO;
            if (actionType.actionTurnDuration == 0)
            {
                temporaryEffect = false;
            }
            else
            {
                temporaryEffect = true;
                effectTurnDuration = actionType.actionTurnDuration;
                //effectAttackDuration = actionType.actionAttacksDuration;
            }
            // AMMO - ACTIVATING EFFECT
            firearm = actionType.firearm;
            firearmDefence = actionType.firearmDefence;
            if (actionType.maxAmmo > 0)
            {
                maxAmmo = actionType.maxAmmo;
                actAmmo = maxAmmo;
            }
        }
        if (cardToSetSpecAb.cardTypeSO.cardType == CardType.Item)
        {
            // COPY ABILITIES
            CopySpecAbs (cardToSetSpecAb);
            // ITEM - ADDS BONUS FROM BASIC CARD ATTS
            attBonus = cardToSetSpecAb.baseAtt;
            defBonus = cardToSetSpecAb.baseDef;
            // ITEM EFFECT WITH TURN DURATION
            ItemTypeSO itemType = cardToSetSpecAb.cardTypeSO as ItemTypeSO;
            if (itemType.temporaryTurnsItemEffect == 0)
            {
                temporaryEffect = false;
            }
            else
            {
                temporaryEffect = true;
                effectTurnDuration = itemType.temporaryTurnsItemEffect;
            }
            // ITEM EFFECT IS ACTIVATING EFFECT
            // AMMO - ACTIVATING EFFECT
            firearm = itemType.firearm;
            firearmDefence = itemType.firearmDefence;
            if (itemType.maxAmmo > 0)
            {
                maxAmmo = itemType.maxAmmo;
                actAmmo = maxAmmo;
            }
            //Debug.Log("ITEM SPEC AB SET OFF - DONE");
        }
    }

    public int GetTurnDuration()
    {
        if (temporaryEffect)
        {
            return effectTurnDuration;
        }
        else
        {
            return 0;
        }
    }

    //public int GetAttacksDuration()
    //{
    //    if (temporaryEffect)
    //    {
    //        return effectAttackDuration;
    //    }
    //    else
    //    {
    //        return 0;
    //    }
    //}

}
