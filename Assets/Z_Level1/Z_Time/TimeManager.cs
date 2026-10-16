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
        public const float MAX_DELTA_TIME = 0.02f;

        /// <summary>Discard stall time; use unscaled time only for pause-independent UI.</summary>
        public static float GetDeltaTime(bool unscaled = false)
        {
            return ClampDeltaTime(unscaled ? Time.unscaledDeltaTime : Time.deltaTime);
        }

        public static float ClampDeltaTime(float deltaTime)
        {
            return float.IsNaN(deltaTime) ? 0f : Mathf.Clamp(deltaTime, 0f, MAX_DELTA_TIME);
        }

        // Higher-level runtimes can provide a persistent gameplay clock without
        // making this assembly depend on their save/progress data types.
        public static Func<float> animationTimeGetter;

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

        public static float GetAnimationTime()
        {
            float animationTime = animationTimeGetter == null ? Time.time : animationTimeGetter();
            if (float.IsNaN(animationTime) || float.IsInfinity(animationTime))
                return Time.time;
            return Mathf.Max(0, animationTime);
        }

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

        /// <summary>
        /// Runs on boundaries of the shared animation clock. Unlike StartTimer,
        /// the phase does not begin when the caller appears or registers.
        /// </summary>
        public Timer StartAnimationTimer(float interval, Func<bool> func, MonoBehaviour bind = null)
        {
            Timer timer = new Timer()
            {
                bind = bind == null ? this : bind
            };
            id++;
            StartCoroutine(WorkAnimationTimer(id, timer, interval, func));
            return timer;
        }
        public void CancelTimer(Timer timer)
        {
            if (timer == null)
                return;
            timer.cancel = true;
        }

        // Variable-duration GIF frames cannot use the fixed-interval scheduler.
        public Timer StartFrameTimer(Func<bool> func, MonoBehaviour bind = null)
        {
            var timer = new Timer { bind = bind == null ? this : bind };
            StartCoroutine(WorkFrameTimer(timer, func));
            return timer;
        }

        private IEnumerator WorkFrameTimer(Timer timer, Func<bool> func)
        {
            while (true)
            {
                yield return null;
                if (timer.cancel || timer.bind == null || !timer.bind.gameObject.activeInHierarchy || func())
                    yield break;
            }
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

        private IEnumerator WorkAnimationTimer(int id, Timer timer, float interval, Func<bool> func)
        {
            if (interval <= 0)
                yield break;

            long animationTick = GetAnimationTick(interval);
            while (true)
            {
                float animationTime = GetAnimationTime();
                double nextTickTime = (Math.Floor(animationTime / interval) + 1d) * interval;
                float waitTime = Mathf.Max(0.0001f, (float)(nextTickTime - animationTime));
                yield return new WaitForSeconds(waitTime);

                if (timer.cancel || timer.bind == null || timer.bind.gameObject == null || !timer.bind.gameObject.activeInHierarchy)
                    break;

                long nextAnimationTick = GetAnimationTick(interval);
                if (nextAnimationTick == animationTick)
                    continue;

                animationTick = nextAnimationTick;
                if (func())
                    break;
            }
        }

        private static long GetAnimationTick(float interval)
        {
            return (long)Math.Floor(GetAnimationTime() / interval);
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
