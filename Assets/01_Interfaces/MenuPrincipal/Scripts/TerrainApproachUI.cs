using UnityEngine;
using UnityEngine.UI;

public class TerrainApproachUI : MonoBehaviour
{
    [SerializeField] private float durationSeconds = 65f;
    [SerializeField] private Vector2 travelOffset = new Vector2(0f, -900f);
    [SerializeField] private float scaleIncrease = 0.7f;
    [SerializeField] private float riseAmount = 360f;
    [SerializeField, Range(0f, 0.25f)] private float fadeInEnd = 0.01f;
    [SerializeField, Range(0.75f, 1f)] private float fadeOutStart = 0.985f;
    [SerializeField, Range(0f, 1f)] private float phaseOffset;
    [SerializeField] private int backSiblingIndex = 2;

    private RectTransform rectTransform;
    private Image image;
    private Vector2 startPosition;
    private Vector3 startScale;
    private Color startColor;
    private float startTime;
    private int lastCycle;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null) return;
        image = GetComponent<Image>();
        startPosition = rectTransform.anchoredPosition;
        startScale = rectTransform.localScale;
        if (image != null) startColor = image.color;
        startTime = Time.unscaledTime;
        lastCycle = Mathf.FloorToInt(phaseOffset);
    }

    private void Update()
    {
        if (rectTransform == null) return;
        float duration = Mathf.Max(0.01f, durationSeconds);
        float totalProgress = (Time.unscaledTime - startTime) / duration + phaseOffset;
        int cycle = Mathf.FloorToInt(totalProgress);
        float t = totalProgress - cycle;

        if (cycle > lastCycle)
            rectTransform.SetSiblingIndex(Mathf.Clamp(backSiblingIndex, 0, rectTransform.parent.childCount - 1));
        lastCycle = cycle;

        // Preserve the hand-placed starting position for the first staggered pass.
        // Later passes start at the far edge (progress 0) and move fully toward the cockpit.
        float referenceProgress = cycle == 0 ? phaseOffset : 0f;
        float arc = Mathf.Sin(Mathf.PI * t) * riseAmount;
        float referenceArc = Mathf.Sin(Mathf.PI * referenceProgress) * riseAmount;
        float verticalAtT = travelOffset.y * t + arc;
        float verticalAtReference = travelOffset.y * referenceProgress + referenceArc;
        rectTransform.anchoredPosition = startPosition + new Vector2(
            travelOffset.x * (t - referenceProgress),
            verticalAtT - verticalAtReference);
        float scaleAtT = 1f + scaleIncrease * t;
        float scaleAtReference = 1f + scaleIncrease * referenceProgress;
        rectTransform.localScale = startScale * (scaleAtT / scaleAtReference);

        if (image != null)
        {
            float fadeIn = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / Mathf.Max(0.001f, fadeInEnd)));
            float fadeOut = 1f - Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((t - fadeOutStart) / Mathf.Max(0.001f, 1f - fadeOutStart)));
            float alpha = Mathf.Min(fadeIn, fadeOut);
            image.color = new Color(startColor.r, startColor.g, startColor.b, startColor.a * alpha);
        }
    }

    private void OnDisable()
    {
        if (image != null) image.color = startColor;
    }
}
