using Form;
using System.Collections;
using System.Collections.Generic;
using Ui.ModSceneMenu;
using Ui.PlaySceneMenu;
using UnityEngine;
using UnityEngine.UI;
using Z_Map;
using Z_ObjectAnimator.Base;
using Z_ObjectAnimator.Core;
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

        }
       
        public void Refresh()
        {
            gameObject.SetActive(GameManager.instance.curProgress.enableMission);
        }
    }

}
