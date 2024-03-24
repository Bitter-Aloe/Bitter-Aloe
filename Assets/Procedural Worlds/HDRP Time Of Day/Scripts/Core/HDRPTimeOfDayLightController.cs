#if HDRPTIMEOFDAY && HDPipeline
using System;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace ProceduralWorlds.HDRPTOD
{
    [Serializable]
    public class CachedData
    {
        #region Public Variables

        //Cached light information
        public float CachedIntensity;
        public bool CachedShadowsEnabled;
        public int CachedShadowQuality;
        public Light LightSource;
        public HDAdditionalLightData LightData;

        #endregion
        #region Public Functions

        /// <summary>
        /// Builds the cache information
        /// </summary>
        /// <param name="controllerRoot"></param>
        /// <param name="profile"></param>
        public void BuildCache(GameObject controllerRoot, HDRPTimeOfDayLightControllerProfile profile)
        {
            GetLight(controllerRoot);
            if (profile != null)
            {
                CachedIntensity = profile.Intensity;
                CachedShadowQuality = (int)profile.LightShadowResolutionQuality;
                CachedShadowsEnabled = profile.CastShadows;
            }
        }
        /// <summary>
        /// Validates that the cache has the required data we need to proceed
        /// </summary>
        /// <returns></returns>
        public bool ValidateCache()
        {
            return LightData != null;
        }

        #endregion
        #region Private Functions

        /// <summary>
        /// Gets and assigns the light data
        /// </summary>
        /// <param name="gameObject"></param>
        private void GetLight(GameObject gameObject)
        {
            if (gameObject != null)
            {
                LightSource = gameObject.GetComponentInChildren<Light>();
                if (LightSource != null)
                {
                    LightData = LightSource.GetComponent<HDAdditionalLightData>();
                }
            }
        }

        #endregion
    }

    [ExecuteAlways]
    public class HDRPTimeOfDayLightController : MonoBehaviour
    {
        #region Properties

        //Add to TOD
        public bool AddLightToTimeOfDay = true;
        //Profile
        public HDRPTimeOfDayLightControllerProfile LightInformation
        {
            get
            {
                return m_lightInformation;
            }
            set
            {
                if (m_lightInformation != value)
                {
                    m_lightInformation = value;
                    UpdateCache();
                }
            }
        }

        #endregion
        #region Private Serilized Variables

        //Serilized properties
        [SerializeField] private HDRPTimeOfDayLightControllerProfile m_lightInformation;
        //Cached globals
        [SerializeField] private bool m_validated;
        [SerializeField] private HDRPTimeOfDay m_hdrpTimeOfDay;
        [SerializeField] private CachedData m_cachedData = new CachedData();

        #endregion
        #region Unity Functions

        /// <summary>
        /// Called when this script is enabled
        /// </summary>
        private void OnEnable()
        {
            Refresh();
            if (m_hdrpTimeOfDay != null && m_cachedData.ValidateCache())
            {
                if (AddLightToTimeOfDay)
                {
                    m_hdrpTimeOfDay.AddAdditionalLight(m_cachedData.LightData);
                }
                else
                {
                    m_hdrpTimeOfDay.RemoveAdditionalLight(m_cachedData.LightData);
                }

                m_hdrpTimeOfDay.onTimeOfDayChanged -= Apply;
                m_hdrpTimeOfDay.onTimeOfDayChanged += Apply;
                Apply(m_hdrpTimeOfDay.TimeOfDay);
            }
        }
        /// <summary>
        /// Called if this script is disabled
        /// </summary>
        private void OnDisable()
        {
            ResetBackToDefault();
            if (m_hdrpTimeOfDay != null && m_cachedData.ValidateCache())
            {
                m_hdrpTimeOfDay.RemoveAdditionalLight(m_cachedData.LightData);
                m_hdrpTimeOfDay.onTimeOfDayChanged -= Apply;
            }
        }

        #endregion
        #region Public Functions

        /// <summary>
        /// Refreshes this system
        /// </summary>
        public void Refresh()
        {
            if (m_hdrpTimeOfDay == null)
            {
                m_hdrpTimeOfDay = HDRPTimeOfDay.Instance;
            }

            UpdateCache();
            if (m_hdrpTimeOfDay != null)
            {
                m_hdrpTimeOfDay.AddLightController(this);
                Apply(m_hdrpTimeOfDay.TimeOfDay);
            }
        }
        /// <summary>
        /// Applies the settings
        /// </summary>
        /// <param name="time"></param>
        public void Apply(float time)
        {
            if (m_validated)
            {
                RayTracingLightingModel rtLightingModel = m_hdrpTimeOfDay.RayTracingLightingModel;
                bool rayTraceEnabled = m_hdrpTimeOfDay.TimeOfDayProfile.RayTracingProfile.IsRTActive();
                LightInformation.Apply(m_cachedData, time / 24f, rayTraceEnabled, rtLightingModel);
            }
        }

        #endregion
        #region Private Functions

        /// <summary>
        /// Updates the cached information
        /// </summary>
        /// <param name="lightInfo"></param>
        private void UpdateCache()
        {
            if (m_lightInformation == null)
            {
                m_validated = false;
                return;
            }

            if (m_cachedData == null)
            {
                m_cachedData = new CachedData();
            }

            m_cachedData.BuildCache(gameObject, m_lightInformation);
            m_validated = m_cachedData.ValidateCache();
            m_validated = m_hdrpTimeOfDay != null;
        }
        /// <summary>
        /// Resets the light intensity back to default saved
        /// </summary>
        private void ResetBackToDefault()
        {
            if (m_validated)
            {
                LightInformation.RevertToDefault(m_cachedData);
            }

            if (m_hdrpTimeOfDay != null)
            {
                m_hdrpTimeOfDay.RemoveLightController(this);
            }
        }

        #endregion
    }
}
#endif