using Form;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting.FullSerializer;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TextCore.Text;
using Z_Code.Form;
using Z_DataSystem;
using Z_DataSystem.Form;
using Z_DesignStyle;
using Z_Map;
using Z_String;
using Z_Text;
using Z_Ui.Notify;
using static UnityEditor.Progress;
using static UnityEngine.Rendering.DebugUI.MessageBox;
namespace Form
{
    public static partial class StoryTexAssetForm
    {
        public static void AddData(TexAssetForm.Data data)
        {
            AddData((Data)data);
        }
    }
}

public class ModAssetCtrl : Z_Controller<ModManager>
{
    public ModAssetCtrl(ModManager super) : base(super)
    {

    }
    public int modId;



    #region story
    public void ImportStoryTex()
    {
        AssetManager.instance.texCtrl.Select(new Vector2Int(100, 100), (form) =>
        {
            GameManager.instance.curStory.icon = form.name;
            GameManager.instance.saveCtrl.AddTex(form);
        });
    }
   

    #endregion

    #region param

    public void CreateCharacterArg(string name)
    {
        if (CharacterParamForm.DataByName.Keys.Count > GlobalSettings.CHARACTER_PARAM_MAX)
            return;

        if (string.IsNullOrEmpty(name))
        {
            name = StringHelper.GetUniqueName(CharacterParamForm.DataByName.Keys);
        }

        CharacterParamForm.AddData(new CharacterParamForm.Data(-1, name, 0, 0f, 0f, 1f, 0, default));
        foreach (var character in CharacterProductForm.DataByUid.Values)
        {
            character.paramDic[name] = new CharacterParamForm.Data(-1, name, 0, 0f, 0f, 1f, 0, default);
        }
    }
    public void DeleteCharacterArg(string name)
    {
        CharacterParamForm.RemoveData(CharacterParamForm.DataByName[name].uid);
        foreach (var character in CharacterProductForm.DataByUid.Values)
        {
            character.paramDic.Remove(name);
        }
    }


    public void CreateItemArg(string name)
    {
        if (ItemParamForm.DataByName.Keys.Count > GlobalSettings.ITEM_PARAM_MAX)
            return;

        if (string.IsNullOrEmpty(name))
        {
            name = StringHelper.GetUniqueName(ItemParamForm.DataByName.Keys);
        }

        ItemParamForm.AddData(new ItemParamForm.Data(-1, name, 0, 0f, 0f, 1f, 0, default));
        foreach (var item in ItemProductForm.DataByUid.Values)
        {
            item.paramDic[name] = new ItemParamForm.Data(-1, name, 0, 0f, 0f, 1f, 0, default);
        }
    }
    public void DeleteItemArg(string name)
    {
        ItemParamForm.RemoveData(ItemParamForm.DataByName[name].uid);
        foreach (var item in ItemProductForm.DataByUid.Values)
        {
            item.paramDic.Remove(name);
        }
    }



    #endregion


    #region texture

    public void CreateTex(string lab = "", string name = null)
    {
        if (string.IsNullOrEmpty(name))
        {
            name = StringHelper.GetUniqueName(MapTextureForm.DataByName.Keys);
        }
        MapTextureForm.AddData(new MapTextureForm.Data(-1, name, GlobalNameHelper.GetDefaultTexName(), 1, new List<string>() { GlobalNameHelper.GetDefaultTexName() }, lab, new Dictionary<string, EventTriggerForm.Data>()));
    }
    public void ImportTex(string name, int id = -1)
    {
        AssetManager.instance.texCtrl.Select(new Vector2Int(100, 100), (form) =>
         {
             if (id != -1 && MapTextureForm.DataByName[name].texsName.Count > id)
             {
                 MapTextureForm.DataByName[name].texsName[id] = form.name;
             }
             else
             {
                 MapTextureForm.DataByName[name].texsName.Add(form.name);
             }
             GameManager.instance.saveCtrl.AddStoryTex(form);

         });

    }


    public void DeleteTexId(string name, int id)
    {
        MapTextureForm.DataByName[name].texsName.RemoveAt(id);
    }

