using System;
using UnityEngine;
#if HDRPTIMEOFDAY && HDPipeline
using UnityEngine.Rendering.HighDefinition;
#endif

namespace ProceduralWorlds.HDRPTOD
{
    [Serializable]
    public class CachedVolumetricCloudSettings
    {
        public bool CachedHasBeenSet = false;
        [Range(0f, 1f)]
        public float DensityMultiplier = 0.4f;
        [Range(0f, 1f)]
        public float ShapeFactor = 0.9f;
        public float ShapeScale = 5f;
        [Range(0f, 1f)]
        public float EarthCurvature = 0.8f;
        public float BottomAltitude = 1200f;
        public float AltitudeRange = 2000f;
        [Range(0f, 1f)]
        public float AmbientLightProbeDimmer = 1f;
        [Range(0f, 1f)]
        public float SunLightDimmer = 1f;
        [Range(0f, 1f)]
        public float ErosionOcclusion = 0.2f;
        public Color ScatteringTint = Color.black;
        [Range(0f, 1f)]
        public float PowderEffectIntensity = 0.2f;
        [Range(0f, 1f)]
        public float MultiScattering = 0.2f;

        /// <summary>
        /// Copies the settings from volumetric cloud component
        /// </summary>
        /// <param name="volumetricClouds"></param>
#if HDRPTIMEOFDAY && HDPipeline
        public void SetNewSettings(VolumetricClouds volumetricClouds)
        {
            CachedHasBeenSet = true;
            DensityMultiplier = volumetricClouds.densityMultiplier.value;
            ShapeFactor = volumetricClouds.shapeFactor.value;
            ShapeScale = volumetricClouds.shapeScale.value;
            EarthCurvature = volumetricClouds.earthCurvature.value;
#if UNITY_2022_2_OR_NEWER
            BottomAltitude = volumetricClouds.bottomAltitude.value;
            AltitudeRange = volumetricClouds.altitudeRange.value;
#else
            BottomAltitude = volumetricClouds.lowestCloudAltitude.value;
            AltitudeRange = volumetricClouds.cloudThickness.value;
#endif
            AmbientLightProbeDimmer = volumetricClouds.ambientLightProbeDimmer.value;
            SunLightDimmer = volumetricClouds.sunLightDimmer.value;
            ErosionOcclusion = volumetricClouds.erosionOcclusion.value;
            ScatteringTint = volumetricClouds.scatteringTint.value;
            PowderEffectIntensity = volumetricClouds.powderEffectIntensity.value;
            MultiScattering = volumetricClouds.multiScattering.value;
        }
#endif
    }

    /// <summary>
    /// Volumetric Cloud Preset
    /// </summary>
    [CreateAssetMenu(fileName = "HDRP TOD Volumetric Cloud Preset", menuName = "Procedural Worlds/HDRP Time Of Day/Profiles/HDRP TOD Volumetric Cloud Preset")]
    public class HDRPTimeOfDayVolumetricCloudPreset : ScriptableObject
    {
        [Header("Global")]
        public string Name;
        [Header("Shape")]
        [Range(0f, 1f)]
        public float DensityMultiplier = 0.4f;
        public AnimationCurve DensityCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.15f, 1f), new Keyframe(1f, 0.1f));
        [Range(0f, 1f)]
        public float ShapeFactor = 0.9f;
        public float ShapeScale = 5f;
        [Range(0f, 1f)]
        public float ErosionFactor = 0.8f;
        public float ErosionScale = 107f;
#if HDRPTIMEOFDAY && HDPipeline
        public VolumetricClouds.CloudErosionNoise ErosionNoiseType = VolumetricClouds.CloudErosionNoise.Perlin32;
