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
using Z_Code.Form;
using static UnityEditor.Progress;
using UnityEditor.DeviceSimulation;
using Z_DesignStyle;

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
                ModManager.instance.assetCtrl.ChooseCharacterParam(TextManager.instance.GetTxt("Choose Hp param"), (item) =>
                {
                    model.data.hpParamName = item.content;
                    Refresh();
                });
            });
            view.btn_moveSpeedParameter.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.ChooseCharacterParam(TextManager.instance.GetTxt("Choose Speed param"), (item) =>
                {
                    model.data.speedParamName = item.content;
                    Refresh();
                });
            });
            view.btn_idleAnim.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.ChooseCharacterAnim(model.data.animDic, TextManager.instance.GetTxt("Choose Idle anim"), (item) =>
                {
                    model.data.idleAnimName = item.content;
                    Refresh();
                });
            });
            view.btn_moveAnim.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.ChooseCharacterAnim(model.data.animDic, TextManager.instance.GetTxt("Choose Idle anim"), (item) =>
                {
                    model.data.moveAnimName = item.content;
                    Refresh();
                });
            });
            view.btn_unique.onClick.AddListener(() =>
            {
                model.data.unique = !model.data.unique;
                Refresh();
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
            view.txt_moveSpeedParameter.text = model.data.speedParamName;
            view.txt_idleAnim.text = model.data.idleAnimName;
            view.txt_moveAnim.text = model.data.moveAnimName;

            view.sta_unique.ChangeState(model.data.unique ? 1 : 0);

            view.model_EventChooseCharacterTouch.Set(new EventChoose.UiEventChooseParam() { dic = model.data.events, key = "onCharacterTouchEvent"});
            view.model_EventChooseCharacterLeave.Set(new EventChoose.UiEventChooseParam() { dic = model.data.events, key = "onCharacterLeaveEvent"});
            view.model_EventChooseObjectTouch.Set(new EventChoose.UiEventChooseParam() { dic = model.data.events, key = "onObjectTouchEvent"});
            view.model_EventChooseObjectLeave.Set(new EventChoose.UiEventChooseParam() { dic = model.data.events, key = "onObjectLeaveEvent" });
            view.model_EventChooseShow.Set(new EventChoose.UiEventChooseParam() { dic = model.data.events, key = "onShowEvent" });

        }
    }

}