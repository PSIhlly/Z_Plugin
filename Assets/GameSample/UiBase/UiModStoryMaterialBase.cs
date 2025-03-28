
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Ui.Base;
using Z_Ui;

namespace Ui.ModStoryMaterial

{




    public partial class UiTextureItemParam:UiParam
    {
    }

    public partial class UiTextureItemView:UiView
    {

            public GameObject go_textureItem;
            public Btn btn_item;
            public Sta sta_item;
            public Img img_;
            public Txt txt_name;
        public UiTextureItemView(UiHolder uiHolder):base(uiHolder)
        {

            go_textureItem = uiHolder.elementTrsLst[0].gameObject;
            btn_item = uiHolder.elementTrsLst[1].GetComponent<Btn>();
            sta_item = uiHolder.elementTrsLst[2].GetComponent<Sta>();
            img_ = uiHolder.elementTrsLst[3].GetComponent<Img>();
            txt_name = uiHolder.elementTrsLst[4].GetComponent<Txt>();
        }

    }
    public partial class UiTextureItemCtrl:UiCtrl
    {
        public UiTextureItemView view;
        public UiTextureItemModel model;
        public UiTextureItemParam param;
        public UiModStoryMaterialTextureCtrl parent=>(UiModStoryMaterialTextureCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiTextureItemParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiTextureItemView(uiHolder);
            model=new UiTextureItemModel();


        }

    }
    public partial class UiTextureItemModel:UiModel
    {
        
    }

    public partial class UiAnimTypeItemParam:UiParam
    {
    }

    public partial class UiAnimTypeItemView:UiView
    {

            public GameObject go_animTypeItem;
            public Btn btn_item;
            public Sta sta_item;
            public Img img_;
            public Txt txt_name;
        public UiAnimTypeItemView(UiHolder uiHolder):base(uiHolder)
        {

            go_animTypeItem = uiHolder.elementTrsLst[0].gameObject;
            btn_item = uiHolder.elementTrsLst[1].GetComponent<Btn>();
            sta_item = uiHolder.elementTrsLst[2].GetComponent<Sta>();
            img_ = uiHolder.elementTrsLst[3].GetComponent<Img>();
            txt_name = uiHolder.elementTrsLst[4].GetComponent<Txt>();
        }

    }
    public partial class UiAnimTypeItemCtrl:UiCtrl
    {
        public UiAnimTypeItemView view;
        public UiAnimTypeItemModel model;
        public UiAnimTypeItemParam param;
        public UiModStoryMaterialTextureCtrl parent=>(UiModStoryMaterialTextureCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiAnimTypeItemParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiAnimTypeItemView(uiHolder);
            model=new UiAnimTypeItemModel();


        }

    }
    public partial class UiAnimTypeItemModel:UiModel
    {
        
    }
    public partial class UiModStoryMaterialTextureParam:UiParam
    {
    }

    public partial class UiModStoryMaterialTextureView:UiView
    {

