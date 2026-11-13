using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Form;
using Ui.PlayEffect;
using UnityEditor;
using UnityEngine;
using Z_Code;
using Z_Code.Form;
using Z_DesignStyle;
using Z_Ui;
using Z_Ui.Base;

// Isolated UI/command fixture: no production scenes, assets or saves are loaded.
public static class FadeCommandsRegression
{
    private static int checks;

    public static void Run()
    {
        GameObject overlay = null, managerRoot = null;
        Sprite staleSprite = null;
        try
        {
            typeof(GameCmdDataForm).GetMethod("Register", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null);
            typeof(FadeInCmd).GetMethod("Init", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null);
            typeof(FadeOutCmd).GetMethod("Init", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null);
            foreach (string name in new[] { "FadeIn", "FadeOut" })
            {
                Check(CmdBase.TryGetForm(name, out var form) && form.prmTypes.SequenceEqual(new[] { "num" })
                    && form.prmNames.Count == 1
                    && form.retTypes.Count == 1 && form.retTypes[0] == "void"
                    && form.defaultCode == name + "(1);" && ((GameCmdDataForm.Data)form).labId == 10013,
                    name + " has one numeric duration and a complete default in Effect");
            }

            managerRoot = new GameObject("fade-ui-manager");
            managerRoot.SetActive(false);
            var manager = managerRoot.AddComponent<UiManager>();
            typeof(Z_MonoSingleton<UiManager>).GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic)
                .SetValue(null, manager);
            overlay = new GameObject("PlayEffect", typeof(RectTransform));
            overlay.SetActive(false);
            overlay.AddComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            overlay.GetComponent<RectTransform>().sizeDelta = new Vector2(800, 600);
            var imageRoot = new GameObject("img_", typeof(RectTransform));
            imageRoot.transform.SetParent(overlay.transform, false);
            var imageRect = imageRoot.GetComponent<RectTransform>();
            imageRect.anchorMin = Vector2.zero;
            imageRect.anchorMax = Vector2.one;
            imageRect.sizeDelta = Vector2.zero;
            var image = imageRoot.AddComponent<Img>();
            var holder = overlay.AddComponent<RegressionFadeHolder>();
            holder.uiName = "PlayEffect";
            holder.elementTrsLst.Add(imageRoot.GetComponent<RectTransform>());
            manager.uiCtrlName2Uis[nameof(UiPlayEffectCtrl)] = new List<UiHolder>();
            holder.InstanceInit<UiPlayEffectCtrl>();
            var ctrl = (UiPlayEffectCtrl)holder.ctrl;

            // FadeOut must work without a preceding FadeIn or clean renderer state.
            imageRoot.SetActive(false);
            image.enabled = false;
            staleSprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), Vector2.one * .5f);
            image.cn = image.en = image.sprite = image.overrideSprite = staleSprite;
            image.canvasRenderer.SetColor(new Color(1, 1, 1, 0));
            image.canvasRenderer.cull = true;
            CheckFade(new FadeOutCmd(), ctrl, overlay, 1f, .5f, 0f, 2f);
            CheckFade(new FadeInCmd(), ctrl, overlay, 0f, .5f, 1f, .25f);
            LegacyAndImmediateDurations(ctrl, overlay);

            Execute(new FadeInCmd());
            Present(ctrl);
            ctrl.model.startedAt = Time.unscaledTimeAsDouble - .25;
            ctrl.OnUpdate();
            Execute(new FadeOutCmd());
            Check(overlay.activeSelf && ctrl.model.fadeOut && Mathf.Abs(image.color.a - 1f) < .001f,
                "a later command restarts the shared overlay in its requested direction");
            ctrl.Close();
            double closedTimestamp = ctrl.model.startedAt;
            Canvas.ForceUpdateCanvases();
            Check(!ctrl.model.waitingForFirstRender && ctrl.model.startedAt == closedTimestamp,
                "closing before the first render cancels its pending Canvas callback");
            ctrl.model.startedAt = Time.unscaledTimeAsDouble - 2;
            ctrl.OnUpdate();
            Check(!ctrl.model.fading && !overlay.activeSelf, "hiding cancels further fade updates");
            Execute(new FadeInCmd());
            Check(!ctrl.model.fadeOut && image.color.a == 0f, "reopening resets a previously hidden fade");

            foreach (string name in new[] { "FadeIn", "FadeOut" })
            foreach (string argument in new[] { "", "2" })
            {
                var program = ProgramDataForm.defaultData;
                program.name = "FadeContinuationRegression";
                Check(program.TryApplyCode(name + "(" + argument + ");Return 42;", out _, out _), name + " old/new source compiles");
                var execution = new InterpretDataForm.Data(-1, new List<BoxDataForm.Data>(),
                    new Dictionary<string, BoxDataForm.Data>(), program, 0, -1, 0, null,
                    new List<BoxDataForm.Data>(), 0);
                var result = execution.Interpret();
                Check(result.complete && result.errors.Count == 0 && result.ret.num == 42f
                    && execution.stack.Count == 0 && overlay.activeSelf && ctrl.model.fading
                    && ctrl.model.duration == (argument == "" ? 1f : 2f),
                    name + " runs the following statement immediately while the overlay is still fading");
            }
            Debug.Log("FADE_COMMANDS_REGRESSION_PASS checks=" + checks);
            EditorApplication.Exit(0);
        }
        catch (Exception error)
        {
            Debug.LogException(error);
            EditorApplication.Exit(1);
        }
        finally
        {
            if (overlay != null) UnityEngine.Object.DestroyImmediate(overlay);
            if (managerRoot != null) UnityEngine.Object.DestroyImmediate(managerRoot);
            if (staleSprite != null) UnityEngine.Object.DestroyImmediate(staleSprite);
        }
    }

    private static void CheckFade(CmdBase command, UiPlayEffectCtrl ctrl, GameObject overlay,
        float start, float middle, float end, float duration)
    {
        var task = Execute(command, Number(duration));
        Check(task.IsComplete() && task.error == null && overlay.activeSelf && ctrl.model.fading,
            command.GetName() + " returns immediately and shows the overlay; error=" + task.error);
        Check(ctrl.view.img_.color == new Color(0, 0, 0, start) && ctrl.model.duration == duration
            && ctrl.view.img_.gameObject.activeSelf && ctrl.view.img_.enabled
            && ctrl.view.img_.sprite == null && ctrl.view.img_.overrideSprite == null
            && !ctrl.view.img_.canvasRenderer.cull && ctrl.view.img_.canvasRenderer.GetColor().a == 1,
            command.GetName() + " starts at its own requested alpha with a drawable graphic");
        ctrl.model.startedAt = Time.unscaledTimeAsDouble - 100;
        ctrl.OnUpdate();
        Check(ctrl.model.waitingForFirstRender && overlay.activeSelf && ctrl.view.img_.color.a == start,
            command.GetName() + " cannot disappear during a load before its first Canvas submission");
        Present(ctrl);
        ctrl.model.startedAt = Time.unscaledTimeAsDouble - duration / 2;
        ctrl.OnUpdate();
        Check(Mathf.Abs(ctrl.view.img_.color.a - middle) < .01f && overlay.activeSelf,
            command.GetName() + " interpolates halfway without closing early");
        float previousScale = Time.timeScale;
        try
        {
            Time.timeScale = 0f;
            ctrl.model.startedAt = Time.unscaledTimeAsDouble - duration - .01;
            ctrl.OnUpdate();
        }
        finally { Time.timeScale = previousScale; }
        Check(ctrl.view.img_.color == new Color(0, 0, 0, end) && !overlay.activeSelf && !ctrl.model.fading,
            command.GetName() + " reaches its exact endpoint and closes after its supplied unscaled duration");
    }

    private static void LegacyAndImmediateDurations(UiPlayEffectCtrl ctrl, GameObject overlay)
    {
        foreach (var command in new CmdBase[] { new FadeInCmd(), new FadeOutCmd() })
        {
            Execute(command);
            Check(ctrl.model.duration == 1f && overlay.activeSelf,
                command.GetName() + " legacy zero-argument invocation keeps one second");
            Execute(command, CodeHelper.CreateBoxByStr(string.Empty));
            Check(ctrl.model.duration == 1f && overlay.activeSelf,
                command.GetName() + " padded omitted argument keeps one second");
        }
        ctrl.SetShow(true, new UiPlayEffectParam { fadeOut = true, duration = 3.5f });
        Check(ctrl.model.duration == 3.5f && ctrl.model.fadeOut && ctrl.view.img_.color.a == 1f,
            "direct UI parameters share duration/restart semantics");
        ctrl.SetShow(true, null);
        Check(ctrl.model.duration == 1f && !ctrl.model.fadeOut,
            "default direct Show retains the old one-second FadeIn");

        foreach (var command in new CmdBase[] { new FadeInCmd(), new FadeOutCmd() })
            foreach (float duration in new[] { 0f, -1f, float.NaN, float.PositiveInfinity, float.NegativeInfinity })
            {
                var task = Execute(command, Number(duration));
                float endpoint = command is FadeOutCmd ? 0f : 1f;
                Check(task.IsComplete() && task.error == null && ctrl.model.duration == 0
                    && !ctrl.model.fading && !ctrl.model.waitingForFirstRender && !overlay.activeSelf
                    && ctrl.view.img_.color.a == endpoint,
                    command.GetName() + " nonpositive/nonfinite duration completes without division or a lingering overlay");
            }
    }

    private static void Present(UiPlayEffectCtrl ctrl)
    {
        double before = Time.unscaledTimeAsDouble;
        Canvas.ForceUpdateCanvases();
        Check(!ctrl.model.waitingForFirstRender && ctrl.model.startedAt >= before,
            "native Canvas submission starts the fade clock once");
    }

    private static BoxDataForm.Data Number(float value) => CodeHelper.CreateBoxByNum(value);

    private static InterpretAsyncTask Execute(CmdBase command, params BoxDataForm.Data[] arguments)
    {
        var task = new InterpretAsyncTask(null);
        command.Execute(arguments.Reverse().ToArray(), new Dictionary<string, BoxDataForm.Data>(), task);
        return task;
    }

    private static void Check(bool value, string message)
    {
        if (!value) throw new InvalidOperationException(message);
        checks++;
    }
}

[ExecuteAlways]
public class RegressionFadeHolder : UiHolder { }
