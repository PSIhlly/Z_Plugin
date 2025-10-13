using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Z_Ui.Base
{
    public class Btn : Button
    {
        // Start is called before the first frame update
        public Action onClickDown;
        public Action onClickUp;
        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);
            onClickDown?.Invoke();
        }
        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);
            onClickUp?.Invoke();
        }
    }
}