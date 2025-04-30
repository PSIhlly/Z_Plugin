using Form;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Z_DataSystem;
using Z_DataSystem.Form;
using Z_DesignStyle;
using Z_Map;


public class ModAssetCtrl : Z_Controller<ModManager>
{
    public ModAssetCtrl(ModManager super) : base(super)
    {

    }
    public int modId;


    #region anim

    public void CreateAnimTex(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            for (int i = 0; i < GlobalMaxSettings.TEXTURE_ANIM_MAX; i++)
            {
                name = "new tex" + i;
                if (!MapTextureForm.DataByName.ContainsKey(name))
                    break;
            }
        }
        MapTextureForm.AddData(new MapTextureForm.Data(-1, name, "", 1, new List<string>() { "" }));
    }
    public void ImportAnimTex(string name, int id)
    {
        AssetManager.instance.SelectTex(new Vector2Int(100, 100), (v, nm) =>
         {
             if (MapTextureForm.DataByName[name].texsName.Count > id)
             {
                 MapTextureForm.DataByName[name].texsName[id] = nm;
             }
             else
             {
                 MapTextureForm.DataByName[name].texsName.Add(nm);
             }
         });

    }

    public void RenameAnimTex(string oldName, string newName)
    {
        MapTextureForm.DataByName[oldName].name = newName;
    }

    public void DeleteAnimTexId(string name, int animId)
    {
        MapTextureForm.DataByName[name].texsName.RemoveAt(animId);
    }

    public void DeleteAnimTex(string name)
    {
        MapTextureForm.RemoveData(MapTextureForm.DataByName[name].id);
    }
    #endregion
    #region mask
    public void CreateMaskTex(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            for (int i = 0; i < GlobalMaxSettings.TEXTURE_ANIM_MAX; i++)
            {
                name = "new mask" + i;
                if (!MapMaskForm.DataByName.ContainsKey(name))
                    break;
            }
        }
        MapMaskForm.AddData(new MapMaskForm.Data(-1, name, "", new List<string>() { "", "", "", "", "", "" }));
    }
    public void ImportMaskTex(string name, int id)
    {
        AssetManager.instance.SelectTex(new Vector2Int(100, 100), (v, nm) =>
        {
            if (MapMaskForm.DataByName[name].texsName.Count > id)
            {
                MapMaskForm.DataByName[name].texsName[id] = nm;
            }
            else
            {
                MapMaskForm.DataByName[name].texsName.Add(nm);
            }
        });

    }

    public void RenameMaskTex(string oldName, string newName)
    {
        MapMaskForm.DataByName[oldName].name = newName;
    }

    public void DeleteMaskTex(string name)
    {
        MapMaskForm.RemoveData(MapMaskForm.DataByName[name].id);
    }
    #endregion

    #region object
    public void DeleteObject(string name)
    {
        MapObjectForm.RemoveData(MapObjectForm.DataByName[name].id);
    }
    public void DeleteObjectUnit(string name, int id)
    {

        var data = MapObjectForm.DataByName[name];
        data.subPrefabUnitName.RemoveAt(id);
        data.subPrefabUnitPos.RemoveAt(id);
        data.subPrefabUnitScale.RemoveAt(id);
        data.subUnitTexsName.RemoveAt(id);
    }
    public void CreateObject(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            for (int i = 0; i < GlobalMaxSettings.OBJECT_MAX; i++)
            {
                name = "new object" + i;
                if (!MapObjectForm.DataByName.ContainsKey(name))
                    break;
            }
        }

        MapObjectForm.AddData(new MapObjectForm.Data(-1, name, "", true, new List<string>(), new List<Vector3>(), new List<Vector3>(), new List<string>()));
    }
    public void CreateObjectUnit(MapObjectForm.Data data)
    {
        ImportObjectTex(data.name, data.subPrefabUnitName.Count);

        data.subPrefabUnitName.Add("Cube");
        data.subPrefabUnitPos.Add(Vector3.zero);
        data.subPrefabUnitScale.Add(Vector3.one);
    }
    public void ImportObjectTex(string name, int id)
    {

        AssetManager.instance.SelectTex(new Vector2Int(100, 100), (v, nm) =>
        {
            if (MapObjectForm.DataByName[name].subUnitTexsName.Count > id)
            {
                MapObjectForm.DataByName[name].subUnitTexsName[id] = nm;
            }
            else
            {
                MapObjectForm.DataByName[name].subUnitTexsName.Add(nm);
            }
        });

    }
    public void RenameObject(string oldName, string newName)
    {

        CharacterProductForm.DataByName[oldName].name = newName;

    }
    #endregion

    #region character

    public void RenameCharacterParam(string oldName, string newName)
    {
        CharacterParamForm.DataByName[oldName].name = newName;
        foreach (var character in CharacterProductForm.DataByUid.Values)
        {
            foreach (var k in character.paramDic.Keys)
            {
                if (k == oldName)
                {
                    var prm = character.paramDic[k];
                    prm.name = newName;
                    character.paramDic.Remove(k);
                    character.paramDic[newName] = prm;
                }
            }
        }
    }

    public void CreateCharacterArg(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            for (int i = 0; i < GlobalMaxSettings.CHARACTER_PARAM_MAX; i++)
            {
                name = "new arg" + i;
                if (!CharacterParamForm.DataByName.ContainsKey(name))
                    break;
            }
        }
        CharacterParamForm.AddData(new CharacterParamForm.Data(-1, name, 0, 0f, 0f, 1f, 0));
        foreach (var character in CharacterProductForm.DataByUid.Values)
        {
            character.paramDic[name] = new CharacterParamForm.Data(-1, name, 0, 0f, 0f, 1f, 0);
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

    public void ImportCharacterAvatar(string name)
    {

        AssetManager.instance.SelectTex(new Vector2Int(100, 100), (v, nm) =>
        {
            CharacterProductForm.DataByName[name].avatarTexName = nm;
        });

    }
    private CharacterAnimForm.Data CreateCharacterAnim(string name = "new anim1")
    {
        return new CharacterAnimForm.Data(0, name, new List<(float, float)>() { (0, 0) }, 0.2f, new List<List<string>>() { new List<string> { "" }, new List<string> { "" } });
    }


    public void CreateCharacter(string name)
    {
        var tmpAnimNm = "new anim1";
        var dic = new Dictionary<string, CharacterParamForm.Data>();
        foreach (var prm in CharacterParamForm.DataByName.Values)
        {
            dic[prm.name] = new CharacterParamForm.Data(prm.uid, prm.name, prm.valueType, prm.min, prm.v, prm.max, prm.SpecialType);

        }
        CharacterProductForm.AddData(new CharacterProductForm.Data(-1, name, "", dic, true, new Dictionary<string, CharacterAnimForm.Data>() { { tmpAnimNm, CreateCharacterAnim(tmpAnimNm) } }, "", "", "", ""));
    }
    public void DeleteCharacter(string name)
    {
        CharacterProductForm.RemoveData(CharacterProductForm.DataByName[name].uid);
    }
    public void RenameCharacter(string oldName, string newName)
    {
        //change
        CharacterProductForm.DataByName[oldName].name = newName;
    }

    public bool DeleteCharacterAnimId(string name, string animNm, int part, int id)
    {
        var data = CharacterProductForm.DataByName[name];
        var anim = data.animDic[animNm];
        anim.partAnimTexsName[part].RemoveAt(id);
        return false;
    }
    public void DeleteCharacterAnim(string name, string animNm)
    {
        CharacterProductForm.DataByName[name].animDic.Remove(animNm);
    }

    public void RenameCharacterAnim(string characterName, string oldName, string newName)
    {
        var data = CharacterProductForm.DataByName[characterName];
        var anim = data.animDic[oldName];
        data.animDic.Remove(oldName);
        anim.name = newName;
        data.animDic[newName] = anim;
    }
    public void ImportCharacterAnim(string characterName, string animNm, int part, int id)
    {

        AssetManager.instance.SelectTex(new Vector2Int(100, 100), (v, nm) =>
        {
            var data = CharacterProductForm.DataByName[characterName];
            var anim = data.animDic[animNm];
            if (anim.partAnimTexsName[part].Count > id)
            {
                anim.partAnimTexsName[part][id] = nm;
            }
            else
            {
                anim.partAnimTexsName[part].Add(nm);
            }
        });
    }
    public void CreateCharacterAnim(string name, string animName)
    {
        var data = CharacterProductForm.DataByName[name];
        for (int i = 0; i < GlobalMaxSettings.CHARACTER_ANIM_MAX; i++)
        {
            name = "new anim" + i;
            if (!data.animDic.ContainsKey(name))
                break;
        }

        data.animDic[animName] = CreateCharacterAnim(animName);

    }
    public void CreateCharacterAnimId(string name, string animNm, int part, int id)
    {
        var data = CharacterProductForm.DataByName[name];
        var anim = data.animDic[animNm];
        anim.animPos.Add((0, 0));
        anim.partAnimTexsName[0].Add("");
        anim.partAnimTexsName[1].Add("");

    }
    #endregion

    #region event
    public void RenameEvent(string oldName, string newName = null, string newLabel = null, string newSubLabel = null)
    {
        var data = EventForm.DataByName[oldName];
        if (newName != null && data.name != newName)
            data.name = newName;
        if (newLabel != null && data.lab != newLabel)
            data.lab = newLabel;
        if (newSubLabel != null && data.subLab != newSubLabel)
            data.subLab = newSubLabel;

    }

    public void CreateEvent(string name = "", string label = "", string subLabel = "")
    {
        if (string.IsNullOrEmpty(name))
        {
            for (int i = 0; i < GlobalMaxSettings.CUSTOM_EVENT_MAX; i++)
            {
                name = "new event" + i;
                if (!EventForm.DataByName.ContainsKey((name)))
                    break;
            }
        }
        EventForm.AddData(new EventForm.Data(-1, name, new List<CmdForm.Data>() { CmdForm.defaultData }, label, subLabel));
    }

    #endregion
}
