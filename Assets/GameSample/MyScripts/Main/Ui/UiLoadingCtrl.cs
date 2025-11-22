using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Form;
using Item;
using Z_Texture;
using Z_Ui.Base;
using Z_Ui;
using Z_Time;
using System;

namespace Ui.Loading
{
    public partial class UiLoadingParam
    {
        public float delaySeconds;
        public Action onComplete;
    }
    public partial class UiLoadingModel
    {
        public Action onComplete;
    }
    public partial class UiLoadingCtrl
    {
       public override void OnCreate()
        {
        }


        public override void OnShow()
        {
            if(param!=null)
            {
                model.onComplete = param.onComplete;
                if (param.delaySeconds>0)
                {
                    TimeManager.instance.StartTimer(param.delaySeconds, 0, () =>
                    {
                        Close();
                        return true;
                    });
                }
            }else
            {
                model.onComplete = null;
            }
            Refresh();
        }
        public void Refresh()
        {
          
        }
        public override void Close()
        {
            base.Close();
            model.onComplete?.Invoke();
        }


    }
   
}
