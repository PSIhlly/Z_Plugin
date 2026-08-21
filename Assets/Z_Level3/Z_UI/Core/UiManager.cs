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
    [DefaultExecutionOrder(-200)]
    public class UiManager : Z_MonoManager<UiManager>
    {
        [SerializeField]
        private UiPreloadConfig preloadConfig;

        public UiPreloadConfig PreloadConfig => preloadConfig;

        public List<GameObject> layerRootLst;
        public Dictionary<UiLayer, int> layer2Id;

        public Dictionary<string, UiCtrl> uiCtrlName2UiCtrl = new Dictionary<string, UiCtrl>();
        public Dictionary<string, UiHolder> uiCtrlName2OriUi = new Dictionary<string, UiHolder>();
        public Dictionary<string, List<UiHolder>> uiCtrlName2Uis = new Dictionary<string, List<UiHolder>>();

        private static List<RectTransform> rectTemp = new List<RectTransform>();
        protected override void Awake()
        {
            base.Awake();
            layer2Id = new Dictionary<UiLayer, int>();
            foreach (UiLayer ly in Enum.GetValues(typeof(UiLayer)))
            {
                layer2Id[ly] = (int)ly;
            }
            if (preloadConfig == null)
            {
                Debug.LogError($"{name} requires a {nameof(UiPreloadConfig)} reference in the Inspector.");
                return;
            }

            var preloadRoot = new GameObject("UiPreloadTemplates");
            preloadRoot.transform.SetParent(transform, false);
            preloadRoot.SetActive(false);
            var registeredUiNames = new HashSet<string>();
            foreach (var uiPrefab in preloadConfig.PreloadUis)
            {
                if (uiPrefab == null || !uiPrefab.TryGetComponent<UiHolder>(out var prefabHolder) ||
                    prefabHolder.uiType != UiType.Panel || string.IsNullOrWhiteSpace(prefabHolder.uiName))
                {
                    Debug.LogError("UI preload config contains an invalid Panel prefab.");
                    continue;
                }

                if (!registeredUiNames.Add(prefabHolder.uiName))
                {
                    Debug.LogError($"UI preload config contains duplicate uiName '{prefabHolder.uiName}'.");
                    continue;
                }

                var ui = Instantiate(uiPrefab, preloadRoot.transform, false);
                ui.name = uiPrefab.name;
                ui.SetActive(false);
                var holder = ui.GetComponent<UiHolder>();
                holder.OriInit();
            }
        }
        public void Update()
        {
        }
        public T GetUi<T>() where T : UiCtrl, new()
        {
            var tp = typeof(T);
            if (!uiCtrlName2Uis.TryGetValue(tp.Name, out var uis) || uis.Count == 0)
            {
                return null;
            }
            var uiHolder = uis[0];
            return (T)uiHolder.ctrl;
        }
        public UiHolder ShowUi<T>(UiParam param = null) where T : UiCtrl, new()
        {
            var tp = typeof(T);
            if (!uiCtrlName2Uis.TryGetValue(tp.Name, out var uis))
            {
                Debug.LogError($"{tp.Name} is not registered in UiManager preload config '{preloadConfig?.name ?? "None"}'.");
                return null;
            }
            if (uis.Count == 0)
            {
                if (CreateUi<T>() == null)
                    return null;
            }
            var uiHolder = uis[0];
            
            uiHolder.ctrl.SetShow(true, param);
            uiHolder.transform.SetAsLastSibling();
            return uiHolder;
        }
        public void CloseUi<T>() where T : UiCtrl, new()
        {
            var tp = typeof(T);
            if (!uiCtrlName2Uis.TryGetValue(tp.Name, out var uis) || uis.Count == 0)
            {
                return;
            }
            var uiHolder = uis[0];
            uiHolder.ctrl.Close();
        }

        public UiHolder CreateUi<T>() where T : UiCtrl, new()
        {
            var tp = typeof(T);
            if (!uiCtrlName2OriUi.TryGetValue(tp.Name, out var oriHolder))
            {
                Debug.LogError($"{tp.Name} has no template in UiManager preload config '{preloadConfig?.name ?? "None"}'.");
                return null;
            }
            var uiHolder = Instantiate(oriHolder.gameObject, layerRootLst[(int)oriHolder.layer].transform).GetComponent<UiHolder>();
            uiHolder.gameObject.SetActive(false);
            uiHolder.InstanceInit<T>();
            return uiHolder;
        }


        public void DestroyUi<T>(T uiCtrl) where T : UiCtrl, new()
        {
            var tp = typeof(T);
            if (!uiCtrlName2Uis.TryGetValue(tp.Name, out var uis) || uis.Count == 0)
            {
                return;
            }
            Destroy(uiCtrl.uiHolder.gameObject);
            uis.Remove(uiCtrl.uiHolder);
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
            var rt = go.GetComponent<RectTransform>();
            if (!rt||go && go.activeInHierarchy)
            {
                if (recursion)
                {
                    go.GetComponentsInChildren(false, rectTemp);
                    for (int i = rectTemp.Count - 1; i >= 0; i--)
                    {
                        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTemp[i]);
                    }
                }
                else
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
                }
            }
        }

    }

}
