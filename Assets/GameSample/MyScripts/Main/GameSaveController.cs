using Form;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Z_ByteSerialize;
using Z_DataSystem;
using Z_DataSystem.Form;
using Z_Debug;
using Z_DesignStyle;
using Z_Map;
using Z_Texture;
using Z_Ui.Form;
using Z_UnitSystem;

public class GameSaveController : Z_Controller<GameManager>
{
    public string mapTextureFormFileName => "mtf";
    public string mapMaskFormFileName => "mmf";
    public string mapObjectFormFileName => "mof";
    public string storyFormFileName => "sf";
    public string characterParamFormFileName => "cpaf";
    public string characterProductFormFileName => "cprf";
    public string eventFormFileName => "ef";
    public string configFormFileName => "cf";
    public string itemParamFormFileName => "ipaf";
    public string itemProductFormFileName => "iprf";
    public GameSaveController(GameManager super) : base(super)
    {
    }
    #region save

    public void SaveOverview(string storyCoreFolder)
    {

        SaveAndLoad.Save(storyCoreFolder + "/" + storyFormFileName, StoryForm.GetJaByDatas().ToString());
        foreach (var data in StoryForm.DataById.Values)
        {
                var nm = data.icon;
                if (TexAssetForm.DataByName.ContainsKey(nm) && nm != "")
                {
                    var tex = TexAssetForm.DataByName[nm];
                    SaveAndLoad.Save(storyCoreFolder + "/" + nm, TextureHelper.GetTextureByte((Texture2D)tex.tex));
                }
        }
    }

