using UnityEngine;
using UnityEngine.EventSystems;

public class LoadSceneOnClickUI : MonoBehaviour, IPointerClickHandler
{
    public string scenePath = "Assets/02_Niveles/Nivel_00_Pruebas/PruebasAvion.unity";
    public HangarDoorTransition transition;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (transition == null)
        {
            Debug.LogError("LoadSceneOnClickUI necesita la transición de hangar asignada.", this);
            return;
        }
        transition.CloseThenLoadScene(scenePath);
    }
}
