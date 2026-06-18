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
    ///   Shift+Z：撤回；Shift+Y：重做；Shift+Q：切换「场景点选UI」模式（可在窗口里开关）。
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
        // root 设定：为空时 position 按世界坐标复制/粘贴；不为空时按相对 root 的局部坐标存储
        static Transform coordRoot;
        static bool copiedWithRoot;
        static Vector3 rootLocalPosition;
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
        const string PREF_PICKMODE = "QuickModifyComponent.PickMode";
        const string UNDO_NAME = "粘贴组件信息";

        static bool HotkeyEnabled
        {
            get => EditorPrefs.GetBool(PREF_HOTKEY, true);
            set => EditorPrefs.SetBool(PREF_HOTKEY, value);
        }

        /// <summary>场景点选 UI 模式：点击场景选中命中点最前面的 Image/TextPro 对象。</summary>
        static bool PickMode
        {
            get => EditorPrefs.GetBool(PREF_PICKMODE, false);
            set => EditorPrefs.SetBool(PREF_PICKMODE, value);
        }

        // 进入点选模式前各 SceneView 的 gizmos 开关状态，退出时还原
        static readonly Dictionary<SceneView, bool> savedGizmos = new Dictionary<SceneView, bool>();
        // 进入点选模式前的变换工具，退出时还原
        static Tool savedTool = Tool.Move;

        /// <summary>统一切换点选模式：进入时隐藏所有 SceneView 的 gizmos 并强制 RectTool，退出时还原。</summary>
        static void SetPickMode(bool on)
        {
            if (on == PickMode)
            {
                return;
            }
            if (on)
            {
                savedGizmos.Clear();
                foreach (SceneView sv in SceneView.sceneViews)
                {
                    if (sv == null)
                    {
                        continue;
                    }
                    savedGizmos[sv] = sv.drawGizmos;
                    sv.drawGizmos = false;
                }
                savedTool = Tools.current;
                Tools.current = Tool.Rect;
            }
            else
            {
                foreach (var kv in savedGizmos)
                {
                    if (kv.Key != null)
                    {
                        kv.Key.drawGizmos = kv.Value;
                    }
                }
                savedGizmos.Clear();
                Tools.current = savedTool;
            }
            PickMode = on;
            if (HasOpenInstances<QuickModifyComponent>())
            {
                GetWindow<QuickModifyComponent>().Repaint();
            }
            SceneView.RepaintAll();
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

            SceneView.duringSceneGui -= OnSceneGui;
            SceneView.duringSceneGui += OnSceneGui;
        }

        static void OnGlobalKey()
        {
            if (!HotkeyEnabled)
            {
                return;
            }
            // 正在重命名 / 搜索框 / 数值框里打字时，Shift+C/V 等应归输入框所有，别去抢。
            if (EditorGUIUtility.editingTextField)
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
                // Shift+V：粘贴到选中对象（支持多选，整批一步可撤销）
                else if (e.keyCode == KeyCode.V)
                {
                    if (hasData && Selection.gameObjects.Length > 0)
                    {
                        PasteToMany(Selection.gameObjects);
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
                // Shift+Q：切换「场景点选 UI」模式
                else if (e.keyCode == KeyCode.Q)
                {
                    SetPickMode(!PickMode);
                    e.Use();
                }
            }
        }

        // ---------------- 场景点选 UI（Shift+Q 开关）----------------
        static void OnSceneGui(SceneView sv)
        {
            if (!PickMode)
            {
                return;
            }

            // 在场景视图角落提示当前处于点选模式
            Handles.BeginGUI();
            var rect = new Rect(8, 8, 220, 20);
            var old = GUI.color;
            GUI.color = new Color(0.3f, 1f, 0.4f, 1f);
            GUI.Label(rect, "点选UI模式 (Shift+Q 关闭)", EditorStyles.boldLabel);
            GUI.color = old;
            Handles.EndGUI();

            var e = Event.current;
            // 抢占默认控件，使左键点击不触发常规拾取
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

            if (e.type == EventType.MouseDown && e.button == 0 && !e.alt)
            {
                // GUI 坐标 → 屏幕像素坐标（SceneView 相机像素坐标系）
                Vector2 screenPoint = HandleUtility.GUIPointToScreenPixelCoordinate(e.mousePosition);
                var hit = PickTopUI(screenPoint, sv.camera);
                if (hit != null)
                {
                    Selection.activeGameObject = hit;
                }
                e.Use();
            }
        }

        /// <summary>
        /// 在所有激活的 UGUI 元素里，收集在 screenPoint 命中的、拥有 Image 或 TMP_Text 的对象，
        /// 再按“循环钻入”规则选出本次目标。Image 还要求该像素 alpha 不为全透明。
        /// 渲染序 order 越大＝显示越靠前＝层级越靠下。
        /// 规则：若已选中某命中对象，则选它“更靠后一层”（order 更小）的；若它已是最靠后，
        /// 则回到最靠前（order 最大），完成循环。无选中时默认选最靠前。
        /// 命中测试统一用 SceneView 相机：场景里的 UI（含 Overlay Canvas）都通过它渲染，
        /// 与点击坐标系一致；用 Canvas 自带相机会因坐标系不符而几乎全部判定为未命中。
        /// </summary>
        static GameObject PickTopUI(Vector2 screenPoint, Camera sceneCam)
        {
            // 命中候选（已按下方需要排序前的原始收集）
            var candGo = new List<GameObject>();
            var candOrder = new List<RenderKey>();

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"[QuickModifyComponent] 点选检测 @ 屏幕({screenPoint.x:0},{screenPoint.y:0})");

            // 收集遍历根：Prefab 编辑模式用 prefab 内容根；否则用场景里的 root Canvas
            var roots = new List<Transform>();
            var stage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
            if (stage != null)
            {
                roots.Add(stage.prefabContentsRoot.transform);
            }
            else
            {
                var canvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
                foreach (var canvas in canvases)
                {
                    // 只处理 root Canvas，避免嵌套 Canvas 重复
                    if (canvas.rootCanvas != canvas)
                    {
                        continue;
                    }
                    if (!canvas.isActiveAndEnabled || !canvas.gameObject.activeInHierarchy)
                    {
                        continue;
                    }
                    roots.Add(canvas.transform);
                }
                // 按 sortingOrder 升序，保证跨 Canvas 的整体 drawIndex 顺序与渲染一致
                roots.Sort((a, b) =>
                {
                    int sa = a.GetComponent<Canvas>().sortingOrder;
                    int sb2 = b.GetComponent<Canvas>().sortingOrder;
                    return sa.CompareTo(sb2);
                });
            }

            int drawIndex = 0;
            foreach (var rootTr in roots)
            {
                // 整棵子树从上到下深度优先遍历，drawIndex 全局递增（越大越靠前显示）
                var all = rootTr.GetComponentsInChildren<Transform>(true);
                for (int k = 0; k < all.Length; k++, drawIndex++)
                {
                    var g = all[k].GetComponent<MaskableGraphic>();
                    if (g == null)
                    {
                        continue;
                    }
                    bool isImage = g is Image;
                    bool isRaw = g is RawImage;
                    bool isText = g is TMP_Text;
                    if (!isImage && !isRaw && !isText)
                    {
                        continue; // 非 Image/RawImage/TextPro，不参与、不记录
                    }
                    string name = GetPath(g.transform);
                    if (!g.isActiveAndEnabled || !g.gameObject.activeInHierarchy)
                    {
                        sb.AppendLine($"  忽略 [{name}]：组件或对象未激活");
                        continue;
                    }
                    var rt = g.rectTransform;
                    if (!RectTransformUtility.RectangleContainsScreenPoint(rt, screenPoint, sceneCam))
                    {
                        // 不在点击矩形内，属于绝大多数；不记录以免刷屏
                        continue;
                    }
                    // 命中矩形的才记录
                    if (isImage && !ImagePixelOpaque((Image)g, screenPoint, sceneCam))
                    {
                        sb.AppendLine($"  忽略 [{name}]：命中矩形，但点击像素为全透明");
                        continue;
                    }
                    if (isRaw && !RawImagePixelOpaque((RawImage)g, screenPoint, sceneCam))
                    {
                        sb.AppendLine($"  忽略 [{name}]：命中矩形，但点击像素为全透明");
                        continue;
                    }

                    var order = new RenderKey { drawIndex = drawIndex };
                    string kind = isImage ? "Image" : (isRaw ? "RawImage" : "TextPro");
                    sb.AppendLine($"  命中候选 [{name}]（{kind}，从上往下第 {drawIndex} 个）");

                    // 同一 GameObject 可能同时被多个 graphic 命中，按对象去重
                    if (!candGo.Contains(g.gameObject))
                    {
                        candGo.Add(g.gameObject);
                        candOrder.Add(order);
                    }
                }
            }

            GameObject best = ChooseNext(candGo, candOrder, Selection.activeGameObject, sb);
            Debug.Log(sb.ToString());
            return best;
        }

        /// <summary>
        /// 渲染顺序键：「从上到下深度优先遍历的全局序号」drawIndex。
        /// drawIndex 越大＝在层级中越靠下＝绘制更晚＝显示更靠前。
        /// 跨 Canvas 时遍历根已按 sortingOrder 升序，drawIndex 因此与整体渲染顺序一致。
        /// </summary>
        class RenderKey : System.IComparable<RenderKey>
        {
            public int drawIndex;

            public int CompareTo(RenderKey o)
            {
                return drawIndex.CompareTo(o.drawIndex);
            }

            public override string ToString() => $"#{drawIndex}";
        }

        /// <summary>
        /// 在命中候选中按“循环钻入”规则挑选：
        /// 候选按 order 升序（靠后→靠前）。若当前选中项在候选中，取其前一个（更靠后、层级更高）；
        /// 若它已是最靠后，回到最靠前（最靠下）完成循环。无选中时取最靠前。
        /// </summary>
        static GameObject ChooseNext(List<GameObject> candGo, List<RenderKey> candOrder, GameObject current, System.Text.StringBuilder sb)
        {
            int count = candGo.Count;
            if (count == 0)
            {
                sb.AppendLine("=> 未选中任何对象（无命中候选）");
                return null;
            }

            // 按 order 升序排序（同步两个并行列表）
            for (int a = 0; a < count - 1; a++)
            {
                for (int b = 0; b < count - 1 - a; b++)
                {
                    if (candOrder[b].CompareTo(candOrder[b + 1]) > 0)
                    {
                        (candOrder[b], candOrder[b + 1]) = (candOrder[b + 1], candOrder[b]);
                        (candGo[b], candGo[b + 1]) = (candGo[b + 1], candGo[b]);
                    }
                }
            }

            int curIdx = current != null ? candGo.IndexOf(current) : -1;

            GameObject best;
            string reason;
            if (curIdx < 0)
            {
                best = candGo[count - 1]; // 无选中/选中项未命中 → 最靠前
                reason = "无已选中命中项，选最靠前（层级最靠下）";
            }
            else if (curIdx - 1 >= 0)
            {
                best = candGo[curIdx - 1]; // 更靠后一层（order 更小、层级更高）
                reason = "当前选中项更靠后一层";
            }
            else
            {
                best = candGo[count - 1]; // 当前已是最靠后 → 回到最靠前，完成循环
                reason = "当前已是最靠后，循环回到最靠前";
            }

            sb.AppendLine($"=> 选中：{GetPath(best.transform)}（{reason}）");
            return best;
        }

        /// <summary>对象的层级路径，便于在 log 中辨认。</summary>
        static string GetPath(Transform t)
        {
            var sb = new System.Text.StringBuilder(t.name);
            var p = t.parent;
            while (p != null)
            {
                sb.Insert(0, p.name + "/");
                p = p.parent;
            }
            return sb.ToString();
        }

        /// <summary>
        /// 判断点击位置 Image 像素是否非全透明。
        /// 纹理不可读时无法采样，按“命中即视为不透明”处理（不阻断选择）。
        /// </summary>
        static bool ImagePixelOpaque(Image img, Vector2 screenPoint, Camera cam)
        {
            var sprite = img.sprite;
            if (sprite == null)
            {
                // 纯色 Image：alpha 取颜色 alpha
                return img.color.a > 0.001f;
            }
            var tex = sprite.texture;
            if (tex == null || !tex.isReadable)
            {
                return true;
            }

            var rt = img.rectTransform;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, screenPoint, cam, out Vector2 local))
            {
                return true;
            }

            // 本地坐标 → [0,1] 归一化（以 rect 左下为原点）
            var r = rt.rect;
            float u = Mathf.InverseLerp(r.xMin, r.xMax, local.x);
            float v = Mathf.InverseLerp(r.yMin, r.yMax, local.y);

            // 映射到 sprite 在图集中的纹理区域
            var tr = sprite.textureRect;
            int px = Mathf.Clamp((int)(tr.x + u * tr.width), (int)tr.x, (int)(tr.x + tr.width) - 1);
            int py = Mathf.Clamp((int)(tr.y + v * tr.height), (int)tr.y, (int)(tr.y + tr.height) - 1);

            float alpha = tex.GetPixel(px, py).a * img.color.a;
            return alpha > 0.001f;
        }

        /// <summary>
        /// 判断点击位置 RawImage 像素是否非全透明。考虑 uvRect 映射；纹理不可读时按命中即不透明处理。
        /// </summary>
        static bool RawImagePixelOpaque(RawImage img, Vector2 screenPoint, Camera cam)
        {
            var tex = img.texture as Texture2D;
            if (tex == null || !tex.isReadable)
            {
                return img.color.a > 0.001f;
            }

            var rt = img.rectTransform;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, screenPoint, cam, out Vector2 local))
            {
                return true;
            }

            var r = rt.rect;
            float u = Mathf.InverseLerp(r.xMin, r.xMax, local.x);
            float v = Mathf.InverseLerp(r.yMin, r.yMax, local.y);

            // 映射到 uvRect 指定的纹理区域
            var uv = img.uvRect;
            float tu = uv.x + u * uv.width;
            float tv = uv.y + v * uv.height;

            float alpha = tex.GetPixelBilinear(tu, tv).a * img.color.a;
            return alpha > 0.001f;
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
            // root 不为空时，额外记录相对 root 的局部坐标
            copiedWithRoot = coordRoot != null;
            rootLocalPosition = coordRoot != null ? coordRoot.InverseTransformPoint(t.position) : Vector3.zero;
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
            // 把本次粘贴（可能同时改 Image / Transform / TextPro 多个对象）合并为一步可撤销。
            Undo.IncrementCurrentGroup();
            int group = Undo.GetCurrentGroup();
            PasteToCore(go);
            Undo.SetCurrentGroupName(UNDO_NAME);
            Undo.CollapseUndoOperations(group);
        }

        /// <summary>
        /// 批量粘贴到多个对象，整批合并为一步可撤销。
        /// </summary>
        static void PasteToMany(IEnumerable<GameObject> gos)
        {
            if (!hasData)
            {
                return;
            }
            Undo.IncrementCurrentGroup();
            int group = Undo.GetCurrentGroup();
            foreach (var go in gos)
            {
                PasteToCore(go);
            }
            Undo.SetCurrentGroupName(UNDO_NAME);
            Undo.CollapseUndoOperations(group);
        }

        /// <summary>粘贴赋值核心：登记 Undo 并写入各组件值；不管理 Undo group 边界。</summary>
        static void PasteToCore(GameObject go)
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
                    Undo.RegisterCompleteObjectUndo(img, UNDO_NAME);
                    img.sprite = copiedSprite;
                    EditorUtility.SetDirty(img);
                }
            }

            if (applyTransform)
            {
                var t = go.transform;
                // 用 RegisterCompleteObjectUndo 而非 RecordObject：后者靠序列化字段差分决定是否建 undo
                // 条目，仅改 Transform 世界 position 时会漏检，导致 Ctrl+Z 无效；前者整对象压栈，位置-only
                // 改动也能可靠回退。
                Undo.RegisterCompleteObjectUndo(t, UNDO_NAME);
                // 复制时带 root 且 root 仍存在：用 root 把相对坐标还原为世界坐标；否则直接用世界坐标
                Vector3 targetWorldPos = (copiedWithRoot && coordRoot != null)
                    ? coordRoot.TransformPoint(rootLocalPosition)
                    : worldPosition;
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
                    rt.position = targetWorldPos;
                }
                else
                {
                    t.position = targetWorldPos;
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
                    Undo.RegisterCompleteObjectUndo(tmp, UNDO_NAME);
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

        /// <summary>一键同步：对每对历史，重新从源复制并粘贴到目标，使目标与源当前值一致。整批合并为一步可撤销。</summary>
        static void SyncAll()
        {
            PruneHistory();
            // 同步会改写复制缓存，记下当前源以便结束后恢复
            var snapshotSource = copySource;
            Undo.IncrementCurrentGroup();
            int group = Undo.GetCurrentGroup();
            foreach (var pair in history)
            {
                Copy(pair.source);
                PasteToOnly(pair.target);
            }
            Undo.SetCurrentGroupName(UNDO_NAME);
            Undo.CollapseUndoOperations(group);
            if (snapshotSource != null)
            {
                Copy(snapshotSource);
            }
        }

        /// <summary>仅执行粘贴赋值，不记录历史、不管理 Undo group（供一键同步内部批量使用）。</summary>
        static void PasteToOnly(GameObject go)
        {
            var saved = copySource;
            copySource = null; // 置空使 RecordHistory 跳过
            PasteToCore(go);
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
            EditorGUILayout.LabelField("Root（空=世界坐标）", EditorStyles.boldLabel);
            coordRoot = (Transform)EditorGUILayout.ObjectField(coordRoot, typeof(Transform), true);

            EditorGUILayout.Space();
            bool hk = HotkeyEnabled;
            bool newHk = EditorGUILayout.ToggleLeft("启用快捷键 Shift+C/V 复制粘贴 Shift+Z/Y 撤回重做", hk);
            if (newHk != hk)
            {
                HotkeyEnabled = newHk;
            }

            bool pm = PickMode;
            bool newPm = EditorGUILayout.ToggleLeft("场景点选UI模式 (Shift+Q)", pm);
            if (newPm != pm)
            {
                SetPickMode(newPm);
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
                    PasteToMany(Selection.gameObjects);
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
                if (copiedWithRoot)
                {
                    EditorGUILayout.Vector3Field("Position (相对 Root)", rootLocalPosition);
                }
                else
                {
                    EditorGUILayout.Vector3Field("Position (World)", worldPosition);
                }
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
