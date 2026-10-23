using Form;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Z_Code.Form;
using Z_DataSystem;
using Z_DesignStyle;
using Z_Map;
using Z_Map.Form;
using Z_Text;
using Z_Ui.Dialog;
using Z_Ui.Notify;
using Z_UnitSystem.Form;

namespace Z_Code
{
    public class GenerateObjectCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new GenerateObjectCmd());
        }
        public override string GetName() => "GenerateObject";
        public override CmdBase GetNew() => new GenerateObjectCmd();
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            var data = GlobalEventHelper.ResolveSceneObjectTemplate(prm[0].str);
            if (data != null)
            {
                var key = data.uid.ToString();

                var newObjectData = MapManager.instance.AddObject(key, MapManager.instance.utilCtrl.MapPos2RealPos(GameManager.PlayerPosToMapPos(new Vector3(prm[1].dic["x"].num, prm[1].dic["height"].num, prm[1].dic["y"].num))), GlobalDefaultHelper.GetRuntimeMapObjectPrefabName(ObjectProductForm.GetTemplateUid(data)), null);

                if (newObjectData != null)
                {
                    // Selecting an existing instance copies its Product details;
                    // the new placement must never share mutable state with it.
                    if (data.protoUid != 0)
                    {
                        var copy = data.Copy(false);
                        copy.ToProduct(ObjectProductForm.GetTemplateUid(data));
                        data = copy;
                    }
                    GameManager.instance.mapCtrl.RegisterObject(newObjectData, data);
                    MapManager.instance.updateCtrl.UpdateSingleOne(newObjectData.unit);
                    asyncTask.res = new BoxDataForm.Data[] { CodeHelper.CreateBoxByStr(GlobalEventHelper.GetSceneObjectReference(newObjectData.unit)) };
                    return true;
                }
            }
            asyncTask.res = new BoxDataForm.Data[] { CodeHelper.CreateBoxByNum(0) };
            return true;
        }
    }
}
