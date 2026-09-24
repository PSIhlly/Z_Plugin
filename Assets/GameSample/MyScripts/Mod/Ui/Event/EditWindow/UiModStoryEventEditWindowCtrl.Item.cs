using Form;
using System;
using System.Collections.Generic;
using Z_Code;
using Z_Code.Form;
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
        private UiContainer<UiUnitCtrl> unitCon;
        private UiContainer<UiDeepthCtrl> deepthCon;
        private readonly List<UiUnitCtrl> renderedUnits = new List<UiUnitCtrl>();

        public override void OnCreate()
        {
            if (param == null)
            {
                gameObject.SetActive(false);
                return;
            }
            view.txt_new.raycastTarget = false;
            deepthCon = new UiContainer<UiDeepthCtrl>(this, view.go_deepth);
            unitCon = new UiContainer<UiUnitCtrl>(this, view.go_unit, false);
            model.prm = param;
            Refresh();
        }

        public void OnLineClick()
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
                    }
                    else if (ProgramDataForm.DataByName.ContainsKey(item.content))
                    {
                        var data = ProgramDataForm.DataByName[item.content];
                        var paramCount = data.paramCount;
                        var rawCode = item.content + "(";
                        for (int i = 0; i < paramCount; i++)
                        {
                            rawCode += $"{(i == 0 ? "" : ",")}param{i + 1}";
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
                parent.SelUnit(model.prm.node, model.prm.node);
            }
        }
        public override void OnShow()
        {

        }


        public void Refresh()
        {
            unitCon.Clear();
            renderedUnits.Clear();
            deepthCon.Clear();
            for (int i = 0; i < model.prm.deepth; i++)
                deepthCon.Add(new UiDeepthParam());
            deepthCon.Refresh();
            for (int i = 0; i < deepthCon.paramLst.Count; i++)
                deepthCon.Get(deepthCon.paramLst[i]).transform.SetSiblingIndex(view.go_deepth.transform.GetSiblingIndex() + i + 1);
            view.sta_isEmpty.ChangeState(model.prm.node == null ? 0 : 1);
            if (model.prm.node == null)
            {
                view.txt_new.text = TextManager.instance.GetTxt("new");
            }
            else
            {
                int curRender = model.prm.con.GetNowRenderId();
                foreach (var sub in model.prm.node.subNodes)
                {
                    if (sub.desc.type == CodeType.Action && !SyntaxAnalysis.IsEmptyNode(sub))
                    {
                        if (model.prm.node.desc.type == CodeType.Reserved && model.prm.node.desc.code == "for")
                        {
                            foreach (var statement in sub.subNodes)
                            {
                                model.prm.con.Add(new UiItemParam()
                                {
                                    con = model.prm.con,
                                    node = statement,
                                    deepth = model.prm.deepth + 1,
                                    targetNewList = sub.subNodes,
                                }, ++curRender);
                            }
                            model.prm.con.Add(new UiItemParam()
                            {
                                con = model.prm.con,
                                node = null,
                                deepth = model.prm.deepth + 1,
                                targetNewList = sub.subNodes,
                            }, ++curRender);
                        }
                        else
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

                unitCon.Add(new UiUnitParam()
                {
                    con = unitCon,
                    parent = view.rtf_unitRoot,
                    node = model.prm.node,
                });
            }
            unitCon.Refresh();
        }

        public void RegisterRenderedUnit(UiUnitCtrl unit)
        {
            renderedUnits.Add(unit);
        }

        public void RefreshUnitSelection()
        {
            foreach (var unit in renderedUnits)
            {
                if (unit.active)
                    unit.RefreshSelection();
            }
        }


    }

    public partial class UiLineParam
    {
        public UiItemCtrl item;
        public SyntaxNode node;
        public int index;
    }

    public partial class UiLineCtrl
    {
        public override void OnCreate()
        {
            if (param == null)
            {
                gameObject.SetActive(false);
                return;
            }
            view.btn_.onClick.AddListener(() => param.item.OnLineClick());
            RefreshSelection();
        }

        public void RefreshSelection()
        {
            view.sta_.ChangeState(param.node != null && parent.model.selItem == param.node
                ? 2
                : param.index % 2);
        }
    }
}
