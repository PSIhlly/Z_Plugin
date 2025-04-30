using Form;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Policy;
using UnityEngine;
using UnityEngine.UI;
using Z_Text;
using Z_Texture;
using Z_Time;
using Z_Ui.Base;
using Z_Ui.Notify;

namespace Ui.ModStoryEventCmds
{
    public partial class UiModStoryEventCmdsParam
    {
        public Action onClose;
        public EventForm.Data data;
    }
    public partial class UiModStoryEventCmdsModel
    {
        public Action onClose;
        public EventForm.Data data;

        public List<List<CmdForm.Data>> dataLst;
        public List<(string, List<string>)> wordsLst;
        public List<(Sprite, List<Sprite>)> spriteLst;
    }

    public partial class UiModStoryEventCmdsCtrl
    {

        UiScrViewContainer<UiCmdCtrl> cmdCon;
        public override void OnCreate()
        {
            model.wordsLst = new List<(string, List<string>)>();
            model.spriteLst = new List<(Sprite, List<Sprite>)>();
            model.dataLst = new List<List<CmdForm.Data>>();
            foreach (var pair in CmdForm.DatasByLab)
            {
                var subWordsLst = new List<string>();
                var subSpriteLst = new List<Sprite>();
                var subDataLst = new List<CmdForm.Data>();
                foreach (var sub in pair.Value)
                {
                    subWordsLst.Add(sub.name);
                    subSpriteLst.Add(TextureHelper.transparentSprite);
                    subDataLst.Add(sub);
                }
                model.wordsLst.Add((pair.Key, subWordsLst));
                model.spriteLst.Add((TextureHelper.transparentSprite, subSpriteLst));
                model.dataLst.Add(subDataLst);
            }

            cmdCon = new UiScrViewContainer<UiCmdCtrl>(view.go_cmd, view.scr_cmds);
            view.btn_close.onClick.AddListener(() =>
            {
                Close();
            });
            view.btn_save.onClick.AddListener(() =>
            {
                Close();
            });
            view.ipt_name.onFinishInput += (s) =>
            {
                ModManager.instance.assetCtrl.RenameEvent(model.data.name,  s);
                Refresh();
            };
            view.ipt_label.onFinishInput+=(s) =>
            {
                ModManager.instance.assetCtrl.RenameEvent(model.data.name, null, s);
                Refresh();
            };
            view.ipt_subLabel.onFinishInput += (s) =>
            {
                ModManager.instance.assetCtrl.RenameEvent(model.data.name, null, null,s);
                Refresh();
            };
        }
        public override void Close()
        {
            GameManager.instance.saveCtrl.SaveEvent(ModManager.instance.GetStoryCoreFolder());
            model.onClose?.Invoke();
            base.Close();
        }
        public override void OnShow()
        {
            if (param != null)
            {
                model.onClose = param.onClose;
                model.data = param.data;
            }


            Refresh();
            LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
        }
        public void Refresh()
        {
            var showCmds = GameManager.instance.evtCtrl.GetShowCmds(model.data.cmds);

            cmdCon.Clear();
            foreach (var showCmd in showCmds)
            {
                cmdCon.Add(new UiCmdParam()
                {
                    showCmd = showCmd
                });
            }
            cmdCon.Refresh();


            view.ipt_name.Set(model.data.name);
            view.ipt_label.Set(model.data.lab);
            view.ipt_subLabel.Set(model.data.subLab);
        }
    }
    public partial class UiCmdParam
    {
        public ShowCmd showCmd;
    }
    public partial class UiCmdModel
    {
        public ShowCmd showCmd;

    }
    public partial class UiCmdCtrl
    {
        public override void OnCreate()
        {
            view.btn_add.onClick.AddListener(() =>
            {
                NotifyManager.instance.AddMultipleChoose("", true, (id) =>
                  {
                      var top = model.showCmd;
                      while (top.prms != null && top.prms.Count > 0)
                          top = top.prms[top.prms.Count - 1];


                      GameEventController.InsertCmd(parent.model.data.cmds, top.oriId, parent.model.dataLst[id.Item1][id.Item2]);


                      parent.Refresh();
                      return true;
                  }, parent.model.wordsLst, parent.model.spriteLst);
            });
            view.btn_name.onClick.AddListener(() =>
            {
                model.showCmd.data.constV = "??";
            });

        }
        public override void OnShow()
        {
            if (param != null)
            {
                model.showCmd = param.showCmd;
            }
            Refresh();
        }
        public void Refresh()
        {
            var belong = model.showCmd.belong != null ? TextManager.instance.GetTxt(model.showCmd.belong.data.prmName[model.showCmd.prmId]) + " : " : "";
            var value = string.IsNullOrEmpty(model.showCmd.data.constV) ? TextManager.instance.GetTxt(model.showCmd.data.name) : model.showCmd.data.constV;

            view.txt_name.text = belong + value;

            view.btn_add.gameObject.SetActive(model.showCmd.data.uid==0||!string.IsNullOrEmpty(model.showCmd.data.lab));
            view.btn_del.gameObject.SetActive(!string.IsNullOrEmpty(model.showCmd.data.lab));
        }
    }


}
