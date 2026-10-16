using Form;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.TextCore.Text;
using Z_DataSystem;
using Z_DataSystem.Form;
using Z_DesignStyle;
using Z_Map;
using Z_Map.Form;
using Z_Texture;
using Z_Time;
using Z_UnitSystem;
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
                CheckParam();
            }
            public void CheckParam()
            {
                if (paramDic != null)
                {
                    var oldParam = paramDic;
                    paramDic = new Dictionary<string, CharacterParamForm.Data>();
                    foreach (var pair in oldParam)
                    {
                        paramDic[pair.Key] = pair.Value.Copy();
                    }
                }
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
                var protoPrm = CharacterParamForm.DataByName[prmName];
                switch (protoPrm.showType)
                {
                    case ParamShowType.Always:
                    case ParamShowType.AlwaysWithPanel:
                    case ParamShowType.AlwaysWithPanelAndScene:
                    case ParamShowType.AlwaysWithPanelAndSceneWithoutPlayer:
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
        CharacterProductForm.changeSizeAction += OnCharacterSizeChanged;
        CharacterProductForm.changePasstypeAction += OnCharacterPassTypeChanged;
    }
    public Dictionary<CharacterUnitForm.Data, AnimController> animControllerDic = new Dictionary<CharacterUnitForm.Data, AnimController>();

    private void OnCharacterSizeChanged(CharacterProductForm.Data productData, int oldValue, int newValue)
    {
        int validSize = Mathf.Max(1, newValue);

        foreach (var unitData in CharacterUnitForm.DataByUid.Values)
        {
            if (unitData.unit.productInfo.Item1 != productData.uid)
                continue;
            if (GameMapData.ApplyCharacterProductSize(unitData, validSize))
                MapManager.instance.updateCtrl.RefreshCharacterOverlap(unitData.unit);
        }
    }

    private void OnCharacterPassTypeChanged(CharacterProductForm.Data productData, List<int> oldValue, List<int> newValue)
    {
        foreach (var unitData in CharacterUnitForm.DataByUid.Values)
        {
            if (unitData.unit.productInfo.Item1 == productData.uid)
                GameMapData.ApplyCharacterProductPassTypes(unitData, productData);
        }
    }

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
        private readonly Dictionary<BodyPartType, AnimDirecton> animDirectionCache = new Dictionary<BodyPartType, AnimDirecton>();

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
                    return AnimDirecton.Fixed;
            }

        }

        public void Reset()
        {
            CancelTimers();
            stateCur.Clear();
            dirCur = AnimDirecton.Fixed;
            foreach (BodyPartType part in Enum.GetValues(typeof(BodyPartType)))
            {
                animCur[part] = "";
                animTar[part] = "";
                stateCd[part] = 0;
                animCurCache[part] = "";
                animTimer[part] = null;
                animDirectionCache[part] = AnimDirecton.Fixed;
            }
        }
        public void CancelTimers()
        {
            foreach (var timer in animTimer.Values)
            {
                if (timer != null)
                    timer.cancel = true;
            }
        }
        public void CloseHideRenderer()
        {
            foreach (BodyPartType part in Enum.GetValues(typeof(BodyPartType)))
            {
                if (part == BodyPartType.None)
                    continue;
                var anim = productData.animDic.GetDv(animCur[part], null);
                bool enabled = anim != null && anim.partEnable.ContainsKey(part) && anim.partEnable[part];
                // Runtime templates start disabled; animation owns part enablement,
                // while map visibility remains in the renderer's _Show override.
                var renderer = data.unit.ins.renderers[(int)part - 1];
                if (renderer != null && renderer.enabled != enabled)
                    renderer.enabled = enabled;
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
            if (part == BodyPartType.None || data.unit.ins == null)
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
                stateCd[part] = 2;
            }

            animTar[part] = tarAnimName;
            if (tar != State.Special && tar != State.Die)
            {
                if (animCur[part] != tarAnimName)
                {
                    stateCd[part]--;
                }
                else if (GetDir() != dirCur)
                {
                    // 方向改变时强制更新动画
                    ChangeState(tar, part, forceReplay, extraAnimName);
                    return;
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
            var newDir = GetDir();
            if (animCurCache.ContainsKey(part) && animCurCache[part] == anim.name)
            {
                if (newDir == animDirectionCache[part])
                {
                    return;
                }
            }
            dirCur = newDir;
            animDirectionCache[part] = newDir;
            TimeManager.instance.CancelTimer(animTimer[part]);
            animTimer[part] = null;
            animCurCache[part] = anim.name;
            if (anim.animClip.TryGetValue(dirCur, out var clip) && clip.Count > 0)
            {
                var texIds = new int[clip.Count];
                for (int i = 0; i < clip.Count; i++)
                    clip[i].partTex.TryGetValue(part, out texIds[i]);
                var first = TextureAnimation.GetSample(texIds, anim.animTimeInterval, 0);
                ApplyAnimationTexture(data.unit.ins, render, (int)part - 1, first.texture);
                if (first.animated || anim.animTimeInterval > 0)
                {
                    double elapsed = 0;
                    var owner = data.unit.ins;
                    bool internalAnimation = false;
                    foreach (int id in texIds)
                        internalAnimation |= TexAssetForm.DataById.TryGetValue(id, out var texData)
                            && texData.GetAnimationDuration() > 0;
                    Func<bool> update = () =>
                    {
                        if (data.unit.ins != owner || owner.unit != data.unit || render == null)
                            return true;
                        return AdvanceTextureAnimation(part, render, texIds, anim.animTimeInterval,
                            ref elapsed, internalAnimation ? TimeManager.GetDeltaTime() : anim.animTimeInterval);
                    };
                    animTimer[part] = internalAnimation
                        ? TimeManager.instance.StartFrameTimer(update, owner)
                        : TimeManager.instance.StartTimer(0, anim.animTimeInterval, update, owner);
                }
            }
            else
                ApplyAnimationTexture(data.unit.ins, render, (int)part - 1, null);
        }

        private bool AdvanceTextureAnimation(BodyPartType part, Renderer render, int[] texIds,
            float interval, ref double elapsed, float delta)
        {
            elapsed += delta;
            bool loop = stateCur[part] != State.Special && stateCur[part] != State.Die;
            var sample = TextureAnimation.GetSample(texIds, interval, elapsed, loop);
            ApplyAnimationTexture(data.unit.ins, render, (int)part - 1, sample.texture);
            if (!sample.completed)
                return false;
            if (stateCur[part] == State.Special)
                ChangeState(State.Idle, part);
            // Die retains the last GIF frame; Special resumes Idle only after its hold.
            return true;
        }

        private static void ApplyAnimationTexture(CharacterInstance ins, Renderer render, int rendererId, Texture texture)
        {
            texture = texture != null ? texture : Texture2D.whiteTexture;
            DisFadeRenderRouting.SetTexture(render, texture, ins.GetRendererRoute(rendererId));

        }
    }

    public void RegisterAnim(CharacterUnitForm.Data data)
    {
        if (!TryGetVisibleCharacter(data, out var productData) || animControllerDic.ContainsKey(data))
        {
            return;
        }
        CharacterAnimForm.Data idleAnim = productData.animDic.GetDv(productData.defaultAnimName.GetDv("idle", null), null);
        CharacterAnimForm.Data moveAnim = productData.animDic.GetDv(productData.defaultAnimName.GetDv("move", null), null);

        CreateAnim(data, idleAnim, moveAnim);
    }
    private static bool TryGetVisibleCharacter(CharacterUnitForm.Data data, out CharacterProductForm.Data product)
    {
        product = null;
        if (data == null || !CharacterUnitForm.DataByUid.TryGetValue(data.uid, out var registered)
            || !ReferenceEquals(registered, data))
            return false;
        var unit = data.unit;
        if (unit == null || !unit.isShowing || !ReferenceEquals(unit.ins.unit, unit))
            return false;
        return CharacterProductForm.DataByUid.TryGetValue(unit.productInfo.Item1, out product) && product != null;
    }
    private bool TryGetAnimController(CharacterUnitForm.Data data, out AnimController controller)
    {
        controller = null;
        if (!TryGetVisibleCharacter(data, out _))
            return false;
        if (!animControllerDic.TryGetValue(data, out controller))
        {
            // Scene data may exist before its first Show, or survive a controller reset.
            RegisterAnim(data);
            animControllerDic.TryGetValue(data, out controller);
        }
        return controller != null;
    }
    public void UnregisterAnim(CharacterUnitForm.Data data)
    {
        if (data == null || !animControllerDic.TryGetValue(data, out var ctrl))
            return;
        ctrl.CancelTimers();
        animControllerDic.Remove(data);
    }
    public void Reset()
    {
        foreach (var ctrl in animControllerDic.Values)
            ctrl.CancelTimers();
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
        if (ins == null || ins.unit == null)
            return;
        var data = ins.unit.data;
        if (!TryGetAnimController(data, out var status))
            return;
        var form = CharacterProductForm.DataByUid[data.unit.productInfo.Item1];
        status.Reset();

        foreach (var keeper in ins.keepers)
        {
            keeper.enableFixedYRotation = form.faceType != FaceType.Flexible;
            keeper.enableFixedZRotation0 = form.faceType != FaceType.Flexible;
            keeper.fixedYRotation = 0;
        }
    }

    public void PlaySpecialAnim(CharacterUnitForm.Data ch, string animName)
    {
        if (!TryGetAnimController(ch, out var status))
            return;

        foreach (BodyPartType part in Enum.GetValues(typeof(BodyPartType)))
        {
            status.TryChangeState(AnimController.State.Special, part, true, animName);
        }
    }

    public void CheckAnim(CharacterInstance ins)
    {
        if (ins == null || ins.unit == null)
        {
            return;
        }
        var data = ins.unit.data;
        if (!TryGetAnimController(data, out var status))
            return;

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
                if (GameMapData.ApplyCharacterProductSize(evt.unit.data))
                    MapManager.instance.updateCtrl.RefreshCharacterOverlap(evt.unit);
                GameMapData.ApplyCharacterProductPassTypes(evt.unit.data);
                RegisterAnim(evt.unit.data);
                LoadModel(evt.unit.ins);
                break;
            case MapEventType.AfterUpdate:
                //manage nav
                var productData = CharacterProductForm.DataByUid.GetDv(evt.unit.productInfo.Item1, null);
                if (productData != null)
                {
                    if (GameMapData.ApplyCharacterProductSize(evt.unit.data, productData))
                        MapManager.instance.updateCtrl.RefreshCharacterOverlap(evt.unit);
                    GameMapData.ApplyCharacterProductPassTypes(evt.unit.data, productData);
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
