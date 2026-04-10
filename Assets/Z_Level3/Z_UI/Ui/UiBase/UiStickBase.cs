
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Ui.Base;
using Z_Ui;
using Z_Texture;using RenderHeads.Media.AVProVideo;
namespace Ui.Stick

{




    public partial class UiStickParam:UiParam
    {
    }

    public partial class UiStickView:UiView
    {

            public GameObject go_stick;
            public GameObject go_area;
            public RectTransform rtf_area;
            public RectTransform rtf_stick;
            public Img img_stick;
        public UiStickView(UiHolder uiHolder):base(uiHolder)
        {

            go_stick = uiHolder.elementTrsLst[0].gameObject;
            go_area = uiHolder.elementTrsLst[1].gameObject;
            rtf_area = uiHolder.elementTrsLst[2].GetComponent<RectTransform>();
            rtf_stick = uiHolder.elementTrsLst[3].GetComponent<RectTransform>();
            img_stick = uiHolder.elementTrsLst[4].GetComponent<Img>();
        }

    }
    public partial class UiStickCtrl:UiCtrl
    {
        public UiStickView view;
        public UiStickModel model;
        public UiStickParam param;
        

        public override void SetParam(UiParam param)
        {
            this.param = (UiStickParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiStickView(uiHolder);
            model=new UiStickModel();


        }

    }
    public partial class UiStickModel:UiModel
    {
        
    }
}
