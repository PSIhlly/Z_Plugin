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
        public void Reset()
        {
            foreach(var obj in textDic.Values)
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
        public void ToastText(string text, float lastTime)
        {
            GameObject tmp = Instantiate(toastTextProto.gameObject, toastTextProto.transform.parent);
            tmp.SetActive(true);
            var textTmp = tmp.GetComponent<TextMeshProUGUI>();
            textTmp.text = text;
            var oriPos = trs.position;
            var tarPos = trs.position + Vector3.up  + Vector3.forward ;
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
