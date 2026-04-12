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
using Z_DesignStyle;
using Z_Map.Analysis;

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

            itemCon = new UiScrViewContainer<UiBigItemCtrl>(this, view.go_bigItem, view.scr_bigItems);

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
                    data = data
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
                ModManager.instance.assetCtrl.CreateCharacterAnim(parent.model.data.uid, parent.model.data.name);
                parent.Refresh();
            });
            view.btn_.onClick.AddListener(() =>
            {
                parent.parent.SelPage(1, model.data);
            });

        }
        public override void OnShow()
        {
            model.data = param.data;
            Refresh();
        }
        public void Refresh()
        {
            view.sta_exist.ChangeState(model.data == null ? 0 : 1);
            if (model.data != null)
            {
                view.txt_.text = model.data.name;
                string icon = GlobalNameHelper.GetDefaultTexName();
                switch (parent.model.data.faceType)
                {
                    case FaceType.Fixed:
                    case FaceType.Flexible:
                        if (model.data.animClip[AnimDirecton.Fixed].Count > 0)
                        {
                            icon = model.data.animClip[AnimDirecton.Fixed][0].partTex.GetDv(BodyPartType.UpperPart, icon);
                        }
                        break;
                    case FaceType.FourDirection:
                        if (model.data.animClip[AnimDirecton.Up].Count > 0)
                        {
                            icon = model.data.animClip[AnimDirecton.Up][0].partTex.GetDv(BodyPartType.UpperPart, icon);
                        }
                        break;
                }

                view.img_.sprite = TexAssetForm.DataByName[icon].GetSprite();
            }
        }
    }


}