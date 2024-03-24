#if HDPipeline && UNITY_2021_2_OR_NEWER
using System.Collections.Generic;
#if TOD_CINEMACHINE
using Cinemachine;
#endif
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.VFX;

namespace ProceduralWorlds.HDRPTOD
{
    public enum GlobalCloudType { None, Volumetric, Procedural, Both }
    public enum CloudLayerType { Single, Double }
    public enum CloudResolutionQuality { Low, Medium, High, VeryHigh, Ultra, Cinematic }
    public enum CloudResolution { Resolution256, Resolution512, Resolution1024, Resolution2048, Resolution4096, Resolution8192 }
    public enum TimeOfDaySkyMode { PhysicallyBased }
    public enum GeneralQuality { Low, Medium, High }
    public enum GeneralRenderMode { Performance, Quality }
    public enum UnderwaterOverrideSystemType { Custom, HDRPTimeOfDay }
    public enum CloudLayerChannelMode { R,G,B,A }
    public enum SSGIRenderMode { Interior, Exterior, Both }
    public enum TimeOfDayExposureMode { Fixed, Automatic, Physical }
    public enum AntiAliasingQuality { Low, Medium, High }
    public enum CompensationMode { None, DayOnly, NightOnly, Both }
    public enum ProcessingMode { Simple, Advanced }
    public enum LightProbeGenerateMode { Trees, TreesAdvanced, MeshRenderers, Volumes, All }
    public enum TODCloudPresets { Presets, Custom }

    [System.Serializable]
    public class CameraSettings
    {
        public bool m_showSettings = false;
        public float m_nearClipPlane = 0.1f;
        public float m_farClipPlane = 20000f;
        public bool m_dynamicResolution = false;
        public bool m_dlss = true;
        public HDAdditionalCameraData.AntialiasingMode m_antialiasingMode = HDAdditionalCameraData.AntialiasingMode.TemporalAntialiasing;
        public AntiAliasingQuality m_antiAliasingQuality = AntiAliasingQuality.High;
        public bool m_stopNan = false;
        public bool m_dithering = false;
        public bool m_overrideSensor = false;
        public Vector2 m_sensorSize = new Vector2(38, 24);

        public void CopySettings(ref CameraSettings settings)
        {
            settings.m_nearClipPlane = m_nearClipPlane;
            settings.m_farClipPlane = m_farClipPlane;
            settings.m_dynamicResolution = m_dynamicResolution;
            settings.m_dlss = m_dlss;
            settings.m_antialiasingMode = m_antialiasingMode;
            settings.m_antiAliasingQuality = m_antiAliasingQuality;
            settings.m_stopNan = m_stopNan;
            settings.m_dithering = m_dithering;
            settings.m_overrideSensor = m_overrideSensor;
            settings.m_sensorSize = m_sensorSize;
        }
        public void ApplyCameraSettings(HDAdditionalCameraData cameraData, Camera camera)
        {
            if (HDRPTimeOfDay.Instance != null)
            {
                if (HDRPTimeOfDay.Instance.UseRayTracing)
                {
#if HDRPTIMEOFDAY
                    if (HDRPTimeOfDay.Instance.RayTracingLightingModel != RayTracingLightingModel.PathTraced)
                    {
                        cameraData.allowDynamicResolution = m_dynamicResolution;
                        cameraData.allowDeepLearningSuperSampling = m_dlss;
                    }
#endif
                }
                else
                {
                    cameraData.allowDynamicResolution = m_dynamicResolution;
                    cameraData.allowDeepLearningSuperSampling = m_dlss;
                }
            }           

            if (camera == null || cameraData == null )
            {
                return;
            }

            float near = Mathf.Clamp(m_nearClipPlane, 0.01f, float.PositiveInfinity);
            float far = Mathf.Clamp(m_farClipPlane, 0.31f, float.PositiveInfinity);
            camera.nearClipPlane = near;
            camera.farClipPlane = far;           
            cameraData.antialiasing = m_antialiasingMode;
            cameraData.stopNaNs = m_stopNan;
            cameraData.dithering = m_dithering;
            if (m_overrideSensor)
            {
                camera.sensorSize = m_sensorSize;
            }
            switch (m_antiAliasingQuality)
            {
                case AntiAliasingQuality.Low:
                {
                    cameraData.TAAQuality = HDAdditionalCameraData.TAAQualityLevel.Low;
                    cameraData.SMAAQuality = HDAdditionalCameraData.SMAAQualityLevel.Low;
                    break;
                }
                case AntiAliasingQuality.Medium:
                {
                    cameraData.TAAQuality = HDAdditionalCameraData.TAAQualityLevel.Medium;
                    cameraData.SMAAQuality = HDAdditionalCameraData.SMAAQualityLevel.Medium;
                    break;
                }
                case AntiAliasingQuality.High:
                {
                    cameraData.TAAQuality = HDAdditionalCameraData.TAAQualityLevel.High;
                    cameraData.SMAAQuality = HDAdditionalCameraData.SMAAQualityLevel.High;
                    break;
                }
            }
        }
    }
    /// <summary>
    /// Settings for the light probe generation
    /// </summary>
    [System.Serializable]
    public class LightProbeGeneratorSettings
    {
        public LightProbeGenerateMode m_generateMode = LightProbeGenerateMode.All;
        public ProcessingMode m_processingMode = ProcessingMode.Simple;
        public bool m_addHeightOffset = true;
        public float m_heightOffset = 0.5f;
        public float m_sphereOffset = 2.5f;
        public bool m_spawnProbeUnderDetaial = true;

        public void CopySettings(LightProbeGeneratorSettings settings)
        {
            m_generateMode = settings.m_generateMode;
            m_processingMode = settings.m_processingMode;
            m_addHeightOffset = settings.m_addHeightOffset;
            m_heightOffset = settings.m_heightOffset;
            m_sphereOffset = settings.m_sphereOffset;
        }
    }
    [System.Serializable]
    public class TimeOfDayAutomaticExposureSettings
    {
        public Vector2 m_mixMaxLimit = new Vector2(11f, 14f);
        public Vector2 m_histogramPercentage = new Vector2(25f, 75f);
        public Vector2 m_adaptionSpeed = new Vector2(3f, 1.5f);
        public bool m_useSmartTransition = true;
        public float m_transitionDayDuration = 1f;
        public float m_transitionNightDuration = 3f;
        public Vector2 m_mixMaxLimitNight = new Vector2(6.75f, 7.25f);

        public void CopySettings(ref TimeOfDayAutomaticExposureSettings settings)
        {
            if (settings != null)
            {
                settings.m_mixMaxLimit = m_mixMaxLimit;
                settings.m_histogramPercentage = m_histogramPercentage;
                settings.m_adaptionSpeed = m_adaptionSpeed;
                settings.m_useSmartTransition = m_useSmartTransition;
                settings.m_transitionDayDuration = m_transitionDayDuration;
                settings.m_transitionNightDuration = m_transitionNightDuration;
                settings.m_mixMaxLimitNight = m_mixMaxLimitNight;
            }
        }
    }
    [System.Serializable]
    public class TimeOfDayPhysicalExposureSettings
    {
        public int m_iso = 200;
        public float m_shutterSpeed = 200f;
        public AnimationCurve m_aperture = AnimationCurve.Constant(0f, 1f, 6f);

        public void CopySettings(ref TimeOfDayPhysicalExposureSettings settings)
        {
            settings.m_iso = m_iso;
            settings.m_shutterSpeed = m_shutterSpeed;
            settings.m_aperture = m_aperture;
        }
    }
    [System.Serializable]
    public class TimeOfDayLensFlareProfile
    {
        public bool m_useLensFlare = true;
        public LensFlareDataSRP m_lensFlareData;
        public AnimationCurve m_intensity;
        public AnimationCurve m_scale;
        public bool m_enableOcclusion = true;
        public float m_occlusionRadius = 0.1f;
        public int m_sampleCount = 32;
        public float m_occlusionOffset = 0.05f;
        public bool m_allowOffScreen = false;
        public bool m_volumetricCloudOcclusion = true;

        /// <summary>
        /// Copies all the settings into another lens flare profile
        /// </summary>
        /// <param name="profile"></param>
        public void CopySettings(ref TimeOfDayLensFlareProfile profile)
        {
            if (profile == null)
            {
                profile = new TimeOfDayLensFlareProfile();
            }

            if (profile != null)
            {
                profile.m_useLensFlare = m_useLensFlare;
                profile.m_lensFlareData = m_lensFlareData;
                profile.m_intensity = TimeOfDayProfileData.CopyCurve(m_intensity);
                profile.m_scale = TimeOfDayProfileData.CopyCurve(m_scale);
                profile.m_enableOcclusion = m_enableOcclusion;
                profile.m_occlusionRadius = m_occlusionRadius;
                profile.m_sampleCount = m_sampleCount;
                profile.m_occlusionOffset = m_occlusionOffset;
                profile.m_allowOffScreen = m_allowOffScreen;
#if UNITY_2022_2_OR_NEWER
                profile.m_volumetricCloudOcclusion = m_volumetricCloudOcclusion;
#endif
            }
        }
    }
    [System.Serializable]
    public class TimeOfDayDebugSettings
    {
        public bool m_roundUp = true;
        public bool m_showDebugLogs = true;
        public float m_simulationSpeed = 1f;
        public bool m_simulate = false;
    }
    [System.Serializable]
    public class DarkerNightSettings
    {
        public bool m_darkerNights = false;
        public float m_automaticExposureAdjustmentValue = 1.25f;
        public float m_fixedExposureAdjustmentValue = 2.5f;
        public float m_physicalExposureAdjustmentValue = 0.2f;
        public float m_moonIntensityAdjustmentValue = 0.125f;
        public float m_ambientIntensityAdjustmentValue = 0.5f;

        public DarkerNightSettings Copy()
        {
            return new DarkerNightSettings()
            {
                m_darkerNights = m_darkerNights,
                m_automaticExposureAdjustmentValue = m_automaticExposureAdjustmentValue,
                m_fixedExposureAdjustmentValue = m_fixedExposureAdjustmentValue,
                m_physicalExposureAdjustmentValue = m_physicalExposureAdjustmentValue,
                m_moonIntensityAdjustmentValue = m_moonIntensityAdjustmentValue,
                m_ambientIntensityAdjustmentValue = m_ambientIntensityAdjustmentValue,
            };
        }
        public void Set(DarkerNightSettings settings)
        {
            if (settings != null)
            {
                m_darkerNights = settings.m_darkerNights;
                m_automaticExposureAdjustmentValue = settings.m_automaticExposureAdjustmentValue;
                m_fixedExposureAdjustmentValue = settings.m_fixedExposureAdjustmentValue;
                m_physicalExposureAdjustmentValue = settings.m_physicalExposureAdjustmentValue;
                m_moonIntensityAdjustmentValue = settings.m_moonIntensityAdjustmentValue;
                m_ambientIntensityAdjustmentValue = settings.m_ambientIntensityAdjustmentValue;
            }
        }
    }
    [System.Serializable]
    public class TimeOfDayProfileData
    {
        #region Variables

        private float m_lastTimeOfDayValue = -1f;
        private TimeOfDayProfileData m_startingData;
        private bool m_hasBeenSetup = false;

        [Header("Duration")]
        public bool m_durationSettings = false;
        public float m_dayDuration = 300f;
        public float m_nightDuration = 120f;
        [Header("Sun")]
        public bool m_sunSettings = false;
        public bool m_enableSunShadows = true;
        public bool m_enableMoonShadows = true;
        public AnimationCurve m_sunIntensity;
        public AnimationCurve m_moonIntensity;
        public AnimationCurve m_sunIntensityMultiplier;
        public AnimationCurve m_sunTemperature;
        public Gradient m_sunColorFilter;
        public AnimationCurve m_moonTemperature;
        public Gradient m_moonColorFilter;
        public AnimationCurve m_sunVolumetrics;
        public AnimationCurve m_sunVolumetricShadowDimmer;
        public bool m_useTwilight = true;
        public AnimationCurve m_twilightIntensity;
        public AnimationCurve m_twilightTemperature;
        public float m_globalLightMultiplier = 1f;
        [Header("Sky")]
        public bool m_skySettings = false;
        public float m_horizonOffset = 0.1f;
        public TimeOfDaySkyMode m_skyMode = TimeOfDaySkyMode.PhysicallyBased;
        public float m_skyboxExposure = 2.2f;
        public Color m_skyboxGroundColor = new Color(0.9609969f, 0.9024923f, 0.8708023f, 1f);
        public bool m_rotateStars = true;
        public AnimationCurve m_starIntensity;
        public bool m_resetStarsRotationOnEnable = true;
        public Vector3 m_starsRotationSpeed = new Vector3(0.001f, 0.002f, 0.001f);
        [Header("Shadows")]
        public AnimationCurve m_shadowDistance;
        public AnimationCurve m_shadowTransmissionMultiplier;
        public int m_shadowCascadeCount = 4;
        public float m_shadowDistanceMultiplier = 1f;
        public float m_shadowDistanceMultiplierNight = 0.15f;
        public AnimationCurve m_sunShadowDimmer;
        public AnimationCurve m_moonShadowDimmer;
        public List<float> m_shadowCascadeFadeLength = new List<float>();
        [Header("Fog")]
        public bool m_fogSettings = false;
        public bool m_useFog = true;
        public bool m_enableVolumetricFog = true;
        public GeneralQuality m_fogQuality = GeneralQuality.Medium;
        public bool m_useDenoising = true;
        public GeneralQuality m_denoisingQuality = GeneralQuality.Medium;
        public Gradient m_fogColor;
        public AnimationCurve m_fogDistance;
        public AnimationCurve m_fogDensity;
        public AnimationCurve m_fogHeight;
        public AnimationCurve m_volumetricFogDistance;
        public bool m_useFogAnisotropyOverride = true;
        public AnimationCurve m_volumetricFogAnisotropy;
        public AnimationCurve m_volumetricFogSliceDistributionUniformity;
        public AnimationCurve m_localFogMultiplier;
        public float m_globalFogMultiplier = 1f;
        public float m_globalFogHeightMultiplier = 1f;
        public float m_globalFogDistanceMultiplier = 1f;
        public float m_localFogSize = 200f;
        [Header("Clouds")]
        public bool m_cloudSettings = false;
        public GlobalCloudType m_globalCloudType = GlobalCloudType.Volumetric;
        //Volumetric
        public bool m_useLocalClouds = false;
        public TODCloudPresets m_cloudPresets = TODCloudPresets.Presets;
        public AnimationCurve m_volumetricDensityMultiplier;
        public AnimationCurve m_volumetricDensityCurve;
        public AnimationCurve m_volumetricShapeFactor;
        public AnimationCurve m_volumetricShapeScale;
        public AnimationCurve m_volumetricErosionFactor;
        public AnimationCurve m_volumetricErosionScale;
        public VolumetricClouds.CloudErosionNoise m_erosionNoiseType;
        public AnimationCurve m_volumetricErosionCurve;
        public AnimationCurve m_volumetricAmbientOcclusionCurve;
        public AnimationCurve m_volumetricLowestCloudAltitude;
        public AnimationCurve m_volumetricCloudThickness;
        public AnimationCurve m_volumetricAmbientLightProbeDimmer;
        public AnimationCurve m_volumetricSunLightDimmer;
        public AnimationCurve m_volumetricErosionOcclusion;
        public Gradient m_volumetricScatteringTint;
        public AnimationCurve m_volumetricPowderEffectIntensity;
        public AnimationCurve m_volumetricMultiScattering;
        public HDRPTimeOfDayVolumetricCloudPreset VolumetricCloudPreset;
        public bool m_volumetricCloudShadows = true;
        public VolumetricClouds.CloudShadowResolution m_volumetricCloudShadowResolution = VolumetricClouds.CloudShadowResolution.Medium256;
        public AnimationCurve m_volumetricCloudShadowOpacity;
        public float m_volumetricCloudHeightMultiplier = 1f;
        public bool m_useHeightBasedVolumetricCloudShadows = false;
        [Range(0f, 1f)]
        public float m_volumetricCloudMinShadowClampValue = 0.25f;
        public float m_volumetricCloudShadowsGroundHeight = 150f;
        //Procedural
        public HDRPTimeOfDayProceduralCloudPreset ProceduralCloudPreset;
        public TODCloudPresets m_proceduralCloudPresets = TODCloudPresets.Presets;
        public CloudLayerType m_cloudLayers = CloudLayerType.Single;
        public CloudResolution m_cloudResolution = CloudResolution.Resolution1024;
        public bool m_useCloudShadows = false;
        public AnimationCurve m_cloudOpacity;
        public Gradient m_cloudTintColor;
        public AnimationCurve m_cloudExposure;
        public AnimationCurve m_cloudWindDirection;
        public AnimationCurve m_cloudWindSpeed;
        public AnimationCurve m_cloudShadowOpacity;
        public bool m_cloudLighting = true;
        public Gradient m_cloudShadowColor;
        public CloudLayerChannelMode m_cloudLayerAChannel = CloudLayerChannelMode.R;
        public AnimationCurve m_cloudLayerAOpacityR;
        public AnimationCurve m_cloudLayerAOpacityG;
        public AnimationCurve m_cloudLayerAOpacityB;
        public AnimationCurve m_cloudLayerAOpacityA;
        public CloudLayerChannelMode m_cloudLayerBChannel = CloudLayerChannelMode.B;
        public AnimationCurve m_cloudLayerBOpacityR;
        public AnimationCurve m_cloudLayerBOpacityG;
        public AnimationCurve m_cloudLayerBOpacityB;
        public AnimationCurve m_cloudLayerBOpacityA;
        [Header("Advanced Lighting")]
        public bool m_advancedLightingSettings = false;
        public bool m_useSSGI = false;
        public bool m_ambientSSGICompensation = false;
        public bool m_reflectionAmbientSSGICompensation = true;
        public float m_ambientCompensationExposure = 2f;
        public float m_ambientCompensationAmount = 2.5f;
        public CompensationMode m_compensationMode = CompensationMode.DayOnly;
        public CompensationMode m_compensationModeAmbient = CompensationMode.DayOnly;
        public GeneralQuality m_ssgiQuality = GeneralQuality.Low;
        public SSGIRenderMode m_ssgiRenderMode = SSGIRenderMode.Exterior;
        public bool m_useSSR = false;
        public GeneralQuality m_ssrQuality = GeneralQuality.Medium;
        public bool m_useSSS = true;
        public bool m_useContactShadows = true;
        public AnimationCurve m_contactShadowsDistance;
        public AnimationCurve m_contactShadowsOpacity;
        public bool m_useAdvancedNearFarContactShadows = true;
        public float m_nearThresholdPercentage = 0.2f;
        [Range(0.01f, 1f)]
        public float m_farShadowPercentageBlend = 0.25f;
        public bool m_setContactShadowsMinDistanceToShadowDistance = false;
        public AnimationCurve m_nearContactShadowsDistance;
        [Range(1, 64)]
        public int m_nearContactShadowsSampleCount = 32;
        [Range(1, 64)]
        public int m_farContactShadowsSampleCount = 64;
        public LayerMask m_groundLayer = -1;
        public bool m_useMicroShadows = false;
        public AnimationCurve m_microShadowOpacity;
        public TimeOfDayExposureMode m_exposureMode = TimeOfDayExposureMode.Fixed;
        public AnimationCurve m_generalExposure;
        public TimeOfDayAutomaticExposureSettings m_autoExposureSettings = new TimeOfDayAutomaticExposureSettings();
        public TimeOfDayPhysicalExposureSettings m_physicalExposureSettings = new TimeOfDayPhysicalExposureSettings();
        public bool m_useNonSSGIAmbientCorrection = true;
        public float m_ambientIntensityMultiplier = 1f;
        public AnimationCurve m_ambientIntensity;
        public AnimationCurve m_ambientReflectionIntensity;
        public AnimationCurve m_planarReflectionIntensity;      
        public TimeOfDayLensFlareProfile m_sunLensFlareProfile;
        public TimeOfDayLensFlareProfile m_moonLensFlareProfile;
        public DarkerNightSettings m_darkerNightSettings = new DarkerNightSettings();
        public const float m_nonSSGIAmbientIntensityDivider = 0.4f;

