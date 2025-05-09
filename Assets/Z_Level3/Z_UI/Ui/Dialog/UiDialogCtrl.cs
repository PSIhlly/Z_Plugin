using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Time;
using Z_Ui;
using Z_Ui.Base;
using Z_Ui.Dialog;

namespace Ui.Dialog
{

    #region func

    public partial class UiFuncCtrl
    {
        public override void OnCreate()
        {
            view.btn_autoPlay.onClick.AddListener(() =>
            {
                view.sta_autoPlay.ChangeState();
                DialogManager.instance.settings.autoPlaySpeed = view.sta_autoPlay.state;
            });
            view.btn_skip.onClick.AddListener(() =>
            {
                Z_EventHelper.Invoke(new ClipPlayEvent()
                {
                    playType = PlayType.clipsOver
                });
            });
            view.btn_hide.onClick.AddListener(() =>
            {
                Z_EventHelper.Invoke(new ShowTypeEvent()
                {
                    showType = ShowType.Hide
                });
            });
            view.btn_history.onClick.AddListener(() =>
            {
                Z_EventHelper.Invoke(new ShowTypeEvent()
                {
                    showType = ShowType.History
                });
            });
        }
        public override void OnShow()
        {
            view.sta_autoPlay.ChangeState((int)DialogManager.instance.settings.autoPlaySpeed);
        }
    }
    #endregion

    #region static 

    public partial class UiProfilePictureCtrl
    {
        public void Display(Sprite pic)
        {
            if (pic == null)
            {
                view.img_.sprite = null;
                return;
            }
            view.img_.sprite = pic;
        }
    }

    public partial class UiMainPictureCtrl
    {
        public void Display(Sprite pic)
        {
            if (pic == null)
            {
                view.img_.sprite = null;
                return;
            }
            view.img_.sprite = pic;
        }
    }

    public partial class UiTitleCtrl
    {
        public void Display(string title)
        {
            if (string.IsNullOrEmpty(title))
            {
                view.txt_.text = "";
                return;
            }
            view.txt_.text = title;
        }
    }
    #endregion

    public partial class UiMainTextModel
    {
        public Timer wordTimer;
        public Timer overTimer;

        public bool isDisplaying;
        public int nowWord;
        public string content;
    }
    public partial class UiMainTextCtrl
    {

        public static float autoPlaydelay = 5f;

