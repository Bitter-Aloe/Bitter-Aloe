#if HDPipeline
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.VFX;

namespace ProceduralWorlds.HDRPTOD
{
    [System.Serializable]
    public class WindSyncPropertySettings
    {
        public string m_windDirectionProperty = "WindDirection";
        public Vector2 m_windDirectionClamp = new Vector2(0f, 1f);
        public string m_windSpeedProperty = "WindIntensity";
        public Vector2 m_windSpeedClamp = new Vector2(0f, 5f);
    }

    public class WeatherWindSyncController : MonoBehaviour
    {
        public static WeatherWindSyncController Instance
        {
            get { return m_instance; }
        }
        [SerializeField] private static WeatherWindSyncController m_instance;

        public WindSyncPropertySettings m_windPropertySettings = new WindSyncPropertySettings();
        [HideInInspector]
        public List<VisualEffect> m_visualEffects = new List<VisualEffect>();

        [SerializeField, HideInInspector] private Volume m_volume;
        [SerializeField, HideInInspector] private VisualEnvironment m_visualEnvironment;
        private bool m_setup = false;

        private void Start()
        {
            m_instance = this;
            m_setup = Setup();
        }
        private void Update()
        {
            if (m_setup)
            {
                if (m_visualEffects.Count > 0)
                {
                    foreach (VisualEffect visualEffect in m_visualEffects)
                    {
#if HDPipeline && UNITY_2021_2_OR_NEWER
                        SyncWind(visualEffect);
#endif
                    }
                }
            }
        }

#if HDPipeline && UNITY_2021_2_OR_NEWER
        /// <summary>
        /// Adds a visual effect to the list
        /// </summary>
        /// <param name="vfx"></param>
        public void AddVisualEffect(HDRPWeatherVisualEffect vfx)
        {
            if (!m_visualEffects.Contains(vfx.m_visualEffect))
            {
                m_visualEffects.Add(vfx.m_visualEffect);
            }
        }
        /// <summary>
        /// Adds a visual effect to the list
        /// </summary>
        /// <param name="vfx"></param>
        public void AddVisualEffects(HDRPWeatherVisualEffect[] vfxs)
        {
            foreach (HDRPWeatherVisualEffect visualEffect in vfxs)
            {
                if (!m_visualEffects.Contains(visualEffect.m_visualEffect))
                {
                    m_visualEffects.Add(visualEffect.m_visualEffect);
                } 
            }
        }
        /// <summary>
        /// Removes a visual effect from the list
        /// </summary>
        /// <param name="vfx"></param>
        public void RemoveVisualEffect(HDRPWeatherVisualEffect vfx)
        {
            if (m_visualEffects.Contains(vfx.m_visualEffect))
            {
                m_visualEffects.Remove(vfx.m_visualEffect);
            }
        }
        /// <summary>
        /// Removes a visual effect from the list
        /// </summary>
        /// <param name="vfx"></param>
        public void RemoveVisualEffects(HDRPWeatherVisualEffect[] vfxs)
        {
            foreach (HDRPWeatherVisualEffect visualEffect in vfxs)
            {
                if (m_visualEffects.Contains(visualEffect.m_visualEffect))
                {
                    m_visualEffects.Remove(visualEffect.m_visualEffect);
                } 
            }
        }
        /// <summary>
        /// Processes a wind sync on a visual effect
        /// </summary>
        /// <param name="visualEffect"></param>
        private void SyncWind(VisualEffect visualEffect)
        {
            if (visualEffect != null)
            {
                if (visualEffect.HasVector3(m_windPropertySettings.m_windDirectionProperty))
                {
                    visualEffect.SetVector3(m_windPropertySettings.m_windDirectionProperty, new Vector3(Mathf.InverseLerp(m_windPropertySettings.m_windDirectionClamp.x, m_windPropertySettings.m_windDirectionClamp.y, m_visualEnvironment.windOrientation.value), 0f, 0f));
                }

                if (visualEffect.HasFloat(m_windPropertySettings.m_windSpeedProperty))
                {
                    visualEffect.SetFloat(m_windPropertySettings.m_windSpeedProperty, Mathf.InverseLerp(m_windPropertySettings.m_windSpeedClamp.x, m_windPropertySettings.m_windSpeedClamp.y, m_visualEnvironment.windSpeed.value));
                }
            }
        }
#endif
        /// <summary>
        /// Clears the list
        /// </summary>
        public void RemoveAllVisualEffects()
        {
            m_visualEffects.Clear();
        }
        /// <summary>
        /// Sets up the components
        /// </summary>
        private bool Setup()
        {
            if (m_volume == null || m_visualEnvironment == null)
            {
                Volume[] volumes = FindObjectsOfType<Volume>();
                if (volumes.Length > 0)
                {
                    foreach (Volume volume in volumes)
                    {
                        if (volume.isGlobal)
                        {
                            if (volume.sharedProfile != null)
                            {
                                if (volume.sharedProfile.TryGet(out VisualEnvironment visualEnvironment))
                                {
                                    m_volume = volume;
                                    m_visualEnvironment = visualEnvironment;
                                    return true;
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }
    }
}
#endif