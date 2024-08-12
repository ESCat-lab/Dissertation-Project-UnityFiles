Shader "ESC_Shaders/BrushShader"
{
    Properties
    {
        _Tint ("Tint", Color) = (1,1,1,1)
    }

    SubShader
    {
        Pass
        {
            Tags 
            {
				"LightMode" = "UniversalForward"
			}

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #define UNITY_INDIRECT_DRAW_ARGS IndirectDrawIndexedArgs
            #include "UnityIndirect.cginc"

            float4 _Tint;

            struct vertexData
            {
                float4 pos : SV_POSITION;
                float4 color : COLOR0;
            };

            StructuredBuffer<float3> _Positions;
            uniform float4x4 _ObjectToWorld;

            vertexData vert(uint svVertexID: SV_VertexID, uint svInstanceID : SV_InstanceID)
            {
                InitIndirectDrawArgs(0);
                vertexData vData;
                
                uint cmdID = GetCommandID(0);
                uint instanceID = GetIndirectInstanceID(svInstanceID);

                float3 pos = _Positions[GetIndirectVertexID(svVertexID)];
                float4 worldPos = mul(_ObjectToWorld, float4(pos, 1.0f));

                vData.pos = mul(UNITY_MATRIX_VP, worldPos);
                vData.color = _Tint;
                return vData;
            }

            float4 frag(vertexData i) : SV_Target
            {
                return i.color;
            }
            ENDHLSL
        }
    }
}