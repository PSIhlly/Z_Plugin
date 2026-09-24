using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Z_DataSystem.Form;
using Z_DesignStyle;
using Z_Time;
using Z_Ui;
using Z_Ui.Base;

namespace Z_Map
{
    public class CanvasHolder : MonoBehaviour
    {

        private Transform _trs;
        public TextMeshProUGUI textProto;
        public TextMeshProUGUI toastTextProto;
        public Slider sliderProto;
        public Timer chatTimer;

        public GameObject chat;
        public Img chatImg;
        public TextMeshProUGUI chatText;

        private Dictionary<int, TextMeshProUGUI> textDic = new Dictionary<int, TextMeshProUGUI>();
        private Dictionary<int, Slider> sliderDic = new Dictionary<int, Slider>();
        private Dictionary<Slider, TextMeshProUGUI> sliderTextDic = new Dictionary<Slider, TextMeshProUGUI>();
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
        public void Reset()
        {
            if (chatTimer != null)
                TimeManager.instance.CancelTimer(chatTimer);
            chatTimer = null;
            if (chat != null)
                chat.SetActive(false);

            foreach (var obj in textDic.Values)
            {
                if (obj != null)
                    Destroy(obj.gameObject);
            }
            foreach (var obj in sliderDic.Values)
            {
                if (obj != null)
                    Destroy(obj.gameObject);
            }
            textDic.Clear();
            sliderDic.Clear();
            sliderTextDic.Clear();

        }
        public void ShowText(string text, int key = -1)
        {
            if (!textDic.TryGetValue(key, out var textObject) || textObject == null)
            {
                textObject = Instantiate(textProto.gameObject, textProto.transform.parent).GetComponent<TextMeshProUGUI>();
                textDic[key] = textObject;
                textObject.gameObject.SetActive(true);
            }
            textObject.text = text;
        }
        public void ShowSlider(float v, float max, int key = -1)
        {
            if (!sliderDic.TryGetValue(key, out var slider) || slider == null)
            {
                slider = Instantiate(sliderProto.gameObject, sliderProto.transform.parent).GetComponent<Slider>();
                sliderDic[key] = slider;
                sliderTextDic[slider] = slider.transform.GetComponentInChildren<TextMeshProUGUI>();
                slider.gameObject.SetActive(true);
            }
            slider.value = max == 0 ? 0 : v / max;
            if (sliderTextDic.TryGetValue(slider, out var sliderText) && sliderText != null)
                sliderText.text = $"{v}/{max}";
        }
        public void Chat(string text, int img, float lastTime)
        {
            chat.SetActive(true);
            chatText.text = text;
            var data = TexAssetForm.DataById.GetDv(img, null);
            bool imgValid = data != null && data.id != GlobalDefaultHelper.ExternDefaultTexId;
            chatImg.gameObject.SetActive(imgValid);
            if (imgValid)
            {
                chatImg.BindTexData(data);
            }

            float timeCur = 0;
            TimeManager.instance.CancelTimer(chatTimer);
            chatTimer = TimeManager.instance.StartTimer(0, 0.0001f, () =>
            {
                timeCur += Time.deltaTime;
                if (timeCur > lastTime)
                {
                    chat.SetActive(false);
                    return true;
                }
                return false;
            }, this);
            UiManager.Rebuild(chat.gameObject);
        }
        // 飘字缓动指数：数值越大，前半段越快、后半段越慢
        private const float toastEasePower = 3f;
        // 渐隐起始进度：0.5 表示后半段开始渐隐
        private const float toastFadeBegin = 0.5f;
        // 飘字速度倍率：2 表示位移用时减半
        private const float toastSpeedScale = 2f;
        // 到达最高点后的停留时间
        private const float toastHoldTime = 0.5f;
        // 出现时的缩放时长：必须明显长于渐显时长，
        // 否则文字完全显形时缩放已经结束，缩放看起来就像没生效
        private const float toastScaleInTime = 0.35f;
        // 缩放缓动指数：2 比 3 更均匀，2 倍到 1 倍的过程更看得清
        private const float toastScaleEasePower = 2f;
        // 出现时的渐显时长
        private const float toastFadeInTime = 0.15f;
        // 出现时的起始缩放倍率，随后回到 1 倍
        private const float toastStartScale = 2f;
        // 世界坐标 z 轴负方向偏移量（累计值：0.2 + 再偏 0.2）
        private const float toastZOffset = 0.4f;
        // 飘字上升距离倍率：1 为原始距离(Vector3.up + Vector3.forward)
        // 当前 0.75 = 上一版 0.5 的 1.5 倍高度
        private const float toastRiseScale = 0.75f;
        public void ToastText(string text, float lastTime)
        {
            GameObject tmp = Instantiate(toastTextProto.gameObject, toastTextProto.transform.parent);
            tmp.SetActive(true);
            var textTmp = tmp.GetComponent<TextMeshProUGUI>();
            textTmp.text = text;
            var oriColor = textTmp.color;
            var oriScale = tmp.transform.localScale;
            // 起点向世界坐标 z 轴负方向偏一点点，终点随之整体偏移
            var oriPos = trs.position - Vector3.forward * toastZOffset;
            var tarPos = oriPos + (Vector3.up + Vector3.forward) * Mathf.Max(0f, toastRiseScale);
            tmp.transform.position = oriPos;

            // 位移用时 = lastTime / 速度倍率，之后在最高点停留一段
            float moveTime = Mathf.Max(0.0001f, lastTime / Mathf.Max(0.0001f, toastSpeedScale));
            float totalTime = moveTime + Mathf.Max(0f, toastHoldTime);
            float timeCur = 0;
            TimeManager.instance.StartTimer(0, 0.0001f, () =>
            {
                timeCur += Time.deltaTime;

                // 出现：2 倍缩放到 1 倍（缩放时长长于渐显，显形后仍能看到缩小过程）
                float scaleIn = toastScaleInTime > 0 ? Mathf.Clamp01(timeCur / toastScaleInTime) : 1f;
                float scaleEase = 1 - Mathf.Pow(1 - scaleIn, toastScaleEasePower);
                tmp.transform.localScale = oriScale * Mathf.Lerp(toastStartScale, 1f, scaleEase);

                // 位移：先快后慢，moveTime 内到达最高点，随后停在最高点
                float move = Mathf.Clamp01(timeCur / moveTime);
                float ease = 1 - Mathf.Pow(1 - move, toastEasePower);
                tmp.transform.position = Vector3.Lerp(oriPos, tarPos, ease);

                // 透明度：渐显 + 后半段渐隐，销毁时刚好完全透明
                float fadeIn = toastFadeInTime > 0 ? Mathf.Clamp01(timeCur / toastFadeInTime) : 1f;
                float fade = Mathf.Clamp01((timeCur / totalTime - toastFadeBegin) / (1 - toastFadeBegin));
                textTmp.color = new Color(oriColor.r, oriColor.g, oriColor.b, oriColor.a * fadeIn * (1 - fade));

                if (timeCur > totalTime)
                {
                    Destroy(tmp.gameObject);
                    return true;
                }
                return false;
            }, this);
        }

    }
}