    public void DeleteTex(string name)
    {
        MapTextureForm.RemoveData(MapTextureForm.DataByName[name].id);
    }
    #endregion
    #region mask
    public void CreateMask(string lab = "", string name = null)
    {
        if (string.IsNullOrEmpty(name))
        {
            name = StringHelper.GetUniqueName(MapMaskForm.DataByName.Keys);
        }
        MapMaskForm.AddData(new MapMaskForm.Data(-1, name, GlobalNameHelper.GetDefaultTexName(), new List<string>() { GlobalNameHelper.GetDefaultTexName(), GlobalNameHelper.GetDefaultTexName(), GlobalNameHelper.GetDefaultTexName(), GlobalNameHelper.GetDefaultTexName(), GlobalNameHelper.GetDefaultTexName(), GlobalNameHelper.GetDefaultTexName() }, lab));
    }
    public void ImportMask(string name, int id)
    {
        AssetManager.instance.texCtrl.Select(new Vector2Int(100, 100), (form) =>
        {


            if (MapMaskForm.DataByName[name].texsName.Count > id)
            {
                MapMaskForm.DataByName[name].texsName[id] = form.name;
            }
            else
            {
                MapMaskForm.DataByName[name].texsName.Add(form.name);
            }
            GameManager.instance.saveCtrl.AddStoryTex(form);
        });

    }

    public void DeleteMask(string name)
    {
        MapMaskForm.RemoveData(MapMaskForm.DataByName[name].id);
    }
    #endregion

    #region object
    public void ChooseModel(string title, Action<EntryItem> act)
    {
        var items = new EntryItem();
        foreach (var form in GameObjectAssetForm.DataById.Values)
        {
            if (!form.name.StartsWith(GlobalNameHelper.GetInternalPrefabName("")))
            {
                items.Add(form.name);
            }
        }
        NotifyManager.instance.AddChoose(title,
            true, (item) =>
            {
                act?.Invoke(item);
                return true;
            }, items);
    }
    public void DeleteObject(string name)
    {
        MapObjectForm.RemoveData(MapObjectForm.DataByName[name].id);
    }

    public void CreateObject(string lab = "", string name = null)
    {
        if (string.IsNullOrEmpty(name))
        {
            name = StringHelper.GetUniqueName(MapObjectForm.DataByName.Keys);
        }

        MapObjectForm.AddData(new MapObjectForm.Data(-1, name, GlobalNameHelper.GetDefaultTexName(), MapModelForm.defaultData, lab, false, new Dictionary<string, EventTriggerForm.Data>()));
    }
    public void DeleteObjectUnit(string name, int id)
    {

        var data = MapObjectForm.DataByName[name];
        data.model.subPrefabUnitName.RemoveAt(id);
        data.model.subPrefabUnitPos.RemoveAt(id);
        data.model.subPrefabUnitScale.RemoveAt(id);
        data.model.subUnitTexsName.RemoveAt(id);
    }
    public void CreateObjectUnit(MapObjectForm.Data data)
    {
        data.model.subPrefabUnitName.Add("Cube");
        data.model.subPrefabUnitPos.Add(Vector3.zero);
        data.model.subPrefabUnitScale.Add(Vector3.one);
    }
    public void ImportObjectUnitTex(int uid, int id)
    {

        AssetManager.instance.texCtrl.Select(new Vector2Int(100, 100), (form) =>
        {

            if (MapObjectForm.DataById[uid].model.subUnitTexsName.Count > id)
            {
                MapObjectForm.DataById[uid].model.subUnitTexsName[id] = form.name;
            }
            else
            {
                MapObjectForm.DataById[uid].model.subUnitTexsName.Add(form.name);
            }
            GameManager.instance.saveCtrl.AddStoryTex(form);
        });

    }
    #endregion

