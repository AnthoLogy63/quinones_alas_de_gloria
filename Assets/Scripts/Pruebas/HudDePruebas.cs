using UnityEngine;

/// <summary>
/// HUD provisional con OnGUI solo para probar las mecánicas. El área de
/// interfaces lo reemplaza con su HUD real suscribiéndose a EventosDeJuego.
/// Teclas: R reinicia el nivel, N reinicia la partida completa.
/// </summary>
public class HudDePruebas : MonoBehaviour
{
    private float combustible, combustibleMax, puntaje;
    private int colisiones, colisionesMax;
    private string mensaje = "";
    private GUIStyle estilo;

    void OnEnable()
    {
        EventosDeJuego.CombustibleCambiado += AlCambiarCombustible;
        EventosDeJuego.PuntajeCambiado += AlCambiarPuntaje;
        EventosDeJuego.ColisionesCambiadas += AlCambiarColisiones;
        EventosDeJuego.NivelCompletado += AlCompletar;
        EventosDeJuego.NivelFallido += AlFallar;
    }

    void OnDisable()
    {
        EventosDeJuego.CombustibleCambiado -= AlCambiarCombustible;
        EventosDeJuego.PuntajeCambiado -= AlCambiarPuntaje;
        EventosDeJuego.ColisionesCambiadas -= AlCambiarColisiones;
        EventosDeJuego.NivelCompletado -= AlCompletar;
        EventosDeJuego.NivelFallido -= AlFallar;
    }

    void AlCambiarCombustible(float actual, float max) { combustible = actual; combustibleMax = max; }
    void AlCambiarPuntaje(float valor) { puntaje = valor; }
    void AlCambiarColisiones(int actuales, int max) { colisiones = actuales; colisionesMax = max; }

    void AlCompletar(ResultadoNivel r)
    {
        mensaje = $"NIVEL COMPLETADO\nPuntaje {r.puntaje:0}%  Sobrante {r.combustibleSobrante:0.0}\n[R] repetir  [N] nueva partida";
    }

    void AlFallar(MotivoDerrota motivo)
    {
        mensaje = $"NIVEL FALLIDO: {motivo}\n[R] reintentar";
    }

    void Update()
    {
        GestorDeNivel gestor = GestorDeNivel.Instancia;
        if (gestor == null)
            return;

        if (Input.GetKeyDown(KeyCode.R))
            gestor.ReiniciarNivel();
        if (Input.GetKeyDown(KeyCode.N))
        {
            ProgresoJugador.NuevaPartida();
            gestor.ReiniciarNivel();
        }
    }

    void OnGUI()
    {
        if (estilo == null)
        {
            estilo = new GUIStyle(GUI.skin.label) { fontSize = 20, fontStyle = FontStyle.Bold };
            estilo.normal.textColor = Color.white;
        }

        GUI.Label(new Rect(15, 10, 500, 30), $"Combustible: {combustible:0.0} / {combustibleMax:0}", estilo);
        GUI.Label(new Rect(15, 38, 500, 30), $"Puntaje: {puntaje:0}%", estilo);
        GUI.Label(new Rect(15, 66, 500, 30), $"Colisiones: {colisiones} / {colisionesMax}", estilo);
        GUI.Label(new Rect(15, 94, 700, 30), "D acelerar | Espacio elevar | A frenar | J disparar", estilo);

        if (mensaje != "")
            GUI.Label(new Rect(Screen.width / 2f - 250, Screen.height / 2f - 60, 500, 120), mensaje, estilo);
    }
}
