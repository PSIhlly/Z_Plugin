using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using Z_DataSystem.Form;
using Z_DesignStyle;
using Z_Texture;
using Z_Ui.Base;

namespace Ui.TeamSelectWindow
{

    public partial class UiTeamSelectWindowParam
    {

    }
    public partial class UiTeamSelectWindowModel
    {
        public int selTeamerUid;
        public int selActiveTeamerUid;
    }
    public partial class UiTeamSelectWindowCtrl
    {

        UiScrViewContainer<UiActiveTeamerCtrl> activeTeamerCon;
        UiScrViewContainer<UiTeamerCtrl> teamerCon;

        public override void OnCreate()
        {
            view.btn_bg.onClick.AddListener(() =>
            {
                Close();
            });
            view.btn_close.onClick.AddListener(() =>
            {
                Close();
            });

            activeTeamerCon = new UiScrViewContainer<UiActiveTeamerCtrl>(this, view.go_activeTeamer, view.scr_activeTeamers);
            teamerCon = new UiScrViewContainer<UiTeamerCtrl>(this, view.go_teamer, view.scr_teamers);

        }
        public override void OnShow()
        {
            model.selTeamerUid = 0;
            model.selActiveTeamerUid = 0;
            Refresh();
        }
        public void Refresh()
        {
            activeTeamerCon.Clear();
            for (int i = 0; i < 4; i++)
            {
                var uid = GameManager.instance.curProgress.teamActive.GetDv(i, 0);
                activeTeamerCon.Add(new UiActiveTeamerParam() { uid = uid, index = i });
            }
            activeTeamerCon.Refresh();

            teamerCon.Clear();
            foreach (var uid in GameManager.instance.curProgress.team.OrderBy(u => u))
            {
                teamerCon.Add(new UiTeamerParam() { uid = uid });
            }
            teamerCon.Refresh();
        }
    }

    public partial class UiActiveTeamerParam
    {
        public int uid;
        public int index;
    }
    public partial class UiActiveTeamerModel
    {
        public int uid;
        public int index;
    }
    public partial class UiActiveTeamerCtrl
    {

        public override void OnCreate()
        {
            view.btn_.onClick.AddListener(() =>
            {
                if (parent.model.selTeamerUid != 0)
                {
                    // 选中了Teamer，点击ActiveTeamer替换
                    PlayManager.instance.infoCtrl.ReplaceActiveTeamer(model.uid, parent.model.selTeamerUid, model.index);
                    parent.model.selTeamerUid = 0;
                    parent.Refresh();
                }
                else
                {
                    // 没有选中Teamer，选中当前ActiveTeamer
                    parent.model.selActiveTeamerUid = model.uid;
                    parent.Refresh();
                }
            });
        }
        public override void OnShow()
        {
            model.uid = param.uid;
            model.index = param.index;
            Refresh();
        }
        public void Refresh()
        {
            view.sta_exist.ChangeState(model.uid == 0 ? 0 : 1);
            view.sta_.ChangeState(model.uid == parent.model.selActiveTeamerUid && model.uid != 0 ? 1 : 0);
            if (model.uid != 0)
            {
                var data = CharacterProductForm.DataByUid.GetDv(model.uid, null);
                if (data != null)
                {
                    view.txt_.text = data.name;
                    view.img_.BindTexData(TexAssetForm.DataById[data.avatarTex]);
                }
            }
        }
    }

    public partial class UiTeamerParam
    {
        public int uid;
    }
    public partial class UiTeamerModel
    {
        public int uid;
    }
    public partial class UiTeamerCtrl
    {

        public override void OnCreate()
        {
            view.btn_.onClick.AddListener(() =>
            {
                if (model.uid == 0) return;
                if (parent.model.selActiveTeamerUid != 0)
                {
                    // 选中了ActiveTeamer，点击Teamer替换
                    PlayManager.instance.infoCtrl.ReplaceTeamer(parent.model.selActiveTeamerUid, model.uid);
                    parent.model.selActiveTeamerUid = 0;
                    parent.Refresh();
                }
                else
                {
                    // 没有选中ActiveTeamer，选中当前Teamer
                    parent.model.selTeamerUid = model.uid;
                    parent.Refresh();
                }
            });
        }
        public override void OnShow()
        {
            model.uid = param.uid;
            Refresh();
        }
        public void Refresh()
        {
            view.sta_exist.ChangeState(model.uid == 0 ? 0 : 1);
            view.sta_.ChangeState(model.uid == parent.model.selTeamerUid && model.uid != 0 ? 1 : 0);
            if (model.uid != 0)
            {
                var data = CharacterProductForm.DataByUid.GetDv(model.uid, null);
                if (data != null)
                {
                    view.txt_.text = data.name;
                    view.img_.BindTexData(TexAssetForm.DataById[data.avatarTex]);
                }
            }
        }
    }

}
