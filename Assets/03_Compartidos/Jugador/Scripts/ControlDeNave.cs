using UnityEngine;

/// <summary>
/// Vuelo 2.5D con física simplificada (avanza en +X, el cabeceo gira en Z).
/// Hay que usar las dos manos:
///   mano izquierda  -> D / A: subir / bajar la potencia (la palanca se queda donde la dejes),
///                      Shift: frenos (ruedas en tierra, aerofrenos en el aire);
///   mano derecha    -> flechas arriba / abajo: morro arriba / abajo.
/// La potencia empuja hacia donde apunta el morro. La sustentación depende de la
/// velocidad y del ángulo de ataque: en tierra hay que rodar hasta la velocidad de
/// rotación y recién ahí levantar el morro. Con demasiado ángulo o muy poca velocidad
/// el avión entra en pérdida y cae. Sin combustible el motor se apaga y solo planea.
/// </summary>
public class ControlDeNave : MonoBehaviour
{
    [Header("Teclas - mano izquierda (motor y frenos)")]
    public KeyCode teclaMasPotencia = KeyCode.D;
    public KeyCode teclaMenosPotencia = KeyCode.A;
    public KeyCode teclaFrenos = KeyCode.LeftShift;

    [Header("Teclas - mano derecha (cabeceo)")]
    public KeyCode teclaMorroArriba = KeyCode.UpArrow;
    public KeyCode teclaMorroAbajo = KeyCode.DownArrow;

    [Header("Motor")]
    [Tooltip("Aceleración (m/s²) a plena potencia, en la dirección del morro.")]
    public float empujeMaximo = 7f;
    [Tooltip("Fracción de potencia que sube por segundo al mantener la tecla.")]
    public float subidaPotencia = 0.5f;
    public float bajadaPotencia = 0.8f;

    [Header("Aerodinámica")]
    [Tooltip("Sustentación (m/s²) = coef * velocidad² * Cl. Con 0.0614 y Cl 0.47 se sostiene a ~22 m/s.")]
    public float coeficienteSustentacion = 0.0614f;
    [Tooltip("Grados de ángulo de ataque 'gratis' del ala curva.")]
    public float anguloCeroSustentacion = 2f;
    [Tooltip("Ángulo de ataque a partir del cual el ala entra en pérdida.")]
    public float anguloDePerdida = 15f;
    public float arrastreParasito = 0.009f;
    public float arrastreInducido = 0.012f;
    [Tooltip("Por debajo de esta velocidad los mandos pierden eficacia.")]
    public float velocidadControl = 12f;
    [Tooltip("En tierra: velocidad mínima para poder levantar el morro.")]
    public float velocidadRotacion = 15f;

    [Header("Cabeceo")]
    public float velocidadCabeceo = 55f;
    [Tooltip("Qué tan rápido el morro tiende a alinearse con la trayectoria si sueltas los mandos.")]
    public float estabilidadCabeceo = 0.6f;
    [Tooltip("Ángulo de ataque al que se estabiliza con los mandos sueltos.")]
    public float anguloDeAtaqueCrucero = 5f;
    [Tooltip("Inclinación máxima con el morro arriba (grados). Un avión real no se empina más que esto.")]
    public float cabeceoMaximo = 35f;
    [Tooltip("Inclinación máxima en picada (grados, negativo).")]
    public float cabeceoMinimo = -30f;
    public float cabeceoMaximoEnTierra = 14f;
    [Tooltip("En pérdida el morro cae solo a esta velocidad (grados/s), aunque sigas tirando hacia arriba.")]
    public float caidaMorroEnPerdida = 25f;

    [Header("Tierra y frenos")]
    public float friccionRodadura = 1.5f;
    public float frenadoRuedas = 12f;
    public float aerofrenos = 5f;
    [Tooltip("Velocidad contra la superficie (m/s) desde la que un toque cuenta como colisión peligrosa. 0 = desactivado.")]
    public float velocidadAterrizajeBrusco = 9f;
    public float limiteDeSeguridad = 45f;

    /// <summary>Lo apaga NaveJugador.Detener() al terminar el nivel.</summary>
    public bool ControlHabilitado { get; set; } = true;
    public bool MotorActivo { get; private set; }

