using UnityEngine;
using UnityEngine.EventSystems;

public class UIPinchToZoom : MonoBehaviour, IDragHandler, IScrollHandler
{
    public float zoomSpeed = 0.01f;   // Speed of zooming
    public float minZoom = 0.5f;     // Minimum scale
    public float maxZoom = 2.0f;     // Maximum scale

    private RectTransform rectTransform;
    private Vector2 originalSizeDelta;
    private Vector3 originalPosition;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        originalSizeDelta = rectTransform.sizeDelta;
        originalPosition = rectTransform.localPosition;
    }

    private void Update()
    {
        // Handle touch input
        if (Input.touchCount == 2)
        {
            Touch touch0 = Input.GetTouch(0);
            Touch touch1 = Input.GetTouch(1);

            Vector2 touch0PrevPos = touch0.position - touch0.deltaPosition;
            Vector2 touch1PrevPos = touch1.position - touch1.deltaPosition;

            float prevTouchDeltaMag = (touch0PrevPos - touch1PrevPos).magnitude;
            float touchDeltaMag = (touch0.position - touch1.position).magnitude;

            float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

            Zoom(deltaMagnitudeDiff, zoomSpeed);
        }
    }

    public void OnScroll(PointerEventData eventData)
    {
        float scrollDelta = eventData.scrollDelta.y;
        Zoom(scrollDelta, zoomSpeed); // Adjust zoom speed for scroll
    }

    private void Zoom(float deltaMagnitudeDiff, float speed)
    {
        float newScale = rectTransform.localScale.x - deltaMagnitudeDiff * speed;
        newScale = Mathf.Clamp(newScale, minZoom, maxZoom);

        rectTransform.localScale = new Vector3(newScale, newScale, 1f);
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector3 newPosition = rectTransform.localPosition + (Vector3)eventData.delta;
        rectTransform.localPosition = new Vector3(
            Mathf.Clamp(newPosition.x, originalPosition.x, originalPosition.x),
            Mathf.Clamp(newPosition.y, originalPosition.y, originalPosition.y),
            newPosition.z
        );
    }
}
