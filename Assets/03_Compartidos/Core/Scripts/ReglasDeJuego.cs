using UnityEngine;

/// <summary>
/// Reglas comunes a los 10 niveles (requisitos del curso). Es un ScriptableObject
/// para que diseño de niveles pueda ajustar valores sin tocar código.
/// Crear desde: Assets > Create > Alas de Gloria > Reglas de juego.
/// </summary>
[CreateAssetMenu(fileName = "ReglasDeJuego", menuName = "Alas de Gloria/Reglas de juego")]
public class ReglasDeJuego : ScriptableObject
{
    [Header("Puntaje (porcentaje)")]
    public float puntajeInicial = 100f;
    public float puntajePerdidoPorColision = 20f; // 100 % / 5 colisiones

    [Header("Colisiones")]
    public int maximoColisiones = 5;
    [Tooltip("Segundos sin recibir más daño después de un impacto peligroso.")]
    public float invulnerabilidadTrasImpacto = 1f;

    [Header("Combustible")]
    public float combustibleInicial = 10f;
    [Tooltip("Tope al acumular sobrante de niveles anteriores.")]
    public float combustibleMaximo = 100f;
    public float consumoPorSegundo = 0.5f;
    [Tooltip("Si es true, el combustible sobrante pasa al siguiente nivel.")]
    public bool acumularSobrante = true;

    [Header("Fin de nivel")]
    [Tooltip("Segundos sin combustible y casi detenido antes de dar el nivel por fallido.")]
    public float segundosSinCombustibleParaFallar = 2f;
    public float alturaMinimaFueraDeLimites = -20f;

    private static ReglasDeJuego defecto;

    /// <summary>Reglas por defecto en memoria, usadas si nadie asignó un asset.</summary>
    public static ReglasDeJuego Defecto
    {
        get
        {
            if (defecto == null)
            {
                defecto = CreateInstance<ReglasDeJuego>();
                defecto.name = "ReglasDeJuego (defecto)";
            }
            return defecto;
        }
    }
}
