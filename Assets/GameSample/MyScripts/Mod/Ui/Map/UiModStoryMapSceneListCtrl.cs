using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z_Ui.Base;
using Z_Texture;
using Z_DataSystem.Form;
using Z_DataSystem;
using Z_Map.Form;
using UnityEngine;

namespace Ui.ModStory.ModStoryMap.ModStoryMapScene.ModStoryMapSceneList
{

    public partial class UiModStoryMapSceneListParam
    {

    }
    public partial class UiModStoryMapSceneListModel
    {
        public SceneForm.Data data;
    }
    public partial class UiModStoryMapSceneListCtrl:IZ_Listener<AssetEvent>
    {

        UiScrViewContainer<UiBigItemCtrl> itemCon;
        public override void OnCreate()
        {
            Z_EventHelper.Register(this);
            itemCon = new UiScrViewContainer<UiBigItemCtrl>(this, view.go_bigItem, view.scr_bigItems);

        }

        public void OnEvent(AssetEvent evt)
        {
            if (active)
                Refresh();
        }

        public override void OnShow()
        {

            Refresh();
        }
        public void Refresh()
        {

          
            itemCon.Clear();
            foreach (var data in SceneForm.DataByName.Values.OrderBy(d => d.uid))
            {
                itemCon.Add(new UiBigItemParam()
                {
                    data= data
                });
            }
            itemCon.Add(new UiBigItemParam()
            {
                data=null
            });
            itemCon.Refresh();



        }
    }

    public partial class UiBigItemParam
    {
        public SceneForm.Data data;
    }
    public partial class UiBigItemModel
    {
        public SceneForm.Data data;
    }
    public partial class UiBigItemCtrl
    {

        public override void OnCreate()
        {

            view.btn_new.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.CreateScene();
                parent.Refresh();
            });
            view.btn_.onClick.AddListener(() =>
            {
                parent.parent.SelPage(1,model.data);
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
                view.img_.BindTexData(TexAssetForm.DataById[model.data.miniMap]);
            }
        }
    }


}