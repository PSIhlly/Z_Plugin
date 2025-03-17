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


public class ModAssetCtrl : Z_Controller<ModManager>
{
    public ModAssetCtrl(ModManager super) :base(super)
    {

    }
    public int modId;

  
    
   
    #region anim
   

    public void ImportAnimTex(string realName, string nickName)
    {
        //try del old
        AssetManager.instance.DeleteTexAssetAutoDel(_super.GetStoryFolder(), realName);

        AssetManager.instance.SelectTexToAutoAdd(_super.GetStoryFolder(), realName,new Vector2Int(100,100), (v) =>
        {
            if (!string.IsNullOrEmpty(nickName))
            { 
                MapTextureForm.AddData(new MapTextureForm.Data(-1, nickName, "", 1)); 
                
            }
        });

    }

    public void RenameAnimTex(string oldName, string newName)
    {
        for (int i = 0; i < GlobalSettings.TEX_ANIM_MAX; i++)
        {
            var oldKey = GlobalHelper.GetTexRealName(oldName, i);
            var newKey = GlobalHelper.GetTexRealName(newName, i);
            if (oldKey != newKey && TexAssetForm.DataByName.ContainsKey(oldKey))
            {
                AssetManager.instance.RenameTargetAssetAuto(_super.GetStoryFolder(), oldKey, newKey);
            }
        }

        //change
        MapTextureForm.DataByName[oldName].name = newName;

    }

    /// <summary>
    /// 返回是否完全移除
    /// </summary>
    /// <param name="name"></param>
    /// <param name="animId"></param>
    /// <returns></returns>
    public bool DeleteAnimTex(string name, int animId)
    {
        var key = GlobalHelper.GetTexRealName(name, animId);
        if (TexAssetForm.DataByName.ContainsKey(key))
        {
            //del
            AssetManager.instance.DeleteTexAssetAutoDel(_super.GetStoryFolder(), key);
        }
        int lastExist = -1;

        for (int i = 0; i < GlobalSettings.TEX_ANIM_MAX; i++)
        {
            var cur = GlobalHelper.GetTexRealName(name, i);
            if (TexAssetForm.DataByName.ContainsKey(cur))
            {
                if (lastExist + 1 != i)
                {
                    var now = GlobalHelper.GetTexRealName(name, lastExist + 1);
                    AssetManager.instance.RenameTargetAssetAuto(_super.GetStoryFolder(), cur, now);
                }
                lastExist++;
            }
        }
        if (lastExist == -1 && MapTextureForm.DataByName.ContainsKey(name))
        {
            MapTextureForm.RemoveData(MapTextureForm.DataByName[name].id);
            return true;
        }
        return false;
    }
    #endregion
    #region mask
   
    public void ImportMaskTex(string realName, string nickName)
    {
        //try del old
        AssetManager.instance.DeleteTexAssetAutoDel(_super.GetStoryFolder(), realName);

        AssetManager.instance.SelectTexToAutoAdd(_super.GetStoryFolder(), realName, new Vector2Int(100, 100), (v) =>
        {
            if (!string.IsNullOrEmpty(nickName))
            {
                //fill other
                MapTransitionMaskForm.AddData(new MapTransitionMaskForm.Data(-1, nickName, ""));
                for (int i = 1; i < Enum.GetValues(typeof(AlphaTexBasic5)).Length; i++)
                {
                    var curName = GlobalHelper.GetMaskRealName(nickName, i);
                    AssetManager.instance.LoadTexBytesAutoAdd(v, _super.GetStoryFolder(), curName,new Vector2Int(100,100));
                }
            }
        });

    }

    public void RenameMaskTex(string oldName, string newName)
    {
        for (int i = 0; i < Enum.GetValues(typeof(AlphaTexBasic5)).Length; i++)
        {
            var oldKey = GlobalHelper.GetMaskRealName(oldName, i);
            var newKey = GlobalHelper.GetMaskRealName(newName, i);
            if (oldKey != newKey && TexAssetForm.DataByName.ContainsKey(oldKey))
            {
                AssetManager.instance.RenameTargetAssetAuto(_super.GetStoryFolder(), oldKey, newKey);
            }
        }

        //change
        MapTransitionMaskForm.DataByName[oldName].name = newName;

    }

