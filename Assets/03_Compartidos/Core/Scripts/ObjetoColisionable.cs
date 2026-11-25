using UnityEngine;

/// <summary>
/// Marca un obstáculo como seguro (suelo, plataforma) o peligroso (resta
/// puntaje y cuenta como colisión). Diseño de niveles solo añade este
/// componente a sus prefabs. También se respetan los tags originales
/// "colisión segura" y "colisión peligrosa".
/// </summary>
public class ObjetoColisionable : MonoBehaviour
{
    public const string TagSegura = "colisión segura";
    public const string TagPeligrosa = "colisión peligrosa";

    public TipoColision tipo = TipoColision.Peligrosa;

    /// <summary>Clasifica un objeto: componente primero, luego tag, luego el valor por defecto.</summary>
    public static TipoColision Clasificar(GameObject objeto, TipoColision porDefecto)
    {
        ObjetoColisionable marca = objeto.GetComponentInParent<ObjetoColisionable>();
        if (marca != null)
            return marca.tipo;

        if (objeto.CompareTag(TagPeligrosa))
            return TipoColision.Peligrosa;
        if (objeto.CompareTag(TagSegura))
            return TipoColision.Segura;

        return porDefecto;
    }
}
