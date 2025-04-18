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
    public GameUtilController(GameManager super) : base(super)
    {
    }


    public GameObject CombineNewCharacterByPrefabs(string name, List<string> texRealName, bool forGame)
    {

        List<bool> showShaddowLst = new List<bool>()
                {
                    false,false,true
                };

        var res = CombineNewGoByPrefabs(name, new List<string>() { "Quad", "Quad", "Capsule" }, texRealName, new List<Vector3>() { Vector3.up * 0.4f, Vector3.up * 0.3f, Vector3.up*0.2f}, new List<Vector3>() { Vector3.one , Vector3.one, new Vector3(0.3f, 0.4f, 0.3f)  }, showShaddowLst);
        if (forGame)
        {
            res.AddComponent<CharacterInstance>();
            res.transform.GetChild(0).gameObject.GetComponent<MeshCollider>().enabled=false;
            res.transform.GetChild(1).gameObject.GetComponent<MeshCollider>().enabled=false;
            var rb=res.AddComponent<Rigidbody>();
            rb.freezeRotation = true;
        }
        return res;
    }
    public GameObject CombineNewItemByPrefabs(string name, List<string> prefabKeys, List<string> texRealName, List<Vector3> poss, List<Vector3> scales, List<bool> showShadow, bool forGame)
    {
        var res = CombineNewGoByPrefabs(name, prefabKeys, texRealName, poss, scales, showShadow);
        if (forGame)
            res.AddComponent<ItemInstance>();
        return res;
    }
    public GameObject CombineNewGoByPrefabs(string name, List<string> prefabKeys, List<string> texRealName, List<Vector3> poss, List<Vector3> scales, List<bool> showShadow)
    {
        var res = new GameObject(name);
        for (int i = 0; i < prefabKeys.Count; i++)
        {
            var go = GameObject.Instantiate(AssetManager.instance.GetGameObject(prefabKeys[i]), res.transform);
            go.transform.localPosition = poss[i] + Vector3.up * scales[i].y / 2f;
            go.transform.localScale = scales[i];

            MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
            var render = go.GetComponent<Renderer>();
            render.GetPropertyBlock(propBlock);

            if (texRealName[i]==null)
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
