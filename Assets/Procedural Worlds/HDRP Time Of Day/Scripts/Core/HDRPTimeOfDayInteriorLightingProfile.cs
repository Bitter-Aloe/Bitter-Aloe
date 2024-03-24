#if HDRPTIMEOFDAY && HDPipeline
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace ProceduralWorlds.HDRPTOD
{
    [CreateAssetMenu(fileName = "HDRP Time Of Day Interior Lighting Profile", menuName = "Procedural Worlds/HDRP Time Of Day/Interior Lighting Profile")]
    public class HDRPTimeOfDayInteriorLightingProfile : ScriptableObject
    {
        [Header("Volume Settings")]
        public VolumeProfile m_volumeProfile;
        public VolumeProfile m_volumeProfileRTX;
        [HideInInspector] public bool m_volumeFoldout = false;

        [Header("Fog Settings")]
        public Color m_fogColor = Color.white;
        public float m_fogDistance = 100f;
#if UNITY_2022_3_OR_NEWER
        public LocalVolumetricFogBlendingMode m_fogBlendingMode = LocalVolumetricFogBlendingMode.Overwrite;
#endif
        public int m_fogPriority = 1;
        public Vector3 m_fogScrollSpeed = Vector3.zero;
        public Vector3 m_fogTiling = Vector3.one;
        public Texture3D m_fogTexture;
        [HideInInspector] public bool m_fogFoldout = false;

        [Header("Reflection Probe Settings")]
        public Cubemap m_probeCubemap;
        public float m_probeIntensity = 1f;
        public float m_probeIntensityRTX = 0.5f;
        [HideInInspector] public bool m_probeFoldout = false;

        [Header("Fake Sun Settings")]
        public float m_sunIntensity = 5000f;
        public float m_sunIntensityRTX = 10f;
        public float m_sunTemperature  = 5500f;
        [HideInInspector] public bool m_sunFoldout = false;

        /// <summary>
        /// Applies the volume settings
        /// </summary>
        /// <param name="volume"></param>
        public void ApplyVolume(Volume volume)
        {
            if (volume == null)
            {
                return;
            }

            volume.sharedProfile = m_volumeProfile;
        }
        /// <summary>
        /// Applies the volume RTX settings
        /// </summary>
        /// <param name="volume"></param>
        public void ApplyVolumeRTX(Volume volume)
        {
            if (volume == null)
            {
                return;
            }

            volume.sharedProfile = m_volumeProfileRTX;
        }
        /// <summary>
        /// Applies the local fog settings
        /// </summary>
        /// <param name="fog"></param>
        public void ApplyLocalFog(LocalVolumetricFog fog)
        {
            if (fog == null)
            {
                return;
            }

            fog.parameters.albedo = m_fogColor;
            fog.parameters.meanFreePath = m_fogDistance;
#if UNITY_2022_3_OR_NEWER
            fog.parameters.blendingMode = m_fogBlendingMode;
#endif
            fog.parameters.volumeMask = m_fogTexture;
            fog.parameters.textureScrollingSpeed = m_fogScrollSpeed;
            fog.parameters.textureTiling = m_fogTiling;
        }
        /// <summary>
        /// Applies the reflection probe settings
        /// </summary>
        /// <param name="reflectionProbe"></param>
        public void ApplyReflectionProbe(HDAdditionalReflectionData reflectionProbe, bool RTXOn)
        {
            if (reflectionProbe == null)
            {
                return;
            }

            reflectionProbe.mode = ProbeSettings.Mode.Custom;
            reflectionProbe.SetTexture(ProbeSettings.Mode.Custom, m_probeCubemap);
            reflectionProbe.multiplier = RTXOn ? m_probeIntensityRTX : m_probeIntensity;
        }
        /// <summary>
        /// Applies the fake sun settings
        /// </summary>
        /// <param name="lightData"></param>
        public void ApplyFakeSun(HDAdditionalLightData lightData, bool RTXOn)
        {
            if (lightData == null)
            {
                return;
            }

            lightData.affectDiffuse = false;
            lightData.affectSpecular = false;
            lightData.lightUnit = LightUnit.Lux;
            lightData.type = HDLightType.Directional;
            lightData.SetIntensity(RTXOn ? m_sunIntensityRTX : m_sunIntensity, LightUnit.Lux);
            lightData.EnableColorTemperature(true);
            lightData.SetColor(Color.white, m_sunTemperature);
        }
    }
}
#endif