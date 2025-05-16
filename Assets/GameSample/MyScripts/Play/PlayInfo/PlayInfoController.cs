using Form;
using Microsoft.Win32;
using System.Collections;
using System.Collections.Generic;
using Ui;
using Ui.ModSceneUnit;
using Ui.ModStory;
using Ui.PlaySceneMain;
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
        foreach(var uid in _super.data.progress.bag)
        {
            GainItem(ItemProductForm.DataByUid[uid].name,false);
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

    }
    public void GainItem(string itemName, bool toast = true)
    {
        var newItem=ItemProductForm.DataByNameIsproto[(itemName, true)].Copy();
        newItem.ToProduct();
        GainItem(newItem, toast);
    }
    public void GainItem(ItemProductForm.Data data, bool toast = true)
    {
        if (!bagName2UidDic.ContainsKey(data.name))
            bagName2UidDic[data.name] = new List<int>();

        bagName2UidDic[data.name].Add(data.uid);
        if (toast)
        {
            NotifyManager.instance.AddTip(TextManager.instance.GetTxt("gain") + " " + data.name + " x" + data.amount);
        }
    }

    public void OnEvent(CollideEvent evt)
    {
        if (!enable)
            return;
        switch (evt.type)
        {
            case CollideEventType.TriggerEnter:
                if (evt.b == _super.sceneCtrl.playerM.unit.ins && evt.a is ObjectInstance obj)
                {
                    if (!string.IsNullOrEmpty(obj.unit.productInfo.Item1))
                    {
                        GainItem(obj.unit.productInfo.Item1);
                        MapManager.instance.RemoveObject(obj.unit.data);
                    }
                }
                break;


        }

    }
}
