using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_DesignStyle;
using Z_Ui.Base;
namespace Z_Ui
{
    public class UiManager : Z_MonoManager<UiManager>
    {
        public List<GameObject> preloadUis;
        public List<GameObject> canvasLst;

        public Dictionary<string, UiCtrl> uiCtrlName2UiCtrl = new Dictionary<string, UiCtrl>();
        public Dictionary<string, UiHolder> uiCtrlName2OriUi = new Dictionary<string, UiHolder>();
        public Dictionary<string, List<UiHolder>> uiCtrlName2Uis = new Dictionary<string, List<UiHolder>>();

        protected override void Awake()
        {
            base.Awake();
            foreach(var ui in preloadUis)
            {
                ui.GetComponent<UiHolder>().OriInit();
                ui.SetActive(false);
            }
        }
        public UiCtrl GetUi<T>() where T : UiCtrl, new()
        {
            var tp = typeof(T);
            if (uiCtrlName2Uis[tp.Name].Count == 0)
            {
                return null;
            }
            var uiHolder =uiCtrlName2Uis[tp.Name][0];
            return uiHolder.ctrl;
        }
        public UiHolder ShowUi<T>(UiParam param=null) where T : UiCtrl, new()
        {
            var tp = typeof(T);
            if (uiCtrlName2Uis[tp.Name].Count == 0)
            {
                CreateUi<T>();
            }
            var uiHolder = uiCtrlName2Uis[tp.Name][0];
            uiHolder.ctrl.SetParam(param);
            uiHolder.gameObject.SetActive(true);
            return uiHolder;
        }
        public void CloseUi<T>() where T : UiCtrl, new()
        {
            var tp = typeof(T);
            if (uiCtrlName2Uis[tp.Name].Count == 0)
            {
                return;
            }
            var uiHolder = uiCtrlName2Uis[tp.Name][0];
            uiHolder.gameObject.SetActive(false);
        }
       
        public UiHolder CreateUi<T>() where T : UiCtrl, new()
        {
            var tp = typeof(T);
            var oriHolder = uiCtrlName2OriUi[tp.Name];
            var uiHolder = Instantiate(oriHolder.gameObject, canvasLst[0].transform).GetComponent<UiHolder>();
            BindMainUi<T>(uiHolder);
            return uiHolder;
        }
        public void BindMainUi<T>(UiHolder uiHolder) where T : UiCtrl, new()
        {
            var tp = typeof(T);
            uiHolder.MainUiBind(new T());
        }

        public void DestroyUi<T>(T uiCtrl) where T : UiCtrl, new()
        {
            var tp = typeof(T);
            if (uiCtrlName2Uis[tp.Name].Count == 0)
            {
                return;
            }
            Destroy(uiCtrl.uiHolder.gameObject);
            uiCtrlName2Uis[tp.Name].Remove(uiCtrl.uiHolder);
        }

    }

}
