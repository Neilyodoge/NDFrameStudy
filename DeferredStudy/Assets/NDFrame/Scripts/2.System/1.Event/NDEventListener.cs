using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

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

    /// <summary>
    /// 一类事件的数据类型包装 ： 包含多个 NDEvenetListenerEventInfo
    /// </summary>
    /// <typeparam name="T"></typeparam>
    private class NDEvenetListenerEventInfos<T>
    {
        // 所有的事件
        private List<NDEvenetListenerEventInfo<T>> eventList = new List<NDEvenetListenerEventInfo<T>>();
        /// <summary>
        /// 添加事件
        /// </summary>
        public void AddLisenter(Action<T, object[]> action, params object[] args)
        {
            NDEvenetListenerEventInfo<T> info = ResManager.Instance.Load<NDEvenetListenerEventInfo<T>>();
            info.Init(action, args);
            eventList.Add(info);
        }
        /// <summary>
        /// 移除事件
        /// </summary>
        public void RemoveListener(Action<T, object> action, bool checkArgs = false, params object[] args)
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

    #region 鼠标事件
    public void OnBeginDrag(PointerEventData eventData)
    {
        throw new System.NotImplementedException();
    }

    public void OnDrag(PointerEventData eventData)
    {
        throw new System.NotImplementedException();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        throw new System.NotImplementedException();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        throw new System.NotImplementedException();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        throw new System.NotImplementedException();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        throw new System.NotImplementedException();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        throw new System.NotImplementedException();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        throw new System.NotImplementedException();
    }
    #endregion
    #region 碰撞事件
    //3D
    private void OnCollisionEnter(Collision collision)
    {

    }
    private void OnCollisionExit(Collision collision)
    {
        
    }
    private void OnCollisionStay(Collision collision)
    {
        
    }
    //2D
    private void OnCollisionEnter2D(Collision2D collision)
    {
        
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        
    }
    private void OnCollisionStay2D(Collision2D collision)
    {
        
    }
    #endregion
    #region 触发事件
    private void OnTriggerEnter(Collider other)
    {
        
    }
    private void OnTriggerExit(Collider other)
    {
        
    }
    private void OnTriggerStay(Collider other)
    {
        
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        
    }
    #endregion
}
