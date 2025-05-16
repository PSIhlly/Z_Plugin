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

        MapObjectForm.AddData(new MapObjectForm.Data(-1, name, "", MapModelForm.defaultData,false));
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
        ImportObjectTex(data.name, data.model.subPrefabUnitName.Count);

        data.model.subPrefabUnitName.Add("Cube");
        data.model.subPrefabUnitPos.Add(Vector3.zero);
        data.model.subPrefabUnitScale.Add(Vector3.one);
    }
    public void ImportObjectTex(string name, int id)
    {

        AssetManager.instance.SelectTex(new Vector2Int(100, 100), (v, nm) =>
        {
            if (MapObjectForm.DataByName[name].model.subUnitTexsName.Count > id)
            {
                MapObjectForm.DataByName[name].model.subUnitTexsName[id] = nm;
            }
            else
            {
                MapObjectForm.DataByName[name].model.subUnitTexsName.Add(nm);
            }
        });

    }
    public void RenameObject(string oldName, string newName)
    {

        CharacterProductForm.DataByNameIsproto[(oldName, true)].name = newName;

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
            CharacterProductForm.DataByNameIsproto[(name, true)].avatarTexName = nm;
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
            dic[prm.name] = prm.Copy();

        }
        CharacterProductForm.AddData(new CharacterProductForm.Data(-1, name, "", dic, true, new Dictionary<string, CharacterAnimForm.Data>() { { tmpAnimNm, CreateCharacterAnim(tmpAnimNm) } }, "", "", "", ""));
    }
    public void DeleteCharacter(string name)
    {
        CharacterProductForm.RemoveData(CharacterProductForm.DataByNameIsproto[(name, true)].uid);
    }
    public void RenameCharacter(string oldName, string newName)
    {
        //change
        CharacterProductForm.DataByNameIsproto[(oldName, true)].name = newName;
    }

    public bool DeleteCharacterAnimId(string name, string animNm, int part, int id)
    {
        var data = CharacterProductForm.DataByNameIsproto[(name, true)];
        var anim = data.animDic[animNm];
        anim.partAnimTexsName[part].RemoveAt(id);
        return false;
    }
    public void DeleteCharacterAnim(string name, string animNm)
    {
        CharacterProductForm.DataByNameIsproto[(name, true)].animDic.Remove(animNm);
    }

    public void RenameCharacterAnim(string characterName, string oldName, string newName)
    {
        var data = CharacterProductForm.DataByNameIsproto[(characterName, true)];
        var anim = data.animDic[oldName];
        data.animDic.Remove(oldName);
        anim.name = newName;
        data.animDic[newName] = anim;
    }
    public void ImportCharacterAnim(string characterName, string animNm, int part, int id)
    {

        AssetManager.instance.SelectTex(new Vector2Int(100, 100), (v, nm) =>
        {
            var data = CharacterProductForm.DataByNameIsproto[(characterName, true)];
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
        var data = CharacterProductForm.DataByNameIsproto[(name, true)];
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
        var data = CharacterProductForm.DataByNameIsproto[(name,true)];
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
    public void ImportClipTex(Action<string> callback)
    {
        AssetManager.instance.SelectTex(callback:(v, nm) =>
        {
            callback?.Invoke(nm);
        });

    }
    #endregion

    #region item

    public void RenameItemParam(string oldName, string newName)
    {
        ItemParamForm.DataByName[oldName].name = newName;
        foreach (var item in ItemProductForm.DataByUid.Values)
        {
            foreach (var k in item.paramDic.Keys)
            {
                if (k == oldName)
                {
                    var prm = item.paramDic[k];
                    prm.name = newName;
                    item.paramDic.Remove(k);
                    item.paramDic[newName] = prm;
                }
            }
        }
    }

    public void CreateItemArg(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            for (int i = 0; i < GlobalMaxSettings.ITEM_PARAM_MAX; i++)
            {
                name = "new arg" + i;
                if (!ItemParamForm.DataByName.ContainsKey(name))
                    break;
            }
        }
        ItemParamForm.AddData(new ItemParamForm.Data(-1, name, 0, 0f, 0f, 1f, 0));
        foreach (var item in ItemProductForm.DataByUid.Values)
        {
            item.paramDic[name] = new ItemParamForm.Data(-1, name, 0, 0f, 0f, 1f, 0);
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

    public void ImportItemIcon(string name)
    {

        AssetManager.instance.SelectTex(new Vector2Int(100, 100), (v, nm) =>
        {
            ItemProductForm.DataByNameIsproto[(name, true)].iconTexName = nm;
        });

    }
   


    public void CreateItem(string name)
    {
        var dic = new Dictionary<string, ItemParamForm.Data>();
        foreach (var prm in ItemParamForm.DataByName.Values)
        {
            dic[prm.name] = prm.Copy();

        }
        var model = MapModelForm.defaultData;
        model.isObstacle = false;
        ItemProductForm.AddData(new ItemProductForm.Data(-1, name, "", dic, true, model,1));
    }
    public void DeleteItem(string name)
    {
        ItemProductForm.RemoveData(ItemProductForm.DataByNameIsproto[(name, true)].uid);
    }

    public void DeleteItemModelUnit(string name, int id)
    {

        var data = ItemProductForm.DataByNameIsproto[(name, true)];
        data.model.subPrefabUnitName.RemoveAt(id);
        data.model.subPrefabUnitPos.RemoveAt(id);
        data.model.subPrefabUnitScale.RemoveAt(id);
        data.model.subUnitTexsName.RemoveAt(id);
    }
    public void CreateItemModelUnit(ItemProductForm.Data data)
    {
        ImportItemModelUnitTex(data.name, data.model.subPrefabUnitName.Count);

        data.model.subPrefabUnitName.Add("Cube");
        data.model.subPrefabUnitPos.Add(Vector3.zero);
        data.model.subPrefabUnitScale.Add(Vector3.one);
    }
    public void ImportItemModelUnitTex(string name, int id)
    {

        AssetManager.instance.SelectTex(new Vector2Int(100, 100), (v, nm) =>
        {
            var data = ItemProductForm.DataByNameIsproto[(name, true)];
            if (data.model.subUnitTexsName.Count > id)
            {
                data.model.subUnitTexsName[id] = nm;
            }
            else
            {
                data.model.subUnitTexsName.Add(nm);
            }
        });

    }
    public void RenameItem(string oldName, string newName)
    {
        //change
        ItemProductForm.DataByNameIsproto[(oldName, true)].name = newName;
    }

   
   
    #endregion
}