#endif
        public AnimationCurve ErosionCurve = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(0.1f, 0.9f), new Keyframe(1f, 1f));
        public AnimationCurve AmbientOcclusionCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.25f, 0.4f), new Keyframe(1f, 0f));
        public float BottomAltitude = 1200f;
        public float AltitudeRange = 2000f;
        public Vector3 ShapeOffset = Vector3.zero;
        [Header("Wind")]
        public AnimationCurve WindDirection = new AnimationCurve(new Keyframe(0f, 0.1f), new Keyframe(0.2f, 0.2f), new Keyframe(0.5f, 0.25f), new Keyframe(0.8f, 0.2f), new Keyframe(1f, 0.1f));
        public AnimationCurve WindSpeed = new AnimationCurve(new Keyframe(0f, 10f), new Keyframe(0.5f, 40f), new Keyframe(1f, 10f));
        [Range(0f, 1f)]
        public float EarthCurvature = 0.8f;
        [Header("Lighting")]
        [Range(0f, 1f)]
        public float AmbientLightProbeDimmer = 1f;
        [Range(0f, 1f)]
        public float SunLightDimmer = 1f;
        [Range(0f, 1f)]
        public float ErosionOcclusion = 0.2f;
        public Color ScatteringTint = Color.black;
        [Range(0f, 1f)]
        public float PowderEffectIntensity = 0.2f;
        [Range(0f, 1f)]
        public float MultiScattering = 0.2f;
        [Header("Shadowing")]
        public bool EnableShadows = true;
#if HDRPTIMEOFDAY && HDPipeline
        public VolumetricClouds.CloudShadowResolution ShadowResolution = VolumetricClouds.CloudShadowResolution.Medium256;
#endif
        public AnimationCurve CloudShadowOpacity = new AnimationCurve(new Keyframe(0f, 0.9f), new Keyframe(0.2f, 0.8f), new Keyframe(0.5f, 0.7f), new Keyframe(0.8f, 0.8f), new Keyframe(1f, 0.9f));
        public float CloudHeightMultiplier = 1f;
        public bool UseHeightBasedVolumetricCloudShadows = false;
        [Range(0f, 1f)]
        public float CloudMinShadowClampValue = 0.35f;
        public float CloudShadowsGroundHeight = 300f;

        /// <summary>
        /// Cached cloud settings to blend with
        /// </summary>
        private CachedVolumetricCloudSettings m_cachedSettings = new CachedVolumetricCloudSettings();

        /// <summary>
        /// Applies the settings to the cloud component
        /// </summary>
        /// <param name="clouds"></param>