            public Sta sta_show;
            public GameObject go_items;
            public ScrView scr_items;
            public Sta sta_innerId;
            public GameObject go_anims;
            public ScrView scr_anims;
            public GameObject go_textureItem;
            public UiTextureItemCtrl sub_TextureItem;
            public Btn btn_play;
            public Ipt ipt_intervalSet;
            public Btn btn_delete;
            public Ipt ipt_name;
            public Txt txt_play;
            public Txt txt_intervalSet;
            public Txt txt_delete;
            public Txt txt_name;
            public Img img_tex;
            public Btn btn_replace;
            public Btn btn_deleteId;
            public Txt txt_replace;
            public Txt txt_deleteId;
            public GameObject go_animTypeItem;
            public UiAnimTypeItemCtrl sub_AnimTypeItem;
        public UiModStoryMaterialTextureView(UiHolder uiHolder):base(uiHolder)
        {

            sta_show = uiHolder.elementTrsLst[0].GetComponent<Sta>();
            go_items = uiHolder.elementTrsLst[1].gameObject;
            scr_items = uiHolder.elementTrsLst[2].GetComponent<ScrView>();
            sta_innerId = uiHolder.elementTrsLst[3].GetComponent<Sta>();
            go_anims = uiHolder.elementTrsLst[4].gameObject;
            scr_anims = uiHolder.elementTrsLst[5].GetComponent<ScrView>();
            go_textureItem = uiHolder.elementTrsLst[6].gameObject;
            sub_TextureItem = (UiTextureItemCtrl) uiHolder.elementTrsLst[7].GetComponent<UiHolder>().ctrl;
            btn_play = uiHolder.elementTrsLst[8].GetComponent<Btn>();
            ipt_intervalSet = uiHolder.elementTrsLst[9].GetComponent<Ipt>();
            btn_delete = uiHolder.elementTrsLst[10].GetComponent<Btn>();
            ipt_name = uiHolder.elementTrsLst[11].GetComponent<Ipt>();
            txt_play = uiHolder.elementTrsLst[12].GetComponent<Txt>();
            txt_intervalSet = uiHolder.elementTrsLst[13].GetComponent<Txt>();
            txt_delete = uiHolder.elementTrsLst[14].GetComponent<Txt>();
            txt_name = uiHolder.elementTrsLst[15].GetComponent<Txt>();
            img_tex = uiHolder.elementTrsLst[16].GetComponent<Img>();
            btn_replace = uiHolder.elementTrsLst[17].GetComponent<Btn>();
            btn_deleteId = uiHolder.elementTrsLst[18].GetComponent<Btn>();
            txt_replace = uiHolder.elementTrsLst[19].GetComponent<Txt>();
            txt_deleteId = uiHolder.elementTrsLst[20].GetComponent<Txt>();
            go_animTypeItem = uiHolder.elementTrsLst[21].gameObject;
            sub_AnimTypeItem = (UiAnimTypeItemCtrl) uiHolder.elementTrsLst[22].GetComponent<UiHolder>().ctrl;
        }

    }
    public partial class UiModStoryMaterialTextureCtrl:UiCtrl
    {
        public UiModStoryMaterialTextureView view;
        public UiModStoryMaterialTextureModel model;
        public UiModStoryMaterialTextureParam param;
        public UiModStoryMaterialCtrl parent=>(UiModStoryMaterialCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiModStoryMaterialTextureParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiModStoryMaterialTextureView(uiHolder);
            model=new UiModStoryMaterialTextureModel();


            view.sub_TextureItem = new UiTextureItemCtrl();
            view.sub_TextureItem.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
            view.sub_AnimTypeItem = new UiAnimTypeItemCtrl();
            view.sub_AnimTypeItem.BindHolderRecursively(uiHolder.subUiHolderLst[1]);
        }

    }
    public partial class UiModStoryMaterialTextureModel:UiModel
    {
        
    }


    public partial class UiMaskItemParam:UiParam
    {
    }

    public partial class UiMaskItemView:UiView
    {

            public GameObject go_maskItem;
            public Btn btn_item;
            public Sta sta_item;
            public Img img_;
            public Txt txt_name;
        public UiMaskItemView(UiHolder uiHolder):base(uiHolder)
        {

            go_maskItem = uiHolder.elementTrsLst[0].gameObject;
            btn_item = uiHolder.elementTrsLst[1].GetComponent<Btn>();
            sta_item = uiHolder.elementTrsLst[2].GetComponent<Sta>();
            img_ = uiHolder.elementTrsLst[3].GetComponent<Img>();
            txt_name = uiHolder.elementTrsLst[4].GetComponent<Txt>();
        }

    }
    public partial class UiMaskItemCtrl:UiCtrl
    {
        public UiMaskItemView view;
        public UiMaskItemModel model;
        public UiMaskItemParam param;
        public UiModStoryMaterialMaskCtrl parent=>(UiModStoryMaterialMaskCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiMaskItemParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiMaskItemView(uiHolder);
            model=new UiMaskItemModel();


        }

    }
    public partial class UiMaskItemModel:UiModel
    {
        
    }

    public partial class UiMaskTypeItemParam:UiParam
    {
    }

    public partial class UiMaskTypeItemView:UiView
    {

