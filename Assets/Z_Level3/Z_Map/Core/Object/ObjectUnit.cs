using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_ByteSerialize;
using Z_Map.Form;
using Z_Math;
using Z_UnitSystem;
using Z_UnitSystem.Form;

namespace Z_Map
{
    public partial class ObjectUnit : MapUnit
    {
        public ObjectUnit(ObjectUnitForm.Data data) : base(data)
        {
        }
        public ObjectUnitForm.Data data => (ObjectUnitForm.Data)_data;

        public ObjectInstance ins
        {
            set { base.ins = value; }
            get { return (ObjectInstance)base.ins; }
        }

        public override Type GetInsType()
        {
            return typeof(ObjectInstance);
        }
        public override void Show()
        {
            if (isShowing)
            {
                return;
            }
            base.Show();

            Z_EventHelper.Invoke(new ObjectEvent()
            {
                type = MapEventType.Show,
                unit = this
            });
        }

        public override void UpdateInfo()
        {
            if (lastUpdateFrame == Time.frameCount)
                return;
            lastUpdateFrame = Time.frameCount;
            /*            if (isShowing)
                        {
                            if (_data.pos != ins.transform.position || _data.euler != ins.transform.eulerAngles)
                                MapManager.instance.updateCtrl.ApplyMove(this, ins.transform.position, ins.transform.eulerAngles);

                        }*/

            Z_EventHelper.Invoke(new ObjectEvent()
            {
                type = MapEventType.AfterUpdate,
                unit = this
            });
        }
        public void Move(Vector3 dir)
        {
            var mag = dir.magnitude;
            var targetPos = data.pos + dir;
            // Object不使用角色/通用位置的0.2边缘内缩；仅当目标中心真正触到或越过地图区域时触发BoundaryTouch。
            bool touchBoundary = manager.enable && !manager.utilCtrl.InArea(targetPos, 0f);
            var avoidDir = new List<Vector3>();
            HashSet<int> exist = new HashSet<int>() { data.uid };
            float res = mag;
            Dictionary<CharacterUnit, Vector3> push = new Dictionary<CharacterUnit, Vector3>();
            if (dir != Vector3.zero && data.isObstacle)
            {
                foreach (var tile in manager.utilCtrl.GetOverlap(data))
                {
                    foreach (var ch in manager.updateCtrl.characterOverlapTileDic.Get(tile))
                    {
                        if (exist.Contains(ch.data.uid))
                            continue;
                        exist.Add(ch.data.uid);

                        var dis = manager.updateCtrl.CheckCollide(this, ch, dir, CollideType.CollideOnly, out _);
                        if (dis < mag)
                        {
                            push[ch] = -dir * 1.1f / mag * (dis - mag);
                        }

                    }
                }
            }
            manager.updateCtrl.ApplyMove(this, targetPos, data.euler);
            foreach (var pair in push)
            {
                pair.Key.Move(pair.Value);
            }
            if (touchBoundary)
            {
                Z_EventHelper.Invoke(new ObjectEvent()
                {
                    type = MapEventType.BoundaryTouch,
                    unit = this
                });
            }
            else
            {
                Z_EventHelper.Invoke(new ObjectEvent()
                {
                    type = MapEventType.Move,
                    unit = this
                });
            }
        }
        public override void Remove()
        {
            ObjectUnitForm.RemoveData(data.uid);
            base.Remove();
        }

    }
}
