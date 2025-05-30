using Form;
using Microsoft.Win32;
using System.Collections;
using System.Collections.Generic;
using Ui;
using Ui.ModSceneUnit;
using Ui.ModStory;
using Ui.PlaySceneMain;
using UnityEditor;
using UnityEngine;
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
    public void Begin(string fileName);
    public void End();
    public void Update();
    public void OnMouse(bool click, Vector3 pos, Vector3 dir);
    public void OnMouseMove(Vector3 pos);

    
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
public class PlaySceneController : Z_Controller<PlayManager>, InternalPlaySceneController, ExternalPlaySceneController
{
    public PlaySceneController(PlayManager super) : base(super)
    {

    }

    bool enable = false;
    bool waitForActive = false;

    Vector3 lastPlayerPos;
    Vector3 setPlayerMove;
    private CharacterUnitForm.Data _playerM;
    private CharacterProductForm.Data _playerG;
    private Dictionary<CharacterUnitForm.Data, CharacterProductForm.Data> _characterDic;

    #region internal Var
    private string _fileName;
    public string fileName { get => _fileName; }
    #endregion

    #region extern Var
    public CharacterUnitForm.Data playerM=> _playerM;
    public CharacterProductForm.Data playerG=> _playerG;

    #endregion


    public void Begin(string fileName)
    {
        this._fileName = fileName;
        GameManager.instance.RegisterInputByPlay();
        CameraInstance.instance.Register(Vector3.zero, Z_Math.Graph.ElementwiseMultiply(MapManager.instance.sizeLimit, MapManager.instance.data.mainData.mapUnitSize), 5, 15);
        //CameraInstance.instance.tarTrs.position = Z_Math.Graph.ElementwiseMultiply(new Vector3(500, 500, 500), MapManager.instance.data.mainData.mapUnitSize);
        setPlayerMove = Vector3.zero;
         enable = true;
        waitForActive = false;
        _characterDic = new Dictionary<CharacterUnitForm.Data, CharacterProductForm.Data>();
        UiManager.instance.ShowUi<UiPlaySceneMainCtrl>();
        _playerM = null;
        _playerG = null;
    }
    public void End()
    {
        enable = false;
        MapManager.instance.End();
        UiManager.instance.CloseAll();
        GameManager.instance.RegisterInputDefault();
        UiManager.instance.ShowUi<UiModStoryCtrl>();

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
        SetPlayerRotation( new Vector3(pos.x - InputManager.instance.screenSize.x/2,0, pos.y - InputManager.instance.screenSize.y / 2));
    }
    
    public void ForceUpdate()
    {

        MapManager.instance.ResetInfo();
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
            _playerG = CharacterProductForm.DataByNameIsproto[(_super.data.progress.characterName,true)];
        }
        if (_playerM == null)
        {
            _playerM = CreateCharacter(_playerG); 
        }
        {
            MapManager.instance.UpdateInfo();
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

        lastPlayerPos = _playerM.pos;
        SetCamera(lastPlayerPos.x, lastPlayerPos.y, lastPlayerPos.z);
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
        var unitData=  MapManager.instance.AddCharacter(_super.data.progress.pos, GlobalNameHelper.GetRuntimePrefabName("character"), true);
        _characterDic[unitData] = data;
        return unitData;
    }

    public Vector3 GetPlayerPos()
    {
        return lastPlayerPos;
    }
    public void SetPlayerPos(Vector3 pos)
    {
      if (_playerM == null|| _playerM.unit.ins==null)
            return;
        _playerM.unit.ins.transform.position = pos;
    }
    public void SetPlayerMove(Vector3 dir)
    {
        if (_playerM == null || _playerM.unit.ins == null)
            return;
        if(CharacterParamForm.DataByName.ContainsKey(_playerG.speedParamName))
        {
            setPlayerMove += dir * (float)_playerG.paramDic[_playerG.speedParamName].v;
        }else
        {
            setPlayerMove += dir;
        }
    }
    public void SetPlayerRotation(Vector3 dir,float speed=360)
    {
        if (_playerM == null|| _playerM.unit.ins==null)
            return;
        dir.y = 0;
        Quaternion targetRotation = Quaternion.LookRotation(dir);
        _playerM.unit.ins.transform.rotation= Quaternion.Slerp(Quaternion.Euler(_playerM.euler), targetRotation, speed * Time.deltaTime);
    }
    public void AddMessage(string content)
    {
        var ctrl = UiManager.instance.GetUi<UiPlaySceneMainCtrl>();
        if(ctrl!=null)
        {
            ctrl.view.page_PlaySceneMessage.AddMessage(content);
        }
    }
}
