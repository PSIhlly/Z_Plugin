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
using Z_Text;
using Z_Ui.Dialog;
using Z_Ui.Notify;
using Z_UnitSystem.Form;

namespace Z_Code
{
    public class DestroyObjectCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new DestroyObjectCmd());
        }
        public override string GetName() => "DestroyObject";
        public override CmdBase GetNew() => new DestroyObjectCmd();
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            var unit = UnitForm.DataByUid[(int)prm[0].num].unit;
            if (unit is ObjectUnit o)
            {
                MapManager.instance.RemoveObject(o.data);
            }
            else if (unit is ItemUnit i)
            {
                MapManager.instance.RemoveItem(i.data);
            }
            else if (unit is CharacterUnit c)
            {
                var form=CharacterProductForm.DataByUid.GetDv(c.productInfo.Item1,null);
                if(form!=null&&!form.unique)
                {
                    CharacterProductForm.RemoveData(form.uid);
                    MapManager.instance.RemoveCharacter(c.data);
                }

            }
            return true;
        }
    }
}
