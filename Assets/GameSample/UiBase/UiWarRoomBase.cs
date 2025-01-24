
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Ui.Base;
using Z_Ui;

namespace Ui
{


    public partial class UiWarRoomParam:UiParam
    {
    }

    public partial class UiWarRoomView:UiView
    {

            public Txt txt_title;
            public Img img_StfIcon;
            public Img img_SpIcon;
            public Txt txt_StfCnt;
            public Txt txt_SpCnt;
        public UiWarRoomView(UiHolder uiHolder):base(uiHolder)
        {

            txt_title = uiHolder.elementTrsLst[0].GetComponent<Txt>();
            img_StfIcon = uiHolder.elementTrsLst[1].GetComponent<Img>();
            img_SpIcon = uiHolder.elementTrsLst[2].GetComponent<Img>();
            txt_StfCnt = uiHolder.elementTrsLst[3].GetComponent<Txt>();
            txt_SpCnt = uiHolder.elementTrsLst[4].GetComponent<Txt>();
        }

    }
    public partial class UiWarRoomCtrl:UiCtrl
    {
        public UiWarRoomView view;
        public UiWarRoomModel model;
        public UiWarRoomParam param;
        

        public override void SetParam(UiParam param)
        {
            this.param = (UiWarRoomParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiWarRoomView(uiHolder);
            model=new UiWarRoomModel();


        }

    }
    public partial class UiWarRoomModel:UiModel
    {
        
    }
}
