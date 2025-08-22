using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Z_DesignStyle;

namespace Z_Time
{

    public class TimeManager : Z_MonoManager<TimeManager>
    {
        public static List<(Action, GameObject)> NextFrameList = new List<(Action, GameObject)>();
        public static List<(Action, GameObject)> NextBigFrameList = new List<(Action, GameObject)>();
        public static List<Action> NextUpdateWithoutCheckList = new List<Action>();
        public static List<(Action, GameObject)> NextFixedFrameList = new List<(Action, GameObject)>();
        public static string queueLock = "queue";
        public Timer StartTimer(float delay,float interval, Func<bool> func, MonoBehaviour bind = null)
        {
            Timer timer = new Timer() {
                bind = bind == null ? this : bind
            };
            StartCoroutine(Work(timer, delay,interval, func));
            return timer;
        }
        public void CancelTimer(Timer timer)
        {
            if (timer == null)
                return;
            timer.cancel = true;
        }
        private IEnumerator Work(Timer timer,float delay,float interval, Func<bool> func)
        {
            yield return new WaitForSeconds(delay);
            while (true)
            {
                yield return new WaitForSeconds(interval);
                if (timer.cancel|| timer.bind ==null|| timer.bind.gameObject==null||!timer.bind.gameObject.activeInHierarchy || func())
                    break;
            }

        }
        public void AddNextBigFrameAction(Action act,GameObject ins)
        {
            NextBigFrameList.Add((act, ins));
        }
        public void AddNextUpdateWithoutCheckList(Action act)
        {
            lock(queueLock)
            {

                NextUpdateWithoutCheckList.Add(act);
            }
        }

        public void Update()
        {
            foreach(var act in NextBigFrameList)
            {
                NextFixedFrameList.Add(act);
            }
            NextBigFrameList.Clear();



            foreach (var act in NextFrameList)
            {
                if(act.Item2!=null)
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
    }
}
