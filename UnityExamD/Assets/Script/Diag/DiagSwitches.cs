using System.Globalization;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

//【調査用・一時的】スマホで GameScene に入ると描画の機能が落ちる件の切り分け。調べ終わったら、このフォルダごと消す
//アドレスの後ろに付けた文字で、機能を切って試せる（例: http://192.168.0.8:8000/?nominimap=1&noshadow=1）
public static class DiagSwitches
{
    static string s_Query = "";
    static int s_CameraLogs;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Init()
    {
        var url = Application.absoluteURL ?? "";
        int q = url.IndexOf('?');
        s_Query = q >= 0 ? url.Substring(q + 1) : "";
        if (s_Query.Length == 0) { return; } //何も付いていないときは、記録も切り替えもしない

        Debug.Log("[diag] query=" + s_Query);
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += s => Debug.Log("[diag] unloaded " + s.name);
        RenderPipelineManager.beginCameraRendering += OnBeginCamera;
        RenderPipelineManager.endCameraRendering += OnEndCamera;

        var rp = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
        float scale = Num("scale", -1f);
        if (rp != null && scale > 0f) { rp.renderScale = scale; }
        Debug.Log("[diag] pipeline=" + (rp != null ? rp.name : "null") + " hdr=" + (rp != null && rp.supportsHDR) + " scale=" + (rp != null ? rp.renderScale : 0f) + " screen=" + Screen.width + "x" + Screen.height);
    }

    static bool Has(string key)
    {
        return ("&" + s_Query + "&").IndexOf("&" + key + "=1&") >= 0;
    }

    static float Num(string key, float def)
    {
        foreach (var part in s_Query.Split('&'))
        {
            var kv = part.Split('=');
            if (kv.Length == 2 && kv[0] == key && float.TryParse(kv[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var v)) { return v; }
        }
        return def;
    }

    //シーンを読み込んだ直後（Start より前、最初の 1 コマを描く前）に、機能を切る
    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("[diag] loaded " + scene.name);
        s_CameraLogs = 0;
        if (Has("nominimap"))
        {
            foreach (var m in Object.FindObjectsByType<Minimap>(FindObjectsSortMode.None))
            {
                m.enabled = false;
                m.GetComponent<Camera>().enabled = false;
                Debug.Log("[diag] minimap off");
            }
        }
        if (Has("nomarker"))
        {
            foreach (var mk in Object.FindObjectsByType<MinimapMarker>(FindObjectsSortMode.None)) { mk.enabled = false; }
            Debug.Log("[diag] marker off");
        }
        if (Has("noshadow"))
        {
            foreach (var l in Object.FindObjectsByType<Light>(FindObjectsSortMode.None)) { l.shadows = LightShadows.None; }
            Debug.Log("[diag] shadow off");
        }
        if (Has("noenemy"))
        {
            foreach (var e in GameObject.FindGameObjectsWithTag("Enemy")) { e.SetActive(false); }
            Debug.Log("[diag] enemy off");
        }
        if (Has("nofloor"))
        {
            foreach (var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                var mat = r.sharedMaterial;
                if (mat != null && mat.shader != null && mat.shader.name == "Custom/AlphaMaskURP") { r.enabled = false; }
            }
            Debug.Log("[diag] floor off");
        }
        if (Has("nocanvas"))
        {
            var c = GameObject.Find("TouchCanvas");
            if (c != null) { c.SetActive(false); }
            Debug.Log("[diag] canvas off");
        }
        if (Has("noitems"))
        {
            //モーションが毎フレーム隠し、Player.LateUpdate が毎フレーム出し直している 7 つの持ち物を、最初から外す
            int n = 0;
            foreach (var p in Object.FindObjectsByType<Player>(FindObjectsSortMode.None))
            {
                foreach (var r in p.GetComponentsInChildren<Renderer>(true))
                {
                    switch (r.name)
                    {
                        case "mesh_dagger": case "mesh_iceRod": case "mesh_arrow_01": case "mesh_arrow_02": case "mesh_arrow_03": case "mesh_quiver": case "mesh_bow":
                            r.gameObject.SetActive(false); n++; break;
                    }
                }
            }
            Debug.Log("[diag] items off " + n);
        }
        if (Has("noanim"))
        {
            foreach (var p in Object.FindObjectsByType<Player>(FindObjectsSortMode.None))
            {
                var a = p.GetComponent<Animator>();
                if (a != null) { a.enabled = false; }
            }
            Debug.Log("[diag] anim off");
        }
    }

    //どのカメラが描き始め・描き終えたか（読み込み直後の数回だけ）
    static void OnBeginCamera(ScriptableRenderContext context, Camera cam)
    {
        if (s_CameraLogs < 8) { Debug.Log("[diag] begin " + cam.name + " f=" + Time.frameCount); }
    }

    static void OnEndCamera(ScriptableRenderContext context, Camera cam)
    {
        if (s_CameraLogs < 8) { Debug.Log("[diag] end " + cam.name + " f=" + Time.frameCount); s_CameraLogs++; }
    }
}