    #region character
    public void ChooseCharacterParam(string title, Action<EntryItem> act)
    {
        var items = new EntryItem();
        foreach (var data in CharacterParamForm.DataByName.Values)
        {
            items.Add(data.name);
        }
        NotifyManager.instance.AddChoose(title,
            true, (item) =>
            {
                act?.Invoke(item);
                return true;
            }, items);
    }
    public void ChooseCharacterAnim(Dictionary<string, CharacterAnimForm.Data> animDic, string title, Action<EntryItem> act)
    {
        var items = new EntryItem();
        foreach (var key in animDic.Keys)
        {
            items.Add(key);
        }
        NotifyManager.instance.AddChoose(title,
            true, (item) =>
            {
                act?.Invoke(item);
                return true;
            }, items);
    }
    public void ChooseCharacter(string title, Action<CharacterProductForm.Data> act)
    {
        var items = new EntryItem();
        if (CharacterProductForm.DatasByIsproto.ContainsKey(true))
        {
            foreach (var data in CharacterProductForm.DatasByIsproto[true])
            {
                items.Add(data.name, TexAssetForm.DataByName[data.avatarTexName].GetSprite());
            }
        }
        NotifyManager.instance.AddChoose(TextManager.instance.GetTxt(title),
            true, (item) =>
            {
                act?.Invoke(CharacterProductForm.DataByName[item.content]);
                return true;
            }, items);
    }
    public void RenameCharacterParam(string oldName, string newName)
    {
        CharacterParamForm.DataByName[oldName].name = newName;
        foreach (var character in CharacterProductForm.DataByUid.Values)
        {
            var prm = character.paramDic.GetDv(oldName,null);
            if (prm!=null)
            {
                character.paramDic[newName] = prm;
                prm.name = newName;
                character.paramDic.Remove(oldName);
            }
        }
    }


    public void ImportCharacterAvatar(int uid)
    {

        AssetManager.instance.texCtrl.Select(new Vector2Int(100, 100), (form) =>
        {
            CharacterProductForm.DataByUid[uid].avatarTexName = form.name;
            GameManager.instance.saveCtrl.AddStoryTex(form);
        });

    }
    public void ImportCharacterTachie(int characterUid)
    {

        AssetManager.instance.texCtrl.Select(new Vector2Int(100, 100), (form) =>
        {
            CharacterProductForm.DataByUid[characterUid].tachie = form.name;
            GameManager.instance.saveCtrl.AddStoryTex(form);
        });

    }
    public CharacterAnimForm.Data CreateCharacterAnim(string name)
    {
        return new CharacterAnimForm.Data(0, name, new List<CharacterAnimClipForm.Data>(), 0.2f, 1f, new Dictionary<BodyPartType, bool>() { { BodyPartType.None, false }, { BodyPartType.UpperPart, true }, { BodyPartType.LowerPart, false } });
    }
    public CharacterAnimClipForm.Data CreateCharacterAnimClip()
    {
        var trs = new Dictionary<EquipPartType, (float, float, int, float)>();
        foreach (EquipPartType tp in Enum.GetValues(typeof(EquipPartType)))
        {
            trs[tp] = (0.5f, 0.5f, 1, 0.5f);
        }
        return new CharacterAnimClipForm.Data(0,
             new Dictionary<EquipPartType, ItemStyle>(),
             trs,
             new Dictionary<BodyPartType, string>() { { BodyPartType.None, GlobalNameHelper.GetDefaultTexName() }, { BodyPartType.UpperPart, GlobalNameHelper.GetDefaultTexName() }, { BodyPartType.LowerPart, GlobalNameHelper.GetDefaultTexName() } });
    }


    public void CreateCharacter(string name = null)
    {
        if (string.IsNullOrEmpty(name))
        {
            name = StringHelper.GetUniqueName(CharacterParamForm.DataByName.Keys);
        }
        var animDic = new Dictionary<string, CharacterAnimForm.Data>() { { "anim", CreateCharacterAnim("anim") } };

        animDic["anim"].animClip.Add(ModManager.instance.assetCtrl.CreateCharacterAnimClip());
        var defaultAnimName = new Dictionary<string, string>();
        defaultAnimName["idle"] = "anim";
        defaultAnimName["move"] = "anim";
        for (int i = 0; i < 4; i++)
        {
            defaultAnimName["idle" + i] = "anim";
            defaultAnimName["move" + i] = "anim";
        }

        var paramDic = new Dictionary<string, CharacterParamForm.Data>();
        foreach (var prm in CharacterParamForm.DataByName.Values)
        {
            paramDic[prm.name] = prm.Copy();
        }
        CharacterProductForm.AddData(new CharacterProductForm.Data(-1, name, "", GlobalNameHelper.GetDefaultCharacterTexName(), paramDic, true, animDic, defaultAnimName, default, "", "", new Dictionary<string, EventTriggerForm.Data>(),new Dictionary<EquipPartType, int>(), "", GlobalNameHelper.GetDefaultCharacterTexName(),false));
    }
    public void DeleteCharacter(int uid)
    {
        CharacterProductForm.RemoveData(uid);
    }


