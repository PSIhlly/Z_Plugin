using Form;
using Ui.ParamShow;
using Ui.PlayData;
using Ui.PlaySceneMenu;
using UnityEngine;
using Z_DataSystem.Form;
using Z_DesignStyle;
using Z_Ui;
using Z_Ui.Base;

namespace Ui.PlaySceneMain
{

    public partial class UiPlaySceneMainCtrl
    {
        Color[] colors = new Color[] { Color.red, Color.blue, Color.yellow };
        UiContainer<UiTeamerCtrl> teamerCon;
        UiContainer<UiParamShowCtrl> prmCon;
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
                if (GameManager.instance.curProgress.blockProgramUid <= 0)
                {
                    UiManager.instance.ShowUi<UiPlayDataCtrl>();
                }
            });
            teamerCon = new UiContainer<UiTeamerCtrl>(view.go_teamer);
            prmCon = new UiContainer<UiParamShowCtrl>(view.model_ParamShow.gameObject);
        }
        public override void OnShow()
        {
            Refresh();
        }

        public void Refresh()
        {
            teamerCon.Clear();
            foreach (var uid in GameManager.instance.curProgress.teamActive)
            {
                teamerCon.Add(new UiTeamerParam()
                {
                    data = CharacterProductForm.DataByUid[uid]
                });
            }
            teamerCon.Refresh();

            int id = 0;
            var cur = CharacterProductForm.DataByUid[GameManager.instance.curProgress.characterUid];
            prmCon.Clear();
            foreach (var prm in cur.paramDic.Values)
            {
                if (prm.showType == ParamShowType.AlwaysWithPanel)
                {
                    prmCon.Add(new UiParamShowParam() { max = prm.max - prm.min, value = prm.v - prm.min, color = colors[id] });
                    if (id < colors.Length - 1)
                    {
                        id++;
                    }
                }
            }
            prmCon.Refresh();
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
        Color[] colors = new Color[] { Color.red, Color.blue, Color.yellow };
        UiContainer<UiParamShowCtrl> con;
        public override void OnCreate()
        {
            con = new UiContainer<UiParamShowCtrl>(view.model_ParamShow.gameObject);
        }
        public override void OnShow()
        {
            model.data = param.data;
            Refresh();
        }
        public void Refresh()
        {
            view.txt_.text = model.data.name;

            con.Clear();
            int id = 0;
            foreach (var prm in model.data.paramDic.Values)
            {
                if (prm.showType == ParamShowType.AlwaysWithPanel)
                {
                    con.Add(new UiParamShowParam() { max = prm.max - prm.min, value = prm.v - prm.min, color = colors[id] });
                    if (id < colors.Length - 1)
                    {
                        id++;
                    }
                }
            }
            con.Refresh();

            view.img_.sprite = TexAssetForm.DataByName.GetDk(model.data.avatarTexName, GlobalNameHelper.GetDefaultCharacterTexName()).GetSprite();

        }
    }

}
