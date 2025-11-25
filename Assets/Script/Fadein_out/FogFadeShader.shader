Shader "Custom/FogFadeShader"
{
    Properties
    {
        _MainTex ("Noise Texture", 2D) = "white" {}
        _Color ("Fade Color", Color) = (0,0,0,1)
        _Cutoff ("Fade Value", Range(0, 1)) = 0
        _Smoothing ("Smoothing", Range(0.01, 0.5)) = 0.1
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        LOD 100
        ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            float _Cutoff;
            float _Smoothing;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float noise = tex2D(_MainTex, i.uv).r;

                // 基本計算
                // Cutoffが0に近いほど、範囲が広がり黒くなる(Alphaが1になる)
                // Cutoffが1に近いほど、範囲が狭まり透明になる(Alphaが0になる)
                float alpha = smoothstep(_Cutoff, _Cutoff + _Smoothing, 1 - noise);
                
                // ▼▼▼ 修正箇所: 補正ロジックを計算結果に合わせる ▼▼▼
                
                // Cutoffが0(以下)なら、強制的に真っ黒(Alpha 1)
                if (_Cutoff <= 0.01) alpha = 1;

                // Cutoffが1(以上)なら、強制的に透明(Alpha 0)
                if (_Cutoff >= 0.99) alpha = 0;
                
                // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲

                return fixed4(_Color.rgb, alpha);
            }
            ENDCG
        }
    }
}