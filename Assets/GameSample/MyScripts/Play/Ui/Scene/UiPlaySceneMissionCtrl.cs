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

// Mission positions use the same player-facing coordinates as Mod scene input.
// Convert them once at the runtime boundary; player positions are already world positions.
public static class MissionGuide
{
    public static Vector3 GetTargetWorldPosition(MissionForm.Data data)
    {
        return MapManager.instance.utilCtrl.MapPos2RealPos(GameManager.PlayerPosToMapPos(data.targetPos));
    }

    public static float GetDistance(MissionForm.Data data, Vector3 playerWorldPosition)
    {
        return Vector3.Distance(GetTargetWorldPosition(data), playerWorldPosition);
    }

    public static string GetDistanceText(MissionForm.Data data)
    {
        var targetScene = SceneForm.DataByUid.GetDv(data.targetSceneId, null);
        if (targetScene == null)
            return "";
        return data.targetSceneId == GameManager.instance.curScene.uid
            ? GetDistance(data, PlayManager.instance.sceneCtrl.GetPlayerPos()).ToString("0") + TextManager.instance.GetTxt("m")
            : TextManager.instance.GetTxt("go to ") + targetScene.name;
    }
}

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
            var data = MissionForm.DataById.GetDv(GameManager.instance.curProgress.curMissionId, null);
            var hasMission = GameManager.instance.curProgress.enableMission &&
                             data != null && data.show && data.received && !data.fail && !data.done;
            gameObject.SetActive(hasMission);
            if (!hasMission)
                return;

            view.txt_.text = data.desc;
            RefreshDistance();
        }

        public void RefreshDistance()
        {
            if (!active)
                return;
            var data = MissionForm.DataById.GetDv(GameManager.instance.curProgress.curMissionId, null);
            view.txt_distance.text = data == null ? "" : MissionGuide.GetDistanceText(data);
        }
    }


}
