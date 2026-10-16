using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.EventSystems;
using Z_DesignStyle;
using Z_Debug;
namespace Z_Input
{
    public class InputKeyDownEvent : Z_Event
    {
        public List<KeyCode> key;
    }
    public class InputKeyEvent : Z_Event
    {
        public List<KeyCode> key;
    }
    public class InputKeyUpEvent : Z_Event
    {
        public List<KeyCode> key;
    }

    public class InputPointDownEvent : Z_Event
    {
        public int id;
        public Vector3 pos;
        public GameObject ui;
    }
    public class InputPointUpEvent : Z_Event
    {
        public int id;
        public Vector3 pos;
        public GameObject ui;
        public bool canceled;
    }
    public class InputPointEvent : Z_Event
    {
        public int id;
        public Vector3 pos;
        public Vector3 delta;
        public GameObject ui;
    }

    public class InputMouseDownEvent : Z_Event
    {
        public int id;
        public Vector3 pos;
        public GameObject ui;
    }
    public class InputMouseUpEvent : Z_Event
    {
        public int id;
        public Vector3 pos;
        public GameObject ui;
    }
    public class InputMouseEvent : Z_Event
    {
        public int id;
        public Vector3 pos;
        public Vector3 delta;
        public GameObject ui;
    }
    public class InputMouseMoveEvent : Z_Event
    {
        public Vector3 pos;
    }
    public class InputMouseScrollEvent : Z_Event
    {
        public float delta;
    }


    [DefaultExecutionOrder(-100)]
    public class InputManager : Z_MonoManager<InputManager>
    {
        List<KeyCode> keyLst = new List<KeyCode>();
        public bool enabled = true;
        public Vector2 screenSize;
        public Vector2 screenWorldSize;


        public Dictionary<int, Vector2> mousePos = new Dictionary<int, Vector2>();
        public Dictionary<int, Vector2> mouseOldPos = new Dictionary<int, Vector2>();

        private Vector3 lastMousePos;

        public Dictionary<int, Vector2> id2Pos = new Dictionary<int, Vector2>();
        public Dictionary<int, Vector2> id2OldPos = new Dictionary<int, Vector2>();

        private List<int> tmpList = new List<int>();
        private HashSet<int> tmpHash = new HashSet<int>();
        private bool hasFocus = true;
        private bool paused;

        public override void Init()
        {
            screenSize = new Vector2(Screen.width, Screen.height);
            float orthographicSize = Camera.main.orthographicSize;
            float aspect = Camera.main.aspect;
            screenWorldSize = new Vector2(orthographicSize * aspect, orthographicSize);
        }
        public void Register()
        {
            lastMousePos = Vector3.zero;
        }


        private void ManageTouch()
        {
            id2OldPos.Clear();
            foreach (var point in id2Pos)
                id2OldPos[point.Key] = point.Value;
            tmpHash.Clear();
            for (int i = 0; i < Input.touchCount; i++)
                ProcessTouch(Input.GetTouch(i));

            // A device can lose a touch without an Ended/Canceled sample.
            tmpList.Clear();
            foreach (var point in id2Pos)
                if (!tmpHash.Contains(point.Key))
                    tmpList.Add(point.Key);
            foreach (var id in tmpList)
                ReleaseTouch(id, id2Pos[id], true);
        }

        private void ProcessTouch(Touch touch)
        {
            int id = touch.fingerId;
            Vector2 pos = touch.position;
            if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                ReleaseTouch(id, pos, touch.phase == TouchPhase.Canceled);
                return;
            }

