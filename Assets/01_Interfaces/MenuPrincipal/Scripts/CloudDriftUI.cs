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

        float halfCloudWidth = rectTransform.rect.width * 0.5f;
        float rightExitCenter = viewportWidth * 0.5f + halfCloudWidth;
        if (position.x >= rightExitCenter)
        {
            float leftSpawnCenter = -rightExitCenter;
            float loopDistance = viewportWidth + rectTransform.rect.width;
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
