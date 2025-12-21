using Z_Time;
using Z_Ui.Dialog;
using Z_Ui.Form;

namespace Ui.DialogMain
{

    #region func
    namespace Options
    {
        public partial class UiOptionsParam
        {
            public ClipForm.Data clip;
        }
        public partial class UiOptionsModel
        {
            public UiOptionsParam prm;
        }
        public partial class UiOptionsCtrl
        {
            public float autoPlaySpeed
            {
                get
                {
                    return DialogManager.instance.settings.autoPlaySpeed;
                }
                set
                {
                    DialogManager.instance.settings.autoPlaySpeed = value;
                }
            }
            public float textDisplaySpeed
            {
                get
                {
                    return DialogManager.instance.settings.textDisplaySpeed;
                }
                set
                {
                    DialogManager.instance.settings.textDisplaySpeed = value;
                }
            }
            public float autoPlayDelay
            {
                get
                {
                    return parent.view.page_MainText.model.autoPlaydelay;
                }
                set
                {
                    parent.view.page_MainText.model.autoPlaydelay = value;
                }
            }


            public override void OnCreate()
            {
                view.btn_over.onClick.AddListener(() =>
                {
                    Z_EventHelper.Invoke(new ClipPlayEvent()
                    {
                        playType = PlayType.ClipMainTextOver
                    });
                });

                view.btn_autoPlay.onClick.AddListener(() =>
                {
                    view.sta_autoPlay.ChangeState();
                    autoPlaySpeed = view.sta_autoPlay.state;
                    textDisplaySpeed = 5;
                    autoPlayDelay = 5f;
                    Z_EventHelper.Invoke(new ClipPlayEvent()
                    {
                        playType = PlayType.ClipSettingChange
                    });
                });
                view.btn_skip.onClickDown = () =>
                {
                    view.sta_autoPlay.ChangeState(0);
                    view.sta_skip.ChangeState(1);
                    autoPlayDelay = 0.1f;

                    autoPlaySpeed = 5;
                    textDisplaySpeed = 60;

                    Z_EventHelper.Invoke(new ClipPlayEvent()
                    {
                        playType = PlayType.ClipSettingChange
                    });
                };
                view.btn_skip.onClickUp = () =>
                {
                    view.sta_skip.ChangeState(0);
                    autoPlaySpeed = view.sta_autoPlay.state;
                    textDisplaySpeed = 5;
                    autoPlayDelay = 5f;

                };
                view.btn_hide.onClick.AddListener(() =>
                {
                    Z_EventHelper.Invoke(new ShowTypeEvent()
                    {
                        showType = ShowType.Hide
                    });
                });
                view.btn_history.onClick.AddListener(() =>
                {
                    DialogManager.instance.ShowHistory();

                });
            }
            public override void OnHide()
            {
                TimeManager.instance.AddNextUpdateWithoutCheckAction(() =>
                {
                    if (!active)
                    {
                        if (view.sta_skip.state == 1)
                        {
                            view.sta_skip.ChangeState(0);
                            autoPlaySpeed = 0;
                            textDisplaySpeed = 5;
                            autoPlayDelay = 5f;
                        }
                    }
                });

            }
            public override void OnShow()
            {
                model.prm = param;
                view.go_func.SetActive(!string.IsNullOrEmpty(model.prm.clip.mainText));
                view.sta_autoPlay.ChangeState((int)DialogManager.instance.settings.autoPlaySpeed > 0 ? 1 : 0);
            }
        }
    }

    #endregion

}