#if HDPipeline && UNITY_2021_2_OR_NEWER
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralWorlds.HDRPTOD
{
    [Serializable]
    public class TimeOfDayPreset
    {
        public string PresetName;
        public Texture2D PresetImage;
        public bool RenameEnabled = false;
        public bool ReplaceImageEnabled = false;
        [Range(0f, 24f)]
        public float TimeOfDay = 10f;
        [Range(0f, 360f)]
        public float DirectionY = 0f;
        public TODCloudPresets CloudPreset = TODCloudPresets.Custom;
        [Range(0.01f, 10f)]
        public float GlobalLightIntensityMultiplier = 1f;
        [Range(0.01f, 10f)]
        public float GlobalAmbientIntensityMultiplier = 1f;
        [Range(0.001f, 15f)]
        public float GlobalNearFogMultiplier = 1f;
        [Range(0.001f, 15f)]
        public float GlobalFarFogMultiplier = 1f;
        [Range(0.001f, 2f)]
        public float GlobalFogHeightMultiplier = 1f;

        public void ApplyPreset(HDRPTimeOfDay timeOfDay)
        {
            if (timeOfDay != null)
            {
                HDRPTimeOfDayProfile profile = timeOfDay.TimeOfDayProfile;
                if (profile != null)
                {
                    profile.TimeOfDayData.m_cloudPresets = CloudPreset;
                    profile.TimeOfDayData.m_globalLightMultiplier = GlobalLightIntensityMultiplier;
                    profile.TimeOfDayData.m_ambientIntensityMultiplier = GlobalAmbientIntensityMultiplier;
                    profile.TimeOfDayData.m_globalFogMultiplier = GlobalNearFogMultiplier;
                    profile.TimeOfDayData.m_globalFogDistanceMultiplier = GlobalFarFogMultiplier;
                    profile.TimeOfDayData.m_globalFogHeightMultiplier = GlobalFogHeightMultiplier;
                    timeOfDay.TimeOfDay = TimeOfDay;
                    timeOfDay.DirectionY = DirectionY;
                    Debug.Log("Preset: " + PresetName + " applied successfully.");
                }
            }
        }

        public TimeOfDayPreset(HDRPTimeOfDay timeOfDay, int countId)
        {
            if (timeOfDay != null)
            {
                PresetName = "New Preset " + countId;
                PresetImage = null;
                TimeOfDay = timeOfDay.TimeOfDay;
                DirectionY = timeOfDay.DirectionY;
                HDRPTimeOfDayProfile profile = timeOfDay.TimeOfDayProfile;
                if (profile != null)
                {
                    CloudPreset = profile.TimeOfDayData.m_cloudPresets;
                    GlobalLightIntensityMultiplier = profile.TimeOfDayData.m_globalLightMultiplier;
                    GlobalAmbientIntensityMultiplier = profile.TimeOfDayData.m_ambientIntensityMultiplier;
                    GlobalNearFogMultiplier = profile.TimeOfDayData.m_globalFogMultiplier;
                    GlobalFarFogMultiplier = profile.TimeOfDayData.m_globalFogDistanceMultiplier;
                    GlobalFogHeightMultiplier = profile.TimeOfDayData.m_globalFogHeightMultiplier;
                }
            }
        }

        public TimeOfDayPreset()
        {

        }
    }

    public class HDRPTimeOfDayPresetProfile : ScriptableObject
    {
        public List<TimeOfDayPreset> TimeOfDayPresets = new List<TimeOfDayPreset>();

        public void CreatePreset(HDRPTimeOfDay timeOfDay)
        {
            int presetCount = TimeOfDayPresets.Count;
            TimeOfDayPresets.Add(new TimeOfDayPreset(timeOfDay, presetCount));
            Debug.Log("New preset has been saved successfuly");
        }
        /// <summary>
        /// Applies the presets to the scene at the preset index
        /// </summary>
        /// <param name="timeOfDay"></param>
        /// <param name="presetIndex"></param>
        public void ApplyPreset(HDRPTimeOfDay timeOfDay, int presetIndex)
        {
            TimeOfDayPreset preset = GetPresetByIndex(presetIndex);
            if (preset != null)
            {
                preset.ApplyPreset(timeOfDay);
            }
        }
        /// <summary>
        /// Applies the presets to the scene with the preset name
        /// </summary>
        /// <param name="timeOfDay"></param>
        /// <param name="presetName"></param>
        public void ApplyPreset(HDRPTimeOfDay timeOfDay, string presetName)
        {
            TimeOfDayPreset preset = GetPresetByName(presetName);
            if (preset != null)
            {
                preset.ApplyPreset(timeOfDay);
            }
        }

        /// <summary>
        /// Gets and returns the preset by name
        /// Returns null if not found
        /// </summary>
        /// <param name="presetName"></param>
        /// <returns></returns>
        private TimeOfDayPreset GetPresetByName(string presetName)
        {
            for (int i = 0; i < TimeOfDayPresets.Count; i++)
            {
                if (TimeOfDayPresets[i].PresetName == presetName)
                {
                    return TimeOfDayPresets[i];
                }
            }

            Debug.LogWarning("Preset named: " + presetName + " could not found. Please make sure the preset profile contains the preset you are looking for.");
            return null;
        }
        /// <summary>
        /// Gets and returns the preset by index (ID)
        /// Returns null if not found
        /// </summary>
        /// <param name="presetIndex"></param>
        /// <returns></returns>
        private TimeOfDayPreset GetPresetByIndex(int presetIndex)
        {
            int presetCount = TimeOfDayPresets.Count - 1;
            if (presetIndex >= 0 && presetIndex <= presetCount)
            {
                return TimeOfDayPresets[presetIndex];
            }

            Debug.LogWarning("Preset index: " + presetIndex + " could not found or was out of range please make sure the index you are looking for is within the range of 0 - " + presetCount);
            return null;
        }
    }
}
#endif