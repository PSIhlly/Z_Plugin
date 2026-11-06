using System;
using System.Collections.Generic;
using UnityEngine;

namespace Form
{
    public static partial class ObjectProductForm
    {
        public partial class Data
        {
            // Retain the existing constructor API for callers creating legacy Products.
            public Data(int uid, string name, int labId, int protoUid, int icon, MapModelForm.Data model,
                bool collision, Dictionary<string, EventTriggerForm.Data> events,
                Dictionary<string, MapObjectParamForm.Data> paramDic, int minimapIcon, FaceType faceType,
                Dictionary<AnimDirecton, List<int>> animClip, bool isWangTile, bool boundsCollision, bool centerCollider)
                : this(uid, name, labId, protoUid, icon, model, collision, events, paramDic, minimapIcon,
                    faceType, animClip, isWangTile, boundsCollision, centerCollider,
                    new Dictionary<AnimDirecton, Vector3>(), new Dictionary<AnimDirecton, Vector3>()) { }

            private static readonly AnimDirecton[] ModelDirections =
                { AnimDirecton.Fixed, AnimDirecton.Up, AnimDirecton.Down, AnimDirecton.Left, AnimDirecton.Right };

            public Vector3 GetModelSize(AnimDirecton direction) =>
                modelSizes != null && modelSizes.TryGetValue(direction, out var size) ? size
                : model?.subPrefabUnitScale != null && model.subPrefabUnitScale.Count > 0
                    ? model.subPrefabUnitScale[0] : Vector3.one;

            public Vector3 GetModelPosition(AnimDirecton direction) =>
                modelPositions != null && modelPositions.TryGetValue(direction, out var position) ? position
                : model?.subPrefabUnitPos != null && model.subPrefabUnitPos.Count > 0
                    ? model.subPrefabUnitPos[0] : Vector3.zero;

            public void SetModelSize(AnimDirecton direction, Vector3 size)
            {
                EnsureDirectionTransforms();
                modelSizes[direction] = size;
            }

            public void SetModelPosition(AnimDirecton direction, Vector3 position)
            {
                EnsureDirectionTransforms();
                modelPositions[direction] = position;
            }

            private void EnsureDirectionTransforms()
            {
                // Seed every direction before mirroring/editing any one of them.
                // Values are structs; facings and runtime Products never share mutable geometry.
                Vector3 size = model?.subPrefabUnitScale != null && model.subPrefabUnitScale.Count > 0
                    ? model.subPrefabUnitScale[0] : Vector3.one;
                Vector3 position = model?.subPrefabUnitPos != null && model.subPrefabUnitPos.Count > 0
                    ? model.subPrefabUnitPos[0] : Vector3.zero;
                modelSizes ??= new Dictionary<AnimDirecton, Vector3>();
                modelPositions ??= new Dictionary<AnimDirecton, Vector3>();
                foreach (var direction in ModelDirections)
                {
                    if (!modelSizes.ContainsKey(direction)) modelSizes[direction] = size;
                    if (!modelPositions.ContainsKey(direction)) modelPositions[direction] = position;
                }
            }

            public void SyncLegacyModelTransform(AnimDirecton direction)
            {
                EnsureDirectionTransforms();
                EnsureOwnedLegacyModel();
                model.subPrefabUnitScale ??= new List<Vector3>();
                model.subPrefabUnitPos ??= new List<Vector3>();
                if (model.subPrefabUnitScale.Count == 0) model.subPrefabUnitScale.Add(Vector3.one);
                if (model.subPrefabUnitPos.Count == 0) model.subPrefabUnitPos.Add(Vector3.zero);
                model.subPrefabUnitScale[0] = GetModelSize(direction);
                model.subPrefabUnitPos[0] = GetModelPosition(direction);
            }

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
                EnsureDirectionTransforms();
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
                EnsureOwnedLegacyModel();
                if (model.subUnitTexsName == null)
                    model.subUnitTexsName = new List<List<int>>();
                if (model.subUnitTexsName.Count == 0)
                    model.subUnitTexsName.Add(new List<int>());
                if (model.subUnitTexsName[0] == null)
                    model.subUnitTexsName[0] = new List<int>();
                return model.subUnitTexsName[0];
            }

            private void EnsureOwnedLegacyModel()
            {
                if (model == null)
                    model = MapModelForm.defaultData;
                else if (ReferenceEquals(model, _defaultData.model))
                    model = model.Copy();
            }
        }
    }
}
