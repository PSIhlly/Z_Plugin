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

public class GameUtilController: Z_Controller<GameManager>
{
    public GameUtilController(GameManager super):base(super)
    { }

    public void ResetPrefabPool()
    {
        InstancePoolManager.instance.Clear();
        foreach (var form in GameObjectAssetForm.DataById.Values)
        {
            if(form.name.StartsWith("$"))
            {
                InstancePoolManager.instance.AddPool(form.go);
            }
        }
        foreach (var form in MapItemForm.DataById.Values)
        {
            InstancePoolManager.instance.AddPool(CombineNewItemByPrefabs(form.name,form.subPrefabUnitName,form.subPrefabUnitPos,form.subPrefabUnitScale,true));
        }
    }

    public GameObject CombineNewItemByPrefabs(string name, List<string> prefabKeys, List<Vector3> poss, List<Vector3> scales,bool forGame)
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
            var tex = AssetManager.instance.GetSprite(GlobalHelper.GetItemTexRealName(name, i))?.texture;
            propBlock.SetTexture("_Tex", tex);
            render.SetPropertyBlock(propBlock);
        }
        if (forGame)
            res.AddComponent<ItemInstance>();
        res.SetActive(false);
        return res;
    }



}
