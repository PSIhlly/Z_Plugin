using System;
using System.Collections.Generic;

namespace Form
{
    public static partial class MapObjectForm
    {
        public partial class Data
        {
            public AnimDirecton GetDefaultAnimDirection()
            {
                return faceType == FaceType.FourDirection ? AnimDirecton.Up : AnimDirecton.Fixed;
            }

            public AnimDirecton GetAnimDirection(float yAngle)
            {
                if (faceType != FaceType.FourDirection)
                    return AnimDirecton.Fixed;

                var angle = (yAngle % 360f + 360f) % 360f;
                if (angle >= 45f && angle < 135f)
                    return AnimDirecton.Right;
                if (angle >= 135f && angle < 225f)
                    return AnimDirecton.Down;
                if (angle >= 225f && angle < 315f)
                    return AnimDirecton.Left;
                return AnimDirecton.Up;
            }

            public void EnsureDirectionData()
            {
                var legacyClip = EnsureLegacyAnimClip();
                var migrateLegacyClip = animClip == null || animClip.Count == 0;
                if (animClip == null)
                    animClip = new Dictionary<AnimDirecton, List<int>>();

                List<int> fallback = null;
                if (!migrateLegacyClip)
                {
                    if (!animClip.TryGetValue(AnimDirecton.Fixed, out fallback) || fallback == null)
                    {
                        if (!animClip.TryGetValue(AnimDirecton.Up, out fallback) || fallback == null)
                        {
                            foreach (var pair in animClip)
                            {
                                if (pair.Value == null)
                                    continue;
                                fallback = pair.Value;
                                break;
                            }
                        }
                    }
                }
                fallback ??= legacyClip;

                foreach (AnimDirecton direction in Enum.GetValues(typeof(AnimDirecton)))
                {
                    if (!animClip.TryGetValue(direction, out var clip) || clip == null)
                        animClip[direction] = new List<int>(fallback);
                }
            }

            public List<int> GetAnimClip(AnimDirecton direction)
            {
                EnsureDirectionData();
                return animClip[direction];
            }

            public void SyncLegacyAnimClip(AnimDirecton direction)
            {
                EnsureDirectionData();
                var clip = new List<int>(animClip[direction]);
                var legacyClip = EnsureLegacyAnimClip();
                legacyClip.Clear();
                legacyClip.AddRange(clip);
            }

            private List<int> EnsureLegacyAnimClip()
            {
                if (model == null)
                    model = MapModelForm.defaultData;
                if (model.subUnitTexsName == null)
                    model.subUnitTexsName = new List<List<int>>();
                if (model.subUnitTexsName.Count == 0)
                    model.subUnitTexsName.Add(new List<int>());
                if (model.subUnitTexsName[0] == null)
                    model.subUnitTexsName[0] = new List<int>();
                return model.subUnitTexsName[0];
            }
        }
    }
}
