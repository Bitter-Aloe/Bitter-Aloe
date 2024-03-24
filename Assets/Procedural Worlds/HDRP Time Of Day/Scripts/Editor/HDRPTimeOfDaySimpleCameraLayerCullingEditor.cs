#if HDPipeline && UNITY_2021_2_OR_NEWER
using PWCommon5;
using UnityEditor;
using UnityEngine;

namespace ProceduralWorlds.HDRPTOD
{
    [CustomEditor(typeof(HDRPTimeOfDaySimpleCameraLayerCulling))]
    public class HDRPTimeOfDaySimpleCameraLayerCullingEditor : PWEditor
    {
        private EditorUtils m_editorUtils;
        private HDRPTimeOfDaySimpleCameraLayerCulling m_simpleCameraLayerCulling;
            
        public void OnEnable()
        {
            m_simpleCameraLayerCulling = (HDRPTimeOfDaySimpleCameraLayerCulling)target;

            if (m_editorUtils == null)
            {
                // Get editor utils for this
                m_editorUtils = PWApp.GetEditorUtils(this);
                m_simpleCameraLayerCulling.Initialize();
                m_simpleCameraLayerCulling.ApplyToSceneCamera();
            }
        }

        public override void OnInspectorGUI()
        {
            //Initialization
            m_editorUtils.Initialize(); // Do not remove this!

            m_editorUtils.Panel("SimpleLayerCulling", SimpleLayerCulling, true);

        }

        private void SimpleLayerCulling(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();
            m_simpleCameraLayerCulling.m_applyToGameCamera = m_editorUtils.Toggle("ApplyToGameCamera", m_simpleCameraLayerCulling.m_applyToGameCamera);
            m_simpleCameraLayerCulling.m_applyToSceneCamera = m_editorUtils.Toggle("ApplyToSceneCamera", m_simpleCameraLayerCulling.m_applyToSceneCamera);

            m_simpleCameraLayerCulling.m_profile = (HDRPTimeOfDaySceneCullingProfile)m_editorUtils.ObjectField("Culling Profile", m_simpleCameraLayerCulling.m_profile, typeof(HDRPTimeOfDaySceneCullingProfile), false, helpEnabled);
            if (m_simpleCameraLayerCulling.m_profile != null)
            {
                m_simpleCameraLayerCulling.m_sunLight = (Light)m_editorUtils.ObjectField("SunLight", m_simpleCameraLayerCulling.m_sunLight, typeof(Light), true, helpEnabled);
                m_simpleCameraLayerCulling.m_moonLight = (Light)m_editorUtils.ObjectField("MoonLight", m_simpleCameraLayerCulling.m_moonLight, typeof(Light), true, helpEnabled);
                m_simpleCameraLayerCulling.LODBias = m_editorUtils.Slider("LODBias", m_simpleCameraLayerCulling.LODBias, 0.001f, 15f, helpEnabled);
                EditorGUILayout.Space();
                m_editorUtils.LabelField("ObjectCullingSettings", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                m_editorUtils.InlineHelp("ObjectCullingSettings", helpEnabled);
                for (int i = 0; i < m_simpleCameraLayerCulling.m_profile.m_layerDistances.Length; i++)
                {
                    string layerName = LayerMask.LayerToName(i);
                    if (!string.IsNullOrEmpty(layerName))
                    {
                        m_simpleCameraLayerCulling.m_profile.m_layerDistances[i] = EditorGUILayout.FloatField(string.Format("[{0}] {1}", i, layerName), m_simpleCameraLayerCulling.m_profile.m_layerDistances[i]);
                    }
                }
                EditorGUI.indentLevel--;
                if (m_editorUtils.Button("RevertCullingToDefaults"))
                {
                    m_simpleCameraLayerCulling.m_profile.UpdateCulling(HDRPTimeOfDay.Instance);
                    EditorUtility.SetDirty(m_simpleCameraLayerCulling.m_profile);
                }

                EditorGUILayout.Space();
                m_editorUtils.LabelField("ShadowCullingSettings", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                m_editorUtils.InlineHelp("ShadowCullingSettings", helpEnabled);
                for (int i = 0; i < m_simpleCameraLayerCulling.m_profile.m_shadowLayerDistances.Length; i++)
                {
                    string layerName = LayerMask.LayerToName(i);
                    if (!string.IsNullOrEmpty(layerName))
                    {
                        m_simpleCameraLayerCulling.m_profile.m_shadowLayerDistances[i] = EditorGUILayout.FloatField(string.Format("[{0}] {1}", i, layerName), m_simpleCameraLayerCulling.m_profile.m_shadowLayerDistances[i]);
                    }
                }

                EditorGUI.indentLevel--;

                if (m_editorUtils.Button("RevertShadowToDefaults"))
                {
                    m_simpleCameraLayerCulling.m_profile.UpdateShadow();
                    EditorUtility.SetDirty(m_simpleCameraLayerCulling.m_profile);
                }

                EditorGUI.indentLevel--;
            }

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(m_simpleCameraLayerCulling.m_profile);
                m_simpleCameraLayerCulling.ApplyToSceneCamera();
                m_simpleCameraLayerCulling.ApplyToGameCamera();
                m_simpleCameraLayerCulling.ResetShadowRenderDistance();
                SceneView.RepaintAll();

            }

        }
    }
}
#endif