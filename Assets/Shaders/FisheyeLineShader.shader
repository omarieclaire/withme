Shader "Custom/FisheyeLineShader" 
    {
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Main Color", Color) = (1,1,1,1)
        _FisheyeStrength ("Fisheye Strength", Float) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

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
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float _FisheyeStrength;
            float4 _Color;

            v2f vert (appdata v)
            {
                v2f o;

                // Apply fisheye distortion
                float2 uv = v.vertex.xy / v.vertex.w;
                float dist = length(uv) * _FisheyeStrength;
                float factor = 1.0 / (1.0 + dist * dist);

                uv *= factor;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return tex2D(_MainTex, i.uv) * _Color;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
