using Form;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Xml.Linq;
using Ui.ModAssetSelectWindow;
using UnityEngine;
using UnityEngine.Rendering;
using Z_Code.Form;
using Z_DataSystem;
using Z_DataSystem.Form;
using Z_DesignStyle;
using Z_Map;
using Z_Map.Form;
using Z_String;
using Z_Text;
using Z_Ui;
using Z_Ui.Notify;
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
                GameManager.instance.curStory.icon = Convert.ToBase64String(data.GetBytes());
            }
        });
    }


    #endregion
    #region mission
    public void CreateMission(int labId = 0, string name = null)
    {
        if (MissionForm.DataByName.Keys.Count > GlobalSettings.CHARACTER_PARAM_MAX)
            return;

        if (string.IsNullOrEmpty(name))
        {
            name = StringHelper.GetUniqueName(MissionForm.DataByName.Keys);
        }

        MissionForm.AddData(new MissionForm.Data(-1, name, labId, "", false, false, false, false, 0, Vector3.zero, 0));
    }
    public void DeleteMission(string name)
    {
        MissionForm.RemoveData(MissionForm.DataByName[name].id);
    }
    public void RenameMission(string oldName, string newName)
    {
        MissionForm.DataByName[oldName].name = newName;
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


    #region pass type

    public void CreatePassType(string name = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            name = StringHelper.GetUniqueName(PassTypeForm.DataByName.Keys);
        PassTypeForm.AddData(new PassTypeForm.Data(-1, name));
    }

    public void RenamePassType(int id, string newName)
    {
        if (PassTypeForm.DataById.TryGetValue(id, out var data) &&
            !string.IsNullOrWhiteSpace(newName) &&
            (data.name == newName || StringHelper.IsUniqueName(PassTypeForm.DataByName.Keys, newName)))
        {
            data.name = newName;
        }
    }

    public void DeletePassType(int id)
    {
        if (!PassTypeForm.DataById.ContainsKey(id))
            return;

        foreach (var texture in MapTextureForm.DataById.Values)
        {
            if (texture.passType == id)
                texture.passType = 0;
        }
        foreach (var character in CharacterProductForm.DataByUid.Values)
        {
            character.passType?.RemoveAll(value => value == id);
            if (character.passType != null)
                character.passType = character.passType;
        }
        foreach (var tile in TileUnitForm.DataByUid.Values)
        {
            if (tile.passType == id)
                tile.passType = 0;
            GameMapData.ApplyTilePassTypes(tile);
        }
        foreach (var character in CharacterUnitForm.DataByUid.Values)
            GameMapData.ApplyCharacterProductPassTypes(character);

        PassTypeForm.RemoveData(id);
    }

    #endregion


    #region texture

    public void CreateTex(int labId = 0, string name = null)
    {
        if (string.IsNullOrEmpty(name))
        {
            name = StringHelper.GetUniqueName(MapTextureForm.DatasByName.Keys);
        }
        MapTextureForm.AddData(new MapTextureForm.Data(-1, name, GlobalDefaultHelper.DefaultTexId, 1, new List<int>() { GlobalDefaultHelper.DefaultTexId }, labId, new Dictionary<string, EventTriggerForm.Data>(), false, new Dictionary<int, int>(), 0, false, new List<int>()));
    }
    public void ImportTex(int texId, int id = -1)
    {
        UiManager.instance.ShowUi<UiModAssetSelectWindowCtrl>(new UiModAssetSelectTexWindowParam()
        {
            onComplete = (data) =>
            {
                if (id != -1 && MapTextureForm.DataById[texId].texs.Count > id)
                {
                    MapTextureForm.DataById[texId].texs[id] = data.id;
                }
                else
                {
                    MapTextureForm.DataById[texId].texs.Add(data.id);
                }
                MapTextureForm.DataById[texId].icon = MapTextureForm.DataById[texId].texs[0];
            }
        });
    }

    public void ImportFrontPartTex(int texId, int id = -1)
    {
        UiManager.instance.ShowUi<UiModAssetSelectWindowCtrl>(new UiModAssetSelectTexWindowParam()
        {
            onComplete = (data) =>
            {
                var frontPartTexs = MapTextureForm.DataById[texId].frontPartTexs;
                if (id != -1 && frontPartTexs.Count > id)
                    frontPartTexs[id] = data.id;
                else
                    frontPartTexs.Add(data.id);
            }
        });
    }


    public void DeleteTexId(int texId, int id)
    {
        MapTextureForm.DataById[texId].texs.RemoveAt(id);
    }

    public void DeleteFrontPartTexId(int texId, int id)
    {
        MapTextureForm.DataById[texId].frontPartTexs.RemoveAt(id);
    }

    public void DeleteTex(int texId)
    {
        MapTextureForm.RemoveData(MapTextureForm.DataById[texId].id);
    }
    #endregion
    #region mask
    public void CreateMask(int labId = 0, string name = null)
    {
        if (string.IsNullOrEmpty(name))
        {
            name = StringHelper.GetUniqueName(MapMaskForm.DatasByName.Keys);
        }
        MapMaskForm.AddData(new MapMaskForm.Data(-1, name, GlobalDefaultHelper.DefaultTexId, new List<int>() { GlobalDefaultHelper.DefaultTexId, GlobalDefaultHelper.DefaultTexId, GlobalDefaultHelper.DefaultTexId, GlobalDefaultHelper.DefaultTexId, GlobalDefaultHelper.DefaultTexId, GlobalDefaultHelper.DefaultTexId }, labId));
    }
    public void ImportMask(int maskId, int id)
    {
        UiManager.instance.ShowUi<UiModAssetSelectWindowCtrl>(new UiModAssetSelectTexWindowParam()
        {
            onComplete = (data) =>
            {
                if (MapMaskForm.DataById[maskId].texsName.Count > id)
                {
                    MapMaskForm.DataById[maskId].texsName[id] = data.id;
                }
                else
                {
                    MapMaskForm.DataById[maskId].texsName.Add(data.id);
                }
            }
        });

    }

    public void DeleteMask(int id)
    {
        MapMaskForm.RemoveData(id);
    }
    #endregion

    #region object
    public void ChooseModel(string title, Action<EntryItem> act)
    {
        var items = new EntryItem();

        foreach (var form in MapPrefabForm.DataById.Values)
        {
            items.Add(form.name, null,GameManager.instance.innerAssetDic[form.innerPrefabName].id);
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

    public void CreateObject(int labId = 0, string name = null)
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
        var modelData = MapModelForm.defaultData.Copy();
        modelData.subUnitTexsName = new List<List<int>>() { new List<int>() };
        var objectData = new MapObjectForm.Data(-1, name, GlobalDefaultHelper.DefaultTexId, modelData, labId, false,
            new Dictionary<string, EventTriggerForm.Data>(), paramDic, GlobalDefaultHelper.DefaultTexId,
            FaceType.Fixed, new Dictionary<AnimDirecton, List<int>>());
        objectData.EnsureDirectionData();
        MapObjectForm.AddData(objectData);
    }
    public void DeleteObjectUnitTex(string name, AnimDirecton direction, int texId)
    {
        var data = MapObjectForm.DataByName[name];
        var clip = data.GetAnimClip(direction);
        if (texId >= 0 && texId < clip.Count)
            clip.RemoveAt(texId);
        data.SyncLegacyAnimClip(direction);
    }
    public void CreateObjectUnitTex(int uid, AnimDirecton direction)
    {
        var data = MapObjectForm.DataById[uid];
        data.GetAnimClip(direction).Add(GlobalDefaultHelper.DefaultTexId);
        data.SyncLegacyAnimClip(direction);
    }
    public void ImportObjectUnitTex(int uid, AnimDirecton direction, int texId)
    {
        UiManager.instance.ShowUi<UiModAssetSelectWindowCtrl>(new UiModAssetSelectTexWindowParam()
        {
            onComplete = (assetData) =>
            {
                var objectData = MapObjectForm.DataById[uid];
                var clip = objectData.GetAnimClip(direction);
                if (texId >= 0 && clip.Count > texId)
                {
                    clip[texId] = assetData.id;
                }
                else
                {
                    clip.Add(assetData.id);
                }
                objectData.SyncLegacyAnimClip(direction);
            }
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
    public void ImportObjectMinimap(int uid)
    {
        UiManager.instance.ShowUi<UiModAssetSelectWindowCtrl>(new UiModAssetSelectTexWindowParam()
        {
            onComplete = (data) =>
            {
                MapObjectForm.DataById[uid].minimapIcon = data.id;
            }
        });

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
                items.Add(data.name, TexAssetForm.DataById[data.avatarTex].GetSprite());
            }
        }
        NotifyManager.instance.AddChoose(title,
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
                items.Add(data.name, TexAssetForm.DataById[data.avatarTex].GetSprite());
            }
        }
        NotifyManager.instance.AddChoose(title,
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
                CharacterProductForm.DataByUid[uid].avatarTex = data.id;
            }
        });
    }
    public void ImportCharacterIllustration(int characterUid)
    {
        UiManager.instance.ShowUi<UiModAssetSelectWindowCtrl>(new UiModAssetSelectTexWindowParam()
        {
            onComplete = (data) =>
            {
                CharacterProductForm.DataByUid[characterUid].illustration = data.id;
            }
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
             new Dictionary<BodyPartType, int>() { { BodyPartType.None, GlobalDefaultHelper.DefaultTexId }, { BodyPartType.UpperPart, GlobalDefaultHelper.DefaultTexId }, { BodyPartType.LowerPart, GlobalDefaultHelper.DefaultTexId} });
    }


    public void CreateCharacter(int labId = 0, string name = null)
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
        CharacterProductForm.AddData(new CharacterProductForm.Data(-1, name, labId, GlobalDefaultHelper.DefaultCharacterTexId, paramDic, 0, animDic, defaultAnimName, default, "", "", new Dictionary<string, EventTriggerForm.Data>(), new Dictionary<EquipPartType, int>(), "", GlobalDefaultHelper.DefaultCharacterTexId, false, new Dictionary<SkillType, int>(), 0, false, GlobalDefaultHelper.DefaultTexId, 1, new List<int>()));
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
                    anim.animClip[dir][id].partTex[part] = data.id;
                }
            }
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
    public void ImportCharacterMinimap(int uid)
    {
        UiManager.instance.ShowUi<UiModAssetSelectWindowCtrl>(new UiModAssetSelectTexWindowParam()
        {
            onComplete = (data) =>
            {
                CharacterProductForm.DataByUid[uid].minimapIcon = data.id;
            }
        });

    }
    #endregion

    #region effect
    public List<EffectClipForm.Data> CreateEffectClips(int texId = -1)
    {
        return new List<EffectClipForm.Data>() { CreateEffectClip(texId) };
    }
    public EffectClipForm.Data CreateEffectClip(int texId = -1)
    {
        return new EffectClipForm.Data(-1, texId==-1 ? GlobalDefaultHelper.DefaultTexId : texId, 1, Vector3.zero, 0, Vector3.one, 1, true);
    }
    public void DeleteEffectClips(int effectUid, int id)
    {
        EffectForm.DataByUid[effectUid].clips.RemoveAt(id);
    }
    public void DeleteEffectClip(int effectUid, int id1, int id2)
    {
        EffectForm.DataByUid[effectUid].clips[id1].RemoveAt(id2);
    }
    public void CreateEffect(int labId = 0, string name = null)
    {
        if (string.IsNullOrEmpty(name))
        {
            name = StringHelper.GetUniqueName(EffectForm.DataByName.Keys);
        }
        EffectForm.AddData(new EffectForm.Data(-1, name, labId, new List<List<EffectClipForm.Data>>() { new List<EffectClipForm.Data>() { CreateEffectClip(GlobalDefaultHelper.DefaultTexId) } }, false));
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
                    clip.tex = data.id;
                }
            }
        });
    }
    public void ChooseEffect(string title, Action<EffectForm.Data> act)
    {
        var items = new EntryItem();

        foreach (var data in EffectForm.DataByUid.Values)
        {
            items.Add(data.name, TexAssetForm.DataById[data.clips[0][0].tex].GetSprite(), data.uid);
        }
        NotifyManager.instance.AddChoose(title,
            true, (item) =>
            {
                act?.Invoke(EffectForm.DataByUid[item.id]);
                return true;
            }, items);
    }
    #endregion
    #region skill
    public void CreateSkill(int labId = 0, string name = null)
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
        SkillProductForm.AddData(new SkillProductForm.Data(-1, name, labId, 0, GlobalDefaultHelper.DefaultTexId, paramDic, new List<SkillType>(), 0, 1, 0, new Dictionary<string, EventTriggerForm.Data>(), 0));
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
                skillData.icon = data.id;
            }
        });
    }
    public void ChooseSkill(string title, Action<SkillProductForm.Data> act)
    {
        var items = new EntryItem();

        foreach (var data in SkillProductForm.DataByUid.Values)
        {
            items.Add(data.name, TexAssetForm.DataById[data.icon].GetSprite(), data.uid);
        }
        NotifyManager.instance.AddChoose(title,
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

    public void CreateEvent(int labId = 0, string name = "")
    {
        if (string.IsNullOrEmpty(name))
        {
            name = StringHelper.GetUniqueName(EventProgramDataForm.DatasByName.Keys);
        }
        EventProgramDataForm.AddData(new EventProgramDataForm.Data(-1, name, "", new List<string>(), new List<int>(), 0, "void", labId));
    }
    public void DeleteEvent(string name)
    {
        var data = EventProgramDataForm.DataByUid.Values.FirstOrDefault(d => d.name == name);
        if (data != null)
            EventProgramDataForm.RemoveData(data.uid);
    }
    public void ImportClipTex(Action<string> callback)
    {
        UiManager.instance.ShowUi<UiModAssetSelectWindowCtrl>(new UiModAssetSelectTexWindowParam()
        {
            onComplete = (data) =>
            {
                callback?.Invoke(data.name);
            }
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
                ItemProductForm.DataByUid[itemUid].iconTexName = data.id;
            }
        });

    }



    public void CreateItem(int labId = 0, string name = null)
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
        model.subUnitTexsName = new List<List<int>>() { new List<int>() };
        model.isObstacle = false;
        var styleTex = new Dictionary<ItemStyle, int>();
        foreach (ItemStyle style in Enum.GetValues(typeof(ItemStyle)))
        {
            styleTex[style] = GlobalDefaultHelper.DefaultTexId;
        }
        ItemProductForm.AddData(new ItemProductForm.Data(-1, name, labId, GlobalDefaultHelper.DefaultTexId, dic, 0, model, "", 1, 99, default, styleTex, 0, false, new Dictionary<string, EventTriggerForm.Data>(), true, new Dictionary<string, CharacterParamForm.Data>(), GlobalDefaultHelper.DefaultTexId));
    }
    public void DeleteItem(int itemUid)
    {
        ItemProductForm.RemoveData(ItemProductForm.DataByUid[itemUid].uid);
    }

    public void DeleteItemModelUnitTex(int itemUid, int texId)
    {
        var data = ItemProductForm.DataByUid[itemUid];
        data.model.subUnitTexsName[0].RemoveAt(texId);
    }
    public void CreateItemModelUnitTex(int itemUid)
    {
        var data = ItemProductForm.DataByUid[itemUid];
        data.model.subUnitTexsName[0].Add(GlobalDefaultHelper.DefaultTexId);
    }
    public void ImportItemModelUnitTex(int itemUid, int texId)
    {
        UiManager.instance.ShowUi<UiModAssetSelectWindowCtrl>(new UiModAssetSelectTexWindowParam()
        {
            onComplete = (data) =>
            {
                var itemData = ItemProductForm.DataByUid[itemUid];
                var model = itemData.model;
                if (model.subUnitTexsName[0].Count > texId)
                {
                    model.subUnitTexsName[0][texId] = data.id;
                }
                else
                {
                    model.subUnitTexsName[0].Add(data.id);
                }
            }
        });

    }
    public void ImportItemStyleTex(int itemUid, ItemStyle style)
    {
        UiManager.instance.ShowUi<UiModAssetSelectWindowCtrl>(new UiModAssetSelectTexWindowParam()
        {
            onComplete = (data) =>
            {
                var itemData = ItemProductForm.DataByUid[itemUid];
                itemData.styleTex[style] = data.id;
            }
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
                items.Add(data.name, TexAssetForm.DataById[data.iconTexName].GetSprite());
            }
        }
        NotifyManager.instance.AddChoose(title,
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
    public void ChooseSceneObject(string title, Action<MapObjectForm.Data> act)
    {
        var items = new EntryItem();
        var addedIds = new HashSet<int>();

        foreach (var data in MapObjectForm.DataById.Values)
        {
            if (data == null || !addedIds.Add(data.id))
                continue;
            items.Add($"{data.name}", null, data.id);
        }
        NotifyManager.instance.AddChoose(title,
            true, (item) =>
            {
                act?.Invoke(MapObjectForm.DataById.GetDv(item.id, null));
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

    public void ImportItemMinimap(int uid)
    {
        UiManager.instance.ShowUi<UiModAssetSelectWindowCtrl>(new UiModAssetSelectTexWindowParam()
        {
            onComplete = (data) =>
            {
                ItemProductForm.DataByUid[uid].minimapIcon = data.id;
            }
        });

    }
    #endregion

    #region scene placement
    public TileUnitForm.Data AddTile(int x, int y, int height)
    {
        Vector3 mapPos = GameManager.PlayerPosToMapPos(new Vector3(x, height, y));
        return AddTile(Vector3Int.RoundToInt(mapPos));
    }

    public TileUnitForm.Data AddTile(Vector3Int mapPos)
    {
        var mapManager = MapManager.instance;
        if (mapManager.data == null
            || !mapManager.utilCtrl.InLimit(mapPos)
            || mapManager.data.maps.ContainsKey((mapPos.x, mapPos.y, mapPos.z)))
        {
            return null;
        }

        return mapManager.AddTile(mapPos);
    }

    public ObjectUnitForm.Data AddObject(MapObjectForm.Data data, Vector3 position, float angle = 0)
    {
        if (data == null || !TryGetPlacementTile(position, out TileUnitForm.Data tile))
            return null;

        var mapManager = MapManager.instance;
        foreach (ObjectUnit current in mapManager.updateCtrl.objectTileDic.Get(tile.unit))
        {
            if (current.data.name == data.name
                && (current.data.pos - position).sqrMagnitude < 0.001f
                && Mathf.Abs(current.data.euler.y - angle) < 1f)
            {
                return null;
            }
        }

        ObjectUnitForm.Data result = mapManager.AddObject(
            data.name,
            position,
            GlobalDefaultHelper.GetRuntimeMapObjectPrefabName(data.id),
            null);
        if (result == null)
            return null;

        GameManager.instance.mapCtrl.RegisterObject(result, data);
        mapManager.updateCtrl.ApplyMove(
            result.unit,
            result.pos,
            new Vector3(result.euler.x, angle, result.euler.z),
            true);

        MapManager.instance.updateCtrl.UpdateSingleOne(result.unit);
        return result;
    }

    public ItemUnitForm.Data AddItem(ItemProductForm.Data data, Vector3 position, float angle = 0)
    {
        if (data == null || !TryGetPlacementTile(position, out TileUnitForm.Data tile))
            return null;

        var mapManager = MapManager.instance;
        foreach (ItemUnit current in mapManager.updateCtrl.itemTileDic.Get(tile.unit))
        {
            if (current.data.name == data.name
                && (current.data.pos - position).sqrMagnitude < 0.001f
                && Mathf.Abs(current.data.euler.y - angle) < 1f)
            {
                return null;
            }
        }

        ItemUnitForm.Data result = mapManager.AddItem(
            data.name,
            position,
            GlobalDefaultHelper.GetRuntimeMapItemPrefabName(data.uid),
            null);
        if (result == null)
            return null;

        result.unit.productInfo = (data.uid, -1);
        result.euler = new Vector3(result.euler.x, angle, result.euler.z);
        return result;
    }

    public CharacterUnitForm.Data AddCharacter(CharacterProductForm.Data data, Vector3 position, float angle = 0)
    {
        if (data == null || !TryGetPlacementTile(position, out TileUnitForm.Data tile))
            return null;

        var mapManager = MapManager.instance;
        foreach (CharacterUnit current in mapManager.updateCtrl.characterTileDic.Get(tile.unit))
        {
            if (current.data.name == data.name
                && (current.data.pos - position).sqrMagnitude < 0.001f
                && Mathf.Abs(current.data.euler.y - angle) < 1f)
            {
                return null;
            }
        }

        CharacterUnitForm.Data result = mapManager.AddCharacter(
            data.name,
            position,
            GlobalDefaultHelper.GetRuntimePrefabName("character"),
            false,
            MapUnit.GetProductInfoString(new Newtonsoft.Json.Linq.JObject(), (data.uid, -1)),
            GameMapData.GetCharacterProductSize(data));
        if (result == null)
            return null;

        result.euler = new Vector3(result.euler.x, angle, result.euler.z);
        return result;
    }

    private static bool TryGetPlacementTile(Vector3 position, out TileUnitForm.Data tile)
    {
        tile = null;
        var mapManager = MapManager.instance;
        if (mapManager.data == null)
            return false;

        Vector3Int mapPos = mapManager.utilCtrl.RealPos2MapPosInt(position);
        return mapManager.data.maps.TryGetValue((mapPos.x, mapPos.y, mapPos.z), out tile);
    }
    #endregion

    #region scene
    public void ImportMapMiniMap()
    {
        UiManager.instance.ShowUi<UiModAssetSelectWindowCtrl>(new UiModAssetSelectTexWindowParam()
        {
            onComplete = (data) =>
            {
                GameManager.instance.curProgress.largeMap = data.id;
            },
        });

    }

    public void CreateScene(string name = "")
    {
        if (string.IsNullOrEmpty(name))
        {
            name = StringHelper.GetUniqueName(SceneForm.DataByName.Keys);
        }
        SceneForm.AddData(new SceneForm.Data(-1, name, GlobalDefaultHelper.DefaultTexId, Vector2.zero,false,false, new Dictionary<string, EventTriggerForm.Data>(), new Dictionary<int, List<string>>(), new Dictionary<int, List<string>>(), false));
    }

    public void ImportSceneMiniMap(string name)
    {
        UiManager.instance.ShowUi<UiModAssetSelectWindowCtrl>(new UiModAssetSelectTexWindowParam()
        {
            onComplete = (data) =>
            {
                SceneForm.DataByName[name].miniMap = data.id;
            },
        });

    }
    public void DeleteScene(int uid)
    {
        SceneForm.RemoveData(uid);
        GameManager.instance.saveCtrl.DeleteSceneMap(ModManager.instance.GetStoryCoreFolder(), uid);
    }

    public void ChooseScene(string title, Action<SceneForm.Data> act)
    {
        var items = new EntryItem();

        foreach (var data in SceneForm.DataByUid.Values)
        {
            items.Add(data.name, StoryTexAssetForm.DataById.GetDv(data.miniMap, StoryTexAssetForm.DataById[GlobalDefaultHelper.ExternDefaultTexId]).GetSprite(), data.uid);
        }
        NotifyManager.instance.AddChoose(title,
            true, (item) =>
            {
                act?.Invoke(SceneForm.DataByUid[item.id]);
                return true;
            }, items);
    }
    #endregion

}
