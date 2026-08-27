using Form;
using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditor.PackageManager;
using UnityEditor.Profiling.Memory.Experimental;
using UnityEngine;
using UnityEngine.UI;
using Z_Code;
using Z_Code.Form;
using Z_CodeVisual;
using Z_DataSystem.Form;
using Z_Time;
using Z_Ui;
using Z_Ui.Base;
using Z_Ui.Notify;
using static UnityEditor.Progress;
using static UnityEngine.InputManagerEntry;
namespace Ui.ModStoryEventEditWindow
{
    public partial class UiModStoryEventEditWindowParam
    {
        public EventProgramDataForm.Data data;
        public Action onClose;
    }
    public partial class UiModStoryEventEditWindowModel
    {
        public Compiler cpr;
        public Decompiler dcpr;
        public List<SyntaxNode> curEntry;
        public EventProgramDataForm.Data data;

        public bool codeEditMode;
        public SyntaxNode selItem;
        public SyntaxNode selUnit;
        public Action onClose;

    }
    public partial class UiModStoryEventEditWindowCtrl
    {
        UiContainer<UiUnitCtrl> unitCon;
        UiContainer<UiItemCtrl> itemCon;

        public override void OnCreate()
        {

            unitCon = new UiContainer<UiUnitCtrl>(this, view.go_unit, false);
            itemCon = new UiContainer<UiItemCtrl>(this, view.go_item, false);

            model.cpr = new Compiler();
            model.dcpr = new Decompiler();
            model.curEntry = new List<SyntaxNode>();

            view.ipt_name.onFinishInput += (v) =>
            {
                model.data.name = v;
                Refresh();
            };
            view.ipt_category.onFinishInput += (v) =>
            {
                SetLab(v, null);
                Refresh();
            };
            view.ipt_type.onFinishInput += (v) =>
            {
                SetLab(null, v);
                Refresh();
            };
            view.btn_switchMod.onClick.AddListener(() =>
            {
                view.btn_apply.onClick.Invoke();//auto apply
                model.codeEditMode = !model.codeEditMode;
                model.selItem = null;
                model.selUnit = null;

                Refresh();
            });
            view.btn_apply.onClick.AddListener(() =>
            {
                if (model.codeEditMode)
                {
                    ApplyCode();

                }
                else
                {
                    ApplyEntry();

                }

            });
            view.btn_close.onClick.AddListener(() =>
            {
                Close();
            });
            view.btn_edit.onClick.AddListener(() =>
            {
                var tp = GlobalEventHelper.GetGameRetType(model.selUnit.desc);
                if (model.selUnit.parentNode != null && model.selUnit.parentNode.desc.type == CodeType.FuncName)
                {
                    if (CmdDataForm.DataByName.ContainsKey(model.selUnit.parentNode.desc.code))
                    {
                        var data = CmdDataForm.DataByName[model.selUnit.parentNode.desc.code];
                        var prmId = model.selUnit.parentNode.subNodes.IndexOf(model.selUnit);
                        tp = data.prmTypes[prmId];
                    }
                    else
                    {
                        tp = "var";
                    }

                }
                ModManager.instance.assetCtrl.ChooseCmd(SceneEventType.All, tp, (item) =>
                {
                    if (GameCmdDataForm.DataByUid.ContainsKey(item.id))
                    {
                        var data = GameCmdDataForm.DataByUid[item.id];
                        BaseData.cmdDic[data.name].GetUnitChooseCode((code) =>
                        {
                            if (tp == "void" && !string.IsNullOrEmpty(data.allowAsVoid))
                                code = $"{data.allowAsVoid}{code};";
                            if (!TryCompileUnitSyntax(code, out var unit))
                                return;
                            ReplaceNode(model.selUnit, unit);
                            ApplyEntry();
                        }, model.selUnit);
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
                        if (!TryCompileSyntax(rawCode, out var res))
                            return;
                        ReplaceNode(model.selUnit, res[0]);
                    }

                });

            });
            view.btn_del.onClick.AddListener(() =>
            {

                var parentList = FindParentList(model.curEntry, model.selItem);
                if (parentList != null)
                {
                    parentList.Remove(model.selItem);
                }

                model.selItem = null;
                ApplyEntry();
            });
            view.btn_insert.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.ChooseCmd(SceneEventType.All, "void", (item) =>
                {
                    if (item != null)
                    {
                        string defaultCode = "";
                        if (GameCmdDataForm.DataByUid.ContainsKey(item.id))
                        {
                            var data = GameCmdDataForm.DataByUid[item.id];
                            GameCmdDataForm.Data sel = GameCmdDataForm.DataByName[data.name];
                            defaultCode = sel.defaultCode;
                            if (!string.IsNullOrEmpty(sel.allowAsVoid))
                                defaultCode = $"{sel.allowAsVoid}{defaultCode};";
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
                            defaultCode = rawCode;
                        }


                        if (!TryCompileSyntax(defaultCode, out var res))
                            return;
                        var parentLst=FindParentList(model.curEntry, model.selItem);
                        int id = parentLst.IndexOf(model.selItem);
                        foreach (var r in res)
                        {
                            parentLst.Insert(id, r);
                            model.selItem = r;
                            id++;
                        }
                        ApplyEntry();
                    }

                });
            });
        }
        public void ApplyEntry()
        {
            var code = model.dcpr.Decompile(model.curEntry);
            if (model.data.TryApplyCode(code, out _, out var errors, model.cpr))
            {
                view.ipt_code.Set(model.data.code);
                Refresh();
            }
            else
            {
                ShowCompileErrors(errors);
            }
        }
        public void ApplyCode()
        {
            if (model.data.TryApplyCode(view.ipt_code.text, out var syntaxNodes,
                    out var errors, model.cpr))
            {
                model.curEntry = syntaxNodes;
                Refresh();
            }
            else
            {
                ShowCompileErrors(errors);
            }
        }

        internal bool TryCompileSyntax(string code, out List<SyntaxNode> syntaxNodes)
        {
            if (model.cpr.TryCompile(code, out _, out syntaxNodes, out _, out _, out _, out var errors))
                return true;

            syntaxNodes = new List<SyntaxNode>();
            ShowCompileErrors(errors);
            return false;
        }

        private bool TryCompileUnitSyntax(string code, out SyntaxNode syntaxNode)
        {
            var statement = (code ?? string.Empty).TrimEnd();
            if (!statement.EndsWith(";", StringComparison.Ordinal))
                statement += ";";

            if (!TryCompileSyntax(statement, out var syntaxNodes))
            {
                syntaxNode = null;
                return false;
            }

            if (syntaxNodes.Count != 1)
            {
                syntaxNode = null;
                NotifyManager.instance.AddTip("Cmd 参数必须是单个表达式");
                return false;
            }

            syntaxNode = syntaxNodes[0];
            return true;
        }

        private static void ShowCompileErrors(List<CompileError> errors)
        {
            if (errors == null || errors.Count == 0)
            {
                NotifyManager.instance.AddTip("编译失败");
                return;
            }

            var message = errors[0].ToString();
            if (errors.Count > 1)
                message += $"\n……另有 {errors.Count - 1} 个错误";
            NotifyManager.instance.AddTip(message);
        }
        public override void OnShow()
        {
            model.codeEditMode = false;
            model.data = param.data;
            model.onClose = param.onClose;

            if (!TryCompileSyntax(model.data.code, out var initialEntries))
                initialEntries = new List<SyntaxNode>();
            model.curEntry = initialEntries;
            view.ipt_code.Set(model.data.code);

            model.selItem = null;
            model.selUnit = null;
            Refresh();
        }
        public override void OnDisable()
        {
            model.onClose?.Invoke();
        }
        public void Refresh()
        {
            int cnt = -1;

            view.sta_switchMod.ChangeState(model.codeEditMode ? 1 : 0);
            view.sta_switchModPanel.ChangeState(model.codeEditMode ? 1 : 0);

            itemCon.Clear();
            foreach (var node in model.curEntry)
            {
                itemCon.Add(new UiItemParam()
                {
                    con = itemCon,
                    node = node,
                    deepth = 0,
                    targetNewList = model.curEntry,
                });
            }
            itemCon.Add(new UiItemParam()
            {
                con = itemCon,
                node = null,
                deepth = 0,
                targetNewList = model.curEntry,
            });
            itemCon.Refresh();

            //
            view.txt_title.text = model.data.name;

            view.ipt_name.Set(model.data.name);
            LabForm.TryGetData(model.data.labId, out var lab);
            view.ipt_category.Set(lab?.lv1Lab ?? string.Empty);
            view.ipt_type.Set(lab?.lv2Lab ?? string.Empty);


            RefreshUnitDetail();
            RefreshUnit();
            TimeManager.instance.AddCurLateUpdateAction(() =>
            {
                UiManager.Rebuild(view.rtf_unitRoot.gameObject, true);
            },gameObject);
        }

        private void SetLab(string category, string type)
        {
            LabForm.TryGetData(model.data.labId, out var current);
            model.data.labId = LabForm.GetOrCreate(
                category ?? current?.lv1Lab ?? string.Empty,
                type ?? current?.lv2Lab ?? string.Empty,
                current?.lv3Lab ?? string.Empty,
                nameof(EventProgramDataForm));
        }

        public void RefreshUnit()
        {
            unitCon.Clear();
            if (model.selItem != null)
            {
                unitCon.Add(new UiUnitParam()
                {
                    con = unitCon,
                    parent = view.rtf_unitRoot,
                    node = model.selItem,
                    deepth = 0,
                });
            }

            unitCon.Refresh();


            TimeManager.instance.AddCurLateUpdateAction(() =>
            {
                UiManager.Rebuild(view.rtf_unitRoot.gameObject, true);
            }, gameObject);

        }
        public void RefreshUnitDetail()
        {
            view.sta_item.ChangeState(model.selItem == null ? 0 : 1);

            view.sta_unit.ChangeState(model.selUnit == null ? 0 : 1);

            if (model.selUnit != null)
            {

            }
        }
        public void SelItem(SyntaxNode node)
        {
            model.selItem = node;
            Refresh();
        }

        public void SelUnit(SyntaxNode node)
        {
            model.selUnit = node;
            Refresh(); 
            TimeManager.instance.AddCurLateUpdateAction(() =>
            {
                UiManager.Rebuild(view.scr_units.gameObject, true);
            }, gameObject);
            if (node != null)
            {
                TimeManager.instance.AddCurLateUpdateAction(() =>
                {
                    for (int i = 0, icnt = unitCon.paramLst.Count; i < icnt; i++)
                    {
                        if (unitCon.paramLst[i] is UiUnitParam uPrm && uPrm.node == node)
                        {
                            UiManager.Jump(unitCon.Get(uPrm).ctrl.rect, view.scr_units);
                        }
                    }
                }, gameObject);
            }
        }
        public void ReplaceNode(SyntaxNode nodeNow, SyntaxNode nodeNew)
        {

            foreach (var o in model.curEntry)
            {
                Debug.Log(o.Contains(nodeNow) + "!!!");
            }
            if (nodeNow.parentNode != null)
            {
                var tmp = nodeNow.parentNode;
                while (tmp != null)
                {
                    if (tmp == model.selItem)
                    {
                        Debug.Log("finded");
                    }
                    tmp = tmp.parentNode;
                }
                for (int i = 0, icnt = nodeNow.parentNode.subNodes.Count; i < icnt; i++)
                {
                    if (nodeNow.parentNode.subNodes[i] == nodeNow)
                    {
                        nodeNow.parentNode.subNodes[i] = nodeNew;
                        nodeNew.parentNode = nodeNow.parentNode;
                    }
                }
            }
            else
            {
                for (int i = 0, icnt = model.curEntry.Count; i < icnt; i++)
                {
                    if (model.curEntry[i] == nodeNow)
                    {
                        model.curEntry[i] = nodeNew;
                    }
                }
            }

            if (nodeNow == model.selItem)
            {
                model.selItem = nodeNew;
            }
            if (nodeNow == model.selUnit)
            {
                model.selUnit = nodeNew;
            }
            foreach (var o in model.curEntry)
            {
                Debug.Log(o.Contains(nodeNew) + "!!!");
            }
        }
        private List<SyntaxNode> FindParentList(List<SyntaxNode> o, SyntaxNode target)
        {
            foreach (var sub in o)
            {
                if (sub == target)
                {
                    return o;
                }
                else
                {
                    var res = FindParentList(sub.subNodes, target);
                    if (res != null)
                    {
                        return res;
                    }
                }
            }
            return null;
        }
    }
}
