using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Z_Time;
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
        Basic,
        Window,
        DialogBg,
        Custom,
        DialogText,
        Notice
    }
    [DefaultExecutionOrder(-100)]
    public partial class UiHolder : MonoBehaviour
    {

        public static string defaultPath = "\\GameSample\\UiBase";
        [SerializeField]
        private UiPreloadConfig preloadConfig;

        public UiPreloadConfig PreloadConfig => preloadConfig;

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
        private bool quiting = false;
        int visitId;

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
            if (binded && ctrl.showed)
            {
                ctrl.OnUpdate();
            }
        }

        protected void OnEnable()
        {
            if (binded)
            {

/*                var pa = this;
                while (pa.parent != null)
                {
                    pa = pa.parent;
                }*/
                Create();
                ctrl.OnEnable();
                Show();
                CheckShow();
            }
        }
        public void Create()
        {
            if (binded && gameObject != null && gameObject.activeInHierarchy)
            {
                CreateSelf();
                visitId++;
                var cur = visitId;
                foreach (var sub in subUiHolderLst)
                {
                    sub.Create();
                    if (cur != visitId)
                        break;
                }
            }
        }
        private void CreateSelf()
        {
            if (!ctrl.inited)
            {
                ctrl.inited = true;
                ctrl.OnCreate();
            }
        }
        private void Show()
        {
            if (binded && gameObject != null && gameObject.activeInHierarchy)
            {
                ShowSelf();
                var cur = visitId;
                foreach (var sub in subUiHolderLst)
                {
                    sub.Show();
                    if (cur != visitId)
                        break;
                }
            }
        }
        
        private void ShowSelf()
        {
            if (!ctrl.showed)
            {
                ctrl.showed = true;
                ctrl.OnShow();
            }
        }
        private void CheckShow()
        {
            if (binded && gameObject != null)
            {
                if (ctrl.showed && !ctrl.active)
                {
                    ctrl.showed = false;
                    ctrl.OnHide();
                }
                foreach (var sub in subUiHolderLst)
                {
                    sub.CheckShow();
                }
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
                ctrl.showed = false;
            }
        }
        protected void OnApplicationQuit()
        {
            quiting = true;
        }
    }

}
