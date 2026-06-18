using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace DP.Editor
{
    /// <summary>
    /// 快速复制 / 粘贴组件信息工具。
    /// - Tools 菜单：打开窗口。
    /// - 右键 GameObject：「复制组件信息」「粘贴组件信息」。
    /// - 快捷键 Shift+C：打开窗口并复制选中对象；Shift+V：粘贴到选中对象；
    ///   Shift+Z：撤回；Shift+Y：重做（可在窗口里开关）。
    /// 复制内容：Image 的 sprite；Transform 的 position（世界坐标）/ rotation / scale，
    /// 若为 RectTransform 额外复制 rect 相关布局（anchoredPosition / sizeDelta / anchors / pivot）；
    /// TextPro（TMP_Text）的 内容 / fontAsset / materialPreset / fontSize / vertexColor。
    /// </summary>
    [InitializeOnLoad]
    public class QuickModifyComponent : EditorWindow
    {
        // ---------------- 复制缓存（静态，跨窗口与右键菜单共享）----------------
        static bool hasData;

        static bool hasImage;
        static Sprite copiedSprite;

        static bool isRect;
        static Vector3 worldPosition;
        static Vector3 localEuler;
        static Vector3 localScale;
        static Vector2 sizeDelta;
        static Vector2 anchorMin;
        static Vector2 anchorMax;
        static Vector2 pivot;
        static Rect rect;

        static bool hasText;
        static string copiedText;
        static TMP_FontAsset copiedFontAsset;
        static Material copiedMaterialPreset;
        static float copiedFontSize;
        static Color copiedVertexColor;

        // ---------------- 勾选项（控制粘贴时应用哪些；静态以便右键菜单读取）----------------
        static bool applyImage = true;
        static bool applyTransform = true;
        static bool applyText = true;

        // ---------------- 历史记录：复制源 → 粘贴目标 的配对 ----------------
        /// <summary>一次粘贴产生一对 (源, 目标)，用于「一键同步」重做。</summary>
        class HistoryPair
        {
            public GameObject source;
            public GameObject target;
        }
        static readonly List<HistoryPair> history = new List<HistoryPair>();
        // 当前这次复制的源对象（粘贴时与目标配对入历史）
        static GameObject copySource;
        Vector2 historyScroll;

        const string PREF_HOTKEY = "QuickModifyComponent.HotkeyEnabled";
        const string UNDO_NAME = "粘贴组件信息";

        static bool HotkeyEnabled
        {
            get => EditorPrefs.GetBool(PREF_HOTKEY, true);
            set => EditorPrefs.SetBool(PREF_HOTKEY, value);
        }

        // ---------------- 全局快捷键（可开关）----------------
        static QuickModifyComponent()
        {
            // 通过反射挂到编辑器全局键盘事件，实现可运行时开关的 Shift+C
            var field = typeof(EditorApplication).GetField(
                "globalEventHandler", BindingFlags.Static | BindingFlags.NonPublic);
            if (field != null)
            {
                var value = (EditorApplication.CallbackFunction)field.GetValue(null);
                value -= OnGlobalKey;
                value += OnGlobalKey;
                field.SetValue(null, value);
            }
        }

        static void OnGlobalKey()
        {
            if (!HotkeyEnabled)
            {
                return;
            }
            var e = Event.current;
            if (e == null || e.type != EventType.KeyDown)
            {
                return;
            }
            if (e.shift && !e.control && !e.alt && !e.command)
            {
                // Shift+C：打开弹窗，若选中了对象则同时复制
                if (e.keyCode == KeyCode.C)
                {
                    OpenWindow();
                    if (Selection.activeGameObject != null)
                    {
                        Copy(Selection.activeGameObject);
                    }
                    e.Use();
                }
                // Shift+V：粘贴到选中对象（支持多选）
                else if (e.keyCode == KeyCode.V)
                {
                    if (hasData && Selection.gameObjects.Length > 0)
                    {
                        foreach (var go in Selection.gameObjects)
                        {
                            PasteTo(go);
                        }
                    }
                    e.Use();
                }
                // Shift+Z：撤回
                else if (e.keyCode == KeyCode.Z)
                {
                    Undo.PerformUndo();
                    e.Use();
                }
                // Shift+Y：重做
                else if (e.keyCode == KeyCode.Y)
                {
                    Undo.PerformRedo();
                    e.Use();
                }
            }
        }

        // ---------------- 菜单 ----------------
        [MenuItem("Tools/QuickModifyComponent")]
        static void OpenWindow()
        {
            var win = GetWindow<QuickModifyComponent>("快速复制组件");
            win.minSize = new Vector2(440, 420);
            win.Show();
        }

        [MenuItem("GameObject/复制组件信息", false, 0)]
        static void CopyMenu(MenuCommand command)
        {
            var go = command.context as GameObject;
            if (go == null)
            {
                return;
            }
            // 多选时右键会对每个对象各调一次，这里只对 active 执行一次（复制只需单个源）
            if (Selection.activeGameObject != null && go != Selection.activeGameObject)
            {
                return;
            }
            Copy(go);
        }

        [MenuItem("GameObject/复制组件信息", true)]
        static bool CopyMenuValidate()
        {
            return Selection.activeGameObject != null;
        }

        [MenuItem("GameObject/粘贴组件信息", false, 0)]
        static void PasteMenu(MenuCommand command)
        {
            var go = command.context as GameObject;
            if (go == null)
            {
                return;
            }
            // 粘贴天然支持多选：Unity 会对每个选中对象各调一次
            PasteTo(go);
        }

        [MenuItem("GameObject/粘贴组件信息", true)]
        static bool PasteMenuValidate()
        {
            return hasData && Selection.activeGameObject != null;
        }

        // ---------------- 复制 / 粘贴核心 ----------------
        static void Copy(GameObject go)
        {
            copySource = go;

            var img = go.GetComponent<Image>();
            hasImage = img != null;
            copiedSprite = img != null ? img.sprite : null;

            var t = go.transform;
            worldPosition = t.position;
            localEuler = t.localEulerAngles;
            localScale = t.localScale;

            var rt = t as RectTransform;
            isRect = rt != null;
            if (rt != null)
            {
                sizeDelta = rt.sizeDelta;
                anchorMin = rt.anchorMin;
                anchorMax = rt.anchorMax;
                pivot = rt.pivot;
                rect = rt.rect;
            }

            // TextPro 继承 TMP_Text，用基类取以兼容 TextPro
            var tmp = go.GetComponent<TMP_Text>();
            hasText = tmp != null;
            if (tmp != null)
            {
                copiedText = tmp.text;
                copiedFontAsset = tmp.font;
                copiedMaterialPreset = tmp.fontSharedMaterial;
                copiedFontSize = tmp.fontSize;
                copiedVertexColor = tmp.color;
            }

            hasData = true;

            if (HasOpenInstances<QuickModifyComponent>())
            {
                GetWindow<QuickModifyComponent>().Repaint();
            }
        }

        static void PasteTo(GameObject go)
        {
            if (!hasData)
            {
                return;
            }

            if (applyImage && hasImage)
            {
                var img = go.GetComponent<Image>();
                if (img != null)
                {
                    Undo.RecordObject(img, UNDO_NAME);
                    img.sprite = copiedSprite;
                    EditorUtility.SetDirty(img);
                }
            }

            if (applyTransform)
            {
                var t = go.transform;
                Undo.RecordObject(t, UNDO_NAME);
                var rt = t as RectTransform;
                if (rt != null && isRect)
                {
                    // RectTransform：先设锚点/轴心/尺寸，最后用世界坐标定位
                    rt.anchorMin = anchorMin;
                    rt.anchorMax = anchorMax;
                    rt.pivot = pivot;
                    rt.localEulerAngles = localEuler;
                    rt.localScale = localScale;
                    rt.sizeDelta = sizeDelta;
                    rt.position = worldPosition;
                }
                else
                {
                    t.position = worldPosition;
                    t.localEulerAngles = localEuler;
                    t.localScale = localScale;
                }
                EditorUtility.SetDirty(t);
            }

            if (applyText && hasText)
            {
                var tmp = go.GetComponent<TMP_Text>();
                if (tmp != null)
                {
                    Undo.RecordObject(tmp, UNDO_NAME);
                    tmp.text = copiedText;
                    tmp.font = copiedFontAsset;
                    // 在 font 之后赋值材质预设，避免被 font 的默认材质覆盖
                    tmp.fontSharedMaterial = copiedMaterialPreset;
                    tmp.fontSize = copiedFontSize;
                    tmp.color = copiedVertexColor;
                    EditorUtility.SetDirty(tmp);
                }
            }

            RecordHistory(go);
        }

        /// <summary>记录一对 (复制源, 粘贴目标)；source/target 相同的配对去重。</summary>
        static void RecordHistory(GameObject target)
        {
            if (copySource == null || target == null)
            {
                return;
            }
            for (int i = 0, icnt = history.Count; i < icnt; i++)
            {
                if (history[i].source == copySource && history[i].target == target)
                {
                    return;
                }
            }
            history.Add(new HistoryPair() { source = copySource, target = target });
        }

        /// <summary>清除任一对象 missing 的历史配对。</summary>
        static void PruneHistory()
        {
            for (int i = history.Count - 1; i >= 0; i--)
            {
                if (history[i].source == null || history[i].target == null)
                {
                    history.RemoveAt(i);
                }
            }
        }

        /// <summary>一键同步：对每对历史，重新从源复制并粘贴到目标，使目标与源当前值一致。</summary>
        static void SyncAll()
        {
            PruneHistory();
            // 同步会改写复制缓存，记下当前源以便结束后恢复
            var snapshotSource = copySource;
            foreach (var pair in history)
            {
                Copy(pair.source);
                PasteToOnly(pair.target);
            }
            if (snapshotSource != null)
            {
                Copy(snapshotSource);
            }
        }

        /// <summary>仅执行粘贴赋值，不记录历史（供一键同步内部使用，避免重复入历史）。</summary>
        static void PasteToOnly(GameObject go)
        {
            var saved = copySource;
            copySource = null; // 置空使 RecordHistory 跳过
            PasteTo(go);
            copySource = saved;
        }

        // ---------------- 窗口 ----------------
        void OnGUI()
        {
            // 每帧清除 missing 的历史配对
            PruneHistory();

            EditorGUILayout.BeginHorizontal();
            DrawLeftPanel();
            DrawRightPanel();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            DrawHistoryPanel();
        }

        void DrawHistoryPanel()
        {
            EditorGUILayout.LabelField($"历史记录（复制源 → 粘贴目标）  共 {history.Count} 对", EditorStyles.boldLabel);

            historyScroll = EditorGUILayout.BeginScrollView(historyScroll, GUILayout.MinHeight(80), GUILayout.MaxHeight(200));
            if (history.Count == 0)
            {
                EditorGUILayout.LabelField("（暂无）");
            }
            else
            {
                using (new EditorGUI.DisabledScope(true))
                {
                    for (int i = 0, icnt = history.Count; i < icnt; i++)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.ObjectField(history[i].source, typeof(GameObject), true);
                        EditorGUILayout.LabelField("→", GUILayout.Width(20));
                        EditorGUILayout.ObjectField(history[i].target, typeof(GameObject), true);
                        EditorGUILayout.EndHorizontal();
                    }
                }
            }
            EditorGUILayout.EndScrollView();

            EditorGUILayout.BeginHorizontal();
            using (new EditorGUI.DisabledScope(history.Count == 0))
            {
                if (GUILayout.Button("一键同步"))
                {
                    SyncAll();
                }
                if (GUILayout.Button("清除"))
                {
                    history.Clear();
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        void DrawLeftPanel()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(170));

            EditorGUILayout.LabelField("应用项", EditorStyles.boldLabel);
            applyImage = EditorGUILayout.ToggleLeft("Image (sprite)", applyImage);
            applyTransform = EditorGUILayout.ToggleLeft("Transform", applyTransform);
            applyText = EditorGUILayout.ToggleLeft("TextPro", applyText);

            EditorGUILayout.Space();
            bool hk = HotkeyEnabled;
            bool newHk = EditorGUILayout.ToggleLeft("启用快捷键 Shift+C/V 复制粘贴 Shift+Z/Y 撤回重做", hk);
            if (newHk != hk)
            {
                HotkeyEnabled = newHk;
            }

            EditorGUILayout.Space();
            using (new EditorGUI.DisabledScope(Selection.activeGameObject == null))
            {
                if (GUILayout.Button("复制选中对象"))
                {
                    Copy(Selection.activeGameObject);
                }
            }
            using (new EditorGUI.DisabledScope(!hasData || Selection.gameObjects.Length == 0))
            {
                if (GUILayout.Button("粘贴到选中对象"))
                {
                    foreach (var go in Selection.gameObjects)
                    {
                        PasteTo(go);
                    }
                }
            }

            EditorGUILayout.EndVertical();
        }

        void DrawRightPanel()
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("已复制数据", EditorStyles.boldLabel);

            if (!hasData)
            {
                EditorGUILayout.HelpBox("尚未复制。右键对象 → 复制组件信息，或选中后点左侧按钮。", MessageType.Info);
                EditorGUILayout.EndVertical();
                return;
            }

            using (new EditorGUI.DisabledScope(true))
            {
                // Image
                EditorGUILayout.LabelField("Image", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                if (hasImage)
                {
                    EditorGUILayout.ObjectField("Sprite", copiedSprite, typeof(Sprite), false);
                }
                else
                {
                    EditorGUILayout.LabelField("（源对象无 Image）");
                }
                EditorGUI.indentLevel--;

                EditorGUILayout.Space();

                // Transform
                EditorGUILayout.LabelField(isRect ? "Transform (RectTransform)" : "Transform", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                EditorGUILayout.Vector3Field("Position (World)", worldPosition);
                EditorGUILayout.Vector3Field("Rotation", localEuler);
                EditorGUILayout.Vector3Field("Scale", localScale);
                if (isRect)
                {
                    EditorGUILayout.Vector2Field("Size (rect w/h)", new Vector2(rect.width, rect.height));
                    EditorGUILayout.Vector2Field("Size Delta", sizeDelta);
                    EditorGUILayout.Vector2Field("Anchor Min", anchorMin);
                    EditorGUILayout.Vector2Field("Anchor Max", anchorMax);
                    EditorGUILayout.Vector2Field("Pivot", pivot);
                }
                EditorGUI.indentLevel--;

                EditorGUILayout.Space();

                // TextPro
                EditorGUILayout.LabelField("TextPro", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                if (hasText)
                {
                    EditorGUILayout.TextField("内容", copiedText);
                    EditorGUILayout.ObjectField("Font Asset", copiedFontAsset, typeof(TMP_FontAsset), false);
                    EditorGUILayout.ObjectField("Material Preset", copiedMaterialPreset, typeof(Material), false);
                    EditorGUILayout.FloatField("Font Size", copiedFontSize);
                    EditorGUILayout.ColorField("Vertex Color", copiedVertexColor);
                }
                else
                {
                    EditorGUILayout.LabelField("（源对象无 TextPro）");
                }
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndVertical();
        }
    }
}
