Shader "Unlit/SimpleMist"
{
    Properties
    {
        _Color ("Mist Color", Color) = (1,1,1,0.5)
        _Speed ("Speed", Vector) = (0.1, 0.05, 0, 0)
        _Scale ("Noise Scale", Float) = 5.0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

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

            float4 _Color;
            float4 _Speed;
            float _Scale;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            // Simple pseudo-random function
            float random (float2 st) {
                return frac(sin(dot(st.xy, float2(12.9898,78.233)))*43758.5453123);
            }

            // Value Noise 2D
            float noise (float2 st) {
                float2 i = floor(st);
                float2 f = frac(st);

                // Four corners
                float a = random(i);
                float b = random(i + float2(1.0, 0.0));
                float c = random(i + float2(0.0, 1.0));
                float d = random(i + float2(1.0, 1.0));

                float2 u = f * f * (3.0 - 2.0 * f); // Smoothstep interpolation logic

                return lerp(a, b, u.x) +
                        (c - a)* u.y * (1.0 - u.x) +
                        (d - b) * u.x * u.y;
            }

            // Fractal Brownian Motion for cloudier look
            float fbm (float2 st) {
                float v = 0.0;
                float a = 0.5;
                float2 shift = float2(100.0, 100.0);
                // Rotate to reduce axial bias
                float2x2 rot = float2x2(cos(0.5), sin(0.5), -sin(0.5), cos(0.50));
                for (int i = 0; i < 4; ++i) {
                    v += a * noise(st);
                    st = mul(rot, st) * 2.0 + shift;
                    a *= 0.5;
                }
                return v;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Animated UV
                float2 st = i.uv * _Scale + _Time.y * _Speed.xy;
                
                // Generate noise value (0.0 to 1.0 approx)
                float n = fbm(st);

                // Use noise for alpha variation
                float4 col = _Color;
                // Soft feeling: alpha depends on noise
                col.a *= n; 

                return col;
            }
            ENDCG
        }
    }
}
