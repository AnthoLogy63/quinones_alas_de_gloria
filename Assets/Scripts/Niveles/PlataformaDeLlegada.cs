using UnityEngine;

/// <summary>
/// Zona de llegada (trigger). Al entrar la nave se completa el nivel. En los
/// niveles de aterrizaje se puede exigir una velocidad máxima.
/// </summary>
[RequireComponent(typeof(Collider))]
public class PlataformaDeLlegada : MonoBehaviour
{
    [Tooltip("0 o menos = no se exige. Si es mayor, la nave debe entrar más lenta que esto.")]
    public float velocidadMaximaDeAterrizaje = 0f;

    void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    void OnTriggerStay(Collider otro)
    {
        NaveJugador nave = otro.GetComponentInParent<NaveJugador>();
        if (nave == null || GestorDeNivel.Instancia == null)
            return;

        if (velocidadMaximaDeAterrizaje > 0f && nave.Cuerpo.linearVelocity.magnitude > velocidadMaximaDeAterrizaje)
            return;

        GestorDeNivel.Instancia.Completar();
    }
}
