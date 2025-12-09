using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Video;
using Z_Code.Form;
using Z_DesignStyle;
using Z_Text;
using Z_Texture;
using Z_Ui.Base;
using Z_Video;

namespace Ui.ParamShow
{

    public partial class UiParamShowParam
    {
        public float value;
        public float max;
        public UnityEngine.Color color;
    }
    public partial class UiParamShowModel
    {
        public UiParamShowParam prm;
    }
    public partial class UiParamShowCtrl
    {
        public override void OnShow()
        {
            model.prm = param;
            Refresh();
        }
        public void Refresh()
        {
            view.sld_.value = model.prm.value;
            view.sld_.maxValue = model.prm.max;
            view.img_.color = model.prm.color;
        }
    }

}