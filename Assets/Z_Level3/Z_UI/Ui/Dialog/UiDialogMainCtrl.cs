using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using Ui.DialogHistory;
using Ui.DialogMain.MainText;
using Ui.DialogMain.Options;
using Ui.DialogMain.Profile;
using Ui.DialogMain.Title;
using UnityEngine;
using Z_DataSystem;
using Z_DataSystem.Form;
using Z_Texture;
using Z_Time;
using Z_Ui;
using Z_Ui.Base;
using Z_Ui.Dialog;
using Z_Ui.Form;

namespace Ui.DialogMain
{

    public partial class UiDialogMainModel
    {
        public UiDialogMainParam prm;
        public Timer timer;
    }
    public partial class UiDialogMainParam
    {
        public ClipForm.Data clip;
    }
    public partial class UiDialogMainCtrl :
        IZ_Listener<ShowTypeEvent>
    {
        public override void OnCreate()
        {
            this.Register<ShowTypeEvent>();
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
                model.prm = param;
            }
            SetSubShow(ShowType.Normal);

        }

        public void SetSubShow(ShowType type)
        {

            switch (type)
            {
                case ShowType.Normal:
                    view.page_MainText.SetShow(true, new UiMainTextParam() { clip = model.prm.clip });
                    view.page_Options.SetShow(true, new UiOptionsParam() { clip = model.prm.clip });
                    view.page_Title.SetShow(true, new UiTitleParam() { clip = model.prm.clip });
                    view.page_Profile.SetShow(true, new UiProfileParam() { clip = model.prm.clip });
                    view.btn_back.gameObject.SetActive(false);
                    DialogManager.instance.CloseHistory();
                    break;
                case ShowType.Hide:
                    view.page_MainText.SetShow(false);
                    view.page_Options.SetShow(false);
                    view.page_Title.SetShow(false);
                    view.page_Profile.SetShow(false);
                    view.btn_back.gameObject.SetActive(true);
                    break;
            }
        }
        public void OnEvent(ShowTypeEvent e)
        {
            SetSubShow(e.showType);
        }
    }


}