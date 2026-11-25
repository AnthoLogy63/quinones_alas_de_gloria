using UnityEngine;

/// <summary>Audio listo para conectar desde OnClick y EventTrigger de la interfaz.</summary>
[DisallowMultipleComponent]
public class AudioDeMenu : MonoBehaviour
{
    public AudioClip musica;
    public AudioClip hover;
    public AudioClip click;
    public AudioClip confirmar;
    public AudioClip volver;
    public AudioClip iniciar;
    public AudioClip abrirPausa;
    public AudioClip cerrarPausa;
    [Range(0f, 1f)] public float volumenMusica = 0.2f;
    [Range(0f, 1f)] public float volumenEfectos = 0.6f;

    private AudioSource fuenteMusica;
    private AudioSource fuenteEfectos;

    void Awake()
    {
        fuenteMusica = gameObject.AddComponent<AudioSource>();
        fuenteMusica.playOnAwake = false;
        fuenteMusica.spatialBlend = 0f;
        fuenteMusica.loop = true;
        fuenteMusica.volume = volumenMusica;
        fuenteEfectos = gameObject.AddComponent<AudioSource>();
        fuenteEfectos.playOnAwake = false;
        fuenteEfectos.spatialBlend = 0f;
        fuenteEfectos.volume = volumenEfectos;
    }

    void OnEnable()
    {
        fuenteMusica.clip = musica;
        if (musica != null && isActiveAndEnabled) fuenteMusica.Play();
    }

    void OnDisable()
    {
        if (fuenteMusica != null) fuenteMusica.Stop();
        if (fuenteEfectos != null) fuenteEfectos.Stop();
    }

    void Reproducir(AudioClip clip)
    {
        if (clip != null && isActiveAndEnabled) fuenteEfectos.PlayOneShot(clip);
    }

    public void Hover() => Reproducir(hover);
    public void Click() => Reproducir(click);
    public void Confirmar() => Reproducir(confirmar);
    public void Volver() => Reproducir(volver);
    public void Iniciar() => Reproducir(iniciar);
    public void AbrirPausa() => Reproducir(abrirPausa);
    public void CerrarPausa() => Reproducir(cerrarPausa);
}
