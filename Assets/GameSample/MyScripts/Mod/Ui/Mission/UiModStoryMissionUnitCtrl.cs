using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z_Ui.Base;
using Z_Texture;
using Z_String;
using Z_Text;

namespace Ui.ModStory.ModStoryMission.ModStoryMissionUnit
{

    public partial class UiModStoryMissionUnitParam
    {
        public MissionForm.Data data;
    }
    public partial class UiModStoryMissionUnitModel
    {
        public MissionForm.Data data;
    }
    public partial class UiModStoryMissionUnitCtrl
    {

        public override void OnCreate()
        {
            view.btn_back.onClick.AddListener(() =>
            {
                parent.SelPage(0);
            });
            view.btn_delete.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.DeleteMission(model.data.name);
                parent.SelPage(0);
            });
            view.btn_show.onClick.AddListener(() =>
            {
                model.data.show = !model.data.show;
                Refresh();
            });
            view.ipt_name.onFinishInput += (s) =>
            {
                if (StringHelper.IsUniqueName(MissionForm.DataByName.Keys, s))
                {
                    ModManager.instance.assetCtrl.RenameMission(model.data.name, s);
                }
                Refresh();
            };
            view.ipt_lab.onFinishInput += (s) =>
            {
                model.data.label = s;
                Refresh();
            };
            view.ipt_desc.onFinishInput += (s) =>
            {
                model.data.desc = s;
                Refresh();
            };

        }
        public override void OnShow()
        {
            if (param != null)
            {
                model.data = param.data;
            }
            Refresh();
        }
        public void Refresh()
        {
            if (model.data != null)
            {
                view.ipt_name.Set(model.data.name);
                view.ipt_lab.Set(model.data.label);
                view.ipt_desc.Set(model.data.desc);
                view.sta_show.ChangeState(model.data.show ? 1 : 0);
            }
        }
    }

}
