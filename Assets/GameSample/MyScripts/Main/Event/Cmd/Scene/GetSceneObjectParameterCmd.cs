using Form;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using Z_Code.Form;
using Z_DesignStyle;
using Z_Map;
using Z_Map.Form;
using Z_Text;
using Z_Ui.Dialog;
using Z_Ui.Notify;
using Z_UnitSystem.Form;

namespace Z_Code
{
    public class GetSceneObjectParameterCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new GetSceneObjectParameterCmd());
        }
        public override string GetName() => "GetSceneObjectParameter";
        public override CmdBase GetNew() => new GetSceneObjectParameterCmd();
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            string name = prm[1].str;
            var unit = GlobalEventHelper.ResolveSceneObjectUnit(prm[0].str);
            GameParamForm.Data parameter = null;
            if (!string.IsNullOrEmpty(name))
            {
                if (unit != null)
                    unit.paramInfo.TryGetValue(name, out parameter);
                if (parameter == null)
                {
                    var products = GlobalEventHelper.ResolveSceneObjectProducts(prm[0].str);
                    if (products.Count > 0 && products[0].paramDic != null
                        && products[0].paramDic.TryGetValue(name, out var productParameter))
                        parameter = productParameter;
                }
                if (parameter == null)
                {
                    var template = GlobalEventHelper.ResolveSceneObjectTemplate(prm[0].str);
                    if (template?.paramDic != null && template.paramDic.TryGetValue(name, out var templateParameter))
                        parameter = templateParameter;
                }
            }
            if (parameter == null && !string.IsNullOrEmpty(name))
                parameter = MapObjectParamForm.DataByName.GetDv(name, null);
            asyncTask.res = new BoxDataForm.Data[]
            {
                parameter == null ? CodeHelper.CreateBox() : parameter.GetValue().DeepCopy()
            };
            return true;
        }
    }
}
