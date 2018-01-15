Shader "Unlit/NewUnlitShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
		SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma geometry geom
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog

			#include "UnityCG.cginc"

			StructuredBuffer<float4> _pos_buffer_soa;

			struct vIn
			{
				uint index : SV_VERTEXID;
			};

			struct v2g
			{
				float4 position : POSITION;
			};

			struct g2f
			{				
				float4 position : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			
			v2g vert (vIn v)
			{
				v2g o;
				o.position = _pos_buffer_soa[v.index];
				o.position = float4(v.index,0,0,1);
				return o;
			}

			static const float3 g_positions[4] =
			{
				float3(-1, 1, 0),
				float3(1, 1, 0),
				float3(-1,-1, 0),
				float3(1,-1, 0),
			};

			[maxvertexcount(4)]
			void geom(point v2g v[1], inout TriangleStream<g2f> out_stream)
			{
				g2f ret = (g2f)0;

				[unroll]
				for (int i = 0; i < 4; ++i)
				{
					ret = (g2f)0;

					float3 pos = g_positions[i];
					//TODO revert to world space;
					float3 pos_ws = pos;

					//get new position
					float3 new_pos = pos_ws + v[0].position;

					ret.position = UnityObjectToClipPos(float4(new_pos, 1.0));
					out_stream.Append(ret);
				}
				out_stream.RestartStrip();
			}
			
			fixed4 frag (g2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = i.position;
				return col;
			}
			ENDCG
		}
	}
}
