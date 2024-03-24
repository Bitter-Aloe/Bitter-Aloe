using System.Collections.Generic;
using UnityEngine;

namespace ProceduralWorlds.HDRPTOD
{
    public class HDRPDefaultsProfile : ScriptableObject
    {
#if HDPipeline && UNITY_2021_2_OR_NEWER
        public ShaderVariantCollection m_hdrpTimeOfDayShaderVarients;
        public HDRPTimeOfDayProfile m_timeOfDayProfile;
        public HDRPTimeOfDayPostFXProfile m_postProcessingProfile;
        public HDRPTimeOfDayAmbientProfile m_ambientAudioProfile;
        public HDRPTimeOfDayReflectionProbeProfile m_reflectionProbeProfile;
        public HDRPTimeOfDaySeasonProfile m_seasonProfile;
        public HDRPTimeOfDayPresetProfile m_presetProfile;
#if HDRPTIMEOFDAY
        public HDRPTimeOfDayVolumetricCloudPreset m_volumetricCloudPreset;
        public HDRPTimeOfDayProceduralCloudPreset m_proceduralCloudPreset;
#endif
        public List<HDRPTimeOfDayWeatherProfile> m_weatherProfiles = new List<HDRPTimeOfDayWeatherProfile>();
        [Range(0f, 24f)]
        public float m_startingTimeOfDay = 8f;
        [Range(0f, 360f)]
        public float m_startingDirection = 20f;
        [Range(-1f, 1f)]
        public float m_startingHorizonOffset = 0.1f;
        public Vector2 m_minMaxWeatherWaitTime = new Vector2(120f, 300f);
        public bool m_useReflectionProbeSystem = true;
        public bool m_autoUpdate = false;
        public bool m_incrementalUpdate = true;
        public int m_incrementalFrameCount = 3;
        public bool m_useOverrideVolumes = false;
        public bool m_enableInteriorControllers = true;
        public bool m_enableAudioReverb = true;
        public bool m_overrideUnderwater = true;
        public UnderwaterOverrideSystemType m_overrideUnderwaterType = UnderwaterOverrideSystemType.HDRPTimeOfDay;

        /// <summary>
        /// Applies the defaults to the time of day
        /// </summary>
        /// <param name="timeOfDay"></param>
        public void ApplyDefaultsToTimeOfDay(HDRPTimeOfDay timeOfDay)
        {
            if (timeOfDay != null)
            {
                timeOfDay.SetHasBeenSetup(timeOfDay.SetupHDRPTimeOfDay());
#if HDRPTIMEOFDAY
                if (m_volumetricCloudPreset != null)
                {
                    if (m_timeOfDayProfile.TimeOfDayData.VolumetricCloudPreset == null)
                    {
                        m_timeOfDayProfile.TimeOfDayData.VolumetricCloudPreset = m_volumetricCloudPreset;
                    }
                }
                if (m_proceduralCloudPreset != null)
                {
                    if (m_timeOfDayProfile.TimeOfDayData.ProceduralCloudPreset == null)
                    {
                        m_timeOfDayProfile.TimeOfDayData.ProceduralCloudPreset = m_proceduralCloudPreset;
                    }
                }
#endif
                if (m_timeOfDayProfile != null)
                {
                    timeOfDay.TimeOfDayProfile = m_timeOfDayProfile;
                    m_timeOfDayProfile.UnderwaterOverrideData.m_useOverrides = m_overrideUnderwater;
                    m_timeOfDayProfile.UnderwaterOverrideData.m_systemType = m_overrideUnderwaterType;
                }

                if (m_postProcessingProfile != null)
                {
                    timeOfDay.TimeOfDayPostFxProfile = m_postProcessingProfile;
                }

                if (m_ambientAudioProfile != null)
                {
                    timeOfDay.AudioProfile = m_ambientAudioProfile;
                }

                if (m_reflectionProbeProfile != null)
                {
                    timeOfDay.ReflectionProbeProfile = m_reflectionProbeProfile;
                }

                if (m_seasonProfile != null)
                {
                    timeOfDay.SeasonProfile = m_seasonProfile;
                }

                if (m_presetProfile != null)
                {
                    timeOfDay.PresetProfile = m_presetProfile;
                }

                if (m_weatherProfiles.Count > 0)
                {
                    timeOfDay.WeatherProfiles.Clear();
                    timeOfDay.WeatherProfiles.AddRange(m_weatherProfiles);
                }

                timeOfDay.TimeOfDay = m_startingTimeOfDay;
                timeOfDay.DirectionY = m_startingDirection;
                timeOfDay.TimeOfDayProfile.TimeOfDayData.m_horizonOffset = m_startingHorizonOffset;
                timeOfDay.m_randomWeatherTimer = m_minMaxWeatherWaitTime;
                timeOfDay.EnableReflectionProbeSync = m_useReflectionProbeSystem;
                timeOfDay.m_enableTimeOfDaySystem = m_autoUpdate;
                timeOfDay.IncrementalUpdate = m_incrementalUpdate;
                timeOfDay.IncrementalFrameCount = m_incrementalFrameCount;
                timeOfDay.UseOverrideVolumes = m_useOverrideVolumes;
                timeOfDay.m_interiorControllerData.m_useInteriorControllers = m_enableInteriorControllers;
                timeOfDay.m_interiorControllerData.m_useAudioReverb = m_enableAudioReverb;
            }
        }
#endif
    }
}