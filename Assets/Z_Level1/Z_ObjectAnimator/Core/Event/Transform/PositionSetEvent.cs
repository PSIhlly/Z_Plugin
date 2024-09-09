using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_ObjectAnimator.Core;
namespace Z_ObjectAnimator.Event
{
   
    public class PositionSetEvent : Event
    {
        [SerializeField]
        private Vector3 _pos;

        private Transform _transform;

        public PositionSetEvent(Transform transform,Vector3 pos)
        {
            _pos = pos;
            _transform = transform;
        }
        
        public override ReturnValue Excute()
        {
            _transform.position = _pos;
            return new ReturnValue();
        }
    }
}