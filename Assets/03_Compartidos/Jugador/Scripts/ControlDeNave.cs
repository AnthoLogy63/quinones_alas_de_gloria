using UnityEngine;

/// <summary>
/// Movimiento de la nave (2.5D, avanza en +X): D acelera hacia la derecha,
/// Space da empuje vertical para despegar y A frena el movimiento horizontal.
/// El motor consume combustible y no funciona sin él; la sustentación por
/// velocidad es gratuita para poder planear.
/// </summary>
public class ControlDeNave : MonoBehaviour
{
    [Header("Teclas")]
    public KeyCode teclaAcelerar = KeyCode.D;
    public KeyCode teclaElevar = KeyCode.Space;
    public KeyCode teclaFrenar = KeyCode.A;

    [Header("Vuelo")]
    public float aceleracionHorizontal = 13f;
    public float aceleracionVertical = 18f;
    public float sustentacionMaxima = 12f;
    public float velocidadMaxima = 25f;
    public float frenadoHorizontal = 18f;

    /// <summary>Lo apaga NaveJugador.Detener() al terminar el nivel.</summary>
    public bool ControlHabilitado { get; set; } = true;
    public bool MotorActivo { get; private set; }

    private Rigidbody rigidBody;
    private AudioSource audioSource;
    private CombustibleNave combustible;

    private bool acelerando;
    private bool elevando;
    private bool frenando;

    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
        combustible = GetComponent<CombustibleNave>();

        if (rigidBody == null)
        {
            Debug.LogError("El avion necesita un Rigidbody.");
            enabled = false;
            return;
        }

        rigidBody.useGravity = true;
        rigidBody.linearDamping = 0f;
        rigidBody.angularDamping = 0f;
        rigidBody.interpolation = RigidbodyInterpolation.Interpolate;
        rigidBody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rigidBody.constraints = RigidbodyConstraints.FreezePositionZ |
                                RigidbodyConstraints.FreezeRotationX |
                                RigidbodyConstraints.FreezeRotationY |
                                RigidbodyConstraints.FreezeRotationZ;
    }

    void Update()
    {
        bool hayCombustible = combustible == null || combustible.TieneCombustible;
        bool puedeVolar = ControlHabilitado && hayCombustible;

        frenando = ControlHabilitado && Input.GetKey(teclaFrenar);
        acelerando = puedeVolar && !frenando && Input.GetKey(teclaAcelerar);
        elevando = puedeVolar && !frenando && Input.GetKey(teclaElevar);

        bool motor = acelerando || elevando;
        if (motor != MotorActivo)
        {
            MotorActivo = motor;
            EventosDeJuego.RaiseMotor(motor);
        }

        if (audioSource == null)
            return;

        if (MotorActivo && !audioSource.isPlaying)
            audioSource.Play();
        else if (!MotorActivo && audioSource.isPlaying)
            audioSource.Stop();
    }

    void FixedUpdate()
    {
        if (rigidBody == null)
            return;

        if (acelerando)
            rigidBody.AddForce(Vector3.right * aceleracionHorizontal, ForceMode.Acceleration);
        if (elevando)
            rigidBody.AddForce(Vector3.up * aceleracionVertical, ForceMode.Acceleration);

        if (MotorActivo && combustible != null)
            combustible.Consumir(Time.fixedDeltaTime);

        AplicarSustentacion();
        ControlarFrenado();
        LimitarVelocidad();
    }

    void AplicarSustentacion()
    {
        // La velocidad hacia delante genera sustentacion, incluso sin motor.
        float velocidadHorizontal = Mathf.Max(0f, rigidBody.linearVelocity.x);
        float proporcionVelocidad = Mathf.Clamp01(velocidadHorizontal / velocidadMaxima);
        rigidBody.AddForce(Vector3.up * (proporcionVelocidad * sustentacionMaxima), ForceMode.Acceleration);
    }

    void ControlarFrenado()
    {
        if (!frenando)
            return;

        // Solo frena el avance: conserva la velocidad vertical del avion.
        Vector3 velocidad = rigidBody.linearVelocity;
        velocidad.x = Mathf.MoveTowards(velocidad.x, 0f, frenadoHorizontal * Time.fixedDeltaTime);
        rigidBody.linearVelocity = velocidad;
    }

    void LimitarVelocidad()
    {
        Vector2 velocidadXY = new Vector2(rigidBody.linearVelocity.x, rigidBody.linearVelocity.y);
        if (velocidadXY.magnitude <= velocidadMaxima)
            return;

        velocidadXY = velocidadXY.normalized * velocidadMaxima;
        rigidBody.linearVelocity = new Vector3(velocidadXY.x, velocidadXY.y, rigidBody.linearVelocity.z);
    }
}
