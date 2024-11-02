using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_DesignStyle;
namespace Z_Ui.Dialog
{
    public class Settings
    {
            public float autoPlaySpeed;//0~2
            public float textDisplaySpeed=5;//1~10
    }

    public class ClipPlayEvent : Z_Event
    {
        public bool isOver;
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
    public class DialogUiBaseManager : Z_MonoManager<DialogUiBaseManager>,
        IZ_Listener<ClipPlayEvent>, 
        IZ_Listener<ShowTypeEvent>
    {
        //sub
        public Settings settings;

        public MainTextController mainTextController;
        public ProfilePictureController profilePictureController;
        public TitleController titleController;
        public MainPictureController mainPictureController;
        public Z_CoroutineWork coroutineWork;

        public GameObject mainUIPanel;

        public GameObject dialogUIPanel;
        public GameObject historyUIPanel;


        private int progress;
        private List<Clip> clipLst = new List<Clip>();
        private Action onComplete;

        public override void Init()
        {
            base.Init();
            mainTextController?.Init(this);
            profilePictureController?.Init(this);
            mainPictureController?.Init(this);
            titleController?.Init(this);

            this.Register<ClipPlayEvent>();
            this.Register<ShowTypeEvent>();
        }
        #region 开始方法
        public void Begin(List<string> titleLst, List<string> mainTextLst, List<Sprite> mainPictureLst, List<Sprite> profilePictureLst, Action onComplete)
        {
            Init();
            progress = 0;
            clipLst.Clear();
            for (int i = 0, icnt = titleLst.Count; i < icnt; i++)
            {
                var clip = new Clip();
                clip.profilePicture = profilePictureLst[i];
                clip.mainPicture = mainPictureLst[i];
                clip.mainText = mainTextLst[i];
                clip.title = titleLst[i];
            }
            this.onComplete = onComplete;
            Display();
            mainUIPanel.SetActive(true);
        }
        #endregion

        public void End()
        {
            mainUIPanel.SetActive(false);
            onComplete?.Invoke();
        }
        public void OnEvent(ClipPlayEvent e)//跳过这一小段
        {
            progress++;
            if (progress == clipLst.Count)
                End();
        }
        private void Display()
        {
            if(mainTextController!=null)
            {
                mainTextController.Display(clipLst[progress].mainText);
            }
            if (profilePictureController != null)
            {
                profilePictureController.Display(clipLst[progress].profilePicture);
            }
            if (mainPictureController != null)
            {
                mainPictureController.Display(clipLst[progress].mainPicture);
            }
            if (titleController != null)
            {
                titleController.Display(clipLst[progress].title);
            }
        }

        public void OnEvent(ShowTypeEvent e)
        {
            switch(e.showType)
            {
                case ShowType.Normal:
                    mainUIPanel.SetActive(true);
                    historyUIPanel.SetActive(false);
                    break;
                case ShowType.Hide:
                    historyUIPanel.SetActive(false);
                    break;
                case ShowType.History:
                    historyUIPanel.SetActive(true);
                    mainUIPanel.SetActive(false);
                    break;
            }
        }
    }
}
