Shader "Unlit/FadeShader"
{
    Properties
    {
        _Color ("Main Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "Queue"="Overlay" }
        Pass
        {
            ZTest Always
            ZWrite Off
            // Cull Off   // Allow backfaces to be rendered
            Blend SrcAlpha OneMinusSrcAlpha
            ColorMaterial AmbientAndDiffuse
            Lighting Off
            SetTexture[_MainTex] {combine primary}
        }
    }
}
