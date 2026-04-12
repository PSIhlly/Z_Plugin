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

namespace Ui.DialogHistory
{
    public partial class UiHistoryItemParam
    {
        public ClipForm.Data clip;
    }
    public partial class UiHistoryItemCtrl
    {
        public override void OnShow()
        {
            if (param != null)
            {
                view.txt_mainText.gameObject.SetActive(!string.IsNullOrEmpty(param.clip.mainText));
                view.txt_mainText.text = param.clip.mainText;

                view.txt_title.gameObject.SetActive(!string.IsNullOrEmpty(param.clip.title));
                view.txt_title.text = param.clip.title;

                var profileData = TexAssetForm.DataByName.GetDv(param.clip.profilePictureName, null);
                view.img_ProfilePicture.gameObject.SetActive(profileData != null);

                if (profileData != null)
                    view.img_ProfilePicture.sprite = profileData.GetSprite();
                else
                    view.img_ProfilePicture.sprite = TextureHelper.transparentSprite;
            }
        }
    }
    public partial class UiDialogHistoryParam
    {
        public List<ClipForm.Data> historyClips;
    }

    public partial class UiDialogHistoryCtrl
    {

        UiScrViewContainer<UiHistoryItemCtrl> con;
        public override void OnCreate()
        {
            con = new UiScrViewContainer<UiHistoryItemCtrl>(this, view.go_historyItem, view.scr_tt);
            view.btn_back.onClick.AddListener(() =>
            {
                Z_EventHelper.Invoke(new ShowTypeEvent()
                {
                    showType = ShowType.Normal
                });
            });
        }

        public override void OnShow()
        {
            con.Clear();
            for (int i = 0; i < param.historyClips.Count; i++)
            {
                con.Add(new UiHistoryItemParam()
                {
                    clip = param.historyClips[i]
                });
            }
            con.Refresh();
        }
        

    }

}