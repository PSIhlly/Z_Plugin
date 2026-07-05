using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
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
            if (!isVising)
            {
                MapManager.instance.updateCtrl.UpdateSingleOne(this);
            }

            if (data.updateType == UpdateType.Always || isShowing || GlobalSettings.UPDATE_ALL_CHARACTER)
            {
                //nav: 通过BFS导航获取移动方向，乘以速度和距离的较小值作为本帧移动量
                if (data.navEnabled && !DynamicGlobalSettings.pauseNav)
                {
                    if ((data.destination - data.pos).sqrMagnitude < data.alertDis * data.alertDis)
                    {
                        Vector3 dir = manager.updateCtrl.GetNavDir(data.pos, data.destination, (int)data.pathDis);
                        Move(dir * Mathf.Min(Time.deltaTime * data.speed, (data.destination - data.pos).magnitude));
                    }

                }

                //gravity: 每帧施加向下的重力移动
                if(GlobalSettings.ENABLE_GRAVITY)
                {
                    Move(Vector3.down * Time.deltaTime * 3f);
                }
                //fix: 同步实例位置
                ins?.UpdatePos();
            }
            if (forceEuler != null)
            {
                data.euler = (Vector3)forceEuler;
                if (ins != null)
                    ins.transform.eulerAngles = (Vector3)forceEuler;
                forceEuler = null;
            }

            Z_EventHelper.Invoke(new CharacterEvent()
            {
                type = MapEventType.AfterUpdate,
                unit = this
            });
        }
        /// <summary>
        /// 角色移动核心方法，包含碰撞检测、避障方向计算和滑行逻辑
        /// 流程：
        /// 1. 将初始移动方向入队
        /// 2. 循环处理队列中的每个移动方向：
        ///    a. 遍历九宫格内tile和上面的ObjectUnit(isObstacle)/CharacterUnit
        ///    b. 用SAT碰撞检测(CheckCollide)获取最短碰撞距离res和避障方向avoidDir
        ///    c. 若有避障方向且在同一半球内，计算滑行方向入队
        ///    d. 按最短碰撞距离截断移动并应用位置
        /// </summary>
        public void Move(Vector3 dir)
        {
            /*
            var floor = manager.updateCtrl.g(this, belongTile, dir, CollideType.CollideOnly,out _);

            if (dir.y < 0 && floor <= 0.01f)
            {
                dir.y = 0;
            }
            else*/
            //移动方向队列：支持多次重试（初始方向+避障滑行后的新方向）
            Queue<Vector3> dirQue = new Queue<Vector3>();
            dirQue.Enqueue(dir);
            //首次尝试标记：首次尝试时即使碰撞距离为0也计算避障
            bool firstTry = true;

            //已尝试的避障方向（当前未使用的旧逻辑）
            HashSet<Vector3> existAvoid = new HashSet<Vector3>();

            //已检测过的单位集合，避免同一帧重复检测
            HashSet<MapUnit> existUnit = new HashSet<MapUnit>();
            bool moved = false;
            int times=0;
            while (dirQue.Count > 0)
            {
                times++;
                dir = dirQue.Dequeue();
                var mag = dir.magnitude;
                //避障方向列表：收集本轮所有碰撞产生的避障法线方向
                var avoidDir = new List<Vector3>();
                //res: 当前可移动的最短距离（碰撞距离），初始为完整移动距离
                float res = mag;

                var euler = data.euler;
                if (dir != Vector3.zero)
                {
                    existUnit.Clear();
                    //遍历角色所属tile周围的九宫格tile
                    foreach (var tile in manager.utilCtrl.GetNineTile((belongTile.data.mapPos.x, belongTile.data.mapPos.y, belongTile.data.mapPos.z), mag))
                    {
                        //踩在本层tile上时，跳过低于本层的tile，避免下层碰撞mesh干扰导致抖动
                        if (tile.data.mapPos.y < belongTile.data.mapPos.y)
                            continue;
                        //距离筛选：只检测1.3范围内的tile
                        if ((tile.data.pos - data.pos).sqrMagnitude > 1.69f)
                            continue;

                        List<Vector3> avoid;

                        //收集待检测的碰撞体：tile自身 + tile上的ObjectUnit + tile上的CharacterUnit
                        List<MapUnit> casts = new List<MapUnit>();
                        casts.Add(tile);
                        casts.AddRange(manager.updateCtrl.objectTileDic.Get(tile));
                        casts.AddRange(manager.updateCtrl.characterTileDic.Get(tile));

                        foreach (var obj in casts)
                        {
                            //跳过非障碍物ObjectUnit（isObstacle=false的不参与碰撞）
                            if (obj is ObjectUnit objU && !objU.data.isObstacle)//ignore no object
                            {
                                continue;
                            }
                            if (existUnit.Contains(obj) || obj == this)
                                continue;
                            existUnit.Add(obj);
                            //CheckCollide: 基于SAT的碰撞检测，返回碰撞距离cur和避障方向avoid
                            var cur = manager.updateCtrl.CheckCollide(this, obj, dir, CollideType.CollideOnly, out avoid);
                            //碰撞距离为0（已嵌入）：累加避障方向
                            if (Mathf.Abs(cur - res) < 0.01f && MathF.Abs(cur) < 0.01f)
                            {
                                avoidDir.AddRange(avoid);
                            }
                            //更短的碰撞距离：替换为新的最短距离和避障方向
                            else if (cur < res)
                            {
                                res = cur;
                                avoidDir.Clear();
                                avoidDir.AddRange(avoid);
                            }
                        }
                    }

                    //更新朝向：根据移动方向在XZ平面的投影计算朝向
                    var faceDir = dir;
                    faceDir.y = 0;
                    if (faceDir != Vector3.zero && !data.isMine)
                        euler = Quaternion.LookRotation(faceDir).eulerAngles;

                }
                //避障滑行逻辑：当存在避障方向时尝试贴墙滑行
                //条件：首次尝试 或 碰撞距离>0.001（还有移动空间），且有避障方向
                if ((firstTry || res > 0.001f) && avoidDir.Count > 0)
                {
                    //IsVectorsInHemisphere: 判断所有避障方向是否在同一半球内
                    //如果是，说明障碍物在同一侧，可以沿墙面滑行
                    if (Z_Math.Graph.IsVectorsInHemisphere(avoidDir, out var hemisphereNormal))
                    {
                        //累加所有避障方向得到合成避障法线
                        Vector3 newDir = Vector3.zero;
                        foreach (var o in avoidDir)//start
                        {
                            newDir += o;
                        }
                        //避障法线与移动方向不平行时才滑行
                        if (Mathf.Abs((newDir.normalized + dir.normalized).sqrMagnitude) > 0.00001f)
                        {
                            //将移动方向截断到碰撞距离处
                            dir = dir * (mag - res) / mag;
                            //loss: 剩余移动距离减去沿避障法线方向的分量（即垂直于墙面的损失）
                            float loss = dir.magnitude - Vector3.Dot(newDir.normalized, dir);
                            //计算滑行方向：从移动方向中减去沿避障法线的分量（含0.1*loss的额外缩减防止贴墙卡住）
                            var slideDir = (dir- (( + Vector3.Dot(newDir.normalized, dir) - 0.1f * loss  ) * newDir.normalized));
                            dirQue.Enqueue(slideDir);
                        }
                        else
                        {
                            //避障方向与移动方向平行，无法滑行
                        }
                    }
                    else
                    {
                        //避障方向不在同一半球，无法滑行
                    }

                    /*foreach (var o in avoidDir)
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
                        *//*               if (dir.x != 0 && dir.z != 0)
                                           Debug.Log(Time.frameCount + " " + realO.magnitude + " " + curDir.magnitude + " " + (curDir - realO).magnitude + " " + dir.magnitude);
                     *//*
                        dirQue.Enqueue(curDir - realO);
                    }*/
                }
                firstTry = false;
                //按最短碰撞距离截断当前移动方向
                dir *= (res) / mag;
                //有可移动距离时应用位置更新
                if (res > 0.01f)
                {
                    moved = true;
                    manager.updateCtrl.ApplyMove(this, data.pos + dir, euler);
                }
                else
                {
                }
            }
            if(moved)
            {
                if (manager.utilCtrl.IsOnBoundary(data.pos))
                {
                    Z_EventHelper.Invoke(new CharacterEvent()
                    {
                        type = MapEventType.BoundaryTouch,
                        unit = this
                    });
                }
                Z_EventHelper.Invoke(new CharacterEvent()
                {
                    type = MapEventType.Move,
                    unit = this
                });
            }

        }

        public override void Remove()
        {
            Z_EventHelper.Invoke(new CharacterEvent()
            {
                type = MapEventType.Hide,
                unit = this
            });
            CharacterUnitForm.RemoveData(data.uid);
            base.Remove();
        }

    }
}


