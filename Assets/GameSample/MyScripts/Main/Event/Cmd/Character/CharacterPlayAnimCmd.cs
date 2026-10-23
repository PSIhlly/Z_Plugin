using Form;
using System;
using System.Collections;
using System.Collections.Generic;
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
    public class CharacterPlayAnimCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new CharacterPlayAnimCmd());
        }
        public override string GetName() => "CharacterPlayAnim";
        public override CmdBase GetNew() => new CharacterPlayAnimCmd();
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            if (!CharacterProductForm.TryGetReferenceUid(prm[0].str, out var uid))
                return true;
            foreach (var data in PlayManager.instance.sceneCtrl.GetCharacterUnits(uid))
            {
                GameManager.instance.characterCtrl.PlaySpecialAnim(data,prm[1].str);
            }
               
            return true;
        }
    }
}