    public void SaveMaterial(string storyCoreFolder)
    {

        SaveAndLoad.Save(storyCoreFolder + "/" + mapTextureFormFileName, MapTextureForm.GetJaByDatas().ToString());

        foreach (var data in MapTextureForm.DataById.Values)
        {
            for (int i = 0; i < data.texsName.Count; i++)
            {
                var nm = data.texsName[i];
                if (TexAssetForm.DataByName.ContainsKey(nm) && nm != "")
                {
                    var tex = TexAssetForm.DataByName[nm];
                    SaveAndLoad.Save(storyCoreFolder + "/" + nm, TextureHelper.GetTextureByte((Texture2D)tex.tex));
                }
            }
        }

        SaveAndLoad.Save(storyCoreFolder + "/" + mapMaskFormFileName, MapMaskForm.GetJaByDatas().ToString());
        foreach (var data in MapMaskForm.DataById.Values)
        {
            for (int i = 0; i < Enum.GetValues(typeof(AlphaTexBasic6)).Length; i++)
            {
                var nm = data.texsName[i];
                if (TexAssetForm.DataByName.ContainsKey(nm) && nm != "")
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
            for (int i = 0; i < data.model.subUnitTexsName.Count; i++)
            {
                var nm = data.model.subUnitTexsName[i];
                if (TexAssetForm.DataByName.ContainsKey(nm) && nm != "")
                {
                    var tex = TexAssetForm.DataByName[nm];
                    SaveAndLoad.Save(storyCoreFolder + "/" + nm, TextureHelper.GetTextureByte((Texture2D)tex.tex));
                }
            }
        }
    }

    public void SaveCharacter(string storyCoreFolder)
    {
        SaveAndLoad.Save(storyCoreFolder + "/" + characterParamFormFileName, CharacterParamForm.GetJaByDatas().ToString());

        SaveAndLoad.Save(storyCoreFolder + "/" + characterProductFormFileName, CharacterProductForm.GetJaByDatas().ToString());
        foreach (var data in CharacterProductForm.DataByUid.Values)
        {
            var icon = data.avatarTexName;
            if (TexAssetForm.DataByName.ContainsKey(icon) && icon != "")
            {
                var tex = TexAssetForm.DataByName[icon];
                SaveAndLoad.Save(storyCoreFolder + "/" + icon, TextureHelper.GetTextureByte((Texture2D)tex.tex));
            }
            foreach (var anim in data.animDic.Values)
            {
               
                    for (int i = 0; i < anim.animClip.Count; i++)
                {
                    foreach (BodyPartType part in Enum.GetValues(typeof(BodyPartType)))
                    {

                        var nm = anim.animClip[i].partTex[part];
                        if (TexAssetForm.DataByName.ContainsKey(nm) && nm != "")
                        {
                            var tex = TexAssetForm.DataByName[nm];
                            SaveAndLoad.Save(storyCoreFolder + "/" + nm, TextureHelper.GetTextureByte((Texture2D)tex.tex));
                        }
                    }
                }

            }
        }
    }
    public void SaveItem(string storyCoreFolder)
    {
        SaveAndLoad.Save(storyCoreFolder + "/" + itemParamFormFileName, ItemParamForm.GetJaByDatas().ToString());

        SaveAndLoad.Save(storyCoreFolder + "/" + itemProductFormFileName, ItemProductForm.GetJaByDatas().ToString());
        foreach (var data in ItemProductForm.DataByUid.Values)
        {
            var icon = data.iconTexName;
            if (TexAssetForm.DataByName.ContainsKey(icon) && icon != "")
            {
                var tex = TexAssetForm.DataByName[icon];
                SaveAndLoad.Save(storyCoreFolder + "/" + icon, TextureHelper.GetTextureByte((Texture2D)tex.tex));
            }
            foreach (var nm in data.model.subUnitTexsName)
            {
                if (TexAssetForm.DataByName.ContainsKey(nm) && nm != "")
                {
                    var tex = TexAssetForm.DataByName[nm];
                    SaveAndLoad.Save(storyCoreFolder + "/" + nm, TextureHelper.GetTextureByte((Texture2D)tex.tex));
                }

            }
        }
    }
    public void SaveEvent(string storyCoreFolder, EventProgramDataForm.Data data = null)
    {
        SaveAndLoad.Save(storyCoreFolder + "/" + eventFormFileName, EventProgramDataForm.GetJaByDatas().ToString());
        if (data != null)
        {
            /*foreach (var cmd in data.cmds)//获取常量图片
            {
            }*/
        }

    }
    public void SaveScene(string scenePath)
    {
        SaveAndLoad.Save(scenePath, JsonConvert.SerializeObject(MapManager.instance.data.GetJsonData()));
    }
    public void SaveScene(string scenePath, MapData data)
    {
        SaveAndLoad.Save(scenePath, JsonConvert.SerializeObject(data.GetJsonData()));
    }
    public void SaveConfig(string storyCoreFolder)
    {
        SaveAndLoad.Save(storyCoreFolder + "/" + configFormFileName, ConfigForm.GetJaByDatas().ToString());
    }
    public void SaveProgress(string progressPath)
    {
        SaveAndLoad.Save(progressPath, JsonConvert.SerializeObject(PlayManager.instance.data.GetJsonData()));
    }


    #endregion

    #region load
    public void AddStoryTex(StoryTexAssetForm.Data data)
    {
        if (StoryTexAssetForm.DataByName.ContainsKey(data.name))
        {
            var oldData = StoryTexAssetForm.DataByName[data.name];
            StoryTexAssetForm.RemoveData(oldData.id);
        }
        StoryTexAssetForm.AddData(data);
    }
    public void AddStoryTex(TexAssetForm.Data rawData)
    {
        StoryTexAssetForm.Data data = (StoryTexAssetForm.Data)rawData;
        if (StoryTexAssetForm.DataByName.ContainsKey(data.name))
        {
            var oldData = StoryTexAssetForm.DataByName[data.name];
            StoryTexAssetForm.RemoveData(oldData.id);
        }
        StoryTexAssetForm.AddData(data);
    }
    public void LoadOverview(string storyCoreFolder)
    {
        var pathForm = storyCoreFolder + "/" + mapTextureFormFileName;
        StoryForm.Clear();
        if (SaveAndLoad.Exist(pathForm))
        {
            foreach (var form in StoryForm.GetDatasByJa(JArray.Parse(SaveAndLoad.Load<string>(pathForm))))
            {
                StoryForm.AddData(form);
                var nm = form.icon;
                var path = storyCoreFolder + "/" + nm;
                if (SaveAndLoad.Exist(path) && !TexAssetForm.DataByName.ContainsKey(nm))
                {
                    AddStoryTex(AssetManager.instance.LoadTexBytes(SaveAndLoad.Load<byte[]>(path), nm));
                }
            }
        }

      
    }
    public void LoadMaterial(string storyCoreFolder)
    {
        var pathForm = storyCoreFolder + "/" + mapTextureFormFileName;
        MapMaskForm.ClearAuto();
        if (SaveAndLoad.Exist(pathForm))
        {
            foreach (var form in MapTextureForm.GetDatasByJa(JArray.Parse(SaveAndLoad.Load<string>(pathForm))))
            {
                MapTextureForm.AddData(form);
            }
        }

        foreach (var data in MapTextureForm.DataById.Values)
        {
            for (int i = 0; i < data.texsName.Count; i++)
            {
                var nm = data.texsName[i];
                var path = storyCoreFolder + "/" + nm;
                if (SaveAndLoad.Exist(path) && !TexAssetForm.DataByName.ContainsKey(nm))
                {
                    AddStoryTex(AssetManager.instance.LoadTexBytes(SaveAndLoad.Load<byte[]>(path), nm));
                }
            }
        }

        pathForm = storyCoreFolder + "/" + mapMaskFormFileName;
        MapMaskForm.ClearAuto();
        if (SaveAndLoad.Exist(pathForm))
        {
            foreach (var form in MapMaskForm.GetDatasByJa(JArray.Parse(SaveAndLoad.Load<string>(pathForm))))
            {
                MapMaskForm.AddData(form);
            }
        }
        foreach (var data in MapMaskForm.DataById.Values)
        {
            for (int i = 0; i < Enum.GetValues(typeof(AlphaTexBasic6)).Length; i++)
            {
                var nm = data.texsName[i];
                var path = storyCoreFolder + "/" + nm;
                if (SaveAndLoad.Exist(path) && !TexAssetForm.DataByName.ContainsKey(nm))
                {
                    AddStoryTex(AssetManager.instance.LoadTexBytes(SaveAndLoad.Load<byte[]>(path), nm));
                }
            }
        }
    }
    public void LoadObject(string storyCoreFolder)
    {
        var pathForm = storyCoreFolder + "/" + mapObjectFormFileName;
        MapObjectForm.ClearAuto();
        if (SaveAndLoad.Exist(pathForm))
        {

            foreach (var form in MapObjectForm.GetDatasByJa(JArray.Parse(SaveAndLoad.Load<string>(pathForm))))
            {
                MapObjectForm.AddData(form);
            }
        }
        foreach (var data in MapObjectForm.DataById.Values)
        {
            for (int i = 0; i < data.model.subUnitTexsName.Count; i++)
            {
                var nm = data.model.subUnitTexsName[i];
                var path = storyCoreFolder + "/" + nm;
                if (SaveAndLoad.Exist(path) && !TexAssetForm.DataByName.ContainsKey(nm))
                {
                    AddStoryTex(AssetManager.instance.LoadTexBytes(SaveAndLoad.Load<byte[]>(path), nm));
                }
            }
        }
    }

    public void LoadCharacter(string storyCoreFolder)
    {
        var pathForm = storyCoreFolder + "/" + characterParamFormFileName;
        CharacterParamForm.ClearAuto();
        if (SaveAndLoad.Exist(pathForm))
        {
            foreach (var form in CharacterParamForm.GetDatasByJa(JArray.Parse(SaveAndLoad.Load<string>(pathForm))))
            {
                CharacterParamForm.AddData(form);
            }
        }

        pathForm = storyCoreFolder + "/" + characterProductFormFileName;
        CharacterProductForm.ClearAuto();
        if (SaveAndLoad.Exist(pathForm))
        {
            foreach (var form in CharacterProductForm.GetDatasByJa(JArray.Parse(SaveAndLoad.Load<string>(pathForm))))
            {
                CharacterProductForm.AddData(form);
            }
        }

        foreach (var data in CharacterProductForm.DataByUid.Values)
        {
            var icon = data.avatarTexName;
            var path = storyCoreFolder + "/" + icon;
            if (SaveAndLoad.Exist(path) && !TexAssetForm.DataByName.ContainsKey(icon))
            {
                AddStoryTex(AssetManager.instance.LoadTexBytes(SaveAndLoad.Load<byte[]>(path), icon));
            }
            foreach (var anim in data.animDic.Values)
            {
               
                    for (int i = 0; i < anim.animClip.Count; i++)
                    {
                    foreach (BodyPartType part in Enum.GetValues(typeof(BodyPartType)))
                    {
                        var nm = anim.animClip[i].partTex[part];
                        path = storyCoreFolder + "/" + nm;
                        if (SaveAndLoad.Exist(path) && !TexAssetForm.DataByName.ContainsKey(nm))
                        {
                            AddStoryTex(AssetManager.instance.LoadTexBytes(SaveAndLoad.Load<byte[]>(path), nm));
                        }
                    }
                    }
            }
        }
    }
    public void LoadItem(string storyCoreFolder)
    {
        var pathForm = storyCoreFolder + "/" + itemParamFormFileName;
        ItemParamForm.ClearAuto();
        if (SaveAndLoad.Exist(pathForm))
        {
            foreach (var form in ItemParamForm.GetDatasByJa(JArray.Parse(SaveAndLoad.Load<string>(pathForm))))
            {
                ItemParamForm.AddData(form);
            }
        }

        pathForm = storyCoreFolder + "/" + itemProductFormFileName;
        ItemProductForm.ClearAuto();
        if (SaveAndLoad.Exist(pathForm))
        {
            foreach (var form in ItemProductForm.GetDatasByJa(JArray.Parse(SaveAndLoad.Load<string>(pathForm))))
            {
                ItemProductForm.AddData(form);
            }
        }

        foreach (var data in ItemProductForm.DataByUid.Values)
        {
            var icon = data.iconTexName;
            var path = storyCoreFolder + "/" + icon;
            if (SaveAndLoad.Exist(path) && !TexAssetForm.DataByName.ContainsKey(icon))
            {
                AddStoryTex(AssetManager.instance.LoadTexBytes(SaveAndLoad.Load<byte[]>(path), icon));
            }
            foreach (var nm in data.model.subUnitTexsName)
            {
                path = storyCoreFolder + "/" + nm;
                if (SaveAndLoad.Exist(path) && !TexAssetForm.DataByName.ContainsKey(nm))
                {
                    AddStoryTex(AssetManager.instance.LoadTexBytes(SaveAndLoad.Load<byte[]>(path), nm));
                }
            }
        }
    }
    public void LoadEvent(string storyCoreFolder)
    {
        var pathForm = storyCoreFolder + "/" + eventFormFileName;
        EventProgramDataForm.ClearAuto();
        if (SaveAndLoad.Exist(pathForm))
        {
            foreach (var form in EventProgramDataForm.GetDatasByJa(JArray.Parse(SaveAndLoad.Load<string>(pathForm))))
            {
                EventProgramDataForm.AddData(form);
/*                foreach (var cmd in form.cmds)//导入图片
                {

                }*/
            }
        }


    }
    public MapData LoadScene(string scenePath)
    {
        var mapData = new GameMapData();
        mapData.Init(SaveAndLoad.Load<string>(scenePath));
        return mapData;
    }
    public void LoadConfig(string storyCoreFolder)
    {
        var pathForm = storyCoreFolder + "/" + configFormFileName;

        if (SaveAndLoad.Exist(pathForm))
        {
            foreach (var form in ConfigForm.GetDatasByJa(JArray.Parse(SaveAndLoad.Load<string>(pathForm))))
            {
                ConfigForm.AddData(form);
            }
        }
    }

    public PlayData LoadProgress(string progressPath)
    {
        if (SaveAndLoad.Exist(progressPath))
        {
            return new PlayData(SaveAndLoad.Load<string>(progressPath));
        }
        return new PlayData();
    }



    public void LoadLocalModStory()
    {

    }
    #endregion

    public void ResetPrefabPool()
    {
        InstancePoolManager.instance.Clear();
        foreach (Transform child in InstancePoolManager.instance.defaultRoot)
        {
            GameObject.Destroy(child.gameObject); // 销毁子物体
        }
        foreach (var form in GameObjectAssetForm.DataById.Values)
        {
            if (form.name.StartsWith(GlobalNameHelper.GetInternalPrefabName("")))
            {
                InstancePoolManager.instance.AddPool(form.go);
            }
        }
        foreach (var form in MapObjectForm.DataById.Values)
        {
            var obj = _super.utilCtrl.CombineNewObjectByPrefabs(form.name, form.model, true);
            obj.transform.parent = InstancePoolManager.instance.defaultRoot;
            InstancePoolManager.instance.AddPool(obj);
        }
        foreach (var form in ItemProductForm.DataByUid.Values)
        {
            var obj = _super.utilCtrl.CombineNewObjectByPrefabs(form.name, form.model, true);
            obj.transform.parent = InstancePoolManager.instance.defaultRoot;
            InstancePoolManager.instance.AddPool(obj);
        }

        var character = _super.utilCtrl.CombineNewCharacterByPrefabs(GlobalNameHelper.GetRuntimePrefabName("character"), new List<string>() { "", "", null }, true);
        character.transform.parent = InstancePoolManager.instance.defaultRoot;
        InstancePoolManager.instance.AddPool(character);

    }


}
