using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BaseFunc
{
    public class SwitchFunc : MonoBehaviour
    {
        public int state;
        [SerializeField]
        private GameObject[] stateGo;
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

