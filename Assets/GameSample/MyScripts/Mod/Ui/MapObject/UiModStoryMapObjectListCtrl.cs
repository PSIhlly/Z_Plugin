using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z_Ui.Base;
using Z_Texture;
using Unity.VisualScripting;

namespace Ui.ModStory.ModStoryMapObject.ModStoryMapObjectList
{

    public partial class UiModStoryMapObjectListParam
    {
        public int type;
        public string lab;
    }
    public partial class UiModStoryMapObjectListModel
    {
        public Dictionary<string,List<MapBaseForm.Data>> datas;
        public Action<string> createAct;
        public string lab;
    }
    public partial class UiModStoryMapObjectListCtrl
    {

        UiScrViewContainer<UiLabCtrl> labCon;
        UiScrViewContainer<UiItemCtrl> itemCon;
        public override void OnCreate()
        {
            model.datas=new Dictionary<string, List<MapBaseForm.Data>>();
            labCon = new UiScrViewContainer<UiLabCtrl>(view.go_lab, view.scr_labs);
            itemCon = new UiScrViewContainer<UiItemCtrl>(view.go_item, view.scr_items);

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
                labCon.Add(new UiLabParam()
                {
                    lab = lab
                });
            }
            labCon.Refresh();
            itemCon.Clear();
            
            if(model.lab!=null)
            {
                foreach (var data in model.datas[model.lab])
                {
                    itemCon.Add(new UiItemParam()
                    {
                        data = data
                    });
                }
                itemCon.Add(new UiItemParam()
                {
                    data = null
                });
            }
            
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
            });

        }
        public override void OnShow()
        {

            Refresh();
        }
        public void Refresh()
        {

            view.sta_isEmpty.ChangeState(model.lab == null ? 0 : 1);
            if (model.lab != null)
            {
                view.txt_.text = model.lab;
            }
        }
    }


    public partial class UiItemParam
    {
        public MapBaseForm.Data data;
    }
    public partial class UiItemModel
    {
        public MapBaseForm.Data data;
    }
    public partial class UiItemCtrl
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

            Refresh();
        }
        public void Refresh()
        {
            view.sta_item.ChangeState(model.data == null ? 0 : 1);
            if (model.data != null)
            {
                view.txt_.text = model.data.name;
                view.img_.sprite = StoryTexAssetForm.DataByName[model.data.icon].sprite;
            }
        }
    }


}