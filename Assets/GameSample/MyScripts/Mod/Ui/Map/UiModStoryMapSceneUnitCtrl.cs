using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z_Ui.Base;
using Z_Texture;
using Z_DataSystem;
using UnityEngine;
using Z_Ui;
using Z_DataSystem.Form;

namespace Ui.ModStory.ModStoryMap.ModStoryMapScene.ModStoryMapSceneUnit
{

    public partial class UiModStoryMapSceneUnitParam
    {
        public SceneForm.Data data;
    }
    public partial class UiModStoryMapSceneUnitModel
    {
        public SceneForm.Data data;
    }
    public partial class UiModStoryMapSceneUnitCtrl : IZ_Listener<AssetEvent>
    {

        public override void OnCreate()
        {
            Z_EventHelper.Register(this);
            view.btn_map.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.ImportSceneMiniMap(model.data.name);
            });
            view.ipt_name.onFinishInput += (s) =>
            {
                model.data.name = s;
                Refresh();
            };
            view.btn_delete.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.DeleteScene(model.data.name);
                parent.SelPage(0);
            });
            view.btn_edit.onClick.AddListener(() =>
            {
                Main2StoryManager.instance.StartLoadSceneUgc(model.data.name);
                UiManager.instance.CloseUi<UiModStoryCtrl>();
            });
        }

        public void OnEvent(AssetEvent evt)
        {
            Refresh();
        }

        public override void OnShow()
        {
            model.data = param.data;
            Refresh();
        }
        public override void Close()
        {
            GameManager.instance.saveCtrl.SaveScene(ModManager.instance.GetStoryCoreFolder());
            base.Close();

        }
        public void Refresh()
        {
            view.img_map.sprite = TexAssetForm.DataByName[model.data.miniMap].sprite;
            view.ipt_name.Set(model.data.name);
        }
    }
}