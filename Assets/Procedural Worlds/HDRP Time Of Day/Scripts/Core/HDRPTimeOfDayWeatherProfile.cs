#if HDPipeline && UNITY_2021_2_OR_NEWER
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace ProceduralWorlds.HDRPTOD
{
    [System.Serializable]
    public class HDRPTimeOfDayWeatherProfileData
    {
        public string m_weatherName;
        public float m_transitionDuration = 10f;
        public Vector2 m_weatherDuration = new Vector2(120f, 300f);
        public TimeOfDayProfileData m_weatherData;

        private float m_lastTimeOfDayValue = -1f;
        private TimeOfDayProfileData m_startingData;
        private bool m_hasBeenSetup = false;

#if UNITY_2022_2_OR_NEWER
        /// <summary>
        /// Applies the weather and lerps the values from current to this profile settings
        /// </summary>
        /// <param name="components"></param>
        /// <param name="isDay"></param>
        /// <param name="time"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        public bool ApplyWeather(HDRPTimeOfDayComponents components, bool isDay, bool overrideLightSource, float time, float duration)
        {
            if (!m_hasBeenSetup)
            {
                Debug.LogError(" Weather was not setup with the starting values please call SetupStartingSettings() before calling Apply weather");
                return false;
            }

            components.m_weatherBlendVolume.weight = duration;
            HDAdditionalLightData lightData = isDay ? components.m_sunLightData : components.m_moonLightData;
            //Sun
            if (ValidateSun())
            {
                if (!overrideLightSource)
                {
                    if (isDay)
                    {
                        lightData.SetIntensity(LerpFloat(m_startingData.m_sunIntensity.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_sunIntensity.Evaluate(time) * m_weatherData.m_globalLightMultiplier, duration));
                        lightData.SetColor(LerpColor(m_startingData.m_sunColorFilter.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_sunColorFilter.Evaluate(time), duration), LerpFloat(m_startingData.m_sunTemperature.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_sunTemperature.Evaluate(time), duration));
                    }
                    else
                    {
                        lightData.SetIntensity(LerpFloat(m_startingData.m_moonIntensity.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_moonIntensity.Evaluate(time) * m_weatherData.m_globalLightMultiplier, duration));
                        lightData.SetColor(LerpColor(m_startingData.m_moonColorFilter.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_moonColorFilter.Evaluate(time), duration), LerpFloat(m_startingData.m_moonTemperature.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_moonTemperature.Evaluate(time), duration));
                    }
                }

                lightData.volumetricDimmer = LerpFloat(m_startingData.m_sunVolumetrics.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_sunVolumetrics.Evaluate(time), duration);
                lightData.volumetricShadowDimmer = LerpFloat(m_startingData.m_sunVolumetricShadowDimmer.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_sunVolumetricShadowDimmer.Evaluate(time), duration);
                lightData.lightDimmer = LerpFloat(m_startingData.m_sunIntensityMultiplier.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_sunIntensityMultiplier.Evaluate(time), duration);
            }

            //Advanced Lighting
            if (ValidateAdvancedLighting())
            {
                //Exposure
                components.m_weatherVolumeComponenets.m_exposure.fixedExposure.value = LerpFloat(m_startingData.m_generalExposure.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_generalExposure.Evaluate(time), duration);
                components.m_weatherVolumeComponenets.m_indirectLightingController.indirectDiffuseLightingMultiplier.value = LerpFloat(m_startingData.m_ambientIntensity.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_ambientIntensity.Evaluate(time) * m_weatherData.m_ambientIntensityMultiplier, duration);
                components.m_weatherVolumeComponenets.m_indirectLightingController.reflectionLightingMultiplier.value = LerpFloat(m_startingData.m_ambientReflectionIntensity.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_ambientReflectionIntensity.Evaluate(time), duration);
                components.m_weatherVolumeComponenets.m_indirectLightingController.reflectionProbeIntensityMultiplier.value = LerpFloat(m_startingData.m_planarReflectionIntensity.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_planarReflectionIntensity.Evaluate(time), duration);
            }
            //Fog
            if (ValidateFog())
            {
                //Local Fog
                components.m_localVolumetricFog.parameters.meanFreePath = LerpFloat(m_startingData.m_fogDensity.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_fogDensity.Evaluate(time) / m_weatherData.m_globalFogMultiplier, duration);
                components.m_localVolumetricFog.parameters.albedo = LerpColor(m_startingData.m_fogColor.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_fogColor.Evaluate(time) * m_weatherData.m_localFogMultiplier.Evaluate(time), duration);
                HDRPTimeOfDay.Instance.SetCurrentLocalFogDistance(components.m_localVolumetricFog.parameters.meanFreePath);
                //Global
                components.m_weatherVolumeComponenets.m_fog.albedo.value = LerpColor(m_startingData.m_fogColor.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_fogColor.Evaluate(time), duration);
                components.m_weatherVolumeComponenets.m_fog.meanFreePath.value = LerpFloat(m_startingData.m_fogDistance.Evaluate(m_lastTimeOfDayValue),m_weatherData.m_fogDistance.Evaluate(time) / m_weatherData.m_globalFogMultiplier, duration);
                float fogHeight = m_weatherData.m_fogHeight.Evaluate(time);
                components.m_weatherVolumeComponenets.m_fog.baseHeight.value = LerpFloat(m_startingData.m_fogHeight.Evaluate(m_lastTimeOfDayValue), fogHeight, duration);
                components.m_weatherVolumeComponenets.m_fog.maximumHeight.value = LerpFloat(m_startingData.m_fogHeight.Evaluate(m_lastTimeOfDayValue), fogHeight * 2f, duration);
                components.m_weatherVolumeComponenets.m_fog.depthExtent.value = LerpFloat(m_startingData.m_volumetricFogDistance.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_volumetricFogDistance.Evaluate(time), duration);
                components.m_weatherVolumeComponenets.m_fog.anisotropy.value = LerpFloat(m_startingData.m_volumetricFogAnisotropy.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_volumetricFogAnisotropy.Evaluate(time), duration);
                components.m_weatherVolumeComponenets.m_fog.sliceDistributionUniformity.value = LerpFloat(m_startingData.m_volumetricFogSliceDistributionUniformity.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_volumetricFogSliceDistributionUniformity.Evaluate(time), duration);
            }
            //Shadows
            if (ValidateShadows())
            {
                components.m_weatherVolumeComponenets.m_shadows.maxShadowDistance.value = LerpFloat(m_startingData.m_shadowDistance.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_shadowDistance.Evaluate(time) * m_weatherData.m_shadowDistanceMultiplier, duration);
                components.m_weatherVolumeComponenets.m_shadows.directionalTransmissionMultiplier.value = LerpFloat(m_startingData.m_shadowTransmissionMultiplier.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_shadowTransmissionMultiplier.Evaluate(time), duration);
                components.m_weatherVolumeComponenets.m_contactShadows.maxDistance.value = LerpFloat(m_startingData.m_contactShadowsDistance.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_contactShadowsDistance.Evaluate(time), duration);
            }
            //Clouds
            if (ValidateClouds())
            {
                //Volumetric
                #region Volumetric

#if UNITY_2022_3_OR_NEWER
                components.m_weatherVolumeComponenets.m_volumetricClouds.cloudPreset = VolumetricClouds.CloudPresets.Custom;
#else
                components.m_weatherVolumeComponenets.m_volumetricClouds.cloudPreset.value = VolumetricClouds.CloudPresets.Custom;
#endif
                if (m_weatherData.m_cloudPresets == TODCloudPresets.Custom)
                {
                    components.m_weatherVolumeComponenets.m_volumetricClouds.densityMultiplier.value = LerpFloat(m_startingData.m_volumetricDensityMultiplier.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_volumetricDensityMultiplier.Evaluate(time), duration);
                    components.m_weatherVolumeComponenets.m_volumetricClouds.shapeFactor.value = LerpFloat(m_startingData.m_volumetricShapeFactor.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_volumetricShapeFactor.Evaluate(time), duration);
                    components.m_weatherVolumeComponenets.m_volumetricClouds.shapeScale.value = LerpFloat(m_startingData.m_volumetricShapeScale.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_volumetricShapeScale.Evaluate(time), duration);
                    components.m_weatherVolumeComponenets.m_volumetricClouds.erosionFactor.value = LerpFloat(m_startingData.m_volumetricErosionFactor.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_volumetricErosionFactor.Evaluate(time), duration);
                    components.m_weatherVolumeComponenets.m_volumetricClouds.erosionScale.value = LerpFloat(m_startingData.m_volumetricErosionScale.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_volumetricErosionScale.Evaluate(time), duration);
                    components.m_weatherVolumeComponenets.m_volumetricClouds.densityCurve.value = m_weatherData.m_volumetricDensityCurve;
                    components.m_weatherVolumeComponenets.m_volumetricClouds.bottomAltitude.value = LerpFloat(m_startingData.m_volumetricLowestCloudAltitude.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_volumetricLowestCloudAltitude.Evaluate(time), duration);
                    components.m_weatherVolumeComponenets.m_volumetricClouds.altitudeRange.value = LerpFloat(m_startingData.m_volumetricCloudThickness.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_volumetricCloudThickness.Evaluate(time), duration);
                }
                else
                {
#if HDRPTIMEOFDAY
                    m_weatherData.VolumetricCloudPreset.Apply(components.m_weatherVolumeComponenets.m_volumetricClouds);
#endif
                }

                components.m_weatherVolumeComponenets.m_volumetricClouds.ambientLightProbeDimmer.value = LerpFloat(m_startingData.m_volumetricAmbientLightProbeDimmer.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_volumetricAmbientLightProbeDimmer.Evaluate(time), duration);
                components.m_weatherVolumeComponenets.m_volumetricClouds.sunLightDimmer.value = LerpFloat(m_startingData.m_volumetricSunLightDimmer.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_volumetricSunLightDimmer.Evaluate(time), duration);
                components.m_weatherVolumeComponenets.m_volumetricClouds.erosionOcclusion.value = LerpFloat(m_startingData.m_volumetricErosionOcclusion.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_volumetricErosionOcclusion.Evaluate(time), duration);
                components.m_weatherVolumeComponenets.m_volumetricClouds.scatteringTint.value = LerpColor(m_startingData.m_volumetricScatteringTint.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_volumetricScatteringTint.Evaluate(time), duration);
                components.m_weatherVolumeComponenets.m_volumetricClouds.powderEffectIntensity.value = LerpFloat(m_startingData.m_volumetricPowderEffectIntensity.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_volumetricPowderEffectIntensity.Evaluate(time), duration);
                components.m_weatherVolumeComponenets.m_volumetricClouds.multiScattering.value = LerpFloat(m_startingData.m_volumetricMultiScattering.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_volumetricMultiScattering.Evaluate(time), duration);
                components.m_weatherVolumeComponenets.m_volumetricClouds.shadowOpacity.value = LerpFloat(m_startingData.m_volumetricCloudShadowOpacity.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_volumetricCloudShadowOpacity.Evaluate(time), duration);

                #endregion
                #region Procedural

                //Procedural
                components.m_weatherVolumeComponenets.m_cloudLayer.opacity.value = LerpFloat(m_startingData.m_cloudOpacity.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_cloudOpacity.Evaluate(time), duration);
                components.m_weatherVolumeComponenets.m_cloudLayer.layerA.tint.value = LerpColor(m_startingData.m_cloudTintColor.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_cloudTintColor.Evaluate(time), duration);
                components.m_weatherVolumeComponenets.m_cloudLayer.layerB.tint.value = LerpColor(m_startingData.m_cloudTintColor.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_cloudTintColor.Evaluate(time), duration);
                components.m_weatherVolumeComponenets.m_cloudLayer.layerA.exposure.value = LerpFloat(m_startingData.m_cloudExposure.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_cloudExposure.Evaluate(time), duration);
                components.m_weatherVolumeComponenets.m_cloudLayer.layerB.exposure.value = LerpFloat(m_startingData.m_cloudExposure.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_cloudExposure.Evaluate(time), duration);
                components.m_weatherVolumeComponenets.m_cloudLayer.shadowMultiplier.value = LerpFloat(m_startingData.m_cloudShadowOpacity.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_cloudShadowOpacity.Evaluate(time), duration);
                components.m_weatherVolumeComponenets.m_cloudLayer.shadowTint.value = LerpColor(m_startingData.m_cloudShadowColor.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_cloudShadowColor.Evaluate(time), duration);
                switch (m_weatherData.m_cloudLayerAChannel)
                {
                    case CloudLayerChannelMode.R:
                    {
                        components.m_weatherVolumeComponenets.m_cloudLayer.layerA.opacityR.value = LerpFloat(m_startingData.m_cloudLayerAOpacityR.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_cloudLayerAOpacityR.Evaluate(time), duration);
                        components.m_weatherVolumeComponenets.m_cloudLayer.layerA.opacityG.value = 0f;
                        components.m_weatherVolumeComponenets.m_cloudLayer.layerA.opacityB.value = 0f;
                        components.m_weatherVolumeComponenets.m_cloudLayer.layerA.opacityA.value = 0f;
                        break;
                    }
                    case CloudLayerChannelMode.G:
                    {
                        components.m_weatherVolumeComponenets.m_cloudLayer.layerA.opacityG.value = LerpFloat(m_startingData.m_cloudLayerAOpacityR.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_cloudLayerAOpacityR.Evaluate(time), duration);
                        components.m_weatherVolumeComponenets.m_cloudLayer.layerA.opacityR.value = 0f;
                        components.m_weatherVolumeComponenets.m_cloudLayer.layerA.opacityB.value = 0f;
                        components.m_weatherVolumeComponenets.m_cloudLayer.layerA.opacityA.value = 0f;
                        break;
                    }
                    case CloudLayerChannelMode.B:
                    {
                        components.m_weatherVolumeComponenets.m_cloudLayer.layerA.opacityB.value = LerpFloat(m_startingData.m_cloudLayerAOpacityR.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_cloudLayerAOpacityR.Evaluate(time), duration);
                        components.m_weatherVolumeComponenets.m_cloudLayer.layerA.opacityR.value = 0f;
                        components.m_weatherVolumeComponenets.m_cloudLayer.layerA.opacityG.value = 0f;
                        components.m_weatherVolumeComponenets.m_cloudLayer.layerA.opacityA.value = 0f;
                        break;
                    }
                    case CloudLayerChannelMode.A:
                    {
                        components.m_weatherVolumeComponenets.m_cloudLayer.layerA.opacityA.value = LerpFloat(m_startingData.m_cloudLayerAOpacityR.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_cloudLayerAOpacityR.Evaluate(time), duration);
                        components.m_weatherVolumeComponenets.m_cloudLayer.layerA.opacityR.value = 0f;
                        components.m_weatherVolumeComponenets.m_cloudLayer.layerA.opacityG.value = 0f;
                        components.m_weatherVolumeComponenets.m_cloudLayer.layerA.opacityB.value = 0f;
                        break;
                    }
                }
                switch (m_weatherData.m_cloudLayerBChannel)
                {
                    case CloudLayerChannelMode.R:
                    {
                        components.m_weatherVolumeComponenets.m_cloudLayer.layerB.opacityR.value = LerpFloat(m_startingData.m_cloudLayerBOpacityR.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_cloudLayerBOpacityR.Evaluate(time), duration);
                        components.m_weatherVolumeComponenets.m_cloudLayer.layerB.opacityG.value = 0f;
                        components.m_weatherVolumeComponenets.m_cloudLayer.layerB.opacityB.value = 0f;
                        components.m_weatherVolumeComponenets.m_cloudLayer.layerB.opacityA.value = 0f;
                        break;
                    }
                    case CloudLayerChannelMode.G:
                    {
                        components.m_weatherVolumeComponenets.m_cloudLayer.layerB.opacityG.value = LerpFloat(m_startingData.m_cloudLayerBOpacityR.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_cloudLayerBOpacityR.Evaluate(time), duration);
                        components.m_weatherVolumeComponenets.m_cloudLayer.layerB.opacityR.value = 0f;
                        components.m_weatherVolumeComponenets.m_cloudLayer.layerB.opacityB.value = 0f;
                        components.m_weatherVolumeComponenets.m_cloudLayer.layerB.opacityA.value = 0f;
                        break;
                    }
                    case CloudLayerChannelMode.B:
                    {
                        components.m_weatherVolumeComponenets.m_cloudLayer.layerB.opacityB.value = LerpFloat(m_startingData.m_cloudLayerBOpacityR.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_cloudLayerBOpacityR.Evaluate(time), duration);
                        components.m_weatherVolumeComponenets.m_cloudLayer.layerB.opacityR.value = 0f;
                        components.m_weatherVolumeComponenets.m_cloudLayer.layerB.opacityG.value = 0f;
                        components.m_weatherVolumeComponenets.m_cloudLayer.layerB.opacityA.value = 0f;
                        break;
                    }
                    case CloudLayerChannelMode.A:
                    {
                        components.m_weatherVolumeComponenets.m_cloudLayer.layerB.opacityA.value = LerpFloat(m_startingData.m_cloudLayerBOpacityR.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_cloudLayerBOpacityR.Evaluate(time), duration);
                        components.m_weatherVolumeComponenets.m_cloudLayer.layerB.opacityR.value = 0f;
                        components.m_weatherVolumeComponenets.m_cloudLayer.layerB.opacityG.value = 0f;
                        components.m_weatherVolumeComponenets.m_cloudLayer.layerB.opacityB.value = 0f;
                        break;
                    }
                }

                #endregion
            }
            //Sun Flare
            if (ValidateSunLensFlare())
            {
                components.m_sunLensFlare.enabled = m_weatherData.m_sunLensFlareProfile.m_useLensFlare;
                if (m_weatherData.m_sunLensFlareProfile.m_useLensFlare)
                {
                    components.m_sunLensFlare.intensity = LerpFloat(m_startingData.m_sunLensFlareProfile.m_intensity.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_sunLensFlareProfile.m_intensity.Evaluate(time), duration);
                    components.m_sunLensFlare.scale = LerpFloat(m_startingData.m_sunLensFlareProfile.m_scale.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_sunLensFlareProfile.m_scale.Evaluate(time), duration);
                }
            }
            //Moon Flare
            if (ValidateMoonLensFlare())
            {
                components.m_moonLensFlare.enabled = m_weatherData.m_moonLensFlareProfile.m_useLensFlare;
                if (m_weatherData.m_moonLensFlareProfile.m_useLensFlare)
                {
                    components.m_moonLensFlare.intensity = LerpFloat(m_startingData.m_moonLensFlareProfile.m_intensity.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_moonLensFlareProfile.m_intensity.Evaluate(time), duration);
                    components.m_moonLensFlare.scale = LerpFloat(m_startingData.m_moonLensFlareProfile.m_scale.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_moonLensFlareProfile.m_scale.Evaluate(time), duration);
                }
            }


            return duration >= 1f;
        }
#else
        /// <summary>
        /// Applies the weather and lerps the values from current to this profile settings
        /// </summary>
        /// <param name="components"></param>
        /// <param name="isDay"></param>
        /// <param name="time"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        public bool ApplyWeather(HDRPTimeOfDayComponents components, bool isDay, bool overrideLightSource, float time, float duration)
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
                        lightData.SetIntensity(LerpFloat(m_startingData.m_sunIntensity.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_sunIntensity.Evaluate(time) * m_weatherData.m_globalLightMultiplier, duration));
                        lightData.SetColor(LerpColor(m_startingData.m_sunColorFilter.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_sunColorFilter.Evaluate(time), duration), LerpFloat(m_startingData.m_sunTemperature.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_sunTemperature.Evaluate(time), duration));
                    }
                    else
                    {
                        lightData.SetIntensity(LerpFloat(m_startingData.m_moonIntensity.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_moonIntensity.Evaluate(time) * m_weatherData.m_globalLightMultiplier, duration));
                        lightData.SetColor(LerpColor(m_startingData.m_moonColorFilter.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_moonColorFilter.Evaluate(time), duration), LerpFloat(m_startingData.m_moonTemperature.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_moonTemperature.Evaluate(time), duration));
                    }
                }

                lightData.volumetricDimmer = LerpFloat(m_startingData.m_sunVolumetrics.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_sunVolumetrics.Evaluate(time), duration);
                lightData.volumetricShadowDimmer = LerpFloat(m_startingData.m_sunVolumetricShadowDimmer.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_sunVolumetricShadowDimmer.Evaluate(time), duration);
                lightData.lightDimmer = LerpFloat(m_startingData.m_sunIntensityMultiplier.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_sunIntensityMultiplier.Evaluate(time), duration);
            }
            //Advanced Lighting
            if (ValidateAdvancedLighting())
            {
                //Exposure
                components.m_timeOfDayVolumeComponenets.m_exposure.fixedExposure.value = LerpFloat(m_startingData.m_generalExposure.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_generalExposure.Evaluate(time), duration);
                components.m_timeOfDayVolumeComponenets.m_indirectLightingController.indirectDiffuseLightingMultiplier.value = LerpFloat(m_startingData.m_ambientIntensity.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_ambientIntensity.Evaluate(time) * m_weatherData.m_ambientIntensityMultiplier, duration);
                components.m_timeOfDayVolumeComponenets.m_indirectLightingController.reflectionLightingMultiplier.value = LerpFloat(m_startingData.m_ambientReflectionIntensity.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_ambientReflectionIntensity.Evaluate(time), duration);
                components.m_timeOfDayVolumeComponenets.m_indirectLightingController.reflectionProbeIntensityMultiplier.value = LerpFloat(m_startingData.m_planarReflectionIntensity.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_planarReflectionIntensity.Evaluate(time), duration);
            }
            //Fog
            if (ValidateFog())
            {
                //Local Fog
                components.m_localVolumetricFog.parameters.meanFreePath = LerpFloat(m_startingData.m_fogDensity.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_fogDensity.Evaluate(time) / m_weatherData.m_globalFogMultiplier, duration);
                components.m_localVolumetricFog.parameters.albedo = LerpColor(m_startingData.m_fogColor.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_fogColor.Evaluate(time) * m_weatherData.m_localFogMultiplier.Evaluate(time), duration);
                HDRPTimeOfDay.Instance.SetCurrentLocalFogDistance(components.m_localVolumetricFog.parameters.meanFreePath);
                //Global
                components.m_timeOfDayVolumeComponenets.m_fog.albedo.value = LerpColor(m_startingData.m_fogColor.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_fogColor.Evaluate(time), duration);
                components.m_timeOfDayVolumeComponenets.m_fog.meanFreePath.value = LerpFloat(m_startingData.m_fogDistance.Evaluate(m_lastTimeOfDayValue),m_weatherData.m_fogDistance.Evaluate(time) / m_weatherData.m_globalFogMultiplier, duration);
                float fogHeight = m_weatherData.m_fogHeight.Evaluate(time);
                components.m_timeOfDayVolumeComponenets.m_fog.baseHeight.value = LerpFloat(m_startingData.m_fogHeight.Evaluate(m_lastTimeOfDayValue), fogHeight, duration);
                components.m_timeOfDayVolumeComponenets.m_fog.maximumHeight.value = LerpFloat(m_startingData.m_fogHeight.Evaluate(m_lastTimeOfDayValue), fogHeight * 2f, duration);
                components.m_timeOfDayVolumeComponenets.m_fog.depthExtent.value = LerpFloat(m_startingData.m_volumetricFogDistance.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_volumetricFogDistance.Evaluate(time), duration);
                components.m_timeOfDayVolumeComponenets.m_fog.anisotropy.value = LerpFloat(m_startingData.m_volumetricFogAnisotropy.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_volumetricFogAnisotropy.Evaluate(time), duration);
                components.m_timeOfDayVolumeComponenets.m_fog.sliceDistributionUniformity.value = LerpFloat(m_startingData.m_volumetricFogSliceDistributionUniformity.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_volumetricFogSliceDistributionUniformity.Evaluate(time), duration);
            }
            //Shadows
            if (ValidateShadows())
            {
                components.m_timeOfDayVolumeComponenets.m_shadows.maxShadowDistance.value = LerpFloat(m_startingData.m_shadowDistance.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_shadowDistance.Evaluate(time) * m_weatherData.m_shadowDistanceMultiplier, duration);
                components.m_timeOfDayVolumeComponenets.m_shadows.directionalTransmissionMultiplier.value = LerpFloat(m_startingData.m_shadowTransmissionMultiplier.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_shadowTransmissionMultiplier.Evaluate(time), duration);
                components.m_timeOfDayVolumeComponenets.m_contactShadows.maxDistance.value = LerpFloat(m_startingData.m_contactShadowsDistance.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_contactShadowsDistance.Evaluate(time), duration);
            }
            //Clouds
            if (ValidateClouds())
            {
                //Volumetric

                #region Volumetric
              
                if (m_weatherData.m_cloudPresets == TODCloudPresets.Custom)
                {
                    components.m_timeOfDayVolumeComponenets.m_volumetricClouds.densityMultiplier.value = LerpFloat(m_startingData.m_volumetricDensityMultiplier.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_volumetricDensityMultiplier.Evaluate(time), duration);
                    components.m_timeOfDayVolumeComponenets.m_volumetricClouds.shapeFactor.value = LerpFloat(m_startingData.m_volumetricShapeFactor.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_volumetricShapeFactor.Evaluate(time), duration);
                    components.m_timeOfDayVolumeComponenets.m_volumetricClouds.shapeScale.value = LerpFloat(m_startingData.m_volumetricShapeScale.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_volumetricShapeScale.Evaluate(time), duration);
                    components.m_timeOfDayVolumeComponenets.m_volumetricClouds.erosionFactor.value = LerpFloat(m_startingData.m_volumetricErosionFactor.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_volumetricErosionFactor.Evaluate(time), duration);
                    components.m_timeOfDayVolumeComponenets.m_volumetricClouds.erosionScale.value = LerpFloat(m_startingData.m_volumetricErosionScale.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_volumetricErosionScale.Evaluate(time), duration);
                    components.m_timeOfDayVolumeComponenets.m_volumetricClouds.customDensityCurve.value = m_weatherData.m_volumetricDensityCurve;
                    components.m_timeOfDayVolumeComponenets.m_volumetricClouds.lowestCloudAltitude.value = LerpFloat(m_startingData.m_volumetricLowestCloudAltitude.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_volumetricLowestCloudAltitude.Evaluate(time), duration);
                    components.m_timeOfDayVolumeComponenets.m_volumetricClouds.cloudThickness.value = LerpFloat(m_startingData.m_volumetricCloudThickness.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_volumetricCloudThickness.Evaluate(time), duration);
                }
                else
                {
#if HDRPTIMEOFDAY
                    m_weatherData.VolumetricCloudPreset.Blend(components.m_timeOfDayVolumeComponenets.m_volumetricClouds, duration);
#endif
                }

                components.m_timeOfDayVolumeComponenets.m_volumetricClouds.ambientLightProbeDimmer.value = LerpFloat(m_startingData.m_volumetricAmbientLightProbeDimmer.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_volumetricAmbientLightProbeDimmer.Evaluate(time), duration);
                components.m_timeOfDayVolumeComponenets.m_volumetricClouds.sunLightDimmer.value = LerpFloat(m_startingData.m_volumetricSunLightDimmer.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_volumetricSunLightDimmer.Evaluate(time), duration);
                components.m_timeOfDayVolumeComponenets.m_volumetricClouds.erosionOcclusion.value = LerpFloat(m_startingData.m_volumetricErosionOcclusion.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_volumetricErosionOcclusion.Evaluate(time), duration);
                components.m_timeOfDayVolumeComponenets.m_volumetricClouds.scatteringTint.value = LerpColor(m_startingData.m_volumetricScatteringTint.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_volumetricScatteringTint.Evaluate(time), duration);
                components.m_timeOfDayVolumeComponenets.m_volumetricClouds.powderEffectIntensity.value = LerpFloat(m_startingData.m_volumetricPowderEffectIntensity.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_volumetricPowderEffectIntensity.Evaluate(time), duration);
                components.m_timeOfDayVolumeComponenets.m_volumetricClouds.multiScattering.value = LerpFloat(m_startingData.m_volumetricMultiScattering.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_volumetricMultiScattering.Evaluate(time), duration);
                components.m_timeOfDayVolumeComponenets.m_volumetricClouds.shadowOpacity.value = LerpFloat(m_startingData.m_volumetricCloudShadowOpacity.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_volumetricCloudShadowOpacity.Evaluate(time), duration);

                #endregion
                #region Procedural

                if (m_weatherData.m_cloudPresets == TODCloudPresets.Custom)
                {
                    //Procedural
                    components.m_timeOfDayVolumeComponenets.m_cloudLayer.opacity.value = LerpFloat(m_startingData.m_cloudOpacity.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_cloudOpacity.Evaluate(time), duration);
                    components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerA.tint.value = LerpColor(m_startingData.m_cloudTintColor.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_cloudTintColor.Evaluate(time), duration);
                    components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerB.tint.value = LerpColor(m_startingData.m_cloudTintColor.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_cloudTintColor.Evaluate(time), duration);
                    components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerA.exposure.value = LerpFloat(m_startingData.m_cloudExposure.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_cloudExposure.Evaluate(time), duration);
                    components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerB.exposure.value = LerpFloat(m_startingData.m_cloudExposure.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_cloudExposure.Evaluate(time), duration);
                    components.m_timeOfDayVolumeComponenets.m_cloudLayer.shadowMultiplier.value = LerpFloat(m_startingData.m_cloudShadowOpacity.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_cloudShadowOpacity.Evaluate(time), duration);
                    components.m_timeOfDayVolumeComponenets.m_cloudLayer.shadowTint.value = LerpColor(m_startingData.m_cloudShadowColor.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_cloudShadowColor.Evaluate(time), duration);
                    switch (m_weatherData.m_cloudLayerAChannel)
                    {
                        case CloudLayerChannelMode.R:
                        {
                            components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerA.opacityR.value = LerpFloat(m_startingData.m_cloudLayerAOpacityR.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_cloudLayerAOpacityR.Evaluate(time), duration);
                            components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerA.opacityG.value = 0f;
                            components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerA.opacityB.value = 0f;
                            components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerA.opacityA.value = 0f;
                            break;
                        }
                        case CloudLayerChannelMode.G:
                        {
                            components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerA.opacityG.value = LerpFloat(m_startingData.m_cloudLayerAOpacityR.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_cloudLayerAOpacityR.Evaluate(time), duration);
                            components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerA.opacityR.value = 0f;
                            components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerA.opacityB.value = 0f;
                            components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerA.opacityA.value = 0f;
                            break;
                        }
                        case CloudLayerChannelMode.B:
                        {
                            components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerA.opacityB.value = LerpFloat(m_startingData.m_cloudLayerAOpacityR.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_cloudLayerAOpacityR.Evaluate(time), duration);
                            components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerA.opacityR.value = 0f;
                            components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerA.opacityG.value = 0f;
                            components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerA.opacityA.value = 0f;
                            break;
                        }
                        case CloudLayerChannelMode.A:
                        {
                            components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerA.opacityA.value = LerpFloat(m_startingData.m_cloudLayerAOpacityR.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_cloudLayerAOpacityR.Evaluate(time), duration);
                            components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerA.opacityR.value = 0f;
                            components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerA.opacityG.value = 0f;
                            components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerA.opacityB.value = 0f;
                            break;
                        }
                    }
                    switch (m_weatherData.m_cloudLayerBChannel)
                    {
                        case CloudLayerChannelMode.R:
                        {
                            components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerB.opacityR.value = LerpFloat(m_startingData.m_cloudLayerBOpacityR.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_cloudLayerBOpacityR.Evaluate(time), duration);
                            components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerB.opacityG.value = 0f;
                            components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerB.opacityB.value = 0f;
                            components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerB.opacityA.value = 0f;
                            break;
                        }
                        case CloudLayerChannelMode.G:
                        {
                            components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerB.opacityG.value = LerpFloat(m_startingData.m_cloudLayerBOpacityR.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_cloudLayerBOpacityR.Evaluate(time), duration);
                            components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerB.opacityR.value = 0f;
                            components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerB.opacityB.value = 0f;
                            components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerB.opacityA.value = 0f;
                            break;
                        }
                        case CloudLayerChannelMode.B:
                        {
                            components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerB.opacityB.value = LerpFloat(m_startingData.m_cloudLayerBOpacityR.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_cloudLayerBOpacityR.Evaluate(time), duration);
                            components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerB.opacityR.value = 0f;
                            components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerB.opacityG.value = 0f;
                            components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerB.opacityA.value = 0f;
                            break;
                        }
                        case CloudLayerChannelMode.A:
                        {
                            components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerB.opacityA.value = LerpFloat(m_startingData.m_cloudLayerBOpacityR.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_cloudLayerBOpacityR.Evaluate(time), duration);
                            components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerB.opacityR.value = 0f;
                            components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerB.opacityG.value = 0f;
                            components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerB.opacityB.value = 0f;
                            break;
                        }
                    }
                }
                else
                {
#if HDRPTIMEOFDAY
                    m_weatherData.ProceduralCloudPreset.Blend(components.m_timeOfDayVolumeComponenets.m_cloudLayer, time, duration);
#endif
                }               

                #endregion
            }
            //Sun Flare
            if (ValidateSunLensFlare())
            {
                components.m_sunLensFlare.enabled = m_weatherData.m_sunLensFlareProfile.m_useLensFlare;
                if (m_weatherData.m_sunLensFlareProfile.m_useLensFlare)
                {
                    components.m_sunLensFlare.intensity = LerpFloat(m_startingData.m_sunLensFlareProfile.m_intensity.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_sunLensFlareProfile.m_intensity.Evaluate(time), duration);
                    components.m_sunLensFlare.scale = LerpFloat(m_startingData.m_sunLensFlareProfile.m_scale.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_sunLensFlareProfile.m_scale.Evaluate(time), duration);
                }
            }
            //Moon Flare
            if (ValidateMoonLensFlare())
            {
                components.m_moonLensFlare.enabled = m_weatherData.m_moonLensFlareProfile.m_useLensFlare;
                if (m_weatherData.m_moonLensFlareProfile.m_useLensFlare)
                {
                    components.m_moonLensFlare.intensity = LerpFloat(m_startingData.m_moonLensFlareProfile.m_intensity.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_moonLensFlareProfile.m_intensity.Evaluate(time), duration);
                    components.m_moonLensFlare.scale = LerpFloat(m_startingData.m_moonLensFlareProfile.m_scale.Evaluate(m_lastTimeOfDayValue), m_weatherData.m_moonLensFlareProfile.m_scale.Evaluate(time), duration);
                }
            }

            return false;
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
        public bool ValidateSun()
        {
            if (m_weatherData.m_sunIntensity == null)
            {
                return false;
            }
            if (m_weatherData.m_moonIntensity == null)
            {
                return false;
            }
            if (m_weatherData.m_sunIntensityMultiplier == null)
            {
                return false;
            }
            if (m_weatherData.m_sunTemperature == null)
            {
                return false;
            }
            if (m_weatherData.m_sunColorFilter == null)
            {
                return false;
            }
            if (m_weatherData.m_moonTemperature == null)
            {
                return false;
            }
            if (m_weatherData.m_moonColorFilter == null)
            {
                return false;
            }
            if (m_weatherData.m_sunVolumetrics == null)
            {
                return false;
            }
            if (m_weatherData.m_sunVolumetricShadowDimmer == null)
            {
                return false;
            }

            return true;
        }
        public bool ValidateAdvancedLighting()
        {
            if (m_weatherData.m_generalExposure == null)
            {
                return false;
            }
            if (m_weatherData.m_ambientIntensity == null)
            {
                m_weatherData.m_ambientIntensity = AnimationCurve.Constant(0f, 1f, 1f);
                return false;
            }
            if (m_weatherData.m_ambientReflectionIntensity == null)
            {
                m_weatherData.m_ambientReflectionIntensity = AnimationCurve.Constant(0f, 1f, 1f);
                return false;
            }

            return true;
        }
        public bool ValidateFog()
        {
            if (m_weatherData.m_fogColor == null)
            {
                return false;
            }
            if (m_weatherData.m_fogDistance == null)
            {
                return false;
            }
            if (m_weatherData.m_fogHeight == null)
            {
                return false;
            }
            if (m_weatherData.m_volumetricFogDistance == null)
            {
                return false;
            }
            if (m_weatherData.m_volumetricFogAnisotropy == null)
            {
                return false;
            }
            if (m_weatherData.m_volumetricFogSliceDistributionUniformity == null)
            {
                return false;
            }
            if (m_weatherData.m_localFogMultiplier == null)
            {
                return false;
            }
            if (m_weatherData.m_fogDensity == null)
            {
                return false;
            }

            return true;
        }
        public bool ValidateShadows()
        {
            if (m_weatherData.m_shadowDistance == null)
            {
                return false;
            }
            if (m_weatherData.m_shadowTransmissionMultiplier == null)
            {
                return false;
            }

            return true;
        }
        public bool ValidateClouds()
        {
            //Procedural
            if (m_weatherData.m_cloudOpacity == null)
            {
                return false;
            }
            if (m_weatherData.m_cloudTintColor == null)
            {
                return false;
            }
            if (m_weatherData.m_cloudExposure == null)
            {
                return false;
            }
            if (m_weatherData.m_cloudWindDirection == null)
            {
                return false;
            }
            if (m_weatherData.m_cloudWindSpeed == null)
            {
                return false;
            }
            if (m_weatherData.m_cloudShadowOpacity == null)
            {
                return false;
            }
            if (m_weatherData.m_cloudShadowColor == null)
            {
                return false;
            }
            if (m_weatherData.m_cloudLayerAOpacityR == null)
            {
                return false;
            }
            if (m_weatherData.m_cloudLayerAOpacityG == null)
            {
                return false;
            }
            if (m_weatherData.m_cloudLayerAOpacityB == null)
            {
                return false;
            }
            if (m_weatherData.m_cloudLayerAOpacityA == null)
            {
                return false;
            }
            if (m_weatherData.m_cloudLayerBOpacityR == null)
            {
                return false;
            }
            if (m_weatherData.m_cloudLayerBOpacityG == null)
            {
                return false;
            }
            if (m_weatherData.m_cloudLayerBOpacityB == null)
            {
                return false;
            }
            if (m_weatherData.m_cloudLayerBOpacityA == null)
            {
                return false;
            }
            //Volumetic
            if (m_weatherData.m_volumetricDensityMultiplier == null)
            {
                return false;
            }
            if (m_weatherData.m_volumetricDensityCurve == null)
            {
                return false;
            }
            if (m_weatherData.m_volumetricShapeFactor == null)
            {
                return false;
            }
            if (m_weatherData.m_volumetricShapeScale == null)
            {
                return false;
            }
            if (m_weatherData.m_volumetricErosionFactor == null)
            {
                return false;
            }
            if (m_weatherData.m_volumetricErosionScale == null)
            {
                return false;
            }
            if (m_weatherData.m_volumetricErosionCurve == null)
            {
                return false;
            }
            if (m_weatherData.m_volumetricAmbientOcclusionCurve == null)
            {
                return false;
            }
            if (m_weatherData.m_volumetricAmbientLightProbeDimmer == null)
            {
                return false;
            }
            if (m_weatherData.m_volumetricSunLightDimmer == null)
            {
                return false;
            }
            if (m_weatherData.m_volumetricErosionOcclusion == null)
            {
                return false;
            }
            if (m_weatherData.m_volumetricScatteringTint == null)
            {
                return false;
            }
            if (m_weatherData.m_volumetricPowderEffectIntensity == null)
            {
                return false;
            }
            if (m_weatherData.m_volumetricMultiScattering == null)
            {
                return false;
            }
            if (m_weatherData.m_volumetricCloudShadowOpacity == null)
            {
                return false;
            }

            return true;
        }
        public bool ValidateSunLensFlare()
        {
            if (m_weatherData.m_sunLensFlareProfile.m_lensFlareData == null)
            {
                return false;
            }
            if (m_weatherData.m_sunLensFlareProfile.m_intensity == null)
            {
                return false;
            }
            if (m_weatherData.m_sunLensFlareProfile.m_scale == null)
            {
                return false;
            }

            return true;
        }
        public bool ValidateMoonLensFlare()
        {
            if (m_weatherData.m_moonLensFlareProfile.m_lensFlareData == null)
            {
                return false;
            }
            if (m_weatherData.m_moonLensFlareProfile.m_intensity == null)
            {
                return false;
            }
            if (m_weatherData.m_moonLensFlareProfile.m_scale == null)
            {
                return false;
            }

            return true;
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
    public class WeatherEffectsData
    {
        public GameObject m_weatherEffect;
        public AudioClip m_weatherAudio;
        [Range(0f, 1f)]
        public float m_weatherEffectVolume = 1f;
        public bool m_randomizeAdditionalEffects = false;
        [Tooltip("-1 = unlimited additional effects")]
        public int m_maxAdditionalEffects = -1;
        [Range(0f, 100f)]
        public float m_randomizationSuccessRate = 70f;
        public List<HDRPWeatherAdditionalEffects> m_additionalEffects = new List<HDRPWeatherAdditionalEffects>();
        [HideInInspector]
        public List<HDRPWeatherAdditionalEffects> m_additionalEffectsCopy = new List<HDRPWeatherAdditionalEffects>();

        private int m_currentAdditionalEffects = 0;

        /// <summary>
        /// Sets up the copied effects that is used in the spawning so it does not touch the effects that people setup
        /// </summary>
        public void SetupAdditionalEffectsCopy()
        {
            if (m_additionalEffects.Count > 0)
            {
                m_additionalEffectsCopy.Clear();
                foreach (HDRPWeatherAdditionalEffects effects in m_additionalEffects)
                {
                    HDRPWeatherAdditionalEffects copiedEffect = new HDRPWeatherAdditionalEffects
                    {
                        m_active = effects.m_active,
                        m_loopAudio = effects.m_loopAudio,
                        m_allowInRandomization = effects.m_allowInRandomization,
                        m_effect = effects.m_effect,
                        m_effectAudio = effects.m_effectAudio,
                        m_effectVolume = effects.m_effectVolume,
                        m_name = effects.m_name
                    };

                    m_additionalEffectsCopy.Add(copiedEffect);
                }
            }
        }
        /// <summary>
        /// Randomize Effects active or disabled
        /// </summary>
        public void RandomizeEffects()
        {
            if (m_randomizeAdditionalEffects)
            {
                m_currentAdditionalEffects = 0;
                if (m_additionalEffectsCopy.Count > 0)
                {
                    foreach (HDRPWeatherAdditionalEffects additionalEffect in m_additionalEffectsCopy)
                    {
                        if (additionalEffect.m_allowInRandomization)
                        {
                            //-1 = unlimited additional effects
                            if (m_maxAdditionalEffects != -1)
                            {
                                if (m_currentAdditionalEffects == m_maxAdditionalEffects)
                                {
                                    break;
                                }
                            }

                            float success = UnityEngine.Random.Range(0f, 100f);
                            if (success > m_randomizationSuccessRate)
                            {
                                m_currentAdditionalEffects++;
                                additionalEffect.m_active = true;
                            }
                            else
                            {
                                additionalEffect.m_active = false;
                            }
                        }
                    }
                }
                else
                {
                    if (m_additionalEffects.Count > 0)
                    {
                        Debug.LogWarning("Additional Effects Copy has not been setup, please call SetupAdditionalEffectsCopy() before calling this function");
                    }
                }
            }
        }
        /// <summary>
        /// Validates that this effect can be used
        /// </summary>
        /// <param name="effect"></param>
        /// <returns></returns>
        public bool ValidateAdditionalEffect(HDRPWeatherAdditionalEffects effect)
        {
            if (effect == null)
            {
                return false;
            }
            if (effect.m_effect == null)
            {
                return false;
            }

            return effect.m_active;
        }
    }
    [System.Serializable]
    public class ThunderData
    {
        public LightShadows m_shadows = LightShadows.Soft;
        public Color m_thunderLight = Color.cyan;
        public float m_temperature = 16000f;
        public float m_intesity = 40000f;
        public float m_nightTimeIntensityMultiplier = 0.005f;
        public Vector2Int m_thunderStrikeCountMinMax = new Vector2Int(1, 5);
        public float m_pauseBetweenStrike = 0.15f;
        [Range(0f, 1f)]
        public float m_volume = 0.5f;
        public List<AudioClip> m_thunderStrikeSounds = new List<AudioClip>();
    }
    [System.Serializable]
    public class HDRPWeatherAdditionalEffects
    {
        public string m_name;
        public bool m_active = false;
        public bool m_allowInRandomization = true;
        public GameObject m_effect;
        public AudioClip m_effectAudio;
        public bool m_loopAudio = true;
        [Range(0f, 1f)]
        public float m_effectVolume = 0.6f;

        /// <summary>
        /// Applies audio settings to the audio GameObject
        /// </summary>
        /// <param name="audioObject"></param>
        public void ApplyGlobalAudioEffect(GameObject audioObject)
        {
            if (m_effectAudio == null)
            {
                return;
            }

            if (audioObject != null)
            {
                AudioSource audioSource = audioObject.GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = audioObject.AddComponent<AudioSource>();
                }

                audioSource.loop = m_loopAudio;
                audioSource.volume = m_effectVolume;
                audioSource.clip = m_effectAudio;
                if (!audioSource.isPlaying)
                {
                    audioSource.Play();
                }
            }
        }
    }

    /// <summary>
    /// Weather Profile containing all the data
    /// </summary>
    public class HDRPTimeOfDayWeatherProfile : ScriptableObject
    {
        public HDRPTimeOfDayWeatherProfileData WeatherData
        {
            get { return m_weatherData; }
            set
            {
                if (m_weatherData != value)
                {
                    m_weatherData = value;
                }
            }
        }
        [SerializeField] private HDRPTimeOfDayWeatherProfileData m_weatherData = new HDRPTimeOfDayWeatherProfileData();

        public UnderwaterOverrideData UnderwaterOverrideData
        {
            get { return m_underwaterOverrideData; }
            set
            {
                if (m_underwaterOverrideData != value)
                {
                    m_underwaterOverrideData = value;
                }
            }
        }
        [SerializeField] private UnderwaterOverrideData m_underwaterOverrideData = new UnderwaterOverrideData();

        public WeatherEffectsData WeatherFXData
        {
            get { return m_weatherFXData; }
            set
            {
                if (m_weatherFXData != value)
                {
                    m_weatherFXData = value;
                }
            }
        }
        [SerializeField] private WeatherEffectsData m_weatherFXData = new WeatherEffectsData();

        public WeatherShaderData WeatherShaderData
        {
            get { return m_weatherShaderData; }
            set
            {
                if (m_weatherShaderData != value)
                {
                    m_weatherShaderData = value;
                }
            }
        }
        [SerializeField] private WeatherShaderData m_weatherShaderData = new WeatherShaderData();

        /// <summary>
        /// Gets the weather fx interface from the gameobject
        /// </summary>
        /// <param name="weatherVFX"></param>
        /// <returns></returns>
        public static IHDRPWeatherVFX GetInterface(GameObject weatherVFX)
        {
            if (weatherVFX != null)
            {
                IHDRPWeatherVFX vfx = weatherVFX.GetComponent<IHDRPWeatherVFX>();
                if (vfx == null)
                {
                    vfx = weatherVFX.GetComponentInChildren<IHDRPWeatherVFX>();
                }

                return vfx;
            }

            return null;
        }
    }
}
#endif