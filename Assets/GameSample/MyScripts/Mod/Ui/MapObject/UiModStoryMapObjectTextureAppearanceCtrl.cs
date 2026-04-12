using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z_Ui.Base;
using Z_Texture;
using Ui.ModStory.ModStoryOverview;
using Z_String;
using System.Drawing;
using Z_DataSystem.Form;
using Z_DataSystem;

namespace Ui.ModStory.ModStoryMapObject.ModStoryMapObjectTexture.ModStoryMapObjectTextureAppearance
{

    public partial class UiModStoryMapObjectTextureAppearanceParam
    {
        public MapTextureForm.Data data;
        public int id;
    }
    public partial class UiModStoryMapObjectTextureAppearanceModel
    {
        public MapTextureForm.Data data;

        public int id;
    }
    public partial class UiModStoryMapObjectTextureAppearanceCtrl:IZ_Listener<AssetEvent>
    {
        UiScrViewContainer<UiItemCtrl> con;
        public override void OnCreate()
        {
            Z_EventHelper.Register(this);
            con = new UiScrViewContainer<UiItemCtrl>(this, view.go_item, view.scr_items);
            view.btn_delete.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.DeleteTex(model.data.name);
                parent.parent.SelType(1);
            });
            view.ipt_label.onFinishInput += (s) =>
            {
                model.data.label = s;
                Refresh();
            };
            view.ipt_name.onFinishInput += (s) =>
            {
                model.data.name = s;
                Refresh();
            };
            view.ipt_interval.onFinishInput += (s) =>
            {
                model.data.animTimeInterval = StringHelper.ToFloat(s, 0, true);
                Refresh();
            };
            view.btn_deleteTex.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.DeleteTexId(model.data.name, model.id);
                model.id = -1;
                Refresh();
            });
            view.btn_image.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.ImportTex(model.data.name, model.id);
            });
        }

        public void OnEvent(AssetEvent evt)
        {
            if (active)
                Refresh();
        }

        public override void OnShow()
        {
            model.id = -1;
            if (param != null)
            {
                model.id = param.id;
                model.data = param.data;
            }
            Refresh();
        }
        public void Refresh()
        {
            view.sta_show.ChangeState(model.id >= 0 ? 1 : 0);

            con.Clear();
            for (int i = 0; i < model.data.texsName.Count; i++)
            {
                con.Add(new UiItemParam()
                {
                    id = i
                });
            }
            con.Add(new UiItemParam()
            {
                id = -1
            });
            con.Refresh();

            if (model.id >= 0)
            {
                view.img_image.sprite = TexAssetForm.DataByName[model.data.texsName[model.id]].GetSprite();
            }
            view.ipt_name.Set(model.data.name);
            view.ipt_label.Set(model.data.label);
            view.ipt_interval.Set(model.data.animTimeInterval.ToString("0.##"));
        }
    }

    public partial class UiItemParam
    {
        public int id;
    }
    public partial class UiItemModel
    {
        public int id;
    }
    public partial class UiItemCtrl
    {

        public override void OnCreate()
        {

            view.btn_new.onClick.AddListener(() =>
            {
                parent.model.data.texsName.Add(GlobalNameHelper.GetDefaultTexName());
                parent.Refresh();
            });
            view.btn_.onClick.AddListener(() =>
            {
                parent.model.id = model.id;
                parent.Refresh();
            });

        }
        public override void OnShow()
        {
            model.id = param.id;
            Refresh();
        }
        public void Refresh()
        {
            view.sta_exist.ChangeState(model.id != -1 ? 1 : 0);
            if (model.id != -1)
            {
                view.sta_.ChangeState(model.id == parent.model.id ? 1 : 0);
                view.img_.sprite = TexAssetForm.DataByName[parent.model.data.texsName[model.id]].GetSprite();
            }
        }
    }

}