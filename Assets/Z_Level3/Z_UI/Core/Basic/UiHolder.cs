using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Z_Ui.Base
{
    public enum UIType
    {
        Panel,
        Sub,
        Model
    }
    public partial class UiHolder : MonoBehaviour
    {

        public static string defaultPath = "\\Z_Level3\\Z_UI\\Sample\\Ui\\UiBase";

        [HideInInspector]
        public string uiName;
        [HideInInspector]
        public string path = defaultPath;
        [HideInInspector]
        public UiHolder parent;

        public List<Transform> elementTrsLst = new List<Transform>();
        
        public List<UiHolder> subUiHolderLst = new List<UiHolder>();

        public UiCtrl ctrl;
        public UIType uiType;

        public bool binded {
            get;
            private set;
        }
        private bool oriInited;
        private bool firstEnter=true;
        public void OriInit()
        {
            if (oriInited)
                return;
                var uiCtrlName = "Ui" + uiName + "Ctrl";
            if (UiManager.instance.uiCtrlName2OriUi.ContainsKey(uiCtrlName) && UiManager.instance.uiCtrlName2OriUi[uiCtrlName] != this)
                Debug.LogError(uiName + " has exist!");

            UiManager.instance.uiCtrlName2OriUi[uiCtrlName] = this;
            UiManager.instance.uiCtrlName2Uis[uiCtrlName]=new List<UiHolder>();
            //init sub
            foreach (var subUiHolder in subUiHolderLst)
            {
                subUiHolder.OriInit();
            }

            oriInited = true;
        }

        public void MainUiBind(UiCtrl ctrl)
        {
            if (binded)
                return;
            ctrl.BindHolder(this);
            binded = true;
            Register();
        }
        public void SubUiBind(UiCtrl ctrl)
        {
            if (binded)
                return;
            ctrl.BindHolder(this);
            binded = true;
        }

        public void Register()
        {
            var uiCtrlName = "Ui" + uiName + "Ctrl";
            UiManager.instance.uiCtrlName2Uis[uiCtrlName].Add(this);
            //register sub
            foreach (var subUiHolder in subUiHolderLst)
            {
                subUiHolder.Register();
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
                ctrl.OnDisable();
            }
        }
        
    }

}
