using Form;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
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
    public const int SCENE_PARAM_MAX = 100000;
    public const int SKILL_PARAM_MAX = 100000;

}
public static partial class GlobalDataHelper
{


}
public enum AnimDirecton
{
    Fixed,
    Up,
    Down,
    Left,
    Right,
}
public enum FaceType
{
    Fixed,
    FourDirection,
    Flexible
}
public enum BodyPartType
{
    None = 0,
    UpperPart = 1,
    LowerPart = 2
}
public enum EquipPartType
{
    None = 0,
    LeftHand = 1,
    RightHand = 2,
    Head = 3,
    Body = 4,
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
            public override void ToProduct(int protoUid)
            {
                base.ToProduct(protoUid);
                CheckSkillProduct();
            }
            public void CheckSkillProduct()
            {
                if (skill != null)
                {
                    var newSkillDic = new Dictionary<SkillType, int>();
                    foreach (var skillKvp in skill)
                    {
                        var skillData = SkillProductForm.DataByUid.GetDv(skillKvp.Value, null);
                        if (skillData != null && skillData.protoUid == 0)
                        {
                            var newSkill = skillData.Copy(false);
                            newSkill.ToProduct(skillData.uid);
                            newSkillDic[skillKvp.Key] = newSkill.uid;
                            newSkill.characterUid = uid;
                        }
                        else
                        {
                            newSkillDic[skillKvp.Key] = skillKvp.Value;
                        }

                    }
                    skill = newSkillDic;
                }
            }
            public bool CanShow(string prmName)
            {
                if (!paramDic.ContainsKey(prmName) || !CharacterParamForm.DataByName.ContainsKey(prmName))
                {
                    return false;
                }
                var prmData = CharacterParamForm.DataByName[prmName];
                switch (prmData.showType)
                {
                    case ParamShowType.Always:
                    case ParamShowType.AlwaysWithPanel:
                        return true;
                    case ParamShowType.OnlyNotZero:
                        return paramDic[prmName].GetValue().num != 0;
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
    public Dictionary<CharacterUnitForm.Data, AnimController> animControllerDic = new Dictionary<CharacterUnitForm.Data, AnimController>();

    public class AnimController
    {
        public enum State
        {
            None,
            Idle,
            Move,
            Special,
            Die
        }
        public AnimController()
        {
            Reset();
        }
        public CharacterUnitForm.Data data;
        public CharacterProductForm.Data productData => CharacterProductForm.DataByUid[data.unit.productInfo.Item1];
        public CharacterAnimForm.Data idleAnim;
        public CharacterAnimForm.Data moveAnim;

        public Dictionary<BodyPartType, State> stateCur = new Dictionary<BodyPartType, State>();
        public Dictionary<BodyPartType, string> animCur = new Dictionary<BodyPartType, string>();
        public Dictionary<BodyPartType, string> animTar = new Dictionary<BodyPartType, string>();
        public Dictionary<BodyPartType, int> stateCd = new Dictionary<BodyPartType, int>();
        public Dictionary<BodyPartType, string> animCurCache = new Dictionary<BodyPartType, string>();
        public Dictionary<BodyPartType, Timer> animTimer = new Dictionary<BodyPartType, Timer>();

        public AnimDirecton dirCur = AnimDirecton.Fixed;
        public CharacterAnimForm.Data GetAnim(State state, string extraAnimName)
        {
            if (!string.IsNullOrEmpty(extraAnimName))
            {
                return productData.animDic.GetDv(extraAnimName, idleAnim);
            }
            switch (state)
            {
                case State.Move:
                    return moveAnim;
                default:
                    return idleAnim;
            }
        }

        private AnimDirecton GetDir()
        {
            switch (productData.faceType)
            {
                case FaceType.FourDirection:
                    var angle = (data.unit.ins.transform.eulerAngles.y % 360 + 360) % 360;
                    if (angle >= 45 && angle < 135)
                    {
                        return AnimDirecton.Right;
                    }
                    else if (angle >= 135 && angle < 225)
                    {
                        return AnimDirecton.Down;
                    }
                    else if (angle >= 225 && angle < 315)
                    {
                        return AnimDirecton.Left;
                    }
                    else
                    {
                        return AnimDirecton.Up;
                    }
                default:
                    return AnimDirecton.Up;
            }

        }

        public void Reset()
        {
            foreach (BodyPartType part in Enum.GetValues(typeof(BodyPartType)))
            {
                animCur[part] = "";
                animTar[part] = "";
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
                var anim = productData.animDic.GetDv(animCur[part], null);
                if (anim != null && anim.partEnable.ContainsKey(part) && anim.partEnable[part])
                {
                    data.unit.ins.renderers[(int)part - 1].enabled = true;
                }
                else
                {
                    data.unit.ins.renderers[(int)part - 1].enabled = false;
                }
            }
        }
        public void ChangeState(State tar, BodyPartType part, bool forceReplay = false, string extraAnimName = "")
        {
            CharacterAnimForm.Data anim = GetAnim(tar, extraAnimName);
            animCur[part] = anim.name;
            stateCur[part] = tar;
            stateCd[part] = 0;
            UpdateAnim(part, data.unit.ins.renderers[(int)part - 1], data, anim, forceReplay);
        }
        public void TryChangeState(State tar, BodyPartType part, bool forceReplay = false, string extraAnimName = "")
        {
            if (part == BodyPartType.None)
                return;
            CharacterAnimForm.Data anim = GetAnim(tar, extraAnimName);
            if (anim == null)
                return;

            if (!anim.partEnable.ContainsKey(part) || !anim.partEnable[part])
            {
                return;
            }

            if (stateCur.ContainsKey(part))
            {
                if (stateCur[part] == State.Die)//die cant interrupt
                {
                    return;
                }
                if ((tar == State.Idle || tar == State.Move) && stateCur[part] == State.Special)
                {
                    return;
                }
            }


            string tarAnimName = anim.name;

            if ((animTar[part] != tarAnimName) && !string.IsNullOrEmpty(animCur[part]))
            {
                stateCd[part] = 10;
            }

            animTar[part] = tarAnimName;
            if (tar != State.Special && tar != State.Die)
            {
                if (animCur[part] != tarAnimName)
                {
                    stateCd[part]--;
                }
                if (stateCd[part] > 0)
                {
                    return;
                }
            }
            else
            {
                stateCd[part] = 0;
            }
            ChangeState(tar, part, forceReplay, extraAnimName);

        }


        private void UpdateAnim(BodyPartType part, Renderer render, CharacterUnitForm.Data data, CharacterAnimForm.Data anim, bool forceReplay = false)
        {
            if (forceReplay)
            {
                animCurCache[part] = "";
            }
            if (anim == null)
            {
                return;
            }
            if (animCurCache.ContainsKey(part) && animCurCache[part] == anim.name && GetDir() == dirCur)
            {
                return;
            }
            dirCur = GetDir();
            MaterialPropertyBlock propBlock;
            if (anim != null && anim.animClip.Count > 0 && anim.animClip[dirCur].Count > 0)
            {

                propBlock = new MaterialPropertyBlock();
                render.GetPropertyBlock(propBlock);
                TimeManager.instance.CancelTimer(animTimer[part]);


                if (anim.animTimeInterval > 0)
                {
                    float all = anim.animTimeInterval * anim.animClip[dirCur].Count;
                    int cur = 0;//(int)((Time.time % all) / anim.animTimeInterval-0.0001f);

                    float timeProgress = 0;// (Time.time % anim.animTimeInterval);

                    animCurCache[part] = anim.name;

                    propBlock.SetTexture("_Tex", TexAssetForm.DataByName[anim.animClip[dirCur][cur].partTex[part]].GetTex());
                    var renderPart = part;
                    animTimer[part] = TimeManager.instance.StartTimer(timeProgress, anim.animTimeInterval, () =>
                    {
                        MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
                        render.GetPropertyBlock(propBlock);

                        if (cur + 1 >= anim.animClip[dirCur].Count)
                        {

                            if (stateCur[part] == State.Special)
                            {
                                ChangeState(State.Idle, part);
                                return true;
                            }
                            if (stateCur[part] == State.Die)
                            {
                                return true;
                            }

                        }
                        cur = (cur + 1) % anim.animClip[dirCur].Count;
                        propBlock.SetTexture("_Tex", TexAssetForm.DataByName[anim.animClip[dirCur][cur].partTex[renderPart]].GetTex());
                        render.SetPropertyBlock(propBlock);
                        return false;
                    }, data.unit.ins);
                }
                else
                {
                    propBlock.SetTexture("_Tex", TexAssetForm.DataByName[anim.animClip[dirCur][0].partTex[part]].GetTex());
                }
                render.SetPropertyBlock(propBlock);
            }

        }
    }

    public void RegisterAnim(CharacterUnitForm.Data data)
    {
        if (animControllerDic.ContainsKey(data))
        {
            return;
        }
        var productData = CharacterProductForm.DataByUid[data.unit.productInfo.Item1];
        CharacterAnimForm.Data idleAnim = productData.animDic.GetDv(productData.defaultAnimName.GetDv("idle", null), null);
        CharacterAnimForm.Data moveAnim = productData.animDic.GetDv(productData.defaultAnimName.GetDv("move", null), null);

        GameManager.instance.characterCtrl.CreateAnim(data, idleAnim, moveAnim);
    }
    public void UnregisterAnim(CharacterUnitForm.Data data)
    {
        var ctrl = animControllerDic.GetDv(data, null);
        if (ctrl != null)
        {
            foreach (var timer in ctrl.animTimer)
            {
                TimeManager.instance.CancelTimer(timer.Value);
            }
        }
    }
    public void Reset()
    {
        animControllerDic.Clear();
    }
    public void CreateAnim(CharacterUnitForm.Data key, CharacterAnimForm.Data idleAnim, CharacterAnimForm.Data moveAnim)
    {
        animControllerDic[key] = new AnimController()
        {
            idleAnim = idleAnim,
            moveAnim = moveAnim,
            data = key
        };
    }

    public void LoadModel(CharacterInstance ins)
    {
        var data = ins.unit.data;
        var form = CharacterProductForm.DataByUid[data.unit.productInfo.Item1];
        if (form == null)
        {
            Debug.LogError("No CharacterProductForm Find! " + ins.gameObject.name);
            return;
        }

        animControllerDic[data].Reset();

        foreach (var keeper in ins.keepers)
        {
            keeper.enableFixedYRotation = form.faceType != FaceType.Flexible;
            keeper.fixedYRotation = 0;
        }
    }

    public void PlaySpecialAnim(CharacterUnitForm.Data ch, string animName)
    {
        var status = animControllerDic[ch];

        foreach (BodyPartType part in Enum.GetValues(typeof(BodyPartType)))
        {
            status.TryChangeState(AnimController.State.Special, part, true, animName);
        }
    }

    public void CheckAnim(CharacterInstance ins)
    {
        if (ins == null)
        {
            return;
        }
        var data = ins.unit.data;
        if (!animControllerDic.ContainsKey(ins.unit.data))
        {
            return;
        }
        var form = CharacterProductForm.DataByUid[ins.unit.productInfo.Item1];
        if (form == null)
        {
            Debug.LogError("No CharacterProductForm Find! " + ins.gameObject.name);
            return;
        }
        var status = animControllerDic[data];

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
                RegisterAnim(evt.unit.data);
                LoadModel(evt.unit.ins);
                break;
            case MapEventType.AfterUpdate:
                //manage nav
                var productData = CharacterProductForm.DataByUid.GetDv(evt.unit.productInfo.Item1, null);
                if (productData != null)
                {
                    evt.unit.data.navEnabled = productData.enableNav && _super.curProgress.seconds > productData.recoveryTime;
                }
                CheckAnim(evt.unit.ins);
                break;
            case MapEventType.Hide:
                //manage nav
                UnregisterAnim(evt.unit.data);
                break;
        }
    }

}
