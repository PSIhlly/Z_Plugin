using Z_Ui;
using Z_Ui.Base;
using Z_Ui.Notify;

namespace Ui.Notify
{
    public partial class UiQuickChooseParam
    {
        public QuickChooseInfo info;
    }
    public partial class UiQuickChooseModel
    {
        public QuickChooseInfo info;
        public EntryItem cur;
    }

    public partial class UiQuickChooseCtrl
    {

        UiContainer<UiQuickItemCtrl> con;
        public override void OnCreate()
        {
            con = new UiContainer<UiQuickItemCtrl>(this, view.go_quickItem);

        }
        public override void Close()
        {
            base.Close();
            parent.RemoveQuickChoose(model.info.id);
        }
        public override void OnShow()
        {
            if (param != null)
            {
                model.info = param.info;
                model.cur = param.info.items;
            }
            Refresh();

        }
        public void Refresh()
        {
            con.Clear();

            foreach (var item in model.info.items.subs.Values)
            {
                con.Add(new UiQuickItemParam()
                {
                    cur = item
                });
            }
            con.Refresh();
            UiManager.Rebuild(gameObject, true);
        }


    }

    public partial class UiQuickItemParam
    {
        public EntryItem cur;
    }
    public partial class UiQuickItemModel
    {
        public EntryItem cur;
    }
    public partial class UiQuickItemCtrl
    {
        public override void OnCreate()
        {
            view.btn_.onClick.AddListener(() =>
            {
                if(parent.model.info.func(model.cur))
                {
                    parent.Close();
                }
            });
        }
        public override void OnShow()
        {
            if (param != null)
            {
                model.cur = param.cur;
            }
            Refresh();
        }
        public void Refresh()
        {
            view.txt_.text = model.cur.content;
           // view.img_.sprite = model.cur.sprite==null?model.cur.sprite;
        }
    }

}
