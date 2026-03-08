
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Ui.Base;
using Z_Ui;
using Z_Texture;using RenderHeads.Media.AVProVideo;
namespace Ui.PlaySceneMain

{

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

            public GameObject go_move;
            public RectTransform rtf_move;
            public GameObject go_attack;
            public RectTransform rtf_attack;
            public RectTransform rtf_moveStick;
            public RectTransform rtf_attackStick;
        public UiPlayerTouchOptView(UiHolder uiHolder):base(uiHolder)
        {

            go_move = uiHolder.elementTrsLst[0].gameObject;
            rtf_move = uiHolder.elementTrsLst[1].GetComponent<RectTransform>();
            go_attack = uiHolder.elementTrsLst[2].gameObject;
            rtf_attack = uiHolder.elementTrsLst[3].GetComponent<RectTransform>();
            rtf_moveStick = uiHolder.elementTrsLst[4].GetComponent<RectTransform>();
            rtf_attackStick = uiHolder.elementTrsLst[5].GetComponent<RectTransform>();
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
    public partial class UiPlaySceneMainParam:UiParam
    {
    }

    public partial class UiPlaySceneMainView:UiView
    {

            public PlaySceneMessage.UiPlaySceneMessageCtrl page_PlaySceneMessage;
            public PlayerTouchOpt.UiPlayerTouchOptCtrl page_PlayerTouchOpt;
            public GameObject go_noScene;
            public GameObject go_map;
            public GameObject go_menu;
            public UiParamShowCtrl model_ParamShow;
            public GameObject go_teamer;
            public UiTeamerCtrl sub_teamer;
            public GameObject go_func;
            public Btn btn_map;
            public Btn btn_menu;
            public Btn btn_data;
        public UiPlaySceneMainView(UiHolder uiHolder):base(uiHolder)
        {

            page_PlaySceneMessage = (PlaySceneMessage.UiPlaySceneMessageCtrl) uiHolder.elementTrsLst[0].GetComponent<UiHolder>().ctrl;
            page_PlayerTouchOpt = (PlayerTouchOpt.UiPlayerTouchOptCtrl) uiHolder.elementTrsLst[1].GetComponent<UiHolder>().ctrl;
            go_noScene = uiHolder.elementTrsLst[2].gameObject;
            go_map = uiHolder.elementTrsLst[3].gameObject;
            go_menu = uiHolder.elementTrsLst[4].gameObject;
            model_ParamShow = (UiParamShowCtrl) uiHolder.elementTrsLst[5].GetComponent<UiHolder>().ctrl;
            go_teamer = uiHolder.elementTrsLst[6].gameObject;
            sub_teamer = (UiTeamerCtrl) uiHolder.elementTrsLst[7].GetComponent<UiHolder>().ctrl;
            go_func = uiHolder.elementTrsLst[8].gameObject;
            btn_map = uiHolder.elementTrsLst[9].GetComponent<Btn>();
            btn_menu = uiHolder.elementTrsLst[10].GetComponent<Btn>();
            btn_data = uiHolder.elementTrsLst[11].GetComponent<Btn>();
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
            view.page_PlayerTouchOpt = new PlayerTouchOpt.UiPlayerTouchOptCtrl();
            view.page_PlayerTouchOpt.BindHolderRecursively(uiHolder.subUiHolderLst[1]);
            view.model_ParamShow = new UiParamShowCtrl();
            view.model_ParamShow.BindHolderRecursively(uiHolder.subUiHolderLst[2]);
            view.sub_teamer = new UiTeamerCtrl();
            view.sub_teamer.BindHolderRecursively(uiHolder.subUiHolderLst[3]);
        }

    }
    public partial class UiPlaySceneMainModel:UiModel
    {
        
    }
}
