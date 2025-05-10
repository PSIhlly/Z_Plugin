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
    public string characterParamFormFileName => "cpaf";
    public string characterProductFormFileName => "cprf";
    public string eventFormFileName => "ef";
    public string configFormFileName => "cf";
    public GameSaveController(GameManager super):base(super)
    { 
    }
    #region save

    public void RemoveTexUse(string startSign)
    {


    }


    public void SaveMaterial(string storyCoreFolder)
    {

        SaveAndLoad.Save(storyCoreFolder + "/" + mapTextureFormFileName, MapTextureForm.GetJaByDatas().ToString());

        foreach (var data in MapTextureForm.DataById.Values)
        {
            for(int i=0;i<data.texsName.Count;i++)
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
            for (int i = 0; i < Enum.GetValues(typeof(AlphaTexBasic5)).Length; i++)
            {
                var nm = data.texsName[i];
                if (TexAssetForm.DataByName.ContainsKey(nm)&&nm!="")
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
            for (int i = 0; i < data.subUnitTexsName.Count; i++)
            {
                var nm = data.subUnitTexsName[i];
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
            foreach (var anim in data.animDic.Values)
            {
                for (int j = 0; j < GlobalMaxSettings.CHARACTER_PART_MAX; j++)
                    for (int i = 0; i < anim.partAnimTexsName[j].Count; i++)
                    {
                        var nm = anim.partAnimTexsName[j][i];
                        if (TexAssetForm.DataByName.ContainsKey(nm) && nm != "")
                        {
                            var tex = TexAssetForm.DataByName[nm];
                            SaveAndLoad.Save(storyCoreFolder + "/"+nm, TextureHelper.GetTextureByte((Texture2D)tex.tex));
                        }
                    }

            }
        }
    }
    public void SaveEvent(string storyCoreFolder)
    {
        SaveAndLoad.Save(storyCoreFolder + "/" + eventFormFileName, EventForm.GetJaByDatas().ToString());
        foreach (var data in EventForm.DataByUid.Values)
        {
            foreach (var cmd in data.cmds)
            {
                if (cmd.isValue && cmd.resTypes[0]==(int)ValueType.Clips&&!string.IsNullOrEmpty(cmd.constV))
                {
                    var jo = JObject.Parse(cmd.constV);
                    var lst=jo.Get<List<ClipForm.Data>>(ValueType.Clips.ToString());
                    foreach (var clip in lst)
                    {
                        var nm = clip.profilePicture;
                        if (TexAssetForm.DataByName.ContainsKey(nm) && nm != "")
                        {
                            var tex = TexAssetForm.DataByName[nm];
                            SaveAndLoad.Save(storyCoreFolder + "/" + nm, TextureHelper.GetTextureByte((Texture2D)tex.tex));
                        }
                        nm = clip.mainPicture;
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
                if (SaveAndLoad.Exist(path)&&!TexAssetForm.DataByName.ContainsKey(nm))
                {
                    AssetManager.instance.LoadTexBytes(SaveAndLoad.Load<byte[]>(path), nm);
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
            for (int i = 0; i < Enum.GetValues(typeof(AlphaTexBasic5)).Length; i++)
            {
                var nm = data.texsName[i];
                var path = storyCoreFolder + "/" + nm;
                if (SaveAndLoad.Exist(path) && !TexAssetForm.DataByName.ContainsKey(nm))
                {
                    AssetManager.instance.LoadTexBytes(SaveAndLoad.Load<byte[]>(path), nm);
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
            for (int i = 0; i < data.subUnitTexsName.Count; i++)
            {
                var nm = data.subUnitTexsName[i];
                var path = storyCoreFolder + "/" + nm;
                if (SaveAndLoad.Exist(path) && !TexAssetForm.DataByName.ContainsKey(nm))
                {
                    AssetManager.instance.LoadTexBytes(SaveAndLoad.Load<byte[]>(path), nm);
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
            foreach (var anim in data.animDic.Values)
            {
                for (int j = 0; j < GlobalMaxSettings.CHARACTER_PART_MAX; j++)
                    for (int i = 0; i < anim.partAnimTexsName[j].Count; i++)
                    {
                        var nm = anim.partAnimTexsName[j][i];
                        var path = storyCoreFolder + "/" + nm;
                        if (SaveAndLoad.Exist(path) && !TexAssetForm.DataByName.ContainsKey(nm))
                        {
                            AssetManager.instance.LoadTexBytes(SaveAndLoad.Load<byte[]>(path), nm);
                        }
                    }
            }
        }
    }
    public void LoadEvent(string storyCoreFolder)
    {
        var pathForm = storyCoreFolder + "/" + eventFormFileName;
        EventForm.ClearAuto();
        if (SaveAndLoad.Exist(pathForm))
        {
            foreach (var form in EventForm.GetDatasByJa(JArray.Parse(SaveAndLoad.Load<string>(pathForm))))
            {
                EventForm.AddData(form);
                foreach (var cmd in form.cmds)
                {
                    if (cmd.isValue && cmd.resTypes[0] == (int)ValueType.Clips && !string.IsNullOrEmpty(cmd.constV))
                    {
                        var jo = JObject.Parse(cmd.constV);
                        var lst = jo.Get<List<ClipForm.Data>>(ValueType.Clips.ToString());
                        foreach (var clip in lst)
                        {
                            var nm = clip.profilePicture;
                            var path = storyCoreFolder + "/" + nm;
                            if (SaveAndLoad.Exist(path) && !TexAssetForm.DataByName.ContainsKey(nm))
                            {
                                AssetManager.instance.LoadTexBytes(SaveAndLoad.Load<byte[]>(path), nm);
                            }
                            nm = clip.mainPicture;
                            path = storyCoreFolder + "/" + nm;
                            if (SaveAndLoad.Exist(path) && !TexAssetForm.DataByName.ContainsKey(nm))
                            {
                                AssetManager.instance.LoadTexBytes(SaveAndLoad.Load<byte[]>(path), nm);
                            }
                        }
                    }



                }
            }
        }

       
    }
    public MapData LoadScene(string scenePath)
    {
        var mapData= new GameMapData();
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
            return new PlayData( SaveAndLoad.Load<string>(progressPath));
        }
        return new PlayData();
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
            List<string> texNameLst = new List<string>();
            List<bool> showShaddowLst = new List<bool>();
            for (int i = 0; i < form.subPrefabUnitName.Count; i++)
            {
                texNameLst.Add(form.subUnitTexsName[i]);
                showShaddowLst.Add(true);
            }
            var obj = _super.utilCtrl.CombineNewItemByPrefabs(form.name, form.subPrefabUnitName, texNameLst, form.subPrefabUnitPos, form.subPrefabUnitScale, showShaddowLst, true);
            obj.transform.parent = InstancePoolManager.instance.defaultRoot;
            InstancePoolManager.instance.AddPool(obj);
        }
        var character = _super.utilCtrl.CombineNewCharacterByPrefabs(GlobalNameHelper.GetRuntimePrefabName("character"), new List<string>() { "", "", null }, true);
        character.transform.parent = InstancePoolManager.instance.defaultRoot;
        InstancePoolManager.instance.AddPool(character);

    }


}
