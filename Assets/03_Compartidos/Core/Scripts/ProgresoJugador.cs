using UnityEngine;

/// <summary>
/// Datos que viajan entre niveles durante una partida (combustible sobrante).
/// Estático para sobrevivir a los cambios de escena. Integración/menús pueden
/// llamar a <see cref="NuevaPartida"/> al pulsar "Jugar".
/// </summary>
public static class ProgresoJugador
{
    public static float CombustibleSobrante { get; private set; }

    public static void NuevaPartida() => CombustibleSobrante = 0f;

    public static void GuardarSobrante(float cantidad) => CombustibleSobrante = Mathf.Max(0f, cantidad);

    public static float CombustibleInicialDeNivel(ReglasDeJuego reglas)
    {
        float extra = reglas.acumularSobrante ? CombustibleSobrante : 0f;
        return Mathf.Min(reglas.combustibleInicial + extra, reglas.combustibleMaximo);
    }
}
