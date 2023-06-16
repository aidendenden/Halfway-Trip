Shader "Hidden/Vista/Graph/GeometryMask"
{
	SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 100
		ColorMask R

		Pass
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma shader_feature_local BLEND_MAX
			#pragma shader_feature_local HEIGHT_MASK
			#pragma shader_feature_local SLOPE_MASK
			#pragma shader_feature_local DIRECTION_MASK

			#include "../Includes/ShaderIncludes.hlsl"
			#include MATH_HLSL
			#include GEOMETRY_HLSL

			struct appdata
			{
				float4 vertex: POSITION;
				float2 uv: TEXCOORD0;
			};

			struct v2f
			{
				float2 uv: TEXCOORD0;
				float4 vertex: SV_POSITION;
				float4 localPos: TEXCOORD1;
			};

			sampler2D _MainTex;
			float4 _MainTex_TexelSize;
			float3 _TerrainSize;

			#if HEIGHT_MASK
				float _MinHeight;
				float _MaxHeight;
				sampler2D _HeightTransition;
			#endif

			#if SLOPE_MASK
				float _MinAngle;
				float _MaxAngle;
				sampler2D _SlopeTransition;
			#endif

			#if DIRECTION_MASK
				float _DirectionAngle;
				float _DirectionTolerance;
				sampler2D _DirectionFalloff;
			#endif

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				o.localPos = v.vertex;
				return o;
			}

			float frag(v2f input): SV_Target
			{
				float h = tex2D(_MainTex, input.localPos).r;
				float mask;
				#if BLEND_MAX
					mask = 0;
				#else
					mask = 1;
				#endif

				#if HEIGHT_MASK
					float fHeightMin = _MinHeight / _TerrainSize.y;
					float fHeightMax = _MaxHeight / _TerrainSize.y;
					float fHeight = saturate(inverseLerp(h, fHeightMin, fHeightMax));
					float heightTransition = tex2D(_HeightTransition, float2(fHeight, 0.5)).r;
					float heightMask = (h >= fHeightMin) * (h <= fHeightMax) * heightTransition;
					#if BLEND_MAX
						mask = max(mask, heightMask);
					#else
						mask = mask * heightMask;
					#endif
				#endif
				
				#if SLOPE_MASK || DIRECTION_MASK
					float3 normal = normalFromHeightMap(_MainTex, _MainTex_TexelSize, _TerrainSize, input.localPos);
				#endif

				#if SLOPE_MASK
					float cosine = abs(normal.y);
					float slopeAngle = acos(cosine);
					float slopeTransitionFactor = (slopeAngle - _MinAngle) / (_MaxAngle - _MinAngle);
					float slopeTransition = tex2D(_SlopeTransition, float2(slopeTransitionFactor, 0.5f)).r;
					float slopeMask = (slopeAngle >= _MinAngle) * (slopeAngle <= _MaxAngle) * slopeTransition;
					#if BLEND_MAX
						mask = max(mask, slopeMask);
					#else
						mask = mask * slopeMask;
					#endif
				#endif

				#if DIRECTION_MASK
					float2 dir = normalize(normal.xz);
					float rad = atan2(dir.y, dir.x);
					float deg = (rad >= 0) * (rad * 57.2958) + (rad < 0) * (359 + rad * 57.2958);
					float minAngle = (_DirectionAngle - _DirectionTolerance * 0.5);
					float maxAngle = (_DirectionAngle + _DirectionTolerance * 0.5);

					float deg0 = (deg + 180) % 360;
					float minAngle0 = (minAngle + 180) % 360;
					float maxAngle0 = (maxAngle + 180) % 360;
					float v0 = deg0 > minAngle0 && deg0 <= maxAngle0;
					float f0 = (1 - abs(inverseLerp(deg0, minAngle0, maxAngle0) * 2 - 1)) * v0;

					float deg1 = (deg + 360) % 360;
					float minAngle1 = (minAngle + 360) % 360;
					float maxAngle1 = (maxAngle + 360) % 360;
					float v1 = deg1 > minAngle1 && deg1 <= maxAngle1;
					float f1 = (1 - abs(inverseLerp(deg1, minAngle1, maxAngle1) * 2 - 1)) * v1;

					float f = max(f0, f1);
					float directionMask = tex2D(_DirectionFalloff, f.xx).r * ((dir.x + dir.y) != 0);
					#if BLEND_MAX
						mask = max(mask, directionMask);
					#else
						mask = mask * directionMask;
					#endif
				#endif

				return mask;
			}
			ENDCG

		}
	}
}
