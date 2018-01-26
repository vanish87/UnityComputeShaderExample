﻿Shader "Render/SnowParticleShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
		SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 100
		//to make intensity in frag shader working
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma geometry geom
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog

			#pragma target 5.0

			#include "UnityCG.cginc"
			#include "Assets/Common.cginc"
			#include "Assets/SnowCommon.cginc"

			StructuredBuffer<SnowParticleStruct> _buffer;
			StructuredBuffer<SmallParticleStruct> _small_buffer;
			float4x4 _inv_view_mat;
			float _particle_size;

			struct vIn
			{
				uint index : SV_VERTEXID;
			};

			struct v2g
			{
				float3 position : TEXCOORD0; //here use TEXCOORD0 to save one byte for w
											 //if use SV_POSITION/POSITION, it will require float4 here
			};

			struct g2f
			{				
				float4 position : SV_POSITION;
				float2 uv		: TEXCOORD0;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			
			v2g vert (vIn v)
			{
				v2g o = (v2g)0;
				o.position = _buffer[v.index].position_;
				//o.position = _small_buffer[v.index].position;
				//o.position = float3(v.index%20, v.index/20,0);
				return o;
			}

			static const float3 g_positions[4] =
			{
				float3(-1, 1, 0),
				float3(1, 1, 0),
				float3(-1,-1, 0),
				float3(1,-1, 0),
			};

			static const float2 g_texcoords[4] =
			{
				float2(0, 0),
				float2(1, 0),
				float2(0, 1),
				float2(1, 1),
			};

			[maxvertexcount(4)]
			void geom(point v2g v[1], inout TriangleStream<g2f> out_stream)
			{
				g2f ret;

				[unroll]
				for (int i = 0; i < 4; ++i)
				{
					ret = (g2f)0;

					float3 pos = g_positions[i] * _particle_size;
					//TODO revert to world space;
					float3 pos_ws = mul(_inv_view_mat, pos);

					//get new position
					float3 new_pos = pos_ws + v[0].position;

					ret.position = UnityObjectToClipPos(float4(new_pos, 1.0));
					ret.uv = g_texcoords[i];

					out_stream.Append(ret);
				}
				out_stream.RestartStrip();
			}
			
			fixed4 frag (g2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = i.position;
				col = 1;
				float intensity = 0.5f - length(float2(0.5f, 0.5f) - i.uv);
				intensity = clamp(intensity, 0.0f, 0.5f) * 2.0f;
				return fixed4(col.xyz, intensity);
			}
			ENDCG
		}
	}
}
