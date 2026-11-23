using UnityEngine;

/// <summary>Vida de enemigos y objetivos destruibles.</summary>
public class Salud : MonoBehaviour, IDanable
{
    public float vida = 3f;

    public void RecibirDanio(float cantidad, GameObject origen)
    {
        vida -= cantidad;
        if (vida > 0f)
            return;

        EventosDeJuego.RaiseEnemigoDestruido(gameObject);
        Destroy(gameObject);
    }
}
