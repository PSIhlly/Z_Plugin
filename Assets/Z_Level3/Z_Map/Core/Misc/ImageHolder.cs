using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Time;

namespace Z_Map
{
    public class ImageHolder : MonoBehaviour
    {
        public Vector3 oriPos;
        public float oriRot;
        public Vector3 oriScale;

        private Transform _trs;
        public Transform trs
        {
            get
            {
                if (_trs == null)
                {
                    _trs = transform;
                }
                return _trs;
            }
        }
        private Renderer _render;
        public Renderer render
        {
            get
            {
                if (_render == null)
                {
                    _render = GetComponentInChildren<Renderer>();
                }
                return _render;
            }
        }
        public Timer animTimer;
    }
}
