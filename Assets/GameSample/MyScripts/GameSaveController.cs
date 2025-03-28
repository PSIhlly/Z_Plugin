using Form;
using Newtonsoft.Json;
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
using Z_Texture;
using Z_UnitSystem;

public class GameSaveController : Z_Controller<GameManager>
{
    public string mapTextureFormFileName => "mtf";
    public string mapMaskFormFileName => "mmf";
    public string mapObjectFormFileName => "mof";
    public string characterParamFormFileName => "cpaf";
    public string characterProductFormFormFileName => "cprf";
    public GameSaveController(GameManager super):base(super)
    { 
    }
    #region save
    public void SaveMaterial(string storyCoreFolder)
    {

        SaveAndLoad.Save(storyCoreFolder+"/"+ mapTextureFormFileName, MapTextureForm.GetJaByDatas().ToString());
        foreach (var data in MapTextureForm.DataById.Values)
        {
            for(int i=0;i<GlobalMaxSettings.TEX_ANIM_MAX;i++)
            {
                var nm = GlobalNameHelper.GetTexRealName(data.name, i);
                if (TexAssetForm.DataByName.ContainsKey(nm))
                {
                    var tex = TexAssetForm.DataByName[nm];
                    SaveAndLoad.Save(storyCoreFolder + "/" + nm, TextureHelper.GetTextureByte((Texture2D)tex.tex));
                }
            }
        }

        SaveAndLoad.Save(storyCoreFolder + "/" + mapMaskFormFileName, MapMaskForm.GetJaByDatas().ToString());
        foreach (var data in MapMaskForm.DataById.Values)
        {
            for (int i = 0; i < Enum.GetValues(typeof(AlphaTexBasic5)).Length; i++)
            {
                var nm = GlobalNameHelper.GetMaskRealName(data.name, i);
                if (TexAssetForm.DataByName.ContainsKey(nm))
                {
                    var tex = TexAssetForm.DataByName[nm];
                    SaveAndLoad.Save(storyCoreFolder + "/" + nm, TextureHelper.GetTextureByte((Texture2D)tex.tex));
                }
            }
        }
    }
    public void SaveObject(string storyCoreFolder)
    {

        SaveAndLoad.Save(storyCoreFolder + "/" + mapObjectFormFileName, MapObjectForm.GetJaByDatas().ToString());
        foreach (var data in MapObjectForm.DataById.Values)
        {
            for (int i = 0; i < GlobalMaxSettings.OBJECT_UNIT_MAX; i++)
            {
                var nm = GlobalNameHelper.GetObjectTexRealName(data.name, i);
                if (TexAssetForm.DataByName.ContainsKey(nm))
                {
                    var tex = TexAssetForm.DataByName[nm];
                    SaveAndLoad.Save(storyCoreFolder + "/" + nm, TextureHelper.GetTextureByte((Texture2D)tex.tex));
                }
            }
        }
    }

    public void SaveCharacter(string storyCoreFolder)
    {
        SaveAndLoad.Save(storyCoreFolder + "/" + characterProductFormFormFileName, CharacterProductForm.GetJaByDatas().ToString());
        foreach (var data in CharacterProductForm.DataByUid.Values)
        {
            for (int k = 0;k < data.animJo.Count;k ++)
            {
                var anim = data.GetCharacterAnim(k);
                for (int i = 0; i < GlobalMaxSettings.CHARACTER_ANIM_MAX; i++)
                    for (int j = 0; j < GlobalMaxSettings.CHARACTER_PART_MAX; j++)
                    {
                        var nm = GlobalNameHelper.GetCharacterAnimName(data.name, anim.name,j, i);
                        if (TexAssetForm.DataByName.ContainsKey(nm))
                        {
                            var tex = TexAssetForm.DataByName[nm];
                            SaveAndLoad.Save(storyCoreFolder + "/"+nm, TextureHelper.GetTextureByte((Texture2D)tex.tex));
                        }
                    }

            }
        }
    }
    public void SaveScene(string scenePath)
    {
        SaveAndLoad.Save(scenePath, JsonConvert.SerializeObject(MapManager.instance.data.GetJsonData()));
    }
    #endregion

