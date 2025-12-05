using Form;
using System.Collections;
using System.Collections.Generic;
using Ui.ModSceneMenu;
using Ui.PlayData;
using Ui.PlaySceneMenu;
using UnityEngine;
using Z_DataSystem.Form;
using Z_Map;
using Z_ObjectAnimator.Base;
using Z_ObjectAnimator.Core;
using Z_Texture;
using Z_Ui;
using Z_Ui.Base;
using Z_DesignStyle;
using Z_DataSystem;

namespace Ui.PlaySceneMain
{

    public partial class UiPlaySceneMainCtrl
    {
        UiContainer<UiTeamerCtrl> teamerCon;
        public override void OnCreate()
        {

#if UNITY_STANDALONE_WIN
            view.page_PlayerTouchOpt.SetShow(false);
#else
            view.page_PlayerTouchOpt.SetActive(true);
#endif

            view.btn_menu.onClick.AddListener(() =>
            {
                UiManager.instance.ShowUi<UiPlaySceneMenuCtrl>();
            });
            view.btn_data.onClick.AddListener(() =>
            {
                if (PlayManager.instance.data.progress.blockProgramUid <= 0)
                {
                    UiManager.instance.ShowUi<UiPlayDataCtrl>();
                }
            });
            teamerCon = new UiContainer<UiTeamerCtrl>(view.go_teamer);

        }
        public override void OnShow()
        {
            Refresh();
        }

        public void Refresh()
        {
            teamerCon.Clear();
            foreach(var uid in GameManager.instance.curProgress.teamActive)
            {
                teamerCon.Add(new UiTeamerParam()
                {
                    data = CharacterProductForm.DataByUid[uid]
                });
            }

                teamerCon.Refresh();
        }

       
    }
    public partial class UiTeamerParam
    {
        public CharacterProductForm.Data data;
    }
    public partial class UiTeamerModel
    {
        public CharacterProductForm.Data data;

    }
    public partial class UiTeamerCtrl
    {

        public override void OnCreate()
        {



        }
        public override void OnShow()
        {
            model.data = param.data;
            Refresh();
        }
        public void Refresh()
        {
            view.txt_.text = model.data.name;
            var prm = model.data.paramDic[model.data.hpParamName];
            view.sld_hp.value = prm.v/ prm.max;

            view.sld_sp.gameObject.SetActive(false);
            view.img_.sprite = TexAssetForm.DataByName.GetDk(model.data.avatarTexName,GlobalNameHelper.GetDefaultCharacterTexName()).GetSprite();
        }
    }

}
