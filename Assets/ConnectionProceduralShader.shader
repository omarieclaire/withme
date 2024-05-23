Shader "Debug/Connection" {
    Properties {

        _Color ("Color", Color) = (1,1,1,1)
        _Size ("Size", float) = .01
        _ConnectionDistance ("Connection Distance", float) = 1
    }


    SubShader{
        Cull Off
        Pass{

            CGPROGRAM
            
            #pragma target 4.5

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"



            uniform int _Count;
            uniform float _Size;
            uniform float3 _Color;
            uniform float _ConnectionDistance;

            
            #if defined( SHADER_API_D3D11 )
            struct Vert{
                float3 pos;
                float size;
            };
            #endif

            #if defined( SHADER_API_METAL )       
            struct Vert{
                float3 pos;
                float size;
            };
            #endif


            #if defined(SHADER_API_METAL)
            StructuredBuffer<Vert> _VertBuffer;
            #endif

            #if defined(SHADER_API_D3D11) 
            StructuredBuffer<Vert> _VertBuffer;
            #endif

            
            


            //uniform float4x4 worldMat;

            //A simple input struct for our pixel shader step containing a position.
            struct varyings {
                float4 pos      : SV_POSITION;
            };

            //Our vertex function simply fetches a point from the buffer corresponding to the vertex index
            //which we transform with the view-projection matrix before passing to the pixel program.
            varyings vert (uint id : SV_VertexID){

                varyings o;

                int base = id / 6;
                int alternate = id %6;

                int id1 = base / _Count;
                int id2 = base % _Count;

                if( id1 == id2 ) return o;
                if( id1 >= _Count ) return o;
                if( id2 >= _Count ) return o;

                float3 p1 = _VertBuffer[id1].pos;
                float3 p2 = _VertBuffer[id2].pos;

                float s1 = _VertBuffer[id1].size;
                float s2 = _VertBuffer[id2].size;

                float3 dir = p2 - p1;


                if( length(dir) < .0001){ return o; }
                if( length(dir) > _ConnectionDistance ){ return o; }

                float3 fwd =  float3(0,1,0);
                fwd = UNITY_MATRIX_V[2].xyz;
                float3 up = normalize(cross( fwd, dir ));
                float3 right = normalize(cross( up, dir ));

                float3 fP1 = p1 - up*_Size* s1;
                float3 fP2 = p2 - up*_Size*s2;
                float3 fP3 = p1 + up*_Size* s1;
                float3 fP4 = p2 + up*_Size* s2;




                float3 extra = float3(0,0,0);

                
                float2 uv = float2(0,0);

                if( alternate == 0 ){ extra = fP1; uv = float2(0,0); }
                if( alternate == 1 ){ extra = fP2; uv = float2(1,0); }
                if( alternate == 2 ){ extra =  fP4; uv = float2(1,1); }
                if( alternate == 3 ){ extra = fP1; uv = float2(0,0); }
                if( alternate == 4 ){ extra =  fP4; uv = float2(1,1); }
                if( alternate == 5 ){ extra = fP3; uv = float2(0,1); }


                

                o.pos = mul (UNITY_MATRIX_VP, float4(extra,1.0f));

                

                return o;

            }




            //Pixel function returns a solid color for each point.
            float4 frag (varyings v) : COLOR {
                return float4(_Color,1 );
            }

            ENDCG

        }
    }

    Fallback Off


}
