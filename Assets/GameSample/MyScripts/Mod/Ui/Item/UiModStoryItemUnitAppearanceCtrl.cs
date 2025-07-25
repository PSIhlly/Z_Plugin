using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z_Ui.Base;
using Z_Texture;
using UnityEngine.UIElements;
using Z_Text;

namespace Ui.ModStory.ModStoryItem.ModStoryItemUnit.ModStoryItemUnitAppearance
{

    public partial class UiModStoryItemUnitAppearanceParam
    {

        public ItemProductForm.Data data;
    }
    public partial class UiModStoryItemUnitAppearanceModel
    {

        public ItemProductForm.Data data;
    }
    public partial class UiModStoryItemUnitAppearanceCtrl
    {
        UiScrViewContainer<UiStyleCtrl> styleCon;
        public override void OnCreate()
        {
            styleCon = new UiScrViewContainer<UiStyleCtrl>(view.go_style, view.scr_styles);

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
            styleCon.Clear();
            foreach(ItemStyle style in Enum.GetValues(typeof(ItemStyle)))
            {
                styleCon.Add(new UiStyleParam()
                {
                    style = style
                });
            }
            styleCon.Refresh();
            
        }
    }
    public partial class UiStyleParam
    {

        public ItemStyle style;
    }
    public partial class UiStyleModel
    {

        public ItemStyle style;
    }
    public partial class UiStyleCtrl
    {
        public override void OnCreate()
        {
            view.btn_.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.ImportItemStyleTex(parent.model.data.name,model.style);
            });
        }
        public override void OnShow()
        {
            if (param != null)
            {
                model.style = param.style;
            }
            Refresh();
        }

        public void Refresh()
        {
            view.txt_.text = TextManager.instance.GetTxt(model.style.ToString());
            view.img_.sprite = StoryTexAssetForm.DataByName[parent.model.data.styleTex[model.style]].sprite;
        }
    }
}