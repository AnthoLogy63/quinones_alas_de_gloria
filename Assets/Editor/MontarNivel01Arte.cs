using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Herramienta del Área 3 (Diseño de entornos y modelado 3D).
/// Genera los Skyboxes URP, los prefabs modulares del Nivel 1 y arma la escena
/// separada Nivel_01_Arte.unity conservando intactos el layout, colliders y scripts
/// creados por Diseño de Niveles (Área 1) y Mecánicas (Área 4).
/// </summary>
public static class MontarNivel01Arte
{
    const string EscenaOrigen = "Assets/02_Niveles/Nivel_01/Nivel_01.unity";
    const string EscenaDestino = "Assets/02_Niveles/Nivel_01/Nivel_01_Arte.unity";

    const string DirSkyboxes = "Assets/03_Compartidos/Entorno/Skyboxes";
    const string DirEntornoPrefabs = "Assets/03_Compartidos/Entorno/Prefabs";
    const string DirJugadorPrefabs = "Assets/03_Compartidos/Jugador/Prefabs";
    const string DirN1Prefabs = "Assets/02_Niveles/Nivel_01/Prefabs";

    const string FbxAvionPapel = "Assets/03_Compartidos/Jugador/Modelos/Avion_N1_Papel.fbx";
    const string FbxRafaga = "Assets/03_Compartidos/Entorno/Modelos/ENV_RafagaViento.fbx";
    const string FbxNube1 = "Assets/03_Compartidos/Entorno/Modelos/ENV_Nube_01.fbx";
    const string FbxNube2 = "Assets/03_Compartidos/Entorno/Modelos/ENV_Nube_02.fbx";
    const string FbxNube3 = "Assets/03_Compartidos/Entorno/Modelos/ENV_Nube_03.fbx";

    const string FbxAlgarrobo = "Assets/02_Niveles/Nivel_01/Modelos/OBS_N1_ArbolAlgarrobo.fbx";
    const string FbxPalmera = "Assets/02_Niveles/Nivel_01/Modelos/OBS_N1_PalmeraCostena.fbx";
    const string FbxArbolBajo = "Assets/02_Niveles/Nivel_01/Modelos/OBS_N1_ArbolBajoCosteno.fbx";
    const string FbxCasa1 = "Assets/02_Niveles/Nivel_01/Modelos/OBS_N1_CasaEpoca_01.fbx";
    const string FbxCasa2 = "Assets/02_Niveles/Nivel_01/Modelos/OBS_N1_CasaEpoca_02.fbx";
    const string FbxCasa3 = "Assets/02_Niveles/Nivel_01/Modelos/OBS_N1_CasaEpoca_03.fbx";
    const string FbxCerca = "Assets/02_Niveles/Nivel_01/Modelos/OBS_N1_CercaMadera.fbx";
    const string FbxArcoLlegada = "Assets/02_Niveles/Nivel_01/Modelos/ENV_N1_ArcoLlegada.fbx";
    const string FbxLineaSalida = "Assets/02_Niveles/Nivel_01/Modelos/ENV_N1_LineaSalida.fbx";
    const string FbxSuelo = "Assets/02_Niveles/Nivel_01/Modelos/ENV_N1_SueloSegmento.fbx";
    const string FbxColina = "Assets/02_Niveles/Nivel_01/Modelos/ENV_N1_ColinaFondo.fbx";

    const string PrefabAvionBase = "Assets/03_Compartidos/Greybox/Prefabs/AvionRoot.prefab";

    [MenuItem("Alas de Gloria/Área 3/1. Crear Skyboxes y Prefabs Nivel 1")]
    public static void CrearSkyboxesYPrefabs()
    {
        AsegurarCarpeta(DirSkyboxes);
        AsegurarCarpeta(DirEntornoPrefabs);
        AsegurarCarpeta(DirJugadorPrefabs);
        AsegurarCarpeta(DirN1Prefabs);

        CrearSkyboxes();
        CrearPrefabsModulares();
        CrearPrefabEscenarioFondo();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[Área 3] Skyboxes URP y prefabs modulares del Nivel 1 creados correctamente.");
    }

    [MenuItem("Alas de Gloria/Área 3/2. Montar Escena Nivel_01_Arte (separada)")]
    public static void MontarEscenaNivel01Arte()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        CrearSkyboxesYPrefabs();

