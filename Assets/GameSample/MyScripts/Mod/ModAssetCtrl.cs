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
        MapTextureForm.AddData(new MapTextureForm.Data(-1, name, "", 1, new List<string>() {""}));
    }
    public void ImportAnimTex(string name,int id)
    {
        AssetManager.instance.SelectTex(new Vector2Int(100, 100),(v,nm)=>
        {
            if(MapTextureForm.DataByName[name].texsName.Count>id)
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
        MapMaskForm.AddData(new MapMaskForm.Data(-1, name, "", new List<string>() {"","","","","",""}));
    }
public void ImportMaskTex(string name,int id)
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
        MapObjectForm.AddData(new MapObjectForm.Data(-1, name, "", true, new List<string>(), new List<Vector3>(), new List<Vector3>(), new List<string>()));
    }
    public void CreateObjectUnit(MapObjectForm.Data data)
    {
        ImportObjectTex(data.name, data.subPrefabUnitName.Count);

        data.subPrefabUnitName.Add("Cube");
        data.subPrefabUnitPos.Add(Vector3.zero);
        data.subPrefabUnitScale.Add(Vector3.one);
    }
    public void ImportObjectTex(string name,int id)
    {
       
        AssetManager.instance.SelectTex(new Vector2Int(100, 100), (v,nm) =>
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

    public void CreateCharacterArg(string name)
    {
        CharacterParamForm.AddData(new CharacterParamForm.Data(-1, name, 0, 0));
    }
    public void DeleteCharacterArg(string name)
    {
        CharacterParamForm.RemoveData(CharacterParamForm.DataByName[name].uid);
    }

    public void ImportCharacterAvatar(string name)
    {

        AssetManager.instance.SelectTex(new Vector2Int(100, 100), (v,nm) =>
        {
             CharacterProductForm.DataByName[name].avatarTexName=nm;
        });

    }
    private string CreateCharacterAnimJo(string name= "newAnim1")
    {
        var data = new CharacterAnimForm.Data(0, name, new List<(float,float)>() { (0,0) }, 0.2f,new List<List<string>>() { new List<string> { "" }, new List<string> { "" } });
        return CharacterAnimForm.GetJoByData(data).ToString();
    }


    public void CreateCharacter(string name)
    {
        CharacterProductForm.AddData(new CharacterProductForm.Data(-1, name,"", new Dictionary<string, (int, int, int)>(), true, new List<string>() { CreateCharacterAnimJo() },"","",""));
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

    public bool DeleteCharacterAnimId(string name, int animId, int part, int id)
    {
        var data = CharacterProductForm.DataByName[name];
        var anim = data.GetCharacterAnim(animId);
        anim.partAnimTexsName[part].RemoveAt(id);
        data.SaveCharacterAnim(animId, anim);
        return false;
    }
    public void DeleteCharacterAnim(string name, int animId)
    {
        CharacterProductForm.DataByName[name].animJo.RemoveAt(animId);
    }

    public void RenameCharacterAnim(string CharacterName, string oldName, string newName)
    {

        //change
        for (int k = 0; k < CharacterProductForm.DataByName[oldName].animJo.Count; k++)
        {
            var data = GlobalDataHelper.GetCharacterAnim(CharacterProductForm.DataByName[oldName],k);
            if (data.name == oldName)
            {
                data.name = newName;
                GlobalDataHelper.SaveCharacterAnim(CharacterProductForm.DataByName[oldName], k, data);
            }
        }
    }
    public void ImportCharacterAnim(string name,int animId,int part,int id)
    {

        AssetManager.instance.SelectTex(new Vector2Int(100, 100), (v,nm) =>
        {
            var data = CharacterProductForm.DataByName[name]; 
            var anim = data.GetCharacterAnim(animId);
            if (anim.partAnimTexsName[part].Count > id)
            {
                anim.partAnimTexsName[part][id] = nm;
            }
            else
            {
                anim.partAnimTexsName[part].Add(nm);
            }
            data.SaveCharacterAnim(animId, anim);
        });
    }
    public void CreateCharacterAnim(string name, string animName)
    {
        var data = CharacterProductForm.DataByName[name];
        data.animJo.Add(CreateCharacterAnimJo(animName));

    }
    public void CreateCharacterAnimId(string name, int animId,int part,int id)
    {
        var data = CharacterProductForm.DataByName[name];
        var anim=data.GetCharacterAnim(animId);
        anim.animPos.Add((0, 0));
        anim.partAnimTexsName[0].Add("");
        anim.partAnimTexsName[1].Add("");
        data.SaveCharacterAnim(animId, anim);

    }
    #endregion
}
