Shader "Hidden/FN2/TerrainRaymarch"
{
    Properties
    {
        _HeightMap ("Heightmap", 2D) = "black" {}
        _HeightScale ("Height Scale", Float) = 0.15
        _CamDist ("Camera Distance", Float) = 1.0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        ZWrite Off ZTest Always Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _HeightMap;
            float _HeightScale;
            float _CamDist;

            static const float3 BG_COLOR = float3(0.0863, 0.0863, 0.0863); // #161616
            static const float3 COLOR_LOW = float3(0.1098, 0.1098, 0.1098); // #1C1C1C
            static const float3 COLOR_HIGH = float3(0.1804, 0.1804, 0.1804); // #2E2E2E

            static const float3 CAM_TARGET = float3(0.5, 0.0, 0.5);
            static const float3 CAM_DIR = normalize(float3(0.5, 0.6, -0.2) - float3(0.5, 0.0, 0.5));
            static const float3 LIGHT_DIR = normalize(float3(0.4, 0.8, -0.3));

            static const int MAX_STEPS = 128;
            static const int BINARY_STEPS = 6;
            static const float EPS = 1.0 / 512.0;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float sampleHeight(float2 xz)
            {
                return tex2Dlod(_HeightMap, float4(xz, 0, 0)).r * _HeightScale;
            }

            float3 calcNormal(float2 xz)
            {
                float hL = sampleHeight(xz - float2(EPS, 0));
                float hR = sampleHeight(xz + float2(EPS, 0));
                float hD = sampleHeight(xz - float2(0, EPS));
                float hU = sampleHeight(xz + float2(0, EPS));
                return normalize(float3(hL - hR, 2.0 * EPS, hD - hU));
            }

            float3 getCameraRay(float2 uv)
            {
                float3 forward = -CAM_DIR;
                float3 right = normalize(cross(float3(0, 1, 0), forward));
                float3 up = cross(forward, right);
                return normalize(forward + (uv.x - 0.5) * right * 1.2 + (uv.y - 0.5) * up * 1.2);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 ro = CAM_TARGET + CAM_DIR * _CamDist;
                float3 rd = getCameraRay(i.uv);

                float t = 0.0;
                bool hit = false;

                // Raymarch
                for (int s = 0; s < MAX_STEPS; s++)
                {
                    float3 p = ro + rd * t;

                    // Out of bounds check
                    if (p.x < -0.1 || p.x > 1.1 || p.z < -0.1 || p.z > 1.1 || t > 3.0 * _CamDist)
                        break;

                    float2 xz = saturate(p.xz);
                    float h = sampleHeight(xz);
                    float diff = p.y - h;

                    if (diff < 0.0005)
                    {
                        // Binary refinement
                        float tA = t - (t > 0 ? 0.01 : 0);
                        float tB = t;
                        for (int b = 0; b < BINARY_STEPS; b++)
                        {
                            float tM = (tA + tB) * 0.5;
                            float3 pM = ro + rd * tM;
                            float hM = sampleHeight(saturate(pM.xz));
                            if (pM.y < hM)
                                tB = tM;
                            else
                                tA = tM;
                        }
                        t = (tA + tB) * 0.5;
                        hit = true;
                        break;
                    }

                    // Adaptive step size — larger steps when far from surface
                    t += max(diff * 0.5, 0.002);
                }

                if (!hit)
                    return fixed4(BG_COLOR, 1);

                float3 p = ro + rd * t;
                float2 xz = saturate(p.xz);
                float h = sampleHeight(xz);

                // Normal and lighting
                float3 n = calcNormal(xz);
                float ndotl = max(dot(n, LIGHT_DIR), 0.0);
                float lighting = 0.4 + 0.6 * ndotl;

                // Height-based color
                float heightNorm = saturate(h / _HeightScale);
                float3 terrainColor = lerp(COLOR_LOW, COLOR_HIGH, heightNorm);

                // Soft AO from height
                float ao = lerp(0.7, 1.0, heightNorm);

                float3 color = terrainColor * lighting * ao;

                // Distance fog (scaled with camera distance)
                float fogFactor = smoothstep(0.5 * _CamDist, 2.5 * _CamDist, t);
                color = lerp(color, BG_COLOR, fogFactor);

                return fixed4(color, 1);
            }
            ENDCG
        }
    }
}
