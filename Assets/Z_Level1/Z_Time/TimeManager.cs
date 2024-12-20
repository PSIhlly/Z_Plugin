using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Z_DesignStyle;

namespace Z_Time
{

    public class TimeManager : Z_MonoManager<TimeManager>
    {
        public Timer StartTimer(float interval, Func<bool> func, MonoBehaviour bind = null)
        {
            Timer timer = new Timer() {
                bind = bind == null ? this : bind
            };
            StartCoroutine(Work(timer,interval, func));
            return timer;
        }
        public void CancelTimer(Timer timer)
        {
            if (timer == null)
                return;
            timer.cancel = true;
        }
        private IEnumerator Work(Timer timer,float interval, Func<bool> func)
        {
            while (true)
            {
                yield return new WaitForSeconds(interval);
                if (timer.cancel||!timer.bind.gameObject.activeInHierarchy || func())
                    break;
            }

        }
    }
}
