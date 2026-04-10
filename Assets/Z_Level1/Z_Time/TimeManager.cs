using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Z_DesignStyle;

namespace Z_Time
{
    [DefaultExecutionOrder(1000)]
    public class TimeManager : Z_MonoManager<TimeManager>
    {
        static List<(Action, GameObject)> NextFrameList = new List<(Action, GameObject)>();
        static List<(Action, GameObject)> CurLateUpdateList = new List<(Action, GameObject)>();
        static List<Action> CurLateUpdateWithoutCheckList = new List<Action>();
        static List<Action> CurFrameEndWithoutCheckList = new List<Action>();
        static List<(Action, GameObject)> NextBigFrameList = new List<(Action, GameObject)>();
        static List<(Action, GameObject)> CurFrameEndList = new List<(Action, GameObject)>();
        static List<Action> NextUpdateWithoutCheckList = new List<Action>();
        static List<(Action, GameObject)> NextUpdateList = new List<(Action, GameObject)>();

        static List<(Action, GameObject)> NextFixedFrameList = new List<(Action, GameObject)>();
        static string queueLock = "queue";
        int id = 0;

        public Timer StartTimer(float delay, float interval, Func<bool> func, MonoBehaviour bind = null)
        {
            Timer timer = new Timer()
            {
                bind = bind == null ? this : bind
            };
            id++;
            StartCoroutine(Work(id, timer, delay, interval, func));
            return timer;
        }
        public Timer StartTimerImmediate(float delay, float interval, Func<bool> func, MonoBehaviour bind = null)
        {
            var timer = StartTimer(delay, interval, func);
            func();
            return timer;
        }
        public void CancelTimer(Timer timer)
        {
            if (timer == null)
                return;
            timer.cancel = true;
        }

        private IEnumerator Work(int id, Timer timer, float delay, float interval, Func<bool> func)
        {
            yield return new WaitForSeconds(delay);
            while (true)
            {
                yield return new WaitForSeconds(interval);
                if (timer.cancel || timer.bind == null || timer.bind.gameObject == null || !timer.bind.gameObject.activeInHierarchy)
                    break;

                if (func())
                {
                    break;
                }
            }
        }
        public void AddNextBigFrameAction(Action act, GameObject ins)
        {
            NextBigFrameList.Add((act, ins));
        }
        public void AddCurLateUpdateAction(Action act, GameObject ins)
        {
            CurLateUpdateList.Add((act, ins));
        }
        public void AddCurLateUpdateWithoutCheckAction(Action act)
        {
            CurLateUpdateWithoutCheckList.Add(act);
        }
        public void AddCurFrameEndWithoutCheckAction(Action act)
        {
            CurFrameEndWithoutCheckList.Add(act);
        }
        public void AddCurFrameEndAction(Action act, GameObject ins)
        {
            CurFrameEndList.Add((act, ins));
        }

        public void AddNextUpdateWithoutCheckAction(Action act)
        {
            lock (queueLock)
            {

                NextUpdateWithoutCheckList.Add(act);
            }
        }
        public void AddNextUpdateAction(Action act, GameObject ins)
        {
            lock (queueLock)
            {

                NextUpdateList.Add((act, ins));
            }
        }
        public void LateUpdate()
        {
            foreach (var act in CurLateUpdateWithoutCheckList)
            {
                act?.Invoke();
            }
            CurLateUpdateWithoutCheckList.Clear();
            foreach (var act in CurLateUpdateList)
            {
                if (act.Item2 != null)
                {
                    act.Item1?.Invoke();
                }
            }
            CurLateUpdateList.Clear();
        }
        public void Awake()
        {
            StartCoroutine(AtFrameEnd());
        }

        public void Update()
        {
            foreach (var act in NextBigFrameList)
            {
                NextFixedFrameList.Add(act);
            }
            NextBigFrameList.Clear();



            foreach (var act in NextFrameList)
            {
                if (act.Item2 != null)
                {

                    act.Item1?.Invoke();
                }
            }
            NextFrameList.Clear();

            lock (queueLock)
            {
                foreach (var act in NextUpdateWithoutCheckList)
                {
                    act?.Invoke();
                }
                NextUpdateWithoutCheckList.Clear();
                foreach (var act in NextUpdateList)
                {
                    if (act.Item2 != null)
                    {

                        act.Item1?.Invoke();
                    }
                }
                NextUpdateList.Clear();

            }
        }
        public void FixedUpdate()
        {
            foreach (var act in NextBigFrameList)
            {

                NextFrameList.Add(act);
            }
            NextBigFrameList.Clear();
            foreach (var act in NextFixedFrameList)
            {
                if (act.Item2 != null)
                {

                    act.Item1?.Invoke();
                }
            }
            NextFixedFrameList.Clear();

        }
        IEnumerator AtFrameEnd()
        {
            while (true)
            {
                yield return new WaitForEndOfFrame(); // 等待帧结束
                foreach (var act in CurFrameEndWithoutCheckList)
                {
                    act?.Invoke();
                }
                CurFrameEndWithoutCheckList.Clear();
                foreach (var act2 in CurFrameEndList)
                {
                    if (act2.Item2 != null)
                        act2.Item1?.Invoke();
                }
                CurFrameEndList.Clear();


            }
        }
    }
}
