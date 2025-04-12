using Form;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_DataSystem.Form;
using Z_DesignStyle;
using Z_Map;
using Z_Map.Form;
using Z_Texture;

public class GameCharacterController : Z_Controller<GameManager>, IZ_Listener<CharacterEvent>
{
    
    public GameCharacterController(GameManager super) : base(super)
    {
        Z_EventHelper.Register(this);
    }
    public Dictionary<CharacterProductForm.Data, List<string>> idleAnimUp=new Dictionary<CharacterProductForm.Data, List<string>>();
    public Dictionary<CharacterProductForm.Data, List<string>> idleAnimDown=new Dictionary<CharacterProductForm.Data, List<string>>();
    public Dictionary<CharacterUnitForm.Data, Dictionary<int, int>> animCurCache = new Dictionary<CharacterUnitForm.Data, Dictionary<int, int>>();



    public void Reset()
    {
        animCurCache.Clear();
    }
    public void CreateAnim(CharacterProductForm.Data key,List<string> idleAnimUpLst, List<string> idleAnimDownLst)
    {
        idleAnimUp[key] = idleAnimUpLst;
        idleAnimDown[key] = idleAnimDownLst;
    }

    public void LoadModel(CharacterInstance ins)
    {
        var data = ins.unit.data;
        var form = PlayManager.instance.sceneCtrl.GetCharacterProduct(ins.unit.data);
        if (form == null)
        {
            Debug.LogError("No CharacterProductForm Find! " + ins.gameObject.name);
            return; 
        }

        if (!animCurCache.ContainsKey(data))
        {
            animCurCache[data] = new Dictionary<int, int>();
        }

        MaterialPropertyBlock propBlock; 
        if (idleAnimUp[form].Count>0)
            {
            propBlock = new MaterialPropertyBlock();
            ins.renderers[0].GetPropertyBlock(propBlock);
            propBlock.SetTexture("_Tex", TexAssetForm.DataByName[idleAnimUp[form][0]].tex);
            propBlock.SetFloat("_Show", 1);

           ins.renderers[0].SetPropertyBlock(propBlock);
        }

        if (idleAnimDown[form].Count > 0)
        {
            propBlock = new MaterialPropertyBlock();
            ins.renderers[1].GetPropertyBlock(propBlock);
            propBlock.SetTexture("_Tex", TexAssetForm.DataByName[idleAnimDown[form][0]].tex); 
            propBlock.SetFloat("_Show", 1);
            ins.renderers[1].SetPropertyBlock(propBlock);
        }

    }
            public void UpdateAnim(CharacterInstance ins)
    {
        
    }
    

   
    public void OnEvent(CharacterEvent evt)
    {
        switch (evt.type)
        {
            case MapEventType.Show:
                LoadModel(evt.unit.ins);
                break;
            case MapEventType.AfterUpdate:
                UpdateAnim(evt.unit.ins);
                break;
        }
    }

}
