using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_ObjectAnimator.Event;

namespace Z_ObjectAnimator.Action
{
    public abstract class Action : MonoBehaviour
    {

        private List<Event.Event> _eventList=new List<Event.Event>();

        // Start is called before the first frame update
        public void Excute()
        {
            StartCoroutine(InternalExcute());
        }

        public void Add(Event.Event @event)
        {
            _eventList.Add(@event);
        }

        public void Clear()
        {
            _eventList.Clear();
        }

        IEnumerator InternalExcute()
        {
            foreach (var @event in _eventList)
            { 
                ReturnValue res = null;
                bool success = true;
                try
                {
                    res = @event.Excute();
                }
                catch (Exception e)
                {
                    success = false;
                    Debug.Log("Event Error: " + e + " Will skip this Event");
                }

                if (success)
                {
                    switch (res.type)
                    {
                        case ReturnValue.Type.NextEvnet:
                            break;
                        case ReturnValue.Type.WaitFrames:
                            for (int i = 0; i < res.framesToWait; i++)
                            {
                                yield return 0;
                            }

                            break;
                        case ReturnValue.Type.WaitSeconds:
                            yield return new WaitForSeconds(res.secondsToWait);
                            break;
                    }
                }
            }
        }
    }
}