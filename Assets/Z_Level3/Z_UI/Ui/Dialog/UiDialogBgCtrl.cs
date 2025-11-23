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
using Z_Video;

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
            view.vp_.gameObject.SetActive(false);
            if (!string.IsNullOrEmpty(param.clip.mainVideoName))
            {
                var videoData = VideoAssetForm.DataByName.GetDv(param.clip.mainVideoName, null);
                if (videoData != null)
                {
                    view.vp_.gameObject.SetActive(true);
                    videoData.Play(view.vp_);
                }
            }
            var data = TexAssetForm.DataByName.GetDv(param.clip.mainPictureName, null);
            view.img_.gameObject.SetActive(false);
            if (data != null)
            {
                view.img_.sprite = data.GetSprite(); 
                view.img_.gameObject.SetActive(true);
            }
        }
    }



}