using UnityEngine;

/// <summary>
/// Controles sencillos de vuelo: D acelera hacia la derecha, Space da
/// sustentacion para despegar y A frena el movimiento horizontal.
/// </summary>
public class ControlDeNave : MonoBehaviour
{
    private Rigidbody rigidBody;
    private AudioSource audioSource;

    [Header("Vuelo")]
    public float aceleracionHorizontal = 13f;
    public float aceleracionVertical = 18f;
    public float sustentacionMaxima = 12f;
    public float velocidadMaxima = 25f;
    public float frenadoHorizontal = 18f;

    private bool motorActivo;

    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();

        if (rigidBody == null)
        {
            Debug.LogError("El avion necesita un Rigidbody.");
            enabled = false;
            return;
        }

        rigidBody.useGravity = true;
        rigidBody.drag = 0f;
        rigidBody.angularDrag = 0f;
        rigidBody.interpolation = RigidbodyInterpolation.Interpolate;
        rigidBody.constraints = RigidbodyConstraints.FreezePositionZ |
                                RigidbodyConstraints.FreezeRotationX |
                                RigidbodyConstraints.FreezeRotationY |
                                RigidbodyConstraints.FreezeRotationZ;
    }

    void Update()
    {
        motorActivo = Input.GetKey(KeyCode.Space) && !Input.GetKey(KeyCode.A);

        if (audioSource == null)
            return;

        if (motorActivo && !audioSource.isPlaying)
            audioSource.Play();
        else if (!motorActivo && audioSource.isPlaying)
            audioSource.Stop();
    }

    void FixedUpdate()
    {
        if (rigidBody == null)
            return;

        ControlarMovimientoHorizontal();
        ControlarDespegue();
        AplicarSustentacion();
        ControlarFrenado();
        LimitarVelocidad();
    }

    void ControlarMovimientoHorizontal()
    {
        if (Input.GetKey(KeyCode.D) && !Input.GetKey(KeyCode.A))
            rigidBody.AddForce(Vector3.right * aceleracionHorizontal, ForceMode.Acceleration);
    }

    void ControlarDespegue()
    {
        if (Input.GetKey(KeyCode.Space) && !Input.GetKey(KeyCode.A))
            rigidBody.AddForce(Vector3.up * aceleracionVertical, ForceMode.Acceleration);
    }

    void AplicarSustentacion()
    {
        // La velocidad hacia delante genera sustentacion, incluso al soltar Space.
        float velocidadHorizontal = Mathf.Max(0f, rigidBody.velocity.x);
        float proporcionVelocidad = Mathf.Clamp01(velocidadHorizontal / velocidadMaxima);
        float sustentacion = proporcionVelocidad * sustentacionMaxima;
        rigidBody.AddForce(Vector3.up * sustentacion, ForceMode.Acceleration);
    }

    void ControlarFrenado()
    {
        if (!Input.GetKey(KeyCode.A))
            return;

        // A solo frena el avance: conserva la velocidad vertical del avion.
        Vector3 velocidad = rigidBody.velocity;
        velocidad.x = Mathf.MoveTowards(velocidad.x, 0f, frenadoHorizontal * Time.fixedDeltaTime);
        rigidBody.velocity = velocidad;
    }

    void LimitarVelocidad()
    {
        Vector2 velocidadXY = new Vector2(rigidBody.velocity.x, rigidBody.velocity.y);
        if (velocidadXY.magnitude <= velocidadMaxima)
            return;

        velocidadXY = velocidadXY.normalized * velocidadMaxima;
        rigidBody.velocity = new Vector3(velocidadXY.x, velocidadXY.y, rigidBody.velocity.z);
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
