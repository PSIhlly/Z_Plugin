using System.Collections.Generic;
using UnityEngine;
using Z_Ui.Base;

namespace Z_Ui
{
    [CreateAssetMenu(fileName = "UiPreloadConfig", menuName = "Z UI/Preload Config")]
    public sealed class UiPreloadConfig : ScriptableObject
    {
        [SerializeField]
        private List<GameObject> preloadUis = new List<GameObject>();

        public IReadOnlyList<GameObject> PreloadUis => preloadUis;

        public bool Register(GameObject ui)
        {
            if (ui == null)
                return false;

            var holder = ui.GetComponent<UiHolder>();
            if (holder == null || holder.uiType != UiType.Panel || string.IsNullOrWhiteSpace(holder.uiName))
                return false;

            var changed = false;
            var registeredIndex = -1;
            for (var i = 0; i < preloadUis.Count;)
            {
                var registeredUi = preloadUis[i];
                var registeredHolder = registeredUi == null ? null : registeredUi.GetComponent<UiHolder>();
                if (registeredHolder == null || registeredHolder.uiType != UiType.Panel ||
                    string.IsNullOrWhiteSpace(registeredHolder.uiName))
                {
                    preloadUis.RemoveAt(i);
                    changed = true;
                    continue;
                }

                if (registeredUi == ui || registeredHolder.uiName == holder.uiName)
                {
                    if (registeredIndex >= 0)
                    {
                        preloadUis.RemoveAt(i);
                        changed = true;
                        continue;
                    }

                    registeredIndex = i;
                    if (registeredUi != ui)
                    {
                        preloadUis[i] = ui;
                        changed = true;
                    }
                }

                i++;
            }

            if (registeredIndex < 0)
            {
                preloadUis.Add(ui);
                changed = true;
            }

            return changed;
        }
    }
}
