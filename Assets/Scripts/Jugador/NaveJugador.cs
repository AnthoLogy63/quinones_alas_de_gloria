using UnityEngine;

/// <summary>
/// Punto de entrada único de la nave del jugador. Cualquier otro sistema
/// (recolectables, proyectiles, plataforma de llegada, HUD) obtiene lo que
/// necesita con GetComponentInParent&lt;NaveJugador&gt;() sin conocer los detalles.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(ControlDeNave))]
[RequireComponent(typeof(CombustibleNave))]
[RequireComponent(typeof(PuntajeNave))]
[RequireComponent(typeof(ColisionesNave))]
public class NaveJugador : MonoBehaviour
{
    public Rigidbody Cuerpo { get; private set; }
    public ControlDeNave Control { get; private set; }
    public CombustibleNave Combustible { get; private set; }
    public PuntajeNave Puntaje { get; private set; }
    public ColisionesNave Colisiones { get; private set; }

    void Awake()
    {
        Cuerpo = GetComponent<Rigidbody>();
        Control = GetComponent<ControlDeNave>();
        Combustible = GetComponent<CombustibleNave>();
        Puntaje = GetComponent<PuntajeNave>();
        Colisiones = GetComponent<ColisionesNave>();
    }

    /// <summary>Prepara todos los sistemas para empezar un nivel.</summary>
    public void Inicializar(ReglasDeJuego reglas, float combustibleInicial)
    {
        Combustible.Inicializar(reglas, combustibleInicial);
        Puntaje.Inicializar(reglas);
        Colisiones.Inicializar(reglas);
        Control.ControlHabilitado = true;
        Colisiones.Habilitado = true;
    }

    /// <summary>Congela controles y daño al terminar el nivel.</summary>
    public void Detener()
    {
        Control.ControlHabilitado = false;
        Colisiones.Habilitado = false;
    }
}
