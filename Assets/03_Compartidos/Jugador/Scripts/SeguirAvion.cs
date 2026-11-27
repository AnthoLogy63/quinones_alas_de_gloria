using UnityEngine;

/// <summary>
/// Cámara lateral que sigue al avión. El zoom depende de la altura sobre el suelo:
/// cerca del suelo se acerca para ver bien el despegue y el aterrizaje, y se aleja
/// (sin pasarse) a medida que el avión gana altura.
/// </summary>
[DefaultExecutionOrder(10000)]
public class SeguirAvion : MonoBehaviour
{
    public Transform objetivo;

    [Header("Seguimiento y zoom por altura")]
    public float distanciaInicial = 16f;
    public float distanciaMaxima = 26f;
    public float alturaInicial = 2.5f;
    public float alturaMaxima = 4f;
    [Tooltip("Altura sobre el suelo (m) a la que la cámara llega a su distancia máxima.")]
    public float alturaParaZoomMaximo = 40f;
    public float suavizado = 0.18f;
    [Tooltip("Suavizado del zoom (s). Más alto = el zoom reacciona más despacio.")]
    public float suavizadoZoom = 0.8f;

    private const float AlcanceDelSuelo = 300f;

    private float velocidadSuavizadoY;
    private float velocidadSuavizadoZ;
    private float velocidadSuavizadoZoom;
    private float progresoZoom;
    private float alturaReferencia;

    void Start()
    {
        BuscarObjetivo();
        if (objetivo == null)
            return;

        alturaReferencia = objetivo.position.y;
        float distanciaInicio = distanciaInicial > 0.1f ? distanciaInicial : 16f;
        float alturaInicio = alturaInicial > 0.1f ? alturaInicial : 2.5f;

        // Arranca ya cerca del avion, sin esperar a que la camara alcance su offset.
        transform.position = objetivo.position + new Vector3(0f, alturaInicio, -distanciaInicio);
        transform.rotation = Quaternion.identity;
    }

    void LateUpdate()
    {
        // Reintenta encontrarlo por si el objetivo no quedó serializado en la escena.
        if (objetivo == null)
            BuscarObjetivo();
        if (objetivo == null)
            return;

        // Evita valores cero (posible al añadir estos campos a una escena antigua).
        float distanciaCerca = distanciaInicial > 0.1f ? distanciaInicial : 16f;
        float distanciaLejos = distanciaMaxima > distanciaCerca ? distanciaMaxima : distanciaCerca + 10f;
        float alturaCerca = alturaInicial > 0.1f ? alturaInicial : 2.5f;
        float alturaLejos = alturaMaxima > alturaCerca ? alturaMaxima : alturaCerca + 1.5f;
        float alturaZoom = alturaParaZoomMaximo > 1f ? alturaParaZoomMaximo : 40f;
        float tiempoSuavizado = suavizado > 0.01f ? suavizado : 0.18f;
        float tiempoZoom = suavizadoZoom > 0.01f ? suavizadoZoom : 0.8f;

        float objetivoZoom = Mathf.Clamp01(AlturaSobreElSuelo() / alturaZoom);
        progresoZoom = Mathf.SmoothDamp(progresoZoom, objetivoZoom, ref velocidadSuavizadoZoom, tiempoZoom);

        float distancia = Mathf.Lerp(distanciaCerca, distanciaLejos, progresoZoom);
        float altura = Mathf.Lerp(alturaCerca, alturaLejos, progresoZoom);

        // El eje X sigue al avión directamente para que nunca se quede atrás.
        Vector3 posicion = transform.position;
        posicion.x = objetivo.position.x;
        posicion.y = Mathf.SmoothDamp(
            posicion.y,
            objetivo.position.y + altura,
            ref velocidadSuavizadoY,
            tiempoSuavizado
        );
        posicion.z = Mathf.SmoothDamp(
            posicion.z,
            objetivo.position.z - distancia,
            ref velocidadSuavizadoZ,
            tiempoSuavizado
        );
        transform.position = posicion;
        transform.rotation = Quaternion.identity;
    }

    void BuscarObjetivo()
    {
        if (objetivo != null)
            return;

        GameObject avion = GameObject.Find("AvionRoot");
        if (avion != null)
            objetivo = avion.transform;
    }

    /// <summary>
    /// Distancia vertical hasta lo que haya debajo del avión (ignorando al propio avión
    /// y los triggers). Si no hay nada debajo, usa la altura respecto al punto de partida.
    /// </summary>
    float AlturaSobreElSuelo()
    {
        float masCerca = float.MaxValue;
        RaycastHit[] impactos = Physics.RaycastAll(
            objetivo.position, Vector3.down, AlcanceDelSuelo, ~0, QueryTriggerInteraction.Ignore);

        foreach (RaycastHit impacto in impactos)
        {
            if (impacto.transform.IsChildOf(objetivo))
                continue;
            masCerca = Mathf.Min(masCerca, impacto.distance);
        }

        if (masCerca < float.MaxValue)
            return Mathf.Max(0f, masCerca);

        return Mathf.Max(0f, objetivo.position.y - alturaReferencia);
    }
}
