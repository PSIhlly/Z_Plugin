

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
                List<(string,Sprite)> lst = new List<(string, Sprite)>();
                if (CharacterProductForm.DatasByIsproto.ContainsKey(true))
                {
                    foreach (var data in CharacterProductForm.DatasByIsproto[true])
                    {
                        lst.Add((data.name, TexAssetForm.DataByName[data.avatarTexName].sprite));
                    }
                }
                NotifyManager.instance.AddChoose(TextManager.instance.GetTxt("Choose main character"),
                    true, (id) =>
                    {
                        GameManager.instance.curConfig.mainCharacterName = lst[id].Item1;
                        Refresh();
                        return true;
                    }, lst);
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