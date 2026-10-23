using UnityEngine;

namespace Z_Map
{
    /// <summary>
    /// 没有 tile 的地图边界处，用一块全屏黑幕做渐变压黑过渡。
    /// 覆盖关系来自自适应分辨率的欧氏距离场遮罩，包括地图内部空洞。
    /// 转角仅在重建时柔化，直边距离和边界纯黑保持不变。
    /// 每帧只更新 shader 全局量并绘制 1 次全屏 quad（单次遮罩采样、无 mip）；
    /// 遮罩只在 tile 增删或 fadeWidth 变化后（节流）重建。
    /// </summary>
    public class MapEdgeTransition : MonoBehaviour
    {
        /// <summary>渐变宽度（格）：tile 外边界纯黑 → 往内 fadeWidth 格完全透明。</summary>
        public float fadeWidth = 1f;
        /// <summary>边界内多宽保持纯黑（格），之后才开始淡出；默认 0 表示从 tile 边界就开始淡出。</summary>
        public float blackWidth = 0f;
        /// <summary>转角柔化半径（格），0 关闭；只影响遮罩重建，不增加 shader 采样。</summary>
        [Range(0f, 1f)] public float cornerSmoothing = 0.5f;
        /// <summary>边界处最大不透明度，1 为纯黑。</summary>
        public float maxAlpha = 1f;
        /// <summary>调试用：把“没有 tile”的区域涂成紫色，用来核对遮罩和 tile 边界是否对齐。</summary>
        public bool debugPurple = false;
        /// <summary>tile 增删后重建遮罩的最小间隔（秒），避免 Mod 连续涂抹时每笔都重建。</summary>
        public float rebuildInterval = 0.1f;
        /// <summary>遮罩 texel 预算：既用来挑每格分辨率，也用来兜底跳过异常大地图。</summary>
        public const int MAX_MASK_TEXELS = 1 << 20;
        /// <summary>遮罩四周的空边格数，保证渐变范围内都能采到“没有 tile”。</summary>
        const int MASK_MARGIN = 2;
        /// <summary>每格遮罩分辨率上限，实际值按 texel 预算自适应；越大轮廓越细。</summary>
        const int MASK_MAX_TEXELS_PER_CELL = 4;

        static readonly int MaskId = Shader.PropertyToID("_MapEdgeMask");
        static readonly int OriginId = Shader.PropertyToID("_MapEdgeOrigin");
        static readonly int RightId = Shader.PropertyToID("_MapEdgeRight");
        static readonly int UpId = Shader.PropertyToID("_MapEdgeUp");
        static readonly int CellSizeId = Shader.PropertyToID("_MapEdgeCellSize");
        static readonly int UvScaleId = Shader.PropertyToID("_MapEdgeUvScale");
        static readonly int UvOffsetId = Shader.PropertyToID("_MapEdgeUvOffset");
        static readonly int StrengthId = Shader.PropertyToID("_MapEdgeStrength");
        static readonly int MaskMinId = Shader.PropertyToID("_MapEdgeMaskMin");
        static readonly int MaskScaleId = Shader.PropertyToID("_MapEdgeMaskScale");
        static readonly int BlackStartId = Shader.PropertyToID("_MapEdgeBlackStart");
        static readonly int RampScaleId = Shader.PropertyToID("_MapEdgeRampScale");
        static readonly int DebugId = Shader.PropertyToID("_MapEdgeDebug");

        Camera cam;
        Material material;
        Mesh mesh;
        Texture2D mask;
        Vector2 maskOrigin;
        Vector2 maskCells = Vector2.one;
        Vector2 cellSize = Vector2.one;
        int maskVersion;
        float maskFade = -1f;
        float maskSmoothing = -1f;
        float maskMin;
        float maskScale = 1f;
        float lastBuildTime = -999f;
        float[] distance;
        float[] smoothBuffer;
        float[] lineInput, lineOutput, intersections;
        int[] sites;
        Color32[] pixels;

