
using UnityEngine;

public class ControlDeNave : MonoBehaviour
{
    private Rigidbody rigidBody;
    private AudioSource audioSource;
    private bool propulsando;

    public float aceleracion = 13f;
    public float velocidadMaxima = 25f;
    public float velocidadRotacion = 25f;

    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();

        ConfigurarFisicas();
        ValidarComponentes();
    }

    void Update()
    {
        propulsando = Input.GetKey(KeyCode.Space);
        ControlarAudio();
    }

    void FixedUpdate()
    {
        if (rigidBody == null)
            return;

        ControlarRotacionDurantePropulsion();
        ControlarRotacion();
        ControlarPropulsion();
        LimitarVelocidad();
    }

    void ConfigurarFisicas()
    {
        if (rigidBody == null)
            return;

        rigidBody.useGravity = true;
        rigidBody.drag = 0f;
        rigidBody.angularDrag = 0f;
        rigidBody.interpolation = RigidbodyInterpolation.Interpolate;

        rigidBody.constraints =
            RigidbodyConstraints.FreezePositionZ |
            RigidbodyConstraints.FreezeRotationX |
            RigidbodyConstraints.FreezeRotationY;
    }

    void ValidarComponentes()
    {
        if (rigidBody == null)
            Debug.LogError("La nave necesita un Rigidbody.");

        if (audioSource == null)
            Debug.LogError("La nave necesita un Audio Source con el sonido del propulsor.");
    }

    void ControlarAudio()
    {
        if (audioSource == null)
            return;

        if (propulsando && !audioSource.isPlaying)
            audioSource.Play();
        else if (!propulsando && audioSource.isPlaying)
            audioSource.Stop();
    }

    void ControlarRotacionDurantePropulsion()
    {
        // Se congela la rotación mientras se mantiene Space.
        if (propulsando)
        {
            rigidBody.constraints |= RigidbodyConstraints.FreezeRotationZ;
        }
        else
        {
            // Se permite girar nuevamente al soltar Space.
            rigidBody.constraints &= ~RigidbodyConstraints.FreezeRotationZ;
        }
    }

    void ControlarRotacion()
    {
        if (propulsando)
            return;

        float direccionGiro = 0f;

        if (Input.GetKey(KeyCode.D))
            direccionGiro = -1f;
        else if (Input.GetKey(KeyCode.A))
            direccionGiro = 1f;

        float giro = direccionGiro * velocidadRotacion * Time.fixedDeltaTime;

        rigidBody.MoveRotation(
            rigidBody.rotation * Quaternion.Euler(0f, 0f, giro)
        );
    }

    void ControlarPropulsion()
    {
        if (propulsando)
        {
            rigidBody.AddForce(
                transform.right * aceleracion,
                ForceMode.Acceleration
            );
        }
    }

    void LimitarVelocidad()
    {
        Vector2 velocidadXY = new Vector2(
            rigidBody.velocity.x,
            rigidBody.velocity.y
        );

        if (velocidadXY.magnitude <= velocidadMaxima)
            return;

        velocidadXY = velocidadXY.normalized * velocidadMaxima;

        rigidBody.velocity = new Vector3(
            velocidadXY.x,
            velocidadXY.y,
            rigidBody.velocity.z
        );
    }
    
    void OnCollisionEnter(Collision collision)
    {
        switch (collision.gameObject.tag)
        {
            case "colisión segura":
                Debug.Log("Colisión segura");
                break;

            case "condición peligrosa":
                Debug.Log("Colisión peligrosa");
                break;

            default:
                Debug.Log("Colisión con objeto sin clasificar");
                break;
        }
    }
}
