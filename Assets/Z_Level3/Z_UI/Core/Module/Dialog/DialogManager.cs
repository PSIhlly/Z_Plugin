using System;
using System.Collections.Generic;
using Ui.DialogBg;
using Ui.DialogHistory;
using Ui.DialogMain;
using Z_DesignStyle;
using Z_Time;
using Z_Ui.Form;
namespace Z_Ui.Dialog
{
    public class Settings
    {
        public float autoPlaySpeed;//0~2
        public float textDisplaySpeed;//1~50
        public int historyMax;//1~10
        public Action menuAct;
    }
    public enum PlayType
    {
        ClipMainTextOver,
        ClipVideoOver,
        ClipSettingChange,
        ClipOver
    }
    public class ClipPlayEvent : Z_Event
    {
        public PlayType playType;
    }

    public enum ShowType
    {
        Normal,
        Hide
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
        private List<ClipForm.Data> clipLst;
        private Action onComplete;

        public bool enabled;
        public int curId;
        bool inited;
        bool clickOrAutoClose;
        List<ClipForm.Data> historyClips;
        public override void Init()
        {
            if (inited)
                return;
            inited = true;
            this.Register<ClipPlayEvent>();
            clipLst = new List<ClipForm.Data>();
            historyClips = new List<ClipForm.Data>();
            settings = new Settings()
            {
                autoPlaySpeed = 0,
                textDisplaySpeed = 20,
                historyMax = 10
            };

        }

        #region 开始方法

        public void SetMenuAct(Action menuAct)
        {
            settings.menuAct = menuAct;
        }

        public void Begin(string title, string mainText, string mainPicture, string mainVideo, string profilePicture, string mainAudio, Action onComplete, bool clickOrAutoClose = true)
        {
            Begin(new List<string>() { title }, new List<string>() { mainText }, new List<string>() { mainPicture }, new List<string>() { mainVideo }, new List<string>() { profilePicture }, new List<string>() { mainAudio }, onComplete, clickOrAutoClose);
        }
        public void Begin(List<string> titleLst, List<string> mainTextLst, List<string> mainPictureLst, List<string> mainVideoLst, List<string> profilePictureLst, List<string> mainAudioLst, Action onComplete, bool clickOrAutoClose = true)
        {
            clipLst.Clear();
            for (int i = 0, icnt = titleLst.Count; i < icnt; i++)
            {
                var clip = new ClipForm.Data(-1, titleLst[i], mainTextLst[i], mainPictureLst[i], mainVideoLst[i], profilePictureLst[i], mainAudioLst[i]);
                clipLst.Add(clip);
            }
            Begin(clipLst, onComplete, clickOrAutoClose);
        }
        public void Begin(ClipForm.Data clip, Action onComplete, bool clickOrAutoClose = true)
        {
            Begin(new List<ClipForm.Data>() { clip }, onComplete, clickOrAutoClose);
        }
        public void Begin(List<ClipForm.Data> clipLst, Action onComplete, bool clickOrAutoClose = true)
        {
            this.clickOrAutoClose = clickOrAutoClose;
            enabled = true;
            this.clipLst = clipLst;
            this.onComplete = onComplete;
            Play(0);
        }
        public void Play(int targetId)
        {
            if (targetId >= clipLst.Count)
            {
                InternalEnd();
                return;
            }
            curId = targetId;
            UiManager.instance.ShowUi<UiDialogBgCtrl>(new UiDialogBgParam()
            {
                clip = clipLst[curId]
            });
            UiManager.instance.ShowUi<UiDialogMainCtrl>(new UiDialogMainParam()
            {
                clip = clipLst[curId]
            });
            AddHistoryClip(clipLst[curId]);
        }
        public void ShowHistory()
        {
            UiManager.instance.ShowUi<UiDialogHistoryCtrl>(new UiDialogHistoryParam()
            {
                historyClips = historyClips
            });
        }
        public void CloseHistory()
        {

            UiManager.instance.CloseUi<UiDialogHistoryCtrl>();
        }
        #endregion


        private void InternalEnd()
        {
            enabled = false;
            if (clickOrAutoClose)
            {
                Close();
            }
            onComplete?.Invoke();
            onComplete -= onComplete;
        }
        public void Close()
        {
            TimeManager.instance.AddCurLateUpdateWithoutCheckAction(() =>
            {
                if (!enabled)
                {
                    UiManager.instance.CloseUi<UiDialogBgCtrl>();
                    UiManager.instance.CloseUi<UiDialogMainCtrl>();
                    UiManager.instance.CloseUi<UiDialogHistoryCtrl>();
                }
            });

        }
        public void AddHistoryClip(ClipForm.Data clip)
        {

            historyClips.Add(clip.Copy());
            while (historyClips.Count > settings.historyMax)
                historyClips.RemoveAt(0);
        }
        public void OnEvent(ClipPlayEvent evt)
        {
            if (evt.playType == PlayType.ClipOver)
            {
                Play(curId + 1);
            }
        }
    }
}
