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
    public partial class UiModStoryMapObjectTextureAppearanceCtrl
    {
        UiScrViewContainer<UiItemCtrl> con;
        public override void OnCreate()
        {
            con = new UiScrViewContainer<UiItemCtrl>(view.go_item,view.scr_items);
            view.btn_delete.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.DeleteTex(model.data.name);
            });
            view.ipt_label.onFinishInput += (s) =>
            {
                model.data.label = s;
            };
            view.ipt_name.onFinishInput += (s) =>
            {
                model.data.name = s;
            };
            view.ipt_interval.onFinishInput += (s) =>
            {
                model.data.animTimeInterval = StringHelper.ToFloat(s, 0, true);
            };
            view.btn_deleteTex.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.DeleteTexId(model.data.name,model.id);
            });
            view.btn_image.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.ImportTex(model.data.name, model.id);
            });
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
            view.sta_show.ChangeState(model.id >= 0?1:0);

            con.Clear();
            for(int i=0;i<model.data.texsName.Count;i++)
            {
                con.Add(new UiItemParam()
                {
                   id=i
                });
            }
            con.Add(new UiItemParam()
            {
                id = -1
            });
            con.Refresh();

            if (model.id >= 0)
            {
                view.img_image.sprite = StoryTexAssetForm.DataByName[model.data.texsName[model.id]].sprite;
            }
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
                parent.model.data.texsName.Add("");
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
            view.sta_item.ChangeState(model.id != -1&&model.id==parent.model.id ?1 : 0);
            view.img_.sprite = StoryTexAssetForm.DataByName[parent.model.data.texsName[model.id]].sprite;
        }
    }

}