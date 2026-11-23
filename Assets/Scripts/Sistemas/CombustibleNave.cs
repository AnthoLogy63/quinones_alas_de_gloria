using UnityEngine;

/// <summary>Combustible de la nave. Se consume mientras el motor está encendido.</summary>
public class CombustibleNave : MonoBehaviour
{
    public float Actual { get; private set; }
    public float Maximo { get; private set; }
    public bool TieneCombustible => Actual > 0f;

    private ReglasDeJuego reglas;

    public void Inicializar(ReglasDeJuego reglasDeJuego, float cantidadInicial)
    {
        reglas = reglasDeJuego;
        Maximo = reglas.combustibleMaximo;
        Actual = Mathf.Clamp(cantidadInicial, 0f, Maximo);
        EventosDeJuego.RaiseCombustible(Actual, Maximo);
    }

    /// <summary>Gasta combustible durante un intervalo de tiempo con el motor encendido.</summary>
    public void Consumir(float segundos)
    {
        if (reglas == null || Actual <= 0f)
            return;

        Actual = Mathf.Max(0f, Actual - reglas.consumoPorSegundo * segundos);
        EventosDeJuego.RaiseCombustible(Actual, Maximo);
    }

    public void Agregar(float cantidad)
    {
        Actual = Mathf.Clamp(Actual + cantidad, 0f, Maximo);
        EventosDeJuego.RaiseCombustible(Actual, Maximo);
    }
}
