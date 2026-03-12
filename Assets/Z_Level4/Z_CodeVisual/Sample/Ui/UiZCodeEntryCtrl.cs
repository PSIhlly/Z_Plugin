using System.Collections.Generic;
using UnityEngine.UI;
using Z_Code;
using Z_Code.Form;
using Z_Time;
using Z_Ui.Base;
namespace Ui.ZCodeEntry
{
    public partial class UiZCodeEntryModel
    {
        public Compiler cpr;
        public Decompiler dcpr;
        public List<SyntaxNode> curEntry;
        public InterpretDataForm.Data interpreter;
    }
    public partial class UiZCodeEntryCtrl
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
            var pg = new ProgramDataForm.Data(-1, "", view.ipt_code.text, model.cpr.Compile(view.ipt_code.text, out var syntaxs));
            model.interpreter = new InterpretDataForm.Data(-1, new List<BoxDataForm.Data>(), new Dictionary<string, BoxDataForm.Data>(), pg, 0, -1,0, null);

            view.btn_run.onClick.AddListener(() =>
            {
                model.interpreter.Interpret();
            });
            view.btn_toCode.onClick.AddListener(() =>
            {
                model.interpreter.program.code = model.dcpr.Decompile(model.curEntry);
                model.interpreter.program.zCode = model.cpr.Compile(model.interpreter.program.code, out var nodes);
                view.ipt_code.Set(model.interpreter.program.code);
            });
            view.btn_toEntry.onClick.AddListener(() =>
            {
                model.interpreter.program.code = view.ipt_code.text;
                model.interpreter.program.zCode = model.cpr.Compile(model.interpreter.program.code, out model.curEntry);
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
                itemCon.Refresh();
            });

        }
        public override void OnShow()
        {

        }


        public void RefreshUnit(SyntaxNode node)
        {

            unitCon.Clear();

            unitCon.Add(new UiUnitParam()
            {
                con = unitCon,
                parent = view.rtf_unitRoot,
                node = node,
                deepth = 0,
            });
            unitCon.Refresh();

            LayoutRebuilder.ForceRebuildLayoutImmediate(view.rtf_unitRoot);
            TimeManager.instance.StartTimer(0.5f, 0, () =>
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(view.rtf_unitRoot);
                return true;
            }, uiHolder);

        }

        public void SelUnit(SyntaxNode node)
        {

        }
    }
}
