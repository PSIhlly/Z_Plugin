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

    public CharacterProductForm.Data GetCharacterProduct(CharacterUnitForm.Data data);
}
public class PlaySceneController : Z_Controller<PlayManager>, InternalPlaySceneController, ExternalPlaySceneController, IZ_Listener<InputKeyEvent>, IZ_Listener<InputMouseEvent>, IZ_Listener<InputMouseDownEvent>, IZ_Listener<InputMouseUpEvent>, IZ_Listener<InputMouseMoveEvent>
{
    public PlaySceneController(PlayManager super) : base(super)
    {
        this.Register<InputKeyEvent>();
        this.Register<InputMouseEvent>();
        this.Register<InputMouseDownEvent>();
        this.Register<InputMouseUpEvent>();
        this.Register<InputMouseMoveEvent>();
    }

    bool enable = false;
    bool waitForActive = false;

    Vector3 lastPlayerPos;
    Vector3 setPlayerMove;
    Quaternion? setPlayerRot;
    float rotHoldingSeconds;

    Vector2 downPos;
    private CharacterUnitForm.Data _playerM;
    private CharacterProductForm.Data _playerG;
    private Dictionary<CharacterUnitForm.Data, CharacterProductForm.Data> _characterDic;

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
        waitForActive = false;
        _characterDic = new Dictionary<CharacterUnitForm.Data, CharacterProductForm.Data>();
        UiManager.instance.ShowUi<UiPlaySceneMainCtrl>();
        _playerM = null;
        _playerG = null;

        //character Reflect

