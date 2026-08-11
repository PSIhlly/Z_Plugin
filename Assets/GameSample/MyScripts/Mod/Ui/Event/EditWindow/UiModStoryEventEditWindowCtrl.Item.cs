using Form;
using System;
using System.Collections.Generic;
using Z_Code;
using Z_Code.Form;
using Z_String;
using Z_Text;
using Z_Ui.Base;
using Z_Ui.Notify;
namespace Ui.ModStoryEventEditWindow
{
    public partial class UiItemParam
    {
        public UiContainer<UiItemCtrl> con;
        public SyntaxNode node;
        public int deepth;
        public Action<SyntaxNode> onClick;
        public List<SyntaxNode> targetNewList;
    }
    public partial class UiItemModel
    {
        public UiItemParam prm;
    }
    public partial class UiItemCtrl
    {


        public override void OnCreate()
        {
            view.btn_.onClick.AddListener(() =>
            {
                if (model.prm.node == null)
                {
                    ModManager.instance.assetCtrl.ChooseCmd(SceneEventType.All, "void", (item) =>
                    {
                        if (GameCmdDataForm.DataByUid.ContainsKey(item.id))
                        {
                            var data = GameCmdDataForm.DataByUid[item.id];
                            GameCmdDataForm.Data sel = GameCmdDataForm.DataByName[data.name];
                            string defaultCode = sel.defaultCode;
                            if (!string.IsNullOrEmpty(sel.allowAsVoid))
                                defaultCode = $"{sel.allowAsVoid}{defaultCode};";
                            if (!parent.TryCompileSyntax(defaultCode, out var res))
                                return;
                            model.prm.targetNewList.AddRange(res);
                            parent.ApplyEntry();
                        }else if(ProgramDataForm.DataByName.ContainsKey(item.content))
                        {
                            var data = ProgramDataForm.DataByName[item.content];
                            var paramCount = data.paramCount;
                            var rawCode = item.content + "(";
                            for (int i = 0; i < paramCount; i++)
                            {
                                rawCode += $"{(i==0?"":",")}param{i+1}";
                            }
                            rawCode += ");";
                            if (!parent.TryCompileSyntax(rawCode, out var res))
                                return;
                            model.prm.targetNewList.AddRange(res);
                            parent.ApplyEntry();
                        }
                        
                    });
                }
                else
                {
                    parent.SelItem(model.prm.node);
                }
            });

            model.prm = param;
            Refresh();
        }
        public override void OnShow()
        {

        }


        public void Refresh()
        {
            view.sta_isEmpty.ChangeState(model.prm.node == null ? 0 : 1);
            if (model.prm.node == null)
            {
                view.txt_new.text = " ".Repeat(model.prm.deepth) + TextManager.instance.GetTxt("new");
            }
            else
            {
                int curRender = model.prm.con.GetNowRenderId();
                foreach (var sub in model.prm.node.subNodes)
                {
                    if (sub.desc.type == CodeType.Action)
                    {
                        model.prm.con.Add(new UiItemParam()
                        {
                            con = model.prm.con,
                            node = sub,
                            deepth = model.prm.deepth + 1,
                            targetNewList = model.prm.targetNewList,
                        }, ++curRender);
                    }

                }

                if (model.prm.node.desc.type == CodeType.Action)
                {
                    foreach (var sub in model.prm.node.subNodes)
                    {
                        model.prm.con.Add(new UiItemParam()
                        {
                            con = model.prm.con,
                            node = sub,
                            deepth = model.prm.deepth + 1,
                            targetNewList = model.prm.node.subNodes,
                        }, ++curRender);
                    }
                    model.prm.con.Add(new UiItemParam()
                    {
                        con = model.prm.con,
                        node = null,
                        deepth = model.prm.deepth + 1,
                        targetNewList = model.prm.node.subNodes,
                    }, ++curRender);
                }

                view.txt_.oriText = " ".Repeat(model.prm.deepth) + GetNodeDesc(model.prm.node);
            }



        }


        public string GetNodeDesc(SyntaxNode node)
        {

            string res = "";
            switch (node.desc.type)
            {
                case CodeType.FuncName:
                case CodeType.Reserved:
                case CodeType.Operator:
                    if (CmdDataForm.DataByName.ContainsKey(node.desc.code))
                    {
                        var form = CmdDataForm.DataByName[node.desc.code];
                        res = form.desc;
                        if (form.prmNames != null)
                        {
                            int i = 0;
                            for (; i < node.subNodes.Count; i++)
                            {
                                res = res.Replace($"{{{i}}}", GetNodeDesc(node.subNodes[i]));
                            }
                            for (; i < form.prmNames.Count; i++)
                            {
                                res = res.Replace($"{{{i}}}", "");
                            }
                        }

                    }
                    else if (int.TryParse(node.desc.code, out int evtId) && EventProgramDataForm.DataByUid.ContainsKey(evtId))
                    {
                        var evtData = EventProgramDataForm.DataByUid[evtId];
                        res = evtData.name + "(";

                        for (int i = 0; i < node.subNodes.Count; i++)
                        {
                            res += $"{(i > 0 ? "," : "")}param{i + 1}={GetNodeDesc(node.subNodes[i])}";
                        }
                        res += ")";
                    }
                    else
                        res = node.desc.code;
                    break;
                default:
                    res = node.desc.code;
                    break;
            }
            return res;
        }
    }
}
