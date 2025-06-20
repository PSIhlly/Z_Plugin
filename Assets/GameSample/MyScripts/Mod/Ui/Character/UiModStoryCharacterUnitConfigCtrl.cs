using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z_Ui.Base;
using Z_Texture;
using UnityEngine;
using Z_Text;
using Z_Ui.Notify;

namespace Ui.ModStory.ModStoryCharacter.ModStoryCharacterUnit.ModStoryCharacterUnitConfig
{

    public partial class UiModStoryCharacterUnitConfigParam
    {
        public CharacterProductForm.Data data;
    }
    public partial class UiModStoryCharacterUnitConfigModel
    {

        public CharacterProductForm.Data data;
    }
    public partial class UiModStoryCharacterUnitConfigCtrl
    {

        public override void OnCreate()
        {

            view.btn_hpArgument.onClick.AddListener(() =>
            {
                List<string> lst = new List<string>();
                foreach (var data in CharacterParamForm.DataByName.Values)
                {
                    lst.Add(data.name);
                }
                NotifyManager.instance.AddChoose(TextManager.instance.GetTxt("Choose Hp param"),
                    true, (id) =>
                    {
                        model.data.hpParamName = lst[id];
                        Refresh();
                        return true;
                    }, lst);
            });
            view.btn_moveSpeedParameter.onClick.AddListener(() =>
            {
                List<string> lst = new List<string>();
                foreach (var data in CharacterParamForm.DataByName.Values)
                {
                    lst.Add(data.name);
                }
                NotifyManager.instance.AddChoose(TextManager.instance.GetTxt("Choose Speed param"),
                    true, (id) =>
                    {
                        model.data.hpParamName = lst[id];
                        Refresh();
                        return true;
                    }, lst);
            });
            view.btn_idleAnim.onClick.AddListener(() =>
            {
                List<string> lst = new List<string>(model.data.animDic.Keys);
                NotifyManager.instance.AddChoose(TextManager.instance.GetTxt("Choose Idle anim"),
                    true, (id) =>
                    {
                        model.data.hpParamName = lst[id];
                        Refresh();
                        return true;
                    }, lst);
            });
            view.btn_moveAnim.onClick.AddListener(() =>
            {
                List<string> lst = new List<string>(model.data.animDic.Keys);
                NotifyManager.instance.AddChoose(TextManager.instance.GetTxt("Choose Move anim"),
                    true, (id) =>
                    {
                        model.data.hpParamName = lst[id];
                        Refresh();
                        return true;
                    }, lst);
            });
            view.btn_onTouchEvent.onClick.AddListener(() =>
            {

            });
            view.btn_onLeaveEvent.onClick.AddListener(() =>
            {

            });
            view.btn_onShowEvent.onClick.AddListener(() =>
            {

            });

        }
        public override void OnShow()
        {

            Refresh();
        }
        public void Refresh()
        {

            view.txt_hpArgument.text = model.data.hpParamName;
            view.txt_moveSpeedParameter.text = model.data.speedParamName;
            view.txt_idleAnim.text = model.data.idleAnimName;
            view.txt_moveAnim.text = model.data.moveAnimName;
            view.txt_onTouchEvent.text = "";
            view.txt_onLeaveEvent.text = "";
            view.txt_onShowEvent.text = "";
        }
    }

}