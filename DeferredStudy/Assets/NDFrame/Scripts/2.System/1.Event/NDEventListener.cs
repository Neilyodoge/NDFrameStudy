using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 事件类型
/// </summary>
public enum NDEventType
{ 
    OnMouseEnter,
    OnMouseExit,
    OnClick,
    OnClickDown,
    OnClickUp,
    OnDrag,
    OnBeginDrag,
    OnEndDrag,
    OnCollisionEnter,
    OnCollisionStay,
    OnCollisionExit,
    OnCollisionEnter2D,
    OnCollisionStay2D,
    OnCollisionExit2D,
    OnTriggerEnter,
    OnTriggerStay,
    OnTriggerExit,
    OnTriggerEnter2D,
    OnTriggerStay2D,
    OnTriggerExit2D,
}
// 定义了一个借口，继承所有事件相关的接口
public interface IMouseEvent : IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IPointerDownHandler,
    IPointerUpHandler, IBeginDragHandler/*开始拖拽*/, IEndDragHandler/*结束拖拽*/, IDragHandler/*拖拽中*/{ }


/// <summary>
/// 事件工具
/// 可以添加 鼠标、碰撞、触发等事件
/// </summary>
public class NDEventListener : MonoBehaviour, IMouseEvent
{
    #region 内部类、接口等
    /// <summary>
    /// 某个事件中的一个时间的数据包装类,一次事件
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Pool]
    private class NDEvenetListenerEventInfo<T>
    {
        // T : 事件本身的参数 （PointerEventData，Collision）
        // object[]: 事件的参数
        public Action<T, object[]> action;
        public object[] args;
        public void Init(Action<T, object[]> action, object[] args)
        {
            this.action = action;
            this.args = args;
        }
        public void TriggerEvent(T eventData)
        {
            action?.Invoke(eventData, args);
        }
    }

    interface INDEvenetListenerEventInfos { void RemoveAll(); }   // 用来转化数据的接口

    /// <summary>
    /// 一类事件的数据类型包装 ： 包含多个 NDEvenetListenerEventInfo
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Pool]
    private class NDEvenetListenerEventInfos<T> : INDEvenetListenerEventInfos
    {
        // 所有的事件
        private List<NDEvenetListenerEventInfo<T>> eventList = new List<NDEvenetListenerEventInfo<T>>();
        /// <summary>
        /// 添加事件
        /// </summary>
        public void AddListener(Action<T, object[]> action, params object[] args)
        {
            NDEvenetListenerEventInfo<T> info = ResManager.Instance.Load<NDEvenetListenerEventInfo<T>>();
            info.Init(action, args);
            eventList.Add(info);
        }
        /// <summary>
        /// 移除事件
        /// </summary>
        public void RemoveListener(Action<T, object[]> action, bool checkArgs = false, params object[] args)
        {
            // 这里要包装两层，就可以移除特定的事件了
            for (int i = 0; i < eventList.Count; i++)
            {
                if (eventList[i].action.Equals(action))
                {
                    // 是否需要检查参数
                    if (checkArgs && args.Length > 0)
                    {
                        // 参数如果相等
                        if (args.ArrayEquals(eventList[i].args))
                        {
                            // 移除
                            eventList[i].action.NDObjectPushPool();
                            eventList.RemoveAt(i);
                            return;
                        }
                    }
                    else
                    {
                        // 移除-移除全部action
                        eventList[i].action.NDObjectPushPool();
                        eventList.RemoveAt(i);
                        return;
                    }
                }
            }
        }
        /// <summary>
        /// 移除全部，全部放进对象池
        /// </summary>
        public void RemoveAll()
        {
            for (int i = 0; i < eventList.Count; i++)
            {
                eventList[i].NDObjectPushPool();
            }
            eventList.Clear();
        }
        public void TriggerEvent(T eventData)
        {
            for (int i = 0; i < eventList.Count; i++)
            {
                eventList[i].TriggerEvent(eventData);
            }
        }
    }
    #endregion

    private Dictionary<NDEventType, INDEvenetListenerEventInfos> eventInfoDic = new Dictionary<NDEventType, INDEvenetListenerEventInfos>();
    #region 外部的访问
    /// <summary>
    /// 添加事件
    /// </summary>
    public void AddListener<T>(NDEventType eventType, Action<T, object[]> action, params object[] args)
    {
        if (eventInfoDic.ContainsKey(eventType))
        {
            // 先用as转成派生类
            (eventInfoDic[eventType] as NDEvenetListenerEventInfos<T>).AddListener(action, args);
        }
        else // 字典中没有类型要去添加
        {
            // 因为要从对象池拿，所以用Instance
            NDEvenetListenerEventInfos<T> infos = ResManager.Instance.Load<NDEvenetListenerEventInfos<T>>();
            infos.AddListener(action, args);
            eventInfoDic.Add(eventType, infos);
        }
    }
    /// <summary>
    /// 移除事件
    /// </summary>
    public void RemoveListener<T>(NDEventType eventType, Action<T, object[]> action, bool checkArgs = false, params object[] args)
    {
        if (eventInfoDic.ContainsKey(eventType))
        {
            (eventInfoDic[eventType] as NDEvenetListenerEventInfos<T>).RemoveListener(action, checkArgs, args);
        }
    }
    /// <summary>
    /// 移除某一个事件类型下的全部事件
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="eventType"></param>
    public void RemoveAllListener(NDEventType eventType)
    {
        if (eventInfoDic.ContainsKey(eventType))
        {
            eventInfoDic[eventType].RemoveAll();
        }
    }
    /// <summary>
    /// 移除全部事件
    /// </summary>
    public void RemoveAllListener()
    {
        foreach (INDEvenetListenerEventInfos infos in eventInfoDic.Values)
        {
            infos.RemoveAll();  // 用了上面的接口
        }
        eventInfoDic.Clear();
    }
    #endregion

    /// <summary>
    /// 触发事件
    /// </summary>
    private void TriggerAction<T>(NDEventType eventType, T eventData)
    {
        if (eventInfoDic.ContainsKey(eventType))
        {
            (eventInfoDic[eventType] as NDEvenetListenerEventInfos<T>).TriggerEvent(eventData);
        }
    }


    #region 鼠标事件
    public void OnPointerEnter(PointerEventData eventData)
    {
        TriggerAction(NDEventType.OnMouseEnter, eventData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TriggerAction(NDEventType.OnMouseExit, eventData);
    }


    public void OnBeginDrag(PointerEventData eventData)
    {
        TriggerAction(NDEventType.OnBeginDrag, eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        TriggerAction(NDEventType.OnDrag, eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        TriggerAction(NDEventType.OnEndDrag, eventData);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        TriggerAction(NDEventType.OnClick, eventData);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        TriggerAction(NDEventType.OnClickDown, eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        TriggerAction(NDEventType.OnClickUp, eventData);
    }
    #endregion

    #region 碰撞事件
    private void OnCollisionEnter(Collision collision)
    {
        TriggerAction(NDEventType.OnCollisionEnter, collision);
    }
    private void OnCollisionStay(Collision collision)
    {
        TriggerAction(NDEventType.OnCollisionStay, collision);
    }
    private void OnCollisionExit(Collision collision)
    {
        TriggerAction(NDEventType.OnCollisionExit, collision);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TriggerAction(NDEventType.OnCollisionEnter2D, collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        TriggerAction(NDEventType.OnCollisionStay2D, collision);
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        TriggerAction(NDEventType.OnCollisionExit2D, collision);
    }
    #endregion

    #region 触发事件
    private void OnTriggerEnter(Collider other)
    {
        TriggerAction(NDEventType.OnTriggerEnter, other);
    }
    private void OnTriggerStay(Collider other)
    {
        TriggerAction(NDEventType.OnTriggerStay, other);
    }
    private void OnTriggerExit(Collider other)
    {
        TriggerAction(NDEventType.OnTriggerExit, other);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        TriggerAction(NDEventType.OnTriggerEnter2D, collision);
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        TriggerAction(NDEventType.OnTriggerStay2D, collision);
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        TriggerAction(NDEventType.OnTriggerExit2D, collision);
    }


    #endregion
}
