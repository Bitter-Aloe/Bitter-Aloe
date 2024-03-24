using UnityEngine;
#if HDPipeline
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
#endif

namespace ProceduralWorlds.HDRPTOD
{
    public enum TimeOfDayComponentType { Lighting, PostProcessing, Sun, Moon, SunRotationObject, LocalFog, RayTracedVolume, WindZone, WeatherBlendVolume, TwilightLight }

    [ExecuteAlways]
    public class HDRPTimeOfDayComponentType : MonoBehaviour
    {
        public TimeOfDayComponentType m_componentType = TimeOfDayComponentType.Lighting;
        public Light m_lightSource;
        public WindZone m_windZone;
#if HDPipeline
        public Volume m_componentVolume;
        public VolumeProfile m_volumeProfile;
        public LocalVolumetricFog m_localFog;
#endif

        private void OnEnable()
        {
            if (m_lightSource == null)
            {
                m_lightSource = GetComponent<Light>();
            }

            if (m_windZone == null)
            {
                m_windZone = GetComponent<WindZone>();
            }
#if HDPipeline
            if (m_localFog == null)
            {
                m_localFog = GetComponent<LocalVolumetricFog>();
            }

            if (m_componentVolume == null)
            {
                m_componentVolume = GetComponent<Volume>();
            }

            if (m_componentVolume != null)
            {
                m_volumeProfile = m_componentVolume.sharedProfile;
            }
#endif
        }
#if HDPipeline
        public Volume GetVolume()
        {
            if (m_componentVolume == null)
            {
                m_componentVolume = GetComponent<Volume>();
            }

            return m_componentVolume;
        }
#endif
    }
}