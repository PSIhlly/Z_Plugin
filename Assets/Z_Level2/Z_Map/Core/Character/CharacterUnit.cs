using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Z_Map
{

    public class CharacterUnit : Unit
    {
        public CharacterUnit(int uid, GameObject prefab, Vector3 pos, Vector3 eular, UpdateType updateType = UpdateType.ShowOnly) : base(uid, prefab, pos, eular, updateType)
        {
        }
        public CharacterInstance ins
        {
            set { base.ins = value; }
            get { return (CharacterInstance)base.ins; }
        }

        public MapUnit map;

        public bool navEnabled;

        public Vector3 destination;
        public float speed = 1;

        public void Show()
        {
            if (ins == null || ins.gameObject == null)
            {
                var go = GameObject.Instantiate(prefab);
                go.transform.SetParent(MapManager.instance.mainGo.transform);
                ins = go.GetComponent<CharacterInstance>();
            }
            ins.gameObject.SetActive(true);
            ins.transform.position = pos;
            ins.transform.eulerAngles = eular;
        }
        public void Hide()
        {
            if (ins == null || ins.gameObject == null)
                return;
            ins.gameObject.SetActive(false);
        }

        public void UpdateInfo()
        {
            

            if (ins != null && ins.gameObject.activeSelf)
            {
                //nav
                if (navEnabled)
                {
                    Vector3 dir=MapManager.instance.GetNavDir(pos, destination);
                    ins.transform.position = ins.transform.position + dir * Time.deltaTime * speed;
                }
                pos = ins.transform.position;
                eular = ins.transform.eulerAngles;

                var newMapPos = MapManager.instance.mapUtilController.RealPos2MapPos(pos);
                if (MapManager.instance.mapUtilController.InArea(newMapPos))
                {
                    map.Unbind(this);
                    var newUnit = MapManager.instance.maps[newMapPos.x, newMapPos.y, newMapPos.z];
                    newUnit.Bind(this);
                    if (!newUnit.isShowing)
                    {
                        Hide();
                    }
                }
            }
        }
    }
}


