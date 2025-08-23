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
namespace Ui.ModStoryEventEditWindow
{
    public partial class UiModStoryEventEditWindowParam
    {
        public EventProgramDataForm.Data data;
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
           
            view.ipt_name.onFinishInput+=(v) =>
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
                if(model.codeEditMode)
                {
                    model.data.code = view.ipt_code.text;
                    model.data.zCode = model.cpr.Compile(model.data.code, out model.curEntry);
                }
                else
                {
                    model.data.code = model.dcpr.Decompile(model.curEntry);
                    model.data.zCode = model.cpr.Compile(model.data.code, out model.curEntry);
                    view.ipt_code.Set(model.data.code);
                }

                Refresh();
            });
            view.btn_close.onClick.AddListener(() =>
            {
                Close();
            });
        }
        public override void OnShow()
        {
            model.codeEditMode = false;
            model.data = param.data;

            model.selItem = null;
            model.selUnit = null;
            Refresh();
        }
        public override void Close()
        {
            GameManager.instance.saveCtrl.SaveEvent(ModManager.instance.GetStoryCoreFolder(),model.data);
            base.Close();
        }
        public void Refresh()
        {
            model.curEntry.Clear();
            model.cpr.Compile(model.data.code, out model.curEntry);

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
            RefreshUnit();

            view.txt_title.text = model.data.name;

            view.ipt_name.Set(model.data.name);
            view.ipt_category.Set(model.data.category);
            view.ipt_type.Set(model.data.type);

            view.sta_switchMod.ChangeState(model.codeEditMode ? 1 : 0);
            view.sta_switchModPanel.ChangeState(model.codeEditMode ? 1 : 0);
        }
        public void RefreshUnit()
        {
            unitCon.Clear();
            if(model.selItem!=null)
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

            LayoutRebuilder.ForceRebuildLayoutImmediate(view.rtf_unitRoot);
            TimeManager.instance.StartTimer(0.5f, 0, () =>
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(view.rtf_unitRoot);
                return true;
            }, uiHolder);

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
        }
    }
}
