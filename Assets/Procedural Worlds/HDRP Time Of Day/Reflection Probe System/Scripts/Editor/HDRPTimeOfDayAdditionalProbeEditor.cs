using System.Collections.Generic;
using PWCommon5;
using UnityEditor;
using UnityEngine;

namespace ProceduralWorlds.HDRPTOD
{
#if HDPipeline
    [CustomEditor(typeof(HDRPTimeOfDayAdditionalProbe))]
    public class HDRPTimeOfDayAdditionalProbeEditor : PWEditor
    {
        private EditorUtils m_editorUtils;
        private HDRPTimeOfDayAdditionalProbe m_manager;
        private GUIStyle m_boxStyle;

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
            m_manager = (HDRPTimeOfDayAdditionalProbe)target;
            m_editorUtils.Initialize();

            //Set up the box style
            if (m_boxStyle == null)
            {
                m_boxStyle = new GUIStyle(GUI.skin.box)
                {
                    normal =
                    {
                        textColor = GUI.skin.label.normal.textColor
                    },
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.UpperLeft
                };
            }

            m_editorUtils.Panel("GlobalPanel", GlobalPanel, true);
        }

        private void GlobalPanel(bool helpEnabled)
        {
            HDRPTimeOfDayReflectionProbeProfile profile = m_manager.Profile;
            if (profile == null)
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.BeginVertical(m_boxStyle);
                EditorGUILayout.LabelField("Global Settings", EditorStyles.boldLabel);
                profile = (HDRPTimeOfDayReflectionProbeProfile)m_editorUtils.ObjectField("ReflectionProbeProfile", profile, typeof(HDRPTimeOfDayReflectionProbeProfile), false, helpEnabled);
                EditorGUILayout.EndVertical();
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(m_manager, "Probe Manager Change Made");
                    m_manager.Profile = profile;
                    EditorUtility.SetDirty(m_manager);
                }

                return;
            }
            float globalMultiplier = m_manager.m_globalMultiplier;
            ProbeRenderMode renderMode = m_manager.Profile.m_renderMode;
            ProbeTimeMode probeTimeMode = m_manager.Profile.m_probeTimeMode;
            ReflectionProbe probe = m_manager.m_globalProbe;
            AnimationCurve transitionCurve = m_manager.Profile.m_transitionIntensityCurve;
            AnimationCurve transitionCurveRT = m_manager.Profile.m_transitionIntensityCurveRT;
            AnimationCurve weightCurve = m_manager.Profile.m_weightCurve;
            bool allowInRayTracing = m_manager.m_allowInRayTracing;


            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginVertical(m_boxStyle);
            EditorGUILayout.LabelField("Global Settings", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            profile = (HDRPTimeOfDayReflectionProbeProfile)m_editorUtils.ObjectField("ReflectionProbeProfile", profile, typeof(HDRPTimeOfDayReflectionProbeProfile), false, helpEnabled);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_manager, "Probe Manager Change Made");
                m_manager.Profile = profile;
                EditorUtility.SetDirty(m_manager);
                EditorGUIUtility.ExitGUI();
            }
            renderMode = (ProbeRenderMode)m_editorUtils.EnumPopup("RenderMode", renderMode, helpEnabled);
            if (renderMode == ProbeRenderMode.Sky)
            {
                EditorGUI.indentLevel++;
                allowInRayTracing = m_editorUtils.Toggle("AllowInRayTracing", allowInRayTracing, helpEnabled);
                globalMultiplier = m_editorUtils.Slider("GlobalMultiplier", globalMultiplier, 0.0001f, 15f, helpEnabled);
                probeTimeMode = (ProbeTimeMode)m_editorUtils.EnumPopup("ProbeTimeMode", probeTimeMode, helpEnabled);
                if (probeTimeMode == ProbeTimeMode.TimeTransition)
                {
                    transitionCurve = m_editorUtils.CurveField("TransitionCurve", transitionCurve, helpEnabled);
                    if (m_manager.m_allowInRayTracing)
                    {
                        transitionCurveRT = m_editorUtils.CurveField("TransitionCurveRT", transitionCurveRT, helpEnabled);
                    }
                    weightCurve = m_editorUtils.CurveField("WeightCurve", weightCurve, helpEnabled);
                }
                probe = (ReflectionProbe)m_editorUtils.ObjectField("GlobalProbe", probe, typeof(ReflectionProbe), true, helpEnabled);
                EditorGUI.indentLevel--;
                EditorGUILayout.EndVertical();

                m_editorUtils.Panel("ProbeDataSettings", ProbeDataPanel);
            }
            else
            {
                EditorGUILayout.EndVertical();
            }

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_manager, "Probe Manager Change Made");
                m_manager.m_globalMultiplier = globalMultiplier;
                m_manager.Profile.m_renderMode = renderMode;
                m_manager.Profile.m_probeTimeMode = probeTimeMode;
                m_manager.m_globalProbe = probe;
                m_manager.Profile.m_transitionIntensityCurve = transitionCurve;
                m_manager.Profile.m_transitionIntensityCurveRT = transitionCurveRT;
                m_manager.Profile.m_weightCurve = weightCurve;
                m_manager.m_allowInRayTracing = allowInRayTracing;
                EditorUtility.SetDirty(m_manager);
                EditorUtility.SetDirty(m_manager.Profile);
