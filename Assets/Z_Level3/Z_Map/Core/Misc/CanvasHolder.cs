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
            foreach (var obj in textDic.Values)
            {
                Destroy(obj.gameObject);
            }
            foreach (var obj in sliderDic.Values)
            {
                Destroy(obj.gameObject);
            }
            foreach (var obj in sliderTextDic.Values)
            {
                Destroy(obj.gameObject);
            }
            textDic.Clear();
            sliderDic.Clear();
            sliderTextDic.Clear();

        }
        public void ShowText(string text, int key = -1)
        {
            if (!textDic.ContainsKey(key))
            {
                textDic[key] = Instantiate(textProto.gameObject, textProto.transform.parent).GetComponent<TextMeshProUGUI>();
                textDic[key].gameObject.SetActive(true);
            }
            textDic[key].text = text;
        }
        public void ShowSlider(float v, float max, int key = -1)
        {
            if (!sliderDic.ContainsKey(key))
            {
                sliderDic[key] = Instantiate(sliderProto.gameObject, sliderProto.transform.parent).GetComponent<Slider>();
                sliderTextDic[sliderDic[key]] = sliderDic[key].transform.GetComponentInChildren<TextMeshProUGUI>();
                sliderDic[key].gameObject.SetActive(true);
            }
            sliderDic[key].value = v / max;
            sliderTextDic[sliderDic[key]].text = $"{v}/{max}";
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
        public void ToastText(string text, float lastTime)
        {
            GameObject tmp = Instantiate(toastTextProto.gameObject, toastTextProto.transform.parent);
            tmp.SetActive(true);
            var textTmp = tmp.GetComponent<TextMeshProUGUI>();
            textTmp.text = text;
            var oriPos = trs.position;
            var tarPos = trs.position + Vector3.up + Vector3.forward;
            tmp.transform.position = oriPos;
            float timeCur = 0;
            TimeManager.instance.StartTimer(0, 0.0001f, () =>
            {
                timeCur += Time.deltaTime;
                tmp.transform.position = Vector3.Lerp(oriPos, tarPos, timeCur * 2 / lastTime);
                if (timeCur > lastTime)
                {
                    Destroy(tmp.gameObject);
                    return true;
                }
                return false;
            }, this);
        }

    }
}
