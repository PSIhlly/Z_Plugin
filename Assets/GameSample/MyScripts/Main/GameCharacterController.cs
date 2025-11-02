using Form;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.Text;
using Z_DataSystem;
using Z_DataSystem.Form;
using Z_DesignStyle;
using Z_Map;
using Z_Map.Form;
using Z_Texture;
using Z_Time;
public static partial class GlobalSettings
{
    public const int CHARACTER_AVATA_MAX = 10;
    public const int CHARACTER_ANIM_MAX = 10;
    public const int CHARACTER_PARAM_MAX = 100000;
    public const int GLOBAL_PARAM_MAX = 100000;
    public const int ITEM_PARAM_MAX = 100000;

}
public static partial class GlobalDataHelper
{
    

}
public enum BodyPartType
{
    None=0,
    UpperPart = 1,
    LowerPart = 2
}
public enum EquipPartType
{
    None = 0,
    LeftHand =1,
    RightHand=2,
    Head=3,
    Body=4,
}
public enum SkillType
{
    LightAttack,
    HeavyAttack,
    E,
    Q,
    Passive
}
namespace Form
{

    public static partial class CharacterProductForm
    {
        public partial class Data
        {
            public bool CanShow(string prmName)
            {
                if(!paramDic.ContainsKey(prmName)|| !CharacterParamForm.DataByName.ContainsKey(prmName))
                {
                    return false;
                }
                var prmData = CharacterParamForm.DataByName[prmName];
                switch(prmData.showType)
                {
                    case ParamShowType.Always:
                        return true;
                    case ParamShowType.OnlyNotZero:
                        return paramDic[prmName].v != 0;
                    case ParamShowType.Hide:
                        return false;
                }
                return false;
            }
        }
    }
}
        public class GameCharacterController : Z_Controller<GameManager>, IZ_Listener<CharacterEvent>
{
    public GameCharacterController(GameManager super) : base(super)
    {
        Z_EventHelper.Register(this);
    }
    public Dictionary<CharacterProductForm.Data, AnimController> animControllerDic = new Dictionary<CharacterProductForm.Data, AnimController>();

    public class AnimController
    {
        public enum State
        {
            None,
            Idle,
            Move
        }
        public AnimController()
        {
            Reset();
        }
        public CharacterProductForm.Data form;
        public CharacterInstance ins;
        public CharacterAnimForm.Data idleAnim;
        public CharacterAnimForm.Data moveAnim;

        public Dictionary<BodyPartType, State> stateCur = new Dictionary<BodyPartType, State>();
        public Dictionary<BodyPartType, State> stateTar = new Dictionary<BodyPartType, State>();
        public Dictionary<BodyPartType, int> stateCd = new Dictionary<BodyPartType, int>();
        public Dictionary<BodyPartType, string> animCurCache = new Dictionary<BodyPartType, string>();
        public Dictionary<BodyPartType, Timer> animTimer = new Dictionary<BodyPartType, Timer>();

        public CharacterAnimForm.Data GetAnim(State state)
        {
            switch (state)
            {
                case State.Move:
                    return moveAnim;
                default:
                    return idleAnim;
            }
        }
       
        public void Reset()
        {
            foreach(BodyPartType part in Enum.GetValues(typeof(BodyPartType)))
            {
                stateCur[part] = State.None;
                stateTar[part] = State.None;
                stateCd[part] = 0;
                animCurCache[part] = "";
                animTimer[part] = null;
            }
        }
        public void CloseHideRenderer()
        {
            foreach (BodyPartType part in Enum.GetValues(typeof(BodyPartType)))
            {
                if (part == BodyPartType.None)
                    continue;
                var anim = GetAnim(stateCur[part]);
                if (anim.partEnable.ContainsKey(part)&&anim.partEnable[part])
                {
                    ins.renderers[(int)part - 1].enabled = true;
                }else
                {
                    ins.renderers[(int)part - 1].enabled = false;
                }
            }
        }
        public void TryChangeState(State tar, BodyPartType part, bool forceReplay = false)
        {
            if (part == BodyPartType.None)
                return;
            CharacterAnimForm.Data anim=GetAnim(tar);
            //Debug.Log((anim == null) + " " + tar);
            if (!anim.partEnable.ContainsKey(part) || !anim.partEnable[part])
            {
                return;
            }


            if (stateTar[part] != tar && stateCur[part] != State.None)
            {
                stateCd[part] = 10;//10frames trans
            }

            stateTar[part] = tar;
            if (stateCur[part] != tar)
            {
                stateCd[part]--;
            }
            if (stateCd[part] > 0)
            {
                return;
            }
            stateCur[part] = tar;
            
            UpdateAnim(part, ins.renderers[(int)part - 1], ins.unit.data, anim, forceReplay);
        }


