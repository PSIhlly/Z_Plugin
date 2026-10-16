param(
    [string]$ProjectRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
)

$ErrorActionPreference = 'Stop'
$checkCount = 0
function Assert-Export($condition, [string]$message) {
    if (-not $condition) { throw $message }
    $script:checkCount++
}

# Compile the actual picker source under each relevant set of platform symbols.
# Gallery stubs capture callbacks and exact bytes; no interactive picker is opened.
$source = Get-Content -LiteralPath (Join-Path $ProjectRoot 'Assets/Z_Level2/Z_DataSystem/Core/AssetManager.cs') -Raw
$start = $source.IndexOf('    public static class AssetFilePicker')
$end = $source.IndexOf('    public abstract class SelectTask', $start)
Assert-Export ($start -ge 0 -and $end -gt $start) 'Picker source boundaries not found.'
$pickerSource = $source.Substring($start, $end - $start)
$uiSource = Get-Content -LiteralPath (Join-Path $ProjectRoot 'Assets/GameSample/MyScripts/Main/Ui/UiModCtrl.cs') -Raw
$uiStart = $uiSource.IndexOf('        public override void OnHide()')
$uiEnd = $uiSource.IndexOf('        public void Refresh()', $uiStart)
Assert-Export ($uiStart -ge 0 -and $uiEnd -gt $uiStart) 'Import UI callback source boundaries not found.'
$uiMethods = $uiSource.Substring($uiStart, $uiEnd - $uiStart)
$exportUiSource = Get-Content -LiteralPath (Join-Path $ProjectRoot 'Assets/GameSample/MyScripts/Main/Ui/UiModStoryCtrl.cs') -Raw
$exportUiStart = $exportUiSource.IndexOf('        public override void OnHide()')
$exportUiEnd = $exportUiSource.IndexOf('        public override void OnShow()', $exportUiStart)
Assert-Export ($exportUiStart -ge 0 -and $exportUiEnd -gt $exportUiStart) 'Export UI async/lifetime source boundaries not found.'
$exportUiMethods = $exportUiSource.Substring($exportUiStart, $exportUiEnd - $exportUiStart)
$loadingSource = Get-Content -LiteralPath (Join-Path $ProjectRoot 'Assets/Z_Level3/Z_UI/Core/Module/Loading/LoadingManager.cs') -Raw
$loadingStart = $loadingSource.IndexOf('    public enum LoadingState')
$loadingEnd = $loadingSource.LastIndexOf('}')
Assert-Export ($loadingStart -ge 0 -and $loadingEnd -gt $loadingStart) 'Loading manager source boundaries not found.'
$loadingTypes = $loadingSource.Substring($loadingStart, $loadingEnd - $loadingStart)
$galleryStub = @'
public static class Application { public static string productName = "Z_Plugin"; }
public static class NativeGallery {
    public enum Permission { Denied = 0, Granted = 1, ShouldAsk = 2 }
    public static bool Busy;
    public static Permission PickPermission = Permission.Granted, SavePermission = Permission.Granted;
    public static int PickCalls, SaveCalls;
    public static string PickTitle, PickMime, SaveAlbum, SaveName;
    public static byte[] SavedBytes;
    public static Action<string> PickCallback;
    public static Action<bool, string> SaveCallback;
    public static void Reset() {
        Busy = false; PickPermission = SavePermission = Permission.Granted;
        PickCalls = SaveCalls = 0;
        PickTitle = PickMime = SaveAlbum = SaveName = null;
        SavedBytes = null; PickCallback = null; SaveCallback = null;
    }
    public static bool IsMediaPickerBusy() => Busy;
    public static Permission GetImageFromGallery(Action<string> callback, string title = "", string mime = "image/*") {
        PickCalls++; PickTitle = title; PickMime = mime; PickCallback = callback;
        return PickPermission;
    }
    public static Permission SaveImageToGallery(byte[] content, string album, string fileName, Action<bool,string> callback = null) {
        SaveCalls++; SavedBytes = content; SaveAlbum = album; SaveName = fileName; SaveCallback = callback;
        return SavePermission;
    }
    public static void GetImagesFromGallery(Action<string[]> callback) {}
    public static void GetAudioFromGallery(Action<string> callback) {}
    public static void GetAudiosFromGallery(Action<string[]> callback) {}
    public static void GetVideosFromGallery(Action<string[]> callback) {}
}
'@
$behaviorFixture = @'
// Execute the production controller's actual import callback/lifetime methods
// against tiny service stubs; generated UI, Forms and user saves are not loaded.
public class UiFixtureBase { public virtual void OnHide() {} }
public class UiImportModel { public bool importing; public int importVersion; }
public class UiImportFixture : UiFixtureBase {
    private CancellationTokenSource importCancellation;
    public UiImportModel model = new UiImportModel();
    public bool active = true;
    public int RefreshCount;
    public void Refresh() { RefreshCount++; }
    public void Click() { ImportStoryPackage(); }
__UI_IMPORT_METHODS__
}
public class UiExportModel { public bool exporting, startingTest; public int exportVersion; }
public class UiExportFixture : UiFixtureBase {
    private CancellationTokenSource exportCancellation;
    public UiExportModel model = new UiExportModel();
    public bool active = true;
    public void Click() { ExportStoryPackage(); }
__UI_EXPORT_METHODS__
}
public class TextManager {
    public static TextManager instance = new TextManager();
    public string GetTxt(string key) => key;
}
public class NotifyManager {
    public static NotifyManager instance = new NotifyManager();
    public int Tips, Errors;
    public void AddTip(string content) { Tips++; }
    public void AddPopup(string title, string content, bool closable, bool enableScr = false) { Errors++; }
}
public class GameManager {
    public static GameManager instance = new GameManager();
    public SaveFixture saveCtrl = new SaveFixture();
    public StoryFixture curStory = new StoryFixture();
}
public class SaveFixture {
    public int Imports, ImportStarts, PackageStarts;
    public byte[] Bytes;
    public byte[] PackageBytes = { 137, 80, 78, 71, 0, 255, 13, 10, 123, 2, 1 };
    public TaskCompletionSource<bool> ImportGate, PackageGate;
    public bool ImportFails, PackageFails;
    public async Task<StoryFixture> ImportPackageAsync(string path, CancellationToken cancellationToken) {
        ImportStarts++;
        cancellationToken.ThrowIfCancellationRequested();
        if (ImportGate != null) {
            using (cancellationToken.Register(() => ImportGate.TrySetCanceled())) { await ImportGate.Task; }
        }
        cancellationToken.ThrowIfCancellationRequested();
        if (ImportFails) throw new IOException("Invalid package fixture.");
        Bytes = File.ReadAllBytes(path); Imports++; return new StoryFixture();
    }
    public async Task<byte[]> PackageAsync(int storyId, CancellationToken cancellationToken) {
        PackageStarts++;
        cancellationToken.ThrowIfCancellationRequested();
        if (PackageGate != null) {
            using (cancellationToken.Register(() => PackageGate.TrySetCanceled())) { await PackageGate.Task; }
        }
        cancellationToken.ThrowIfCancellationRequested();
        if (PackageFails) throw new IOException("Could not build the package fixture.");
        return PackageBytes;
    }
}
public class StoryFixture { public string name = "test"; public int id = 12; }
public static class Debug { public static int Errors; public static void LogException(Exception exception) { Errors++; } }
public class Z_Event {}
public class Z_Manager<T> where T : new() { public static T instance = new T(); public virtual void Init() {} }
public class UiLoadingCtrl {}
public class UiManager {
    public static UiManager instance = new UiManager();
    public int Shows, Closes;
    public bool Visible;
    public void ShowUi<T>() { Shows++; Visible = true; }
    public void CloseUi<T>() { Closes++; Visible = false; }
}
public class TimeManager {
    public static TimeManager instance = new TimeManager();
    private readonly Queue<Action> endFrame = new Queue<Action>(), nextUpdate = new Queue<Action>();
    public void AddCurFrameEndWithoutCheckAction(Action action) { endFrame.Enqueue(action); }
    public void AddNextUpdateWithoutCheckAction(Action action) { nextUpdate.Enqueue(action); }
    public void EndFrame() { var count = endFrame.Count; while (count-- > 0) endFrame.Dequeue()(); }
    public void NextUpdate() { var count = nextUpdate.Count; while (count-- > 0) nextUpdate.Dequeue()(); }
}
__LOADING_TYPES__
public static class GalleryBehaviorRegression {
    private static int checks;
    private static void Check(bool condition, string message) {
        if (!condition) throw new InvalidOperationException(message);
        checks++;
    }
    public static int Run() {
        checks = 0;
        int picks = 0, errors = 0;
        string selected = "not completed";
        NativeGallery.Reset();
        AssetFilePicker.GetModPackage(path => { picks++; selected = path; }, error => errors++, "Import package");
        Check(NativeGallery.PickCalls == 1 && NativeGallery.PickMime == "image/png", "Mod import must share PNG image selection.");
        Check(NativeGallery.PickTitle == "Import package" && picks == 0, "Title/asynchronous pick callback was not preserved.");
        string original = "/gallery/raw % 中文 package.PNG";
        NativeGallery.PickCallback(original);
        Check(picks == 1 && errors == 0 && selected == original, "Selected PNG path was changed or rejected.");

        NativeGallery.Reset(); picks = errors = 0;
        AssetFilePicker.GetModPackage(path => { picks++; selected = path; }, error => errors++, "Import");
        NativeGallery.PickCallback(null);
        Check(picks == 1 && selected == null && errors == 0, "Cancel must produce one null pick callback without an error.");
        NativeGallery.Reset(); picks = errors = 0;
        AssetFilePicker.GetModPackage(path => { picks++; selected = path; }, error => errors++, "Import");
        NativeGallery.PickCallback("");
        Check(picks == 1 && selected == "" && errors == 0, "Empty gallery selection must remain a harmless cancel.");
        foreach (string cachePath in new[] { "/gallery/pickedMedia.tmp", "/gallery/story.jpg", "/gallery/story.png.txt", "/gallery/no-extension" }) {
            NativeGallery.Reset(); picks = errors = 0;
            AssetFilePicker.GetModPackage(path => { picks++; selected = path; }, error => errors++, "Import");
            NativeGallery.PickCallback(cachePath);
            Check(picks == 1 && errors == 0 && selected == cachePath, "Gallery cache paths must be passed unchanged; bytes, not cache extensions, identify PNG packages.");
        }

        NativeGallery.Reset(); selected = null;
        AssetFilePicker.GetImage(path => selected = path);
        Check(NativeGallery.PickMime == "image/*" && NativeGallery.PickTitle == "Select image file", "Ordinary image import defaults changed.");
        NativeGallery.PickCallback("/gallery/ordinary.jpg");
        Check(selected == "/gallery/ordinary.jpg", "Package restriction must not block ordinary JPG image import.");
        foreach (NativeGallery.Permission permission in new[] { NativeGallery.Permission.Denied, NativeGallery.Permission.ShouldAsk }) {
            NativeGallery.Reset(); picks = errors = 0; NativeGallery.PickPermission = permission;
            AssetFilePicker.GetModPackage(path => picks++, error => errors++, "Import");
            Check(picks == 0 && errors == 1 && NativeGallery.PickCalls == 1, "Missing gallery permission must finish with an error.");
        }
        NativeGallery.Reset(); picks = errors = 0; NativeGallery.Busy = true;
        AssetFilePicker.GetModPackage(path => picks++, error => errors++, "Import");
        Check(picks == 0 && errors == 1 && NativeGallery.PickCalls == 0, "Busy picker must reject without launching another pick.");
        NativeGallery.Reset(); picks = 0; NativeGallery.PickPermission = NativeGallery.Permission.Denied;
        AssetFilePicker.GetImage(path => { picks++; selected = path; });
        Check(picks == 1 && selected == null, "Permission denial without error handler must return null.");
        NativeGallery.Reset(); picks = 0; NativeGallery.Busy = true;
        AssetFilePicker.GetImage(path => { picks++; selected = path; });
        Check(picks == 1 && selected == null && NativeGallery.PickCalls == 0, "Busy fallback without error handler must return null.");

        byte[] png = { 137, 80, 78, 71, 0, 255, 13, 10, 123, 2, 1 };
        int built = 0, completed = 0; bool saved = false;
        NativeGallery.Reset(); errors = 0;
        AssetFilePicker.ExportModPackage(() => { built++; return png; }, "CON", "Output", ok => { completed++; saved = ok; }, error => errors++);
        Check(built == 1 && NativeGallery.SaveCalls == 1 && Object.ReferenceEquals(png, NativeGallery.SavedBytes), "Gallery export must receive the exact original byte[] without re-encoding.");
        Check(NativeGallery.SaveAlbum == Application.productName && NativeGallery.SaveName == "_CON.png", "Gallery album/safe filename is incorrect.");
        Check(completed == 0 && errors == 0, "Export must wait for the gallery save result.");
        NativeGallery.SaveCallback(true, "/gallery/saved.png");
        Check(completed == 1 && saved && errors == 0, "Successful gallery export callback is incorrect.");
        NativeGallery.Reset(); completed = errors = 0;
        AssetFilePicker.ExportModPackage(() => png, "story", "Output", ok => completed++, error => errors++);
        NativeGallery.SaveCallback(false, null);
        Check(completed == 0 && errors == 1, "Failed gallery export must report an error, not success.");
        NativeGallery.Reset(); completed = errors = 0; NativeGallery.SavePermission = NativeGallery.Permission.Denied;
        AssetFilePicker.ExportModPackage(() => png, "story", "Output", ok => completed++, error => errors++);
        Check(completed == 0 && errors == 1, "Denied gallery save permission must report an error.");
        NativeGallery.Reset(); completed = 0; saved = true;
        AssetFilePicker.ExportModPackage(() => png, "story", "Output", ok => { completed++; saved = ok; }, null);
        NativeGallery.SaveCallback(false, null);
        Check(completed == 1 && !saved, "Failed gallery save without error handler must finish false.");
        NativeGallery.Reset(); completed = 0; saved = true; NativeGallery.SavePermission = NativeGallery.Permission.Denied;
        AssetFilePicker.ExportModPackage(() => png, "story", "Output", ok => { completed++; saved = ok; }, null);
        Check(completed == 1 && !saved, "Denied gallery save without error handler must finish false.");

        return checks;
    }
}
'@
$behaviorFixture = $behaviorFixture.Replace('__UI_IMPORT_METHODS__', $uiMethods)
$behaviorFixture = $behaviorFixture.Replace('__UI_EXPORT_METHODS__', $exportUiMethods)
$behaviorFixture = $behaviorFixture.Replace('__LOADING_TYPES__', $loadingTypes)
$asyncFixture = @'
// Drive production Task APIs and UI lifetime methods without opening a native
// dialog, touching a real story, or relying on wall-clock frame timing.
public static class AsyncPackageRegression {
    private static int checks;
    private static void Check(bool condition, string message) {
        if (!condition) throw new InvalidOperationException(message);
        checks++;
    }
    private static void Reset() {
        NativeGallery.Reset(); GameManager.instance = new GameManager();
        NotifyManager.instance = new NotifyManager(); UiManager.instance = new UiManager();
        TimeManager.instance = new TimeManager(); LoadingManager.instance = new LoadingManager();
        LoadingManager.instance.Init(); Debug.Errors = 0;
    }
    private static int Items() {
        var field = typeof(LoadingManager).GetField("loadingItems", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        return ((HashSet<string>)field.GetValue(LoadingManager.instance)).Count;
    }
    private static void Draw() { TimeManager.instance.EndFrame(); TimeManager.instance.NextUpdate(); }
    private static void Closed(string context) {
        Check(Items() == 0 && !UiManager.instance.Visible && UiManager.instance.Closes == 1,
            context + ": loading must close exactly once with no residual item.");
    }
    private static void Fault<T>(Task task, string message) where T : Exception {
        bool expected = false;
        try { task.GetAwaiter().GetResult(); } catch (T) { expected = true; }
        Check(expected, message);
    }
    public static int Run() {
        checks = 0;
        LoadingLifetime(); PickerTasks();
        string file = Path.Combine(Path.GetTempPath(), "mod-async-regression-" + Guid.NewGuid().ToString("N") + ".tmp");
        try {
            File.WriteAllBytes(file, new byte[] { 137, 80, 78, 71, 0, 255, 13, 10, 123, 2, 1 });
            ImportLifetime(file); ExportLifetime();
        } finally { if (File.Exists(file)) File.Delete(file); }
        return checks;
    }
    private static void LoadingLifetime() {
        Reset(); int started = 0;
        var gate = new TaskCompletionSource<bool>();
        Task task = LoadingManager.instance.RunAsync("same", () => { started++; return gate.Task; });
        Check(Items() == 1 && UiManager.instance.Visible && started == 0, "Loading must precede potentially blocking work.");
        TimeManager.instance.EndFrame();
        Check(started == 0 && !task.IsCompleted, "EndFrame alone must not start work before the rendered frame finishes.");
        TimeManager.instance.NextUpdate();
        Check(started == 1 && !task.IsCompleted && Items() == 1, "The next Update must start work while loading remains visible.");
        gate.SetResult(true); task.GetAwaiter().GetResult(); Closed("Loading success");

        Reset();
        task = LoadingManager.instance.RunAsync("failure", () => Task.FromException(new IOException("fixture")));
        Draw(); Fault<IOException>(task, "Loading failures must propagate to their owner."); Closed("Loading failure");
        using (var cancellation = new CancellationTokenSource()) {
            Reset(); started = 0;
            task = LoadingManager.instance.RunAsync("cancel", () => { started++; return Task.CompletedTask; }, cancellation.Token);
            cancellation.Cancel(); Fault<OperationCanceledException>(task, "Cancel before rendering must cancel the operation.");
            Draw(); Check(started == 0, "Canceled render callbacks must not start deferred work."); Closed("Loading cancellation");
        }
        using (var cancellation = new CancellationTokenSource()) {
            Reset(); var oldGate = new TaskCompletionSource<bool>(); var newGate = new TaskCompletionSource<bool>();
            Task old = LoadingManager.instance.RunAsync("same", () => oldGate.Task, cancellation.Token); Draw();
            cancellation.Cancel();
            Task current = LoadingManager.instance.RunAsync("same", () => newGate.Task, CancellationToken.None); Draw();
            Check(Items() == 2, "Concurrent same-name requests must own distinct loading items.");
            oldGate.SetResult(true); old.GetAwaiter().GetResult();
            Check(Items() == 1 && UiManager.instance.Visible && UiManager.instance.Closes == 0,
                "An old request's finally must not close a newer request's loading.");
            newGate.SetResult(true); current.GetAwaiter().GetResult(); Closed("Overlapping loading requests");
        }
    }
    private static void PickerTasks() {
        Reset(); Task<string> picked = AssetFilePicker.GetModPackageAsync("Import async");
        Check(!picked.IsCompleted && NativeGallery.PickCalls == 1 && NativeGallery.PickMime == "image/png", "Async import must await the PNG picker.");
        Action<string> callback = NativeGallery.PickCallback; callback("/gallery/pickedMedia.tmp");
        Check(picked.GetAwaiter().GetResult() == "/gallery/pickedMedia.tmp", "Async selection must preserve gallery cache paths.");
        callback("/gallery/stale.png");
        Check(picked.GetAwaiter().GetResult() == "/gallery/pickedMedia.tmp", "Duplicate picker callbacks must not change the completed result.");
        Reset(); picked = AssetFilePicker.GetModPackageAsync("Cancel"); NativeGallery.PickCallback(null);
        Check(picked.GetAwaiter().GetResult() == null, "Picker cancel must remain a harmless null result.");
        Reset(); NativeGallery.PickPermission = NativeGallery.Permission.Denied;
        Fault<IOException>(AssetFilePicker.GetModPackageAsync("Denied"), "Async denied picker permission must fault without waiting for a callback.");
        Reset(); NativeGallery.Busy = true;
        Fault<IOException>(AssetFilePicker.GetModPackageAsync("Busy"), "Async busy picker must fault without launching another request.");
        Check(NativeGallery.PickCalls == 0, "Busy async picker must not launch.");
        using (var cancellation = new CancellationTokenSource()) {
            Reset(); picked = AssetFilePicker.GetModPackageAsync("Canceled", cancellation.Token); callback = NativeGallery.PickCallback;
            cancellation.Cancel(); Fault<OperationCanceledException>(picked, "Active picker cancellation must terminate its Task.");
            callback("/gallery/stale.png"); Check(picked.IsCanceled, "Late gallery callbacks must not revive a canceled Task.");
            Reset(); Fault<OperationCanceledException>(AssetFilePicker.GetModPackageAsync("Canceled", cancellation.Token), "Pre-canceled import must cancel immediately.");
            Check(NativeGallery.PickCalls == 0, "Pre-canceled import must not open the picker.");
        }
        Reset(); byte[] bytes = GameManager.instance.saveCtrl.PackageBytes;
        var bytesGate = new TaskCompletionSource<byte[]>();
        Task<bool> saved = AssetFilePicker.ExportModPackageAsync(() => bytesGate.Task, "CON", "Output async");
        Check(!saved.IsCompleted && NativeGallery.SaveCalls == 0, "Async export must await package generation.");
        bytesGate.SetResult(bytes);
        Check(NativeGallery.SaveCalls == 1 && Object.ReferenceEquals(bytes, NativeGallery.SavedBytes), "Async export must pass original lossless bytes to the gallery.");
        Check(NativeGallery.SaveName == "_CON.png" && NativeGallery.SaveAlbum == Application.productName && !saved.IsCompleted,
            "Async export must keep safe PNG naming and await the save callback.");
        Action<bool,string> saveCallback = NativeGallery.SaveCallback; saveCallback(true, "/gallery/output.png");
        Check(saved.GetAwaiter().GetResult(), "Successful async export must return true."); saveCallback(false, null);
        Check(saved.GetAwaiter().GetResult(), "Duplicate save callbacks must not change a completed export.");
        Reset(); NativeGallery.SavePermission = NativeGallery.Permission.Denied;
        Fault<IOException>(AssetFilePicker.ExportModPackageAsync(() => Task.FromResult(bytes), "story", "Denied"), "Denied save permission must fault without waiting for a callback.");
        Reset(); saved = AssetFilePicker.ExportModPackageAsync(() => Task.FromResult(bytes), "story", "Failure"); NativeGallery.SaveCallback(false, null);
        Fault<IOException>(saved, "Gallery save failure must fault the async operation.");
        using (var cancellation = new CancellationTokenSource()) {
            Reset(); saved = AssetFilePicker.ExportModPackageAsync(() => Task.FromResult(bytes), "story", "Canceled", cancellation.Token);
            saveCallback = NativeGallery.SaveCallback; cancellation.Cancel();
            Fault<OperationCanceledException>(saved, "Cancellation must terminate an outstanding gallery save Task.");
            saveCallback(true, "/gallery/stale.png"); Check(saved.IsCanceled, "Late successful save must not revive canceled export.");
            int factories = 0; Reset();
            Fault<OperationCanceledException>(AssetFilePicker.ExportModPackageAsync(() => { factories++; return Task.FromResult(bytes); }, "story", "Canceled", cancellation.Token),
                "Pre-canceled export must cancel immediately.");
            Check(factories == 0 && NativeGallery.SaveCalls == 0, "Pre-canceled export must neither build bytes nor call native gallery.");
        }
    }
    private static void ImportLifetime(string file) {
        Reset(); var ui = new UiImportFixture(); ui.Click(); ui.Click();
        Check(ui.model.importing && NativeGallery.PickCalls == 0 && Items() == 1, "Repeated import clicks must retain one rendered-loading request.");
        TimeManager.instance.EndFrame(); Check(NativeGallery.PickCalls == 0, "Import picker must not launch during EndFrame.");
        TimeManager.instance.NextUpdate(); Check(NativeGallery.PickCalls == 1, "Import picker must launch after the loading frame.");
        Action<string> first = NativeGallery.PickCallback; first(file);
        Check(GameManager.instance.saveCtrl.Imports == 1 && ui.RefreshCount == 1 && !ui.model.importing && NotifyManager.instance.Tips == 1,
            "Successful async import must refresh once, notify once, and release busy state.");
        Check(Convert.ToBase64String(File.ReadAllBytes(file)) == Convert.ToBase64String(GameManager.instance.saveCtrl.Bytes), "Async import must use exact original file bytes.");
        first(file); Check(GameManager.instance.saveCtrl.Imports == 1 && ui.RefreshCount == 1, "Duplicate callback must not import a story twice."); Closed("UI import success");

        Reset(); ui = new UiImportFixture(); ui.Click(); Draw(); NativeGallery.PickCallback(null);
        Check(!ui.model.importing && GameManager.instance.saveCtrl.ImportStarts == 0 && NotifyManager.instance.Errors == 0, "Canceled selection must release import silently."); Closed("UI import picker cancel");
        Reset(); ui = new UiImportFixture(); ui.Click(); ui.active = false; ui.OnHide(); Draw();
        Check(NativeGallery.PickCalls == 0 && !ui.model.importing, "Hide before drawing must cancel the deferred picker."); Closed("UI import pre-render hide");

        Reset(); ui = new UiImportFixture(); ui.Click(); Draw(); Action<string> stale = NativeGallery.PickCallback;
        ui.active = false; ui.OnHide(); ui.active = true; ui.Click(); Draw(); Action<string> current = NativeGallery.PickCallback;
        stale(file);
        Check(GameManager.instance.saveCtrl.ImportStarts == 0 && ui.model.importing && Items() == 1 && UiManager.instance.Visible,
            "Stale callback after reopen must not affect the new import or loading.");
        current(file);
        Check(GameManager.instance.saveCtrl.Imports == 1 && !ui.model.importing && NotifyManager.instance.Errors == 0, "Reopened import must still succeed.");
        Check(Items() == 0 && UiManager.instance.Closes == 2, "Each hidden/reopened import must release only its own loading item.");

        Reset(); ui = new UiImportFixture(); var save = GameManager.instance.saveCtrl; save.ImportGate = new TaskCompletionSource<bool>();
        ui.Click(); Draw(); NativeGallery.PickCallback(file); ui.Click();
        Check(save.ImportStarts == 1 && save.Imports == 0 && ui.model.importing && Items() == 1 && NativeGallery.PickCalls == 1,
            "Import busy/loading must span asynchronous file preparation, not end at picker callback.");
        ui.active = false; ui.OnHide();
        Check(save.Imports == 0 && !ui.model.importing && NotifyManager.instance.Errors == 0, "Hide during preparation must not publish a canceled story."); Closed("UI import prepare cancel");

        foreach (int failure in new[] { 0, 1, 2, 3 }) {
            Reset(); ui = new UiImportFixture(); save = GameManager.instance.saveCtrl;
            if (failure == 0) NativeGallery.PickPermission = NativeGallery.Permission.Denied;
            if (failure == 1) NativeGallery.Busy = true;
            if (failure == 2) save.ImportFails = true;
            ui.Click(); Draw();
            if (failure >= 2) NativeGallery.PickCallback(failure == 3 ? file + ".missing" : file);
            Check(!ui.model.importing && NotifyManager.instance.Errors == 1 && save.Imports == 0, "Failed import must release UI state and report one error."); Closed("UI import failure");
            NativeGallery.PickPermission = NativeGallery.Permission.Granted; NativeGallery.Busy = false; save.ImportFails = false;
            ui.Click(); Draw(); NativeGallery.PickCallback(file);
            Check(save.Imports == 1 && !ui.model.importing && NotifyManager.instance.Errors == 1 && Items() == 0 && UiManager.instance.Closes == 2,
                "Import must remain usable after permission, busy, parsing, or file-read failure.");
        }
    }
    private static void ExportLifetime() {
        Reset(); var ui = new UiExportFixture(); ui.Click(); ui.Click();
        Check(ui.model.exporting && GameManager.instance.saveCtrl.PackageStarts == 0 && Items() == 1, "Repeated export clicks must keep one loading request before rendering.");
        TimeManager.instance.EndFrame(); Check(GameManager.instance.saveCtrl.PackageStarts == 0, "Export generation must wait beyond EndFrame.");
        TimeManager.instance.NextUpdate();
        Check(GameManager.instance.saveCtrl.PackageStarts == 1 && NativeGallery.SaveCalls == 1 && ui.model.exporting && Items() == 1,
            "Export busy/loading must span package generation through gallery completion.");
        Action<bool,string> first = NativeGallery.SaveCallback; first(true, "/gallery/export.png");
        Check(!ui.model.exporting && NotifyManager.instance.Tips == 1 && NotifyManager.instance.Errors == 0, "Successful async export must notify and release busy state.");
        first(true, "/gallery/export.png"); Check(NotifyManager.instance.Tips == 1, "Duplicate export callback must not repeat its success message."); Closed("UI export success");
        Reset(); ui = new UiExportFixture(); ui.model.startingTest = true; ui.Click();
        Check(!ui.model.exporting && Items() == 0 && UiManager.instance.Shows == 0, "Test transition guard must not start export.");
        ui.model.startingTest = false; GameManager.instance.curStory = null; ui.Click();
        Check(!ui.model.exporting && Items() == 0, "Missing current story must not start export.");

        Reset(); ui = new UiExportFixture(); ui.Click(); ui.active = false; ui.OnHide(); Draw();
        Check(GameManager.instance.saveCtrl.PackageStarts == 0 && !ui.model.exporting && NotifyManager.instance.Errors == 0,
            "Hide before drawing must cancel deferred export silently."); Closed("UI export pre-render hide");
        Reset(); ui = new UiExportFixture(); ui.Click(); Draw(); Action<bool,string> stale = NativeGallery.SaveCallback;
        ui.active = false; ui.OnHide(); ui.active = true; ui.Click(); Draw(); Action<bool,string> current = NativeGallery.SaveCallback;
        stale(true, "/gallery/stale.png");
        Check(ui.model.exporting && NotifyManager.instance.Tips == 0 && Items() == 1 && UiManager.instance.Visible,
            "Stale export after reopen must not release or notify the new request.");
        stale(false, null); Check(NotifyManager.instance.Errors == 0, "Stale gallery failure must not show an error on reopened UI.");
        current(true, "/gallery/current.png");
        Check(!ui.model.exporting && NotifyManager.instance.Tips == 1 && Items() == 0 && UiManager.instance.Closes == 2,
            "Reopened export must succeed and preserve its own loading pairing.");

        Reset(); ui = new UiExportFixture(); var save = GameManager.instance.saveCtrl; save.PackageGate = new TaskCompletionSource<bool>();
        ui.Click(); Draw(); ui.Click();
        Check(save.PackageStarts == 1 && NativeGallery.SaveCalls == 0 && ui.model.exporting && Items() == 1, "Export must remain busy while byte generation is asynchronous.");
        ui.active = false; ui.OnHide();
        Check(NativeGallery.SaveCalls == 0 && !ui.model.exporting && NotifyManager.instance.Errors == 0, "Hide during generation must not call native gallery with canceled bytes."); Closed("UI export generation cancel");
        foreach (int failure in new[] { 0, 1, 2 }) {
            Reset(); ui = new UiExportFixture(); save = GameManager.instance.saveCtrl;
            if (failure == 0) NativeGallery.SavePermission = NativeGallery.Permission.Denied;
            if (failure == 1) save.PackageFails = true;
            ui.Click(); Draw(); if (failure == 2) NativeGallery.SaveCallback(false, null);
            Check(!ui.model.exporting && NotifyManager.instance.Errors == 1 && NotifyManager.instance.Tips == 0, "Export failure must report once and release its busy state."); Closed("UI export failure");
            NativeGallery.SavePermission = NativeGallery.Permission.Granted; save.PackageFails = false;
            ui.Click(); Draw(); NativeGallery.SaveCallback(true, "/gallery/retry.png");
            Check(!ui.model.exporting && NotifyManager.instance.Tips == 1 && NotifyManager.instance.Errors == 1 && Items() == 0 && UiManager.instance.Closes == 2,
                "Export must remain usable after permission, generation, or gallery-save failure.");
        }
    }
}
'@
$cases = @(
    @{ Name = $null; Expected = 'Story.png' },
    @{ Name = ''; Expected = 'Story.png' },
    @{ Name = '  ...  '; Expected = 'Story.png' },
    @{ Name = '东方 剧情'; Expected = '东方 剧情.png' },
    @{ Name = '  测试.  '; Expected = '测试.png' },
    @{ Name = 'a<>:"/\|?*b'; Expected = 'a_________b.png' },
    @{ Name = "a`0`n`tb"; Expected = 'a___b.png' },
    @{ Name = '../scene'; Expected = '.._scene.png' },
    @{ Name = 'CON'; Expected = '_CON.png' },
    @{ Name = 'con.event'; Expected = '_con.event.png' },
    @{ Name = 'PRN'; Expected = '_PRN.png' },
    @{ Name = 'AUX'; Expected = '_AUX.png' },
    @{ Name = 'NUL'; Expected = '_NUL.png' },
    @{ Name = 'COM1'; Expected = '_COM1.png' },
    @{ Name = 'LPT9'; Expected = '_LPT9.png' },
    @{ Name = 'COM¹'; Expected = '_COM¹.png' },
    @{ Name = 'COM10'; Expected = 'COM10.png' },
    @{ Name = 'LPT0'; Expected = 'LPT0.png' }
)
$platforms = @(
    @{ Name = 'EditorWindows'; Symbols = 'UNITY_EDITOR,UNITY_EDITOR_WIN,UNITY_ANDROID'; Native = $true },
    @{ Name = 'PlayerWindows'; Symbols = 'UNITY_STANDALONE_WIN'; Native = $true },
    @{ Name = 'PlayerAndroid'; Symbols = 'UNITY_ANDROID'; Native = $false }
)
foreach ($platform in $platforms) {
    $namespace = 'ModPackageFileRegression' + $platform.Name
    $code = "using System; using System.Collections.Generic; using System.IO; using System.Threading; using System.Threading.Tasks; namespace $namespace {`n$pickerSource`n$galleryStub`n$behaviorFixture`n$asyncFixture`n}"
    $types = Add-Type -TypeDefinition $code -CompilerOptions "/define:$($platform.Symbols)" -PassThru
    $picker = $types | Where-Object Name -eq 'AssetFilePicker'
    Assert-Export ($null -ne $picker) "$($platform.Name) source compilation failed."
    $nameMethod = $picker.GetMethod('GetModPackageFileName')
    foreach ($case in $cases) {
        $actual = $nameMethod.Invoke($null, [object[]]@($case.Name))
        Assert-Export ($actual -ceq $case.Expected) "$($platform.Name): '$($case.Name)' -> '$actual', expected '$($case.Expected)'."
        Assert-Export ($actual.IndexOfAny([IO.Path]::GetInvalidFileNameChars()) -lt 0) "Invalid output filename: $actual"
    }
    $longName = $nameMethod.Invoke($null, [object[]]@([string]::new([char]'x', 500)))
    Assert-Export ($longName.Length -eq 204) 'Long suggested name was not bounded.'
    $unicodeName = [string]::new([char]'x', 199) + [char]0xD83D + [char]0xDE00
    $unicodeOutput = $nameMethod.Invoke($null, [object[]]@($unicodeName))
    Assert-Export ($unicodeOutput -eq ([string]::new([char]'x', 199) + '.png')) 'Truncation split a surrogate pair.'

    $flags = [Reflection.BindingFlags]'NonPublic,Static'
    $nativeMethod = $picker.GetMethod('GetSaveFileName', $flags)
    Assert-Export (($null -ne $nativeMethod) -eq $platform.Native) "$($platform.Name): incorrect native API platform guard."
    if ($platform.Native) {
        $import = [Attribute]::GetCustomAttribute($nativeMethod, [Runtime.InteropServices.DllImportAttribute])
        Assert-Export ($import.Value -eq 'comdlg32.dll' -and $import.EntryPoint -eq 'GetSaveFileNameW') 'Save dialog is not the native Unicode API.'
        $structure = $picker.GetNestedType('OpenFileName', [Reflection.BindingFlags]::NonPublic)
        $size = [Runtime.InteropServices.Marshal]::SizeOf([type]$structure)
        Assert-Export ($size -eq $(if ([IntPtr]::Size -eq 8) { 152 } else { 88 })) "Incorrect OPENFILENAME layout size: $size"
        Assert-Export ($picker.GetField('OfnOverwritePrompt', $flags).GetRawConstantValue() -eq 2) 'Native overwrite confirmation flag is missing.'
    }
    else {
        $unsupported = $false
        try { $null = $picker.GetMethod('GetModPackageSavePath').Invoke($null, [object[]]@('story', 'Output')) }
        catch { $unsupported = $_.Exception.GetBaseException() -is [PlatformNotSupportedException] }
        Assert-Export $unsupported 'Android must not open a Windows save dialog.'
        $fixture = $types | Where-Object Name -eq 'GalleryBehaviorRegression'
        $behaviorChecks = $fixture.GetMethod('Run').Invoke($null, [object[]]@())
        $checkCount += [int]$behaviorChecks
        $asyncType = $types | Where-Object Name -eq 'AsyncPackageRegression'
        $asyncChecks = $asyncType.GetMethod('Run').Invoke($null, [object[]]@())
        $checkCount += [int]$asyncChecks
    }
}
if ($IsWindows) {
    $library = [Runtime.InteropServices.NativeLibrary]::Load('comdlg32.dll')
    try {
        Assert-Export ([Runtime.InteropServices.NativeLibrary]::GetExport($library, 'GetSaveFileNameW') -ne [IntPtr]::Zero) 'Native save dialog entry point is unavailable.'
        Assert-Export ([Runtime.InteropServices.NativeLibrary]::GetExport($library, 'CommDlgExtendedError') -ne [IntPtr]::Zero) 'Native cancel/error entry point is unavailable.'
    }
    finally { [Runtime.InteropServices.NativeLibrary]::Free($library) }
}
# Packages share the ordinary image picker, with PNG-only selection. The Android
# file-management Activity/JNI bridge must not remain in Player source/manifest.
Assert-Export (-not $pickerSource.Contains('ModPackageFileBridge')) 'The removed file-management bridge is still referenced.'
Assert-Export ($pickerSource.Contains('}, title, "image/png", onError);')) 'Mod import must reuse the shared PNG image picker.'
Assert-Export ($pickerSource.Contains('mime == "image/png" ? "PNG image files\0*.png\0\0"')) 'Windows shared image picker must restrict package selection to PNG.'
Assert-Export ($uiMethods.Contains('await GameManager.instance.saveCtrl.ImportPackageAsync(path, cancellation.Token)')) 'Import UI must await path-based package preparation with its lifetime token.'
Assert-Export ($uiMethods.Contains('await LoadingManager.instance.RunAsync("modImport"') -and $exportUiMethods.Contains('await LoadingManager.instance.RunAsync("modExport"')) 'Import/export UI must both own asynchronous loading operations.'
$saveSource = Get-Content -LiteralPath (Join-Path $ProjectRoot 'Assets/GameSample/MyScripts/Main/GameSaveController.cs') -Raw
Assert-Export ($saveSource.Contains('137, 80, 78, 71, 13, 10, 26, 10') -and $saveSource.Contains('!signature.SequenceEqual(content.Take(8))')) 'Package import must identify PNG by its byte signature, not a gallery cache extension.'
Assert-Export ($pickerSource.Contains('Mod package (*.png)\0*.png\0\0')) 'Windows package save filter must use .png.'
Assert-Export ($pickerSource.Contains('defExt = "png"')) 'Windows save dialog must default to .png.'
Assert-Export ([regex]::Matches($pickerSource, [regex]::Escape('Path.GetExtension(path), ".png"')).Count -eq 1 -and $pickerSource.Contains('Path.GetExtension(path).Equals(".png", StringComparison.OrdinalIgnoreCase)')) 'Windows import/export extension validation must both use .png.'
foreach ($removedPath in @('Assets/Z_Level2/Z_DataSystem/Core/ModPackageFileBridge.cs',
    'Assets/Z_Level2/Z_DataSystem/Core/ModPackageFileBridge.cs.meta',
    'Assets/Plugins/Android/ModPackagePicker.java', 'Assets/Plugins/Android/ModPackagePicker.java.meta')) {
    Assert-Export (-not (Test-Path -LiteralPath (Join-Path $ProjectRoot $removedPath))) "Removed file-management code remains: $removedPath"
}
[xml]$manifest = Get-Content -LiteralPath (Join-Path $ProjectRoot 'Assets/Plugins/Android/AndroidManifest.xml') -Raw
$activity = @($manifest.manifest.application.activity) | Where-Object { $_.GetAttribute('name', 'http://schemas.android.com/apk/res/android') -eq 'com.z.modpackage.ModPackagePicker' }
Assert-Export ($null -eq $activity) 'Removed Android document Activity is still declared.'
Write-Output "MOD_PACKAGE_FILE_REGRESSION_PASS checks=$checkCount"
