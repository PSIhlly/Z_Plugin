using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Z_Ui.Base
{
    public class UiParam
    {

    }
    public class UiCtrl
    {
        public static string name = "Ui";
        public UiHolder uiHolder;
        public GameObject gameObject => uiHolder?.gameObject;
        public bool active
        {
            get
            {
                try
                {
                    if(uiHolder==null)
                    {
                        Debug.LogError(name+"aaaaaa??");
                    }
                    return gameObject != null && gameObject.activeInHierarchy;
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    return false;
                }
            }
        }


        public RectTransform rect => uiHolder?.gameObject?.GetComponent<RectTransform>();
        public virtual void BindHolderRecursively(UiHolder uiHolder)
        {
            this.uiHolder = uiHolder;
            uiHolder.ctrl = this;
        }
        public virtual void SetParam(UiParam param)
        {

        }
        public virtual void OnCreate()
        {

        }
        public virtual void OnEnable()
        {

        }
        public virtual void OnShow()
        {

        }
        public virtual void OnUpdate()
        {

        }

        public virtual void OnHide()
        {

        }
        public virtual void OnDisable()
        {

        }
        public virtual void Close()
        {
            gameObject.SetActive(false);
        }
        public void SetActive(bool active, UiParam param = null)
        {
            if (uiHolder != null && uiHolder.gameObject != null)
            {
                SetParam(param);
                uiHolder.gameObject.SetActive(active);
            }
        }
        public bool isActive => (uiHolder != null && uiHolder.gameObject != null) ? uiHolder.gameObject.activeSelf : false;
    }
}