        #endregion

#if UNITY_2022_2_OR_NEWER
        /// <summary>
        /// Applies the weather and lerps the values from current to this profile settings
        /// </summary>
        /// <param name="lightData"></param>
        /// <param name="physicallyBasedSky"></param>
        /// <param name="gradientSky"></param>
        /// <param name="exposure"></param>
        /// <param name="fog"></param>
        /// <param name="clouds"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public bool ReturnFromWeather(HDRPTimeOfDayComponents components, bool isDay, bool overrideLightSource, float time, float duration)
        {
            if (!m_hasBeenSetup)
            {
                Debug.LogError(" Weather was not setup with the starting values please call SetupStartingSettings() before calling Apply weather");
                return false;
            }

            components.m_weatherBlendVolume.weight = 1f - duration;
            HDAdditionalLightData lightData = isDay ? components.m_sunLightData : components.m_moonLightData;
            //Sun
            if (ValidateSun())
            {
                if (!overrideLightSource)
                {
                    if (isDay)
                    {
                        lightData.SetIntensity(LerpFloat(m_startingData.m_sunIntensity.Evaluate(m_lastTimeOfDayValue), m_sunIntensity.Evaluate(time) * m_globalLightMultiplier, duration));
                        lightData.SetColor(LerpColor(m_startingData.m_sunColorFilter.Evaluate(m_lastTimeOfDayValue), m_sunColorFilter.Evaluate(time), duration), LerpFloat(m_startingData.m_sunTemperature.Evaluate(m_lastTimeOfDayValue), m_sunTemperature.Evaluate(time), duration));
                    }
                    else
                    {
                        lightData.SetIntensity(LerpFloat(m_startingData.m_moonIntensity.Evaluate(m_lastTimeOfDayValue), m_moonIntensity.Evaluate(time) * m_globalLightMultiplier, duration));
                        lightData.SetColor(LerpColor(m_startingData.m_moonColorFilter.Evaluate(m_lastTimeOfDayValue), m_moonColorFilter.Evaluate(time), duration), LerpFloat(m_startingData.m_moonTemperature.Evaluate(m_lastTimeOfDayValue), m_moonTemperature.Evaluate(time), duration));
                    }
                }

                lightData.volumetricDimmer = LerpFloat(m_startingData.m_sunVolumetrics.Evaluate(m_lastTimeOfDayValue), m_sunVolumetrics.Evaluate(time), duration);
                lightData.volumetricShadowDimmer = LerpFloat(m_startingData.m_sunVolumetricShadowDimmer.Evaluate(m_lastTimeOfDayValue), m_sunVolumetricShadowDimmer.Evaluate(time), duration);
                lightData.lightDimmer = LerpFloat(m_startingData.m_sunIntensityMultiplier.Evaluate(m_lastTimeOfDayValue), m_sunIntensityMultiplier.Evaluate(time), duration);
            }
            //Advanced Lighting
            if (ValidateAdvancedLighting())
            {
                //Exposure
                ApplyExposure(components, isDay, time, duration, HDRPTimeOfDay.Instance.GetCurrentOverrideData(), true);
                if (m_useSSGI)
                {
                    components.m_timeOfDayVolumeComponenets.m_indirectLightingController.indirectDiffuseLightingMultiplier.value = LerpFloat(m_startingData.m_ambientIntensity.Evaluate(m_lastTimeOfDayValue), m_ambientIntensity.Evaluate(time) * m_ambientIntensityMultiplier, duration);
                }
                else
                {
                    components.m_timeOfDayVolumeComponenets.m_indirectLightingController.indirectDiffuseLightingMultiplier.value = LerpFloat(m_startingData.m_ambientIntensity.Evaluate(m_lastTimeOfDayValue), m_ambientIntensity.Evaluate(time) * (m_ambientIntensityMultiplier / m_nonSSGIAmbientIntensityDivider), duration);
                }

                components.m_timeOfDayVolumeComponenets.m_indirectLightingController.reflectionLightingMultiplier.value = LerpFloat(m_startingData.m_ambientReflectionIntensity.Evaluate(m_lastTimeOfDayValue), m_ambientReflectionIntensity.Evaluate(time), duration);
                components.m_timeOfDayVolumeComponenets.m_indirectLightingController.reflectionProbeIntensityMultiplier.value = LerpFloat(m_startingData.m_planarReflectionIntensity.Evaluate(m_lastTimeOfDayValue), m_planarReflectionIntensity.Evaluate(time), duration);
            }
            //Fog
            if (ValidateFog())
            {
                //Local Fog
                components.m_localVolumetricFog.parameters.meanFreePath = LerpFloat(m_startingData.m_fogDensity.Evaluate(m_lastTimeOfDayValue), m_fogDensity.Evaluate(time) / m_globalFogMultiplier, duration);
                components.m_localVolumetricFog.parameters.albedo = LerpColor(m_startingData.m_fogColor.Evaluate(m_lastTimeOfDayValue), m_fogColor.Evaluate(time) * m_localFogMultiplier.Evaluate(time), duration);
                components.m_localVolumetricFog.parameters.size = new Vector3(m_localFogSize, m_localFogSize, m_localFogSize);
                HDRPTimeOfDay.Instance.SetCurrentLocalFogDistance(components.m_localVolumetricFog.parameters.meanFreePath);
                //Global
                components.m_timeOfDayVolumeComponenets.m_fog.albedo.value = LerpColor(m_startingData.m_fogColor.Evaluate(m_lastTimeOfDayValue), m_fogColor.Evaluate(time), duration);
                components.m_timeOfDayVolumeComponenets.m_fog.meanFreePath.value = LerpFloat(m_startingData.m_fogDistance.Evaluate(m_lastTimeOfDayValue),m_fogDistance.Evaluate(time) / m_globalFogDistanceMultiplier, duration);
                float fogHeight = m_fogHeight.Evaluate(time) * m_globalFogHeightMultiplier;
                components.m_timeOfDayVolumeComponenets.m_fog.baseHeight.value = LerpFloat(m_startingData.m_fogHeight.Evaluate(m_lastTimeOfDayValue), fogHeight, duration);
                components.m_timeOfDayVolumeComponenets.m_fog.maximumHeight.value = LerpFloat(m_startingData.m_fogHeight.Evaluate(m_lastTimeOfDayValue) * 4f, fogHeight * 4f, duration);
                components.m_timeOfDayVolumeComponenets.m_fog.depthExtent.value = LerpFloat(m_startingData.m_volumetricFogDistance.Evaluate(m_lastTimeOfDayValue), m_volumetricFogDistance.Evaluate(time), duration);
                components.m_timeOfDayVolumeComponenets.m_fog.anisotropy.value = LerpFloat(m_startingData.m_volumetricFogAnisotropy.Evaluate(m_lastTimeOfDayValue), m_volumetricFogAnisotropy.Evaluate(time), duration);
                components.m_timeOfDayVolumeComponenets.m_fog.sliceDistributionUniformity.value = LerpFloat(m_startingData.m_volumetricFogSliceDistributionUniformity.Evaluate(m_lastTimeOfDayValue), m_volumetricFogSliceDistributionUniformity.Evaluate(time), duration);
            }
            //Shadows
            if (ValidateShadows())
            {
                components.m_timeOfDayVolumeComponenets.m_shadows.maxShadowDistance.value = LerpFloat(m_startingData.m_shadowDistance.Evaluate(m_lastTimeOfDayValue) * m_startingData.m_shadowDistanceMultiplier, m_shadowDistance.Evaluate(time) * m_shadowDistanceMultiplier, duration);
                components.m_timeOfDayVolumeComponenets.m_shadows.directionalTransmissionMultiplier.value = LerpFloat(m_startingData.m_shadowTransmissionMultiplier.Evaluate(m_lastTimeOfDayValue), m_shadowTransmissionMultiplier.Evaluate(time), duration);
            }
            //Clouds
            if (ValidateClouds())
            {
                //Volumetric
#if UNITY_2022_3_OR_NEWER
                components.m_timeOfDayVolumeComponenets.m_volumetricClouds.cloudPreset = VolumetricClouds.CloudPresets.Custom;
#else
                components.m_timeOfDayVolumeComponenets.m_volumetricClouds.cloudPreset.value = VolumetricClouds.CloudPresets.Custom;
#endif
                if (m_cloudPresets == TODCloudPresets.Custom)
                {
                    components.m_timeOfDayVolumeComponenets.m_volumetricClouds.densityMultiplier.value = LerpFloat(
                        m_startingData.m_volumetricDensityMultiplier.Evaluate(m_lastTimeOfDayValue),
                        m_volumetricDensityMultiplier.Evaluate(time), duration);
                    components.m_timeOfDayVolumeComponenets.m_volumetricClouds.shapeFactor.value = LerpFloat(
                        m_startingData.m_volumetricShapeFactor.Evaluate(m_lastTimeOfDayValue),
                        m_volumetricShapeFactor.Evaluate(time), duration);
                    components.m_timeOfDayVolumeComponenets.m_volumetricClouds.shapeScale.value = LerpFloat(
                        m_startingData.m_volumetricShapeScale.Evaluate(m_lastTimeOfDayValue),
                        m_volumetricShapeScale.Evaluate(time), duration);
                    components.m_timeOfDayVolumeComponenets.m_volumetricClouds.erosionFactor.value = LerpFloat(
                        m_startingData.m_volumetricErosionFactor.Evaluate(m_lastTimeOfDayValue),
                        m_volumetricErosionFactor.Evaluate(time), duration);
                    components.m_timeOfDayVolumeComponenets.m_volumetricClouds.erosionScale.value = LerpFloat(
                        m_startingData.m_volumetricErosionScale.Evaluate(m_lastTimeOfDayValue),
                        m_volumetricErosionScale.Evaluate(time), duration);
#if UNITY_2022_2_OR_NEWER
                    components.m_timeOfDayVolumeComponenets.m_volumetricClouds.densityCurve.value =
                        m_volumetricDensityCurve;
                    components.m_timeOfDayVolumeComponenets.m_volumetricClouds.bottomAltitude.value = LerpFloat(
                        m_startingData.m_volumetricLowestCloudAltitude.Evaluate(m_lastTimeOfDayValue),
                        m_volumetricLowestCloudAltitude.Evaluate(time) * m_volumetricCloudHeightMultiplier, duration);
                    components.m_timeOfDayVolumeComponenets.m_volumetricClouds.altitudeRange.value = LerpFloat(
                        m_startingData.m_volumetricCloudThickness.Evaluate(m_lastTimeOfDayValue),
                        m_volumetricCloudThickness.Evaluate(time), duration);
#else
                components.m_timeOfDayVolumeComponenets.m_volumetricClouds.customDensityCurve.value =
 m_volumetricDensityCurve;
                components.m_timeOfDayVolumeComponenets.m_volumetricClouds.lowestCloudAltitude.value =
 LerpFloat(m_startingData.m_volumetricLowestCloudAltitude.Evaluate(m_lastTimeOfDayValue), m_volumetricLowestCloudAltitude.Evaluate(time) * m_volumetricCloudHeightMultiplier, duration);
                components.m_timeOfDayVolumeComponenets.m_volumetricClouds.cloudThickness.value =
 LerpFloat(m_startingData.m_volumetricCloudThickness.Evaluate(m_lastTimeOfDayValue), m_volumetricCloudThickness.Evaluate(time), duration);
#endif
                }
                else
                {
#if HDRPTIMEOFDAY
                    VolumetricCloudPreset.Apply(components.m_timeOfDayVolumeComponenets.m_volumetricClouds);
                    VolumetricCloudPreset.ApplyWind(components.m_timeOfDayVolumeComponenets.m_visualEnvironment, time);
#endif
                }

                components.m_timeOfDayVolumeComponenets.m_volumetricClouds.ambientLightProbeDimmer.value = LerpFloat(m_startingData.m_volumetricAmbientLightProbeDimmer.Evaluate(m_lastTimeOfDayValue), m_volumetricAmbientLightProbeDimmer.Evaluate(time), duration);
                components.m_timeOfDayVolumeComponenets.m_volumetricClouds.sunLightDimmer.value = LerpFloat(m_startingData.m_volumetricSunLightDimmer.Evaluate(m_lastTimeOfDayValue), m_volumetricSunLightDimmer.Evaluate(time), duration);
                components.m_timeOfDayVolumeComponenets.m_volumetricClouds.erosionOcclusion.value = LerpFloat(m_startingData.m_volumetricErosionOcclusion.Evaluate(m_lastTimeOfDayValue), m_volumetricErosionOcclusion.Evaluate(time), duration);
                components.m_timeOfDayVolumeComponenets.m_volumetricClouds.scatteringTint.value = LerpColor(m_startingData.m_volumetricScatteringTint.Evaluate(m_lastTimeOfDayValue), m_volumetricScatteringTint.Evaluate(time), duration);
                components.m_timeOfDayVolumeComponenets.m_volumetricClouds.powderEffectIntensity.value = LerpFloat(m_startingData.m_volumetricPowderEffectIntensity.Evaluate(m_lastTimeOfDayValue), m_volumetricPowderEffectIntensity.Evaluate(time), duration);
                components.m_timeOfDayVolumeComponenets.m_volumetricClouds.multiScattering.value = LerpFloat(m_startingData.m_volumetricMultiScattering.Evaluate(m_lastTimeOfDayValue), m_volumetricMultiScattering.Evaluate(time), duration);
                components.m_timeOfDayVolumeComponenets.m_volumetricClouds.shadowOpacity.value = LerpFloat(m_startingData.m_volumetricCloudShadowOpacity.Evaluate(m_lastTimeOfDayValue), m_volumetricCloudShadowOpacity.Evaluate(time), duration);

                //Procedural
                components.m_timeOfDayVolumeComponenets.m_cloudLayer.opacity.value = LerpFloat(m_startingData.m_cloudOpacity.Evaluate(m_lastTimeOfDayValue), m_cloudOpacity.Evaluate(time), duration);
                components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerA.tint.value = Color.white;
                components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerB.tint.value = Color.white;
                components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerA.exposure.value = LerpFloat(m_startingData.m_cloudExposure.Evaluate(m_lastTimeOfDayValue), m_cloudExposure.Evaluate(time), duration);
                components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerB.exposure.value = LerpFloat(m_startingData.m_cloudExposure.Evaluate(m_lastTimeOfDayValue), m_cloudExposure.Evaluate(time), duration);
                components.m_timeOfDayVolumeComponenets.m_cloudLayer.shadowMultiplier.value = LerpFloat(m_startingData.m_cloudShadowOpacity.Evaluate(m_lastTimeOfDayValue), m_cloudShadowOpacity.Evaluate(time), duration);
                components.m_timeOfDayVolumeComponenets.m_cloudLayer.shadowTint.value = LerpColor(m_startingData.m_cloudShadowColor.Evaluate(m_lastTimeOfDayValue), m_cloudShadowColor.Evaluate(time), duration);

                switch (m_cloudLayerAChannel)
                {
                    case CloudLayerChannelMode.R:
                    {
                        components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerA.opacityR.value = LerpFloat(m_startingData.m_cloudLayerAOpacityR.Evaluate(m_lastTimeOfDayValue), m_cloudLayerAOpacityR.Evaluate(time), duration);
                        components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerA.opacityG.value = 0f;
                        components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerA.opacityB.value = 0f;
                        components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerA.opacityA.value = 0f;
                        break;
                    }
                    case CloudLayerChannelMode.G:
                    {
                        components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerA.opacityG.value = LerpFloat(m_startingData.m_cloudLayerAOpacityR.Evaluate(m_lastTimeOfDayValue), m_cloudLayerAOpacityR.Evaluate(time), duration);
                        components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerA.opacityR.value = 0f;
                        components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerA.opacityB.value = 0f;
                        components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerA.opacityA.value = 0f;
                        break;
                    }
                    case CloudLayerChannelMode.B:
                    {
                        components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerA.opacityB.value = LerpFloat(m_startingData.m_cloudLayerAOpacityR.Evaluate(m_lastTimeOfDayValue), m_cloudLayerAOpacityR.Evaluate(time), duration);
                        components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerA.opacityR.value = 0f;
                        components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerA.opacityG.value = 0f;
                        components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerA.opacityA.value = 0f;
                        break;
                    }
                    case CloudLayerChannelMode.A:
                    {
                        components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerA.opacityA.value = LerpFloat(m_startingData.m_cloudLayerAOpacityR.Evaluate(m_lastTimeOfDayValue), m_cloudLayerAOpacityR.Evaluate(time), duration);
                        components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerA.opacityR.value = 0f;
                        components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerA.opacityG.value = 0f;
                        components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerA.opacityB.value = 0f;
                        break;
                    }
                }
                switch (m_cloudLayerBChannel)
                {
                    case CloudLayerChannelMode.R:
                    {
                        components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerB.opacityR.value = LerpFloat(m_startingData.m_cloudLayerBOpacityR.Evaluate(m_lastTimeOfDayValue), m_cloudLayerBOpacityR.Evaluate(time), duration);
                        components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerB.opacityG.value = 0f;
                        components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerB.opacityB.value = 0f;
                        components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerB.opacityA.value = 0f;
                        break;
                    }
                    case CloudLayerChannelMode.G:
                    {
                        components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerB.opacityG.value = LerpFloat(m_startingData.m_cloudLayerBOpacityR.Evaluate(m_lastTimeOfDayValue), m_cloudLayerBOpacityR.Evaluate(time), duration);
                        components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerB.opacityR.value = 0f;
                        components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerB.opacityB.value = 0f;
                        components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerB.opacityA.value = 0f;
                        break;
                    }
                    case CloudLayerChannelMode.B:
                    {
                        components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerB.opacityB.value = LerpFloat(m_startingData.m_cloudLayerBOpacityR.Evaluate(m_lastTimeOfDayValue), m_cloudLayerBOpacityR.Evaluate(time), duration);
                        components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerB.opacityR.value = 0f;
                        components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerB.opacityG.value = 0f;
                        components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerB.opacityA.value = 0f;
                        break;
                    }
                    case CloudLayerChannelMode.A:
                    {
                        components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerB.opacityA.value = LerpFloat(m_startingData.m_cloudLayerBOpacityR.Evaluate(m_lastTimeOfDayValue), m_cloudLayerBOpacityR.Evaluate(time), duration);
                        components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerB.opacityR.value = 0f;
                        components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerB.opacityG.value = 0f;
                        components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerB.opacityB.value = 0f;
                        break;
                    }
                }
            }
            //Sun Flare
            if (ValidateSunLensFlare())
            {
                components.m_sunLensFlare.intensity = LerpFloat(m_startingData.m_sunLensFlareProfile.m_intensity.Evaluate(m_lastTimeOfDayValue), m_sunLensFlareProfile.m_intensity.Evaluate(time), duration);
                components.m_sunLensFlare.scale = LerpFloat(m_startingData.m_sunLensFlareProfile.m_scale.Evaluate(m_lastTimeOfDayValue), m_sunLensFlareProfile.m_scale.Evaluate(time), duration);
            }
            //Moon Flare
            if (ValidateMoonLensFlare())
            {
                components.m_moonLensFlare.intensity = LerpFloat(m_startingData.m_moonLensFlareProfile.m_intensity.Evaluate(m_lastTimeOfDayValue), m_moonLensFlareProfile.m_intensity.Evaluate(time), duration);
                components.m_moonLensFlare.scale = LerpFloat(m_startingData.m_moonLensFlareProfile.m_scale.Evaluate(m_lastTimeOfDayValue), m_moonLensFlareProfile.m_scale.Evaluate(time), duration);
            }

            return duration >= 1f;
        }
#else
        /// <summary>
        /// Applies the weather and lerps the values from current to this profile settings
        /// </summary>
        /// <param name="lightData"></param>
        /// <param name="physicallyBasedSky"></param>
        /// <param name="gradientSky"></param>
        /// <param name="exposure"></param>
        /// <param name="fog"></param>
        /// <param name="clouds"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public bool ReturnFromWeather(HDRPTimeOfDayComponents components, bool isDay, bool overrideLightSource, float time, float duration)
        {
            if (!m_hasBeenSetup)
            {
                Debug.LogError(" Weather was not setup with the starting values please call SetupStartingSettings() before calling Apply weather");
                return false;
            }

            if (duration >= 1f)
            {
                return true;
            }

            HDAdditionalLightData lightData = isDay ? components.m_sunLightData : components.m_moonLightData;
            //Sun
            if (ValidateSun())
            {
                if (!overrideLightSource)
                {
                    if (isDay)
                    {
                        lightData.SetIntensity(LerpFloat(m_startingData.m_sunIntensity.Evaluate(m_lastTimeOfDayValue), m_sunIntensity.Evaluate(time) * m_globalLightMultiplier, duration));
                        lightData.SetColor(LerpColor(m_startingData.m_sunColorFilter.Evaluate(m_lastTimeOfDayValue), m_sunColorFilter.Evaluate(time), duration), LerpFloat(m_startingData.m_sunTemperature.Evaluate(m_lastTimeOfDayValue), m_sunTemperature.Evaluate(time), duration));
                    }
                    else
                    {
                        lightData.SetIntensity(LerpFloat(m_startingData.m_moonIntensity.Evaluate(m_lastTimeOfDayValue), m_moonIntensity.Evaluate(time) * m_globalLightMultiplier, duration));
                        lightData.SetColor(LerpColor(m_startingData.m_moonColorFilter.Evaluate(m_lastTimeOfDayValue), m_moonColorFilter.Evaluate(time), duration), LerpFloat(m_startingData.m_moonTemperature.Evaluate(m_lastTimeOfDayValue), m_moonTemperature.Evaluate(time), duration));
                    }
                }

                lightData.volumetricDimmer = LerpFloat(m_startingData.m_sunVolumetrics.Evaluate(m_lastTimeOfDayValue), m_sunVolumetrics.Evaluate(time), duration);
                lightData.volumetricShadowDimmer = LerpFloat(m_startingData.m_sunVolumetricShadowDimmer.Evaluate(m_lastTimeOfDayValue), m_sunVolumetricShadowDimmer.Evaluate(time), duration);
                lightData.lightDimmer = LerpFloat(m_startingData.m_sunIntensityMultiplier.Evaluate(m_lastTimeOfDayValue), m_sunIntensityMultiplier.Evaluate(time), duration);
            }
            //Advanced Lighting
            if (ValidateAdvancedLighting())
            {
                //Exposure
                ApplyExposure(components, isDay, time, duration, HDRPTimeOfDay.Instance.GetCurrentOverrideData(), true);
                if (m_useSSGI)
                {
                    components.m_timeOfDayVolumeComponenets.m_indirectLightingController.indirectDiffuseLightingMultiplier.value = LerpFloat(m_startingData.m_ambientIntensity.Evaluate(m_lastTimeOfDayValue), m_ambientIntensity.Evaluate(time) * m_ambientIntensityMultiplier, duration);
                }
                else
                {
                    components.m_timeOfDayVolumeComponenets.m_indirectLightingController.indirectDiffuseLightingMultiplier.value = LerpFloat(m_startingData.m_ambientIntensity.Evaluate(m_lastTimeOfDayValue), m_ambientIntensity.Evaluate(time) * (m_ambientIntensityMultiplier / m_nonSSGIAmbientIntensityDivider), duration);
                }

                components.m_timeOfDayVolumeComponenets.m_indirectLightingController.reflectionLightingMultiplier.value = LerpFloat(m_startingData.m_ambientReflectionIntensity.Evaluate(m_lastTimeOfDayValue), m_ambientReflectionIntensity.Evaluate(time), duration);
                components.m_timeOfDayVolumeComponenets.m_indirectLightingController.reflectionProbeIntensityMultiplier.value = LerpFloat(m_startingData.m_planarReflectionIntensity.Evaluate(m_lastTimeOfDayValue), m_planarReflectionIntensity.Evaluate(time), duration);
            }
            //Fog
            if (ValidateFog())
            {
                //Local Fog
                components.m_localVolumetricFog.parameters.meanFreePath = LerpFloat(m_startingData.m_fogDensity.Evaluate(m_lastTimeOfDayValue), m_fogDensity.Evaluate(time) / m_globalFogMultiplier, duration);
                components.m_localVolumetricFog.parameters.albedo = LerpColor(m_startingData.m_fogColor.Evaluate(m_lastTimeOfDayValue), m_fogColor.Evaluate(time) * m_localFogMultiplier.Evaluate(time), duration);
                components.m_localVolumetricFog.parameters.size = new Vector3(m_localFogSize, m_localFogSize, m_localFogSize);
                HDRPTimeOfDay.Instance.SetCurrentLocalFogDistance(components.m_localVolumetricFog.parameters.meanFreePath);
                //Global
                components.m_timeOfDayVolumeComponenets.m_fog.albedo.value = LerpColor(m_startingData.m_fogColor.Evaluate(m_lastTimeOfDayValue), m_fogColor.Evaluate(time), duration);
                components.m_timeOfDayVolumeComponenets.m_fog.meanFreePath.value = LerpFloat(m_startingData.m_fogDistance.Evaluate(m_lastTimeOfDayValue),m_fogDistance.Evaluate(time) / m_globalFogDistanceMultiplier, duration);
                float fogHeight = m_fogHeight.Evaluate(time) * m_globalFogHeightMultiplier;
                components.m_timeOfDayVolumeComponenets.m_fog.baseHeight.value = LerpFloat(m_startingData.m_fogHeight.Evaluate(m_lastTimeOfDayValue), fogHeight, duration);
                components.m_timeOfDayVolumeComponenets.m_fog.maximumHeight.value = LerpFloat(m_startingData.m_fogHeight.Evaluate(m_lastTimeOfDayValue) * 4f, fogHeight * 4f, duration);
                components.m_timeOfDayVolumeComponenets.m_fog.depthExtent.value = LerpFloat(m_startingData.m_volumetricFogDistance.Evaluate(m_lastTimeOfDayValue), m_volumetricFogDistance.Evaluate(time), duration);
                if (m_useFogAnisotropyOverride)
                {
                    components.m_timeOfDayVolumeComponenets.m_fog.anisotropy.value = LerpFloat(m_startingData.m_volumetricFogAnisotropy.Evaluate(m_lastTimeOfDayValue), m_volumetricFogAnisotropy.Evaluate(time), duration);
                }
                else
                {
                    components.m_timeOfDayVolumeComponenets.m_fog.anisotropy.value = 0;
                }
                components.m_timeOfDayVolumeComponenets.m_fog.sliceDistributionUniformity.value = LerpFloat(m_startingData.m_volumetricFogSliceDistributionUniformity.Evaluate(m_lastTimeOfDayValue), m_volumetricFogSliceDistributionUniformity.Evaluate(time), duration);
            }
            //Shadows
            if (ValidateShadows())
            {
                components.m_timeOfDayVolumeComponenets.m_shadows.maxShadowDistance.value = LerpFloat(m_startingData.m_shadowDistance.Evaluate(m_lastTimeOfDayValue) * m_startingData.m_shadowDistanceMultiplier, m_shadowDistance.Evaluate(time) * m_shadowDistanceMultiplier, duration);
                components.m_timeOfDayVolumeComponenets.m_shadows.directionalTransmissionMultiplier.value = LerpFloat(m_startingData.m_shadowTransmissionMultiplier.Evaluate(m_lastTimeOfDayValue), m_shadowTransmissionMultiplier.Evaluate(time), duration);
            } 

            //Clouds
            if (ValidateClouds())
            {
                //Volumetric
                components.m_timeOfDayVolumeComponenets.m_volumetricClouds.cloudPreset.value = VolumetricClouds.CloudPresets.Custom;
                if (m_cloudPresets == TODCloudPresets.Custom)
                {
                    components.m_timeOfDayVolumeComponenets.m_volumetricClouds.densityMultiplier.value = LerpFloat(
                        m_startingData.m_volumetricDensityMultiplier.Evaluate(m_lastTimeOfDayValue),
                        m_volumetricDensityMultiplier.Evaluate(time), duration);
                    components.m_timeOfDayVolumeComponenets.m_volumetricClouds.shapeFactor.value = LerpFloat(
                        m_startingData.m_volumetricShapeFactor.Evaluate(m_lastTimeOfDayValue),
                        m_volumetricShapeFactor.Evaluate(time), duration);
                    components.m_timeOfDayVolumeComponenets.m_volumetricClouds.shapeScale.value = LerpFloat(
                        m_startingData.m_volumetricShapeScale.Evaluate(m_lastTimeOfDayValue),
                        m_volumetricShapeScale.Evaluate(time), duration);
                    components.m_timeOfDayVolumeComponenets.m_volumetricClouds.erosionFactor.value = LerpFloat(
                        m_startingData.m_volumetricErosionFactor.Evaluate(m_lastTimeOfDayValue),
                        m_volumetricErosionFactor.Evaluate(time), duration);
                    components.m_timeOfDayVolumeComponenets.m_volumetricClouds.erosionScale.value = LerpFloat(
                        m_startingData.m_volumetricErosionScale.Evaluate(m_lastTimeOfDayValue),
                        m_volumetricErosionScale.Evaluate(time), duration);
#if UNITY_2022_2_OR_NEWER
                    components.m_timeOfDayVolumeComponenets.m_volumetricClouds.densityCurve.value =
                        m_volumetricDensityCurve;
                    components.m_timeOfDayVolumeComponenets.m_volumetricClouds.bottomAltitude.value = LerpFloat(
                        m_startingData.m_volumetricLowestCloudAltitude.Evaluate(m_lastTimeOfDayValue),
                        m_volumetricLowestCloudAltitude.Evaluate(time) * m_volumetricCloudHeightMultiplier, duration);
                    components.m_timeOfDayVolumeComponenets.m_volumetricClouds.altitudeRange.value = LerpFloat(
                        m_startingData.m_volumetricCloudThickness.Evaluate(m_lastTimeOfDayValue),
                        m_volumetricCloudThickness.Evaluate(time), duration);
#else
                    components.m_timeOfDayVolumeComponenets.m_volumetricClouds.customDensityCurve.value =
     m_volumetricDensityCurve;
                    components.m_timeOfDayVolumeComponenets.m_volumetricClouds.lowestCloudAltitude.value =
     LerpFloat(m_startingData.m_volumetricLowestCloudAltitude.Evaluate(m_lastTimeOfDayValue), m_volumetricLowestCloudAltitude.Evaluate(time) * m_volumetricCloudHeightMultiplier, duration);
                    components.m_timeOfDayVolumeComponenets.m_volumetricClouds.cloudThickness.value =
     LerpFloat(m_startingData.m_volumetricCloudThickness.Evaluate(m_lastTimeOfDayValue), m_volumetricCloudThickness.Evaluate(time), duration);
#endif
                }
                else
                {
#if HDRPTIMEOFDAY
                    VolumetricCloudPreset.Apply(components.m_timeOfDayVolumeComponenets.m_volumetricClouds);
                    VolumetricCloudPreset.ApplyWind(components.m_timeOfDayVolumeComponenets.m_visualEnvironment, time);
#endif
                }

                components.m_timeOfDayVolumeComponenets.m_volumetricClouds.ambientLightProbeDimmer.value = LerpFloat(m_startingData.m_volumetricAmbientLightProbeDimmer.Evaluate(m_lastTimeOfDayValue), m_volumetricAmbientLightProbeDimmer.Evaluate(time), duration);
                components.m_timeOfDayVolumeComponenets.m_volumetricClouds.sunLightDimmer.value = LerpFloat(m_startingData.m_volumetricSunLightDimmer.Evaluate(m_lastTimeOfDayValue), m_volumetricSunLightDimmer.Evaluate(time), duration);
                components.m_timeOfDayVolumeComponenets.m_volumetricClouds.erosionOcclusion.value = LerpFloat(m_startingData.m_volumetricErosionOcclusion.Evaluate(m_lastTimeOfDayValue), m_volumetricErosionOcclusion.Evaluate(time), duration);
                components.m_timeOfDayVolumeComponenets.m_volumetricClouds.scatteringTint.value = LerpColor(m_startingData.m_volumetricScatteringTint.Evaluate(m_lastTimeOfDayValue), m_volumetricScatteringTint.Evaluate(time), duration);
                components.m_timeOfDayVolumeComponenets.m_volumetricClouds.powderEffectIntensity.value = LerpFloat(m_startingData.m_volumetricPowderEffectIntensity.Evaluate(m_lastTimeOfDayValue), m_volumetricPowderEffectIntensity.Evaluate(time), duration);
                components.m_timeOfDayVolumeComponenets.m_volumetricClouds.multiScattering.value = LerpFloat(m_startingData.m_volumetricMultiScattering.Evaluate(m_lastTimeOfDayValue), m_volumetricMultiScattering.Evaluate(time), duration);
                components.m_timeOfDayVolumeComponenets.m_volumetricClouds.shadowOpacity.value = LerpFloat(m_startingData.m_volumetricCloudShadowOpacity.Evaluate(m_lastTimeOfDayValue), m_volumetricCloudShadowOpacity.Evaluate(time), duration);

                //Procedural
                components.m_timeOfDayVolumeComponenets.m_cloudLayer.opacity.value = LerpFloat(m_startingData.m_cloudOpacity.Evaluate(m_lastTimeOfDayValue), m_cloudOpacity.Evaluate(time), duration);
                components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerA.tint.value = Color.white;
                components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerB.tint.value = Color.white;
                components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerA.exposure.value = LerpFloat(m_startingData.m_cloudExposure.Evaluate(m_lastTimeOfDayValue), m_cloudExposure.Evaluate(time), duration);
                components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerB.exposure.value = LerpFloat(m_startingData.m_cloudExposure.Evaluate(m_lastTimeOfDayValue), m_cloudExposure.Evaluate(time), duration);
                components.m_timeOfDayVolumeComponenets.m_cloudLayer.shadowMultiplier.value = LerpFloat(m_startingData.m_cloudShadowOpacity.Evaluate(m_lastTimeOfDayValue), m_cloudShadowOpacity.Evaluate(time), duration);
                components.m_timeOfDayVolumeComponenets.m_cloudLayer.shadowTint.value = LerpColor(m_startingData.m_cloudShadowColor.Evaluate(m_lastTimeOfDayValue), m_cloudShadowColor.Evaluate(time), duration);

                switch (m_cloudLayerAChannel)
                {
                    case CloudLayerChannelMode.R:
                        {
                            components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerA.opacityR.value = LerpFloat(m_startingData.m_cloudLayerAOpacityR.Evaluate(m_lastTimeOfDayValue), m_cloudLayerAOpacityR.Evaluate(time), duration);
                            components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerA.opacityG.value = 0f;
                            components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerA.opacityB.value = 0f;
                            components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerA.opacityA.value = 0f;
                            break;
                        }
                    case CloudLayerChannelMode.G:
                        {
                            components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerA.opacityG.value = LerpFloat(m_startingData.m_cloudLayerAOpacityR.Evaluate(m_lastTimeOfDayValue), m_cloudLayerAOpacityR.Evaluate(time), duration);
                            components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerA.opacityR.value = 0f;
                            components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerA.opacityB.value = 0f;
                            components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerA.opacityA.value = 0f;
                            break;
                        }
                    case CloudLayerChannelMode.B:
                        {
                            components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerA.opacityB.value = LerpFloat(m_startingData.m_cloudLayerAOpacityR.Evaluate(m_lastTimeOfDayValue), m_cloudLayerAOpacityR.Evaluate(time), duration);
                            components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerA.opacityR.value = 0f;
                            components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerA.opacityG.value = 0f;
                            components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerA.opacityA.value = 0f;
                            break;
                        }
                    case CloudLayerChannelMode.A:
                        {
                            components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerA.opacityA.value = LerpFloat(m_startingData.m_cloudLayerAOpacityR.Evaluate(m_lastTimeOfDayValue), m_cloudLayerAOpacityR.Evaluate(time), duration);
                            components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerA.opacityR.value = 0f;
                            components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerA.opacityG.value = 0f;
                            components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerA.opacityB.value = 0f;
                            break;
                        }
                }
                switch (m_cloudLayerBChannel)
                {
                    case CloudLayerChannelMode.R:
                        {
                            components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerB.opacityR.value = LerpFloat(m_startingData.m_cloudLayerBOpacityR.Evaluate(m_lastTimeOfDayValue), m_cloudLayerBOpacityR.Evaluate(time), duration);
                            components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerB.opacityG.value = 0f;
                            components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerB.opacityB.value = 0f;
                            components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerB.opacityA.value = 0f;
                            break;
                        }
                    case CloudLayerChannelMode.G:
                        {
                            components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerB.opacityG.value = LerpFloat(m_startingData.m_cloudLayerBOpacityR.Evaluate(m_lastTimeOfDayValue), m_cloudLayerBOpacityR.Evaluate(time), duration);
                            components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerB.opacityR.value = 0f;
                            components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerB.opacityB.value = 0f;
                            components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerB.opacityA.value = 0f;
                            break;
                        }
                    case CloudLayerChannelMode.B:
                        {
                            components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerB.opacityB.value = LerpFloat(m_startingData.m_cloudLayerBOpacityR.Evaluate(m_lastTimeOfDayValue), m_cloudLayerBOpacityR.Evaluate(time), duration);
                            components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerB.opacityR.value = 0f;
                            components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerB.opacityG.value = 0f;
                            components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerB.opacityA.value = 0f;
                            break;
                        }
                    case CloudLayerChannelMode.A:
                        {
                            components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerB.opacityA.value = LerpFloat(m_startingData.m_cloudLayerBOpacityR.Evaluate(m_lastTimeOfDayValue), m_cloudLayerBOpacityR.Evaluate(time), duration);
                            components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerB.opacityR.value = 0f;
                            components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerB.opacityG.value = 0f;
                            components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerB.opacityB.value = 0f;
                            break;
                        }
                }
            }

            //Sun Flare
            if (ValidateSunLensFlare())
            {
                components.m_sunLensFlare.intensity = LerpFloat(m_startingData.m_sunLensFlareProfile.m_intensity.Evaluate(m_lastTimeOfDayValue), m_sunLensFlareProfile.m_intensity.Evaluate(time), duration);
                components.m_sunLensFlare.scale = LerpFloat(m_startingData.m_sunLensFlareProfile.m_scale.Evaluate(m_lastTimeOfDayValue), m_sunLensFlareProfile.m_scale.Evaluate(time), duration);
            }
            //Moon Flare
            if (ValidateMoonLensFlare())
            {
                components.m_moonLensFlare.intensity = LerpFloat(m_startingData.m_moonLensFlareProfile.m_intensity.Evaluate(m_lastTimeOfDayValue), m_moonLensFlareProfile.m_intensity.Evaluate(time), duration);
                components.m_moonLensFlare.scale = LerpFloat(m_startingData.m_moonLensFlareProfile.m_scale.Evaluate(m_lastTimeOfDayValue), m_moonLensFlareProfile.m_scale.Evaluate(time), duration);
            }

            return false;
        }
#endif
        /// <summary>
        /// Gets the divider value if you are not using SSGI to conpensate for it in the ambient lighting
        /// </summary>
        /// <returns></returns>
#if HDRPTIMEOFDAY
        public float GetNonSSGIAmbientDivider(bool isDay, bool useRayTracing, RayTraceSettings rayTraceSettings)
        {
            if (useRayTracing && rayTraceSettings.m_rayTraceSSGI)
            {
#if UNITY_2022_2_OR_NEWER

                if (rayTraceSettings.RTGlobalQualitySettings.CurrentQuality != RayTracingOverallQuality.Quality)
                {
                    if (rayTraceSettings.RTGlobalQualitySettings.CurrentQuality == RayTracingOverallQuality.Performance)
                    {
                        return isDay ? 0.45f : 0.5f;
                    }
                    else
                    {
                        return isDay ? 0.625f : 0.5f;
                    }
                }
                else
                {
                    return isDay ? 1.25f : 1f;
                }
#else
                return 1.25f;
#endif
            }
            else if (m_useNonSSGIAmbientCorrection && !m_useSSGI)
            {
                return isDay ? 0.45f : 1.4f;
            }
            else
            {
                if (m_ambientSSGICompensation)
                {
                    if (m_compensationModeAmbient == CompensationMode.Both)
                    {
                        if (isDay)
                        {
                            return m_useSSGI ? m_ambientCompensationAmount : m_nonSSGIAmbientIntensityDivider;
                        }
                        else
                        {
                            return m_useSSGI ? m_ambientCompensationAmount * m_nonSSGIAmbientIntensityDivider : m_nonSSGIAmbientIntensityDivider;
                        }
                    }
                    else if (m_compensationModeAmbient == CompensationMode.DayOnly)
                    {
                        if (isDay)
                        {
#if UNITY_2022_2_OR_NEWER
                            if (rayTraceSettings.m_rayTraceAmbientOcclusion)
                            {
                                return m_useSSGI ? m_ambientCompensationAmount / 4f : m_nonSSGIAmbientIntensityDivider;
                            }
                            else
                            {
                                return m_useSSGI ? m_ambientCompensationAmount : m_nonSSGIAmbientIntensityDivider;
                            }
#else
                            return m_useSSGI ? m_ambientCompensationAmount : m_nonSSGIAmbientIntensityDivider;
#endif
                        }
                        else
                        {
                            return m_useSSGI ? 1f : m_nonSSGIAmbientIntensityDivider;
                        }
                    }
                    else
                    {
                        if (!isDay)
                        {
                            return m_useSSGI ? m_ambientCompensationAmount : m_nonSSGIAmbientIntensityDivider;
                        }
                        else
                        {
                            return m_useSSGI ? 1f : m_nonSSGIAmbientIntensityDivider;
                        }
                    }
                }
                else
                {
                    return 1f;
                }
            }
        }
#endif
        /// <summary>
        /// Sets up the time of day to allow transitions
        /// </summary>
        /// <param name="startingData"></param>
        /// <param name="currentTimeOfDayValue"></param>
        public void SetupStartingSettings(TimeOfDayProfileData startingData, float currentTimeOfDayValue)
        {
            m_startingData = startingData;
            m_lastTimeOfDayValue = currentTimeOfDayValue;
            m_hasBeenSetup = startingData != null && m_lastTimeOfDayValue != -1;
        }
        /// <summary>
        /// Resets the setup for this profile
        /// </summary>
        public void Reset()
        {
            m_startingData = null;
            m_hasBeenSetup = false;
            m_lastTimeOfDayValue = -1f;
        }
        /// <summary>
        /// Applies Exposure
        /// </summary>
        /// <param name="components"></param>
        /// <param name="isDay"></param>
        public void ApplyExposure(HDRPTimeOfDayComponents components, bool isDay, float time, float duration, OverrideDataInfo overrideData = null, bool isReturningWeather = false)
        {
            bool transitionComplete = true;
            if (overrideData != null)
            {
                transitionComplete = overrideData.m_transitionTime < 1f;
            }
            switch (m_exposureMode)
            {
                case TimeOfDayExposureMode.Automatic:
                {
                    float darkerNightMin = 0f;
                    if (m_darkerNightSettings.m_darkerNights)
                    {
                        darkerNightMin = isDay ? 0f : m_darkerNightSettings.m_automaticExposureAdjustmentValue;
                    }
                    float currentMin = components.m_timeOfDayVolumeComponenets.m_exposure.limitMin.value;
                    float currentMax = components.m_timeOfDayVolumeComponenets.m_exposure.limitMax.value;
                    float newMin = m_autoExposureSettings.m_mixMaxLimit.x;
                    float newMax = m_autoExposureSettings.m_mixMaxLimit.y;
                    components.m_timeOfDayVolumeComponenets.m_exposure.mode.value = ExposureMode.AutomaticHistogram;
                    if (m_autoExposureSettings.m_useSmartTransition)
                    {
                        newMin = isDay ? m_autoExposureSettings.m_mixMaxLimit.x : m_autoExposureSettings.m_mixMaxLimitNight.x;           
                        newMax = isDay ? m_autoExposureSettings.m_mixMaxLimit.y : m_autoExposureSettings.m_mixMaxLimitNight.y;                 
                        if (!isDay)
                        {
                            newMin += HDRPTimeOfDay.Instance.WeatherActive() ? 1f : 0f;
                            newMax += HDRPTimeOfDay.Instance.WeatherActive() ? 1f : 0f;
                        }

                        newMin -= darkerNightMin;
                        newMax -= darkerNightMin;
                        if (currentMin != newMin || currentMax != newMax)
                        {
                            HDRPTimeOfDay.Instance.StartExposureMinLerp(currentMin, newMin, currentMax, newMax, isDay ? m_autoExposureSettings.m_transitionDayDuration : m_autoExposureSettings.m_transitionNightDuration);
                        }
                    }
                    else
                    {
                        components.m_timeOfDayVolumeComponenets.m_exposure.limitMin.value = newMin;
                        components.m_timeOfDayVolumeComponenets.m_exposure.limitMax.value = newMax;
                    }

                    components.m_timeOfDayVolumeComponenets.m_exposure.histogramPercentages.value = m_autoExposureSettings.m_histogramPercentage;
                    components.m_timeOfDayVolumeComponenets.m_exposure.adaptationSpeedDarkToLight.value = m_autoExposureSettings.m_adaptionSpeed.x;
                    components.m_timeOfDayVolumeComponenets.m_exposure.adaptationSpeedLightToDark.value = m_autoExposureSettings.m_adaptionSpeed.y;

                    //Override compensation for volumes
                    if (!overrideData.m_isInVolue)
                    {
                        if (transitionComplete)
                        {
                            components.m_timeOfDayVolumeComponenets.m_exposure.compensation.value = Mathf.Lerp(components.m_timeOfDayVolumeComponenets.m_exposure.compensation.value, 0f, overrideData.m_transitionTime);
                        }
                        else
                        {
                            components.m_timeOfDayVolumeComponenets.m_exposure.compensation.value = 0f;
                        }
                    }
                    else
                    {
                        if (overrideData.m_settings != null)
                        {
                            if (overrideData.m_settings.m_exposure.overrideState)
                            {
                                if (transitionComplete)
                                {
                                    components.m_timeOfDayVolumeComponenets.m_exposure.compensation.value = Mathf.Lerp(components.m_timeOfDayVolumeComponenets.m_exposure.compensation.value, overrideData.m_settings.m_exposure.value, overrideData.m_transitionTime);
                                }
                                else
                                {
                                    components.m_timeOfDayVolumeComponenets.m_exposure.compensation.value = overrideData.m_settings.m_exposure.value;
                                }
                            }
                            else
                            {
                                if (transitionComplete)
                                {
                                    components.m_timeOfDayVolumeComponenets.m_exposure.compensation.value = Mathf.Lerp(components.m_timeOfDayVolumeComponenets.m_exposure.compensation.value, 0f, overrideData.m_transitionTime);
                                }
                                else
                                {
                                    components.m_timeOfDayVolumeComponenets.m_exposure.compensation.value = 0f;
                                }
                            }
                        }
                    }
                    break;
                }
                case TimeOfDayExposureMode.Fixed:
                {
                    float darkerNightMin = 0f;
                    if (m_darkerNightSettings.m_darkerNights)
                    {
                        darkerNightMin = isDay ? 0f : m_darkerNightSettings.m_fixedExposureAdjustmentValue;
                    }
                    components.m_timeOfDayVolumeComponenets.m_exposure.mode.value = ExposureMode.Fixed;
                    if (isReturningWeather)
                    {
                        components.m_timeOfDayVolumeComponenets.m_exposure.fixedExposure.value = LerpFloat(m_startingData.m_generalExposure.Evaluate(m_lastTimeOfDayValue), m_generalExposure.Evaluate(time) - darkerNightMin, duration);
                    }
                    else
                    {
                        if (!overrideData.m_isInVolue)
                        {
                            if (transitionComplete)
                            {
                                components.m_timeOfDayVolumeComponenets.m_exposure.fixedExposure.value = Mathf.Lerp(components.m_timeOfDayVolumeComponenets.m_exposure.fixedExposure.value, m_generalExposure.Evaluate(time) - darkerNightMin, overrideData.m_transitionTime);
                            }
                            else
                            {
                                components.m_timeOfDayVolumeComponenets.m_exposure.fixedExposure.value = m_generalExposure.Evaluate(time) - darkerNightMin;
                            }
                        }
                        else
                        {
                            if (overrideData.m_settings != null)
                            {
                                if (overrideData.m_settings.m_exposure.overrideState)
                                {
                                    if (transitionComplete)
                                    {
                                        components.m_timeOfDayVolumeComponenets.m_exposure.fixedExposure.value = Mathf.Lerp(components.m_timeOfDayVolumeComponenets.m_exposure.fixedExposure.value, overrideData.m_settings.m_exposure.value, overrideData.m_transitionTime);
                                    }
                                    else
                                    {
                                        components.m_timeOfDayVolumeComponenets.m_exposure.fixedExposure.value = overrideData.m_settings.m_exposure.value;
                                    }
                                }
                                else
                                {
                                    if (transitionComplete)
                                    {
                                        components.m_timeOfDayVolumeComponenets.m_exposure.fixedExposure.value = Mathf.Lerp(components.m_timeOfDayVolumeComponenets.m_exposure.fixedExposure.value, m_generalExposure.Evaluate(time) - darkerNightMin, overrideData.m_transitionTime);
                                    }
                                    else
                                    {
                                        components.m_timeOfDayVolumeComponenets.m_exposure.fixedExposure.value = m_generalExposure.Evaluate(time) - darkerNightMin;
                                    }
                                }
                            }
                        }
                    }
                    break;
                }
                case TimeOfDayExposureMode.Physical:
                {
                    float darkerNightMin = 0f;
                    if (m_darkerNightSettings.m_darkerNights)
                    {
                        darkerNightMin = isDay ? 0f : m_darkerNightSettings.m_physicalExposureAdjustmentValue;
                    }
                    if (!overrideData.m_isInVolue)
                    {
                        if (transitionComplete)
                        {
                            components.m_timeOfDayVolumeComponenets.m_exposure.compensation.value = Mathf.Lerp(components.m_timeOfDayVolumeComponenets.m_exposure.compensation.value, 0f, overrideData.m_transitionTime);
                        }
                        else
                        {
                            components.m_timeOfDayVolumeComponenets.m_exposure.compensation.value = 0f;
                        }
                    }
                    else
                    {
                        if (overrideData.m_settings != null)
                        {
                            if (overrideData.m_settings.m_exposure.overrideState)
                            {
                                if (transitionComplete)
                                {
                                    components.m_timeOfDayVolumeComponenets.m_exposure.compensation.value = Mathf.Lerp(components.m_timeOfDayVolumeComponenets.m_exposure.compensation.value, overrideData.m_settings.m_exposure.value, overrideData.m_transitionTime);
                                }
                                else
                                {
                                    components.m_timeOfDayVolumeComponenets.m_exposure.compensation.value = overrideData.m_settings.m_exposure.value;
                                }
                            }
                            else
                            {
                                if (transitionComplete)
                                {
                                    components.m_timeOfDayVolumeComponenets.m_exposure.compensation.value = Mathf.Lerp(components.m_timeOfDayVolumeComponenets.m_exposure.compensation.value, 0f, overrideData.m_transitionTime);
                                }
                                else
                                {
                                    components.m_timeOfDayVolumeComponenets.m_exposure.compensation.value = 0f;
                                }
                            }
                        }
                    }

                    components.m_timeOfDayVolumeComponenets.m_exposure.mode.value = ExposureMode.UsePhysicalCamera;
                    if (!components.m_camera.usePhysicalProperties)
                    {
                        float fov = components.m_camera.fieldOfView;
                        components.m_camera.usePhysicalProperties = true;
                        components.m_camera.fieldOfView = fov;
                    }

#if UNITY_2022_2_OR_NEWER
                    components.m_camera.iso = m_physicalExposureSettings.m_iso;
                    components.m_camera.shutterSpeed = m_physicalExposureSettings.m_shutterSpeed;
                    components.m_camera.aperture = m_physicalExposureSettings.m_aperture.Evaluate(time) - darkerNightMin;
#else
                    components.m_cameraData.physicalParameters.iso = m_physicalExposureSettings.m_iso;
                    components.m_cameraData.physicalParameters.shutterSpeed = m_physicalExposureSettings.m_shutterSpeed;
                    components.m_cameraData.physicalParameters.aperture = m_physicalExposureSettings.m_aperture.Evaluate(time) - darkerNightMin;
#endif
#if TOD_CINEMACHINE
                    if (components.m_cinemachineVirtualCameras.Count > 0)
                    {
                        for (int i = 0; i < components.m_cinemachineVirtualCameras.Count; i++)
                        {
                            CinemachineVirtualCamera vCamera = components.m_cinemachineVirtualCameras[i];
                            if (vCamera != null)
                            {
                                vCamera.m_Lens.Iso = m_physicalExposureSettings.m_iso;
                                vCamera.m_Lens.ShutterSpeed = m_physicalExposureSettings.m_shutterSpeed;
                                vCamera.m_Lens.Aperture = m_physicalExposureSettings.m_aperture.Evaluate(time) - darkerNightMin;
                            }
                        }
                    }
#endif
                    break;
                }
            }
        }
        /// <summary>
        /// Copies all the settings to another profile
        /// </summary>
        /// <param name="copyTo"></param>
        public static void CopySettings(TimeOfDayProfileData copyTo, TimeOfDayProfileData copyFrom)
        {
            if (copyTo != null && copyFrom != null)
            {
                //Sun
                copyTo.m_enableSunShadows = copyFrom.m_enableSunShadows;
                copyTo.m_enableMoonShadows = copyFrom.m_enableMoonShadows;
                copyTo.m_sunIntensity = CopyCurve(copyFrom.m_sunIntensity);
                copyTo.m_moonIntensity = CopyCurve(copyFrom.m_moonIntensity);
                copyTo.m_sunIntensityMultiplier = CopyCurve(copyFrom.m_sunIntensityMultiplier);
                copyTo.m_sunTemperature = CopyCurve(copyFrom.m_sunTemperature);
                copyTo.m_sunColorFilter = CopyGradient(copyFrom.m_sunColorFilter);
                copyTo.m_moonTemperature = CopyCurve(copyFrom.m_moonTemperature);
                copyTo.m_moonColorFilter = CopyGradient(copyFrom.m_moonColorFilter);
                copyTo.m_sunVolumetrics = CopyCurve(copyFrom.m_sunVolumetrics);
                copyTo.m_sunVolumetricShadowDimmer = CopyCurve(copyFrom.m_sunVolumetricShadowDimmer);
                copyTo.m_sunShadowDimmer = CopyCurve(copyFrom.m_sunShadowDimmer);
                copyTo.m_moonShadowDimmer = CopyCurve(copyFrom.m_moonShadowDimmer);
                copyTo.m_globalLightMultiplier = copyFrom.m_globalLightMultiplier;
                //Sky
                copyTo.m_horizonOffset = copyFrom.m_horizonOffset;
                copyTo.m_skyMode = copyFrom.m_skyMode;
                copyTo.m_skyboxExposure = copyFrom.m_skyboxExposure;
                copyTo.m_skyboxGroundColor = copyFrom.m_skyboxGroundColor;
                copyTo.m_rotateStars = copyFrom.m_rotateStars;
                copyTo.m_starIntensity = CopyCurve(copyFrom.m_starIntensity);
                copyTo.m_resetStarsRotationOnEnable = copyFrom.m_resetStarsRotationOnEnable;
                copyTo.m_starsRotationSpeed = copyFrom.m_starsRotationSpeed;
                //Shadows
                copyTo.m_shadowDistance = CopyCurve(copyFrom.m_shadowDistance);
                copyTo.m_shadowTransmissionMultiplier = CopyCurve(copyFrom.m_shadowTransmissionMultiplier);
                copyTo.m_shadowCascadeCount = copyFrom.m_shadowCascadeCount;
                copyTo.m_shadowDistanceMultiplier = copyFrom.m_shadowDistanceMultiplier;
                copyTo.m_shadowDistanceMultiplierNight = copyFrom.m_shadowDistanceMultiplierNight;
                copyTo.m_shadowCascadeFadeLength = copyFrom.m_shadowCascadeFadeLength;
                //Fog
                copyTo.m_useFog = copyFrom.m_useFog;
                copyTo.m_enableVolumetricFog = copyFrom.m_enableVolumetricFog;
                copyTo.m_fogQuality = copyFrom.m_fogQuality;
                copyTo.m_useDenoising = copyFrom.m_useDenoising;
                copyTo.m_denoisingQuality = copyFrom.m_denoisingQuality;
                copyTo.m_fogColor = CopyGradient(copyFrom.m_fogColor);
                copyTo.m_fogDistance = CopyCurve(copyFrom.m_fogDistance);
                copyTo.m_fogDensity = CopyCurve(copyFrom.m_fogDensity);
                copyTo.m_fogHeight = CopyCurve(copyFrom.m_fogHeight);
                copyTo.m_volumetricFogDistance = CopyCurve(copyFrom.m_volumetricFogDistance);
                copyTo.m_volumetricFogAnisotropy = CopyCurve(copyFrom.m_volumetricFogAnisotropy);
                copyTo.m_useFogAnisotropyOverride = copyFrom.m_useFogAnisotropyOverride;
                copyTo.m_volumetricFogSliceDistributionUniformity = CopyCurve(copyFrom.m_volumetricFogSliceDistributionUniformity);
                copyTo.m_localFogMultiplier = CopyCurve(copyFrom.m_localFogMultiplier);
                copyTo.m_globalFogMultiplier = copyFrom.m_globalFogMultiplier;
                copyTo.m_globalFogHeightMultiplier = copyFrom.m_globalFogHeightMultiplier;
                copyTo.m_globalFogDistanceMultiplier = copyFrom.m_globalFogDistanceMultiplier;
                copyTo.m_localFogSize = copyFrom.m_localFogSize;
                //Clouds Volumetric
                copyTo.m_cloudPresets = copyFrom.m_cloudPresets;
                copyTo.m_volumetricDensityMultiplier = CopyCurve(copyFrom.m_volumetricDensityMultiplier);
                copyTo.m_volumetricDensityCurve = CopyCurve(copyFrom.m_volumetricDensityCurve);
                copyTo.m_volumetricShapeFactor = CopyCurve(copyFrom.m_volumetricShapeFactor);
                copyTo.m_volumetricShapeScale = CopyCurve(copyFrom.m_volumetricShapeScale);
                copyTo.m_volumetricErosionFactor = CopyCurve(copyFrom.m_volumetricErosionFactor);
                copyTo.m_volumetricErosionScale = CopyCurve(copyFrom.m_volumetricErosionScale);
                copyTo.m_erosionNoiseType = copyFrom.m_erosionNoiseType;
                copyTo.m_volumetricErosionCurve = CopyCurve(copyFrom.m_volumetricErosionCurve);
                copyTo.m_volumetricAmbientOcclusionCurve = CopyCurve(copyFrom.m_volumetricAmbientOcclusionCurve);
                copyTo.m_volumetricLowestCloudAltitude = CopyCurve(copyFrom.m_volumetricLowestCloudAltitude);
                copyTo.m_volumetricCloudThickness = CopyCurve(copyFrom.m_volumetricCloudThickness);
                copyTo.m_volumetricAmbientLightProbeDimmer = CopyCurve(copyFrom.m_volumetricAmbientLightProbeDimmer);
                copyTo.m_volumetricSunLightDimmer = CopyCurve(copyFrom.m_volumetricSunLightDimmer);
                copyTo.m_volumetricErosionOcclusion = CopyCurve(copyFrom.m_volumetricErosionOcclusion);
                copyTo.m_volumetricScatteringTint = CopyGradient(copyFrom.m_volumetricScatteringTint);
                copyTo.m_volumetricPowderEffectIntensity = CopyCurve(copyFrom.m_volumetricPowderEffectIntensity);
                copyTo.m_volumetricMultiScattering = CopyCurve(copyFrom.m_volumetricMultiScattering);
                copyTo.m_volumetricCloudShadows = copyFrom.m_volumetricCloudShadows;
                copyTo.m_volumetricCloudShadowResolution = copyFrom.m_volumetricCloudShadowResolution;
                copyTo.m_volumetricCloudShadowOpacity = CopyCurve(copyFrom.m_volumetricCloudShadowOpacity);
                copyTo.m_volumetricCloudHeightMultiplier = copyFrom.m_volumetricCloudHeightMultiplier;
                //Cloud Procedural
                copyTo.m_cloudLayers = copyFrom.m_cloudLayers;
                copyTo.m_cloudResolution = copyFrom.m_cloudResolution;
                copyTo.m_useCloudShadows = copyFrom.m_useCloudShadows;
                copyTo.m_cloudOpacity = CopyCurve(copyFrom.m_cloudOpacity);
                copyTo.m_cloudTintColor = CopyGradient(copyFrom.m_cloudTintColor);
                copyTo.m_cloudExposure = CopyCurve(copyFrom.m_cloudExposure);
                copyTo.m_cloudWindDirection = CopyCurve(copyFrom.m_cloudWindDirection);
                copyTo.m_cloudWindSpeed = CopyCurve(copyFrom.m_cloudWindSpeed);
                copyTo.m_cloudShadowOpacity = CopyCurve(copyFrom.m_cloudShadowOpacity);
                copyTo.m_cloudLighting = copyFrom.m_cloudLighting;
                copyTo.m_cloudShadowColor = CopyGradient(copyFrom.m_cloudShadowColor);
                copyTo.m_cloudLayerAChannel = copyFrom.m_cloudLayerAChannel;
                copyTo.m_cloudLayerAOpacityR = CopyCurve(copyFrom.m_cloudLayerAOpacityR);
                copyTo.m_cloudLayerAOpacityG = CopyCurve(copyFrom.m_cloudLayerAOpacityG);
                copyTo.m_cloudLayerAOpacityB = CopyCurve(copyFrom.m_cloudLayerAOpacityB);
                copyTo.m_cloudLayerAOpacityA = CopyCurve(copyFrom.m_cloudLayerAOpacityA);
                copyTo.m_cloudLayerBChannel = copyFrom.m_cloudLayerBChannel;
                copyTo.m_cloudLayerBOpacityR = CopyCurve(copyFrom.m_cloudLayerBOpacityR);
                copyTo.m_cloudLayerBOpacityG = CopyCurve(copyFrom.m_cloudLayerBOpacityG);
                copyTo.m_cloudLayerBOpacityB = CopyCurve(copyFrom.m_cloudLayerBOpacityB);
                copyTo.m_cloudLayerBOpacityA = CopyCurve(copyFrom.m_cloudLayerBOpacityA);
                //Advanced Lighting
                copyTo.m_useSSGI = copyFrom.m_useSSGI;
                copyTo.m_ambientSSGICompensation = copyFrom.m_ambientSSGICompensation;
                copyTo.m_reflectionAmbientSSGICompensation = copyFrom.m_reflectionAmbientSSGICompensation;
                copyTo.m_ambientCompensationExposure = copyFrom.m_ambientCompensationExposure;
                copyTo.m_compensationMode = copyFrom.m_compensationMode;
                copyTo.m_ssgiQuality = copyFrom.m_ssgiQuality;
                copyTo.m_ssgiRenderMode = copyFrom.m_ssgiRenderMode;
                copyTo.m_useSSR = copyFrom.m_useSSR;
                copyTo.m_ssrQuality = copyFrom.m_ssrQuality;
                copyTo.m_useContactShadows = copyFrom.m_useContactShadows;
                copyTo.m_useAdvancedNearFarContactShadows = copyFrom.m_useAdvancedNearFarContactShadows;
                copyTo.m_nearThresholdPercentage = copyFrom.m_nearThresholdPercentage;
                copyTo.m_farShadowPercentageBlend = copyFrom.m_farShadowPercentageBlend;
                copyTo.m_groundLayer = copyFrom.m_groundLayer;
                copyTo.m_contactShadowsDistance = CopyCurve(copyFrom.m_contactShadowsDistance);
                copyTo.m_nearContactShadowsDistance = CopyCurve(copyFrom.m_nearContactShadowsDistance);
                copyTo.m_contactShadowsOpacity = CopyCurve(copyFrom.m_contactShadowsOpacity);
                copyTo.m_useMicroShadows = copyFrom.m_useMicroShadows;
                copyTo.m_microShadowOpacity = CopyCurve(copyFrom.m_microShadowOpacity);
                copyTo.m_exposureMode = copyFrom.m_exposureMode;
                copyTo.m_generalExposure = CopyCurve(copyFrom.m_generalExposure);
                copyFrom.m_autoExposureSettings.CopySettings(ref copyTo.m_autoExposureSettings);
                copyTo.m_ambientIntensityMultiplier = copyFrom.m_ambientIntensityMultiplier;
                copyTo.m_ambientIntensity = CopyCurve(copyFrom.m_ambientIntensity);
                copyTo.m_ambientReflectionIntensity = CopyCurve(copyFrom.m_ambientReflectionIntensity);
                copyTo.m_planarReflectionIntensity = CopyCurve(copyFrom.m_planarReflectionIntensity);
                copyFrom.m_sunLensFlareProfile.CopySettings(ref copyTo.m_sunLensFlareProfile);
                copyFrom.m_moonLensFlareProfile.CopySettings(ref copyTo.m_moonLensFlareProfile);
            }
        }
        /// <summary>
        /// Copies animation curve into a new animation curve field
        /// </summary>
        /// <param name="curve"></param>
        /// <returns></returns>
        public static AnimationCurve CopyCurve(AnimationCurve curve)
        {
            if (curve != null)
            {
                Keyframe[] keyFrames = curve.keys;
                AnimationCurve newAnimationCurve = new AnimationCurve();
                foreach (Keyframe keyframe in keyFrames)
                {
                    newAnimationCurve.AddKey(keyframe);
                }

                return newAnimationCurve;
            }
            return new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 1f));
        }
        /// <summary>
        /// Copies gradient into a new gradient field
        /// </summary>
        /// <param name="gradient"></param>
        /// <returns></returns>
        public static Gradient CopyGradient(Gradient gradient)
        {
            if (gradient != null)
            {
                GradientAlphaKey[] alphaKeys = gradient.alphaKeys;
                GradientColorKey[] colorKeys = gradient.colorKeys;
                Gradient newGradient = new Gradient();
                GradientAlphaKey[] newAlphaKeys = new GradientAlphaKey[gradient.alphaKeys.Length];
                for (int i = 0; i < gradient.alphaKeys.Length; i++)
                {
                    newAlphaKeys[i].time = alphaKeys[i].time;
                    newAlphaKeys[i].alpha = alphaKeys[i].alpha;
                }

                GradientColorKey[] newColorKeys = new GradientColorKey[gradient.colorKeys.Length];
                for (int i = 0; i < gradient.colorKeys.Length; i++)
                {
                    newColorKeys[i].time = colorKeys[i].time;
                    newColorKeys[i].color = colorKeys[i].color;
                }
                newGradient.SetKeys(newColorKeys, newAlphaKeys);
                return newGradient;
            }

            return new Gradient();
        }
        /// <summary>
        /// Gets a reflection intensity if SSGI is enabled
        /// </summary>
        /// <returns></returns>
