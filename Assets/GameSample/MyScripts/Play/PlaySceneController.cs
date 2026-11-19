using Form;
using Microsoft.Win32;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using Ui;
using Ui.ModSceneUnit;
using Ui.PlaySceneMain;
using UnityEngine;
using UnityEngine.TextCore.Text;
using Z_DataSystem;
using Z_DataSystem.Form;
using Z_Debug;
using Z_DesignStyle;
using Z_Input;
using Z_Map;
using Z_Map.Form;
using Z_Time;
using Z_Ui;
using Z_Ui.Notify;
using Z_UnitSystem;

public interface InternalPlaySceneController
{
    public string fileName { get; }
    public void Begin(int id);
    public void End();
    public void Update();


}
public interface ExternalPlaySceneController
{
    public CharacterUnitForm.Data playerM { get; }
    public CharacterProductForm.Data playerG { get; }
    public void SetCamera(float x, float y, float z);
    public Vector3 GetPlayerPos();
    public void SetPlayerPos(Vector3 pos);
    public void SetPlayerMove(Vector3 dir);
    public void SetPlayerRotation(Vector3 dir, float speed);
    public void AddMessage(string content);
    public void ForceUpdate();
    public void RefreshCurrentCharacter();
    public SkillType? GetCurOptSkill();
    public void SetCurOptSkill(SkillType? type);

    public CharacterUnitForm.Data GetCharacterUnit(int productUid);
    public List<CharacterUnitForm.Data> GetCharacterUnits(int productUid);
    public bool RemoveCharacter(CharacterUnitForm.Data data);
}
public class PlaySceneController : Z_Controller<PlayManager>, InternalPlaySceneController, ExternalPlaySceneController, IZ_Listener<InputKeyEvent>, IZ_Listener<InputKeyUpEvent>, IZ_Listener<InputMouseEvent>, IZ_Listener<InputMouseDownEvent>, IZ_Listener<InputMouseUpEvent>, IZ_Listener<InputMouseMoveEvent>
{
    private static readonly Unity.Profiling.ProfilerMarker PlayerMoveMarker = new Unity.Profiling.ProfilerMarker("Play.PlayerMovement");
    private static readonly Unity.Profiling.ProfilerMarker CameraMarker = new Unity.Profiling.ProfilerMarker("Play.CameraFollow");
    private static readonly Unity.Profiling.ProfilerMarker TriggersMarker = new Unity.Profiling.ProfilerMarker("Play.DeferredSceneTriggers");
    private static readonly Unity.Profiling.ProfilerMarker NavigationMarker = new Unity.Profiling.ProfilerMarker("Map.NavigationUpdate");
    public PlaySceneController(PlayManager super) : base(super)
    {

    }

    bool enable = false;
    bool waitForActive = false;

    // Play 相机正交尺寸（视野半高，世界单位）：原 4，现 +1 为 5
    private const float PlayCameraOrthographicSize = 5f;
    // Play 相机正交尺寸下限/上限，下限保持原值，仅初始尺寸变大
    private const float PlayCameraOrthographicSizeMin = 4f;
    private const float PlayCameraOrthographicSizeMax = 15f;

    Vector3 lastPlayerPos;
    Vector3 setPlayerMove;
    Quaternion? setPlayerRot;
    float rotHoldingSeconds;

    SkillType? curOptSkill;

    Vector2 downPos;
    private CharacterUnitForm.Data _playerM;
    private CharacterProductForm.Data _playerG;
    private Dictionary<CharacterProductForm.Data, CharacterUnitForm.Data> _characterDic;
    private readonly Dictionary<CharacterProductForm.Data, List<CharacterUnitForm.Data>> characterUnitGroups =
        new Dictionary<CharacterProductForm.Data, List<CharacterUnitForm.Data>>();
    private static int characterLookupVersion;
    private static bool characterLookupTracking;
    private int cachedCharacterLookupVersion = -1;
    private Dictionary<int, CharacterUnitForm.Data> cachedCharacterRows;
    private Dictionary<int, CharacterProductForm.Data> cachedCharacterProducts;
    private int cachedCharacterCount;
    private int cachedCharacterProductCount;