            tmpHash.Add(id);
            if (touch.phase == TouchPhase.Began)
            {
                if (id2Pos.TryGetValue(id, out var canceledPos))
                    ReleaseTouch(id, canceledPos, true);
                id2Pos[id] = pos;
                id2OldPos[id] = pos;
                Z_EventHelper.Invoke(new InputPointDownEvent
                {
                    id = id,
                    pos = pos,
                    ui = UICheck(pos)
                });
            }
            else if (id2Pos.TryGetValue(id, out var previous))
            {
                // Unity fingerId, unlike nearest-position matching, stays
                // attached to the same finger when two sticks cross/swap.
                id2Pos[id] = pos;
                Z_EventHelper.Invoke(new InputPointEvent
                {
                    id = id,
                    pos = pos,
                    delta = pos - (id2OldPos.TryGetValue(id, out var old) ? old : previous),
                    ui = UICheck(pos)
                });
            }
            // After focus loss, ignore a still-held finger until a fresh Began.
        }

        private void ReleaseTouch(int id, Vector2 pos, bool canceled)
        {
            if (!id2Pos.Remove(id))
                return;
            id2OldPos.Remove(id);
            Z_EventHelper.Invoke(new InputPointUpEvent
            {
                id = id,
                pos = pos,
                ui = UICheck(pos),
                canceled = canceled
            });
        }

        private void ClearTouchState()
        {
            tmpList.Clear();
            tmpList.AddRange(id2Pos.Keys);
            foreach (var id in tmpList)
                if (id2Pos.TryGetValue(id, out var pos))
                    ReleaseTouch(id, pos, true);
            id2OldPos.Clear();
        }

        private void OnApplicationFocus(bool focus)
        {
            hasFocus = focus;
#if UNITY_ANDROID && !UNITY_EDITOR
            if (!focus)
                ClearTouchState();
#endif
        }

        private void OnApplicationPause(bool pause)
        {
            paused = pause;
#if UNITY_ANDROID && !UNITY_EDITOR
            if (pause)
                ClearTouchState();
#endif
        }

