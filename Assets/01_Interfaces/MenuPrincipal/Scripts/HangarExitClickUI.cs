using UnityEngine;
using UnityEngine.EventSystems;

public class HangarExitClickUI : MonoBehaviour, IPointerClickHandler
{
    public HangarDoorTransition transition;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (transition != null)
            transition.CloseThenQuit();
    }
}
