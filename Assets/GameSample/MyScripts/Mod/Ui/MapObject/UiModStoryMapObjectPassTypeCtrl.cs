using Form;
using System.Linq;
using Z_String;
using Z_Ui.Base;

namespace Ui.ModStory.ModStoryMapObject.ModStoryMapObjectPassType
{
    public partial class UiModStoryMapObjectPassTypeCtrl
    {
        private UiScrViewContainer<UiBigItemCtrl> itemCon;

        public override void OnCreate()
        {
            itemCon = new UiScrViewContainer<UiBigItemCtrl>(this, view.go_item, view.scr_items);
            view.btn_back.onClick.AddListener(() => parent.SelType(0));
        }

        public override void OnShow()
        {
            Refresh();
        }

        public void Refresh()
        {
            itemCon.Clear();
            foreach (var data in PassTypeForm.DataById.Values.OrderBy(data => data.id))
                itemCon.Add(new UiBigItemParam { data = data });
            itemCon.Add(new UiBigItemParam());
            itemCon.Refresh();
        }
    }

    public partial class UiBigItemParam
    {
        public PassTypeForm.Data data;
    }

    public partial class UiBigItemModel
    {
        public PassTypeForm.Data data;
    }

    public partial class UiBigItemCtrl
    {
        public override void OnCreate()
        {
            view.btn_new.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.CreatePassType();
                parent.Refresh();
            });
            view.btn_delete.onClick.AddListener(() =>
            {
                if (model.data == null)
                    return;
                ModManager.instance.assetCtrl.DeletePassType(model.data.id);
                parent.Refresh();
            });
            view.ipt_name.onFinishInput += value =>
            {
                if (model.data == null)
                    return;
                if (value == model.data.name ||
                    (!string.IsNullOrWhiteSpace(value) && StringHelper.IsUniqueName(PassTypeForm.DataByName.Keys, value)))
                {
                    ModManager.instance.assetCtrl.RenamePassType(model.data.id, value);
                }
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
            view.sta_exist.ChangeState(model.data == null ? 0 : 1);
            if (model.data != null)
                view.ipt_name.Set(model.data.name);
        }
    }
}
