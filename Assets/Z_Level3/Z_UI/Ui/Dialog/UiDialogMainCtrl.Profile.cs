using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using Z_DataSystem;
using Z_DataSystem.Form;
using Z_DesignStyle;
using Z_Texture;
using Z_Time;
using Z_Ui;
using Z_Ui.Base;
using Z_Ui.Dialog;
using Z_Ui.Form;

namespace Ui.DialogMain
{

    namespace Profile
    {
        public partial class UiProfileParam
        {
            public ClipForm.Data clip;
        }
        public partial class UiProfileCtrl
        {
            public override void OnShow()
            {
                var data = TexAssetForm.DataByName.GetDv(param.clip.mainPictureName, null);
                if(data == null)
                {
                    Close();
                }else
                {
                    view.img_.sprite = data.GetSprite();
                }
            }
        }
    }
   

}