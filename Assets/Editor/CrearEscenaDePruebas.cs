using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Genera la escena "PruebasAvion" a partir de PruebasMecanicas (conserva el
/// avión y la cámara) y le añade todas las mecánicas: combustible, colisiones,
/// recolectables, obstáculos fijos y móviles, torreta, enemigo y llegada.
/// Menú: Alas de Gloria > Crear escena de pruebas del avión.
/// </summary>
public static class CrearEscenaDePruebas
{
    const string EscenaOrigen = "Assets/Scenes/PruebasMecanicas.unity";
    const string EscenaDestino = "Assets/Scenes/PruebasAvion.unity";
    const string RutaReglas = "Assets/Data/ReglasDeJuego.asset";
    const string RutaProyectil = "Assets/Prefabs/Obstaculos/Proyectil.prefab";
    const string RaizNombre = "--- PRUEBAS (generado) ---";
    const float Z = 0.5f; // plano de juego (la nave tiene Z congelado)

    [MenuItem("Alas de Gloria/Crear escena de pruebas del avión")]
    public static void Crear()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        EditorSceneManager.OpenScene(EscenaOrigen, OpenSceneMode.Single);
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), EscenaDestino, false);

        ReglasDeJuego reglas = ObtenerReglas();
        Proyectil proyectil = ObtenerPrefabProyectil();

        GameObject viejo = GameObject.Find(RaizNombre);
        if (viejo != null)
            Object.DestroyImmediate(viejo);

        GameObject raiz = new GameObject(RaizNombre);

        NaveJugador nave = ConfigurarNave(proyectil);
        ConfigurarSuelo();

        GestorDeNivel gestor = new GameObject("GestorDeNivel").AddComponent<GestorDeNivel>();
        gestor.transform.SetParent(raiz.transform);
        gestor.reglas = reglas;
        gestor.nave = nave;
        gestor.gameObject.AddComponent<HudDePruebas>();

        CrearEscenario(raiz.transform, proyectil);

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();
        Debug.Log("Escena de pruebas creada: " + EscenaDestino);
    }

    // ---------------------------------------------------------------- nave

    static NaveJugador ConfigurarNave(Proyectil proyectil)
    {
        GameObject avion = GameObject.Find("AvionRoot");
        if (avion == null)
            throw new System.Exception("No se encontró AvionRoot en " + EscenaOrigen);

        NaveJugador nave = avion.GetComponent<NaveJugador>();
        if (nave == null)
            nave = avion.AddComponent<NaveJugador>();

        AudioSource audio = avion.GetComponent<AudioSource>();
        if (audio == null)
            audio = avion.AddComponent<AudioSource>();
        audio.playOnAwake = false;
        audio.loop = true;
        audio.clip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/sonido/propulsor.mp3");

        Disparador disparador = avion.GetComponent<Disparador>();
        if (disparador == null)
            disparador = avion.AddComponent<Disparador>();
        disparador.prefabProyectil = proyectil;
        disparador.daniaAlJugador = false;
        disparador.daniaAEnemigos = true;
        disparador.velocidadProyectil = 40f;
        disparador.segundosEntreDisparos = 0.25f;
        disparador.distanciaDeSalida = 3f;

        if (avion.GetComponent<DisparoNave>() == null)
            avion.AddComponent<DisparoNave>();

        return nave;
    }

    static void ConfigurarSuelo()
    {
        GameObject suelo = GameObject.Find("Ground_Collision");
        if (suelo == null)
            return;

        ObjetoColisionable marca = suelo.GetComponent<ObjetoColisionable>();
        if (marca == null)
            marca = suelo.AddComponent<ObjetoColisionable>();
        marca.tipo = TipoColision.Segura;
    }

    // ------------------------------------------------------------ escenario

    static void CrearEscenario(Transform raiz, Proyectil proyectil)
    {
        Material matObstaculo = CrearMaterial("Prueba_Obstaculo", new Color(0.55f, 0.35f, 0.2f));
        Material matCombustible = CrearMaterial("Prueba_Combustible", new Color(1f, 0.85f, 0.1f));
        Material matMovil = CrearMaterial("Prueba_Movil", new Color(0.9f, 0.4f, 0.1f));
        Material matEnemigo = CrearMaterial("Prueba_Enemigo", new Color(0.8f, 0.1f, 0.1f));
        Material matLlegada = CrearMaterial("Prueba_Llegada", new Color(0.2f, 0.8f, 0.3f));

        Transform obstaculos = Grupo(raiz, "Obstaculos");
        Transform recolectables = Grupo(raiz, "Recolectables");
        Transform enemigos = Grupo(raiz, "Enemigos");

        // Combustible
        for (int i = 0; i < 4; i++)
        {
            GameObject c = Forma(PrimitiveType.Sphere, "Combustible_" + (i + 1), recolectables,
                new Vector3(35f + i * 40f, 6f + (i % 2) * 4f, Z), Vector3.one * 1.5f, matCombustible);
            c.AddComponent<RecolectableCombustible>();
        }

        // Obstáculos fijos peligrosos (suben desde el suelo)
        float[] posX = { 55f, 95f, 135f };
        float[] alto = { 8f, 12f, 9f };
        for (int i = 0; i < posX.Length; i++)
        {
            GameObject o = Forma(PrimitiveType.Cube, "ObstaculoFijo_" + (i + 1), obstaculos,
                new Vector3(posX[i], alto[i] * 0.5f - 0.1f, Z), new Vector3(3f, alto[i], 3f), matObstaculo);
            Peligroso(o);
        }

        // Obstáculo móvil
        GameObject movil = Forma(PrimitiveType.Cube, "ObstaculoMovil", obstaculos,
            new Vector3(75f, 8f, Z), new Vector3(3f, 3f, 3f), matMovil);
        Peligroso(movil);
        MovimientoPatrulla patrulla = movil.AddComponent<MovimientoPatrulla>();
        patrulla.recorrido = new Vector3(0f, 12f, 0f);
        patrulla.velocidad = 1.5f;

        // Torreta antiaérea
        GameObject torreta = Forma(PrimitiveType.Cylinder, "TorretaAntiaerea", enemigos,
            new Vector3(115f, 1f, Z), new Vector3(2f, 1f, 2f), matEnemigo);
        Peligroso(torreta);
        torreta.AddComponent<Salud>().vida = 3f;
        ConfigurarDisparador(torreta, proyectil, 12f, 2f, 40f);

        // Enemigo volador
        GameObject volador = Forma(PrimitiveType.Capsule, "EnemigoVolador", enemigos,
            new Vector3(150f, 10f, Z), new Vector3(2f, 2f, 2f), matEnemigo);
        volador.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
        Peligroso(volador);
        volador.AddComponent<Salud>().vida = 2f;
        MovimientoPatrulla vuelo = volador.AddComponent<MovimientoPatrulla>();
        vuelo.recorrido = new Vector3(0f, 8f, 0f);
        vuelo.velocidad = 1f;
        ConfigurarDisparador(volador, proyectil, 14f, 2.5f, 35f);

        // Plataforma de llegada
        GameObject llegada = Forma(PrimitiveType.Cube, "PlataformaDeLlegada", raiz,
            new Vector3(190f, 1.5f, Z), new Vector3(12f, 3f, 4f), matLlegada);
        llegada.AddComponent<PlataformaDeLlegada>();
        llegada.GetComponent<Collider>().isTrigger = true;
    }

    static void ConfigurarDisparador(GameObject objeto, Proyectil proyectil, float velocidad, float cadencia, float rango)
    {
        Disparador d = objeto.AddComponent<Disparador>();
        d.prefabProyectil = proyectil;
        d.velocidadProyectil = velocidad;
        d.segundosEntreDisparos = cadencia;
        d.daniaAlJugador = true;
        d.daniaAEnemigos = false;
        objeto.AddComponent<DisparadorAutomatico>().rango = rango;
    }

    static void Peligroso(GameObject objeto)
    {
        objeto.AddComponent<ObjetoColisionable>().tipo = TipoColision.Peligrosa;
        objeto.tag = ObjetoColisionable.TagPeligrosa;
    }

    static Transform Grupo(Transform padre, string nombre)
    {
        GameObject g = new GameObject(nombre);
        g.transform.SetParent(padre);
        return g.transform;
    }

    static GameObject Forma(PrimitiveType tipo, string nombre, Transform padre, Vector3 posicion, Vector3 escala, Material material)
    {
        GameObject g = GameObject.CreatePrimitive(tipo);
        g.name = nombre;
        g.transform.SetParent(padre);
        g.transform.position = posicion;
        g.transform.localScale = escala;
        g.GetComponent<Renderer>().sharedMaterial = material;
        return g;
    }

    // --------------------------------------------------------------- assets

    static ReglasDeJuego ObtenerReglas()
    {
        ReglasDeJuego reglas = AssetDatabase.LoadAssetAtPath<ReglasDeJuego>(RutaReglas);
        if (reglas != null)
            return reglas;

        AsegurarCarpeta(Path.GetDirectoryName(RutaReglas));
        reglas = ScriptableObject.CreateInstance<ReglasDeJuego>();
        AssetDatabase.CreateAsset(reglas, RutaReglas);
        return reglas;
    }

    static Proyectil ObtenerPrefabProyectil()
    {
        Proyectil existente = AssetDatabase.LoadAssetAtPath<Proyectil>(RutaProyectil);
        if (existente != null)
            return existente;

        AsegurarCarpeta(Path.GetDirectoryName(RutaProyectil));
        GameObject bala = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        bala.name = "Proyectil";
        bala.transform.localScale = Vector3.one * 0.6f;
        bala.GetComponent<Collider>().isTrigger = true;
        bala.GetComponent<Renderer>().sharedMaterial = CrearMaterial("Prueba_Proyectil", new Color(1f, 0.3f, 0.1f));
        bala.AddComponent<Rigidbody>();
        bala.AddComponent<Proyectil>();

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(bala, RutaProyectil);
        Object.DestroyImmediate(bala);
        return prefab.GetComponent<Proyectil>();
    }

    static Material CrearMaterial(string nombre, Color color)
    {
        string ruta = "Assets/Materials/Pruebas/" + nombre + ".mat";
        Material m = AssetDatabase.LoadAssetAtPath<Material>(ruta);
        if (m == null)
        {
            AsegurarCarpeta("Assets/Materials/Pruebas");
            m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            AssetDatabase.CreateAsset(m, ruta);
        }
        m.SetColor("_BaseColor", color);
        EditorUtility.SetDirty(m);
        return m;
    }

    static void AsegurarCarpeta(string carpeta)
    {
        carpeta = carpeta.Replace('\\', '/');
        if (AssetDatabase.IsValidFolder(carpeta))
            return;

        string padre = Path.GetDirectoryName(carpeta).Replace('\\', '/');
        AsegurarCarpeta(padre);
        AssetDatabase.CreateFolder(padre, Path.GetFileName(carpeta));
    }
}
