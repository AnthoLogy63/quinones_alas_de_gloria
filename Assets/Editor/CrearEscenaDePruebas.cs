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
    const string EscenaOrigen = "Assets/02_Niveles/Nivel_00_Pruebas/PruebasMecanicas.unity";
    const string EscenaDestino = "Assets/02_Niveles/Nivel_00_Pruebas/PruebasAvion.unity";
    const string RutaReglas = "Assets/03_Compartidos/Core/Data/ReglasDeJuego.asset";
    const string RutaProyectil = "Assets/03_Compartidos/Enemigos/Prefabs/Proyectil.prefab";
    const string RaizNombre = "--- PRUEBAS (generado) ---";
    const float Z = 0.5f; // plano de juego (la nave tiene Z congelado)
    const float LargoPista = 660f;

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
        ConfigurarAudio(gestor.gameObject);

        CrearEscenario(raiz.transform, proyectil);

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();
        Debug.Log("Escena de pruebas creada: " + EscenaDestino);
    }

    // ---------------------------------------------------------------- nave

    static AudioClip Clip(string ruta) => AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/" + ruta);

    static void ConfigurarAudio(GameObject objeto)
    {
        AudioDeNivel audio = objeto.AddComponent<AudioDeNivel>();
        audio.musica = Clip("03_Compartidos/Audio/Musica/MUS_Combate.ogg");
        audio.ambiente = Clip("02_Niveles/Nivel_01/Audio/Ambiente/AMB_Viento_Loop.wav");
        audio.disparoJugador = Clip("03_Compartidos/Jugador/Audio/SFX_Avion_Disparo.wav");
        audio.disparoEnemigo = Clip("03_Compartidos/Enemigos/Audio/SFX_Enemigo_Disparo.wav");
        audio.disparoTorreta = Clip("03_Compartidos/Enemigos/Audio/SFX_Torreta_Disparo.wav");
        audio.impacto = Clip("03_Compartidos/Jugador/Audio/SFX_Avion_Impacto.wav");
        audio.recogerCombustible = Clip("03_Compartidos/Audio/SFX/SFX_Combustible_Recoger.wav");
        audio.enemigoDestruido = Clip("03_Compartidos/Enemigos/Audio/SFX_Enemigo_Explosion.wav");
        audio.combustibleBajo = Clip("03_Compartidos/Jugador/Audio/SFX_Avion_CombustibleBajo.wav");
        audio.victoria = Clip("01_Interfaces/NivelCompletado/Audio/SFX_Nivel_Victoria.wav");
        audio.derrota = Clip("01_Interfaces/NivelFallido/Audio/SFX_Nivel_Derrota.wav");
    }

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
        audio.volume = 0.35f;
        audio.clip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/03_Compartidos/Jugador/Audio/SFX_Avion_Motor_Loop.wav");

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

        // Pista larga: x de 0 a LargoPista.
        suelo.transform.position = new Vector3(LargoPista * 0.5f, suelo.transform.position.y, suelo.transform.position.z);
        Vector3 escala = suelo.transform.localScale;
        escala.x = LargoPista;
        suelo.transform.localScale = escala;
    }

    // ------------------------------------------------------------ escenario

    // Circuito de pruebas (x crece hacia la derecha; la nave arranca en x=6):
    //   0-100   pista libre para rodar y despegar (combustible a ras de suelo)
    //   130-220 dos pilares (x=175 y x=220): sobrevolarlos, con combustible justo encima
    //   270     obstaculo movil vertical
    //   330-360 blancos en el aire para comprobar que el disparo sale por el morro
    //   400-480 torreta y enemigo volador
    //   540-580 plataforma de aterrizaje (hay que entrar a <= 12 m/s y frenar)
    // El combustible inicial alcanza para ~20 s a plena potencia: si no se recoge
    // ninguno, se acaba hacia la mitad del circuito y solo queda planear.
    static void CrearEscenario(Transform raiz, Proyectil proyectil)
    {
        Material matObstaculo = CrearMaterial("Prueba_Obstaculo", new Color(0.55f, 0.35f, 0.2f));
        Material matCombustible = CrearMaterial("Prueba_Combustible", new Color(1f, 0.85f, 0.1f));
        Material matMovil = CrearMaterial("Prueba_Movil", new Color(0.9f, 0.4f, 0.1f));
        Material matEnemigo = CrearMaterial("Prueba_Enemigo", new Color(0.8f, 0.1f, 0.1f));
        Material matBlanco = CrearMaterial("Prueba_Blanco", new Color(0.2f, 0.5f, 0.95f));
        Material matLlegada = CrearMaterial("Prueba_Llegada", new Color(0.2f, 0.8f, 0.3f));
        Material matMarca = CrearMaterial("Prueba_Marca", new Color(0.9f, 0.9f, 0.9f));

        Transform obstaculos = Grupo(raiz, "Obstaculos");
        Transform recolectables = Grupo(raiz, "Recolectables");
        Transform enemigos = Grupo(raiz, "Enemigos");
        Transform marcas = Grupo(raiz, "Marcas");

        // Marcas de distancia: franja en el suelo cada 20 m y poste cada 100 m.
        for (float x = 0f; x <= LargoPista; x += 20f)
        {
            bool poste = Mathf.Approximately(x % 100f, 0f);
            GameObject m = Forma(PrimitiveType.Cube, "Marca_" + x, marcas,
                new Vector3(x, -0.09f, Z), new Vector3(poste ? 1f : 0.4f, 0.02f, 3.5f), matMarca);
            Object.DestroyImmediate(m.GetComponent<Collider>());
            if (!poste)
                continue;

            GameObject p = Forma(PrimitiveType.Cube, "Poste_" + x, marcas,
                new Vector3(x, 4f, Z + 2f), new Vector3(0.3f, 8f, 0.3f), matMarca);
            Object.DestroyImmediate(p.GetComponent<Collider>());
        }

        // Combustible a ras de suelo (se recoge rodando) ...
        Combustible(recolectables, matCombustible, "Combustible_Pista_1", 45f, 2.2f);
        Combustible(recolectables, matCombustible, "Combustible_Pista_2", 75f, 2.2f);

        // ... y combustible en el aire: obliga a subir y bajar el morro.
        Combustible(recolectables, matCombustible, "Combustible_Pilar_1", 130f, 15f);
        Combustible(recolectables, matCombustible, "Combustible_Pilar_2", 175f, 19f);
        Combustible(recolectables, matCombustible, "Combustible_Pilar_3", 220f, 15f);
        Combustible(recolectables, matCombustible, "Combustible_Movil", 300f, 12f);
        Combustible(recolectables, matCombustible, "Combustible_Blancos", 380f, 14f);
        Combustible(recolectables, matCombustible, "Combustible_Enemigos_1", 425f, 10f);
        Combustible(recolectables, matCombustible, "Combustible_Enemigos_2", 500f, 9f);

        // Pilares peligrosos (suben desde el suelo)
        float[] posX = { 175f, 220f };
        float[] alto = { 14f, 10f };
        for (int i = 0; i < posX.Length; i++)
        {
            GameObject o = Forma(PrimitiveType.Cube, "ObstaculoFijo_" + (i + 1), obstaculos,
                new Vector3(posX[i], alto[i] * 0.5f - 0.1f, Z), new Vector3(3f, alto[i], 3f), matObstaculo);
            Peligroso(o);
        }

        // Obstaculo movil vertical
        GameObject movil = Forma(PrimitiveType.Cube, "ObstaculoMovil", obstaculos,
            new Vector3(270f, 8f, Z), new Vector3(3f, 3f, 3f), matMovil);
        Peligroso(movil);
        MovimientoPatrulla patrulla = movil.AddComponent<MovimientoPatrulla>();
        patrulla.recorrido = new Vector3(0f, 14f, 0f);
        patrulla.velocidad = 2f;

        // Blancos para probar el disparo recto por el morro (no penalizan al chocar)
        float[] blancoX = { 330f, 345f, 360f };
        float[] blancoY = { 8f, 15f, 22f };
        for (int i = 0; i < blancoX.Length; i++)
        {
            GameObject b = Forma(PrimitiveType.Cube, "Blanco_" + (i + 1), enemigos,
                new Vector3(blancoX[i], blancoY[i], Z), new Vector3(2.5f, 2.5f, 2.5f), matBlanco);
            b.AddComponent<ObjetoColisionable>().tipo = TipoColision.Segura;
            b.AddComponent<Salud>().vida = 1f;
        }

        // Torreta antiaerea
        GameObject torreta = Forma(PrimitiveType.Cylinder, "TorretaAntiaerea", enemigos,
            new Vector3(410f, 1f, Z), new Vector3(2f, 1f, 2f), matEnemigo);
        Peligroso(torreta);
        torreta.AddComponent<Salud>().vida = 3f;
        ConfigurarDisparador(torreta, proyectil, 12f, 2f, 45f);

        // Enemigo volador
        GameObject volador = Forma(PrimitiveType.Capsule, "EnemigoVolador", enemigos,
            new Vector3(460f, 10f, Z), new Vector3(2f, 2f, 2f), matEnemigo);
        volador.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
        Peligroso(volador);
        volador.AddComponent<Salud>().vida = 2f;
        MovimientoPatrulla vuelo = volador.AddComponent<MovimientoPatrulla>();
        vuelo.recorrido = new Vector3(0f, 8f, 0f);
        vuelo.velocidad = 1f;
        ConfigurarDisparador(volador, proyectil, 14f, 2.5f, 40f);

        // Plataforma de aterrizaje: solo cuenta si se entra despacio y a ras de suelo.
        GameObject llegada = Forma(PrimitiveType.Cube, "PlataformaDeLlegada", raiz,
            new Vector3(560f, 1.5f, Z), new Vector3(40f, 3f, 4f), matLlegada);
        PlataformaDeLlegada plataforma = llegada.AddComponent<PlataformaDeLlegada>();
        plataforma.velocidadMaximaDeAterrizaje = 12f;
        llegada.GetComponent<Collider>().isTrigger = true;
    }

    static void Combustible(Transform padre, Material material, string nombre, float x, float y)
    {
        GameObject c = Forma(PrimitiveType.Sphere, nombre, padre, new Vector3(x, y, Z), Vector3.one * 1.5f, material);
        c.AddComponent<RecolectableCombustible>().cantidad = 3f;
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
        string ruta = "Assets/02_Niveles/Nivel_00_Pruebas/Greybox/Materials/" + nombre + ".mat";
        Material m = AssetDatabase.LoadAssetAtPath<Material>(ruta);
        if (m == null)
        {
            AsegurarCarpeta("Assets/02_Niveles/Nivel_00_Pruebas/Greybox/Materials");
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
