using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public static class NDEventListenerExtend
{
    #region 工具函數
    private static NDEventListener GetOrAddNDEventListener(Component com)
    {
        NDEventListener lis = com.GetComponent<NDEventListener>();
        if (lis == null) return com.gameObject.AddComponent<NDEventListener>();
        else return lis;
    }
    public static void AddEventListener<T>(this Component com, NDEventType eventType, Action<T, object[]> action, params object[] args)
    {
        NDEventListener lis = GetOrAddNDEventListener(com);
        lis.AddListener(eventType, action, args);
    }

    public static void RemoveEventListener<T>(this Component com, NDEventType eventType, Action<T, object[]> action, bool checkArgs = false, params object[] args)
    {
        NDEventListener lis = GetOrAddNDEventListener(com);
        lis.RemoveListener(eventType, action, checkArgs, args);
    }
    public static void RemoveAllListener(this Component com, NDEventType eventType)
    {
        NDEventListener lis = GetOrAddNDEventListener(com);
        lis.RemoveAllListener(eventType);
    }
    public static void RemoveAllListener(this Component com)
    {
        NDEventListener lis = GetOrAddNDEventListener(com);
        lis.RemoveAllListener();
    }
    #endregion

    #region 鼠标相关事件
    public static void OnMouseEnter(this Component com, Action<PointerEventData, object[]> action, params object[] args)
    {
        AddEventListener(com, NDEventType.OnMouseEnter, action, args);
    }
    public static void OnMouseExit(this Component com, Action<PointerEventData, object[]> action, params object[] args)
    {
        AddEventListener(com, NDEventType.OnMouseExit, action, args);
    }
    public static void OnClick(this Component com, Action<PointerEventData, object[]> action, params object[] args)
    {
        AddEventListener(com, NDEventType.OnClick, action, args);
    }
    public static void OnClickDown(this Component com, Action<PointerEventData, object[]> action, params object[] args)
    {
        AddEventListener(com, NDEventType.OnClickDown, action, args);
    }
    public static void OnClickUp(this Component com, Action<PointerEventData, object[]> action, params object[] args)
    {
        AddEventListener(com, NDEventType.OnClickUp, action, args);
    }
    public static void OnDrag(this Component com, Action<PointerEventData, object[]> action, params object[] args)
    {
        AddEventListener(com, NDEventType.OnDrag, action, args);
    }
    public static void OnBeginDrag(this Component com, Action<PointerEventData, object[]> action, params object[] args)
    {
        AddEventListener(com, NDEventType.OnBeginDrag, action, args);
    }
    public static void OnEndDrag(this Component com, Action<PointerEventData, object[]> action, bool checkArgs = false,params object[] args)
    {
        AddEventListener(com, NDEventType.OnEndDrag, action, args);
    }
    public static void RemoveClick(this Component com, Action<PointerEventData, object[]> action, bool checkArgs = false, params object[] args)
    {
        RemoveEventListener(com, NDEventType.OnClick, action, checkArgs, args);
    }
    public static void RemoveClickDown(this Component com, Action<PointerEventData, object[]> action, bool checkArgs = false, params object[] args)
    {
        RemoveEventListener(com, NDEventType.OnClickDown, action, checkArgs, args);
    }
    public static void RemoveClickUp(this Component com, Action<PointerEventData, object[]> action, bool checkArgs = false, params object[] args)
    {
        RemoveEventListener(com, NDEventType.OnClickUp, action, checkArgs, args);
    }
    public static void RemoveDrag(this Component com, Action<PointerEventData, object[]> action, bool checkArgs = false, params object[] args)
    {
        RemoveEventListener(com, NDEventType.OnDrag, action, checkArgs, args);
    }
    public static void RemoveBeginDrag(this Component com, Action<PointerEventData, object[]> action, bool checkArgs = false, params object[] args)
    {
        RemoveEventListener(com, NDEventType.OnBeginDrag, action, checkArgs, args);
    }
    public static void RemoveEndDrag(this Component com, Action<PointerEventData, object[]> action, bool checkArgs = false, params object[] args)
    {
        RemoveEventListener(com, NDEventType.OnEndDrag, action, checkArgs, args);
    }


    #endregion

    #region 碰撞相关事件

    public static void OnCollisionEnter(this Component com, Action<Collision, object[]> action, params object[] args)
    {
        com.AddEventListener(NDEventType.OnCollisionEnter, action, args);
    }


    public static void OnCollisionStay(this Component com, Action<Collision, object[]> action, params object[] args)
    {
        AddEventListener(com, NDEventType.OnCollisionStay, action, args);
    }
    public static void OnCollisionExit(this Component com, Action<Collision, object[]> action, params object[] args)
    {
        AddEventListener(com, NDEventType.OnCollisionExit, action, args);
    }
    public static void OnCollisionEnter2D(this Component com, Action<Collision, object[]> action, params object[] args)
    {
        AddEventListener(com, NDEventType.OnCollisionEnter2D, action, args);
    }
    public static void OnCollisionStay2D(this Component com, Action<Collision, object[]> action, params object[] args)
    {
        AddEventListener(com, NDEventType.OnCollisionStay2D, action, args);
    }
    public static void OnCollisionExit2D(this Component com, Action<Collision, object[]> action, params object[] args)
    {
        AddEventListener(com, NDEventType.OnCollisionExit2D, action, args);
    }
    public static void RemoveCollisionEnter(this Component com, Action<Collision, object[]> action, bool checkArgs = false, params object[] args)
    {
        RemoveEventListener(com, NDEventType.OnCollisionEnter, action, checkArgs, args);
    }
    public static void RemoveCollisionStay(this Component com, Action<Collision, object[]> action, bool checkArgs = false, params object[] args)
    {
        RemoveEventListener(com, NDEventType.OnCollisionStay, action, checkArgs, args);
    }
    public static void RemoveCollisionExit(this Component com, Action<Collision, object[]> action, bool checkArgs = false, params object[] args)
    {
        RemoveEventListener(com, NDEventType.OnCollisionExit, action, checkArgs, args);
    }
    public static void RemoveCollisionEnter2D(this Component com, Action<Collision2D, object[]> action, bool checkArgs = false, params object[] args)
    {
        RemoveEventListener(com, NDEventType.OnCollisionEnter2D, action, checkArgs, args);
    }
    public static void RemoveCollisionStay2D(this Component com, Action<Collision2D, object[]> action, bool checkArgs = false, params object[] args)
    {
        RemoveEventListener(com, NDEventType.OnCollisionStay2D, action, checkArgs, args);
    }
    public static void RemoveCollisionExit2D(this Component com, Action<Collision2D, object[]> action, bool checkArgs = false, params object[] args)
    {
        RemoveEventListener(com, NDEventType.OnCollisionExit2D, action, checkArgs, args);
    }
    #endregion

    #region 触发相关事件
    public static void OnTriggerEnter(this Component com, Action<Collider, object[]> action, bool checkArgs = false, params object[] args)
    {
        AddEventListener(com, NDEventType.OnTriggerEnter, action, checkArgs, args);
    }
    public static void OnTriggerStay(this Component com, Action<Collider, object[]> action, bool checkArgs = false, params object[] args)
    {
        AddEventListener(com, NDEventType.OnTriggerStay, action, checkArgs, args);
    }
    public static void OnTriggerExit(this Component com, Action<Collider, object[]> action, bool checkArgs = false, params object[] args)
    {
        AddEventListener(com, NDEventType.OnTriggerExit, action, checkArgs, args);
    }
    public static void OnTriggerEnter2D(this Component com, Action<Collider, object[]> action, bool checkArgs = false, params object[] args)
    {
        AddEventListener(com, NDEventType.OnTriggerEnter2D, action, checkArgs, args);
    }
    public static void OnTriggerStay2D(this Component com, Action<Collider, object[]> action, bool checkArgs = false, params object[] args)
    {
        AddEventListener(com, NDEventType.OnTriggerStay2D, action, checkArgs, args);
    }
    public static void OnTriggerExit2D(this Component com, Action<Collider, object[]> action, bool checkArgs = false, params object[] args)
    {
        AddEventListener(com, NDEventType.OnTriggerExit2D, action, checkArgs, args);
    }
    public static void RemoveTriggerEnter(this Component com, Action<Collider, object[]> action, bool checkArgs = false, params object[] args)
    {
        RemoveEventListener(com, NDEventType.OnTriggerEnter, action, checkArgs, args);
    }
    public static void RemoveTriggerStay(this Component com, Action<Collider, object[]> action, bool checkArgs = false, params object[] args)
    {
        RemoveEventListener(com, NDEventType.OnTriggerStay, action, checkArgs, args);
    }
    public static void RemoveTriggerExit(this Component com, Action<Collider, object[]> action, bool checkArgs = false, params object[] args)
    {
        RemoveEventListener(com, NDEventType.OnTriggerExit, action, checkArgs, args);
    }
    public static void RemoveTriggerEnter2D(this Component com, Action<Collider2D, object[]> action, bool checkArgs = false, params object[] args)
    {
        RemoveEventListener(com, NDEventType.OnTriggerEnter2D, action, checkArgs, args);
    }
    public static void RemoveTriggerStay2D(this Component com, Action<Collider2D, object[]> action, bool checkArgs = false, params object[] args)
    {
        RemoveEventListener(com, NDEventType.OnTriggerStay2D, action, checkArgs, args);
    }
    public static void RemoveTriggerExit2D(this Component com, Action<Collider2D, object[]> action, bool checkArgs = false, params object[] args)
    {
        RemoveEventListener(com, NDEventType.OnTriggerExit2D, action, checkArgs, args);
    }
    #endregion

}
