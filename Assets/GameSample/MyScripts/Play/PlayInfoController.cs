using Form;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using TuanjieMuse.Chat.ViewModel;
using Ui;
using Ui.ModSceneUnit;
using Ui.PlaySceneMain;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using Z_Code;
using Z_Code.Form;
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
using static UnityEditor.Progress;
public enum StoryItemEventType
{
    Add,
    Remove,
    Use
}
public class StoryItemEvent : Z_Event
{
    public StoryItemEventType type;
    public ItemProductForm.Data data;
}
public enum StoryCharacterEventType
{
    ParamChange,
    SkillParamChange,
    ChangeCharacter
}
public class StoryCharacterEvent : Z_Event
{
    public StoryCharacterEventType type;
    public string name;
    public CharacterProductForm.Data data;
    public object obj;
}


public interface InternalPlayInfoController
{
    public void Begin();
    public void End();

    public void Update();


}
public interface ExternalPlayInfoController
{
    public void GainItem(int uid, int amount, bool toast = true, bool message = true);
    public void LostItem(int uid, int amount, bool toast = true, bool message = true);
    public void UseItem(int uid, int amount, bool toast = true, bool message = true);
    public void ChangeCharacterParam(int characterUid, string name, object value);
    public void ChangeSkillParam(int skillUid, string name, object value);
    public void Equip(int characterUid, int itemUid, EquipPartType part);
    public void Unequip(int characterUid, EquipPartType part);
    public void ChooseTeamCharacter(string title, Action<CharacterProductForm.Data> act);
    public void ChooseEquipItems(string title, Action<ItemProductForm.Data> act, EquipPartType part);

    public CharacterProductForm.Data GetTeamEquipedCharacter(int ItemProductUid, out EquipPartType partType);
    public void UseSkill(int characterUid, int skillUid,bool ignoreCd);

    public void ChooseCurrentCharacter(int characterUid);
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
        bagName2UidDic.Clear();
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

