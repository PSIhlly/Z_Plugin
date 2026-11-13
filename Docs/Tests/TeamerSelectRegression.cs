// Production command and UI controller sources are inserted by the runner.
// These UI/service spies avoid opening the game or modifying stories/saves.
namespace UnityEngine
{
    public class Sprite {}
    public enum RuntimeInitializeLoadType { AfterAssembliesLoaded }
    public class RuntimeInitializeOnLoadMethodAttribute : Attribute { public RuntimeInitializeOnLoadMethodAttribute(RuntimeInitializeLoadType type) {} }
}
namespace Z_DesignStyle {}
namespace Z_DataSystem.Form
{
    public static class TexAssetForm
    {
        public class Data {public UnityEngine.Sprite GetSprite()=>null;}
        public static Dictionary<int,Data> DataById=new Dictionary<int,Data>{{0,new Data()}};
    }
}
namespace Form
{
    public static class CharacterProductForm
    {
        public class Data { public int uid,avatarTex; public string name; }
        public static Dictionary<int,Data> DataByUid=new Dictionary<int,Data>();
    }
}
namespace Z_Ui.Base
{
    public class UiParam {}
    public class UiHolder {}
    public class UiCtrl
    {
        public bool active;
        public virtual void OnCreate() {}
        public virtual void OnShow() {}
        public virtual void Close() {active=false;}
    }
    public class UiScrViewContainer<T>
    {
        public UiScrViewContainer(object owner,object template,object scroll) {}
        public void Clear() {}
        public void Add(object value) {}
        public void Refresh() {}
    }
}
public class ButtonSpy
{
    public class ClickEvent {private Action actions;public void AddListener(Action action){actions+=action;}public void Invoke(){actions?.Invoke();}}
    public ClickEvent onClick=new ClickEvent();
}
public class StateSpy {public void ChangeState(int state){}}
public class TextSpy {public string text;}
public class ImageSpy {public void BindTexData(TexAssetForm.Data data){}}
public static class DictionarySpy
{
    public static V GetDv<K,V>(this Dictionary<K,V> data,K key,V fallback)=>data.TryGetValue(key,out var value)?value:fallback;
    public static V GetDv<V>(this List<V> data,int index,V fallback)=>index>=0 && index<data.Count?data[index]:fallback;
}
public class ProgressSpy {public List<int> team=new List<int>{1,2,3};public List<int> teamActive=new List<int>{1};}
public class GameManager {public static GameManager instance=new GameManager();public ProgressSpy curProgress=new ProgressSpy();public EventControllerSpy evtCtrl=new EventControllerSpy();}
public class PlayManager {public static PlayManager instance=new PlayManager();public PlayInfoSpy infoCtrl=new PlayInfoSpy();}
public class PlayInfoSpy
{
    public int replacements,lastTeamer;
    public void ReplaceActiveTeamer(int oldUid,int newUid,int index){replacements++;lastTeamer=newUid;}
    public void ReplaceTeamer(int oldUid,int newUid){replacements++;lastTeamer=newUid;}
}
public static class GlobalEventHelper {public const string CHARACTER="$ch$";public static string GetName(string marker,string uid)=>marker+uid+marker;}
namespace Z_Code.Form
{
    public static class BoxDataForm {public class Data {public string str;}}
}
namespace Z_Code
{
    public static class CodeHelper {public static BoxDataForm.Data CreateBoxByStr(string value)=>new BoxDataForm.Data{str=value};}
    public class InterpretAsyncTask
    {
        public BoxDataForm.Data[] res;public bool complete,cancelled;public int completeCount;
        public void Complete(){complete=true;completeCount++;}
        public void Cancel(){cancelled=true;}
        public bool IsCancelled()=>cancelled;
    }
    public abstract class CmdBase
    {
        protected static void Register(CmdBase command){}
        public abstract string GetName();public abstract CmdBase GetNew();
        protected abstract bool ExecuteInternal(BoxDataForm.Data[] args,InterpretAsyncTask task);
        public void Execute(InterpretAsyncTask task){if(ExecuteInternal(new BoxDataForm.Data[0],task))task.Complete();}
    }
}
public class EventControllerSpy
{
    private sealed class Task {public InterpretAsyncTask async;public Func<bool> update;public Action cancel;}
    private List<Task> tasks=new List<Task>();
    public void StartTask(InterpretAsyncTask async,Func<bool> update,Action onCancel=null)
    {if(!update())tasks.Add(new Task{async=async,update=update,cancel=onCancel});}
    public void Poll(){for(int i=tasks.Count-1;i>=0;i--)if(tasks[i].update())tasks.RemoveAt(i);}
    public void CancelFirst(){var task=tasks[0];tasks.RemoveAt(0);task.async.Cancel();task.cancel?.Invoke();}
    public int Count=>tasks.Count;
}
namespace Ui.TeamSelectWindow
{
    public partial class UiTeamSelectWindowParam:UiParam {}
    public class WindowViewSpy
    {
        public ButtonSpy btn_bg=new ButtonSpy(),btn_close=new ButtonSpy();
        public object go_activeTeamer=new object(),go_teamer=new object(),scr_activeTeamers=new object(),scr_teamers=new object();
    }
    public partial class UiTeamSelectWindowCtrl:UiCtrl {public UiTeamSelectWindowModel model=new UiTeamSelectWindowModel();public WindowViewSpy view=new WindowViewSpy();}
    public class RowViewSpy {public ButtonSpy btn_=new ButtonSpy();public StateSpy sta_exist=new StateSpy(),sta_=new StateSpy();public TextSpy txt_=new TextSpy();public ImageSpy img_=new ImageSpy();}
    public partial class UiActiveTeamerCtrl:UiCtrl {public UiTeamSelectWindowCtrl parent;public UiActiveTeamerParam param;public UiActiveTeamerModel model=new UiActiveTeamerModel();public RowViewSpy view=new RowViewSpy();}
    public partial class UiTeamerCtrl:UiCtrl {public UiTeamSelectWindowCtrl parent;public UiTeamerParam param;public UiTeamerModel model=new UiTeamerModel();public RowViewSpy view=new RowViewSpy();}
}
namespace Z_Ui
{
    public class UiManager
    {
        public static UiManager instance=new UiManager();public bool fail;public UiTeamSelectWindowCtrl window;public UiNotifyCtrl notify;
        public UiHolder ShowUi<T>()
        {
            if(fail)return null;
            if(window==null){window=new UiTeamSelectWindowCtrl();window.OnCreate();}
            window.active=true;window.OnShow();return new UiHolder();
        }
        public T GetUi<T>() where T:class=>(object)(typeof(T)==typeof(UiNotifyCtrl)?(object)notify:window) as T;
    }
}
namespace Ui.Notify
{
    public class NotifyModelSpy {public List<ChooseInfo> chooseInfos=new List<ChooseInfo>();}
    public class UiNotifyCtrl {public bool active=true;public NotifyModelSpy model=new NotifyModelSpy();public void RemoveChoose(int id){model.chooseInfos.RemoveAll(info=>info.id==id);}}
}
namespace Z_Text {public class TextManager {public static TextManager instance=new TextManager();public string GetTxt(string key)=>key;}}
namespace Z_Ui.Notify
{
    public class EntryItem {public string content;public UnityEngine.Sprite sprite;public int id,deepth;public EntryItem parent;public Dictionary<string,EntryItem> subs=new Dictionary<string,EntryItem>();}
    public class ChooseInfo {public int id;public bool canClose;public string title;public EntryItem items;public Func<EntryItem,bool> func;}
    public class NotifyManager
    {
        public static NotifyManager instance=new NotifyManager();private int next;
        public ChooseInfo AddChoose(string title,bool canClose,Func<EntryItem,bool> func,EntryItem items)
        {
            if(UiManager.instance.notify==null)UiManager.instance.notify=new UiNotifyCtrl();
            var info=new ChooseInfo{id=++next,title=title,canClose=canClose,func=func,items=items};
            UiManager.instance.notify.active=true;UiManager.instance.notify.model.chooseInfos.Add(info);return info;
        }
    }
}
public static class TeamerSelectRegression
{
    private static int checks;
    private static UiTeamSelectWindowCtrl Window=>UiManager.instance.window;
    private static void Check(bool condition,string message){if(!condition)throw new InvalidOperationException(message);checks++;}
    private static void Reset()
    {
        GameManager.instance=new GameManager();PlayManager.instance=new PlayManager();UiManager.instance=new UiManager();CharacterProductForm.DataByUid.Clear();
        for(int i=1;i<=3;i++)CharacterProductForm.DataByUid[i]=new CharacterProductForm.Data{uid=i,name="Teamer"+i};
    }
    private static InterpretAsyncTask Start()
    {
        var task=new InterpretAsyncTask();new ShowTeamerSelectCmd().Execute(task);return task;
    }
    private static void Teamer(int uid)
    {
        var row=new UiTeamerCtrl{parent=Window,param=new UiTeamerParam{uid=uid}};row.OnCreate();row.OnShow();row.view.btn_.onClick.Invoke();
    }
    private static void Active(int uid,int index=0)
    {
        var row=new UiActiveTeamerCtrl{parent=Window,param=new UiActiveTeamerParam{uid=uid,index=index}};row.OnCreate();row.OnShow();row.view.btn_.onClick.Invoke();
    }
    private static void Close(bool background=false)
    {
        (background?Window.view.btn_bg:Window.view.btn_close).onClick.Invoke();GameManager.instance.evtCtrl.Poll();
    }
    private static InterpretAsyncTask Select()
    {
        var task=new InterpretAsyncTask();new SelectOneTeamerCmd().Execute(task);return task;
    }
    private static ChooseInfo Choice=>UiManager.instance.notify.model.chooseInfos.Last();
    private static void Pick(int uid,ChooseInfo info=null)
    {
        info=info??Choice;
        if(info.func(info.items.subs[uid.ToString(CultureInfo.InvariantCulture)]))UiManager.instance.notify.RemoveChoose(info.id);
        GameManager.instance.evtCtrl.Poll();
    }
    private static void Result(InterpretAsyncTask task,string value,string message)
    {
        Check(task.complete && !task.cancelled && task.res!=null && task.res.Length==1 && task.res[0].str==value,message);
    }
    public static int Run()
    {
        Reset();var task=Start();
        Check(Window.active && !task.complete && task.res==null,"ShowTeamerSelect only opens and waits for party management");
        Teamer(2);Active(1);GameManager.instance.evtCtrl.Poll();
        Check(!task.complete && PlayManager.instance.infoCtrl.lastTeamer==2,"party management retains team-to-active replacement");
        Close();Check(task.complete && task.res==null,"closing ShowTeamerSelect does not return a selection");
        Reset();task=Start();Active(1);Teamer(3);Close(true);
        Check(task.complete && task.res==null && PlayManager.instance.infoCtrl.lastTeamer==3,"inverse replacement and background close retain old behavior");
        Reset();GameManager.instance.curProgress=null;task=Start();Check(task.complete && task.res==null,"missing Progress safely skips management");

        Reset();CharacterProductForm.DataByUid[4]=new CharacterProductForm.Data{uid=4,name="outsider"};
        GameManager.instance.curProgress.team=new List<int>{3,1,2,2,999,0,-1};
        task=Select();
        Check(!task.complete && UiManager.instance.window==null && GameManager.instance.evtCtrl.Count==1,"selection uses generic Choose without opening management");
        Check(Choice.items.subs.Values.Select(item=>item.id).SequenceEqual(new[]{1,2,3}),"all valid team members, including deployed, appear once in UID order");
        Check(!Choice.canClose && Choice.items.subs.Values.All(item=>item.deepth==1 && item.parent==Choice.items),"generic selector requires a selectable leaf");
        Check(!Choice.func(null) && !task.complete,"invalid/no selection does not complete");
        Pick(2);Result(task,"$ch$2$ch$","selecting reserve returns exact Character handle");
        Check(PlayManager.instance.infoCtrl.replacements==0 && GameManager.instance.curProgress.team.SequenceEqual(new[]{3,1,2,2,999,0,-1}),"selection never mutates party membership or deployment");
        Check(task.completeCount==1 && UiManager.instance.notify.model.chooseInfos.Count==0,"selection completes once and closes its dialog");
        Reset();task=Select();Pick(1);Result(task,"$ch$1$ch$","deployed members are selectable too");
        Reset();CharacterProductForm.DataByUid[1].name=CharacterProductForm.DataByUid[2].name="same";
        task=Select();Check(Choice.items.subs.Count==3,"duplicate display names do not overwrite rows");Pick(2);Result(task,"$ch$2$ch$","duplicate name selection retains exact identity");
        Reset();task=Select();CharacterProductForm.DataByUid.Remove(2);Pick(2);Result(task,"","deleted member returns empty");
        Reset();task=Select();CharacterProductForm.DataByUid[2]=new CharacterProductForm.Data{uid=2,name="replacement"};Pick(2);Result(task,"","UID reuse cannot retarget a selection");
        Reset();task=Select();GameManager.instance.curProgress.team.Remove(2);Pick(2);Result(task,"","member removed from team returns empty");
        Reset();GameManager.instance.curProgress.team.Clear();task=Select();Result(task,"","empty team returns immediately");Check(UiManager.instance.notify==null,"empty team does not show an unusable dialog");
        Reset();GameManager.instance.curProgress=null;task=Select();Result(task,"","missing Progress returns empty");
        Reset();GameManager.instance.curProgress.team=null;task=Select();Result(task,"","null team returns empty");
        Reset();task=Select();var old=Choice;GameManager.instance.evtCtrl.CancelFirst();
        Check(task.cancelled && UiManager.instance.notify.model.chooseInfos.Count==0,"cancellation removes only its owned choice");old.func(old.items.subs["2"]);Check(!task.complete,"late choice cannot complete canceled event");
        Reset();var first=Select();old=Choice;var second=Select();var current=Choice;GameManager.instance.evtCtrl.CancelFirst();
        Check(first.cancelled && UiManager.instance.notify.model.chooseInfos.SequenceEqual(new[]{current}),"canceling old choice leaves newer choice intact");Pick(3);Result(second,"$ch$3$ch$","newer choice remains usable");
        Reset();task=Select();UiManager.instance.notify.model.chooseInfos.Clear();GameManager.instance.evtCtrl.Poll();Result(task,"","externally dismissed choice finishes empty");
        Reset();task=Select();GameManager.instance.curProgress=new ProgressSpy();GameManager.instance.evtCtrl.Poll();Result(task,"","changed Progress invalidates old selection");
        Check(UiManager.instance.notify.model.chooseInfos.Count==0,"progress change releases old dialog");
        Check(new SelectOneTeamerCmd().GetName()=="SelectOneTeamer" && new SelectOneTeamerCmd().GetNew() is SelectOneTeamerCmd,"new command has independent canonical invocation");
        return checks;
    }
}
