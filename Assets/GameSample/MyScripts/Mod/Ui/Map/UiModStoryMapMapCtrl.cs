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
using Z_DataSystem.Form;

namespace Ui.ModStory.ModStoryMap.ModStoryMapMap
{

    public partial class UiModStoryMapMapParam
    {

    }
    public partial class UiModStoryMapMapModel
    {

    }
    public partial class UiModStoryMapMapCtrl : IZ_Listener<AssetEvent>
    {
        UiContainer<UiMapSceneCtrl> con;
        public override void OnCreate()
        {
            con = new UiContainer<UiMapSceneCtrl>(view.go_mapScene);
            view.btn_import.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.ImportMapMiniMap();
            });

        }

        public void OnEvent(AssetEvent evt)
        {
            Refresh();
        }

        public override void OnShow()
        {

            Refresh();
        }
        public override void Close()
        {
            GameManager.instance.saveCtrl.SaveConfig(ModManager.instance.GetStoryCoreFolder());
            base.Close();

        }
        public void Refresh()
        {
            view.img_map.sprite = TexAssetForm.DataByName[GameManager.instance.curConfig.miniMap].sprite;
            con.Clear();
            foreach (var data in SceneForm.DataByUid.Values)
            {
                con.Add(new UiMapSceneParam()
                {
                    data = data
                });
            }
            con.Refresh();
        }
    }
    public partial class UiMapSceneParam
    {
        public SceneForm.Data data;
    }
    public partial class UiMapSceneModel
    {
        public SceneForm.Data data;

    }
    public partial class UiMapSceneCtrl : IZ_Listener<AssetEvent>
    {

        public override void OnCreate()
        {
            Z_EventHelper.Register(this);
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
            base.Close();
        }
        public void Refresh()
        {
            view.txt_.text = model.data.name;
            gameObject.transform.position = Z_Math.Graph.GetRelativePos(new Vector2(model.data.pos.Item1, model.data.pos.Item2),parent.rect);
        }
    }
}