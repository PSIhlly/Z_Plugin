using System;
using System.Collections.Generic;
using UnityEngine;
using Z_Time;
using Z_Ui.Base;
using Z_Ui.Dialog;
using Z_Ui.Form;
using Z_DataSystem.Form;
using RenderHeads.Media.AVProVideo;
using UnityEngine.Events;

namespace UnityEngine
{
    public class GameObject { public bool activeSelf = true; public void SetActive(bool value) { activeSelf = value; } }
    public static class Mathf { public static float Max(float a, float b) => Math.Max(a, b); public static int Max(int a, int b) => Math.Max(a, b); }
}
namespace Z_Ui.Base
{
    public class UiCtrl
    {
        public bool active = true;
        public object uiHolder = new object();
        public virtual void OnCreate() { }
        public virtual void OnShow() { }
        public virtual void OnHide() { }
    }
    public class Btn
    {
        public Action onClickDown, onClickUp;
        public GameObject gameObject = new GameObject();
        public ClickEvent onClick = new ClickEvent();
        public class ClickEvent { private Action actions; public void AddListener(Action action) { actions += action; } public void Invoke() { actions?.Invoke(); } }
    }
    public class Sta { public int state; public void ChangeState(int value) { state = value; } public void ChangeState() { state = 1 - state; } }
    public class Txt { private string value; public int writes; public string text { get => value; set { writes++; this.value = value; } } }
}
namespace UnityEngine.Events { public delegate void UnityAction<T1, T2, T3>(T1 a, T2 b, T3 c); }
namespace RenderHeads.Media.AVProVideo
{
    public enum ErrorCode { None }
    public enum MediaPathType { AbsolutePathOrURL }
    public class MediaPath { public string path; public MediaPath(string value, MediaPathType type) { path = value; } }
    public class MediaPlayerEvent
    {
        public enum EventType { FinishedPlaying, Closing }
        private UnityAction<MediaPlayer, EventType, ErrorCode> listeners;
        public int count => listeners == null ? 0 : listeners.GetInvocationList().Length;
        public void AddListener(UnityAction<MediaPlayer, EventType, ErrorCode> action) { listeners += action; }
        public void RemoveListener(UnityAction<MediaPlayer, EventType, ErrorCode> action) { listeners -= action; }
        public void Invoke(MediaPlayer player, EventType type, ErrorCode error) { listeners?.Invoke(player, type, error); }
    }
    // Models AVPro's distinct disable/pause, close and enable/resume states.
    public class MediaPlayer
    {
        public int stops;
        public bool opened, playing, wasPlayingOnPause;
        public string path;
        public GameObject gameObject = new GameObject();
        public MediaPlayerEvent Events = new MediaPlayerEvent();
        public bool OpenMedia(MediaPath media, bool autoPlay)
        {
            CloseMedia(); path = media.path;
            opened = path != "missing"; playing = opened && autoPlay; return opened;
        }
        public void Pause() { playing = false; wasPlayingOnPause = false; }
        public void CloseMedia()
        {
            if (opened) Events.Invoke(this, MediaPlayerEvent.EventType.Closing, ErrorCode.None);
            opened = playing = false; path = null;
        }
        public void Stop() { stops++; playing = false; }
        public void Disable() { if (playing) { Pause(); wasPlayingOnPause = true; } }
        public void Enable() { if (wasPlayingOnPause && opened) playing = true; wasPlayingOnPause = false; }
        public void Finish() { playing = false; Events.Invoke(this, MediaPlayerEvent.EventType.FinishedPlaying, ErrorCode.None); }
    }
}
namespace Z_Ui.Form { public class ClipForm { public class Data { public string mainText; public int mainAudio, mainVideo, mainPicture; } } }
namespace Z_DataSystem.Form
{
    public class AudioAssetForm
    {
        public class Data { public string path; public void Play(MediaPlayer player) { player.OpenMedia(new MediaPath(path, MediaPathType.AbsolutePathOrURL), true); } }
        public class Table : Dictionary<int, Data> { public Data GetDv(int id, Data fallback) => TryGetValue(id, out var value) ? value : fallback; }
        public static Table DataById = new Table();
    }
    public partial class VideoAssetForm
    {
        public partial class Data { public string path; }
        public class Table : Dictionary<int, Data> { public Data GetDv(int id, Data fallback) => TryGetValue(id, out var value) ? value : fallback; }
        public static Table DataById = new Table();
    }
    public class TexAssetForm
    {
        public class Table { public object GetDk(int id, int fallback) => null; }
        public static Table DataById = new Table();
    }
}
public static class GlobalDefaultHelper { public static int DefaultTexId; }
namespace Ui.DialogBg
{
    public class TestImage { public void BindTexData(object data) { } }
    public class TestView { public MediaPlayer mp_ = new MediaPlayer(); public TestImage img_ = new TestImage(); }
    public partial class UiDialogBgCtrl : UiCtrl
    {
        public UiDialogBgModel model = new UiDialogBgModel();
        public UiDialogBgParam param;
        public TestView view = new TestView();
    }
    public partial class UiDialogBgModel { }
}
namespace Z_Time
{
    public class Timer { public bool cancel, completed; public float delay, interval; public Func<bool> callback; }
    public class TimeManager
    {
        public static TimeManager instance = new TimeManager();
        public readonly List<Timer> timers = new List<Timer>();
        private readonly List<Action> nextUpdate = new List<Action>();
        public Timer StartTimer(float delay, float interval, Func<bool> callback, object bind)
        {
            if (float.IsNaN(delay) || float.IsInfinity(delay)) throw new Exception("Invalid timer delay");
            var timer = new Timer { delay = delay, interval = interval, callback = callback }; timers.Add(timer); return timer;
        }
        public void CancelTimer(Timer timer) { if (timer != null) timer.cancel = true; }
        public void Fire(Timer timer) { if (timer != null && !timer.cancel && !timer.completed) timer.completed = timer.callback(); }
        public void AddNextUpdateWithoutCheckAction(Action action) { nextUpdate.Add(action); }
        public void NextUpdate() { var actions = nextUpdate.ToArray(); nextUpdate.Clear(); foreach (var action in actions) action(); }
    }
}
namespace Z_Ui.Dialog
{
    __DIALOG_SETTINGS__
    public enum ShowType { Hide }
    public class ShowTypeEvent : Z_Event { public ShowType showType; }
    public class DialogManager
    {
        public static DialogManager instance = new DialogManager();
        public Settings settings = new Settings { textDisplaySpeed = 20 };
        public void ShowHistory() { }
    }
}
namespace Ui.DialogMain
{
    public class TestParent { public TestParentView view = new TestParentView(); }
    public class TestParentView { public MainText.UiMainTextCtrl page_MainText; }
}
namespace Ui.DialogMain.MainText
{
    public class TestView { public GameObject go_bg = new GameObject(); public MediaPlayer mp_ = new MediaPlayer(); public Txt txt_ = new Txt(); }
    public partial class UiMainTextCtrl : UiCtrl
    {
        public UiMainTextModel model = new UiMainTextModel();
        public UiMainTextParam param;
        public TestView view = new TestView();
    }
}
namespace Ui.DialogMain.Options
{
    public class TestView
    {
        public Btn btn_over = new Btn(), btn_skipVideo = new Btn(), btn_autoPlay = new Btn(), btn_skip = new Btn(), btn_hide = new Btn(), btn_history = new Btn(), btn_menu = new Btn();
        public Sta sta_autoPlay = new Sta(), sta_skip = new Sta();
        public GameObject go_func = new GameObject();
    }
    public partial class UiOptionsCtrl : UiCtrl
    {
        public UiOptionsModel model = new UiOptionsModel();
        public UiOptionsParam param;
        public TestView view = new TestView();
        public TestParent parent;
    }
}
public class DialogSkipRegression : IZ_Listener<ClipPlayEvent>
{
    private int advances, videoEnds;
    private static int checks;
    public void OnEvent(ClipPlayEvent evt) { if (evt.playType == PlayType.ClipOver) advances++; if (evt.playType == PlayType.ClipVideoOver) videoEnds++; }
    private static void Check(bool result, string text) { checks++; if (!result) throw new Exception(text); }
    private static void Show(Ui.DialogMain.MainText.UiMainTextCtrl main, Ui.DialogMain.Options.UiOptionsCtrl options, string text, int video = 0)
    {
        var clip = new ClipForm.Data { mainText = text, mainVideo = video };
        main.param = new Ui.DialogMain.MainText.UiMainTextParam { clip = clip };
        options.param = new Ui.DialogMain.Options.UiOptionsParam { clip = clip };
        main.active = options.active = true;
        main.OnShow(); options.OnShow();
    }
    public static int Run()
    {
        var main = new Ui.DialogMain.MainText.UiMainTextCtrl();
        var options = new Ui.DialogMain.Options.UiOptionsCtrl { parent = new Ui.DialogMain.TestParent() };
        options.parent.view.page_MainText = main;
        var observer = new DialogSkipRegression(); Z_EventHelper.Register(observer);
        main.OnCreate(); options.OnCreate();
        var clock = TimeManager.instance; var settings = DialogManager.instance.settings;
        string sentence = "<color=red>" + new string('字', 6000) + "</color>";
        Show(main, options, sentence);
        var oldWord = main.model.wordTimer;
        Check(main.view.txt_.text == "" && main.model.isDisplaying, "normal show still starts the typewriter");
        clock.Fire(oldWord);
        Check(main.model.nowWord == 2 && Math.Abs(main.model.wordTimer.interval - 0.1f) < 0.000001f, "normal text speed is two characters per 0.1 s (0.05 s per character)");
        int writes = main.view.txt_.writes;
        options.view.btn_skip.onClickDown();
        Check(settings.skipping && options.view.sta_skip.state == 1, "press enters explicit skip state");
        Check(main.view.txt_.text == sentence && main.view.txt_.writes == writes + 1, "whole rich-text sentence assigned immediately once");
        Check(main.model.nowWord == sentence.Length && !main.model.isDisplaying && oldWord.cancel, "skip ends and cancels the typewriter");
        var pending = main.model.overTimer;
        Check(Math.Abs(pending.delay - 0.02f) < 0.000001f && observer.advances == 0, "retain short delayed next-sentence transition");
        clock.Fire(oldWord); Check(main.view.txt_.text == sentence, "old typewriter cannot append after completion");
        options.view.btn_skip.onClickUp();
        Check(!settings.skipping && settings.autoPlaySpeed == 0 && options.view.sta_skip.state == 0, "release returns to manual mode");
        Check(pending.cancel, "release cancels pending skip transition");
        clock.Fire(pending); Check(observer.advances == 0, "release does not advance an extra sentence");

        options.view.btn_skip.onClickDown(); clock.Fire(main.model.overTimer);
        Check(observer.advances == 1, "holding still advances after the short delay");
        Show(main, options, "下一句");
        Check(main.view.txt_.text == "下一句" && !main.model.isDisplaying, "next sentence also completes while held");
        Check(options.view.sta_autoPlay.state == 0 && settings.skipping, "held skip does not turn ordinary auto play on");
        pending = main.model.overTimer;
        Show(main, options, "第三句");
        Check(pending.cancel && main.view.txt_.text == "第三句", "refresh cancels previous sentence's pending transition");
        options.view.btn_skip.onClickUp();
        Check(settings.autoPlaySpeed == 0, "release remains manual across multiple sentences");

        Show(main, options, "普通句子");
        oldWord = main.model.wordTimer;
        Check(main.view.txt_.text == "" && Math.Abs(oldWord.interval - 0.1f) < 0.000001f, "next sentence returns to ordinary typewriter timing (0.05 s per character)");
        for (int i = 0; i <= "普通句子".Length; i++) clock.Fire(oldWord);
        Check(main.view.txt_.text == "普通句子" && !main.model.isDisplaying && oldWord.completed && !oldWord.cancel, "ordinary completed timer matches production completion semantics");
        options.view.btn_autoPlay.onClick.Invoke();
        Check(main.model.wordTimer == oldWord && Math.Abs(main.model.overTimer.delay - 5) < 0.000001f, "ordinary auto toggle schedules a finished sentence without restarting typing");
        pending = main.model.overTimer; options.view.btn_autoPlay.onClick.Invoke();
        Check(pending.cancel && settings.autoPlaySpeed == 0, "turning auto off cancels its pending transition without division by zero");

        options.view.btn_skip.onClickDown(); pending = main.model.overTimer;
        Show(main, options, null, 1);
        Check(pending.cancel && main.view.txt_.text == "" && !main.view.go_bg.activeSelf, "video-only clip clears previous text and timer");
        Check(!options.view.go_func.activeSelf && options.view.btn_skipVideo.gameObject.activeSelf, "video-only controls remain unchanged");
        options.view.btn_skip.onClickUp();

        Show(main, options, "隐藏前"); options.view.btn_skip.onClickDown();
        pending = main.model.overTimer;
        main.active = options.active = false; main.OnHide(); options.OnHide(); clock.NextUpdate();
        Check(pending.cancel && !settings.skipping && settings.autoPlaySpeed == 0, "real hide clears held skip and timers");
        int timers = clock.timers.Count;
        Z_EventHelper.Invoke(new ClipPlayEvent { playType = PlayType.ClipSettingChange });
        Check(clock.timers.Count == timers, "hidden text page ignores global setting events");
        Show(main, options, "重新打开");
        Check(main.view.txt_.text == "" && main.model.isDisplaying, "reopen starts normally without held skip residue");

        options.view.btn_skip.onClickDown();
        options.active = false; options.OnHide();
        Show(main, options, "同帧连续对话"); clock.NextUpdate();
        Check(settings.skipping && main.view.txt_.text == "同帧连续对话", "deferred hide cleanup does not cancel a continuous shown dialog");
        options.view.btn_skip.onClickUp();
        Check(!settings.skipping && main.model.overTimer.cancel, "continuous-dialog release still stops skipping");

        AudioAssetForm.DataById[1] = new AudioAssetForm.Data { path = "voice-a" };
        main.param = new Ui.DialogMain.MainText.UiMainTextParam { clip = new ClipForm.Data { mainAudio = 1 } };
        main.OnShow();
        Check(main.view.mp_.playing && main.view.mp_.path == "voice-a", "new voice still starts normally");
        Show(main, options, "无配音");
        Check(!main.view.mp_.opened && !main.view.mp_.playing, "silent clip closes previous voice on active-panel reuse");
        main.param.clip.mainAudio = 1; main.OnShow();
        main.view.mp_.Disable(); main.OnHide(); main.view.mp_.Enable();
        Check(!main.view.mp_.opened && !main.view.mp_.playing && !main.view.mp_.wasPlayingOnPause, "hide closes voice and clears the disabled player's resume flag");
        main.param.clip.mainAudio = 99; main.OnShow();
        Check(!main.view.mp_.opened, "missing audio record leaves the player closed on reopen");

        var bg = new Ui.DialogBg.UiDialogBgCtrl();
        VideoAssetForm.DataById[1] = new VideoAssetForm.Data { path = "video-a" };
        VideoAssetForm.DataById[2] = new VideoAssetForm.Data { path = "video-b" };
        VideoAssetForm.DataById[3] = new VideoAssetForm.Data { path = "missing" };
        int externalEvents = 0;
        bg.view.mp_.Events.AddListener((mp, type, error) => externalEvents++);
        bg.param = new Ui.DialogBg.UiDialogBgParam { clip = new ClipForm.Data { mainVideo = 1 } };
        bg.OnShow();
        Check(bg.view.mp_.playing && bg.view.mp_.Events.count == 2, "video starts with its own and unrelated listeners");
        bg.param.clip.mainVideo = 2; bg.OnShow();
        Check(bg.view.mp_.playing && bg.view.mp_.path == "video-b" && bg.view.mp_.Events.count == 2, "replacing an unfinished video releases only its completion listener");
        int videoEnds = observer.videoEnds;
        bg.view.mp_.Finish();
        Check(observer.videoEnds == videoEnds + 1 && bg.view.mp_.Events.count == 1 && externalEvents > 0, "only the current video's completion advances dialog; other listeners survive");
        bg.param.clip.mainVideo = 1; bg.OnShow();
        bg.view.mp_.Disable(); bg.OnHide(); bg.view.mp_.Enable();
        Check(!bg.view.mp_.opened && !bg.view.mp_.playing && !bg.view.mp_.wasPlayingOnPause && bg.view.mp_.Events.count == 1, "hide closes video, clears resume and releases its listener");
        bg.param.clip.mainVideo = 0; bg.OnShow();
        Check(!bg.view.mp_.opened && !bg.view.mp_.gameObject.activeSelf, "clip without video keeps the reused player closed and hidden");
        bg.param.clip.mainVideo = 3; bg.OnShow();
        Check(!bg.view.mp_.opened, "failed native video open leaves no media playing");
        bg.param.clip.mainVideo = 2; bg.OnShow();
        videoEnds = observer.videoEnds; bg.view.mp_.Finish();
        Check(observer.videoEnds == videoEnds + 1 && bg.view.mp_.Events.count == 1, "failed-open callback cannot fire for the next successful video");
        bg.OnHide(); main.OnHide();
        return checks;
    }
}
