using UnityEngine;

/// <summary>
/// Dispara proyectiles con cadencia limitada. Lo usan la nave del jugador, las
/// torretas y los enemigos: solo cambian el prefab y las banderas de daño.
/// </summary>
public class Disparador : MonoBehaviour
{
    public Proyectil prefabProyectil;
    public Transform puntoDeDisparo;
    public float velocidadProyectil = 15f;
    public float segundosEntreDisparos = 0.4f;
    public bool daniaAlJugador = true;
    public bool daniaAEnemigos = false;
    [Tooltip("Separación del origen para no nacer dentro del propio collider.")]
    public float distanciaDeSalida = 2f;

    private float proximoDisparo;

    public bool Disparar(Vector3 direccion)
    {
        if (prefabProyectil == null || Time.time < proximoDisparo)
            return false;

        direccion.z = 0f;
        if (direccion.sqrMagnitude < 0.0001f)
            return false;
        direccion.Normalize();

        proximoDisparo = Time.time + segundosEntreDisparos;

        Vector3 origen = puntoDeDisparo != null ? puntoDeDisparo.position : transform.position;
        origen += direccion * distanciaDeSalida;

        Proyectil bala = Instantiate(prefabProyectil, origen, Quaternion.LookRotation(direccion));
        bala.velocidad = velocidadProyectil;
        bala.daniaAlJugador = daniaAlJugador;
        bala.daniaAEnemigos = daniaAEnemigos;
        bala.Lanzar(direccion, gameObject);

        EventosDeJuego.RaiseDisparo(gameObject);
        return true;
    }
}