    internal static void InvalidateCharacterLookup() { unchecked { characterLookupVersion++; } }
    private static void OnCharacterRowChanged(CharacterUnitForm.Data _) => InvalidateCharacterLookup();
    private static void OnCharacterProductChanged(CharacterProductForm.Data _) => InvalidateCharacterLookup();

    private static void TrackCharacterLookupChanges()
    {
        if (characterLookupTracking) return;
        CharacterUnitForm.addAction += OnCharacterRowChanged;
        CharacterUnitForm.removeAction += OnCharacterRowChanged;
        CharacterProductForm.addAction += OnCharacterProductChanged;
        CharacterProductForm.removeAction += OnCharacterProductChanged;
        characterLookupTracking = true;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetCharacterLookupTracking()
    {
        CharacterUnitForm.addAction -= OnCharacterRowChanged;
        CharacterUnitForm.removeAction -= OnCharacterRowChanged;
        CharacterProductForm.addAction -= OnCharacterProductChanged;
        CharacterProductForm.removeAction -= OnCharacterProductChanged;
        characterLookupTracking = false;
        InvalidateCharacterLookup();
    }
    private int updateNavFrame;
    #region internal Var
    private string _fileName;
    public string fileName { get => _fileName; }
    #endregion

    #region extern Var
    public CharacterUnitForm.Data playerM => _playerM;
    public CharacterProductForm.Data playerG => _playerG;

    #endregion


    public void Begin(int id)
    {
        updateNavFrame = 0;
        curOptSkill = null;
        setPlayerRot = null;
        downPos = Vector2.zero;

        this._fileName = Main2StoryManager.GetSceneFileNameById(id);
        CameraInstance.instance.Register(Vector3.zero, Z_Math.Graph.ElementwiseMultiply(MapManager.instance.sizeLimit, MapManager.instance.data.mainData.mapUnitSize), PlayCameraOrthographicSizeMin, PlayCameraOrthographicSizeMax, PlayCameraOrthographicSize);
        //CameraInstance.instance.tarTrs.position = Z_Math.Graph.ElementwiseMultiply(new Vector3(500, 500, 500), MapManager.instance.data.mainData.mapUnitSize);
        switch (DynamicGlobalSettings.cameraMode)
        {
            case CameraMode.Overhead:
                CameraInstance.instance.cam.transform.localPosition = new Vector3(0, 8, 0);
                CameraInstance.instance.cam.transform.eulerAngles = new Vector3(90, 0, 0);
                CameraInstance.instance.globalLight.transform.eulerAngles = Vector3.right * 80;
                CameraInstance.instance.globalLight.intensity = 1f;
                break;
            case CameraMode.Isometric:
                CameraInstance.instance.cam.transform.localPosition = new Vector3(0, 8, -8);
                CameraInstance.instance.cam.transform.eulerAngles = new Vector3(45, 0, 0);
                CameraInstance.instance.globalLight.transform.eulerAngles = Vector3.right * 70;
                CameraInstance.instance.globalLight.intensity = 1.3f;
                break;
        }

        // 事件 Cmd(SetLightIntensity) 记录过的强度优先于相机视角默认值。
        GameManager.ApplyProgressLightIntensity();

        setPlayerMove = Vector3.zero;
        enable = true;
        this.Register<InputKeyEvent>();
        this.Register<InputKeyUpEvent>();
        this.Register<InputMouseEvent>();
        this.Register<InputMouseDownEvent>();
        this.Register<InputMouseUpEvent>();
        this.Register<InputMouseMoveEvent>();
        waitForActive = false;
        _characterDic = new Dictionary<CharacterProductForm.Data, CharacterUnitForm.Data>();
        cachedCharacterLookupVersion = -1;
        characterUnitGroups.Clear();
        UiManager.instance.ShowUi<UiPlaySceneMainCtrl>();


        //character Reflect


        foreach (var data in CharacterUnitForm.DataByUid.Values)
        {
            var ch = CharacterProductForm.DataByUid[data.unit.productInfo.Item1];

            if (ch.protoUid == 0 && !ch.unique)
            {
                var newCharacter = ch.Copy(false);
                newCharacter.ToProduct(ch.uid);

                Debug.Log(Time.frameCount + ":" + newCharacter.uid + " replaced " + ch.uid);
                data.name = newCharacter.name;
                data.unit.productInfo = (newCharacter.uid, -1);
                if (GameMapData.ApplyCharacterProductSize(data, newCharacter))
                    MapManager.instance.updateCtrl.RefreshCharacterOverlap(data.unit);
                GameMapData.ApplyCharacterProductPassTypes(data, newCharacter);
                _characterDic[newCharacter] = data;
                if (data.minimapIcon<=0)
                {
                    data.minimapIcon = newCharacter.minimapIcon;
                }
            }
            else
            {
                data.name = ch.name;
                data.unit.productInfo = (ch.uid, -1);
                if (GameMapData.ApplyCharacterProductSize(data, ch))
                    MapManager.instance.updateCtrl.RefreshCharacterOverlap(data.unit);
                GameMapData.ApplyCharacterProductPassTypes(data, ch);
                _characterDic[ch] = data;
            }
        }
        //object correct
        foreach (var data in ObjectUnitForm.DataByUid.Values)
        {
            var cur = data.unit.BindProduct();
            if (cur != null)
            {
                data.isObstacle = cur.collision;
                if (data.minimapIcon <= 0)
                {
                    data.minimapIcon = cur.minimapIcon;
                }
            }
        }
        //item correct
        foreach (var data in ItemUnitForm.DataByUid.Values)
        {
            var cur = ItemProductForm.DataByUid.GetDv(data.unit.productInfo.Item1, null);
            if (cur != null)
            {
                if (data.minimapIcon<=0)
                {
                    data.minimapIcon = cur.minimapIcon;
                }
            }
        }
        foreach (var data in TileUnitForm.DataByUid.Values)
            GameMapData.ApplyTilePassTypes(data);

        MapManager.instance.navigationCtrl.UpdateMap(int.MaxValue);


        _playerM = null;
        _playerG = null;
        PlayInfoController.RepairCurrentCharacterHealth(GameManager.instance.curProgress);
        RefreshCurrentCharacter();
        var sceneUi = UiManager.instance.GetUi<UiPlaySceneMainCtrl>();
        if (sceneUi != null && sceneUi.active)
            sceneUi.Refresh();
    }
    public void RefreshCurrentCharacter()
    {
        var progress = GameManager.instance.curProgress;
        if (!enable || _characterDic == null || progress == null)
            return;
        var next = CharacterProductForm.DataByUid.GetDv(progress.characterUid, null);
        if (next != null && ReferenceEquals(_playerG, next) && _playerM != null
            && CharacterUnitForm.DataByUid.TryGetValue(_playerM.uid, out var registered)
            && ReferenceEquals(registered, _playerM) && _playerM.unit != null
            && _playerM.unit.productInfo.Item1 == next.uid)
            return;
        Vector3? previousEuler = _playerM?.euler;
        setPlayerMove = Vector3.zero;
        setPlayerRot = null;
        curOptSkill = null;
        if (_playerM != null)
        {
            progress.pos = _playerM.pos;
            RemoveCharacter(_playerM);
        }
        _playerG = next;
        if (_playerG == null)
        {
            _playerM = null;
            lastPlayerPos = GameManager.instance.curProgress.pos;
            SetCamera(lastPlayerPos.x, lastPlayerPos.y, lastPlayerPos.z);
            return;
        }
        _playerM = GetOrNewCharacter(_playerG);
        // A unique character may already exist: inherit the outgoing player's pose,
        // not the incoming NPC's old facing or queued rotation.
        if (previousEuler.HasValue)
            _playerM.unit.forceEuler = null;
        MapManager.instance.updateCtrl.ApplyMove(_playerM.unit, GameManager.instance.curProgress.pos,
            previousEuler ?? _playerM.euler, true);
        GameManager.instance.curProgress.pos = _playerM.pos;
        lastPlayerPos = _playerM.pos;
        SetCamera(lastPlayerPos.x, lastPlayerPos.y, lastPlayerPos.z);
    }
    public void End()
    {
        GameManager.instance.evtCtrl.ClearSceneEvent();
        CameraInstance.instance.RestorePlayerFollow();

        // UnloadScenePlay saves once, before resetting the old scene controllers.
        enable = false;

        NotifyManager.instance.ClearAll();

        this.Unregister<InputKeyEvent>();
        this.Unregister<InputKeyUpEvent>();
        this.Unregister<InputMouseEvent>();
        this.Unregister<InputMouseDownEvent>();
        this.Unregister<InputMouseUpEvent>();
        this.Unregister<InputMouseMoveEvent>();

        MapManager.instance.End();
        _characterDic?.Clear();
        characterUnitGroups.Clear();
        cachedCharacterRows = null;
        cachedCharacterProducts = null;
        cachedCharacterLookupVersion = -1;
        UiManager.instance.CloseAll();
        GameManager.instance.RegisterInputDefault();
        //UiManager.instance.ShowUi<UiModStoryCtrl>();

    }
    public T TryGetUnit<T>(RaycastHit[] hits)
    {
        foreach (var hit in hits)
        {
            var tmp = hit.transform.parent.GetComponent<Instance>();
            if (tmp != null && tmp.unit.isVising)
            {
                return (T)(object)tmp.unit;
            }
        }
        return default(T);
    }
    public void OnMouse(bool drag, bool release, Vector3 pos, Vector3 dir)
    {
        if (GameManager.instance.curProgress.blockProgramUid != 0)
        {
            return;
        }
        if (waitForActive || !enable)
            return;
        // set z
        //if(release)
        {
            if (_playerG != null && _playerG.skill.ContainsKey(SkillType.LightAttack) && SkillProductForm.DataByUid.ContainsKey(_playerG.skill[SkillType.LightAttack]))
            {
                var data = SkillProductForm.DataByUid[_playerG.skill[SkillType.LightAttack]];
                _super.infoCtrl.UseSkill(_playerG.uid, data.uid, false);

            }
        }

    }
    public void OnMouseMove(Vector3 pos)
    {
        if (GameManager.instance.curProgress.blockProgramUid > 0)
        {
            return;
        }
        SetPlayerRotation(new Vector3(pos.x - InputManager.instance.screenSize.x / 2, 0, (pos.y - InputManager.instance.screenSize.y / 2)*(Z_Map.DynamicGlobalSettings.cameraMode == CameraMode.Isometric?1.414f:1) ));
    }

    public void ForceUpdate()
    {

        MapManager.instance.updateCtrl.ResetView();
        if (waitForActive)
            return;

        waitForActive = true;
        TimeManager.instance.AddNextBigFrameAction(() =>
        {
            waitForActive = false;
        }, _super.gameObject);
    }
    public void Update()
    {
        if (!enable)
            return;

        {
            MapManager.instance.updateCtrl.UpdateInfo();
            {
                MapManager.instance.SetPos(CameraInstance.instance.tarTrs.position);
            }
        }

        if (setPlayerMove != Vector3.zero)
        {
            if (_playerM != null)
            {
                using (PlayerMoveMarker.Auto()) _playerM.unit.Move(setPlayerMove);
            }
            setPlayerMove = Vector3.zero;
        }

        if (setPlayerRot != null && _playerM != null)
        {
            _playerM.unit.forceEuler = ((Quaternion)setPlayerRot).eulerAngles;

#if PLATFORM_ANDROID
            rotHoldingSeconds -= TimeManager.GetDeltaTime();
            if (rotHoldingSeconds < 0)
            {
                setPlayerRot = null;
                rotHoldingSeconds = 2f;
            }
#endif
        }
        if (_playerM != null)
        {

            lastPlayerPos = _playerM.pos;
            using (CameraMarker.Auto()) SetCamera(lastPlayerPos.x, lastPlayerPos.y, lastPlayerPos.z);
            GameManager.instance.curProgress.pos = _playerM.pos;
        }

        using (TriggersMarker.Auto())
        {
            GameManager.instance.evtCtrl.sceneTriggerCtrl.evts?.Invoke();
            GameManager.instance.evtCtrl.sceneTriggerCtrl.evts -= GameManager.instance.evtCtrl.sceneTriggerCtrl.evts;
        }

        if (updateNavFrame < Time.frameCount)
        {
            using (NavigationMarker.Auto()) MapManager.instance.navigationCtrl.UpdateMap(Mathf.Max((TileUnitForm.DataByUid.Count * 3 + ObjectUnitForm.DataByUid.Count) / 100, 5));
            updateNavFrame = Time.frameCount + 60;
        }
    }
    public void SetCamera(float x, float y, float z)
    {
        CameraInstance.instance.FollowPlayer(new Vector3(x, y, z));
        //Z_EventHelper.Invoke(new CameraMoveEvent());
    }

    public CharacterUnitForm.Data GetCharacterUnit(int productUid)
    {
        if (productUid <= 0 || !CharacterProductForm.DataByUid.TryGetValue(productUid, out var source))
            return null;
        EnsureCharacterLookup();
        var result = GetCachedCharacter(source);
        if (source.protoUid == 0 && CharacterProductForm.DatasByProtouid.TryGetValue(productUid, out var copies))
            foreach (var copy in copies)
            {
                var unit = GetCachedCharacter(copy);
                if (unit != null && (result == null || unit.uid < result.uid)) result = unit;
            }
        return result;
    }

    public List<CharacterUnitForm.Data> GetCharacterUnits(int productUid)
    {
        var result = new List<CharacterUnitForm.Data>();
        if (productUid <= 0 || !CharacterProductForm.DataByUid.TryGetValue(productUid, out var source))
            return result;
        EnsureCharacterLookup();
        AddCachedCharacters(source, result);
        if (source.protoUid == 0 && CharacterProductForm.DatasByProtouid.TryGetValue(productUid, out var copies))
            foreach (var copy in copies) AddCachedCharacters(copy, result);
        result.Sort((left, right) => left.uid.CompareTo(right.uid));
        return result;
    }

    private static bool IsCurrentCharacter(CharacterUnitForm.Data unit, CharacterProductForm.Data product)
    {
        return unit != null && product != null
            && CharacterProductForm.DataByUid.TryGetValue(product.uid, out var currentProduct)
            && ReferenceEquals(product, currentProduct)
            && CharacterUnitForm.DataByUid.TryGetValue(unit.uid, out var currentUnit) && ReferenceEquals(unit, currentUnit)
            && Z_UnitSystem.Form.UnitForm.DataByUid.TryGetValue(unit.uid, out var registered) && ReferenceEquals(unit, registered)
            && unit.unit != null && ReferenceEquals(unit.unit.data, unit) && unit.unit.productInfo.Item1 == product.uid;
    }

    private void EnsureCharacterLookup()
    {
        TrackCharacterLookupChanges();
        var rows = CharacterUnitForm.DataByUid;
        var products = CharacterProductForm.DataByUid;
        if (_characterDic != null && cachedCharacterLookupVersion == characterLookupVersion
            && ReferenceEquals(rows, cachedCharacterRows) && ReferenceEquals(products, cachedCharacterProducts)
            && cachedCharacterCount == rows.Count && cachedCharacterProductCount == products.Count)
            return;
        if (_characterDic == null) _characterDic = new Dictionary<CharacterProductForm.Data, CharacterUnitForm.Data>();
        else _characterDic.Clear();
        characterUnitGroups.Clear();
        foreach (var unit in rows.Values)
        {
            if (unit?.unit == null || !products.TryGetValue(unit.unit.productInfo.Item1, out var product)
                || !IsCurrentCharacter(unit, product)) continue;
            if (!characterUnitGroups.TryGetValue(product, out var group))
                characterUnitGroups.Add(product, group = new List<CharacterUnitForm.Data>());
            group.Add(unit);
            if (!_characterDic.TryGetValue(product, out var first) || unit.uid < first.uid) _characterDic[product] = unit;
        }
        cachedCharacterRows = rows;
        cachedCharacterProducts = products;
        cachedCharacterCount = rows.Count;
        cachedCharacterProductCount = products.Count;
        cachedCharacterLookupVersion = characterLookupVersion;
    }

    private CharacterUnitForm.Data GetCachedCharacter(CharacterProductForm.Data product)
    {
        if (_characterDic.TryGetValue(product, out var unit) && IsCurrentCharacter(unit, product)) return unit;
        CharacterUnitForm.Data result = null;
        if (characterUnitGroups.TryGetValue(product, out var group))
            foreach (var candidate in group)
                if (IsCurrentCharacter(candidate, product) && (result == null || candidate.uid < result.uid)) result = candidate;
        return result;
    }

    private void AddCachedCharacters(CharacterProductForm.Data product, List<CharacterUnitForm.Data> result)
    {
        if (characterUnitGroups.TryGetValue(product, out var group))
            foreach (var unit in group)
                if (IsCurrentCharacter(unit, product)) result.Add(unit);
    }

    public bool RemoveCharacter(CharacterUnitForm.Data data)
    {
        if (data == null || !CharacterUnitForm.DataByUid.TryGetValue(data.uid, out var registered)
            || !ReferenceEquals(registered, data))
            return false;

        MapManager.instance.RemoveCharacter(data);
        var product = CharacterProductForm.DataByUid.GetDv(data.unit.productInfo.Item1, null);
        if (product != null && _characterDic != null
            && _characterDic.TryGetValue(product, out var cached) && ReferenceEquals(cached, data))
            _characterDic.Remove(product);
        if (ReferenceEquals(_playerM, data))
        {
            lastPlayerPos = data.pos;
            _playerM = null;
            _playerG = null;
            setPlayerMove = Vector3.zero;
            setPlayerRot = null;
        }
        return true;
    }

    public CharacterUnitForm.Data GetOrNewCharacter(CharacterProductForm.Data data)
    {
        EnsureCharacterLookup();
        var unitData = GetCachedCharacter(data);
        if (unitData == null)
        {
            unitData = MapManager.instance.AddCharacter(data.name, GameManager.instance.curProgress.pos, GlobalDefaultHelper.GetRuntimePrefabName("character"), true, MapUnit.GetProductInfoString(new Newtonsoft.Json.Linq.JObject(), (data.uid, -1)), GameMapData.GetCharacterProductSize(data));
        }
        if (GameMapData.ApplyCharacterProductSize(unitData, data))
            MapManager.instance.updateCtrl.RefreshCharacterOverlap(unitData.unit);
        _characterDic[data] = unitData;
        return unitData;
    }

    public Vector3 GetPlayerPos()
    {
        return _playerM != null ? _playerM.pos : lastPlayerPos;
    }
    public void SetPlayerPos(Vector3 pos)
    {
        GameManager.instance.curProgress.pos = pos;
        if (_playerM == null || _playerM.unit.ins == null)
            return;
        _playerM.unit.ins.transform.position = pos;
        _playerM.pos = pos;
    }
    public void SetPlayerMove(Vector3 dir)
    {

        if (_playerM == null || _playerM.unit.ins == null)
            return;
        if (CharacterParamForm.DataByName.ContainsKey(_playerG.speedParamName))
        {
            setPlayerMove += dir * (float)_playerG.paramDic[_playerG.speedParamName].GetValue().num;
        }
        else
        {
            setPlayerMove += dir;
        }
    }
    public void SetPlayerRotation(Vector3 dir, float speed = 360)
    {
        if (_playerM == null || _playerM.unit.ins == null)
            return;
        dir.y = 0;
        Quaternion targetRotation = Quaternion.LookRotation(dir);
        setPlayerRot = Quaternion.Slerp(Quaternion.Euler(_playerM.euler), targetRotation, speed * MapUtilController.MovementDeltaTime);


    }
    public void AddMessage(string content)
    {
        var ctrl = UiManager.instance.GetUi<UiPlaySceneMainCtrl>();
        if (ctrl != null)
        {
            ctrl.view.page_PlaySceneMessage.AddMessage(content);
        }
    }


    public SkillType? GetCurOptSkill()
    {
        return curOptSkill;
    }
    public void SetCurOptSkill(SkillType? type)
    {
        curOptSkill = type;
    }

    #region op
    public void OnEvent(InputKeyEvent evt)
    {
        if (!enable || GameManager.instance.curProgress.blockProgramUid != 0)
            return;

        var cur = evt.key.Where((o) => o == KeyCode.W || o == KeyCode.S || o == KeyCode.A || o == KeyCode.D);
        var step = Vector3.zero;
        foreach (var c in cur)
        {
            switch (c)
            {
                case KeyCode.W:
                    step += Vector3.forward;
                    break;
                case KeyCode.S:
                    step += Vector3.back;
                    break;

                case KeyCode.A:
                    step += Vector3.left;
                    break;
                case KeyCode.D:
                    step += Vector3.right;
                    break;
            }
        }
        SetPlayerMove(step.normalized * MapUtilController.MovementDeltaTime);
    }
    public void OnEvent(InputKeyUpEvent evt)
    {
        int tar = -1;
        if (evt.key.Contains(KeyCode.Alpha1))
        {
            tar = 0;
        }
        else if (evt.key.Contains(KeyCode.Alpha2))
        {
            tar = 1;
        }
        else if (evt.key.Contains(KeyCode.Alpha3))
        {
            tar = 2;
        }
        else if (evt.key.Contains(KeyCode.Alpha4))
        {
            tar = 3;
        }
        if (tar != -1 && GameManager.instance.curProgress.teamActive.Count > tar)
        {
            PlayManager.instance.infoCtrl.ChooseCurrentCharacter(GameManager.instance.curProgress.teamActive[tar]);
        }

        if (evt.key.Contains(KeyCode.E))
        {
            if (_playerG != null && _playerG.skill.ContainsKey(SkillType.E) && SkillProductForm.DataByUid.ContainsKey(_playerG.skill[SkillType.E]))
            {
                var data = SkillProductForm.DataByUid[_playerG.skill[SkillType.E]];
                _super.infoCtrl.UseSkill(_playerG.uid, data.uid, false);
            }
        }
        if (evt.key.Contains(KeyCode.Q))
        {
            if (_playerG != null && _playerG.skill.ContainsKey(SkillType.Q) && SkillProductForm.DataByUid.ContainsKey(_playerG.skill[SkillType.Q]))
            {
                var data = SkillProductForm.DataByUid[_playerG.skill[SkillType.Q]];
                _super.infoCtrl.UseSkill(_playerG.uid, data.uid, false);
            }
        }
    }

    public void OnEvent(InputMouseEvent evt)
    {
        if (!enable || GameManager.instance.curProgress.blockProgramUid != 0)
            return;
        if (evt.id == 0 && evt.ui == null && downPos != Vector2.zero)
        {
            OnMouse((downPos - new Vector2(evt.pos.x, evt.pos.y)).sqrMagnitude >= GlobalSettings.DRAG_DIS2, false, evt.pos, evt.delta);
        }
    }

    public void OnEvent(InputMouseDownEvent evt)
    {
        if (!enable || GameManager.instance.curProgress.blockProgramUid != 0)
            return;
        if (evt.ui == null)
        {
            downPos = evt.pos;
        }
        else
        {
            downPos = Vector2.zero;
        }
    }

    public void OnEvent(InputMouseUpEvent evt)
    {
        if (!enable || GameManager.instance.curProgress.blockProgramUid != 0)
            return;

        if (evt.id == 0 && evt.ui == null && downPos != Vector2.zero)
        {
            OnMouse((downPos - new Vector2(evt.pos.x, evt.pos.y)).sqrMagnitude < GlobalSettings.DRAG_DIS2, true, evt.pos, Vector3.zero);
        }
        downPos = Vector2.zero;
    }

    public void OnEvent(InputMouseMoveEvent evt)
    {
        if (!enable || GameManager.instance.curProgress.blockProgramUid != 0)
            return;
        OnMouseMove(evt.pos);
    }
    #endregion
}
