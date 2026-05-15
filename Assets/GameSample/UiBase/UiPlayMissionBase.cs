
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Ui.Base;
using Z_Ui;
using Z_Texture;using RenderHeads.Media.AVProVideo;
namespace Ui.PlayMission

{







    public partial class UiMissionParam:UiParam
    {
    }

    public partial class UiMissionView:UiView
    {

            public GameObject go_mission;
            public Btn btn_;
            public Sta sta_;
            public Txt txt_desc;
            public Txt txt_distance;
            public Txt txt_name;
        public UiMissionView(UiHolder uiHolder):base(uiHolder)
        {

            go_mission = uiHolder.elementTrsLst[0].gameObject;
            btn_ = uiHolder.elementTrsLst[1].GetComponent<Btn>();
            sta_ = uiHolder.elementTrsLst[2].GetComponent<Sta>();
            txt_desc = uiHolder.elementTrsLst[3].GetComponent<Txt>();
            txt_distance = uiHolder.elementTrsLst[4].GetComponent<Txt>();
            txt_name = uiHolder.elementTrsLst[5].GetComponent<Txt>();
        }

    }
    public partial class UiMissionCtrl:UiCtrl
    {
        public UiMissionView view;
        public UiMissionModel model;
        public UiMissionParam param;
        public UiPlayMissionCtrl parent=>(UiPlayMissionCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiMissionParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiMissionView(uiHolder);
            model=new UiMissionModel();


        }

    }
    public partial class UiMissionModel:UiModel
    {
        
    }
    public partial class UiPlayMissionParam:UiParam
    {
    }

    public partial class UiPlayMissionView:UiView
    {

            public Btn btn_bg;
            public GameObject go_mission;
            public UiMissionCtrl sub_mission;
        public UiPlayMissionView(UiHolder uiHolder):base(uiHolder)
        {

            btn_bg = uiHolder.elementTrsLst[0].GetComponent<Btn>();
            go_mission = uiHolder.elementTrsLst[1].gameObject;
            sub_mission = (UiMissionCtrl) uiHolder.elementTrsLst[2].GetComponent<UiHolder>().ctrl;
        }

    }
    public partial class UiPlayMissionCtrl:UiCtrl
    {
        public UiPlayMissionView view;
        public UiPlayMissionModel model;
        public UiPlayMissionParam param;
        

        public override void SetParam(UiParam param)
        {
            this.param = (UiPlayMissionParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiPlayMissionView(uiHolder);
            model=new UiPlayMissionModel();


            view.sub_mission = new UiMissionCtrl();
            view.sub_mission.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
        }

    }
    public partial class UiPlayMissionModel:UiModel
    {
        
    }
}
