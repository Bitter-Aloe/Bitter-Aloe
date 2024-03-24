#if HDRPTIMEOFDAY && HDPipeline
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralWorlds.HDRPTOD
{
    [CreateAssetMenu(fileName = "Light Controller Profile", menuName = "Procedural Worlds/HDRP Time Of Day/Profiles/Light Controller Profile")]
    public class HDRPTimeOfDayLightControllerProfile : ScriptableObject
    {
        #region Public Variables

        [Header("Intensity")]
        public float Intensity;
        public AnimationCurve IntensityMultiplierAnimationCurve = AnimationCurve.Constant(0f, 1f, 1f);
        [Header("Shadows")]
        public bool CastShadows = true;
        public LightShadowResolutionQuality LightShadowResolutionQuality = LightShadowResolutionQuality.High;

        [Header("Ray Tracing")]
        public List<RayTracedLightOptions> RayTracingLightOptions = new List<RayTracedLightOptions>();

        #endregion
        #region Public Apply Functions

        /// <summary>
        /// Applies the settings to the light source
        /// </summary>
        /// <param name="cachedData"></param>
        /// <param name="time"></param>
        /// <param name="rayTracingEnabled"></param>
        public void Apply(CachedData cachedData, float time, bool rayTracingEnabled, RayTracingLightingModel rtLightingModel)
        {
            cachedData.LightData.SetIntensity(cachedData.CachedIntensity * IntensityMultiplierAnimationCurve.Evaluate(time));
            if (rayTracingEnabled)
            {
                RefreshRayTracedLightOptions(cachedData, rtLightingModel);
            }
            else
            {
                cachedData.LightData.EnableShadows(CastShadows);
                cachedData.LightData.useRayTracedShadows = false;
                cachedData.LightData.SetShadowResolutionLevel((int)LightShadowResolutionQuality);
            }
        }
        /// <summary>
        /// Reserts the settings back to the profile default
        /// This mean the intensity will not have the multiplier applied
        /// </summary>
        /// <param name="cachedData"></param>
        public void RevertToDefault(CachedData cachedData)
        {
            cachedData.LightData.SetIntensity(cachedData.CachedIntensity);
            cachedData.LightData.EnableShadows(CastShadows);
            cachedData.LightData.SetShadowResolution((int)LightShadowResolutionQuality);
        }

        #endregion
        #region Private Function

        /// <summary>
        /// Refreshes the ray traced light options
        /// </summary>
        private void RefreshRayTracedLightOptions(CachedData cachedData, RayTracingLightingModel lightingModel)
        {
            if (RayTracingLightOptions.Count > 0)
            {
                for (int i = 0; i < RayTracingLightOptions.Count; i++)
                {
                    RayTracingLightOptions[i].Apply(cachedData.LightData, lightingModel);
                }
            }
        }

        #endregion
    }
}
#endif