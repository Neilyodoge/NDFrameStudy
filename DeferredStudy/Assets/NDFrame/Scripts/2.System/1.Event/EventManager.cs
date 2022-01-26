using System;
using System.Collections.Generic;

/// <summary>
/// 事件系统管理器
/// </summary>
public static class EventManager
{
    #region 内部接口、内部类
    /// <summary>
    /// 事件信息接口
    /// </summary>
    private interface IEventInfo { };
    /// <summary>
    /// 无参类型
    /// </summary>
    private class EventInfo
    {
        public Action action;
        public void Init(Action action)
        {
            this.action += action;
        }
    }
    /// <summary>
    /// 一个参数
    /// </summary>
    private class EventInfo<T>
    {
        public Action<T> action;
        public void Init(Action<T> action)
        {
            this.action += action;
        }
    }
    /// <summary>
    /// 两个参数
    /// </summary>
    private class EventInfo<T, K>
    {
        public Action<T, K> action;
        public void Init(Action<T,K> action)
        {
            this.action += action;
        }
    }
    /// <summary>
    /// 三个参数
    /// </summary>
    private class EventInfo<T, K, L>
    {
        public Action<T, K, L> action;
        public void Init(Action<T, K, L> action)
        {
            this.action += action;
        }
    }
    #endregion

    private static Dictionary<string, IEventInfo> eventInfoDic = new Dictionary<string, IEventInfo>();

}
