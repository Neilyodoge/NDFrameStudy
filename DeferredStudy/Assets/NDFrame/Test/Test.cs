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
        // EventManager.AddEventListener("TestEvent", TestFunction);
       /* EventManager.RemoveEventListener("TestEvent", TestFunction);
        EventManager.AddEventListener("TestEvent", TestFunction);*/

    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            EventManager.AddEventListener("TestEvent", TestFunction);
            //EventManager.AddEventListener<string>("TestEvent2", TestFunction2);
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            EventManager.EventTrigger("TestEvent");
            //EventManager.EventTrigger("TestEvent2");
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            EventManager.RemoveEventListener("TestEvent", TestFunction);
            //EventManager.RemoveEventListener<string>("TestEvent2", TestFunction2);
        }
        if(Input.GetKeyDown(KeyCode.D))
        {
            EventManager.Clear();
        }
    }
    private void TestFunction()
    {
        Debug.Log("TestFun");
    }
    private void TestFunction2(string str)
    {
        Debug.Log("TestFun2");
    }

}
