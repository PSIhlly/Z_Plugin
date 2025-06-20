using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z_Ui.Base;
using Z_Texture;
using Z_String;

namespace Ui.ModStory.ModStoryCharacter.ModStoryCharacterUnit.ModStoryCharacterUnitOverview
{

    public partial class UiModStoryCharacterUnitOverviewParam
    {
        public CharacterProductForm.Data data;
    }
    public partial class UiModStoryCharacterUnitOverviewModel
    {
        public CharacterProductForm.Data data;

    }
    public partial class UiModStoryCharacterUnitOverviewCtrl
    {

        public override void OnCreate()
        {

            view.btn_delete.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.DeleteCharacter(model.data.name);
                parent.parent.SelPage(0);

            });
            view.ipt_name.onEndEdit.AddListener((s) =>
            {
                var lst = new List<string>();
                foreach(var data in CharacterProductForm.DataByNameIsproto.Values)
                    lst.Add(data.name);

                if(StringHelper.IsUniqueName(lst, s))
                    model.data.name = s;
            });
            view.ipt_label.onEndEdit.AddListener((s) =>
            {
                model.data.label = s;
            });
            view.btn_image.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.ImportCharacterAvatar(model.data.name);
            });

        }
        public override void OnShow()
        {
            if (param != null)
                model.data = param.data;
            Refresh();
        }
        public void Refresh()
        {
            view.ipt_name.Set(model.data.name);
            view.ipt_label.Set(model.data.label);
            view.img_image.sprite = StoryTexAssetForm.DataByName[model.data.avatarTexName].sprite;
        }
    }

}