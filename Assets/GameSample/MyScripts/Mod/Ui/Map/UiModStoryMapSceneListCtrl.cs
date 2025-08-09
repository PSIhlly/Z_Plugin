using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z_Ui.Base;
using Z_Texture;

namespace Ui.ModStory.ModStoryMap.ModStoryMapScene.ModStoryMapSceneList
{

    public partial class UiModStoryMapSceneListParam
    {

    }
    public partial class UiModStoryMapSceneListModel
    {
        public SceneForm.Data data;
    }
    public partial class UiModStoryMapSceneListCtrl
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
            foreach (var data in SceneForm.DataByName.Values)
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

    public partial class UiItemParam
    {
        public SceneForm.Data data;
    }
    public partial class UiItemModel
    {
        public SceneForm.Data data;
    }
    public partial class UiItemCtrl
    {

        public override void OnCreate()
        {

            view.btn_new.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.CreateScene();
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
                view.img_.sprite = StoryTexAssetForm.DataByName[model.data.miniMap].sprite;
            }
        }
    }


}