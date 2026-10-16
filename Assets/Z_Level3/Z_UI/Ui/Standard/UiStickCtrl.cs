using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
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
        // Start-only checks (cooldowns/skill ownership) need not stay true while aiming.
        public Func<bool> canContinueFunc;
        public Action hideAct;
        public bool startFromStick;
        // Floating controls keep their hit area fixed and use the press as origin.
        public bool floating;
        public Sprite icon;
    }
    public partial class UiStickModel
    {
        public int curId = -1;
        public bool curIsTouch;
        public Vector3 lastPos;
        public Vector3 downPos;
        public Graphic areaGraphic;
        public Color areaColor;
        public bool areaEnabled;
        public bool stickActive;
        public bool floatingPrepared;
        public UiStickParam prm;
    }
    public partial class UiStickCtrl : IZ_Listener<InputMouseDownEvent>, IZ_Listener<InputMouseUpEvent>, IZ_Listener<InputMouseEvent>,
        IZ_Listener<InputPointDownEvent>, IZ_Listener<InputPointUpEvent>, IZ_Listener<InputPointEvent>
    {
        public override void OnCreate()
        {

        }
        public override void OnShow()
        {
            this.Register<InputMouseDownEvent>();
            this.Register<InputMouseUpEvent>();
            this.Register<InputMouseEvent>();
            this.Register<InputPointDownEvent>();
            this.Register<InputPointUpEvent>();
            this.Register<InputPointEvent>();
            ResetPointer();
            RestoreFloatingState();
            model.prm = param;
            if (model.prm.floating)
                PrepareFloatingState();
            if(model.prm.icon!=null)
                view.img_stick.sprite = model.prm.icon;
        }
        public override void OnHide()
        {
            this.Unregister<InputMouseDownEvent>();
            this.Unregister<InputMouseUpEvent>();
            this.Unregister<InputMouseEvent>();
            this.Unregister<InputPointDownEvent>();
            this.Unregister<InputPointUpEvent>();
            this.Unregister<InputPointEvent>();
            ResetPointer();
            model.prm?.hideAct?.Invoke();
        }
        public void SetDragingAct(Action act)
        {

        }
        public override void OnUpdate()
        {
            if (model.curId != -1 && model.prm.canContinueFunc != null && !model.prm.canContinueFunc())
            {
                ResetPointer();
                model.prm.hideAct?.Invoke();
            }
            if (model.curId != -1)
            {
                float dis = model.lastPos.magnitude;
                var realDelta = model.lastPos.normalized * Math.Clamp(dis, -0.5f, 0.5f);
                var center = model.prm.floating ? model.downPos : view.rtf_area.position;
                var size = model.prm.floating ? view.rtf_stick : view.rtf_area;
                view.rtf_stick.position = center + Graph.GetSize(size).x * realDelta;
                model.prm.dragAct?.Invoke(model.lastPos);
            }
            else
            {
                view.rtf_stick.position = view.rtf_area.position;
            }


        }
        public void HandleMouseDown(InputMouseDownEvent evt)
        {
            HandlePointerDown(evt.id, evt.pos, evt.ui, false);
        }

        private void HandlePointerDown(int id, Vector3 pos, GameObject ui, bool isTouch)
        {
            // A stick belongs to its first pointer until that pointer releases it.
            if (model.curId != -1)
                return;
            // The knob is a sibling of the area and wins the center raycast.
            // Q/E even disable the area's Graphic and rely on the knob alone.
            if (ui == view.rtf_area.gameObject || ui == view.rtf_stick.gameObject)
            {
                if(model.prm.canStartFunc!=null&&!model.prm.canStartFunc())
                {
                    ResetPointer();
                    return;
                }
                if (model.prm.floating)
                {
                    model.curId = id;
                    model.curIsTouch = isTouch;
                    model.downPos = new Vector3(pos.x, pos.y, view.rtf_stick.position.z);
                    model.lastPos = Vector3.zero;
                    view.rtf_stick.position = model.downPos;
                    view.rtf_stick.gameObject.SetActive(true);
                    return;
                }
                var re = Vector3.one;
                if (model.prm.startFromStick)
                {
                    re = Graph.GetNormalizedRelativePos(pos, view.rtf_stick) - new Vector2(0.5f, 0.5f);
                }
                else
                {
                    re = Graph.GetNormalizedRelativePos(pos, view.rtf_area) - new Vector2(0.5f, 0.5f);
                }
                
                if (re.magnitude <= 0.5f)
                {
                    model.curId = id;
                    model.curIsTouch = isTouch;
                    model.lastPos = re;
                }
            }
        }

        public void HandleMouseUp(InputMouseUpEvent evt)
        {
            HandlePointerUp(evt.id, false);
        }

        private void HandlePointerUp(int id, bool isTouch, bool canceled = false)
        {
            if (model.curId != -1 && model.curId == id && model.curIsTouch == isTouch)
            {
                var lastPos = model.lastPos;
                canceled |= model.prm.canContinueFunc != null && !model.prm.canContinueFunc();
                ResetPointer();
                if (canceled)
                    model.prm.hideAct?.Invoke();
                else
                    model.prm.upAct?.Invoke(lastPos);
            }
        }

        public bool HandleMouse(InputMouseEvent evt)
        {
            return HandlePointer(evt.id, evt.pos, false);
        }

        private bool HandlePointer(int id, Vector3 pos, bool isTouch)
        {
            if (model.curId != -1 && id == model.curId && model.curIsTouch == isTouch)
            {
                if (model.prm.floating)
                {
                    // Use the visual knob size, not the much larger input region,
                    // so its existing deadzone remains a short, scale-aware drag.
                    float size = Math.Max(0.0001f, Graph.GetSize(view.rtf_stick).x);
                    model.lastPos = new Vector3(pos.x - model.downPos.x, pos.y - model.downPos.y) / size;
                }
                else
                    model.lastPos = Graph.GetNormalizedRelativePos(pos, view.rtf_area) - new Vector2(0.5f, 0.5f);
                return true;
            }
            return false;
        }

        private void ResetPointer()
        {
            model.curId = -1;
            model.curIsTouch = false;
            model.lastPos = Vector3.zero;
            model.downPos = Vector3.zero;
            if (model.prm?.floating == true)
            {
                view.rtf_stick.gameObject.SetActive(false);
                view.rtf_stick.position = view.rtf_area.position;
            }
        }

        private void PrepareFloatingState()
        {
            model.areaGraphic = view.rtf_area.GetComponent<Graphic>();
            if (model.areaGraphic != null)
            {
                model.areaColor = model.areaGraphic.color;
                model.areaEnabled = model.areaGraphic.enabled;
                var color = model.areaColor;
                color.a = 0;
                model.areaGraphic.color = color;
                // The authored Play areas disable this Graphic. Keep the region
                // raycastable while hiding only its visual, not its GameObject.
                model.areaGraphic.enabled = true;
            }
            model.stickActive = view.rtf_stick.gameObject.activeSelf;
            model.floatingPrepared = true;
            view.rtf_stick.gameObject.SetActive(false);
        }

        private void RestoreFloatingState()
        {
            if (!model.floatingPrepared)
                return;
            if (model.areaGraphic != null)
            {
                model.areaGraphic.color = model.areaColor;
                model.areaGraphic.enabled = model.areaEnabled;
            }
            view.rtf_stick.gameObject.SetActive(model.stickActive);
            model.floatingPrepared = false;
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

        public void OnEvent(InputPointDownEvent evt)
        {
            HandlePointerDown(evt.id, evt.pos, evt.ui, true);
        }

        public void OnEvent(InputPointUpEvent evt)
        {
            HandlePointerUp(evt.id, true, evt.canceled);
        }

        public void OnEvent(InputPointEvent evt)
        {
            HandlePointer(evt.id, evt.pos, true);
        }

    }

}
