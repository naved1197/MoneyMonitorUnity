using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
public class DragRectTransform : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [SerializeField]private RectTransform rectTransform;
    private Vector2 startPosition;
    private bool isDragging;

    public float clampMinY = -100f;
    public float clampMaxY = 100f;
    public float retractSpeed = 5f;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        startPosition = rectTransform.anchoredPosition;
        isDragging = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (isDragging)
            rectTransform.DOAnchorPosY(startPosition.y, 0.5f).From(rectTransform.anchoredPosition).SetEase(Ease.OutSine);
        isDragging = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging)
            return;
        float newY= rectTransform.anchoredPosition.y + eventData.delta.y;
        newY = Mathf.Clamp(newY, clampMinY, clampMaxY);
        rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, newY);
    }
}
