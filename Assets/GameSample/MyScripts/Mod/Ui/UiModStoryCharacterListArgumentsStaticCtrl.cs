

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

namespace Ui.ModStoryCharacterListArguments.ModStoryCharacterListArgumentsStatic
{

    public partial class UiModStoryCharacterListArgumentsStaticParam
    {
        public CharacterProductForm.Data data;
    }
    public partial class UiModStoryCharacterListArgumentsStaticModel
    {
        public CharacterProductForm.Data data;
    }
    public partial class UiModStoryCharacterListArgumentsStaticCtrl
    {

        public override void OnCreate()
        {

            view.btn_hpArgument.onClick.AddListener(() =>
            {
                List<string> lst = new List<string>();
                List<Sprite> spriteLst = new List<Sprite>();
                foreach (var data in CharacterParamForm.DataByName.Values)
                {
                    lst.Add(data.name);
                    spriteLst.Add(TextureHelper.transparentSprite);
                }
                NotifyManager.instance.AddChoose(TextManager.instance.GetTxt("Choose Hp Param"),
                    true, (id) =>
                    {
                        model.data.hpParamName = lst[id];
                        Refresh();
                        return true;
                    }, lst, spriteLst);
            });
            view.btn_speedArgument.onClick.AddListener(() =>
            {
                List<string> lst = new List<string>();
                List<Sprite> spriteLst = new List<Sprite>();
                foreach (var data in CharacterParamForm.DataByName.Values)
                {
                    lst.Add(data.name);
                    spriteLst.Add(TextureHelper.transparentSprite);
                }
                NotifyManager.instance.AddChoose(TextManager.instance.GetTxt("Choose Speed Param"),
                    true, (id) =>
                    {
                        model.data.speedParamName = lst[id];
                        Refresh();
                        return true;
                    }, lst, spriteLst);
            });
            view.btn_idleAnim.onClick.AddListener(() =>
            {
                List<string> lst = new List<string>();
                List<Sprite> spriteLst = new List<Sprite>();
                for (int i=0;i< model.data.animJo.Count;i++)
                {
                    var anim = model.data.GetCharacterAnim(i);
                    lst.Add(anim.name);
                    spriteLst.Add(TextureHelper.transparentSprite);
                }
                NotifyManager.instance.AddChoose(TextManager.instance.GetTxt("Choose Idle anim"),
                    true, (id) =>
                    {
                        model.data.idleAnimName = lst[id];
                        Refresh();
                        return true;
                    }, lst, spriteLst);
            });
            view.btn_moveAnim.onClick.AddListener(() =>
            {
                List<string> lst = new List<string>();
                List<Sprite> spriteLst = new List<Sprite>();
                for (int i = 0; i < model.data.animJo.Count; i++)
                {
                    var anim = model.data.GetCharacterAnim(i);
                    lst.Add(anim.name);
                    spriteLst.Add(TextureHelper.transparentSprite);
                }
                NotifyManager.instance.AddChoose(TextManager.instance.GetTxt("Choose Move anim"),
                    true, (id) =>
                    {
                        model.data.moveAnimName = lst[id];
                        Refresh();
                        return true;
                    }, lst, spriteLst);
            });

        }
        public override void OnShow()
        {
            model.data = param.data;
            Refresh();
        }
        public void Refresh()
        {
            view.txt_hpArgument.text = model.data.hpParamName;
            view.txt_speedArgument.text = model.data.speedParamName;
            view.txt_idleAnim.text = model.data.idleAnimName;
            view.txt_moveAnim.text = model.data.moveAnimName;
        }
    }

}