using Form;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using Z_Code.Form;
using Z_Map;
using Z_Map.Form;
using Z_Text;
using Z_Ui.Dialog;
using Z_Ui.Notify;
using Z_UnitSystem.Form;

namespace Z_Code
{
    public class GetCharactersInAreaCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new GetCharactersInAreaCmd());
        }
        public override string GetName() => "GetCharactersInArea";
        public override CmdBase GetNew() => new GetCharactersInAreaCmd();
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            var components0 = prm[0].GetOrCreateDictionary();
            var pos = GameManager.PlayerPosToMapPos(new Vector3(components0["x"].num, components0["height"].num, components0["y"].num));
            var box = CodeHelper.CreateBox();
            int i = 0;
            foreach (var o in CharacterUnitForm.DataByUid.Values)
            {
                var rela = pos - o.pos;
                if (rela.sqrMagnitude < prm[1].num * prm[1].num)
                {
                    box.dic[i.ToString()] = CodeHelper.CreateBoxByStr(GlobalEventHelper.GetName(GlobalEventHelper.CHARACTER, o.unit.productInfo.Item1.ToString()));
                    i++;
                }
            }

            asyncTask.res = new BoxDataForm.Data[] { box };
            return true;
        }
    }
}
