using System.Linq;
using Z_Code;
using Z_Code.Form;
using Z_Ui.Base;

namespace Ui.ModStory.ModStoryEvent.ModStoryGlobalVar
{
    public partial class UiModStoryGlobalVarCtrl
    {
        private UiScrViewContainer<UiArgCtrl> argCon;

        public override void OnCreate()
        {
            argCon = new UiScrViewContainer<UiArgCtrl>(this, view.go_arg, view.scr_args);
        }

        public override void OnShow()
        {
            Refresh();
        }

        public void Refresh()
        {
            argCon.Clear();
            foreach (var data in EventGlobalVariableForm.DataByUid.Values.OrderBy(data => data.uid))
                argCon.Add(new UiArgParam { data = data });
            argCon.Add(new UiArgParam());
            argCon.Refresh();
        }
    }

    public partial class UiArgParam
    {
        public EventGlobalVariableForm.Data data;
    }

    public partial class UiArgModel
    {
        public EventGlobalVariableForm.Data data;
    }

    public partial class UiArgCtrl
    {
        public override void OnCreate()
        {
            view.btn_new.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.CreateGlobalVar();
                parent.Refresh();
            });
            view.btn_delete.onClick.AddListener(() =>
            {
                var data = model.data;
                if (!IsRegistered(data))
                    return;
                ModManager.instance.assetCtrl.DeleteGlobalVar(data.uid);
                parent.Refresh();
            });
            view.ipt_name.onFinishInput += name =>
            {
                var data = model.data;
                if (!IsRegistered(data))
                    return;
                ModManager.instance.assetCtrl.RenameGlobalVar(data.uid, name);
                Refresh();
            };
            view.btn_value.onClick.AddListener(() =>
            {
                var data = model.data;
                if (!IsRegistered(data))
                    return;
                // Edit a draft: cancel or a removed/replaced row must not publish a value.
                var value = data.data?.DeepCopy() ?? CodeHelper.CreateBoxByNum(0);
                ModManager.instance.assetCtrl.ChooseBasicCmd(value, () =>
                {
                    if (!IsRegistered(data))
                        return;
                    data.data = value;
                    if (active && ReferenceEquals(model.data, data))
                        Refresh();
                });
            });
        }

        public override void OnShow()
        {
            model.data = param?.data;
            if (param == null)
            {
                gameObject.SetActive(false);
                return;
            }
            Refresh();
        }

        public void Refresh()
        {
            view.sta_exist.ChangeState(model.data == null ? 0 : 1);
            if (model.data == null)
                return;
            view.ipt_name.Set(model.data.name);
            view.txt_value.text = model.data.data?.ToString() ?? "0";
        }

        private static bool IsRegistered(EventGlobalVariableForm.Data data) => data != null
            && EventGlobalVariableForm.DataByUid.TryGetValue(data.uid, out var current)
            && ReferenceEquals(current, data);
    }
}
