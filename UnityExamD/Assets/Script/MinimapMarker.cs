using UnityEngine;

//ミニマップにだけ映る、色の付いた丸い印（再生したときに作る。Scene ビューやシーンには残らない）
//プレイヤー・敵・星の部品に付けておくと、生徒が置いた分にも自動で印が付く
public class MinimapMarker : MonoBehaviour
{
    [Label("印の色")] public Color Color = Color.white;
    [Label("印の大きさ")] public float Size = 1.6f;
    [Label("重なり順（大きいほど上）")] public int Order = 0;

    const string LayerName = "Minimap";
    const float Height = 3f; //壁（高さ 1）より上に置いて、壁に隠れないようにする
    static Sprite s_Circle;

    void Start()
    {
        int layer = LayerMask.NameToLayer(LayerName);
        if (layer < 0) { return; }
        MakeCircle("MinimapMarker_Outline", Color.white, Size * 1.3f, Order * 2, layer);
        MakeCircle("MinimapMarker", Color, Size, Order * 2 + 1, layer);
    }

    void MakeCircle(string name, Color color, float size, int order, int layer)
    {
        var go = new GameObject(name);
        go.layer = layer;
        go.transform.SetParent(transform, false);
        go.transform.position = new Vector3(transform.position.x, Height, transform.position.z);
        go.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        float parentScale = Mathf.Max(0.0001f, transform.lossyScale.x);
        go.transform.localScale = Vector3.one * (size / parentScale);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = Circle();
        sr.color = color;
        sr.sortingOrder = order;
    }

    //白い丸の画像を、プログラムで 1 回だけ作る
    static Sprite Circle()
    {
        if (s_Circle != null) { return s_Circle; }
        const int n = 64;
        var tex = new Texture2D(n, n, TextureFormat.RGBA32, false);
        float r = n / 2f;
        for (int y = 0; y < n; y++)
        {
            for (int x = 0; x < n; x++)
            {
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(r, r));
                float a = Mathf.Clamp01(r - d); //縁をなめらかにする
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
        }
        tex.Apply();
        s_Circle = Sprite.Create(tex, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), n);
        return s_Circle;
    }
}
