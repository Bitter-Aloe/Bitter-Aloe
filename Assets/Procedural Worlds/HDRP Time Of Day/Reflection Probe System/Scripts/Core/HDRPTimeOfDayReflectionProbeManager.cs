using UnityEngine;
#if HDPipeline
using UnityEngine.Rendering.HighDefinition;
#endif

namespace ProceduralWorlds.HDRPTOD
{
    [ExecuteAlways]
    public class HDRPTimeOfDayReflectionProbeManager : MonoBehaviour
    {
        public static HDRPTimeOfDayReflectionProbeManager Instance
        {
            get { return m_instance; }
        }
        [SerializeField] private static HDRPTimeOfDayReflectionProbeManager m_instance;

        public float RenderDistance
        {
            get { return m_renderDistance; }
            set
            {
                if (m_renderDistance != value)
                {
                    m_renderDistance = value;
                    if (m_profile != null)
                    {
                        m_profile.m_renderDistance = value;
                    }
#if HDPipeline && UNITY_2021_2_OR_NEWER
                    m_globalHDProbeData.settingsRaw.influence.boxSize = new Vector3(value, value, value);
#endif
                }
            }
        }
        [SerializeField] private float m_renderDistance = 5000f;

        public HDRPTimeOfDayReflectionProbeProfile Profile
        {
            get { return m_profile; }
            set
            {
                if (m_profile != value)
                {
                    m_profile = value;
                    RenderDistance = value.m_renderDistance;
                }
            }
        }
        [SerializeField] private HDRPTimeOfDayReflectionProbeProfile m_profile;

        public Transform m_playerCamera;
        public ReflectionProbe m_globalProbe;
        public ReflectionProbeTODData m_currentData;
        public float m_globalMultiplier = 1f;
        public bool m_allowInRayTracing = false;
        private float m_currentTransitionValue = 0f;

#if HDPipeline && UNITY_2021_2_OR_NEWER
        [SerializeField]
        private HDAdditionalReflectionData m_globalHDProbeData;

        #region Unity Functions

        private void OnEnable()
        {
            m_instance = this;
            if (m_globalProbe != null)
            {
                m_globalProbe.enabled = enabled;
            }

            if (m_playerCamera == null)
            {
                m_playerCamera = HDRPTimeOfDayAPI.GetCamera();
            }

            if (HDRPTimeOfDay.Instance != null)
            {
                UpdateProbeSystem(HDRPTimeOfDayAPI.RayTracingSSGIActive());
            }
        }
        private void OnDisable()
        {
            if (m_globalProbe != null)
            {
                m_globalProbe.enabled = false;
            }
        }
        private void LateUpdate()
        {
            FollowPlayer();
        }

        #endregion
        #region Public Functions

        /// <summary>
        /// Gets the transition value
        /// </summary>
        /// <returns></returns>
        public float GetCurrentTransitionCurveValue()
        {
            return m_currentTransitionValue;
        }
        /// <summary>
        /// Refreshes the probe system
        /// </summary>
        public void Refresh(bool rayTracing)
        {
            UpdateProbeSystem(rayTracing);
        }

        #endregion
        #region Private Functions

