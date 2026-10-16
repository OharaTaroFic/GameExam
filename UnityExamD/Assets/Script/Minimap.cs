using UnityEngine;
using UnityEngine.UI;

//ミニマップ: 迷路全体を真上から映して、画面の隅（RawImage）に出す
//再生したときに、迷路（Map）の壁の範囲に合わせて、カメラの位置と映す広さを自動で決める
[RequireComponent(typeof(Camera))]
public class Minimap : MonoBehaviour
{
    [Label("映す先（画面の隅の画像）")] public RawImage MapImage;
    [Label("画像の横幅")] public float Width = 270f;
    [Label("まわりの余白")] public float Margin = 0.5f;

    const string MapName = "Map";
    RenderTexture _rt;

    void Start()
    {
        var cam = GetComponent<Camera>();
        var map = GameObject.Find(MapName);
        if (map == null || MapImage == null) { gameObject.SetActive(false); return; }

        //迷路の範囲（床を除いた壁をすべて囲む箱）
        Bounds? b = null;
        foreach (var col in map.GetComponentsInChildren<Collider>())
        {
            if (col.bounds.size.y < 0.01f) { continue; }
            if (b.HasValue) { var e = b.Value; e.Encapsulate(col.bounds); b = e; }
            else { b = col.bounds; }
        }
        if (!b.HasValue) { gameObject.SetActive(false); return; }
        var bounds = b.Value;
        float w = bounds.size.x + Margin * 2f;
        float h = bounds.size.z + Margin * 2f;

        //真上から、範囲がちょうど収まるように映す
        cam.orthographic = true;
        cam.orthographicSize = h / 2f;
        transform.position = new Vector3(bounds.center.x, 50f, bounds.center.z);
        transform.rotation = Quaternion.Euler(90f, 0f, 0f);

        //縦横の比に合わせた映像の入れ物を作って、画面の隅の画像に貼る
        int texW = 256;
        int texH = Mathf.RoundToInt(texW * h / w);
        _rt = new RenderTexture(texW, texH, 16);
        cam.aspect = w / h;
        cam.targetTexture = _rt;
        MapImage.texture = _rt;
        MapImage.rectTransform.sizeDelta = new Vector2(Width, Width * h / w);
        MapImage.enabled = true; //シーンでは消しておき、映像ができてから出す（再生前に白い四角が出ないように）
    }

    void OnDestroy()
    {
        if (_rt != null) { _rt.Release(); }
    }
}