    public bool DeleteCharacterAnimId(int characterUid, string animNm, int id)
    {
        var data = CharacterProductForm.DataByUid[characterUid];
        var anim = data.animDic[animNm];

        anim.animClip.RemoveAt(id);

        return false;
    }
    public void DeleteCharacterAnim(int characterUid, string animNm)
    {
        CharacterProductForm.DataByUid[characterUid].animDic.Remove(animNm);
    }

    public void RenameCharacterAnim(int characterUid, string oldName, string newName)
    {
        var data = CharacterProductForm.DataByUid[characterUid];
        var anim = data.animDic[oldName];
        data.animDic.Remove(oldName);
        anim.name = newName;
        data.animDic[newName] = anim;
    }
    public void ImportCharacterAnim(int characterUid, string animNm, BodyPartType part, int id)
    {

        AssetManager.instance.texCtrl.Select(new Vector2Int(100, 100), (form) =>
        {
            var data = CharacterProductForm.DataByUid[characterUid];
            var anim = data.animDic[animNm];
            if (anim.animClip.Count > id)
            {
                anim.animClip[id].partTex[part] = form.name;
            }
            GameManager.instance.saveCtrl.AddStoryTex(form);
        });
    }
    public void CreateCharacterAnim(int characterUid, string animName = null)
    {
        var data = CharacterProductForm.DataByUid[characterUid];
        if (data.animDic.Keys.Count > GlobalSettings.CHARACTER_ANIM_MAX)
            return;
        if (string.IsNullOrEmpty(animName))
        {
            animName = StringHelper.GetUniqueName(data.animDic.Keys);
        }
        data.animDic[animName] = CreateCharacterAnim(animName);

    }
    public void CreateCharacterAnimId(int characterUid, string animNm)
    {
        var data = CharacterProductForm.DataByUid[characterUid];
        var anim = data.animDic[animNm];
        anim.animClip.Add(CreateCharacterAnimClip());
    }
    #endregion

    #region effect
    public EffectClipForm.Data CreateEffectClip(string texName)
    {
        return new EffectClipForm.Data(-1, texName, 1,Vector3.zero,0,Vector3.one,1,true);
    }
    public void DeleteEffectClip(int effectUid,int id)
    {
        EffectForm.DataByUid[effectUid].clips.RemoveAt(id);
    }
    public void CreateEffect(string lab = "", string name = null)
    {
        if (string.IsNullOrEmpty(name))
        {
            name = StringHelper.GetUniqueName(EffectForm.DataByName.Keys);
        }
        EffectForm.AddData(new EffectForm.Data(-1, name, lab, new List<EffectClipForm.Data>() { CreateEffectClip(GlobalNameHelper.GetDefaultTexName()) }));
    }
    public void DeleteEffect(int effectUid)
    {
        EffectForm.RemoveData(effectUid);
    }
    public void ImportEffectImage(int effectUid)
    {

        AssetManager.instance.texCtrl.Select(new Vector2Int(100, 100), (form) =>
        {
            var data = EffectForm.DataByUid[effectUid];
            foreach(var clip in data.clips)
            {
                clip.tex = form.name;
            }
            GameManager.instance.saveCtrl.AddStoryTex(form);
        });
    }
    public void ChooseEffectr(string title, Action<EffectForm.Data> act)
    {
        var items = new EntryItem();
        
            foreach (var data in EffectForm.DataByUid.Values)
            {
                items.Add(data.name, TexAssetForm.DataByName[data.clips[0].tex].GetSprite());
            }
        NotifyManager.instance.AddChoose(TextManager.instance.GetTxt(title),
            true, (item) =>
            {
                act?.Invoke(EffectForm.DataByName[item.content]);
                return true;
            }, items);
    }
    #endregion
    #region skill
    public void CreateSkill(string lab = "", string name = null)
    {
        if (string.IsNullOrEmpty(name))
        {
            name = StringHelper.GetUniqueName(SkillForm.DataByName.Keys);
        }
        SkillForm.AddData(new SkillForm.Data(-1,name,"",GlobalNameHelper.GetDefaultTexName(),new List<SkillType>(),1,new Dictionary<string, EventTriggerForm.Data>(),0));
    }
    public void DeleteSkill(int uid)
    {
        SkillForm.RemoveData(uid);
    }
    public void ImportSkillIcon(int skillUid)
    {
        AssetManager.instance.texCtrl.Select(new Vector2Int(100, 100), (form) =>
        {
            var data = SkillForm.DataByUid[skillUid];
            data.icon= form.name;
            GameManager.instance.saveCtrl.AddStoryTex(form);
        });
    }
    #endregion


