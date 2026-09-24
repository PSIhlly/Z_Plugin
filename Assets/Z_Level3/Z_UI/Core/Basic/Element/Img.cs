using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Z_DataSystem;
using Z_DataSystem.Form;
using Z_Language;
using Z_Texture;

namespace Z_Ui.Base
{
    public class Img : Image
    {
        [SerializeField]
        public Sprite cn;
        public Sprite en;

        private TexAssetForm.Data _texData;
        private Coroutine _animationCoroutine;
        private int _animationFrameIndex;
        private IReadOnlyList<AnimatedFrameData> _animationFrames;
        private IReadOnlyList<Sprite> _animationSprites;

        public void BindTexData(TexAssetForm.Data data)
        {
            UnbindTexData();
            _texData = data;
            if (data == null)
            {
                sprite = null;
                return;
            }
            if (data.isGif || data.isWebP)
            {
                _animationFrames = data.GetAnimationFrames();
                _animationSprites = data.GetAnimationSprites();
                if (_animationFrames == null || _animationSprites == null || _animationSprites.Count == 0)
                {
                    sprite = null;
                    return;
                }
                _animationFrameIndex = 0;
                sprite = _animationSprites[0];
                StartAnimation();
            }
            else
            {
                sprite = data.GetSprite();
            }
        }

        public void UnbindTexData()
        {
            StopAnimation();
            _texData = null;
            _animationFrames = null;
            _animationSprites = null;
        }

        private void StartAnimation()
        {
            if (_animationCoroutine == null && isActiveAndEnabled && _animationSprites != null && _animationSprites.Count > 1)
                _animationCoroutine = StartCoroutine(PlayAnimation());
        }

        private void StopAnimation()
        {
            if (_animationCoroutine == null)
                return;
            StopCoroutine(_animationCoroutine);
            _animationCoroutine = null;
        }

        private IEnumerator PlayAnimation()
        {
            while (_animationFrames != null && _animationSprites != null)
            {
                yield return new WaitForSeconds(Mathf.Max(0.02f, _animationFrames[_animationFrameIndex].delaySeconds));
                if (_animationFrames == null || _animationSprites == null)
                    break;
                _animationFrameIndex = (_animationFrameIndex + 1) % _animationSprites.Count;
                sprite = _animationSprites[_animationFrameIndex];
            }
            _animationCoroutine = null;
        }

        protected override void OnDestroy()
        {
            UnbindTexData();
            base.OnDestroy();
        }

        protected override void OnEnable()
        {
            switch (LanguageManager.instance.language)
            {
                case Language.Cn:
                    if (cn != null)
                    {
                        sprite = cn;
                    }
                    break;
                default:
                    if (en != null)
                    {
                        sprite = en;
                    }
                    break;
            }
            base.OnEnable();
            if (_animationSprites != null && _animationSprites.Count > 0)
                sprite = _animationSprites[_animationFrameIndex];
            StartAnimation();
        }

        protected override void OnDisable()
        {
            StopAnimation();
            base.OnDisable();
        }
    }
}
