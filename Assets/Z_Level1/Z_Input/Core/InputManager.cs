using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_DesignStyle;
namespace Z_Input
{

    public class InputConfig
    {
        public Action onButonDownW;
        public Action onButonDownS;
        public Action onButonDownA;
        public Action onButonDownD;

        public Action onButonUpW;
        public Action onButonUpS;
        public Action onButonUpA;
        public Action onButonUpD;


    }

    public class InputManager : Z_MonoManager<InputManager>
    {
        public bool enabled = true;
        public InputConfig cur;
        public override void Init()
        {
        }

        public void Update()
        {
            if (!enabled)
                return;
#if UNITY_ANDROID && !UNITY_EDITOR

#else
            if (Input.GetKeyDown(KeyCode.W))
            {
                cur?.onButonDownW();
            }
            if (Input.GetKeyDown(KeyCode.S))
            {
                cur?.onButonDownS();
            }
            if (Input.GetKeyDown(KeyCode.A))
            {
                cur?.onButonDownA();
            }
            if (Input.GetKeyDown(KeyCode.D))
            {
                cur?.onButonDownD();
            }

            if (Input.GetKeyUp(KeyCode.W))
            {
                cur?.onButonUpW();
            }
            if (Input.GetKeyUp(KeyCode.S))
            {
                cur?.onButonUpS();
            }
            if (Input.GetKeyUp(KeyCode.A))
            {
                cur?.onButonUpA();
            }
            if (Input.GetKeyUp(KeyCode.D))
            {
                cur?.onButonUpD();
            }
#endif

        }

    }
}
