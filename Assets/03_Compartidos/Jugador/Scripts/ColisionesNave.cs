using System.Collections;
using UnityEngine;

/// <summary>
/// Cuenta colisiones de la nave. Los impactos peligrosos restan puntaje, suman a
/// la cuenta (máximo 5 por reglas) y dan un breve periodo de invulnerabilidad.
/// Los proyectiles (triggers) llaman a <see cref="RegistrarImpacto"/> directamente.
/// </summary>
public class ColisionesNave : MonoBehaviour
{
    [Tooltip("Cómo tratar objetos sin ObjetoColisionable ni tag.")]
    public TipoColision tipoPorDefecto = TipoColision.Segura;

    public int Actuales { get; private set; }
    public int Maximo { get; private set; }
    public bool Habilitado { get; set; } = true;

    private ReglasDeJuego reglas;
    private PuntajeNave puntaje;
    private float finInvulnerabilidad;
    private Renderer[] renderers;

    void Awake()
    {
        puntaje = GetComponent<PuntajeNave>();
        renderers = GetComponentsInChildren<Renderer>();
    }

    public void Inicializar(ReglasDeJuego reglasDeJuego)
    {
        reglas = reglasDeJuego;
        Maximo = reglas.maximoColisiones;
        Actuales = 0;
        finInvulnerabilidad = 0f;
        EventosDeJuego.RaiseColisiones(Actuales, Maximo);
    }

    void OnCollisionEnter(Collision collision)
    {
        TipoColision tipo = ObjetoColisionable.Clasificar(collision.gameObject, tipoPorDefecto);
        RegistrarImpacto(tipo, collision.gameObject);
    }

    /// <returns>true si el impacto se contó como colisión.</returns>
    public bool RegistrarImpacto(TipoColision tipo, GameObject origen)
    {
        if (!Habilitado || reglas == null)
            return false;

        EventosDeJuego.RaiseNaveColisiono(tipo, origen);

        if (tipo == TipoColision.Segura || Time.time < finInvulnerabilidad)
            return false;

        Actuales++;
        finInvulnerabilidad = Time.time + reglas.invulnerabilidadTrasImpacto;
        puntaje.Restar(reglas.puntajePerdidoPorColision);
        EventosDeJuego.RaiseColisiones(Actuales, Maximo);
        StartCoroutine(Parpadeo(reglas.invulnerabilidadTrasImpacto));
        return true;
    }

    // Retroalimentación mínima; el equipo de efectos puede reemplazarla por la suya.
    private IEnumerator Parpadeo(float duracion)
    {
        float fin = Time.time + duracion;
        bool visible = true;
        while (Time.time < fin)
        {
            visible = !visible;
            foreach (Renderer r in renderers)
                r.enabled = visible;
            yield return new WaitForSeconds(0.08f);
        }
        foreach (Renderer r in renderers)
            r.enabled = true;
    }
}