            public GameObject go_maskTypeItem;
            public Btn btn_item;
            public Sta sta_item;
            public Img img_;
            public Txt txt_name;
        public UiMaskTypeItemView(UiHolder uiHolder):base(uiHolder)
        {

            go_maskTypeItem = uiHolder.elementTrsLst[0].gameObject;
            btn_item = uiHolder.elementTrsLst[1].GetComponent<Btn>();
            sta_item = uiHolder.elementTrsLst[2].GetComponent<Sta>();
            img_ = uiHolder.elementTrsLst[3].GetComponent<Img>();
            txt_name = uiHolder.elementTrsLst[4].GetComponent<Txt>();
        }

    }
    public partial class UiMaskTypeItemCtrl:UiCtrl
    {
        public UiMaskTypeItemView view;
        public UiMaskTypeItemModel model;
        public UiMaskTypeItemParam param;
        public UiModStoryMaterialMaskCtrl parent=>(UiModStoryMaterialMaskCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiMaskTypeItemParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiMaskTypeItemView(uiHolder);
            model=new UiMaskTypeItemModel();


        }

    }
    public partial class UiMaskTypeItemModel:UiModel
    {
        
    }
    public partial class UiModStoryMaterialMaskParam:UiParam
    {
    }

    public partial class UiModStoryMaterialMaskView:UiView
    {

            public Sta sta_exist;
            public GameObject go_items;
            public ScrView scr_items;
            public Txt txt_usageExample;
            public Img img_tex;
            public Btn btn_replace;
            public Img img_1;
            public Img img_2;
            public Img img_3;
            public Img img_4;
            public Img img_5;
            public Img img_6;
            public Img img_7;
            public Img img_8;
            public Img img_9;
            public Btn btn_delete;
            public Ipt ipt_name;
            public GameObject go_masks;
            public ScrView scr_masks;
            public GameObject go_maskItem;
            public UiMaskItemCtrl sub_MaskItem;
            public Txt txt_replace;
            public Txt txt_delete;
            public Txt txt_name;
            public GameObject go_maskTypeItem;
            public UiMaskTypeItemCtrl sub_MaskTypeItem;
        public UiModStoryMaterialMaskView(UiHolder uiHolder):base(uiHolder)
        {

            sta_exist = uiHolder.elementTrsLst[0].GetComponent<Sta>();
            go_items = uiHolder.elementTrsLst[1].gameObject;
            scr_items = uiHolder.elementTrsLst[2].GetComponent<ScrView>();
            txt_usageExample = uiHolder.elementTrsLst[3].GetComponent<Txt>();
            img_tex = uiHolder.elementTrsLst[4].GetComponent<Img>();
            btn_replace = uiHolder.elementTrsLst[5].GetComponent<Btn>();
            img_1 = uiHolder.elementTrsLst[6].GetComponent<Img>();
            img_2 = uiHolder.elementTrsLst[7].GetComponent<Img>();
            img_3 = uiHolder.elementTrsLst[8].GetComponent<Img>();
            img_4 = uiHolder.elementTrsLst[9].GetComponent<Img>();
            img_5 = uiHolder.elementTrsLst[10].GetComponent<Img>();
            img_6 = uiHolder.elementTrsLst[11].GetComponent<Img>();
            img_7 = uiHolder.elementTrsLst[12].GetComponent<Img>();
            img_8 = uiHolder.elementTrsLst[13].GetComponent<Img>();
            img_9 = uiHolder.elementTrsLst[14].GetComponent<Img>();
            btn_delete = uiHolder.elementTrsLst[15].GetComponent<Btn>();
            ipt_name = uiHolder.elementTrsLst[16].GetComponent<Ipt>();
            go_masks = uiHolder.elementTrsLst[17].gameObject;
            scr_masks = uiHolder.elementTrsLst[18].GetComponent<ScrView>();
            go_maskItem = uiHolder.elementTrsLst[19].gameObject;
            sub_MaskItem = (UiMaskItemCtrl) uiHolder.elementTrsLst[20].GetComponent<UiHolder>().ctrl;
            txt_replace = uiHolder.elementTrsLst[21].GetComponent<Txt>();
            txt_delete = uiHolder.elementTrsLst[22].GetComponent<Txt>();
            txt_name = uiHolder.elementTrsLst[23].GetComponent<Txt>();
            go_maskTypeItem = uiHolder.elementTrsLst[24].gameObject;
            sub_MaskTypeItem = (UiMaskTypeItemCtrl) uiHolder.elementTrsLst[25].GetComponent<UiHolder>().ctrl;
        }

    }
    public partial class UiModStoryMaterialMaskCtrl:UiCtrl
    {
        public UiModStoryMaterialMaskView view;
        public UiModStoryMaterialMaskModel model;
        public UiModStoryMaterialMaskParam param;
        public UiModStoryMaterialCtrl parent=>(UiModStoryMaterialCtrl)uiHolder.parent.ctrl;

