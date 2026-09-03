using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Z_Text;
using Z_DataSystem;
using Z_DataSystem.Form;
using Z_DesignStyle;
using static System.Net.Mime.MediaTypeNames;







#if UNITY_EDITOR
using UnityEditor;
#endif
namespace Z_Ui.Base
{
    public class Txt : TextMeshProUGUI, ITextPreprocessor
    {
        private string _oriTxt;
        private bool suppressImageRefresh;
        public string oriText
        {
            get
            {
                return _oriTxt;
            }
            set
            {
                _oriTxt = value;

                Refresh();

            }
        }
        [Tooltip("use language manage")]
        public bool languageTranslatable = true;
        [Tooltip("use image")]
        public bool imageEnable = false;

        public override string text
        {
            get => base.text;
            set
            {
                if (suppressImageRefresh)
                {
                    base.text = value;
                    return;
                }

                if (imageEnable)
                {
                    RefreshImageContent(value);
                }
                else
                {
                    HideImages();
                    sprites.Clear();
                    SetTextInternal(value);
                }
            }
        }


        private List<Sprite> sprites = new List<Sprite>();
        private List<Img> images = new List<Img>();
        private readonly List<ImageLayout> imageLayouts = new List<ImageLayout>();
        private bool imageLayoutPending;

        private struct ImageLayout
        {
            public bool visible;
            public Vector3 localPosition;
            public Vector2 size;
        }

        public string PreprocessText(string text)
        {
            return text;
        }

        protected override void Awake()
        {
#if UNITY_EDITOR
            if (EditorApplication.isPlaying&&string.IsNullOrEmpty(oriText))
            {
                oriText = text;
            }
#else
            if (string.IsNullOrEmpty(oriText))
            {
                oriText = text;
            }
#endif
            base.Awake();
            OnPreRenderText += RenderImages;
        }
        protected override void OnEnable()
        {
            base.OnEnable();
#if UNITY_EDITOR
            if (EditorApplication.isPlaying)
            {
                Refresh();
            }
#else
            Refresh();
#endif
        }

        protected override void OnDisable()
        {
            HideImages();
            base.OnDisable();
        }

        public void Refresh()
        {
            HideImages();
            sprites.Clear();

            var sourceText = oriText;
            if (languageTranslatable)
            {
                sourceText = TextManager.instance.GetTxt(oriText);
            }

            if (imageEnable)
            {
                RefreshImageContent(sourceText);
            }
            else
            {
                SetTextInternal(sourceText);
            }
        }

        private void RefreshImageContent(string sourceText)
        {
            HideImages();
            sprites.Clear();

            if (!imageEnable)
                return;

            var mark = AssetManager.instance.texCtrl.GetMark();
            if (string.IsNullOrEmpty(mark))
            {
                SetTextInternal(sourceText);
                return;
            }

            var parts = (sourceText ?? string.Empty).Split(new[] { mark }, StringSplitOptions.None);
            var displayText = new StringBuilder(sourceText?.Length ?? 0);
            for (int i = 0; i < parts.Length; i++)
            {
                displayText.Append(parts[i]);
                if ((i & 1) != 0 || i + 1 >= parts.Length)
                    continue;

                var assetId = int.TryParse(parts[++i], out var id) ? id : 0;
                sprites.Add(TexAssetForm.DataById.GetDk(assetId, GlobalDefaultHelper.DefaultTexId)?.GetSprite());
                displayText.Append(mark);
            }

            SetTextInternal(displayText.ToString());
            ForceMeshUpdate();
        }

        private void LateUpdate()
        {
            if (imageLayoutPending && !IsCanvasRebuilding())
                ApplyImageLayout();
        }

        private static bool IsCanvasRebuilding()
        {
            return CanvasUpdateRegistry.IsRebuildingGraphics() || CanvasUpdateRegistry.IsRebuildingLayout();
        }

        private void SetTextInternal(string value)
        {
            suppressImageRefresh = true;
            base.text = value;
            suppressImageRefresh = false;
        }

        private void RenderImages(TMP_TextInfo info)
        {
            imageLayouts.Clear();
            for (int i = 0; i < sprites.Count; i++)
                imageLayouts.Add(default);

            if (imageEnable && sprites.Count > 0 && info != null)
            {
                var mark = AssetManager.instance.texCtrl.GetMark();
                if (!string.IsNullOrEmpty(mark))
                {
                    var parts = (base.text ?? string.Empty).Split(new[] { mark }, StringSplitOptions.None);
                    var markerCount = Mathf.Min(sprites.Count, parts.Length - 1);
                    var characterOffset = 0;
                    for (int i = 0; i < markerCount; i++)
                    {
                        var characterIndex = characterOffset + parts[i].Length + mark.Length / 2;
                        characterOffset += parts[i].Length + mark.Length;

                        if (sprites[i] == null || characterIndex < 0 ||
                            characterIndex >= info.characterCount ||
                            characterIndex >= info.characterInfo.Length)
                            continue;

                        var character = info.characterInfo[characterIndex];
                        if (!character.isVisible)
                            continue;

                        imageLayouts[i] = new ImageLayout
                        {
                            visible = true,
                            localPosition = (character.topLeft + character.bottomRight) / 2,
                            size = Vector2.one * (Mathf.Abs(character.bottomRight.x - character.topLeft.x) * 7)
                        };
                    }
                }
            }

            imageLayoutPending = true;
            if (!IsCanvasRebuilding())
                ApplyImageLayout();
        }

        private void ApplyImageLayout()
        {
            if (IsCanvasRebuilding())
                return;

            imageLayoutPending = false;
            for (int i = 0; i < images.Count; i++)
            {
                var visible = i < imageLayouts.Count && imageLayouts[i].visible && i < sprites.Count && sprites[i] != null;
                if (!visible && images[i] != null && images[i].gameObject.activeSelf)
                    images[i].gameObject.SetActive(false);
            }

            for (int i = 0; i < imageLayouts.Count && i < sprites.Count; i++)
            {
                var layout = imageLayouts[i];
                if (!layout.visible || sprites[i] == null)
                    continue;

                var img = GetImage(i);
                if (img.sprite != sprites[i])
                    img.sprite = sprites[i];
                if (img.transform.localPosition != layout.localPosition)
                    img.transform.localPosition = layout.localPosition;
                if (img.rectTransform.sizeDelta != layout.size)
                    img.rectTransform.sizeDelta = layout.size;

                if (!img.gameObject.activeSelf)
                    img.gameObject.SetActive(true);
            }
        }

        private Img GetImage(int index)
        {
            while (images.Count <= index)
            {
                var go = new GameObject("icon");
                go.SetActive(false);
                var img = go.AddComponent<Img>();
                img.raycastTarget = false;
                go.transform.SetParent(transform, false);
                images.Add(img);
            }

            if (images[index] == null)
            {
                var go = new GameObject("icon");
                go.SetActive(false);
                var img = go.AddComponent<Img>();
                img.raycastTarget = false;
                go.transform.SetParent(transform, false);
                images[index] = img;
            }

            return images[index];
        }

        private void HideImages()
        {
            imageLayouts.Clear();
            imageLayoutPending = false;
            foreach (var img in images)
            {
                if (img != null && img.gameObject != null)
                {
                    img.sprite = null;
                    img.gameObject.SetActive(false);
                }
            }
        }
    }
}
