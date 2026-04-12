using Form;
using System.Collections;
using System.Collections.Generic;
using Ui.ModSceneMenu;
using Ui.PlaySceneMenu;
using UnityEngine;
using UnityEngine.UI;
using Z_Map;
using Z_ObjectAnimator.Base;
using Z_ObjectAnimator.Core;
using Z_Texture;
using Z_Time;
using Z_Ui;
using Z_Ui.Base;

namespace Ui.PlaySceneMain.PlaySceneMessage
{
    public partial class UiPlaySceneMessageModel
    {
        public int idTot;
        public List<UiMessageParam> subs=new List<UiMessageParam>();
    }
    public partial class UiPlaySceneMessageCtrl
    {
        UiContainer<UiMessageCtrl> con;
        public override void OnCreate()
        {
            con = new UiContainer<UiMessageCtrl>(this, view.go_message);

        }
        public void AddMessage(string content)
        {
            if (model.idTot > 100000)
                model.idTot = 0;
            model.idTot++;
            model.subs.Add(new UiMessageParam()
            {
                content = content,
                id = model.idTot,
                time=Time.time+10
            }) ;
            Refresh();
        }
        public void RemoveMessage(int id)
        {
            for(int i=0;i< model.subs.Count;i++)
            {
                if(model.subs[i].id==id)
                {
                    model.subs.RemoveAt(i);
                    break;
                }
            }
            Refresh();
        }
        public void Refresh()
        {
            con.Clear();
            foreach(var prm in model.subs)
            {
                con.Add(prm);
            }
            con.Refresh();
        }
    }
    public partial class UiMessageParam
    {
        public float time;
        public int id;
        public string content;
    }
    public partial class UiMessageModel
    {
        public float time;
        public int id;
        public string content;
        public Timer removeTimer;
    }
    public partial class UiMessageCtrl
    {
        public override void OnShow()
        {
            if(param!=null)
            {
                model.time = param.time;
                model.id = param.id;
                model.content = param.content;
            }
            Refresh();
        }
       

        public void Refresh()
        {
            view.txt_.text = model.content;
            TimeManager.instance.CancelTimer(model.removeTimer);
            model.removeTimer = TimeManager.instance.StartTimer(model.time - Time.time, 0, () =>
            {
                parent.RemoveMessage(model.id);
                return true;
            }, uiHolder);
            TimeManager.instance.AddNextBigFrameAction(() =>
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(view.txt_.rectTransform);
                LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
            }, gameObject);
        }
    }

}
