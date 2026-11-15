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
        UiTreeContainer<UiItemCtrl> itemCon;
        UiTreeContainer<UiLineCtrl> lineCon;
        private int codeErrorRequestId;
        private string compiledSource;
        private bool entriesDirty, listDirty = true;
        public int CompileCount { get; private set; }

        public override void OnCreate()
        {

            itemCon = new UiTreeContainer<UiItemCtrl>(this, view.go_item, viewport: view.scr_items);
            lineCon = new UiTreeContainer<UiLineCtrl>(this, view.go_line, viewport: view.scr_items, sizeContent: false);

            model.cpr = new Compiler();
            model.dcpr = new Decompiler();
            model.curEntry = new List<SyntaxNode>();
            view.ipt_code.richText = false;

            view.ipt_name.onFinishInput += (v) =>
            {
                if (model.data.name != v) compiledSource = null;
                model.data.name = v;
                view.txt_title.text = v;
            };
            view.btn_lab.onClick.AddListener(ChooseLab);
            view.btn_switchMod.onClick.AddListener(() =>
            {
                if (!ApplyCurrentMode(false))
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
                }
                RefreshSelection();
            });
            view.btn_apply.onClick.AddListener(() =>
            {
                if (ApplyCurrentMode())
                    NotifyManager.instance.AddTip(TextManager.instance.GetTxt("eventApplied"));
            });
            view.btn_close.onClick.AddListener(() =>
            {
                if (ApplyCurrentMode(false))
                    Close();
            });
            view.btn_edit.onClick.AddListener(() =>
            {
                var tp = GlobalEventHelper.GetGameRetType(model.selUnit.desc);
                if (model.selUnit.parentNode != null && model.selUnit.parentNode.desc.type == CodeType.FuncName)
                {
                    if (CmdBase.TryGetForm(model.selUnit.parentNode.desc.code, out var data))
                    {
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
                                code = BuildCommandStatementCode(data, code);
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
                var parentList = FindParentList(model.curEntry, model.selItem);
                if (parentList != null)
                {
                    parentList.Remove(model.selItem);
                    MarkEntriesDirty();
                }

                model.selItem = null;
                model.selUnit = null;
                Refresh();
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
                            defaultCode = BuildCommandStatementCode(sel, sel.defaultCode);
                        }
                        else if (ProgramDataForm.DataByName.TryGetValue(item.content, out var program))
                        {
                            defaultCode = BuildProgramCallCode(program);
                        }


                        if (!TryCompileSyntax(defaultCode, out var res))
                            return;
                        var parentLst = FindParentList(model.curEntry, model.selItem);
                        if (parentLst == null)
                            return;
                        int id = parentLst.IndexOf(model.selItem);
                        foreach (var r in res)
                        {
                            parentLst.Insert(id, r);
                            model.selItem = r;
                            id++;
                        }
                        model.selItem = null;
                        model.selUnit = null;
                        // Structural edits need new Item/Line rows, but remain an unapplied draft.
                        MarkEntriesDirty();
                        Refresh();
                    }

                });
            });
        }
        internal static string BuildCommandStatementCode(GameCmdDataForm.Data command, string code)
        {
            if (string.IsNullOrEmpty(command.allowAsVoid))
                return code;
            // Some authored defaults already include their result assignment.
            // Keep complete statements intact; wrap only a call expression.
            if (code.TrimStart().StartsWith(command.allowAsVoid, StringComparison.Ordinal))
                return code;
            return command.allowAsVoid + code.TrimEnd().TrimEnd(';').TrimEnd() + ";";
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

        private bool ApplyCurrentMode(bool refreshList = true)
        {
            return model.codeEditMode ? ApplyCode() : ApplyEntry(refreshList);
        }

        public bool ApplyEntry(bool refreshList = true)
        {
            if (!entriesDirty && compiledSource == model.data.code)
            {
                SetCodeText(model.data.code);
                if (refreshList && listDirty) Refresh();
                return true;
            }
            var code = model.dcpr.Decompile(model.curEntry);
            CompileCount++;
            if (model.data.TryApplyCode(code, out var syntaxNodes, out var errors, model.cpr))
            {
                // The draft AST already describes this source. Keep its identity
                // and bound rows instead of replacing it with an equivalent tree.
                compiledSource = model.data.code;
                entriesDirty = false;
                model.selItem = null;
                model.selUnit = null;
                SetCodeText(model.data.code);
                if (refreshList) Refresh();
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
            if (!entriesDirty && compiledSource == model.data.code && view.ipt_code.text == compiledSource)
                return true;
            CompileCount++;
            if (model.data.TryApplyCode(view.ipt_code.text, out var syntaxNodes,
                    out var errors, model.cpr))
            {
                model.curEntry = syntaxNodes;
                compiledSource = model.data.code;
                entriesDirty = false;
                listDirty = true;
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
                    NotifyManager.instance.AddComment(message, position, 5f);
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
            CompileCount++;
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
            lineCon.Clear(); itemCon.Clear();
            model.codeEditMode = false;
            model.data = param.data;
            model.onClose = param.onClose;

            bool validSource = TryCompileSyntax(model.data.code, out var initialEntries);
            if (!validSource)
                initialEntries = new List<SyntaxNode>();
            model.curEntry = initialEntries;
            compiledSource = validSource ? model.data.code : null;
            entriesDirty = !validSource;
            listDirty = true;
            SetCodeText(model.data.code);

            model.selItem = null;
            model.selUnit = null;
            Refresh();
        }
        public override void OnDisable()
        {
            codeErrorRequestId++;
            UiTreeLayout.Cancel(view.scr_items.content);
            itemCon.SetViewportEnabled(false); lineCon.SetViewportEnabled(false);
            model.onClose?.Invoke();
        }
        public void Refresh()
        {
            if (!model.codeEditMode && listDirty)
            {
                // Recycle stale hidden rows before making the Item panel visible.
                lineCon.Clear();
                itemCon.Clear();
                foreach (var node in model.curEntry)
                    PlanRow(node, 0, model.curEntry);
                PlanRow(null, 0, model.curEntry);
                for (int i = 0; i < itemCon.paramLst.Count; i++)
                {
                    var item = (UiItemParam)itemCon.paramLst[i];
                    lineCon.Add(new UiLineParam()
                    {
                        row = item,
                        node = item.node,
                        index = i,
                    });
                }
                listDirty = false;
            }
            RefreshMode();
            if (!model.codeEditMode)
            {
                itemCon.Refresh();
                lineCon.Refresh();
                if (!itemCon.IsVirtualized) UiTreeLayout.Request(view.scr_items.content);
            }

            //
            view.txt_title.text = model.data.name;

            view.ipt_name.Set(model.data.name);
            LabForm.TryGetData(model.data.labId, out var lab);
            view.txt_lab.text = GetLabLevelText(lab?.lv1Lab);
        }

        private void RefreshMode()
        {
            itemCon.SetViewportEnabled(!model.codeEditMode);
            lineCon.SetViewportEnabled(!model.codeEditMode);
            if (model.codeEditMode) UiTreeLayout.Cancel(view.scr_items.content);
            view.sta_switchMod.ChangeState(model.codeEditMode ? 1 : 0);
            view.sta_switchModPanel.ChangeState(model.codeEditMode ? 1 : 0);
            RefreshUnitDetail();
        }

        internal void MarkEntriesDirty(bool structure = true)
        {
            entriesDirty = true;
            listDirty |= structure;
        }

        private void SetCodeText(string code)
        {
            if (view.ipt_code.text != code) view.ipt_code.Set(code);
        }

        // Expand structural rows independently of OnShow, so offscreen blocks
        // and their New rows remain in the scroll extent and AST editing model.
        private void PlanRow(SyntaxNode node, int depth, List<SyntaxNode> target)
        {
            itemCon.Add(new UiItemParam { con = itemCon, node = node, deepth = depth,
                targetNewList = target, childrenPlanned = true });
            if (node == null) return;
            foreach (var sub in node.subNodes)
                if (sub.desc.type == CodeType.Action && !SyntaxAnalysis.IsEmptyNode(sub))
                {
                    if (node.desc.type == CodeType.Reserved && node.desc.code == "for")
                    {
                        foreach (var statement in sub.subNodes) PlanRow(statement, depth + 1, sub.subNodes);
                        PlanRow(null, depth + 1, sub.subNodes);
                    }
                    else PlanRow(sub, depth + 1, target);
                }
            if (node.desc.type == CodeType.Action)
            {
                foreach (var sub in node.subNodes) PlanRow(sub, depth + 1, node.subNodes);
                PlanRow(null, depth + 1, node.subNodes);
            }
        }

        internal bool TryGetItem(UiItemParam row, out UiItemCtrl item)
        {
            item = itemCon.TryGet(row, out var holder) ? (UiItemCtrl)holder.ctrl : null;
            return item != null;
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
            RefreshSelection();
        }

        private void RefreshSelection()
        {
            foreach (UiItemParam item in itemCon.paramLst)
            {
                if (itemCon.TryGet(item, out var holder)) ((UiItemCtrl)holder.ctrl).RefreshUnitSelection();
            }
            foreach (UiLineParam line in lineCon.paramLst)
            {
                if (lineCon.TryGet(line, out var holder)) ((UiLineCtrl)holder.ctrl).RefreshSelection();
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

            MarkEntriesDirty(nodeNow == itemNode);
            if (nodeNow == itemNode)
            {
                Refresh();
                return;
            }
            foreach (UiItemParam item in itemCon.paramLst)
            {
                if (item.node != itemNode)
                    continue;
                if (!TryGetItem(item, out var itemCtrl)) break;
                itemCtrl.RefreshUnits();
                if (itemCon.IsVirtualized)
                {
                    UiTreeLayout.Rebuild(itemCtrl.rect);
                    itemCon.InvalidateViewport();
                }
                else UiTreeLayout.Request(view.scr_items.content);
                break;
            }
            RefreshUnitDetail();
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
