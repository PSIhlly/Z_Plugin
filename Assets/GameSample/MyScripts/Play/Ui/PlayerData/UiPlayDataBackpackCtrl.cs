using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z_Ui.Base;
using Z_Texture;
using Z_DataSystem.Form;
using Z_Text;
using Z_Ui.Notify;

namespace Ui.PlayData.PlayDataBackpack
{

    public partial class UiPlayDataBackpackParam
    {

    }
    public partial class UiPlayDataBackpackModel
    {
        public ItemProductForm.Data sel;
        public string lab;
    }
    public partial class UiPlayDataBackpackCtrl
    {

        UiScrViewContainer<UiGameItemCtrl> itemCon;
        UiScrViewContainer<UiLabCtrl> labCon;
        UiScrViewContainer<UiGameArgsCtrl> gameArgCon;
        public override void OnCreate()
        {

            view.btn_.onClick.AddListener(() =>
            {

            });
            view.btn_drop.onClick.AddListener(() =>
            {
                if (model.sel.amount > 1)
                {
                    NotifyManager.instance.AddInputArea(TextManager.instance.GetTxt("input drop amount"), true, (res) =>
                    {
                        if (int.TryParse(res, out var amount))
                        {
                            PlayManager.instance.infoCtrl.LostItem(model.sel.uid, amount);
                        }
                        return true;
                    }, "1");
                }
                else
                {
                    PlayManager.instance.infoCtrl.LostItem(model.sel.uid, 1);
                }

            });
            view.btn_use.onClick.AddListener(() =>
            {
                PlayManager.instance.infoCtrl.UseItem(model.sel.uid,1);
            });
            view.btn_equip.onClick.AddListener(() =>
            {
                var equipCharacter = PlayManager.instance.infoCtrl.GetTeamEquipedCharacter(model.sel.uid, out var part);
                if(GameManager.instance.curProgress.team.Count<=1)
                {
                    PlayManager.instance.infoCtrl.Equip(GameManager.instance.curProgress.team[0], model.sel.uid, ItemProductForm.DataByUid[model.sel.uid].equip);
                    Refresh();
                }
                else
                {
                    PlayManager.instance.infoCtrl.ChooseTeamCharacter(TextManager.instance.GetTxt("Equip character"), (ch) =>
                    {
                        PlayManager.instance.infoCtrl.Equip(ch.uid, model.sel.uid, ItemProductForm.DataByUid[model.sel.uid].equip);
                    });
                    Refresh();
                }
            });
            view.btn_unequip.onClick.AddListener(() =>
            {
                var equipCharacter = PlayManager.instance.infoCtrl.GetTeamEquipedCharacter(model.sel.uid, out var part);
                if(equipCharacter!=null)
                {
                    PlayManager.instance.infoCtrl.Unequip(equipCharacter.uid, ItemProductForm.DataByUid[model.sel.uid].equip);
                }
                Refresh();
            });
            itemCon = new UiScrViewContainer<UiGameItemCtrl>(view.go_gameItem, view.scr_gameItems);
            labCon = new UiScrViewContainer<UiLabCtrl>(view.go_lab, view.scr_labs);
            gameArgCon = new UiScrViewContainer<UiGameArgsCtrl>(view.go_gameArgs, view.scr_gameArgs);
        }
        public override void OnShow()
        {
            model.sel = null;
            Refresh();
        }
        public void Refresh()
        {
            labCon.Clear();
            labCon.Add(new UiLabParam()
            {
                lab = null
            });
            foreach (var lab in ItemProductForm.DatasByLabel.Keys)
            {
                if (lab != "")
                {
                    labCon.Add(new UiLabParam()
                    {
                        lab = lab
                    });
                }
            }
            labCon.Refresh();

            itemCon.Clear();
            for (int i = 0, icnt = GameManager.instance.curProgress.bag.Count; i < icnt; i++)
            {
                var data = ItemProductForm.DataByUid[GameManager.instance.curProgress.bag[i]];
                if (model.lab == null || data.label == model.lab)
                {
                    itemCon.Add(new UiGameItemParam()
                    {
                        data = data
                    });
                }

            }
            itemCon.Refresh();

            gameArgCon.Clear();
            view.sta_show.ChangeState(model.sel != null ? 1 : 0);
            if (model.sel != null)
            {
                view.img_.sprite = TexAssetForm.DataByName[model.sel.iconTexName].GetSprite();
                view.txt_desc.text = model.sel.desc;
                view.txt_name.text = model.sel.name;
                view.txt_amount.text = TextManager.instance.GetTxt("count") + ":" + model.sel.amount.ToString();
                foreach (var arg in model.sel.paramDic)
                {
                    if (model.sel.CanShow(arg.Key))
                        gameArgCon.Add(new UiGameArgsParam()
                        {
                            content = arg.Key + ":" + arg.Value.v
                        });
                }
                var equipCharacter = PlayManager.instance.infoCtrl.GetTeamEquipedCharacter(model.sel.uid, out var part);
                view.sta_showArea.ChangeState(model.sel.canEquipe ? (equipCharacter != null ? 2 : 1) : 0);
            }
            gameArgCon.Refresh();
        }
        public void Sel(ItemProductForm.Data data)
        {
            model.sel = data;
            Refresh();
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
            view.sta_.ChangeState(parent.model.lab == model.lab ? 1 : 0);
            if (model.lab != null)
            {
                view.txt_.text = model.lab;
            }
        }
    }
    public partial class UiGameItemParam
    {

        public ItemProductForm.Data data;
    }
    public partial class UiGameItemModel
    {

        public ItemProductForm.Data data;
    }
    public partial class UiGameItemCtrl
    {

        public override void OnCreate()
        {

            view.btn_.onClick.AddListener(() =>
            {
                parent.Sel(model.data);
            });

        }
        public override void OnShow()
        {
            if (param != null)
            {
                model.data = param.data;
            }
            Refresh();
        }
        public void Refresh()
        {
            view.sta_exist.ChangeState(1);

            view.txt_.text = model.data.name;
            view.txt_count.text = model.data.amount.ToString();
            view.img_.sprite = TexAssetForm.DataByName[model.data.iconTexName].GetSprite();

        }
    }
    public partial class UiGameArgsParam
    {
        public string content;
    }
    public partial class UiGameArgsModel
    {
        public string content;

    }
    public partial class UiGameArgsCtrl
    {

        public override void OnCreate()
        {



        }
        public override void OnShow()
        {
            model.content = param.content;
            Refresh();
        }
        public void Refresh()
        {

            view.txt_.text = model.content;
        }
    }

}