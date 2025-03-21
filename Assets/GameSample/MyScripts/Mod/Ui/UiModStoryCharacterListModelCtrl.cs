using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z_Ui.Base;

namespace Ui.ModStoryCharacterListModel
{
    public partial class UiModStoryCharacterListModelParam
    {
        public CharacterProductForm.Data data;
    }
    public partial class UiModStoryCharacterListModelModel
    {
        public CharacterProductForm.Data data;
        public int animId;
        public int part;
        public int id;
    }
    public partial class UiModStoryCharacterListModelCtrl
    {
        UiScrViewContainer<UiItemCtrl> con;
        public override void OnCreate()
        {
            con = new UiScrViewContainer<UiItemCtrl>(view.go_unit, view.scr_units);
            view.btn_back.onClick.AddListener(() =>
            {
                Close();
            });
            view.btn_delete.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.DeleteCharacterAnim(model.data.name,model.data.animName[model.animId], model.part, model.id);
                Refresh();
            });
            view.btn_delete.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.DeleteCharacterAnim(model.data.name, model.data.animName[model.animId], model.part, model.id);
                Refresh();
            });
            view.ipt_name.onFinishInput += (s) =>
            {
                ModManager.instance.assetCtrl.RenameCharacterAnim(model.data.name, model.data.animName[model.animId], s);
                Refresh();
            };
        }
        public override void OnShow()
        {
            model.data = param.data;
            Refresh();
        }
        public void Refresh()
        {
            con.Clear();
            foreach (var data in CharacterParamForm.DataByUid.Values)
            {
                con.Add(new UiItemParam()
                {
                   // data = data
                });
            }
            con.Refresh();

            view.ipt_name.Set(model.data.animName[model.animId]);
            view.ipt_interval.Set(model.data.animTimeInterval[model.animId].ToString("#0.00"));
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
            view.btn_item.onClick.AddListener(() =>
            {

            });
        }
        public override void OnShow()
        {
            model.id = param.id;
            Refresh();
        }
        public void Refresh()
        {
            
        }

    }


}
