// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "ESC_Shaders/MetallicLightTestShader"
{
    Properties
    {
        _Tint ("Tint", Color) = (1,1,1,1)
        _Smoothness ("Smoothness", Range(0, 1)) = 0.5
		_Metallic ("Metallic", Range(0, 1)) = 0
    }

    SubShader 
    {

		Pass {
            Tags 
            {
				"LightMode" = "UniversalForward"
			}

            HLSLPROGRAM
			#pragma vertex MyVertexProgram
			#pragma fragment MyFragmentProgram

            //#include "UnityCG.cginc"
            //code below includes it
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            float4 _Tint;
            float _Smoothness;
			float _Metallic;

            struct VertexData {
				float4 position : POSITION;
				float3 normal : NORMAL;
				float2 uv : TEXCOORD0;
			};

			struct Interpolators 
            {
				float4 position : SV_POSITION;
				float2 uv : TEXCOORD0;
                float3 normal : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
			};

            Interpolators MyVertexProgram (VertexData v)
            {
                Interpolators i;
                i.position = TransformObjectToHClip(v.position);
                i.worldPos = mul(unity_ObjectToWorld, v.position);
                i.uv = v.uv;
				i.normal = mul(
					transpose((float3x3)unity_WorldToObject),
					v.normal
				);

				i.normal = normalize(i.normal);
                return i;
			}

			float4 MyFragmentProgram (Interpolators i) :SV_TARGET
            {
                //this is a small correction that can be removed to optimize
				i.normal = normalize(i.normal);
				float3 lightDir = _MainLightPosition.xyz;
                float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
				float3 lightColor = _MainLightColor.rgb;

                float3 albedo = _Tint.rgb;
                half oneMinusDielectricSpec = unity_ColorSpaceDielectricSpec.a;
				float3 specColor = lerp(unity_ColorSpaceDielectricSpec.rgb, albedo, metallic);
                float oneMinusReflectivity = oneMinusDielectricSpec - metallic * oneMinusDielectricSpec;
                albedo = albedo * oneMinusReflectivity;

				albedo = albedo * (1 - max(_SpecularTint.r, max(_SpecularTint.g, _SpecularTint.b)));     
				float3 diffuse = albedo * lightColor * clamp(dot(lightDir, i.normal),0,1);
                
				float3 halfVector = normalize(lightDir + viewDir);
                float3 specular = specularTint *lightColor * pow(
					clamp(dot(halfVector, i.normal),0,1),
					clamp(_Smoothness, 0.0001, 1) * 100
				);           
                return float4(diffuse + specular, 1);
			}
            ENDHLSL
		}
	}
}
