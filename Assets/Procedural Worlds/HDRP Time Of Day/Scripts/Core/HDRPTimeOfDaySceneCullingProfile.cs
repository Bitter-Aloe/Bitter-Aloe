#if HDPipeline && UNITY_2021_2_OR_NEWER
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ProceduralWorlds.HDRPTOD
{
    public class HDRPTimeOfDaySceneCullingProfile : ScriptableObject
    {
        public enum ShadowCullingType
        {
            Small,
            Medium,
            Large
        }

        [Header("Global Settings")]
        public bool m_applyToEditorCamera = false;
        public bool m_realtimeUpdate = false;
        public float[] m_layerDistances = new float[32];
        public string[] m_layerNames = new string[32];
        public float[] m_shadowLayerDistances = new float[32];
        public float m_additionalCullingDistance = 0f;

        /// <summary>
        /// Updates the culling distance defaults
        /// </summary>
        /// <param name="timeOfDay"></param>
        public void UpdateCulling(HDRPTimeOfDay timeOfDay)
        {
            if (timeOfDay == null)
            {
                return;
            }

            Camera camera = timeOfDay.GetMainCamera();
            if (camera == null)
            {
                return;
            }

            Terrain terrain = Terrain.activeTerrain;

            //Objects
            m_layerDistances = new float[32];
            for (int i = 0; i < m_layerDistances.Length; i++)
            {
                string layerName = LayerMask.LayerToName(i);
                switch (layerName)
                {
                    case "Default":
                    case "Water":
                    case "PW_VFX":
                        m_layerDistances[i] = 0f;
                        break;
                    case "PW_Object_Small":
                        m_layerDistances[i] = CalculateCameraCullingLayerValue(terrain, GeneralQuality.Low, 5f);
                        break;
                    case "PW_Object_Medium":
                        m_layerDistances[i] = CalculateCameraCullingLayerValue(terrain, GeneralQuality.Medium, 3f);
                        break;
                    case "PW_Object_Large":
                        m_layerDistances[i] = CalculateCameraCullingLayerValue(terrain, GeneralQuality.High);
                        break;
                    default:
                        m_layerDistances[i] = 0f;
                        break;
                }
            }
        }
        /// <summary>
        /// Updates the shadow defaults
        /// </summary>
        public void UpdateShadow()
        {
            //Shadows
            m_shadowLayerDistances = new float[32];
            for (int i = 0; i < m_shadowLayerDistances.Length; i++)
            {
                string layerName = LayerMask.LayerToName(i);
                switch (layerName)
                {
                    case "Default":
                    case "Water":
                    case "PW_VFX":
                        m_shadowLayerDistances[i] = 0f;
                        break;
                    case "PW_Object_Small":
                        m_shadowLayerDistances[i] = CalculateShadowCullingLayerValue(ShadowCullingType.Small, QualitySettings.shadowDistance, 0f, 0f, 5f);
                        break;
                    case "PW_Object_Medium":
                        m_shadowLayerDistances[i] = CalculateShadowCullingLayerValue(ShadowCullingType.Medium, QualitySettings.shadowDistance, 0f, 3f, 0f);
                        break;
                    case "PW_Object_Large":
                        m_shadowLayerDistances[i] = CalculateShadowCullingLayerValue(ShadowCullingType.Large, QualitySettings.shadowDistance, 1f, 0f, 0f);
                        break;
                    default:
                        m_shadowLayerDistances[i] = 0f;
                        break;
                }
            }
        }

        /// <summary>
        /// Function used to set the value of the camera layer culling based on valued provided
        /// </summary>
        /// <param name="terrain"></param>
        /// <param name="targetQuality"></param>
        /// <param name="extraValue"></param>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        public static float CalculateCameraCullingLayerValue(Terrain terrain, GeneralQuality targetQuality, float extraValue = 1f, float maxValue = 2000f)
        {
            float returningValue = maxValue;
            if (terrain == null)
            {
                return returningValue;
            }
                
            switch (targetQuality)
            {
                case GeneralQuality.Low:
                {
                    returningValue = terrain.terrainData.size.x / (8f * extraValue);
                    break;
                }
                case GeneralQuality.Medium:
                {
                    returningValue = terrain.terrainData.size.x / (5f * extraValue);
                    break;
                }
                case GeneralQuality.High:
                {
                    returningValue = terrain.terrainData.size.x / (1f * extraValue);
                    break;
                }
            }

            return Mathf.Round(returningValue);
        }
        /// <summary>
        /// Update the shadow distance based on the quality settings shadow distance.
        /// The value is devided by the float 'Large, Medium, Small' based on the type provided
        /// </summary>
        /// <param name="type"></param>
        /// <param name="qualityShadowDistance"></param>
        /// <param name="largeLayer"></param>
        /// <param name="mediumLayer"></param>
        /// <param name="smallLayer"></param>
        /// <returns></returns>
        public static float CalculateShadowCullingLayerValue(ShadowCullingType type, float qualityShadowDistance, float largeLayer, float mediumLayer, float smallLayer)
        {
            float returningValue = qualityShadowDistance;
            switch (type)
            {
                case ShadowCullingType.Large:
                    returningValue = qualityShadowDistance / largeLayer;
                    break;
                case ShadowCullingType.Medium:
                    returningValue = qualityShadowDistance / mediumLayer;
                    break;
                case ShadowCullingType.Small:
                    returningValue = qualityShadowDistance / smallLayer;
                    break;
            }

            return Mathf.Round(returningValue);
        }

        /// <summary>
        /// Create HDRP Time Of Day Culling System Profile asset
        /// </summary>
#if UNITY_EDITOR
        [MenuItem("Assets/Create/Procedural Worlds/HDRP Time Of Day/Scene Culling Profile")]
        public static void CreateCullingProfileMenu()
        {
            HDRPTimeOfDaySceneCullingProfile asset = ScriptableObject.CreateInstance<HDRPTimeOfDaySceneCullingProfile>();
            asset.UpdateCulling(HDRPTimeOfDay.Instance);
            asset.UpdateShadow();
            AssetDatabase.CreateAsset(asset, "Assets/HDRP Time Of Day Scene Culling Profile.asset");
            AssetDatabase.SaveAssets();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
        }
#endif
    }
}
#endif