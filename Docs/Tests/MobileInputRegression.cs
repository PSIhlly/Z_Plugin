// Runtime sources are inserted by Run-MobileInputRegression.ps1. These small
// Unity/service spies exercise actual input and UI code without starting Unity,
// loading Forms, or touching authored stories/saves. Not a Player/device test.
namespace UnityEngine
{
    public class DefaultExecutionOrder : Attribute { public DefaultExecutionOrder(int value) {} }
    public class GameObject { public int layer = 5; public string name; public bool activeSelf=true; public bool activeInHierarchy=>activeSelf; public GameObject(string name = "") { this.name = name; } public void SetActive(bool value) {activeSelf=value;} }
    public class Sprite {}
    public static class Time { public static float unscaledTime; }
    public struct Color { public float r,g,b,a; public Color(float r,float g,float b,float a) {this.r=r;this.g=g;this.b=b;this.a=a;} }
    namespace UI { public class Graphic { public bool enabled=true,raycastTarget=true; public Color color=new Color(1,1,1,1); } }
    public struct Vector2
    {
        public float x, y;
        public Vector2(float x, float y) { this.x = x; this.y = y; }
        public static Vector2 zero => new Vector2();
        public float sqrMagnitude => x*x + y*y;
        public static Vector2 operator -(Vector2 a, Vector2 b) => new Vector2(a.x-b.x, a.y-b.y);
        public static implicit operator Vector2(Vector3 v) => new Vector2(v.x, v.y);
        public static implicit operator Vector3(Vector2 v) => new Vector3(v.x, v.y, 0);
    }
    public struct Vector3
    {
        public float x,y,z;
        public Vector3(float x,float y,float z = 0) { this.x=x; this.y=y; this.z=z; }
        public static Vector3 zero => new Vector3();
        public static Vector3 one => new Vector3(1,1,1);
        public float sqrMagnitude => x*x+y*y+z*z;
        public float magnitude => (float)Math.Sqrt(sqrMagnitude);
        public Vector3 normalized => magnitude > 0 ? this / magnitude : zero;
        public static Vector3 operator +(Vector3 a,Vector3 b) => new Vector3(a.x+b.x,a.y+b.y,a.z+b.z);
        public static Vector3 operator -(Vector3 a,Vector3 b) => new Vector3(a.x-b.x,a.y-b.y,a.z-b.z);
        public static Vector3 operator *(Vector3 a,float b) => new Vector3(a.x*b,a.y*b,a.z*b);
        public static Vector3 operator *(float b,Vector3 a) => a*b;
        public static Vector3 operator /(Vector3 a,float b) => a*(1/b);
        public static bool operator ==(Vector3 a,Vector3 b) => a.x==b.x && a.y==b.y && a.z==b.z;
        public static bool operator !=(Vector3 a,Vector3 b) => !(a==b);
        public override bool Equals(object obj) => obj is Vector3 v && this==v;
        public override int GetHashCode() => HashCode.Combine(x,y,z);
    }
    public class RectTransform { public GameObject gameObject = new GameObject(); public Vector3 position; public Vector2 size = new Vector2(100,100); public UI.Graphic graphic=new UI.Graphic(); public T GetComponent<T>() where T:class => graphic as T; }
    public enum TouchPhase { Began, Moved, Stationary, Ended, Canceled }
    public struct Touch { public int fingerId; public Vector2 position; public TouchPhase phase; }
    public enum KeyCode { W,S,A,D,E,Q,Alpha1,Alpha2,Alpha3,Alpha4 }
    public static class Input
    {
        public static Touch[] touches = Array.Empty<Touch>();
        public static int touchCount => touches.Length;
        public static Touch GetTouch(int i) => touches[i];
        public static Vector3 mousePosition;
        public static bool[] down = new bool[2], up = new bool[2], held = new bool[2];
        public static bool GetMouseButtonDown(int i) => down[i];
        public static bool GetMouseButtonUp(int i) => up[i];
        public static bool GetMouseButton(int i) => held[i];
        public static float GetAxis(string name) => 0;
        public static bool GetKeyDown(KeyCode key) => false;
        public static bool GetKeyUp(KeyCode key) => false;
        public static bool GetKey(KeyCode key) => false;
        public static void Reset() { touches=Array.Empty<Touch>(); mousePosition=Vector3.zero; Array.Clear(down,0,2); Array.Clear(up,0,2); Array.Clear(held,0,2); }
    }
    public static class Screen { public static int width=800, height=600; }
    public class Camera { public static Camera main = new Camera(); public float orthographicSize=5, aspect=1.5f; }
    namespace EventSystems
    {
        public class PointerEventData { public Vector2 position; public PointerEventData(EventSystem system) {} }
        public struct RaycastResult { public GameObject gameObject; }
        public class EventSystem
        {
            public static EventSystem current = new EventSystem();
            public Func<Vector2,GameObject> Hit = point => null;
            public void RaycastAll(PointerEventData pointer,List<RaycastResult> results) { var hit=Hit(pointer.position); if(hit!=null) results.Add(new RaycastResult{gameObject=hit}); }
        }
    }
}
namespace Z_DesignStyle { public class Z_MonoManager<T> { public virtual void Init() {} } }
namespace Z_Debug { public static class Z_Log { public static void Log(object item) {} } }
namespace Z_Ui.Base { public class UiCtrl { public virtual void OnCreate() {} public virtual void OnShow() {} public virtual void OnHide() {} public virtual void OnUpdate() {} } }
namespace Z_Math
{
    public static class Graph
    {
        public static Vector2 GetSize(RectTransform rect) => rect.size;
        public static Vector2 GetNormalizedRelativePos(Vector3 point,RectTransform rect) => new Vector2((point.x-rect.position.x)/rect.size.x+0.5f,(point.y-rect.position.y)/rect.size.y+0.5f);
    }
}
namespace Ui.Stick
{
    public class ImageSpy { public Sprite sprite; }
    public class UiStickView { public RectTransform rtf_area=new RectTransform(),rtf_stick=new RectTransform(); public ImageSpy img_stick=new ImageSpy(); }
    public partial class UiStickCtrl : UiCtrl
    {
        public UiStickView view=new UiStickView(); public UiStickModel model=new UiStickModel(); public UiStickParam param=new UiStickParam();
        public bool active=true;
        public int ShowCalls;
        public void SetShow(bool show,UiStickParam prm) {ShowCalls++;param=prm;active=show;if(show)OnShow();else OnHide();}
    }
}
namespace Form
{
    public enum SkillType { LightAttack, Other, E, Q }
    public static class CharacterProductForm
    {
        public class Data { public int uid; public Dictionary<SkillType,int> skill=new Dictionary<SkillType,int>(); }
        public static Dictionary<int,Data> DataByUid = new Dictionary<int,Data>();
    }
}
public static class DictionaryFixtureExtension { public static V GetDv<K,V>(this Dictionary<K,V> values,K key,V fallback) => values.TryGetValue(key,out var value) ? value : fallback; }
namespace Z_Map { public static class MapUtilController { public const float MovementDeltaTime=0.02f; } }
public class ProgressSpy { public int blockProgramUid,characterUid; public bool enableSkill=true; }
public class GameManager { public static GameManager instance=new GameManager(); public ProgressSpy curProgress=new ProgressSpy(); }
public class ScenePlaySpy
{
    public SkillType? Opt;
    public int Moves,Rotations;
    public Vector3 LastMove;
    public Vector3 LastRotation;
    public void SetPlayerMove(Vector3 delta) { Moves++; LastMove=delta; }
    public void SetPlayerRotation(Vector3 dir,int speed) { Rotations++; LastRotation=dir; }
    public SkillType? GetCurOptSkill() => Opt;
    public void SetCurOptSkill(SkillType? skill) { Opt=skill; }
}
public class PlayInfoSpy
{
    public int Uses,LastCharacter,LastSkill,CanCalls;
    public Action OnUse;
    public Dictionary<SkillType,bool> Available=new Dictionary<SkillType,bool>();
    public Dictionary<SkillType,int> CallsByType=new Dictionary<SkillType,int>();
    public bool CanUseSkill(int character,SkillType type) {CanCalls++;CallsByType[type]=CallsByType.TryGetValue(type,out var calls)?calls+1:1;return PlayManager.instance.sceneCtrl.Opt==null && (!Available.TryGetValue(type,out var available)||available);}
    public void UseSkill(int character,int skill,bool flag) {Uses++;LastCharacter=character;LastSkill=skill;OnUse?.Invoke();}
}
public class PlayManager { public static PlayManager instance=new PlayManager(); public ScenePlaySpy sceneCtrl=new ScenePlaySpy(); public PlayInfoSpy infoCtrl=new PlayInfoSpy(); }
public class StickSlotSpy
{
    public UiStickCtrl Ctrl;
    public UiStickParam Param;
    public UiStickModel model => Ctrl.model;
    public void SetShow(bool show,UiStickParam param) { Param=param; if(Ctrl!=null) { Ctrl.param=param; Ctrl.OnShow(); } }
}
namespace Ui.PlaySceneMain.PlayerTouchOpt
{
    public class PlayerTouchViewSpy { public StickSlotSpy model_moveStick=new StickSlotSpy(),model_attackStick=new StickSlotSpy(); }
    public partial class UiPlayerTouchOptCtrl : UiCtrl { public PlayerTouchViewSpy view=new PlayerTouchViewSpy(); public UiPlayerTouchOptModel model=new UiPlayerTouchOptModel(); }
}
public static class GlobalSettings { public const float DRAG_DIS2=25; }
public class ModManager
{
    public static ModManager instance=new ModManager();
    public int Moves;
    public Vector3 LastPos,LastDelta;
    public void OnMouse(bool click,Vector3 pos,Vector3 delta) { Moves++; LastPos=pos; LastDelta=delta; }
}
public class ModHistorySpy
{
    public int Begins,Commits,EditedCommits;
    public IDisposable BeginOperation(Func<bool> changed) { Begins++; return new Operation(this,changed); }
    private class Operation : IDisposable { private ModHistorySpy owner; private Func<bool> changed; public Operation(ModHistorySpy owner,Func<bool> changed) {this.owner=owner;this.changed=changed;} public void Dispose() {if(owner!=null) {owner.Commits++;if(changed())owner.EditedCommits++;owner=null;}} }
}
public class ModPointerFixture : IZ_Listener<InputPointDownEvent>,IZ_Listener<InputPointEvent>,IZ_Listener<InputPointUpEvent>,IZ_Listener<InputMouseDownEvent>,IZ_Listener<InputMouseEvent>,IZ_Listener<InputMouseUpEvent>
{
    bool enable=true, pointerDown, pointerIsTouch, eventLayerBrushTipShown;
    int pointerId,brushRevision;
    Vector2 downPos;
    public object curData;
    public ModHistorySpy history=new ModHistorySpy();
    private IDisposable brushOperation;
    public int Clicks;
    public bool ThrowOnClick;
    public bool Captured => pointerDown;
    public int Id => pointerId;
    public bool IsTouch => pointerIsTouch;
    public bool BrushTip => eventLayerBrushTipShown;
    public void MarkChanged() { brushRevision++; }
    public void OnMouse(bool click,Vector3 pos,Vector3 delta) { Clicks++; if(ThrowOnClick) throw new InvalidOperationException("click failed"); }
    public void SetEnabled(bool enabled) { enable=enabled; }
    public void FinishTouch() { FinishTouchGesture(); }
    public void ResetForLifecycle() { CommitPendingOperation(); ResetPointer(); }
__MOD_COMMIT_METHOD__
__MOD_POINTER_METHODS__
}
public class SkillButtonsViewSpy { public UiStickCtrl model_eStick,model_qStick; }
public class SkillButtonFixture
{
    public SkillButtonsViewSpy view=new SkillButtonsViewSpy();
    UiStickParam eParam,qParam;
    public UiStickParam E => eParam;
    public UiStickParam Q => qParam;
    public void InitializeCallbacks()
    {
__SKILL_SETUP__
    }
    public void Bind(UiStickCtrl e,UiStickCtrl q) {view.model_eStick=e;view.model_qStick=q;e.SetShow(true,eParam);q.SetShow(true,qParam);}
    public void RefreshButtons()
    {
__SKILL_REFRESH_CALLS__
    }
__SKILL_HELPERS__
}
public class InputRecorder : IZ_Listener<InputPointDownEvent>,IZ_Listener<InputPointEvent>,IZ_Listener<InputPointUpEvent>,IZ_Listener<InputMouseDownEvent>,IZ_Listener<InputMouseEvent>,IZ_Listener<InputMouseUpEvent>
{
    public readonly List<InputPointDownEvent> Down=new List<InputPointDownEvent>();
    public readonly List<InputPointEvent> Move=new List<InputPointEvent>();
    public readonly List<InputPointUpEvent> Up=new List<InputPointUpEvent>();
    public int MouseDown,MouseMove,MouseUp;
    public InputManager Manager;
    public bool RemovedBeforeUp=true;
    public InputRecorder(InputManager manager) {Manager=manager;this.Register<InputPointDownEvent>();this.Register<InputPointEvent>();this.Register<InputPointUpEvent>();this.Register<InputMouseDownEvent>();this.Register<InputMouseEvent>();this.Register<InputMouseUpEvent>();}
    public void OnEvent(InputPointDownEvent evt) {Down.Add(evt);}
    public void OnEvent(InputPointEvent evt) {Move.Add(evt);}
    public void OnEvent(InputPointUpEvent evt) {Up.Add(evt);RemovedBeforeUp &= !Manager.id2Pos.ContainsKey(evt.id);}
    public void OnEvent(InputMouseDownEvent evt) {MouseDown++;}
    public void OnEvent(InputMouseEvent evt) {MouseMove++;}
    public void OnEvent(InputMouseUpEvent evt) {MouseUp++;}
}
public static class MobileInputRegression
{
    static int checks;
    static void Check(bool condition,string message) {if(!condition) throw new InvalidOperationException(message); checks++;}
    static void Call(object target,string method,params object[] args) {target.GetType().GetMethod(method,BindingFlags.Instance|BindingFlags.NonPublic).Invoke(target,args);}
    static void Reset() {((System.Collections.IDictionary)typeof(Z_EventHelper).GetField("type2Listener",BindingFlags.Static|BindingFlags.NonPublic).GetValue(null)).Clear();Input.Reset();Time.unscaledTime=0;EventSystem.current=new EventSystem();}
    static Touch T(int id,float x,float y,TouchPhase phase) => new Touch{fingerId=id,position=new Vector2(x,y),phase=phase};
    static void Frame(InputManager manager,params Touch[] touches) {Input.touches=touches;Call(manager,"ManageTouch");}
    static UiStickCtrl Stick(float x,UiStickParam param=null) {var stick=new UiStickCtrl();stick.view.rtf_area.position=new Vector3(x,100);stick.view.rtf_stick.position=stick.view.rtf_area.position;stick.param=param??new UiStickParam();stick.OnShow();return stick;}
    static InputPointDownEvent PD(int id,float x,GameObject ui) => new InputPointDownEvent{id=id,pos=new Vector3(x,100),ui=ui};
    static InputPointEvent PM(int id,float x) => new InputPointEvent{id=id,pos=new Vector3(x,100)};
    static InputPointUpEvent PU(int id,bool canceled=false) => new InputPointUpEvent{id=id,pos=new Vector3(100,100),canceled=canceled};
    public static int Run()
    {
        checks=0; TouchRouting(); StickRouting(); FloatingSticks(); PlayerGates(); PlayerFacing(); SkillButtons(); ModPointers(); return checks;
    }
    static void TouchRouting()
    {
        Reset();var input=new InputManager();var record=new InputRecorder(input);
        Frame(input,T(21,100,10,TouchPhase.Began),T(73,300,10,TouchPhase.Began));
        Check(record.Down.Select(e=>e.id).SequenceEqual(new[]{21,73}) && input.id2Pos.Count==2,"Began must preserve Unity finger IDs.");
        Frame(input,T(73,100,10,TouchPhase.Moved),T(21,300,10,TouchPhase.Moved));
        Check(record.Move[0].id==73 && record.Move[0].delta.x==-200 && record.Move[1].id==21 && record.Move[1].delta.x==200,"Crossing/reordered fingers must retain IDs and their own deltas.");
        Frame(input,T(73,100,10,TouchPhase.Stationary),T(21,300,10,TouchPhase.Ended));
        Check(record.Move.Last().delta==Vector3.zero && record.Up.Last().id==21 && !record.Up.Last().canceled,"Stationary must be zero delta and Ended must be ordinary up.");
        Frame(input,T(73,100,10,TouchPhase.Canceled));
        Check(record.Up.Last().id==73 && record.Up.Last().canceled && input.id2Pos.Count==0 && input.id2OldPos.Count==0 && record.RemovedBeforeUp,"Cancellation must remove pointer before notifying listeners.");
        Frame(input,T(91,50,50,TouchPhase.Began));Frame(input);
        Check(record.Up.Last().id==91 && record.Up.Last().canceled,"A disappeared touch must cancel its gesture.");
        int down=record.Down.Count;Frame(input,T(99,50,50,TouchPhase.Moved));
        Check(record.Down.Count==down && !input.id2Pos.ContainsKey(99),"A held finger without Began must not be captured.");
        Frame(input,T(5,50,50,TouchPhase.Began));Frame(input,T(5,70,50,TouchPhase.Began));
        Check(record.Up.Last().id==5 && record.Up.Last().canceled && input.id2Pos[5].x==70,"Duplicate Began must cancel the old capture before reuse.");
        EventSystem.current=null;Frame(input,T(5,70,50,TouchPhase.Ended));
        Check(record.Up.Last().ui==null,"Missing EventSystem must be harmless during release.");

        Reset();input=new InputManager();record=new InputRecorder(input);Input.down[0]=Input.held[0]=true;Input.mousePosition=new Vector3(200,100);Input.touches=new[]{T(17,100,100,TouchPhase.Began)};input.Update();
#if UNITY_ANDROID && !UNITY_EDITOR
        Check(record.Down.Count==1 && record.MouseDown==0 && record.MouseMove==0,"Android must route touch only, without synthetic mouse dispatch.");
        Call(input,"OnApplicationFocus",false);Check(input.id2Pos.Count==0 && record.Up.Last().canceled,"Focus loss must cancel every live pointer.");
        Input.touches=new[]{T(17,100,100,TouchPhase.Stationary)};input.Update();Call(input,"OnApplicationFocus",true);input.Update();
        Check(input.id2Pos.Count==0,"Focus resume must wait for a fresh Began.");
        Input.touches=new[]{T(18,100,100,TouchPhase.Began)};input.Update();Call(input,"OnApplicationPause",true);
        Check(record.Up.Last().id==18 && record.Up.Last().canceled && input.id2Pos.Count==0,"Pause must cancel captured fingers.");
        Call(input,"OnApplicationPause",false);Input.touches=new[]{T(18,100,100,TouchPhase.Stationary)};input.Update();Check(input.id2Pos.Count==0,"Unpause must not recapture a held finger.");
        Input.touches=new[]{T(19,100,100,TouchPhase.Began)};input.Update();Call(input,"OnDisable");Check(record.Up.Last().id==19 && record.Up.Last().canceled,"Component disable must cancel touch state.");
        Input.touches=new[]{T(20,100,100,TouchPhase.Began)};input.Update();input.enabled=false;input.Update();Check(record.Up.Last().id==20 && record.Up.Last().canceled && input.id2Pos.Count==0,"Disabled input processing must not leave held fingers.");
#else
        Check(record.Down.Count==0 && record.MouseDown==1,"Desktop must retain mouse routing, not Android touch polling.");
        Input.down[0]=false;Input.mousePosition=new Vector3(230,100);input.Update();Check(record.MouseMove==1,"Desktop held mouse movement must remain available.");
        Input.up[0]=true;Input.held[0]=false;input.Update();Check(record.MouseUp==1,"Desktop mouse release must remain available.");
#endif
    }
    static void StickRouting()
    {
        Reset();int leftDrag=0,leftUp=0,leftHide=0,rightUp=0;bool allow=true,start=true;
        var left=Stick(100,new UiStickParam{dragAct=v=>leftDrag++,upAct=v=>leftUp++,hideAct=()=>leftHide++,canStartFunc=()=>start,canContinueFunc=()=>allow});
        var right=Stick(300,new UiStickParam{upAct=v=>rightUp++});
        Z_EventHelper.Invoke(PD(21,125,left.view.rtf_area.gameObject));Z_EventHelper.Invoke(PD(73,325,right.view.rtf_area.gameObject));
        Check(left.model.curId==21 && right.model.curId==73,"Both sticks must capture independent fingers.");
        Z_EventHelper.Invoke(PM(21,330));Check(right.model.lastPos.x==0.25f && left.model.curId==21,"A finger crossing into another stick must not move or steal that stick.");
        Z_EventHelper.Invoke(PU(73));Check(left.model.curId==21 && right.model.curId==-1 && rightUp==1 && leftUp==0,"Releasing the other stick must not release this one.");
        left.OnEvent(new InputMouseUpEvent{id=21});left.OnEvent(new InputMouseEvent{id=21,pos=new Vector3(100,100)});
        Check(left.model.curId==21 && left.model.lastPos.x>2,"Mouse with the same numeric ID must not affect a touch capture.");
        left.OnEvent(PD(77,100,left.view.rtf_area.gameObject));Check(left.model.curId==21,"Another finger must not overwrite the first capture.");
        start=false;left.OnUpdate();Check(leftDrag==1 && left.model.curId==21,"Start-only checks must not cancel a gesture after skill ownership changes.");
        allow=false;left.OnUpdate();Check(left.model.curId==-1 && leftHide==1 && leftUp==0 && leftDrag==1,"Continuation failure must stop dragging without release action.");
        left.OnEvent(PU(21));Check(leftUp==0,"A later release of a canceled gesture must not trigger an action.");
        start=allow=true;left.OnEvent(PD(8,125,left.view.rtf_area.gameObject));left.OnEvent(PU(8,true));Check(left.model.curId==-1 && leftHide==2 && leftUp==0,"Canceled touch must use hide action, not up action.");
        left.OnEvent(PD(9,125,left.view.rtf_area.gameObject));allow=false;left.OnEvent(PU(9));Check(leftHide==3 && leftUp==0,"Same-frame block before Up must not release a skill.");
        allow=true;left.OnEvent(PD(10,125,left.view.rtf_area.gameObject));left.OnHide();Check(left.model.curId==-1 && left.model.lastPos==Vector3.zero,"Hide must reset capture and displacement.");
        left.OnShow();left.OnEvent(PM(10,145));Check(left.model.curId==-1,"Reopening must not restore the previous held finger.");
        left.OnEvent(new InputMouseDownEvent{id=0,pos=new Vector3(125,100),ui=left.view.rtf_area.gameObject});left.OnEvent(PU(0));Check(left.model.curId==0 && !left.model.curIsTouch,"Touch ID zero must not release a desktop mouse capture.");
        left.OnEvent(new InputMouseEvent{id=0,pos=new Vector3(140,100)});left.OnEvent(new InputMouseUpEvent{id=0});Check(left.model.curId==-1 && leftUp==1,"Desktop mouse stick drag/release must remain available.");
        left.OnHide();right.OnHide();

        Reset();leftUp=leftDrag=0;Vector3 released=Vector3.one;
        left=Stick(100,new UiStickParam{dragAct=v=>leftDrag++,upAct=v=>{leftUp++;released=v;}});
        left.OnEvent(PD(101,100,left.view.rtf_stick.gameObject));Check(left.model.curId==101 && left.model.lastPos==Vector3.zero,"The authored center Graphic must start and capture a zero-offset gesture.");
        left.OnUpdate();left.OnEvent(PM(101,140));left.OnUpdate();left.OnEvent(PU(101));
        Check(leftDrag==2 && leftUp==1 && Math.Abs(released.x-0.4f)<0.00001 && left.model.curId==-1,"Center press must remain captured through edge movement and normal release.");
        left.OnEvent(PD(102,100,new GameObject("unrelated root child")));Check(left.model.curId==-1,"Accepting the center must not broaden capture to unrelated root Graphics.");left.OnHide();

#if UNITY_ANDROID && !UNITY_EDITOR
        Reset();leftUp=leftHide=0;left=Stick(100,new UiStickParam{upAct=v=>leftUp++,hideAct=()=>leftHide++});
        EventSystem.current.Hit=p=>left.view.rtf_area.gameObject;var manager=new InputManager();Input.touches=new[]{T(2,125,100,TouchPhase.Began)};manager.Update();Call(manager,"OnApplicationFocus",false);
        Check(left.model.curId==-1 && leftUp==0 && leftHide==1,"Focus cancellation must propagate through real events to the active stick.");left.OnHide();
#endif
    }
    static void FloatingSticks()
    {
        Reset();int drags=0,ups=0,hides=0;bool allowed=true;Vector3 last=Vector3.one;
        var left=new UiStickCtrl();left.view.rtf_area.position=new Vector3(500,300);left.view.rtf_area.size=new Vector2(1000,600);left.view.rtf_stick.size=new Vector2(100,100);
        var area=left.view.rtf_area.graphic;area.enabled=false;area.color=new Color(0.3f,0.4f,0.5f,0.2f);
        left.param=new UiStickParam{floating=true,dragAct=v=>{drags++;last=v;},upAct=v=>ups++,hideAct=()=>hides++,canStartFunc=()=>allowed,canContinueFunc=()=>allowed};left.OnShow();
        Check(!left.view.rtf_stick.gameObject.activeSelf && area.enabled && area.color.a==0 && area.raycastTarget,"Floating idle must hide visuals but enable the transparent authored input area.");
        Vector3 original=left.view.rtf_area.position;
        left.OnEvent(new InputPointDownEvent{id=11,pos=new Vector3(50,40),ui=left.view.rtf_area.gameObject});
        Check(left.model.curId==11 && left.model.lastPos==Vector3.zero && left.model.downPos==new Vector3(50,40),"An off-center/corner region press must capture with zero direction.");
        Check(left.view.rtf_stick.gameObject.activeSelf && left.view.rtf_stick.position==new Vector3(50,40) && left.view.rtf_area.position==original,"Only the visual moves to the press; the input region stays fixed.");
        left.OnUpdate();Check(drags==1 && last==Vector3.zero,"Press alone must not imply a move or attack direction.");
        left.OnEvent(new InputPointEvent{id=11,pos=new Vector3(80,80)});left.OnUpdate();
        Check(Math.Abs(last.x-0.3f)<0.00001 && Math.Abs(last.y-0.4f)<0.00001,"Direction must be relative to the press and normalized by visual size, not the 1000px input area.");
        Check(left.view.rtf_stick.position==new Vector3(80,80),"The visible knob follows short drags from its press origin.");
        left.OnEvent(new InputPointEvent{id=11,pos=new Vector3(350,440)});left.OnUpdate();
        Check(left.view.rtf_stick.position==new Vector3(80,80),"Long drags clamp visual travel without changing the fixed origin.");
        left.OnEvent(PU(99));left.OnEvent(new InputMouseUpEvent{id=11});Check(left.model.curId==11 && left.view.rtf_stick.gameObject.activeSelf,"Unowned finger/mouse release cannot hide the captured floating stick.");
        left.OnEvent(PU(11));Check(ups==1 && !left.view.rtf_stick.gameObject.activeSelf && left.model.lastPos==Vector3.zero,"Owner release hides the visual immediately and clears direction.");
        left.OnEvent(new InputPointDownEvent{id=12,pos=new Vector3(850,450),ui=left.view.rtf_area.gameObject});
        Check(left.model.downPos==new Vector3(850,450) && left.view.rtf_stick.position==new Vector3(850,450) && left.model.lastPos==Vector3.zero,"The next press establishes a fresh origin, not the previous gesture's center.");
        left.OnEvent(PU(12,true));Check(hides==1 && ups==1 && !left.view.rtf_stick.gameObject.activeSelf,"Canceled release hides without invoking the release action.");
        allowed=false;left.OnEvent(PD(13,500,left.view.rtf_area.gameObject));Check(left.model.curId==-1 && !left.view.rtf_stick.gameObject.activeSelf,"Rejected press must stay hidden.");
        allowed=true;left.OnEvent(PD(14,500,left.view.rtf_area.gameObject));allowed=false;left.OnUpdate();Check(left.model.curId==-1 && !left.view.rtf_stick.gameObject.activeSelf && hides==2,"Blocking a held gesture cancels and hides it.");
        allowed=true;left.OnEvent(PD(15,500,left.view.rtf_area.gameObject));left.OnHide();Check(!left.view.rtf_stick.gameObject.activeSelf && left.model.curId==-1,"Hiding the page clears a held floating gesture.");
        left.OnShow();Check(!left.view.rtf_stick.gameObject.activeSelf && area.enabled && area.color.a==0,"Reopening remains idle-hidden and raycastable.");
        left.OnEvent(PD(16,500,new GameObject("another UI button")));Check(left.model.curId==-1 && !left.view.rtf_stick.gameObject.activeSelf,"Floating mode must not capture a press consumed by another UI button.");
        left.view.rtf_stick.size=new Vector2(200,200);left.OnEvent(PD(17,500,left.view.rtf_area.gameObject));left.OnEvent(PM(17,580));left.OnUpdate();
        Check(Math.Abs(left.model.lastPos.x-0.4f)<0.00001f && (left.view.rtf_stick.position-new Vector3(580,100)).magnitude<0.0001f,"Visual/canvas scaling must scale floating drag normalization and travel together.");
        left.OnEvent(PU(17));
        left.param=new UiStickParam();left.OnShow();Check(!area.enabled && Math.Abs(area.color.a-0.2f)<0.00001 && area.color.r==0.3f && left.view.rtf_stick.gameObject.activeSelf,"Returning to fixed mode restores the authored Graphic state and knob visibility.");
        left.OnHide();

        Reset();left=Stick(100,new UiStickParam{floating=true});var right=Stick(300,new UiStickParam{floating=true});
        left.OnEvent(PD(21,125,left.view.rtf_area.gameObject));right.OnEvent(PD(73,325,right.view.rtf_area.gameObject));
        left.OnEvent(PM(21,325));right.OnEvent(PM(21,325));right.OnEvent(PM(73,125));
        Check(left.model.lastPos.x==2 && right.model.lastPos.x==-2 && left.model.downPos.x==125 && right.model.downPos.x==325,"Crossing fingers preserve both floating origins and independent directions.");
        left.OnEvent(PU(21));right.OnEvent(PU(21));Check(!left.view.rtf_stick.gameObject.activeSelf && right.view.rtf_stick.gameObject.activeSelf && right.model.curId==73,"Releasing movement cannot hide another finger's attack stick.");
        right.OnEvent(PU(73));left.OnEvent(new InputMouseDownEvent{id=0,pos=new Vector3(130,100),ui=left.view.rtf_area.gameObject});left.OnEvent(new InputMouseEvent{id=0,pos=new Vector3(100,140)});left.OnUpdate();
        Check((left.view.rtf_stick.position-new Vector3(100,140)).magnitude<0.0001f && Math.Abs(left.model.lastPos.x+0.3f)<0.00001f && Math.Abs(left.model.lastPos.y-0.4f)<0.00001f,"Floating origin/drag also works with mouse input.");
        left.OnEvent(new InputMouseUpEvent{id=0});Check(!left.view.rtf_stick.gameObject.activeSelf,"Mouse release also hides the floating stick.");
        left.OnHide();right.OnHide();
    }
    static void PlayerGates()
    {
        Reset();GameManager.instance=new GameManager();PlayManager.instance=new PlayManager();CharacterProductForm.DataByUid.Clear();
        var character=new CharacterProductForm.Data{uid=1};character.skill[SkillType.LightAttack]=12;CharacterProductForm.DataByUid[1]=character;GameManager.instance.curProgress.characterUid=1;
        var ctrl=new Ui.PlaySceneMain.PlayerTouchOpt.UiPlayerTouchOptCtrl();var move=Stick(100);var attack=Stick(300);ctrl.view.model_moveStick.Ctrl=move;ctrl.view.model_attackStick.Ctrl=attack;ctrl.OnShow();
        Check(move.param.canStartFunc() && attack.param.canStartFunc(),"Unblocked gameplay must allow both sticks.");
        Check(move.param.floating && attack.param.floating && !move.view.rtf_stick.gameObject.activeSelf && !attack.view.rtf_stick.gameObject.activeSelf,"Play opts both move and LightAttack into idle-hidden floating mode.");
        GameManager.instance.curProgress.blockProgramUid=4;Check(!move.param.canStartFunc() && !attack.param.canStartFunc() && !move.param.canContinueFunc() && !attack.param.canContinueFunc(),"Blocked gameplay must reject starting/continuing both sticks.");
        GameManager.instance.curProgress.blockProgramUid=0;attack.OnEvent(PD(7,340,attack.view.rtf_area.gameObject));attack.OnUpdate();
        Check(PlayManager.instance.infoCtrl.Uses==0 && PlayManager.instance.sceneCtrl.Rotations==0,"An off-center attack press must not cast or rotate before dragging.");
        attack.OnEvent(PM(7,380));attack.OnUpdate();attack.OnUpdate();
        Check(attack.model.curId==7 && PlayManager.instance.sceneCtrl.Opt==SkillType.LightAttack && PlayManager.instance.infoCtrl.Uses==2,"LightAttack continuation must not cancel itself once its option is set.");
        move.OnEvent(PD(8,140,move.view.rtf_area.gameObject));move.OnEvent(PM(8,180));move.OnUpdate();Check(PlayManager.instance.sceneCtrl.Moves==1 && Math.Abs(PlayManager.instance.sceneCtrl.LastMove.x-0.02f)<0.00001,"Movement stick must use the bounded frame delta.");
        GameManager.instance.curProgress.blockProgramUid=5;move.OnUpdate();attack.OnUpdate();Check(move.model.curId==-1 && attack.model.curId==-1 && PlayManager.instance.sceneCtrl.Opt==null && PlayManager.instance.sceneCtrl.Moves==1,"Blocking during drag must stop movement and clear LightAttack.");
        GameManager.instance.curProgress.blockProgramUid=0;PlayManager.instance.sceneCtrl.Opt=SkillType.Other;Check(!attack.param.canStartFunc() && !attack.param.canContinueFunc() && move.param.canStartFunc(),"Another skill must prevent attack capture, without blocking movement.");
        GameManager.instance.curProgress=null;Check(!move.param.canStartFunc() && !attack.param.canStartFunc() && !move.param.canContinueFunc() && !attack.param.canContinueFunc(),"Missing progress must be safe and not accept a gesture.");move.OnHide();attack.OnHide();
    }
    static void PlayerFacing()
    {
        Reset();GameManager.instance=new GameManager();PlayManager.instance=new PlayManager();CharacterProductForm.DataByUid.Clear();
        var ctrl=new Ui.PlaySceneMain.PlayerTouchOpt.UiPlayerTouchOptCtrl();var move=Stick(100);var attack=Stick(300);ctrl.view.model_moveStick.Ctrl=move;ctrl.view.model_attackStick.Ctrl=attack;ctrl.OnShow();
        var scene=PlayManager.instance.sceneCtrl;
        move.OnEvent(PD(21,140,move.view.rtf_area.gameObject));move.OnEvent(PM(21,180));move.OnUpdate();
#if UNITY_ANDROID
        Check(scene.Rotations==1 && scene.LastRotation==new Vector3(1,0,0),"Android movement faces its direction immediately when attack has never been used.");
        move.OnEvent(new InputPointEvent{id=21,pos=new Vector3(140,60)});move.OnUpdate();
        Check(scene.LastRotation==new Vector3(0,0,-1),"Movement facing uses world XZ direction, not screen XY.");
        int rotations=scene.Rotations;
        Time.unscaledTime=2;attack.OnEvent(PD(73,340,attack.view.rtf_area.gameObject));move.OnUpdate();
        Check(scene.Rotations==rotations,"A newly captured attack pointer blocks movement facing before the attack's first Update.");
        attack.OnUpdate();move.OnUpdate();Check(scene.Rotations==rotations,"Holding the neutral attack center preserves facing too.");
        attack.OnEvent(PM(73,380));attack.OnUpdate();rotations=scene.Rotations;
        Check(scene.LastRotation==new Vector3(1,0,0),"Dragging attack still sets its aiming direction.");
        Time.unscaledTime=8;move.OnUpdate();Check(scene.Rotations==rotations,"An attack held longer than one second still owns facing even with reversed Update order.");
        Time.unscaledTime=10;attack.OnEvent(PU(73));move.OnUpdate();Check(scene.Rotations==rotations,"Attack release starts the one-second grace period instead of immediately following movement.");
        Time.unscaledTime=10.99f;move.OnUpdate();Check(scene.Rotations==rotations,"Less than one real second after release must retain attack facing.");
        Time.unscaledTime=11;move.OnUpdate();Check(scene.Rotations==rotations+1 && scene.LastRotation==new Vector3(0,0,-1),"At one second after release movement facing resumes.");
        rotations=scene.Rotations;move.OnEvent(PM(21,140));move.OnUpdate();Check(scene.Rotations==rotations,"Zero movement never invokes a zero-direction rotation.");
        move.OnEvent(new InputPointEvent{id=21,pos=new Vector3(140,140)});
        Time.unscaledTime=12;attack.OnEvent(PD(74,340,attack.view.rtf_area.gameObject));attack.OnUpdate();attack.OnEvent(PU(74));
        Time.unscaledTime=12.99f;move.OnUpdate();Check(scene.Rotations==rotations,"A new neutral attack interaction restarts the grace period.");
        Time.unscaledTime=13;move.OnUpdate();Check(scene.LastRotation==new Vector3(0,0,1) && scene.Rotations==rotations+1,"Facing follows the current movement vector after renewed timeout.");
        rotations=scene.Rotations;Time.unscaledTime=14;attack.OnEvent(PD(75,340,attack.view.rtf_area.gameObject));attack.OnEvent(PU(75,true));
        Time.unscaledTime=14.99f;move.OnUpdate();Check(scene.Rotations==rotations,"Canceled attack interaction also retains the last facing for one second.");
        Time.unscaledTime=15;move.OnUpdate();Check(scene.Rotations==rotations+1,"Canceled attack does not leave movement facing permanently blocked.");
        rotations=scene.Rotations;Time.unscaledTime=100;scene.Opt=SkillType.Q;move.OnUpdate();scene.Opt=SkillType.E;move.OnUpdate();
        Check(scene.Rotations==rotations,"Movement never overrides an active Q/E aim after attack timeout.");
        scene.Opt=null;move.OnUpdate();Check(scene.Rotations==rotations+1,"Clearing skill aim restores ordinary movement facing.");
        rotations=scene.Rotations;GameManager.instance.curProgress.blockProgramUid=8;move.OnUpdate();Check(scene.Rotations==rotations,"Blocked gameplay must not turn the character.");GameManager.instance.curProgress.blockProgramUid=0;
        move.OnHide();attack.OnHide();ctrl.OnShow();move.OnEvent(PD(22,100,move.view.rtf_area.gameObject));move.OnEvent(PM(22,140));move.OnUpdate();
        Check(scene.Rotations==rotations+1,"Reopening the controls clears stale attack timing and permits movement facing.");
#else
        Check(scene.Rotations==0,"Desktop movement must not gain Android auto-facing.");
        Time.unscaledTime=100;move.OnUpdate();Check(scene.Rotations==0,"Android attack timeout must not affect desktop movement later either.");
#endif
        move.OnHide();attack.OnHide();
    }
    static void SkillButtons()
    {
        foreach(var type in new[]{SkillType.E,SkillType.Q})
        {
            Reset();GameManager.instance=new GameManager();PlayManager.instance=new PlayManager();CharacterProductForm.DataByUid.Clear();
            var character=new CharacterProductForm.Data{uid=1};character.skill[SkillType.E]=201;character.skill[SkillType.Q]=301;CharacterProductForm.DataByUid[1]=character;GameManager.instance.curProgress.characterUid=1;
            var main=new SkillButtonFixture();main.InitializeCallbacks();var e=Stick(300);var q=Stick(500);var move=Stick(100);main.Bind(e,q);
            var selected=type==SkillType.E?e:q;var other=type==SkillType.E?q:e;float center=selected.view.rtf_area.position.x;
#if UNITY_STANDALONE_WIN
            Check(!main.E.canStartFunc() && !main.Q.canStartFunc(),"Standalone Windows must preserve its disabled mobile skill-button start contract.");
            selected.param.dragAct(new Vector3(0,1));Check(PlayManager.instance.sceneCtrl.Opt==type && PlayManager.instance.sceneCtrl.LastRotation==new Vector3(0,0,1),"Actual skill callback must bind its own type and map screen Y to world Z.");
            selected.param.upAct(Vector3.zero);Check(PlayManager.instance.infoCtrl.LastSkill==character.skill[type] && PlayManager.instance.sceneCtrl.Opt==null,"Actual skill release callback must cast the bound Q/E skill.");
#else
            move.OnEvent(PD(2,125,move.view.rtf_area.gameObject));
            Z_EventHelper.Invoke(PD(1,center,selected.view.rtf_stick.gameObject));selected.OnUpdate();
            Check(selected.model.curId==1 && selected.model.lastPos==Vector3.zero && PlayManager.instance.sceneCtrl.Opt==type,"Q/E center click must capture zero offset and choose its own aiming type.");
            Check(PlayManager.instance.sceneCtrl.Rotations==0,"Zero-offset skill press must not alter character facing.");
            int shows=selected.ShowCalls;int queries=PlayManager.instance.infoCtrl.CallsByType.TryGetValue(type,out var count)?count:0;main.RefreshButtons();
            Check(selected.active && selected.model.curId==1 && selected.ShowCalls==shows,"Refresh must preserve an owned skill capture without hiding or re-showing/resetting it.");
            Check((PlayManager.instance.infoCtrl.CallsByType.TryGetValue(type,out count)?count:0)==queries,"Owned aiming must not fail a fresh CanUseSkill query merely because its option is already set.");
            other.OnEvent(PD(3,other.view.rtf_area.position.x,other.view.rtf_stick.gameObject));selected.OnEvent(PU(3));selected.OnEvent(new InputMouseUpEvent{id=1});
            Check(other.model.curId==-1 && selected.model.curId==1 && move.model.curId==2,"Skill buttons must not steal each other or release the movement finger.");
            selected.OnEvent(new InputPointEvent{id=1,pos=new Vector3(center,140)});selected.OnUpdate();selected.OnUpdate();main.RefreshButtons();
            Check(selected.model.curId==1 && PlayManager.instance.sceneCtrl.LastRotation==new Vector3(0,0,1) && PlayManager.instance.sceneCtrl.Opt==type,"Held Q/E screen-up aiming must continue in world XZ, not cancel itself or choose Q unconditionally.");
            bool releasedBeforeCast=false;
            PlayManager.instance.infoCtrl.OnUse=()=>{releasedBeforeCast=PlayManager.instance.sceneCtrl.Opt==null;PlayManager.instance.infoCtrl.Available[type]=false;main.RefreshButtons();};
            selected.OnEvent(PU(1));
            Check(PlayManager.instance.infoCtrl.Uses==1 && PlayManager.instance.infoCtrl.LastCharacter==1 && PlayManager.instance.infoCtrl.LastSkill==character.skill[type],"Releasing Q/E must invoke exactly its own equipped skill.");
            Check(releasedBeforeCast && selected.model.curId==-1 && !selected.active && move.model.curId==2,"Release must clear its own option before synchronous skill Refresh, without touching the movement pointer.");
            PlayManager.instance.infoCtrl.Available[type]=true;main.RefreshButtons();int rotations=PlayManager.instance.sceneCtrl.Rotations;
            selected.OnEvent(PD(4,center,selected.view.rtf_stick.gameObject));selected.OnEvent(PU(4));
            Check(PlayManager.instance.infoCtrl.Uses==2 && PlayManager.instance.sceneCtrl.Rotations==rotations,"A plain center click must cast on release without changing direction.");
            PlayManager.instance.infoCtrl.Available[type]=true;PlayManager.instance.infoCtrl.OnUse=null;main.RefreshButtons();
            selected.OnEvent(PD(5,center,selected.view.rtf_stick.gameObject));selected.OnUpdate();selected.OnEvent(PU(5,true));
            Check(PlayManager.instance.infoCtrl.Uses==2 && PlayManager.instance.sceneCtrl.Opt==null && selected.model.curId==-1,"Canceled Q/E aim must clear only its option and never cast.");
            selected.OnEvent(PD(6,center,selected.view.rtf_stick.gameObject));selected.OnUpdate();GameManager.instance.curProgress.enableSkill=false;main.RefreshButtons();
            Check(!selected.active && selected.model.curId==-1 && PlayManager.instance.sceneCtrl.Opt==null && move.model.curId==2,"Disabling skills must still cancel an owned aim during Refresh.");
            GameManager.instance.curProgress.enableSkill=true;main.RefreshButtons();selected.OnEvent(PD(7,center,selected.view.rtf_stick.gameObject));selected.OnUpdate();GameManager.instance.curProgress.blockProgramUid=9;main.RefreshButtons();selected.OnEvent(PU(7));
            Check(!selected.active && selected.model.curId==-1 && PlayManager.instance.sceneCtrl.Opt==null && PlayManager.instance.infoCtrl.Uses==2,"Blocking the program must cancel aim and prevent a later release cast.");
            GameManager.instance.curProgress.blockProgramUid=0;main.RefreshButtons();selected.OnEvent(PD(8,center,selected.view.rtf_stick.gameObject));selected.OnUpdate();PlayManager.instance.sceneCtrl.Opt=SkillType.Other;main.RefreshButtons();selected.OnEvent(PU(8,true));
            Check(!selected.active && selected.model.curId==-1 && PlayManager.instance.sceneCtrl.Opt==SkillType.Other,"Losing skill ownership must cancel capture without clearing another skill's option.");
            PlayManager.instance.sceneCtrl.Opt=null;main.RefreshButtons();selected.OnEvent(PD(9,center,selected.view.rtf_stick.gameObject));selected.OnUpdate();PlayManager.instance.infoCtrl.OnUse=()=>PlayManager.instance.sceneCtrl.Opt=SkillType.Other;selected.OnEvent(PU(9));
            Check(PlayManager.instance.infoCtrl.Uses==3 && PlayManager.instance.sceneCtrl.Opt==SkillType.Other,"Post-cast synchronous work must not be overwritten by stale option cleanup.");
            PlayManager.instance.sceneCtrl.Opt=null;PlayManager.instance.infoCtrl.Available[type]=false;main.RefreshButtons();Check(!selected.active,"An idle unavailable skill must still be hidden after ordinary CanUseSkill evaluation.");
#endif
            e.OnHide();q.OnHide();move.OnHide();
        }
    }
    static void ModPointers()
    {
        Reset();ModManager.instance=new ModManager();var mod=new ModPointerFixture();var ui=new GameObject("UI");
        mod.OnEvent(PD(1,100,ui));Check(!mod.Captured,"A gesture beginning on UI must not capture the scene.");
        mod.OnEvent(new InputPointEvent{id=1,pos=new Vector3(130,100),delta=new Vector3(30,0)});Check(ModManager.instance.Moves==0,"A UI-originating finger moving into the scene must remain ignored.");
        mod.curData=new object();mod.OnEvent(PD(12,100,null));mod.OnEvent(PD(19,100,null));Check(mod.Captured && mod.Id==12 && mod.history.Begins==1,"Only the first non-UI scene finger may own the brush stroke.");
        mod.OnEvent(new InputPointEvent{id=19,pos=new Vector3(140,100),delta=new Vector3(40,0)});mod.OnEvent(new InputMouseUpEvent{id=0});mod.OnEvent(PU(19));Check(mod.Captured && ModManager.instance.Moves==0 && mod.history.Commits==0,"Other finger/mouse movement or release must not alter the captured stroke.");
        mod.OnEvent(new InputPointEvent{id=12,pos=new Vector3(120,100),delta=new Vector3(20,0)});mod.MarkChanged();Check(ModManager.instance.Moves==1 && ModManager.instance.LastDelta.x==20 && !mod.BrushTip,"Owner movement must reach the original scene drag/brush path.");
        mod.OnEvent(new InputPointEvent{id=12,pos=new Vector3(130,100),delta=new Vector3(10,0),ui=ui});Check(ModManager.instance.Moves==1,"Dragging over UI must not edit the map.");
        mod.OnEvent(new InputPointUpEvent{id=12,pos=new Vector3(100,100),ui=ui});Check(!mod.Captured && mod.history.Commits==1 && mod.history.EditedCommits==1 && mod.Clicks==0,"Owner release over UI must commit/reset without selecting the scene.");
        mod.OnEvent(PD(13,100,null));mod.OnEvent(PU(13,true));Check(!mod.Captured && mod.history.Commits==2 && mod.Clicks==0,"Canceled brush touch must commit/reset without click action.");
        mod.OnEvent(PD(14,100,null));mod.FinishTouch();Check(!mod.Captured && mod.history.Commits==3,"Changing touch-owned tools/modes must finish the gesture.");mod.FinishTouch();Check(mod.history.Commits==3,"Finishing a gesture twice must not double-commit.");
        mod.OnEvent(PD(15,100,null));mod.OnEvent(new InputPointUpEvent{id=15,pos=new Vector3(150,100)});Check(mod.Clicks==0 && !mod.Captured,"A drag release must not become a click.");
        mod.OnEvent(PD(16,100,null));mod.OnEvent(PU(16));Check(mod.Clicks==1 && !mod.Captured,"An ordinary short touch must retain scene selection click.");
        mod.OnEvent(new InputMouseDownEvent{id=1,pos=new Vector3(100,100)});Check(!mod.Captured,"Right mouse must not become the left scene pointer.");
        mod.OnEvent(new InputMouseDownEvent{id=0,pos=new Vector3(100,100)});mod.OnEvent(PU(0));Check(mod.Captured && !mod.IsTouch,"Touch ID zero must not release a mouse gesture.");
        mod.OnEvent(new InputMouseEvent{id=0,pos=new Vector3(120,100),delta=new Vector3(20,0)});mod.OnEvent(new InputMouseUpEvent{id=0,pos=new Vector3(130,100)});Check(!mod.Captured && ModManager.instance.Moves==2,"Desktop mouse scene drag must remain available.");
        mod.OnEvent(PD(17,100,null));mod.ResetForLifecycle();Check(!mod.Captured && mod.Id==0,"Lifecycle pointer reset must clear ownership.");
        mod.SetEnabled(false);mod.OnEvent(PD(18,100,null));Check(!mod.Captured,"Disabled scene editing must not accept pointers.");
        mod.SetEnabled(true);mod.ThrowOnClick=true;mod.OnEvent(PD(20,100,null));int commits=mod.history.Commits;try{mod.OnEvent(PU(20));}catch(InvalidOperationException){}
        Check(!mod.Captured && mod.history.Commits==commits+1,"Click exceptions must still reset and commit the brush.");
#if UNITY_ANDROID && !UNITY_EDITOR
        Reset();ModManager.instance=new ModManager();mod=new ModPointerFixture{curData=new object()};mod.Register<InputPointDownEvent>();mod.Register<InputPointEvent>();mod.Register<InputPointUpEvent>();
        var manager=new InputManager();Input.touches=new[]{T(33,100,100,TouchPhase.Began)};manager.Update();mod.MarkChanged();Call(manager,"OnApplicationFocus",false);
        Check(!mod.Captured && mod.history.Commits==1 && mod.history.EditedCommits==1 && mod.Clicks==0,"Focus cancellation must route through the actual event bus and finish a Mod brush without a click.");
#endif
    }
}
