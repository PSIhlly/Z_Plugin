using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_ObjectAnimator.Core;
namespace Z_ObjectAnimator.Event
{
   
    public class LocalPositionSetEvent : Event
    {
        [SerializeField]
        private Vector3 _pos;

        private Transform _transform;

        public LocalPositionSetEvent(Transform transform,Vector3 pos)
        {
            _pos = pos;
            _transform = transform;
        }
        
        public override ReturnValue Excute()
        {
            _transform.localPosition = _pos;
            return new ReturnValue();
        }
    }
}