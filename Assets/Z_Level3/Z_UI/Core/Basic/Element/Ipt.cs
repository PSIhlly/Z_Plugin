using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System;

namespace Z_Ui.Base
{
    public class Ipt : TMP_InputField
    {
        public Action<string> onInput;
        public Action<string> onFinishInput;
        public bool scrollParentOnDragOutside;
        private bool invokeAction=true;

        private string oldStr;
        private ScrollRect dragScrollRect;
        private PointerEventData dragEventData;
        private readonly Vector3[] dragContentCorners = new Vector3[4];
        private const float DragScrollSpeed = 4f;
        private const float MaxDragScrollSpeed = 800f;
        public Ipt()
        {
            onValueChanged.AddListener((v) =>
            {
                if (invokeAction)
                {
                    onInput?.Invoke(v);
                }
            });
            onSelect.AddListener((v)=>
            {
                oldStr = v;
            });
            onDeselect.AddListener((v) =>
            {
                if (invokeAction&& oldStr != text)
                {
                    oldStr = text;
                    onFinishInput?.Invoke(v);
                }
            });
            onSubmit.AddListener((v) =>
            {
                if (invokeAction && oldStr != text)
                {
                    oldStr = text;
                    onFinishInput?.Invoke(v);
                }
            });
        }
        public void Set(string content, bool onlyNotFocus = true,bool invokeAction = false)
        {
            if (isFocused && onlyNotFocus)
                return;
            this.invokeAction = invokeAction;
            text = content;
            oldStr = text;
            this.invokeAction = true;
        }

        public override void OnBeginDrag(PointerEventData eventData)
        {
            base.OnBeginDrag(eventData);
            if (!scrollParentOnDragOutside || eventData.button != PointerEventData.InputButton.Left ||
                !IsActive() || !IsInteractable())
                return;

            var parentScrollRect = GetComponentInParent<ScrollRect>();
            if (parentScrollRect == null || parentScrollRect.content == null ||
                !transform.IsChildOf(parentScrollRect.content))
                return;

            dragScrollRect = parentScrollRect;
            dragEventData = eventData;
        }

        public override void OnDrag(PointerEventData eventData)
        {
            base.OnDrag(eventData);
            if (dragScrollRect != null)
                dragEventData = eventData;
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            base.OnEndDrag(eventData);
            StopParentDragScroll();
        }

        protected override void OnDisable()
        {
            StopParentDragScroll();
            base.OnDisable();
        }

        protected override void LateUpdate()
        {
            base.LateUpdate();
            if (dragScrollRect == null)
                return;
            if (!scrollParentOnDragOutside || !Input.GetMouseButton(0))
            {
                StopParentDragScroll();
                return;
            }

            var viewport = dragScrollRect.viewport != null
                ? dragScrollRect.viewport : (RectTransform)dragScrollRect.transform;
            var screenPosition = (Vector2)Input.mousePosition;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    viewport, screenPosition, dragEventData.pressEventCamera, out var localPosition))
                return;

            var rect = viewport.rect;
            var overflowX = localPosition.x < rect.xMin ? localPosition.x - rect.xMin
                : localPosition.x > rect.xMax ? localPosition.x - rect.xMax : 0f;
            var overflowY = localPosition.y < rect.yMin ? localPosition.y - rect.yMin
                : localPosition.y > rect.yMax ? localPosition.y - rect.yMax : 0f;
            if (overflowX == 0f && overflowY == 0f)
                return;

            dragScrollRect.content.GetWorldCorners(dragContentCorners);
            var corner0 = viewport.InverseTransformPoint(dragContentCorners[0]);
            var corner2 = viewport.InverseTransformPoint(dragContentCorners[2]);
            var scrollableWidth = Mathf.Max(0f, Mathf.Abs(corner2.x - corner0.x) - rect.width);
            var scrollableHeight = Mathf.Max(0f, Mathf.Abs(corner2.y - corner0.y) - rect.height);

            var previousPosition = dragScrollRect.normalizedPosition;
            dragScrollRect.velocity = Vector2.zero;
            if (dragScrollRect.horizontal && scrollableWidth > 0f && overflowX != 0f)
                dragScrollRect.horizontalNormalizedPosition += ScrollStep(overflowX) / scrollableWidth;
            if (dragScrollRect.vertical && scrollableHeight > 0f && overflowY != 0f)
                dragScrollRect.verticalNormalizedPosition += ScrollStep(overflowY) / scrollableHeight;

            if (dragScrollRect.normalizedPosition != previousPosition)
            {
                // The pointer may be stationary while the content moves beneath it.
                // TMP still needs a drag update to extend the text selection.
                dragEventData.position = screenPosition;
                base.OnDrag(dragEventData);
            }
        }

        private static float ScrollStep(float overflow)
        {
            return Mathf.Sign(overflow) * Mathf.Min(Mathf.Abs(overflow) * DragScrollSpeed,
                MaxDragScrollSpeed) * Time.unscaledDeltaTime;
        }

        private void StopParentDragScroll()
        {
            dragScrollRect = null;
            dragEventData = null;
        }
        
    }
}
