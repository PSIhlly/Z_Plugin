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
    public partial class UiPlayMissionCtrl
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
                parent.Refresh();
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
            var targetScene = SceneForm.DataByUid.GetDv(model.prm.data.targetSceneId, null);
            if (targetScene != null)
                view.txt_distance.text = model.prm.data.targetSceneId == GameManager.instance.curScene.uid ? Vector3.Magnitude(model.prm.data.targetPos - PlayManager.instance.sceneCtrl.GetPlayerPos()) + TextManager.instance.GetTxt("m") : TextManager.instance.GetTxt("go to ") + targetScene.name;
        }
    }


}