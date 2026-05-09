using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z_Ui.Base;
using Z_Texture;
using Ui.ModStory.ModStoryMission.ModStoryMissionUnit;

namespace Ui.ModStory.ModStoryMission
{

    public partial class UiModStoryMissionParam
    {
        public int selPage;
    }
    public partial class UiModStoryMissionModel
    {
        public int selPage;
        public MissionForm.Data data;
    }
    public partial class UiModStoryMissionCtrl
    {

        public override void OnCreate()
        {


        }
        public override void OnShow()
        {
            model.selPage = 0;
            if (param != null)
                model.selPage = param.selPage;
            Refresh();
        }

        public void SelPage(int id, MissionForm.Data data = null)
        {
            model.selPage = id;
            model.data = data;
            Refresh();
        }
        public void Refresh()
        {
            view.page_ModStoryMissionList.SetShow(model.selPage == 0);
            view.page_ModStoryMissionUnit.SetShow(model.selPage == 1, new UiModStoryMissionUnitParam() { data = model.data });
        }
    }

}