#if HDRPTIMEOFDAY && HDPipeline
        public void Apply(VolumetricClouds clouds)
        {
            if (clouds != null)
            {
                //Shape
                clouds.densityMultiplier.value = DensityMultiplier;         
                clouds.shapeFactor.value = ShapeFactor;
                clouds.shapeScale.value = ShapeScale;
                clouds.erosionFactor.value = ErosionFactor;
                clouds.erosionScale.value = ErosionScale;
                clouds.erosionNoiseType.value = ErosionNoiseType;
                clouds.shapeOffset.value = ShapeOffset;
                clouds.earthCurvature.value = EarthCurvature;
#if UNITY_2022_2_OR_NEWER
                clouds.erosionCurve.value = ErosionCurve;
                clouds.densityCurve.value = DensityCurve;
                clouds.ambientOcclusionCurve.value = AmbientOcclusionCurve;
                clouds.bottomAltitude.value = BottomAltitude;
                clouds.altitudeRange.value = AltitudeRange;
#else
                clouds.customErosionCurve.value = ErosionCurve;
                clouds.customDensityCurve.value = DensityCurve;
                clouds.customAmbientOcclusionCurve.value = AmbientOcclusionCurve;
                clouds.lowestCloudAltitude.value = BottomAltitude;
                clouds.cloudThickness.value = AltitudeRange;
#endif
                //Lighting
                clouds.ambientLightProbeDimmer.value = AmbientLightProbeDimmer;
                clouds.sunLightDimmer.value = SunLightDimmer;
                clouds.erosionOcclusion.value = ErosionOcclusion;
                clouds.scatteringTint.value = ScatteringTint;
                clouds.powderEffectIntensity.value = PowderEffectIntensity;
                clouds.multiScattering.value = MultiScattering;
                //Shadows
                clouds.shadows.value = EnableShadows;
                clouds.shadowResolution.value = ShadowResolution;
            }
        } 
        /// <summary>
        /// Applies volumetric cloud shadows
        /// </summary>
        /// <param name="clouds"></param>
        /// <param name="time"></param>
        /// <param name="groundLayers"></param>
        public void ApplyShadows(VolumetricClouds clouds, float time, LayerMask groundLayers)
        {
            if (EnableShadows)
            {
#if HDRPTIMEOFDAY
                if (UseHeightBasedVolumetricCloudShadows)
                {
                    Transform player = HDRPTimeOfDay.Instance.Player;
                    if (player != null)
                    {
                        if (Physics.Raycast(player.position, Vector3.down, out RaycastHit hit, 3000f, groundLayers))
                        {
                            if (hit.distance < CloudShadowsGroundHeight)
                            {
                                clouds.shadowOpacity.value = Mathf.Clamp(hit.distance / CloudShadowsGroundHeight, CloudMinShadowClampValue, 1f);
                            }
                            else
                            {
                                clouds.shadowOpacity.value = 1f;
                            }
                        }
                        else
                        {
                            clouds.shadowOpacity.value = CloudShadowOpacity.Evaluate(time);
                        }
                    }
                    else
                    {
                        clouds.shadowOpacity.value = CloudShadowOpacity.Evaluate(time);
                    }
                }
                else
                {
                    clouds.shadowOpacity.value = CloudShadowOpacity.Evaluate(time);
                }
#endif
            }
        }
        /// <summary>
        /// Applies the cloud wind settings
        /// </summary>
        /// <param name="visualEnvironment"></param>
        /// <param name="time"></param>
        public void ApplyWind(VisualEnvironment visualEnvironment, float time)
        {
            if (visualEnvironment != null)
            {
                visualEnvironment.windOrientation.value = WindDirection.Evaluate(time);
                visualEnvironment.windSpeed.value = WindSpeed.Evaluate(time);
            }
        }
        /// <summary>
        /// Blends current to this preset
        /// </summary>
        /// <param name="clouds"></param>
        /// <param name="time"></param>
        public void Blend(VolumetricClouds clouds, float time)
        {
            if (!m_cachedSettings.CachedHasBeenSet)
            {
                Debug.LogWarning("Cache volumetric settings has not been setup. Cloud blending with presets will not be proceed when you call Blend().");
                return;
            }        

            //Shape
            clouds.densityMultiplier.value = Mathf.Lerp(m_cachedSettings.DensityMultiplier, DensityMultiplier, time);
            clouds.shapeFactor.value = Mathf.Lerp(m_cachedSettings.ShapeFactor, ShapeFactor, time);
            clouds.shapeScale.value = Mathf.Lerp(m_cachedSettings.ShapeScale, ShapeScale, time);
            clouds.earthCurvature.value = Mathf.Lerp(m_cachedSettings.EarthCurvature, EarthCurvature, time);
#if UNITY_2022_2_OR_NEWER
            clouds.bottomAltitude.value = Mathf.Lerp(m_cachedSettings.BottomAltitude, BottomAltitude, time);
            clouds.altitudeRange.value = Mathf.Lerp(m_cachedSettings.AltitudeRange, AltitudeRange, time);
#else
            clouds.lowestCloudAltitude.value = Mathf.Lerp(m_cachedSettings.BottomAltitude, BottomAltitude, time);
            clouds.cloudThickness.value = Mathf.Lerp(m_cachedSettings.AltitudeRange, AltitudeRange, time);
#endif
            //Lighting
            clouds.ambientLightProbeDimmer.value = Mathf.Lerp(m_cachedSettings.AmbientLightProbeDimmer, AmbientLightProbeDimmer, time);
            clouds.sunLightDimmer.value = Mathf.Lerp(m_cachedSettings.SunLightDimmer, SunLightDimmer, time);
            clouds.erosionOcclusion.value = Mathf.Lerp(m_cachedSettings.ErosionOcclusion, ErosionOcclusion, time);
            clouds.scatteringTint.value = Color.Lerp(m_cachedSettings.ScatteringTint, ScatteringTint, time);
            clouds.powderEffectIntensity.value = Mathf.Lerp(m_cachedSettings.PowderEffectIntensity, PowderEffectIntensity, time);
            clouds.multiScattering.value = Mathf.Lerp(m_cachedSettings.MultiScattering, MultiScattering, time);
        }
        /// <summary>
        /// Assigns the new cached settings
        /// This need to be set for a successful cloud blend if you are using the Blend function
        /// </summary>
        /// <param name="volumetricClouds"></param>
        public void SetCachedCloudSettings(VolumetricClouds volumetricClouds)
        {
            m_cachedSettings.SetNewSettings(volumetricClouds);
        }
#endif
    }
}