using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Test : MonoBehaviour
{

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(OnPointerClick);
        GetComponent<Button>().onClick.RemoveListener(OnPointerClick);
    }


    void Click()
    {
        
    }

    public void OnPointerClick()
    {
        Debug.Log("click");
    }
}
