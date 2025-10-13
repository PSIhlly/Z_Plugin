using System;
using System.Collections;
using System.Collections.Generic;
using Ui;
using Ui.Dialog;
using UnityEngine;
using Z_DesignStyle;
using Z_Ui.Form;
namespace Z_Ui.Dialog
{
    public class Settings
    {
            public float autoPlaySpeed;//0~2
            public float textDisplaySpeed;//1~10
    }
    public enum PlayType
    {
        clipMainTextOver,
        clipsOver,
    }
    public class ClipPlayEvent : Z_Event
    {
        public PlayType playType;
    }

    public enum ShowType
    {
        Normal,
        Hide,
        History
    }
    public class ShowTypeEvent : Z_Event
    {
        public ShowType showType;
    }
    public class DialogManager : Z_Manager<DialogManager>,
        IZ_Listener<ClipPlayEvent>
    {
        //sub
        public Settings settings;

        private List<ClipForm.Data> clipLst = new List<ClipForm.Data>();
        private Action onComplete;

        public bool enabled;
        bool inited;

        public override void Init()
        {
            if (inited)
                return;
            inited = true;
            this.Register<ClipPlayEvent>();
            
            settings = new Settings()
            {
                autoPlaySpeed = 0,
                textDisplaySpeed = 5,
            };

        }
        #region 开始方法
        public void Begin(List<string> titleLst, List<string> mainTextLst, List<string> mainPictureLst, List<string> profilePictureLst, Action onComplete)
        {
            clipLst.Clear();
            for (int i = 0, icnt = titleLst.Count; i < icnt; i++)
            {
                var clip = new ClipForm.Data(-1,titleLst[i], mainTextLst[i], mainPictureLst[i], profilePictureLst[i]);
                clipLst.Add(clip);
            }
            Begin(clipLst, onComplete);
        }
        public void Begin(List<ClipForm.Data> clipLst, Action onComplete)
        {
            Init();
            enabled = true;
            this.clipLst = clipLst;
            this.onComplete = onComplete;
            UiManager.instance.ShowUi<UiDialogCtrl>(new UiDialogParam()
            {
                clips = clipLst
            });
        }
        #endregion

        public void End()
        {
            enabled = false;
            UiManager.instance.CloseUi<UiDialogCtrl>();
            onComplete?.Invoke();
        }
        public void OnEvent(ClipPlayEvent e)//跳过这一大段
        {
            switch(e.playType)
            {
                case PlayType.clipsOver:
                    End();
                    break;
            }
        }

       
    }
}
