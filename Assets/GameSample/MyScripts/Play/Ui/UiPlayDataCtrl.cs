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
                model.selPage = 0;
            });
            view.btn_character.onClick.AddListener(() =>
            {
                model.selPage = 1;
            });

        }
        public override void OnShow()
        {
            model.selPage = 0;
            if (param != null)
                model.selPage = param.selPage;
            Refresh();
        }
        public void Refresh()
        {

            view.page_PlayDataBackpack.SetActive(model.selPage == 0);
        }
    }

}


