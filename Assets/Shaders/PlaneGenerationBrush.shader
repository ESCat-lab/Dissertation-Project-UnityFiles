Shader "Custom/GeometryPlaneGenerator"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Constants
            {
                float sensitivity;
                float rotation; // Could be a randomization factor if needed  
                float2 XYRatio;
                uint brushDensity;
                uint seed;
                float planeSize;
                bool flipIndices;
                uint triangleCount; // Number of triangles in the reference mesh
            };

            struct _vertex
            {
                float3 position;
                float3 normal;
                int subMesh;
                float2 uv;
                float size;
            };

            struct appdata
            {
                uint vertexID : SV_VertexID; // Ensure this has the correct semantic
            };

            struct v2g
            {
                float4 pos : POSITION;  // Ensure POSITION semantic is used
                float3 normal : NORMAL;  // Ensure NORMAL semantic is used
            };

            struct g2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 color : COLOR;
            };

            // Buffer input from compute shader
            StructuredBuffer<_vertex> vertices : register(t0);
            StructuredBuffer<Constants> constants : register(t1);

            v2g vert(appdata v)
            {
                v2g o;
                o.pos = float4(vertices[v.vertexID].position, 1.0f);
                o.normal = vertices[v.vertexID].normal;
                return o;
            }

            [maxvertexcount(6)] // Each quad is made of two triangles = 6 vertices
            void geom(triangle v2g input[3], inout TriangleStream<g2f> triStream) // Input as triangle
            {
                // You may want to check how you get your vertex count from the input
                v2g center = input[0]; // Ensure you're reading from the right vertex
                
                // Calculate quad size based on input size and scaling sensitivity
                float planeSize = vertices[0].size / constants[0].planeSize; //find a way to get the plane size here!
                planeSize *= lerp(0.5f, 1.0f, frac(sin(dot(center.pos.xyz, float3(12.9898, 78.233, 0.0))) * 43758.5453)); // Random factor

                // Calculate dimensions based on XY ratio
                float planeSizeX = planeSize * constants[0].XYRatio.x / (constants[0].XYRatio.x + constants[0].XYRatio.y);
                float planeSizeY = planeSize * constants[0].XYRatio.y / (constants[0].XYRatio.x + constants[0].XYRatio.y);

                // Calculate basis vectors for the quad
                float3 up = float3(0, 1, 0);
                if (abs(dot(up, center.normal)) > 0.95f)
                {
                    up = float3(1, 0, 0); // Choose a different up vector if normal is almost parallel
                }

                // Compute rotation offset
                float3 rotationOffset = normalize(float3(sin(center.pos.y * constants[0].rotation), cos(center.pos.x * constants[0].rotation), sin(center.pos.z * constants[0].rotation)));

                // Calculate the orthogonal basis vectors
                float3 a = normalize(cross(center.normal, up)) + rotationOffset;
                float3 b = normalize(cross(center.normal, a));

                // Create the four quad corners
                float3 quadCorner0 = center.pos.xyz - (a * planeSizeX + b * planeSizeY) + center.normal * 0.01f;
                float3 quadCorner1 = center.pos.xyz - (a * planeSizeX - b * planeSizeY) + center.normal * 0.01f;
                float3 quadCorner2 = center.pos.xyz + (a * planeSizeX + b * planeSizeY) + center.normal * 0.01f;
                float3 quadCorner3 = center.pos.xyz + (a * planeSizeX - b * planeSizeY) + center.normal * 0.01f;

                // Define UV coordinates for the quad
                float2 uv0 = float2(0, 1);
                float2 uv1 = float2(0, 0);
                float2 uv2 = float2(1, 0);
                float2 uv3 = float2(1, 1);

                // Emit the two triangles that form the quad
                g2f output;
                output.color = float3(0.5, 0.5, 0.8); // Color for testing

                // First triangle
                output.pos = float4(TransformObjectToHClip(quadCorner0)); output.uv = uv0; triStream.Append(output);
                output.pos = float4(TransformObjectToHClip(quadCorner1)); output.uv = uv1; triStream.Append(output);
                output.pos = float4(TransformObjectToHClip(quadCorner2)); output.uv = uv2; triStream.Append(output);

                // Second triangle
                output.pos = float4(TransformObjectToHClip(quadCorner2)); output.uv = uv2; triStream.Append(output);
                output.pos = float4(TransformObjectToHClip(quadCorner3)); output.uv = uv3; triStream.Append(output);
                output.pos = float4(TransformObjectToHClip(quadCorner0)); output.uv = uv0; triStream.Append(output);
            }

            half4 frag(g2f i) : SV_Target
            {
                return half4(i.color, 1.0f);
            }

            ENDHLSL
        }
    }
    FallBack "Diffuse"
}
