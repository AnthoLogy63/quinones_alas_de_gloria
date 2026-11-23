# Mecánicas (área 4) — guía para el resto del equipo

Probar: menú **Alas de Gloria > Crear escena de pruebas del avión** → abre `Assets/Scenes/PruebasAvion.unity` → Play.

## Estructura
| Carpeta | Contenido |
|---|---|
| `Core/` | `ReglasDeJuego` (valores del curso), `EventosDeJuego` (bus de eventos), `ProgresoJugador`, `ObjetoColisionable`, `IDanable` |
| `Jugador/` | `NaveJugador` (punto de entrada), `ControlDeNave`, `DisparoNave`, `SeguirAvion` (cámara) |
| `Sistemas/` | `CombustibleNave`, `PuntajeNave`, `ColisionesNave` |
| `Recolectables/` | `Recolectable` (base), `RecolectableCombustible` |
| `Enemigos/` | `Salud`, `Proyectil`, `Disparador`, `DisparadorAutomatico`, `MovimientoPatrulla` |
| `Niveles/` | `GestorDeNivel` (victoria/derrota), `PlataformaDeLlegada` |
| `Pruebas/` | `HudDePruebas` (provisional) |

## Por área
- **Niveles (1):** en cada escena poner `GestorDeNivel` + la nave (`AvionRoot` con `NaveJugador`) + `PlataformaDeLlegada`.
  Obstáculos: añadir `ObjetoColisionable` (Peligrosa = resta 20 % y cuenta colisión; Segura = no penaliza).
  Móviles: `MovimientoPatrulla`. Combustible: prefab con `RecolectableCombustible`.
  Enemigos: `Salud` + `Disparador` + `DisparadorAutomatico`.
- **Interfaces (2):** suscribirse en `OnEnable` / desuscribirse en `OnDisable` a `EventosDeJuego`:
  `CombustibleCambiado`, `PuntajeCambiado`, `ColisionesCambiadas`, `NivelCompletado`, `NivelFallido`.
  Botones: `GestorDeNivel.Instancia.ReiniciarNivel()` / `SiguienteNivel()`; en "Jugar" llamar `ProgresoJugador.NuevaPartida()`.
- **Modelos (3):** reemplazar el modelo hijo de `AvionRoot`; los obstáculos solo necesitan collider + `ObjetoColisionable`.
- **Audio/pruebas (5):** `EventosDeJuego.MotorCambiado`, `NaveColisiono`, `DisparoRealizado`, `EnemigoDestruido`, `ObjetoRecolectado`.

## Reglas (editar `Assets/Data/ReglasDeJuego.asset`)
Puntaje 100 %, 5 colisiones máx. (−20 % c/u), 10 de combustible al inicio, consumo 0.5/s con motor encendido,
sobrante acumulable al siguiente nivel (tope 100). Pendiente confirmar la regla exacta de acumulación con el docente.

## Controles
D acelerar · Espacio elevar · A frenar · J disparar (niveles de combate) · R reiniciar nivel · N nueva partida.
