using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>Displays the matching hover artwork while the pointer rests on an item.</summary>
public sealed class MenuHoverUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private GameObject hoverVisual;
    public void SetHoverVisual(GameObject visual) { hoverVisual = visual; if (hoverVisual != null) hoverVisual.SetActive(false); }
    public void OnPointerEnter(PointerEventData eventData) { if (hoverVisual != null) hoverVisual.SetActive(true); }
    public void OnPointerExit(PointerEventData eventData) { if (hoverVisual != null) hoverVisual.SetActive(false); }
}
