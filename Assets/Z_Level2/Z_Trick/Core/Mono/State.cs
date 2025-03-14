using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Z_Trick.BaseFunc
{
    public class State : MonoBehaviour
    {
        [SerializeField]
        public int state
        {
            get;
            private set;
        }
        [SerializeField]
        private GameObject[] stateGo;
        public void Awake()
        {
            for(int i=0;i< stateGo.Length;i++)
            {
                stateGo[i].SetActive(false);
            }
            ChangeState(state);
        }
        public void ChangeState(int tar)
        {
            stateGo[state].SetActive(false);
            state = tar;
            if (state >= stateGo.Length)
                state = 0;
            stateGo[state].SetActive(true);
        }
        public void ChangeState()
        {
            stateGo[state].SetActive(false);
            state++;
            if (state >= stateGo.Length)
                state = 0;
            stateGo[state].SetActive(true);
        }
    }

}

