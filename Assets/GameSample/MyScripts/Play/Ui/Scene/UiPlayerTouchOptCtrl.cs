using Form;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using Ui.Start;
using UnityEngine;
using Z_Input;
using Z_Language;
using Z_Map;
using Z_Text;
using Z_Ui;
using Z_Ui.Notify;
using Z_UnitSystem;
using Z_DesignStyle;
using Z_Math;
using Z_Map.Analysis;
using Ui.Stick;
namespace Ui.PlaySceneMain.PlayerTouchOpt
{
    public partial class UiPlayerTouchOptModel
    {
    }
    public partial class UiPlayerTouchOptCtrl
    {
        public override void OnCreate()
        {


        }
        public override void OnShow()
        {
            view.model_moveStick.SetShow(true, new UiStickParam()
            {
                dragAct = (dir) =>
                {
                    dir = GetDirection(dir);
                    PlayManager.instance.sceneCtrl.SetPlayerMove(Time.deltaTime * dir);
                },
                canStartFunc = () => GameManager.instance.curProgress.blockProgramUid <= 0

            });
            view.model_attackStick.SetShow(true, new UiStickParam()
            {
                dragAct = (dir) =>
                {
                    PlayManager.instance.sceneCtrl.SetCurOptSkill(SkillType.LightAttack);
                    dir = GetDirection(dir);
                    if (dir != Vector3.zero)
                    {
                        PlayManager.instance.sceneCtrl.SetPlayerRotation(dir, 360);

                        var character = CharacterProductForm.DataByUid.GetDv(GameManager.instance.curProgress.characterUid, null);
                        if (character != null && character.skill.ContainsKey(SkillType.LightAttack))
                            PlayManager.instance.infoCtrl.UseSkill(character.uid, character.skill[SkillType.LightAttack], false);
                    }
                },
                upAct = (dir) =>
                {
                    PlayManager.instance.sceneCtrl.SetCurOptSkill(null);
                },
                canStartFunc = () => GameManager.instance.curProgress.blockProgramUid <= 0 && PlayManager.instance.sceneCtrl.GetCurOptSkill() == null

            });
        }

        private Vector3 GetDirection(Vector3 dir)
        {


            float dis = dir.magnitude;
            if (dis < 0.25f)
                return Vector3.zero;

            var delta = dir.normalized;
            delta.z = delta.y;
            delta.y = 0;
            return delta;
        }

    }
}
