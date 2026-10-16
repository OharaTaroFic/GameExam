using UnityEngine;

//ミニマップにだけ映る、色の付いた丸い印（プレイヤーは向きの分かる矢印。再生したときに作る。Scene ビューやシーンには残らない）
//プレイヤー・敵・星の部品に付けておくと、生徒が置いた分にも自動で印が付く
public class MinimapMarker : MonoBehaviour
{
    [Label("印の色")] public Color Color = Color.white;
    [Label("印の大きさ")] public float Size = 1.6f;
    [Label("重なり順（大きいほど上）")] public int Order = 0;
    [Label("矢印にする（向きが分かる。プレイヤー用）")] public bool Arrow = false;

    const string LayerName = "Minimap";
    const float Height = 3f; //壁（高さ 1）より上に置いて、壁に隠れないようにする
    static Sprite s_Circle;

    void Start()
    {
        int layer = LayerMask.NameToLayer(LayerName);
        if (layer < 0) { return; }
        if (Arrow)
        {
            //白い矢印（縁が印の色）。持ち主と一緒に回るので、向いている方向を指す
            MakeMark("MinimapMarker", ArrowSprite(Color), Color.white, Size * 1.5f, Order * 2 + 1, layer);
            return;
        }
        MakeMark("MinimapMarker_Outline", Circle(), Color.white, Size * 1.3f, Order * 2, layer);
        MakeMark("MinimapMarker", Circle(), Color, Size, Order * 2 + 1, layer);
    }

    void MakeMark(string name, Sprite sprite, Color color, float size, int order, int layer)
    {
        var go = new GameObject(name);
        go.layer = layer;
        go.transform.SetParent(transform, false);
        go.transform.position = new Vector3(transform.position.x, Height, transform.position.z);
        go.transform.rotation = Quaternion.Euler(90f, transform.eulerAngles.y, 0f);//画像の上が、持ち主の前を向く
        float parentScale = Mathf.Max(0.0001f, transform.lossyScale.x);
        go.transform.localScale = Vector3.one * (size / parentScale);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = color;
        sr.sortingOrder = order;
    }

    //矢印の画像を、プログラムで作る（中は白、縁は edge の色。上向き）
    static Sprite ArrowSprite(Color edge)
    {
        const int n = 64;
        const float border = 5f; //縁の太さ（画素）
        //矢印の形（-1〜1 の範囲。先・右下・後ろのくぼみ・左下）
        var pts = new[] { new Vector2(0f, 0.95f), new Vector2(0.8f, -0.85f), new Vector2(0f, -0.4f), new Vector2(-0.8f, -0.85f) };
        for (int i = 0; i < pts.Length; i++) { pts[i] = (pts[i] + Vector2.one) * (n / 2f); }
        var tex = new Texture2D(n, n, TextureFormat.RGBA32, false);
        for (int y = 0; y < n; y++)
        {
            for (int x = 0; x < n; x++)
            {
                var p = new Vector2(x + 0.5f, y + 0.5f);
                float d = float.MaxValue; //いちばん近い辺までの距離
                bool inside = false;
                for (int i = 0, j = pts.Length - 1; i < pts.Length; j = i++)
                {
                    d = Mathf.Min(d, DistanceToSegment(p, pts[j], pts[i]));
                    if ((pts[i].y > p.y) != (pts[j].y > p.y) && p.x < (pts[j].x - pts[i].x) * (p.y - pts[i].y) / (pts[j].y - pts[i].y) + pts[i].x) { inside = !inside; }
                }
                float a = inside ? 1f : Mathf.Clamp01(0.5f - d); //外側の縁をなめらかにする
                if (inside) { a = Mathf.Clamp01(d + 0.5f); }
                float t = inside ? Mathf.Clamp01(d - border + 0.5f) : 0f; //0 = 縁の色、1 = 白
                var c = Color.Lerp(edge, Color.white, t);
                c.a = a;
                tex.SetPixel(x, y, c);
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), n);
    }

    static float DistanceToSegment(Vector2 p, Vector2 a, Vector2 b)
    {
        var ab = b - a;
        float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / ab.sqrMagnitude);
        return Vector2.Distance(p, a + ab * t);
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
