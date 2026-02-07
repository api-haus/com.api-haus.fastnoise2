Shader "Hidden/FN2/TerrainRaymarch"
{
    Properties
    {
        _HeightMap ("Heightmap", 2D) = "black" {}
        _HeightScale ("Height Scale", Float) = 0.15
        _CamYaw ("Camera Yaw", Float) = 0.0
        _CamPitch ("Camera Pitch", Float) = 0.7
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
            float _CamYaw;
            float _CamPitch;

            static const float3 BG_COLOR = float3(0.0863, 0.0863, 0.0863); // #161616
            static const float3 COLOR_LOW = float3(0.1098, 0.1098, 0.1098); // #1C1C1C
            static const float3 COLOR_HIGH = float3(0.1804, 0.1804, 0.1804); // #2E2E2E

            static const float3 LIGHT_DIR = normalize(float3(0.4, 0.8, -0.3));

            static const int MAX_STEPS = 192;
            static const int BINARY_STEPS = 8;
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
                // Zero out samples outside [0,1] to avoid clamping artifacts at edges
                float2 inside = step(0, xz) * step(xz, 1);
                return tex2Dlod(_HeightMap, float4(saturate(xz), 0, 0)).r * _HeightScale * inside.x * inside.y;
            }

            float3 calcNormal(float2 xz)
            {
                float hL = sampleHeight(xz - float2(EPS, 0));
                float hR = sampleHeight(xz + float2(EPS, 0));
                float hD = sampleHeight(xz - float2(0, EPS));
                float hU = sampleHeight(xz + float2(0, EPS));
                return normalize(float3(hL - hR, 2.0 * EPS, hD - hU));
            }

            float3 getCameraRay(float2 uv, float3 camDir)
            {
                float3 forward = -camDir;
                float3 right = normalize(cross(float3(0, 1, 0), forward));
                float3 up = cross(forward, right);
                return normalize(forward + (uv.x - 0.5) * right * 1.2 + (uv.y - 0.5) * up * 1.2);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float cp = cos(_CamPitch);
                float3 camDir = float3(cp * sin(_CamYaw), sin(_CamPitch), -cp * cos(_CamYaw));

                // Auto-frame: sample heightmap grid to find actual height range
                float s0 = tex2Dlod(_HeightMap, float4(0.50, 0.50, 0, 0)).r;
                float s1 = tex2Dlod(_HeightMap, float4(0.25, 0.25, 0, 0)).r;
                float s2 = tex2Dlod(_HeightMap, float4(0.75, 0.25, 0, 0)).r;
                float s3 = tex2Dlod(_HeightMap, float4(0.25, 0.75, 0, 0)).r;
                float s4 = tex2Dlod(_HeightMap, float4(0.75, 0.75, 0, 0)).r;
                float s5 = tex2Dlod(_HeightMap, float4(0.50, 0.25, 0, 0)).r;
                float s6 = tex2Dlod(_HeightMap, float4(0.50, 0.75, 0, 0)).r;
                float s7 = tex2Dlod(_HeightMap, float4(0.25, 0.50, 0, 0)).r;
                float s8 = tex2Dlod(_HeightMap, float4(0.75, 0.50, 0, 0)).r;
                float hMin = min(min(min(min(s0,s1),min(s2,s3)),min(min(s4,s5),min(s6,s7))),s8) * _HeightScale;
                float hMax = max(max(max(max(s0,s1),max(s2,s3)),max(max(s4,s5),max(s6,s7))),s8) * _HeightScale;
                float midH = (hMin + hMax) * 0.5;
                float halfH = max((hMax - hMin) * 0.5, 0.01);

                float3 camTarget = float3(0.5, midH, 0.5);
                float radius = length(float3(0.5, halfH, 0.5));
                float camDist = radius / 0.55;
                float3 ro = camTarget + camDir * camDist;
                float3 rd = getCameraRay(i.uv, camDir);

                // Ray-AABB intersection to find valid march range
                float3 bmin = float3(-0.1, -0.05, -0.1);
                float3 bmax = float3(1.1, _HeightScale + 0.1, 1.1);
                float3 invRd = 1.0 / rd;
                float3 tA = (bmin - ro) * invRd;
                float3 tB = (bmax - ro) * invRd;
                float3 tNear = min(tA, tB);
                float3 tFar = max(tA, tB);
                float tEnter = max(max(tNear.x, tNear.y), max(tNear.z, 0.0));
                float tExit = min(min(tFar.x, tFar.y), tFar.z);

                if (tEnter >= tExit)
                    return fixed4(BG_COLOR, 1);

                float t = tEnter;
                bool hit = false;

                // Scale step conservatism with height: steeper terrain needs smaller steps
                float adaptFactor = min(0.5, 0.075 / max(_HeightScale, 0.01));

                // Raymarch
                for (int s = 0; s < MAX_STEPS; s++)
                {
                    float3 p = ro + rd * t;

                    if (t > tExit)
                        break;

                    float h = sampleHeight(p.xz);
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
                            float hM = sampleHeight(pM.xz);
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
                    t += max(diff * adaptFactor, 0.001);
                }

                if (!hit)
                    return fixed4(BG_COLOR, 1);

                float3 p = ro + rd * t;
                float h = sampleHeight(p.xz);

                // Normal and lighting
                float3 n = calcNormal(p.xz);
                float ndotl = max(dot(n, LIGHT_DIR), 0.0);
                float lighting = 0.4 + 0.6 * ndotl;

                // Height-based color
                float heightNorm = saturate(h / _HeightScale);
                float3 terrainColor = lerp(COLOR_LOW, COLOR_HIGH, heightNorm);

                // Soft AO from height
                float ao = lerp(0.7, 1.0, heightNorm);

                float3 color = terrainColor * lighting * ao;

                // Distance fog (scaled with camera distance)
                float fogFactor = smoothstep(0.5 * camDist, 2.5 * camDist, t);
                color = lerp(color, BG_COLOR, fogFactor);

                return fixed4(color, 1);
            }
            ENDCG
        }
    }
}
