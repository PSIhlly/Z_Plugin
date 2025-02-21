using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_ObjectAnimator.Base;
namespace Z_ObjectAnimator.Core
{

   
    public class Controller : MonoBehaviour
    {
        int curPlayId;
        public List<ActionGroup> groups;
        public Controller(List<ActionGroup> groups)
        {
            this.groups = groups;
        }
        public Controller(ActionGroup group)
        {
            this.groups = new List<ActionGroup>() { group };
        }
        public void Play(int id, System.Action onComplete=null)
        {
            groups[id].PlayAll(onComplete);
        }
        public void Pause(int id)
        {
            groups[id].PauseAll();
        }
        public void Stop(int id)
        {
            groups[id].StopAll();
        }
        public GroupState GetState(int id)
        {
            return groups[id].GetState();
        }
    }
}
