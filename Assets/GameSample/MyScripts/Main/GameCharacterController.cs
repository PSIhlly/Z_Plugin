using Form;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_DataSystem.Form;
using Z_DesignStyle;
using Z_Map;
using Z_Map.Form;
using Z_Texture;
using Z_Time;
public static partial class GlobalMaxSettings
{
    public const int CHARACTER_AVATA_MAX = 10;
    public const int CHARACTER_ANIM_MAX = 10;
    public const int CHARACTER_PART_MAX = 2;
    public const int CHARACTER_PARAM_MAX = 100000;
    public const int ITEM_PARAM_MAX = 100000;

}
public static partial class GlobalDataHelper
{
    

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

        public Dictionary<int, State> stateCur = new Dictionary<int, State>();
        public Dictionary<int, State> stateTar = new Dictionary<int, State>();
        public Dictionary<int, int> stateCd = new Dictionary<int, int>();
        public Dictionary<int, string> animCurCache = new Dictionary<int, string>();
        public Dictionary<int, Timer> animTimer = new Dictionary<int, Timer>();
        public void Reset()
        {
            for (int i = 0; i < GlobalMaxSettings.CHARACTER_PART_MAX; i++)
            {
                stateCur[i] = State.None;
                stateTar[i] = State.None;
                stateCd[i] = 0;
                animCurCache[i] = "";
                animTimer[i] = null;
            }
        }

        public void TryChangeState(State tar, int part, bool forceReplay = false)
        {
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
            CharacterAnimForm.Data anim;
            switch (tar)
            {
                case State.Move:
                    anim = moveAnim;
                    break;
                default:
                    anim = idleAnim;
                    break;
            }
            UpdateAnim(part, ins.renderers[part], ins.unit.data, anim, forceReplay);
        }


        private void UpdateAnim(int part, Renderer render, CharacterUnitForm.Data data, CharacterAnimForm.Data anim, bool forceReplay = false)
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
            if (anim != null && anim.partAnimTexsName[part].Count > 0)
            {
                propBlock = new MaterialPropertyBlock();
                render.GetPropertyBlock(propBlock);
                TimeManager.instance.CancelTimer(animTimer[part]);
                if (anim.animTimeInterval > 0)
                {
                    float all = anim.animTimeInterval * anim.partAnimTexsName[part].Count;
                    int cur = (int)((Time.time % all) / anim.animTimeInterval-0.0001f);
                 
                    float timeProgress = (Time.time % anim.animTimeInterval);
                    render.GetPropertyBlock(propBlock);
                    animCurCache[part] = anim.name;
                    propBlock.SetTexture("_Tex", TexAssetForm.DataByName[anim.partAnimTexsName[part][cur]].tex);
                    int renderId = part;
                    animTimer[part] = TimeManager.instance.StartTimer(timeProgress, anim.animTimeInterval, () =>
                    {
                        MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
                        render.GetPropertyBlock(propBlock);
                        cur = (cur + 1) % anim.partAnimTexsName[renderId].Count;
                        propBlock.SetTexture("_Tex", TexAssetForm.DataByName[anim.partAnimTexsName[renderId][cur]].tex);
                        render.SetPropertyBlock(propBlock);
                        return false;
                    }, data.unit.ins);
                }
                else
                {
                    propBlock.SetTexture("_Tex", TexAssetForm.DataByName[anim.partAnimTexsName[part][0]].tex);
                }
                render.SetPropertyBlock(propBlock);
            }

        }
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
        var form = PlayManager.instance.sceneCtrl.GetCharacterProduct(ins.unit.data);
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
        var form = PlayManager.instance.sceneCtrl.GetCharacterProduct(ins.unit.data);
        if (form == null)
        {
            Debug.LogError("No CharacterProductForm Find! " + ins.gameObject.name);
            return;
        }
        var status = animControllerDic[form];

        if (ins.step != Vector3.zero)
        {
            for (int i = 0; i < GlobalMaxSettings.CHARACTER_PART_MAX; i++)
            {
                status.TryChangeState(AnimController.State.Move, i);
            }
        }
        else
        {
            for (int i = 0; i < GlobalMaxSettings.CHARACTER_PART_MAX; i++)
            {
                status.TryChangeState(AnimController.State.Idle, i);
            }
        }

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