        public override void OnCreate()
        {
            view.btn_over.onClick.AddListener(() =>
            {
                TimeManager.instance.CancelTimer(model.wordTimer);
                if (model.nowWord < model.content.Length)
                {
                    while (model.nowWord < model.content.Length)
                    {
                        Write();
                    }
                }
                else
                {
                    DelayForOver(0);
                }
            });
        }
        public override void OnShow()
        {
            Display();
        }
        private void UpdateContent(string content)
        {
            if (content==null)
                return;
            model.content = content;
            model.nowWord = 0;
            view.txt_.text = "";
        }
        public void Display(string content = null)
        {
            if (content != null)
                UpdateContent(content);
            if (string.IsNullOrEmpty(model.content))
                return;
            model.isDisplaying = true;
            var interval = Mathf.Max(0.1f, 1f / DialogManager.instance.settings.textDisplaySpeed);

            TimeManager.instance.CancelTimer(model.wordTimer);
            model.wordTimer = TimeManager.instance.StartTimer(0,interval, Write, uiHolder);
        }
        public void DelayForOver(float delay)
        {
            TimeManager.instance.CancelTimer(model.overTimer);
            model.overTimer = TimeManager.instance.StartTimer(delay,0, () =>
            {
                Z_EventHelper.Invoke(new ClipPlayEvent()
                {
                    playType = PlayType.clipMainTextOver
                });
                return true;
            }, uiHolder);
        }
        public bool Write()
        {
            //l clip ok
            if (model.nowWord == model.content.Length)
            {
                model.isDisplaying = false;

                if (DialogManager.instance.settings.autoPlaySpeed > 0)
                {
                    DelayForOver(autoPlaydelay / DialogManager.instance.settings.autoPlaySpeed);
                }

                return true;
            }

            int count = Mathf.Max(1, (int)DialogManager.instance.settings.textDisplaySpeed / 10);
            for (int i = 0; i < count && model.nowWord < model.content.Length; i++)
            {
                view.txt_.text += model.content[model.nowWord];
                model.nowWord++;
            }

            return false;
        }
    }
    public partial class UiDialogModel
    {
        public List<Clip> clips;
        public int curClipId=-1;
        public Timer timer;
    }
    public partial class UiDialogParam
    {
        public List<Clip> clips;
    }
    public partial class UiDialogCtrl :
        IZ_Listener<ShowTypeEvent>,
        IZ_Listener<ClipPlayEvent>
    {
        public override void OnCreate()
        {
            this.Register<ShowTypeEvent>();
            this.Register<ClipPlayEvent>();

            view.btn_back.onClick.AddListener(() =>
            {
                Z_EventHelper.Invoke(new ShowTypeEvent()
                {
                    showType = ShowType.Normal
                });
            });
        }
        public override void OnShow()
        {
            if (param != null)
            {
                model.clips = param.clips;
            }
            Display();
        }
        private void Display(List<Clip> clips = null)
        {
            model.curClipId = -1;
            if (clips != null)
            {
                model.clips = clips;
            }
            DisplayNext();
        }

        private void DisplayNext()
        {
            view.btn_back.gameObject.SetActive(false);
            model.curClipId++;
            if (model.curClipId >= model.clips.Count)
            {
                Z_EventHelper.Invoke(new ClipPlayEvent()
                {
                    playType = PlayType.clipsOver
                });
                return;
            }
            SetSubShow(ShowType.Normal);
            view.sub_MainText.SetActive(!string.IsNullOrEmpty(model.clips[model.curClipId].mainText));
            view.sub_Title.SetActive(!string.IsNullOrEmpty(model.clips[model.curClipId].title));
            view.sub_MainPicture.SetActive(model.clips[model.curClipId].mainPicture != null);
            view.sub_ProfilePicture.SetActive(model.clips[model.curClipId].profilePicture != null);

            view.sub_MainText.Display(model.clips[model.curClipId].mainText);
            view.sub_Title.Display(model.clips[model.curClipId].title);
            view.sub_MainPicture.Display(model.clips[model.curClipId].mainPicture);
            view.sub_ProfilePicture.Display(model.clips[model.curClipId].profilePicture);

            view.sub_History.AddClip(model.clips[model.curClipId]);
        }
        public void SetSubShow(ShowType type)
        {

            switch (type)
            {
                case ShowType.Normal:
                    view.go_op.SetActive(true);
                    view.go_history.SetActive(false);
                    view.btn_back.gameObject.SetActive(false);
                    break;
                case ShowType.Hide:
                    view.go_op.SetActive(false);
                    view.go_history.SetActive(false);
                    view.btn_back.gameObject.SetActive(true);

                    break;
                case ShowType.History:

                    view.go_op.SetActive(true);
                    view.go_history.SetActive(true);
                    view.btn_back.gameObject.SetActive(false);
                    break;

            }
           
        }
        public void OnEvent(ClipPlayEvent e)
        {
            switch (e.playType)
            {
                case PlayType.clipMainTextOver:
                    if(view.sub_MainText.isActive)
                        DisplayNext();
                    break;
            }

        }
        public void OnEvent(ShowTypeEvent e)
        {
            SetSubShow(e.showType);
        }
    }
    public partial class UiHistoryItemParam
    {
        public Clip clip;
    }
    public partial class UiHistoryItemCtrl
    {
        public override void OnShow()
        {
            if(param!=null)
            {
                view.txt_mainText.gameObject.SetActive(!string.IsNullOrEmpty(param.clip.mainText));
                view.txt_mainText.text = param.clip.mainText;

                view.txt_title.gameObject.SetActive(!string.IsNullOrEmpty(param.clip.title));
                view.txt_title.text = param.clip.title;

                view.img_ProfilePicture.gameObject.SetActive(param.clip.profilePicture!=null);
                view.img_ProfilePicture.sprite = param.clip.profilePicture;
            }
        }
    }
    public partial class UiHistoryModel
    {
        public List<Clip> historyClips=new List<Clip>();
    }

    public partial class UiHistoryCtrl
    {
        public static int limit = 10;

        UiScrViewContainer<UiHistoryItemCtrl> con;
        public override void OnCreate()
        {
            con = new UiScrViewContainer<UiHistoryItemCtrl>(view.go_historyItem,view.scr_tt);
            view.btn_back.onClick.AddListener(() =>
            {
                Z_EventHelper.Invoke(new ShowTypeEvent()
                {
                    showType = ShowType.Normal
                });
            });
        }

        public override void OnShow()
        {
            con.Clear();
            for (int i = 0; i < model.historyClips.Count;i++)
            {
                con.Add(new UiHistoryItemParam()
                {
                    clip = model.historyClips[i]
                });
            }
            con.Refresh();
        }
        public void AddClip(Clip clip)
        {
            model.historyClips.Add(clip);
            while (model.historyClips.Count > limit)
                model.historyClips.RemoveAt(0);
        }

    }

}