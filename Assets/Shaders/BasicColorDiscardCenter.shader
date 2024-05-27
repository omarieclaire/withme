// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Unlit/BasicColorDiscardCenter"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _RingSize("_RingSize", Range(0.0, 1.0)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float match: TEXCOORD0;
            };


            float4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                float3 wNor = UnityObjectToWorldNormal(v.normal);
                float3 eye = _WorldSpaceCameraPos - mul(unity_ObjectToWorld,v.vertex).xyz;
                float eyeMatch = dot(normalize(eye), wNor);
                o.match = saturate(1.0 - abs(eyeMatch));
                return o;
            }

            float _RingSize;
            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = _Color;

                if(i.match < _RingSize)
                {
                    discard;
                }
                return col;
            }
            ENDCG
        }
    }
}
