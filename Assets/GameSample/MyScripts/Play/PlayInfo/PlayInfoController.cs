using Form;
using Microsoft.Win32;
using System.Collections;
using System.Collections.Generic;
using Ui;
using Ui.ModSceneUnit;
using Ui.PlaySceneMain;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using Z_DataSystem;
using Z_DataSystem.Form;
using Z_Debug;
using Z_DesignStyle;
using Z_Input;
using Z_Map;
using Z_Map.Form;
using Z_Text;
using Z_Time;
using Z_Ui;
using Z_Ui.Notify;
using Z_UnitSystem;
public enum StoryItemEventType
{
    Add,
    Remove
}
public class StoryItemEvent : Z_Event
{
    public StoryItemEventType type;
    public ItemProductForm.Data data;
}
public enum StoryCharacterEventType
{
    ParamChange,
}
public class StoryCharacterEvent : Z_Event
{
    public StoryCharacterEventType type;
    public string name;
    public CharacterProductForm.Data data;
}

public interface InternalPlayInfoController
{
    public void Begin();
    public void End();

    public void Update();


}
public interface ExternalPlayInfoController
{
}
public class PlayInfoController : Z_Controller<PlayManager>, InternalPlayInfoController, ExternalPlayInfoController, IZ_Listener<CollideEvent>
{
    Dictionary<string, List<int>> bagName2UidDic;
    public PlayInfoController(PlayManager super) : base(super)
    {
        Z_EventHelper.Register<CollideEvent>(this);
        bagName2UidDic = new Dictionary<string, List<int>>();
    }

    private bool enable;
    #region internal Var

    #endregion

    #region extern Var


    #endregion


    public void Begin()
    {
        enable = true;
        foreach (var uid in GameManager.instance.curProgress.bag)
        {
            GainItem(uid, false, false);
        }
    }
    public void End()
    {
        enable = false;
    }

    public void Update()
    {
        if (!enable)
            return;
        int oldSecond = (int)GameManager.instance.curProgress.seconds;
        GameManager.instance.curProgress.seconds += Time.deltaTime;
        if (oldSecond < (int)GameManager.instance.curProgress.seconds)
        {
            Z_EventHelper.Invoke(new StoryLifeEvent() { type = StoryLifeEventType.EverySecond });
        }
    }

    public void ChangeParam(int characterUid,string name, float value)
    {
       var data= CharacterProductForm.DataByUid.GetDv(characterUid, null);
        if(data!=null)
        {
            var prm = data.paramDic.GetDv(name, null);
            if(prm!=null)
            {
                prm.v = Mathf.Min(Mathf.Max(prm.v, prm.min), prm.max);
                Z_EventHelper.Invoke(new StoryCharacterEvent() { type = StoryCharacterEventType.ParamChange, data = data, name=name });

            }
        }
    }

    public void GainItem(int uid, bool toast = true, bool message = true)
    {
        var newItem = ItemProductForm.DataByUid[uid].Copy(false);
        newItem.ToProduct();
        GainItem(newItem, toast, message);
    }
    public void GainItem(ItemProductForm.Data data, bool toast = true, bool message = true)
    {
        var content = TextManager.instance.GetTxt("gain") + " " + data.name + " x" + data.amount;
        if (toast)
        {
            NotifyManager.instance.AddTip(content);
        }
        if (message)
        {
            _super.sceneCtrl.AddMessage(content);
        }

        Z_EventHelper.Invoke(new StoryItemEvent() { type = StoryItemEventType.Add, data = data });

        if (!bagName2UidDic.ContainsKey(data.name))
            bagName2UidDic[data.name] = new List<int>();

        foreach (var uid in bagName2UidDic[data.name])
        {
            var old = ItemProductForm.DataByUid[uid];
            if (old.amount < old.maxAmountPer)
            {
                int addition = Mathf.Min(old.maxAmountPer - old.amount, data.amount);
                data.amount -= addition;
                old.amount += addition;
            }
        }
        if (data.amount == 0)
        {
            data.DestroyProduct();
            return;
        }

        bagName2UidDic[data.name].Add(data.uid);
        GameManager.instance.curProgress.bag.Add(data.uid);

    }

    public void OnEvent(CollideEvent evt)
    {
        if (!enable)
            return;
        switch (evt.type)
        {
            case CollideEventType.TriggerEnter:
                if (evt.b == _super.sceneCtrl.playerM.unit && evt.a is ItemUnit obj)
                {
                    if (obj.productInfo.Item1 > 0)
                    {
                        GainItem(obj.productInfo.Item1);
                        MapManager.instance.RemoveItem(obj.data);
                    }
                }
                break;


        }

    }
}
