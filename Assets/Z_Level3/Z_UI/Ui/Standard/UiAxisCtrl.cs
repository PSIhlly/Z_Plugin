using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z_Ui.Base;
using Z_Texture;
using UnityEngine;
using Z_Ui.Notify;
using Z_Time;
using Z_Input;
using static UnityEngine.UI.GridLayoutGroup;
using UnityEngine.UIElements;

namespace Ui.Axis
{

    public partial class UiAxisParam
    {
        public bool noRotate;
        public Vector2 pos;
        public RectTransform limitRtf;
        public Action<(Vector2,float)> onTrsChange;
    }
    public partial class UiAxisModel
    {
        public RectTransform limitRtf;
        public Action<(Vector2,float)> onTrsChange;
        public Timer timer;
        public int oriId;
        public Vector2 oriPos;
        public float oriAngle;

        public float startAngle;
    }
    public partial class UiAxisCtrl
    {
        public override void OnCreate()
        {
            var vectorA = new Vector3(0, 1, 0);
            var vectorB = view.btn_rot.transform.position - view.rtf_axis.position;
            float unsignedAngle = Vector2.Angle(vectorA, vectorB);
            float crossZ = vectorA.x * vectorB.y - vectorA.y * vectorB.x;
            model.startAngle = crossZ > 0 ? unsignedAngle : -unsignedAngle;

            view.btn_x.onClickDown += () =>
            {
                OnClickDown(() =>
                {
                    var cur = InputManager.instance.id2Pos[model.oriId];
                    view.rtf_axis.Translate((cur - model.oriPos).x, 0, 0);

                    Vector3[] corners = new Vector3[4];
                    model.limitRtf.GetWorldCorners(corners);
                    view.rtf_axis.position = Z_Math.Graph.Clamp(view.rtf_axis.position, corners[0], corners[2]);
                    model.oriPos = cur;
                });
            };
            view.btn_x.onClickUp += () =>
            {
                TimeManager.instance.CancelTimer(model.timer);
            };


            view.btn_y.onClickDown += () =>
            {
                OnClickDown(() =>
                {
                    var cur = InputManager.instance.id2Pos[model.oriId];
                    view.rtf_axis.Translate(0, (cur - model.oriPos).y, 0);

                    Vector3[] corners = new Vector3[4];
                    model.limitRtf.GetWorldCorners(corners);
                    view.rtf_axis.position = Z_Math.Graph.Clamp(view.rtf_axis.position, corners[0], corners[2]);
                    model.oriPos = cur;
                });
            };
            view.btn_y.onClickUp += () =>
            {
                TimeManager.instance.CancelTimer(model.timer);
            };

            view.btn_rot.onClickDown += () =>
            {
                OnClickDown(() =>
                {
                    var cur = InputManager.instance.id2Pos[model.oriId];
                    var vectorA = new Vector3(0, 1, 0);
                    var vectorB = cur - (Vector2)view.rtf_axis.position;
                    view.rtf_axis.eulerAngles = new Vector3(0, 0, Vector2.Angle(vectorA, vectorB) - model.startAngle);
                });
            };
            view.btn_rot.onClickUp += () =>
            {
                TimeManager.instance.CancelTimer(model.timer);
            };
        }
        public override void OnShow()
        {
            if(param==null)
            {
                Close();
                return;
            }
            view.btn_rot.gameObject.SetActive(!param.noRotate);
            model.limitRtf = param.limitRtf;
            Vector3[] corners = new Vector3[4];
            model.limitRtf.GetWorldCorners(corners);
            view.rtf_axis.transform.position = new Vector3(corners[0].x + param.pos.x * (corners[2].x - corners[0].x), corners[0].y + param.pos.y * (corners[1].y - corners[0].y), model.limitRtf.position. z) ;
            model.onTrsChange = param.onTrsChange;
        }
        private void OnClickDown(Action act)
        {
            model.oriId = InputManager.instance.GetClosedId(view.rtf_axis.position);
            model.oriPos = InputManager.instance.id2Pos[model.oriId];
            model.timer = TimeManager.instance.StartTimer(0, 0.1f, () =>
            {
                act?.Invoke();
                Vector3[] corners= new Vector3[4];
                model.limitRtf.GetWorldCorners(corners);
                model.onTrsChange?.Invoke((new Vector2((view.rtf_axis.position.x - corners[0].x) / (corners[2].x - corners[0].x), (view.rtf_axis.position.y-corners[0].y) /(corners[1].y- corners[0].y)), view.rtf_axis.eulerAngles.z));
                return false;
            });
        }
        public override void OnHide()
        {
            TimeManager.instance.CancelTimer(model.timer);
            base.OnHide();
        }
    }

}