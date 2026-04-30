
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Ui.Base;
using Z_Ui;
using Z_Texture;using RenderHeads.Media.AVProVideo;
namespace Ui.PlaySceneMain

{

using Ui.Stick;
using Ui.ParamShow;



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
            public UiMessageCtrl sub_message;
        public UiPlaySceneMessageView(UiHolder uiHolder):base(uiHolder)
        {

            go_message = uiHolder.elementTrsLst[0].gameObject;
            sub_message = (UiMessageCtrl) uiHolder.elementTrsLst[1].GetComponent<UiHolder>().ctrl;
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


            view.sub_message = new UiMessageCtrl();
            view.sub_message.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
        }

    }
    public partial class UiPlaySceneMessageModel:UiModel
    {
        
    }
}

namespace PlayerTouchOpt

{




    public partial class UiPlayerTouchOptParam:UiParam
    {
    }

    public partial class UiPlayerTouchOptView:UiView
    {

            public GameObject go_moveStick;
            public UiStickCtrl model_moveStick;
            public GameObject go_attackStick;
            public UiStickCtrl model_attackStick;
        public UiPlayerTouchOptView(UiHolder uiHolder):base(uiHolder)
        {

            go_moveStick = uiHolder.elementTrsLst[0].gameObject;
            model_moveStick = (UiStickCtrl) uiHolder.elementTrsLst[1].GetComponent<UiHolder>().ctrl;
            go_attackStick = uiHolder.elementTrsLst[2].gameObject;
            model_attackStick = (UiStickCtrl) uiHolder.elementTrsLst[3].GetComponent<UiHolder>().ctrl;
        }

    }
    public partial class UiPlayerTouchOptCtrl:UiCtrl
    {
        public UiPlayerTouchOptView view;
        public UiPlayerTouchOptModel model;
        public UiPlayerTouchOptParam param;
        public UiPlaySceneMainCtrl parent=>(UiPlaySceneMainCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiPlayerTouchOptParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiPlayerTouchOptView(uiHolder);
            model=new UiPlayerTouchOptModel();


            view.model_moveStick = new UiStickCtrl();
            view.model_moveStick.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
            view.model_attackStick = new UiStickCtrl();
            view.model_attackStick.BindHolderRecursively(uiHolder.subUiHolderLst[1]);
        }

    }
    public partial class UiPlayerTouchOptModel:UiModel
    {
        
    }
}




    public partial class UiTeamerParam:UiParam
    {
    }

    public partial class UiTeamerView:UiView
    {

            public GameObject go_teamer;
            public Btn btn_;
            public Txt txt_;
            public Img img_;
            public UiParamShowCtrl model_ParamShow;
        public UiTeamerView(UiHolder uiHolder):base(uiHolder)
        {

            go_teamer = uiHolder.elementTrsLst[0].gameObject;
            btn_ = uiHolder.elementTrsLst[1].GetComponent<Btn>();
            txt_ = uiHolder.elementTrsLst[2].GetComponent<Txt>();
            img_ = uiHolder.elementTrsLst[3].GetComponent<Img>();
            model_ParamShow = (UiParamShowCtrl) uiHolder.elementTrsLst[4].GetComponent<UiHolder>().ctrl;
        }

    }
    public partial class UiTeamerCtrl:UiCtrl
    {
        public UiTeamerView view;
        public UiTeamerModel model;
        public UiTeamerParam param;
        public UiPlaySceneMainCtrl parent=>(UiPlaySceneMainCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiTeamerParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiTeamerView(uiHolder);
            model=new UiTeamerModel();


            view.model_ParamShow = new UiParamShowCtrl();
            view.model_ParamShow.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
        }

    }
    public partial class UiTeamerModel:UiModel
    {
        
    }



    public partial class UiActionParam:UiParam
    {
    }

    public partial class UiActionView:UiView
    {

            public GameObject go_action;
            public Img img_;
            public Btn btn_;
            public Txt txt_;
        public UiActionView(UiHolder uiHolder):base(uiHolder)
        {

            go_action = uiHolder.elementTrsLst[0].gameObject;
            img_ = uiHolder.elementTrsLst[1].GetComponent<Img>();
            btn_ = uiHolder.elementTrsLst[2].GetComponent<Btn>();
            txt_ = uiHolder.elementTrsLst[3].GetComponent<Txt>();
        }

    }
    public partial class UiActionCtrl:UiCtrl
    {
        public UiActionView view;
        public UiActionModel model;
        public UiActionParam param;
        public UiPlaySceneMainCtrl parent=>(UiPlaySceneMainCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiActionParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiActionView(uiHolder);
            model=new UiActionModel();


        }

    }
    public partial class UiActionModel:UiModel
    {
        
    }
    public partial class UiPlaySceneMainParam:UiParam
    {
    }

    public partial class UiPlaySceneMainView:UiView
    {

            public PlaySceneMessage.UiPlaySceneMessageCtrl page_PlaySceneMessage;
            public GameObject go_eStick;
            public UiStickCtrl model_eStick;
            public GameObject go_qStick;
            public UiStickCtrl model_qStick;
            public PlayerTouchOpt.UiPlayerTouchOptCtrl page_PlayerTouchOpt;
            public GameObject go_noScene;
            public GameObject go_map;
            public GameObject go_menu;
            public UiParamShowCtrl model_ParamShow;
            public GameObject go_teamer;
            public UiTeamerCtrl sub_teamer;
            public GameObject go_func;
            public GameObject go_action;
            public UiActionCtrl sub_action;
            public Btn btn_map;
            public Btn btn_menu;
            public Btn btn_data;
        public UiPlaySceneMainView(UiHolder uiHolder):base(uiHolder)
        {

            page_PlaySceneMessage = (PlaySceneMessage.UiPlaySceneMessageCtrl) uiHolder.elementTrsLst[0].GetComponent<UiHolder>().ctrl;
            go_eStick = uiHolder.elementTrsLst[1].gameObject;
            model_eStick = (UiStickCtrl) uiHolder.elementTrsLst[2].GetComponent<UiHolder>().ctrl;
            go_qStick = uiHolder.elementTrsLst[3].gameObject;
            model_qStick = (UiStickCtrl) uiHolder.elementTrsLst[4].GetComponent<UiHolder>().ctrl;
            page_PlayerTouchOpt = (PlayerTouchOpt.UiPlayerTouchOptCtrl) uiHolder.elementTrsLst[5].GetComponent<UiHolder>().ctrl;
            go_noScene = uiHolder.elementTrsLst[6].gameObject;
            go_map = uiHolder.elementTrsLst[7].gameObject;
            go_menu = uiHolder.elementTrsLst[8].gameObject;
            model_ParamShow = (UiParamShowCtrl) uiHolder.elementTrsLst[9].GetComponent<UiHolder>().ctrl;
            go_teamer = uiHolder.elementTrsLst[10].gameObject;
            sub_teamer = (UiTeamerCtrl) uiHolder.elementTrsLst[11].GetComponent<UiHolder>().ctrl;
            go_func = uiHolder.elementTrsLst[12].gameObject;
            go_action = uiHolder.elementTrsLst[13].gameObject;
            sub_action = (UiActionCtrl) uiHolder.elementTrsLst[14].GetComponent<UiHolder>().ctrl;
            btn_map = uiHolder.elementTrsLst[15].GetComponent<Btn>();
            btn_menu = uiHolder.elementTrsLst[16].GetComponent<Btn>();
            btn_data = uiHolder.elementTrsLst[17].GetComponent<Btn>();
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
            view.model_eStick = new UiStickCtrl();
            view.model_eStick.BindHolderRecursively(uiHolder.subUiHolderLst[1]);
            view.model_qStick = new UiStickCtrl();
            view.model_qStick.BindHolderRecursively(uiHolder.subUiHolderLst[2]);
            view.page_PlayerTouchOpt = new PlayerTouchOpt.UiPlayerTouchOptCtrl();
            view.page_PlayerTouchOpt.BindHolderRecursively(uiHolder.subUiHolderLst[3]);
            view.model_ParamShow = new UiParamShowCtrl();
            view.model_ParamShow.BindHolderRecursively(uiHolder.subUiHolderLst[4]);
            view.sub_teamer = new UiTeamerCtrl();
            view.sub_teamer.BindHolderRecursively(uiHolder.subUiHolderLst[5]);
            view.sub_action = new UiActionCtrl();
            view.sub_action.BindHolderRecursively(uiHolder.subUiHolderLst[6]);
        }

    }
    public partial class UiPlaySceneMainModel:UiModel
    {
        
    }
}
