using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public static class ComprobarAudio
{
    static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
    }

    public static void Run()
    {
        try
        {
            Type nivel = Type.GetType("AudioDeNivel, Assembly-CSharp");
            Type menu = Type.GetType("AudioDeMenu, Assembly-CSharp");
            Check(nivel != null, "Falta AudioDeNivel: los eventos todavía no reproducen audio.");
            Check(menu != null, "Falta AudioDeMenu.");
            CheckClips();
            RunCases(nivel, menu);
            Debug.Log("AUDIO_VALIDATION_PASS");
            EditorApplication.Exit(0);
        }
        catch (Exception e)
        {
            Debug.LogError("AUDIO_VALIDATION_FAIL: " + e);
            EditorApplication.Exit(1);
        }
    }

    static void RunCases(Type nivel, Type menu)
    {
        var events = Type.GetType("EventosDeJuego, Assembly-CSharp");
        var clip = AudioClip.Create("Prueba", 44100, 1, 44100, false);
        var go = new GameObject("Prueba nivel");
        go.SetActive(false);
        var component = go.AddComponent(nivel);
        nivel.GetField("musica").SetValue(component, clip);
        nivel.GetField("ambiente").SetValue(component, clip);
        nivel.GetField("victoria").SetValue(component, clip);
        Invoke(component, "Awake");
        Invoke(component, "OnEnable");
        var music = (AudioSource)nivel.GetField("fuenteMusica", BindingFlags.Instance|BindingFlags.NonPublic).GetValue(component);
        var ambient = (AudioSource)nivel.GetField("fuenteAmbiente", BindingFlags.Instance|BindingFlags.NonPublic).GetValue(component);
        Check(music.clip == clip && music.loop, "La música debe usar el clip asignado y repetirse.");
        Check(ambient.clip == clip && ambient.loop, "El ambiente debe repetirse en su propia fuente.");
        int subscribed = ListenerCount(events, "DisparoRealizado");
        Check(subscribed == 1, "Debe existir una sola suscripción al evento de disparo.");
        var managerObject = new GameObject("Gestor de prueba");
        managerObject.SetActive(false);
        var managerType=Type.GetType("GestorDeNivel, Assembly-CSharp");
        var rulesType=Type.GetType("ReglasDeJuego, Assembly-CSharp");
        var manager=managerObject.AddComponent(managerType);
        var rules=ScriptableObject.CreateInstance(rulesType);
        managerType.GetField("reglas").SetValue(manager,rules);
        Invoke(manager,"Awake");
        rulesType.GetField("consumoPorSegundo").SetValue(rules,0f);
        events.GetMethod("RaiseCombustible").Invoke(null,new object[]{1f,100f});
        var warned=nivel.GetField("avisoCombustibleEmitido",BindingFlags.Instance|BindingFlags.NonPublic);
        Check(!(bool)warned.GetValue(component),"El tutorial sin consumo no debe emitir una alarma de combustible.");
        rulesType.GetField("consumoPorSegundo").SetValue(rules,0.5f);
        events.GetMethod("RaiseCombustible").Invoke(null,new object[]{1f,100f});
        Check((bool)warned.GetValue(component),"Debe avisar al bajar del umbral en niveles con consumo.");
        events.GetMethod("RaiseCombustible").Invoke(null,new object[]{3f,100f});
        Check(!(bool)warned.GetValue(component),"Recargar combustible debe permitir un nuevo aviso futuro.");
        Invoke(manager,"OnDestroy");
        UnityEngine.Object.DestroyImmediate(managerObject);
        UnityEngine.Object.DestroyImmediate(rules);
        var resultType = Type.GetType("ResultadoNivel, Assembly-CSharp");
        events.GetMethod("RaiseNivelCompletado").Invoke(null, new[]{Activator.CreateInstance(resultType)});
        Check((bool)nivel.GetField("terminado", BindingFlags.Instance|BindingFlags.NonPublic).GetValue(component), "La victoria debe cerrar el audio del nivel.");
        Check(!music.isPlaying && !ambient.isPlaying, "La victoria debe detener música y ambiente.");
        Invoke(component, "OnDisable");
        Check(ListenerCount(events,"DisparoRealizado") == 0, "Deshabilitar debe quitar las suscripciones.");
        Invoke(component, "OnEnable");
        Check(ListenerCount(events,"DisparoRealizado") == 1, "Reactivar no debe duplicar los sonidos.");
        Check(!(bool)nivel.GetField("terminado", BindingFlags.Instance|BindingFlags.NonPublic).GetValue(component), "Reactivar debe restaurar el estado de audio.");
        Invoke(component, "OnDisable");
        UnityEngine.Object.DestroyImmediate(go);

        go = new GameObject("Prueba menu");
        go.SetActive(false);
        component = go.AddComponent(menu);
        menu.GetField("musica").SetValue(component,clip);
        Invoke(component,"Awake");
        Invoke(component,"OnEnable");
        Check(go.GetComponents<AudioSource>().Length == 2, "El menú necesita fuentes separadas para música y botones.");
        foreach (string method in new[]{"Hover","Click","Confirmar","Volver","Iniciar","AbrirPausa","CerrarPausa"})
            menu.GetMethod(method).Invoke(component,null); // Clips sin asignar deben ser tolerados.
        Invoke(component,"OnDisable");
        foreach(var source in go.GetComponents<AudioSource>()) Check(!source.isPlaying,"El menú no debe seguir sonando deshabilitado.");
        UnityEngine.Object.DestroyImmediate(go);
        UnityEngine.Object.DestroyImmediate(clip);
    }

    [Serializable] class Catalogo { public Entrada[] archivos; }
    [Serializable] class Entrada { public string ruta; }

    static void CheckClips()
    {
        var text=AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/03_Compartidos/Audio/CatalogoAudio.json");
        Check(text != null,"El catálogo debe importarse como TextAsset.");
        var catalog=JsonUtility.FromJson<Catalogo>(text.text);
        Check(catalog.archivos.Length==35,"Se esperan 35 archivos de audio nuevos.");
        foreach(var entry in catalog.archivos)
        {
            var clip=AssetDatabase.LoadAssetAtPath<AudioClip>(entry.ruta);
            Check(clip != null && clip.length>0f,"Unity no puede importar "+entry.ruta);
        }
        var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/01_Interfaces/MenuPrincipal/Prefabs/AudioMenu.prefab");
        Check(prefab != null,"Debe existir el prefab de audio de menú.");
        var menu=prefab.GetComponent(Type.GetType("AudioDeMenu, Assembly-CSharp"));
        Check(menu != null,"El prefab debe llevar AudioDeMenu.");
        foreach(string field in new[]{"musica","hover","click","confirmar","volver","iniciar","abrirPausa","cerrarPausa"})
            Check(menu.GetType().GetField(field).GetValue(menu) != null,"Clip sin asignar en menú: "+field);
    }

    static int ListenerCount(Type events, string name)
    {
        var callback=(Delegate)events.GetField(name,BindingFlags.Static|BindingFlags.NonPublic).GetValue(null);
        return callback?.GetInvocationList().Length ?? 0;
    }

    static void Invoke(Component component, string method) => component.GetType().GetMethod(method, BindingFlags.Instance|BindingFlags.NonPublic).Invoke(component,null);
}
