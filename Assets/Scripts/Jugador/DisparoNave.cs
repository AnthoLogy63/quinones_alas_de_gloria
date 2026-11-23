using UnityEngine;

/// <summary>
/// Disparo del jugador (niveles de combate, 7 en adelante). Desactivar este
/// componente en los niveles donde la nave no dispara.
/// </summary>
[RequireComponent(typeof(Disparador))]
public class DisparoNave : MonoBehaviour
{
    public KeyCode teclaDisparo = KeyCode.J;

    private Disparador disparador;
    private ControlDeNave control;

    void Awake()
    {
        disparador = GetComponent<Disparador>();
        control = GetComponent<ControlDeNave>();
    }

    void Update()
    {
        if (control != null && !control.ControlHabilitado)
            return;

        if (Input.GetKey(teclaDisparo))
            disparador.Disparar(Vector3.right);
    }
}
