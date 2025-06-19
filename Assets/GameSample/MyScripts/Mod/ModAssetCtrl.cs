using Form;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Z_DataSystem;
using Z_DataSystem.Form;
using Z_DesignStyle;
using Z_Map;
using Z_String;
namespace Form
{
    public  static partial class StoryTexAssetForm
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
        AssetManager.instance.SelectTex(new Vector2Int(100, 100), (form) =>
        {
            StoryTexAssetForm.AddData(form);
            StoryForm.DataById[1].icon = form.name;
            Z_EventHelper.Invoke(new AssetEvent()
            {
                importAssetName = form.name
            });
        });

    }

    #endregion

    #region param
    public void CreateGlobalArg(string name)
    {
        if (GlobalParamForm.DataByName.Keys.Count > GlobalMaxSettings.GLOBAL_PARAM_MAX)
            return;

        if (string.IsNullOrEmpty(name))
        {
            name=StringHelper.GetUniqueName(GlobalParamForm.DataByName.Keys);
        }
        GlobalParamForm.AddData(new GlobalParamForm.Data(-1, name, 0, 0f, 0f, 1f, 0));
    }
    public void DeleteGlobalArg(string name)
    {
        
        GlobalParamForm.RemoveData(CharacterParamForm.DataByName[name].uid);
    }
    public void CreateCharacterArg(string name)
    {
        if (CharacterParamForm.DataByName.Keys.Count > GlobalMaxSettings.CHARACTER_PARAM_MAX)
            return;

        if (string.IsNullOrEmpty(name))
        {
            name = StringHelper.GetUniqueName(CharacterParamForm.DataByName.Keys);
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


    public void CreateItemArg(string name)
    {
        if (ItemParamForm.DataByName.Keys.Count > GlobalMaxSettings.ITEM_PARAM_MAX)
            return;

        if (string.IsNullOrEmpty(name))
        {
            name = StringHelper.GetUniqueName(ItemParamForm.DataByName.Keys);
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



    #endregion


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
        MapTextureForm.AddData(new MapTextureForm.Data(-1, name, "", 1, new List<string>() { "" },""));
    }
    public void ImportAnimTex(string name, int id)
    {
        AssetManager.instance.SelectTex(new Vector2Int(100, 100), (form) =>
         {
             GameManager.instance.saveCtrl.AddStoryTex(form);

             if (MapTextureForm.DataByName[name].texsName.Count > id)
             {
                 MapTextureForm.DataByName[name].texsName[id] = form.name;
             }
             else
             {
                 MapTextureForm.DataByName[name].texsName.Add(form.name);
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
        MapMaskForm.AddData(new MapMaskForm.Data(-1, name, "", new List<string>() { "", "", "", "", "", "" }, ""));
    }
    public void ImportMaskTex(string name, int id)
    {
        AssetManager.instance.SelectTex(new Vector2Int(100, 100), (form) =>
        {
            GameManager.instance.saveCtrl.AddStoryTex(form);


            if (MapMaskForm.DataByName[name].texsName.Count > id)
            {
                MapMaskForm.DataByName[name].texsName[id] = form.name;
            }
            else
            {
                MapMaskForm.DataByName[name].texsName.Add(form.name);
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

        MapObjectForm.AddData(new MapObjectForm.Data(-1, name, "", MapModelForm.defaultData,false, ""));
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

        AssetManager.instance.SelectTex(new Vector2Int(100, 100), (form) =>
        {
            GameManager.instance.saveCtrl.AddStoryTex(form);

            if (MapObjectForm.DataByName[name].model.subUnitTexsName.Count > id)
            {
                MapObjectForm.DataByName[name].model.subUnitTexsName[id] = form.name;
            }
            else
            {
                MapObjectForm.DataByName[name].model.subUnitTexsName.Add(form.name);
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

    
    public void ImportCharacterAvatar(string name)
    {

        AssetManager.instance.SelectTex(new Vector2Int(100, 100), (form) =>
        {
            GameManager.instance.saveCtrl.AddStoryTex(form);


            CharacterProductForm.DataByNameIsproto[(name, true)].avatarTexName = form.name;
        });

    }
    private CharacterAnimForm.Data CreateCharacterAnim(string name)
    {
        return new CharacterAnimForm.Data(0, name, new List<CharacterAnimClipForm.Data>(), 0.2f, 1f);
    }
    private CharacterAnimClipForm.Data CreateCharacterAnimClip()
    {
       return new CharacterAnimClipForm.Data(0,
            new Dictionary<EquipPartType,ItemStyle>(), 
            new Dictionary<EquipPartType, (float, float,int,float)>(),
            new Dictionary<BodyPartType, bool>() { { BodyPartType.UpperPart,true },{ BodyPartType.LowerPart, false } },
            new Dictionary<BodyPartType, string>() { { BodyPartType.UpperPart, "" }, { BodyPartType.LowerPart, "" } });
    }


    public void CreateCharacter(string name=null)
    {
        if (string.IsNullOrEmpty(name))
        {
            name = StringHelper.GetUniqueName(CharacterParamForm.DataByName.Keys);
        }
        var animDic = new Dictionary<string, CharacterAnimForm.Data>() { { "new1", CreateCharacterAnim("new1") } } ;
        var paramDic = new Dictionary<string, CharacterParamForm.Data>();
        foreach (var prm in CharacterParamForm.DataByName.Values)
        {
            paramDic[prm.name] = prm.Copy();
        }
        CharacterProductForm.AddData(new CharacterProductForm.Data(-1, name, "", "", paramDic, true, animDic, "", "", "", "","","",""));
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

    public bool DeleteCharacterAnimId(string name, string animNm, int id)
    {
        var data = CharacterProductForm.DataByNameIsproto[(name, true)];
        var anim = data.animDic[animNm];

        anim.animClip.RemoveAt(id);

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
    public void ImportCharacterAnim(string characterName, string animNm, BodyPartType part, int id)
    {

        AssetManager.instance.SelectTex(new Vector2Int(100, 100), (form) =>
        {
            GameManager.instance.saveCtrl.AddStoryTex(form);


            var data = CharacterProductForm.DataByNameIsproto[(characterName, true)];
            var anim = data.animDic[animNm];
            if (anim.animClip.Count > id)
            {
                anim.animClip[id].partTex[part] = form.name;
            }
        });
    }
    public void CreateCharacterAnim(string name, string animName=null)
    {
        var data = CharacterProductForm.DataByNameIsproto[(name, true)];
        if (data.animDic.Keys.Count > GlobalMaxSettings.CHARACTER_ANIM_MAX)
            return;
        if (string.IsNullOrEmpty(animName))
        {
            animName = StringHelper.GetUniqueName(data.animDic.Keys);
        }
        data.animDic[animName] = CreateCharacterAnim(animName);

    }
    public void CreateCharacterAnimId(string name, string animNm)
    {
        var data = CharacterProductForm.DataByNameIsproto[(name,true)];
        var anim = data.animDic[animNm];
        anim.animClip.Add(CreateCharacterAnimClip());
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
        AssetManager.instance.SelectTex(callback:(form) =>
        {
            GameManager.instance.saveCtrl.AddStoryTex(form);


            callback?.Invoke(form.name);
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

   

    public void ImportItemIcon(string name)
    {

        AssetManager.instance.SelectTex(new Vector2Int(100, 100), (form) =>
        {
            GameManager.instance.saveCtrl.AddStoryTex(form);


            ItemProductForm.DataByNameIsproto[(name, true)].iconTexName = form.name;
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
        ItemProductForm.AddData(new ItemProductForm.Data(-1, name, "", "", dic, true, model,"",1,1,default,new Dictionary<ItemStyle,string>()));
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

        AssetManager.instance.SelectTex(new Vector2Int(100, 100), (form) =>
        {
            GameManager.instance.saveCtrl.AddStoryTex(form);


            var data = ItemProductForm.DataByNameIsproto[(name, true)];
            if (data.model.subUnitTexsName.Count > id)
            {
                data.model.subUnitTexsName[id] = form.name;
            }
            else
            {
                data.model.subUnitTexsName.Add(form.name);
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