#if HDRPTIMEOFDAY
        public float GetReflectionAmbientMultiplier(bool isDay, bool rayTacing = false, RayTraceSettings rayTraceSettings = null)
        {
            if (m_reflectionAmbientSSGICompensation && !m_useSSGI)
            {
                return isDay ? 0.85f : 1.1f;
            }
            else if (rayTacing)
            {
                if (rayTraceSettings != null)
                {
                    return rayTraceSettings.m_rayTraceSSGI ? 0.8f : 1f;
                }
            }

            return 1f;
        }
#endif

        public bool ValidateStars()
        {
            if (m_starIntensity == null)
            {
                m_starIntensity = AnimationCurve.Constant(0f, 1f, 4500);
                return false;
            }
            return true;
        }
        public void ApplyStarSettings(HDRPTimeOfDayComponents components, float time)
        {
            components.m_timeOfDayVolumeComponenets.m_physicallyBasedSky.spaceEmissionMultiplier.value = m_starIntensity.Evaluate(time);
            if (m_rotateStars)
            {
                components.m_timeOfDayVolumeComponenets.m_physicallyBasedSky.spaceRotation.value += new Vector3(0.001f, 0.002f, 0.001f);
            }
        }

        public bool ValidateSun()
        {
            if (m_sunIntensity == null)
            {
                return false;
            }
            if (m_moonIntensity == null)
            {
                return false;
            }
            if (m_sunIntensityMultiplier == null)
            {
                return false;
            }
            if (m_sunTemperature == null)
            {
                return false;
            }
            if (m_sunColorFilter == null)
            {
                return false;
            }
            if (m_moonTemperature == null)
            {
                return false;
            }
            if (m_moonColorFilter == null)
            {
                return false;
            }
            if (m_sunVolumetrics == null)
            {
                return false;
            }
            if (m_sunVolumetricShadowDimmer == null)
            {
                return false;
            }
            if (m_sunShadowDimmer == null)
            {
                m_sunShadowDimmer = AnimationCurve.Constant(0f, 1f, 1f);
                return false;
            }

            if (m_moonShadowDimmer == null)
            {
                m_moonShadowDimmer = AnimationCurve.Constant(0f, 1f, 1f);
                return false;
            }

            return true;
        }
        public void ApplySunSettings(HDAdditionalLightData lightData, float time, bool isDay, bool overrideSource, OverrideDataInfo overrideData)
        {
            lightData.EnableColorTemperature(true);
            if (!overrideSource)
            {
                float darkerNightValue = 0f;
                if (m_darkerNightSettings.m_darkerNights)
                {
                    darkerNightValue = isDay ? 0f : m_darkerNightSettings.m_moonIntensityAdjustmentValue;
                }
                if (isDay)
                {
                    lightData.SetColor(m_sunColorFilter.Evaluate(time), m_sunTemperature.Evaluate(time));
                    lightData.SetIntensity(m_sunIntensity.Evaluate(time) * m_globalLightMultiplier);
                    lightData.EnableShadows(m_enableSunShadows);
                    lightData.shadowDimmer = HDRPTimeOfDayAPI.RayTracingSSGIActive() ? 1f : Mathf.Clamp01(m_sunShadowDimmer.Evaluate(time));
                }
                else
                {
                    lightData.SetColor(m_moonColorFilter.Evaluate(time), m_moonTemperature.Evaluate(time));
                    if (m_darkerNightSettings.m_darkerNights)
                    {
                        lightData.SetIntensity((m_moonIntensity.Evaluate(time) * darkerNightValue) * m_globalLightMultiplier);
                    }
                    else
                    {
                        lightData.SetIntensity(m_moonIntensity.Evaluate(time) * m_globalLightMultiplier);
                    }
           
                    lightData.EnableShadows(m_enableMoonShadows);
                    lightData.shadowDimmer = HDRPTimeOfDayAPI.RayTracingSSGIActive() ? 1f : Mathf.Clamp01(m_moonShadowDimmer.Evaluate(time));
                }

                lightData.shadowResolution.useOverride = false;
            }

            lightData.affectsVolumetric = true;
            lightData.lightDimmer = m_sunIntensityMultiplier.Evaluate(time);
            bool transitionComplete = overrideData.m_transitionTime < 1f;
            if (!overrideData.m_isInVolue)
            {
                if (transitionComplete)
                {
                    lightData.volumetricDimmer = Mathf.Lerp(lightData.volumetricDimmer, m_sunVolumetrics.Evaluate(time), overrideData.m_transitionTime);
                    lightData.volumetricShadowDimmer = Mathf.Lerp(lightData.volumetricShadowDimmer, m_sunVolumetricShadowDimmer.Evaluate(time), overrideData.m_transitionTime);
                }
                else
                {
                    lightData.volumetricDimmer = m_sunVolumetrics.Evaluate(time);
                    lightData.volumetricShadowDimmer = m_sunVolumetricShadowDimmer.Evaluate(time);
                }
            }
            else
            {
                if (overrideData.m_settings != null)
                {
                    if (overrideData.m_settings.m_sunVolumetric.overrideState)
                    {
                        if (transitionComplete)
                        {
                            lightData.volumetricDimmer = Mathf.Lerp(lightData.volumetricDimmer, overrideData.m_settings.m_sunVolumetric.value, overrideData.m_transitionTime);
                        }
                        else
                        {
                            lightData.volumetricDimmer = overrideData.m_settings.m_sunVolumetric.value;
                        }
                    }
                    else
                    {
                        if (transitionComplete)
                        {
                            lightData.volumetricDimmer = Mathf.Lerp(lightData.volumetricDimmer, m_sunVolumetrics.Evaluate(time), overrideData.m_transitionTime);
                        }
                        else
                        {
                            lightData.volumetricDimmer = m_sunVolumetrics.Evaluate(time);
                        }
                    }

                    if (overrideData.m_settings.m_sunVolumetricDimmer.overrideState)
                    {
                        if (transitionComplete)
                        {
                            lightData.volumetricShadowDimmer = Mathf.Lerp(lightData.volumetricShadowDimmer, overrideData.m_settings.m_sunVolumetricDimmer.value, overrideData.m_transitionTime);
                        }
                        else
                        {
                            lightData.volumetricShadowDimmer = overrideData.m_settings.m_sunVolumetricDimmer.value;
                        }
                    }
                    else
                    {
                        if (transitionComplete)
                        {
                            lightData.volumetricShadowDimmer = Mathf.Lerp(lightData.volumetricShadowDimmer, m_sunVolumetricShadowDimmer.Evaluate(time), overrideData.m_transitionTime);
                        }
                        else
                        {
                            lightData.volumetricShadowDimmer = m_sunVolumetricShadowDimmer.Evaluate(time);
                        }
                    }
                }
            }
        }

        public bool ValidateTwilight()
        {
            if (m_twilightIntensity == null || m_twilightIntensity.keys.Length <= 0)
            {
                m_twilightIntensity = AnimationCurve.Constant(0f, 1f, 300f);
                return false;
            }

            if (m_twilightTemperature == null || m_twilightTemperature.keys.Length <= 0)
            {
                m_twilightTemperature = AnimationCurve.Constant(0f, 1f, 5500f);
                return false;
            }

            return true;
        }
        /// <summary>
        /// Applies the twilight settings
        /// </summary>
        /// <param name="components"></param>
        /// <param name="isDay"></param>
        /// <param name="time"></param>
        public void ApplyTwilight(HDRPTimeOfDayComponents components, bool isDay, float time, bool isInInterior)
        {
            if (ValidateTwilight())
            {
                if (m_useTwilight && !isInInterior && !HDRPTimeOfDayAPI.RayTracingActive())
                {
                    components.m_twilightLight.enabled = true;
                    if (isDay)
                    {
                        components.m_twilightLight.transform.localRotation = time >= 0.5f ? Quaternion.Euler(new Vector3(-115f, 0f, 0f)) : Quaternion.Euler(new Vector3(-75f, 0f, 0f));
                    }
                    else
                    {
                        components.m_twilightLight.transform.localRotation = Quaternion.Euler(new Vector3(105, 0f, 0f));
                    }
                    components.m_twilightLightData.SetIntensity(m_twilightIntensity.Evaluate(time));
                    Color tempColor = isDay ? components.m_sunLightData.color : components.m_moonLightData.color;
                    components.m_twilightLightData.SetColor(tempColor, m_twilightTemperature.Evaluate(time));
                }
                else
                {
                    components.m_twilightLight.enabled = false;
                }
            }
        }

        public bool ValidateAdvancedLighting()
        {
            if (m_generalExposure == null)
            {
                return false;
            }
            if (m_ambientIntensity == null)
            {
                m_ambientIntensity = AnimationCurve.Constant(0f, 1f, 1f);
                return false;
            }
            if (m_ambientReflectionIntensity == null)
            {
                m_ambientReflectionIntensity = AnimationCurve.Constant(0f, 1f, 1f);
                return false;
            }
            if (m_planarReflectionIntensity == null)
            {
                m_planarReflectionIntensity = AnimationCurve.Constant(0f, 1f, 1f);
                return false;
            }

            if (m_contactShadowsDistance == null)
            {
                m_contactShadowsDistance = AnimationCurve.Constant(0f, 1f, 10000f);
                return false;
            }

            if (m_nearContactShadowsDistance == null)
            {
                m_nearContactShadowsDistance = AnimationCurve.Constant(0f, 1f, 100f);
                return false;
            }

            if (m_contactShadowsOpacity == null)
            {
                m_contactShadowsOpacity = AnimationCurve.Constant(0f, 1f, 1f);
                return false;
            }

            if (m_microShadowOpacity == null)
            {
                m_microShadowOpacity = AnimationCurve.Constant(0f, 1f, 1f);
            }

            return true;
        }
