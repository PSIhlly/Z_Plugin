using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;
using Z_Map.Form;
using Z_UnitSystem.Form;

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
        public Vector3? forceEuler;

        public float pathDis;

        public override Type GetInsType()
        {
            return typeof(CharacterInstance);
        }
        public override void Show()
        {
            GameManager.instance.characterCtrl.RegisterAnim(data);
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
                        Move(dir * Mathf.Min(Time.deltaTime * data.speed, (data.destination - data.pos).magnitude));
                    }

                }

                //gravity)
                Move(Vector3.down * Time.deltaTime * 2f);
                //fix
                ins?.UpdatePos();
            }
            if(forceEuler!=null)
            {
                data.euler = (Vector3)forceEuler;
                ins.transform.eulerAngles = (Vector3)forceEuler;
                forceEuler = null;
            }

            Z_EventHelper.Invoke(new CharacterEvent()
            {
                type = MapEventType.AfterUpdate,
                unit = this
            });
        }
        public void Move(Vector3 dir)
        {
            /*
            var floor = manager.updateCtrl.g(this, belongTile, dir, CollideType.CollideOnly,out _);

            if (dir.y < 0 && floor <= 0.01f)
            {
                dir.y = 0;
            }
            else*/
            Queue<Vector3> dirQue = new Queue<Vector3>();
            dirQue.Enqueue(dir);
            bool firstTry = true;

            HashSet<Vector3> existAvoid = new HashSet<Vector3>();

            HashSet<MapUnit> existUnit = new HashSet<MapUnit>();

            while (dirQue.Count > 0)
            {
                dir = dirQue.Dequeue();
                var mag = dir.magnitude;
                var avoidDir = new List<Vector3>();
                float res = mag;

                var euler = data.euler;
                if (dir != Vector3.zero)
                {
                    existUnit.Clear();
                    foreach (var tile in manager.utilCtrl.GetNineTile((belongTile.data.mapPos.x, belongTile.data.mapPos.y, belongTile.data.mapPos.z),mag))
                    {
                        if ((tile.data.pos - data.pos).sqrMagnitude > 1.69f)
                            continue;

                        List<Vector3> avoid;

                        List<MapUnit> casts = new List<MapUnit>();
                        casts.Add(tile);
                        casts.AddRange(manager.updateCtrl.objectTileDic.Get(tile));
                        casts.AddRange(manager.updateCtrl.characterTileDic.Get(tile));

                        foreach (var obj in casts)
                        {
                            if(obj is ObjectUnit objU&&!objU.data.isObstacle)//ignore no object
                            {
                                continue;
                            }
                            if (existUnit.Contains(obj) || obj == this)
                                continue;
                            existUnit.Add(obj);
                            //Debug.Log(data.uid + "   " + Time.frameCount + " " + obj.data.uid + " " + dir+"  : "+obj.belongTile.data.uid);
                            var cur = manager.updateCtrl.CheckCollide(this, obj, dir, CollideType.CollideOnly, out avoid);
                            if (Mathf.Abs(cur - res) < 0.01f && MathF.Abs(cur) < 0.01f)
                            {
                                avoidDir.AddRange(avoid);
                            }
                            else if (cur < res)
                            {
                                res = cur;
                                avoidDir.Clear();
                                avoidDir.AddRange(avoid);
                            }
                        }
                    }

                    var faceDir = dir;
                    faceDir.y = 0;
                    if (faceDir != Vector3.zero&&!data.isMine)
                        euler = Quaternion.LookRotation(faceDir).eulerAngles;

                }
                if ((firstTry || res > 0.001f) && avoidDir.Count > 0)
                {
                    var curDir = dir * (mag - res) / mag;
                    dirQue.Clear();
                    existAvoid.Clear();
                    foreach (var o in avoidDir)
                    {
                        if (existAvoid.Contains(o))
                            continue;
                        existAvoid.Add(o);
                        var dot = Vector3.Dot(curDir, o) - 0.001f;
                        if (dot >= 0)
                            continue;
                        var realO = o * dot;
                        if ((curDir - realO).sqrMagnitude < 0.0001f)
                            continue;
                        /*               if (dir.x != 0 && dir.z != 0)
                                           Debug.Log(Time.frameCount + " " + realO.magnitude + " " + curDir.magnitude + " " + (curDir - realO).magnitude + " " + dir.magnitude);
                     */
                        dirQue.Enqueue(curDir - realO);
                    }
                }
                firstTry = false;
                dir *= (res) / mag;
                if (res > 0.01f)
                    manager.updateCtrl.ApplyMove(this, data.pos + dir, euler);
            }


        }

        public override void Remove()
        {
            CharacterUnitForm.RemoveData(data.uid);
            base.Remove();
        }

    }
}


