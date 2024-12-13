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
        public virtual void BindHolder(UiHolder uiHolder)
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
       
        public virtual void OnDisable()
        {

        }
        public void SetActive(bool active)
        {
            if(uiHolder!=null&&uiHolder.gameObject!=null)
            uiHolder.gameObject.SetActive(active);
        }
        public bool isActive => (uiHolder != null && uiHolder.gameObject != null)?uiHolder.gameObject.activeSelf:false;
    }
}