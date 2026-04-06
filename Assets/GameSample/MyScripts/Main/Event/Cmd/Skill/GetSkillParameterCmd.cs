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
using Z_Text;
using Z_Ui.Dialog;
using Z_Ui.Notify;
using Z_UnitSystem.Form;

namespace Z_Code
{
    public class GetSkillParameterCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new GetSkillParameterCmd());
        }
        public override string GetName() => "GetSkillParameter";
        public override CmdBase GetNew() => new GetSkillParameterCmd();
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            var data = SkillProductForm.GetByEvtName(prm[0].str);
            if (data != null && data.paramDic.ContainsKey(prm[1].str))
            {
                asyncTask.res = new BoxDataForm.Data[] {data.paramDic[prm[1].str].GetValue().DeepCopy() };
            }
            else
            {
                Debug.LogError("未找到" + prm[0].str);
                asyncTask.res = new BoxDataForm.Data[] { CodeHelper.CreateBox() };
            }
            return true;
        }
    }
}