        EditorSceneManager.OpenScene(EscenaOrigen, OpenSceneMode.Single);
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), EscenaDestino, false);

        AplicarIluminacionYSkyboxN1();
        VestirAvionEnEscena();
        VestirLimitesEnEscena();
        VestirLlegadaYPropulsorEnEscena();
        VestirObstaculosGreyboxEnEscena();
        ColocarFondoEnDecorado();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();

        Debug.Log("[Área 3] Escena separada lista para jugar: " + EscenaDestino);
    }

    // --------------------------------------------------------- 1. Skyboxes

    static void CrearSkyboxes()
    {
        // 6 Skyboxes procedurales que cubren toda la curva visual de los 10 niveles
        CrearSkyboxProcedural("SKY_01_MananaCalida",
            skyTint: new Color(0.36f, 0.64f, 0.94f),
            ground: new Color(0.76f, 0.62f, 0.42f),
            atmosphere: 1.05f, exposure: 1.25f, sunSize: 0.045f);

        CrearSkyboxProcedural("SKY_02_CieloDespejado",
            skyTint: new Color(0.30f, 0.58f, 0.95f),
            ground: new Color(0.55f, 0.58f, 0.48f),
            atmosphere: 0.90f, exposure: 1.30f, sunSize: 0.04f);

        CrearSkyboxProcedural("SKY_03_CostaMilitar",
            skyTint: new Color(0.42f, 0.54f, 0.68f),
            ground: new Color(0.56f, 0.48f, 0.36f),
            atmosphere: 1.15f, exposure: 1.10f, sunSize: 0.035f);

        CrearSkyboxProcedural("SKY_04_NubladoTension",
            skyTint: new Color(0.45f, 0.49f, 0.55f),
            ground: new Color(0.38f, 0.36f, 0.32f),
            atmosphere: 1.45f, exposure: 0.95f, sunSize: 0.025f);

        CrearSkyboxProcedural("SKY_05_AmanecerTumbes",
            skyTint: new Color(0.82f, 0.48f, 0.32f),
            ground: new Color(0.32f, 0.24f, 0.20f),
            atmosphere: 1.75f, exposure: 1.15f, sunSize: 0.06f);

        CrearSkyboxProcedural("SKY_06_CombateTormenta",
            skyTint: new Color(0.28f, 0.30f, 0.36f),
            ground: new Color(0.24f, 0.20f, 0.18f),
            atmosphere: 1.65f, exposure: 0.85f, sunSize: 0.03f);
    }

    static Material CrearSkyboxProcedural(string nombre, Color skyTint, Color ground, float atmosphere, float exposure, float sunSize)
    {
        string ruta = $"{DirSkyboxes}/{nombre}.mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(ruta);
        Shader shader = Shader.Find("Skybox/Procedural");
        if (shader == null)
        {
            Debug.LogWarning("[Área 3] No se encontró el shader Skybox/Procedural.");
            return null;
        }

        if (mat == null)
        {
            mat = new Material(shader);
            AssetDatabase.CreateAsset(mat, ruta);
        }
        else
        {
            mat.shader = shader;
        }

        mat.SetColor("_SkyTint", skyTint);
        mat.SetColor("_GroundColor", ground);
        mat.SetFloat("_AtmosphereThickness", atmosphere);
        mat.SetFloat("_Exposure", exposure);
        mat.SetFloat("_SunSize", sunSize);
        mat.SetFloat("_SunSizeConvergence", 5f);
        EditorUtility.SetDirty(mat);
        return mat;
    }

    // --------------------------------------------------- 2. Prefabs modulares

    static void CrearPrefabsModulares()
    {
        // Obstáculos temáticos Nivel 1 (con BoxCollider + ObjetoColisionable Peligrosa)
        CrearPrefabObstaculo("OBS_N1_ArbolAlgarrobo", FbxAlgarrobo, new Vector3(0f, 4.2f, 0f), new Vector3(4.5f, 8.2f, 3.6f));
        CrearPrefabObstaculo("OBS_N1_PalmeraCostena", FbxPalmera, new Vector3(0.5f, 5.8f, 0f), new Vector3(2.8f, 11.4f, 2.8f));
        CrearPrefabObstaculo("OBS_N1_ArbolBajoCosteno", FbxArbolBajo, new Vector3(0f, 2.4f, 0f), new Vector3(3.8f, 4.6f, 2.8f));
        CrearPrefabObstaculo("OBS_N1_CasaEpoca_01", FbxCasa1, new Vector3(0f, 3.3f, 0f), new Vector3(7.6f, 6.5f, 5.2f));
        CrearPrefabObstaculo("OBS_N1_CasaEpoca_02", FbxCasa2, new Vector3(0f, 4.5f, 0f), new Vector3(8.0f, 8.9f, 5.6f));
        CrearPrefabObstaculo("OBS_N1_CasaEpoca_03", FbxCasa3, new Vector3(0f, 2.0f, 0f), new Vector3(7.2f, 4.0f, 4.6f));
        CrearPrefabObstaculo("OBS_N1_CercaMadera", FbxCerca, new Vector3(0f, 1.3f, 0f), new Vector3(5.0f, 2.6f, 1.0f));

        // Nubes reutilizables (decorativas, sin collider)
        CrearPrefabDecorativo("ENV_Nube_01", FbxNube1, DirEntornoPrefabs);
        CrearPrefabDecorativo("ENV_Nube_02", FbxNube2, DirEntornoPrefabs);
        CrearPrefabDecorativo("ENV_Nube_03", FbxNube3, DirEntornoPrefabs);

        // Ráfaga de viento (trigger)
        CrearPrefabRafaga();

        // Plataforma de llegada Nivel 1 (trigger + PlataformaDeLlegada)
        CrearPrefabLlegadaN1();

        // Variante del avión del jugador para Nivel 1 (AvionRoot_N1_Papel)
        CrearPrefabAvionPapel();
    }

    static void CrearPrefabObstaculo(string nombre, string fbxRuta, Vector3 colCenter, Vector3 colSize)
    {
        GameObject fbx = AssetDatabase.LoadAssetAtPath<GameObject>(fbxRuta);
        if (fbx == null)
        {
            Debug.LogWarning("[Área 3] No se encontró FBX: " + fbxRuta);
            return;
        }

        GameObject root = new GameObject(nombre);
        root.tag = ObjetoColisionable.TagPeligrosa;

        BoxCollider box = root.AddComponent<BoxCollider>();
        box.center = colCenter;
        box.size = colSize;

        ObjetoColisionable col = root.AddComponent<ObjetoColisionable>();
        col.tipo = TipoColision.Peligrosa;

        GameObject visual = (GameObject)PrefabUtility.InstantiatePrefab(fbx);
        visual.name = "Modelo";
        visual.transform.SetParent(root.transform, false);

        string destino = $"{DirN1Prefabs}/{nombre}.prefab";
        PrefabUtility.SaveAsPrefabAsset(root, destino);
        Object.DestroyImmediate(root);
    }

    static void CrearPrefabDecorativo(string nombre, string fbxRuta, string dirDestino)
    {
        GameObject fbx = AssetDatabase.LoadAssetAtPath<GameObject>(fbxRuta);
        if (fbx == null) return;

        GameObject root = new GameObject(nombre);
        GameObject visual = (GameObject)PrefabUtility.InstantiatePrefab(fbx);
        visual.name = "Modelo";
        visual.transform.SetParent(root.transform, false);

        PrefabUtility.SaveAsPrefabAsset(root, $"{dirDestino}/{nombre}.prefab");
        Object.DestroyImmediate(root);
    }

    static void CrearPrefabRafaga()
    {
        GameObject fbx = AssetDatabase.LoadAssetAtPath<GameObject>(FbxRafaga);
        if (fbx == null) return;

        GameObject root = new GameObject("ENV_RafagaViento");
        CapsuleCollider col = root.AddComponent<CapsuleCollider>();
        col.isTrigger = true;
        col.direction = 0; // X axis
        col.radius = 1.65f;
        col.height = 3.2f;

        GameObject visual = (GameObject)PrefabUtility.InstantiatePrefab(fbx);
        visual.name = "Modelo";
        visual.transform.SetParent(root.transform, false);

        PrefabUtility.SaveAsPrefabAsset(root, $"{DirEntornoPrefabs}/ENV_RafagaViento.prefab");
        Object.DestroyImmediate(root);
    }

    static void CrearPrefabLlegadaN1()
    {
        GameObject fbx = AssetDatabase.LoadAssetAtPath<GameObject>(FbxArcoLlegada);
        if (fbx == null) return;

        GameObject root = new GameObject("ENV_N1_PlataformaLlegada");
        BoxCollider box = root.AddComponent<BoxCollider>();
        box.isTrigger = true;
        box.center = new Vector3(0f, 1.5f, 0f);
        box.size = new Vector3(12f, 3f, 4f);

        root.AddComponent<PlataformaDeLlegada>();

        GameObject visual = (GameObject)PrefabUtility.InstantiatePrefab(fbx);
        visual.name = "Modelo";
        visual.transform.SetParent(root.transform, false);

        PrefabUtility.SaveAsPrefabAsset(root, $"{DirN1Prefabs}/ENV_N1_PlataformaLlegada.prefab");
        Object.DestroyImmediate(root);
    }

    static void CrearPrefabAvionPapel()
    {
        GameObject basePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabAvionBase);
        GameObject fbx = AssetDatabase.LoadAssetAtPath<GameObject>(FbxAvionPapel);
        if (basePrefab == null || fbx == null) return;

        GameObject inst = (GameObject)PrefabUtility.InstantiatePrefab(basePrefab);
        PrefabUtility.UnpackPrefabInstance(inst, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        inst.name = "AvionRoot_N1_Papel";

        // Eliminar el modelo NA-50 anterior manteniendo todos los componentes de la raíz intactos
        for (int i = inst.transform.childCount - 1; i >= 0; i--)
            Object.DestroyImmediate(inst.transform.GetChild(i).gameObject);

        GameObject visual = (GameObject)PrefabUtility.InstantiatePrefab(fbx);
        visual.name = "Modelo_AvionPapel";
        visual.transform.SetParent(inst.transform, false);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localRotation = Quaternion.identity;
        visual.transform.localScale = Vector3.one;

        // Collider limpio en la raíz para física estable en 2.5D
        BoxCollider col = inst.GetComponent<BoxCollider>();
        if (col == null) col = inst.AddComponent<BoxCollider>();
        col.center = new Vector3(0.05f, -0.05f, 0f);
        col.size = new Vector3(3.1f, 0.75f, 2.2f);

        PrefabUtility.SaveAsPrefabAsset(inst, $"{DirJugadorPrefabs}/AvionRoot_N1_Papel.prefab");
        Object.DestroyImmediate(inst);
    }

    // ----------------------------------------- 3. Prefab de fondo Nivel 1

    static void CrearPrefabEscenarioFondo()
    {
        GameObject fbxSuelo = AssetDatabase.LoadAssetAtPath<GameObject>(FbxSuelo);
        GameObject fbxColina = AssetDatabase.LoadAssetAtPath<GameObject>(FbxColina);
        GameObject fbxCasa1 = AssetDatabase.LoadAssetAtPath<GameObject>(FbxCasa1);
        GameObject fbxCasa2 = AssetDatabase.LoadAssetAtPath<GameObject>(FbxCasa2);
        GameObject fbxCasa3 = AssetDatabase.LoadAssetAtPath<GameObject>(FbxCasa3);
        GameObject fbxAlgarrobo = AssetDatabase.LoadAssetAtPath<GameObject>(FbxAlgarrobo);
        GameObject fbxPalmera = AssetDatabase.LoadAssetAtPath<GameObject>(FbxPalmera);
        GameObject fbxArbolBajo = AssetDatabase.LoadAssetAtPath<GameObject>(FbxArbolBajo);
        GameObject fbxCerca = AssetDatabase.LoadAssetAtPath<GameObject>(FbxCerca);
        GameObject[] nubes =
        {
            AssetDatabase.LoadAssetAtPath<GameObject>(FbxNube1),
            AssetDatabase.LoadAssetAtPath<GameObject>(FbxNube2),
            AssetDatabase.LoadAssetAtPath<GameObject>(FbxNube3),
        };

        GameObject root = new GameObject("ENV_N1_EscenarioFondo");

        Transform grpSuelo = NuevoGrupo(root.transform, "SueloCosteno");
        Transform grpColinas = NuevoGrupo(root.transform, "ColinasFondo");
        Transform grpCasas = NuevoGrupo(root.transform, "Pueblo1920");
        Transform grpCercas = NuevoGrupo(root.transform, "CercasYCampos");
        Transform grpArboles = NuevoGrupo(root.transform, "VegetacionCostena");
        Transform grpNubes = NuevoGrupo(root.transform, "NubesCielo");

        // 1) Suelo continuo cubriendo X = -50..850 (18 segmentos de 50m centrados en Z = 0.5)
        if (fbxSuelo != null)
        {
            for (int i = 0; i < 18; i++)
            {
                float x = -25f + i * 50f;
                Instanciar(fbxSuelo, grpSuelo, $"Suelo_{i:00}", new Vector3(x, 0f, 0.5f), Quaternion.identity, Vector3.one);
            }
        }

        // 2) Colinas lejanas en paralaje (Z = 34..54)
        if (fbxColina != null)
        {
            for (int i = 0; i < 16; i++)
            {
                float x = -20f + i * 58f;
                float z = (i % 2 == 0) ? 38f : 48f;
                float sy = (i % 3 == 0) ? 1.25f : (i % 2 == 0 ? 0.95f : 1.1f);
                float sx = (i % 2 == 0) ? 1.15f : 1.35f;
                Instanciar(fbxColina, grpColinas, $"Colina_{i:00}", new Vector3(x, -0.2f, z), Quaternion.identity, new Vector3(sx, sy, 1.1f));
            }
        }

        // 3) Pueblo costeño ~1920 en el plano medio (Z = 9..17, nunca invade el carril Z=0.5!)
        GameObject[] casas = { fbxCasa1, fbxCasa2, fbxCasa3 };
        float[] posCasasX = { 12f, 45f, 88f, 155f, 215f, 285f, 355f, 420f, 495f, 575f, 645f, 710f, 765f };
        for (int i = 0; i < posCasasX.Length; i++)
        {
            GameObject c = casas[i % casas.Length];
            if (c == null) continue;
            float z = (i % 3 == 1) ? 14.5f : 10.5f;
            float rotY = (i % 2 == 0) ? -6f : 7f;
            Instanciar(c, grpCasas, $"Casa_{i:00}", new Vector3(posCasasX[i], 0.1f, z), Quaternion.Euler(0f, rotY, 0f), Vector3.one);
        }

        // 4) Tramos de cercas rústicas bordeando patios y campos (Z = 5.6..7.2)
        if (fbxCerca != null)
        {
            float[] iniciosCerca = { 22f, 110f, 240f, 385f, 520f, 665f };
            int idxC = 0;
            foreach (float xIni in iniciosCerca)
            {
                for (int k = 0; k < 4; k++)
                {
                    Instanciar(fbxCerca, grpCercas, $"Cerca_{idxC++:00}",
                        new Vector3(xIni + k * 5.0f, 0.05f, 6.2f), Quaternion.identity, Vector3.one);
                }
            }
        }

        // 5) Vegetación costeña variada (Algarrobo, Palmera, Faique bajo) en Z = 7..23
        GameObject[] arboles = { fbxAlgarrobo, fbxPalmera, fbxArbolBajo };
        for (int i = 0; i < 42; i++)
        {
            GameObject a = arboles[i % arboles.Length];
            if (a == null) continue;
            float x = -15f + i * 20.5f + ((i % 3) * 3.5f);
            float z = 7.5f + (i % 4) * 4.2f;
            float s = 0.85f + (i % 3) * 0.16f;
            float rotY = (i * 53f) % 360f;
            Instanciar(a, grpArboles, $"Arbol_{i:00}", new Vector3(x, 0.1f, z), Quaternion.Euler(0f, rotY, 0f), new Vector3(s, s, s));
        }

        // 6) Nubes en el cielo (fondo Z = 16..38) + banda suave en el techo (Y = 29..31)
        for (int i = 0; i < 36; i++)
        {
            GameObject n = nubes[i % nubes.Length];
            if (n == null) continue;
            float x = -20f + i * 24f;
            bool esTecho = (i % 3 == 0);
            float y = esTecho ? (29.5f + (i % 2) * 1.2f) : (15f + (i % 4) * 3.5f);
            float z = esTecho ? 6.5f : (18f + (i % 3) * 8f);
            float s = esTecho ? 1.15f : (1.1f + (i % 3) * 0.35f);
            Instanciar(n, grpNubes, $"Nube_{i:00}", new Vector3(x, y, z), Quaternion.identity, new Vector3(s, s, s));
        }

        PrefabUtility.SaveAsPrefabAsset(root, $"{DirN1Prefabs}/ENV_N1_EscenarioFondo.prefab");
        Object.DestroyImmediate(root);
    }

    // ----------------------------------------- 4. Montaje en Nivel_01_Arte

    static void AplicarIluminacionYSkyboxN1()
    {
        Material sky = AssetDatabase.LoadAssetAtPath<Material>($"{DirSkyboxes}/SKY_01_MananaCalida.mat");
        if (sky != null)
            RenderSettings.skybox = sky;

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = new Color(0.76f, 0.86f, 0.96f);
        RenderSettings.fogDensity = 0.0022f;

        Light sol = Object.FindFirstObjectByType<Light>();
        if (sol != null)
        {
            sol.color = new Color(1.0f, 0.95f, 0.84f);
            sol.intensity = 1.25f;
            sol.transform.rotation = Quaternion.Euler(42f, -32f, 0f);
            RenderSettings.sun = sol;
        }
    }

    static void VestirAvionEnEscena()
    {
        GameObject avion = GameObject.Find("AvionRoot");
        GameObject fbx = AssetDatabase.LoadAssetAtPath<GameObject>(FbxAvionPapel);
        if (avion == null || fbx == null) return;

        // Desempaquetar solo si es instancia de AvionRoot greybox para cambiar el hijo visual sin perder scripts
        if (PrefabUtility.IsPartOfPrefabInstance(avion))
            PrefabUtility.UnpackPrefabInstance(avion, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);

        for (int i = avion.transform.childCount - 1; i >= 0; i--)
            Object.DestroyImmediate(avion.transform.GetChild(i).gameObject);

        GameObject visual = (GameObject)PrefabUtility.InstantiatePrefab(fbx);
        visual.name = "Modelo_AvionPapel";
        visual.transform.SetParent(avion.transform, false);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localRotation = Quaternion.identity;
        visual.transform.localScale = Vector3.one;

        BoxCollider col = avion.GetComponent<BoxCollider>();
        if (col == null) col = avion.AddComponent<BoxCollider>();
        col.center = new Vector3(0.05f, -0.05f, 0f);
        col.size = new Vector3(3.1f, 0.75f, 2.2f);
    }

    static void VestirLimitesEnEscena()
    {
        // Ocultamos solo el MeshRenderer de Suelo y Techo greybox (manteniendo sus BoxCollider y ObjetoColisionable!)
        OcultarRenderer("Suelo");
        OcultarRenderer("Techo");
    }

    static void VestirLlegadaYPropulsorEnEscena()
    {
        // 1) Línea / pista de salida en la posición de inicio de AvionRoot (sin collider, puramente visual)
        GameObject avion = GameObject.Find("AvionRoot");
        GameObject fbxSalida = AssetDatabase.LoadAssetAtPath<GameObject>(FbxLineaSalida);
        if (avion != null && fbxSalida != null)
        {
            GameObject decorado = GameObject.Find("Decorado");
            Transform padreSalida = decorado != null ? decorado.transform : null;
            GameObject viejaSalida = GameObject.Find("Visual_LineaSalida");
            if (viejaSalida != null) Object.DestroyImmediate(viejaSalida);

            GameObject visualSalida = (GameObject)PrefabUtility.InstantiatePrefab(fbxSalida);
            visualSalida.name = "Visual_LineaSalida";
            if (padreSalida != null) visualSalida.transform.SetParent(padreSalida, true);
            Vector3 pa = avion.transform.position;
            visualSalida.transform.position = new Vector3(pa.x, 0.06f, pa.z);
            visualSalida.transform.rotation = Quaternion.identity;
            visualSalida.transform.localScale = Vector3.one;
        }

        // 2) Llegada: conserva su BoxCollider trigger + PlataformaDeLlegada
        // Buscamos por componente PlataformaDeLlegada (más robusto que GameObject.Find por nombre)
        PlataformaDeLlegada compLlegada = Object.FindFirstObjectByType<PlataformaDeLlegada>();
        GameObject llegada = compLlegada != null ? compLlegada.gameObject : GameObject.Find("PlataformaDeLlegada");
        GameObject fbxArco = AssetDatabase.LoadAssetAtPath<GameObject>(FbxArcoLlegada);
        if (llegada != null && fbxArco != null)
        {
            OcultarRenderer(llegada);
            GameObject visualPrev = GameObject.Find("Visual_ArcoLlegada");
            if (visualPrev != null) Object.DestroyImmediate(visualPrev);

            // Elevamos ligeramente Y = 0.06f para evitar solapamiento con el suelo costeño y damos buena altura visible
            GameObject visual = (GameObject)PrefabUtility.InstantiatePrefab(fbxArco);
            visual.name = "Visual_ArcoLlegada";
            visual.transform.SetParent(llegada.transform.parent, true);
            Vector3 p = llegada.transform.position;
            visual.transform.position = new Vector3(p.x, 0.06f, p.z);
            visual.transform.rotation = Quaternion.identity;
            visual.transform.localScale = new Vector3(1.0f, 1.35f, 1.0f);
        }

        // 3) Propulsor (Ráfaga de viento): conserva su CapsuleCollider trigger
        GameObject prop = GameObject.Find("Prop_01");
        GameObject fbxRafaga = AssetDatabase.LoadAssetAtPath<GameObject>(FbxRafaga);
        if (prop != null && fbxRafaga != null)
        {
            OcultarRenderer(prop);
            GameObject viejaRaf = GameObject.Find("Visual_RafagaViento");
            if (viejaRaf != null) Object.DestroyImmediate(viejaRaf);

            GameObject visual = (GameObject)PrefabUtility.InstantiatePrefab(fbxRafaga);
            visual.name = "Visual_RafagaViento";
            visual.transform.SetParent(prop.transform.parent, true);
            visual.transform.position = prop.transform.position;
            visual.transform.rotation = Quaternion.identity;
            visual.transform.localScale = new Vector3(1.35f, 1.1f, 1.1f);
        }
    }

    static void VestirObstaculosGreyboxEnEscena()
    {
        // Viste visualmente los 6 Obst_Fijo_* en las MISMAS posiciones exactas de Área 1,
        // ocultando los cubos rojos pero conservando intactos sus BoxCollider y ObjetoColisionable.
        // - Obstáculos desde el suelo: árboles/casas de pie (NUNCA de cabeza).
        // - Obstáculos superiores/aéreos: formación vertical de nubes densas bajas cubriendo el collider.
        string[] nombres = { "Obst_Fijo_01", "Obst_Fijo_02", "Obst_Fijo_03", "Obst_Fijo_04", "Obst_Fijo_05", "Obst_Fijo_06" };
        string[] modelosSuelo = { FbxArbolBajo, FbxAlgarrobo, FbxPalmera, FbxPalmera, FbxPalmera, FbxCasa2 };
        GameObject[] nubesFbx =
        {
            AssetDatabase.LoadAssetAtPath<GameObject>(FbxNube1),
            AssetDatabase.LoadAssetAtPath<GameObject>(FbxNube2),
            AssetDatabase.LoadAssetAtPath<GameObject>(FbxNube3),
        };

        // Material más grisáceo para diferenciar nubes densas peligrosas de las nubes blancas decorativas
        Material matNubePeligro = ObtenerMaterialNubeDensa();

        for (int i = 0; i < nombres.Length; i++)
        {
            GameObject obst = GameObject.Find(nombres[i]);
            if (obst == null) continue;

            OcultarRenderer(obst);

            GameObject viejoVis = GameObject.Find("Visual_" + nombres[i]);
            if (viejoVis != null) Object.DestroyImmediate(viejoVis);

            Collider col = obst.GetComponent<Collider>();
            Bounds b = col != null ? col.bounds : new Bounds(obst.transform.position, obst.transform.lossyScale);

            if (b.min.y <= 1.2f)
            {
                // Obstáculo que nace desde el suelo: siempre de pie (rotación Y = 0, nunca invertido)
                GameObject fbx = AssetDatabase.LoadAssetAtPath<GameObject>(modelosSuelo[i]);
                if (fbx == null) continue;

                GameObject visual = (GameObject)PrefabUtility.InstantiatePrefab(fbx);
                visual.name = "Visual_" + nombres[i];
                visual.transform.SetParent(obst.transform.parent, true);
                visual.transform.position = new Vector3(b.center.x, 0f, b.center.z);
                visual.transform.rotation = Quaternion.identity;

                float alturaDeseada = Mathf.Max(2f, b.size.y);
                Bounds mb = CalcularBounds(visual);
                if (mb.size.y > 0.1f)
                {
                    float factor = Mathf.Clamp(alturaDeseada / mb.size.y, 0.75f, 1.35f);
                    visual.transform.localScale = Vector3.one * factor;
                }
            }
            else
            {
                // Obstáculo aéreo superior (colgando del techo): columna de nubes densas bajas
                GameObject contenedor = new GameObject("Visual_" + nombres[i]);
                contenedor.transform.SetParent(obst.transform.parent, true);
                contenedor.transform.position = b.center;

                int capas = Mathf.Max(2, Mathf.RoundToInt(b.size.y / 3.4f));
                for (int k = 0; k < capas; k++)
                {
                    GameObject nube = nubesFbx[(i + k) % nubesFbx.Length];
                    if (nube == null) continue;

                    float t = capas > 1 ? (float)k / (capas - 1) : 0.5f;
                    float yPos = Mathf.Lerp(b.min.y + 1.2f, b.max.y - 1.0f, t);
                    float xOff = (k % 2 == 0) ? -0.35f : 0.35f;

                    GameObject inst = (GameObject)PrefabUtility.InstantiatePrefab(nube, contenedor.transform);
                    inst.name = $"NubeObst_{k:00}";
                    inst.transform.position = new Vector3(b.center.x + xOff, yPos, b.center.z);
                    inst.transform.rotation = Quaternion.identity;
                    inst.transform.localScale = new Vector3(0.55f, 1.05f, 0.85f);

                    if (matNubePeligro != null)
                    {
                        foreach (MeshRenderer mr in inst.GetComponentsInChildren<MeshRenderer>())
                        {
                            Material[] mats = new Material[mr.sharedMaterials.Length];
                            for (int m = 0; m < mats.Length; m++) mats[m] = matNubePeligro;
                            mr.sharedMaterials = mats;
                        }
                    }
                }
            }
        }
    }

    static Material ObtenerMaterialNubeDensa()
    {
        string dirMat = "Assets/02_Niveles/Nivel_01/Materials";
        AsegurarCarpeta(dirMat);
        string ruta = $"{dirMat}/MAT_N1_NubeDensaPeligro.mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(ruta);
        if (mat == null)
        {
            Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
            if (urpLit == null) urpLit = Shader.Find("Standard");
            mat = new Material(urpLit);
            mat.SetColor("_BaseColor", new Color(0.56f, 0.62f, 0.70f, 1f));
            mat.SetFloat("_Smoothness", 0.1f);
            AssetDatabase.CreateAsset(mat, ruta);
        }
        return mat;
    }

    static void ColocarFondoEnDecorado()
    {
        GameObject decorado = GameObject.Find("Decorado");
        if (decorado == null)
            decorado = new GameObject("Decorado");

        Transform viejo = decorado.transform.Find("ENV_N1_EscenarioFondo");
        if (viejo != null)
            Object.DestroyImmediate(viejo.gameObject);

        GameObject prefabFondo = AssetDatabase.LoadAssetAtPath<GameObject>($"{DirN1Prefabs}/ENV_N1_EscenarioFondo.prefab");
        if (prefabFondo == null) return;

        GameObject inst = (GameObject)PrefabUtility.InstantiatePrefab(prefabFondo, decorado.transform);
        inst.name = "ENV_N1_EscenarioFondo";
        inst.transform.localPosition = Vector3.zero;
    }

    // ----------------------------------------------------------- Helpers

    static void OcultarRenderer(string nombre)
    {
        GameObject go = GameObject.Find(nombre);
        if (go != null) OcultarRenderer(go);
    }

    static void OcultarRenderer(GameObject go)
    {
        MeshRenderer mr = go.GetComponent<MeshRenderer>();
        if (mr != null) mr.enabled = false;
    }

    static Transform NuevoGrupo(Transform padre, string nombre)
    {
        GameObject go = new GameObject(nombre);
        go.transform.SetParent(padre, false);
        return go.transform;
    }

    static GameObject Instanciar(GameObject asset, Transform padre, string nombre, Vector3 pos, Quaternion rot, Vector3 scale)
    {
        GameObject inst = (GameObject)PrefabUtility.InstantiatePrefab(asset, padre);
        inst.name = nombre;
        inst.transform.localPosition = pos;
        inst.transform.localRotation = rot;
        inst.transform.localScale = scale;
        return inst;
    }

    static Bounds CalcularBounds(GameObject go)
    {
        Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return new Bounds(go.transform.position, Vector3.one);
        Bounds b = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++) b.Encapsulate(renderers[i].bounds);
        return b;
    }

    static void AsegurarCarpeta(string ruta)
    {
        if (AssetDatabase.IsValidFolder(ruta)) return;
        string padre = Path.GetDirectoryName(ruta).Replace("\\", "/");
        string nombre = Path.GetFileName(ruta);
        AsegurarCarpeta(padre);
        AssetDatabase.CreateFolder(padre, nombre);
    }
}
