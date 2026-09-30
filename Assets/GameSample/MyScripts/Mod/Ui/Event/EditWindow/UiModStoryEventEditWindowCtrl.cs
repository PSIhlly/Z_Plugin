using Form;
using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using Ui.ModStory.ModStoryEvent.ModStoryEventCustom;
using UnityEngine;
using UnityEngine.UI;
using Z_Code;
using Z_Code.Form;
using Z_CodeVisual;
using Z_DataSystem.Form;
using Z_Text;
using Z_Time;
using Z_Ui;
using Z_Ui.Base;
using Z_Ui.Notify;
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
        UiContainer<UiItemCtrl> itemCon;
        UiContainer<UiLineCtrl> lineCon;
        private int codeErrorRequestId;

        public override void OnCreate()
        {

            itemCon = new UiContainer<UiItemCtrl>(this, view.go_item, false);
            lineCon = new UiContainer<UiLineCtrl>(this, view.go_line, false);

            model.cpr = new Compiler();
            model.dcpr = new Decompiler();
            model.curEntry = new List<SyntaxNode>();
            view.ipt_code.richText = false;

            view.ipt_name.onFinishInput += (v) =>
            {
                model.data.name = v;
                view.txt_title.text = v;
            };
            view.btn_lab.onClick.AddListener(ChooseLab);
            view.btn_switchMod.onClick.AddListener(() =>
            {
                if (!ApplyCurrentMode())
                    return;
                model.codeEditMode = !model.codeEditMode;
                model.selItem = null;
                model.selUnit = null;
                if (model.codeEditMode)
                {
                    RefreshMode();
                }
                else
                {
                    // Item rows must be created after their panel is visible, otherwise
                    // nested ContentSizeFitters measure the hidden hierarchy as zero.
                    Refresh();
                    TimeManager.instance.AddNextUpdateAction(() =>
                    {
                        if (active && !model.codeEditMode)
                            UiManager.Rebuild(view.scr_items.gameObject, true);
                    }, gameObject);
                }
            });
            view.btn_apply.onClick.AddListener(() => { ApplyCurrentMode(); });
            view.btn_close.onClick.AddListener(() =>
            {
                if (ApplyCurrentMode())
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
                        }, model.selUnit);
                    }
                    else if (ProgramDataForm.DataByName.TryGetValue(item.content, out var program))
                    {
                        if (!TryCompileSyntax(BuildProgramCallCode(program), out var res))
                            return;
                        ReplaceNode(model.selUnit, res[0]);
                    }

                });

            });
            view.btn_del.onClick.AddListener(() =>
            {
                var deletedNode = model.selItem;
                var parentList = FindParentList(model.curEntry, model.selItem);
                if (parentList != null)
                {
                    parentList.Remove(model.selItem);
                    HideRenderedItems(deletedNode, true);
                }

                model.selItem = null;
                model.selUnit = null;
                RefreshUnitDetail();
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
                        else if (ProgramDataForm.DataByName.TryGetValue(item.content, out var program))
                        {
                            defaultCode = BuildProgramCallCode(program);
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
                        model.selItem = null;
                        model.selUnit = null;
                        RefreshUnitDetail();
                    }

                });
            });
        }
        internal static string BuildProgramCallCode(ProgramDataForm.Data program)
        {
            var code = new StringBuilder(program.name).Append('(');
            for (var i = 1; i <= program.paramCount; i++)
            {
                if (i > 1)
                    code.Append(',');
                code.Append("param").Append(i);
            }
            return code.Append(");").ToString();
        }

        private bool ApplyCurrentMode()
        {
            return model.codeEditMode ? ApplyCode() : ApplyEntry();
        }

        public bool ApplyEntry()
        {
            var code = model.dcpr.Decompile(model.curEntry);
            if (model.data.TryApplyCode(code, out var syntaxNodes, out var errors, model.cpr))
            {
                model.curEntry = syntaxNodes;
                model.selItem = null;
                model.selUnit = null;
                view.ipt_code.Set(model.data.code);
                Refresh();
                return true;
            }
            else
            {
                ShowCompileErrors(errors);
                return false;
            }
        }
        public bool ApplyCode()
        {
            codeErrorRequestId++;
            if (model.data.TryApplyCode(view.ipt_code.text, out var syntaxNodes,
                    out var errors, model.cpr))
            {
                model.curEntry = syntaxNodes;
                model.selItem = null;
                model.selUnit = null;
                return true;
            }
            else
            {
                ShowCodeCompileError(errors);
                return false;
            }
        }

        private void ShowCodeCompileError(List<CompileError> errors)
        {
            var error = errors != null && errors.Count > 0 ? errors[0] : null;
            var source = view.ipt_code.text ?? string.Empty;
            var errorIndex = Mathf.Clamp(error?.StartIndex ?? 0, 0, source.Length);
            view.ipt_code.ActivateInputField();
            view.ipt_code.stringPosition = errorIndex;
            view.ipt_code.ForceLabelUpdate();
            var requestId = codeErrorRequestId;
            TimeManager.instance.AddNextUpdateAction(() =>
            {
                TimeManager.instance.AddCurFrameEndAction(() =>
                {
                    if (!active || !model.codeEditMode || requestId != codeErrorRequestId ||
                        view.ipt_code.text != source)
                        return;

                    var position = GetCodeErrorScreenPosition(errorIndex);
                    var message = error?.ToString() ?? "编译失败";
                    if (errors != null && errors.Count > 1)
                        message += $"\n……另有 {errors.Count - 1} 个错误";
                    NotifyManager.instance.AddComment(message, position);
                }, gameObject);
            }, gameObject);
        }

        private Vector2 GetCodeErrorScreenPosition(int stringIndex)
        {
            var input = view.ipt_code;
            var text = input.textComponent;
            text.ForceMeshUpdate();
            var textInfo = text.textInfo;
            var canvas = text.canvas;
            var camera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? canvas.worldCamera
                : null;
            if (textInfo.characterCount == 0)
                return RectTransformUtility.WorldToScreenPoint(camera, input.textViewport.position);

            int caretIndex = textInfo.characterCount - 1;
            for (int i = 0; i < textInfo.characterCount; i++)
            {
                if (textInfo.characterInfo[i].index < stringIndex)
                    continue;
                caretIndex = i;
                break;
            }

            var caretCharacter = textInfo.characterInfo[caretIndex];
            int line = caretCharacter.lineNumber;
            bool atLineStart = caretIndex == textInfo.lineInfo[line].firstCharacterIndex;
            TMP_CharacterInfo positionCharacter = atLineStart
                ? caretCharacter
                : textInfo.characterInfo[caretIndex - 1];
            float x = atLineStart ? positionCharacter.origin : positionCharacter.xAdvance;
            var worldPosition = text.rectTransform.TransformPoint(
                new Vector3(x, positionCharacter.ascender, 0));
            return RectTransformUtility.WorldToScreenPoint(camera, worldPosition) + new Vector2(12, 12);
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
            codeErrorRequestId++;
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
            codeErrorRequestId++;
            model.onClose?.Invoke();
        }
        public void Refresh()
        {
            RefreshMode();

            lineCon.Clear();
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
            for (int i = 0; i < itemCon.paramLst.Count; i++)
            {
                var item = (UiItemParam)itemCon.paramLst[i];
                lineCon.Add(new UiLineParam()
                {
                    item = (UiItemCtrl)itemCon.Get(item).ctrl,
                    node = item.node,
                    index = i,
                });
            }
            lineCon.Refresh();
            UiManager.Rebuild(view.scr_items.gameObject, true);

            //
            view.txt_title.text = model.data.name;

            view.ipt_name.Set(model.data.name);
            LabForm.TryGetData(model.data.labId, out var lab);
            view.txt_lab.text = GetLabLevelText(lab?.lv1Lab);
        }

        private void RefreshMode()
        {
            view.sta_switchMod.ChangeState(model.codeEditMode ? 1 : 0);
            view.sta_switchModPanel.ChangeState(model.codeEditMode ? 1 : 0);
            RefreshUnitDetail();
        }

        private static string GetLabLevelText(string level)
        {
            return string.IsNullOrEmpty(level)
                ? TextManager.instance.GetTxt(GlobalDefaultHelper.defaultLab)
                : level;
        }

        private void ChooseLab()
        {
            var categories = new List<string> { string.Empty };
            categories.AddRange(UiModStoryEventCustomCtrl.GetCategories());
            ChooseLabLevel("label", categories, category =>
            {
                LabForm.TryGetData(model.data.labId, out var current);
                if (category == (current?.lv1Lab ?? string.Empty))
                    return;

                model.data.labId = LabForm.GetOrCreate(
                    category, string.Empty, string.Empty, nameof(EventProgramDataForm));
                view.txt_lab.text = GetLabLevelText(category);
            });
        }

        private static void ChooseLabLevel(string titleKey, List<string> levels, Action<string> onSelected)
        {
            var items = new Z_Ui.Notify.EntryItem();
            for (var i = 0; i < levels.Count; i++)
            {
                var displayName = GetLabLevelText(levels[i]);
                var uniqueName = displayName;
                var suffix = 2;
                while (items.subs.ContainsKey(uniqueName))
                    uniqueName = $"{displayName} ({suffix++})";
                items.Add(uniqueName, null, i);
            }

            NotifyManager.instance.AddChoose(TextManager.instance.GetTxt(titleKey), true, item =>
            {
                if (item.id < 0 || item.id >= levels.Count)
                    return false;

                onSelected(levels[item.id]);
                return true;
            }, items);
        }

        public void RefreshUnitDetail()
        {
            view.sta_item.ChangeState(model.selItem == null ? 0 : 1);

            view.sta_unit.ChangeState(model.selUnit == null ? 0 : 1);

            if (model.selUnit != null)
            {

            }
        }
        public void SelUnit(SyntaxNode node, SyntaxNode itemNode)
        {
            model.selItem = itemNode;
            model.selUnit = node;
            foreach (UiItemParam item in itemCon.paramLst)
            {
                ((UiItemCtrl)itemCon.Get(item).ctrl).RefreshUnitSelection();
            }
            foreach (UiLineParam line in lineCon.paramLst)
            {
                ((UiLineCtrl)lineCon.Get(line).ctrl).RefreshSelection();
            }
            RefreshUnitDetail();
        }
        public void ReplaceNode(SyntaxNode nodeNow, SyntaxNode nodeNew)
        {
            var itemNode = model.selItem;
            if (nodeNow.parentNode != null)
            {
                var parentNode = nodeNow.parentNode;
                var index = parentNode.subNodes.IndexOf(nodeNow);
                if (index < 0)
                    return;
                parentNode.subNodes[index] = nodeNew;
                nodeNew.parentNode = parentNode;
            }
            else
            {
                var index = model.curEntry.IndexOf(nodeNow);
                if (index < 0)
                    return;
                model.curEntry[index] = nodeNew;
                nodeNew.parentNode = null;
            }

            if (nodeNow == itemNode)
                model.selItem = nodeNew;
            if (nodeNow == model.selUnit)
                model.selUnit = nodeNew;

            if (nodeNow == itemNode)
                HideRenderedItems(nodeNow, false);
            foreach (UiItemParam item in itemCon.paramLst)
            {
                if (item.node != itemNode)
                    continue;
                if (nodeNow == itemNode)
                    item.node = nodeNew;
                var itemCtrl = (UiItemCtrl)itemCon.Get(item).ctrl;
                itemCtrl.RefreshUnits();
                UiManager.Rebuild(itemCtrl.gameObject, true);
                break;
            }
            if (nodeNow == itemNode)
                foreach (UiLineParam line in lineCon.paramLst)
                    if (line.node == itemNode)
                    {
                        line.node = nodeNew;
                        ((UiLineCtrl)lineCon.Get(line).ctrl).RefreshSelection();
                        break;
                    }
            RefreshUnitDetail();
        }

        private void HideRenderedItems(SyntaxNode subtree, bool includeRoot)
        {
            if (subtree == null)
                return;

            var start = -1;
            for (var i = 0; i < itemCon.paramLst.Count; i++)
                if (((UiItemParam)itemCon.paramLst[i]).node == subtree)
                {
                    start = i;
                    break;
                }
            if (start < 0)
                return;

            var depth = ((UiItemParam)itemCon.paramLst[start]).deepth;
            for (var i = includeRoot ? start : start + 1; i < itemCon.paramLst.Count; i++)
            {
                var item = (UiItemParam)itemCon.paramLst[i];
                if (i > start && item.deepth <= depth)
                    break;
                itemCon.Get(item).gameObject.SetActive(false);
                lineCon.Get(lineCon.paramLst[i]).gameObject.SetActive(false);
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