    /// <summary>Palanca de potencia, de 0 a 1.</summary>
    public float Potencia { get; private set; }
    /// <summary>Cabeceo en grados (positivo = morro arriba).</summary>
    public float Cabeceo => cabeceo;
    /// <summary>Hacia dónde apunta el morro (los disparos salen por aquí).</summary>
    public Vector3 Morro
    {
        get
        {
            float rad = cabeceo * Mathf.Deg2Rad;
            return new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f);
        }
    }
    public float VelocidadAire { get; private set; }
    public bool EnTierra { get; private set; }
    public bool EnPerdida { get; private set; }

    private Rigidbody rigidBody;
    private AudioSource audioSource;
    private CombustibleNave combustible;
    private ColisionesNave colisiones;

    private float cabeceo;
    private float entradaPotencia;
    private float entradaCabeceo;
    private bool entradaFrenos;
    private bool entradaExterna;
    private bool contactoSuelo;

    /// <summary>
    /// Sustituye al teclado (pruebas automáticas, tutoriales, repeticiones).
    /// Potencia y cabeceo van de -1 a 1.
    /// </summary>
    public void UsarEntradaExterna(float cambioPotencia, float cabeceoPedido, bool frenos)
    {
        entradaExterna = true;
        entradaPotencia = Mathf.Clamp(cambioPotencia, -1f, 1f);
        entradaCabeceo = Mathf.Clamp(cabeceoPedido, -1f, 1f);
        entradaFrenos = frenos;
    }

    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
        combustible = GetComponent<CombustibleNave>();
        colisiones = GetComponent<ColisionesNave>();

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
        // El cabeceo lo fija este script; la física no debe girar el avión.
        rigidBody.constraints = RigidbodyConstraints.FreezePositionZ |
                                RigidbodyConstraints.FreezeRotationX |
                                RigidbodyConstraints.FreezeRotationY |
                                RigidbodyConstraints.FreezeRotationZ;

        QuitarFriccionDeColliders();

        cabeceo = 0f;
        rigidBody.rotation = Quaternion.identity;
    }

    // La fricción la simulamos nosotros (rodadura y frenos); la del collider frenaría el despegue.
    void QuitarFriccionDeColliders()
    {
        PhysicsMaterial liso = new PhysicsMaterial("NaveSinFriccion")
        {
            dynamicFriction = 0f,
            staticFriction = 0f,
            bounciness = 0f,
            frictionCombine = PhysicsMaterialCombine.Minimum,
            bounceCombine = PhysicsMaterialCombine.Minimum
        };

        foreach (Collider c in GetComponentsInChildren<Collider>())
            if (!c.isTrigger)
                c.sharedMaterial = liso;
    }

    void Update()
    {
        if (!entradaExterna)
        {
            entradaPotencia = AxisTeclas(teclaMasPotencia, teclaMenosPotencia);
            entradaCabeceo = AxisTeclas(teclaMorroArriba, teclaMorroAbajo);
            entradaFrenos = Input.GetKey(teclaFrenos) || Input.GetKey(KeyCode.RightShift);
        }

        if (audioSource == null)
            return;

        if (MotorActivo)
        {
            audioSource.pitch = Mathf.Lerp(0.8f, 1.4f, Potencia);
            if (!audioSource.isPlaying)
                audioSource.Play();
        }
        else if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    static float AxisTeclas(KeyCode positiva, KeyCode negativa)
    {
        return (Input.GetKey(positiva) ? 1f : 0f) - (Input.GetKey(negativa) ? 1f : 0f);
    }

    void FixedUpdate()
    {
        if (rigidBody == null)
            return;

        float dt = Time.fixedDeltaTime;
        EnTierra = contactoSuelo;
        contactoSuelo = false;

        bool mandos = ControlHabilitado;
        float pedidoPotencia = mandos ? entradaPotencia : 0f;
        float pedidoCabeceo = mandos ? entradaCabeceo : 0f;
        bool frenos = mandos && entradaFrenos;

        ActualizarPotencia(pedidoPotencia, dt);

        Vector3 v = rigidBody.linearVelocity;
        v.z = 0f;
        float velocidad = v.magnitude;
        VelocidadAire = velocidad;

        float trayectoria = velocidad > 0.5f ? Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg : cabeceo;
        float ataque = Mathf.DeltaAngle(trayectoria, cabeceo) + anguloCeroSustentacion;
        float cl = CoeficienteDeSustentacion(ataque);
        EnPerdida = !EnTierra && velocidad > 3f && Mathf.Abs(ataque) > anguloDePerdida;

        Vector3 aceleracion = Morro * (empujeMaximo * Potencia);

        if (velocidad > 0.5f)
        {
            Vector3 dirV = v / velocidad;
            float v2 = velocidad * velocidad;

            // Sustentación: perpendicular a la trayectoria, solo si el aire entra por el morro.
            if (Vector3.Dot(v, Morro) > 0f)
            {
                Vector3 dirSustentacion = new Vector3(-dirV.y, dirV.x, 0f);
                aceleracion += dirSustentacion * (coeficienteSustentacion * v2 * cl);
            }

            float arrastre = (arrastreParasito + arrastreInducido * cl * cl) * v2;
            if (frenos && !EnTierra)
                arrastre += aerofrenos;
            aceleracion -= dirV * arrastre;
        }

        rigidBody.AddForce(aceleracion, ForceMode.Acceleration);

        ActualizarCabeceo(pedidoCabeceo, velocidad, trayectoria, dt);

        if (EnTierra)
        {
            float rodadura = friccionRodadura + (frenos ? frenadoRuedas : 0f);
            Vector3 vel = rigidBody.linearVelocity;
            vel.x = Mathf.MoveTowards(vel.x, 0f, rodadura * dt);
            rigidBody.linearVelocity = vel;
        }

        if (rigidBody.linearVelocity.magnitude > limiteDeSeguridad)
            rigidBody.linearVelocity = rigidBody.linearVelocity.normalized * limiteDeSeguridad;

        rigidBody.angularVelocity = Vector3.zero;
        rigidBody.MoveRotation(Quaternion.Euler(0f, 0f, cabeceo));
    }

    void ActualizarPotencia(float pedido, float dt)
    {
        bool hayCombustible = combustible == null || combustible.TieneCombustible;

        if (!hayCombustible)
            Potencia = 0f;
        else if (!ControlHabilitado)
            Potencia = Mathf.MoveTowards(Potencia, 0f, bajadaPotencia * dt);
        else
            Potencia = Mathf.Clamp01(Potencia + pedido * (pedido > 0f ? subidaPotencia : bajadaPotencia) * dt);

        bool motor = hayCombustible && Potencia > 0.05f;
        if (motor != MotorActivo)
        {
            MotorActivo = motor;
            EventosDeJuego.RaiseMotor(motor);
        }

        if (MotorActivo && combustible != null)
            combustible.Consumir(dt * Potencia);
    }

    void ActualizarCabeceo(float pedido, float velocidad, float trayectoria, float dt)
    {
        if (EnTierra)
        {
            // Rodando: el morro solo se levanta cuando hay velocidad de rotación.
            if (pedido > 0f && velocidad >= velocidadRotacion)
                cabeceo += pedido * velocidadCabeceo * dt;
            else
                cabeceo = Mathf.MoveTowards(cabeceo, 0f, velocidadCabeceo * dt);

            cabeceo = Mathf.Clamp(cabeceo, 0f, cabeceoMaximoEnTierra);
            return;
        }

        // En el aire los mandos dependen de la velocidad del aire sobre las superficies.
        float autoridad = Mathf.Clamp01(velocidad / velocidadControl);
        cabeceo += pedido * velocidadCabeceo * autoridad * dt;

        // Con los mandos sueltos el morro tiende a seguir la trayectoria.
        float objetivo = trayectoria + anguloDeAtaqueCrucero;
        float peso = 1f - Mathf.Abs(pedido);
        cabeceo += Mathf.DeltaAngle(cabeceo, objetivo) * estabilidadCabeceo * autoridad * peso * dt;

        // En pérdida el ala deja de sostener y el morro se hunde: hay que bajarlo para recuperar.
        if (EnPerdida)
            cabeceo -= caidaMorroEnPerdida * dt;

        cabeceo = Mathf.Clamp(cabeceo, cabeceoMinimo, cabeceoMaximo);
    }

    float CoeficienteDeSustentacion(float ataque)
    {
        float abs = Mathf.Abs(ataque);
        if (abs <= anguloDePerdida)
            return ataque / anguloDePerdida;

        // Pasado el ángulo crítico la sustentación se desploma.
        float caida = Mathf.Clamp01((abs - anguloDePerdida) / 10f);
        return Mathf.Sign(ataque) * Mathf.Lerp(1f, 0.25f, caida);
    }

    void OnCollisionEnter(Collision collision)
    {
        MarcarContactoConSuelo(collision);

        if (velocidadAterrizajeBrusco <= 0f || colisiones == null || collision.contactCount == 0)
            return;

        float contraSuperficie = Mathf.Abs(Vector3.Dot(collision.relativeVelocity, collision.GetContact(0).normal));
        if (contraSuperficie >= velocidadAterrizajeBrusco)
            colisiones.RegistrarImpacto(TipoColision.Peligrosa, collision.gameObject);
    }

    void OnCollisionStay(Collision collision)
    {
        MarcarContactoConSuelo(collision);
    }

    void MarcarContactoConSuelo(Collision collision)
    {
        for (int i = 0; i < collision.contactCount; i++)
        {
            if (collision.GetContact(i).normal.y > 0.6f)
            {
                contactoSuelo = true;
                return;
            }
        }
    }
}
