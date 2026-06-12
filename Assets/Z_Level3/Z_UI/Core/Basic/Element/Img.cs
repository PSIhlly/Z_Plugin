using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Z_DataSystem;
using Z_DataSystem.Form;
using Z_Language;
using Z_Texture;
using Z_Time;

namespace Z_Ui.Base
{
    public class Img : Image
    {
        [SerializeField]
        public Sprite cn;
        public Sprite en;

        private TexAssetForm.Data _texData;
        private Timer _gifTimer;
        private int _gifFrameIndex;
        private List<Sprite> _gifSprites;

        public void BindTexData(TexAssetForm.Data data)
        {
            UnbindTexData();
            _texData = data;
            if (data == null)
            {
                sprite = null;
                return;
            }
            if (data.isGif)
            {
                _gifSprites = data.GetGifSprites();
                if (_gifSprites == null || _gifSprites.Count <= 1)
                {
                    sprite = data.GetSprite();
                    return;
                }
                var frames = data.GetGifFrames();
                _gifFrameIndex = 0;
                sprite = _gifSprites[0];
                _gifTimer = TimeManager.instance.StartTimerImmediate(0, frames[0].delaySeconds, () =>
                {
                    _gifFrameIndex = (_gifFrameIndex + 1) % _gifSprites.Count;
                    if (this == null || gameObject == null)
                        return true;
                    sprite = _gifSprites[_gifFrameIndex];
                    return false;
                }, this);
            }
            else
            {
                sprite = data.GetSprite();
            }
        }

        public void UnbindTexData()
        {
            if (_gifTimer != null)
            {
                TimeManager.instance.CancelTimer(_gifTimer);
                _gifTimer = null;
            }
            _texData = null;
            _gifSprites = null;
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
        }
    }
}
