using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Z_Gal.GalUI
{
    public class GalUIInputController : MonoBehaviour
    {
        public GalUIManagerBaseType galUIBaseType;
        public GraphicRaycaster graphicRaycaster; 

        // Update is called once per frame

        public bool ClickNoAction(Vector2 clickPos)
        {
            if(CheckClickButton(clickPos)&&CheckHistoryUIShow()&& CheckMainUIHide()&& CheckMainTextIsDisplaying())
            return true;
            return false;
        }
        private bool CheckClickButton(Vector2 clickPos)
        {
            PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
            pointerEventData.position = clickPos;
            List<RaycastResult> results = new List<RaycastResult>();
            graphicRaycaster.Raycast(pointerEventData, results);
            foreach(var result in results)
            {
                if (result.gameObject.TryGetComponent<Selectable>(out var interactItem))
                    return false;
            }
            return true;
        }
        private bool CheckHistoryUIShow()
        {
            if (galUIBaseType.GetHistoryUIActive())
            {
                galUIBaseType.ChangeHistoryUIActive();
                return false;
            }
            return true;
        }
        private bool CheckMainUIHide()
        {
            if(!galUIBaseType.GetMainUIActive())
            {
                galUIBaseType.ChangeMainUIActive();
                return false;
            }
            return true;
        }
        private bool CheckMainTextIsDisplaying()
        {
            if (galUIBaseType.mainTextController.isDisplaying)
            {
                galUIBaseType.mainTextController.skipCurrent = true;
                return false;
            }
            return true;
        }
    }
}