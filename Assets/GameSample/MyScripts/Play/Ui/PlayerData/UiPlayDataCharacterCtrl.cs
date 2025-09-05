using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z_Ui.Base;
using Z_Texture;
using Z_DataSystem.Form;
using Ui.PlayData.PlayDataCharacter.PlayDataCharacterData;
using Ui.PlayData.PlayDataCharacter.PlayDataCharacterEquip;
using Ui.PlayData.PlayDataCharacter.PlayDataCharacterSkill;

namespace Ui.PlayData.PlayDataCharacter
{

    public partial class UiPlayDataCharacterParam
    {

    }
    public partial class UiPlayDataCharacterModel
    {
        public CharacterProductForm.Data sel;
        public int module;
    }
    public partial class UiPlayDataCharacterCtrl
    {

        UiScrViewContainer<UiGameItemCtrl> itemCon;
        public override void OnCreate()
        {

            view.btn_data.onClick.AddListener(() =>
            {
                model.module = 1;
                Refresh();
            });
            view.btn_equip.onClick.AddListener(() =>
            {
                model.module = 2;
                Refresh();

            });
            view.btn_skill.onClick.AddListener(() =>
            {
                model.module = 3;
                Refresh();

            });

            itemCon = new UiScrViewContainer<UiGameItemCtrl>(view.go_gameItem, view.scr_gameCharacters);

        }
        public override void OnShow()
        {
            model.sel = null;
            if (PlayManager.instance.data.progress.team.Count>0)
            {
                model.sel = CharacterProductForm.DataByUid[PlayManager.instance.data.progress.team[0]];
            }
            Refresh();
        }
        public void Refresh()
        {

            itemCon.Clear();
            for (int i = 0, icnt = PlayManager.instance.data.progress.team.Count; i < icnt; i++)
            {
                var data = CharacterProductForm.DataByUid[PlayManager.instance.data.progress.team[i]];
                itemCon.Add(new UiGameItemParam()
                {
                    data = data
                });

            }
            itemCon.Refresh();


            view.sta_data.ChangeState(model.module == 1 ? 1 : 0);
            view.page_PlayDataCharacterData.SetActive(model.module == 1, new UiPlayDataCharacterDataParam()
            {
                data = model.sel
            });
            view.sta_equip.ChangeState(model.module == 2 ? 1 : 0);
            view.page_PlayDataCharacterEquip.SetActive(model.module == 2, new UiPlayDataCharacterEquipParam()
            {
                data = model.sel
            });
            view.btn_skill.gameObject.SetActive(false);
            /*            view.sta_skill.ChangeState(model.module == 3 ? 1 : 0);
                        view.page_PlayDataCharacterSkill.SetActive(model.module == 3, new UiPlayDataCharacterSkillParam()
                        {
                            data = model.sel
                        });*/
        }
        public void Sel(CharacterProductForm.Data data)
        {
            model.sel = data;
            Refresh();
        }
    }


    public partial class UiGameItemParam
    {

        public CharacterProductForm.Data data;
    }
    public partial class UiGameItemModel
    {

        public CharacterProductForm.Data data;
    }
    public partial class UiGameItemCtrl
    {

        public override void OnCreate()
        {

            view.btn_.onClick.AddListener(() =>
            {
                parent.Sel(model.data);
            });

        }
        public override void OnShow()
        {
            if (param != null)
            {
                model.data = param.data;
            }
            Refresh();
        }
        public void Refresh()
        {
            view.sta_exist.ChangeState(1);
            view.txt_.text = model.data.name;
            view.img_.sprite = TexAssetForm.DataByName[model.data.avatarTexName].sprite;
            view.sta_.ChangeState(parent.model.sel == model.data ? 1 : 0);
        }
    }


}