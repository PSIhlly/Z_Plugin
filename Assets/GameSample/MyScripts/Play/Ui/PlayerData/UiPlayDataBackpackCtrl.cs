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

namespace Ui.PlayDataBackpack
{

    public partial class UiPlayDataBackpackParam
    {

    }
    public partial class UiPlayDataBackpackModel
    {
        public ItemProductForm.Data sel;
        public int? labId;
    }
    public partial class UiPlayDataBackpackCtrl
    {

        UiScrViewContainer<UiGameItemCtrl> itemCon;
        UiScrViewContainer<UiLabCtrl> labCon;
        UiScrViewContainer<UiGameArgsCtrl> gameArgCon;
        public override void OnCreate()
        {
            view.btn_back.onClick.AddListener(() =>
            {
                Close();
            });

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
            itemCon = new UiScrViewContainer<UiGameItemCtrl>(this, view.go_gameItem, view.scr_gameItems);
            labCon = new UiScrViewContainer<UiLabCtrl>(this, view.go_lab, view.scr_labs);
            gameArgCon = new UiScrViewContainer<UiGameArgsCtrl>(this, view.go_gameArgs, view.scr_gameArgs);
        }
        public override void OnShow()
        {
            model.labId = HasUnclassified() ? LabForm.NoneId : (int?)null;
            model.sel = null;
            Refresh();
        }
        bool HasUnclassified()
        {
            return GameManager.instance.curProgress.bag.Any(uid =>
                ItemProductForm.DataByUid.ContainsKey(uid) &&
                ItemProductForm.DataByUid[uid].labId == LabForm.NoneId);
        }
        public void Refresh()
        {
            var hasUnclassified = HasUnclassified();
            if (model.labId == LabForm.NoneId && !hasUnclassified)
                model.labId = null;
            labCon.Clear();
            labCon.Add(new UiLabParam()
            {
                state = UiLabRenderHelper.AllState
            });
            if (hasUnclassified)
            {
                labCon.Add(new UiLabParam()
                {
                    labId = LabForm.NoneId,
                    state = UiLabRenderHelper.UnclassifiedState
                });
            }
            foreach (var labId in UiLabRenderHelper.GetLabIds(nameof(ItemProductForm)))
            {
                labCon.Add(new UiLabParam()
                {
                    labId = labId,
                    state = UiLabRenderHelper.LabState
                });
            }
            labCon.Add(new UiLabParam()
            {
                state = UiLabRenderHelper.NewState
            });
            labCon.Refresh();

            itemCon.Clear();
            foreach (var uid in GameManager.instance.curProgress.bag.OrderBy(u => u))
            {
                var data = ItemProductForm.DataByUid[uid];
                if (model.labId == null || data.labId == model.labId.Value)
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
                view.img_.BindTexData(TexAssetForm.DataById[model.sel.iconTexName]);
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
                view.sta_showArea.ChangeState(model.sel.canEquipe&&GameManager.instance.curProgress.enableEquip ? (equipCharacter != null ? 2 : 1) : 0);
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
        public int? labId;
        public int state;
    }
    public partial class UiLabModel
    {
        public int? labId;
        public int state;
    }
    public partial class UiLabCtrl
    {

        public override void OnCreate()
        {

            view.btn_.onClick.AddListener(() =>
            {
                if (model.state == UiLabRenderHelper.NewState)
                {
                    UiLabRenderHelper.Create(nameof(ItemProductForm), labId =>
                    {
                        parent.model.labId = labId;
                        parent.Refresh();
                    });
                    return;
                }

                parent.model.labId = model.labId;
                parent.Refresh();
            });

        }
        public override void OnShow()
        {
            model.labId = param.labId;
            model.state = param.state;
            Refresh();
        }
        public void Refresh()
        {
            var isNew = model.state == UiLabRenderHelper.NewState;
            view.sta_state.ChangeState(model.state);
            view.sta_.ChangeState(!isNew && parent.model.labId == model.labId ? 1 : 0);
            view.txt_.text = UiLabRenderHelper.GetText(model.labId, isNew);
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
            view.img_.BindTexData(TexAssetForm.DataById[model.data.iconTexName]);

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
