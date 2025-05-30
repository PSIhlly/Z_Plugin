
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Ui.Base;
using Z_Ui;

namespace Ui.PlaySceneMain

{


namespace PlaySceneMessage

{



    public partial class UiMessageParam:UiParam
    {
    }

    public partial class UiMessageView:UiView
    {

            public GameObject go_message;
            public Txt txt_;
        public UiMessageView(UiHolder uiHolder):base(uiHolder)
        {

            go_message = uiHolder.elementTrsLst[0].gameObject;
            txt_ = uiHolder.elementTrsLst[1].GetComponent<Txt>();
        }

    }
    public partial class UiMessageCtrl:UiCtrl
    {
        public UiMessageView view;
        public UiMessageModel model;
        public UiMessageParam param;
        public UiPlaySceneMessageCtrl parent=>(UiPlaySceneMessageCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiMessageParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiMessageView(uiHolder);
            model=new UiMessageModel();


        }

    }
    public partial class UiMessageModel:UiModel
    {
        
    }
    public partial class UiPlaySceneMessageParam:UiParam
    {
    }

    public partial class UiPlaySceneMessageView:UiView
    {

            public GameObject go_message;
            public UiMessageCtrl sub_Message;
        public UiPlaySceneMessageView(UiHolder uiHolder):base(uiHolder)
        {

            go_message = uiHolder.elementTrsLst[0].gameObject;
            sub_Message = (UiMessageCtrl) uiHolder.elementTrsLst[1].GetComponent<UiHolder>().ctrl;
        }

    }
    public partial class UiPlaySceneMessageCtrl:UiCtrl
    {
        public UiPlaySceneMessageView view;
        public UiPlaySceneMessageModel model;
        public UiPlaySceneMessageParam param;
        public UiPlaySceneMainCtrl parent=>(UiPlaySceneMainCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiPlaySceneMessageParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiPlaySceneMessageView(uiHolder);
            model=new UiPlaySceneMessageModel();


            view.sub_Message = new UiMessageCtrl();
            view.sub_Message.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
        }

    }
    public partial class UiPlaySceneMessageModel:UiModel
    {
        
    }
}

    public partial class UiPlaySceneMainParam:UiParam
    {
    }

    public partial class UiPlaySceneMainView:UiView
    {

            public Btn btn_menu;
            public PlaySceneMessage.UiPlaySceneMessageCtrl page_PlaySceneMessage;
            public Btn btn_data;
            public Txt txt_menu;
            public Txt txt_data;
        public UiPlaySceneMainView(UiHolder uiHolder):base(uiHolder)
        {

            btn_menu = uiHolder.elementTrsLst[0].GetComponent<Btn>();
            page_PlaySceneMessage = (PlaySceneMessage.UiPlaySceneMessageCtrl) uiHolder.elementTrsLst[1].GetComponent<UiHolder>().ctrl;
            btn_data = uiHolder.elementTrsLst[2].GetComponent<Btn>();
            txt_menu = uiHolder.elementTrsLst[3].GetComponent<Txt>();
            txt_data = uiHolder.elementTrsLst[4].GetComponent<Txt>();
        }

    }
    public partial class UiPlaySceneMainCtrl:UiCtrl
    {
        public UiPlaySceneMainView view;
        public UiPlaySceneMainModel model;
        public UiPlaySceneMainParam param;
        

        public override void SetParam(UiParam param)
        {
            this.param = (UiPlaySceneMainParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiPlaySceneMainView(uiHolder);
            model=new UiPlaySceneMainModel();


            view.page_PlaySceneMessage = new PlaySceneMessage.UiPlaySceneMessageCtrl();
            view.page_PlaySceneMessage.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
        }

    }
    public partial class UiPlaySceneMainModel:UiModel
    {
        
    }
}
