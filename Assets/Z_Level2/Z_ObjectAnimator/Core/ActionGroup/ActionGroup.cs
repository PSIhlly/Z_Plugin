using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Z_ObjectAnimator.Base
{
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

        public void PlayAll()
        {
            int cnt = 0;
            foreach (var act in acts)
            {
                act.Excute((o)=>
                {
                    if(cycle)
                    {
                        cnt++;
                        if(cnt == acts.Count)
                        {
                            PlayAll();
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
    }
}