        public static MapEdgeTransition Create(MapManager manager)
        {
            var go = new GameObject("MapEdgeTransition");
            if (manager.mainGo != null)
                go.transform.SetParent(manager.mainGo.transform, false);
            return go.AddComponent<MapEdgeTransition>();
        }

        void Awake()
        {
            var shader = Resources.Load<Shader>("MapEdgeTransition");
            if (shader == null)
                shader = Shader.Find("Z_Map/EdgeTransition");
            if (shader != null)
                material = new Material(shader);
            // 着色器编译失败时 Unity 会换成内部错误着色器（屏幕整片紫），此时宁可不画。
            if (material != null && material.shader.name == "Hidden/InternalErrorShader")
                material = null;
            mesh = CreateQuadMesh();
        }

        void OnDestroy()
        {
            if (material != null)
                Destroy(material);
            if (mesh != null)
                Destroy(mesh);
            if (mask != null)
                Destroy(mask);
        }

        void LateUpdate()
        {
            if (!DynamicGlobalSettings.edgeTransition || material == null)
                return;

            var manager = MapManager.instance;
            if (manager == null || !manager.enable || manager.data == null)
                return;
            if (!RefreshMask(manager))
                return;
            if (!EnsureCamera())
                return;

            var trs = cam.transform;
            Vector3 forward = trs.forward;
            // 相机几乎平视地面时屏幕点无法稳定投到地面，直接不画。
            if (Mathf.Abs(forward.y) < 0.01f)
                return;

            // 遮罩按当前图层的地面高度投影；curCenterPos.y 可能是层间的插值，先吸附到最近的层，
            // 否则整块遮罩会相对 tile 有一格上下的位移。
            float groundY = 0f;
            if (manager.updateCtrl != null)
            {
                float layerY = manager.data.mainData.mapUnitSize.y;
                groundY = Mathf.Abs(layerY) > 0.0001f
                    ? Mathf.Round(manager.updateCtrl.curCenterPos.y / layerY) * layerY
                    : manager.updateCtrl.curCenterPos.y;
            }

            float halfH = cam.orthographicSize;
            float halfW = halfH * cam.aspect;
            Vector3 right = trs.right;
            Vector3 up = trs.up;
            Vector3 center = trs.position;

            // 正交相机下，屏幕四个角沿视线投到地面(y=groundY)是仿射映射：
            // 只需左下、右下、左上三个点就能推出整屏的地面 XZ。
            Vector3 origin = ProjectToGround(center - right * halfW - up * halfH, forward, groundY);
            Vector3 edgeX = ProjectToGround(center + right * halfW - up * halfH, forward, groundY);
            Vector3 edgeY = ProjectToGround(center - right * halfW + up * halfH, forward, groundY);

            Shader.SetGlobalTexture(MaskId, mask);
            Shader.SetGlobalVector(OriginId, new Vector4(origin.x, origin.z, 0f, 0f));
            Shader.SetGlobalVector(RightId, new Vector4(edgeX.x - origin.x, edgeX.z - origin.z, 0f, 0f));
            Shader.SetGlobalVector(UpId, new Vector4(edgeY.x - origin.x, edgeY.z - origin.z, 0f, 0f));
            Shader.SetGlobalVector(CellSizeId, new Vector4(cellSize.x, cellSize.y, 0f, 0f));
            // 格坐标 → 遮罩 uv：(cell - maskOrigin + 0.5) / 格数，使格子中心落在它那两个 texel 中间。
            Shader.SetGlobalVector(UvScaleId, new Vector4(1f / maskCells.x, 1f / maskCells.y, 0f, 0f));
            Shader.SetGlobalVector(UvOffsetId, new Vector4(
                (-maskOrigin.x + 0.5f) / maskCells.x, (-maskOrigin.y + 0.5f) / maskCells.y, 0f, 0f));
            Shader.SetGlobalFloat(StrengthId, Mathf.Clamp01(maxAlpha));
            Shader.SetGlobalFloat(MaskMinId, maskMin);
            Shader.SetGlobalFloat(MaskScaleId, maskScale);
            // 纯黑带：coverage(=距离/fadeWidth) 小于 blackStart 时 alpha 恒为 1，其余线性淡出。
            // blackStart 必须留一点余量：万一 blackWidth >= fadeWidth（比如 fadeWidth = 0），
            // blackStart 顶到 1 会让 rampScale 变成无穷大，整片地图都会变黑。
            float fade = Mathf.Max(0.0001f, fadeWidth);
            float blackStart = Mathf.Min(0.9995f, Mathf.Clamp01(Mathf.Clamp(blackWidth, 0f, fade) / fade));
            Shader.SetGlobalFloat(BlackStartId, blackStart);
            Shader.SetGlobalFloat(RampScaleId, 1f / Mathf.Max(0.0005f, 1f - blackStart));
            Shader.SetGlobalFloat(DebugId, debugPurple ? 1f : 0f);

            var matrix = Matrix4x4.TRS(center + forward * (cam.nearClipPlane + 0.5f), trs.rotation,
                new Vector3(halfW * 2f, halfH * 2f, 1f));
            Graphics.DrawMesh(mesh, matrix, material, 0, cam, 0, null, false, false);
        }

