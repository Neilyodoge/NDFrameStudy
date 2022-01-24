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
        CubeController cube = ResManager.Instance.Load<CubeController>("Cube");
        cube.OnClick(Click,123,"AA",1.25f);
        cube.RemoveAllListener(NDEventType.OnClick);
    }

    void Click(PointerEventData data,params object[] args)
    {
        Debug.Log("鼠标点击位置"+ data.position);
        Debug.Log("参数个数 "+ args.Length);
        Debug.Log(args[0]);
    }

}