        /// <summary>
        /// Applies new probe data to the global probe
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private bool SetNewData(ReflectionProbeTODData data, bool rayTracing)
        {
            if (data.Validate(HDRPTimeOfDay.Instance, Profile.m_probeTimeMode))
            {
                m_currentData = data;
                if (Profile.m_probeTimeMode == ProbeTimeMode.TimeTransition)
                {
                    m_globalHDProbeData.settingsRaw.lighting.multiplier = GetCurveValue(rayTracing) * GetMultiplier(data, rayTracing);
                    m_globalHDProbeData.settingsRaw.lighting.weight = Profile.m_weightCurve.Evaluate(HDRPTimeOfDay.Instance.ConvertTimeOfDay());
                }
                else
                {
                    m_globalHDProbeData.settingsRaw.lighting.multiplier = (data.m_intensity * GetMultiplier(data, rayTracing));
                }
                m_globalHDProbeData.settingsRaw.cameraSettings.probeLayerMask = data.m_renderLayers;
                m_globalHDProbeData.settingsRaw.cameraSettings.culling.cullingMask = data.m_renderLayers;
                m_globalHDProbeData.SetTexture(ProbeSettings.Mode.Custom, data.m_probeCubeMap);
                return true;
            }

            return false;
        }
        /// <summary>
        /// Updates the probe systems
        /// </summary>
        private void UpdateProbeSystem(bool rayTracing)
        {
            if (m_profile == null)
            {
                return;
            }

            bool isEnabled = m_profile.m_renderMode == ProbeRenderMode.Sky;
            if (rayTracing && !m_allowInRayTracing)
            {
                isEnabled = false;
            }
            m_globalProbe.enabled = isEnabled;

            if (isEnabled && CanProcess())
            {
                foreach (ReflectionProbeTODData data in m_profile.m_probeTODData)
                {
                    if (SetNewData(data, rayTracing))
                    {
                        break;
                    }
                }
            }
        }
        /// <summary>
        /// Gets the curve value
        /// </summary>
        /// <param name="rayTracing"></param>
        /// <returns></returns>
        private float GetCurveValue(bool rayTracing)
        {
            if (rayTracing)
            {
                return Profile.m_transitionIntensityCurveRT.Evaluate(HDRPTimeOfDay.Instance.ConvertTimeOfDay());
            }
            else
            {
                return Profile.m_transitionIntensityCurve.Evaluate(HDRPTimeOfDay.Instance.ConvertTimeOfDay());
            }
        }
        /// <summary>
        /// Gets the active weather probe data weather intensity multiplier
        /// </summary>
        /// <param name="data"></param>
        /// <param name="rayTracing"></param>
        /// <returns></returns>
        private float GetMultiplier(ReflectionProbeTODData data, bool rayTracing)
        {
            if (HDRPTimeOfDay.Instance != null)
            {
                float value = 1f;
                if (HDRPTimeOfDay.Instance.WeatherActive())
                {
                    IHDRPWeatherVFX weatherEffect = HDRPTimeOfDay.Instance.m_weatherVFX;
                    if (weatherEffect != null)
                    {
                        value = Mathf.Lerp(1f, data.m_weatherIntensityMultiplier, weatherEffect.GetCurrentDuration());
                    }
                }
                if (HDRPTimeOfDay.Instance.IsSSGIEnabled())
                {
                    if (rayTracing && m_allowInRayTracing)
                    {
                        return (value * m_globalMultiplier);
                    }
                    else
                    {
                        float SSGI = data.m_enableSSGIMultiplier ? data.m_ssgiMultiplier : 1f;
                        return (value * m_globalMultiplier) * SSGI;
                    }
                }
                else
                {
                    return value * m_globalMultiplier;
                }
            }

            return 1f;
        }
        /// <summary>
        /// Moves the probe position to the player position
        /// </summary>
        private void FollowPlayer()
        {
            if (m_profile.m_followPlayer)
            {
                if (m_playerCamera != null && m_globalProbe != null)
                {
                    Vector3 playerPos = m_playerCamera.position;
                    playerPos.y = GetYPlayerPosition();
                    m_globalProbe.transform.position = playerPos;
                }
            }
        }
        /// <summary>
        /// Gets the y for the follow position
        /// </summary>
        /// <returns></returns>
        private float GetYPlayerPosition()
        {
            float value = m_globalHDProbeData.settings.influence.boxSize.y / 2f;
            HDRPTimeOfDay timeOfDay = HDRPTimeOfDay.Instance;
            if (timeOfDay != null)
            {
                float seaLevel = timeOfDay.TimeOfDayProfile.UnderwaterOverrideData.m_seaLevel;
                if (seaLevel >= 0f)
                {
                    value += seaLevel * 4f;
                }
                else
                {
                    value -= Mathf.Abs(seaLevel * 4f);
                }
            }

            if (m_profile != null)
            {
                if (m_profile.m_seaLevelOffset >= 0f)
                {
                    value += m_profile.m_seaLevelOffset;
                }
                else
                {
                    value -= Mathf.Abs(m_profile.m_seaLevelOffset);
                }
            }

            return value;
        }
        /// <summary>
        /// Checks to see if it can be processed
        /// </summary>
        /// <returns></returns>
        private bool CanProcess()
        {
            if (m_profile.m_renderMode != ProbeRenderMode.Sky)
            {
                return false;
            }

            if (m_globalProbe == null)
            {
                m_globalProbe = GetComponent<ReflectionProbe>();
                if (m_globalProbe == null)
                {
                    return false;
                }
            }

            if (m_globalHDProbeData == null)
            {
                if (m_globalProbe != null)
                {
                    m_globalHDProbeData = m_globalProbe.GetComponent<HDAdditionalReflectionData>();
                    if (m_globalHDProbeData == null)
                    {
                        m_globalHDProbeData = m_globalProbe.gameObject.AddComponent<HDAdditionalReflectionData>();
                    }
                }
            }

            return true;
        }

        #endregion
#endif
    }
}