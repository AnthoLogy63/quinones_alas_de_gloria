using UnityEngine;
using UnityEngine.SceneManagement;

public enum EstadoNivel { Preparando, Jugando, Completado, Fallido }

/// <summary>
/// Reglas de victoria/derrota de un nivel. Cada escena de nivel lleva uno.
/// Diseño de niveles solo coloca obstáculos y la plataforma de llegada;
/// interfaces y audio reaccionan a <see cref="EventosDeJuego"/>.
/// </summary>
[DefaultExecutionOrder(-50)]
public class GestorDeNivel : MonoBehaviour
{
    public static GestorDeNivel Instancia { get; private set; }

    public ReglasDeJuego reglas;
    public NaveJugador nave;

    public EstadoNivel Estado { get; private set; } = EstadoNivel.Preparando;
    public MotivoDerrota UltimaDerrota { get; private set; }
    public ResultadoNivel UltimoResultado { get; private set; }

    private float tiempoSinCombustible;

    void Awake()
    {
        Instancia = this;
        if (reglas == null)
            reglas = ReglasDeJuego.Defecto;
    }

    void OnEnable()
    {
        EventosDeJuego.ColisionesCambiadas += AlCambiarColisiones;
    }

    void OnDisable()
    {
        EventosDeJuego.ColisionesCambiadas -= AlCambiarColisiones;
    }

    void OnDestroy()
    {
        if (Instancia == this)
            Instancia = null;
    }

    void Start()
    {
        if (nave == null)
            nave = FindFirstObjectByType<NaveJugador>();

        if (nave == null)
        {
            Debug.LogError("GestorDeNivel: no hay NaveJugador en la escena.");
            enabled = false;
            return;
        }

        nave.Inicializar(reglas, ProgresoJugador.CombustibleInicialDeNivel(reglas));
        Estado = EstadoNivel.Jugando;
        EventosDeJuego.RaiseNivelIniciado();
    }

    void Update()
    {
        if (Estado != EstadoNivel.Jugando)
            return;

        if (nave.transform.position.y < reglas.alturaMinimaFueraDeLimites)
        {
            Fallar(MotivoDerrota.FueraDeLimites);
            return;
        }

        // Sin combustible y casi detenido (en el suelo) = no puede continuar.
        bool varado = !nave.Combustible.TieneCombustible && nave.Cuerpo.linearVelocity.magnitude < 0.5f;
        tiempoSinCombustible = varado ? tiempoSinCombustible + Time.deltaTime : 0f;
        if (tiempoSinCombustible >= reglas.segundosSinCombustibleParaFallar)
            Fallar(MotivoDerrota.SinCombustible);
    }

    private void AlCambiarColisiones(int actuales, int maximo)
    {
        if (Estado == EstadoNivel.Jugando && actuales >= maximo)
            Fallar(MotivoDerrota.MaximoColisiones);
    }

    public void Completar()
    {
        if (Estado != EstadoNivel.Jugando)
            return;

        Estado = EstadoNivel.Completado;
        nave.Detener();

        UltimoResultado = new ResultadoNivel
        {
            puntaje = nave.Puntaje.Actual,
            colisiones = nave.Colisiones.Actuales,
            combustibleSobrante = nave.Combustible.Actual
        };
        ProgresoJugador.GuardarSobrante(UltimoResultado.combustibleSobrante);
        EventosDeJuego.RaiseNivelCompletado(UltimoResultado);
    }

    public void Fallar(MotivoDerrota motivo)
    {
        if (Estado != EstadoNivel.Jugando)
            return;

        Estado = EstadoNivel.Fallido;
        UltimaDerrota = motivo;
        nave.Detener();
        EventosDeJuego.RaiseNivelFallido(motivo);
    }

    public void ReiniciarNivel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void SiguienteNivel()
    {
        int siguiente = SceneManager.GetActiveScene().buildIndex + 1;
        if (siguiente < SceneManager.sceneCountInBuildSettings)
            SceneManager.LoadScene(siguiente);
        else
            Debug.Log("No hay más niveles en Build Settings.");
    }
}
