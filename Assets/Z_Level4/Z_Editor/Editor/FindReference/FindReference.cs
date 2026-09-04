// FindReference —— 单文件版「资产反向引用查询」工具
//
// 用法：把本文件拷进任意 Unity 项目的任意目录（不必是 Editor/ 文件夹），
//       菜单 Window/FindReference 打开窗口，首次点「初始化缓存」全量扫描。
//
// 全文用 #if UNITY_EDITOR 包裹，因此落在被 Runtime asmdef 覆盖的目录里也不会污染构建。

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace FindReference
{
    /// <summary>
    /// 引用关系缓存的核心管理器：负责加载 / 全量重建 / 增量更新 / 查询。
    /// 正反向映射在内存里用字典保存以加速查询，落盘时序列化成行式文本。
    /// </summary>
    [InitializeOnLoad]
    public static class FRDatabase
    {
        // guid -> 它直接引用的 guid 集合
        private static Dictionary<string, HashSet<string>> _forward;
        // guid -> 直接引用它的 guid 集合
        private static Dictionary<string, HashSet<string>> _reverse;
        private static bool _savePending;
        private static string _cachePath;
        // 总被引用次数（含间接）的惰性缓存；图变更时清空。供 Project 角标按可见项按需计算。
        private static readonly Dictionary<string, int> _totalRefCount = new Dictionary<string, int>();

        /// <summary>缓存或数据发生变化时触发，供 UI 重绘 / Project 角标刷新。</summary>
        public static event Action OnChanged;

        static FRDatabase()
        {
            Load();
        }

        /// <summary>
        /// 缓存落在 Library 下而非 Assets：它是可从工程再生的派生数据，
        /// 放 Library 天然不进版本控制，也不给宿主项目多出 .asset + .meta。
        /// </summary>
        public static string CachePath
        {
            get
            {
                if (_cachePath == null)
                {
                    string projectRoot = Path.GetDirectoryName(Application.dataPath) ?? ".";
                    _cachePath = Path.Combine(projectRoot, "Library", "FindReference.cache");
                }
                return _cachePath;
            }
        }

        /// <summary>缓存是否已初始化（磁盘缓存已加载，或刚做过全量重建）。</summary>
        public static bool IsReady => _forward != null && _reverse != null;

        // ---------------------------------------------------------------- 落盘格式
        //
        // 每行一条：<F|R> <guid> <target guid…>，空格分隔。
        // guid 是定长 hex、不含空格，所以不需要转义。空集合的条目不落盘。

        private const char ForwardTag = 'F';
        private const char ReverseTag = 'R';

        // ---------------------------------------------------------------- 加载

        public static void Load()
        {
            _forward = null;
            _reverse = null;

            if (File.Exists(CachePath))
            {
                try
                {
                    var forward = new Dictionary<string, HashSet<string>>();
                    var reverse = new Dictionary<string, HashSet<string>>();
                    foreach (var line in File.ReadLines(CachePath))
                    {
                        var parts = line.Split(' ');
                        if (parts.Length < 2 || parts[0].Length != 1)
                            continue;

                        Dictionary<string, HashSet<string>> dict;
                        if (parts[0][0] == ForwardTag) dict = forward;
                        else if (parts[0][0] == ReverseTag) dict = reverse;
                        else continue;

                        var set = new HashSet<string>();
                        for (int i = 2; i < parts.Length; i++)
                            if (!string.IsNullOrEmpty(parts[i]))
                                set.Add(parts[i]);
                        if (set.Count > 0)
                            dict[parts[1]] = set;
                    }
                    _forward = forward;
                    _reverse = reverse;
                }
                catch (Exception e)
                {
                    // 缓存损坏不该拖垮 Editor 启动，降级成「未初始化」让用户重建即可
                    Debug.LogWarning($"[FindReference] 缓存读取失败，请在窗口里重建：{e.Message}");
                    _forward = null;
                    _reverse = null;
                }
            }

            RaiseChanged();
        }

        // ---------------------------------------------------------------- 全量重建

        /// <summary>扫描全部 Assets，重建正反向映射并落盘。由窗口按钮触发。</summary>
        public static void Rebuild()
        {
            try
            {
                _forward = new Dictionary<string, HashSet<string>>();
                _reverse = new Dictionary<string, HashSet<string>>();

                string[] allPaths = AssetDatabase.GetAllAssetPaths();
                int total = allPaths.Length;
                for (int i = 0; i < total; i++)
                {
                    string path = allPaths[i];
                    if (!IsTrackable(path))
                        continue;

                    // 位与做节流：刷进度条本身比算一次依赖还贵，不必每个资产都刷
                    if ((i & 63) == 0 &&
                        EditorUtility.DisplayCancelableProgressBar(
                            "Find Reference", $"正在构建引用缓存… {i}/{total}",
                            total == 0 ? 1f : (float)i / total))
                    {
                        EditorUtility.ClearProgressBar();
                        return;
                    }

                    string guid = AssetDatabase.AssetPathToGUID(path);
                    if (string.IsNullOrEmpty(guid))
                        continue;

                    var deps = CollectDirectDeps(path, guid);
                    if (deps.Count > 0)
                    {
                        _forward[guid] = deps;
                        foreach (var t in deps)
                            GetOrAdd(_reverse, t).Add(guid);
                    }
                }

                Flush();
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            RaiseChanged();
        }

        // ---------------------------------------------------------------- 增量更新

        /// <summary>资产被导入 / 修改：重算其直接依赖并据差异维护反向映射。</summary>
        public static void UpdateAsset(string guid, string path)
        {
            if (!IsReady || string.IsNullOrEmpty(guid))
                return;

            var newDeps = CollectDirectDeps(path, guid);
            _forward.TryGetValue(guid, out var oldDeps);
            oldDeps = oldDeps ?? new HashSet<string>();

            // removed: 旧有、新无 —— 从对应 reverse 里摘掉自己
            foreach (var t in oldDeps)
                if (!newDeps.Contains(t) && _reverse.TryGetValue(t, out var rs))
                {
                    rs.Remove(guid);
                    if (rs.Count == 0) _reverse.Remove(t);
                }

            // added: 新有、旧无 —— 加入对应 reverse
            foreach (var t in newDeps)
                if (!oldDeps.Contains(t))
                    GetOrAdd(_reverse, t).Add(guid);

            if (newDeps.Count > 0)
                _forward[guid] = newDeps;
            else
                _forward.Remove(guid);

            MarkDirty();
        }

        /// <summary>资产被删除：从正反向映射中彻底清除。</summary>
        public static void RemoveAsset(string guid)
        {
            if (!IsReady || string.IsNullOrEmpty(guid))
                return;

            // 它引用的目标，反向里去掉它
            if (_forward.TryGetValue(guid, out var deps))
            {
                foreach (var t in deps)
                    if (_reverse.TryGetValue(t, out var rs))
                    {
                        rs.Remove(guid);
                        if (rs.Count == 0) _reverse.Remove(t);
                    }
                _forward.Remove(guid);
            }

            // 引用它的来源，正向里去掉它（它已不存在，关系失效）
            if (_reverse.TryGetValue(guid, out var refs))
            {
                foreach (var r in refs)
                    if (_forward.TryGetValue(r, out var fs))
                    {
                        fs.Remove(guid);
                        if (fs.Count == 0) _forward.Remove(r);
                    }
                _reverse.Remove(guid);
            }

            MarkDirty();
        }

        // ---------------------------------------------------------------- 查询

        /// <summary>直接引用该 guid 的来源 guid 集合（被引用者，直接）。</summary>
        public static IEnumerable<string> GetReferences(string guid)
        {
            if (IsReady && _reverse.TryGetValue(guid, out var set))
                return set;
            return Array.Empty<string>();
        }

        /// <summary>间接引用该 guid 的来源 guid 集合（传递闭包，已剔除直接引用与自身）。</summary>
        public static HashSet<string> GetIndirectReferences(string guid)
        {
            var all = Traverse(guid, _reverse);
            if (IsReady && _reverse.TryGetValue(guid, out var direct))
                all.ExceptWith(direct);
            return all;
        }

        /// <summary>该 guid 直接引用的目标 guid 集合（依赖，直接）。</summary>
        public static IEnumerable<string> GetDependencies(string guid)
        {
            if (IsReady && _forward.TryGetValue(guid, out var set))
                return set;
            return Array.Empty<string>();
        }

        /// <summary>该 guid 间接依赖的目标 guid 集合（传递闭包，已剔除直接依赖与自身）。</summary>
        public static HashSet<string> GetIndirectDependencies(string guid)
        {
            var all = Traverse(guid, _forward);
            if (IsReady && _forward.TryGetValue(guid, out var direct))
                all.ExceptWith(direct);
            return all;
        }

        /// <summary>直接被引用次数。</summary>
        public static int GetDirectReferenceCount(string guid)
        {
            if (IsReady && _reverse.TryGetValue(guid, out var set))
                return set.Count;
            return 0;
        }

        /// <summary>总被引用次数（直接 + 间接）。结果惰性缓存，图变更时失效。</summary>
        public static int GetTotalReferenceCount(string guid)
        {
            if (!IsReady || string.IsNullOrEmpty(guid))
                return 0;
            if (_totalRefCount.TryGetValue(guid, out var cached))
                return cached;
            int count = Traverse(guid, _reverse).Count;
            _totalRefCount[guid] = count;
            return count;
        }

        // 在反向 / 正向图上做广度优先遍历，返回从 start 可达的节点集合（不含 start，带 visited 防环）。
        private static HashSet<string> Traverse(string start, Dictionary<string, HashSet<string>> graph)
        {
            var visited = new HashSet<string>();
            if (graph == null || string.IsNullOrEmpty(start))
                return visited;

            var queue = new Queue<string>();
            queue.Enqueue(start);
            visited.Add(start);
            while (queue.Count > 0)
            {
                var cur = queue.Dequeue();
                if (graph.TryGetValue(cur, out var next))
                    foreach (var n in next)
                        if (visited.Add(n))
                            queue.Enqueue(n);
            }
            visited.Remove(start);
            return visited;
        }

        // ---------------------------------------------------------------- 内部辅助

        /// <summary>只跟踪 Assets/ 下的真实文件，排除目录。</summary>
        public static bool IsTrackable(string path)
        {
            if (string.IsNullOrEmpty(path) || !path.StartsWith("Assets/"))
                return false;
            if (AssetDatabase.IsValidFolder(path))
                return false;
            return true;
        }

        // 计算某资产的直接依赖 guid 集合，剔除自身
        private static HashSet<string> CollectDirectDeps(string path, string selfGuid)
        {
            var result = new HashSet<string>();
            if (string.IsNullOrEmpty(path))
                return result;

            foreach (var dep in AssetDatabase.GetDependencies(path, false))
            {
                if (dep == path)
                    continue;
                string g = AssetDatabase.AssetPathToGUID(dep);
                if (!string.IsNullOrEmpty(g) && g != selfGuid)
                    result.Add(g);
            }
            return result;
        }

        private static HashSet<string> GetOrAdd(Dictionary<string, HashSet<string>> dict, string key)
        {
            if (!dict.TryGetValue(key, out var set))
            {
                set = new HashSet<string>();
                dict[key] = set;
            }
            return set;
        }

        // 标脏 + 合并一次落盘（避免一次资产导入触发多次写盘）
        private static void MarkDirty()
        {
            if (!IsReady)
                return;
            RaiseChanged();
            if (_savePending)
                return;
            _savePending = true;
            EditorApplication.delayCall += DeferredFlush;
        }

        private static void DeferredFlush()
        {
            _savePending = false;
            Flush();
        }

        // 把内存字典序列化写盘
        private static void Flush()
        {
            if (!IsReady)
                return;

            try
            {
                var sb = new StringBuilder();
                AppendSection(sb, ForwardTag, _forward);
                AppendSection(sb, ReverseTag, _reverse);

                string dir = Path.GetDirectoryName(CachePath);
                if (!string.IsNullOrEmpty(dir))
                    Directory.CreateDirectory(dir);
                File.WriteAllText(CachePath, sb.ToString());
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[FindReference] 缓存写入失败：{e.Message}");
            }
        }

        private static void AppendSection(StringBuilder sb, char tag, Dictionary<string, HashSet<string>> dict)
        {
            foreach (var kv in dict)
            {
                if (kv.Value == null || kv.Value.Count == 0)
                    continue;
                sb.Append(tag).Append(' ').Append(kv.Key);
                foreach (var t in kv.Value)
                    sb.Append(' ').Append(t);
                sb.Append('\n');
            }
        }

        private static void RaiseChanged()
        {
            _totalRefCount.Clear();
            OnChanged?.Invoke();
        }
    }

    /// <summary>
    /// 监听资产导入 / 删除 / 移动，对引用缓存做增量维护。
    /// 仅在缓存已初始化时工作；未初始化则完全不干预（等用户在窗口里手动重建）。
    /// </summary>
    public class FRAssetPostprocessor : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            if (!FRDatabase.IsReady)
                return;

            foreach (var path in importedAssets)
            {
                if (!FRDatabase.IsTrackable(path))
                    continue;
                string guid = AssetDatabase.AssetPathToGUID(path);
                FRDatabase.UpdateAsset(guid, path);
            }

            // 移动不改变 guid，但目标路径可能由不跟踪变为跟踪（或反之），统一按当前路径重算
            foreach (var path in movedAssets)
            {
                string guid = AssetDatabase.AssetPathToGUID(path);
                if (FRDatabase.IsTrackable(path))
                    FRDatabase.UpdateAsset(guid, path);
                else
                    FRDatabase.RemoveAsset(guid);
            }

            foreach (var path in deletedAssets)
            {
                string guid = AssetDatabase.AssetPathToGUID(path);
                FRDatabase.RemoveAsset(guid);
            }
        }
    }

    /// <summary>
    /// 在 Project 窗口每个资产名后绘制「被引用次数」角标。
    /// </summary>
    [InitializeOnLoad]
    public static class FRProjectOverlay
    {
        private static GUIStyle _style;

        static FRProjectOverlay()
        {
            EditorApplication.projectWindowItemOnGUI += OnItemGUI;
            FRDatabase.OnChanged += () => EditorApplication.RepaintProjectWindow();
        }

        private static void OnItemGUI(string guid, Rect selectionRect)
        {
            if (!FRDatabase.IsReady || string.IsNullOrEmpty(guid))
                return;

            int total = FRDatabase.GetTotalReferenceCount(guid);
            if (total <= 0)
                return;

            int direct = FRDatabase.GetDirectReferenceCount(guid);

            if (_style == null)
            {
                _style = new GUIStyle(EditorStyles.miniLabel)
                {
                    alignment = TextAnchor.MiddleRight,
                    normal = { textColor = new Color(0.45f, 0.65f, 1f) },
                };
            }

            // 右侧绘制总被引用次数（含间接）；tooltip 区分直接 / 总数
            var rect = selectionRect;
            rect.xMin = rect.xMax - 30f;
            var content = new GUIContent(total.ToString(), $"被引用：直接 {direct} / 总计 {total}（含间接）");
            GUI.Label(rect, content, _style);
        }
    }

    /// <summary>
    /// 引用查询窗口：显示当前选中资产「被谁引用」与「引用了谁」，并提供缓存重建入口。
    /// </summary>
    public class FRWindow : EditorWindow
    {
        private string _targetGuid;
        private bool _locked;
        private Vector2 _scroll;

        [MenuItem("Window/FindReference")]
        public static void Open()
        {
            var win = GetWindow<FRWindow>();
            win.titleContent = new GUIContent("FindReference");
            win.minSize = new Vector2(320, 240);
            win.Show();
        }

        private void OnEnable()
        {
            Selection.selectionChanged += OnSelectionChanged;
            FRDatabase.OnChanged += Repaint;
            OnSelectionChanged();
        }

        private void OnDisable()
        {
            Selection.selectionChanged -= OnSelectionChanged;
            FRDatabase.OnChanged -= Repaint;
        }

        private void OnSelectionChanged()
        {
            if (_locked)
                return;
            var obj = Selection.activeObject;
            if (obj == null)
                return;
            string path = AssetDatabase.GetAssetPath(obj);
            if (string.IsNullOrEmpty(path))
                return;
            _targetGuid = AssetDatabase.AssetPathToGUID(path);
            Repaint();
        }

        private void OnGUI()
        {
            DrawToolbar();

            if (!FRDatabase.IsReady)
            {
                EditorGUILayout.HelpBox("引用缓存尚未初始化。点击上方「初始化缓存」全量扫描一次。", MessageType.Warning);
                return;
            }

            if (string.IsNullOrEmpty(_targetGuid))
            {
                EditorGUILayout.HelpBox("在 Project 中选择一个资产以查看其引用关系。", MessageType.Info);
                return;
            }

            string targetPath = AssetDatabase.GUIDToAssetPath(_targetGuid);
            EditorGUILayout.Space(2);
            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                var icon = AssetDatabase.GetCachedIcon(targetPath);
                GUILayout.Label(new GUIContent(targetPath, icon), EditorStyles.boldLabel);
            }

            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            var directRefs = new List<string>(FRDatabase.GetReferences(_targetGuid));
            var indirectRefs = new List<string>(FRDatabase.GetIndirectReferences(_targetGuid));
            EditorGUILayout.LabelField($"被引用：直接 {directRefs.Count} / 间接 {indirectRefs.Count}", EditorStyles.boldLabel);
            DrawSection($"直接引用 ({directRefs.Count})", directRefs);
            DrawSection($"间接引用 ({indirectRefs.Count})", indirectRefs);

            EditorGUILayout.Space(6);

            var directDeps = new List<string>(FRDatabase.GetDependencies(_targetGuid));
            var indirectDeps = new List<string>(FRDatabase.GetIndirectDependencies(_targetGuid));
            EditorGUILayout.LabelField($"引用了：直接 {directDeps.Count} / 间接 {indirectDeps.Count}", EditorStyles.boldLabel);
            DrawSection($"直接依赖 ({directDeps.Count})", directDeps);
            DrawSection($"间接依赖 ({indirectDeps.Count})", indirectDeps);

            EditorGUILayout.EndScrollView();
        }

        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                if (GUILayout.Button(FRDatabase.IsReady ? "重建缓存" : "初始化缓存",
                        EditorStyles.toolbarButton, GUILayout.Width(80)))
                {
                    FRDatabase.Rebuild();
                }

                GUILayout.FlexibleSpace();
                _locked = GUILayout.Toggle(_locked, "锁定", EditorStyles.toolbarButton, GUILayout.Width(50));
            }
        }

        private void DrawSection(string title, List<string> guids)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.LabelField(title);
            if (guids.Count == 0)
            {
                EditorGUILayout.LabelField("    （无）", EditorStyles.miniLabel);
                EditorGUI.indentLevel--;
                return;
            }

            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrEmpty(path))
                    continue;

                using (new EditorGUILayout.HorizontalScope())
                {
                    var icon = AssetDatabase.GetCachedIcon(path);
                    if (GUILayout.Button(new GUIContent(" " + path, icon),
                            EditorStyles.label, GUILayout.Height(18)))
                    {
                        var obj = AssetDatabase.LoadMainAssetAtPath(path);
                        EditorGUIUtility.PingObject(obj);
                    }

                    if (GUILayout.Button("选中", GUILayout.Width(40)))
                    {
                        var obj = AssetDatabase.LoadMainAssetAtPath(path);
                        Selection.activeObject = obj;
                    }
                }
            }
            EditorGUI.indentLevel--;
        }
    }
}
#endif
