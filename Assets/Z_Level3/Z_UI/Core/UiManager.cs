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
        public List<GameObject> layerRootLst;
        public Dictionary<UiLayer, int> layer2Id=new Dictionary<UiLayer, int>() { { UiLayer.Bottom,0 }, { UiLayer.Mid, 1 }, { UiLayer.Top, 2 } };

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
        public T GetUi<T>() where T : UiCtrl, new()
        {
            var tp = typeof(T);
            if (uiCtrlName2Uis[tp.Name].Count == 0)
            {
                return null;
            }
            var uiHolder =uiCtrlName2Uis[tp.Name][0];
            return (T)uiHolder.ctrl;
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
            uiHolder.transform.SetAsLastSibling();
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
            uiHolder.ctrl.Close();
        }
       
        public UiHolder CreateUi<T>() where T : UiCtrl, new()
        {
            var tp = typeof(T);
            var oriHolder = uiCtrlName2OriUi[tp.Name];
            var uiHolder = Instantiate(oriHolder.gameObject, layerRootLst[(int)oriHolder.layer].transform).GetComponent<UiHolder>();
            uiHolder.InstanceInit<T>();
            return uiHolder;
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
        public void CloseAll(UiLayer layer)
        {
            for(int i=0;i< layerRootLst[(int)layer].transform.childCount;i++)
            {
               var holder= layerRootLst[(int)layer].transform.GetChild(i).GetComponent<UiHolder>();
                if(holder.ctrl.isActive)
                {
                    holder.ctrl.Close();
                }
            }
        }
        public void CloseAll()
        {
            foreach(var layer in layer2Id)
            {
                CloseAll(layer.Key);
            }
        }

    }

}
