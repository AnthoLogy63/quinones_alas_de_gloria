using UnityEngine;

[DefaultExecutionOrder(10000)]
public class SeguirAvion : MonoBehaviour
{
    public Transform objetivo;

    [Header("Seguimiento y zoom por velocidad")]
    public float distanciaInicial = 16f;
    public float distanciaMaxima = 26f;
    public float alturaInicial = 2.5f;
    public float alturaMaxima = 4f;
    public float velocidadParaZoomMaximo = 25f;
    public float suavizado = 0.18f;

    private Rigidbody rigidBodyObjetivo;
    private float velocidadSuavizadoY;
    private float velocidadSuavizadoZ;

    void Start()
    {
        if (objetivo == null)
        {
            GameObject avion = GameObject.Find("AvionRoot");
            if (avion != null)
                objetivo = avion.transform;
        }

        if (objetivo == null)
            return;

        rigidBodyObjetivo = objetivo.GetComponent<Rigidbody>();
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
        {
            GameObject avion = GameObject.Find("AvionRoot");
            if (avion != null)
            {
                objetivo = avion.transform;
                rigidBodyObjetivo = objetivo.GetComponent<Rigidbody>();
            }
        }

        if (objetivo == null)
            return;

        if (rigidBodyObjetivo == null)
            rigidBodyObjetivo = objetivo.GetComponent<Rigidbody>();

        // Evita valores cero (posible al añadir estos campos a una escena antigua).
        float distanciaCerca = distanciaInicial > 0.1f ? distanciaInicial : 16f;
        float distanciaLejos = distanciaMaxima > distanciaCerca
            ? distanciaMaxima
            : 26f;
        float alturaCerca = alturaInicial > 0.1f ? alturaInicial : 2.5f;
        float alturaLejos = alturaMaxima > alturaCerca ? alturaMaxima : 4f;
        float velocidadZoom = velocidadParaZoomMaximo > 0.1f
            ? velocidadParaZoomMaximo
            : 25f;
        float tiempoSuavizado = suavizado > 0.01f ? suavizado : 0.18f;

        float velocidad = rigidBodyObjetivo != null
            ? Mathf.Max(0f, rigidBodyObjetivo.linearVelocity.x)
            : 0f;
        float progresoZoom = Mathf.Clamp01(velocidad / velocidadZoom);
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
}
