using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.EventSystems;
using Z_DesignStyle;
namespace Z_Input
{

    public class InputConfig
    {
        public Action onButtonDownW;
        public Action onButtonDownS;
        public Action onButtonDownA;
        public Action onButtonDownD;

        public Action onButtonDownE;

        public Action onButtonW;
        public Action onButtonS;
        public Action onButtonA;
        public Action onButtonD;

        public Action onButtonUpW;
        public Action onButtonUpS;
        public Action onButtonUpA;
        public Action onButtonUpD;

        public Action<int, Vector3, GameObject> onPointDown;
        public Action<int, Vector3, Vector3, GameObject> onPoint;
        public Action<int, Vector3, GameObject> onPointUp;

        public Action<int, Vector3,GameObject> onMouseDown;
        public Action<int, Vector3, Vector3, GameObject> onMouse;
        public Action<int, Vector3, GameObject> onMouseUp;
        public Action<Vector3> onMouseMove;

        public Action<float> onMouseScroll;

    }
    [DefaultExecutionOrder(-100)]
    public class InputManager : Z_MonoManager<InputManager>
    {
        public bool enabled = true;
        public InputConfig cur;
        public Vector2 screenSize;
        public Vector2 screenWorldSize;


        public Dictionary<int, Vector2> mousePos = new Dictionary<int, Vector2>();
        public Dictionary<int, Vector2> mouseOldPos = new Dictionary<int, Vector2>();

        private Vector3 lastMousePos;

        public Dictionary<int, Vector2> id2Pos = new Dictionary<int, Vector2>();
        public Dictionary<int, Vector2> id2OldPos = new Dictionary<int, Vector2>();

        private Dictionary<int, int> touchId2id = new Dictionary<int, int>();
        private List<int> tmpList = new List<int>();
        private HashSet<int> tmpHash = new HashSet<int>();
        private int pointCnt;

        public override void Init()
        {
            screenSize = new Vector2(Screen.width, Screen.height);
            float orthographicSize = Camera.main.orthographicSize;
            float aspect = Camera.main.aspect;
            screenWorldSize = new Vector2(orthographicSize * aspect, orthographicSize);
        }
        public void Register(InputConfig config)
        {
            lastMousePos = Vector3.zero;
            cur = config;
        }


        private void ManageTouch()
        {
            id2OldPos.Clear();
            foreach (var point in id2Pos)
            {
                id2OldPos[point.Key] = point.Value;
            }


            touchId2id.Clear();
            tmpHash.Clear();
            for (int i = 0; i < Input.touchCount; i++)
            {
                var closetId = -1;
                var min = float.MaxValue;
                //find Closet
                foreach (var point in id2Pos)
                {
                    if (tmpHash.Contains(point.Key))
                        continue;

                    var length2 = (Input.touches[i].position - point.Value).sqrMagnitude;
                    if (length2 < min)
                    {
                        closetId = point.Key;
                        min = length2;
                    }

                }
                touchId2id[i] = closetId;
                tmpHash.Add(closetId);
            }


            tmpList.Clear();
            //less
            foreach (var point in id2Pos)
            {
                if (!touchId2id.ContainsValue(point.Key))
                {
                    tmpList.Add(point.Key);
                }
            }
            foreach (var k in tmpList)
            {
                cur?.onPointUp?.Invoke(k, id2Pos[k], UICheck(id2Pos[k]));

                id2Pos.Remove(k);
            }

            

            for (int i = 0; i < Input.touchCount; i++)
            {
                //more
                int id = touchId2id[i];
                if (id == -1)
                {
                    pointCnt = (pointCnt + 1) % 100;
                    id2Pos[pointCnt] = Input.touches[i].position;
                    cur?.onPointDown?.Invoke(pointCnt, Input.touches[i].position, UICheck(Input.touches[i].position));
                }
            }


            //keep
            for (int i = 0; i < Input.touchCount; i++)
            {
                int id = touchId2id[i];
                if (id != -1)
                {
                    id2Pos[id] = Input.touches[i].position;
                    cur?.onPoint?.Invoke(id, id2Pos[id], id2Pos[id] - id2OldPos[id], UICheck(id2Pos[id]));
                }
            }

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
                    cur?.onMouseUp?.Invoke(i, Input.mousePosition, UICheck(Input.mousePosition));
                    tmpHash.Add(i);
                }
            }

            for (int i = 0; i <= 1; i++)
            {
                if (Input.GetMouseButtonDown(i))
                {
                    mousePos[i] = Input.mousePosition;
                    cur?.onMouseDown?.Invoke(i, Input.mousePosition,UICheck(mousePos[i]));
                    tmpHash.Add(i);

                }
            }

            for (int i = 0; i <= 1; i++)
            {
                if (Input.GetMouseButton(i)&&!tmpHash.Contains(i)&& mouseOldPos.ContainsKey(i))//ignore first frame
                {
                    mousePos[i] = Input.mousePosition;
                    cur?.onMouse?.Invoke(i, mousePos[i], mousePos[i] - mouseOldPos[i], UICheck(mousePos[i]));

                }
            }
            //scroll
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll!=0)
            {
                cur?.onMouseScroll?.Invoke(scroll);
            }
            if((lastMousePos- Input.mousePosition).sqrMagnitude>0.0001f)
            {
                lastMousePos = Input.mousePosition;
                cur?.onMouseMove?.Invoke(lastMousePos);
            }

        }

        /// <summary>
        /// if ui hover
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        private GameObject UICheck(Vector2 point)
        {
            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                position = point
            };

            var results = new System.Collections.Generic.List<RaycastResult>();

            EventSystem.current.RaycastAll(pointerData, results);

            foreach(var result in results)
            {
                if(result.gameObject.layer == 5)
                {
                    return result.gameObject;
                }
            }

            return null;
        }

        public void Update()
        {
            if (!enabled)
                return;

#if UNITY_ANDROID && !UNITY_EDITOR

            ManagePoint();
#else

            ManageMouse();
            if (Input.GetKeyDown(KeyCode.W))
            {
                cur?.onButtonDownW?.Invoke();
            }
            if (Input.GetKeyDown(KeyCode.S))
            {
                cur?.onButtonDownS?.Invoke();
            }
            if (Input.GetKeyDown(KeyCode.A))
            {
                cur?.onButtonDownA?.Invoke();
            }
            if (Input.GetKeyDown(KeyCode.D))
            {
                cur?.onButtonDownD?.Invoke();
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                cur?.onButtonDownE?.Invoke();
            }

            if (Input.GetKey(KeyCode.W))
            {
                cur?.onButtonW?.Invoke();
            }
            if (Input.GetKey(KeyCode.S))
            {
                cur?.onButtonS?.Invoke();
            }
            if (Input.GetKey(KeyCode.A))
            {
                cur?.onButtonA?.Invoke();
            }
            if (Input.GetKey(KeyCode.D))
            {
                cur?.onButtonD?.Invoke();
            }


            if (Input.GetKeyUp(KeyCode.W))
            {
                cur?.onButtonUpW?.Invoke();
            }
            if (Input.GetKeyUp(KeyCode.S))
            {
                cur?.onButtonUpS?.Invoke();
            }
            if (Input.GetKeyUp(KeyCode.A))
            {
                cur?.onButtonUpA?.Invoke();
            }
            if (Input.GetKeyUp(KeyCode.D))
            {
                cur?.onButtonUpD?.Invoke();
            }

            
#endif

        }

    }
}
