using Form;
using Microsoft.Win32;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using Ui;
using Ui.ModSceneUnit;
using Ui.PlaySceneMain;
using UnityEditor;
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
using Z_UnitSystem;
using static UnityEditor.PlayerSettings;

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
}
public class PlaySceneController : Z_Controller<PlayManager>, InternalPlaySceneController, ExternalPlaySceneController, IZ_Listener<InputKeyEvent>, IZ_Listener<InputKeyUpEvent>, IZ_Listener<InputMouseEvent>, IZ_Listener<InputMouseDownEvent>, IZ_Listener<InputMouseUpEvent>, IZ_Listener<InputMouseMoveEvent>
{
    public PlaySceneController(PlayManager super) : base(super)
    {

    }

    bool enable = false;
    bool waitForActive = false;

    Vector3 lastPlayerPos;
    Vector3 setPlayerMove;
    Quaternion? setPlayerRot;
    float rotHoldingSeconds;

    SkillType? curOptSkill;

    Vector2 downPos;
    private CharacterUnitForm.Data _playerM;
    private CharacterProductForm.Data _playerG;
    private Dictionary<CharacterProductForm.Data, CharacterUnitForm.Data> _characterDic;
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
        CameraInstance.instance.Register(Vector3.zero, Z_Math.Graph.ElementwiseMultiply(MapManager.instance.sizeLimit, MapManager.instance.data.mainData.mapUnitSize), 5, 15);
        //CameraInstance.instance.tarTrs.position = Z_Math.Graph.ElementwiseMultiply(new Vector3(500, 500, 500), MapManager.instance.data.mainData.mapUnitSize);
        switch (DynamicGlobalSettings.cameraMode)
        {
            case CameraMode.Overhead:
                CameraInstance.instance.cam.transform.localPosition = new Vector3(0, 8, 0);
                CameraInstance.instance.cam.transform.eulerAngles = new Vector3(90, 0, 0);
                break;
            case CameraMode.Isometric:
                CameraInstance.instance.cam.transform.localPosition = new Vector3(0, 8, -8);
                CameraInstance.instance.cam.transform.eulerAngles = new Vector3(45, 0, 0);
                break;
        }

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
                _characterDic[newCharacter] = data;
                if (string.IsNullOrEmpty(data.minimapIcon))
                {
                    data.minimapIcon = newCharacter.minimapIcon;
                }
            }
            else
            {
                data.name = ch.name;
                data.unit.productInfo = (ch.uid, -1);
                _characterDic[ch] = data;
            }
        }
        //object correct
        foreach (var data in ObjectUnitForm.DataByUid.Values)
        {
            var cur = MapObjectForm.DataById.GetDv(data.unit.productInfo.Item1, null);
            if (cur != null)
            {
                data.isObstacle = cur.collision;
                if (string.IsNullOrEmpty(data.minimapIcon))
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
                if (string.IsNullOrEmpty(data.minimapIcon))
                {
                    data.minimapIcon = cur.minimapIcon;
                }
            }
        }

        MapManager.instance.navigationCtrl.UpdateMap(int.MaxValue);


        _playerM = null;
        _playerG = null;
        RefreshCurrentCharacter();
    }
    public void RefreshCurrentCharacter()
    {
        if (_playerM != null)
        {
            MapManager.instance.RemoveCharacter(_playerM);
        }
        _playerG = CharacterProductForm.DataByUid[GameManager.instance.curProgress.characterUid];
        _playerM = GetOrNewCharacter(_playerG);
    }
    public void End()
    {
        GameManager.instance.evtCtrl.ClearSceneEvent();

        GameManager.instance.saveCtrl.SaveSceneMap(PlayManager.instance.GetSceneCacheFileName());
        enable = false;

        this.Unregister<InputKeyEvent>();
        this.Unregister<InputKeyUpEvent>();
        this.Unregister<InputMouseEvent>();
        this.Unregister<InputMouseDownEvent>();
        this.Unregister<InputMouseUpEvent>();
        this.Unregister<InputMouseMoveEvent>();

        MapManager.instance.End();
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
            pos.z = CameraInstance.instance.cam.nearClipPlane;
            // to world
            Vector3 worldPosition = CameraInstance.instance.cam.ScreenToWorldPoint(pos);
            worldPosition.y = CameraInstance.instance.tarTrs.position.y;
            var hits = Physics.RaycastAll(worldPosition + Vector3.up * 100, Vector3.down);
            var hitPos = MapManager.instance.utilCtrl.RealPos2MapPos(worldPosition);
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
        SetPlayerRotation(new Vector3(pos.x - InputManager.instance.screenSize.x / 2, 0, pos.y - InputManager.instance.screenSize.y / 2));
    }

    public void ForceUpdate()
    {

        MapManager.instance.updateCtrl.ResetInfo();
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
                _playerM.unit.Move(setPlayerMove);
            }
            setPlayerMove = Vector3.zero;
        }

        if (setPlayerRot != null)
        {
            _playerM.unit.forceEuler = ((Quaternion)setPlayerRot).eulerAngles;

#if PLATFORM_ANDROID
            rotHoldingSeconds -= Time.deltaTime;
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
            SetCamera(lastPlayerPos.x, lastPlayerPos.y, lastPlayerPos.z);
            GameManager.instance.curProgress.pos = _playerM.pos;
        }

        GameManager.instance.evtCtrl.sceneTriggerCtrl.evts?.Invoke();
        GameManager.instance.evtCtrl.sceneTriggerCtrl.evts -= GameManager.instance.evtCtrl.sceneTriggerCtrl.evts;

        if (updateNavFrame < Time.frameCount)
        {
            MapManager.instance.navigationCtrl.UpdateMap(Mathf.Max((TileUnitForm.DataByUid.Count * 3 + ObjectUnitForm.DataByUid.Count) / 50, 10));
            updateNavFrame = Time.frameCount + 60;
        }
    }
    public void SetCamera(float x, float y, float z)
    {
        CameraInstance.instance.tarTrs.position = new Vector3(x, y, z);
        //Z_EventHelper.Invoke(new CameraMoveEvent());
    }

    public CharacterUnitForm.Data GetCharacterUnit(int productUid)
    {
        var data = CharacterProductForm.DataByUid.GetDv(productUid, null);
        if (data == null || !_characterDic.ContainsKey(data))
            return null;
        return _characterDic[data];
    }

    public CharacterUnitForm.Data GetOrNewCharacter(CharacterProductForm.Data data)
    {
        CharacterUnitForm.Data unitData = null;
        foreach (var curData in CharacterUnitForm.DataByUid.Values)
        {
            if (curData.unit.productInfo.Item1 == data.uid)
            {
                unitData = curData;
                break;
            }
        }
        if (unitData == null)
        {
            unitData = MapManager.instance.AddCharacter(data.name, GameManager.instance.curProgress.pos, GlobalNameHelper.GetRuntimePrefabName("character"), true, MapUnit.GetProductInfoString(new Newtonsoft.Json.Linq.JObject(), (data.uid, -1)));
        }
        _characterDic[data] = unitData;
        return unitData;
    }

    public Vector3 GetPlayerPos()
    {
        return lastPlayerPos;
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
        setPlayerRot = Quaternion.Slerp(Quaternion.Euler(_playerM.euler), targetRotation, speed * Time.deltaTime);


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
        SetPlayerMove(step.normalized * 0.02f);
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
