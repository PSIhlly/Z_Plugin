
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




    public partial class UiTeamerParam:UiParam
    {
    }

    public partial class UiTeamerView:UiView
    {

            public GameObject go_teamer;
            public Btn btn_;
            public Sld sld_hp;
            public Sld sld_sp;
            public Txt txt_;
            public Img img_;
        public UiTeamerView(UiHolder uiHolder):base(uiHolder)
        {

            go_teamer = uiHolder.elementTrsLst[0].gameObject;
            btn_ = uiHolder.elementTrsLst[1].GetComponent<Btn>();
            sld_hp = uiHolder.elementTrsLst[2].GetComponent<Sld>();
            sld_sp = uiHolder.elementTrsLst[3].GetComponent<Sld>();
            txt_ = uiHolder.elementTrsLst[4].GetComponent<Txt>();
            img_ = uiHolder.elementTrsLst[5].GetComponent<Img>();
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
            public GameObject go_menu;
            public GameObject go_map;
            public GameObject go_teamer;
            public UiTeamerCtrl sub_Teamer;
            public GameObject go_func;
            public Btn btn_menu;
            public Btn btn_map;
            public Btn btn_data;
        public UiPlaySceneMainView(UiHolder uiHolder):base(uiHolder)
        {

            page_PlaySceneMessage = (PlaySceneMessage.UiPlaySceneMessageCtrl) uiHolder.elementTrsLst[0].GetComponent<UiHolder>().ctrl;
            go_menu = uiHolder.elementTrsLst[1].gameObject;
            go_map = uiHolder.elementTrsLst[2].gameObject;
            go_teamer = uiHolder.elementTrsLst[3].gameObject;
            sub_Teamer = (UiTeamerCtrl) uiHolder.elementTrsLst[4].GetComponent<UiHolder>().ctrl;
            go_func = uiHolder.elementTrsLst[5].gameObject;
            btn_menu = uiHolder.elementTrsLst[6].GetComponent<Btn>();
            btn_map = uiHolder.elementTrsLst[7].GetComponent<Btn>();
            btn_data = uiHolder.elementTrsLst[8].GetComponent<Btn>();
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
            view.sub_Teamer = new UiTeamerCtrl();
            view.sub_Teamer.BindHolderRecursively(uiHolder.subUiHolderLst[1]);
        }

    }
    public partial class UiPlaySceneMainModel:UiModel
    {
        
    }
}
