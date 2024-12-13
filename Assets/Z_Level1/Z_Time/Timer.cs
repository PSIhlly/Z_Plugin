using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Z_Time
{

    public class Timer
    {
        public static MonoBehaviour root;
        public static void SetRoot(MonoBehaviour root)
        {
            Timer.root = root;
        }

        Coroutine coroutine;
        MonoBehaviour bind;
        private Timer(Coroutine coroutine, MonoBehaviour bind)
        {
            this.coroutine = coroutine;
            this.bind = bind;
        }

        public static Timer StartTimer(float interval, Func<bool> func, MonoBehaviour bind = null)
        {

            if (bind == null)
                return new Timer(bind.StartCoroutine(Work(interval, func)), bind);
            return new Timer(bind.StartCoroutine(Work(interval, func)), root);
        }
        public static void CancelTimer(Timer timer)
        {
            if (timer == null)
                return;
            if (timer.coroutine != null && timer.bind != null)
                timer.bind.StopCoroutine(timer.coroutine);
            timer.coroutine = null;
        }
        private static IEnumerator Work(float interval, Func<bool> func)
        {
            while(true)
            {
                yield return new WaitForSeconds(interval);
                if (func())
                    break;
            }
        }

    }
}
