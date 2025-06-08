using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardSpecAbSlot : MonoBehaviour
{
    public TMP_Text specAbNameText;
    public TMP_Text descriptionText;
    public TMP_Text ammoText;
    public Image specAbImage;
    public Transform ammoPanel;

    public GameObject ammoDotPrefab;

    public Sprite ammoE;
    public Sprite ammoF;

    public void SetCardSpecAbSlot(SpecialAbilitySO specAb)
    {
        specAbNameText.text = specAb.specAbName;
        descriptionText.text = specAb.specAbDesc;
        if (specAb.maxAmmo > 0)
        {
            ClearAmmoPanel();
            ammoText.gameObject.SetActive(true);
            SetAmmoPanel(specAb);
        }
        else
        {
            ClearAmmoPanel();
            ammoText.gameObject.SetActive(false);
            
        }
        specAbImage.sprite = specAb.specAbIcon;

    }

    private void ClearAmmoPanel()
    {
        if (ammoPanel.childCount > 0)
        {
            foreach (Transform ammoDot in ammoPanel)
            {
                Destroy(ammoDot.gameObject);
            }
        }
    }

    private void SetAmmoPanel(SpecialAbilitySO specAb)
    {
        for (int i = 0; i < specAb.maxAmmo; i++)
        {
            GameObject ammoDot = Instantiate(ammoDotPrefab, ammoPanel);
            ammoDot.GetComponent<Image>().sprite = ammoF;
        }
    }
}
