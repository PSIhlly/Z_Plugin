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
using Z_Math;
using Z_Texture;
using Z_Time;
using Z_UnitSystem;

public class GameUtilController : Z_Controller<GameManager>
{
    public int emptyTexId => GlobalDefaultHelper.DefaultTexId;
    public GameUtilController(GameManager super) : base(super)
    {
    }


    public GameObject CombineNewCharacterByPrefabs(string name, List<int> texRealId, bool forGame)
    {

        List<bool> showShaddowLst = new List<bool>()
                {
                    false,false
                };

        var res = CombineNewGoByPrefabs(name, new List<int>() { _super.innerAssetDic["sphere"].id, _super.innerAssetDic["sphere"].id }, texRealId, new List<Vector3>() { Vector3.zero, Vector3.zero }, new List<Vector3>() { Vector3.one, Vector3.one }, showShaddowLst);
        var renders = res.GetComponentsInChildren<Renderer>();
        renders[0].transform.GetComponent<PerspectiveKeeper>().deepth = 0.01f;
        renders[1].transform.GetComponent<PerspectiveKeeper>().deepth = 0.05f;

        res.GetComponentsInChildren<SphereCollider>()[0].radius = 0.4f;
        GameObject.Destroy(res.GetComponentsInChildren<SphereCollider>()[2].gameObject);
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
    public GameObject CombineNewObjectByPrefabs(string name, MapModelForm.Data model, bool forGame, bool isItem = false)
    {
        if (model == null)
            return new GameObject(name);

        var prefabKeys = model.subPrefabUnitName ?? new List<int>();
        var showShadow = new List<bool>();
        var texRealId = new List<int>();
        var poss = new List<Vector3>();
        var scales = new List<Vector3>();
        for (int i = 0; i < prefabKeys.Count; i++)
        {
            showShadow.Add(true);
            texRealId.Add(model.subUnitTexsName != null && i < model.subUnitTexsName.Count &&
                          model.subUnitTexsName[i] != null && model.subUnitTexsName[i].Count > 0
                ? model.subUnitTexsName[i][0]
                : GlobalDefaultHelper.DefaultTexId);
            poss.Add(model.subPrefabUnitPos != null && i < model.subPrefabUnitPos.Count
                ? model.subPrefabUnitPos[i]
                : Vector3.zero);
            scales.Add(model.subPrefabUnitScale != null && i < model.subPrefabUnitScale.Count
                ? model.subPrefabUnitScale[i]
                : Vector3.one);
        }
        var res = CombineNewGoByPrefabs(name, prefabKeys, texRealId, poss, scales, showShadow);
        foreach (var col in res.GetComponentsInChildren<BoxCollider>())
        {
            col.transform.localScale = new Vector3(model.colliderScale, 1, model.colliderScale);
        }
        foreach (var col in res.GetComponentsInChildren<SphereCollider>())
        {
            col.transform.localScale = new Vector3(model.colliderScale, model.colliderScale, model.colliderScale);
        }
        if (forGame)
        {
            if (isItem)
                res.AddComponent<ItemInstance>();
            else
                res.AddComponent<ObjectInstance>();

        }

        return res;
    }
    private GameObject CombineNewGoByPrefabs(string name, List<int> prefabKeys, List<int> texRealId, List<Vector3> poss, List<Vector3> scales, List<bool> showShadow)
    {
        var res = new GameObject(name);
        for (int i = 0; i < prefabKeys.Count; i++)
        {
            Debug.Log(name + " " + prefabKeys[i]);
            var go = GameObject.Instantiate(GameObjectAssetForm.DataById[prefabKeys[i]].GetGo(), res.transform);
            go.transform.localPosition = poss[i] + Vector3.up / 2;//̧
            go.transform.localScale = scales[i];

            MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
            var render = go.GetComponentInChildren<Renderer>();
            render.GetPropertyBlock(propBlock);
            if (texRealId[i] != 0 && !TexAssetForm.DataById.ContainsKey(texRealId[i]))
            {
                Debug.LogError(name + " miss tex " + texRealId[i]);
            }
            if (texRealId[i] == 0 || texRealId[i] == emptyTexId || !TexAssetForm.DataById.ContainsKey(texRealId[i]))
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
                var tex = TexAssetForm.DataById[texRealId[i]].GetTex();
                render.shadowCastingMode = showShadow[i] ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off;
                propBlock.SetTexture("_Tex", tex);
            }
            render.SetPropertyBlock(propBlock);

            foreach (var com in go.GetComponentsInChildren<Collider>())
            {
                var scale = com.transform.lossyScale;
                if (com is BoxCollider box)
                {
                    var trigger = com.gameObject.AddComponent<BoxCollider>();
                    trigger.size = box.size + Graph.ElementwiseDivide(Vector3.one * 0.02f, scale);
                    trigger.center = box.center;
                    trigger.isTrigger = true;
                }
                else if (com is SphereCollider sphere)
                {
                    var trigger = com.gameObject.AddComponent<SphereCollider>();
                    trigger.radius = sphere.radius + 0.02f * Mathf.Max(scale.x, scale.y, scale.z);
                    trigger.center = sphere.center;
                    trigger.isTrigger = true;
                }
            }
        }
        res.SetActive(false);

        return res;
    }


}
