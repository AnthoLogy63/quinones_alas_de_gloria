using UnityEngine;

/// <summary>Puntaje en porcentaje: empieza en 100 % y baja con cada colisión peligrosa.</summary>
public class PuntajeNave : MonoBehaviour
{
    public float Actual { get; private set; }

    private float maximo = 100f;

    public void Inicializar(ReglasDeJuego reglas)
    {
        maximo = reglas.puntajeInicial;
        Actual = reglas.puntajeInicial;
        EventosDeJuego.RaisePuntaje(Actual);
    }

    public void Restar(float cantidad) => Establecer(Actual - cantidad);

    /// <summary>Bonificaciones (p. ej. enemigos derribados). Nunca supera el puntaje inicial.</summary>
    public void Sumar(float cantidad) => Establecer(Actual + cantidad);

    private void Establecer(float valor)
    {
        Actual = Mathf.Clamp(valor, 0f, maximo);
        EventosDeJuego.RaisePuntaje(Actual);
    }
}