    /// <summary>
    /// 返回是否完全移除
    /// </summary>
    /// <param name="name"></param>
    /// <param name="animId"></param>
    /// <returns></returns>
    public void DeleteMaskTex(string name)
    {

        for (int i = 0; i < Enum.GetValues(typeof(AlphaTexBasic5)).Length; i++)
        {
            var key = GlobalHelper.GetMaskRealName(name, i);
            if (TexAssetForm.DataByName.ContainsKey(key))
            {
                //del
                AssetManager.instance.DeleteTexAssetAutoDel(_super.GetStoryFolder(), key);
            }
        }
       MapTransitionMaskForm.RemoveData(MapTransitionMaskForm.DataByName[name].id);
    }
    #endregion

    #region item
    public void DeleteItem(string name)
    {
        for (int i = 0; i < GlobalSettings.ITEM_UNIT_MAX; i++)
        {
            var realName = GlobalHelper.GetMaskRealName(name, i);
            AssetManager.instance.DeleteTexAssetAutoDel(_super.GetStoryFolder(), realName);
        }
        MapItemForm.RemoveData(MapItemForm.DataByName[name].id);
    }
    public void DeleteItemUnit(string name,int id)
    {
        var key = GlobalHelper.GetItemTexRealName(name, id);
        if (TexAssetForm.DataByName.ContainsKey(key))
        {
            //del
            AssetManager.instance.DeleteTexAssetAutoDel(_super.GetStoryFolder(), key);
        }
        int lastExist = -1;

        for (int i = 0; i < GlobalSettings.ITEM_UNIT_MAX; i++)
        {
            var cur = GlobalHelper.GetItemTexRealName(name, i);
            if (TexAssetForm.DataByName.ContainsKey(cur))
            {
                if (lastExist + 1 != i)
                {
                    var now = GlobalHelper.GetItemTexRealName(name, lastExist + 1);
                    AssetManager.instance.RenameTargetAssetAuto(_super.GetStoryFolder(), cur, now);
                }
                lastExist++;
            }
        }
        var data=MapItemForm.DataByName[name];
        data.subPrefabUnitName.RemoveAt(id);
        data.subPrefabUnitPos.RemoveAt(id);
        data.subPrefabUnitScale.RemoveAt(id);
    }
    public void CreateItem(string name)
    {
        MapItemForm.AddData(new MapItemForm.Data(-1, name, "", true, new List<string>(), new List<Vector3>(), new List<Vector3>()));
    }
    public void CreateItemUnit(MapItemForm.Data data)
    {
        ImportItemTex(GlobalHelper.GetItemTexRealName(data.name, data.subPrefabUnitName.Count));

        data.subPrefabUnitName.Add("Cube");
        data.subPrefabUnitPos.Add(Vector3.zero);
        data.subPrefabUnitScale.Add(Vector3.one);
    }
    public void ImportItemTex(string realName)
    {
        //try del old
        AssetManager.instance.DeleteTexAssetAutoDel(_super.GetStoryFolder(), realName);

        AssetManager.instance.SelectTexToAutoAdd(_super.GetStoryFolder(), realName, new Vector2Int(100, 100), (v) =>
        {

        });

    }
    public void RenameItem(string oldName, string newName)
    {
        for (int i = 0; i < GlobalSettings.ITEM_UNIT_MAX; i++)
        {
            var oldKey = GlobalHelper.GetMaskRealName(oldName, i);
            var newKey = GlobalHelper.GetMaskRealName(newName, i);
            if (oldKey != newKey && TexAssetForm.DataByName.ContainsKey(oldKey))
            {
                AssetManager.instance.RenameTargetAssetAuto(_super.GetStoryFolder(), oldKey, newKey);
            }
        }
        //change
        MapItemForm.DataByName[oldName].name = newName;

    }
    #endregion
}
