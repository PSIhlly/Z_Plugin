using Form;
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


public class ModAssetManager : Z_MonoManager<ModAssetManager>
{
    public int modId;
    public string GetModPath() => Application.persistentDataPath + "/Mod" + modId + "/"; 

    public string GetTexRealName(string nickName,int animId)
    {
        return "b$" + nickName + "$" + animId;
    }
    public string GetMaskTexRealName(string nickName, int maskId)
    {
        return "a$" + nickName + "$" + maskId;
    }

    public void ImportAnimTex(string name,string nickName)
    {
        AssetManager.instance.SelectTexToAutoAdd(GetModPath(), name,(v)=>
        {
            if(!string.IsNullOrEmpty(nickName))
                MapTextureForm.AddData(new MapTextureForm.Data(-1, nickName, "", 1));
        });

    }

    public void RenameAnimTex(string oldName, string newName)
    {
        for (int i = 0; i < GlobalSettings.ANIM_MAX; i++)
        {
            var oldKey = GetTexRealName(oldName, i);
            var newKey = GetTexRealName(newName, i);
            if (oldKey != newKey&&TexAssetForm.DataByName.ContainsKey(oldKey))
            {
                var data = TexAssetForm.DataByName[oldKey];
                //copy
                AssetManager.instance.LoadTexBytesAutoAdd((Texture2D)TexAssetForm.DataByName[oldKey].tex,GetModPath(), newKey);

                //del
                AssetManager.instance.DeleteTargetAssetAutoDel(GetModPath(), oldKey);
                
            }
        } 
        
        //change
        MapTextureForm.DataByName[oldName].name = newName;

    }
}