        bool EnsureCamera()
        {
            if (cam != null && cam.isActiveAndEnabled && cam.orthographic)
                return true;
            cam = Camera.main;
            return cam != null && cam.orthographic;
        }

        static Vector3 ProjectToGround(Vector3 rayOrigin, Vector3 forward, float groundY)
        {
            float t = (groundY - rayOrigin.y) / forward.y;
            return rayOrigin + forward * t;
        }

        /// <summary>
        /// tile 增删（mapVersion）或 fadeWidth 变化时重建距离场遮罩，其余时间复用。
        /// </summary>
        bool RefreshMask(MapManager manager)
        {
            var data = manager.data;
            if (data.mapXZ2Y == null || data.mapXZ2Y.Count == 0)
                return false;

            Vector3 unit = data.mainData.mapUnitSize;
            cellSize = new Vector2(unit.x > 0f ? unit.x : 1f, unit.z > 0f ? unit.z : 1f);

            bool fadeChanged = !Mathf.Approximately(fadeWidth, maskFade);
            bool smoothingChanged = !Mathf.Approximately(cornerSmoothing, maskSmoothing);
            if (data.mapVersion == maskVersion && !fadeChanged && !smoothingChanged)
                return mask != null;
            // 节流：Mod 连续涂抹时先用旧遮罩，下一次允许后再重建。
            if (Time.realtimeSinceStartup - lastBuildTime < rebuildInterval)
                return mask != null;

            maskVersion = data.mapVersion;
            maskFade = fadeWidth;
            maskSmoothing = cornerSmoothing;
            lastBuildTime = Time.realtimeSinceStartup;
            BuildMask(data);
            return mask != null;
        }