#if HDRPTIMEOFDAY
        public void ApplyAdvancedLighting(HDRPTimeOfDayComponents components, bool rayTracing, RayTraceSettings rayTraceSettings, bool isDay, float time, OverrideDataInfo overrideData)
        {
            bool transitionComplete = overrideData.m_transitionTime < 1f;
            //SSGI
            components.m_timeOfDayVolumeComponenets.m_globalIllumination.active = true;
            components.m_timeOfDayVolumeComponenets.m_globalIllumination.enable.value = m_useSSGI;
            rayTraceSettings.RTGlobalQualitySettings.RTSSGIQuality[(int)m_ssgiQuality].Apply(components.m_timeOfDayVolumeComponenets.m_globalIllumination);
            switch (m_ssgiRenderMode)
            {
                case SSGIRenderMode.Interior:
                {
                    components.m_timeOfDayVolumeComponenets.m_globalIllumination.rayMiss.value = RayMarchingFallbackHierarchy.ReflectionProbes;
                    break;
                }
                case SSGIRenderMode.Exterior:
                {
                    components.m_timeOfDayVolumeComponenets.m_globalIllumination.rayMiss.value = RayMarchingFallbackHierarchy.Sky;
                    break;
                }
                case SSGIRenderMode.Both:
                {
                    components.m_timeOfDayVolumeComponenets.m_globalIllumination.rayMiss.value = RayMarchingFallbackHierarchy.ReflectionProbesAndSky;
                    break;
                }
            }
            //SSR
            components.m_timeOfDayVolumeComponenets.m_screenSpaceReflection.active = true;
            components.m_timeOfDayVolumeComponenets.m_screenSpaceReflection.enabled.value = m_useSSR;
            components.m_timeOfDayVolumeComponenets.m_screenSpaceReflection.enabledTransparent.value = m_useSSR;
            rayTraceSettings.RTGlobalQualitySettings.RTSSRQuality[(int)m_ssrQuality].Apply(components.m_timeOfDayVolumeComponenets.m_screenSpaceReflection, false, rayTraceSettings.RTGlobalQualitySettings.RTSceneCheck, rayTraceSettings.RTGlobalQualitySettings.SceneNames);
            //Exposure
            ApplyExposure(components, isDay, time, 0f, overrideData);
            //Shadows
            components.m_timeOfDayVolumeComponenets.m_microShadowing.active = true;
            components.m_timeOfDayVolumeComponenets.m_microShadowing.enable.value = m_useMicroShadows;
            components.m_timeOfDayVolumeComponenets.m_microShadowing.opacity.value = Mathf.Clamp01(m_microShadowOpacity.Evaluate(time));
            //Ambient
            float ambientIntensity = m_ambientIntensity.Evaluate(time);
            float overrideAmbientIntensity = ambientIntensity;
            if (overrideData.m_isInVolue)
            {
                overrideAmbientIntensity = overrideData.m_settings.m_ambientIntensity.value;
            }

            components.m_timeOfDayVolumeComponenets.m_exposure.compensation.overrideState = true;
            if (!overrideData.m_isInVolue)
            {
                if (m_useSSGI && m_ambientSSGICompensation)
                {
                    switch (m_compensationMode)
                    {
                        case CompensationMode.Both:
                        {
                            components.m_timeOfDayVolumeComponenets.m_exposure.compensation.value = m_ambientCompensationExposure;
                            break;
                        }
                        case CompensationMode.DayOnly:
                        {
                            if (isDay)
                            {
                                components.m_timeOfDayVolumeComponenets.m_exposure.compensation.value = m_ambientCompensationExposure;
                            }
                            else
                            {
                                components.m_timeOfDayVolumeComponenets.m_exposure.compensation.value = 0f;
                            }

                            break;
                        }
                        case CompensationMode.NightOnly:
                        {
                            if (!isDay)
                            {
                                components.m_timeOfDayVolumeComponenets.m_exposure.compensation.value = m_ambientCompensationExposure;
                            }
                            else
                            {
                                components.m_timeOfDayVolumeComponenets.m_exposure.compensation.value = 0f;
                            }

                            break;
                        }
                    }
                }
                else
                {
                    components.m_timeOfDayVolumeComponenets.m_exposure.compensation.value = 0f;
                }
            }

            float darkerNightAmbient = 0f;
            if (m_darkerNightSettings.m_darkerNights)
            {
                darkerNightAmbient = isDay ? 0f : m_darkerNightSettings.m_ambientIntensityAdjustmentValue;
            }

            if (!overrideData.m_isInVolue)
            {
                if (transitionComplete)
                {
                    if (m_useSSGI)
                    {
                        components.m_timeOfDayVolumeComponenets.m_indirectLightingController.indirectDiffuseLightingMultiplier.value = Mathf.Lerp(components.m_timeOfDayVolumeComponenets.m_indirectLightingController.indirectDiffuseLightingMultiplier.value, ambientIntensity * (m_ambientIntensityMultiplier * GetNonSSGIAmbientDivider(isDay, rayTracing, rayTraceSettings)) - darkerNightAmbient, overrideData.m_transitionTime);
                    }
                    else
                    {
                        components.m_timeOfDayVolumeComponenets.m_indirectLightingController.indirectDiffuseLightingMultiplier.value = Mathf.Lerp(components.m_timeOfDayVolumeComponenets.m_indirectLightingController.indirectDiffuseLightingMultiplier.value, ambientIntensity * (m_ambientIntensityMultiplier * GetNonSSGIAmbientDivider(isDay, rayTracing, rayTraceSettings)) - darkerNightAmbient, overrideData.m_transitionTime);
                    }

                    components.m_timeOfDayVolumeComponenets.m_indirectLightingController.reflectionLightingMultiplier.value = Mathf.Lerp(components.m_timeOfDayVolumeComponenets.m_indirectLightingController.reflectionLightingMultiplier.value, m_ambientReflectionIntensity.Evaluate(time) * GetReflectionAmbientMultiplier(isDay, rayTracing, rayTraceSettings), overrideData.m_transitionTime);
                }
                else
                {
                    components.m_timeOfDayVolumeComponenets.m_indirectLightingController.indirectDiffuseLightingMultiplier.value = ambientIntensity * (m_ambientIntensityMultiplier * GetNonSSGIAmbientDivider(isDay, rayTracing, rayTraceSettings) - darkerNightAmbient);
                    components.m_timeOfDayVolumeComponenets.m_indirectLightingController.reflectionLightingMultiplier.value = m_ambientReflectionIntensity.Evaluate(time) * GetReflectionAmbientMultiplier(isDay, rayTracing, rayTraceSettings);
                    components.m_timeOfDayVolumeComponenets.m_indirectLightingController.reflectionProbeIntensityMultiplier.value = m_planarReflectionIntensity.Evaluate(time);
                }
            }
            else
            {
                if (overrideData.m_settings.m_ambientIntensity.overrideState)
                {
                    if (transitionComplete)
                    {
                        if (m_useSSGI)
                        {
                            components.m_timeOfDayVolumeComponenets.m_indirectLightingController.indirectDiffuseLightingMultiplier.value = Mathf.Lerp(components.m_timeOfDayVolumeComponenets.m_indirectLightingController.indirectDiffuseLightingMultiplier.value, overrideAmbientIntensity * m_ambientIntensityMultiplier, overrideData.m_transitionTime);
                        }
                        else
                        {
                            components.m_timeOfDayVolumeComponenets.m_indirectLightingController.indirectDiffuseLightingMultiplier.value = Mathf.Lerp(components.m_timeOfDayVolumeComponenets.m_indirectLightingController.indirectDiffuseLightingMultiplier.value, overrideAmbientIntensity * (m_ambientIntensityMultiplier * GetNonSSGIAmbientDivider(isDay, rayTracing, rayTraceSettings)), overrideData.m_transitionTime);
                        }
                    }
                    else
                    {
                        if (m_useSSGI)
                        {
                            components.m_timeOfDayVolumeComponenets.m_indirectLightingController.indirectDiffuseLightingMultiplier.value = overrideAmbientIntensity * m_ambientIntensityMultiplier - darkerNightAmbient;
                        }
                        else
                        {
                            components.m_timeOfDayVolumeComponenets.m_indirectLightingController.indirectDiffuseLightingMultiplier.value = overrideAmbientIntensity * (m_ambientIntensityMultiplier * GetNonSSGIAmbientDivider(isDay, rayTracing, rayTraceSettings) - darkerNightAmbient);
                        }
                    }
                }
                else
                {
                    if (transitionComplete)
                    {
                        if (m_useSSGI)
                        {
                            components.m_timeOfDayVolumeComponenets.m_indirectLightingController.indirectDiffuseLightingMultiplier.value = Mathf.Lerp(components.m_timeOfDayVolumeComponenets.m_indirectLightingController.indirectDiffuseLightingMultiplier.value, ambientIntensity * m_ambientIntensityMultiplier - darkerNightAmbient, overrideData.m_transitionTime);
                        }
                        else
                        {
                            components.m_timeOfDayVolumeComponenets.m_indirectLightingController.indirectDiffuseLightingMultiplier.value = Mathf.Lerp(components.m_timeOfDayVolumeComponenets.m_indirectLightingController.indirectDiffuseLightingMultiplier.value, ambientIntensity * (m_ambientIntensityMultiplier * GetNonSSGIAmbientDivider(isDay, rayTracing, rayTraceSettings) - darkerNightAmbient), overrideData.m_transitionTime);
                        }
                    }
                    else
                    {
                        if (m_useSSGI)
                        {
                            components.m_timeOfDayVolumeComponenets.m_indirectLightingController.indirectDiffuseLightingMultiplier.value = ambientIntensity * m_ambientIntensityMultiplier - darkerNightAmbient;
                        }
                        else
                        {
                            components.m_timeOfDayVolumeComponenets.m_indirectLightingController.indirectDiffuseLightingMultiplier.value = ambientIntensity * (m_ambientIntensityMultiplier * GetNonSSGIAmbientDivider(isDay, rayTracing, rayTraceSettings) - darkerNightAmbient);
                        }
                    }
                }
                
                if (overrideData.m_settings.m_ambientReflectionIntensity.overrideState)
                {
                    if (transitionComplete)
                    {
                        if (m_useSSGI)
                        {
                            components.m_timeOfDayVolumeComponenets.m_indirectLightingController.reflectionLightingMultiplier.value = Mathf.Lerp(components.m_timeOfDayVolumeComponenets.m_indirectLightingController.reflectionLightingMultiplier.value, overrideData.m_settings.m_ambientReflectionIntensity.value, overrideData.m_transitionTime);
                        }
                        else
                        {
                            components.m_timeOfDayVolumeComponenets.m_indirectLightingController.reflectionLightingMultiplier.value = Mathf.Lerp(components.m_timeOfDayVolumeComponenets.m_indirectLightingController.reflectionLightingMultiplier.value, overrideData.m_settings.m_ambientReflectionIntensity.value * GetReflectionAmbientMultiplier(isDay, rayTracing, rayTraceSettings), overrideData.m_transitionTime);
                        }
                    }
                    else
                    {
                        if (m_useSSGI)
                        {
                            components.m_timeOfDayVolumeComponenets.m_indirectLightingController.reflectionLightingMultiplier.value = overrideData.m_settings.m_ambientReflectionIntensity.value;
                        }
                        else
                        {
                            components.m_timeOfDayVolumeComponenets.m_indirectLightingController.reflectionLightingMultiplier.value = overrideData.m_settings.m_ambientReflectionIntensity.value * GetReflectionAmbientMultiplier(isDay, rayTracing, rayTraceSettings);
                        }
                    }
                }
                else
                {
                    if (transitionComplete)
                    {
                        if (m_useSSGI)
                        {
                            components.m_timeOfDayVolumeComponenets.m_indirectLightingController.reflectionLightingMultiplier.value = Mathf.Lerp(components.m_timeOfDayVolumeComponenets.m_indirectLightingController.reflectionLightingMultiplier.value, m_ambientReflectionIntensity.Evaluate(time), overrideData.m_transitionTime);
                        }
                        else
                        {
                            components.m_timeOfDayVolumeComponenets.m_indirectLightingController.reflectionLightingMultiplier.value = Mathf.Lerp(components.m_timeOfDayVolumeComponenets.m_indirectLightingController.reflectionLightingMultiplier.value, m_ambientReflectionIntensity.Evaluate(time) * GetReflectionAmbientMultiplier(isDay, rayTracing, rayTraceSettings), overrideData.m_transitionTime);
                        }
                    }
                    else
                    {
                        if (m_useSSGI)
                        {
                            components.m_timeOfDayVolumeComponenets.m_indirectLightingController.reflectionLightingMultiplier.value = m_ambientReflectionIntensity.Evaluate(time);
                        }
                        else
                        {
                            components.m_timeOfDayVolumeComponenets.m_indirectLightingController.reflectionLightingMultiplier.value = m_ambientReflectionIntensity.Evaluate(time) * GetReflectionAmbientMultiplier(isDay, rayTracing, rayTraceSettings);
                        }
                    }
                }
            }
        }
