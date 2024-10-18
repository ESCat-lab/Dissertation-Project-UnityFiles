Shader "Custom/BrushGeneration"
{
    Properties
    {   
        _MainTex ("Brush Texture", 2D) = "white" {}
        _Albedo ("Albedo", 2D) = "white" {}
        _Tint ("Tint", Color) = (1,1,1,1)
        _ShadowColor ("Shadow Tint", Color) = (1,1,1,1)
        _Smoothness ("Smoothness", Range(0, 1)) = 0.5
        _SpecularTint ("Specular", Color) = (0.5, 0.5, 0.5)

        _XRatio("X Lenght of Brush Strokes", Integer) = 1
        _YRatio("Y Lenght of Brush Strokes", Integer) = 1
        _brushDensity("Brush Count Per Triangle", Integer) = 3
        _seed("Randomizer Seed", Integer) = 42
        _maxPlaneSize("Brush Size Maximum", Float) = 1
        _minPlaneSize("Brush Size Minimum", Float) = 0.1
        _rotation("Brush Rotation Amount", Float)  = 0.1
    }
    SubShader
    {
        Tags 
        { 
            "Queue"="Transparent" 
            "RenderType"="Transparent" 
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
        }

        Pass
        {
            Cull Back      // Render the front-facing triangles
            ZWrite Off       // Enable depth writing
            ZTest LEqual    // Test depth for proper order
            Blend SrcAlpha OneMinusSrcAlpha  // Standard alpha blending

            Tags 
            { 
                "LightMode" = "UniversalForward"
            }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile_fog

            //Generation Parameters
            int _XRatio;
            int _YRatio;
            int _brushDensity;
            int _seed;
            float _maxPlaneSize;
            float _minPlaneSize;
            float _rotation;

            //Lighting Parameters
            sampler2D _MainTex;
            sampler2D _Albedo;
            float4 _Tint;
            float4 _ShadowColor;
            float _Smoothness;
            float4 _SpecularTint;
    
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct appdata
            {
                float4 positionOS : POSITION; //Object Space
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2g //Vertex to Geometry Shader
            {
                float4 positionOS : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct g2f //Geometry to Fragment Shader
            {
                float4 positionCS : SV_POSITION; //Clip Space
                float4 positionWS : TEXCOORD2; //World Space
                float3 normal : TEXCOORD1;
                float2 uv : TEXCOORD0;
                float2 uvOG : TEXCOORD3;
            };

            // Translated from Python Code by Bolster (2023) to HLSL Code.
            // Bolster, A. (2023). Generate a Random 3D Unit Vector with Uniform Spherical Distribution. [online] Github. 
            // Available at: https://gist.github.com/andrewbolster/10274979 [Accessed 13 Oct. 2024].
            float3 randomThreeVector(float rand1, float rand2)
            {   
                float phi = rand1 ;
    
                float costheta = rand2;
                float theta = acos(costheta);

                // Convert spherical coordinates to Cartesian coordinates
                float x = sin(theta) * cos(phi);
                float y = sin(theta) * sin(phi);
                float z = cos(theta);

                return float3(x, y, z);
            }
            //-----------------------------------------------------------------------------------------------------------

            float RandomFloatRange(float2 seed, float min, float max)
            {
                float hashedValue = frac(sin(dot(seed, float2(12.9898, 78.233)) + 4.1414) * 43758.5453);

                // Map the 0-1 random value to the range
                return min + hashedValue * (max - min);
            }

            v2g AssignV2g(float4 positionOS, float3 normal, float2 uv)
            {
                //float4 positionOS : POSITION;
                //float3 normal : NORMAL;
                //float2 uv : TEXCOORD0;

                v2g output0 = (v2g)0;
                output0.positionOS = positionOS;
                output0.normal = normal;
                output0.uv = uv;
                return output0;
            }

            g2f AssignG2f(float3 positionOS, float3 normal, float2 uv, float2 uvOG)
            {
                //float4 positionCS : SV_POSITION; //Clip Space
                //float4 positionWS : TEXCOORD2; //World Space
                //float3 normal : TEXCOORD1;
                //float2 uv : TEXCOORD0;

                g2f output0 = (g2f)0;
                output0.positionCS = TransformObjectToHClip(float4(positionOS, 1.0));
                output0.positionWS = mul(unity_ObjectToWorld, float4(positionOS, 1.0));
                output0.normal = normal;
                output0.uv = uv;
                output0.uvOG = uvOG;
                return output0;
            }

            //MAIN FUNCTIONS START HERE
            v2g vert (appdata v)
            {
                v2g i = (v2g)0;
                i = AssignV2g(v.positionOS, normalize(i.normal), v.uv);
                return i;
            }

            [maxvertexcount(66)] //TODO: Make a maximum vertex output
            void geom(triangle v2g input[3], inout TriangleStream<g2f> triStream, uint triangleID: SV_PrimitiveID)
            {
                float3 vert0 = input[0].positionOS.xyz;
                float3 vert1 = input[1].positionOS.xyz;
                float3 vert2 = input[2].positionOS.xyz;
                
                //Area = Square root of√s(s - a)(s - b)(s - c) where s is half the perimeter, or (a + b + c)/2.
                float a = distance(vert0, vert1);
                float b = distance(vert1, vert2);
                float c = distance(vert2, vert0);
                float s = length(a + b + c)/2;
                float area = sqrt(s * (s - a) * (s - b) * (s - c));
                float size = lerp(_minPlaneSize, _maxPlaneSize, area);
                
                int tempSeed = _seed + triangleID;
                for(int i = 0; i < _brushDensity; i++)
                {
                    // Compute point on the plane by linear combination of triangle vertices
                    // Vector3 P = A + pointDensityA * (B - A) + pointDensityB * (C - A); 
                    float pointDensityA = RandomFloatRange(float2(tempSeed, tempSeed), 0.0, 1.0);
                    tempSeed++;
    
                    float pointDensityB = RandomFloatRange(float2(tempSeed, tempSeed), 0.0, 1.0);
                    tempSeed++;
          
                    // Ensure the points are inside the triangle (barycentric coordinate system)
                    if (pointDensityA + pointDensityB > 1)
                    {
                        pointDensityA = 1 - pointDensityA;
                        pointDensityB = 1 - pointDensityB;
                    }
    
                    float3 P = vert0 + pointDensityA * (vert1 - vert0) + pointDensityB * (vert2 - vert0);
                    // P = mul(unity_WorldToObject, float4(P, 1.0)).xyz;
                    // Average the normals
                    float3 N = normalize(cross(vert1 - vert0, vert2 - vert0));
    
                    float3 up = float3(0, 1, 0);
                
                    if(abs(dot(up, N)) > 0.95)
                    {
                        up = float3(1,0,0);
                    }
    
                    float3 a = normalize(cross(N, up));
                    float3 b = normalize(cross(N, a));
                
                    float3 zOffset = N * RandomFloatRange(float2(tempSeed, tempSeed), -0.01, 0.01);
                    tempSeed++;
    
                    if(_XRatio == 0)
                    {
                        _XRatio = 1;
                    }
                    if(_YRatio == 0)
                    {
                        _YRatio = 1;
                    }
                    
                    float planeSizeX = size * _XRatio / (_XRatio + _YRatio);
                    float planeSizeY = size * _YRatio / (_XRatio + _YRatio);
    
                    float3 pos0 = P - (a * planeSizeX + b * planeSizeY) * size + zOffset;
                    float3 pos1 = P - (a * planeSizeX - b * planeSizeY) * size + zOffset;
                    float3 pos2 = P + (a * planeSizeX + b * planeSizeY) * size + zOffset;
                    float3 pos3 = P + (a * planeSizeX - b * planeSizeY) * size + zOffset;
    
                    g2f output0 = AssignG2f(pos0, N, float2(0,1), input[0].uv);
                    g2f output1 = AssignG2f(pos1, N, float2(0,0), input[1].uv);
                    g2f output2 = AssignG2f(pos2, N, float2(1,0), input[2].uv);
                    g2f output3 = AssignG2f(pos3, N, float2(1,1), input[0].uv);
    
                    triStream.Append(output0);
                    triStream.Append(output3);
                    triStream.Append(output2);
    
                    triStream.RestartStrip();
    
                    triStream.Append(output2);
                    triStream.Append(output1);
                    triStream.Append(output0);
    
                    triStream.RestartStrip();
                }
    
            }

            //Blinn Phong Lighting Model referencing guide by
            //Flick, J. (2016). Rendering 4. [online] Catlikecoding.com. 
            //Available at: https://catlikecoding.com/unity/tutorials/rendering/part-4/ [Accessed 16 Oct. 2024].
			half4 frag (g2f i) :SV_TARGET
            {
                //this is a small correction that can be removed to optimize
				i.normal = normalize(i.normal);
				float3 lightDir = _MainLightPosition.xyz;
                float3 viewDir = normalize(_WorldSpaceCameraPos - i.positionWS);
				float3 lightColor = _MainLightColor.rgb;
                half4 shadowColor = _ShadowColor;

                float3 albedo = tex2D(_Albedo, i.uvOG).rgb  * _Tint;
				albedo = albedo * (1 - max(_SpecularTint.r, max(_SpecularTint.g, _SpecularTint.b)));
                
                #ifdef _MAIN_LIGHT_SHADOWS
                    VertexPositionInputs vertexInput = (VertexPositionInputs)0;
                    vertexInput.positionWS = i.positionWS;
                    float4 shadowCoord = GetShadowCoord(vertexInput);
                    half shadowAttenutation = MainLightRealtimeShadow(shadowCoord);
                    shadowColor = lerp(float4(albedo, 1), _ShadowColor, (1.0 - shadowAttenutation) * _ShadowColor.a);
                    //shadowColor *= clamp(dot(lightDir, i.normal), 0, 1);
                #endif

                //lightColor *= clamp(dot(lightDir, i.normal), 0, 1);
                float3 shade = lerp(shadowColor.xyz, lightColor, clamp(dot(lightDir, i.normal), 0, 1));
				float3 diffuse = albedo * shade;

				float3 halfVector = normalize(lightDir + viewDir);
                float3 specular = _SpecularTint.rgb * lightColor * pow(
					clamp(dot(halfVector, i.normal),0,1),
					clamp(_Smoothness, 0.0001, 1) * 100
				);

                half4 texColor = tex2D(_MainTex, i.uv) * float4(diffuse + specular, 1);
                return texColor;
			}
            ENDHLSL
        }
            // extra pass that renders to depth buffer only
        Pass 
        {
            ZWrite On
            ColorMask 0
        }
    }
}
