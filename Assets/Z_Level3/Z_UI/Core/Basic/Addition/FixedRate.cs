using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Z_Time;

public class FixedRate : MonoBehaviour
{
    private RectTransform _rectTransform;
    public float WidthToHeightRatio;

    void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        // 初始化时同步一次高度
        SyncHeightToWidth();
    }

    /// <summary>
    /// 关键回调：RectTransform 尺寸/锚点/缩放变化时触发
    /// </summary>
    private void OnRectTransformDimensionsChange()
    {
        SyncHeightToWidth();
    }

    private void SyncHeightToWidth()
    {
        if (_rectTransform == null) return;

        float currentWidth = _rectTransform.rect.width;
        float targetHeight = currentWidth * WidthToHeightRatio;

        if (Mathf.Abs(_rectTransform.rect.height - targetHeight) > 0.01f)
        {
            // 兼容锚点拉伸/点锚定两种场景
            if (_rectTransform.anchorMin != _rectTransform.anchorMax)
            {
                _rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, targetHeight);
            }
            else
            {
                Vector2 sizeDelta = _rectTransform.sizeDelta;
                sizeDelta.y = targetHeight;
                _rectTransform.sizeDelta = sizeDelta;
            }
        }
    }

    // 编辑器实时预览
   /* private void OnValidate()
    {
        if (_rectTransform == null)
            _rectTransform = GetComponent<RectTransform>();
        SyncHeightToWidth();
    }*/

}
