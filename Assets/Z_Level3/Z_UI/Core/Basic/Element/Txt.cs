using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Z_Text;
using Z_DataSystem;
using Z_DataSystem.Form;
using Z_DesignStyle;
using static System.Net.Mime.MediaTypeNames;
using Z_Time;
using System.Xml;







#if UNITY_EDITOR
using UnityEditor;
#endif
namespace Z_Ui.Base
{
    public class Txt : TextMeshProUGUI, ITextPreprocessor
    {
        private string _oriTxt;
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


        private List<Sprite> sprites = new List<Sprite>();
        private List<Img> images = new List<Img>();
        private Timer timer;

        public string PreprocessText(string text)
        {
            return text;
        }

        protected override void Awake()
        {
#if UNITY_EDITOR
            if (EditorApplication.isPlaying)
            {
                oriText = text;
            }
#endif
            base.Awake();
        }
        protected override void OnEnable()
        {
#if UNITY_EDITOR
            if (EditorApplication.isPlaying)
            {
                Refresh();
            }
#else
            Refresh();
#endif

            base.OnEnable();
        }

        public void Refresh()
        {
            text = oriText;
            if (languageTranslatable)
            {
                text = TextManager.instance.GetTxt(oriText);
            }
            if (imageEnable)
            {
                var parts = text.Split(AssetManager.instance.texCtrl.GetMark());
                text = "";
                sprites.Clear();
                int i = 1;
                for (; i < parts.Length; i += 2)
                {
                    sprites.Add(TexAssetForm.DataByName.GetDv(AssetManager.instance.texCtrl.GetName(parts[i]), null)?.GetSprite());
                    text += parts[i - 1];
                    text += AssetManager.instance.texCtrl.GetMark();
                }
                text += parts[i - 1];
                var curText = text;
                TimeManager.instance.CancelTimer(timer);
                timer = TimeManager.instance.StartTimer(0, 0, () =>
                {
                    if (curText != text || textInfo.characterCount != text.Length)
                        return false;

                    var parts = text.Split(AssetManager.instance.texCtrl.GetMark());
                    int cnt = 0;
                    foreach (var img in images)
                    {
                        img.gameObject.SetActive(false);
                    }

                    for (int i = 0; i < parts.Length - 1; i++, cnt += 3)
                    {
                        cnt += parts[i].Length;
                        if (images.Count <= i)
                        {
                            var go = new GameObject("icon");
                            var img = go.AddComponent<Img>();
                            go.transform.parent = transform;
                            images.Add(img);
                        }
                        var info = textInfo.characterInfo[cnt + 1];
                        images[i].transform.localPosition = (info.topLeft + info.bottomRight) / 2;

                        var size = (info.bottomRight.x - info.topLeft.x) * 7;
                        var rect = images[i].rectTransform.sizeDelta = new Vector2(size, size);

                        images[i].sprite = sprites[i];
                        images[i].gameObject.SetActive(true);
                    }
                    return true;
                }, this);
            }
        }
    }
}
