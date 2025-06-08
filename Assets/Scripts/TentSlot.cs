using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TentSlot : MonoBehaviour, IDropHandler
{
    public Player owner;
    public Image highlightImage;
    public Image glowImage;
    public Image leftShield;
    public Image rightShield;


    void Awake()
    {
        GameEvents.current.onShowPosibleAttacks += HighlightTentToAttack;
        GameEvents.current.onEndHighlightUnit += EndHighlight;
    }


    public void OnDrop(PointerEventData eventData)
    {
        //Debug.Log("ON DROP ON TENT: " + eventData.pointerDrag.name);
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
                 if (!GameManager.instance.unitIsDragging) return;
                 // ATAK TYLKO JEDNOSTKAMI PRZECIWNIKA
                 if (eventData.pointerDrag.GetComponent<Unit>().cardSO.GetOwner() == owner) return;
                 // RUSZAM TYLKO JEDNOSTKAMI AKTYWNYMI
                 if (!eventData.pointerDrag.GetComponent<Unit>().CheckActive()) return;
                 //Debug.Log("ATTACK ON TENT BY UNIT: " + eventData.pointerDrag.name);
                 if (eventData.pointerDrag.GetComponent<Unit>().CheckIfCanAttackOpponentsTent())
                 {
                     TentAttack(eventData.pointerDrag.GetComponent<Unit>());
                 }
                 else
                 {
                     Debug.Log("ATTACK ON ENEMY BASE NOT POSIBLE");
                 }
            }
        }
    }

    private void HighlightTentToAttack(Unit attacker)
    {
        //Debug.Log("HighLightUnit: " + GetComponent<Unit>().cardSO.cardName);
        //Debug.Log("UNIT ATTACKING: " + unitAttacking.cardSO.cardName);
        if (attacker == null) return;
        if (attacker.cardSO.GetOwner() == owner) return;
        // TO DO (2 strony do ataku?)
        if (attacker.CheckIfCanAttackOpponentsTent())
        {
            highlightImage.gameObject.SetActive(true);
            highlightImage.sprite = GameManager.instance.attackIcon;
            highlightImage.color = new Color32(220, 40, 40, 240);
            glowImage.gameObject.SetActive(true);
            glowImage.color = new Color32(255, 0, 0, 255);
            glowImage.gameObject.GetComponent<Animator>().SetBool("RedGlow", true);
        }
    }

    public void RefreshShieldIcon()
    {
        int leftUnitsCount = 0;
        int rightUnitsCount = 0;
        for (int i = 0; i < 4; i++)
        {
            if (owner.deployLine.GetChild(i).GetComponent<BattleSlot>().GetUnit() != null)
            {
                leftUnitsCount++;
            }
        }
        for (int i = 4; i < 8; i++)
        {
            if (owner.deployLine.GetChild(i).GetComponent<BattleSlot>().GetUnit() != null)
            {
                rightUnitsCount++;
            }
        }
        if (leftUnitsCount > 0)
        {
            leftShield.gameObject.SetActive(true);
            leftShield.sprite = GameManager.instance.smallShieldIcon;
        }
        else
        {
            leftShield.gameObject.SetActive(false);
            leftShield.sprite = null; 
        }
        if (rightUnitsCount > 0)
        {
            rightShield.gameObject.SetActive(true);
            rightShield.sprite = GameManager.instance.smallShieldIcon;
        }
        else
        {
            rightShield.gameObject.SetActive(false);
            rightShield.sprite = null;
        }
    }

    public bool CheckIfLeftSideProtected()
    {
        for (int i = 0; i < 4; i++)
        {
            if (owner.deployLine.GetChild(i).GetComponent<BattleSlot>().GetUnit() != null)
            {
                return true;
            }
        }
        return false;
    }

    public bool CheckIfRightSideProtected()
    {
        for (int i = 4; i < 8; i++)
        {
            if (owner.deployLine.GetChild(i).GetComponent<BattleSlot>().GetUnit() != null)
            {
                return true;
            }
        }
        return false;
    }


    private void EndHighlight()
    {
        highlightImage.gameObject.SetActive(false);
        highlightImage.sprite = null;
        highlightImage.color = new Color32(0, 0, 0, 0);
        if (glowImage.gameObject.activeSelf == true)
        {
            glowImage.gameObject.GetComponent<Animator>().SetBool("RedGlow", false);
        }
        glowImage.gameObject.SetActive(false);
        glowImage.color = new Color32(0, 0, 0, 0);
    }


    public void TentAttack(Unit attacker)
    {
        // 1. BLOCK INTERACTION
        attacker.isFighting = true;
        GameEvents.current.BlockInteraction();
        GameEvents.current.BlockZoom();
        // 2. START ANIMATION COROUTINE
        GetComponent<Animator>().SetTrigger("Wound");
        if (attacker.cardSO.GetUnitType() == UnitType.Artillery)
        {
            StartCoroutine(TentDamageAnimation(attacker, AttackType.SiegeDefence));
        }
        else
        {
            StartCoroutine(TentDamageAnimation(attacker, AttackType.MelleDefence));
        }
        // 3. REFRESH VISUALS
        // SPEC ABS TEMP EFFECTS
        attacker.cardSO.DecreaseAllAttackTemporaryEffects();
        // DESTROY TEMPORARY ABILITIES IF SO
        attacker.cardSO.CheckAndEndTemporaryEffect();
        GameEvents.current.RefreshUnitVisuals();
        GameEvents.current.EndShowPosibleBattleSlot();
        GameEvents.current.EndHighlightUnit();
        attacker.GetComponent<UnitBehaviour>().ClearLineRenderer();
    }

    public IEnumerator TentDamageAnimation(Unit attacker, AttackType attackType)
    {
        // 0. ANIMATION PLAYS
        GameManager.instance.noOfAnimationsPlaying++;
        // 1. CALCULATE DAMAGE
        int damage = attacker.GetTentDamage();
        // 2. ANIMATION
        // 2.1 UNIT APPROACH
        if (attackType == AttackType.MelleDefence)
        {
            attacker.GetComponent<UnitBehaviour>().SetSuperiorSortingLayer();
            if (attacker.cardSO.GetOwner() == GameManager.instance.playerSouth)
            {
                LeanTween.move(attacker.gameObject, new Vector3(transform.position.x, transform.position.y - 280), 0.5f);
            }
            else
            {
                LeanTween.move(attacker.gameObject, new Vector3(transform.position.x, transform.position.y + 280), 0.5f);
            }
            yield return new WaitForSeconds(0.6f);
        }
        // ANIMATION
        float xPos = owner.playerMoraleText.transform.localPosition.x;
        float yPos = owner.playerMoraleText.transform.localPosition.y;
        int startMorale = owner.playerActMorale;
        for (int i = 0; i < damage; i++)
        {
            LeanTween.moveLocalY(owner.playerMoraleText.gameObject, -82, 0.5f);
            yield return new WaitForSeconds(0.5f);
            owner.playerMoraleText.transform.localPosition = new Vector3(xPos, yPos);
            owner.playerMoraleText.text = (startMorale - (i + 1)).ToString();
        }
        // 3. MORALE REDUCTION
        owner.playerActMorale -= damage;
        // ANIMATION 
        if (attackType == AttackType.MelleDefence && attacker.cardSO.GetDef() > 0)
        {
            LeanTween.moveLocal(attacker.gameObject, new Vector3(0, 0, 0), 0.5f);
            yield return new WaitForSeconds(0.6f);
            // SORTING ORDER
            attacker.GetComponent<UnitBehaviour>().SetStantardSortingLayer();
        }
        // 4. DEACTIVATE UNIT AFTER ATTACK
        attacker.DeactivateAfterTentAttack();
        GameManager.instance.noOfAnimationsPlaying--;
        ////// WAIT UNTIL ALL ANIMATION ENDS
        yield return new WaitUntil(() => !GameManager.instance.IsAnimationPlayig());
        // 5. CHECK END GAME!
        owner.CheckEndGame();
        yield return new WaitForSeconds(0.2f);
        if (GameManager.instance.boolEndGame)
        {
            attacker.isFighting = false;
            GameEvents.current.RefreshUnitVisuals();
            owner.RefreshAttributeVisuals();
        }
        else
        {
            attacker.isFighting = false;
            GameEvents.current.RefreshUnitVisuals();
            owner.RefreshAttributeVisuals();
            GameEvents.current.StartInteraction();
            GameEvents.current.UnBlockZoom();
        }
        
    }

}
