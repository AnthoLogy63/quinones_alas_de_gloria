using UnityEngine;

/// <summary>Combustible coleccionable.</summary>
public class RecolectableCombustible : Recolectable
{
    public float cantidad = 2f;

    protected override bool AlRecolectar(NaveJugador nave)
    {
        nave.Combustible.Agregar(cantidad);
        return true;
    }
}
