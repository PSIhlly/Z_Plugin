using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Z_DesignStyle;

namespace Z_Ui.Base
{
    public class UiScrViewContainer<T> : UiContainer<T> where T : UiCtrl, new()
    {
        private ScrView sv;
        public UiScrViewContainer(GameObject ori, ScrView sv) : base(ori,true)
        {
            this.sv = sv;
            sv.ContainerAdd = (id) => { return AddReal(paramLst[id]); };
            sv.ContainerDel = DelReal;
        }

        public override void Clear()
        {
            base.Clear();
            paramLst.Clear();
        }
        public override void Refresh(List<Vector3> offsets=null)
        {
            if (offsets == null)
                offsets = new List<Vector3>();
            sv.RefreshView(paramLst.Count, offsets);
        }
        
    }
}