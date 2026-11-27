using UnityEngine;

/// <summary>
/// Disparo del jugador (niveles de combate, 7 en adelante). Desactivar este
/// componente en los niveles donde la nave no dispara.
/// No se puede apuntar: el disparo sale siempre por el morro y va en línea recta.
/// </summary>
[RequireComponent(typeof(Disparador))]
public class DisparoNave : MonoBehaviour
{
    public KeyCode teclaDisparo = KeyCode.J;
    public KeyCode teclaDisparoAlterna = KeyCode.Space;

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

        if (Input.GetKey(teclaDisparo) || Input.GetKey(teclaDisparoAlterna))
        {
            Vector3 morro = control != null ? control.Morro : transform.right;
            disparador.Disparar(morro);
        }
    }
}
