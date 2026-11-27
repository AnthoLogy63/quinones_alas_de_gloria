using UnityEngine;

/// <summary>Audio de las mecánicas existentes. Añadir una instancia por escena de nivel.</summary>
[DisallowMultipleComponent]
public class AudioDeNivel : MonoBehaviour
{
    [Header("Bucles")]
    public AudioClip musica;
    public AudioClip ambiente;
    [Range(0f, 1f)] public float volumenMusica = 0.18f;
    [Range(0f, 1f)] public float volumenAmbiente = 0.08f;
    [Range(0f, 1f)] public float volumenEfectos = 0.55f;

    [Header("Mecánicas")]
    public AudioClip disparoJugador;
    public AudioClip disparoEnemigo;
    public AudioClip disparoTorreta;
    public AudioClip impacto;
    public AudioClip recogerCombustible;
    public AudioClip enemigoDestruido;
    public AudioClip combustibleBajo;
    public AudioClip victoria;
    public AudioClip derrota;
    [Min(0f)] public float umbralCombustibleBajo = 2f;

    private AudioSource fuenteMusica;
    private AudioSource fuenteAmbiente;
    private AudioSource fuenteEfectos;
    private int ultimasColisiones;
    private bool avisoCombustibleEmitido;
    private bool terminado;

    void Awake()
    {
        fuenteMusica = CrearFuente(volumenMusica);
        fuenteAmbiente = CrearFuente(volumenAmbiente);
        fuenteEfectos = CrearFuente(volumenEfectos);
    }

    AudioSource CrearFuente(float volumen)
    {
        AudioSource fuente = gameObject.AddComponent<AudioSource>();
        fuente.playOnAwake = false;
        fuente.spatialBlend = 0f;
        fuente.volume = volumen;
        return fuente;
    }

    void OnEnable()
    {
        EventosDeJuego.NivelIniciado += AlIniciar;
        EventosDeJuego.DisparoRealizado += AlDisparar;
        EventosDeJuego.ColisionesCambiadas += AlCambiarColisiones;
        EventosDeJuego.ObjetoRecolectado += AlRecolectar;
        EventosDeJuego.EnemigoDestruido += AlDestruirEnemigo;
        EventosDeJuego.CombustibleCambiado += AlCambiarCombustible;
        EventosDeJuego.NivelCompletado += AlCompletar;
        EventosDeJuego.NivelFallido += AlFallar;
        AlIniciar();
    }

    void OnDisable()
    {
        EventosDeJuego.NivelIniciado -= AlIniciar;
        EventosDeJuego.DisparoRealizado -= AlDisparar;
        EventosDeJuego.ColisionesCambiadas -= AlCambiarColisiones;
        EventosDeJuego.ObjetoRecolectado -= AlRecolectar;
        EventosDeJuego.EnemigoDestruido -= AlDestruirEnemigo;
        EventosDeJuego.CombustibleCambiado -= AlCambiarCombustible;
        EventosDeJuego.NivelCompletado -= AlCompletar;
        EventosDeJuego.NivelFallido -= AlFallar;
        if (fuenteMusica != null) fuenteMusica.Stop();
        if (fuenteAmbiente != null) fuenteAmbiente.Stop();
        if (fuenteEfectos != null) fuenteEfectos.Stop();
    }

    void AlIniciar()
    {
        terminado = false;
        ultimasColisiones = 0;
        avisoCombustibleEmitido = false;
        PrepararBucle(fuenteMusica, musica);
        PrepararBucle(fuenteAmbiente, ambiente);
    }

    void PrepararBucle(AudioSource fuente, AudioClip clip)
    {
        fuente.clip = clip;
        fuente.loop = true;
        if (clip != null && isActiveAndEnabled && !fuente.isPlaying)
            fuente.Play();
    }

    void Reproducir(AudioClip clip)
    {
        if (clip != null && isActiveAndEnabled)
            fuenteEfectos.PlayOneShot(clip);
    }

    void AlDisparar(GameObject emisor)
    {
        if (terminado || emisor == null) return;
        if (emisor.GetComponent<NaveJugador>() != null)
            Reproducir(disparoJugador);
        else
            Reproducir(emisor.GetComponent<MovimientoPatrulla>() != null ? disparoEnemigo : disparoTorreta);
    }

    void AlCambiarColisiones(int actuales, int maximo)
    {
        // El contador solo aumenta por daño válido, respetando la invulnerabilidad.
        if (!terminado && actuales > ultimasColisiones)
            Reproducir(impacto);
        ultimasColisiones = actuales;
    }

    void AlRecolectar(GameObject objeto)
    {
        if (!terminado && objeto != null && objeto.GetComponent<RecolectableCombustible>() != null)
            Reproducir(recogerCombustible);
    }

    void AlDestruirEnemigo(GameObject enemigo)
    {
        if (!terminado) Reproducir(enemigoDestruido);
    }

    void AlCambiarCombustible(float actual, float maximo)
    {
        GestorDeNivel gestor = GestorDeNivel.Instancia;
        if (terminado || gestor == null || gestor.reglas == null || gestor.reglas.consumoPorSegundo <= 0f)
            return;
        if (actual > umbralCombustibleBajo)
            avisoCombustibleEmitido = false;
        else if (!avisoCombustibleEmitido && actual > 0f)
        {
            avisoCombustibleEmitido = true;
            Reproducir(combustibleBajo);
        }
    }

    void AlCompletar(ResultadoNivel resultado) => Terminar(victoria);
    void AlFallar(MotivoDerrota motivo) => Terminar(derrota);

    void Terminar(AudioClip resultado)
    {
        if (terminado) return;
        terminado = true;
        fuenteMusica.Stop();
        fuenteAmbiente.Stop();
        fuenteEfectos.Stop();
        Reproducir(resultado);
    }
}
