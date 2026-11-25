using System;
using UnityEngine;

public enum TipoColision { Segura, Peligrosa }

public enum MotivoDerrota { MaximoColisiones, SinCombustible, FueraDeLimites }

public struct ResultadoNivel
{
    public float puntaje;
    public int colisiones;
    public float combustibleSobrante;
}

/// <summary>
/// Bus de eventos del juego. Las demás áreas (HUD, audio, efectos, niveles) solo
/// se suscriben aquí en OnEnable y se desuscriben en OnDisable; no necesitan
/// referencias a las clases de mecánicas.
/// </summary>
public static class EventosDeJuego
{
    // Estado del nivel
    public static event Action NivelIniciado;
    public static event Action<ResultadoNivel> NivelCompletado;
    public static event Action<MotivoDerrota> NivelFallido;

    // Valores para el HUD
    public static event Action<float, float> CombustibleCambiado;   // actual, máximo
    public static event Action<float> PuntajeCambiado;              // porcentaje
    public static event Action<int, int> ColisionesCambiadas;       // actuales, máximo

    // Eventos para audio / efectos visuales
    public static event Action<TipoColision, GameObject> NaveColisiono;
    public static event Action<GameObject> DisparoRealizado;
    public static event Action<GameObject> EnemigoDestruido;
    public static event Action<GameObject> ObjetoRecolectado;
    public static event Action<bool> MotorCambiado;                 // encendido

    public static void RaiseNivelIniciado() => NivelIniciado?.Invoke();
    public static void RaiseNivelCompletado(ResultadoNivel r) => NivelCompletado?.Invoke(r);
    public static void RaiseNivelFallido(MotivoDerrota m) => NivelFallido?.Invoke(m);
    public static void RaiseCombustible(float actual, float max) => CombustibleCambiado?.Invoke(actual, max);
    public static void RaisePuntaje(float p) => PuntajeCambiado?.Invoke(p);
    public static void RaiseColisiones(int actuales, int max) => ColisionesCambiadas?.Invoke(actuales, max);
    public static void RaiseNaveColisiono(TipoColision t, GameObject o) => NaveColisiono?.Invoke(t, o);
    public static void RaiseDisparo(GameObject emisor) => DisparoRealizado?.Invoke(emisor);
    public static void RaiseEnemigoDestruido(GameObject e) => EnemigoDestruido?.Invoke(e);
    public static void RaiseRecolectado(GameObject o) => ObjetoRecolectado?.Invoke(o);
    public static void RaiseMotor(bool encendido) => MotorCambiado?.Invoke(encendido);
}
