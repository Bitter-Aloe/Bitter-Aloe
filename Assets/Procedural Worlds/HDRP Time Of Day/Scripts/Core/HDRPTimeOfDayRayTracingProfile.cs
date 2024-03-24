using System;
using UnityEngine;
#if HDRPTIMEOFDAY && HDPipeline
using UnityEngine.Rendering.HighDefinition;
#endif
using UnityEngine.Rendering;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace ProceduralWorlds.HDRPTOD
{
    /// <summary>
    /// Enum for the mode types for RT lighting models
    /// </summary>
    public enum RayTracingLightingModel { Default, RayTraced, RayTracedAdvanced, PathTraced }

    /// <summary>
    /// RTX optimize data class that holds the settings
    /// This has the function to process the mesh renderer
    /// </summary>
    [Serializable]
    public class RayTracingOptimizationData
    {
        public bool m_ignoreIfAnimatorFound = true;
        public bool m_setStaticShadowCasting = true;
        public bool m_setObjectLayer = false;
        public bool m_overrideStatic = true;
        public LayerMask m_rayTracedLayer = -1;
        public string m_nonRayTracedLayer = "RTX Ignore";
        public float m_nonRayTracedObjectScale = 1f;
        public UnityEngine.Experimental.Rendering.RayTracingMode m_staticRayTracingMode = UnityEngine.Experimental.Rendering.RayTracingMode.Static;
        public UnityEngine.Experimental.Rendering.RayTracingMode m_dynamicRayTracingMode = UnityEngine.Experimental.Rendering.RayTracingMode.DynamicTransform;
        public LightProbeUsage m_lightProbes = LightProbeUsage.BlendProbes;
        public MotionVectorGenerationMode m_motionVectors = MotionVectorGenerationMode.Camera;
        public bool m_dynamicOcclusion = true;

        public RayTracingOptimizationData(bool defaults)
        {
            if (defaults)
            {
                m_ignoreIfAnimatorFound = true;
                m_setStaticShadowCasting = true;
                m_setObjectLayer = true;
                m_overrideStatic = true;
                m_rayTracedLayer = -1;
                m_nonRayTracedLayer = "RTX Ignore";
                m_nonRayTracedObjectScale = 1.5f;
                m_staticRayTracingMode = UnityEngine.Experimental.Rendering.RayTracingMode.Static;
                m_dynamicRayTracingMode = UnityEngine.Experimental.Rendering.RayTracingMode.DynamicTransform;
                m_lightProbes = LightProbeUsage.BlendProbes;
                m_motionVectors = MotionVectorGenerationMode.Camera;
                m_dynamicOcclusion = true;
            }
        }

        public void ProcessMesh(MeshRenderer renderer)
        {
            if (renderer != null)
            {
                //Size check
                Vector3 size = renderer.bounds.size;
                float largestSizeValue = size.x;
                if (size.y > largestSizeValue)
                {
                    largestSizeValue = size.y;
                }
                else if (size.z > largestSizeValue)
                {
                    largestSizeValue = size.z;
                }

                renderer.lightProbeUsage = m_lightProbes;
                renderer.allowOcclusionWhenDynamic = m_dynamicOcclusion;

                bool isStatic = renderer.gameObject.isStatic;
                renderer.rayTracingMode = isStatic ? m_staticRayTracingMode : m_dynamicRayTracingMode;
                if (m_overrideStatic || isStatic)
                {
                    renderer.rayTracingMode = m_staticRayTracingMode;
                    renderer.motionVectorGenerationMode = MotionVectorGenerationMode.Camera;
                }
                else
                {
                    renderer.motionVectorGenerationMode = m_motionVectors;
                }

                bool isTooSmall = largestSizeValue < m_nonRayTracedObjectScale;
                if (m_setObjectLayer)
                {
                    if (isTooSmall)
                    {
                        renderer.gameObject.layer = LayerMask.NameToLayer(m_nonRayTracedLayer);
                    }
                }
                if (isTooSmall)
                {
                    renderer.rayTracingMode = UnityEngine.Experimental.Rendering.RayTracingMode.Off;
                }
                if (m_setStaticShadowCasting)
                {
                    renderer.staticShadowCaster = isStatic;
                }
            }
        }
    }
    /// <summary>
    /// Ray traced options for the time of day light component
    /// This allow different quality and casting of shadows for different GlobalRayTracingMode modes
    /// </summary>
    [Serializable]
    public class RayTracedLightOptions
    {
        public string OptionName;
        public RayTracingLightingModel LightingModel = RayTracingLightingModel.Default;
        public bool CastShadows = true;
        public LightShadowResolutionQuality LightShadowResolutionQuality = LightShadowResolutionQuality.High;
        public int CustomShadowResolution = 4096;
        public bool RayTracedShadows = false;
        public bool RayTracedContactShadows = false;
        [Range(1, 32)]
        public int RayTracedShadowsSampleCount = 8;

        [NonSerialized] private bool m_cached;
        [NonSerialized] private bool m_cachedCastShadows;
        [NonSerialized] private LightShadowResolutionQuality m_cachedLightShadowResolutionQuality;
        [NonSerialized] private int m_cachedCustomShadowResolution;
        [NonSerialized] private bool m_cachedRayTracedShadows;
        [NonSerialized] private bool m_cachedRayTracedContactShadows;

        /// <summary>
        /// Applies the RT settings
        /// </summary>
        /// <param name="data"></param>
        /// <param name="lightingModel"></param>
#if HDRPTIMEOFDAY && HDPipeline
        public void Apply(HDAdditionalLightData data, RayTracingLightingModel lightingModel)
        {
            BuildCache(data);
            if (lightingModel == LightingModel)
            {
                data.EnableShadows(CastShadows);
                data.SetShadowResolutionLevel((int)LightShadowResolutionQuality);
                if (LightShadowResolutionQuality == LightShadowResolutionQuality.Custom)
                {
                    data.SetShadowResolution(CustomShadowResolution);
                }

                data.useRayTracedShadows = RayTracedShadows;
                data.rayTraceContactShadow = RayTracedContactShadows;
                data.numRayTracingSamples = RayTracedShadowsSampleCount;
            }
        }
#endif
        /// <summary>
        /// Applies the RT settings
        /// </summary>
        /// <param name="data"></param>
        /// <param name="mode"></param>
#if HDRPTIMEOFDAY && HDPipeline
        public void Apply(HDAdditionalLightData data)
        {
            if (data == null)
            {
                return;
            }

            BuildCache(data);
            data.EnableShadows(CastShadows);
            if (LightShadowResolutionQuality == LightShadowResolutionQuality.Custom)
            {
                data.shadowResolution.useOverride = true;
            }
            else
            {
                data.SetShadowResolutionLevel((int)LightShadowResolutionQuality);
                data.shadowResolution.useOverride = false;
            }

            data.SetShadowResolution(CustomShadowResolution);
            data.useRayTracedShadows = RayTracedShadows;
            data.rayTraceContactShadow = RayTracedContactShadows;
            data.numRayTracingSamples = RayTracedShadowsSampleCount;
        }
#endif
        /// <summary>
        /// Reverts to the cached settings
        /// </summary>
        /// <param name="data"></param>
#if HDRPTIMEOFDAY && HDPipeline
        public void Revert(HDAdditionalLightData data)
        {
            if (data != null && m_cached)
            {
                data.EnableShadows(m_cachedCastShadows);
                data.SetShadowResolutionLevel((int)m_cachedLightShadowResolutionQuality);
                if (m_cachedLightShadowResolutionQuality == LightShadowResolutionQuality.Custom)
                {
                    data.shadowResolution.useOverride = true;
                    data.SetShadowResolution(m_cachedCustomShadowResolution);
                }
                else
                {
                    data.shadowResolution.useOverride = false;
                }
                data.useRayTracedShadows = m_cachedRayTracedShadows;
                data.rayTraceContactShadow = m_cachedRayTracedContactShadows;
            }
        }
#endif
        /// <summary>
        /// Builds the cached data
        /// </summary>
        /// <param name="data"></param>
#if HDRPTIMEOFDAY && HDPipeline
        private void BuildCache(HDAdditionalLightData data)
        {
            if (!m_cached)
            {
                m_cachedCastShadows = data.GetComponent<Light>().shadows == LightShadows.Soft;
                m_cachedLightShadowResolutionQuality = (LightShadowResolutionQuality)data.shadowResolution.level;
                m_cachedCustomShadowResolution = data.shadowResolution.@override;
                m_cachedRayTracedShadows = data.useRayTracedShadows;
                m_cachedRayTracedContactShadows = data.rayTraceContactShadow;
                m_cached = true;
            }
        }
#endif
    }
    /// <summary>
    /// Ray tracing options for the advanced ray tracing model mode this is mainly used for ultra high ray tracing quality.
    /// </summary>
    [Serializable]
    public class GlobalAdvancedRayTracingOptions
    {
        [Header("Sun/Moon Light Settings")]
        public bool OverrideSunMoonLight = false;
        public RayTracedLightOptions SunMoonLightQuality = new RayTracedLightOptions();
        [Header("Additional Lights Settings")]
        public bool OverrideAdditionalLights = false;
        public RayTracedLightOptions AdditionalLightsQuality = new RayTracedLightOptions();
        [Header("Global Illumination")]
        public bool OverrideGlobalIllumination = false;
        public RTScreenSpaceGlobalIlluminationQuality RTSSGIQuality = new RTScreenSpaceGlobalIlluminationQuality();
        [Header("Reflections")]
        public bool OverrideReflections = false;
        public RTScreenSpaceReflectionQuality RTSSRQuality = new RTScreenSpaceReflectionQuality();
        [Header("Ambient Occlusion")]
        public bool OverrideAmbientOcclusion = false;
        public RTAmbientOcclusionQuality RTAOQuality = new RTAmbientOcclusionQuality();
        [Header("Recursive Rendering")]
        public bool OverrideRecursiveRendering = false;
        public RTRecursiveRenderingQuality RTRecursiveRenderingQuality = new RTRecursiveRenderingQuality();
        [Header("Sub Surface Scattering")]
        public bool OverrideSubSurfaceScattering = false;
        public RTSubSurfaceScatteringQuality RTSSSQuality = new RTSubSurfaceScatteringQuality();
        [Header("Path Tracing")]
        public bool AllowPathTracing = true;
        public RTPathTracingQuality PathTracingQuality = new RTPathTracingQuality();

        /// <summary>
        /// Applies the ray tracing options
        /// </summary>
        /// <param name="components"></param>
        /// <param name="additionalLights"></param>
#if HDRPTIMEOFDAY && HDPipeline
        public void ApplyAdvancedRayTracingOptions(HDRPTimeOfDayComponents components, HDAdditionalLightData[] additionalLights, HDRPTimeOfDay timeOfDay)
        {
#if RAY_TRACING_ENABLED
            if (timeOfDay.RayTracingLightingModel == RayTracingLightingModel.Default || timeOfDay.RayTracingLightingModel == RayTracingLightingModel.RayTraced)
            {
                timeOfDay.RefreshTimeOfDay();
                //Apply additional lights
                if (OverrideAdditionalLights)
                {
                    //Loop through them all
                    for (int i = 0; i < additionalLights.Length; i++)
                    {
                        //Apply to the light
                        AdditionalLightsQuality.Revert(additionalLights[i]);
                    }
                }

                //Ray tracing settings
                if (timeOfDay.RayTracingLightingModel == RayTracingLightingModel.RayTraced)
                {
                    components.m_rayTracingSettings.extendCameraCulling.value = true;
                    components.m_rayTracingSettings.extendShadowCulling.value = true;
                }
                else
                {
                    components.m_rayTracingSettings.extendCameraCulling.value = false;
                    components.m_rayTracingSettings.extendShadowCulling.value = false;
                }
            }
            else if (timeOfDay.RayTracingLightingModel == RayTracingLightingModel.RayTracedAdvanced)
            {
                //Apply Sun/moon light
                if (OverrideSunMoonLight)
                {
                    SunMoonLightQuality.Apply(components.m_sunLightData);
                    SunMoonLightQuality.Apply(components.m_moonLightData);
                }
                //Apply additional lights
                if (OverrideAdditionalLights)
                {
                    //Loop through them all
                    for (int i = 0; i < additionalLights.Length; i++)
                    {
                        //Apply to the light
                        AdditionalLightsQuality.Apply(additionalLights[i]);
                    }
                }

                //Apply SSGI
                RTSSGIQuality.Apply(components.m_rayTracedGlobalIllumination, true);
                //Apply SSR
                RTSSRQuality.Apply(components.m_rayTracedScreenSpaceReflection, true);
                //Apply AO
                RTAOQuality.Apply(components.m_rayTracedAmbientOcclusion);
                //Apply RR
                RTRecursiveRenderingQuality.Apply(components.m_rayTracedRecursiveRendering, OverrideRecursiveRendering);
                //Apply SSS
                RTSSSQuality.Apply(components.m_rayTracedSubSurfaceScattering);

                //Ray tracing settings
                components.m_rayTracingSettings.extendCameraCulling.value = true;
                components.m_rayTracingSettings.extendShadowCulling.value = true;
            }
#endif
        }
#endif
        /// <summary>
        /// Applies the path tracing settings
        /// </summary>
        /// <param name="components"></param>
#if HDPipeline
        public void ApplyPathTracing(HDRPTimeOfDayComponents components, HDRPTimeOfDay timeOfDay)
        {
#if RAY_TRACING_ENABLED
            if (components.m_pathTracing != null)
            {
                components.m_pathTracing.active = AllowPathTracing;
                components.m_rayTracingExposure.active = AllowPathTracing;
                PathTracingQuality.Apply(components, timeOfDay.IsDayTime());
                components.m_cameraData.allowDynamicResolution = false;
            }
#endif
        }
#endif
    }
    /// <summary>
    /// Ray traced Globall Illumination quality
    /// </summary>
    [Serializable]
    public class RTScreenSpaceGlobalIlluminationQuality
    {
        public string m_qualityName;
#if HDRPTIMEOFDAY && HDPipeline
        public RayCastingMode m_tracingMode = RayCastingMode.Mixed;
#endif
        [Range(0, 7)]
        public int m_textureLODBias = 7;
#if HDRPTIMEOFDAY && HDPipeline
        public RayMarchingFallbackHierarchy m_rayMiss = RayMarchingFallbackHierarchy.ReflectionProbesAndSky;
        public RayMarchingFallbackHierarchy m_lastBounce = RayMarchingFallbackHierarchy.ReflectionProbesAndSky;
#endif
        [Range(0f, 1f)]
        public float m_probeDimmer = 1f;
        public float m_maxRayLength = 50f;
        [Range(0.001f, 10f)]
        public float m_clampValue = 10f;
        public bool m_fullResolution = false;
        public int m_maxRaySteps = 48;
        public bool m_denoise = true;
        public bool m_halfResDenoise = false;
        [Range(0.001f, 1f)]
        public float m_denoiseRadius = 0.66f;
        public bool m_secondDenoiserPass = true;
        public bool m_motionRejection = true;
#if HDRPTIMEOFDAY && HDPipeline
        public RayTracingMode m_rayTracingMode = RayTracingMode.Performance;
#endif
        public int m_sampleCount = 1;
        public int m_bounceCount = 1;

        /// <summary>
        /// Applies the settings
        /// </summary>
        /// <param name="gi"></param>
#if HDRPTIMEOFDAY && HDPipeline
        public void Apply(GlobalIllumination gi, bool isRayTracing = false)
        {
            if (gi != null)
            {
                gi.quality.value = 3;
                gi.tracing.value = isRayTracing ? m_tracingMode : RayCastingMode.RayMarching;
                gi.textureLodBias.value = m_textureLODBias;
                gi.rayMiss.value = m_rayMiss;
                gi.lastBounceFallbackHierarchy.value = m_lastBounce;
#if UNITY_2022_3_OR_NEWER
                gi.ambientProbeDimmer.value = m_probeDimmer;
#endif
                gi.rayLength = m_maxRayLength;
                gi.clampValue = m_clampValue;
                gi.fullResolution = m_fullResolution;
                gi.maxRaySteps = m_maxRaySteps;
                gi.maxMixedRaySteps = m_maxRaySteps;
                gi.denoise = m_denoise;
                gi.halfResolutionDenoiser = m_halfResDenoise;
                gi.halfResolutionDenoiserSS = m_secondDenoiserPass;
                gi.denoiserRadius = m_denoiseRadius;
                gi.denoiserRadiusSS = m_denoiseRadius;
                gi.denoiseSS = m_secondDenoiserPass;
                gi.receiverMotionRejection.value = m_motionRejection;
                gi.mode.value = m_rayTracingMode;
                gi.sampleCount.value = m_sampleCount;
                gi.bounceCount.value = m_bounceCount;
            }
        }
#endif
    }
    /// <summary>
    /// Ray traced Additional Lights quality
    /// </summary>
    [Serializable]
    public class RTAdditionalLightsQuality
    {
        public bool CastLightShadows = true;
        public LightShadowResolutionQuality LightShadowQuality = LightShadowResolutionQuality.High;
        public bool RayTraceShadows = true;
        [Range(1, 32)]
        public int RayTraceShadowsSampleCount = 4;

        /// <summary>
        /// Applies the settings in this quality level to the provided light data
        /// </summary>
        /// <param name="lightData"></param>
#if HDRPTIMEOFDAY && HDPipeline
        public void Apply(HDAdditionalLightData lightData)
        {
            if (lightData != null)
            {
                lightData.EnableShadows(CastLightShadows);
                lightData.SetShadowResolutionLevel((int)LightShadowQuality);
                lightData.useRayTracedShadows = RayTraceShadows;
                lightData.numRayTracingSamples = RayTraceShadowsSampleCount;
            }
        }
#endif
    }
    /// <summary>
    /// Path Tracing quality
    /// </summary>
    [Serializable]
    public class RTPathTracingQuality
    {
        [Range(1, 16384)]
        public int MaxSamples = 8192;
        [Range(1, 32)]
        public int MinimumDepth = 1;
        [Range(1, 32)]
        public int MaximumDepth = 2;
        [Range(1f, 100f)]
        public float MaximumIntensity = 20f;
#if HDNOISE
        public SkyImportanceSamplingMode SkyImportanceSamplingMode = SkyImportanceSamplingMode.On;
        public HDDenoiserType Denoiser = HDDenoiserType.Optix;
#endif
        public bool UseAOV = true;
        public bool Temporal = true;

        [Header("Exposure")]
        [Range(-5f, 15f)]
        public float m_dayLimitMin = 2f;
        [Range(-5f, 15f)]
        public float m_dayLimitMax = 10f;
        [Range(-5f, 15f)]
        public float m_nightLimitMin = 2f;
        [Range(-5f, 15f)]
        public float m_nightLimitMax = 7f;

        /// <summary>
        /// Applies the path tracing settings
        /// </summary>
        /// <param name="components"></param>
#if HDPipeline
        public void Apply(HDRPTimeOfDayComponents components, bool isDay)
        {
#if RAY_TRACING_ENABLED
            if (components.m_pathTracing != null)
            {
                components.m_pathTracing.maximumSamples.value = MaxSamples;
                components.m_pathTracing.minimumDepth.value = MinimumDepth;
                components.m_pathTracing.maximumDepth.value = MaximumDepth;
                components.m_pathTracing.maximumIntensity.value = MaximumIntensity;
#if UNITY_2022_3_OR_NEWER
                components.m_pathTracing.skyImportanceSampling.value = SkyImportanceSamplingMode;
                components.m_pathTracing.denoising.value = Denoiser;
                components.m_pathTracing.useAOVs.value = UseAOV;
                components.m_pathTracing.temporal.value = Temporal;
#endif
            }

            if (components.m_rayTracingExposure != null)
            {
                components.m_rayTracingExposure.limitMin.value = isDay ? m_dayLimitMin : m_nightLimitMin;
                components.m_rayTracingExposure.limitMax.value = isDay ? m_dayLimitMax : m_nightLimitMax;
            }
#endif
            }
#endif
    }
    /// <summary>
    /// Ray traced Recursive Rendering quality
    /// </summary>
    [Serializable]
    public class RTRecursiveRenderingQuality
    {
        public string QualityName;
        [Range(1, 10)]
        public int MaxDepth = 4;
        [Range(0f, 150f)]
        public float MaxRayLength = 10f;
        [Range(0f, 1f)]
        public float MinSmoothness = 0.5f;
        [Range(0f, 1f)]
        public float AmbientDimmer = 1f;
#if HDRPTIMEOFDAY && HDPipeline
        public RayTracingFallbackHierachy RayMiss = RayTracingFallbackHierachy.ReflectionProbesAndSky;
        public RayTracingFallbackHierachy LastBounce = RayTracingFallbackHierachy.ReflectionProbesAndSky;
#endif

        /// <summary>
        /// Applies the recursive rendering settings
        /// </summary>
        /// <param name="recursiveRendering"></param>
        /// <param name="enabled"></param>
#if HDRPTIMEOFDAY && HDPipeline
        public void Apply(RecursiveRendering recursiveRendering, bool enabled)
        {
            if (recursiveRendering != null)
            {
                recursiveRendering.active = enabled;
                if (enabled)
                {
                    recursiveRendering.maxDepth.value = MaxDepth;
                    recursiveRendering.rayLength.value = MaxRayLength;
                    recursiveRendering.minSmoothness.value = MinSmoothness;
                    recursiveRendering.rayMiss.value = RayMiss;
                    recursiveRendering.lastBounce.value = LastBounce;
#if UNITY_2022_3_OR_NEWER
                    recursiveRendering.ambientProbeDimmer.value = AmbientDimmer;
#endif
                }
            }
        }
#endif
    }
    /// <summary>
    /// Ray traced Reflections quality
    /// </summary>
    [Serializable]
    public class RTScreenSpaceReflectionQuality
    {
        public string m_qualityName;
#if HDRPTIMEOFDAY && HDPipeline
        public RayCastingMode m_tracingMode = RayCastingMode.Mixed;
#endif
        [Range(0, 7)]
        public int m_textureLODBias = 7;
#if HDRPTIMEOFDAY && HDPipeline
        public RayTracingFallbackHierachy m_rayMiss = RayTracingFallbackHierachy.ReflectionProbesAndSky;
        public RayTracingFallbackHierachy m_lastBounce = RayTracingFallbackHierachy.ReflectionProbesAndSky;
#endif
        [Range(0f, 1f)]
        public float m_ambientDimmer = 1f;
        [Range(0f, 1f)]
        public float m_minimumSmoothness = 0.2f;
        [Range(0f, 1f)]
        public float m_smoothnessFadeStart = 0f;
        public float m_maxRayLength = 50f;
        public float m_clampValue = 10f;
        public bool m_fullResolution = false;
        public int m_maxRaySteps = 64;
        public bool m_denoise = true;
        public int m_denoiseRadius = 16;
        public bool m_affectsSmoothSurfaces = false;
#if HDRPTIMEOFDAY && HDPipeline
        public RayTracingMode m_rayTracingMode = RayTracingMode.Performance;
#endif
        public int m_sampleCount = 1;
        public int m_bounceCount = 1;

        /// <summary>
        /// Applies the settings
        /// </summary>
        /// <param name="gi"></param>
#if HDRPTIMEOFDAY && HDPipeline
        public void Apply(ScreenSpaceReflection ssr, bool isRayTracing = false, bool sceneCheck = false, List<string> scenes = null)
        {
            if (ssr != null)
            {
                ssr.quality.value = 3;
                if (sceneCheck)
                {
                    RayCastingMode traceMode = RayCastingMode.RayMarching;
                    if (scenes != null)
                    {
                        string currentScenes = SceneManager.GetActiveScene().name;
                        foreach (string scene in scenes)
                        {
                            if (scene == currentScenes)
                            {
                                traceMode = m_tracingMode;
                                break;
                            }
                        }
                    }

                    ssr.tracing.value = isRayTracing ? traceMode : RayCastingMode.RayMarching;
                }
                else
                {
                    ssr.tracing.value = m_tracingMode;
                }
                ssr.textureLodBias.value = m_textureLODBias;
                ssr.rayMiss.value = m_rayMiss;
                ssr.lastBounceFallbackHierarchy.value = m_lastBounce;
#if UNITY_2022_3_OR_NEWER
                ssr.ambientProbeDimmer.value = m_ambientDimmer;
#endif
                ssr.minSmoothness = m_minimumSmoothness;
                ssr.smoothnessFadeStart = Mathf.Clamp(m_smoothnessFadeStart, ssr.minSmoothness + 0.05f, 1f);
                ssr.rayLength = m_maxRayLength;
                ssr.clampValue = m_clampValue;
                ssr.fullResolution = m_fullResolution;
                ssr.rayMaxIterationsRT = m_maxRaySteps;
                ssr.denoise = m_denoise;
                ssr.denoiserRadius = m_denoiseRadius;
#if !UNITY_2023_2_OR_NEWER
                ssr.affectSmoothSurfaces = m_affectsSmoothSurfaces;
#endif
                ssr.mode.value = m_rayTracingMode;
                ssr.sampleCount.value = m_sampleCount;
                ssr.bounceCount.value = m_bounceCount;
            }
        }
#endif
    }
    /// <summary>
    /// Ray traced Ambient Occlusion quality
    /// </summary>
    [Serializable]
    public class RTAmbientOcclusionQuality
    {
        public string m_qualityName;
        [Range(0f, 4f)]
        public float m_intenisty = 1f;
        [Range(0f, 1f)]
        public float m_directLightIntensity = 0f;
        public float m_maxRayLength = 10f;
        public int m_sampleCount = 2;
        public bool m_fullResolution = true;
        public bool m_denoise = true;
        [Range(0.001f, 1f)]
        public float m_denoiseRadius = 0.66f;
        public bool m_occluderMotionRejection = true;
        public bool m_receiverMotionRejection = true;

        /// <summary>
        /// Applies the settings
        /// </summary>
        /// <param name="gi"></param>
#if HDRPTIMEOFDAY && HDPipeline
#if UNITY_2022_2_OR_NEWER
        public void Apply(ScreenSpaceAmbientOcclusion ao)
#else
        public void Apply(AmbientOcclusion ao)
#endif
        {
            if (ao != null)
            {
                ao.quality.value = 3;
                ao.intensity.value = m_intenisty;
                ao.directLightingStrength.value = m_directLightIntensity;
                ao.rayLength = m_maxRayLength;
                ao.sampleCount = m_sampleCount;
                ao.fullResolution = m_fullResolution;
                ao.denoise = m_denoise;
                ao.denoiserRadius = m_denoiseRadius;
                ao.occluderMotionRejection.value = m_occluderMotionRejection;
                ao.receiverMotionRejection.value = m_receiverMotionRejection;
            }
        }
#endif
    }
    /// <summary>
    /// Ray traced sub surface scattering quality
    /// </summary>
    [Serializable]
    public class RTSubSurfaceScatteringQuality
    {
        public string m_qualityName;
        public bool m_isActive = true;
        [Range(1, 32)]
        public int m_sampleCount = 2;

        /// <summary>
        /// Applies the settings
        /// </summary>
        /// <param name="sss"></param>
#if HDRPTIMEOFDAY && HDPipeline
        public void Apply(SubSurfaceScattering sss)
        {
            if (sss != null)
            {
                sss.rayTracing.value = m_isActive;
                sss.sampleCount.value = m_sampleCount;
            }
        }
#endif
    }
    /// <summary>
    /// Ray traced global master settings where all the base quality settings are
    /// </summary>
    [Serializable]
    public class RTGlobalQualitySettings
    {
        public RayTracingOverallQuality CurrentQuality = RayTracingOverallQuality.Off;
        public bool UseRTAdditionalLightOptimization = true;
        public bool SortClosesLightSources = true;
        public int MaxRTAdditionalLightSources = 2;
        public float MaxRenderDistanceCheck = 300f;
        public bool RTSceneCheck = true;
        public List<string> SceneNames = new List<string>();
        public List<RTScreenSpaceGlobalIlluminationQuality> RTSSGIQuality = new List<RTScreenSpaceGlobalIlluminationQuality>();
        public List<RTScreenSpaceReflectionQuality> RTSSRQuality = new List<RTScreenSpaceReflectionQuality>();
        public List<RTAmbientOcclusionQuality> RTAOQuality = new List<RTAmbientOcclusionQuality>();
        public List<RTRecursiveRenderingQuality> RTRRQuality = new List<RTRecursiveRenderingQuality>();
        public List<RTSubSurfaceScatteringQuality> RTSSSQuality = new List<RTSubSurfaceScatteringQuality>();
    }
    /// <summary>
    /// Ray traced global master settings
    /// </summary>
    [Serializable]
    public class RayTraceSettings
    {
        public bool m_rayTraceSettings = false;
        public bool m_renderInEditMode = false;
        public bool m_rayTraceSSGI = false;
#if HDPipeline
        public GeneralQuality m_ssgiQuality = GeneralQuality.High;
#endif
        public float m_ssgiAmbientAmount = 0.75f;
        public bool m_rayTraceSSR = true;
#if HDPipeline
        public GeneralQuality m_ssrQuality = GeneralQuality.High;
#endif
        public float m_ssrAmbientAmount = 0.75f;
        public bool m_rayTraceAmbientOcclusion = false;
#if HDPipeline
        public GeneralQuality m_aoQuality = GeneralQuality.High;
#endif
        public bool m_recursiveRendering = true;
#if HDPipeline
        public GeneralQuality m_recursiveRenderingQuality = GeneralQuality.High;
#endif
        public bool m_rayTraceSubSurfaceScattering = false;
#if HDPipeline
        public GeneralQuality m_sssQuality = GeneralQuality.Low;
#endif
        public bool m_overrideQuality = false;
        public bool m_rayTraceShadows = false;
        public bool m_rayTraceContactShadows = true;
        public bool m_colorShadows = true;
        public bool m_denoiseShadows = true;
#if HDPipeline
        public GeneralQuality m_shadowsQuality = GeneralQuality.Low;
        public GeneralQuality m_overrideQualityValue = GeneralQuality.Low;
#endif

        public RTGlobalQualitySettings RTGlobalQualitySettings = new RTGlobalQualitySettings();
        public GlobalAdvancedRayTracingOptions AdvancedRTGlobalSettings = new GlobalAdvancedRayTracingOptions();
    }

    [CreateAssetMenu(fileName = "HDRP Time Of Day Ray Tracing Profile", menuName = "Procedural Worlds/HDRP Time Of Day/Profiles/HDRP Time Of Day Ray Tracing Profile")]
    public class HDRPTimeOfDayRayTracingProfile : ScriptableObject
    {
        public RayTraceSettings RayTracingSettings
        {
            get { return m_rayTracingSettings; }
            set
            {
                if (m_rayTracingSettings != value)
                {
                    m_rayTracingSettings = value;
                }
            }
        }
        [SerializeField] private RayTraceSettings m_rayTracingSettings = new RayTraceSettings();

        /// <summary>
        /// Checks if ray tracing is enabled
        /// </summary>
        /// <returns></returns>
        public bool IsRTActive()
        {
            if (m_rayTracingSettings.m_rayTraceSSGI || m_rayTracingSettings.m_rayTraceAmbientOcclusion || m_rayTracingSettings.m_rayTraceShadows || m_rayTracingSettings.m_rayTraceSSR ||
                m_rayTracingSettings.m_rayTraceContactShadows || m_rayTracingSettings.m_rayTraceSubSurfaceScattering)
            {
                return true;
            }

            return false;
        }
    }
}