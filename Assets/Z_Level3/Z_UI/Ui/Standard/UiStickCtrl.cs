using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using Z_Input;
using Z_Math;
using Z_Texture;
using Z_Time;
using Z_Ui.Base;
using Z_Ui.Notify;
using static UnityEngine.UI.GridLayoutGroup;

namespace Ui.Stick
{

    public partial class UiStickParam
    {
        public Action<Vector3> dragAct;
        public Action<Vector3> upAct;
        public Func<bool> canStartFunc;
        public Action hideAct;
        public bool startFromStick;
        public Sprite icon;
    }
    public partial class UiStickModel
    {
        public int curId = -1;
        public Vector3 lastPos;
        public UiStickParam prm;
    }
    public partial class UiStickCtrl : IZ_Listener<InputMouseDownEvent>, IZ_Listener<InputMouseUpEvent>, IZ_Listener<InputMouseEvent>
    {
        public override void OnCreate()
        {

        }
        public override void OnShow()
        {
            this.Register<InputMouseDownEvent>();
            this.Register<InputMouseUpEvent>();
            this.Register<InputMouseEvent>();
            model.curId = -1;
            model.prm = param;
            if(model.prm.icon!=null)
                view.img_stick.sprite = model.prm.icon;
        }
        public override void OnHide()
        {
            this.Unregister<InputMouseDownEvent>();
            this.Unregister<InputMouseUpEvent>();
            this.Unregister<InputMouseEvent>();
            model.prm?.hideAct?.Invoke();
        }
        public void SetDragingAct(Action act)
        {

        }
        public override void OnUpdate()
        {
            if (model.curId != -1)
            {
                float dis = model.lastPos.magnitude;
                var realDelta = model.lastPos.normalized * Math.Clamp(dis, -0.5f, 0.5f);
                view.rtf_stick.position = view.rtf_area.position + Graph.GetSize(view.rtf_area).x * realDelta;
                model.prm.dragAct?.Invoke(model.lastPos);
            }
            else
            {
                view.rtf_stick.position = view.rtf_area.position;
            }


        }
        public void HandleMouseDown(InputMouseDownEvent evt)
        {
            if (evt.ui == view.rtf_area.gameObject)
            {
                if(model.prm.canStartFunc!=null&&!model.prm.canStartFunc())
                {
                    model.curId = -1;
                    return;
                }
                var re = Vector3.one;
                if (model.prm.startFromStick)
                {
                    re = Graph.GetNormalizedRelativePos(evt.pos, view.rtf_stick) - new Vector2(0.5f, 0.5f);
                }
                else
                {
                    re = Graph.GetNormalizedRelativePos(evt.pos, view.rtf_area) - new Vector2(0.5f, 0.5f);
                }
                
                if (re.magnitude <= 0.5f)
                {
                    model.curId = evt.id;
                    model.lastPos = re;
                }
            }
        }

        public void HandleMouseUp(InputMouseUpEvent evt)
        {
            if (model.curId == evt.id)
            {
                model.prm.upAct?.Invoke(model.lastPos);
                model.curId = -1;
            }
        }

        public bool HandleMouse(InputMouseEvent evt)
        {
            if (evt.id == model.curId)
            {
                model.lastPos = Graph.GetNormalizedRelativePos(evt.pos, view.rtf_area) - new Vector2(0.5f, 0.5f);
                return true;
            }
            return false;
        }


        



        public void OnEvent(InputMouseDownEvent evt)
        {
            HandleMouseDown(evt);
        }

        public void OnEvent(InputMouseUpEvent evt)
        {
            HandleMouseUp(evt);
        }

        public void OnEvent(InputMouseEvent evt)
        {
            HandleMouse(evt);
        }

    }

}
