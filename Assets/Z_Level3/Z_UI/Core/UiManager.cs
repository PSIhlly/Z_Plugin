using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Z_Debug;
using Z_DesignStyle;
using Z_Math;
using Z_Time;
using Z_Ui.Base;
namespace Z_Ui
{
    [DefaultExecutionOrder(-20)]
    public class UiManager : Z_MonoManager<UiManager>
    {
        public List<GameObject> preloadUis;
        public List<GameObject> layerRootLst;
        public Dictionary<UiLayer, int> layer2Id;

        public Dictionary<string, UiCtrl> uiCtrlName2UiCtrl = new Dictionary<string, UiCtrl>();
        public Dictionary<string, UiHolder> uiCtrlName2OriUi = new Dictionary<string, UiHolder>();
        public Dictionary<string, List<UiHolder>> uiCtrlName2Uis = new Dictionary<string, List<UiHolder>>();
        public List<Action> uiOnShowEventLst = new List<Action>();
        protected override void Awake()
        {
            base.Awake();
            layer2Id = new Dictionary<UiLayer, int>();
            foreach (UiLayer ly in Enum.GetValues(typeof(UiLayer)))
            {
                layer2Id[ly] = (int)ly;
            }
            foreach (var ui in preloadUis)
            {
                ui.GetComponent<UiHolder>().OriInit();
                ui.SetActive(false);
            }
        }
        public void Update()
        {
            InvokeEvents();
        }
        public T GetUi<T>() where T : UiCtrl, new()
        {
            var tp = typeof(T);
            if (uiCtrlName2Uis[tp.Name].Count == 0)
            {
                return null;
            }
            var uiHolder = uiCtrlName2Uis[tp.Name][0];
            return (T)uiHolder.ctrl;
        }
        public UiHolder ShowUi<T>(UiParam param = null) where T : UiCtrl, new()
        {
            var tp = typeof(T);
            if (uiCtrlName2Uis[tp.Name].Count == 0)
            {
                CreateUi<T>();
            }
            var uiHolder = uiCtrlName2Uis[tp.Name][0];
            uiHolder.ctrl.SetShow(true, param);

            uiHolder.transform.SetAsLastSibling();
            InvokeEvents();
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
            for (int i = 0; i < layerRootLst[(int)layer].transform.childCount; i++)
            {
                var holder = layerRootLst[(int)layer].transform.GetChild(i).GetComponent<UiHolder>();
                if (holder.ctrl.active)
                {
                    holder.ctrl.Close();
                }
            }
        }
        public void CloseAll()
        {
            foreach (var layer in layer2Id)
            {
                CloseAll(layer.Key);
            }
        }
        public static void Jump(RectTransform tar, ScrollRect scr)
        {
            var oldInertia = scr.inertia;
            scr.inertia = false;
            Vector3 dir = scr.viewport.GetCenterWorldPos() - tar.GetCenterWorldPos();
            scr.content.position = scr.content.position + dir;
            scr.inertia = oldInertia;
        }
        public static void Rebuild(GameObject go, bool recursion = false)
        {
            var rts = new List<RectTransform>() { go.GetComponent<RectTransform>() };


            if (recursion)
            {
                rts.Clear();
                rts.AddRange(go.GetComponentsInChildren<RectTransform>());
            }


          
                foreach (var rt in rts)
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
                }

        }
        public void InvokeEvents()
        {
            for (int id = 0; id < uiOnShowEventLst.Count; id++)
            {
                uiOnShowEventLst[id]();
            }
            uiOnShowEventLst.Clear();
        }
    }

}
