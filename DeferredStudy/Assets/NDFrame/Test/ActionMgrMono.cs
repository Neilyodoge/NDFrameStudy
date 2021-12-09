using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Koko.Game.Tool
{
    public class ActionMgrMono : MonoBehaviour
    {
        public static ActionMgrMono Ins;
        private static bool _inited = false;
        public static string LogTag = "----Unity----KoKoGameTool---ActionMgr---";

        private void Awake()
        {
            Ins = this;
        }

        public static void Init()
        {
            if (_inited) return;
            _inited = true;
            var go = new GameObject("ActionMgr", typeof(ActionMgrMono));
            DontDestroyOnLoad(go);
        }
    }

    public static class ActionMgr
    {
        //private static List<ActionCell_Nomal> nomallist = new List<ActionCell_Nomal>(10);
        private static List<NormalProperty> nomallist = new List<NormalProperty>(10);
        private static List<SequenceActionCell> Sequencelist = new List<SequenceActionCell>(10);
        private static Dictionary<Guid, NormalProperty> DicPropertNormal = new Dictionary<Guid, NormalProperty>(10);

        private static Dictionary<Guid, SequenceActionCell> DicPropertSequence =
            new Dictionary<Guid, SequenceActionCell>(10);

        /// <summary>
        /// 循环执行事件至指定条件
        /// </summary>
        /// <param name="mainaction">主事件</param>
        /// <param name="interval">循环时间间隔</param>
        /// <param name="breakcheck">跳出条件,不设条件为死循环，条件为真跳出</param>
        public static NormalCell InvokeUntilCheckBreak(Action mainaction, float interval, Func<bool> breakcheck)
        {
            var guid = Guid.NewGuid();
            NormalCell normalCell;
            foreach (var NormalCellInPool in nomallist.Where(NormalCellInPool => !NormalCellInPool.ActiveState))
            {
                NormalCellInPool.SetData(mainaction, interval, breakcheck);
                DicPropertNormal.Add(guid, NormalCellInPool);
                normalCell = new NormalCell(guid);

                NormalCellInPool.Invoke();
                return normalCell;
            }

            var NormalCell = new NormalProperty();
            NormalCell.SetData(mainaction, interval, breakcheck);
            DicPropertNormal.Add(guid, NormalCell);
            normalCell = new NormalCell(guid);
            nomallist.Add(NormalCell);
            NormalCell.Invoke();
            return normalCell;
        }
        
        /// <summary>
        /// 循环事件至指定次数
        /// </summary>
        /// <param name="mainaction">主事件</param>
        /// <param name="interval">循环时间间隔</param>
        /// <param name="Times">指定循环次数</param>
        /// <returns></returns>
        public static NormalCell InvokeUntilTimes(Action mainaction, float interval, int Times)
        {
            var guid = Guid.NewGuid();
            NormalCell normalCell;
            foreach (var NormalCellInPool in nomallist.Where(NormalCellInPool => !NormalCellInPool.ActiveState))
            {
                NormalCellInPool.SetData(mainaction, interval, () => NormalCellInPool.LoopTime >= Times);
                DicPropertNormal.Add(guid, NormalCellInPool);
                normalCell = new NormalCell(guid);

                NormalCellInPool.Invoke();
                return normalCell;
            }

            var NormalCell = new NormalProperty();
            NormalCell.SetData(mainaction, interval, () => NormalCell.LoopTime >= Times);
            DicPropertNormal.Add(guid, NormalCell);
            normalCell = new NormalCell(guid);
            nomallist.Add(NormalCell);
            NormalCell.Invoke();
            return normalCell;
        }

        /// <summary>
        /// 事件队列
        /// </summary>
        /// <param name="interval"> 队列中事件执行间隔，默认为0，即一帧执行一条 </param>
        /// <returns> SequenceActionCell队列 </returns>
        public static SequenceCell Sequence(float interval = 0)
        {
            var guid = Guid.NewGuid();
            SequenceCell sequenceCell;
            foreach (var sequenceCellInPool in Sequencelist.Where(sequenceCellInPool => !sequenceCellInPool.ActiveState)
            )
            {
                sequenceCellInPool.SetData(interval);
                DicPropertSequence.Add(guid, sequenceCellInPool);
                sequenceCell = new SequenceCell(guid);
                sequenceCellInPool.Invoke();
                return sequenceCell;
            }

            var temp = new SequenceActionCell();
            temp.SetData(interval);
            Sequencelist.Add(temp);
            DicPropertSequence.Add(guid, temp);
            sequenceCell = new SequenceCell(guid);
            temp.Invoke();
            return sequenceCell;
        }

        public struct NormalCell
        {
            private readonly Guid guid;

            public NormalCell(Guid guid)
            {
                this.guid = guid;
            }

            private NormalProperty property
            {
                get
                {
                    DicPropertNormal.TryGetValue(guid, out var _ActionCell);
                    return _ActionCell;
                }
            }

            public NormalCell SetDelay(float  delayTime)
            {
                property?.SetDelay(delayTime);
                return this;
            }
            
            public NormalCell OnComplete(Action action)
            {
                property?.OnComplete(action);
                return this;
            }

            public NormalCell Kill()
            {
                property?.Kill();
                return this;
            }

            public NormalCell Stop()
            {
                property?.Stop();
                return this;
            }

            public NormalCell Pause()
            {
                property?.Pause();
                return this;
            }

            public NormalCell Continue()
            {
                property?.Continue();
                return this;
            }
        }


        public struct SequenceCell
        {
            private readonly Guid guid;

            public SequenceCell(Guid guid)
            {
                this.guid = guid;
            }

            private SequenceActionCell property
            {
                get
                {
                    DicPropertSequence.TryGetValue(guid, out var _ActionCell);
                    return _ActionCell;
                }
            }

            public SequenceCell SetDelay(float delayTime)
            {
                property?.SetDelay(delayTime);
                return this;
            }
            
            public SequenceCell OnComplete(Action action)
            {
                property?.OnComplete(action);
                return this;
            }

            public SequenceCell OnStepComplete(Action action)
            {
                property?.OnStepComplete(action);
                return this;
            }

            public SequenceCell Kill()
            {
                property?.Kill();
                return this;
            }

            public SequenceCell Stop()
            {
                property?.Stop();
                return this;
            }

            public SequenceCell Pause()
            {
                property?.Pause();
                return this;
            }

            public SequenceCell Continue()
            {
                property?.Continue();
                return this;
            }

            public SequenceCell Append(Action action)
            {
                property?.Append(action);
                return this;
            }
        }


        private abstract class ActionCell
        {
            #region PublicProperty

            public bool ActiveState { get; protected set; }

            public int LoopTime { get; protected set; }

            public Coroutine MonoCoroutine { get; protected set; }

            public Action action_main { get; protected set; }

            public Func<bool> func_IsCanBreak { get; protected set; }

            public Action action_complete = null;

            public float DelayTime = 0;

            #endregion

            #region ProtectedProperty

            protected float IntervalTime;

            protected bool PauseState = false;

            #endregion

            #region PublicVirtualMethod

            /// <summary>
            /// 激活事件对象
            /// </summary>
            public virtual void Invoke()
            {
                action_main?.Invoke();
            }

            /// <summary>
            /// 杀死事件对象,不执行完成回调,并释放所占用事件对象
            /// </summary>
            public virtual void Kill()
            {
                ClearDic();
                ReSetProperty();
            }

            /// <summary>
            /// 终止事件对象,执行完成回调,并释放所占用事件对象
            /// </summary>
            public virtual void Stop()
            {
                ClearDic();
                action_complete?.Invoke();
                ReSetProperty();
            }

            /// <summary>
            /// 暂停事件对象
            /// </summary>
            public virtual void Pause()
            {
                if (PauseState) return;
                PauseState = true;
            }

            /// <summary>
            /// 继续事件对象
            /// </summary>
            public virtual void Continue()
            {
                if (!PauseState) return;
                PauseState = false;
            }

            #endregion

            #region ProtectedVirtualMethod

            /// <summary>
            /// 释放事件对象占用
            /// </summary>
            protected virtual void ClearDic()
            {
                foreach (var keyValue in DicPropertNormal.Where(keyValue => keyValue.Value == this))
                {
                    DicPropertNormal.Remove(keyValue.Key);
                }
            }

            /// <summary>
            /// 重置事件对象属性
            /// </summary>
            protected virtual void ReSetProperty()
            {
                ActiveState = false;
                LoopTime = 0;
                MonoCoroutine = null;
                action_main = null;
                func_IsCanBreak = null;
                action_complete = null;
                DelayTime = 0;
                IntervalTime = 0;
                PauseState = false;
            }

            #endregion
        }

        private class NormalProperty : ActionCell
        {
            public void SetData(Action mainaction, float interval, Func<bool> breakcheck)
            {
                ReSetProperty();
                action_main = mainaction;
                IntervalTime = interval;
                func_IsCanBreak = breakcheck;
            }

            public override void Kill()
            {
                if (!ActiveState)
                {
                    return;
                }

                if (MonoCoroutine == null)
                {
                    return;
                }

                ActionMgrMono.Ins.StopCoroutine(MonoCoroutine);
                ClearDic();
                ReSetProperty();
            }

            public override void Stop()
            {
                if (!ActiveState)
                {
                    return;
                }

                if (MonoCoroutine == null)
                { 
                    return;
                }

                ActionMgrMono.Ins.StopCoroutine(MonoCoroutine);
                action_complete?.Invoke();
                ClearDic();
                ReSetProperty();
            }

            public override void Invoke()
            {
                if (ActiveState)
                {
                    return;
                }
                ActiveState = true;
                MonoCoroutine = ActionMgrMono.Ins.StartCoroutine(LoopMain());
            }

            private IEnumerator LoopMain()
            {
                while (PauseState)
                {
                    yield return null;
                }
                yield return null;

                if (DelayTime>0)
                {
                    var DelayBase = Time.time;
                    var DelayCount = DelayBase + DelayTime;
                    while (DelayCount > DelayBase)
                    {
                        while (PauseState)
                        {
                            yield return null;
                        }
                        DelayCount -= Time.deltaTime;
                        yield return null;
                    }
                }
                while (func_IsCanBreak == null || !func_IsCanBreak.Invoke())
                {
                    action_main?.Invoke();
                    LoopTime++;
                    if (IntervalTime == 0)
                    {
                        yield return null;
                    }
                    else
                    {
                        var IntervalBase = Time.time;
                        var IntervalCount = IntervalBase + IntervalTime;
                        while (IntervalCount > IntervalBase)
                        {
                            while (PauseState)
                            {
                                yield return null;
                            }
                            IntervalCount -= Time.deltaTime;
                            yield return null;
                        }
                    }
                }

                action_complete?.Invoke();
                ClearDic();
                ReSetProperty();
            }
        }


        private class SequenceActionCell : ActionCell
        {
            private List<ActionCell> ActionCellInSequence = new List<ActionCell>();
            private List<Action> ActionsNeedExecute = new List<Action>();
            public Action action_stepAction;
            protected override void ClearDic()
            {
                foreach (var keyValue in DicPropertSequence.Where(keyValue => keyValue.Value == this))
                {
                    DicPropertNormal.Remove(keyValue.Key);
                }
            }
            public void SetData(float Interval)
            {
                IntervalTime = Interval;
            }
            protected override void ReSetProperty()
            {
                base.ReSetProperty();
                action_stepAction = null;
                ActionsNeedExecute = new List<Action>();
                ActionCellInSequence = new List<ActionCell>();
            }

            public override void Continue()
            {
                if (!PauseState)
                { 
                    return;
                }

                PauseState = false;
                foreach (var NormalCell in ActionCellInSequence)
                {
                    NormalCell.Continue();
                }
            }

            public override void Pause()
            {
                if (PauseState)
                {
                    return;
                }

                PauseState = true;
                foreach (var NormalCell in ActionCellInSequence)
                {
                    NormalCell.Pause();
                }
            }

            public override void Kill()
            {
                if (!ActiveState)
                {
                    return;
                }

                if (MonoCoroutine == null)
                { 
                    return;
                }

                ActionMgrMono.Ins.StopCoroutine(MonoCoroutine);
                ClearDic();
                ReSetProperty();
            }

            public override void Stop()
            {
                if (!ActiveState)
                {
                    return;
                }

                if (MonoCoroutine == null)
                {
                 
                    return;
                }

                ActionMgrMono.Ins.StopCoroutine(MonoCoroutine);
                action_complete?.Invoke();
                ClearDic();
                ReSetProperty();
            }

            public override void Invoke()
            {
                if (ActiveState)
                {
                    return;
                }
                ActiveState = true;
                MonoCoroutine = ActionMgrMono.Ins.StartCoroutine(SequenceMain());
            }

            public SequenceActionCell Append(Action action)
            {
                ActionsNeedExecute.Add(action);
                return this;
            }
            
            private IEnumerator SequenceMain()
            {
                while (PauseState)
                {
                    yield return null;
                }

                yield return null;
                if (DelayTime > 0)
                {
                    var DelayBase = Time.time;
                    var DelayCount = DelayBase + DelayTime;
                    while (DelayCount > DelayBase)
                    {
                        while (PauseState)
                        {
                            yield return null;
                        }
                        DelayCount -= Time.deltaTime;
                        yield return null;
                    }
                }

                while (true)
                {
                    while (ActionsNeedExecute.Count <= 0)
                    {
                        yield return null;
                    }
                    Debug.LogError($"SequenceDo");
                    ActionsNeedExecute[0]?.Invoke();
                    ActionsNeedExecute.RemoveAt(0);
                    action_stepAction?.Invoke();
                    if (IntervalTime == 0)
                    {
                        yield return null;
                    }
                    else
                    {
                        var IntervalBase = Time.time;
                        var IntervalCount = IntervalBase+ IntervalTime;
                        while ( IntervalCount>IntervalBase)
                        {
                   
                            while (PauseState)
                            {
                                yield return null;
                            }

                            IntervalCount -= Time.deltaTime;
                            yield return null;
                        }
                    }
                }
            }
        }

        #region ExtentionMethod

        private static SequenceActionCell OnStepComplete(this SequenceActionCell SequenceActionCell, Action action)
        {
            SequenceActionCell.action_stepAction = action;
            return SequenceActionCell;
        }

        private static T OnComplete<T>(this T ActionCell, Action action) where T : ActionCell
        {
            ActionCell.action_complete = action;
            return ActionCell;
        }

        private static T SetDelay<T>(this T ActionCell, float delayTime) where T : ActionCell
        {
            ActionCell.DelayTime = delayTime;
            return ActionCell;
        }

        #endregion
    }
}