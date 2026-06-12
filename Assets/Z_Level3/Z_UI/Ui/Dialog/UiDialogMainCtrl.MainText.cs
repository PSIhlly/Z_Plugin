using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using Z_DataSystem;
using Z_DataSystem.Form;
using Z_DesignStyle;
using Z_Texture;
using Z_Time;
using Z_Ui;
using Z_Ui.Base;
using Z_Ui.Dialog;
using Z_Ui.Form;

namespace Ui.DialogMain
{

    namespace MainText
    {
        public partial class UiMainTextParam
        {
            public ClipForm.Data clip;
        }

        public partial class UiMainTextModel
        {
            public Timer wordTimer;
            public Timer overTimer;

            public bool isDisplaying;
            public int nowWord;
            public UiMainTextParam prm;

            public float autoPlaydelay = 5f;
        }
        public partial class UiMainTextCtrl : IZ_Listener<ClipPlayEvent>
        {


            public override void OnCreate()
            {
                Z_EventHelper.Register(this);
            }
            public override void OnShow()
            {
                model.prm = param;
                view.go_bg.SetActive(!string.IsNullOrEmpty(model.prm.clip.mainText));

                var audioData = AudioAssetForm.DataById.GetDv(model.prm.clip.mainAudio, null);
                if (audioData != null)
                {
                    audioData.Play(view.mp_);
                }

                if (!string.IsNullOrEmpty(model.prm.clip.mainText))
                {
                    model.isDisplaying = true;
                    model.nowWord = 0;
                    view.txt_.text = "";

                    Display();

                }
            }
            private void Display()
            {
                var interval = Mathf.Max(0.1f, 1f / DialogManager.instance.settings.textDisplaySpeed);
                TimeManager.instance.CancelTimer(model.wordTimer);
                model.wordTimer = TimeManager.instance.StartTimer(0, interval, Write, uiHolder);
            }

            private void DelayForOver(float delay)
            {
                TimeManager.instance.CancelTimer(model.overTimer);
                view.mp_.Stop();
                model.overTimer = TimeManager.instance.StartTimer(delay, 0, () =>
                {
                    Z_EventHelper.Invoke(new ClipPlayEvent()
                    {
                        playType = PlayType.ClipOver
                    });
                    return true;
                }, uiHolder);
            }
            private bool Write()
            {
                //l clip ok
                if (model.nowWord == model.prm.clip.mainText.Length)
                {
                    model.isDisplaying = false;

                    if (DialogManager.instance.settings.autoPlaySpeed > 0)
                    {
                        DelayForOver(model.autoPlaydelay / DialogManager.instance.settings.autoPlaySpeed);
                    }

                    return true;
                }

                int count = Mathf.Max(1, (int)DialogManager.instance.settings.textDisplaySpeed / 10);
                for (int i = 0; i < count && model.nowWord < model.prm.clip.mainText.Length; i++)
                {
                    view.txt_.text += model.prm.clip.mainText[model.nowWord];
                    model.nowWord++;
                }
                return false;
            }
            public void OnEvent(ClipPlayEvent e)
            {
                switch (e.playType)
                {
                    case PlayType.ClipSettingChange:
                        if (model.wordTimer == null || model.wordTimer.cancel)
                        {
                            DelayForOver(model.autoPlaydelay / DialogManager.instance.settings.autoPlaySpeed);
                        }
                        else
                        {
                            Display();
                        }
                        break;
                    case PlayType.ClipMainTextOver:

                        if (model.prm.clip.mainText == null && model.nowWord < model.prm.clip.mainText.Length)
                        {
                            while (model.nowWord < model.prm.clip.mainText.Length)
                            {
                                Write();
                            }
                        }
                        else
                        {
                            DelayForOver(0);
                        }
                        break;
                    case PlayType.ClipVideoOver:
                        DelayForOver(0);
                        break;
                }
            }
        }
    }


}