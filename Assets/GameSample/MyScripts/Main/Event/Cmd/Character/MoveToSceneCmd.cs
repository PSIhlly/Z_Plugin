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
using Z_Math;
using Z_Text;
using Z_Time;
using Z_Ui.Dialog;
using Z_Ui.Notify;
using Z_UnitSystem.Form;

namespace Z_Code
{
    public class MoveToSceneCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new MoveToSceneCmd());
        }
        public override string GetName() => "MoveToScene";
        public override CmdBase GetNew() => new MoveToSceneCmd();
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            var scene = SceneForm.DataByName.GetDv(prm[0].str, null);
            var ch = CharacterProductForm.DataByUid.GetDv(
                    GameManager.instance.curProgress.characterUid, null);
            var pos= new Vector3(prm[1].dic["x"].num, prm[1].dic["height"].num, prm[1].dic["y"].num);
            if (scene != null && ch != null)
            {
                GameManager.instance.curProgress.targetScene = (scene.uid, pos);
            }
            return true;
        }
    }
}
