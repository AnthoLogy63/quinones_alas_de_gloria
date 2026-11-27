using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>Displays the matching hover artwork while the pointer rests on an item.</summary>
public sealed class MenuHoverUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] private GameObject hoverVisual;
    [SerializeField] private AudioDeMenu audioMenu;
    public void SetHoverVisual(GameObject visual) { hoverVisual = visual; if (hoverVisual != null) hoverVisual.SetActive(false); }
    public void SetAudio(AudioDeMenu audio) { audioMenu = audio; }
    public void OnPointerEnter(PointerEventData eventData) { if (hoverVisual != null) hoverVisual.SetActive(true); if (audioMenu != null) audioMenu.Hover(); }
    public void OnPointerExit(PointerEventData eventData) { if (hoverVisual != null) hoverVisual.SetActive(false); }
    public void OnPointerClick(PointerEventData eventData) { if (eventData.button == PointerEventData.InputButton.Left && audioMenu != null) audioMenu.Click(); }
}
