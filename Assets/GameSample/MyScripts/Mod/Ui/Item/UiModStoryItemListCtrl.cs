using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ui.ModStoryItemListArguments;
using Ui.ModStoryItemListModel;
using UnityEngine;
using Z_DataSystem;
using Z_DataSystem.Form;
using Z_Text;
using Z_Ui;
using Z_Ui.Base;

namespace Ui.ModStoryItem.ModStoryItemList
{
    public partial class UiModStoryItemListModel
    {
        public ItemProductForm.Data curData;
    }
    public partial class UiModStoryItemListCtrl
    {
        UiScrViewContainer<UiItemCtrl> con;
        public override void OnCreate()
        {
            con = new UiScrViewContainer<UiItemCtrl>(view.go_item, view.scr_items);


            view.btn_delete.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.DeleteItem(model.curData.name);
                SetCur(null);

                Refresh();
            });
            view.ipt_name.onFinishInput += (v) =>
            {
                ModManager.instance.assetCtrl.RenameItem(model.curData.name, v);
                Refresh();
            };
            view.btn_avatar.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.ImportItemIcon(model.curData.name);
                Refresh();
            });
            view.btn_args.onClick.AddListener(() =>
            {
                UiManager.instance.ShowUi<UiModStoryItemListArgumentsCtrl>(new UiModStoryItemListArgumentsParam()
                {
                    data = model.curData
                });
            });
            view.btn_model.onClick.AddListener(() =>
            {
                UiManager.instance.ShowUi<UiModStoryItemListModelCtrl>(new UiModStoryItemListModelParam()
                {
                    data = model.curData
                });
            });

        }

        public override void OnShow()
        {
            DisplayCameraAreaManager.instance.Show();
            SetCur(null);
            Refresh();
        }
        public void Refresh()
        {
            con.Clear();

            if(ItemProductForm.DatasByIsproto.ContainsKey(true))
            {
                foreach (var data in ItemProductForm.DatasByIsproto[true])
                {
                    if (data.uid > ProductForm.autoUidCnt)
                        continue;
                    con.Add(new UiItemParam
                    {
                        id = data.uid
                    });
                }
            }
            
            con.Add(new UiItemParam
            {
                id = -1
            });
            con.Refresh();

            view.sta_exist.ChangeState(model.curData == null ? 0 : 1);

            if (model.curData != null)
            {
                view.ipt_name.Set(model.curData.name);
                view.img_icon.sprite = TexAssetForm.DataByName[model.curData.iconTexName].sprite;
            }

        }

        public void SetCur(ItemProductForm.Data data)
        {
            model.curData = data;
        }

    }


    public partial class UiItemModel
    {
        public int id;
    }
    public partial class UiItemParam
    {
        public int id;
    }
    public partial class UiItemCtrl
    {
        public override void OnCreate()
        {
            view.btn_item.onClick.AddListener(() =>
            {
                if (model.id == -1)
                {
                    int max = 1;
                    if (ItemProductForm.DatasByIsproto.ContainsKey(true))
                    {
                        foreach (var o in ItemProductForm.DatasByIsproto[true])
                        {
                            var splt = o.name.Split("newItem");
                            if (splt.Length > 1)
                            {
                                if (int.TryParse(splt[1], out int v))
                                {
                                    max = Mathf.Max(max, v + 1);
                                }
                            }
                        }
                    }
                    ModManager.instance.assetCtrl.CreateItem("newItem" + max);
                    parent.Refresh();
                }
                else
                {
                    parent.SetCur(ItemProductForm.DataByUid[model.id]);
                    parent.Refresh();
                }
            });
        }
        public override void OnShow()
        {
            model.id = param.id;
            Refresh();
        }
        public void Refresh()
        {
            if (model.id != -1)
            {
                view.img_.sprite = TexAssetForm.DataByName[ItemProductForm.DataByUid[model.id].iconTexName].sprite;
                view.txt_name.text = ItemProductForm.DataByUid[model.id].name;
            }
            else
            {
                view.txt_name.text = TextManager.instance.GetTxt("new");
                //view.img_.sprite = AssetManager.instance.GetSprite(ModAssetManager.instance.GetTexRealName(MapTextureForm.DataById[model.id].name, 0));
            }
            view.sta_item.ChangeState(parent.model.curData?.uid == model.id ? 1 : 0);
        }


    }


}
