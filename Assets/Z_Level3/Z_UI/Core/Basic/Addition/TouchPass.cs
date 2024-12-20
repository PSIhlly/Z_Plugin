using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TouchPass : MonoBehaviour,
    IPointerDownHandler, IPointerUpHandler, IPointerClickHandler,
    IInitializePotentialDragHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    PointerEventData pointerDownEvent;
    GameObject pointerDownTarget;
    private static List<RaycastResult> resultCache = new List<RaycastResult>();
    public void Log(int id)
    {
        Debug.Log(id);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        //throw new System.NotImplementedException();
    }

    public void OnDrag(PointerEventData eventData)
    {
        //throw new System.NotImplementedException();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        //throw new System.NotImplementedException();
    }

    public void OnInitializePotentialDrag(PointerEventData eventData)
    {
        //throw new System.NotImplementedException();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        pointerDownTarget = null;
        pointerDownEvent = null;
        pointerDownTarget = GetRaycastTarget<IPointerDownHandler>(gameObject, eventData);
        pointerDownEvent = eventData;
        if (pointerDownTarget)
        {
            ExecuteEvents.Execute(pointerDownTarget, eventData, ExecuteEvents.pointerClickHandler);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        pointerDownTarget = null;
        pointerDownEvent = null;
        pointerDownTarget = GetRaycastTarget<IPointerDownHandler>(gameObject, eventData);
        pointerDownEvent = eventData;
        if (pointerDownTarget)
        {
            ExecuteEvents.Execute(pointerDownTarget, eventData, ExecuteEvents.pointerDownHandler);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
    }

    public static GameObject GetRaycastTarget<T>(GameObject gameObject, PointerEventData eventData) where T : IEventSystemHandler
    {
        bool afterSelf = false;
        GameObject find = null;
        GameObject first = null;
        EventSystem.current.RaycastAll(eventData, resultCache);
        foreach (var raycast in resultCache)
        {
            var go = raycast.gameObject;
            if (!go) continue;
            var excuteGo = ExecuteEvents.GetEventHandler<T>(go);
            if (excuteGo == gameObject)
            {
                afterSelf = true;
                continue;
            }
            if (afterSelf)
            {
                find = excuteGo;
                break;
            }
            else
            {
                if (!first) first = excuteGo;
            }
        }
        resultCache.Clear();
        if (!afterSelf)
        {
            find = first;
        }
        return find;
    }
}
