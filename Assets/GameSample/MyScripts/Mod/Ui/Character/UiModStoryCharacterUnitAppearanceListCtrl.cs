using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z_Ui.Base;
using Z_Texture;
using Z_DataSystem.Form;
using UnityEngine;

namespace Ui.ModStory.ModStoryCharacter.ModStoryCharacterUnit.ModStoryCharacterUnitAppearance.ModStoryCharacterUnitAppearanceList
{

    public partial class UiModStoryCharacterUnitAppearanceListParam
    {
        public CharacterProductForm.Data data;
    }
    public partial class UiModStoryCharacterUnitAppearanceListModel
    {
        public CharacterProductForm.Data data;
    }
    public partial class UiModStoryCharacterUnitAppearanceListCtrl
    {

        UiScrViewContainer<UiBigItemCtrl> itemCon;
        public override void OnCreate()
        {

            itemCon = new UiScrViewContainer<UiBigItemCtrl>(view.go_bigItem, view.scr_bigItems);

        }
        public override void OnShow()
        {
            model.data = param.data;
            Refresh();
        }
        public void Refresh()
        {

            itemCon.Clear();
            foreach (var data in model.data.animDic.Values)
            {
                itemCon.Add(new UiBigItemParam()
                {
                    data= data
                });
            }
            itemCon.Add(new UiBigItemParam()
            {
                data = null
            });
            itemCon.Refresh();
        }
    }

    public partial class UiBigItemParam
    {
        public CharacterAnimForm.Data data;
    }
    public partial class UiBigItemModel
    {
        public CharacterAnimForm.Data data;
    }
    public partial class UiBigItemCtrl
    {

        public override void OnCreate()
        {

            view.btn_new.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.CreateCharacterAnim(parent.model.data.name);
                parent.Refresh();
            });
            view.btn_.onClick.AddListener(() =>
            {
                parent.parent.SelPage(1, model.data);
            });

        }
        public override void OnShow()
        {
            model.data=param.data;
            Refresh();
        }
        public void Refresh()
        {
            view.sta_exist.ChangeState(model.data == null ? 0 : 1);
            if (model.data != null) {
                view.txt_.text = model.data.name;
                if(model.data.animClip.Count > 0&& model.data.animClip[0].partTex.ContainsKey(BodyPartType.UpperPart))
                {
                    view.img_.sprite = TexAssetForm.DataByName[model.data.animClip[0].partTex[BodyPartType.UpperPart]].GetSprite();
                }
                else
                {
                    view.img_.sprite = TexAssetForm.DataByName[GlobalNameHelper.GetDefaultTexName()].GetSprite();
                }
            }
        }
    }


}