    #region event
    public void ImportImage(Action<TexAssetForm.Data> act)
    {
        AssetManager.instance.texCtrl.Select(new Vector2Int(1000, 1000), (form) =>
        {
            GameManager.instance.saveCtrl.AddStoryTex(form);
            act?.Invoke(form);
        });
    }
    public void ImportVideo(Action<VideoAssetForm.Data> act)
    {
        AssetManager.instance.videoCtrl.Select(new Vector2Int(100, 100), (form) =>
        {
            GameManager.instance.saveCtrl.AddStoryVideo(form);
            act?.Invoke(form);
        });
    }
    public void ImportAudio(Action<AudioAssetForm.Data> act)
    {
        AssetManager.instance.audioCtrl.Select(new Vector2Int(100, 100), (form) =>
        {
            GameManager.instance.saveCtrl.AddStoryAudio(form);
            act?.Invoke(form);
        });
    }
    public void ChooseEvent(SceneEventType type, string retType, string title, Action<EntryItem> act)
    {
        var items = GameManager.instance.evtCtrl.GetEventEntry(type, retType);
        NotifyManager.instance.AddMultipleChoose(title, true, (res) =>
        {
            act?.Invoke(res);
            return true;
        }, items);
    }
    public void ChooseEventTriggerType(Action<TriggerType> act)
    {
        var items = new EntryItem();
        foreach(TriggerType tp in Enum.GetValues(typeof(TriggerType)))
        {
            items.Add(TextManager.instance.GetTxt(tp.ToString()),null,(int)tp);
        }
        NotifyManager.instance.AddMultipleChoose(TextManager.instance.GetTxt("Choose trigger condition"), true, (res) =>
        {
            act?.Invoke((TriggerType)res.id);
            return true;
        }, items);
    }

    public void ChooseTriggerCondition(string title,Action<EntryItem> act)
    {
        var items = GameManager.instance.evtCtrl.GetTriggerConditionEntry();
        NotifyManager.instance.AddMultipleChoose(title, true, (res) =>
        {
            act?.Invoke(res);
            return true;
        }, items);
    }
    public void ChooseCmd(SceneEventType type, string retType, Action<EntryItem> act,bool createOnly=false)
    {
        var items = GameManager.instance.evtCtrl.GetCmdEntry(type, retType, createOnly,out var defaultItem);
        NotifyManager.instance.AddMultipleChoose(TextManager.instance.GetTxt("Choose command"), true, (res) =>
        {
            act?.Invoke(res);
            return true;
        }, items, defaultItem);
    }

    public void CreateEvent(string name = "", string category = "", string type = "")
    {
        if (string.IsNullOrEmpty(name))
        {
            name = StringHelper.GetUniqueName(EventProgramDataForm.DataByName.Keys);
        }
        if (category == null)
            category = "";
        if (type == null)
            type = "";

        EventProgramDataForm.AddData(new EventProgramDataForm.Data(-1, name, "", new List<string>(), category, type));
    }
    public void DeleteEvent(string name)
    {
        EventProgramDataForm.RemoveData(EventProgramDataForm.DataByName[name].uid);
    }
    public void ImportClipTex(Action<string> callback)
    {
        AssetManager.instance.texCtrl.Select(callback: (form) =>
        {
            callback?.Invoke(form.name);
            GameManager.instance.saveCtrl.AddStoryTex(form);
        });

    }
    #endregion

    #region item
    public void ChooseItemStyle(string title, Action<EntryItem> act)
    {
        var items = new EntryItem();
        foreach (var e in Enum.GetValues(typeof(ItemStyle)))
        {
            items.Add((string)e);
        }
        NotifyManager.instance.AddChoose(title,
            true, (item) =>
            {
                act?.Invoke(item);
                return true;
            }, items);
    }


    public void RenameItemParam(string oldName, string newName)
    {
        ItemParamForm.DataByName[oldName].name = newName;
        foreach (var item in ItemProductForm.DataByUid.Values)
        {
            var prm = item.paramDic.GetDv(oldName, null);
            if (prm != null)
            {
                item.paramDic[newName] = prm;
                prm.name = newName;
                item.paramDic.Remove(oldName);
            }
        }
    }



