using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Runtime.ConstrainedExecution;
using UnityEngine;
using Z_ByteSerialize;
using Z_Map;
using Z_Map.Analysis;
using Z_Map.Form;
using Z_Math;
using Z_UnitSystem;
using Z_UnitSystem.Form;
using static UnityEditor.Progress;

namespace Z_Map
{

    public partial class CharacterUnit : MapUnit
    {
        public CharacterUnit(CharacterUnitForm.Data data) : base(data)
        {
        }
        public CharacterUnitForm.Data data => (CharacterUnitForm.Data)_data;

        public CharacterInstance ins
        {
            set { base.ins = value; }
            get { return (CharacterInstance)base.ins; }
        }

        public float pathDis;

        public override Type GetInsType()
        {
            return typeof(CharacterInstance);
        }
        public override void Show()
        {
            base.Show();
            Z_EventHelper.Invoke(new CharacterEvent()
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

            if (data.updateType == UpdateType.Always || isShowing)
            {
                //nav
                if (data.navEnabled)
                {
                    if ((data.destination - data.pos).sqrMagnitude < data.alertDis * data.alertDis)
                    {
                        Vector3 dir = manager.updateCtrl.GetNavDir(data.pos, data.destination, (int)data.pathDis);
                        Move(data.pos + dir * Time.deltaTime * data.speed);
                    }

                }

                //gravity)
                Move(Vector3.down * Time.deltaTime * 2f);
                //fix
                ins?.UpdatePos();
            }

            Z_EventHelper.Invoke(new CharacterEvent()
            {
                type = MapEventType.AfterUpdate,
                unit = this
            });
        }
        public void Move(Vector3 dir)
        {
            var mag = dir.magnitude;
            HashSet<int> exist = new HashSet<int>() { data.uid };

            float res = mag;
            var floor = manager.updateCtrl.CheckCollide(this, belongTile, dir, CollideType.CollideOnly);

            if (dir.y < 0 && floor <= 0.01f)
            {
                dir.y = 0;
            }
            else
            {
                res = Math.Min(manager.updateCtrl.CheckCollide(this, belongTile, dir, CollideType.CollideOnly), res);
            }
            if (dir != Vector3.zero)
            {
                foreach (var tile in manager.utilCtrl.GetNineTile((belongTile.data.mapPos.x, belongTile.data.mapPos.y, belongTile.data.mapPos.z)))
                {
                    foreach (var obj in manager.updateCtrl.objectTileDic.Get(tile))
                    {
                        if (exist.Contains(obj.data.uid))
                            continue;
                        exist.Add(obj.data.uid);
                        res = Math.Min(manager.updateCtrl.CheckCollide(this, obj, dir, CollideType.CollideOnly), res);


                    }
                    foreach (var ch in manager.updateCtrl.characterTileDic.Get(tile))
                    {
                        if (exist.Contains(ch.data.uid))
                            continue;
                        exist.Add(ch.data.uid);

                        res = Math.Min(manager.updateCtrl.CheckCollide(this, ch, dir, CollideType.CollideOnly), res);

                    }
                }

            }
            dir *= (res) / mag;
            manager.updateCtrl.ApplyMove(this, data.pos + dir, data.euler);


        }

        public override void Remove()
        {
            CharacterUnitForm.RemoveData(data.uid);
            base.Remove();
        }

    }
}