#if HDPipeline && UNITY_2021_2_OR_NEWER
                m_manager.Refresh(HDRPTimeOfDayAPI.RayTracingSSGIActive());
#endif
            }
        }

        private void ProbeDataPanel(bool helpEnabled)
        {
            if (m_manager.m_currentData != null)
            {
                EditorGUILayout.LabelField("Current Selected Data: " + m_manager.m_currentData.m_name, EditorStyles.boldLabel);
            }

#if HDPipeline && UNITY_2021_2_OR_NEWER
            if (m_manager.Profile.m_probeTimeMode == ProbeTimeMode.CustomSetTime)
            {
                EditorGUILayout.BeginHorizontal();
                if (m_editorUtils.Button("-1 Hour"))
                {
                    float time = HDRPTimeOfDayAPI.GetCurrentTime();
                    time -= 1f;
                    if (time < 0f)
                    {
                        time = 24f;
                    }
                    HDRPTimeOfDayAPI.SetCurrentTime(time);
                }

                if (m_editorUtils.Button("+1 Hour"))
                {
                    float time = HDRPTimeOfDayAPI.GetCurrentTime();
                    time += 1f;
                    if (time > 24f)
                    {
                        time = 0f;
                    }
                    HDRPTimeOfDayAPI.SetCurrentTime(time);
                }

                if (m_editorUtils.Button("Round Hour"))
                {
                    HDRPTimeOfDayAPI.SetCurrentTime(Mathf.RoundToInt(HDRPTimeOfDayAPI.GetCurrentTime()));
                }

                EditorGUILayout.EndHorizontal();
            }
#endif

            List<ReflectionProbeTODData> todData = m_manager.Profile.m_probeTODData;
            EditorGUI.BeginChangeCheck();
            for (int i = 0; i < todData.Count; i++)
            {
                EditorGUILayout.BeginVertical(m_boxStyle);
                todData[i].m_showSettings = EditorGUILayout.BeginFoldoutHeaderGroup(todData[i].m_showSettings, todData[i].m_name);
                if (todData[i].m_showSettings)
                {
                    todData[i].m_name = m_editorUtils.TextField("Name", todData[i].m_name, helpEnabled);
                    todData[i].m_probeCubeMap = (Cubemap)m_editorUtils.ObjectField("Cubemap", todData[i].m_probeCubeMap, typeof(Cubemap), false, helpEnabled, GUILayout.MaxHeight(16f));
                    todData[i].m_intensity = m_editorUtils.FloatField("Intensity", todData[i].m_intensity, helpEnabled);
                    todData[i].m_weatherIntensityMultiplier = m_editorUtils.FloatField("WeatherMultiplier", todData[i].m_weatherIntensityMultiplier, helpEnabled);
#if HDPipeline && UNITY_2021_2_OR_NEWER
                    todData[i].m_renderLayers = HDRPTimeOfDayEditor.LayerMaskField(m_editorUtils, "RenderLayers", todData[i].m_renderLayers, helpEnabled);
#endif
                    todData[i].m_isNightProbe = m_editorUtils.Toggle("IsNightProbe", todData[i].m_isNightProbe, helpEnabled);
                    todData[i].m_timeOfDayAcceptance = m_editorUtils.Vector2Field("TimeAcceptanceValue", todData[i].m_timeOfDayAcceptance, helpEnabled);
                    todData[i].m_enableSSGIMultiplier = m_editorUtils.Toggle("EnableSSGIMultiplier", todData[i].m_enableSSGIMultiplier, helpEnabled);
                    if (todData[i].m_enableSSGIMultiplier)
                    {
                        EditorGUI.indentLevel++;
                        todData[i].m_ssgiMultiplier = m_editorUtils.Slider("SSGIMultiplier", todData[i].m_ssgiMultiplier, 0f, 5f, helpEnabled);
                        EditorGUI.indentLevel--;
                    }
                    if (m_editorUtils.Button("Remove"))
                    {
                        todData.RemoveAt(i);
                    }
                }
                EditorGUILayout.EndFoldoutHeaderGroup();
                EditorGUILayout.EndVertical();
            }

            if (m_editorUtils.Button("AddNewProbeData"))
            {
                todData.Add(new ReflectionProbeTODData());
            }

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_manager, "Probe Manager Change Made");
                m_manager.Profile.m_probeTODData = todData;
                EditorUtility.SetDirty(m_manager);
                EditorUtility.SetDirty(m_manager.Profile);
            }
        }
    }
#endif
                }