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
        public GameObject historyUIPanel;
        public override void Init()
        {
            this.Register<ClipPlayEvent>();
            this.Register<ShowTypeEvent>();
        }

        // Start is called before the first frame update
        public void StartClip()
        {


        }
        public void OnEvent(ClipPlayEvent e)//跳过这一大段
        {

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
