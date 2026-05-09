
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Ui.Base;
using Z_Ui;
using Z_Texture;using RenderHeads.Media.AVProVideo;
namespace Ui.PlaySceneMap

{







    public partial class UiMarkParam:UiParam
    {
    }

    public partial class UiMarkView:UiView
    {

            public GameObject go_mark;
            public Img img_mark;
            public Btn btn_mark;
        public UiMarkView(UiHolder uiHolder):base(uiHolder)
        {

            go_mark = uiHolder.elementTrsLst[0].gameObject;
            img_mark = uiHolder.elementTrsLst[1].GetComponent<Img>();
            btn_mark = uiHolder.elementTrsLst[2].GetComponent<Btn>();
        }

    }
    public partial class UiMarkCtrl:UiCtrl
    {
        public UiMarkView view;
        public UiMarkModel model;
        public UiMarkParam param;
        public UiPlaySceneMapCtrl parent=>(UiPlaySceneMapCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiMarkParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiMarkView(uiHolder);
            model=new UiMarkModel();


        }

    }
    public partial class UiMarkModel:UiModel
    {
        
    }



    public partial class UiSceneParam:UiParam
    {
    }

    public partial class UiSceneView:UiView
    {

            public GameObject go_scene;
            public Img img_scene;
            public Btn btn_;
            public Txt txt_;
        public UiSceneView(UiHolder uiHolder):base(uiHolder)
        {

            go_scene = uiHolder.elementTrsLst[0].gameObject;
            img_scene = uiHolder.elementTrsLst[1].GetComponent<Img>();
            btn_ = uiHolder.elementTrsLst[2].GetComponent<Btn>();
            txt_ = uiHolder.elementTrsLst[3].GetComponent<Txt>();
        }

    }
    public partial class UiSceneCtrl:UiCtrl
    {
        public UiSceneView view;
        public UiSceneModel model;
        public UiSceneParam param;
        public UiPlaySceneMapCtrl parent=>(UiPlaySceneMapCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiSceneParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiSceneView(uiHolder);
            model=new UiSceneModel();


        }

    }
    public partial class UiSceneModel:UiModel
    {
        
    }
    public partial class UiPlaySceneMapParam:UiParam
    {
    }

    public partial class UiPlaySceneMapView:UiView
    {

            public Btn btn_bg;
            public Sta sta_type;
            public Btn btn_world;
            public Btn btn_area;
            public RectTransform rtf_area;
            public RectTransform rtf_world;
            public RImg rimg_unlock;
            public GameObject go_mark;
            public Img img_mark;
            public Btn btn_mark;
            public UiMarkCtrl sub_mark;
            public Img img_largeMap;
            public GameObject go_scene;
            public Img img_scene;
            public UiSceneCtrl sub_scene;
            public Img img_real;
        public UiPlaySceneMapView(UiHolder uiHolder):base(uiHolder)
        {

            btn_bg = uiHolder.elementTrsLst[0].GetComponent<Btn>();
            sta_type = uiHolder.elementTrsLst[1].GetComponent<Sta>();
            btn_world = uiHolder.elementTrsLst[2].GetComponent<Btn>();
            btn_area = uiHolder.elementTrsLst[3].GetComponent<Btn>();
            rtf_area = uiHolder.elementTrsLst[4].GetComponent<RectTransform>();
            rtf_world = uiHolder.elementTrsLst[5].GetComponent<RectTransform>();
            rimg_unlock = uiHolder.elementTrsLst[6].GetComponent<RImg>();
            go_mark = uiHolder.elementTrsLst[7].gameObject;
            img_mark = uiHolder.elementTrsLst[8].GetComponent<Img>();
            btn_mark = uiHolder.elementTrsLst[9].GetComponent<Btn>();
            sub_mark = (UiMarkCtrl) uiHolder.elementTrsLst[10].GetComponent<UiHolder>().ctrl;
            img_largeMap = uiHolder.elementTrsLst[11].GetComponent<Img>();
            go_scene = uiHolder.elementTrsLst[12].gameObject;
            img_scene = uiHolder.elementTrsLst[13].GetComponent<Img>();
            sub_scene = (UiSceneCtrl) uiHolder.elementTrsLst[14].GetComponent<UiHolder>().ctrl;
            img_real = uiHolder.elementTrsLst[15].GetComponent<Img>();
        }

    }
    public partial class UiPlaySceneMapCtrl:UiCtrl
    {
        public UiPlaySceneMapView view;
        public UiPlaySceneMapModel model;
        public UiPlaySceneMapParam param;
        

        public override void SetParam(UiParam param)
        {
            this.param = (UiPlaySceneMapParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiPlaySceneMapView(uiHolder);
            model=new UiPlaySceneMapModel();


            view.sub_mark = new UiMarkCtrl();
            view.sub_mark.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
            view.sub_scene = new UiSceneCtrl();
            view.sub_scene.BindHolderRecursively(uiHolder.subUiHolderLst[1]);
        }

    }
    public partial class UiPlaySceneMapModel:UiModel
    {
        
    }
}
