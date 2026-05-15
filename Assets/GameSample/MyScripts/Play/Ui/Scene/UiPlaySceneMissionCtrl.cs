using Form;
using System.Collections;
using System.Collections.Generic;
using Ui.ModSceneMenu;
using Ui.PlayMission;
using Ui.PlaySceneMenu;
using UnityEngine;
using UnityEngine.UI;
using Z_DesignStyle;
using Z_Map;
using Z_ObjectAnimator.Base;
using Z_ObjectAnimator.Core;
using Z_Text;
using Z_Texture;
using Z_Time;
using Z_Ui;
using Z_Ui.Base;

namespace Ui.PlaySceneMain.PlaySceneMission
{
    public partial class UiPlaySceneMissionModel
    {
    }
    public partial class UiPlaySceneMissionCtrl
    {
     
        public override void OnCreate()
        {
            view.btn_mission.onClick.AddListener(() =>
            {
                UiManager.instance.ShowUi<UiPlayMissionCtrl>();
            });
        }
       
        public void Refresh()
        {
            gameObject.SetActive(GameManager.instance.curProgress.enableMission);
            var data = MissionForm.DataById.GetDv(GameManager.instance.curProgress.curMissionId, null);
            if (data == null || !data.received)
            {
                view.txt_.text = "";
                view.txt_distance.text = "";
            }
            else
            {
                view.txt_.text = data.desc;
                var targetScene = SceneForm.DataByUid.GetDv(data.targetSceneId, null);
                if (targetScene != null)
                    view.txt_distance.text = data.targetSceneId == GameManager.instance.curScene.uid ? Vector3.Magnitude(data.targetPos - PlayManager.instance.sceneCtrl.GetPlayerPos()) + TextManager.instance.GetTxt("m") : TextManager.instance.GetTxt("go to ") + targetScene.name;
            }
        }
    }


}