    #region load
    public void LoadMaterial(string storyCoreFolder)
    {

        MapTextureForm.GetDatasByJa(JArray.Parse(SaveAndLoad.Load<string>(storyCoreFolder + "/" + mapTextureFormFileName)));
        foreach (var data in MapTextureForm.DataById.Values)
        {
            for (int i = 0; i < GlobalMaxSettings.TEX_ANIM_MAX; i++)
            {
                var nm = GlobalNameHelper.GetTexRealName(data.name, i);
                var path = storyCoreFolder + "/" + nm;
                if (SaveAndLoad.Exist(path))
                {
                    AssetManager.instance.LoadTexBytes(SaveAndLoad.Load<byte[]>(path), nm);
                }
            }
        }
        MapMaskForm.GetDatasByJa(JArray.Parse(SaveAndLoad.Load<string>(storyCoreFolder + "/" + mapMaskFormFileName)));
        foreach (var data in MapMaskForm.DataById.Values)
        {
            for (int i = 0; i < Enum.GetValues(typeof(AlphaTexBasic5)).Length; i++)
            {
                var nm = GlobalNameHelper.GetMaskRealName(data.name, i);
                var path = storyCoreFolder + "/" + nm;
                if (SaveAndLoad.Exist(path))
                {
                    AssetManager.instance.LoadTexBytes(SaveAndLoad.Load<byte[]>(path), nm);
                }
            }
        }
    }
    public void LoadObject(string storyCoreFolder)
    {
        MapObjectForm.GetDatasByJa(JArray.Parse(SaveAndLoad.Load<string>(storyCoreFolder + "/" + mapObjectFormFileName)));
        foreach (var data in MapObjectForm.DataById.Values)
        {
            for (int i = 0; i < GlobalMaxSettings.OBJECT_UNIT_MAX; i++)
            {
                var nm = GlobalNameHelper.GetObjectTexRealName(data.name, i);
                var path = storyCoreFolder + "/" + nm;
                if (SaveAndLoad.Exist(path))
                {
                    AssetManager.instance.LoadTexBytes(SaveAndLoad.Load<byte[]>(path), nm);
                }
            }
        }
    }

    public void LoadCharacter(string storyCoreFolder)
    {
        CharacterProductForm.GetDatasByJa(JArray.Parse(SaveAndLoad.Load<string>(storyCoreFolder + "/" + characterProductFormFormFileName)));
        foreach (var data in CharacterProductForm.DataByUid.Values)
        {
            for (int k = 0; k < data.animJo.Count; k++)
            {
                var anim = data.GetCharacterAnim(k);
                for (int i = 0; i < GlobalMaxSettings.CHARACTER_ANIM_MAX; i++)
                    for (int j = 0; j < GlobalMaxSettings.CHARACTER_PART_MAX; j++)
                    {
                        var nm = GlobalNameHelper.GetCharacterAnimName(data.name, anim.name, j, i);
                        var path = storyCoreFolder + "/" + nm;
                        if (SaveAndLoad.Exist(path))
                        {
                            AssetManager.instance.LoadTexBytes(SaveAndLoad.Load<byte[]>(path), nm);
                        }
                    }
            }
        }
    }
    public MapData LoadScene(string scenePath)
    {
        return new MapData(SaveAndLoad.Load<string>(scenePath));
    }
    #endregion

    public void ResetPrefabPool()
    {
        InstancePoolManager.instance.Clear();
        foreach (var form in GameObjectAssetForm.DataById.Values)
        {
            if (form.name.StartsWith(GlobalNameHelper.GetInternalPrefabName("")))
            {
                InstancePoolManager.instance.AddPool(form.go);
            }
        }
        foreach (var form in MapObjectForm.DataById.Values)
        {
            List<string> texNameLst = new List<string>();
            List<bool> showShaddowLst = new List<bool>();
            for (int i = 0; i < form.subPrefabUnitName.Count; i++)
            {
                texNameLst.Add(GlobalNameHelper.GetObjectTexRealName(form.name, i));
                showShaddowLst.Add(true);
            }
            InstancePoolManager.instance.AddPool(_super.utilCtrl.CombineNewItemByPrefabs(form.name, form.subPrefabUnitName, texNameLst, form.subPrefabUnitPos, form.subPrefabUnitScale, showShaddowLst, true));
        }
    }


}
