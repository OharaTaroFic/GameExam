//模様を、箱の大きさではなく「場所」で貼る（壁を伸ばしても、レンガの大きさが変わらない）
//向きのそろった箱（壁）用：面の向きに合わせて、横から見た面・上の面にそれぞれ模様を貼る
Shader "UnityExamD/WorldTiled"
{
    Properties
    {
        _Color ("色", Color) = (1,1,1,1)
        _MainTex ("模様", 2D) = "white" {}
        _TileSize ("模様 1 枚の大きさ", Float) = 1
        _Glossiness ("つや", Range(0,1)) = 0.1
        _Metallic ("金属っぽさ", Range(0,1)) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0

        sampler2D _MainTex;
        fixed4 _Color;
        float _TileSize;
        half _Glossiness;
        half _Metallic;

        struct Input
        {
            float3 worldPos;
            float3 worldNormal;
        };

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float3 n = abs(IN.worldNormal);
            float2 uv;
            if (n.y > n.x && n.y > n.z) { uv = IN.worldPos.xz; }//上・下の面
            else if (n.x > n.z) { uv = IN.worldPos.zy; }//左・右を向いた面
            else { uv = IN.worldPos.xy; }//前・後ろを向いた面
            fixed4 c = tex2D(_MainTex, uv / _TileSize) * _Color;
            o.Albedo = c.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = 1;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
