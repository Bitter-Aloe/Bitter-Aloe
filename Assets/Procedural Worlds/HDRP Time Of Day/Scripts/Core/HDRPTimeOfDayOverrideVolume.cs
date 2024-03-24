#if HDPipeline && UNITY_2021_2_OR_NEWER
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace ProceduralWorlds.HDRPTOD
{
    public enum OverrideTODType { Day, Night }

    [System.Serializable]
    public class OverrideVolumeSettings
    {
        public bool m_drawGimzo = true;
        public bool m_drawGizmoSelectedOnly = true;
        public bool m_volumeActiveInWeather = false;
        public bool m_addOverrideVolumeType = true;
        public bool m_removeFromController = true;
        public OverrideTODType m_volumeType = OverrideTODType.Day;
        public Color m_gizmoColor = new Color(1f, 0f, 0f, 0.25f);
        public float m_blendTime = 3f;
        public ClampedFloatParameter m_sunVolumetric = new ClampedFloatParameter(1f, 0f, 16f, false);
        public ClampedFloatParameter m_sunVolumetricDimmer = new ClampedFloatParameter(1f, 0f, 1f, false);
        public ClampedFloatParameter m_exposure = new ClampedFloatParameter(0f, -10f, 10f);
        public ClampedFloatParameter m_ambientIntensity = new ClampedFloatParameter(1f, 0f, 4f, false);
        public ClampedFloatParameter m_ambientReflectionIntensity = new ClampedFloatParameter(1f, 0f, 4f, false);
        public ClampedFloatParameter m_shadowDistanceMultiplier = new ClampedFloatParameter(1f, 0.005f, 5f);

        public bool IsAnyOverrideEnabled()
        {
            if (m_sunVolumetric.overrideState)
            {
                return true;
            }
            if (m_sunVolumetricDimmer.overrideState)
            {
                return true;
            }
            if (m_exposure.overrideState)
            {
                return true;
            }
            if (m_ambientIntensity.overrideState)
            {
                return true;
            }
            if (m_ambientReflectionIntensity.overrideState)
            {
                return true;
            }
            if (m_shadowDistanceMultiplier.overrideState)
            {
                return true;
            }

            return false;
        }
    }
    [System.Serializable]
    public class OverrideVolumeLocalFog
    {
        public bool m_enabled = false;
        public bool m_useTODParameters = true;
        public float m_scaleMultiplier = 1f;
        public Color m_scatteringAlbedo = Color.white;
        public Color m_scatteringAlbedoMultiplier = Color.white;
        public float m_fogDistance = 150f;
        public float m_fogDistanceMultiplier = 1f;
        public float m_renderDistance = 1000f;
        public Texture m_densityMask;
        public Vector3 m_scrollSpeed = Vector3.zero;
        public Vector3 m_tiling = Vector3.one;
    }
    [System.Serializable]
    public class OverrideDataInfo
    {
        public bool m_isInVolue = false;
        public float m_transitionTime = 0f;
        public OverrideVolumeSettings m_settings = new OverrideVolumeSettings();
    }
    public enum OverrideVolumeType { Box, Sphere }

    [ExecuteAlways]
    public class HDRPTimeOfDayOverrideVolume : MonoBehaviour
    {
        public OverrideVolumeSettings m_volumeSettings = new OverrideVolumeSettings();
        public OverrideVolumeLocalFog m_localFogSettings = new OverrideVolumeLocalFog();

        [SerializeField] private LocalVolumetricFog m_localFog;
        [SerializeField] private int m_id;
        [SerializeField] private Bounds m_bounds;
        [SerializeField] private bool m_isInBounds = false;
        [SerializeField] private Vector3 m_lastScale = Vector3.zero;
        [SerializeField] private Vector3 m_lastPosition = Vector3.zero;

        private void OnEnable()
        {
            Setup();
            m_bounds = BuildAndGetBounds();
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                EditorApplication.update -= EditorUpdate;
                EditorApplication.update += EditorUpdate;
            }
            else
            {
                EditorApplication.update -= EditorUpdate;
            }
#endif
        }
        private void OnDestroy()
        {
            if (HDRPTimeOfDay.Instance != null)
            {
                m_isInBounds = HDRPTimeOfDay.Instance.RemoveOverrideVolume(m_id);
            }

            HDRPTimeOfDayOverrideVolumeController controller = HDRPTimeOfDayOverrideVolumeController.Instance;
            if (controller != null)
            {
                controller.RemoveOverrideVolume(this, m_volumeSettings.m_volumeType);
            }
#if UNITY_EDITOR
            EditorApplication.update -= EditorUpdate;
#endif
        }
        private void OnDisable()
        {
            if (HDRPTimeOfDay.Instance != null)
            {
                m_isInBounds = HDRPTimeOfDay.Instance.RemoveOverrideVolume(m_id);
            }
#if UNITY_EDITOR
            EditorApplication.update -= EditorUpdate;
#endif
        }
        private void OnDrawGizmos()
        {
            if (!m_volumeSettings.m_drawGimzo || m_volumeSettings.m_drawGizmoSelectedOnly)
            {
                return;
            }

            Gizmos.color = m_volumeSettings.m_gizmoColor;
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, m_bounds.size);
            Gizmos.DrawCube(Vector3.zero, Vector3.one);
        }
        private void OnDrawGizmosSelected()
        {
            if (!m_volumeSettings.m_drawGimzo && !m_volumeSettings.m_drawGizmoSelectedOnly)
            {
                return;
            }

            Gizmos.color = m_volumeSettings.m_gizmoColor;
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, m_bounds.size);
            Gizmos.DrawCube(Vector3.zero, Vector3.one);

            //Rebuild bounds if scale changes
            if (m_lastScale != transform.localScale || m_lastPosition != transform.position)
            {
                m_bounds = BuildAndGetBounds();
                ApplyLocalFogVolume(true);
                m_lastScale = transform.localScale;
                m_lastPosition = transform.position;
            }
        }
        private void Update()
        {
            if (HDRPTimeOfDay.Instance == null || !HDRPTimeOfDay.Instance.UseOverrideVolumes || !Application.isPlaying)
            {
                return;
            }

            IsInBounds(HDRPTimeOfDay.Instance.GetOverrideVolumeTransform());
            if (m_localFogSettings.m_enabled)
            {
                ApplyLocalFogVolume(true);
            }
        }

        /// <summary>
        /// Sets up this override volume and sets the volume ID
        /// </summary>
        /// <param name="ID"></param>
        public void Setup(int ID = -1)
        {
            if (ID != -1)
            {
                m_id = ID;
            }
            LoadVolume();
        }
        /// <summary>
        /// Loads the volume data and sets it up
        /// </summary>
        public void LoadVolume()
        {
            m_isInBounds = false;
        }
        /// <summary>
        /// Checks to see if the transform player is in the bounding volume
        /// </summary>
        /// <param name="player"></param>
        public void IsInBounds(Transform player)
        {
            if (!enabled)
            {
                return;
            }

            if (player != null)
            {
                bool inBounds = m_bounds.Contains(player.position);
                if (inBounds)
                {
                    m_isInBounds = HDRPTimeOfDay.Instance.AddOverrideVolume(m_id, this);
                }
                else
                {
                    if (m_isInBounds)
                    {
                        m_isInBounds = HDRPTimeOfDay.Instance.RemoveOverrideVolume(m_id);
                    }
                }
            }
        }
        /// <summary>
        /// Adds the volume to the controller that with be managed is active state if it's day or night
        /// </summary>
        public void SetupVolumeTypeToController()
        {
            HDRPTimeOfDayOverrideVolumeController controller = HDRPTimeOfDayOverrideVolumeController.Instance;
            if (!m_volumeSettings.m_addOverrideVolumeType)
            {
                if (controller != null && m_volumeSettings.m_removeFromController)
                {
                    controller.RemoveOverrideVolume(this, m_volumeSettings.m_volumeType);
                }
                return;
            }

            if (controller != null)
            {
                controller.AddOverrideVolume(this, m_volumeSettings.m_volumeType);
            }
        }
        /// <summary>
        /// Applies local fog
        /// </summary>
        /// <param name="runTimeSyncOnly"></param>
        public void ApplyLocalFogVolume(bool runTimeSyncOnly)
        {
            if (m_localFogSettings.m_enabled)
            {
                HDRPTimeOfDay tod = HDRPTimeOfDay.Instance;
                if (tod == null)
                {
                    return; 
                }

                if (m_localFog == null)
                {
                    m_localFog = GetComponent<LocalVolumetricFog>();
                    if (m_localFog == null)
                    {
                        m_localFog = gameObject.AddComponent<LocalVolumetricFog>();
                    }
                }
                if (m_localFog != null)
                {
                    m_localFog.enabled = enabled;
                    if (m_localFogSettings.m_useTODParameters)
                    {
                        m_localFog.parameters.albedo = (tod.GetCurrentFogColor() * m_localFogSettings.m_scatteringAlbedoMultiplier);
                        m_localFog.parameters.meanFreePath = (tod.GetCurrentLocalFogDistance() / m_localFogSettings.m_fogDistanceMultiplier);
                    }
                    else
                    {
                        m_localFog.parameters.albedo = m_localFogSettings.m_scatteringAlbedo;
                        m_localFog.parameters.meanFreePath = (m_localFogSettings.m_fogDistance * m_localFogSettings.m_fogDistanceMultiplier);
                    }

                    m_localFog.parameters.size = transform.localScale * m_localFogSettings.m_scaleMultiplier;
                    if (!runTimeSyncOnly)
                    {
                        m_localFog.parameters.volumeMask = m_localFogSettings.m_densityMask;
                        m_localFog.parameters.textureScrollingSpeed = m_localFogSettings.m_scrollSpeed;
                        m_localFog.parameters.textureTiling = m_localFogSettings.m_tiling;
                        m_localFog.parameters.distanceFadeStart = m_localFogSettings.m_renderDistance;
                        m_localFog.parameters.distanceFadeEnd = m_localFogSettings.m_renderDistance;
                    }
                }
            }
            else
            {
                if (m_localFog != null)
                {
                    DestroyImmediate(m_localFog);
                }
            }
        }
        /// <summary>
        /// Removes the local fog volume
        /// </summary>
        public void RemoveLocalFogVolume()
        {
            if (m_localFog != null)
            {
                DestroyImmediate(m_localFog);
            }
        }

        /// <summary>
        /// Creates and returns the bounds
        /// </summary>
        /// <returns></returns>
        public Bounds BuildAndGetBounds()
        {
            m_lastScale = transform.localScale;
            return new Bounds(transform.position, new Vector3(transform.localScale.x, transform.localScale.y, transform.localScale.z));
        }
        /// <summary>
        /// Editor update function
        /// </summary>
        private void EditorUpdate()
        {
#if UNITY_EDITOR
            if (HDRPTimeOfDay.Instance == null || !HDRPTimeOfDay.Instance.UseOverrideVolumes)
            {
                return;
            }

            if (m_localFogSettings.m_enabled)
            {
                ApplyLocalFogVolume(true);
            }

            SceneView sceneView = SceneView.lastActiveSceneView;
            if (sceneView != null)
            {
                Camera camera = sceneView.camera;
                if (camera != null)
                {
                    IsInBounds(camera.transform);
                }
            }
#endif
        }
    }
}
#endif