using Form;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using System.Xml.Linq;
using Ui.ModAssetSelectWindow;
using UnityEngine;
using Z_Code.Form;
using Z_DataSystem;
using Z_DataSystem.Form;
using Z_DesignStyle;
using Z_Map;
using Z_String;
using Z_Text;
using Z_Ui;
using Z_Ui.Notify;
using static UnityEditor.Progress;
using static UnityEngine.Rendering.DebugUI.MessageBox;
namespace Form
{
    public static partial class StoryTexAssetForm
    {
        public static void AddData(TexAssetForm.Data data)
        {
            AddData(new Data(data));
        }
    }
    public static partial class StoryAudioAssetForm
    {
        public static void AddData(AudioAssetForm.Data data)
        {
            AddData(new Data(data));
        }
    }
    public static partial class StoryVideoAssetForm
    {
        public static void AddData(VideoAssetForm.Data data)
        {
            AddData(new Data(data));
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
        UiManager.instance.ShowUi<UiModAssetSelectWindowCtrl>(new UiModAssetSelectTexWindowParam()
        {
            onComplete = (data) =>
            {
                GameManager.instance.curStory.icon = data.name;
            },
            sizeLimit = new Vector2Int(400, 400)
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

        CharacterParamForm.AddData(new CharacterParamForm.Data(-1, name, 0, "", "", "", default));
        foreach (var character in CharacterProductForm.DataByUid.Values)
        {
            character.paramDic[name] = new CharacterParamForm.Data(-1, name, 0, "", "", "", default);
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
    public void RenameCharacterParam(string oldName, string newName)
    {
        CharacterParamForm.DataByName[oldName].name = newName;
        foreach (var character in CharacterProductForm.DataByUid.Values)
        {
            var prm = character.paramDic.GetDv(oldName, null);
            if (prm != null)
            {
                character.paramDic[newName] = prm;
                prm.name = newName;
                character.paramDic.Remove(oldName);
            }
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

        ItemParamForm.AddData(new ItemParamForm.Data(-1, name, 0, "", "", "", default));
        foreach (var item in ItemProductForm.DataByUid.Values)
        {
            item.paramDic[name] = new ItemParamForm.Data(-1, name, 0, "", "", "", default);
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

    public void CreateSceneObjectArg(string name)
    {
        if (MapObjectParamForm.DataByName.Keys.Count > GlobalSettings.SCENE_PARAM_MAX)
            return;

        if (string.IsNullOrEmpty(name))
        {
            name = StringHelper.GetUniqueName(MapObjectParamForm.DataByName.Keys);
        }

        MapObjectParamForm.AddData(new MapObjectParamForm.Data(-1, name, 0, "", "", ""));
        foreach (var obj in MapObjectForm.DataById.Values)
        {
            obj.paramDic[name] = new MapObjectParamForm.Data(-1, name, 0, "", "", "");
            obj.paramDic = obj.paramDic;//refresh
        }
    }
    public void DeleteSceneObjectArg(string name)
    {
        MapObjectParamForm.RemoveData(MapObjectParamForm.DataByName[name].uid);
        foreach (var obj in MapObjectForm.DataById.Values)
        {
            obj.paramDic.Remove(name);
            obj.paramDic = obj.paramDic;//refresh
        }
    }

    public void RenameSceneObjectParam(string oldName, string newName)
    {
        MapObjectParamForm.DataByName[oldName].name = newName;
        foreach (var obj in MapObjectForm.DataById.Values)
        {
            obj.paramDic[newName] = obj.paramDic[oldName];
            obj.paramDic.Remove(oldName);
            obj.paramDic = obj.paramDic;//refresh
        }
    }
    
    public void CreateSkillArg(string name)
    {
        if (SkillParamForm.DataByName.Keys.Count > GlobalSettings.SKILL_PARAM_MAX)
            return;

        if (string.IsNullOrEmpty(name))
        {
            name = StringHelper.GetUniqueName(SkillParamForm.DataByName.Keys);
        }

        SkillParamForm.AddData(new SkillParamForm.Data(-1, name, 0, "", "", "", default));
        foreach (var skill in SkillProductForm.DataByUid.Values)
        {
            skill.paramDic[name] = new SkillParamForm.Data(-1, name, 0, "", "", "", default);
        }
    }
    public void DeleteSkillArg(string name)
    {
        SkillParamForm.RemoveData(SkillParamForm.DataByName[name].uid);
        foreach (var skill in SkillProductForm.DataByUid.Values)
        {
            skill.paramDic.Remove(name);
        }
    }
    public void RenameSkillParam(string oldName, string newName)
    {
        SkillParamForm.DataByName[oldName].name = newName;
        foreach (var obj in SkillProductForm.DataByUid.Values)
        {
            obj.paramDic[newName] = obj.paramDic[oldName];
            obj.paramDic.Remove(oldName);
            obj.paramDic = obj.paramDic;//refresh
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
        UiManager.instance.ShowUi<UiModAssetSelectWindowCtrl>(new UiModAssetSelectTexWindowParam()
        {
            onComplete = (data) =>
            {
                if (id != -1 && MapTextureForm.DataByName[name].texsName.Count > id)
                {
                    MapTextureForm.DataByName[name].texsName[id] = data.name;
                }
                else
                {
                    MapTextureForm.DataByName[name].texsName.Add(data.name);
                }
            },
            sizeLimit = new Vector2Int(100, 100)
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
        UiManager.instance.ShowUi<UiModAssetSelectWindowCtrl>(new UiModAssetSelectTexWindowParam()
        {
            onComplete = (data) =>
            {
                if (MapMaskForm.DataByName[name].texsName.Count > id)
                {
                    MapMaskForm.DataByName[name].texsName[id] = data.name;
                }
                else
                {
                    MapMaskForm.DataByName[name].texsName.Add(data.name);
                }
            },
            sizeLimit = new Vector2Int(100, 100)
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
        var paramDic = new Dictionary<string, MapObjectParamForm.Data>();
        foreach (var prm in MapObjectParamForm.DataByName.Values)
        {
            paramDic[prm.name] = prm.Copy();
        }
        MapObjectForm.AddData(new MapObjectForm.Data(-1, name, GlobalNameHelper.GetDefaultTexName(), MapModelForm.defaultData, lab, false, new Dictionary<string, EventTriggerForm.Data>(), paramDic));
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
        UiManager.instance.ShowUi<UiModAssetSelectWindowCtrl>(new UiModAssetSelectTexWindowParam()
        {
            onComplete = (data) =>
            {
                if (MapObjectForm.DataById[uid].model.subUnitTexsName.Count > id)
                {
                    MapObjectForm.DataById[uid].model.subUnitTexsName[id] = data.name;
                }
                else
                {
                    MapObjectForm.DataById[uid].model.subUnitTexsName.Add(data.name);
                }
            },
            sizeLimit = new Vector2Int(100, 100)
        });

    }

    public void ChooseSceneObjectParam(string title, Action<EntryItem> act, EntryItem addItem = null)
    {
        var items = new EntryItem();
        if (addItem != null)
        {
            items.Add(addItem);
        }
        foreach (var data in MapObjectParamForm.DataByName.Values)
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
    #endregion

    #region character
    public void ChooseCharacterParam(string title, Action<EntryItem> act, EntryItem addItem = null)
    {
        var items = new EntryItem();
        if (addItem != null)
        {
            items.Add(addItem);
        }
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
    public void ChooseCharacter(string title, Action<CharacterProductForm.Data> act, EntryItem addItem = null)
    {
        var items = new EntryItem();
        if (CharacterProductForm.DatasByProtouid.ContainsKey(0))
        {
            if (addItem != null)
            {
                items.Add(addItem);
            }
            foreach (var data in CharacterProductForm.DatasByProtouid[0])
            {
                items.Add(data.name, TexAssetForm.DataByName[data.avatarTexName].GetSprite());
            }
        }
        NotifyManager.instance.AddChoose(TextManager.instance.GetTxt(title),
            true, (item) =>
            {
                if (item == addItem)
                {
                    act?.Invoke(null);
                }
                else
                {
                    act?.Invoke(CharacterProductForm.DataByNameProtouid.GetDv((item.content, 0), null));
                }
                return true;
            }, items);
    }
    public void ChooseActiveCharacter(string title, Action<CharacterProductForm.Data> act, EntryItem addItem = null)
    {
        var items = new EntryItem();
        if (addItem != null)
        {
            items.Add(addItem);
        }

        foreach (var uid in GameManager.instance.curProgress.team)
        {
            var data = CharacterProductForm.DataByUid.GetDv(uid, null);
            if (data != null)
            {
                items.Add(data.name, TexAssetForm.DataByName[data.avatarTexName].GetSprite());
            }
        }
        NotifyManager.instance.AddChoose(TextManager.instance.GetTxt(title),
            true, (item) =>
            {
                if (item == addItem)
                {
                    act?.Invoke(null);
                }
                else
                {
                    act?.Invoke(CharacterProductForm.DataByNameProtouid.GetDv((item.content, 0), null));
                }
                return true;
            }, items);
    }


    public void ImportCharacterAvatar(int uid)
    {
        UiManager.instance.ShowUi<UiModAssetSelectWindowCtrl>(new UiModAssetSelectTexWindowParam()
        {
            onComplete = (data) =>
            {
                CharacterProductForm.DataByUid[uid].avatarTexName = data.name;
            },
            sizeLimit = new Vector2Int(100, 100)
        });
    }
    public void ImportCharacterIllustration(int characterUid)
    {
        UiManager.instance.ShowUi<UiModAssetSelectWindowCtrl>(new UiModAssetSelectTexWindowParam()
        {
            onComplete = (data) =>
            {
                CharacterProductForm.DataByUid[characterUid].illustration = data.name;
            },
            sizeLimit = new Vector2Int(100, 100)
        });

    }
    public CharacterAnimForm.Data CreateCharacterAnim(string name)
    {
        var dic = new Dictionary<AnimDirecton, List<CharacterAnimClipForm.Data>>();
        foreach (AnimDirecton dir in Enum.GetValues(typeof(AnimDirecton)))
        {
            dic[dir] = new List<CharacterAnimClipForm.Data>();
            dic[dir].Add(ModManager.instance.assetCtrl.CreateCharacterAnimClip());
        }
        return new CharacterAnimForm.Data(0, name, dic, 0.2f, 1f, new Dictionary<BodyPartType, bool>() { { BodyPartType.None, false }, { BodyPartType.UpperPart, true }, { BodyPartType.LowerPart, false } });
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
            name = StringHelper.GetUniqueName(CharacterProductForm.DataByUid.Keys);
        }
        var animDic = new Dictionary<string, CharacterAnimForm.Data>() { { "anim", CreateCharacterAnim("anim") } };

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
        CharacterProductForm.AddData(new CharacterProductForm.Data(-1, name, "", GlobalNameHelper.GetDefaultCharacterTexName(), paramDic, 0, animDic, defaultAnimName, default, "", "", new Dictionary<string, EventTriggerForm.Data>(), new Dictionary<EquipPartType, int>(), "", GlobalNameHelper.GetDefaultCharacterTexName(), false, new Dictionary<SkillType, int>(), 0, false));
    }
    public void DeleteCharacter(int uid)
    {
        CharacterProductForm.RemoveData(uid);
    }


    public bool DeleteCharacterAnimId(int characterUid, string animNm, AnimDirecton dir, int id)
    {
        var data = CharacterProductForm.DataByUid[characterUid];
        var anim = data.animDic[animNm];

        anim.animClip[dir].RemoveAt(id);

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
    public void ImportCharacterAnim(int characterUid, string animNm, AnimDirecton dir, BodyPartType part, int id)
    {
        UiManager.instance.ShowUi<UiModAssetSelectWindowCtrl>(new UiModAssetSelectTexWindowParam()
        {
            onComplete = (data) =>
            {
                var chracterData = CharacterProductForm.DataByUid[characterUid];
                var anim = chracterData.animDic[animNm];
                if (anim.animClip[dir].Count > id)
                {
                    anim.animClip[dir][id].partTex[part] = data.name;
                }
            },
            sizeLimit = new Vector2Int(100, 100)
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
    public void CreateCharacterAnimId(int characterUid, string animNm, AnimDirecton dir)
    {
        var data = CharacterProductForm.DataByUid[characterUid];
        var anim = data.animDic[animNm];
        if (!anim.animClip.ContainsKey(dir))
        {
            anim.animClip[dir] = new List<CharacterAnimClipForm.Data>();
        }
        anim.animClip[dir].Add(CreateCharacterAnimClip());
    }
    #endregion

    #region effect
    public List<EffectClipForm.Data> CreateEffectClips(string texName = null)
    {
        return new List<EffectClipForm.Data>() { CreateEffectClip(texName) };
    }
    public EffectClipForm.Data CreateEffectClip(string texName = null)
    {
        return new EffectClipForm.Data(-1, string.IsNullOrEmpty(texName) ? GlobalNameHelper.GetDefaultTexName() : texName, 1, Vector3.zero, 0, Vector3.one, 1, true);
    }
    public void DeleteEffectClips(int effectUid, int id)
    {
        EffectForm.DataByUid[effectUid].clips.RemoveAt(id);
    }
    public void DeleteEffectClip(int effectUid, int id1, int id2)
    {
        EffectForm.DataByUid[effectUid].clips[id1].RemoveAt(id2);
    }
    public void CreateEffect(string lab = "", string name = null)
    {
        if (string.IsNullOrEmpty(name))
        {
            name = StringHelper.GetUniqueName(EffectForm.DataByName.Keys);
        }
        EffectForm.AddData(new EffectForm.Data(-1, name, lab, new List<List<EffectClipForm.Data>>() { new List<EffectClipForm.Data>() { CreateEffectClip(GlobalNameHelper.GetDefaultTexName()) } }, false));
    }
    public void DeleteEffect(int effectUid)
    {
        EffectForm.RemoveData(effectUid);
    }
    public void ImportEffectImage(int effectUid, int clipId)
    {
        UiManager.instance.ShowUi<UiModAssetSelectWindowCtrl>(new UiModAssetSelectTexWindowParam()
        {
            onComplete = (data) =>
            {
                var effectData = EffectForm.DataByUid[effectUid];
                foreach (var clip in effectData.clips[clipId])
                {
                    clip.tex = data.name;
                }
            },
            sizeLimit = new Vector2Int(1000, 1000)
        });
    }
    public void ChooseEffect(string title, Action<EffectForm.Data> act)
    {
        var items = new EntryItem();

        foreach (var data in EffectForm.DataByUid.Values)
        {
            items.Add(data.name, TexAssetForm.DataByName[data.clips[0][0].tex].GetSprite(), data.uid);
        }
        NotifyManager.instance.AddChoose(TextManager.instance.GetTxt(title),
            true, (item) =>
            {
                act?.Invoke(EffectForm.DataByUid[item.id]);
                return true;
            }, items);
    }
    #endregion
    #region skill
    public void CreateSkill(string lab = "", string name = null)
    {
        if (string.IsNullOrEmpty(name))
        {
            name = StringHelper.GetUniqueName(SkillProductForm.DataByUid.Keys);
        }
        var paramDic = new Dictionary<string, SkillParamForm.Data>();
        foreach (var prm in SkillParamForm.DataByName.Values)
        {
            paramDic[prm.name] = prm.Copy();
        }
        SkillProductForm.AddData(new SkillProductForm.Data(-1, name, "", 0, GlobalNameHelper.GetDefaultTexName(), paramDic, new List<SkillType>(), 0, 1, 0, new Dictionary<string, EventTriggerForm.Data>(), 0));
    }
    public void DeleteSkill(int uid)
    {
        SkillProductForm.RemoveData(uid);
    }
    public void ImportSkillIcon(int skillUid)
    {
        UiManager.instance.ShowUi<UiModAssetSelectWindowCtrl>(new UiModAssetSelectTexWindowParam()
        {
            onComplete = (data) =>
            {
                var skillData = SkillProductForm.DataByUid[skillUid];
                skillData.icon = data.name;
            },
            sizeLimit = new Vector2Int(100, 100)
        });
    }
    public void ChooseSkill(string title, Action<SkillProductForm.Data> act)
    {
        var items = new EntryItem();

        foreach (var data in SkillProductForm.DataByUid.Values)
        {
            items.Add(data.name, TexAssetForm.DataByName[data.icon].GetSprite(), data.uid);
        }
        NotifyManager.instance.AddChoose(TextManager.instance.GetTxt(title),
            true, (item) =>
            {
                act?.Invoke(SkillProductForm.DataByUid[item.id]);
                return true;
            }, items);
    }
    public void ChooseSkillParam(string title, Action<EntryItem> act, EntryItem addItem = null)
    {
        var items = new EntryItem();
        if (addItem != null)
        {
            items.Add(addItem);
        }
        foreach (var data in SkillParamForm.DataByName.Values)
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
    #endregion


    #region event
    public void ImportImage(Action<TexAssetForm.Data> act)
    {
        UiManager.instance.ShowUi<UiModAssetSelectWindowCtrl>(new UiModAssetSelectTexWindowParam()
        {
            onComplete = (data) =>
            {
                act?.Invoke(data);
            },
        });
    }
    public void ImportVideo(Action<VideoAssetForm.Data> act)
    {
        UiManager.instance.ShowUi<UiModAssetSelectWindowCtrl>(new UiModAssetSelectVideoWindowParam()
        {
            onComplete = (data) =>
            {
                act?.Invoke(data);
            },
        });
    }
    public void ImportAudio(Action<AudioAssetForm.Data> act)
    {
        UiManager.instance.ShowUi<UiModAssetSelectWindowCtrl>(new UiModAssetSelectAudioWindowParam()
        {
            onComplete = (data) =>
            {
                act?.Invoke(data);
            },
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
        foreach (TriggerType tp in Enum.GetValues(typeof(TriggerType)))
        {
            items.Add(TextManager.instance.GetTxt(tp.ToString()), null, (int)tp);
        }
        NotifyManager.instance.AddMultipleChoose(TextManager.instance.GetTxt("Choose trigger condition"), true, (res) =>
        {
            act?.Invoke((TriggerType)res.id);
            return true;
        }, items);
    }

    public void ChooseTriggerCondition(string title, Action<EntryItem> act)
    {
        var items = GameManager.instance.evtCtrl.GetTriggerConditionEntry();
        NotifyManager.instance.AddMultipleChoose(title, true, (res) =>
        {
            act?.Invoke(res);
            return true;
        }, items);
    }
    public void ChooseBasicCmd(BoxDataForm.Data box, Action onComplete)
    {
        var items = new EntryItem();
        items.Add(TextManager.instance.GetTxt("Num"), null, 1);
        items.Add(TextManager.instance.GetTxt("Text"), null, 2);
        NotifyManager.instance.AddChoose(TextManager.instance.GetTxt("Choose command"), true, (res) =>
        {
            switch (res.id)
            {
                case 1:
                    NotifyManager.instance.AddInputArea(TextManager.instance.GetTxt("input value"), true, (res) =>
                    {
                        if (float.TryParse(res, out float val))
                        {
                            box.str = null;
                            box.num = val;
                        }
                        onComplete?.Invoke();
                        return true;
                    }, box.num.ToString());

                    break;
                case 2:
                    NotifyManager.instance.AddInputArea(TextManager.instance.GetTxt("input value"), true, (res) =>
                    {
                        box.str = res;
                        return true;
                    }, box.num.ToString());
                    break;
            }
            return true;
        }, items);
    }
    public void ChooseCmd(SceneEventType type, string retType, Action<EntryItem> act)
    {
        var items = GameManager.instance.evtCtrl.GetCmdEntry(type, retType, out var defaultItem);
        items.Merge("#", GameManager.instance.evtCtrl.GetEventEntry(type, retType));
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

        EventProgramDataForm.AddData(new EventProgramDataForm.Data(-1, name, "", new List<string>(), 0, "void", category, type));
    }
    public void DeleteEvent(string name)
    {
        EventProgramDataForm.RemoveData(EventProgramDataForm.DataByName[name].uid);
    }
    public void ImportClipTex(Action<string> callback)
    {
        UiManager.instance.ShowUi<UiModAssetSelectWindowCtrl>(new UiModAssetSelectTexWindowParam()
        {
            onComplete = (data) =>
            {
                callback?.Invoke(data.name);
            },
            sizeLimit = new Vector2Int(100, 100)
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






    public void ImportItemIcon(int itemUid)
    {
        UiManager.instance.ShowUi<UiModAssetSelectWindowCtrl>(new UiModAssetSelectTexWindowParam()
        {
            onComplete = (data) =>
            {
                ItemProductForm.DataByUid[itemUid].iconTexName = data.name;
            },
            sizeLimit = new Vector2Int(100, 100)
        });

    }



    public void CreateItem(string name = null)
    {
        if (string.IsNullOrEmpty(name))
        {
            name = StringHelper.GetUniqueName(ItemProductForm.DataByUid.Keys);
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
        ItemProductForm.AddData(new ItemProductForm.Data(-1, name, "", GlobalNameHelper.GetDefaultTexName(), dic, 0, model, "", 1, 99, default, styleTex, 0, false, new Dictionary<string, EventTriggerForm.Data>(), true, new Dictionary<string, CharacterParamForm.Data>()));
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
        UiManager.instance.ShowUi<UiModAssetSelectWindowCtrl>(new UiModAssetSelectTexWindowParam()
        {
            onComplete = (data) =>
            {
                var itemData = ItemProductForm.DataByUid[itemUid];
                if (itemData.model.subUnitTexsName.Count > id)
                {
                    itemData.model.subUnitTexsName[id] = data.name;
                }
                else
                {
                    itemData.model.subUnitTexsName.Add(data.name);
                }
            },
            sizeLimit = new Vector2Int(100, 100)
        });

    }
    public void ImportItemStyleTex(int itemUid, ItemStyle style)
    {
        UiManager.instance.ShowUi<UiModAssetSelectWindowCtrl>(new UiModAssetSelectTexWindowParam()
        {
            onComplete = (data) =>
            {
                var itemData = ItemProductForm.DataByUid[itemUid];
                itemData.styleTex[style] = data.name;
            },
            sizeLimit = new Vector2Int(100, 100)
        });

    }

    public void ChooseItem(string title, Action<ItemProductForm.Data> act, EntryItem addItem = null)
    {
        var items = new EntryItem();
        if (ItemProductForm.DatasByProtouid.ContainsKey(0))
        {
            if (addItem != null)
            {
                items.Add(addItem);
            }
            foreach (var data in ItemProductForm.DatasByProtouid[0])
            {
                items.Add(data.name, TexAssetForm.DataByName[data.iconTexName].GetSprite());
            }
        }
        NotifyManager.instance.AddChoose(TextManager.instance.GetTxt(title),
            true, (item) =>
            {
                if (item == addItem)
                {
                    act?.Invoke(null);
                }
                else
                {
                    act?.Invoke(ItemProductForm.DataByNameProtouid.GetDv((item.content, 0), null));
                }
                return true;
            }, items);
    }
    public void ChooseItemParam(string title, Action<EntryItem> act, EntryItem addItem = null)
    {
        var items = new EntryItem();
        if (addItem != null)
        {
            items.Add(addItem);
        }
        foreach (var data in ItemParamForm.DataByName.Values)
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
    #endregion

    #region scene
    public void ImportMapMiniMap()
    {


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
        UiManager.instance.ShowUi<UiModAssetSelectWindowCtrl>(new UiModAssetSelectTexWindowParam()
        {
            onComplete = (data) =>
            {
                SceneForm.DataByName[name].miniMap = data.name;
            },
            sizeLimit = new Vector2Int(100, 100)
        });

    }
    public void DeleteScene(int uid)
    {
        SceneForm.RemoveData(uid);
        GameManager.instance.saveCtrl.DeleteSceneMap(ModManager.instance.GetStoryCoreFolder(), uid);
    }
    #endregion

}
