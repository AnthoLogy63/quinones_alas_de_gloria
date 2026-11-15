# Proyecto Nave (ProyectoNave_2026B)

Juego/simulador de control y físicas de una nave espacial desarrollado en **Unity**, enfocado en el manejo de propulsión, inercia, rotación y detección de colisiones.

---

## 🛠️ Requisitos previos

- **Unity Editor:** Versión `2022.3.49f1` (LTS recomendada) o superior.
- **Unity Hub** instalado en tu sistema.
- Módulos requeridos: Soporte de compilación para tu sistema operativo (Windows / Mac / Linux).

---

## 🚀 Cómo abrir y ejecutar el proyecto

1. **Clonar o descargar el repositorio:**
   ```bash
   git clone <URL_DEL_REPOSITORIO>
   ```
2. **Abrir con Unity Hub:**
   - Abre **Unity Hub**.
   - Haz clic en el botón **Add** (Añadir) > **Add project from disk**.
   - Selecciona la carpeta raíz de este repositorio (`ProyectoNave_2026B`).
   - Asegúrate de seleccionar el editor **Unity 2022.3.49f1** (o la versión compatible instalada).
3. **Cargar la escena:**
   - En la ventana del *Project* de Unity, navega a:
     `Assets/Scenes/`
   - Abre la escena principal: **`SampleScene.unity`** (o las escenas de prueba `Escena1`, `Escena2`, `Escena3`).
4. **Jugar:**
   - Pulsa el botón **Play** (▶️) en la parte superior del editor.

> **Nota:** Al abrir el proyecto por primera vez, Unity tardará un par de minutos en regenerar automáticamente la carpeta `Library/`. Esto es normal y mantiene el repositorio ligero.

---

## 🎮 Controles del Juego

| Tecla / Acción | Función |
| :--- | :--- |
| **Espacio (`Space`)** | Activa la propulsión (empuje hacia adelante) y reproduce el sonido del motor. |
| **A** | Rota la nave hacia la izquierda. |
| **D** | Rota la nave hacia la derecha. |

*(Nota: La rotación se bloquea momentáneamente mientras se mantiene activa la propulsión).*

---

## 📂 Estructura del Proyecto

Solo se rastrean en Git los directorios esenciales para el funcionamiento de Unity:

- **`Assets/`**: Modelos, materiales, prefabs, sonidos, escenas y scripts (`ControlDeNave.cs`).
- **`Packages/`**: Lista de dependencias del Package Manager (`manifest.json`).
- **`ProjectSettings/`**: Configuración de físicas, capas, tags, gráficos y versión del editor.

Carpetas temporales y de caché como `Library/`, `Temp/`, `Logs/` y `UserSettings/` están excluidas mediante el `.gitignore` estándar para evitar subir archivos pesados o conflictos entre usuarios.
