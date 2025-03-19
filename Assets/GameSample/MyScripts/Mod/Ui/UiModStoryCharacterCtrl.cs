using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Form;
using Item;
using Z_Texture;
using Z_Ui.Base;
using Z_Ui;
using Z_Map;

namespace Ui.ModStoryCharacter
{
    public partial class UiModStoryCharacterModel
    {
        public int select;

    }
    public partial class UiModStoryCharacterCtrl
    {
        public override void OnCreate()
        {
            view.btn_back.onClick.AddListener(() =>
            {
                Close();
            });
            view.btn_args.onClick.AddListener(() =>
            {
                model.select = 0;
                Refresh();
            });
            view.btn_list.onClick.AddListener(() =>
            {
                model.select = 1;
                Refresh();
            });
        }
        public override void OnShow()
        {
            Refresh();
        }
        public void Refresh()
        {
            view.sta_args.ChangeState(model.select == 0 ? 1 : 0);
            view.sta_list.ChangeState(model.select == 1 ? 1 : 0);
            view.sub_ModStoryCharacterArguments.SetActive(model.select == 0);
            view.sub_ModStoryCharacterList.SetActive(model.select == 1);
        }
    }

}

