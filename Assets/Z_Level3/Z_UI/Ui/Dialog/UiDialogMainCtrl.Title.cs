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

    namespace Title
    {
        public partial class UiTitleParam
        {
            public ClipForm.Data clip;
        }
        public partial class UiTitleCtrl
        {
            public override void OnShow()
            {
                
                if(string.IsNullOrEmpty(param.clip.title))
                {
                    Close();
                }else
                {
                    view.txt_.text = param.clip.title;
                }
            }
        }
    }
   

}