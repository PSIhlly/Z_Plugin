using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.Progress;
using Z_Code;
using Z_CodeVisual;
using Z_Ui.Base;
using UnityEngine.UI;
using Z_Time;
using Z_Code.Form;
using Form;
using UnityEditor.Experimental.GraphView;
using System;
using Z_Ui;
using static Unity.Burst.Intrinsics.X86.Avx;
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

            unitCon = new UiContainer<UiUnitCtrl>(view.go_unit, false);
            itemCon = new UiContainer<UiItemCtrl>(view.go_item);

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
                model.data.category = v;
                Refresh();
            };
            view.ipt_type.onFinishInput += (v) =>
            {
                model.data.type = v;
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
                ModManager.instance.assetCtrl.ChooseCmd(SceneEventType.All, GlobalEventHelper.GetGameRetType(model.selUnit.desc), (item) =>
                {
                    var data = GameCmdDataForm.DataByUid[item.id];
                    BaseData.cmdDic[data.name].GetUnitChooseCode((code) =>
                    {
                        model.cpr.Compile(code, out var res);
                        ReplaceNode(model.selUnit, res[0]);
                        ApplyEntry();
                    });
                });

            });
            view.btn_del.onClick.AddListener(() =>
            {
                model.curEntry.Remove(model.selItem);
                model.selItem = null;
                ApplyEntry();
            });
            view.btn_insert.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.ChooseCmd(SceneEventType.All, "", (item) =>
                {
                    if (item != null)
                    {
                        var data = GameCmdDataForm.DataByUid[item.id];
                        GameCmdDataForm.Data sel = GameCmdDataForm.DataByName[data.name];
                        model.cpr.Compile(sel.defaultCode, out var res);
                        int id = model.curEntry.IndexOf(model.selItem);
                        foreach (var r in res)
                        {
                            model.curEntry.Insert(id, r);
                            model.selItem = r;
                            id++;
                        }
                        ApplyEntry();
                    }

                }, true);
            });
        }
        public void ApplyEntry()
        {
            model.data.code = model.dcpr.Decompile(model.curEntry);
            model.data.zCode = model.cpr.Compile(model.data.code, out _);
            view.ipt_code.Set(model.data.code);
            Refresh();
        }
        public void ApplyCode()
        {
            model.data.code = view.ipt_code.text;
            model.data.zCode = model.cpr.Compile(model.data.code, out model.curEntry);
            Refresh();
        }
        public override void OnShow()
        {
            model.codeEditMode = false;
            model.data = param.data;
            model.onClose = param.onClose;
            model.data.zCode = model.cpr.Compile(model.data.code, out model.curEntry);
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

            itemCon.Clear();
            foreach (var node in model.curEntry)
            {
                itemCon.Add(new UiItemParam()
                {
                    con = itemCon,
                    node = node,
                    deepth = 0,
                });
            }
            itemCon.Add(new UiItemParam()
            {
                con = itemCon,
                node = null,
                deepth = 0,
            });
            itemCon.Refresh();
            //
            view.txt_title.text = model.data.name;

            view.ipt_name.Set(model.data.name);
            view.ipt_category.Set(model.data.category);
            view.ipt_type.Set(model.data.type);

            view.sta_switchMod.ChangeState(model.codeEditMode ? 1 : 0);
            view.sta_switchModPanel.ChangeState(model.codeEditMode ? 1 : 0);
            RefreshUnitDetail(); 
            RefreshUnit();
            TimeManager.instance.AddCurLateUpdateAction(() =>
            {
                UiManager.Rebuild(view.rtf_unitRoot.gameObject, true);
            }, gameObject);
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
    }
}
