// Filename: PreProcessBrushGeometry.shader

Shader "Custom/PreProcessBrushGeometry"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag

            #include "UnityCG.cginc"

            // Buffer input from compute shader
            StructuredBuffer<float3> vertices : register(t0);
            StructuredBuffer<float3> normals : register(t1);

            struct appdata
            {
                uint vertexID : SV_VertexID;
            };

            struct v2g
            {
                float4 pos : POSITION;
                float3 normal : NORMAL;
            };

            struct g2f
            {
                float4 pos : SV_POSITION;
                float3 color : COLOR;
            };

            v2g vert(appdata v)
            {
                v2g o;
                o.pos = float4(vertices[v.vertexID], 1.0f);
                o.normal = normals[v.vertexID];
                return o;
            }

            // Geometry shader: generates quads for each point
            [maxvertexcount(4)]
            void geom(triangle v2g input[3], inout TriangleStream<g2f> triStream)
            {
                g2f output;

                for (int i = 0; i < 3; i++)
                {
                    output.pos = input[i].pos;
                    output.color = float3(0.5, 0.5, 0.8); // Some color for testing
                    triStream.Append(output);
                }
            }

            half4 frag(g2f i) : SV_Target
            {
                return half4(i.color, 1.0f);
            }

            ENDCG
        }
    }
    FallBack "Diffuse"
}
