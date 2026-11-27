# Audio — Alas de Gloria

Se añadieron 35 audios (5 músicas y 30 efectos/ambientes) organizados por su uso.
`CatalogoAudio.json` guarda la ruta exacta, autor, licencia, adaptación, duración,
GUID y SHA-256 de cada archivo. Todos se descargaron de Kenney u OpenGameArt.

## Dónde encontrar cada sonido

- `01_Interfaces/MenuPrincipal/Audio/Musica`: música del menú.
- `01_Interfaces/MenuPrincipal/Audio/SFX`: hover, clic, confirmar, volver e iniciar.
- `01_Interfaces/Pausa/Audio/SFX`: abrir y cerrar pausa.
- `01_Interfaces/NivelCompletado/Audio`: victoria.
- `01_Interfaces/NivelFallido/Audio`: derrota.
- `01_Interfaces/IntroduccionNarrativa/Audio/Musica`: introducción.
- `01_Interfaces/Creditos/Audio/Musica`: créditos.
- `03_Compartidos/Jugador/Audio`: motor, variantes de vuelo, disparo, impacto,
  aviso de combustible, aterrizaje y explosión del jugador.
- `03_Compartidos/Enemigos/Audio`: disparos de enemigo/torreta y explosión.
- `03_Compartidos/Audio/SFX`: recoger combustible, impacto de bala, choque e impulso.
- `03_Compartidos/Audio/Musica`: música para combate.
- `02_Niveles/Nivel_01/Audio`: música provisional del tutorial y ambiente de viento.

Las rutas anteriores parten de `Assets/`. Los efectos están en WAV PCM16 mono
a 44.1 kHz, con picos a -3 dBFS. La música está en OGG Vorbis estéreo, con
importación en streaming. Los archivos originales descargados y herramientas
de preparación temporal están en `Temp/` y no se suben a Git.

## Qué ya está conectado

`Nivel_01` y `PruebasAvion` tienen un objeto **AudioNivel**, con `AudioDeNivel`:

- Música y viento en fuentes independientes, con volúmenes iniciales bajos.
- Disparo del jugador, del enemigo volador y de la torreta.
- Impactos de daño válido, respetando el contador y la invulnerabilidad existentes.
- Recogida de combustible y destrucción de enemigos.
- Aviso al bajar de 2 unidades de combustible; no avisa en el tutorial sin consumo.
- Jingle de victoria/derrota; detiene música y ambiente al terminar.

El motor nuevo está asignado al prefab `AvionRoot` (usado por `Nivel_01`) y al
avión de `PruebasAvion`, con volumen 0.35. `ControlDeNave` conserva el encendido
y apagado al pulsar/soltar aceleración o elevación. El generador de la escena
de pruebas también asigna estos sonidos al volver a crearla.

## Menú principal

En `01_Interfaces/MenuPrincipal/Prefabs/AudioMenu.prefab` hay un prefab con
`AudioDeMenu` y los clips asignados. La escena `MenuPrincipal.unity` ya incluye
su controlador, música, un AudioListener en la cámara y sonido de hover/clic
en sus cinco controles. `MenuHoverUI` conserva el comportamiento visual y añade
audio; las acciones de navegación siguen siendo responsabilidad de interfaces.
El generador `CrearMenuPrincipal` también conserva esta configuración.

1. En escenas nuevas, arrastrar **AudioMenu**. La música empieza sola. No añadir
   otra instancia al menú actual, que ya tiene una.
2. En `Button > On Click()`, arrastrar ese objeto y elegir
   `AudioDeMenu.Click`, `Confirmar`, `Volver` o `Iniciar`, según el botón.
3. Para hover, añadir un `EventTrigger` con `Pointer Enter` y llamar a
   `AudioDeMenu.Hover`. La selección con teclado puede usar el evento `Select`.
4. `AbrirPausa` y `CerrarPausa` quedan disponibles para la pantalla de pausa.
5. Las músicas de introducción y créditos están listas para sus futuras escenas.

Los sonidos de clic deben reproducirse antes de destruir/cambiar la escena;
la transición de pantalla debe permitir que termine el efecto si se desea oírlo
completo. El prefab no crea transiciones ni cambia escenas. Los cinco controles
actuales usan `MenuHoverUI.SetAudio` y ya están enlazados; no añadirles un segundo
EventTrigger/OnClick para el mismo efecto, porque se duplicaría el sonido.

## Variantes y efectos aún sin disparador

Encendido, acelerar, desacelerar, apagado y despegue son variantes diseñadas del
mismo bucle de hélice, con cambios de tono y envolventes; no son grabaciones
independientes de esas maniobras. La torreta usa una variante grave del disparo
de ametralladora como efecto provisional, y el impulso se diseñó con viento.

Las variantes de motor, aterrizaje, explosión del jugador, impacto de bala,
choque adicional, ráfaga e impulso están disponibles en las carpetas, sin
inventar eventos nuevos ni activar mecánicas que todavía no existen. El motor
continuo, el daño y el resto de eventos enumerados arriba sí están enlazados.
La selección requiere una escucha en Play Mode para ajustar la mezcla al gusto.

## Créditos y licencias

Incluir el contenido de **CREDITOS_AUDIO.txt** en los créditos o documentación
que se distribuya con el juego. El motor, la introducción y los disparos usan
licencias **CC BY**, que requieren atribución; los demás audios seleccionados
son CC0. El catálogo identifica cada caso y los cambios realizados.

`propulsor.mp3` era del proyecto y se conserva como respaldo. Su procedencia
no está documentada por esta incorporación.

## Verificación

`Tests/Audio/VerificarAudio.ps1` importa una copia mínima en `Temp/AudioValidation`
y prueba los controladores y los 35 AudioClips con un Unity instalado, sin abrir
el proyecto original. Se usa el editor declarado si está disponible o se puede
pasar `-UnityEditorPath` explícitamente. No sustituye una prueba auditiva en juego.