        private void UpdateAnim(BodyPartType part, Renderer render, CharacterUnitForm.Data data, CharacterAnimForm.Data anim, bool forceReplay = false)
        {
            if (forceReplay)
            {
                animCurCache[part] = "";
            }
            if (anim==null||(animCurCache.ContainsKey(part) && animCurCache[part] == anim.name))
            {
                return;
            }

            MaterialPropertyBlock propBlock;
            if (anim != null && anim.animClip.Count > 0)
            {

                propBlock = new MaterialPropertyBlock();
                render.GetPropertyBlock(propBlock);
                TimeManager.instance.CancelTimer(animTimer[part]);


                if (anim.animTimeInterval > 0)
                {
                    float all = anim.animTimeInterval * anim.animClip.Count;
                    int cur = 0;//(int)((Time.time % all) / anim.animTimeInterval-0.0001f);

                    float timeProgress = 0;// (Time.time % anim.animTimeInterval);
                    render.GetPropertyBlock(propBlock);
                    animCurCache[part] = anim.name;

                    propBlock.SetTexture("_Tex", TexAssetForm.DataByName[anim.animClip[cur].partTex[part]].GetTex());
                    var renderPart = part;
                    animTimer[part] = TimeManager.instance.StartTimer(timeProgress, anim.animTimeInterval, () =>
                    {
                        MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
                        render.GetPropertyBlock(propBlock);
                        cur = (cur + 1) % anim.animClip.Count;
                        propBlock.SetTexture("_Tex", TexAssetForm.DataByName[anim.animClip[cur].partTex[renderPart]].GetTex());
                        render.SetPropertyBlock(propBlock);
                        return false;
                    }, data.unit.ins);
                }
                else
                {
                    propBlock.SetTexture("_Tex", TexAssetForm.DataByName[anim.animClip[0].partTex[part]].GetTex());
                }
                render.SetPropertyBlock(propBlock);
            }

        }
    }

    public void RegisterAnim(CharacterProductForm.Data character)
    {
        CharacterAnimForm.Data idleAnim = null;
        CharacterAnimForm.Data moveAnim = null;
        foreach (var anim in character.animDic.Values)
        {
            if (anim.name == character.idleAnimName)
            {
                idleAnim = anim;
            }
            if (anim.name == character.moveAnimName)
            {
                moveAnim = anim;
            }
        }
        GameManager.instance.characterCtrl.CreateAnim(character, idleAnim, moveAnim);
    }
    public void Reset()
    {
        animControllerDic.Clear();
    }
    public void CreateAnim(CharacterProductForm.Data key, CharacterAnimForm.Data idleAnim, CharacterAnimForm.Data moveAnim)
    {
        animControllerDic[key] = new AnimController()
        {
            idleAnim = idleAnim,
            moveAnim = moveAnim,
            form = key
        };
    }

    public void LoadModel(CharacterInstance ins)
    {
        var data = ins.unit.data;
        var form = CharacterProductForm.DataByUid[AssetManager.GetKeyId(data.name)];
        if (form == null)
        {
            Debug.LogError("No CharacterProductForm Find! " + ins.gameObject.name);
            return;
        }

        animControllerDic[form].Reset();
        animControllerDic[form].ins = ins;
    }



    public void CheckAnim(CharacterInstance ins)
    {
        var data = ins.unit.data;
        var form = CharacterProductForm.DataByUid[AssetManager.GetKeyId(ins.unit.data.name)];
        if (form == null)
        {
            Debug.LogError("No CharacterProductForm Find! " + ins.gameObject.name);
            return;
        }
        var status = animControllerDic[form];

        if (ins.step != Vector3.zero)
        {
            foreach (BodyPartType part in Enum.GetValues(typeof(BodyPartType)))
            {
                status.TryChangeState(AnimController.State.Move, part);
            }
        }
        else
        {
            foreach (BodyPartType part in Enum.GetValues(typeof(BodyPartType)))
            {
                status.TryChangeState(AnimController.State.Idle, part);
            }
        }
        status.CloseHideRenderer();
    }



    public void OnEvent(CharacterEvent evt)
    {
        switch (evt.type)
        {
            case MapEventType.Show:
                LoadModel(evt.unit.ins);
                break;
            case MapEventType.AfterUpdate:
                CheckAnim(evt.unit.ins);
                break;
        }
    }

}
