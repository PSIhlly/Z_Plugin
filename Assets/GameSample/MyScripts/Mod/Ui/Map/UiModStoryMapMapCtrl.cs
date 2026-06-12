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
using Ui.Axis;
using Z_DesignStyle;

namespace Ui.ModStory.ModStoryMap.ModStoryMapMap
{

    public partial class UiModStoryMapMapParam
    {

    }
    public partial class UiModStoryMapMapModel
    {
        public SceneForm.Data sel;
    }
    public partial class UiModStoryMapMapCtrl : IZ_Listener<AssetEvent>
    {
        UiContainer<UiMapSceneCtrl> con;
        public override void OnCreate()
        {
            Z_EventHelper.Register(this);
            con = new UiContainer<UiMapSceneCtrl>(this, view.go_mapScene);
            view.btn_import.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.ImportMapMiniMap();
            });

        }

        public void OnEvent(AssetEvent evt)
        {
            if (active)
                Refresh();
        }

        public override void OnShow()
        {
            model.sel = null;
            Refresh();
        }
        public override void Close()
        {
            base.Close();

        }
        public void Refresh()
        {
            view.img_map.BindTexData(TexAssetForm.DataById.GetDv(GameManager.instance.curProgress.largeMap, TexAssetForm.DataById[GlobalDefaultHelper.DefaultTexId]));

            RefreshScenes();
            view.model_axis.SetShow(false);

            if (model.sel != null)
            {
                view.model_axis.SetShow(true, new UiAxisParam()
                {
                    pos = model.sel.pos,
                    limitRtf = view.img_map.rectTransform,
                    noRotate = true,
                    onTrsChange = (tp) =>
                    {
                        model.sel.pos = tp.Item1;
                        RefreshScenes();
                    }
                });
            }

        }
        private void RefreshScenes()
        {
            con.Clear();
            foreach (var data in SceneForm.DataByUid.Values)
            {
                if (data.hideInLargeMap)
                    continue;
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
    public partial class UiMapSceneCtrl
    {
        public override void OnCreate()
        {
            view.btn_.onClick.AddListener(() =>
            {
                parent.model.sel = model.data;
                parent.Refresh();
            });
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
            gameObject.transform.position = Z_Math.Graph.GetRealPos(model.data.pos, parent.view.img_map.rectTransform);
        }
    }
}