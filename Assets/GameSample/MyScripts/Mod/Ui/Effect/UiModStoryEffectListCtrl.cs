using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z_Ui.Base;
using Z_Texture;
using Z_DataSystem.Form;
using Z_Ui;

namespace Ui.ModStory.ModStoryEffect.ModStoryEffectList
{

    public partial class UiModStoryEffectListParam
    {

    }
    public partial class UiModStoryEffectListModel
    {
        public string lab;
        public EffectForm.Data data;
    }
    public partial class UiModStoryEffectListCtrl
    {

        UiScrViewContainer<UiLabCtrl> labCon;
        UiScrViewContainer<UiBigItemCtrl> itemCon;
        public override void OnCreate()
        {

            labCon = new UiScrViewContainer<UiLabCtrl>(this, view.go_lab, view.scr_labs);
            itemCon = new UiScrViewContainer<UiBigItemCtrl>(this, view.go_bigItem, view.scr_bigItems);

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
            foreach (var lab in EffectForm.DatasByLabel.Keys)
            {
                if(lab!="")
                labCon.Add(new UiLabParam()
                {
                    lab = lab
                });
            }
            labCon.Refresh();
            itemCon.Clear();
            var datas = string.IsNullOrEmpty(model.lab) ? new List<EffectForm.Data>(EffectForm.DataByUid.Values) :
                (EffectForm.DatasByLabel.ContainsKey(model.lab) ? EffectForm.DatasByLabel[model.lab] : new List<EffectForm.Data>());

            foreach (var data in datas.OrderBy(d => d.uid))
            {
                itemCon.Add(new UiBigItemParam()
                {
                    data = data
                });
            }
            itemCon.Add(new UiBigItemParam()
            {
                data = null
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
                parent.Refresh();
            });

        }
        public override void OnShow()
        {
            model.lab = param.lab;
            Refresh();
        }
        public void Refresh()
        {

            view.sta_valid.ChangeState(model.lab == null ? 0 : 1);
            view.sta_.ChangeState(model.lab == parent.model.lab ? 1 : 0);
            if (model.lab != null)
            {
                view.txt_.text = model.lab;
            }
        }
    }


    public partial class UiBigItemParam
    {
        public EffectForm.Data data;
    }
    public partial class UiBigItemModel
    {
        public EffectForm.Data data;
    }
    public partial class UiBigItemCtrl
    {

        public override void OnCreate()
        {

            view.btn_new.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.CreateEffect();
                parent.Refresh();
            });
            view.btn_.onClick.AddListener(() =>
            {
                parent.parent.SelPage(1, model.data);
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
                view.img_.BindTexData(model.data.clips.Count > 0 ? TexAssetForm.DataById[model.data.clips[0][0].tex] : TexAssetForm.DataById[GlobalDefaultHelper.DefaultTexId]);
            }
        }
    }


}