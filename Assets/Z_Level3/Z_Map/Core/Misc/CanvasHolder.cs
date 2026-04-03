using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Z_Time;

namespace Z_Map
{
    public class CanvasHolder : MonoBehaviour
    {
        private Transform _trs;
        public TextMeshProUGUI textProto;
        public TextMeshProUGUI toastTextProto;
        public Slider sliderProto;

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
        public void ShowText(string text, int key = -1)
        {
            if (!textDic.ContainsKey(key))
            {
                textDic[key] = Instantiate(textProto.gameObject, textProto.transform.parent).GetComponent<TextMeshProUGUI>();
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
        public void ToastText(string text, float lastTime)
        {
            var tmp = Instantiate(toastTextProto, toastTextProto.transform.parent).GetComponent<TextMeshProUGUI>();
            tmp.text = text;
            var oriPos = trs.position + Vector3.down / 2 + Vector3.back / 2;
            var tarPos = trs.position + Vector3.up / 2 + Vector3.forward / 2;
            tmp.transform.position = oriPos;
            float timeCur = 0;
            TimeManager.instance.StartTimer(0, 0.0001f, () =>
            {
                timeCur += Time.deltaTime;
                tmp.transform.position = Vector3.Lerp(oriPos, tarPos, timeCur / lastTime);
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
