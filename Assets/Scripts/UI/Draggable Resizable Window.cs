using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableWindow : MonoBehaviour, IDragHandler, IBeginDragHandler
{
    [Header("拖拽区域")]
    public RectTransform dragHandle;
    public RectTransform windowTransform;

    private void Awake()
    {
        if (windowTransform == null)
            windowTransform = GetComponent<RectTransform>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // 不做额外处理
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragHandle == null) return;

        // 检查拖拽点是否在 dragHandle 内
        if (!RectTransformUtility.RectangleContainsScreenPoint(dragHandle, eventData.position, eventData.pressEventCamera))
            return;

        windowTransform.anchoredPosition += eventData.delta / GetComponentInParent<Canvas>().scaleFactor;
    }
}