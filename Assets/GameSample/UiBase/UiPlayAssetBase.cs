
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Ui.Base;
using Z_Ui;
using Z_Texture;
using Z_Video;
using UnityEngine.Video;
namespace Ui.PlayAsset

{







    public partial class UiImageParam:UiParam
    {
    }

    public partial class UiImageView:UiView
    {

            public GameObject go_image;
            public Img img_image;
        public UiImageView(UiHolder uiHolder):base(uiHolder)
        {

            go_image = uiHolder.elementTrsLst[0].gameObject;
            img_image = uiHolder.elementTrsLst[1].GetComponent<Img>();
        }

    }
    public partial class UiImageCtrl:UiCtrl
    {
        public UiImageView view;
        public UiImageModel model;
        public UiImageParam param;
        public UiPlayAssetCtrl parent=>(UiPlayAssetCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiImageParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiImageView(uiHolder);
            model=new UiImageModel();


        }

    }
    public partial class UiImageModel:UiModel
    {
        
    }
    public partial class UiPlayAssetParam:UiParam
    {
    }

    public partial class UiPlayAssetView:UiView
    {

            public GameObject go_image;
            public Img img_image;
            public UiImageCtrl sub_Image;
        public UiPlayAssetView(UiHolder uiHolder):base(uiHolder)
        {

            go_image = uiHolder.elementTrsLst[0].gameObject;
            img_image = uiHolder.elementTrsLst[1].GetComponent<Img>();
            sub_Image = (UiImageCtrl) uiHolder.elementTrsLst[2].GetComponent<UiHolder>().ctrl;
        }

    }
    public partial class UiPlayAssetCtrl:UiCtrl
    {
        public UiPlayAssetView view;
        public UiPlayAssetModel model;
        public UiPlayAssetParam param;
        

        public override void SetParam(UiParam param)
        {
            this.param = (UiPlayAssetParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiPlayAssetView(uiHolder);
            model=new UiPlayAssetModel();


            view.sub_Image = new UiImageCtrl();
            view.sub_Image.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
        }

    }
    public partial class UiPlayAssetModel:UiModel
    {
        
    }
}
