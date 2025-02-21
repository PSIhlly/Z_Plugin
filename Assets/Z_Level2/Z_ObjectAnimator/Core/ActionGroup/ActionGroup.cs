using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Z_ObjectAnimator.Base
{
    public enum GroupState
    {
        Playing,
        Pausing,
        End
    }
    public enum StopType
    {
        BackToStart,
        Keep,
        MoveToEnd
    }
    public class ActionGroup
    {
        public List<Action> acts;
        public StopType stopType;
        public bool cycle;


        public bool enable;
        public ActionGroup(List<Action> acts, StopType stopType = StopType.BackToStart, bool cycle = true)
        {
            this.acts = acts;
            this.stopType = stopType;
            this.cycle = cycle;
        }
        public ActionGroup(Action act, StopType stopType = StopType.BackToStart, bool cycle = true)
        {
            this.acts = new List<Action>() { act };
            this.stopType = stopType;
            this.cycle = cycle;
        }


        public void PlayAll(System.Action onComplete)
        {
            int cnt = 0;
            foreach (var act in acts)
            {
                act.Excute((o)=>
                {
                    cnt++;
                    if (cnt == acts.Count)
                    {
                        onComplete?.Invoke();
                        if (cycle)
                        {
                            PlayAll(onComplete);
                        }
                    }
                    
                        
                       
                });
            }
        }
        public void PauseAll()
        {
            foreach (var act in acts)
            {
                act.Pause();
            }
        }
        public void StopAll()
        {
            foreach (var act in acts)
            {
                act.Stop();
                switch(stopType)
                {
                    case StopType.BackToStart:
                        act.ToStart();
                        break;
                    case StopType.MoveToEnd:
                        act.ToEnd();
                        break;
                }
            }
        }
        public GroupState GetState()
        {
            foreach (var act in acts)
            {
                switch (act.state)
                {
                    case State.Playing:
                        return GroupState.Playing;
                    case State.Pausing:
                        return GroupState.Pausing;
                        break;
                }
            }
            return GroupState.End;
        }
    }
}
