using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Form;
using Item;
using Z_Texture;
using Z_Ui.Base;
using Z_Ui;
using Z_Time;
using Z_Ui.Loading;
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
        // 只在百分比真正变化时写文本，避免每帧刷新 TMP。
        private int lastPercent = -1;

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
        public override void OnUpdate()
        {
            RefreshProgress();
        }
        public void Refresh()
        {
            lastPercent = -1;
            RefreshProgress();
        }
        private void RefreshProgress()
        {
            var txt = view != null ? view.txt_progress : null;
            if (txt == null)
                return;

            int percent = Mathf.RoundToInt(LoadingManager.instance.progress * 100f);
            if (percent == lastPercent)
                return;
            lastPercent = percent;
            // 运行时覆盖：百分比不经过语言表翻译。
            txt.text = percent + "%";
        }
        public override void Close()
        {
            base.Close();
            model.onComplete?.Invoke();
        }


    }
   
}
