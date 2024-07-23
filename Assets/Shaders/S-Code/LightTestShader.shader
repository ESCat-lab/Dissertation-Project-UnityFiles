// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "ESC_Shaders/LightTestShader"
{
    Properties
    {
        _Tint ("Tint", Color) = (1,1,1,1)
    }

    SubShader 
    {

		Pass {
            CGPROGRAM
			#pragma vertex MyVertexProgram
			#pragma fragment MyFragmentProgram

            #include "UnityCG.cginc"

            float4 _Tint;

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
			};

            Interpolators MyVertexProgram (VertexData v)
            {
                Interpolators i;
                i.position = UnityObjectToClipPos(v.position);
                i.uv = v.uv;
                //This is more optimized
                i.normal = UnityObjectToWorldNormal(v.normal);

                //under the hood this code looks like the one below:
				//i.normal = mul(
				//	transpose((float3x3)unity_WorldToObject),
				//	v.normal
				//);

				i.normal = normalize(i.normal);
                return i;
			}

			float4 MyFragmentProgram (Interpolators i) :SV_TARGET
            {
                //this is a small correction that can be removed to optimize
                i.normal = normalize(i.normal);
                return float4(i.normal * 0.5 + 0.5, 1);
			}
            ENDCG
		}
	}
}
