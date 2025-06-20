using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z_Ui.Base;
using Z_Texture;

namespace Ui.ModStory.ModStoryCharacter.ModStoryCharacterList
{

    public partial class UiModStoryCharacterListParam
    {

    }
    public partial class UiModStoryCharacterListModel
    {
        public string lab;
        public CharacterProductForm.Data data;
    }
    public partial class UiModStoryCharacterListCtrl
    {

        UiScrViewContainer<UiLabCtrl> labCon;
        UiScrViewContainer<UiItemCtrl> itemCon;
        public override void OnCreate()
        {

            labCon = new UiScrViewContainer<UiLabCtrl>(view.go_lab, view.scr_labs);
            itemCon = new UiScrViewContainer<UiItemCtrl>(view.go_item, view.scr_items);

        }
        public override void OnShow()
        {

            Refresh();
        }
        public void Refresh()
        {

            labCon.Clear();
            labCon.Add(new UiLabParam()
            {
                lab = null
            });
            foreach (var lab in CharacterProductForm.DatasByLabel.Keys)
            {
                labCon.Add(new UiLabParam()
                {
                    lab=lab
                });
            }
            labCon.Refresh();
            itemCon.Clear();
            var datas = model.lab == null || !CharacterProductForm.DatasByLabelIsproto.ContainsKey((model.lab, true)) ? CharacterProductForm.DatasByIsproto[true] : CharacterProductForm.DatasByLabelIsproto[(model.lab, true)];
            foreach (var data in datas)
            {
                itemCon.Add(new UiItemParam()
                {
                    data= data
                });
            }
            itemCon.Add(new UiItemParam()
            {
                data=null
            });
            itemCon.Refresh();



        }
    }

    public partial class UiLabParam
    {
        public string lab;
    }
    public partial class UiLabModel
    {
        public string lab;
    }
    public partial class UiLabCtrl
    {

        public override void OnCreate()
        {

            view.btn_.onClick.AddListener(() =>
            {
                parent.model.lab = model.lab;
            });

        }
        public override void OnShow()
        {

            Refresh();
        }
        public void Refresh()
        {

            view.sta_isEmpty.ChangeState(model.lab==null?0:1);
            if(model.lab != null)
            {
                view.txt_.text = model.lab;
            }
        }
    }


    public partial class UiItemParam
    {
        public CharacterProductForm.Data data;
    }
    public partial class UiItemModel
    {
        public CharacterProductForm.Data data;
    }
    public partial class UiItemCtrl
    {

        public override void OnCreate()
        {

            view.btn_new.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.CreateCharacter();
            });
            view.btn_.onClick.AddListener(() =>
            {
                parent.parent.SelPage(1,model.data);
            });

        }
        public override void OnShow()
        {

            Refresh();
        }
        public void Refresh()
        {
            view.sta_item.ChangeState(model.data == null ? 0 : 1);
            if (model.data != null)
            {
                view.txt_.text = model.data.name;
                view.img_.sprite = StoryTexAssetForm.DataByName[model.data.avatarTexName].sprite;
            }
        }
    }


}