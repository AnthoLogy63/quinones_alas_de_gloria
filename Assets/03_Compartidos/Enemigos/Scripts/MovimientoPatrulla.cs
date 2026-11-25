using UnityEngine;

/// <summary>
/// Mueve un objeto de ida y vuelta (senoidal) respecto a su posición inicial.
/// Para obstáculos móviles y enemigos voladores.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class MovimientoPatrulla : MonoBehaviour
{
    [Tooltip("Desplazamiento total del recorrido, relativo a la posición inicial.")]
    public Vector3 recorrido = new Vector3(0f, 6f, 0f);
    public float velocidad = 1f;

    private Rigidbody cuerpo;
    private Vector3 origen;
    private float desfase;

    void Awake()
    {
        cuerpo = GetComponent<Rigidbody>();
        cuerpo.isKinematic = true;
        cuerpo.useGravity = false;
        origen = transform.position;
        desfase = Random.value * Mathf.PI * 2f;
    }

    void FixedUpdate()
    {
        float t = Mathf.Sin(Time.time * velocidad + desfase) * 0.5f;
        cuerpo.MovePosition(origen + recorrido * t);
    }
}