    public void ImportItemIcon(int itemUid)
    {

        AssetManager.instance.texCtrl.Select(new Vector2Int(100, 100), (form) =>
        {
            ItemProductForm.DataByUid[itemUid].iconTexName = form.name;
            GameManager.instance.saveCtrl.AddStoryTex(form);
        });

    }



    public void CreateItem(string name = null)
    {
        if (string.IsNullOrEmpty(name))
        {
            name = StringHelper.GetUniqueName(ItemParamForm.DataByName.Keys);
        }
        var dic = new Dictionary<string, ItemParamForm.Data>();
        foreach (var prm in ItemParamForm.DataByName.Values)
        {
            dic[prm.name] = prm.Copy();
        }
        var model = MapModelForm.defaultData.Copy();
        model.isObstacle = false;
        var styleTex = new Dictionary<ItemStyle, string>();
        foreach (ItemStyle style in Enum.GetValues(typeof(ItemStyle)))
        {
            styleTex[style] = GlobalNameHelper.GetDefaultTexName();
        }
        ItemProductForm.AddData(new ItemProductForm.Data(-1, name, "", GlobalNameHelper.GetDefaultTexName(), dic, true, model, "", 1, 99, default, styleTex, 0, false, new Dictionary<string, EventTriggerForm.Data>()));
    }
    public void DeleteItem(int itemUid)
    {
        ItemProductForm.RemoveData(ItemProductForm.DataByUid[itemUid].uid);
    }

    public void DeleteItemModelUnit(int itemUid, int id)
    {

        var data = ItemProductForm.DataByUid[itemUid];
        data.model.subPrefabUnitName.RemoveAt(id);
        data.model.subPrefabUnitPos.RemoveAt(id);
        data.model.subPrefabUnitScale.RemoveAt(id);
        data.model.subUnitTexsName.RemoveAt(id);
    }
    public void CreateItemModelUnit(ItemProductForm.Data data)
    {
        data.model.subPrefabUnitName.Add("Cube");
        data.model.subPrefabUnitPos.Add(Vector3.zero);
        data.model.subPrefabUnitScale.Add(Vector3.one);
    }
    public void ImportItemModelUnitTex(int itemUid, int id)
    {
        AssetManager.instance.texCtrl.Select(new Vector2Int(100, 100), (form) =>
        {
            var data = ItemProductForm.DataByUid[itemUid];
            if (data.model.subUnitTexsName.Count > id)
            {
                data.model.subUnitTexsName[id] = form.name;
            }
            else
            {
                data.model.subUnitTexsName.Add(form.name);
            }
            GameManager.instance.saveCtrl.AddStoryTex(form);

        });

    }
    public void ImportItemStyleTex(int itemUid, ItemStyle style)
    {

        AssetManager.instance.texCtrl.Select(new Vector2Int(100, 100), (form) =>
        {
            var data = ItemProductForm.DataByUid[itemUid];
            data.styleTex[style] = form.name;
            GameManager.instance.saveCtrl.AddStoryTex(form);
        });

    }



    #endregion

    #region scene
    public void ImportMapMiniMap()
    {
/*        AssetManager.instance.texCtrl.Select(new Vector2Int(1000, 1000), (form) =>
        {
            GameManager.instance.saveCtrl.AddStoryTex(form);
        });*/

    }

    public void CreateScene(string name = "")
    {
        if (string.IsNullOrEmpty(name))
        {
            name = StringHelper.GetUniqueName(SceneForm.DataByName.Keys);
        }
        SceneForm.AddData(new SceneForm.Data(-1, name, GlobalNameHelper.GetDefaultTexName(), (0.5f, 0.5f)));
    }

    public void ImportSceneMiniMap(string name)
    {
        AssetManager.instance.texCtrl.Select(new Vector2Int(1000, 1000), (form) =>
        {
            var data = SceneForm.DataByName[name].miniMap = form.name;
            GameManager.instance.saveCtrl.AddStoryTex(form);

        });

    }
    public void DeleteScene(int uid)
    {
        SceneForm.RemoveData(uid);
        GameManager.instance.saveCtrl.DeleteSceneMap(ModManager.instance.GetStoryCoreFolder(), uid);
    }
    #endregion

}
