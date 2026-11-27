using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HangarDoorTransition : MonoBehaviour
{
    public RectTransform leftDoor;
    public RectTransform rightDoor;
    public Image leftImage;
    public Image rightImage;
    public float holdSeconds = 1.5f;
    public float openSeconds = 1.2f;
    public float closeSeconds = 0.85f;

    private bool opened;
    private bool closing;
    private float closeElapsed;
    private string pendingScenePath;

    private IEnumerator Start()
    {
        if (leftDoor == null || rightDoor == null)
        {
            Debug.LogError("HangarDoorTransition necesita las dos puertas asignadas.", this);
            yield break;
        }

        leftDoor.anchoredPosition = Vector2.zero;
        rightDoor.anchoredPosition = Vector2.zero;
        if (leftImage != null) leftImage.raycastTarget = false;
        if (rightImage != null) rightImage.raycastTarget = false;

        yield return null;
        yield return new WaitForSecondsRealtime(holdSeconds);
        yield return MoveDoors(-1f, openSeconds);
        opened = true;
    }

    public void CloseThenLoadScene(string scenePath)
    {
        if (!opened || closing || string.IsNullOrEmpty(scenePath)) return;
        pendingScenePath = scenePath;
        BeginClose();
    }

    public void CloseThenQuit()
    {
        if (!opened || closing) return;
        pendingScenePath = null;
        BeginClose();
    }

    private void BeginClose()
    {
        closing = true;
        closeElapsed = 0f;
        if (leftImage != null) leftImage.raycastTarget = true;
        if (rightImage != null) rightImage.raycastTarget = true;

        float halfWidth = GetHalfWidth();
        leftDoor.anchoredPosition = new Vector2(-halfWidth, 0f);
        rightDoor.anchoredPosition = new Vector2(halfWidth, 0f);
    }

    private void Update()
    {
        if (!closing) return;

        closeElapsed += Time.unscaledDeltaTime;
        float t = Mathf.Clamp01(closeElapsed / Mathf.Max(0.01f, closeSeconds));
        t = t * t * (3f - 2f * t);
        leftDoor.anchoredPosition = Vector2.LerpUnclamped(new Vector2(-GetHalfWidth(), 0f), Vector2.zero, t);
        rightDoor.anchoredPosition = Vector2.LerpUnclamped(new Vector2(GetHalfWidth(), 0f), Vector2.zero, t);

        if (t >= 1f)
        {
            closing = false;
            leftDoor.anchoredPosition = Vector2.zero;
            rightDoor.anchoredPosition = Vector2.zero;
            if (!string.IsNullOrEmpty(pendingScenePath))
            {
                SceneManager.LoadScene(pendingScenePath);
                return;
            }
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }

    private IEnumerator MoveDoors(float destinationFactor, float duration)
    {
        float halfWidth = GetHalfWidth();
        Vector2 leftStart = leftDoor.anchoredPosition;
        Vector2 rightStart = rightDoor.anchoredPosition;
        Vector2 leftEnd = new Vector2(halfWidth * destinationFactor, 0f);
        Vector2 rightEnd = new Vector2(-halfWidth * destinationFactor, 0f);
        float elapsed = 0f;
        duration = Mathf.Max(0.01f, duration);

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            t = t * t * (3f - 2f * t);
            leftDoor.anchoredPosition = Vector2.LerpUnclamped(leftStart, leftEnd, t);
            rightDoor.anchoredPosition = Vector2.LerpUnclamped(rightStart, rightEnd, t);
            yield return null;
        }

        leftDoor.anchoredPosition = leftEnd;
        rightDoor.anchoredPosition = rightEnd;
    }

    private float GetHalfWidth()
    {
        RectTransform canvasRect = transform.parent as RectTransform;
        return canvasRect != null ? canvasRect.rect.width * 0.5f : Screen.width * 0.5f;
    }
}
