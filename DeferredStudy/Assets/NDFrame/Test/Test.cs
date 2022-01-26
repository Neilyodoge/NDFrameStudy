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
        this.OnClick(Click);
        this.RemoveAllListener(NDEventType.OnClick);
        PoolManager.Instance.OnClick(Click);
    }

    void Click(PointerEventData data,params object[] args)
    {
        
    }

}
