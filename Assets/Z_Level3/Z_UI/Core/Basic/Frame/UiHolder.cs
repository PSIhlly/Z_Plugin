using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Z_Ui.Base
{
    public enum UiType
    {
        Panel,
        Sub,
        Model
    }
    public enum UiLayer
    {
        Bottom,
        Mid,
        Top
    }
    public partial class UiHolder : MonoBehaviour
    {

        public static string defaultPath = "\\GameSample\\UiBase";
        [HideInInspector]
        public string uiName;
        [HideInInspector]
        public string path = defaultPath;
        [HideInInspector]
        public UiHolder parent;

        public List<Transform> elementTrsLst = new List<Transform>();

        public List<UiHolder> subUiHolderLst = new List<UiHolder>();

        public UiCtrl ctrl;
        public UiType uiType;
        public UiLayer layer;

        public bool binded => ctrl != null;
        private bool oriInited;
        private bool firstEnter = true;
        private bool quiting = false;
        public void OriInit()
        {
            if (oriInited)
                return;
            var uiCtrlName = "Ui" + uiName + "Ctrl";
            if (uiType == UiType.Panel && UiManager.instance.uiCtrlName2OriUi.ContainsKey(uiCtrlName) && UiManager.instance.uiCtrlName2OriUi[uiCtrlName] != this)
                Debug.LogError(uiName + " has exist!");

            UiManager.instance.uiCtrlName2OriUi[uiCtrlName] = this;
            UiManager.instance.uiCtrlName2Uis[uiCtrlName] = new List<UiHolder>();
            //init sub
            foreach (var subUiHolder in subUiHolderLst)
            {
                if (subUiHolder == null)
                    Debug.LogError(uiName + " has empty sub");
                subUiHolder.OriInit();
            }

            oriInited = true;
        }

        public void InstanceInit<T>() where T : UiCtrl, new()
        {
            if (binded)
                return;
            ctrl = new T();
            ctrl.BindHolderRecursively(this);
            RegisterRecursively();
        }
        public void SubUiBind(UiCtrl ctrl)
        {
            if (binded)
                return;
            ctrl.BindHolderRecursively(this);
        }

        public void RegisterRecursively()
        {
            if (uiType == UiType.Panel)
            {
                var uiCtrlName = "Ui" + uiName + "Ctrl";
                UiManager.instance.uiCtrlName2Uis[uiCtrlName].Add(this);
                //register sub
                foreach (var subUiHolder in subUiHolderLst)
                {
                    subUiHolder.RegisterRecursively();
                }
            }
        }
        protected void Update()
        {
            if (binded)
            {
                ctrl.OnUpdate();
            }
        }

        protected void OnEnable()
        {

            if (binded)
            {
                if (firstEnter)
                    ctrl.OnCreate();
                firstEnter = false;
                ctrl.OnEnable();
                ctrl.OnShow();
            }
        }
        protected void OnDisable()
        {
            if (binded)
            {
                if (!quiting)
                {
                    ctrl.OnHide();
                }
                ctrl.OnDisable();
            }
        }
        protected void OnApplicationQuit()
        {
            quiting = true;
        }
    }

}
