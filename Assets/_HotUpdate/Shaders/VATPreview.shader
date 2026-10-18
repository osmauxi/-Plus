Shader "Custom/VATPreview"
{
    Properties
    {
        _MainTex("Main Texture", 2D) = "white" {}
        _BaseColor("Base Color", Color) = (1,1,1,1)
        _HitFlashColor("Hit Flash Color", Color) = (1,1,1,1)
        _HitFlashAmount("Hit Flash Amount", Range(0,1)) = 0
        _PositionTexture("Position Texture", 2D) = "white" {}
        _NormalTexture("Normal Texture", 2D) = "white" {}
        _CurrentFrame("Current Frame", Float) = 0
        _NextFrame("Next Frame", Float) = 0
        _Blend("Animation Blend", Range(0,1)) = 0
        _VertexCount("Vertex Count", Float) = 1
        _FrameCount("Frame Count", Float) = 1
        _Metallic("Metallic", Range(0,1)) = 0
        _Smoothness("Smoothness", Range(0,1)) = 0.25
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Opaque"
            "Queue" = "Geometry"
        }

        HLSLINCLUDE

        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);
        TEXTURE2D(_PositionTexture);
        SAMPLER(sampler_PositionTexture);
        TEXTURE2D(_NormalTexture);
        SAMPLER(sampler_NormalTexture);

        CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            half _Metallic;
            half _Smoothness;
            float _VertexCount;
            float _FrameCount;
        CBUFFER_END

        UNITY_INSTANCING_BUFFER_START(VATProps)
            UNITY_DEFINE_INSTANCED_PROP(half4, _BaseColor)
            UNITY_DEFINE_INSTANCED_PROP(half4, _HitFlashColor)
            UNITY_DEFINE_INSTANCED_PROP(half, _HitFlashAmount)
            UNITY_DEFINE_INSTANCED_PROP(float, _CurrentFrame)
            UNITY_DEFINE_INSTANCED_PROP(float, _NextFrame)
            UNITY_DEFINE_INSTANCED_PROP(float, _Blend)
        UNITY_INSTANCING_BUFFER_END(VATProps)

        float3 SampleVATPosition(float vertexID, float frame)
        {
            float2 uv;
            uv.x = (vertexID + 0.5) / max(_VertexCount, 1.0);
            uv.y = (frame + 0.5) / max(_FrameCount, 1.0);
            return SAMPLE_TEXTURE2D_LOD(_PositionTexture, sampler_PositionTexture, uv, 0).xyz;
        }

        float3 SampleVATNormal(float vertexID, float frame)
        {
            float2 uv;
            uv.x = (vertexID + 0.5) / max(_VertexCount, 1.0);
            uv.y = (frame + 0.5) / max(_FrameCount, 1.0);
            return SAMPLE_TEXTURE2D_LOD(_NormalTexture, sampler_NormalTexture, uv, 0).xyz;
        }

        float3 GetVATPosition(float vertexID)
        {
            float currentFrame = UNITY_ACCESS_INSTANCED_PROP(VATProps, _CurrentFrame);
            float nextFrame = UNITY_ACCESS_INSTANCED_PROP(VATProps, _NextFrame);
            float blend = saturate(UNITY_ACCESS_INSTANCED_PROP(VATProps, _Blend));
            return lerp(SampleVATPosition(vertexID, currentFrame), SampleVATPosition(vertexID, nextFrame), blend);
        }

        float3 GetVATNormal(float vertexID)
        {
            float currentFrame = UNITY_ACCESS_INSTANCED_PROP(VATProps, _CurrentFrame);
            float nextFrame = UNITY_ACCESS_INSTANCED_PROP(VATProps, _NextFrame);
            float blend = saturate(UNITY_ACCESS_INSTANCED_PROP(VATProps, _Blend));
            return normalize(lerp(SampleVATNormal(vertexID, currentFrame), SampleVATNormal(vertexID, nextFrame), blend));
        }

        ENDHLSL

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM

            #pragma target 3.5
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _FORWARD_PLUS
            #pragma multi_compile_fog
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                float2 vatData : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                half3 normalWS : TEXCOORD2;
                float4 shadowCoord : TEXCOORD3;
                half fogFactor : TEXCOORD4;
                half3 vertexLighting : TEXCOORD5;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings Vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                float3 positionOS = GetVATPosition(input.vatData.x);
                float3 normalOS = GetVATNormal(input.vatData.x);
                VertexPositionInputs positionInputs = GetVertexPositionInputs(positionOS);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(normalOS);
                output.positionCS = positionInputs.positionCS;
                output.positionWS = positionInputs.positionWS;
                output.normalWS = normalInputs.normalWS;
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                #if defined(_MAIN_LIGHT_SHADOWS_SCREEN)
                    output.shadowCoord = ComputeScreenPos(positionInputs.positionCS);
                #else
                    output.shadowCoord = TransformWorldToShadowCoord(positionInputs.positionWS);
                #endif
                output.fogFactor = ComputeFogFactor(positionInputs.positionCS.z);
                output.vertexLighting = VertexLighting(positionInputs.positionWS, normalInputs.normalWS);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                half4 baseColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv) * UNITY_ACCESS_INSTANCED_PROP(VATProps, _BaseColor);
                SurfaceData surfaceData = (SurfaceData)0;
                surfaceData.albedo = baseColor.rgb;
                surfaceData.alpha = baseColor.a;
                surfaceData.metallic = _Metallic;
                surfaceData.specular = half3(0.04, 0.04, 0.04);
                surfaceData.smoothness = _Smoothness;
                surfaceData.normalTS = half3(0, 0, 1);
                surfaceData.occlusion = 1;
                surfaceData.emission = 0;
                surfaceData.clearCoatMask = 0;
                surfaceData.clearCoatSmoothness = 0;
                InputData inputData = (InputData)0;
                inputData.positionWS = input.positionWS;
                inputData.positionCS = input.positionCS;
                inputData.normalWS = NormalizeNormalPerPixel(input.normalWS);
                inputData.viewDirectionWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
                inputData.shadowCoord = input.shadowCoord;
                inputData.fogCoord = input.fogFactor;
                inputData.vertexLighting = input.vertexLighting;
                inputData.bakedGI = SampleSH(inputData.normalWS);
                inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
                inputData.shadowMask = half4(1, 1, 1, 1);
                half4 color = UniversalFragmentPBR(inputData, surfaceData);
                color.rgb = MixFog(color.rgb, inputData.fogCoord);
                half hitFlashAmount = saturate(UNITY_ACCESS_INSTANCED_PROP(VATProps, _HitFlashAmount));
                color.rgb = lerp(color.rgb, UNITY_ACCESS_INSTANCED_PROP(VATProps, _HitFlashColor).rgb, hitFlashAmount);
                return color;
            }

            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            ZWrite On
            ZTest LEqual
            ColorMask 0

            HLSLPROGRAM

            #pragma target 3.5
            #pragma vertex ShadowVert
            #pragma fragment ShadowFrag
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            float3 _LightDirection;
            float3 _LightPosition;

            struct ShadowAttributes
            {
                float4 positionOS : POSITION;
                float2 vatData : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct ShadowVaryings
            {
                float4 positionCS : SV_POSITION;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            ShadowVaryings ShadowVert(ShadowAttributes input)
            {
                ShadowVaryings output = (ShadowVaryings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                float3 positionOS = GetVATPosition(input.vatData.x);
                float3 normalOS = GetVATNormal(input.vatData.x);
                float3 positionWS = TransformObjectToWorld(positionOS);
                float3 normalWS = TransformObjectToWorldNormal(normalOS);
                #if defined(_CASTING_PUNCTUAL_LIGHT_SHADOW)
                    float3 lightDirectionWS = normalize(_LightPosition - positionWS);
                #else
                    float3 lightDirectionWS = _LightDirection;
                #endif
                positionWS = ApplyShadowBias(positionWS, normalWS, lightDirectionWS);
                output.positionCS = TransformWorldToHClip(positionWS);
                #if UNITY_REVERSED_Z
                    output.positionCS.z = min(output.positionCS.z, output.positionCS.w * UNITY_NEAR_CLIP_VALUE);
                #else
                    output.positionCS.z = max(output.positionCS.z, output.positionCS.w * UNITY_NEAR_CLIP_VALUE);
                #endif
                return output;
            }

            half4 ShadowFrag(ShadowVaryings input) : SV_Target
            {
                return 0;
            }

            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            ZWrite On
            ColorMask 0

            HLSLPROGRAM

            #pragma target 3.5
            #pragma vertex DepthVert
            #pragma fragment DepthFrag
            #pragma multi_compile_instancing

            struct DepthAttributes
            {
                float4 positionOS : POSITION;
                float2 vatData : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct DepthVaryings
            {
                float4 positionCS : SV_POSITION;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            DepthVaryings DepthVert(DepthAttributes input)
            {
                DepthVaryings output = (DepthVaryings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                output.positionCS = TransformObjectToHClip(GetVATPosition(input.vatData.x));
                return output;
            }

            half4 DepthFrag(DepthVaryings input) : SV_Target
            {
                return 0;
            }

            ENDHLSL
        }
    }

    FallBack Off
}