        /// <summary>
        /// 覆盖图 → 精确欧氏距离变换 → 柔化转角 → 烘焙；着色器仍只采样一次。
        /// </summary>
        void BuildMask(MapInfo data)
        {
            int minX = int.MaxValue;
            int maxX = int.MinValue;
            int minZ = int.MaxValue;
            int maxZ = int.MinValue;
            foreach (var column in data.mapXZ2Y.Keys)
            {
                if (column.Item1 < minX)
                    minX = column.Item1;
                if (column.Item1 > maxX)
                    maxX = column.Item1;
                if (column.Item2 < minZ)
                    minZ = column.Item2;
                if (column.Item2 > maxZ)
                    maxZ = column.Item2;
            }

            int cellsX = maxX - minX + 1 + MASK_MARGIN * 2;
            int cellsZ = maxZ - minZ + 1 + MASK_MARGIN * 2;
            // 每格分辨率按 texel 预算自适应：小图更细（轮廓更顺），大图自动降级而不是直接不画。
            int R = MASK_MAX_TEXELS_PER_CELL;
            while (R > 1 && (long)cellsX * cellsZ * R * R > MAX_MASK_TEXELS)
                R >>= 1;
            int width = cellsX * R;
            int height = cellsZ * R;
            if ((long)width * height > MAX_MASK_TEXELS)
                return;

            if (mask == null || mask.width != width || mask.height != height)
            {
                if (mask != null)
                    Destroy(mask);
                mask = new Texture2D(width, height, TextureFormat.Alpha8, false, true)
                {
                    name = "MapEdgeTransitionMask",
                    filterMode = FilterMode.Bilinear,
                    wrapMode = TextureWrapMode.Clamp,
                };
            }
            maskOrigin = new Vector2(minX - MASK_MARGIN, minZ - MASK_MARGIN);
            maskCells = new Vector2(cellsX, cellsZ);

            int count = width * height;
            if (distance == null || distance.Length != count)
            {
                distance = new float[count];
                pixels = new Color32[count];
            }

            // 不做“至少一格”的兜底：fadeWidth = 0 就是硬边（着色器里 saturate 会把它压成阶跃）。
            float fade = Mathf.Max(0.0001f, fadeWidth);
            int radius = Mathf.CeilToInt(Mathf.Clamp01(cornerSmoothing) * R);
            float big = Mathf.CeilToInt(fade * R) + radius + 2f;
            float bigSquared = big * big;
            for (int i = 0; i < count; i++)
                distance[i] = 0f;   // 空格（含四周空边）作为距离源
            foreach (var column in data.mapXZ2Y.Keys)
            {
                int x0 = (column.Item1 - minX + MASK_MARGIN) * R;
                int z0 = (column.Item2 - minZ + MASK_MARGIN) * R;
                for (int dz = 0; dz < R; dz++)
                {
                    int row = (z0 + dz) * width + x0;
                    for (int dx = 0; dx < R; dx++)
                        distance[row + dx] = bigSquared;
                }
            }

            TransformDistances(width, height);
            for (int i = 0; i < count; i++)
                distance[i] = Mathf.Sqrt(distance[i]);
            if (radius > 0)
                SmoothCorners(width, height, radius, Mathf.Clamp01(cornerSmoothing) * R / radius);

            // distance 是 texel 级距离，减掉半 texel 得到“到 tile 边界的格数”（边界外为负）。
            // 8 位只能存正数，如果把负的一半直接截断成 0，双线性在边界那一格上会插出
            // 残留覆盖度（边界处 alpha 只有 0.88~0.94，看着“不是纯黑”）。所以把整个窗口
            // 两侧各外扩一个 texel，存成 (d + texel) / (fade + 2*texel)：边界外和渐变结束处的
            // 相邻 texel 都不会被截断，插值是精确的，着色器再用 min/scale 反算回真实的 d。
            float texel = 1f / R;
            float window = fade + texel * 2f;
            maskMin = texel / window;
            maskScale = window / fade;
            for (int i = 0; i < count; i++)
            {
                float d = (distance[i] - 0.5f) * texel;
                float v = Mathf.Clamp01((d + texel) / window);
                byte value = (byte)(v * 255f + 0.5f);
                pixels[i] = new Color32(value, value, value, value);
            }
            mask.SetPixels32(pixels);
            mask.Apply(false, false);
        }

