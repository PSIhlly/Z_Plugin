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
using Z_Texture;
using Z_UnitSystem;

public class GameUtilController : Z_Controller<GameManager>
{
    public string emptyTexName => GlobalNameHelper.GetDefaultTexName();
    public GameUtilController(GameManager super) : base(super)
    {
    }


    public GameObject CombineNewCharacterByPrefabs(string name, List<string> texRealName, bool forGame)
    {

        List<bool> showShaddowLst = new List<bool>()
                {
                    false,false
                };

        var res = CombineNewGoByPrefabs(name, new List<string>() { "Quad", "Quad"}, texRealName, new List<Vector3>() { Vector3.zero, Vector3.zero}, new List<Vector3>() { Vector3.one , Vector3.one }, showShaddowLst);
        var renders=res.GetComponentsInChildren<Renderer>();
        renders[0].transform.GetComponent<PerspectiveKeeper>().deepth = 0.01f;
        renders[1].transform.GetComponent<PerspectiveKeeper>().deepth = 0.05f;
        if (forGame)
        {
            res.AddComponent<CharacterInstance>();
            //default disable
            foreach (var r in renders)
            {
                r.enabled = false;
            }

        }
        return res;
    }
    public GameObject CombineNewObjectByPrefabs(string name, MapModelForm.Data model, bool forGame,bool isItem=false)
    {
        if (model == null)
            return new GameObject(name);

        var showShadow = new List<bool>();
        for (int i = 0; i < model.subPrefabUnitName.Count; i++)
            showShadow.Add(true);

        var res = CombineNewGoByPrefabs(name, model.subPrefabUnitName, model.subUnitTexsName, model.subPrefabUnitPos, model.subPrefabUnitScale, showShadow);
        if (forGame)
        {
            if (isItem)
                res.AddComponent<ItemInstance>();
            else
                res.AddComponent<ObjectInstance>();
        }
            
        return res;
    }
    private GameObject CombineNewGoByPrefabs(string name, List<string> prefabKeys, List<string> texRealName, List<Vector3> poss, List<Vector3> scales, List<bool> showShadow)
    {
        var res = new GameObject(name);
        for (int i = 0; i < prefabKeys.Count; i++)
        {
            var go = GameObject.Instantiate(AssetManager.instance.GetGameObject(prefabKeys[i]), res.transform);
            go.transform.localPosition = poss[i] +Vector3.up/2;//Ì§¸ß
            go.transform.localScale = scales[i];

            MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
            var render = go.GetComponentInChildren<Renderer>();
            render.GetPropertyBlock(propBlock);
            if(texRealName[i] !=null&& !TexAssetForm.DataByName.ContainsKey(texRealName[i]))
            {
                Debug.LogError(name+" miss tex " + texRealName[i]);
            }
            if (texRealName[i]==null|| texRealName[i] == emptyTexName ||!TexAssetForm.DataByName.ContainsKey(texRealName[i]))
            {
                propBlock.SetTexture("_Tex", Texture2D.whiteTexture);
                if (showShadow[i])
                {
                    render.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
                }
                else
                {
                    propBlock.SetFloat("_Show", 0);
                }
            }
            else
            {
                var tex = TexAssetForm.DataByName[texRealName[i]].tex;
                render.shadowCastingMode = showShadow[i] ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off;
                propBlock.SetTexture("_Tex", tex);
            }
            render.SetPropertyBlock(propBlock);
        }
        res.SetActive(false);
        return res;
    }


}
