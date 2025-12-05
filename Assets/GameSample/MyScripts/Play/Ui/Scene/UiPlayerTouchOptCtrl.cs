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
namespace Ui.PlaySceneMain.PlayerTouchOpt
{
    public partial class UiPlayerTouchOptModel
    {
        public int curMoveId;
        public int curAttackId;

        public Vector3 lastMovePos;
        public Vector3 lastAttackPos;
    }
    public partial class UiPlayerTouchOptCtrl : IZ_Listener<InputMouseDownEvent>, IZ_Listener<InputMouseUpEvent>, IZ_Listener<InputMouseEvent>
    {
        public override void OnCreate()
        {
            this.Register<InputMouseDownEvent>();
            this.Register<InputMouseUpEvent>();
            this.Register<InputMouseEvent>();

        }
        public override void OnShow()
        {
            model.curAttackId = -1;
            model.curMoveId = -1;
        }


        public override void OnUpdate()
        {
            if (model.curMoveId != -1)
            {

                float dis = model.lastMovePos.magnitude;
                var realDelta = model.lastMovePos.normalized * Math.Clamp(dis,-0.5f,0.5f);
                if (dis < 0.25f)
                    dis = 0;
                else
                    dis = 1;

                var delta = model.lastMovePos.normalized * dis;

                delta.z = delta.y;
                delta.y = 0;

                if (PlayManager.instance.data.progress.blockProgramUid <= 0)
                {
                    PlayManager.instance.sceneCtrl.SetPlayerMove(Time.deltaTime * delta);
                }
                view.rtf_moveStick.position = view.rtf_move.position + Graph.GetSize(view.rtf_move).x  * realDelta;
            }
            else
            {
                view.rtf_moveStick.position = view.rtf_move.position;
            }
            if (model.curAttackId != -1)
            {
                float dis = model.lastAttackPos.magnitude;
                var realDelta = model.lastAttackPos.normalized*Math.Clamp(dis, -0.5f, 0.5f);

                var delta = model.lastAttackPos.normalized * dis;

                delta.z = delta.y;
                delta.y = 0;

                view.rtf_attackStick.position = view.rtf_attack.position + Graph.GetSize(view.rtf_attack).x * realDelta;
                //PlayManager.instance.sceneCtrl.SetPlayerMove(Time.deltaTime * model.lastMovePos.normalized * dis);
            }
            else
            {
                view.rtf_attackStick.position = view.rtf_attack.position;
            }

        }
        public void OnEvent(InputMouseDownEvent evt)
        {
            if (evt.ui.ContainsParent(view.go_move))
            {
                var re = Graph.GetNormalizedRelativePos(evt.pos, view.rtf_move) - new Vector2(0.5f, 0.5f);
                if (re.magnitude <= 0.5f)
                {
                    model.curMoveId = evt.id;
                    model.lastMovePos = re;
                }
            }
            if (evt.ui.ContainsParent(view.go_attack))
            {
                var re = Graph.GetNormalizedRelativePos(evt.pos, view.rtf_attack) - new Vector2(0.5f, 0.5f);
                if (re.magnitude <= 0.5f)
                {
                    model.curAttackId = evt.id;
                    model.lastAttackPos = re;
                }
            }
        }

        public void OnEvent(InputMouseUpEvent evt)
        {
            if (model.curMoveId == evt.id)
            {
                model.curMoveId = -1;
            }
            if (model.curAttackId == evt.id)
            {
                model.curAttackId = -1;
            }
        }

        public void OnEvent(InputMouseEvent evt)
        {
            if (evt.id == model.curMoveId)
            {
                model.lastMovePos = Graph.GetNormalizedRelativePos(evt.pos, view.rtf_move) - new Vector2(0.5f, 0.5f);
            }
            if (evt.id == model.curAttackId)
            {
                model.lastAttackPos = Graph.GetNormalizedRelativePos(evt.pos, view.rtf_attack) - new Vector2(0.5f, 0.5f);
            }
        }
    }
}
