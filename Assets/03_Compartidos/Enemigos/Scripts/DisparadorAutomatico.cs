using UnityEngine;

/// <summary>
/// Apunta y dispara solo a la nave cuando está en rango. Sirve para torretas
/// antiaéreas (niveles 6, 8, 10) y para enemigos voladores (nivel 7).
/// </summary>
[RequireComponent(typeof(Disparador))]
public class DisparadorAutomatico : MonoBehaviour
{
    public float rango = 30f;

    private Disparador disparador;
    private Transform objetivo;

    void Awake()
    {
        disparador = GetComponent<Disparador>();
    }

    void Update()
    {
        if (objetivo == null)
        {
            NaveJugador nave = FindFirstObjectByType<NaveJugador>();
            if (nave == null)
                return;
            objetivo = nave.transform;
        }

        Vector3 haciaNave = objetivo.position - transform.position;
        haciaNave.z = 0f;
        if (haciaNave.magnitude <= rango)
            disparador.Disparar(haciaNave);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, rango);
    }
}
