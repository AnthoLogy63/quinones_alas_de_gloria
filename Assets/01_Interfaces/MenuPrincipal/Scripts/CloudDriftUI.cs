using UnityEngine;

/// <summary>Moves a cloud layer left-to-right and wraps it after it fully leaves the UI viewport.</summary>
public sealed class CloudDriftUI : MonoBehaviour
{
    [SerializeField] private float speedPixelsPerSecond = 35f;
    [SerializeField] private float viewportWidth = 1920f;

    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void Update()
    {
        if (rectTransform == null || speedPixelsPerSecond <= 0f || viewportWidth <= 0f) return;

        Vector2 position = rectTransform.anchoredPosition;
        position.x += speedPixelsPerSecond * Time.unscaledDeltaTime;

        // RectTransform.rect is unscaled. Use the cloud's actual size in canvas
        // units so scaled sprites leave and re-enter at the correct edges.
        float scaleX = Mathf.Abs(rectTransform.localScale.x);
        for (Transform parent = rectTransform.parent; parent != null && parent.GetComponent<Canvas>() == null; parent = parent.parent)
            scaleX *= Mathf.Abs(parent.localScale.x);
        float cloudWidth = rectTransform.rect.width * scaleX;
        float halfCloudWidth = cloudWidth * 0.5f;
        float rightExitCenter = viewportWidth * 0.5f + halfCloudWidth;
        if (position.x >= rightExitCenter)
        {
            float leftSpawnCenter = -rightExitCenter;
            float loopDistance = viewportWidth + cloudWidth;
            position.x = leftSpawnCenter + Mathf.Repeat(position.x - rightExitCenter, loopDistance);
        }

        rectTransform.anchoredPosition = position;
    }

    public void Configure(float speed, float viewWidth)
    {
        speedPixelsPerSecond = speed;
        viewportWidth = viewWidth;
    }
}
