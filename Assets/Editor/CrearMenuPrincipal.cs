using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;


public static class CrearMenuPrincipal
{
    private const string Root = "Assets/01_Interfaces/MenuPrincipal/";
    private const string Sprites = Root + "Sprites/";
    private const string Anim = Root + "Animaciones/";
    private const string Clips = Anim + "Clips/";
    private const string Controllers = Anim + "Controllers/";
    private const string ScenePath = Root + "MenuPrincipal.unity";
    private const int CanvasWidth = 1920;
    private const int CanvasHeight = 1080;

    [MenuItem("Alas de Gloria/Crear escena de Menu Principal")]
    public static void CreateMenu()
    {
        EnsureFolder(Anim, "Clips");
        EnsureFolder(Anim, "Controllers");

        ConfigureSingle("FONDO.png");
        ConfigureSingle("Muted Misty River Valley.png");
        ConfigureSingle("NUBES_CAPA_01_BANCO.png");
        ConfigureSingle("NUBES_CAPA_02_CUMULOS.png");
        ConfigureSingle("NUBES_CAPA_03_CIRROS.png");
        ConfigureSingle("NUBES_CAPA_04_GRUPOS.png");
        ConfigureSingle("MANDO.png");
        ConfigureSingle("MANDO_HOVER.png");
        ConfigureSingle("OPCIONES.png");
        ConfigureSingle("OPCIONES_HOVER.png");
        ConfigureSingle("NIVELES.png");
        ConfigureSingle("NIVELES_HOVER.png");
        ConfigureSingle("CREDITOS.png");
        ConfigureSingle("CREDITOS_HOVER.png");
        ConfigureSingle("PALANCA_SALIDA.png");
        ConfigureSingle("PALANCA_SALIDA_HOVER.png");
        ConfigureSingle("HELICE_3_ASPAS_PROPUESTA.png");
        ConfigureGrid("ANIM_HELICE_FONDO_RAPIDA_24F.png", 6, 4);
        ConfigureGrid("ANIM_HELICE_CARGA_LENTA_8F.png", 4, 2);
        ConfigureGrid("FUEGO_ANIM_8F.png", 4, 2);
        ConfigureGrid("HUMO_NEGRO_ANIM_8F.png", 4, 2);
        ConfigureGrid("MANECILLA_180_SUPERIOR.png", 13, 1);

        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        CreateClipsAndControllers();

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "MenuPrincipal";

        var canvasObject = new GameObject("Canvas_MenuPrincipal", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
        var camera = cameraObject.GetComponent<Camera>();
        camera.orthographic = true;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = Color.black;
        camera.cullingMask = 0;
        cameraObject.transform.position = new Vector3(0f, 0f, -10f);
        camera.tag = "MainCamera";

        var canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.pixelPerfect = true;
        canvas.sortingOrder = 0;

        var scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(CanvasWidth, CanvasHeight);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        scaler.referencePixelsPerUnit = 100f;

        var environment = NewLayer(canvasObject.transform, "01_Paisaje_y_efectos");
        AddFullScreen(environment, "Paisaje exterior", LoadSprite("Muted Misty River Valley.png"), false);

        AddCloud(environment, "Nubes_Banco", "NUBES_CAPA_01_BANCO.png", 110f, 95f, 840f, 360f, 0.22f, 34f, 82f);
        AddCloud(environment, "Nubes_Cumulos", "NUBES_CAPA_02_CUMULOS.png", 790f, 110f, 930f, 215f, 0.23f, 25f, 58f);
        AddCloud(environment, "Nubes_Cirros", "NUBES_CAPA_03_CIRROS.png", 240f, 10f, 1480f, 168f, 0.20f, 42f, 110f);
        AddCloud(environment, "Nubes_Grupos", "NUBES_CAPA_04_GRUPOS.png", 870f, 215f, 850f, 143f, 0.19f, 20f, 67f);

        var smoke = AddAnimatedImage(environment, "Humo_negro", "HUMO_NEGRO_ANIM_8F.png", "Humo_Negro", 1300f, 175f, 400f, 330f, 0.76f);
        var fire = AddAnimatedImage(environment, "Fuego_distante", "FUEGO_ANIM_8F.png", "Fuego", 1440f, 335f, 190f, 190f, 1f);
        var propeller = AddAnimatedImage(environment, "Helice_fondo", "ANIM_HELICE_FONDO_RAPIDA_24F.png", "Helice_Fondo_Rapida", 825f, 130f, 270f, 270f, 0.48f);

        AddFullScreen(canvasObject.transform, "02_Cabina", LoadSprite("FONDO.png"), false);

        // The hover art is behind the matching original art, which stays fully opaque.
        var options = AddInteractive(canvasObject.transform, "03_OPCIONES", "OPCIONES.png", "OPCIONES_HOVER.png", 260f, 580f, 555f, 401f);
        AddAnimatedImage(canvasObject.transform, "Aguja_OPCIONES_180_Superior", "MANECILLA_180_SUPERIOR.png", "Manecilla_180_Superior", 324f, 681f, 128f, 128f, 1f);
        var continueButton = AddInteractive(canvasObject.transform, "04_CONTINUAR_MANDO", "MANDO.png", "MANDO_HOVER.png", 777f, 590f, 366f, 489f);
        var levels = AddInteractive(canvasObject.transform, "05_NIVELES", "NIVELES.png", "NIVELES_HOVER.png", 1110f, 510f, 604f, 571f);
        var credits = AddInteractive(canvasObject.transform, "06_CREDITOS_FOTO", "CREDITOS.png", "CREDITOS_HOVER.png", 1500f, 710f, 272f, 279f);
        var exit = AddInteractive(canvasObject.transform, "07_SALIR_PALANCA", "PALANCA_SALIDA.png", "PALANCA_SALIDA_HOVER.png", 120f, 780f, 187f, 255f);

        var audioPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(Root + "Prefabs/AudioMenu.prefab");
        if (audioPrefab != null)
        {
            var audioObject = (GameObject)PrefabUtility.InstantiatePrefab(audioPrefab);
            var audio = audioObject.GetComponent<AudioDeMenu>();
            foreach (var target in new[] { options, continueButton, levels, credits, exit })
                target.GetComponentInChildren<MenuHoverUI>(true).SetAudio(audio);
        }

        var eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        eventSystem.transform.SetParent(null);

        EditorSceneManager.SaveScene(scene, ScenePath);
        AddToBuildSettings(ScenePath);
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Selection.activeObject = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
        Debug.Log("Menu Principal listo: fondo, capas de nubes, humo/fuego, hélice, controles, hover y animaciones. Los controles solo responden visualmente al puntero.");
    }

    private static Transform NewLayer(Transform parent, string name)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        return go.transform;
    }

