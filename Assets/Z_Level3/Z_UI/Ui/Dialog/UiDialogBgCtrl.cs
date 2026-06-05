using System.Collections;
using System.Collections.Generic;
using System.Security.Policy;
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
 

namespace Ui.DialogBg
{
    public partial class UiDialogBgParam
    {
        public ClipForm.Data clip;
    }
    public partial class UiDialogBgCtrl
    {
        public override void OnShow()
        {
            view.mp_.gameObject.SetActive(false);
            if (!string.IsNullOrEmpty(param.clip.mainVideoName))
            {
                var videoData = VideoAssetForm.DataByName.GetDv(param.clip.mainVideoName, null);
                if (videoData != null)
                {
                    view.mp_.gameObject.SetActive(true);
                    videoData.Play(view.mp_, () =>
                    {
                        Z_EventHelper.Invoke(new ClipPlayEvent()
                        {
                            playType = PlayType.ClipVideoOver
                        });
                    });
                }
            }
            var data = TexAssetForm.DataByName.GetDv(param.clip.mainPictureName, null);
            if (data != null)
            {
                view.img_.sprite = data.GetSprite(); 
            }
        }
    }



}