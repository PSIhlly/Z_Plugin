using Form;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using Z_Code.Form;
using Z_Map;
using Z_Text;
using Z_Ui.Dialog;
using Z_Ui.Notify;
using Z_UnitSystem.Form;

namespace Z_Code
{
    public class SetSceneObjectParameterCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new SetSceneObjectParameterCmd());
        }
        public override string GetName() => "SetSceneObjectParameter";
        public override CmdBase GetNew() => new SetSceneObjectParameterCmd();
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            string name = prm[1].str;
            if (string.IsNullOrEmpty(name))
                return true;
            var products = GlobalEventHelper.ResolveSceneObjectProducts(prm[0].str);
            foreach (var product in products)
            {
                if (product.paramDic == null)
                    product.paramDic = new Dictionary<string, MapObjectParamForm.Data>();
                if (!product.paramDic.TryGetValue(name, out var parameter) || parameter == null)
                    product.paramDic[name] = parameter = new MapObjectParamForm.Data(-1, name, 0, "", "", "");
                parameter.SetValue(prm[2]);
            }
            foreach (var unit in GlobalEventHelper.ResolveSceneObjectUnits(prm[0].str))
            {
                if (!SceneUnitCommand.IsLive(unit))
                    continue;
                if (unit is ObjectUnit obj && ObjectProductForm.GetProduct(obj.data) != null)
                {
                    ObjectProductForm.RefreshUnitParameters(obj.data);
                    continue;
                }
                // Bare legacy $so$ references also address Items, Tiles and
                // Characters. Preserve their existing scene-local parameters.
                var dic = unit.paramInfo;
                if (!dic.TryGetValue(name, out var parameter) || parameter == null)
                    dic[name] = parameter = new MapObjectParamForm.Data(-1, name, 0, "", "", "");
                parameter.SetValue(prm[2]);
                unit.paramInfo = dic;
            }
          
            return true;
        }
    }
}
