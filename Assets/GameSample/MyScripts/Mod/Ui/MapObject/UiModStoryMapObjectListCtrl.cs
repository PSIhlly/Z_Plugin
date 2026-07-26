using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z_Ui.Base;
using Z_Texture;
using Unity.VisualScripting;
using Z_DataSystem.Form;

namespace Ui.ModStory.ModStoryMapObject.ModStoryMapObjectList
{

    public partial class UiModStoryMapObjectListParam
    {
        public int type;
        public string lab;
    }
    public partial class UiModStoryMapObjectListModel
    {
        public Dictionary<string, List<MapBaseForm.Data>> datas;
        public Action<string> createAct;
        public string lab;
    }
    public partial class UiModStoryMapObjectListCtrl
    {

        UiScrViewContainer<UiLabCtrl> labCon;
        UiScrViewContainer<UiBigItemCtrl> itemCon;
        public override void OnCreate()
        {
            model.datas = new Dictionary<string, List<MapBaseForm.Data>>();
            labCon = new UiScrViewContainer<UiLabCtrl>(this, view.go_lab, view.scr_labs);
            itemCon = new UiScrViewContainer<UiBigItemCtrl>(this, view.go_bigItem, view.scr_bigItems);
            view.btn_back.onClick.AddListener(() =>
            {
                parent.SelType(0);
            });
        }
        public override void OnShow()
        {
            model.lab = null;
            Update();
            Refresh();
        }
        public void Update()
        {
            model.datas.Clear();
            switch (param.type)
            {
                case 1:
                    foreach (var pair in MapTextureForm.DatasByLabel)
                    {
                        model.datas[pair.Key] = new List<MapBaseForm.Data>();
                        foreach (var data in pair.Value)
                        {
                            model.datas[pair.Key].Add(data);
                        }
                    }
                    model.createAct = (lab) =>
                    {
                        if (lab == null)
                        {
                            lab = "";
                        }
                        ModManager.instance.assetCtrl.CreateTex(lab);
                    };
                    break;
                case 2:
                    foreach (var pair in MapMaskForm.DatasByLabel)
                    {
                        model.datas[pair.Key] = new List<MapBaseForm.Data>();
                        foreach (var data in pair.Value)
                        {
                            model.datas[pair.Key].Add(data);
                        }
                    }
                    model.createAct = (lab) =>
                    {
                        if (lab == null)
                        {
                            lab = "";
                        }
                        ModManager.instance.assetCtrl.CreateMask(lab);
                    };
                    break;
                case 3:
                    foreach (var pair in MapObjectForm.DatasByLabel)
                    {
                        model.datas[pair.Key] = new List<MapBaseForm.Data>();
                        foreach (var data in pair.Value)
                        {
                            model.datas[pair.Key].Add(data);
                        }
                    }
                    model.createAct = (lab) =>
                    {
                        if (lab == null)
                        {
                            lab = "";
                        }
                        ModManager.instance.assetCtrl.CreateObject(lab);
                    };
                    break;

            }
        }
        public void Refresh()
        {

            labCon.Clear();
            labCon.Add(new UiLabParam()
            {
                lab = null
            });
            foreach (var lab in model.datas.Keys)
            {
                if (lab != "")
                    labCon.Add(new UiLabParam()
                    {
                        lab = lab
                    });
            }
            labCon.Refresh();
            itemCon.Clear();

            List<MapBaseForm.Data> datas = null;
            if (model.lab == null)
            {
                datas=new List<MapBaseForm.Data>();
                foreach(var v in model.datas.Values)
                {
                    datas.AddRange(v);
                }
            }
            else
            {
                datas= model.datas[model.lab];
            }

            foreach (var data in datas.OrderBy(d => d.id))
            {
                itemCon.Add(new UiBigItemParam()
                {
                    data = data
                });
            }
            itemCon.Add(new UiBigItemParam()
            {
                data = null
            });
            itemCon.Refresh();



        }
    }

    public partial class UiLabParam
    {
        public string lab;
    }
    public partial class UiLabModel
    {
        public string lab;
    }
    public partial class UiLabCtrl
    {

        public override void OnCreate()
        {

            view.btn_.onClick.AddListener(() =>
            {
                parent.model.lab = model.lab;
                parent.Refresh();
            });

        }
        public override void OnShow()
        {
            model.lab = param.lab;
            Refresh();
        }
        public void Refresh()
        {

            view.sta_valid.ChangeState(model.lab == null ? 0 : 1);
            view.sta_.ChangeState(model.lab == parent.model.lab ? 1 : 0);
            if (model.lab != null)
            {
                view.txt_.text = model.lab;
            }
        }
    }


    public partial class UiBigItemParam
    {
        public MapBaseForm.Data data;
    }
    public partial class UiBigItemModel
    {
        public MapBaseForm.Data data;
    }
    public partial class UiBigItemCtrl
    {

        public override void OnCreate()
        {

            view.btn_new.onClick.AddListener(() =>
            {
                parent.model.createAct?.Invoke(parent.model.lab);
                parent.Update();
                parent.Refresh();
            });
            view.btn_.onClick.AddListener(() =>
            {
                parent.parent.SelData(model.data);
            });

        }
        public override void OnShow()
        {
            model.data = param.data;
            Refresh();
        }
        public void Refresh()
        {
            view.sta_exist.ChangeState(model.data == null ? 0 : 1);
            if (model.data != null)
            {
                view.txt_.text = model.data.name;
                int texId = GetFirstAnimTexId(model.data);
                if (texId != 0 && TexAssetForm.DataById.ContainsKey(texId))
                    view.img_.BindTexData(TexAssetForm.DataById[texId]);
            }
        }
        /// <summary>
        /// 获取data动画帧的第一张贴图id，替代icon字段
        /// </summary>
        private int GetFirstAnimTexId(MapBaseForm.Data data)
        {
            if (data is MapTextureForm.Data texData && texData.texs != null && texData.texs.Count > 0)
                return texData.texs[0];
            if (data is MapMaskForm.Data maskData && maskData.texsName != null && maskData.texsName.Count > 0)
                return maskData.texsName[0];
            if (data is MapObjectForm.Data objData && objData.model != null
                && objData.model.subUnitTexsName != null && objData.model.subUnitTexsName.Count > 0
                && objData.model.subUnitTexsName[0] != null && objData.model.subUnitTexsName[0].Count > 0)
                return objData.model.subUnitTexsName[0][0];
            return 0;
        }
    }


}