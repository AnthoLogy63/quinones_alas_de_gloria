using UnityEngine;

/// <summary>Cualquier cosa que pueda recibir daño de un proyectil.</summary>
public interface IDanable
{
    void RecibirDanio(float cantidad, GameObject origen);
}
