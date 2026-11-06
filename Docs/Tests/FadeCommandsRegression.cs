using System;
using System.Collections.Generic;
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
        try
        {
            typeof(GameCmdDataForm).GetMethod("Register", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null);
            typeof(FadeInCmd).GetMethod("Init", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null);
            typeof(FadeOutCmd).GetMethod("Init", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null);
            foreach (string name in new[] { "FadeIn", "FadeOut" })
            {
                Check(CmdBase.TryGetForm(name, out var form) && (form.prmTypes == null || form.prmTypes.Count == 0)
                    && form.retTypes.Count == 1 && form.retTypes[0] == "void"
                    && form.defaultCode == name + "();" && ((GameCmdDataForm.Data)form).labId == 10013,
                    name + " has executable zero-argument metadata in the Effect category");
            }

            managerRoot = new GameObject("fade-ui-manager");
            managerRoot.SetActive(false);
            var manager = managerRoot.AddComponent<UiManager>();
            typeof(Z_MonoSingleton<UiManager>).GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic)
                .SetValue(null, manager);
            overlay = new GameObject("PlayEffect", typeof(RectTransform));
            overlay.SetActive(false);
            var imageRoot = new GameObject("img_", typeof(RectTransform));
            imageRoot.transform.SetParent(overlay.transform, false);
            var image = imageRoot.AddComponent<Img>();
            var holder = overlay.AddComponent<RegressionFadeHolder>();
            holder.uiName = "PlayEffect";
            holder.elementTrsLst.Add(imageRoot.GetComponent<RectTransform>());
            manager.uiCtrlName2Uis[nameof(UiPlayEffectCtrl)] = new List<UiHolder>();
            holder.InstanceInit<UiPlayEffectCtrl>();
            var ctrl = (UiPlayEffectCtrl)holder.ctrl;

            CheckFade(new FadeInCmd(), ctrl, overlay, 0f, .5f, 1f);
            CheckFade(new FadeOutCmd(), ctrl, overlay, 1f, .5f, 0f);

            Execute(new FadeInCmd());
            ctrl.model.startedAt = Time.unscaledTimeAsDouble - .25;
            ctrl.OnUpdate();
            Execute(new FadeOutCmd());
            Check(overlay.activeSelf && ctrl.model.fadeOut && Mathf.Abs(image.color.a - 1f) < .001f,
                "a later command restarts the shared overlay in its requested direction");
            ctrl.Close();
            ctrl.model.startedAt = Time.unscaledTimeAsDouble - 2;
            ctrl.OnUpdate();
            Check(!ctrl.model.fading && !overlay.activeSelf, "hiding cancels further fade updates");
            Execute(new FadeInCmd());
            Check(!ctrl.model.fadeOut && image.color.a == 0f, "reopening resets a previously hidden fade");

            foreach (string name in new[] { "FadeIn", "FadeOut" })
            {
                var program = ProgramDataForm.defaultData;
                program.name = "FadeContinuationRegression";
                Check(program.TryApplyCode(name + "();Return 42;", out _, out _), name + " compiles in event source");
                var execution = new InterpretDataForm.Data(-1, new List<BoxDataForm.Data>(),
                    new Dictionary<string, BoxDataForm.Data>(), program, 0, -1, 0, null,
                    new List<BoxDataForm.Data>(), 0);
                var result = execution.Interpret();
                Check(result.complete && result.errors.Count == 0 && result.ret.num == 42f
                    && overlay.activeSelf && ctrl.model.fading,
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
        }
    }

    private static void CheckFade(CmdBase command, UiPlayEffectCtrl ctrl, GameObject overlay,
        float start, float middle, float end)
    {
        var task = Execute(command);
        Check(task.IsComplete() && task.error == null && overlay.activeSelf && ctrl.model.fading,
            command.GetName() + " returns immediately and shows the overlay; error=" + task.error);
        Check(ctrl.view.img_.color == new Color(0, 0, 0, start), command.GetName() + " starts with the requested black alpha");
        ctrl.model.startedAt = Time.unscaledTimeAsDouble - .5;
        ctrl.OnUpdate();
        Check(Mathf.Abs(ctrl.view.img_.color.a - middle) < .01f && overlay.activeSelf,
            command.GetName() + " interpolates halfway without closing early");
        float previousScale = Time.timeScale;
        try
        {
            Time.timeScale = 0f;
            ctrl.model.startedAt = Time.unscaledTimeAsDouble - 1.01;
            ctrl.OnUpdate();
        }
        finally { Time.timeScale = previousScale; }
        Check(ctrl.view.img_.color == new Color(0, 0, 0, end) && !overlay.activeSelf && !ctrl.model.fading,
            command.GetName() + " reaches its exact endpoint and closes after one unscaled second");
    }

    private static InterpretAsyncTask Execute(CmdBase command)
    {
        var task = new InterpretAsyncTask(null);
        command.Execute(Array.Empty<BoxDataForm.Data>(), new Dictionary<string, BoxDataForm.Data>(), task);
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
