#if HDPipeline && UNITY_2021_2_OR_NEWER
using System.Collections;
using System.Collections.Generic;
using PWCommon5;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace ProceduralWorlds.HDRPTOD
{
    [CustomEditor(typeof(HDRPTimeOfDayLightComponent))]
    public class HDRPTimeOfDayLightComponentEditor : PWEditor
    {
        private HDRPTimeOfDayLightComponent m_editor;
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
            m_editor = (HDRPTimeOfDayLightComponent)target;
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
            EditorGUILayout.LabelField("Component Settings");
            m_editor.m_lightSource = (Light)m_editorUtils.ObjectField("Light Source", m_editor.m_lightSource, typeof(Light), true, helpEnabled);
            m_editor.m_lightData = (HDAdditionalLightData)m_editorUtils.ObjectField("Light Data", m_editor.m_lightData, typeof(HDAdditionalLightData), true, helpEnabled);

            EditorGUILayout.EndVertical();
            if (EditorGUI.EndChangeCheck())
            {
                m_editor.RefreshLightSource();
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginVertical(m_boxStyle);
            EditorGUILayout.LabelField("Light Sync Settings");
            m_editor.m_renderMode = (LightSyncRenderMode)m_editorUtils.EnumPopup("RenderMode", m_editor.m_renderMode, helpEnabled);
            if (m_editor.m_renderMode != LightSyncRenderMode.AlwaysOn)
            {
                EditorGUI.indentLevel++;
                m_editor.m_useRenderDistance = m_editorUtils.Toggle("UseRenderDistance", m_editor.m_useRenderDistance, helpEnabled);
                if (m_editor.m_useRenderDistance)
                {
                    EditorGUI.indentLevel++;
                    m_editor.m_renderDistance = m_editorUtils.FloatField("RenderDistance", m_editor.m_renderDistance, helpEnabled);
                    EditorGUI.indentLevel--;
                }

                if (m_editor.m_renderMode == LightSyncRenderMode.NightOnly)
                {
                    m_editor.m_turnOffDelay = m_editorUtils.Toggle("TurnOffDelay", m_editor.m_turnOffDelay, helpEnabled);
                    if (m_editor.m_turnOffDelay)
                    {
                        EditorGUI.indentLevel++;
                        m_editor.m_delayTimer = m_editorUtils.FloatField("DelayTimer", m_editor.m_delayTimer, helpEnabled);
                        EditorGUI.indentLevel--;
                    }
                }

                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndVertical();
            if (EditorGUI.EndChangeCheck())
            {
                m_editor.Refresh();
            }
        }
    }
}
#endif