        foreach (var data in CharacterUnitForm.DataByUid.Values)
        {
            var ch = CharacterProductForm.DataByUid[AssetManager.GetKeyId(data.name)];
            if (ch.isProto && !ch.unique)
            {
                var newCharacter = ch.Copy(false);
                newCharacter.ToProduct();
                data.name = AssetManager.GetIdNameKey(newCharacter.uid, newCharacter.name);
                GameManager.instance.characterCtrl.RegisterAnim(newCharacter);
            }
        }
    }
    public void End()
    {

        GameManager.instance.saveCtrl.SaveSceneMap(PlayManager.instance.GetSceneCacheFileName());
        enable = false;
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
    public void OnMouse(bool click, Vector3 pos, Vector3 dir)
    {
        if (GameManager.instance.curProgress.blockProgramUid > 0)
        {
            return;
        }
        if (waitForActive || !enable)
            return;
        // set z
        pos.z = CameraInstance.instance.cam.nearClipPlane;
        // to world
        Vector3 worldPosition = CameraInstance.instance.cam.ScreenToWorldPoint(pos);
        worldPosition.y = CameraInstance.instance.tarTrs.position.y;
        var hits = Physics.RaycastAll(worldPosition + Vector3.up * 100, Vector3.down);
        var hitPos = MapManager.instance.utilCtrl.RealPos2MapPos(worldPosition);

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
        if (_playerG == null)
        {
            _playerG = CharacterProductForm.DataByUid[GameManager.instance.curProgress.characterUid];
        }
        if (_playerM == null)
        {
            _playerM = CreateCharacter(_playerG);
        }
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
            _playerM.unit.ins.transform.rotation = (Quaternion)setPlayerRot;

#if PLATFORM_ANDROID
            rotHoldingSeconds-=Time.deltaTime;
            if(rotHoldingSeconds < 0)
            {
                setPlayerRot = null;
                rotHoldingSeconds = 2f;
            }
#endif
        }

        lastPlayerPos = _playerM.pos;
        SetCamera(lastPlayerPos.x, lastPlayerPos.y, lastPlayerPos.z);

        GameManager.instance.curProgress.pos = _playerM.pos;
    }
    public void SetCamera(float x, float y, float z)
    {
        CameraInstance.instance.tarTrs.position = new Vector3(x, y, z);
        //Z_EventHelper.Invoke(new CameraMoveEvent());
    }
    public CharacterProductForm.Data GetCharacterProduct(CharacterUnitForm.Data data)
    {
        if (!_characterDic.ContainsKey(data))
            return null;
        return _characterDic[data];
    }

    public CharacterUnitForm.Data CreateCharacter(CharacterProductForm.Data data)
    {
        CharacterUnitForm.Data unitData = null;
        foreach (var unit in CharacterUnitForm.DataByUid.Values)
        {
            if (unit.name == AssetManager.GetIdNameKey(data.uid, data.name))
            {
                unitData = unit;
                break;
            }
        }
        if (unitData == null)
        {
            unitData=MapManager.instance.AddCharacter(AssetManager.GetIdNameKey(data.uid, data.name), GameManager.instance.curProgress.pos, GlobalNameHelper.GetRuntimePrefabName("character"), true); ;
        }
        _characterDic[unitData] = data;
        return unitData;
    }

    public Vector3 GetPlayerPos()
    {
        return lastPlayerPos;
    }
    public void SetPlayerPos(Vector3 pos)
    {
        if (_playerM == null || _playerM.unit.ins == null)
            return;
        _playerM.unit.ins.transform.position = pos;
    }
    public void SetPlayerMove(Vector3 dir)
    {

        if (_playerM == null || _playerM.unit.ins == null)
            return;
        if (CharacterParamForm.DataByName.ContainsKey(_playerG.speedParamName))
        {
            setPlayerMove += dir * (float)_playerG.paramDic[_playerG.speedParamName].v;
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
        setPlayerRot= Quaternion.Slerp(Quaternion.Euler(_playerM.euler), targetRotation, speed * Time.deltaTime);
        
        
    }
    public void AddMessage(string content)
    {
        var ctrl = UiManager.instance.GetUi<UiPlaySceneMainCtrl>();
        if (ctrl != null)
        {
            ctrl.view.page_PlaySceneMessage.AddMessage(content);
        }
    }


    #region op
    public void OnEvent(InputKeyEvent evt)
    {
        if (!enable || GameManager.instance.curProgress.blockProgramUid >0)
            return;
        var cur = evt.key.Where((o) => o == KeyCode.W || o == KeyCode.S || o == KeyCode.A || o == KeyCode.D);
        var step = Vector3.zero;
        foreach(var c in cur)
        {
            switch(c)
            {
                case KeyCode.W:
                    step+= Vector3.forward;
                    break;
                case KeyCode.S:
                    step += Vector3.back;
                    break;

                case KeyCode.A:
                    step +=Vector3.left;
                    break;
                case KeyCode.D:
                    step +=  Vector3.right;
                    break;
            }
        }
        SetPlayerMove(step.normalized* 0.02f);
    }

    public void OnEvent(InputMouseEvent evt)
    {
        if (!enable || GameManager.instance.curProgress.blockProgramUid > 0)
            return;
        if (evt.id == 0 && evt.ui == null && downPos != Vector2.zero)//&& (downPos - new Vector2(pos.x, pos.y)).sqrMagnitude > dragDis2
        {
            OnMouse(false, evt.pos, evt.delta);
        }
    }

    public void OnEvent(InputMouseDownEvent evt)
    {
        if (!enable || GameManager.instance.curProgress.blockProgramUid > 0)
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
        if (!enable || GameManager.instance.curProgress.blockProgramUid > 0)
            return;
        if (evt.id == 0 && evt.ui == null && downPos != Vector2.zero && (downPos - new Vector2(evt.pos.x, evt.pos.y)).sqrMagnitude < GlobalSettings.DRAG_DIS2)
        {
            OnMouse(true, evt.pos, Vector3.zero);
        }
        downPos = Vector2.zero;
    }

    public void OnEvent(InputMouseMoveEvent evt)
    {
        if (!enable || GameManager.instance.curProgress.blockProgramUid > 0)
            return;
        OnMouseMove(evt.pos);
    }
    #endregion
}
