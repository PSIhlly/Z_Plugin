

using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z_Ui.Base;
using Z_Texture;
using UnityEngine;
using Z_DataSystem.Form;
using Z_Text;
using Z_Ui.Notify;

namespace Ui.ModStory.ModStoryParameter.ModStoryConfig
{

    public partial class UiModStoryConfigParam
    {

    }
    public partial class UiModStoryConfigModel
    {

    }
    public partial class UiModStoryConfigCtrl
    {

        public override void OnCreate()
        {
            view.btn_mainCharacter.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.ChooseCharacter(TextManager.instance.GetTxt("Choose main character"), (item) =>
                {
                    GameManager.instance.curConfig.mainCharacterName = item.content;
                    Refresh();
                });
            });

        }
        public override void OnShow()
        {

            Refresh();
        }

        public void Refresh()
        {

            view.txt_mainCharacter.text = GameManager.instance.curConfig.mainCharacterName;
        }
    }

}