using UnityEngine;

/// <summary>
/// Base de todo objeto que la nave recoge al tocarlo. Para crear uno nuevo
/// (puntos extra, escudo...) heredar y sobrescribir <see cref="AlRecolectar"/>.
/// </summary>
[RequireComponent(typeof(Collider))]
public abstract class Recolectable : MonoBehaviour
{
    public float velocidadGiro = 90f;

    protected virtual void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    void Update()
    {
        transform.Rotate(0f, velocidadGiro * Time.deltaTime, 0f, Space.World);
    }

    void OnTriggerEnter(Collider otro)
    {
        NaveJugador nave = otro.GetComponentInParent<NaveJugador>();
        if (nave == null)
            return;

        if (AlRecolectar(nave))
        {
            EventosDeJuego.RaiseRecolectado(gameObject);
            Destroy(gameObject);
        }
    }

    /// <returns>true si se consumió y debe desaparecer.</returns>
    protected abstract bool AlRecolectar(NaveJugador nave);
}
