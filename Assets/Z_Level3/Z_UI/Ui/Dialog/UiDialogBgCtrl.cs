using RenderHeads.Media.AVProVideo;
using System.Collections;
using System.Collections.Generic;
using System.Security.Policy;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Events;
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
    public partial class UiDialogBgModel
    {
        public UnityAction<MediaPlayer, MediaPlayerEvent.EventType, ErrorCode> videoCompleteHandler;
    }
    public partial class UiDialogBgCtrl
    {
        public override void OnShow()
        {
            ClearMedia();
            view.mp_.gameObject.SetActive(false);
            if (param.clip.mainVideo>0)
            {
                var videoData = VideoAssetForm.DataById.GetDv(param.clip.mainVideo, null);
                if (videoData != null)
                {
                    view.mp_.gameObject.SetActive(true);
                    model.videoCompleteHandler = videoData.Play(view.mp_, () =>
                    {
                        model.videoCompleteHandler = null;
                        Z_EventHelper.Invoke(new ClipPlayEvent()
                        {
                            playType = PlayType.ClipVideoOver
                        });
                    });
                }
            }
            var data = TexAssetForm.DataById.GetDk(param.clip.mainPicture, GlobalDefaultHelper.DefaultTexId);
            if (data != null)
            {
                view.img_.BindTexData(data); 
            }
        }

        public override void OnHide()
        {
            ClearMedia();
        }

        private void ClearMedia()
        {
            if (model.videoCompleteHandler != null)
            {
                view.mp_.Events.RemoveListener(model.videoCompleteHandler);
                model.videoCompleteHandler = null;
            }
            view.mp_.Pause();
            view.mp_.CloseMedia();
        }
    }



}