    private static void AddFullScreen(Transform parent, string name, Sprite sprite, bool raycast)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.pivot = new Vector2(0.5f, 0.5f);
        var image = go.GetComponent<Image>();
        image.sprite = sprite;
        image.preserveAspect = false;
        image.raycastTarget = raycast;
        image.color = Color.white;
    }

    private static void AddCloud(Transform parent, string name, string file, float x, float top, float width, float height, float alpha, float amplitude, float period)
    {
        var go = CreateImageAt(parent, name, LoadSprite(file), x, top, width, height, alpha, false);
        var drift = go.AddComponent<CloudDriftUI>();
        drift.Configure(amplitude, period);
    }

    private static GameObject AddAnimatedImage(Transform parent, string name, string sheetFile, string controllerName, float x, float top, float width, float height, float alpha)
    {
        var go = CreateImageAt(parent, name, FirstFrame(sheetFile), x, top, width, height, alpha, false);
        var animator = go.AddComponent<Animator>();
        animator.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(Controllers + controllerName + ".controller");
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        animator.applyRootMotion = false;
        return go;
    }

    private static GameObject AddInteractive(Transform parent, string name, string normalFile, string hoverFile, float x, float top, float width, float height)
    {
        var group = NewLayer(parent, name).gameObject;
        var hover = CreateImageAt(group.transform, "Hover_trasero", LoadSprite(hoverFile), 0f, 0f, width, height, 1f, false, true);
        var normal = CreateImageAt(group.transform, "Arte_normal", LoadSprite(normalFile), 0f, 0f, width, height, 1f, true);
        SetPosition(group.GetComponent<RectTransform>(), x, top, width, height);
        var hoverRect = hover.GetComponent<RectTransform>();
        hoverRect.anchorMin = Vector2.zero;
        hoverRect.anchorMax = Vector2.one;
        hoverRect.offsetMin = Vector2.zero;
        hoverRect.offsetMax = Vector2.zero;
        var normalRect = normal.GetComponent<RectTransform>();
        normalRect.anchorMin = Vector2.zero;
        normalRect.anchorMax = Vector2.one;
        normalRect.offsetMin = Vector2.zero;
        normalRect.offsetMax = Vector2.zero;
        normalRect.SetAsLastSibling();
        var interaction = normal.AddComponent<MenuHoverUI>();
        interaction.SetHoverVisual(hover);
        return group;
    }

    private static GameObject CreateImageAt(Transform parent, string name, Sprite sprite, float x, float top, float width, float height, float alpha, bool raycast, bool hidden = false)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        var image = go.GetComponent<Image>();
        image.sprite = sprite;
        image.color = new Color(1f, 1f, 1f, alpha);
        image.preserveAspect = false;
        image.raycastTarget = raycast;
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        SetPosition(rt, x, top, width, height);
        go.SetActive(!hidden);
        return go;
    }

    private static void SetPosition(RectTransform rt, float x, float top, float width, float height)
    {
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(width, height);
        rt.anchoredPosition = new Vector2(x + width * 0.5f - CanvasWidth * 0.5f, CanvasHeight * 0.5f - top - height * 0.5f);
    }

    private static Sprite LoadSprite(string file)
    {
        var path = Sprites + file;
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sprite == null) throw new InvalidOperationException("No se encontró el sprite " + path);
        return sprite;
    }

    private static Sprite FirstFrame(string file)
    {
        var path = Anim + file;
        var frames = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().OrderBy(s => s.name, StringComparer.Ordinal).ToArray();
        if (frames.Length == 0) throw new InvalidOperationException("La hoja no tiene sprites importados: " + path);
        return frames[0];
    }

    private static void ConfigureSingle(string file)
    {
        ConfigureTexture(Sprites + file, 1, 1, false);
    }

    private static void ConfigureGrid(string file, int columns, int rows)
    {
        ConfigureTexture(Anim + file, columns, rows, true);
    }

    private static void ConfigureTexture(string path, int columns, int rows, bool multiple)
    {
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null) throw new InvalidOperationException("No se pudo configurar: " + path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = multiple ? SpriteImportMode.Multiple : SpriteImportMode.Single;
        importer.alphaIsTransparency = true;
        importer.filterMode = FilterMode.Bilinear;
        importer.textureCompression = TextureImporterCompression.CompressedHQ;
        importer.mipmapEnabled = false;
        importer.spritePixelsPerUnit = 100f;
        if (multiple)
        {
#pragma warning disable 618
            var metadata = new List<SpriteMetaData>();
            int width = importer.maxTextureSize > 0 ? GetPngWidth(path) : 0;
            int height = GetPngHeight(path);
            int cellWidth = width / columns;
            int cellHeight = height / rows;
            for (int row = 0; row < rows; row++)
            for (int col = 0; col < columns; col++)
            {
                metadata.Add(new SpriteMetaData
                {
                    name = Path.GetFileNameWithoutExtension(path) + "_r" + row.ToString("00") + "_c" + col.ToString("00"),
                    rect = new Rect(col * cellWidth, height - (row + 1) * cellHeight, cellWidth, cellHeight),
                    alignment = (int)SpriteAlignment.Center,
                    pivot = new Vector2(0.5f, 0.5f)
                });
            }
            importer.spritesheet = metadata.ToArray();
#pragma warning restore 618
        }
        importer.SaveAndReimport();
    }

    private static int GetPngWidth(string path)
    {
        var bytes = File.ReadAllBytes(path);
        return (bytes[16] << 24) | (bytes[17] << 16) | (bytes[18] << 8) | bytes[19];
    }

    private static int GetPngHeight(string path)
    {
        var bytes = File.ReadAllBytes(path);
        return (bytes[20] << 24) | (bytes[21] << 16) | (bytes[22] << 8) | bytes[23];
    }

    private static void CreateClipsAndControllers()
    {
        CreateLoop("Helice_Fondo_Rapida", "ANIM_HELICE_FONDO_RAPIDA_24F.png", 24, false, true);
        CreateLoop("Helice_Carga_Lenta", "ANIM_HELICE_CARGA_LENTA_8F.png", 4, false, true);
        CreateLoop("Fuego", "FUEGO_ANIM_8F.png", 8, false, true);
        CreateLoop("Humo_Negro", "HUMO_NEGRO_ANIM_8F.png", 6, false, true);
        CreateLoop("Manecilla_180_Superior", "MANECILLA_180_SUPERIOR.png", 12, true, false);
    }

    private static void CreateLoop(string clipName, string sourceFile, int sampleRate, bool pingPong, bool loop)
    {
        var path = Anim + sourceFile;
        var frames = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().OrderBy(s => s.name, StringComparer.Ordinal).ToList();
        if (frames.Count == 0) throw new InvalidOperationException("No hay cuadros en " + path);
        if (pingPong && frames.Count > 2)
            frames.AddRange(frames.Skip(1).Take(frames.Count - 2).Reverse());

        string clipPath = Clips + clipName + ".anim";
        string controllerPath = Controllers + clipName + ".controller";
        if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(clipPath) != null) AssetDatabase.DeleteAsset(clipPath);
        if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(controllerPath) != null) AssetDatabase.DeleteAsset(controllerPath);

        var clip = new AnimationClip { name = clipName, frameRate = sampleRate };
        var binding = EditorCurveBinding.PPtrCurve(string.Empty, typeof(Image), "m_Sprite");
        var keys = new ObjectReferenceKeyframe[frames.Count];
        for (int i = 0; i < frames.Count; i++)
            keys[i] = new ObjectReferenceKeyframe { time = i / (float)sampleRate, value = frames[i] };
        AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);
        var settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = loop;
        AnimationUtility.SetAnimationClipSettings(clip, settings);
        AssetDatabase.CreateAsset(clip, clipPath);

        var controller = AnimatorController.CreateAnimatorControllerAtPathWithClip(controllerPath, clip);
        if (controller == null) throw new InvalidOperationException("No se pudo crear el controlador " + controllerPath);
    }

    private static void EnsureFolder(string parentPath, string name)
    {
        var folder = parentPath + name;
        if (!AssetDatabase.IsValidFolder(folder))
            AssetDatabase.CreateFolder(parentPath.TrimEnd('/'), name);
    }

    private static void AddToBuildSettings(string scenePath)
    {
        var existing = EditorBuildSettings.scenes
            .Where(s => s.path != scenePath && AssetDatabase.LoadAssetAtPath<SceneAsset>(s.path) != null)
            .ToList();
        existing.Insert(0, new EditorBuildSettingsScene(scenePath, true));
        EditorBuildSettings.scenes = existing.ToArray();
    }
}





