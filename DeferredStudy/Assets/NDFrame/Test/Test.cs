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

    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            AudioManager.Instance.PlayOnShot("cannon_01",Camera.main,1,true, CallBack, 2);
        }
    }
    void CallBack()
    {
        Debug.Log("callback");
    }


}
