#if HDPipeline && UNITY_2021_2_OR_NEWER
using PWCommon5;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace ProceduralWorlds.HDRPTOD
{
    [CustomEditor(typeof(HDRPTimeOfDayOverrideVolume))]
    public class HDRPTimeOfDayOverrideVolumeEditor : PWEditor
    {
        private HDRPTimeOfDayOverrideVolume m_overrideVolume;
        private GUIStyle m_boxStyle;
        private EditorUtils m_editorUtils;

        private void OnEnable()
        {
            m_overrideVolume = (HDRPTimeOfDayOverrideVolume) target;
            if (m_overrideVolume != null)
            {
                m_overrideVolume.SetupVolumeTypeToController();
            }

            if (HDRPTimeOfDay.Instance != null)
            {
                if (HDRPTimeOfDay.Instance.AutoOrganizeOverrideVolumes)
                {
                    HDRPTimeOfDay.Instance.ParentOverrideVolume(m_overrideVolume);
                }
            }

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
            m_overrideVolume = (HDRPTimeOfDayOverrideVolume) target;
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
            if (HDRPTimeOfDay.Instance != null)
            {
                if (HDRPTimeOfDay.Instance.UseOverrideVolumes)
                {
                    Color color = m_overrideVolume.m_volumeSettings.m_gizmoColor;
                    bool drawGizmoSelected = m_overrideVolume.m_volumeSettings.m_drawGizmoSelectedOnly;
                    bool activeInWeather = m_overrideVolume.m_volumeSettings.m_volumeActiveInWeather;
                    float blendTime = m_overrideVolume.m_volumeSettings.m_blendTime;
                    bool addOverrideVolumeType = m_overrideVolume.m_volumeSettings.m_addOverrideVolumeType;
                    bool removeFromController = m_overrideVolume.m_volumeSettings.m_removeFromController;
                    OverrideTODType volumeType = m_overrideVolume.m_volumeSettings.m_volumeType;
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.BeginVertical(m_boxStyle);
                    color = m_editorUtils.ColorField("GizmoColor", color, helpEnabled);
                    EditorGUI.indentLevel++;
                    drawGizmoSelected = m_editorUtils.Toggle("DrawGizmoSelectedOnly", drawGizmoSelected, helpEnabled);
                    EditorGUI.indentLevel--;
                    blendTime = m_editorUtils.FloatField("BlendTime", blendTime, helpEnabled);
                    GUI.enabled = false;
                    activeInWeather = m_editorUtils.Toggle("ActiveInWeather", activeInWeather, helpEnabled);
                    GUI.enabled = true;
                    activeInWeather = false;
                    if (!activeInWeather)
                    {
                        EditorGUILayout.HelpBox("Active in weather is still in development and has been disabled in the meantime. The overrides will not work while a weather effect is activated.", MessageType.Info);
                    }
                    addOverrideVolumeType = m_editorUtils.Toggle("AddVolumeToController", addOverrideVolumeType, helpEnabled);
                    EditorGUI.indentLevel++;
                    if (addOverrideVolumeType)
                    {
                        volumeType = (OverrideTODType)m_editorUtils.EnumPopup("VolumeTimeOfDayType", volumeType, helpEnabled);
                    }
                    else
                    {
                        removeFromController = m_editorUtils.Toggle("RemoveVolumeFromController", removeFromController, helpEnabled);
                    }
                    EditorGUI.indentLevel--;
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(m_overrideVolume, "Override Volume Changes Made");
                        m_overrideVolume.m_volumeSettings.m_gizmoColor = color;
                        m_overrideVolume.m_volumeSettings.m_drawGizmoSelectedOnly = drawGizmoSelected;
                        m_overrideVolume.m_volumeSettings.m_volumeActiveInWeather = activeInWeather;
                        m_overrideVolume.m_volumeSettings.m_blendTime = blendTime;
                        m_overrideVolume.m_volumeSettings.m_addOverrideVolumeType = addOverrideVolumeType;
                        m_overrideVolume.m_volumeSettings.m_removeFromController = removeFromController;
                        m_overrideVolume.m_volumeSettings.m_volumeType = volumeType;
                        m_overrideVolume.SetupVolumeTypeToController();
                        if (HDRPTimeOfDay.Instance.AutoOrganizeOverrideVolumes)
                        {
                            HDRPTimeOfDay.Instance.ParentOverrideVolume(m_overrideVolume);
                        }
                        EditorUtility.SetDirty(m_overrideVolume);
                    }

                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.BeginVertical(m_boxStyle);
                    EditorGUILayout.LabelField("Override Settings", EditorStyles.boldLabel);
                    DrawClampedSliderParamaterGUI(ref m_overrideVolume.m_volumeSettings.m_sunVolumetric, m_overrideVolume, "SunVolumetric", m_editorUtils, helpEnabled);
                    DrawClampedSliderParamaterGUI(ref m_overrideVolume.m_volumeSettings.m_sunVolumetricDimmer, m_overrideVolume, "SunVolumetricDimmer", m_editorUtils, helpEnabled);
                    DrawClampedSliderParamaterGUI(ref m_overrideVolume.m_volumeSettings.m_exposure, m_overrideVolume, "Exposure", m_editorUtils, helpEnabled);
                    DrawClampedSliderParamaterGUI(ref m_overrideVolume.m_volumeSettings.m_ambientIntensity, m_overrideVolume, "AmbientIntensity", m_editorUtils, helpEnabled);
                    DrawClampedSliderParamaterGUI(ref m_overrideVolume.m_volumeSettings.m_ambientReflectionIntensity, m_overrideVolume, "AmbientReflectionIntensity", m_editorUtils, helpEnabled);
                    DrawClampedSliderParamaterGUI(ref m_overrideVolume.m_volumeSettings.m_shadowDistanceMultiplier, m_overrideVolume, "ShadowDistanceMultiplier", m_editorUtils, helpEnabled);
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.BeginVertical(m_boxStyle);
                    EditorGUILayout.LabelField("Local Fog Settings", EditorStyles.boldLabel);

                    bool enabled = m_overrideVolume.m_localFogSettings.m_enabled;
                    bool useTODParameters = m_overrideVolume.m_localFogSettings.m_useTODParameters;
                    float fogScaleMultiplier = m_overrideVolume.m_localFogSettings.m_scaleMultiplier;
                    Color scatteringAlbedo = m_overrideVolume.m_localFogSettings.m_scatteringAlbedo;
                    Color scatteringAlbedoMultiplier = m_overrideVolume.m_localFogSettings.m_scatteringAlbedoMultiplier;
                    float fogDistance = m_overrideVolume.m_localFogSettings.m_fogDistance;
                    float fogDistanceMultiplier = m_overrideVolume.m_localFogSettings.m_fogDistanceMultiplier;
                    float renderDistance = m_overrideVolume.m_localFogSettings.m_renderDistance;
                    Texture densityMask = m_overrideVolume.m_localFogSettings.m_densityMask;
                    Vector3 scrollSpeed = m_overrideVolume.m_localFogSettings.m_scrollSpeed;
                    Vector3 tiling = m_overrideVolume.m_localFogSettings.m_tiling;

                    EditorGUI.BeginChangeCheck();
                    enabled = EditorGUILayout.Toggle("Enable", enabled);
                    if (enabled)
                    {
                        EditorGUI.indentLevel++;
                        renderDistance = m_editorUtils.FloatField("RenderDistance", renderDistance, helpEnabled);
                        if (renderDistance < 0.1f)
                        {
                            renderDistance = 0.1f;
                        }
                        fogScaleMultiplier = m_editorUtils.Slider("Fog Size Multiplier", fogScaleMultiplier, 0.05f, 25f, helpEnabled);
                        useTODParameters = m_editorUtils.Toggle("UseTODParameters", useTODParameters, helpEnabled);
                        if (!useTODParameters)
                        {
                            scatteringAlbedo = m_editorUtils.ColorField("ScatteringAlbedo", scatteringAlbedo, helpEnabled);
                            fogDistance = m_editorUtils.FloatField("FogDistance", fogDistance, helpEnabled);
                            fogDistanceMultiplier = m_editorUtils.FloatField("FogDensityMultiplier", fogDistanceMultiplier, helpEnabled);
                            if (fogDistanceMultiplier < 0.01f)
                            {
                                fogDistanceMultiplier = 0.01f;
                            }
                        }
                        else
                        {
                            scatteringAlbedoMultiplier = m_editorUtils.ColorField("ScatteringAlbedoMultiplier", scatteringAlbedoMultiplier, helpEnabled);
                            fogDistanceMultiplier = m_editorUtils.FloatField("FogDensityMultiplier", fogDistanceMultiplier, helpEnabled);
                            if (fogDistanceMultiplier < 0.01f)
                            {
                                fogDistanceMultiplier = 0.01f;
                            }
                        }
                        densityMask = (Texture)m_editorUtils.ObjectField("DensityMask", densityMask, typeof(Texture), false, helpEnabled, GUILayout.MaxHeight(16f));
                        if (densityMask != null)
                        {
                            scrollSpeed = m_editorUtils.Vector3Field("ScrollSpeed", scrollSpeed, helpEnabled);
                            tiling = m_editorUtils.Vector3Field("Tiling", tiling, helpEnabled);
                        }
                        EditorGUI.indentLevel--;
                    }
                    EditorGUILayout.EndVertical();
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(m_overrideVolume, "Override Volume Changes Made");
                        m_overrideVolume.m_localFogSettings.m_enabled = enabled;
                        m_overrideVolume.m_localFogSettings.m_useTODParameters = useTODParameters;
                        m_overrideVolume.m_localFogSettings.m_scaleMultiplier = fogScaleMultiplier;
                        m_overrideVolume.m_localFogSettings.m_scatteringAlbedo = scatteringAlbedo;
                        m_overrideVolume.m_localFogSettings.m_scatteringAlbedoMultiplier = scatteringAlbedoMultiplier;
                        m_overrideVolume.m_localFogSettings.m_fogDistance = fogDistance;
                        m_overrideVolume.m_localFogSettings.m_fogDistanceMultiplier = fogDistanceMultiplier;
                        m_overrideVolume.m_localFogSettings.m_renderDistance = renderDistance;
                        m_overrideVolume.m_localFogSettings.m_densityMask = densityMask;
                        m_overrideVolume.m_localFogSettings.m_scrollSpeed = scrollSpeed;
                        m_overrideVolume.m_localFogSettings.m_tiling = tiling;
                        m_overrideVolume.ApplyLocalFogVolume(false);
                        EditorUtility.SetDirty(m_overrideVolume);
                    }

                    EditorGUILayout.EndVertical();
                    if (EditorGUI.EndChangeCheck())
                    {
                        if (HDRPTimeOfDay.Instance != null)
                        {
                            HDRPTimeOfDay.Instance.RefreshOverrideVolume = true;
                        }
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("Use Override Volumes is disabled, to use override volumes in the Time Of Day system please enable this feature", MessageType.Info);
                    if (m_editorUtils.Button("EnableOverrideVolumeSupport"))
                    {
                        HDRPTimeOfDay.Instance.UseOverrideVolumes = true;
                    }
                }
            }
            else
            {
                EditorGUILayout.HelpBox("HDRP Time Of Day was not found this system requires it to work. You can add it 'Window/Procedural Worlds/HDRP/Time Of Day/Add Time Of Day' or click 'Add Time Of Day Below'.", MessageType.Warning);
                if (m_editorUtils.Button("AddTimeOfDay"))
                {
                    HDRPTimeOfDay.CreateTimeOfDay(null, false);
                    EditorGUIUtility.ExitGUI();
                }
            }
        }
        /// <summary>
        /// Draws bool paramater GUI
        /// </summary>
        /// <param name="boolParameter"></param>
        /// <param name="localization"></param>
        /// <param name="localizationKey"></param>
        /// <returns></returns>
        public static void DrawBoolParamaterGUI(ref BoolParameter boolParameter, Object recordedObject, string localization, EditorUtils editorUtils, bool helpEnabled)
        {
            bool overrideState = boolParameter.overrideState;
            bool value = boolParameter.value;

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginHorizontal();
            overrideState = EditorGUILayout.Toggle(new GUIContent(""), overrideState, GUILayout.MaxWidth(25f));
            if (!overrideState)
            {
                GUI.enabled = false;
            }
            else
            {
                GUI.enabled = true;
            }
            EditorGUILayout.LabelField(new GUIContent(editorUtils.GetTextValue(localization), editorUtils.GetTooltip(localization)), GUILayout.MaxWidth(145f));
            value = EditorGUILayout.Toggle(new GUIContent(""), value);
            EditorGUILayout.EndHorizontal();
            editorUtils.InlineHelp(localization, helpEnabled);
            GUI.enabled = true;

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(recordedObject, "Changes made");
                boolParameter.overrideState = overrideState;
                boolParameter.value = value;
                EditorUtility.SetDirty(recordedObject);
            }
        }
        /// <summary>
        /// Draws clamped float paramater GUI
        /// </summary>
        /// <param name="boolParameter"></param>
        /// <param name="localization"></param>
        /// <param name="localizationKey"></param>
        /// <returns></returns>
        public static void DrawClampedFloatParamaterGUI(ref ClampedFloatParameter floatParameter, Object recordedObject, string localization, EditorUtils editorUtils, bool helpEnabled)
        {
            bool overrideState = floatParameter.overrideState;
            float value = floatParameter.value;

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginHorizontal();
            overrideState = EditorGUILayout.Toggle(new GUIContent(""), overrideState, GUILayout.MaxWidth(25f));
            if (!overrideState)
            {
                GUI.enabled = false;
            }
            else
            {
                GUI.enabled = true;
            }
            EditorGUILayout.LabelField(new GUIContent(editorUtils.GetTextValue(localization), editorUtils.GetTooltip(localization)), GUILayout.MaxWidth(145f));
            value = EditorGUILayout.FloatField(new GUIContent(""), value);
            EditorGUILayout.EndHorizontal();
            editorUtils.InlineHelp(localization, helpEnabled);
            GUI.enabled = true;

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(recordedObject, "Changes made");
                floatParameter.overrideState = overrideState;
                floatParameter.value = value;
                EditorUtility.SetDirty(recordedObject);
            }
        }
        /// <summary>
        /// Draws clamped float paramater GUI
        /// </summary>
        /// <param name="boolParameter"></param>
        /// <param name="localization"></param>
        /// <param name="localizationKey"></param>
        /// <returns></returns>
        public static void DrawFloatParamaterGUI(ref FloatParameter floatParameter, Object recordedObject, string localization, EditorUtils editorUtils, bool helpEnabled)
        {
            bool overrideState = floatParameter.overrideState;
            float value = floatParameter.value;

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginHorizontal();
            overrideState = EditorGUILayout.Toggle(new GUIContent(""), overrideState, GUILayout.MaxWidth(25f));
            if (!overrideState)
            {
                GUI.enabled = false;
            }
            else
            {
                GUI.enabled = true;
            }
            EditorGUILayout.LabelField(new GUIContent(editorUtils.GetTextValue(localization), editorUtils.GetTooltip(localization)), GUILayout.MaxWidth(145f));
            value = EditorGUILayout.FloatField(new GUIContent(""), value);
            EditorGUILayout.EndHorizontal();
            editorUtils.InlineHelp(localization, helpEnabled);
            GUI.enabled = true;

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(recordedObject, "Changes made");
                floatParameter.overrideState = overrideState;
                floatParameter.value = value;
                EditorUtility.SetDirty(recordedObject);
            }
        }
        /// <summary>
        /// Draws clamped float paramater GUI
        /// </summary>
        /// <param name="boolParameter"></param>
        /// <param name="localization"></param>
        /// <param name="localizationKey"></param>
        /// <returns></returns>
        public static void DrawClampedSliderParamaterGUI(ref ClampedFloatParameter floatParameter, Object recordedObject, string localization, EditorUtils editorUtils, bool helpEnabled)
        {
            bool overrideState = floatParameter.overrideState;
            float value = floatParameter.value;

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginHorizontal();
            overrideState = EditorGUILayout.Toggle(new GUIContent(""), overrideState, GUILayout.MaxWidth(25f));
            if (!overrideState)
            {
                GUI.enabled = false;
            }
            else
            {
                GUI.enabled = true;
            }
            EditorGUILayout.LabelField(new GUIContent(editorUtils.GetTextValue(localization), editorUtils.GetTooltip(localization)), GUILayout.MaxWidth(EditorGUIUtility.labelWidth - 25f));
            value = EditorGUILayout.Slider(new GUIContent(""), value, floatParameter.min, floatParameter.max);
            EditorGUILayout.EndHorizontal();
            editorUtils.InlineHelp(localization, helpEnabled);
            GUI.enabled = true;

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(recordedObject, "Changes made");
                floatParameter.overrideState = overrideState;
                floatParameter.value = value;
                EditorUtility.SetDirty(recordedObject);
            }
        }
        /// <summary>
        /// Draws clamped float paramater GUI
        /// </summary>
        /// <param name="boolParameter"></param>
        /// <param name="localization"></param>
        /// <param name="localizationKey"></param>
        /// <returns></returns>
        public static void DrawClampedSliderParamaterGUI(Rect rect, ref ClampedFloatParameter floatParameter, Object recordedObject, string localization, EditorUtils editorUtils, bool helpEnabled)
        {
            bool overrideState = floatParameter.overrideState;
            float value = floatParameter.value;

            EditorGUI.BeginChangeCheck();
            rect.x = EditorGUIUtility.currentViewWidth / 4f;
            overrideState = EditorGUI.Toggle(rect, overrideState);
            if (!overrideState)
            {
                GUI.enabled = false;
            }
            else
            {
                GUI.enabled = true;
            }
            rect.x = EditorGUIUtility.currentViewWidth / 3f;
            EditorGUI.LabelField(rect, new GUIContent(editorUtils.GetTextValue(localization), editorUtils.GetTooltip(localization)));
            rect.x = EditorGUIUtility.currentViewWidth / 2f;
            value = EditorGUI.Slider(rect, value, floatParameter.min, floatParameter.max);
            editorUtils.InlineHelp(localization, helpEnabled);
            GUI.enabled = true;

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(recordedObject, "Changes made");
                floatParameter.overrideState = overrideState;
                floatParameter.value = value;
                EditorUtility.SetDirty(recordedObject);
            }
        }
        /// <summary>
        /// Draws clamped float paramater GUI
        /// </summary>
        /// <param name="boolParameter"></param>
        /// <param name="localization"></param>
        /// <param name="localizationKey"></param>
        /// <returns></returns>
        public static void DrawEnumPopupParamaterGUI(ref ClampedIntParameter intParameter, Object recordedObject, string localization, string[] options, EditorUtils editorUtils, bool helpEnabled)
        {
            bool overrideState = intParameter.overrideState;
            int value = intParameter.value;

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginHorizontal();
            overrideState = EditorGUILayout.Toggle(new GUIContent(""), overrideState, GUILayout.MaxWidth(25f));
            if (!overrideState)
            {
                GUI.enabled = false;
            }
            else
            {
                GUI.enabled = true;
            }
            EditorGUILayout.LabelField(new GUIContent(editorUtils.GetTextValue(localization), editorUtils.GetTooltip(localization)), GUILayout.MaxWidth(145f));
            value = EditorGUILayout.Popup(new GUIContent(""), value, options);
            EditorGUILayout.EndHorizontal();
            editorUtils.InlineHelp(localization, helpEnabled);
            GUI.enabled = true;
            
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(recordedObject, "Changes made");
                intParameter.overrideState = overrideState;
                intParameter.value = value;
                EditorUtility.SetDirty(recordedObject);
            }
        }
    }
}
#endif