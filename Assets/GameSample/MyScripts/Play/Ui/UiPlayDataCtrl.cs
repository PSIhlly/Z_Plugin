using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z_Ui.Base;
using Z_Texture;

namespace Ui.PlayData
{

    public partial class UiPlayDataParam
    {

        public int selPage;
    }
    public partial class UiPlayDataModel
    {

        public int selPage;
    }
    public partial class UiPlayDataCtrl
    {

        public override void OnCreate()
        {

            view.btn_back.onClick.AddListener(() =>
            {
                Close();
            });
            view.btn_backpack.onClick.AddListener(() =>
            {
                model.selPage = 1;
                Refresh();
            });
            view.btn_character.onClick.AddListener(() =>
            {
                model.selPage = 2;
                Refresh();
            });

        }
        public override void OnShow()
        {
            model.selPage = 1;
            if (param != null)
                model.selPage = param.selPage;
            Refresh();
        }
        public void Refresh()
        {

            view.page_PlayDataBackpack.SetShow(model.selPage == 1);
            view.sta_backpack.ChangeState(model.selPage == 1 ? 1 : 0);
            view.page_PlayDataCharacter.SetShow(model.selPage == 2);
            view.sta_character.ChangeState(model.selPage == 2 ? 1 : 0);

        }
    }

}


