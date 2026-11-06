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
using Z_Language;
using Z_Text.Form;

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
        private int lastSession = -1;
        private string lastTitleKey;
        private Language lastLanguage;

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
            // Adding a nested visibility guard must not reset this session's text.
            if (lastSession != LoadingManager.instance.sessionId)
            {
                lastTitleKey = null;
                Refresh();
            }
        }
        public override void OnUpdate()
        {
            RefreshProgress();
        }
        public void Refresh()
        {
            RefreshProgress();
        }
        private void RefreshProgress()
        {
            var loading = LoadingManager.instance;
            RefreshTitle(loading);
            var txt = view != null ? view.txt_progress : null;
            if (txt == null)
                return;

            if (lastSession != loading.sessionId)
            {
                lastSession = loading.sessionId;
                lastPercent = -2;
            }
            // Work can be discovered later; retain the last shown percentage.
            int percent = loading.hasProgress ? Mathf.Min(99, Mathf.RoundToInt(loading.progress * 100f)) : -1;
            if (percent >= 0) percent = Mathf.Max(lastPercent, percent);
            if (percent == lastPercent)
                return;
            lastPercent = percent;
            // 运行时覆盖：百分比不经过语言表翻译。
            txt.text = percent < 0 ? "…" : percent + "%";
        }

        private void RefreshTitle(LoadingManager loading)
        {
            var title = view != null ? view.txt_title : null;
            if (title == null) return;
            string phase = loading.currentPhase;
            string key = TextBaseForm.DataByKey.ContainsKey(phase) ? phase : "loading";
            var language = LanguageManager.instance.language;
            if (key == lastTitleKey && language == lastLanguage) return;
            lastTitleKey = key;
            lastLanguage = language;
            title.languageTranslatable = true;
            title.oriText = key;
        }
        public override void Close()
        {
            base.Close();
            model.onComplete?.Invoke();
        }


    }
   
}