        Z_Map.DynamicGlobalSettings.pauseNav = GameManager.instance.curProgress.blockProgramUid == 0 ? false : true;
        if (GameManager.instance.curProgress.blockProgramUid != 0)
        {
            return;
        }
        int oldSecond = (int)GameManager.instance.curProgress.seconds;
        GameManager.instance.curProgress.seconds += Time.deltaTime;
        if (oldSecond < (int)GameManager.instance.curProgress.seconds)
        {
            Z_EventHelper.Invoke(new StoryLifeEvent() { type = StoryLifeEventType.EverySecond });
        }
    }
    #region param
    public void ChangeCharacterParam(int characterUid, string name, object value)
    {
        var data = CharacterProductForm.DataByUid.GetDv(characterUid, null);
        if (data != null)
        {
            var prm = data.paramDic.GetDv(name, null);
            float oldValue = prm.GetValue().num;
            if (prm != null)
            {
                if (value is float num)
                {
                    var min = prm.GetMin().num;
                    var max = prm.GetMax().num;
                    prm.SetValue(Mathf.Min(Mathf.Max(num, min), max));
                }
                else
                {
                    prm.SetValue(value);
                }
                Z_EventHelper.Invoke(new StoryCharacterEvent() { type = StoryCharacterEventType.ParamChange, data = data, name = name, obj = (prm.GetValue().num - oldValue) });
            }
            else
            {

                Debug.LogError("未找到" + name);
                data.paramDic[name] = new CharacterParamForm.Data(-1, name, 0, "", "", "", ParamShowType.Hide);
                data.paramDic[name].SetValue(value);

            }
        }


    }
    #endregion

    #region skill

    public void UseSkill(int characterUid, int skillUid,bool ignoreCd)
    {
        var character = CharacterProductForm.DataByUid[characterUid];
        var skill = SkillProductForm.DataByUid[skillUid];
        if (ignoreCd||(skill.lastUseTime == 0 || skill.lastUseTime + skill.cd < GameManager.instance.curProgress.seconds))
        {
            skill.lastUseTime = GameManager.instance.curProgress.seconds;
            Z_EventHelper.Invoke(new CharacterSkillEvent()
            {
                data = character,
                skillUid = skill.uid
            });
        }
    }

    public void ChangeSkillParam(int skillUid, string name, object value)
    {
        var data = SkillProductForm.DataByUid.GetDv(skillUid, null);
        if (data != null)
        {
            var prm = data.paramDic.GetDv(name, null);
            float oldValue = prm.GetValue().num;
            if (prm != null)
            {
                if (value is float num)
                {
                    var min = prm.GetMin().num;
                    var max = prm.GetMax().num;
                    prm.SetValue(Mathf.Min(Mathf.Max(num, min), max));
                }
                else
                {
                    prm.SetValue(value);
                }
                Z_EventHelper.Invoke(new StoryCharacterEvent() { type = StoryCharacterEventType.SkillParamChange, data = CharacterProductForm.DataByUid[data.characterUid], name = skillUid.ToString(), obj = (prm.GetValue().num - oldValue) });
            }
            else
            {
                Debug.LogError("未找到" + name);
                data.paramDic[name] = new SkillParamForm.Data(-1, name, 0, "", "", "", ParamShowType.Hide);
                data.paramDic[name].SetValue(value);
            }
        }
    }


    #endregion



    #region item
    public void UseItem(int uid, int amount, bool toast = true, bool message = true)
    {
        var item = ItemProductForm.DataByUid.GetDv(uid, null);
        if (bagName2UidDic.ContainsKey(item.name))
        {
            ItemProductForm.Data res = null;
            if (item.isConsume)
            {
                res = RemoveItemInternal(uid, amount);
            }
            else
            {
                res = item;
            }
            var content = TextManager.instance.GetTxt("use") + " " + res.name + " x" + res.amount;
            if (toast)
            {
                NotifyManager.instance.AddTip(content);
            }
            if (message)
            {
                _super.sceneCtrl.AddMessage(content);
            }
            Z_EventHelper.Invoke(new StoryItemEvent() { type = StoryItemEventType.Use, data = res });
        }


    }
    private ItemProductForm.Data RemoveItemInternal(int uid, int amount)
    {
        var item = ItemProductForm.DataByUid.GetDv(uid, null);
        item = item.Copy();
        item.amount = amount;

        if (!bagName2UidDic.ContainsKey(item.name))
            bagName2UidDic[item.name] = new List<int>();
        foreach (var mineUid in bagName2UidDic[item.name])
        {
            var old = ItemProductForm.DataByUid[mineUid];
            var lost = Mathf.Min(old.amount, item.amount);
            item.amount -= lost;
            old.amount -= lost;

            if (old.amount <= 0)
            {
                bagName2UidDic[item.name].Remove(old.uid);
                GameManager.instance.curProgress.bag.Remove(old.uid);
                old.DestroyProduct();
            }
        }
        item.amount = (amount - item.amount);
        return item;
    }
    public void LostItem(int uid, int amount, bool toast = true, bool message = true)
    {
        var item = RemoveItemInternal(uid, amount);
        var content = TextManager.instance.GetTxt("lost") + " " + item.name + " x" + item.amount;
        if (toast)
        {
            NotifyManager.instance.AddTip(content);
        }
        if (message)
        {
            _super.sceneCtrl.AddMessage(content);
        }
        Z_EventHelper.Invoke(new StoryItemEvent() { type = StoryItemEventType.Remove, data = item });
    }
    public void LostItem(string name, int amount, bool toast = true, bool message = true)
    {
        var item = ItemProductForm.DataByNameProtouid.GetDv((name, 0), null);
        if (item == null)
        {
            return;
        }
        LostItem(item.uid, amount, toast, message);
    }
    public void GainItem(int uid, int amount, bool toast = true, bool message = true)
    {
        var newItem = ItemProductForm.DataByUid.GetDv(uid, null);

        if (newItem == null)
            return;
        newItem = newItem.Copy(false);
        newItem.ToProduct(uid);
        newItem.amount = amount;
        GainItem(newItem, toast, message);
    }
    public void GainItem(string name, int amount, bool toast = true, bool message = true)
    {
        var newItem = ItemProductForm.DataByNameProtouid.GetDv((name, 0), null);
        if (newItem == null)
        {
            return;
        }
        GainItem(newItem.uid, amount, toast, message);
    }
    public void GainItem(int uid, bool toast = true, bool message = true)
    {
        var newItem = ItemProductForm.DataByUid[uid];
        if (newItem.protoUid == 0)
        {
            int protoUid = newItem.uid;
            newItem = newItem.Copy(false);
            newItem.ToProduct(protoUid);
        }
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


        if (!bagName2UidDic.ContainsKey(data.name))
            bagName2UidDic[data.name] = new List<int>();
        if (GameManager.instance.curProgress.bag.Contains(data.uid))
        {
            bagName2UidDic[data.name].Add(data.uid);
        }
        else
        {
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
            while (data.amount > 0)
            {
                var newData = data.Copy(false);
                newData.ToProduct(data.protoUid);
                newData.amount = Mathf.Min(newData.maxAmountPer, data.amount);
                data.amount -= newData.amount;

                bagName2UidDic[newData.name].Add(newData.uid);
                GameManager.instance.curProgress.bag.Add(newData.uid);
            }

            data.DestroyProduct();

            Z_EventHelper.Invoke(new StoryItemEvent() { type = StoryItemEventType.Add, data = data });
        }


    }
    #endregion

    #region equip
    public void Equip(int characterUid, int itemUid, EquipPartType part)
    {
        var ch = CharacterProductForm.DataByUid.GetDv(characterUid, null);
        var item = ItemProductForm.DataByUid.GetDv(itemUid, null);
        if (ch != null && ItemProductForm.DataByUid.ContainsKey(itemUid) && part != EquipPartType.None && item.equip == part)
        {
            var oldCh = GetTeamEquipedCharacter(itemUid, out var oldPart);
            if (oldCh != null)
            {
                Unequip(oldCh.uid, oldPart);
            }
            Unequip(characterUid, part);
            ch.equips[part] = itemUid;
        }
    }
    public void Unequip(int characterUid, EquipPartType part)
    {
        var ch = CharacterProductForm.DataByUid.GetDv(characterUid, null);
        if (ch != null)
        {
            ch.equips[part] = 0;
        }
    }
    public void ChooseEquipItems(string title, Action<ItemProductForm.Data> act, EquipPartType part)
    {
        var items = new EntryItem();
        foreach (var uid in GameManager.instance.curProgress.bag)
        {
            var it = ItemProductForm.DataByUid[uid];
            if (it.canEquipe && it.equip == part)
            {
                items.Add(it.name, StoryTexAssetForm.DataByName.GetDv(it.iconTexName, StoryTexAssetForm.defaultData).GetSprite(), uid);
            }
        }
        NotifyManager.instance.AddChoose(title,
            true, (item) =>
            {
                var it = ItemProductForm.DataByUid[item.id];
                act?.Invoke(it);
                return true;
            }, items);
    }
    public CharacterProductForm.Data GetTeamEquipedCharacter(int ItemProductUid, out EquipPartType partType)
    {
        partType = default;
        var progress = GameManager.instance.curProgress;
        if (progress == null || progress.team == null)
            return null;

        foreach (var characterUid in progress.team)
        {
            var characterData = CharacterProductForm.DataByUid.GetDv(characterUid, null);
            if (characterData == null || characterData.equips == null)
                continue;

            foreach (var kvp in characterData.equips)
            {
                if (kvp.Value == ItemProductUid)
                {
                    partType = kvp.Key;
                    return characterData;
                }
            }
        }

        return null;
    }

    #endregion

    #region team

    public void ChooseCurrentCharacter(int characterUid)
    {
        var ch = CharacterProductForm.DataByUid.GetDv(characterUid, null);
        if (ch != null && GameManager.instance.curProgress.team.Contains(ch.uid))
        {
            GameManager.instance.curProgress.characterUid = ch.uid;
            _super.sceneCtrl.RefreshCurrentCharacter();
            Z_EventHelper.Invoke(new StoryCharacterEvent()
            {
                type = StoryCharacterEventType.ChangeCharacter,
                data = ch
            });
        }
    }

    public void ChooseTeamCharacter(string title, Action<CharacterProductForm.Data> act)
    {
        var items = new EntryItem();
        foreach (var uid in GameManager.instance.curProgress.team)
        {
            var ch = CharacterProductForm.DataByUid[uid];
            items.Add(ch.name, StoryTexAssetForm.DataByName.GetDv(ch.avatarTexName, StoryTexAssetForm.defaultData).GetSprite(), uid);
        }
        NotifyManager.instance.AddChoose(title,
            true, (item) =>
            {
                var ch = CharacterProductForm.DataByUid[item.id];
                act?.Invoke(ch);
                return true;
            }, items);
    }
    #endregion

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
