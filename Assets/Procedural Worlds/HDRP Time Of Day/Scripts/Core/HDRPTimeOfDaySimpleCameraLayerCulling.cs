#if HDPipeline && UNITY_2021_2_OR_NEWER
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Compilation;
#endif
using UnityEngine;

namespace ProceduralWorlds.HDRPTOD
{
    [ExecuteAlways]
    public class HDRPTimeOfDaySimpleCameraLayerCulling : MonoBehaviour
    {
        public HDRPTimeOfDaySceneCullingProfile m_profile;
        public bool m_applyToGameCamera = true;
        public bool m_applyToSceneCamera = true;
        public Camera m_mainCamera;
        public Light m_sunLight;
        public Light m_moonLight;
        public HDRPTimeOfDay m_timeOfDay;

        public float LODBias
        {
            get { return m_LODBias; }
            set
            {
                if (m_LODBias != value)
                {
                    m_LODBias = value;
                    ApplyAll();
                }
            }
        }
        [SerializeField] private float m_LODBias = 1f;

        private void OnEnable()
        {
            Initialize();
        }
        private void OnDestroy()
        {
#if UNITY_EDITOR
            CompilationPipeline.compilationFinished -= OnCompilationFinished;
#endif
        }

        /// <summary>
        /// Starts the system
        /// </summary>
        public void Initialize()
        {
            if (m_timeOfDay == null)
            {
                m_timeOfDay = HDRPTimeOfDay.Instance;
                if (m_timeOfDay == null)
                {
                    return;
                }
            }

            if (m_mainCamera == null)
            {
                m_mainCamera = m_timeOfDay.GetMainCamera();
            }

            if (m_sunLight == null || m_moonLight == null)
            {
                HDRPTimeOfDayComponentType[] componentTypes = FindObjectsOfType<HDRPTimeOfDayComponentType>();
                if (componentTypes.Length > 0)
                {
                    foreach (HDRPTimeOfDayComponentType type in componentTypes)
                    {
                        if (type.m_componentType == TimeOfDayComponentType.Sun)
                        {
                            if (m_sunLight == null)
                            {
                                m_sunLight = type.m_lightSource;
                            }
                            continue;
                        }
                        if (type.m_componentType == TimeOfDayComponentType.Moon)
                        {
                            if (m_moonLight == null)
                            {
                                m_moonLight = type.m_lightSource;
                            }
                            continue;
                        }
                    }
                }
            }

            ApplyToGameCamera();
            ApplyToSceneCamera();
#if UNITY_EDITOR
            CompilationPipeline.compilationFinished -= OnCompilationFinished;
            CompilationPipeline.compilationFinished += OnCompilationFinished;
#endif
        }
        /// <summary>
        /// Applies all the settings to everything
        /// </summary>
        public void ApplyAll()
        {
            ApplyToGameCamera();
            ApplyToSceneCamera();
            ApplyShadowsToMainLights();
        }
        /// <summary>
        /// Applies the camera rendering dsitance
        /// </summary>
        public void ApplyToGameCamera()
        {
            if (m_mainCamera == null || m_profile == null)
            {
                return;
            }

            if (m_applyToGameCamera)
            {
                //Layer Culling
                float[] layerCulling = new float[32];
                if (layerCulling.Length == 32)
                {
                    for (int i = 0; i < layerCulling.Length; i++)
                    {
                        float value = m_profile.m_layerDistances[i];
                        layerCulling[i] = value * LODBias;
                    }
                }
                m_mainCamera.layerCullDistances = layerCulling;
                ApplyShadowsToMainLights();
            }
            else
            {
                float[] layerCulls = new float[32];
                for (int i = 0; i < layerCulls.Length; i++)
                {
                    layerCulls[i] = 0f;
                }

                m_mainCamera.layerCullDistances = layerCulls;
            }
        }
        /// <summary>
        /// Applies the scene view settings
        /// </summary>
        public void ApplyToSceneCamera()
        {
#if UNITY_EDITOR
            if (m_profile == null)
            {
                return;
            }

            if (m_applyToSceneCamera)
            {
                //Layer Culling
                float[] layerCulling = new float[32];
                if (layerCulling.Length == 32)
                {
                    for (int i = 0; i < layerCulling.Length; i++)
                    {
                        float value = m_profile.m_layerDistances[i];
                        layerCulling[i] = value * LODBias;
                    }
                }

                foreach (var sceneCamera in SceneView.GetAllSceneCameras())
                {
                    sceneCamera.layerCullDistances = layerCulling;
                }
                ApplyShadowsToMainLights();
            }
            else
            {
                float[] layerCulls = new float[32];
                for (int i = 0; i < layerCulls.Length; i++)
                {
                    layerCulls[i] = 0f;
                }
                foreach (var sceneCamera in SceneView.GetAllSceneCameras())
                {
                    sceneCamera.layerCullDistances = layerCulls;
                }
            }
#endif
        }
        /// <summary>
        /// Applies the shadow distance to the main lights (Sun/Moon)
        /// </summary>
        public void ApplyShadowsToMainLights()
        {
            if (m_profile == null)
            {
                return;
            }

            if (m_sunLight == null || m_moonLight == null)
            {
                if (m_timeOfDay != null)
                {
                    m_timeOfDay.GetMainLightSources(out m_sunLight, out m_moonLight);
                }

                if (m_sunLight == null || m_moonLight == null)
                {
                    return;
                }
            }

            //Shadow Culling
            float[] shadowLayer = new float[32];
            if (shadowLayer.Length == 32)
            {
                for (int i = 0; i < shadowLayer.Length; i++)
                {
                    float value = m_profile.m_shadowLayerDistances[i];
                    shadowLayer[i] = value * LODBias;
                }
            }

            m_sunLight.layerShadowCullDistances = shadowLayer;
            m_moonLight.layerShadowCullDistances = shadowLayer;
        }
        /// <summary>
        /// Resets the shadow culling distance
        /// </summary>
        public void ResetShadowRenderDistance()
        {
            if (!m_applyToGameCamera && Application.isPlaying || !m_applyToSceneCamera && !Application.isPlaying)
            {
                if (m_sunLight != null)
                {
                    float[] layers = new float[32];
                    for (int i = 0; i < layers.Length; i++)
                    {
                        layers[i] = 0f;
                    }
                    m_sunLight.layerShadowCullDistances = layers;
                }
            }
        }
        /// <summary>
        /// When scripts recompiled, call this
        /// </summary>
        /// <param name="obj"></param>
        private void OnCompilationFinished(object obj)
        {
            Refresh();
        }
        /// <summary>
        /// Refreshes all the systems, please don't call every frame and only on awake, start or onenable
        /// </summary>
        public static void Refresh()
        {
            foreach (HDRPTimeOfDaySimpleCameraLayerCulling sclc in FindObjectsOfType<HDRPTimeOfDaySimpleCameraLayerCulling>())
            {
                sclc.ApplyToGameCamera();
                sclc.ApplyToSceneCamera();
            }
        }
    }
}
#endif