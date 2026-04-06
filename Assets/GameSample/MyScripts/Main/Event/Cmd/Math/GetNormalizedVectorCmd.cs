using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Z_Code.Form;
using Z_Text;
using Z_Ui.Notify;

namespace Z_Code
{
    public class GetNormalizedVectorCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new GetNormalizedVectorCmd());
        }
        public override string GetName() => "GetNormalizedVector";
        public override CmdBase GetNew() => new GetNormalizedVectorCmd();
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            var v = MapManager.instance.utilCtrl.MapPos2RealPos(new Vector3(prm[0].dic["x"].num, prm[0].dic["height"].num, prm[0].dic["y"].num)).normalized;

            var box = CodeHelper.CreateBox();
            box.dic["x"] = CodeHelper.CreateBoxByNum(v.x);
            box.dic["height"] = CodeHelper.CreateBoxByNum(v.y);
            box.dic["y"] = CodeHelper.CreateBoxByNum(v.z);
            asyncTask.res = new BoxDataForm.Data[] {box };

            return true;
        }
    }
}
