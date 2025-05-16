using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Z_Text;
using Z_Ui.Base;

namespace Ui.ModStoryItem.ModStoryItemArguments
{
    public partial class UiModStoryItemArgumentsCtrl
    {
        UiScrViewContainer<UiArgCtrl> con;

        public override void OnCreate()
        {
            con = new UiScrViewContainer<UiArgCtrl>(view.go_arg, view.scr_items);

        }

        public override void OnShow()
        {
            Refresh();
        }
        public override void OnDisable()
        {
        }
        public void Refresh()
        {
            con.Clear();
            foreach (var data in ItemParamForm.DataByUid.Values)
            {
                if (data.uid > MapBaseForm.autoIdCnt)
                    continue;
                con.Add(new UiArgParam
                {
                    id = data.uid
                });
            }
            con.Add(new UiArgParam
            {
                id = -1
            });
            con.Refresh();

        }


    }





    public partial class UiArgModel
    {
        public int id;
    }
    public partial class UiArgParam
    {
        public int id;
    }
    public partial class UiArgCtrl
    {
        public override void OnCreate()
        {
            view.btn_new.onClick.AddListener(() =>
            {
                
                    ModManager.instance.assetCtrl.CreateItemArg("");
                    parent.Refresh();
            });

            view.btn_delete.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.DeleteItemArg(ItemParamForm.DataByUid[model.id].name);
                parent.Refresh();
            });
            view.ipt_name.onFinishInput += (s) =>
            {
                ModManager.instance.assetCtrl.RenameItemParam(ItemParamForm.DataByUid[model.id].name, s);
                parent.Refresh();
            };

        }
        public override void OnShow()
        {
            model.id = param.id;
            Refresh();
        }
        public void Refresh()
        {
            view.sta_arg.ChangeState(model.id ==-1?0:1);
            if (model.id != -1)
            {
                view.ipt_name.text = ItemParamForm.DataByUid[model.id].name;
            }
            else
            {
                view.txt_new.text = TextManager.instance.GetTxt("new");
            }
        }


    }
}
