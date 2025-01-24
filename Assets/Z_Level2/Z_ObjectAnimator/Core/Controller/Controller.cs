using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_ObjectAnimator.Base;
namespace Z_ObjectAnimator.Core
{

   
    public class Controller : MonoBehaviour
    {
        public List<ActionGroup> groups;
        public Controller(List<ActionGroup> groups)
        {
            this.groups = groups;
        }
        public void Play(int id)
        {
            groups[id].PlayAll();
        }
        public void Pause(int id)
        {
            groups[id].PauseAll();
        }
        public void Stop(int id)
        {
            groups[id].StopAll();
        }
    }
}
