
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Ui.Base;
using Z_Ui;

namespace Ui.ModStoryEventCmdClips

{



    public partial class UiClipParam:UiParam
    {
    }

    public partial class UiClipView:UiView
    {

            public GameObject go_clip;
            public Sta sta_exist;
            public Btn btn_add;
            public Img img_profilePicture;
            public Btn btn_profilePicture;
            public Btn btn_title;
            public Btn btn_mainText;
            public Img img_mainPicture;
            public Btn btn_mainPicture;
            public Txt txt_add;
            public Txt txt_title;
            public Txt txt_mainText;
        public UiClipView(UiHolder uiHolder):base(uiHolder)
        {

            go_clip = uiHolder.elementTrsLst[0].gameObject;
            sta_exist = uiHolder.elementTrsLst[1].GetComponent<Sta>();
            btn_add = uiHolder.elementTrsLst[2].GetComponent<Btn>();
            img_profilePicture = uiHolder.elementTrsLst[3].GetComponent<Img>();
            btn_profilePicture = uiHolder.elementTrsLst[4].GetComponent<Btn>();
            btn_title = uiHolder.elementTrsLst[5].GetComponent<Btn>();
            btn_mainText = uiHolder.elementTrsLst[6].GetComponent<Btn>();
            img_mainPicture = uiHolder.elementTrsLst[7].GetComponent<Img>();
            btn_mainPicture = uiHolder.elementTrsLst[8].GetComponent<Btn>();
            txt_add = uiHolder.elementTrsLst[9].GetComponent<Txt>();
            txt_title = uiHolder.elementTrsLst[10].GetComponent<Txt>();
            txt_mainText = uiHolder.elementTrsLst[11].GetComponent<Txt>();
        }

    }
    public partial class UiClipCtrl:UiCtrl
    {
        public UiClipView view;
        public UiClipModel model;
        public UiClipParam param;
        public UiModStoryEventCmdClipsCtrl parent=>(UiModStoryEventCmdClipsCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiClipParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiClipView(uiHolder);
            model=new UiClipModel();


        }

    }
    public partial class UiClipModel:UiModel
    {
        
    }
    public partial class UiModStoryEventCmdClipsParam:UiParam
    {
    }

    public partial class UiModStoryEventCmdClipsView:UiView
    {

            public ScrView scr_tt;
            public Btn btn_confirm;
            public Txt txt_confirm;
            public GameObject go_clip;
            public UiClipCtrl sub_Clip;
        public UiModStoryEventCmdClipsView(UiHolder uiHolder):base(uiHolder)
        {

            scr_tt = uiHolder.elementTrsLst[0].GetComponent<ScrView>();
            btn_confirm = uiHolder.elementTrsLst[1].GetComponent<Btn>();
            txt_confirm = uiHolder.elementTrsLst[2].GetComponent<Txt>();
            go_clip = uiHolder.elementTrsLst[3].gameObject;
            sub_Clip = (UiClipCtrl) uiHolder.elementTrsLst[4].GetComponent<UiHolder>().ctrl;
        }

    }
    public partial class UiModStoryEventCmdClipsCtrl:UiCtrl
    {
        public UiModStoryEventCmdClipsView view;
        public UiModStoryEventCmdClipsModel model;
        public UiModStoryEventCmdClipsParam param;
        

        public override void SetParam(UiParam param)
        {
            this.param = (UiModStoryEventCmdClipsParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiModStoryEventCmdClipsView(uiHolder);
            model=new UiModStoryEventCmdClipsModel();


            view.sub_Clip = new UiClipCtrl();
            view.sub_Clip.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
        }

    }
    public partial class UiModStoryEventCmdClipsModel:UiModel
    {
        
    }
}
