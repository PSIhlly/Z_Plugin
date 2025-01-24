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

        public Action onButonDownE;

        public Action onButonW;
        public Action onButonS;
        public Action onButonA;
        public Action onButonD;

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
        public void Register(InputConfig config)
        {
            cur = config;
        }

        public void Update()
        {
            if (!enabled)
                return;
#if UNITY_ANDROID && !UNITY_EDITOR

#else
            if (Input.GetKeyDown(KeyCode.W))
            {
                cur?.onButonDownW?.Invoke();
            }
            if (Input.GetKeyDown(KeyCode.S))
            {
                cur?.onButonDownS?.Invoke();
            }
            if (Input.GetKeyDown(KeyCode.A))
            {
                cur?.onButonDownA?.Invoke();
            }
            if (Input.GetKeyDown(KeyCode.D))
            {
                cur?.onButonDownD?.Invoke();
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                cur?.onButonDownE?.Invoke();
            }
            
            if (Input.GetKey(KeyCode.W))
            {
                cur?.onButonW?.Invoke();
            }
            if (Input.GetKey(KeyCode.S))
            {
                cur?.onButonS?.Invoke();
            }
            if (Input.GetKey(KeyCode.A))
            {
                cur?.onButonA?.Invoke();
            }
            if (Input.GetKey(KeyCode.D))
            {
                cur?.onButonD?.Invoke();
            }


            if (Input.GetKeyUp(KeyCode.W))
            {
                cur?.onButonUpW?.Invoke();
            }
            if (Input.GetKeyUp(KeyCode.S))
            {
                cur?.onButonUpS?.Invoke();
            }
            if (Input.GetKeyUp(KeyCode.A))
            {
                cur?.onButonUpA?.Invoke();
            }
            if (Input.GetKeyUp(KeyCode.D))
            {
                cur?.onButonUpD?.Invoke();
            }
#endif

        }

    }
}
