using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopUpWindow : MonoBehaviour
{
    public TMPro.TMP_Text information;
    public Button oKbutton;

    //private Action callback;

    //public void SetupCallBack()
    //{
    //    OKbutton.onClick.AddListener(()=> 
    //    { 
    //        callback?.Invoke(); 
    //        gameObject.SetActive(false);
    //    });
    //}

    public void GenerateWindow(string text, Action okAction)
    {
        information.text = text;
        oKbutton.onClick.AddListener(() =>
        {
            gameObject.SetActive(false);
            okAction();
        });
        gameObject.SetActive(true);
    }
    
}
