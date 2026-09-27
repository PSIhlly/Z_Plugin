using Form;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Z_Code;
using Z_Code.Form;
using Z_DataSystem.Form;
using Z_Debug;
using Z_DesignStyle;
using Z_Map;
using Z_Map.Form;
using Z_Text;
using Z_Texture;
using Z_Time;
using Z_Ui.Base;
using Z_UnitSystem.Form;
using static UnityEngine.Rendering.DebugUI.Table;

namespace Ui.PlayMission
{

    public partial class UiPlayMissionParam
    {

    }
    public partial class UiPlayMissionModel
    {
        public int cur = 0;
        public int y = 0;
        public bool isArea;
    }
    public partial class UiPlayMissionCtrl : IZ_Listener<StoryLifeEvent>, IZ_Listener<MissionEvent>
    {
        UiContainer<UiMissionCtrl> con;
        public override void OnCreate()
        {
            con = new UiContainer<UiMissionCtrl>(this, view.go_mission);

            view.btn_bg.onClick.AddListener(() =>
            {
                Close();
            });
        }
        public override void OnEnable()
        {

        }
        public override void OnDisable()
        {
        }
        public override void OnShow()
        {
            this.Register<StoryLifeEvent>();
            this.Register<MissionEvent>();
            Refresh();
        }
        public override void OnHide()
        {
            this.Unregister<StoryLifeEvent>();
            this.Unregister<MissionEvent>();
        }
        public void OnEvent(StoryLifeEvent evt)
        {
            if (!active || evt.type != StoryLifeEventType.EverySecond)
                return;
            foreach (var prm in con.paramLst)
                ((UiMissionCtrl)con.Get(prm).ctrl).RefreshDistance();
        }
        public void OnEvent(MissionEvent evt)
        {
            if (active)
                Refresh();
        }
        public void Refresh()
        {
            con.Clear();

            foreach (var o in MissionForm.DataById.Values)
            {
                if (o.show && o.received && !o.fail && !o.done)
                {
                    con.Add(new UiMissionParam()
                    {
                        data = o
                    });
                }
            }

            con.Refresh();
        }

    }
    public partial class UiMissionParam
    {
        public MissionForm.Data data;
    }
    public partial class UiMissionModel
    {
        public UiMissionParam prm;
    }
    public partial class UiMissionCtrl
    {
        public override void OnCreate()
        {
            view.btn_.onClick.AddListener(() =>
            {
                GameManager.instance.curProgress.curMissionId = model.prm.data.id;
                Z_EventHelper.Invoke(new MissionEvent { type = MissionEventType.Select, data = model.prm.data });
            });
        }
        public override void OnShow()
        {
            model.prm = param;
            Refresh();
        }

        public void Refresh()
        {
            view.sta_.ChangeState(GameManager.instance.curProgress.curMissionId == model.prm.data.id ? 1 : 0);
            view.txt_name.text = model.prm.data.name;
            view.txt_desc.text = model.prm.data.desc;
            RefreshDistance();
        }
        public void RefreshDistance()
        {
            view.txt_distance.text = MissionGuide.GetDistanceText(model.prm.data);
        }
    }


}