        public override void SetParam(UiParam param)
        {
            this.param = (UiModStoryMaterialMaskParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiModStoryMaterialMaskView(uiHolder);
            model=new UiModStoryMaterialMaskModel();


            view.sub_MaskItem = new UiMaskItemCtrl();
            view.sub_MaskItem.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
            view.sub_MaskTypeItem = new UiMaskTypeItemCtrl();
            view.sub_MaskTypeItem.BindHolderRecursively(uiHolder.subUiHolderLst[1]);
        }

    }
    public partial class UiModStoryMaterialMaskModel:UiModel
    {
        
    }
    public partial class UiModStoryMaterialParam:UiParam
    {
    }

    public partial class UiModStoryMaterialView:UiView
    {

            public UiModStoryMaterialTextureCtrl sub_ModStoryMaterialTexture;
            public UiModStoryMaterialMaskCtrl sub_ModStoryMaterialMask;
            public Btn btn_back;
            public Btn btn_texture;
            public Sta sta_texture;
            public Btn btn_mask;
            public Sta sta_mask;
            public Img img_texture;
            public Txt txt_texture;
            public Img img_mask;
            public Txt txt_mask;
        public UiModStoryMaterialView(UiHolder uiHolder):base(uiHolder)
        {

            sub_ModStoryMaterialTexture = (UiModStoryMaterialTextureCtrl) uiHolder.elementTrsLst[0].GetComponent<UiHolder>().ctrl;
            sub_ModStoryMaterialMask = (UiModStoryMaterialMaskCtrl) uiHolder.elementTrsLst[1].GetComponent<UiHolder>().ctrl;
            btn_back = uiHolder.elementTrsLst[2].GetComponent<Btn>();
            btn_texture = uiHolder.elementTrsLst[3].GetComponent<Btn>();
            sta_texture = uiHolder.elementTrsLst[4].GetComponent<Sta>();
            btn_mask = uiHolder.elementTrsLst[5].GetComponent<Btn>();
            sta_mask = uiHolder.elementTrsLst[6].GetComponent<Sta>();
            img_texture = uiHolder.elementTrsLst[7].GetComponent<Img>();
            txt_texture = uiHolder.elementTrsLst[8].GetComponent<Txt>();
            img_mask = uiHolder.elementTrsLst[9].GetComponent<Img>();
            txt_mask = uiHolder.elementTrsLst[10].GetComponent<Txt>();
        }

    }
    public partial class UiModStoryMaterialCtrl:UiCtrl
    {
        public UiModStoryMaterialView view;
        public UiModStoryMaterialModel model;
        public UiModStoryMaterialParam param;
        

        public override void SetParam(UiParam param)
        {
            this.param = (UiModStoryMaterialParam)param;
        }

        public override void BindHolderRecursively(UiHolder uiHolder)
        {

            base.BindHolderRecursively(uiHolder);

            view = new UiModStoryMaterialView(uiHolder);
            model=new UiModStoryMaterialModel();


            view.sub_ModStoryMaterialTexture = new UiModStoryMaterialTextureCtrl();
            view.sub_ModStoryMaterialTexture.BindHolderRecursively(uiHolder.subUiHolderLst[0]);
            view.sub_ModStoryMaterialMask = new UiModStoryMaterialMaskCtrl();
            view.sub_ModStoryMaterialMask.BindHolderRecursively(uiHolder.subUiHolderLst[1]);
        }

    }
    public partial class UiModStoryMaterialModel:UiModel
    {
        
    }
}
