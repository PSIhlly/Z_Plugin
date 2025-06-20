using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z_Ui.Base;
using Z_Texture;

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

        UiScrViewContainer<UiItemCtrl> itemCon;
        public override void OnCreate()
        {

            itemCon = new UiScrViewContainer<UiItemCtrl>(view.go_item, view.scr_items);

        }
        public override void OnShow()
        {

            Refresh();
        }
        public void Refresh()
        {

            itemCon.Clear();
            foreach (var data in model.data.animDic.Values)
            {
                itemCon.Add(new UiItemParam()
                {
                    data= data
                });
            }
            itemCon.Refresh();
        }
    }

    public partial class UiItemParam
    {
        public CharacterAnimForm.Data data;
    }
    public partial class UiItemModel
    {
        public CharacterAnimForm.Data data;
    }
    public partial class UiItemCtrl
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

            Refresh();
        }
        public void Refresh()
        {
            view.sta_item.ChangeState(model.data == null ? 0 : 1);
            if (model.data != null) {
                view.txt_.text = model.data.name;
                view.img_.sprite = StoryTexAssetForm.DataByName[model.data.animClip.Count>0? model.data.animClip[0].partTex[0] : ""].sprite;
            }
        }
    }


}