        // Separable lower envelopes of parabolas: exact squared Euclidean
        // distances in O(texels), rather than the eight-direction chamfer's
        // octagonal contours. Reuse line buffers across rebuilds.
        void TransformDistances(int width, int height)
        {
            int length = Mathf.Max(width, height);
            if (lineInput == null || lineInput.Length < length)
            {
                lineInput = new float[length];
                lineOutput = new float[length];
                sites = new int[length];
                intersections = new float[length + 1];
            }
            for (int z = 0; z < height; z++)
            {
                int row = z * width;
                for (int x = 0; x < width; x++) lineInput[x] = distance[row + x];
                TransformLine(width);
                for (int x = 0; x < width; x++) distance[row + x] = lineOutput[x];
            }
            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < height; z++) lineInput[z] = distance[z * width + x];
                TransformLine(height);
                for (int z = 0; z < height; z++) distance[z * width + x] = lineOutput[z];
            }
        }

        void TransformLine(int length)
        {
            int last = 0;
            sites[0] = 0;
            intersections[0] = float.NegativeInfinity;
            intersections[1] = float.PositiveInfinity;
            for (int q = 1; q < length; q++)
            {
                float split;
                while (true)
                {
                    int p = sites[last];
                    // Algebraically equivalent to subtracting q*q and p*p,
                    // but avoids cancellation on long, thin maps.
                    split = (lineInput[q] - lineInput[p]) / (2f * (q - p)) + (q + p) * 0.5f;
                    if (split > intersections[last]) break;
                    last--;
                }
                sites[++last] = q;
                intersections[last] = split;
                intersections[last + 1] = float.PositiveInfinity;
            }
            last = 0;
            for (int q = 0; q < length; q++)
            {
                while (intersections[last + 1] < q) last++;
                float delta = q - sites[last];
                lineOutput[q] = delta * delta + lineInput[sites[last]];
            }
        }

        void SmoothCorners(int width, int height, int radius, float strength)
        {
            if (smoothBuffer == null || smoothBuffer.Length != distance.Length)
                smoothBuffer = new float[distance.Length];
            for (int z = 0; z < height; z++)
            {
                int row = z * width;
                System.Array.Copy(distance, row, lineInput, 0, width);
                SmoothLine(width, radius);
                System.Array.Copy(lineOutput, 0, smoothBuffer, row, width);
            }
            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < height; z++) lineInput[z] = smoothBuffer[z * width + x];
                SmoothLine(height, radius);
                for (int z = 0; z < height; z++)
                {
                    int i = z * width + x;
                    // Only soften the inward crease where edges meet. Never
                    // reveal empty cells/holes or widen the straight-edge ramp.
                    float softened = Mathf.Min(distance[i], lineOutput[z]);
                    distance[i] = Mathf.Lerp(distance[i], softened, strength);
                }
            }
        }

        // Rolling triangular window: constant work per texel, independent of
        // radius. Double sums avoid visible drift over long map dimensions.
        void SmoothLine(int length, int radius)
        {
            double sum = 0, left = 0, right = 0;
            for (int k = -radius; k <= radius; k++)
                sum += (double)lineInput[Mathf.Clamp(k, 0, length - 1)] * (radius + 1 - Mathf.Abs(k));
            for (int k = -radius; k <= 0; k++) left += lineInput[Mathf.Clamp(k, 0, length - 1)];
            for (int k = 1; k <= radius + 1; k++) right += lineInput[Mathf.Min(k, length - 1)];
            double weightSum = (radius + 1) * (radius + 1);
            for (int q = 0; q < length; q++)
            {
                lineOutput[q] = (float)(sum / weightSum);
                sum += right - left;
                float next = lineInput[Mathf.Min(q + 1, length - 1)];
                left += (double)next - lineInput[Mathf.Max(q - radius, 0)];
                right += (double)lineInput[Mathf.Min(q + radius + 2, length - 1)] - next;
            }
        }

        static Mesh CreateQuadMesh()
        {
            var quad = new Mesh { name = "MapEdgeTransitionQuad" };
            quad.vertices = new[]
            {
                new Vector3(-0.5f, -0.5f, 0f),
                new Vector3(0.5f, -0.5f, 0f),
                new Vector3(-0.5f, 0.5f, 0f),
                new Vector3(0.5f, 0.5f, 0f),
            };
            quad.uv = new[]
            {
                new Vector2(0f, 0f),
                new Vector2(1f, 0f),
                new Vector2(0f, 1f),
                new Vector2(1f, 1f),
            };
            quad.triangles = new[] { 0, 2, 1, 2, 3, 1 };
            quad.RecalculateBounds();
            return quad;
        }
    }
}
