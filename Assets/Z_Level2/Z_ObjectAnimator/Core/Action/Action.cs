using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Z_ObjectAnimator.Base
{
    public enum State
    {
        Playing,
        Pausing,
        End
    }
    public class Action
    {

        public State state
        {
            get;
            private set;
        } = State.End;
        MonoBehaviour playObj;
        public Action(MonoBehaviour playObj)
        {
            this.playObj = playObj;
        }
        public Action(MonoBehaviour playObj, Event @event)
        {
            this.playObj = playObj;
            Add(@event);
        }

        public Action(MonoBehaviour playObj, List<Event> @events)
        {
            this.playObj = playObj;
            foreach (var e in @events)
            { 
                Add(e); 
            }
        }
        private List<Event> _eventList = new List<Event>();

        private Action<bool> onSucces;
        private int curOpId;

        // Start is called before the first frame update
        public void Excute(Action<bool> onSucces=null)
        {
            this.onSucces -= this.onSucces;
            this.onSucces += onSucces;
            if (state == State.Pausing)
            {
                state = State.Playing;
            }
            else if (state == State.Playing)
            {
                ToStart();
                state = State.Playing;
                curOpId++;
                playObj.StartCoroutine(InternalExcute());
            }
            else
            {
                state = State.Playing;
                curOpId++;
                playObj.StartCoroutine(InternalExcute());
            }
        }
        public void Pause()
        {
            state = State.Pausing;
        }
        public void Stop()
        {
            state = State.End;
        }
        public void ToStart()
        {
            state = State.End;
            if (_eventList.Count > 0)
                _eventList[0].ToStart();
        }
        public void ToEnd()
        {
            state = State.End;
            if (_eventList.Count > 0)
                _eventList[_eventList.Count-1].ToEnd();
        }

        public void Add(Event @event)
        {
            _eventList.Add(@event);
        }

        public void Clear()
        {
            _eventList.Clear();
        }

        IEnumerator InternalExcute()
        {
            int opId = curOpId;
            foreach (var @event in _eventList)
            {
                while (true)
                {
                    while (true)
                    {
                        if(opId!= curOpId)
                            yield break;

                        if (state == State.Playing)
                            break;
                        if (state == State.Pausing)
                            yield return -1;
                        if (state == State.End)
                            yield break;
                    }

                    if (!playObj.gameObject.activeInHierarchy)
                    {
                        Debug.Log("Event Error: " + playObj.gameObject.name + " is hide with stop");
                        yield break;
                    }
                    ReturnValue res = null;
                    bool success = true;
                    bool completed = false;
                    try
                    {
                        res = @event.Excute();
                    }
                    catch (Exception e)
                    {
                        success = false;
                        completed = true;
                        Debug.Log("Event Error: " + e + " Will skip this Event");

                    }

                    if (success)
                    {
                        switch (res.type)
                        {
                            case ReturnValue.Type.Next:
                                completed = true;
                                break;
                            case ReturnValue.Type.Continue:
                                completed = false;
                                break;
                        }
                    }

                    yield return -1;

                    if (completed)
                    {
                        break;
                    }
                }

            }
            state = State.End;
            onSucces?.Invoke(true);
        }
    }
}