using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_ObjectAnimator.Core;
namespace Z_ObjectAnimator.Base
{
   
    public class PositionSetEvent : Event
    {
        private Vector3 _pos;
        private Transform _transform;
        private Vector3 _posOri;
        private bool _sqr;

        public PositionSetEvent(Transform transform, Vector3 posOri, Vector3 pos, float duration=0,bool sqr=true):base(duration)
        {
            _pos = pos;
            _transform = transform;
            _posOri = posOri;
            _sqr = sqr;
        }

        public override ReturnValue Excute()
        {
            if(_sqr)
            {
                _transform.position = Vector3.Lerp(_posOri, _pos, 1f * ((_times + 1)* (_times + 1)) / _maxTimes/ _maxTimes);
            }
            else
            {
                _transform.position = Vector3.Lerp(_posOri, _pos, 1f * (_times + 1) / _maxTimes);
            }
            return base.Excute();
        }
        public override void ToEnd()
        {
            base.ToEnd();
            _transform.position = _pos;
        }
        public override void ToStart()
        {
            base.ToStart();
            _transform.position = _posOri;
        }
    }
}