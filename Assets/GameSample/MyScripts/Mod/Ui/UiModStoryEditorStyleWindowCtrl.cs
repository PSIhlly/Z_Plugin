using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z_Ui.Base;
using Z_Texture;

using UnityEngine.Video;
using UnityEngine.UI;

namespace Ui.ModStoryEditorStyleWindow
{

    public partial class UiModStoryEditorStyleWindowParam
    {
        public Action<EditorStyle> onSelect;
    }
    public partial class UiModStoryEditorStyleWindowModel
    {
        public UiModStoryEditorStyleWindowParam prm;
        public EditorStyle sel;
    }
    public partial class UiModStoryEditorStyleWindowCtrl
    {

        UiContainer<UiStyleCtrl> styleCon;
        public override void OnCreate()
        {

            view.btn_bbg.onClick.AddListener(() =>
            {

            });
            styleCon = new UiContainer<UiStyleCtrl>(view.go_style);
            view.btn_close.onClick.AddListener(() =>
            {
                Close();
            });
            view.btn_.onClick.AddListener(() =>
            {
                model.prm.onSelect?.Invoke(model.sel);
                Close();
            });
        }
        public override void OnShow()
        {
            model.prm = param;
            model.sel = GameManager.instance.curProgress == null ? EditorStyle.Avg : GameManager.instance.curProgress.editorStyle;
            Refresh();
        }
        public void Refresh()
        {

            view.txt_desc.oriText = model.sel + "_desc";
            styleCon.Clear();
            for (int i = 0, icnt = Enum.GetValues(typeof(EditorStyle)).Length; i < icnt; i++)
            {
                styleCon.Add(new UiStyleParam()
                {
                    type = (EditorStyle)i
                });
            }
            styleCon.Refresh();
        }
    }

    public partial class UiStyleParam
    {
        public EditorStyle type;
    }
    public partial class UiStyleModel
    {
        public UiStyleParam prm;
    }
    public partial class UiStyleCtrl
    {

        public override void OnCreate()
        {

            view.btn_.onClick.AddListener(() =>
            {
                parent.model.sel = model.prm.type;
                parent.Refresh();
            });

        }
        public override void OnShow()
        {
            model.prm = param;
            Refresh();
        }
        public void Refresh()
        {
            view.txt_.oriText = model.prm.type.ToString();
            view.sta_.ChangeState(model.prm.type == parent.model.sel ? 1 : 0);
        }
    }


}