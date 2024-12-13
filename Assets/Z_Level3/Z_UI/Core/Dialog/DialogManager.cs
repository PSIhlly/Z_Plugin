using System;
using System.Collections;
using System.Collections.Generic;
using Ui.Dialog;
using UnityEngine;
using Z_DesignStyle;
namespace Z_Ui.Dialog
{
    public class Settings
    {
            public float autoPlaySpeed;//0~2
            public float textDisplaySpeed=5;//1~10
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
    public class Clip
    {
        public string title;
        public string mainText;
        public Sprite mainPicture;
        public Sprite profilePicture;
    }
    public class DialogManager : Z_MonoManager<DialogManager>,
        IZ_Listener<ClipPlayEvent>
    {
        //sub
        public Settings settings;

        private List<Clip> clipLst = new List<Clip>();
        private Action onComplete;

        public override void Init()
        {
            base.Init();
            this.Register<ClipPlayEvent>();
        }
        #region 开始方法
        public void Begin(List<string> titleLst, List<string> mainTextLst, List<Sprite> mainPictureLst, List<Sprite> profilePictureLst, Action onComplete)
        {
            clipLst.Clear();
            for (int i = 0, icnt = titleLst.Count; i < icnt; i++)
            {
                var clip = new Clip();
                clip.profilePicture = profilePictureLst[i];
                clip.mainPicture = mainPictureLst[i];
                clip.mainText = mainTextLst[i];
                clip.title = titleLst[i];
            }
            Begin(clipLst, onComplete);
        }
        public void Begin(List<Clip> clipLst, Action onComplete)
        {
            Init();
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

        public void OnEvent(ShowTypeEvent evt)
        {
            throw new NotImplementedException();
        }
    }
}
