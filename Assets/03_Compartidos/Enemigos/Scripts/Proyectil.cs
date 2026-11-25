using UnityEngine;

/// <summary>
/// Proyectil recto en el plano XY. Si toca la nave cuenta como impacto peligroso;
/// si toca algo con <see cref="IDanable"/> le hace daño (solo si daniaAEnemigos).
/// Se destruye al tocar cualquier sólido o al agotar su vida.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class Proyectil : MonoBehaviour
{
    public float velocidad = 15f;
    public float danio = 1f;
    public float vida = 6f;
    public bool daniaAlJugador = true;
    public bool daniaAEnemigos = false;

    private Rigidbody cuerpo;
    private Vector3 direccion = Vector3.right;
    private GameObject emisor;

    void Awake()
    {
        cuerpo = GetComponent<Rigidbody>();
        cuerpo.isKinematic = true;
        cuerpo.useGravity = false;
    }

    void Start()
    {
        Destroy(gameObject, vida);
    }

    public void Lanzar(Vector3 nuevaDireccion, GameObject quienDispara)
    {
        direccion = nuevaDireccion.normalized;
        emisor = quienDispara;
    }

    void FixedUpdate()
    {
        cuerpo.MovePosition(cuerpo.position + direccion * (velocidad * Time.fixedDeltaTime));
    }

    void OnTriggerEnter(Collider otro)
    {
        if (otro.isTrigger)
            return;
        if (emisor != null && otro.transform.IsChildOf(emisor.transform))
            return;

        NaveJugador nave = otro.GetComponentInParent<NaveJugador>();
        if (nave != null)
        {
            if (daniaAlJugador)
                nave.Colisiones.RegistrarImpacto(TipoColision.Peligrosa, gameObject);
            Destroy(gameObject);
            return;
        }

        IDanable danable = otro.GetComponentInParent<IDanable>();
        if (danable != null && daniaAEnemigos)
            danable.RecibirDanio(danio, emisor);

        Destroy(gameObject);
    }
}
