#if HDPipeline && UNITY_2021_2_OR_NEWER
using PWCommon5;
using UnityEditor;
using UnityEngine;

namespace ProceduralWorlds.HDRPTOD
{
    [CustomEditor(typeof(HDRPTimeOfDayLightProbeVolume))]
    public class HDRPTimeOfDayLightProbeVolumeEditor : PWEditor
    {
        private HDRPTimeOfDayLightProbeVolume m_editor;
        private GUIStyle m_boxStyle;
        private EditorUtils m_editorUtils;

        private void OnEnable()
        {
            m_editorUtils = PWApp.GetEditorUtils(this);
        }
        private void OnDestroy()
        {
            if (m_editorUtils != null)
            {
                m_editorUtils.Dispose();
                m_editorUtils = null;
            }
        }
        public override void OnInspectorGUI()
        {
            m_editorUtils.Initialize();
            m_editor = (HDRPTimeOfDayLightProbeVolume)target;
            //Set up the box style
            if (m_boxStyle == null)
            {
                m_boxStyle = new GUIStyle(GUI.skin.box)
                {
                    normal = {textColor = GUI.skin.label.normal.textColor},
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.UpperLeft
                };
            }
          
            m_editorUtils.Panel("GlobalPanel", GlobalPanel, true);
        }

        private void GlobalPanel(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginVertical(m_boxStyle);
            EditorGUILayout.LabelField("Global Settings", EditorStyles.boldLabel);
            m_editor.m_isEnabled = m_editorUtils.Toggle("Enabled", m_editor.m_isEnabled);
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(m_boxStyle);
            EditorGUILayout.LabelField("Volume Settings", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            m_editor.m_useJitter = m_editorUtils.Toggle("UseJitter", m_editor.m_useJitter, helpEnabled);
            if (m_editor.m_useJitter)
            {
                EditorGUI.indentLevel++;
                m_editor.m_jitterAmount = m_editorUtils.Slider("JitterAmount", m_editor.m_jitterAmount, 0f, 1f, helpEnabled);
                EditorGUI.indentLevel--;
            }

            if (EditorGUI.EndChangeCheck())
            {
                m_editor.BuildCachedPosition();
                EditorUtility.SetDirty(m_editor);
            }
            m_editor.DensityX = m_editorUtils.IntField("DensityX", m_editor.DensityX, helpEnabled);
            m_editor.DensityY = m_editorUtils.IntField("DensityY", m_editor.DensityY, helpEnabled);
            m_editor.DensityZ = m_editorUtils.IntField("DensityZ", m_editor.DensityZ, helpEnabled);
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(m_boxStyle);
            EditorGUILayout.LabelField("Filter Settings", EditorStyles.boldLabel);
            m_editor.m_useIncludeVolumes = m_editorUtils.Toggle("UseIncludeVolumes", m_editor.m_useIncludeVolumes, helpEnabled);
            m_editor.m_excludeMeshRendererBounds = m_editorUtils.Toggle("FilterMeshRenderers", m_editor.m_excludeMeshRendererBounds, helpEnabled);
            m_editor.m_testSeaLevel = m_editorUtils.Toggle("FilterSeaLevel", m_editor.m_testSeaLevel, helpEnabled);
            if (m_editor.m_testSeaLevel)
            {
                EditorGUI.indentLevel++;
                m_editor.m_seaLevel = m_editorUtils.FloatField("SeaLevel", m_editor.m_seaLevel, helpEnabled);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(m_boxStyle);
            EditorGUILayout.LabelField("Debug Settings", EditorStyles.boldLabel);
            m_editor.m_drawGizmos = m_editorUtils.Toggle("DrawGizmos", m_editor.m_drawGizmos, helpEnabled);
            if (m_editor.m_drawGizmos)
            {
                EditorGUI.indentLevel++;
                m_editor.m_drawProbePositionGizmos = m_editorUtils.Toggle("DrawProbeGizmos", m_editor.m_drawProbePositionGizmos, helpEnabled);
                m_editor.m_boundsGizmoColor = m_editorUtils.ColorField("BoundsColor", m_editor.m_boundsGizmoColor, helpEnabled);
                m_editor.m_probePositionGizmoColor = m_editorUtils.ColorField("ProbePositionColor", m_editor.m_probePositionGizmoColor, helpEnabled);
                m_editor.m_probeGizmoSize = m_editorUtils.FloatField("ProbePositionGizmoSize", m_editor.m_probeGizmoSize, helpEnabled);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(m_editor);
            }

            if (m_editorUtils.Button("BuildThisVolume"))
            {
                m_editor.BuildVolumeProbes();
            }
            if (m_editorUtils.Button("RefreshVolume"))
            {
                m_editor.Refresh();
            }
        }
    }
}
#endif