        private void OnDisable()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            ClearTouchState();
#endif
        }
        private void ManageMouse()
        {
            mouseOldPos.Clear();
            tmpHash.Clear();
            foreach (var mouse in mousePos)
            {
                mouseOldPos[mouse.Key] = mouse.Value;
            }

            for (int i = 0; i <= 1; i++)
            {
                if (Input.GetMouseButtonUp(i))
                {
                    Z_EventHelper.Invoke(new InputMouseUpEvent()
                    {
                        id = i,
                        pos = Input.mousePosition,
                        ui = UICheck(Input.mousePosition)
                    });
                }
            }

            for (int i = 0; i <= 1; i++)
            {
                if (Input.GetMouseButtonDown(i))
                {
                    mousePos[i] = Input.mousePosition;
                    var ui = UICheck(mousePos[i]);
#if UNITY_EDITOR
                    Z_Log.Log(ui);
#endif
                    Z_EventHelper.Invoke(new InputMouseDownEvent()
                    {
                        id = i,
                        pos = Input.mousePosition,
                        ui = ui
                    });
                    tmpHash.Add(i);

                }
            }

            for (int i = 0; i <= 1; i++)
            {
                if (Input.GetMouseButton(i) && !tmpHash.Contains(i) && mouseOldPos.ContainsKey(i))//ignore first frame
                {
                    mousePos[i] = Input.mousePosition;
                    Z_EventHelper.Invoke(new InputMouseEvent()
                    {
                        id = i,
                        pos = mousePos[i],
                        delta = mousePos[i] - mouseOldPos[i],
                        ui = UICheck(Input.mousePosition)
                    });

                }
            }
            //scroll
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0)
            {
                Z_EventHelper.Invoke(new InputMouseScrollEvent()
                {
                    delta = scroll
                });
            }
            if ((lastMousePos - Input.mousePosition).sqrMagnitude > 0.0001f)
            {
                lastMousePos = Input.mousePosition;
                Z_EventHelper.Invoke(new InputMouseMoveEvent()
                {
                    pos = lastMousePos
                });
            }

            //save
            id2Pos.Clear();
            id2Pos[0] = Input.mousePosition;
        }

        /// <summary>
        /// if ui hover
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        private GameObject UICheck(Vector2 point)
        {
            if (EventSystem.current == null)
                return null;
            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                position = point
            };

            var results = new System.Collections.Generic.List<RaycastResult>();

            EventSystem.current.RaycastAll(pointerData, results);

            foreach (var result in results)
            {
                if (result.gameObject.layer == 5)
                {
                    return result.gameObject;
                }
            }

            return null;
        }
        public int GetClosedId(Vector2 pos)
        {
            float min = float.MaxValue;
            int id = 0;
            foreach (var pair in id2Pos)
            {
                var dis2 = (pair.Value - pos).sqrMagnitude;
                if (dis2 < min)
                {
                    min = dis2;
                    id = pair.Key;
                }
            }
            return id;
        }
        public void Update()
        {
            if (!enabled)
            {
#if UNITY_ANDROID && !UNITY_EDITOR
                ClearTouchState();
#endif
                return;
            }

#if UNITY_ANDROID && !UNITY_EDITOR
            if (!hasFocus || paused)
                return;
            ManageTouch();
#else

            ManageMouse();
            keyLst.Clear();

            if (Input.GetKeyDown(KeyCode.W))
            {
                keyLst.Add(KeyCode.W);
            }
            if (Input.GetKeyDown(KeyCode.S))
            {

                keyLst.Add(KeyCode.S);
            }
            if (Input.GetKeyDown(KeyCode.A))
            {

                keyLst.Add(KeyCode.A);
            }
            if (Input.GetKeyDown(KeyCode.D))
            {
                keyLst.Add(KeyCode.D);
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                keyLst.Add(KeyCode.E);
            }
            if (keyLst.Count > 0)
            {
                Z_EventHelper.Invoke(new InputKeyDownEvent()
                {
                    key = keyLst
                });
            }

            keyLst.Clear();
            if (Input.GetKey(KeyCode.W))
            {
                keyLst.Add(KeyCode.W);
            }
            if (Input.GetKey(KeyCode.S))
            {
                keyLst.Add(KeyCode.S);
            }
            if (Input.GetKey(KeyCode.A))
            {
                keyLst.Add(KeyCode.A);
            }
            if (Input.GetKey(KeyCode.D))
            {
                keyLst.Add(KeyCode.D);
            }
            if (keyLst.Count > 0)
            {
                Z_EventHelper.Invoke(new InputKeyEvent()
                {
                    key = keyLst
                });
            }

            keyLst.Clear();
            if (Input.GetKeyUp(KeyCode.Alpha1))
            {
                keyLst.Add(KeyCode.Alpha1);
            }
            if (Input.GetKeyUp(KeyCode.Alpha2))
            {
                keyLst.Add(KeyCode.Alpha2);
            }
            if (Input.GetKeyUp(KeyCode.Alpha3))
            {
                keyLst.Add(KeyCode.Alpha3);
            }
            if (Input.GetKeyUp(KeyCode.Alpha4))
            {
                keyLst.Add(KeyCode.Alpha4);
            }
            if (Input.GetKeyUp(KeyCode.W))
            {
                keyLst.Add(KeyCode.W);
            }
            if (Input.GetKeyUp(KeyCode.S))
            {
                keyLst.Add(KeyCode.S);
            }
            if (Input.GetKeyUp(KeyCode.A))
            {
                keyLst.Add(KeyCode.A);
            }
            if (Input.GetKeyUp(KeyCode.D))
            {
                keyLst.Add(KeyCode.D);
            }
            if (Input.GetKeyUp(KeyCode.E))
            {
                keyLst.Add(KeyCode.E);
            }
            if (Input.GetKeyUp(KeyCode.Q))
            {
                keyLst.Add(KeyCode.Q);
            }
            if (keyLst.Count > 0)
            {
                Z_EventHelper.Invoke(new InputKeyUpEvent()
                {
                    key = keyLst
                });
            }

#endif

        }

    }
}
