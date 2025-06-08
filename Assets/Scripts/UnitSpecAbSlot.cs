using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitSpecAbSlot : MonoBehaviour
{
    public TMP_Text specAbName;
    private string specAbNameTemp;
    public TMP_Text description;
    public Image specAbImage;
    public Image inactiveImage;
    public Transform ammoGrid;
    public TMP_Text ammoText;
    public TMP_Text turnsText;
    public TMP_Text turnsNo;
    //public TMP_Text attacksText;
    //public TMP_Text attacksNo;

    public GameObject ammoPrefab;

    public Sprite emptyAmmo;
    public Sprite fullAmmo;

    public void SetSpecAb(SpecialAbilitySO specAb)
    {
        specAbName.text = specAb.specAbName;
        specAbNameTemp = specAb.specAbName;
        description.text = specAb.specAbDesc;
        specAbImage.sprite = specAb.specAbIcon;
        // ACTIVE
        if (!specAb.specAbIsActive)
        {
            SetToInactive();
        }
        else
        {
            SetToActive();
        }
        // AMMO
        SetAmmoImage(specAb);
        // DURATION
        SetDuration(specAb);
    }

    public void SetDuration(SpecialAbilitySO specAb)
    {
        //if (specAb.effectAttackDuration > 0)
        //{
        //    attacksText.gameObject.SetActive(true);
        //    attacksNo.gameObject.SetActive(true);
        //    attacksNo.text = specAb.effectAttackDuration.ToString();
        //}
        //else
        //{
        //    attacksText.gameObject.SetActive(false);
        //    attacksNo.gameObject.SetActive(false);
        //}
        if (specAb.effectTurnDuration > 0)
        {
            turnsText.gameObject.SetActive(true);
            turnsNo.gameObject.SetActive(true);
            turnsNo.text = specAb.effectTurnDuration.ToString();
        }
        else
        {
            turnsText.gameObject.SetActive(false);
            turnsNo.gameObject.SetActive(false);
        }
    }

    public void SetAmmoImage(SpecialAbilitySO specAb)
    {
        // AMMO
        ResetAmmoGrid();
        if (specAb.maxAmmo > 0)
        {
            ammoText.gameObject.SetActive(true);
        }
        else
        {
            ammoText.gameObject.SetActive(false);
        }
        for (int i = 0; i < specAb.maxAmmo; i++)
        {
            GameObject ammo = Instantiate(ammoPrefab, ammoGrid);
            ammo.GetComponent<Image>().sprite = emptyAmmo;
        }
        int loadedCounter = 0;
        foreach (Transform emptyAmmo in ammoGrid)
        {
            loadedCounter++;
            if (loadedCounter <= specAb.actAmmo)
            {
                emptyAmmo.GetComponent<Image>().sprite = fullAmmo;
            }
        }
    }

    private void ResetAmmoGrid()
    {
        if (ammoGrid.childCount > 0)
        {
            foreach (Transform t in ammoGrid)
            {
                Destroy(t);
            }
        }
    }


    public void SetToInactive()
    {
        inactiveImage.color = new Color32(0, 0, 0, 150);
        specAbName.text = specAbNameTemp + " (inactive)";
    }

    public void SetToActive()
    {
        inactiveImage.color = new Color32(0, 0, 0, 0);
        specAbName.text = specAbNameTemp;
    }

    public void SetAmmo(int actAmmo, int maxAmmo)
    {

    }
}