#endif
        /// <summary>
        /// Applies the contact shadows
        /// </summary>
        /// <param name="components"></param>
        /// <param name="time"></param>
        /// <param name="isDay"></param>
        public void ApplyContactShadows(HDRPTimeOfDayComponents components, float time, bool isDay)
        {
            components.m_timeOfDayVolumeComponenets.m_contactShadows.active = true;
            components.m_timeOfDayVolumeComponenets.m_contactShadows.enable.value = m_useContactShadows;
            if (!m_useContactShadows)
            {
                return;
            }

            components.m_timeOfDayVolumeComponenets.m_contactShadows.opacity.value = Mathf.Clamp01(m_contactShadowsOpacity.Evaluate(time));
            if (m_useAdvancedNearFarContactShadows)
            {
                Transform player = HDRPTimeOfDay.Instance.Player;
                float shadowMultiplier = m_shadowDistanceMultiplier;
                if (!isDay)
                {
                    shadowMultiplier = m_shadowDistanceMultiplierNight;
                }

                float minusPercentage = (m_shadowDistance.Evaluate(time) * m_farShadowPercentageBlend) * shadowMultiplier;
                float shadowDistance = (m_shadowDistance.Evaluate(time) * shadowMultiplier) - minusPercentage;
                if (!m_setContactShadowsMinDistanceToShadowDistance)
                {
                    shadowDistance = 0.01f;
                }
                if (Physics.Raycast(player.position, Vector3.down, out RaycastHit hit, 3000f, m_groundLayer))
                {
                    if (hit.distance <= m_nearThresholdPercentage)
                    {
                        components.m_timeOfDayVolumeComponenets.m_contactShadows.maxDistance.value = Mathf.Clamp(m_nearContactShadowsDistance.Evaluate(time), 0.01f, float.MaxValue);
                        components.m_timeOfDayVolumeComponenets.m_contactShadows.minDistance.value = 0.01f;
                        components.m_timeOfDayVolumeComponenets.m_contactShadows.sampleCount = m_nearContactShadowsSampleCount;
#if RAY_TRACING_ENABLED
                        components.m_rayTracedContactShadows.active = true;
#endif
                    }
                    else
                    {
                        components.m_timeOfDayVolumeComponenets.m_contactShadows.maxDistance.value = Mathf.Clamp(m_contactShadowsDistance.Evaluate(time), 0.01f, float.MaxValue);
                        components.m_timeOfDayVolumeComponenets.m_contactShadows.minDistance.value = Mathf.Clamp(shadowDistance, 0.01f, components.m_timeOfDayVolumeComponenets.m_contactShadows.maxDistance.value);
                        components.m_timeOfDayVolumeComponenets.m_contactShadows.sampleCount = m_farContactShadowsSampleCount;
#if RAY_TRACING_ENABLED
                        components.m_rayTracedContactShadows.active = false;
#endif
                    }
                }
                else
                {
                    components.m_timeOfDayVolumeComponenets.m_contactShadows.maxDistance.value = Mathf.Clamp(m_contactShadowsDistance.Evaluate(time), 0.01f, float.MaxValue);
                    components.m_timeOfDayVolumeComponenets.m_contactShadows.minDistance.value = Mathf.Clamp(shadowDistance, 0.01f, components.m_timeOfDayVolumeComponenets.m_contactShadows.maxDistance.value);
                    components.m_timeOfDayVolumeComponenets.m_contactShadows.sampleCount = m_farContactShadowsSampleCount;
#if RAY_TRACING_ENABLED
                    components.m_rayTracedContactShadows.active = false;
#endif
                }
            }
            else
            {
                components.m_timeOfDayVolumeComponenets.m_contactShadows.maxDistance.value = Mathf.Clamp(m_contactShadowsDistance.Evaluate(time), 0.01f, float.MaxValue);
                components.m_timeOfDayVolumeComponenets.m_contactShadows.minDistance.value = 0.01f;
                components.m_timeOfDayVolumeComponenets.m_contactShadows.sampleCount = m_farContactShadowsSampleCount;
#if RAY_TRACING_ENABLED
                components.m_rayTracedContactShadows.active = false;
#endif
            }
        }
        /// <summary>
        /// Applies volumetric cloud sahdows
        /// </summary>
        /// <param name="components"></param>
        /// <param name="time"></param>
        public void ApplyVolumetricCloudShadows(HDRPTimeOfDayComponents components, float time)
        {
            if (m_cloudPresets == TODCloudPresets.Presets)
            {
#if HDRPTIMEOFDAY
                if (VolumetricCloudPreset != null)
                {
                    VolumetricCloudPreset.ApplyShadows(components.m_timeOfDayVolumeComponenets.m_volumetricClouds, time, m_groundLayer);
                }
#endif
            }
            else
            {
                if (m_volumetricCloudShadows)
                {
                    if (m_useHeightBasedVolumetricCloudShadows)
                    {
                        Transform player = HDRPTimeOfDay.Instance.Player;
                        if (player != null)
                        {
                            if (Physics.Raycast(player.position, Vector3.down, out RaycastHit hit, 3000f, m_groundLayer))
                            {
                                if (hit.distance < m_volumetricCloudShadowsGroundHeight)
                                {
                                    components.m_timeOfDayVolumeComponenets.m_volumetricClouds.shadowOpacity.value = Mathf.Clamp(hit.distance / m_volumetricCloudShadowsGroundHeight, m_volumetricCloudMinShadowClampValue, 1f);
                                }
                                else
                                {
                                    components.m_timeOfDayVolumeComponenets.m_volumetricClouds.shadowOpacity.value = 1f;
                                }
                            }
                            else
                            {
                                components.m_timeOfDayVolumeComponenets.m_volumetricClouds.shadowOpacity.value = m_volumetricCloudShadowOpacity.Evaluate(time);
                            }
                        }
                        else
                        {
                            components.m_timeOfDayVolumeComponenets.m_volumetricClouds.shadowOpacity.value = m_volumetricCloudShadowOpacity.Evaluate(time);
                        }
                    }
                    else
                    {
                        components.m_timeOfDayVolumeComponenets.m_volumetricClouds.shadowOpacity.value = m_volumetricCloudShadowOpacity.Evaluate(time);
                    }
                }
            }
        }

        public bool ValidateFog()
        {
            if (m_fogColor == null)
            {
                return false;
            }
            if (m_fogDistance == null)
            {
                return false;
            }
            if (m_fogHeight == null)
            {
                return false;
            }
            if (m_volumetricFogDistance == null)
            {
                return false;
            }
            if (m_volumetricFogAnisotropy == null)
            {
                return false;
            }
            if (m_volumetricFogSliceDistributionUniformity == null)
            {
                return false;
            }
            if (m_localFogMultiplier == null)
            {
                return false;
            }
            if (m_fogDensity == null)
            {
                return false;
            }

            return true;
        }
        public void ApplyFogSettings(HDRPTimeOfDayComponents components, float time, out Color currrentFogColor, out float currentFogDistance)
        {
            components.m_localVolumetricFog.parameters.meanFreePath = m_fogDensity.Evaluate(time) / m_globalFogMultiplier;
            currentFogDistance = components.m_localVolumetricFog.parameters.meanFreePath;
            components.m_localVolumetricFog.parameters.albedo = m_fogColor.Evaluate(time) * m_localFogMultiplier.Evaluate(time);
            components.m_localVolumetricFog.parameters.size = new Vector3(m_localFogSize, m_localFogSize, m_localFogSize);
            currrentFogColor = components.m_localVolumetricFog.parameters.albedo;

            components.m_timeOfDayVolumeComponenets.m_fog.active = m_useFog;
            components.m_localVolumetricFog.enabled = m_useFog;
            components.m_timeOfDayVolumeComponenets.m_fog.albedo.value = m_fogColor.Evaluate(time);
            components.m_timeOfDayVolumeComponenets.m_fog.meanFreePath.value = m_fogDistance.Evaluate(time) / m_globalFogDistanceMultiplier;
            float fogHeight = m_fogHeight.Evaluate(time) * m_globalFogHeightMultiplier;
            components.m_timeOfDayVolumeComponenets.m_fog.baseHeight.value = fogHeight;
            components.m_timeOfDayVolumeComponenets.m_fog.maximumHeight.value = fogHeight * 4f;
            components.m_timeOfDayVolumeComponenets.m_fog.depthExtent.value = m_volumetricFogDistance.Evaluate(time);
            if (m_useFogAnisotropyOverride)
            {
                components.m_timeOfDayVolumeComponenets.m_fog.anisotropy.value = m_volumetricFogAnisotropy.Evaluate(time);
            }
            else
            {
                components.m_timeOfDayVolumeComponenets.m_fog.anisotropy.value = 0;
            }

            components.m_timeOfDayVolumeComponenets.m_fog.sliceDistributionUniformity.value = m_volumetricFogSliceDistributionUniformity.Evaluate(time);
            components.m_timeOfDayVolumeComponenets.m_fog.quality.value = (int)m_fogQuality;
            components.m_timeOfDayVolumeComponenets.m_fog.enableVolumetricFog.value = m_enableVolumetricFog;
            if (m_useDenoising)
            {
                switch (m_denoisingQuality)
                {
                    case GeneralQuality.Low:
                    {
                        components.m_timeOfDayVolumeComponenets.m_fog.denoisingMode.value = FogDenoisingMode.Reprojection;
                        break;
                    }
                    case GeneralQuality.Medium:
                    {
                        components.m_timeOfDayVolumeComponenets.m_fog.denoisingMode.value = FogDenoisingMode.Gaussian;
                        break;
                    }
                    case GeneralQuality.High:
                    {
                        components.m_timeOfDayVolumeComponenets.m_fog.denoisingMode.value = FogDenoisingMode.Both;
                        break;
                    }
                }
            }
            else
            {
                components.m_timeOfDayVolumeComponenets.m_fog.denoisingMode.value = FogDenoisingMode.None;
            }
        }

        public bool ValidateShadows()
        {
            if (m_shadowDistance == null)
            {
                return false;
            }
            if (m_shadowTransmissionMultiplier == null)
            {
                return false;
            }

            return true;
        }
        public void ApplyShadowSettings(HDRPTimeOfDayComponents components, float time, OverrideDataInfo overrideData, bool isDay)
        {
            components.m_timeOfDayVolumeComponenets.m_shadows.directionalTransmissionMultiplier.value = m_shadowTransmissionMultiplier.Evaluate(time);
            components.m_timeOfDayVolumeComponenets.m_shadows.cascadeShadowSplitCount.value = Mathf.Clamp(m_shadowCascadeCount, 1, 4);
            if (m_shadowCascadeFadeLength.Count == 3)
            {
                components.m_timeOfDayVolumeComponenets.m_shadows.cascadeShadowSplit0.value = m_shadowCascadeFadeLength[0];
                components.m_timeOfDayVolumeComponenets.m_shadows.cascadeShadowSplit1.value = m_shadowCascadeFadeLength[1];
                components.m_timeOfDayVolumeComponenets.m_shadows.cascadeShadowSplit2.value = m_shadowCascadeFadeLength[2];
            }
            else
            {
                Debug.Log("Shadow cascade count needs to be a length of at least 3 to be applied.");
            }
            bool transitionComplete = overrideData.m_transitionTime < 1f;
            float shadowMultiplier = m_shadowDistanceMultiplier;
            if (!isDay)
            {
                shadowMultiplier = m_shadowDistanceMultiplierNight;
            }
            if (!overrideData.m_isInVolue)
            {
                if (transitionComplete)
                {
                    components.m_timeOfDayVolumeComponenets.m_shadows.maxShadowDistance.value = Mathf.Lerp(components.m_timeOfDayVolumeComponenets.m_shadows.maxShadowDistance.value, (m_shadowDistance.Evaluate(time) * shadowMultiplier), overrideData.m_transitionTime);
                }
                else
                {
                    components.m_timeOfDayVolumeComponenets.m_shadows.maxShadowDistance.value = (m_shadowDistance.Evaluate(time) * shadowMultiplier);
                }
            }
            else
            {
                if (overrideData.m_settings.m_shadowDistanceMultiplier.overrideState)
                {
                    if (transitionComplete)
                    {
                        components.m_timeOfDayVolumeComponenets.m_shadows.maxShadowDistance.value = Mathf.Lerp(components.m_timeOfDayVolumeComponenets.m_shadows.maxShadowDistance.value,  (m_shadowDistance.Evaluate(time) * overrideData.m_settings.m_shadowDistanceMultiplier.value), overrideData.m_transitionTime);
                    }
                    else
                    {
                        components.m_timeOfDayVolumeComponenets.m_shadows.maxShadowDistance.value = (m_shadowDistance.Evaluate(time) * overrideData.m_settings.m_shadowDistanceMultiplier.value);
                    }
                }
                else
                {
                    if (transitionComplete)
                    {
                        components.m_timeOfDayVolumeComponenets.m_shadows.maxShadowDistance.value = Mathf.Lerp(components.m_timeOfDayVolumeComponenets.m_shadows.maxShadowDistance.value, (m_shadowDistance.Evaluate(time) * shadowMultiplier), overrideData.m_transitionTime);
                    }
                    else
                    {
                        components.m_timeOfDayVolumeComponenets.m_shadows.maxShadowDistance.value = (m_shadowDistance.Evaluate(time) * shadowMultiplier);
                    }
                }
            }
        }

        public bool ValidateClouds()
        {
            //Procedural
            if (m_cloudOpacity == null)
            {
                return false;
            }
            if (m_cloudTintColor == null)
            {
                return false;
            }
            if (m_cloudExposure == null)
            {
                return false;
            }
            if (m_cloudWindDirection == null)
            {
                return false;
            }
            if (m_cloudWindSpeed == null)
            {
                return false;
            }
            if (m_cloudShadowOpacity == null)
            {
                return false;
            }
            if (m_cloudShadowColor == null)
            {
                return false;
            }
            if (m_cloudLayerAOpacityR == null)
            {
                return false;
            }
            if (m_cloudLayerAOpacityG == null)
            {
                return false;
            }
            if (m_cloudLayerAOpacityB == null)
            {
                return false;
            }
            if (m_cloudLayerAOpacityA == null)
            {
                return false;
            }
            if (m_cloudLayerBOpacityR == null)
            {
                return false;
            }
            if (m_cloudLayerBOpacityG == null)
            {
                return false;
            }
            if (m_cloudLayerBOpacityB == null)
            {
                return false;
            }
            if (m_cloudLayerBOpacityA == null)
            {
                return false;
            }
            //Volumetic
            if (m_volumetricDensityMultiplier == null)
            {
                return false;
            }
            if (m_volumetricDensityCurve == null)
            {
                return false;
            }
            if (m_volumetricShapeFactor == null)
            {
                return false;
            }
            if (m_volumetricShapeScale == null)
            {
                return false;
            }
            if (m_volumetricErosionFactor == null)
            {
                return false;
            }
            if (m_volumetricErosionScale == null)
            {
                return false;
            }
            if (m_volumetricErosionCurve == null)
            {
                return false;
            }
            if (m_volumetricAmbientOcclusionCurve == null)
            {
                return false;
            }
            if (m_volumetricAmbientLightProbeDimmer == null)
            {
                return false;
            }
            if (m_volumetricSunLightDimmer == null)
            {
                return false;
            }
            if (m_volumetricErosionOcclusion == null)
            {
                return false;
            }
            if (m_volumetricScatteringTint == null)
            {
                return false;
            }
            if (m_volumetricPowderEffectIntensity == null)
            {
                return false;
            }
            if (m_volumetricMultiScattering == null)
            {
                return false;
            }
            if (m_volumetricCloudShadowOpacity == null)
            {
                return false;
            }

            return true;
        }
        public float ApplyCloudSettings(HDRPTimeOfDayComponents components, float time)
        {
            #region Volumetric

            components.m_timeOfDayVolumeComponenets.m_volumetricClouds.localClouds.value = m_useLocalClouds;
#if UNITY_2022_3_OR_NEWER
            components.m_timeOfDayVolumeComponenets.m_volumetricClouds.cloudPreset = VolumetricClouds.CloudPresets.Custom;
#else
            components.m_timeOfDayVolumeComponenets.m_volumetricClouds.cloudPreset.value = VolumetricClouds.CloudPresets.Custom;
#endif
            if (m_cloudPresets == TODCloudPresets.Custom)
            {
                components.m_timeOfDayVolumeComponenets.m_volumetricClouds.densityMultiplier.value =
                    m_volumetricDensityMultiplier.Evaluate(time);
                components.m_timeOfDayVolumeComponenets.m_volumetricClouds.shapeFactor.value =
                    m_volumetricShapeFactor.Evaluate(time);
                components.m_timeOfDayVolumeComponenets.m_volumetricClouds.shapeScale.value =
                    m_volumetricShapeScale.Evaluate(time);
                components.m_timeOfDayVolumeComponenets.m_volumetricClouds.erosionFactor.value =
                    m_volumetricErosionFactor.Evaluate(time);
                components.m_timeOfDayVolumeComponenets.m_volumetricClouds.erosionScale.value =
                    m_volumetricErosionScale.Evaluate(time);
                components.m_timeOfDayVolumeComponenets.m_volumetricClouds.erosionNoiseType.value = m_erosionNoiseType;
#if UNITY_2022_2_OR_NEWER
                components.m_timeOfDayVolumeComponenets.m_volumetricClouds.densityCurve.value =
                    m_volumetricDensityCurve;
                components.m_timeOfDayVolumeComponenets.m_volumetricClouds.erosionCurve.value =
                    m_volumetricErosionCurve;
                components.m_timeOfDayVolumeComponenets.m_volumetricClouds.ambientOcclusionCurve.value =
                    m_volumetricAmbientOcclusionCurve;
                components.m_timeOfDayVolumeComponenets.m_volumetricClouds.bottomAltitude.value =
                    m_volumetricLowestCloudAltitude.Evaluate(time) * m_volumetricCloudHeightMultiplier;
                components.m_timeOfDayVolumeComponenets.m_volumetricClouds.altitudeRange.value =
                    m_volumetricCloudThickness.Evaluate(time);
#else
            components.m_timeOfDayVolumeComponenets.m_volumetricClouds.customDensityCurve.value =
 m_volumetricDensityCurve;
            components.m_timeOfDayVolumeComponenets.m_volumetricClouds.customErosionCurve.value =
 m_volumetricErosionCurve;
            components.m_timeOfDayVolumeComponenets.m_volumetricClouds.customAmbientOcclusionCurve.value =
 m_volumetricAmbientOcclusionCurve;
            components.m_timeOfDayVolumeComponenets.m_volumetricClouds.lowestCloudAltitude.value =
 m_volumetricLowestCloudAltitude.Evaluate(time) * m_volumetricCloudHeightMultiplier;
            components.m_timeOfDayVolumeComponenets.m_volumetricClouds.cloudThickness.value =
 m_volumetricCloudThickness.Evaluate(time);
#endif
                //Lighting
                components.m_timeOfDayVolumeComponenets.m_volumetricClouds.ambientLightProbeDimmer.value = m_volumetricAmbientLightProbeDimmer.Evaluate(time);
                components.m_timeOfDayVolumeComponenets.m_volumetricClouds.sunLightDimmer.value = m_volumetricSunLightDimmer.Evaluate(time);
                components.m_timeOfDayVolumeComponenets.m_volumetricClouds.erosionOcclusion.value = m_volumetricErosionOcclusion.Evaluate(time);
                components.m_timeOfDayVolumeComponenets.m_volumetricClouds.scatteringTint.value = m_volumetricScatteringTint.Evaluate(time);
                components.m_timeOfDayVolumeComponenets.m_volumetricClouds.powderEffectIntensity.value = m_volumetricPowderEffectIntensity.Evaluate(time);
                components.m_timeOfDayVolumeComponenets.m_volumetricClouds.multiScattering.value = m_volumetricMultiScattering.Evaluate(time);
                //Shadow
                components.m_timeOfDayVolumeComponenets.m_volumetricClouds.shadows.value = m_volumetricCloudShadows;
                if (m_volumetricCloudShadows)
                {
                    components.m_timeOfDayVolumeComponenets.m_volumetricClouds.shadowResolution.value = m_volumetricCloudShadowResolution;
                    components.m_timeOfDayVolumeComponenets.m_volumetricClouds.shadowOpacity.value = m_volumetricCloudShadowOpacity.Evaluate(time);
                }
            }
            else
            {
#if HDRPTIMEOFDAY
                if (VolumetricCloudPreset != null)
                {
                    VolumetricCloudPreset.Apply(components.m_timeOfDayVolumeComponenets.m_volumetricClouds);
                    VolumetricCloudPreset.ApplyWind(components.m_timeOfDayVolumeComponenets.m_visualEnvironment, time);
                }
#endif
            }

#endregion
            #region Procedural

            if (m_proceduralCloudPresets == TODCloudPresets.Custom)
            {
                components.m_timeOfDayVolumeComponenets.m_cloudLayer.layers.value = (CloudMapMode)m_cloudLayers;
                switch (m_cloudResolution)
                {
                    case CloudResolution.Resolution256:
                    {
                        components.m_timeOfDayVolumeComponenets.m_cloudLayer.resolution.value = UnityEngine.Rendering.HighDefinition.CloudResolution.CloudResolution256;
                        break;
                    }
                    case CloudResolution.Resolution512:
                    {
                        components.m_timeOfDayVolumeComponenets.m_cloudLayer.resolution.value = UnityEngine.Rendering.HighDefinition.CloudResolution.CloudResolution512;
                        break;
                    }
                    case CloudResolution.Resolution1024:
                    {
                        components.m_timeOfDayVolumeComponenets.m_cloudLayer.resolution.value = UnityEngine.Rendering.HighDefinition.CloudResolution.CloudResolution1024;
                        break;
                    }
                    case CloudResolution.Resolution2048:
                    {
                        components.m_timeOfDayVolumeComponenets.m_cloudLayer.resolution.value = UnityEngine.Rendering.HighDefinition.CloudResolution.CloudResolution2048;
                        break;
                    }
                    case CloudResolution.Resolution4096:
                    {
                        components.m_timeOfDayVolumeComponenets.m_cloudLayer.resolution.value = UnityEngine.Rendering.HighDefinition.CloudResolution.CloudResolution4096;
                        break;
                    }
                    case CloudResolution.Resolution8192:
                    {
                        components.m_timeOfDayVolumeComponenets.m_cloudLayer.resolution.value = UnityEngine.Rendering.HighDefinition.CloudResolution.CloudResolution8192;
                        break;
                    }
                }
                components.m_timeOfDayVolumeComponenets.m_cloudLayer.opacity.value = m_cloudOpacity.Evaluate(time);
                components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerA.castShadows.value = m_useCloudShadows;
                components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerA.lighting.value = m_cloudLighting;
                components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerB.castShadows.value = m_useCloudShadows;
                components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerB.lighting.value = m_cloudLighting;
#if UNITY_2022_2_OR_NEWER
                components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerA.tint.value = Color.white;
                components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerB.tint.value = Color.white;
#else
                components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerA.tint.value = m_cloudTintColor.Evaluate(time);
                components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerB.tint.value = m_cloudTintColor.Evaluate(time);
#endif
                components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerA.exposure.value = m_cloudExposure.Evaluate(time);
                components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerB.exposure.value = m_cloudExposure.Evaluate(time);
                components.m_timeOfDayVolumeComponenets.m_cloudLayer.shadowMultiplier.value = m_cloudShadowOpacity.Evaluate(time);
                components.m_timeOfDayVolumeComponenets.m_cloudLayer.shadowTint.value = m_cloudShadowColor.Evaluate(time);
                switch (m_cloudLayerAChannel)
                {
                    case CloudLayerChannelMode.R:
                    {
                        components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerA.opacityR.value = m_cloudLayerAOpacityR.Evaluate(time);
                        components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerA.opacityG.value = 0f;
                        components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerA.opacityB.value = 0f;
                        components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerA.opacityA.value = 0f;
                        break;
                    }
                    case CloudLayerChannelMode.G:
                    {
                        components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerA.opacityG.value = m_cloudLayerAOpacityR.Evaluate(time);
                        components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerA.opacityR.value = 0f;
                        components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerA.opacityB.value = 0f;
                        components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerA.opacityA.value = 0f;
                        break;
                    }
                    case CloudLayerChannelMode.B:
                    {
                        components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerA.opacityB.value = m_cloudLayerAOpacityR.Evaluate(time);
                        components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerA.opacityR.value = 0f;
                        components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerA.opacityG.value = 0f;
                        components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerA.opacityA.value = 0f;
                        break;
                    }
                    case CloudLayerChannelMode.A:
                    {
                        components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerA.opacityA.value = m_cloudLayerAOpacityR.Evaluate(time);
                        components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerA.opacityR.value = 0f;
                        components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerA.opacityG.value = 0f;
                        components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerA.opacityB.value = 0f;
                        break;
                    }
                }
                switch (m_cloudLayerBChannel)
                {
                    case CloudLayerChannelMode.R:
                    {
                        components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerB.opacityR.value = m_cloudLayerBOpacityR.Evaluate(time);
                        components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerB.opacityG.value = 0f;
                        components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerB.opacityB.value = 0f;
                        components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerB.opacityA.value = 0f;
                        break;
                    }
                    case CloudLayerChannelMode.G:
                    {
                        components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerB.opacityG.value = m_cloudLayerBOpacityR.Evaluate(time);
                        components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerB.opacityR.value = 0f;
                        components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerB.opacityB.value = 0f;
                        components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerB.opacityA.value = 0f;
                        break;
                    }
                    case CloudLayerChannelMode.B:
                    {
                        components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerB.opacityB.value = m_cloudLayerBOpacityR.Evaluate(time);
                        components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerB.opacityR.value = 0f;
                        components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerB.opacityG.value = 0f;
                        components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerB.opacityA.value = 0f;
                        break;
                    }
                    case CloudLayerChannelMode.A:
                    {
                        components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerB.opacityA.value = m_cloudLayerBOpacityR.Evaluate(time);
                        components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerB.opacityR.value = 0f;
                        components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerB.opacityG.value = 0f;
                        components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerB.opacityB.value = 0f;
                        break;
                    }
                }

                float windOrientation = Mathf.Clamp01(m_cloudWindDirection.Evaluate(time)) * 360f;
                components.m_timeOfDayVolumeComponenets.m_visualEnvironment.windOrientation.value = windOrientation;
                components.m_timeOfDayVolumeComponenets.m_visualEnvironment.windSpeed.value = m_cloudWindSpeed.Evaluate(time);
                Vector3 rotation = components.m_windZone.transform.eulerAngles;
                rotation.y = windOrientation;
                components.m_windZone.transform.eulerAngles = rotation;
            }
            else
            {
#if HDRPTIMEOFDAY
                if (ProceduralCloudPreset != null)
                {
                    ProceduralCloudPreset.Apply(components.m_timeOfDayVolumeComponenets.m_cloudLayer, time);
                }
#endif
            }

            return components.m_timeOfDayVolumeComponenets.m_cloudLayer.opacity.value;

            #endregion
        }

        public bool ValidateSunLensFlare()
        {
            if (m_sunLensFlareProfile.m_lensFlareData == null)
            {
                return false;
            }
            if (m_sunLensFlareProfile.m_intensity == null)
            {
                return false;
            }
            if (m_sunLensFlareProfile.m_scale == null)
            {
                return false;
            }

            return true;
        }
        public bool ValidateMoonLensFlare()
        {
            if (m_moonLensFlareProfile.m_lensFlareData == null)
            {
                return false;
            }
            if (m_moonLensFlareProfile.m_intensity == null)
            {
                return false;
            }
            if (m_moonLensFlareProfile.m_scale == null)
            {
                return false;
            }

            return true;
        }
        public void ApplyLensFlare(HDRPTimeOfDayComponents componenets, float time, bool isDay)
        {
            LensFlareComponentSRP activeLensflare = isDay ? componenets.m_sunLensFlare : componenets.m_moonLensFlare;
            if (isDay)
            {
                activeLensflare.enabled = m_sunLensFlareProfile.m_useLensFlare;
                componenets.m_moonLensFlare.enabled = false;
                if (m_sunLensFlareProfile.m_useLensFlare)
                {
                    activeLensflare.lensFlareData = m_sunLensFlareProfile.m_lensFlareData;
                    activeLensflare.intensity = m_sunLensFlareProfile.m_intensity.Evaluate(time);
                    activeLensflare.scale = m_sunLensFlareProfile.m_scale.Evaluate(time);
                    activeLensflare.useOcclusion = m_sunLensFlareProfile.m_enableOcclusion;
                    activeLensflare.occlusionRadius = m_sunLensFlareProfile.m_occlusionRadius;
                    activeLensflare.sampleCount = (uint) m_sunLensFlareProfile.m_sampleCount;
                    activeLensflare.occlusionOffset = m_sunLensFlareProfile.m_occlusionOffset;
                    activeLensflare.allowOffScreen = m_sunLensFlareProfile.m_allowOffScreen;
#if UNITY_2022_2_OR_NEWER
                    activeLensflare.volumetricCloudOcclusion = m_sunLensFlareProfile.m_volumetricCloudOcclusion;
#endif
                }
            }
            else
            {
                activeLensflare.enabled = m_moonLensFlareProfile.m_useLensFlare;
                componenets.m_sunLensFlare.enabled = false;
                if (m_moonLensFlareProfile.m_useLensFlare)
                {
                    activeLensflare.lensFlareData = m_moonLensFlareProfile.m_lensFlareData;
                    activeLensflare.intensity = m_moonLensFlareProfile.m_intensity.Evaluate(time);
                    activeLensflare.scale = m_moonLensFlareProfile.m_scale.Evaluate(time);
                    activeLensflare.useOcclusion = m_moonLensFlareProfile.m_enableOcclusion;
                    activeLensflare.occlusionRadius = m_moonLensFlareProfile.m_occlusionRadius;
                    activeLensflare.sampleCount = (uint) m_moonLensFlareProfile.m_sampleCount;
                    activeLensflare.occlusionOffset = m_moonLensFlareProfile.m_occlusionOffset;
                    activeLensflare.allowOffScreen = m_moonLensFlareProfile.m_allowOffScreen;
#if UNITY_2022_2_OR_NEWER
                    activeLensflare.volumetricCloudOcclusion = m_moonLensFlareProfile.m_volumetricCloudOcclusion;
#endif
                }
            }
        }

        private float LerpFloat(float startingValue, float endValue, float time)
        {
            return Mathf.Lerp(startingValue, endValue, time);
        }
        private Color LerpColor(Color startingValue, Color endValue, float time)
        {
            return Color.Lerp(startingValue, endValue, time);
        }
    }
    [System.Serializable]
    public class UnderwaterOverrideData
    {
        public UnderwaterOverrideSystemType m_systemType = UnderwaterOverrideSystemType.HDRPTimeOfDay;
        public bool m_useOverrides = true;
        public float m_seaLevel = 0f;
        public Gradient m_underwaterFogColor;
        public AnimationCurve m_underwaterFogColorMultiplier;
        public Color m_multiplyColor = Color.white;
        public bool m_useUnderwaterReverb = true;
        public UnderwaterReverbFilterSettings m_reverbFilterSettings = new UnderwaterReverbFilterSettings();

        /// <summary>
        /// Applies the settings
        /// Time of day should be from 0-1
        /// </summary>
        /// <param name="timeOfDay"></param>
        public bool ApplySettings(float timeOfDay, Transform playerCamera, List<VisualEffect> visualEffects)
        {
            if (m_systemType == UnderwaterOverrideSystemType.HDRPTimeOfDay)
            {
                if (playerCamera != null)
                {
                    bool isUnderwater = playerCamera.transform.position.y < m_seaLevel;
                    //Weather Particles
                    if (visualEffects.Count > 0)
                    {
                        foreach (VisualEffect visualEffect in visualEffects)
                        {
                            if (visualEffect != null)
                            {
                                if (isUnderwater)
                                {
                                    visualEffect.enabled = false;
                                    visualEffect.Stop();
                                }
                                else
                                {
                                    visualEffect.enabled = true;
                                    visualEffect.Play();
                                }
                            }
                        }
                    }

                    return isUnderwater;
                }
            }
            else
            {
                //Add your custom code here
            }

            return false;
        }
    }
    [System.Serializable]
    public class UnderwaterReverbFilterSettings
    {
        public float m_dryLevel = -2500f;
        public float m_room = -1000f;
        public float m_roomHF = -4000f;
        public float m_roomLF = 0f;
        public float m_decayTime = 1.49f;
        public float m_decayHFRatio = 0.1f;
        public float m_reflectionsLevel = -449f;
        public float m_reflectionsDelay = 0f;
        public float m_reverbLevel = 1700f;
        public float m_reverbDelay = 0.011f;
        public float m_hFReference = 5000f;
        public float m_lFReference = 250f;
        public float m_diffusion = 100f;
        public float m_density = 100f;

        /// <summary>
        /// Applies the reverb settings to the filter
        /// Will add one if the filter is null
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="camera"></param>
        /// <param name="destroy"></param>
        /// <returns></returns>
        public AudioReverbFilter ApplyReverb(AudioReverbFilter filter, Camera camera, bool underwater)
        {
            if (filter == null && camera != null)
            {
                filter = camera.gameObject.GetComponent<AudioReverbFilter>();
                if (underwater)
                {
                    if (filter == null)
                    {
                        filter = camera.gameObject.AddComponent<AudioReverbFilter>();
                    }
                }
                else
                {
                    if (filter != null)
                    {
                        GameObject.DestroyImmediate(filter, true);
                        return null;
                    }
                }
            }

            if (filter != null)
            {
                filter.reverbPreset = AudioReverbPreset.User;
                //Settings
                filter.dryLevel = m_dryLevel;
                filter.room = m_room;
                filter.roomHF = m_roomHF;
                filter.roomLF = m_roomLF;
                filter.decayTime = m_decayTime;
                filter.decayHFRatio = m_decayHFRatio;
                filter.reflectionsLevel = m_reflectionsLevel;
                filter.reflectionsDelay = m_reflectionsDelay;
                filter.reverbLevel = m_reverbLevel;
                filter.reverbDelay = m_reverbDelay;
                filter.hfReference = m_hFReference;
                filter.lfReference = m_lFReference;
                filter.diffusion = m_diffusion;
                filter.density = m_density;
            }

            return filter;
        }
        /// <summary>
        /// Applies the reverb settings to the filter
        /// Will add one if the filter is null
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="camera"></param>
        /// <param name="destroy"></param>
        /// <returns></returns>
        public AudioReverbFilter ApplyReverb(AudioReverbFilter filter, Transform camera, bool underwater)
        {
            if (filter == null && camera != null)
            {
                filter = camera.gameObject.GetComponent<AudioReverbFilter>();
                if (underwater)
                {
                    if (filter == null)
                    {
                        filter = camera.gameObject.AddComponent<AudioReverbFilter>();
                    }
                }
                else
                {
                    if (filter != null)
                    {
                        GameObject.DestroyImmediate(filter, true);
                        return null;
                    }
                }
            }

            if (filter != null)
            {
                filter.reverbPreset = AudioReverbPreset.User;
                //Settings
                filter.dryLevel = m_dryLevel;
                filter.room = m_room;
                filter.roomHF = m_roomHF;
                filter.roomLF = m_roomLF;
                filter.decayTime = m_decayTime;
                filter.decayHFRatio = m_decayHFRatio;
                filter.reflectionsLevel = m_reflectionsLevel;
                filter.reflectionsDelay = m_reflectionsDelay;
                filter.reverbLevel = m_reverbLevel;
                filter.reverbDelay = m_reverbDelay;
                filter.hfReference = m_hFReference;
                filter.lfReference = m_lFReference;
                filter.diffusion = m_diffusion;
                filter.density = m_density;
            }

            return filter;
        }
    }

    public class HDRPTimeOfDayProfile : ScriptableObject
    {
        public TimeOfDayProfileData TimeOfDayData
        {
            get { return m_timeOfDayData; }
            set
            {
                if (m_timeOfDayData != value)
                {
                    m_timeOfDayData = value;
                }
            }
        }
        [SerializeField] private TimeOfDayProfileData m_timeOfDayData = new TimeOfDayProfileData();

        public UnderwaterOverrideData UnderwaterOverrideData
        {
            get { return m_underwaterOverrideData; }
            set
            {
                m_underwaterOverrideData = value;
            }
        }
        [SerializeField] private UnderwaterOverrideData m_underwaterOverrideData = new UnderwaterOverrideData();

        public HDRPTimeOfDayRayTracingProfile RayTracingProfile
        {
            get { return m_rayTracingProfile; }
            set
            {
                if (m_rayTracingProfile != value)
                {
                    m_rayTracingProfile = value;
                    if (HDRPTimeOfDay.Instance != null)
                    {
                        HDRPTimeOfDay.Instance.RefreshRayTracing();
                    }
                }
            }
        }
        [SerializeField] private HDRPTimeOfDayRayTracingProfile m_rayTracingProfile;
    }
}
#endif