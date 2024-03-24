#if HDPipeline && UNITY_2021_2_OR_NEWER
using System.Collections.Generic;
using PWCommon5;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace ProceduralWorlds.HDRPTOD
{
    [CustomEditor(typeof(HDRPTimeOfDay))]
    public class HDRPTimeOfDayEditor : PWEditor
    {
        public static Color m_defaultIndicatorColor = Color.yellow;
        public static float m_defaultIndicatorWidth = 2.0f;
        
        private HDRPTimeOfDay m_tod;
        private GUIStyle m_boxStyle;
        private SceneView m_sceneView;
        private static EditorUtils m_editorUtils;
        private float TODPercentage => m_tod != null ? m_tod.TimeOfDay / 24.0f : 0.0f;

        #region Unity Functions

        private void OnEnable()
        {
            m_tod = (HDRPTimeOfDay) target;
            if (m_tod != null)
            {
                m_tod.SetHasBeenSetup(m_tod.SetupHDRPTimeOfDay());
                m_tod.m_cameraSettings.ApplyCameraSettings(m_tod.GetTODComponents().m_cameraData, m_tod.GetTODComponents().m_camera);
            }

            EditorApplication.update -= SimulateUpdate;
            m_sceneView = SceneView.lastActiveSceneView;

            m_editorUtils = PWApp.GetEditorUtils(this);
            CheckBakedRealtimeGIState();
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
            if (m_editorUtils == null)
            {
                return;
            }

            m_editorUtils.Initialize();
            if (m_tod == null)
            {
                m_tod = (HDRPTimeOfDay) target;
            }

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

            m_editorUtils.GUIHeader(true, "", "", "HDRP Time Of Day");
            m_editorUtils.GUINewsHeader(true, new URLParameters() { m_product = "HDRP Time Of Day" });

            m_editorUtils.GUINewsFooter(false, new URLParameters() { m_product = "HDRP Time Of Day" });

            m_editorUtils.Panel("GlobalPanel", GlobalPanel, true);
            m_editorUtils.Panel("TimeOfDayPanel", TimeOfDayPanel, true);
            m_editorUtils.Panel("PostProcessingPanel", PostProcessingPanel);
            m_editorUtils.Panel("AmbientAudioPanel", AmbientAudioPanel);
            m_editorUtils.Panel("SeasonPanel", SeasonPanel);
            m_editorUtils.Panel("UnderwaterPanel", UnderwaterPanel);
            m_editorUtils.Panel("WeatherPanel", WeatherPanel);
            m_editorUtils.Panel("LightProbePanel", LightProbePanel);
            m_editorUtils.Panel("DebugPanel", DebugPanel);
        }

        #endregion
        #region Panels

        private void GlobalPanel(bool helpEnabled)
        {
            EditorGUILayout.BeginVertical(m_boxStyle);
            EditorGUI.BeginChangeCheck();
            m_tod.Player = (Transform)m_editorUtils.ObjectField("PlayerCamera", m_tod.Player, typeof(Transform), true, helpEnabled);
            m_tod.VolumeOverrideTransform = (Transform)m_editorUtils.ObjectField("VolumeOverrideTransform", m_tod.VolumeOverrideTransform, typeof(Transform), true, helpEnabled);

            //Camera Settings
            EditorGUILayout.BeginVertical(m_boxStyle);
            EditorGUI.indentLevel++;
            m_tod.m_cameraSettings.m_showSettings = EditorGUILayout.Foldout(m_tod.m_cameraSettings.m_showSettings, "Camera Settings", true);
            if (m_tod.m_cameraSettings.m_showSettings)
            {
                CameraSettings(helpEnabled);
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();

            //Reflection Probe Settings
            EditorGUILayout.BeginVertical(m_boxStyle);
            EditorGUI.indentLevel++;
            m_tod.m_reflectionProbeSettings = EditorGUILayout.Foldout(m_tod.m_reflectionProbeSettings, "Reflection Probe Settings", true);
            if (m_tod.m_reflectionProbeSettings)
            {
                ReflectionProbeSyncSettings(helpEnabled);
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();

            //Light Sync
            EditorGUILayout.BeginVertical(m_boxStyle);
            EditorGUI.indentLevel++;
            m_tod.m_lightSyncSettings = EditorGUILayout.Foldout(m_tod.m_lightSyncSettings, "Light Sync Settings", true);
            if (m_tod.m_lightSyncSettings)
            {
                LightSyncSettings(helpEnabled);
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();

            bool useOverrideVolumes = m_tod.UseOverrideVolumes;

            //Override Volume Settings
            EditorGUILayout.BeginVertical(m_boxStyle);
            EditorGUI.indentLevel++;
            m_tod.m_overrideVolumeSettings = EditorGUILayout.Foldout(m_tod.m_overrideVolumeSettings, "Override Volume Settings", true);
            if (m_tod.m_overrideVolumeSettings)
            {
                useOverrideVolumes = m_editorUtils.Toggle("UseOverrideVolumes", useOverrideVolumes, helpEnabled);
                if (useOverrideVolumes)
                {
                    EditorGUI.indentLevel++;
                    m_tod.AutoOrganizeOverrideVolumes = m_editorUtils.Toggle("AutoOrganize", m_tod.AutoOrganizeOverrideVolumes, helpEnabled);
                    EditorGUI.indentLevel--;
                }
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                if (!useOverrideVolumes)
                {
                    HDRPTimeOfDayOverrideVolumeController controller = HDRPTimeOfDayOverrideVolumeController.Instance;
                    if (controller != null)
                    {
                        if (EditorUtility.DisplayDialog("Remove Volume Controller",
                                "Would you like to remove the override volume controller?", "Yes", "No"))
                        {
                            DestroyImmediate(controller);
                            m_tod.UseOverrideVolumes = useOverrideVolumes;
                            EditorUtility.SetDirty(m_tod);
                            EditorGUIUtility.ExitGUI();
                        }
                    }
                }

                m_tod.UseOverrideVolumes = useOverrideVolumes;
                EditorUtility.SetDirty(m_tod);
            }
            EditorGUILayout.EndVertical();
        }
        private void TimeOfDayPanel(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginVertical(m_boxStyle);
            m_tod.TimeOfDayProfile = (HDRPTimeOfDayProfile)m_editorUtils.ObjectField("TimeOfDayProfile", m_tod.TimeOfDayProfile, typeof(HDRPTimeOfDayProfile), false, helpEnabled);
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(m_tod);
                RepaintSceneView();
            }
            if (m_tod.TimeOfDayProfile != null)
            {
                EditorGUI.BeginChangeCheck();
                m_tod.TimeOfDay = m_editorUtils.Slider("TimeOfDay", m_tod.TimeOfDay, 0f, 24f, helpEnabled);
                m_tod.DateDay = m_editorUtils.IntSlider("Day", m_tod.DateDay, 1, 28);
                m_tod.DateMonth = m_editorUtils.IntSlider("Month", m_tod.DateMonth, 1, 12);
                m_tod.DateYear = m_editorUtils.IntField("Year", m_tod.DateYear);

                string direction = "N";
                if (m_tod.DirectionY < 45)
                {
                    direction = "N";
                }
                else if (m_tod.DirectionY >= 45 && m_tod.DirectionY <= 90)
                {
                    direction = "NE";
                }
                else if (m_tod.DirectionY >= 90 && m_tod.DirectionY <= 135)
                {
                    direction = "E";
                }
                else if (m_tod.DirectionY >= 135 && m_tod.DirectionY <= 180)
                {
                    direction = "SE";
                }
                else if (m_tod.DirectionY >= 180 && m_tod.DirectionY <= 225)
                {
                    direction = "S";
                }
                else if (m_tod.DirectionY >= 225 && m_tod.DirectionY <= 270)
                {
                    direction = "SW";
                }
                else if (m_tod.DirectionY >= 270 && m_tod.DirectionY <= 315)
                {
                    direction = "W";
                }
                else
                {
                    direction = "NW";
                }
                m_tod.DirectionY = EditorGUILayout.Slider(m_editorUtils.GetTextValue("Direction") + " (" + direction + ")", m_tod.DirectionY, 0f, 360f);
                m_editorUtils.InlineHelp("Direction", helpEnabled);
                m_tod.m_enableTimeOfDaySystem = m_editorUtils.Toggle("AutoUpdate", m_tod.m_enableTimeOfDaySystem, helpEnabled);
                if (m_tod.m_enableTimeOfDaySystem)
                {
                    EditorGUI.indentLevel++;
                    m_tod.m_timeOfDayMultiplier = m_editorUtils.FloatField("TimeOfDayMultiplier", m_tod.m_timeOfDayMultiplier, helpEnabled);
                    m_tod.IncrementalUpdate = m_editorUtils.Toggle("IncrementalUpdate", m_tod.IncrementalUpdate, helpEnabled);
                    if (m_tod.IncrementalUpdate)
                    {
                        EditorGUI.indentLevel++;
                        m_tod.IncrementalFrameCount = m_editorUtils.IntField("IncrementalFrameCount", m_tod.IncrementalFrameCount, helpEnabled);
                        EditorGUI.indentLevel--;
                    }
                    EditorGUI.indentLevel--;
                }
                if (EditorGUI.EndChangeCheck())
                {
                    EditorUtility.SetDirty(m_tod);
                    RepaintSceneView();
                }

                EditorGUI.BeginChangeCheck();

                //Advanced Lighting
                EditorGUILayout.BeginVertical(m_boxStyle);
                EditorGUI.indentLevel++;
                m_tod.TimeOfDayProfile.TimeOfDayData.m_advancedLightingSettings = EditorGUILayout.Foldout(m_tod.TimeOfDayProfile.TimeOfDayData.m_advancedLightingSettings, "Advanced Lighting Settings", true);
                if (m_tod.TimeOfDayProfile.TimeOfDayData.m_advancedLightingSettings)
                {
                    AdvancedLightingSettings(helpEnabled);
                }
                EditorGUI.indentLevel--;
                EditorGUILayout.EndVertical();

                //Cloud Settings
                EditorGUILayout.BeginVertical(m_boxStyle);
                EditorGUI.indentLevel++;
                m_tod.TimeOfDayProfile.TimeOfDayData.m_cloudSettings = EditorGUILayout.Foldout(m_tod.TimeOfDayProfile.TimeOfDayData.m_cloudSettings, "Cloud Settings", true);
                if (m_tod.TimeOfDayProfile.TimeOfDayData.m_cloudSettings)
                {
                    GlobalCloudType cloudType = m_tod.TimeOfDayProfile.TimeOfDayData.m_globalCloudType;
                    cloudType = (GlobalCloudType)m_editorUtils.EnumPopup("GlobalCloudType", cloudType, helpEnabled);
                    if (cloudType != m_tod.TimeOfDayProfile.TimeOfDayData.m_globalCloudType)
                    {
                        m_tod.TimeOfDayProfile.TimeOfDayData.m_globalCloudType = cloudType;
                        m_tod.SetupVisualEnvironment();
                    }
                    switch (m_tod.TimeOfDayProfile.TimeOfDayData.m_globalCloudType)
                    {
                        case GlobalCloudType.Volumetric:
                        {
                            VolumetricCloudSettings(helpEnabled);
                            break;
                        }
                        case GlobalCloudType.Procedural:
                        {

                            ProceduralCloudSettings(helpEnabled);
                            break;
                        }
                        case GlobalCloudType.Both:
                        {
                            VolumetricCloudSettings(helpEnabled);
                            ProceduralCloudSettings(helpEnabled);
                            break;
                        }
                    }
                }
                EditorGUI.indentLevel--;
                EditorGUILayout.EndVertical();

                //Duration Settings
                EditorGUILayout.BeginVertical(m_boxStyle);
                EditorGUI.indentLevel++;
                m_tod.TimeOfDayProfile.TimeOfDayData.m_durationSettings = EditorGUILayout.Foldout(m_tod.TimeOfDayProfile.TimeOfDayData.m_durationSettings, "Duration Settings", true);
                if (m_tod.TimeOfDayProfile.TimeOfDayData.m_durationSettings)
                {
                    m_tod.TimeOfDayProfile.TimeOfDayData.m_dayDuration = m_editorUtils.FloatField("DayDuration", m_tod.TimeOfDayProfile.TimeOfDayData.m_dayDuration, helpEnabled);
                    m_tod.TimeOfDayProfile.TimeOfDayData.m_nightDuration = m_editorUtils.FloatField("NightDuration", m_tod.TimeOfDayProfile.TimeOfDayData.m_nightDuration, helpEnabled);
                }
                EditorGUI.indentLevel--;
                EditorGUILayout.EndVertical();

                //Fog Settings
                EditorGUILayout.BeginVertical(m_boxStyle);
                EditorGUI.indentLevel++;
                m_tod.TimeOfDayProfile.TimeOfDayData.m_fogSettings = EditorGUILayout.Foldout(m_tod.TimeOfDayProfile.TimeOfDayData.m_fogSettings, "Fog Settings", true);
                if (m_tod.TimeOfDayProfile.TimeOfDayData.m_fogSettings)
                {
                    FogSettings(helpEnabled);
                }
                EditorGUI.indentLevel--;
                EditorGUILayout.EndVertical();

                //Sky Settings
                EditorGUILayout.BeginVertical(m_boxStyle);
                EditorGUI.indentLevel++;
                m_tod.TimeOfDayProfile.TimeOfDayData.m_skySettings = EditorGUILayout.Foldout(m_tod.TimeOfDayProfile.TimeOfDayData.m_skySettings, "Sky Settings", true);
                if (m_tod.TimeOfDayProfile.TimeOfDayData.m_skySettings)
                {
                    SkySettings(helpEnabled);
                }
                EditorGUI.indentLevel--;
                EditorGUILayout.EndVertical();

                //Sun Settings
                EditorGUILayout.BeginVertical(m_boxStyle);
                EditorGUI.indentLevel++;
                m_tod.TimeOfDayProfile.TimeOfDayData.m_sunSettings = EditorGUILayout.Foldout(m_tod.TimeOfDayProfile.TimeOfDayData.m_sunSettings, "Sun/Moon Settings", true);
                if (m_tod.TimeOfDayProfile.TimeOfDayData.m_sunSettings)
                {
                    SunSettings(helpEnabled);
                    LensFlareSettings(helpEnabled);
                }
                EditorGUI.indentLevel--;
                EditorGUILayout.EndVertical();

                //Ray Tracing
                EditorGUILayout.BeginVertical(m_boxStyle);
                EditorGUI.indentLevel++;
#if HDRPTIMEOFDAY
                if (m_tod.TimeOfDayProfile.RayTracingProfile != null)
                {
                    m_tod.TimeOfDayProfile.RayTracingProfile.RayTracingSettings.m_rayTraceSettings = EditorGUILayout.Foldout(m_tod.TimeOfDayProfile.RayTracingProfile.RayTracingSettings.m_rayTraceSettings, "Ray Tracing Settings", true);
                    if (m_tod.TimeOfDayProfile.RayTracingProfile.RayTracingSettings.m_rayTraceSettings)
                    {
                        RayTracingSettings(helpEnabled);
                    }
                }
#endif
                EditorGUI.indentLevel--;
                EditorGUILayout.EndVertical();

                if (EditorGUI.EndChangeCheck())
                {
                    EditorUtility.SetDirty(m_tod.TimeOfDayProfile);
                    EditorUtility.SetDirty(m_tod);
                    m_tod.SetCurrentIncrimentalFrameValue(int.MaxValue);
                    m_tod.ProcessTimeOfDay();
                    RepaintSceneView();
                }
            }
            EditorGUILayout.EndVertical();
        }
        private void PostProcessingPanel(bool helpEnabled)
        {
            //Post FX
            EditorGUILayout.BeginVertical(m_boxStyle);
            EditorGUI.BeginChangeCheck();
            m_tod.UsePostFX = EditorGUILayout.Toggle("Use Post Processing", m_tod.UsePostFX);
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(m_tod);
            }
            if (m_tod.UsePostFX)
            {
                EditorGUI.BeginChangeCheck();
                EditorGUI.indentLevel++;
                HDRPTimeOfDayPostFXProfile postFXProfile = m_tod.TimeOfDayPostFxProfile;
                postFXProfile = (HDRPTimeOfDayPostFXProfile)m_editorUtils.ObjectField("PostProcessingProfile", postFXProfile, typeof(HDRPTimeOfDayPostFXProfile), false, helpEnabled);
                if (postFXProfile != m_tod.TimeOfDayPostFxProfile)
                {
                    m_tod.TimeOfDayPostFxProfile = postFXProfile;
                    m_tod.SetHasBeenSetup(m_tod.SetupHDRPTimeOfDay());
                }
                if (EditorGUI.EndChangeCheck())
                {
                    EditorUtility.SetDirty(m_tod);
                }
                EditorGUI.indentLevel--;
                if (m_tod.TimeOfDayPostFxProfile != null)
                {
                    EditorGUI.BeginChangeCheck();
                    EditorGUI.indentLevel++;
                    m_tod.TimeOfDayPostFxProfile.TimeOfDayPostFXData.m_postProcessingQuality = (GeneralQuality)m_editorUtils.EnumPopup("PostProcessingQuality", m_tod.TimeOfDayPostFxProfile.TimeOfDayPostFXData.m_postProcessingQuality, helpEnabled);
                    EditorGUI.indentLevel--;
                    //Ambient Occlusion Settings
                    EditorGUILayout.BeginVertical(m_boxStyle);
                    EditorGUI.indentLevel++;
                    m_tod.TimeOfDayPostFxProfile.TimeOfDayPostFXData.m_ambientOcclusionSettings = EditorGUILayout.Foldout(m_tod.TimeOfDayPostFxProfile.TimeOfDayPostFXData.m_ambientOcclusionSettings, "Ambient Occlusion Settings", true);
                    if (m_tod.TimeOfDayPostFxProfile.TimeOfDayPostFXData.m_ambientOcclusionSettings)
                    {
                        AmbientOcclusionSettings(helpEnabled);
                    }
                    EditorGUI.indentLevel--;
                    EditorGUILayout.EndVertical();

                    //Bloom Settings
                    EditorGUILayout.BeginVertical(m_boxStyle);
                    EditorGUI.indentLevel++;
                    m_tod.TimeOfDayPostFxProfile.TimeOfDayPostFXData.m_bloomSettings = EditorGUILayout.Foldout(m_tod.TimeOfDayPostFxProfile.TimeOfDayPostFXData.m_bloomSettings, "Bloom Settings", true);
                    if (m_tod.TimeOfDayPostFxProfile.TimeOfDayPostFXData.m_bloomSettings)
                    {
                        BloomSettings(helpEnabled);
                    }
                    EditorGUI.indentLevel--;
                    EditorGUILayout.EndVertical();

                    //Color Grading Settings
                    EditorGUILayout.BeginVertical(m_boxStyle);
                    EditorGUI.indentLevel++;
                    m_tod.TimeOfDayPostFxProfile.TimeOfDayPostFXData.m_colorGradingSettings = EditorGUILayout.Foldout(m_tod.TimeOfDayPostFxProfile.TimeOfDayPostFXData.m_colorGradingSettings, "Color Grading Settings", true);
                    if (m_tod.TimeOfDayPostFxProfile.TimeOfDayPostFXData.m_colorGradingSettings)
                    {
                        ColorGradingSettings(helpEnabled);
                    }
                    EditorGUI.indentLevel--;
                    EditorGUILayout.EndVertical();

                    //Depth Of Field Settings
                    EditorGUILayout.BeginVertical(m_boxStyle);
                    EditorGUI.indentLevel++;
                    m_tod.TimeOfDayPostFxProfile.TimeOfDayPostFXData.m_depthOfFieldSettings = EditorGUILayout.Foldout(m_tod.TimeOfDayPostFxProfile.TimeOfDayPostFXData.m_depthOfFieldSettings, "Depth Of Field Settings", true);
                    if (m_tod.TimeOfDayPostFxProfile.TimeOfDayPostFXData.m_depthOfFieldSettings)
                    {
                        DepthOfFieldSettings(helpEnabled);
                    }
                    EditorGUI.indentLevel--;
                    EditorGUILayout.EndVertical();

                    //Shadow Toning Settings
                    EditorGUILayout.BeginVertical(m_boxStyle);
                    EditorGUI.indentLevel++;
                    m_tod.TimeOfDayPostFxProfile.TimeOfDayPostFXData.m_shadowToningSettings = EditorGUILayout.Foldout(m_tod.TimeOfDayPostFxProfile.TimeOfDayPostFXData.m_shadowToningSettings, "Shadow Toning Settings", true);
                    if (m_tod.TimeOfDayPostFxProfile.TimeOfDayPostFXData.m_shadowToningSettings)
                    {
                        ShadowToningSettings(helpEnabled);
                    }
                    EditorGUI.indentLevel--;
                    EditorGUILayout.EndVertical();

                    //Vignette Settings
                    EditorGUILayout.BeginVertical(m_boxStyle);
                    EditorGUI.indentLevel++;
                    m_tod.TimeOfDayPostFxProfile.TimeOfDayPostFXData.m_vignetteSettings = EditorGUILayout.Foldout(m_tod.TimeOfDayPostFxProfile.TimeOfDayPostFXData.m_vignetteSettings, "Vignette Settings", true);
                    if (m_tod.TimeOfDayPostFxProfile.TimeOfDayPostFXData.m_vignetteSettings)
                    {
                        VignetteSettings(helpEnabled);
                    }
                    EditorGUI.indentLevel--;
                    EditorGUILayout.EndVertical();

                    if (EditorGUI.EndChangeCheck())
                    {
                        EditorUtility.SetDirty(m_tod.TimeOfDayPostFxProfile);
                        m_tod.SetCurrentIncrimentalFrameValue(int.MaxValue);
                        m_tod.ProcessTimeOfDay();
                    }
                }
            }
            EditorGUILayout.EndVertical();
        }
        private void InteriorControllerPanel(bool helpEnabled)
        {
            EditorGUILayout.BeginVertical(m_boxStyle);
            InteriorControllerSettings(helpEnabled);
            EditorGUILayout.EndVertical();
        }
        private void SeasonPanel(bool helpEnabled)
        {
            EditorGUILayout.BeginVertical(m_boxStyle);
            EditorGUI.BeginChangeCheck();

            if (m_tod.SeasonProfile != null)
            {
                m_tod.m_enableSeasons = m_editorUtils.Toggle("EnableSeasons", m_tod.m_enableSeasons, helpEnabled);
                if (m_tod.m_enableSeasons)
                {
                    m_tod.SeasonProfile = (HDRPTimeOfDaySeasonProfile)m_editorUtils.ObjectField("SeasonProfile", m_tod.SeasonProfile, typeof(HDRPTimeOfDaySeasonProfile), false, helpEnabled);
                    EditorGUI.indentLevel++;
                    EditorGUILayout.LabelField("Current Season: " + m_tod.SeasonProfile.GetCurrentSeason());
                    m_tod.SeasonProfile.m_season = m_editorUtils.Slider("SeasonalValue", m_tod.SeasonProfile.m_season, 0f, 3.99f, helpEnabled);
                    m_tod.SeasonProfile.m_seasonTransitionDuration = m_editorUtils.FloatField("SeasonDuration", m_tod.SeasonProfile.m_seasonTransitionDuration, helpEnabled);
                    m_tod.SeasonProfile.m_seasonWinterTint = m_editorUtils.ColorField("WinterTint", m_tod.SeasonProfile.m_seasonWinterTint, helpEnabled);
                    m_tod.SeasonProfile.m_seasonSpringTint = m_editorUtils.ColorField("SpringTint", m_tod.SeasonProfile.m_seasonSpringTint, helpEnabled);
                    m_tod.SeasonProfile.m_seasonSummerTint = m_editorUtils.ColorField("SummerTint", m_tod.SeasonProfile.m_seasonSummerTint, helpEnabled);
                    m_tod.SeasonProfile.m_seasonAutumnTint = m_editorUtils.ColorField("AutumnTint", m_tod.SeasonProfile.m_seasonAutumnTint, helpEnabled);
                    EditorGUI.indentLevel--;
                }
            }
            EditorGUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(m_tod);
                if (m_tod.SeasonProfile != null)
                {
                    m_tod.SeasonProfile.Setup();
                    EditorUtility.SetDirty(m_tod.SeasonProfile);
                }
            }
        }
        private void AmbientAudioPanel(bool helpEnabled)
        {
            //Ambient
            EditorGUILayout.BeginVertical(m_boxStyle);
            m_tod.UseAmbientAudio = m_editorUtils.Toggle("UseAmbientAudio", m_tod.UseAmbientAudio, helpEnabled);
            if (m_tod.UseAmbientAudio)
            {
                EditorGUI.indentLevel++;
                AmbientAudio(helpEnabled);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndVertical();
        }
        private void UnderwaterPanel(bool helpEnabled)
        {
            //Underwater
            EditorGUILayout.BeginVertical(m_boxStyle);
            UnderwaterOverridesSettings(helpEnabled);
            EditorGUILayout.EndVertical();
        }
        private void WeatherPanel(bool helpEnabled)
        {
            //Weather
            EditorGUILayout.BeginVertical(m_boxStyle);
            WeatherSettings(helpEnabled);
            EditorGUILayout.EndVertical();
        }
        private void DebugPanel(bool helpEnabled)
        {
            EditorGUILayout.BeginVertical(m_boxStyle);
            DebugSettings(helpEnabled);
            EditorGUILayout.EndVertical();
        }

        #endregion
        #region Panels Functions

        private void InteriorControllerSettings(bool helpEnabled)
        {
            EditorGUILayout.LabelField("Interior Controller Settings", EditorStyles.boldLabel);
            m_tod.m_interiorControllerData.m_useInteriorControllers = m_editorUtils.Toggle("EnableInteriorControllers", m_tod.m_interiorControllerData.m_useInteriorControllers, helpEnabled);
            if (m_tod.m_interiorControllerData.m_useInteriorControllers)
            {
                EditorGUI.indentLevel++;
                m_tod.m_interiorControllerData.m_refreshControllerSystemsOnWeatherStart = m_editorUtils.Toggle("RefreshOnWeatherStart", m_tod.m_interiorControllerData.m_refreshControllerSystemsOnWeatherStart, helpEnabled);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Controllers Found: " + m_tod.m_interiorControllerData.m_controllers.Count);
                if (m_editorUtils.Button("RefreshControllers"))
                {
                    m_tod.m_interiorControllerData.GetAllControllers();
                }
                EditorGUILayout.EndHorizontal();
                EditorGUI.indentLevel--;

                m_tod.m_interiorControllerData.m_useAudioReverb = m_editorUtils.Toggle("UseAudioReverb", m_tod.m_interiorControllerData.m_useAudioReverb, helpEnabled);
                if (m_tod.m_interiorControllerData.m_useAudioReverb)
                {
                    EditorGUI.indentLevel++;
                    m_tod.m_interiorControllerData.m_exteriorReverbPreset = (AudioReverbPreset)m_editorUtils.EnumPopup("GlobalReverbPreset", m_tod.m_interiorControllerData.m_exteriorReverbPreset, helpEnabled);
                    m_tod.m_interiorControllerData.m_defaultParticleCollisionSettings = m_editorUtils.Vector4Field("CollisionSettings", m_tod.m_interiorControllerData.m_defaultParticleCollisionSettings, helpEnabled);
                    EditorGUI.indentLevel--;
                }
            }
        }
        /// <summary>
        /// Camera Settings
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void CameraSettings(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();
            m_tod.m_cameraSettings.m_nearClipPlane = m_editorUtils.FloatField("NearClipPlane", m_tod.m_cameraSettings.m_nearClipPlane, helpEnabled);
            m_tod.m_cameraSettings.m_farClipPlane = m_editorUtils.FloatField("FarClipPlane", m_tod.m_cameraSettings.m_farClipPlane, helpEnabled);
            m_tod.m_cameraSettings.m_dynamicResolution = m_editorUtils.Toggle("DynamicResolution", m_tod.m_cameraSettings.m_dynamicResolution, helpEnabled);
            if (m_tod.m_cameraSettings.m_dynamicResolution)
            {
                EditorGUI.indentLevel++;
                m_tod.m_cameraSettings.m_dlss = m_editorUtils.Toggle("UseDLSS", m_tod.m_cameraSettings.m_dlss, helpEnabled);
                EditorGUI.indentLevel--;
            }
            m_tod.m_cameraSettings.m_stopNan = m_editorUtils.Toggle("StopNaNs", m_tod.m_cameraSettings.m_stopNan, helpEnabled);
            m_tod.m_cameraSettings.m_dithering = m_editorUtils.Toggle("Dithering", m_tod.m_cameraSettings.m_dithering, helpEnabled);
            m_tod.m_cameraSettings.m_antialiasingMode = (HDAdditionalCameraData.AntialiasingMode)m_editorUtils.EnumPopup("AntiAliasingMode", m_tod.m_cameraSettings.m_antialiasingMode, helpEnabled);
            if (m_tod.m_cameraSettings.m_antialiasingMode == HDAdditionalCameraData.AntialiasingMode.TemporalAntialiasing || m_tod.m_cameraSettings.m_antialiasingMode == HDAdditionalCameraData.AntialiasingMode.SubpixelMorphologicalAntiAliasing)
            {
                EditorGUI.indentLevel++;
                m_tod.m_cameraSettings.m_antiAliasingQuality = (AntiAliasingQuality)m_editorUtils.EnumPopup("AntiAliasingQuality", m_tod.m_cameraSettings.m_antiAliasingQuality, helpEnabled);
                EditorGUI.indentLevel--;
            }
            m_tod.m_cameraSettings.m_overrideSensor = m_editorUtils.Toggle("OverrideSensor", m_tod.m_cameraSettings.m_overrideSensor, helpEnabled);
            if (m_tod.m_cameraSettings.m_overrideSensor)
            {
                EditorGUI.indentLevel++;
                m_tod.m_cameraSettings.m_sensorSize = m_editorUtils.Vector2Field("SensorSize", m_tod.m_cameraSettings.m_sensorSize, helpEnabled);
                EditorGUI.indentLevel--;
            }

#if !TOD_CINEMACHINE
            if (GUILayout.Button(new GUIContent("Enable Cinemachine", "Enables Cinemachine support in HDRP Time Of Day.")))
            {
                HDRPTimeOfDayScriptDefines.AddCinemachineDefine(true);
            }
#else
            if (GUILayout.Button(new GUIContent("Remove Cinemachine", "Removes Cinemachine support in HDRP Time Of Day.")))
            {
                HDRPTimeOfDayScriptDefines.AddCinemachineDefine(true);
            }
#endif

            if (EditorGUI.EndChangeCheck())
            {
                m_tod.m_cameraSettings.ApplyCameraSettings(m_tod.GetTODComponents().m_cameraData, m_tod.GetTODComponents().m_camera);
                EditorUtility.SetDirty(m_tod);
            }
        }
        /// <summary>
        /// Reflection Probe
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void ReflectionProbeSyncSettings(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();
            m_tod.EnableReflectionProbeSync = m_editorUtils.Toggle("UseReflectionProbeSystem", m_tod.EnableReflectionProbeSync, helpEnabled);
            if (m_tod.EnableReflectionProbeSync)
            {
                if (m_tod.TimeOfDayProfile != null)
                {
                    if (!m_tod.TimeOfDayProfile.TimeOfDayData.m_useSSGI && !m_tod.TimeOfDayProfile.TimeOfDayData.m_useNonSSGIAmbientCorrection)
                    {
                        EditorGUILayout.HelpBox("Reflection Probe System works best when SSGI (Screen Space Global Illumination) is enabled. To get the best results click Enable SSGI below.", MessageType.Info);
                        if (m_editorUtils.Button("EnableSSGI"))
                        {
                            m_tod.TimeOfDayProfile.TimeOfDayData.m_useSSGI = true;
                            m_tod.SetCurrentIncrimentalFrameValue(int.MaxValue);
                            m_tod.ProcessTimeOfDay();
                        }

                        EditorGUILayout.HelpBox("SSGI is not enabled and the NonSSGICorrection is disabled, lighting in your scene may be too bright. We recommend you enable NonSSGICorrection to help with the lighting in your scene", MessageType.Info);
                        if (GUILayout.Button("EnableNonSSGICorrection"))
                        {
                            m_tod.TimeOfDayProfile.TimeOfDayData.m_useNonSSGIAmbientCorrection = true;
                            m_tod.SetCurrentIncrimentalFrameValue(int.MaxValue);
                            m_tod.ProcessTimeOfDay();
                        }
                    }
                }
                EditorGUI.indentLevel++;
                m_tod.UseAdditionalReflectionProbes = m_editorUtils.Toggle("UseAdditionalProbes", m_tod.UseAdditionalReflectionProbes, helpEnabled);
                if (m_tod.UseAdditionalReflectionProbes)
                {
                    EditorGUILayout.HelpBox("Add a HDRPTimeOfDayAdditionalProbe component to any additional reflection probe in your scene. You can then assign reflection probe profiles to this and adjust reflection settings throught the time of day to give you more realistic looking lighting for lets save a cave or inside a dark bored up house.", MessageType.Info);
                }
                EditorGUILayout.BeginHorizontal();
                m_tod.ReflectionProbeProfile = (HDRPTimeOfDayReflectionProbeProfile)m_editorUtils.ObjectField("ReflectionProbeProfile", m_tod.ReflectionProbeProfile, typeof(HDRPTimeOfDayReflectionProbeProfile), false, helpEnabled);
                if (m_editorUtils.Button("Edit", GUILayout.MaxWidth(35f)))
                {
                    HDRPTimeOfDayComponents components = m_tod.GetTODComponents();
                    if (components != null)
                    {
                        if (components.m_reflectionProbeManager == null)
                        {
                            Debug.LogError("Reflection probe manager is null, please refresh the TOD components in debug section.");
                        }

                        Selection.activeObject = components.m_reflectionProbeManager;
                    }
                }
                EditorGUILayout.EndHorizontal();
                m_editorUtils.InlineHelp("ReflectionProbeProfile", helpEnabled);
                EditorGUI.indentLevel++;
                if (m_tod.ReflectionProbeProfile != null)
                {
                    m_tod.ProbeRenderMode = (ProbeRenderMode)m_editorUtils.EnumPopup("ProbeRenderMode", m_tod.ProbeRenderMode, helpEnabled);
                    m_tod.ProbeRenderDistance = m_editorUtils.FloatField("ProbeRenderDistance", m_tod.ProbeRenderDistance, helpEnabled);
                }
                EditorGUI.indentLevel--;
                EditorGUI.indentLevel--;
            }
            if (EditorGUI.EndChangeCheck())
            {
                m_tod.UpdateReflectionProbeSystem();
            }
        }
        /// <summary>
        /// Light Sync
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void LightSyncSettings(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();
            m_tod.UseLightSync = m_editorUtils.Toggle("UseLightSync", m_tod.UseLightSync, helpEnabled);
            if (m_tod.UseLightSync)
            {
                EditorGUI.indentLevel++;
                m_tod.m_lightLODBias = m_editorUtils.Slider("LightLODBias", m_tod.m_lightLODBias, 0.01f, 10f, helpEnabled);
                EditorGUI.indentLevel--;
                EditorGUILayout.HelpBox("Add a HDRPTimeOfDayLightComponent to any point/spot light you want in the scene to allow HDRP Time Of Day to control it and turn in on durning the night or off during the day. On the componenet you can set it to always be on from the Render Mode.", MessageType.Info);
            }
            if (EditorGUI.EndChangeCheck())
            {
                m_tod.ProcessTimeOfDay();
            }
        }
        /// <summary>
        /// Debugging
        /// </summary>
        private void DebugSettings(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();
            bool roundUp = m_tod.DebugSettings.m_roundUp;
            bool showDebug = m_tod.DebugSettings.m_showDebugLogs;
            float simulateSpeed = m_tod.DebugSettings.m_simulationSpeed;
            simulateSpeed = m_editorUtils.FloatField("SimulateSpeed", simulateSpeed, helpEnabled);
            roundUp = m_editorUtils.Toggle("RoundUp", roundUp, helpEnabled);
            showDebug = m_editorUtils.Toggle("ShowDebugLog", showDebug, helpEnabled);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_tod, "ChangedDebugSettings");
                m_tod.DebugSettings.m_roundUp = roundUp;
                m_tod.DebugSettings.m_showDebugLogs = showDebug;
                m_tod.DebugSettings.m_simulationSpeed = simulateSpeed;
                EditorUtility.SetDirty(m_tod);
            }

            EditorGUILayout.HelpBox("When installing a new version of HDRP Time Of Day we always recommend that you press 'Refresh Time Of Day Components'. This will get the apply the latest changes from the time of day components.", MessageType.Info);
            if (m_editorUtils.Button("RefreshTimeOfDayComponents"))
            {
                m_tod.RefreshTimeOfDayComponents();
                if (m_tod.HasBeenSetup())
                {
                    Debug.Log("Components refreshed successfully.");
                    EditorUtility.SetDirty(m_tod);
                }
            }
            if (m_editorUtils.Button("FetchDebugInformation"))
            {
                m_tod.GetDebugInformation();
            }

            if (Application.isPlaying)
            {
                GUI.enabled = false;
            }
            if (m_tod.DebugSettings.m_simulate)
            {
                if (m_editorUtils.Button("StopSimulation"))
                {
                    m_tod.StopSimulate();
                    EditorApplication.update -= SimulateUpdate;
                }
            }
            else
            {
                if (m_editorUtils.Button("StartSimulation"))
                {
                    m_tod.StartSimulate();
                    EditorApplication.update -= SimulateUpdate;
                    EditorApplication.update += SimulateUpdate;
                }
            }
            GUI.enabled = true;
            if (m_editorUtils.Button("CleanUpSceneMapID"))
            {
                m_tod.CleanupSceneMapID();
            }

            if (m_editorUtils.Button("SaveCurrentAsPreset"))
            {
                m_tod.SaveCurrentAsPreset();
                if (m_tod.PresetProfile != null)
                {
                    EditorUtility.SetDirty(m_tod.PresetProfile);
                }
            }

            if (HDRPTimeOfDayDebugUI.Instance == null)
            {
                if (m_editorUtils.Button("AddTimeOfDayDebugUI"))
                {
                    GameObject uiPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(HDRPTimeOfDay.GetAssetPath("Time Of Day Debug UI.prefab"));
                    if (uiPrefab != null)
                    {
                        PrefabUtility.InstantiatePrefab(uiPrefab);
                    }

                    EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                }
            }
            else
            {
                if (m_editorUtils.Button("RemoveTimeOfDayDebugUI"))
                {
                    GameObject.DestroyImmediate(HDRPTimeOfDayDebugUI.Instance.gameObject);
                    EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                }
            }
            if (m_editorUtils.Button("AddCameraLayerCulling"))
            {
                Camera camera = HDRPTimeOfDay.Instance.GetMainCamera();
                if (camera == null)
                {
                    camera = Camera.main;
                }
                if (camera != null)
                {
                    HDRPTimeOfDaySimpleCameraLayerCulling culling = camera.GetComponent<HDRPTimeOfDaySimpleCameraLayerCulling>();
                    if (culling != null)
                    {
                        Debug.LogWarning("Layer culling already exists on " + camera.name);
                    }
                    else
                    {
                        culling = camera.gameObject.AddComponent<HDRPTimeOfDaySimpleCameraLayerCulling>();
                        culling.m_profile = AssetDatabase.LoadAssetAtPath<HDRPTimeOfDaySceneCullingProfile>(HDRPTimeOfDay.GetAssetPath("HDRP Time Of Day Scene Culling Profile.asset"));
                        if (culling.m_profile == null)
                        {
                            Debug.LogWarning("Culling profile could not be assigned as it was not found (Default profile: HDRP Time Of Day Scene Culling Profile.asset. You will need to assign or create yur own profile.");
                            EditorGUIUtility.PingObject(camera.gameObject);
                        }
                        else
                        {
                            Debug.Log("Camera culling added successfully, pinging camera now that it was added to.");
                            EditorGUIUtility.PingObject(camera.gameObject);
                        }
                    }
                }
            }

            if (m_editorUtils.Button("RemoveTimeOfDaySystem"))
            {
                HDRPTimeOfDay.RemoveTimeOfDay();
                EditorGUIUtility.ExitGUI();
            }
        }
        /// <summary>
        /// Lighting
        /// </summary>
        private void AdvancedLightingSettings(bool helpEnabled)
        {
            EditorGUILayout.BeginVertical(m_boxStyle);
            EditorGUILayout.LabelField("Advanced Features Settings", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(m_boxStyle);
            m_tod.TimeOfDayProfile.TimeOfDayData.m_darkerNightSettings.m_darkerNights = m_editorUtils.Toggle("DarkerNights", m_tod.TimeOfDayProfile.TimeOfDayData.m_darkerNightSettings.m_darkerNights, helpEnabled);
            if (m_tod.TimeOfDayProfile.TimeOfDayData.m_darkerNightSettings.m_darkerNights)
            {
                EditorGUI.indentLevel++;
                m_tod.TimeOfDayProfile.TimeOfDayData.m_darkerNightSettings.m_automaticExposureAdjustmentValue = m_editorUtils.FloatField("AutoExposureValue", m_tod.TimeOfDayProfile.TimeOfDayData.m_darkerNightSettings.m_automaticExposureAdjustmentValue, helpEnabled);
                m_tod.TimeOfDayProfile.TimeOfDayData.m_darkerNightSettings.m_fixedExposureAdjustmentValue = m_editorUtils.FloatField("FixedExposureValue", m_tod.TimeOfDayProfile.TimeOfDayData.m_darkerNightSettings.m_fixedExposureAdjustmentValue, helpEnabled);
                m_tod.TimeOfDayProfile.TimeOfDayData.m_darkerNightSettings.m_physicalExposureAdjustmentValue = m_editorUtils.FloatField("PhysicalExposureValue", m_tod.TimeOfDayProfile.TimeOfDayData.m_darkerNightSettings.m_physicalExposureAdjustmentValue, helpEnabled);
                m_tod.TimeOfDayProfile.TimeOfDayData.m_darkerNightSettings.m_moonIntensityAdjustmentValue = m_editorUtils.FloatField("MoonIntensityValue", m_tod.TimeOfDayProfile.TimeOfDayData.m_darkerNightSettings.m_moonIntensityAdjustmentValue, helpEnabled);
                m_tod.TimeOfDayProfile.TimeOfDayData.m_darkerNightSettings.m_ambientIntensityAdjustmentValue = m_editorUtils.FloatField("AmbientIntensityValue", m_tod.TimeOfDayProfile.TimeOfDayData.m_darkerNightSettings.m_ambientIntensityAdjustmentValue, helpEnabled);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.LabelField("Screen Space Effect Settings", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(m_boxStyle);
            m_tod.TimeOfDayProfile.TimeOfDayData.m_useSSGI = m_editorUtils.Toggle("UseSSGI", m_tod.TimeOfDayProfile.TimeOfDayData.m_useSSGI, helpEnabled);
            if (m_tod.TimeOfDayProfile.TimeOfDayData.m_useSSGI)
            {
                EditorGUI.indentLevel++;
                m_tod.TimeOfDayProfile.TimeOfDayData.m_ssgiQuality = (GeneralQuality)m_editorUtils.EnumPopup("SSGIQuality", m_tod.TimeOfDayProfile.TimeOfDayData.m_ssgiQuality, helpEnabled);
                m_tod.TimeOfDayProfile.TimeOfDayData.m_ssgiRenderMode = (SSGIRenderMode)m_editorUtils.EnumPopup("SSGIRenderMode", m_tod.TimeOfDayProfile.TimeOfDayData.m_ssgiRenderMode, helpEnabled);
                m_tod.TimeOfDayProfile.TimeOfDayData.m_ambientSSGICompensation = m_editorUtils.Toggle("AmbientLightingCompensation", m_tod.TimeOfDayProfile.TimeOfDayData.m_ambientSSGICompensation, helpEnabled);
                if (m_tod.TimeOfDayProfile.TimeOfDayData.m_ambientSSGICompensation)
                {
                    EditorGUI.indentLevel++;
                    m_tod.TimeOfDayProfile.TimeOfDayData.m_compensationMode = (CompensationMode)m_editorUtils.EnumPopup("CompensationMode", m_tod.TimeOfDayProfile.TimeOfDayData.m_compensationMode, helpEnabled);
                    EditorGUI.indentLevel++;
                    m_tod.TimeOfDayProfile.TimeOfDayData.m_ambientCompensationExposure = m_editorUtils.Slider("AmbientCompensation", m_tod.TimeOfDayProfile.TimeOfDayData.m_ambientCompensationExposure, 0f, 5f, helpEnabled);
                    EditorGUILayout.HelpBox("Exposure compensation is applied when SSGI is enabled this is to compensate for the lacks of exposure in the scene when it's enabled.", MessageType.Info);
                    EditorGUI.indentLevel--;
                    m_tod.TimeOfDayProfile.TimeOfDayData.m_compensationModeAmbient = (CompensationMode)m_editorUtils.EnumPopup("CompensationModeAmbient", m_tod.TimeOfDayProfile.TimeOfDayData.m_compensationModeAmbient, helpEnabled);
                    EditorGUI.indentLevel++;
                    m_tod.TimeOfDayProfile.TimeOfDayData.m_ambientCompensationAmount = m_editorUtils.Slider("AmbientCompensation", m_tod.TimeOfDayProfile.TimeOfDayData.m_ambientCompensationAmount, 0f, 5f, helpEnabled);
                    EditorGUILayout.HelpBox("Ambient compensation is applied when SSGI is enabled this is to compensate for the lacks of ambient lighting in the scene when it's enabled.", MessageType.Info);
                    EditorGUI.indentLevel--;
                    EditorGUI.indentLevel--;
                }
                EditorGUI.indentLevel--;
            }

            EditorGUI.indentLevel++;
            m_tod.TimeOfDayProfile.TimeOfDayData.m_useNonSSGIAmbientCorrection = m_editorUtils.Toggle("NonSSGICorrection", m_tod.TimeOfDayProfile.TimeOfDayData.m_useNonSSGIAmbientCorrection, helpEnabled);
            m_tod.TimeOfDayProfile.TimeOfDayData.m_reflectionAmbientSSGICompensation = m_editorUtils.Toggle("NonSSGIReflectionCorrection", m_tod.TimeOfDayProfile.TimeOfDayData.m_reflectionAmbientSSGICompensation, helpEnabled);
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(m_boxStyle);
            m_tod.TimeOfDayProfile.TimeOfDayData.m_useSSR = m_editorUtils.Toggle("UseSSR", m_tod.TimeOfDayProfile.TimeOfDayData.m_useSSR, helpEnabled);
            if (m_tod.TimeOfDayProfile.TimeOfDayData.m_useSSR)
            {
                EditorGUI.indentLevel++;
                m_tod.TimeOfDayProfile.TimeOfDayData.m_ssrQuality = (GeneralQuality)m_editorUtils.EnumPopup("SSRQuality", m_tod.TimeOfDayProfile.TimeOfDayData.m_ssrQuality, helpEnabled);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(m_boxStyle);
            m_tod.TimeOfDayProfile.TimeOfDayData.m_useContactShadows = m_editorUtils.Toggle("UseContactShadows", m_tod.TimeOfDayProfile.TimeOfDayData.m_useContactShadows, helpEnabled);
            if (m_tod.TimeOfDayProfile.TimeOfDayData.m_useContactShadows)
            {
                EditorGUI.indentLevel++;
                m_tod.TimeOfDayProfile.TimeOfDayData.m_contactShadowsDistance = CurveField("ContactShadowsMaxDistance", m_tod.TimeOfDayProfile.TimeOfDayData.m_contactShadowsDistance, helpEnabled);
                m_tod.TimeOfDayProfile.TimeOfDayData.m_contactShadowsOpacity = CurveField("ContactShadowsOpacity", m_tod.TimeOfDayProfile.TimeOfDayData.m_contactShadowsOpacity, helpEnabled);
                m_tod.TimeOfDayProfile.TimeOfDayData.m_farContactShadowsSampleCount = m_editorUtils.IntSlider("CSFarSampleCount", m_tod.TimeOfDayProfile.TimeOfDayData.m_farContactShadowsSampleCount, 2, 64, helpEnabled);
                m_tod.TimeOfDayProfile.TimeOfDayData.m_useAdvancedNearFarContactShadows = m_editorUtils.Toggle("AdvancedNearContactShadows", m_tod.TimeOfDayProfile.TimeOfDayData.m_useAdvancedNearFarContactShadows, helpEnabled);
                if (m_tod.TimeOfDayProfile.TimeOfDayData.m_useAdvancedNearFarContactShadows)
                {
                    EditorGUI.indentLevel++;
                    m_tod.TimeOfDayProfile.TimeOfDayData.m_farShadowPercentageBlend = m_editorUtils.Slider("FarPercentageBlend", m_tod.TimeOfDayProfile.TimeOfDayData.m_farShadowPercentageBlend, 0.01f, 1f, helpEnabled);
                    m_tod.TimeOfDayProfile.TimeOfDayData.m_nearContactShadowsSampleCount = m_editorUtils.IntSlider("CSNearSampleCount", m_tod.TimeOfDayProfile.TimeOfDayData.m_nearContactShadowsSampleCount, 2, 64, helpEnabled);
                    m_tod.TimeOfDayProfile.TimeOfDayData.m_nearContactShadowsDistance = CurveField("NearContactShadowsDistance", m_tod.TimeOfDayProfile.TimeOfDayData.m_nearContactShadowsDistance, helpEnabled);
                    m_tod.TimeOfDayProfile.TimeOfDayData.m_setContactShadowsMinDistanceToShadowDistance = m_editorUtils.Toggle("SetNearToShadowDistance", m_tod.TimeOfDayProfile.TimeOfDayData.m_setContactShadowsMinDistanceToShadowDistance, helpEnabled);
                    EditorGUI.indentLevel--;
                }
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(m_boxStyle);
            m_tod.TimeOfDayProfile.TimeOfDayData.m_useMicroShadows = m_editorUtils.Toggle("UseMicroShadows", m_tod.TimeOfDayProfile.TimeOfDayData.m_useMicroShadows, helpEnabled);
            if (m_tod.TimeOfDayProfile.TimeOfDayData.m_useMicroShadows)
            {
                EditorGUI.indentLevel++;
                m_tod.TimeOfDayProfile.TimeOfDayData.m_microShadowOpacity = CurveField("MicroShadowOpacity", m_tod.TimeOfDayProfile.TimeOfDayData.m_microShadowOpacity, helpEnabled);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(m_boxStyle);
            EditorGUILayout.LabelField("Subsurface Scattering Settings", EditorStyles.boldLabel);
            m_tod.TimeOfDayProfile.TimeOfDayData.m_useSSS = m_editorUtils.Toggle("UseSSS", m_tod.TimeOfDayProfile.TimeOfDayData.m_useSSS, helpEnabled);
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(m_boxStyle);
            EditorGUILayout.LabelField("Exposure Settings", EditorStyles.boldLabel);
            m_tod.TimeOfDayProfile.TimeOfDayData.m_exposureMode = (TimeOfDayExposureMode)m_editorUtils.EnumPopup("ExposureMode", m_tod.TimeOfDayProfile.TimeOfDayData.m_exposureMode, helpEnabled);
            EditorGUI.indentLevel++;
            if (m_tod.TimeOfDayProfile.TimeOfDayData.m_exposureMode == TimeOfDayExposureMode.Fixed)
            {            
                m_tod.TimeOfDayProfile.TimeOfDayData.m_generalExposure = CurveField("GeneralExposure", m_tod.TimeOfDayProfile.TimeOfDayData.m_generalExposure, helpEnabled);
            }
            else if (m_tod.TimeOfDayProfile.TimeOfDayData.m_exposureMode == TimeOfDayExposureMode.Automatic)
            {
                //EditorGUILayout.HelpBox("Automatic Exposure mode is a work in progress mode and is not currently recommended for the final release of your product.", MessageType.Info);
                EditorGUILayout.LabelField("Automatic Exposure Settings", EditorStyles.boldLabel);
                m_tod.TimeOfDayProfile.TimeOfDayData.m_autoExposureSettings.m_mixMaxLimit = m_editorUtils.Vector2Field("MinMaxLimit", m_tod.TimeOfDayProfile.TimeOfDayData.m_autoExposureSettings.m_mixMaxLimit, helpEnabled);
                m_tod.TimeOfDayProfile.TimeOfDayData.m_autoExposureSettings.m_adaptionSpeed = m_editorUtils.Vector2Field("AdaptionSpeed", m_tod.TimeOfDayProfile.TimeOfDayData.m_autoExposureSettings.m_adaptionSpeed, helpEnabled);
                m_tod.TimeOfDayProfile.TimeOfDayData.m_autoExposureSettings.m_useSmartTransition = m_editorUtils.Toggle("UseSmartTransition", m_tod.TimeOfDayProfile.TimeOfDayData.m_autoExposureSettings.m_useSmartTransition, helpEnabled);
                if (m_tod.TimeOfDayProfile.TimeOfDayData.m_autoExposureSettings.m_useSmartTransition)
                {
                    EditorGUI.indentLevel++;
                    m_tod.TimeOfDayProfile.TimeOfDayData.m_autoExposureSettings.m_mixMaxLimitNight = m_editorUtils.Vector2Field("MinMaxLimitNight", m_tod.TimeOfDayProfile.TimeOfDayData.m_autoExposureSettings.m_mixMaxLimitNight, helpEnabled);
                    m_tod.TimeOfDayProfile.TimeOfDayData.m_autoExposureSettings.m_transitionDayDuration = m_editorUtils.FloatField("SmartDayTransitionDuration", m_tod.TimeOfDayProfile.TimeOfDayData.m_autoExposureSettings.m_transitionDayDuration, helpEnabled);
                    m_tod.TimeOfDayProfile.TimeOfDayData.m_autoExposureSettings.m_transitionNightDuration = m_editorUtils.FloatField("SmartNightTransitionDuration", m_tod.TimeOfDayProfile.TimeOfDayData.m_autoExposureSettings.m_transitionNightDuration, helpEnabled);
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.BeginHorizontal();
                m_editorUtils.LabelField("HistogramPercentage", GUILayout.MaxWidth(EditorGUIUtility.labelWidth - 15f));
                m_tod.TimeOfDayProfile.TimeOfDayData.m_autoExposureSettings.m_histogramPercentage.x = EditorGUILayout.FloatField(m_tod.TimeOfDayProfile.TimeOfDayData.m_autoExposureSettings.m_histogramPercentage.x, GUILayout.MaxWidth(100f));
                EditorGUILayout.MinMaxSlider(ref m_tod.TimeOfDayProfile.TimeOfDayData.m_autoExposureSettings.m_histogramPercentage.x, ref m_tod.TimeOfDayProfile.TimeOfDayData.m_autoExposureSettings.m_histogramPercentage.y, 0f, 100f);
                m_tod.TimeOfDayProfile.TimeOfDayData.m_autoExposureSettings.m_histogramPercentage.y = EditorGUILayout.FloatField(m_tod.TimeOfDayProfile.TimeOfDayData.m_autoExposureSettings.m_histogramPercentage.y, GUILayout.MaxWidth(100f));
                EditorGUILayout.EndHorizontal();
                m_editorUtils.InlineHelp("HistogramPercentage", helpEnabled);
            }
            else
            {
                EditorGUILayout.LabelField("Physical Exposure Settings", EditorStyles.boldLabel);
                m_tod.TimeOfDayProfile.TimeOfDayData.m_physicalExposureSettings.m_iso = m_editorUtils.IntField("ISO", m_tod.TimeOfDayProfile.TimeOfDayData.m_physicalExposureSettings.m_iso, helpEnabled);
                m_tod.TimeOfDayProfile.TimeOfDayData.m_physicalExposureSettings.m_shutterSpeed = m_editorUtils.FloatField("ShutterSpeed", m_tod.TimeOfDayProfile.TimeOfDayData.m_physicalExposureSettings.m_shutterSpeed, helpEnabled);
                m_tod.TimeOfDayProfile.TimeOfDayData.m_physicalExposureSettings.m_aperture = CurveField("Aperture", m_tod.TimeOfDayProfile.TimeOfDayData.m_physicalExposureSettings.m_aperture, helpEnabled);
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(m_boxStyle);
            EditorGUILayout.LabelField("Ambient Settings", EditorStyles.boldLabel);
            m_tod.TimeOfDayProfile.TimeOfDayData.m_ambientIntensityMultiplier = m_editorUtils.Slider("AmbientIntensityMultiplier", m_tod.TimeOfDayProfile.TimeOfDayData.m_ambientIntensityMultiplier, 0.01f, 10f, helpEnabled);
            m_tod.TimeOfDayProfile.TimeOfDayData.m_ambientIntensity = CurveField("AmbientIntensity", m_tod.TimeOfDayProfile.TimeOfDayData.m_ambientIntensity, helpEnabled);
            m_tod.TimeOfDayProfile.TimeOfDayData.m_ambientReflectionIntensity = CurveField("AmbientReflectionIntensity", m_tod.TimeOfDayProfile.TimeOfDayData.m_ambientReflectionIntensity, helpEnabled);
            m_tod.TimeOfDayProfile.TimeOfDayData.m_planarReflectionIntensity = CurveField("PlanarReflectionIntensity", m_tod.TimeOfDayProfile.TimeOfDayData.m_planarReflectionIntensity, helpEnabled);
            EditorGUILayout.EndVertical();


            EditorGUILayout.BeginVertical(m_boxStyle);
            EditorGUILayout.LabelField("Shadow Settings", EditorStyles.boldLabel);
            m_tod.TimeOfDayProfile.TimeOfDayData.m_enableSunShadows = m_editorUtils.Toggle("SunShadows", m_tod.TimeOfDayProfile.TimeOfDayData.m_enableSunShadows, helpEnabled);
            if (m_tod.TimeOfDayProfile.TimeOfDayData.m_enableSunShadows)
            {
                EditorGUI.indentLevel++;
                m_tod.TimeOfDayProfile.TimeOfDayData.m_sunShadowDimmer = CurveField("SunShadowDimmer", m_tod.TimeOfDayProfile.TimeOfDayData.m_sunShadowDimmer, helpEnabled);
                EditorGUI.indentLevel--;
            }
            m_tod.TimeOfDayProfile.TimeOfDayData.m_enableMoonShadows = m_editorUtils.Toggle("MoonShadows", m_tod.TimeOfDayProfile.TimeOfDayData.m_enableMoonShadows, helpEnabled);
            if (m_tod.TimeOfDayProfile.TimeOfDayData.m_enableMoonShadows)
            {
                EditorGUI.indentLevel++;
                m_tod.TimeOfDayProfile.TimeOfDayData.m_moonShadowDimmer = CurveField("MoonShadowDimmer", m_tod.TimeOfDayProfile.TimeOfDayData.m_moonShadowDimmer, helpEnabled);
                EditorGUI.indentLevel--;
            }
            m_tod.TimeOfDayProfile.TimeOfDayData.m_shadowCascadeCount = m_editorUtils.IntSlider("ShadowCascadeCount", m_tod.TimeOfDayProfile.TimeOfDayData.m_shadowCascadeCount, 1, 4, helpEnabled);
            m_tod.TimeOfDayProfile.TimeOfDayData.m_shadowDistanceMultiplier = m_editorUtils.Slider("ShadowDistanceMultiplier", m_tod.TimeOfDayProfile.TimeOfDayData.m_shadowDistanceMultiplier, 0.01f, 5f, helpEnabled);
            m_tod.TimeOfDayProfile.TimeOfDayData.m_shadowDistanceMultiplierNight = m_editorUtils.Slider("ShadowDistanceMultiplierNight", m_tod.TimeOfDayProfile.TimeOfDayData.m_shadowDistanceMultiplierNight, 0.01f, 5f, helpEnabled);
            m_tod.TimeOfDayProfile.TimeOfDayData.m_shadowDistance = CurveField("ShadowDistance", m_tod.TimeOfDayProfile.TimeOfDayData.m_shadowDistance, helpEnabled);
            m_tod.TimeOfDayProfile.TimeOfDayData.m_shadowTransmissionMultiplier = CurveField("TransmissionMultiplier", m_tod.TimeOfDayProfile.TimeOfDayData.m_shadowTransmissionMultiplier, helpEnabled);
            if (m_tod.TimeOfDayProfile.TimeOfDayData.m_shadowCascadeFadeLength.Count <= 0)
            {
                m_tod.TimeOfDayProfile.TimeOfDayData.m_shadowCascadeFadeLength = new List<float>
                {
                    0.11f,
                    0.25f,
                    0.7f
                };
                EditorUtility.SetDirty(m_tod.TimeOfDayProfile);
            }
            else
            {
                for (int i = 0; i < m_tod.TimeOfDayProfile.TimeOfDayData.m_shadowCascadeFadeLength.Count; i++)
                {
                    if (i <= 2)
                    {
                        float currentValue = m_tod.TimeOfDayProfile.TimeOfDayData.m_shadowCascadeFadeLength[i];
                        currentValue = EditorGUILayout.Slider("Shadow Cascade Fade " + i, currentValue, 0f, 1f);
                        m_tod.TimeOfDayProfile.TimeOfDayData.m_shadowCascadeFadeLength[i] = currentValue;
                    }
                }
            }
            EditorGUILayout.EndVertical();
        }
        private void SkySettings(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();
            TimeOfDaySkyMode skyMode = m_tod.TimeOfDayProfile.TimeOfDayData.m_skyMode;
            skyMode = (TimeOfDaySkyMode) m_editorUtils.EnumPopup("SkyMode", skyMode, helpEnabled);
            if (skyMode != m_tod.TimeOfDayProfile.TimeOfDayData.m_skyMode)
            {
                m_tod.TimeOfDayProfile.TimeOfDayData.m_skyMode = skyMode;
                m_tod.SetupVisualEnvironment();
            }
            m_tod.TimeOfDayProfile.TimeOfDayData.m_horizonOffset = m_editorUtils.Slider("HorizonOffset", m_tod.TimeOfDayProfile.TimeOfDayData.m_horizonOffset, -1f, 1f, helpEnabled);
            if (EditorGUI.EndChangeCheck())
            {
                m_tod.SetCurrentIncrimentalFrameValue(int.MaxValue);
                m_tod.ProcessTimeOfDay();
                EditorUtility.SetDirty(m_tod);
            }
            switch (m_tod.TimeOfDayProfile.TimeOfDayData.m_skyMode)
            {
                case TimeOfDaySkyMode.PhysicallyBased:
                {
                    EditorGUI.BeginChangeCheck();
                    m_tod.TimeOfDayProfile.TimeOfDayData.m_skyboxExposure = m_editorUtils.FloatField("SkyExposure", m_tod.TimeOfDayProfile.TimeOfDayData.m_skyboxExposure, helpEnabled);
                    m_tod.TimeOfDayProfile.TimeOfDayData.m_skyboxGroundColor = m_editorUtils.ColorField("SkyGroundColor", m_tod.TimeOfDayProfile.TimeOfDayData.m_skyboxGroundColor, helpEnabled);
                    m_tod.TimeOfDayProfile.TimeOfDayData.m_starIntensity = CurveField("StarIntensity", m_tod.TimeOfDayProfile.TimeOfDayData.m_starIntensity, helpEnabled);
                    m_tod.TimeOfDayProfile.TimeOfDayData.m_resetStarsRotationOnEnable = m_editorUtils.Toggle("ResetStarsRotation", m_tod.TimeOfDayProfile.TimeOfDayData.m_resetStarsRotationOnEnable, helpEnabled);
                    m_tod.TimeOfDayProfile.TimeOfDayData.m_rotateStars = m_editorUtils.Toggle("RotateStars", m_tod.TimeOfDayProfile.TimeOfDayData.m_rotateStars, helpEnabled);
                    if (m_tod.TimeOfDayProfile.TimeOfDayData.m_rotateStars)
                    {
                        EditorGUI.indentLevel++;
                        m_tod.TimeOfDayProfile.TimeOfDayData.m_starsRotationSpeed = m_editorUtils.Vector3Field("StarsRotationSpeed", m_tod.TimeOfDayProfile.TimeOfDayData.m_starsRotationSpeed, helpEnabled);
                        EditorGUI.indentLevel--;
                    }

                    if (EditorGUI.EndChangeCheck())
                    {
                        m_tod.SetSkySettings(m_tod.TimeOfDayProfile.TimeOfDayData.m_skyboxExposure, m_tod.TimeOfDayProfile.TimeOfDayData.m_skyboxGroundColor);
                    }
                    break;
                }
            }
        }
        private void SunSettings(bool helpEnabled)
        {
            m_tod.TimeOfDayProfile.TimeOfDayData.m_globalLightMultiplier = m_editorUtils.Slider("GlobalLightIntensityMultiplier", m_tod.TimeOfDayProfile.TimeOfDayData.m_globalLightMultiplier, 0.001f, 10f, helpEnabled);
            EditorGUILayout.BeginVertical(m_boxStyle);
            EditorGUILayout.LabelField("Sun Settings", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            m_tod.TimeOfDayProfile.TimeOfDayData.m_sunIntensity = CurveField("SunIntensity", m_tod.TimeOfDayProfile.TimeOfDayData.m_sunIntensity, helpEnabled);
            m_tod.TimeOfDayProfile.TimeOfDayData.m_sunIntensityMultiplier = CurveField("SunMoonIntensityMultiplier", m_tod.TimeOfDayProfile.TimeOfDayData.m_sunIntensityMultiplier, helpEnabled);
            m_tod.TimeOfDayProfile.TimeOfDayData.m_sunTemperature = CurveField("SunTemperature", m_tod.TimeOfDayProfile.TimeOfDayData.m_sunTemperature, helpEnabled);
            m_tod.TimeOfDayProfile.TimeOfDayData.m_sunColorFilter = GradientField("SunColorFilter", m_tod.TimeOfDayProfile.TimeOfDayData.m_sunColorFilter, false, helpEnabled);
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(m_boxStyle);
            EditorGUILayout.LabelField("Moon Settings", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            m_tod.TimeOfDayProfile.TimeOfDayData.m_moonIntensity = CurveField("MoonIntensity", m_tod.TimeOfDayProfile.TimeOfDayData.m_moonIntensity, helpEnabled);
            m_tod.TimeOfDayProfile.TimeOfDayData.m_moonTemperature = CurveField("MoonTemperature", m_tod.TimeOfDayProfile.TimeOfDayData.m_moonTemperature, helpEnabled);
            m_tod.TimeOfDayProfile.TimeOfDayData.m_moonColorFilter = GradientField("MoonColorFilter", m_tod.TimeOfDayProfile.TimeOfDayData.m_moonColorFilter, false, helpEnabled);
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(m_boxStyle);
            EditorGUILayout.LabelField("Volumetric Settings", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            m_tod.TimeOfDayProfile.TimeOfDayData.m_sunVolumetrics = CurveField("SunMoonVolumetric", m_tod.TimeOfDayProfile.TimeOfDayData.m_sunVolumetrics, helpEnabled);
            m_tod.TimeOfDayProfile.TimeOfDayData.m_sunVolumetricShadowDimmer = CurveField("SunMoonVolumetricDimmer", m_tod.TimeOfDayProfile.TimeOfDayData.m_sunVolumetricShadowDimmer, helpEnabled);
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(m_boxStyle);
            EditorGUILayout.LabelField("Twilight Settings", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            m_tod.TimeOfDayProfile.TimeOfDayData.m_useTwilight = m_editorUtils.Toggle("UseTwilight", m_tod.TimeOfDayProfile.TimeOfDayData.m_useTwilight, helpEnabled);
            if (m_tod.TimeOfDayProfile.TimeOfDayData.m_useTwilight)
            {
                EditorGUI.indentLevel++;
                m_tod.TimeOfDayProfile.TimeOfDayData.m_twilightIntensity = CurveField("TwilightIntensity", m_tod.TimeOfDayProfile.TimeOfDayData.m_twilightIntensity, helpEnabled);
                m_tod.TimeOfDayProfile.TimeOfDayData.m_twilightTemperature = CurveField("TwilightTemperature", m_tod.TimeOfDayProfile.TimeOfDayData.m_twilightTemperature, helpEnabled);
                EditorGUI.indentLevel--;
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
        }
        private void FogSettings(bool helpEnabled)
        {
            m_tod.TimeOfDayProfile.TimeOfDayData.m_useFog = m_editorUtils.Toggle("UseFog", m_tod.TimeOfDayProfile.TimeOfDayData.m_useFog, helpEnabled);
            if (m_tod.TimeOfDayProfile.TimeOfDayData.m_useFog)
            {
                m_tod.TimeOfDayProfile.TimeOfDayData.m_fogQuality = (GeneralQuality)m_editorUtils.EnumPopup("FogQuality", m_tod.TimeOfDayProfile.TimeOfDayData.m_fogQuality, helpEnabled);
                m_tod.TimeOfDayProfile.TimeOfDayData.m_enableVolumetricFog = m_editorUtils.Toggle("EnableVolumetricFog", m_tod.TimeOfDayProfile.TimeOfDayData.m_enableVolumetricFog, helpEnabled);
                m_tod.TimeOfDayProfile.TimeOfDayData.m_useDenoising = m_editorUtils.Toggle("UseDenoising", m_tod.TimeOfDayProfile.TimeOfDayData.m_useDenoising, helpEnabled);
                if (m_tod.TimeOfDayProfile.TimeOfDayData.m_useDenoising)
                {
                    EditorGUI.indentLevel++;
                    m_tod.TimeOfDayProfile.TimeOfDayData.m_denoisingQuality = (GeneralQuality)m_editorUtils.EnumPopup("DenoisingQuality", m_tod.TimeOfDayProfile.TimeOfDayData.m_denoisingQuality, helpEnabled);
                    EditorGUI.indentLevel--;
                }
                m_tod.TimeOfDayProfile.TimeOfDayData.m_fogColor = GradientField("FogColor", m_tod.TimeOfDayProfile.TimeOfDayData.m_fogColor, false, helpEnabled);
                m_tod.TimeOfDayProfile.TimeOfDayData.m_fogDistance = CurveField("FogDistance", m_tod.TimeOfDayProfile.TimeOfDayData.m_fogDistance, helpEnabled);
                m_tod.TimeOfDayProfile.TimeOfDayData.m_fogHeight = CurveField("FogHeight", m_tod.TimeOfDayProfile.TimeOfDayData.m_fogHeight, helpEnabled);
                m_tod.TimeOfDayProfile.TimeOfDayData.m_fogDensity = CurveField("LocalFogDensity", m_tod.TimeOfDayProfile.TimeOfDayData.m_fogDensity, helpEnabled);
                m_tod.TimeOfDayProfile.TimeOfDayData.m_localFogMultiplier = CurveField("LocalFogMultiplier", m_tod.TimeOfDayProfile.TimeOfDayData.m_localFogMultiplier, helpEnabled);
                m_tod.TimeOfDayProfile.TimeOfDayData.m_volumetricFogDistance = CurveField("VolumetricDistance", m_tod.TimeOfDayProfile.TimeOfDayData.m_volumetricFogDistance, helpEnabled);

                m_tod.TimeOfDayProfile.TimeOfDayData.m_useFogAnisotropyOverride = m_editorUtils.Toggle("OverrideAnisotropy", m_tod.TimeOfDayProfile.TimeOfDayData.m_useFogAnisotropyOverride, helpEnabled);
                if (m_tod.TimeOfDayProfile.TimeOfDayData.m_useFogAnisotropyOverride)
                {
                    EditorGUI.indentLevel++;
                    m_tod.TimeOfDayProfile.TimeOfDayData.m_volumetricFogAnisotropy = CurveField("VolumetricAnisotropy", m_tod.TimeOfDayProfile.TimeOfDayData.m_volumetricFogAnisotropy, helpEnabled);
                    EditorGUI.indentLevel--;
                }

                m_tod.TimeOfDayProfile.TimeOfDayData.m_volumetricFogSliceDistributionUniformity = CurveField("SliceDistributionUniformity", m_tod.TimeOfDayProfile.TimeOfDayData.m_volumetricFogSliceDistributionUniformity, helpEnabled);
                m_tod.TimeOfDayProfile.TimeOfDayData.m_localFogSize = m_editorUtils.FloatField("LocalFogSize", m_tod.TimeOfDayProfile.TimeOfDayData.m_localFogSize, helpEnabled);
                m_tod.TimeOfDayProfile.TimeOfDayData.m_globalFogMultiplier = m_editorUtils.Slider("GlobalFogMultiplier", m_tod.TimeOfDayProfile.TimeOfDayData.m_globalFogMultiplier, 0.001f, 15f, helpEnabled);
                m_tod.TimeOfDayProfile.TimeOfDayData.m_globalFogHeightMultiplier = m_editorUtils.Slider("GlobalFogHeightMultiplier", m_tod.TimeOfDayProfile.TimeOfDayData.m_globalFogHeightMultiplier, 0.001f, 2f, helpEnabled);
                m_tod.TimeOfDayProfile.TimeOfDayData.m_globalFogDistanceMultiplier = m_editorUtils.Slider("GlobalFogDistanceMultiplier", m_tod.TimeOfDayProfile.TimeOfDayData.m_globalFogDistanceMultiplier, 0.001f, 15f, helpEnabled);
            }
        }
        private void LensFlareSettings(bool helpEnabled)
        {
            //Sun
            EditorGUI.BeginChangeCheck();
            TimeOfDayLensFlareProfile sunLensFlare = m_tod.TimeOfDayProfile.TimeOfDayData.m_sunLensFlareProfile;
            TimeOfDayLensFlareProfile moonLensFlare = m_tod.TimeOfDayProfile.TimeOfDayData.m_moonLensFlareProfile;
            EditorGUILayout.LabelField("Sun Lens Flare Settings", EditorStyles.boldLabel);
            if (sunLensFlare.m_useLensFlare || moonLensFlare.m_useLensFlare)
            {
                SunFlareInfoHelp();
            }
            EditorGUI.indentLevel++;
            sunLensFlare.m_useLensFlare = m_editorUtils.Toggle("UseSunLensFlare", sunLensFlare.m_useLensFlare, helpEnabled);
            if (sunLensFlare.m_useLensFlare)
            {
                sunLensFlare.m_lensFlareData = (LensFlareDataSRP)m_editorUtils.ObjectField("LensFlareData", sunLensFlare.m_lensFlareData, typeof(LensFlareDataSRP), false, helpEnabled);
                sunLensFlare.m_intensity = CurveField("SunFlareIntensity", sunLensFlare.m_intensity, helpEnabled);
                sunLensFlare.m_scale = CurveField("SunFlareScale", sunLensFlare.m_scale, helpEnabled);
                sunLensFlare.m_enableOcclusion = m_editorUtils.Toggle("SunFlareEnableOcclusion", sunLensFlare.m_enableOcclusion, helpEnabled);
                if (sunLensFlare.m_enableOcclusion)
                {
                    EditorGUI.indentLevel++;
                    sunLensFlare.m_occlusionRadius = m_editorUtils.FloatField("SunFlareRadius", sunLensFlare.m_occlusionRadius, helpEnabled);
                    sunLensFlare.m_sampleCount = m_editorUtils.IntSlider("SunFlareSampleCount", sunLensFlare.m_sampleCount, 1, 64, helpEnabled);
                    sunLensFlare.m_occlusionOffset = m_editorUtils.FloatField("SunFlareOffset", sunLensFlare.m_occlusionOffset, helpEnabled);
                    sunLensFlare.m_allowOffScreen = m_editorUtils.Toggle("SunFlareAllowOffScreen", sunLensFlare.m_allowOffScreen, helpEnabled);
                    sunLensFlare.m_volumetricCloudOcclusion = m_editorUtils.Toggle("VolumetricCloudOcclusion", sunLensFlare.m_volumetricCloudOcclusion, helpEnabled);
                    EditorGUI.indentLevel--;
                }
            }
            EditorGUI.indentLevel--;
            if (EditorGUI.EndChangeCheck())
            {
                m_tod.TimeOfDayProfile.TimeOfDayData.m_sunLensFlareProfile = sunLensFlare;
            }

            //Moon
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.LabelField("Moon Lens Flare Settings", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            moonLensFlare.m_useLensFlare = m_editorUtils.Toggle("UseMoonLensFlare", moonLensFlare.m_useLensFlare, helpEnabled);
            if (moonLensFlare.m_useLensFlare)
            {
                moonLensFlare.m_lensFlareData = (LensFlareDataSRP)m_editorUtils.ObjectField("LensFlareData", moonLensFlare.m_lensFlareData, typeof(LensFlareDataSRP), false);
                moonLensFlare.m_intensity = CurveField("SunFlareIntensity", moonLensFlare.m_intensity, helpEnabled);
                moonLensFlare.m_scale = CurveField("SunFlareScale", moonLensFlare.m_scale, helpEnabled);
                moonLensFlare.m_enableOcclusion = m_editorUtils.Toggle("SunFlareEnableOcclusion", moonLensFlare.m_enableOcclusion, helpEnabled);
                if (moonLensFlare.m_enableOcclusion)
                {
                    EditorGUI.indentLevel++;
                    moonLensFlare.m_occlusionRadius = m_editorUtils.FloatField("SunFlareRadius", moonLensFlare.m_occlusionRadius, helpEnabled);
                    moonLensFlare.m_sampleCount = m_editorUtils.IntSlider("SunFlareSampleCount", moonLensFlare.m_sampleCount, 1, 64, helpEnabled);
                    moonLensFlare.m_occlusionOffset = m_editorUtils.FloatField("SunFlareOffset", moonLensFlare.m_occlusionOffset, helpEnabled);
                    moonLensFlare.m_allowOffScreen = m_editorUtils.Toggle("SunFlareAllowOffScreen", moonLensFlare.m_allowOffScreen, helpEnabled);
                    moonLensFlare.m_volumetricCloudOcclusion = m_editorUtils.Toggle("VolumetricCloudOcclusion", moonLensFlare.m_volumetricCloudOcclusion, helpEnabled);
                    EditorGUI.indentLevel--;
                }
            }
            EditorGUI.indentLevel--;
            if (EditorGUI.EndChangeCheck())
            {
                m_tod.TimeOfDayProfile.TimeOfDayData.m_moonLensFlareProfile = moonLensFlare;
            }
        }
        private void VolumetricCloudSettings(bool helpEnabled)
        {
            EditorGUILayout.BeginVertical(m_boxStyle);
            EditorGUILayout.LabelField("Volumetric Cloud Settings", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            m_tod.TimeOfDayProfile.TimeOfDayData.m_useLocalClouds = m_editorUtils.Toggle("UseLocalClouds", m_tod.TimeOfDayProfile.TimeOfDayData.m_useLocalClouds, helpEnabled);
            if (m_tod.TimeOfDayProfile.TimeOfDayData.m_useLocalClouds)
            {
                EditorGUILayout.HelpBox("Local Clouds is enabled, you need to have a high far clip plane value to use this feature. Recommend a min value of 15000 for the far clip plane on the camera.", MessageType.Info);
            }
            m_tod.TimeOfDayProfile.TimeOfDayData.m_cloudPresets = (TODCloudPresets)m_editorUtils.EnumPopup("CloudPreset", m_tod.TimeOfDayProfile.TimeOfDayData.m_cloudPresets, helpEnabled);
            switch (m_tod.TimeOfDayProfile.TimeOfDayData.m_cloudPresets)
            {
                case TODCloudPresets.Custom:
                {
                    //Wind
                    m_tod.TimeOfDayProfile.TimeOfDayData.m_cloudWindDirection = CurveField("CloudWindDirection", m_tod.TimeOfDayProfile.TimeOfDayData.m_cloudWindDirection, helpEnabled);
                    m_tod.TimeOfDayProfile.TimeOfDayData.m_cloudWindSpeed = CurveField("CloudWindSpeed", m_tod.TimeOfDayProfile.TimeOfDayData.m_cloudWindSpeed, helpEnabled);

                    //Shape
                    m_tod.TimeOfDayProfile.TimeOfDayData.m_erosionNoiseType = (VolumetricClouds.CloudErosionNoise)m_editorUtils.EnumPopup("ErosionNoiseType", m_tod.TimeOfDayProfile.TimeOfDayData.m_erosionNoiseType, helpEnabled);
                    m_tod.TimeOfDayProfile.TimeOfDayData.m_volumetricDensityMultiplier = CurveField("DensityMultiplier", m_tod.TimeOfDayProfile.TimeOfDayData.m_volumetricDensityMultiplier, helpEnabled);
                    m_tod.TimeOfDayProfile.TimeOfDayData.m_volumetricDensityCurve = CurveField("CustomDensityCurve", m_tod.TimeOfDayProfile.TimeOfDayData.m_volumetricDensityCurve, helpEnabled);
                    m_tod.TimeOfDayProfile.TimeOfDayData.m_volumetricShapeFactor = CurveField("ShapeFactor", m_tod.TimeOfDayProfile.TimeOfDayData.m_volumetricShapeFactor, helpEnabled);
                    m_tod.TimeOfDayProfile.TimeOfDayData.m_volumetricShapeScale = CurveField("ShapeScale", m_tod.TimeOfDayProfile.TimeOfDayData.m_volumetricShapeScale, helpEnabled);
                    m_tod.TimeOfDayProfile.TimeOfDayData.m_volumetricErosionFactor = CurveField("ErosionFactor", m_tod.TimeOfDayProfile.TimeOfDayData.m_volumetricErosionFactor, helpEnabled);
                    m_tod.TimeOfDayProfile.TimeOfDayData.m_volumetricErosionScale = CurveField("ErosionScale", m_tod.TimeOfDayProfile.TimeOfDayData.m_volumetricErosionScale, helpEnabled);
                    m_tod.TimeOfDayProfile.TimeOfDayData.m_volumetricErosionCurve = CurveField("CustomErosionCurve", m_tod.TimeOfDayProfile.TimeOfDayData.m_volumetricErosionCurve, helpEnabled);
                    m_tod.TimeOfDayProfile.TimeOfDayData.m_volumetricAmbientOcclusionCurve = CurveField("CustomAmbientOcclusionCurve", m_tod.TimeOfDayProfile.TimeOfDayData.m_volumetricAmbientOcclusionCurve, helpEnabled);
                    m_tod.TimeOfDayProfile.TimeOfDayData.m_volumetricLowestCloudAltitude = CurveField("LowestCloudAltitude", m_tod.TimeOfDayProfile.TimeOfDayData.m_volumetricLowestCloudAltitude, helpEnabled);
                    m_tod.TimeOfDayProfile.TimeOfDayData.m_volumetricCloudThickness = CurveField("CloudThickness", m_tod.TimeOfDayProfile.TimeOfDayData.m_volumetricCloudThickness, helpEnabled);

                    //Lighting
                    m_tod.TimeOfDayProfile.TimeOfDayData.m_volumetricAmbientLightProbeDimmer = CurveField("AmbientLightProbeDimmer", m_tod.TimeOfDayProfile.TimeOfDayData.m_volumetricAmbientLightProbeDimmer, helpEnabled);
                    m_tod.TimeOfDayProfile.TimeOfDayData.m_volumetricSunLightDimmer = CurveField("SunLightDimmer", m_tod.TimeOfDayProfile.TimeOfDayData.m_volumetricSunLightDimmer, helpEnabled);
                    m_tod.TimeOfDayProfile.TimeOfDayData.m_volumetricErosionOcclusion = CurveField("ErosionOcclusion", m_tod.TimeOfDayProfile.TimeOfDayData.m_volumetricErosionOcclusion, helpEnabled);
                    m_tod.TimeOfDayProfile.TimeOfDayData.m_volumetricScatteringTint = GradientField("ScatteringTint", m_tod.TimeOfDayProfile.TimeOfDayData.m_volumetricScatteringTint, false, helpEnabled);
                    m_tod.TimeOfDayProfile.TimeOfDayData.m_volumetricPowderEffectIntensity = CurveField("PowderEffectIntensity", m_tod.TimeOfDayProfile.TimeOfDayData.m_volumetricPowderEffectIntensity, helpEnabled);
                    m_tod.TimeOfDayProfile.TimeOfDayData.m_volumetricMultiScattering = CurveField("MultiScattering", m_tod.TimeOfDayProfile.TimeOfDayData.m_volumetricMultiScattering, helpEnabled);

                    //Shadows
                    m_tod.TimeOfDayProfile.TimeOfDayData.m_volumetricCloudShadows = m_editorUtils.Toggle("EnableShadows", m_tod.TimeOfDayProfile.TimeOfDayData.m_volumetricCloudShadows, helpEnabled);
                    if (m_tod.TimeOfDayProfile.TimeOfDayData.m_volumetricCloudShadows)
                    {
                        EditorGUI.indentLevel++;
                        m_tod.TimeOfDayProfile.TimeOfDayData.m_volumetricCloudShadowResolution = (VolumetricClouds.CloudShadowResolution)m_editorUtils.EnumPopup("ShadowResolution", m_tod.TimeOfDayProfile.TimeOfDayData.m_volumetricCloudShadowResolution, helpEnabled); 
                        m_tod.TimeOfDayProfile.TimeOfDayData.m_useHeightBasedVolumetricCloudShadows = m_editorUtils.Toggle("HeightBasedShadowOpacity", m_tod.TimeOfDayProfile.TimeOfDayData.m_useHeightBasedVolumetricCloudShadows, helpEnabled);
                        if (m_tod.TimeOfDayProfile.TimeOfDayData.m_useHeightBasedVolumetricCloudShadows)
                        {
                            m_tod.TimeOfDayProfile.TimeOfDayData.m_volumetricCloudMinShadowClampValue = m_editorUtils.Slider("MinShadowClampValue", m_tod.TimeOfDayProfile.TimeOfDayData.m_volumetricCloudMinShadowClampValue, 0f, 1f, helpEnabled);
                            m_tod.TimeOfDayProfile.TimeOfDayData.m_volumetricCloudShadowsGroundHeight = m_editorUtils.FloatField("HeightRangeFromTheGround", m_tod.TimeOfDayProfile.TimeOfDayData.m_volumetricCloudShadowsGroundHeight, helpEnabled);
                        }
                        else
                        {
                            m_tod.TimeOfDayProfile.TimeOfDayData.m_volumetricCloudShadowOpacity = CurveField("ShadowOpacity", m_tod.TimeOfDayProfile.TimeOfDayData.m_volumetricCloudShadowOpacity, helpEnabled);
                        }
                        EditorGUI.indentLevel--;
                    }

                    break;
                }
                case TODCloudPresets.Presets:
                {
#if HDRPTIMEOFDAY
                    HDRPTimeOfDayVolumetricCloudPreset preset = m_tod.TimeOfDayProfile.TimeOfDayData.VolumetricCloudPreset;
                    preset = (HDRPTimeOfDayVolumetricCloudPreset)m_editorUtils.ObjectField("VolumetricCloudPreset", preset, typeof(HDRPTimeOfDayVolumetricCloudPreset), false, helpEnabled);
                    if (preset != m_tod.TimeOfDayProfile.TimeOfDayData.VolumetricCloudPreset)
                    {
                        m_tod.TimeOfDayProfile.TimeOfDayData.VolumetricCloudPreset = preset;
                    }
#endif
                    break;
                }
            }

            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
        }
        private void ProceduralCloudSettings(bool helpEnabled)
        {
            EditorGUILayout.BeginVertical(m_boxStyle);
            EditorGUILayout.LabelField("Procedual Cloud Settings", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            m_tod.TimeOfDayProfile.TimeOfDayData.m_proceduralCloudPresets = (TODCloudPresets)m_editorUtils.EnumPopup("CloudPreset", m_tod.TimeOfDayProfile.TimeOfDayData.m_proceduralCloudPresets, helpEnabled);
            switch (m_tod.TimeOfDayProfile.TimeOfDayData.m_proceduralCloudPresets)
            {
                case TODCloudPresets.Custom:
                {
                    m_tod.TimeOfDayProfile.TimeOfDayData.m_cloudLayers = (CloudLayerType) m_editorUtils.EnumPopup("CloudLayersType", m_tod.TimeOfDayProfile.TimeOfDayData.m_cloudLayers, helpEnabled);
                    m_tod.TimeOfDayProfile.TimeOfDayData.m_cloudResolution = (CloudResolution) m_editorUtils.EnumPopup("CloudResolution", m_tod.TimeOfDayProfile.TimeOfDayData.m_cloudResolution, helpEnabled);
                    m_tod.TimeOfDayProfile.TimeOfDayData.m_cloudOpacity = CurveField("CloudOpacity", m_tod.TimeOfDayProfile.TimeOfDayData.m_cloudOpacity, helpEnabled);
                    m_tod.TimeOfDayProfile.TimeOfDayData.m_cloudLayerAChannel = (CloudLayerChannelMode)m_editorUtils.EnumPopup("CloudLayerAChannelMode", m_tod.TimeOfDayProfile.TimeOfDayData.m_cloudLayerAChannel, helpEnabled);
                    EditorGUI.indentLevel++;
                    m_tod.TimeOfDayProfile.TimeOfDayData.m_cloudLayerAOpacityR = CurveField("CloudLayerAOpacity", m_tod.TimeOfDayProfile.TimeOfDayData.m_cloudLayerAOpacityR, helpEnabled);
                    EditorGUI.indentLevel--;
                    if (m_tod.TimeOfDayProfile.TimeOfDayData.m_cloudLayers == CloudLayerType.Double)
                    {
                        m_tod.TimeOfDayProfile.TimeOfDayData.m_cloudLayerBChannel = (CloudLayerChannelMode)m_editorUtils.EnumPopup("CloudLayerBChannelMode", m_tod.TimeOfDayProfile.TimeOfDayData.m_cloudLayerBChannel, helpEnabled);
                        EditorGUI.indentLevel++;
                        m_tod.TimeOfDayProfile.TimeOfDayData.m_cloudLayerBOpacityR = CurveField("CloudLayerBOpacity", m_tod.TimeOfDayProfile.TimeOfDayData.m_cloudLayerBOpacityR, helpEnabled);
                        EditorGUI.indentLevel--;
                    }

                    m_tod.TimeOfDayProfile.TimeOfDayData.m_cloudTintColor = GradientField("TintColor", m_tod.TimeOfDayProfile.TimeOfDayData.m_cloudTintColor, false, helpEnabled);
                    m_tod.TimeOfDayProfile.TimeOfDayData.m_cloudExposure = CurveField("CloudExposure", m_tod.TimeOfDayProfile.TimeOfDayData.m_cloudExposure, helpEnabled);
                    if (m_tod.TimeOfDayProfile.TimeOfDayData.m_globalCloudType == GlobalCloudType.Procedural)
                    {
                        m_tod.TimeOfDayProfile.TimeOfDayData.m_cloudWindDirection = CurveField("CloudWindDirection", m_tod.TimeOfDayProfile.TimeOfDayData.m_cloudWindDirection, helpEnabled);
                        m_tod.TimeOfDayProfile.TimeOfDayData.m_cloudWindSpeed = CurveField("CloudWindSpeed", m_tod.TimeOfDayProfile.TimeOfDayData.m_cloudWindSpeed, helpEnabled);
                    }

                    m_tod.TimeOfDayProfile.TimeOfDayData.m_cloudLighting = m_editorUtils.Toggle("UseCloudLighting", m_tod.TimeOfDayProfile.TimeOfDayData.m_cloudLighting, helpEnabled);
                    m_tod.TimeOfDayProfile.TimeOfDayData.m_useCloudShadows = m_editorUtils.Toggle("UseCloudShadows", m_tod.TimeOfDayProfile.TimeOfDayData.m_useCloudShadows, helpEnabled);
                    if (m_tod.TimeOfDayProfile.TimeOfDayData.m_useCloudShadows)
                    {
                        m_tod.TimeOfDayProfile.TimeOfDayData.m_cloudShadowOpacity = CurveField("CloudShadowOpacity", m_tod.TimeOfDayProfile.TimeOfDayData.m_cloudShadowOpacity, helpEnabled);
                        m_tod.TimeOfDayProfile.TimeOfDayData.m_cloudShadowColor = GradientField("CloudShadowColor", m_tod.TimeOfDayProfile.TimeOfDayData.m_cloudShadowColor, false, helpEnabled);
                    }
                    break;
                }
                case TODCloudPresets.Presets:
                {
#if HDRPTIMEOFDAY
                    HDRPTimeOfDayProceduralCloudPreset preset = m_tod.TimeOfDayProfile.TimeOfDayData.ProceduralCloudPreset;
                    preset = (HDRPTimeOfDayProceduralCloudPreset)m_editorUtils.ObjectField("ProceduralCloudPreset", preset, typeof(HDRPTimeOfDayProceduralCloudPreset), false, helpEnabled);
                    if (preset != m_tod.TimeOfDayProfile.TimeOfDayData.ProceduralCloudPreset)
                    {
                        m_tod.TimeOfDayProfile.TimeOfDayData.ProceduralCloudPreset = preset;
                    }
#endif
                    break;
                }
            }

            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
        }
        private void WeatherSettings(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();
            bool useWeather = m_tod.UseWeatherFX;
            useWeather = m_editorUtils.Toggle("UseWeatherFX", useWeather, helpEnabled);
            if (useWeather != m_tod.UseWeatherFX)
            {
                m_tod.UseWeatherFX = useWeather;
                if (useWeather)
                {
                    EditorGUIUtility.ExitGUI();
                }
            }
            if (useWeather)
            {
                EditorGUI.indentLevel++;
                m_tod.m_enableAutoWeather = m_editorUtils.Toggle("EnableAutoWeather", m_tod.m_enableAutoWeather, helpEnabled);
                m_tod.m_avoidSameRandomWeather = m_editorUtils.Toggle("AvoidSameWeatherProfile", m_tod.m_avoidSameRandomWeather, helpEnabled);
                m_tod.m_resetWeatherShaderProperty = m_editorUtils.Toggle("ResetShaderProperties", m_tod.m_resetWeatherShaderProperty, helpEnabled);
                m_tod.m_instantWeatherStop = m_editorUtils.Toggle("InstantWeatherStop", m_tod.m_instantWeatherStop, helpEnabled);
                m_tod.m_randomWeatherTimer = m_editorUtils.Vector2Field("WeatherMinMaxWaitTime", m_tod.m_randomWeatherTimer, helpEnabled);
                InteriorControllerPanel(helpEnabled);
                if (m_tod.WeatherProfiles.Count > 0)
                {
                    for (int i = 0; i < m_tod.WeatherProfiles.Count; i++)
                    {
                        EditorGUILayout.BeginVertical(m_boxStyle);
                        EditorGUILayout.BeginHorizontal();
                        if (m_tod.WeatherProfiles[i] == null)
                        {
                            m_tod.WeatherProfiles[i] = (HDRPTimeOfDayWeatherProfile)EditorGUILayout.ObjectField("NoProfileSpecified", m_tod.WeatherProfiles[i], typeof(HDRPTimeOfDayWeatherProfile), false);
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(m_tod.WeatherProfiles[i].WeatherData.m_weatherName))
                            {
                                m_tod.WeatherProfiles[i] = (HDRPTimeOfDayWeatherProfile)EditorGUILayout.ObjectField("No Name Specified", m_tod.WeatherProfiles[i], typeof(HDRPTimeOfDayWeatherProfile), false);
                            }
                            else
                            {
                                m_tod.WeatherProfiles[i] = (HDRPTimeOfDayWeatherProfile)EditorGUILayout.ObjectField("Profile: " + m_tod.WeatherProfiles[i].WeatherData.m_weatherName, m_tod.WeatherProfiles[i], typeof(HDRPTimeOfDayWeatherProfile), false);
                            }
                        }
                        if (m_editorUtils.Button("Remove", GUILayout.MaxWidth(80f)))
                        {
                            m_tod.WeatherProfiles.RemoveAt(i);
                            EditorUtility.SetDirty(m_tod);
                            EditorGUIUtility.ExitGUI();
                        }
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal();
                        if (!Application.isPlaying)
                        {
                            GUI.enabled = false;
                        }
                        else
                        {
                            if (m_tod.WeatherActive())
                            {
                                GUI.enabled = false;
                            }
                        }

                        if (m_editorUtils.Button("StartWeather"))
                        {
                            m_tod.StartWeather(i);
                        }

                        if (Application.isPlaying)
                        {
                            GUI.enabled = true;
                        }

                        if (m_tod.WeatherActive())
                        {
                            GUI.enabled = true;
                        }
                        else
                        {
                            GUI.enabled = false;
                        }

                        if (m_editorUtils.Button("StopWeather"))
                        {
                            m_tod.StopWeather(m_tod.m_instantWeatherStop);
                        }

                        GUI.enabled = true;

                        EditorGUILayout.EndHorizontal();

                        if (m_editorUtils.Button("CopySettingsOver"))
                        {
                            if (EditorUtility.DisplayDialog("Copy Settings", "Are you sure you want to copy the settings over?", "Yes", "No"))
                            {
                                TimeOfDayProfileData.CopySettings(m_tod.WeatherProfiles[i].WeatherData.m_weatherData, m_tod.TimeOfDayProfile.TimeOfDayData);
                                EditorUtility.SetDirty(m_tod.WeatherProfiles[i]);
                            }
                        }
                        EditorGUILayout.EndVertical();
                    }
                }
                m_editorUtils.InlineHelp("WeatherProfile", helpEnabled);
                if (m_editorUtils.Button("AddNewWeatherProfile"))
                {
                    m_tod.WeatherProfiles.Add(null);
                }
                EditorGUI.indentLevel--;
            }

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(m_tod);
            }
        }
        private void SunFlareInfoHelp()
        {
            EditorGUILayout.HelpBox("Please note that Unity sun flare in HDRP does not yet get culled by volumetric clouds and they will render through the clouds. We hope Unity will add support for this in the future, if this notice is removed then unity has added support.", MessageType.Info);
        }
        /// <summary>
        /// Post Processing
        /// </summary>
        private void AmbientOcclusionSettings(bool helpEnabled)
        {
            m_tod.TimeOfDayPostFxProfile.TimeOfDayPostFXData.m_useAmbientOcclusion = m_editorUtils.Toggle("UsePostFX", m_tod.TimeOfDayPostFxProfile.TimeOfDayPostFXData.m_useAmbientOcclusion, helpEnabled);
            if (m_tod.TimeOfDayPostFxProfile.TimeOfDayPostFXData.m_useAmbientOcclusion)
            {
                m_tod.TimeOfDayPostFxProfile.TimeOfDayPostFXData.m_ambientIntensity = CurveField("AOIntensity", m_tod.TimeOfDayPostFxProfile.TimeOfDayPostFXData.m_ambientIntensity, helpEnabled);
                m_tod.TimeOfDayPostFxProfile.TimeOfDayPostFXData.m_ambientDirectStrength = CurveField("AODirectLightIntensity", m_tod.TimeOfDayPostFxProfile.TimeOfDayPostFXData.m_ambientDirectStrength, helpEnabled);
                m_tod.TimeOfDayPostFxProfile.TimeOfDayPostFXData.m_ambientRadius = CurveField("AORadius", m_tod.TimeOfDayPostFxProfile.TimeOfDayPostFXData.m_ambientRadius, helpEnabled);
            }
        }
        private void ColorGradingSettings(bool helpEnabled)
        {
            m_tod.TimeOfDayPostFxProfile.TimeOfDayPostFXData.m_useColorGrading = m_editorUtils.Toggle("UsePostFX", m_tod.TimeOfDayPostFxProfile.TimeOfDayPostFXData.m_useColorGrading, helpEnabled);
            if (m_tod.TimeOfDayPostFxProfile.TimeOfDayPostFXData.m_useColorGrading)
            {
                m_tod.TimeOfDayPostFxProfile.TimeOfDayPostFXData.m_contrast = CurveField("CGContrast", m_tod.TimeOfDayPostFxProfile.TimeOfDayPostFXData.m_contrast, helpEnabled);
                m_tod.TimeOfDayPostFxProfile.TimeOfDayPostFXData.m_colorFilter = GradientField("CGColorFilter", m_tod.TimeOfDayPostFxProfile.TimeOfDayPostFXData.m_colorFilter, true, helpEnabled);
                m_tod.TimeOfDayPostFxProfile.TimeOfDayPostFXData.m_saturation = CurveField("CGSaturation", m_tod.TimeOfDayPostFxProfile.TimeOfDayPostFXData.m_saturation, helpEnabled);
                m_tod.TimeOfDayPostFxProfile.TimeOfDayPostFXData.m_temperature = CurveField("CGTemperature", m_tod.TimeOfDayPostFxProfile.TimeOfDayPostFXData.m_temperature, helpEnabled);
                m_tod.TimeOfDayPostFxProfile.TimeOfDayPostFXData.m_tint = CurveField("CGTint", m_tod.TimeOfDayPostFxProfile.TimeOfDayPostFXData.m_tint, helpEnabled);
            }
        }
        private void DepthOfFieldSettings(bool helpEnabled)
        {
            m_tod.TimeOfDayPostFxProfile.TimeOfDayPostFXData.m_useDepthOfField = m_editorUtils.Toggle("UsePostFX", m_tod.TimeOfDayPostFxProfile.TimeOfDayPostFXData.m_useDepthOfField, helpEnabled);
            if (m_tod.TimeOfDayPostFxProfile.TimeOfDayPostFXData.m_useDepthOfField)
            {
                m_tod.TimeOfDayPostFxProfile.TimeOfDayPostFXData.m_depthOfFieldMode = (DepthOfFieldMode)m_editorUtils.EnumPopup("DepthOfFieldMode", m_tod.TimeOfDayPostFxProfile.TimeOfDayPostFXData.m_depthOfFieldMode, helpEnabled);
                if (m_tod.TimeOfDayPostFxProfile.TimeOfDayPostFXData.m_depthOfFieldMode != DepthOfFieldMode.Off)
                {
                    EditorGUI.BeginChangeCheck();
                    m_tod.TimeOfDayPostFxProfile.TimeOfDayPostFXData.m_physicallyBased = m_editorUtils.Toggle("PhysicallyBased", m_tod.TimeOfDayPostFxProfile.TimeOfDayPostFXData.m_physicallyBased, helpEnabled);
                    if (m_tod.TimeOfDayPostFxProfile.TimeOfDayPostFXData.m_physicallyBased)
                    {
                        EditorGUILayout.HelpBox(m_editorUtils.GetTooltip("PhysicallyBasedHelp"), MessageType.Info);
                    }
                    if (EditorGUI.EndChangeCheck())
                    {
                        m_tod.TimeOfDayPostFxProfile.TimeOfDayPostFXData.SetDepthOfFieldQuality(m_tod.GetTODComponents().m_depthOfField);
                    }
                }

                if (m_tod.TimeOfDayPostFxProfile.TimeOfDayPostFXData.m_depthOfFieldMode == DepthOfFieldMode.Manual)
                {
                    EditorGUILayout.LabelField("Near Blur", EditorStyles.boldLabel);
                    EditorGUI.indentLevel++;
                    m_tod.TimeOfDayPostFxProfile.TimeOfDayPostFXData.m_nearBlurMultiplier = m_editorUtils.Slider("DOFNearBlurMultiplier", m_tod.TimeOfDayPostFxProfile.TimeOfDayPostFXData.m_nearBlurMultiplier, 0.01f, 25f, helpEnabled);
                    m_tod.TimeOfDayPostFxProfile.TimeOfDayPostFXData.m_nearBlurStart = CurveField("DOFNearBlurStart", m_tod.TimeOfDayPostFxProfile.TimeOfDayPostFXData.m_nearBlurStart, helpEnabled);
                    m_tod.TimeOfDayPostFxProfile.TimeOfDayPostFXData.m_nearBlurEnd = CurveField("DOFNearBlurEnd", m_tod.TimeOfDayPostFxProfile.TimeOfDayPostFXData.m_nearBlurEnd, helpEnabled);
                    EditorGUI.indentLevel--;
                    EditorGUILayout.LabelField("Far Blur", EditorStyles.boldLabel);
                    EditorGUI.indentLevel++;
                    m_tod.TimeOfDayPostFxProfile.TimeOfDayPostFXData.m_farBlurMultiplier = m_editorUtils.Slider("DOFFarBlurMultiplier", m_tod.TimeOfDayPostFxProfile.TimeOfDayPostFXData.m_farBlurMultiplier, 0.01f, 300f, helpEnabled);
                    m_tod.TimeOfDayPostFxProfile.TimeOfDayPostFXData.m_farBlurStart = CurveField("DOFFarBlurStart", m_tod.TimeOfDayPostFxProfile.TimeOfDayPostFXData.m_farBlurStart, helpEnabled);
                    m_tod.TimeOfDayPostFxProfile.TimeOfDayPostFXData.m_farBlurEnd = CurveField("DOFFarBlurEnd", m_tod.TimeOfDayPostFxProfile.TimeOfDayPostFXData.m_farBlurEnd, helpEnabled);
                    EditorGUI.indentLevel--;
                }
                else if (m_tod.TimeOfDayPostFxProfile.TimeOfDayPostFXData.m_depthOfFieldMode == DepthOfFieldMode.UsePhysicalCamera)
                {
                    EditorGUILayout.HelpBox(m_editorUtils.GetTooltip("PhysicalCamera"), MessageType.Info);
                    if (m_editorUtils.Button("SelectCamera"))
                    {
                        Selection.activeObject = m_tod.Player;
                    }
                }
            }
        }
        private void BloomSettings(bool helpEnabled)
        {
            m_tod.TimeOfDayPostFxProfile.TimeOfDayPostFXData.m_useBloom = m_editorUtils.Toggle("UsePostFX", m_tod.TimeOfDayPostFxProfile.TimeOfDayPostFXData.m_useBloom, helpEnabled);
            if (m_tod.TimeOfDayPostFxProfile.TimeOfDayPostFXData.m_useBloom)
            {
                m_tod.TimeOfDayPostFxProfile.TimeOfDayPostFXData.m_bloomThreshold = CurveField("BloomThreshold", m_tod.TimeOfDayPostFxProfile.TimeOfDayPostFXData.m_bloomThreshold, helpEnabled);
                m_tod.TimeOfDayPostFxProfile.TimeOfDayPostFXData.m_bloomIntensity = CurveField("BloomIntensity", m_tod.TimeOfDayPostFxProfile.TimeOfDayPostFXData.m_bloomIntensity, helpEnabled);
                m_tod.TimeOfDayPostFxProfile.TimeOfDayPostFXData.m_bloomScatter = CurveField("BloomScatter", m_tod.TimeOfDayPostFxProfile.TimeOfDayPostFXData.m_bloomScatter, helpEnabled);
                m_tod.TimeOfDayPostFxProfile.TimeOfDayPostFXData.m_bloomTint = GradientField("BloomTint", m_tod.TimeOfDayPostFxProfile.TimeOfDayPostFXData.m_bloomTint, false, helpEnabled);
            }
        }
        private void ShadowToningSettings(bool helpEnabled)
        {
            m_tod.TimeOfDayPostFxProfile.TimeOfDayPostFXData.m_useShadowToning = m_editorUtils.Toggle("UsePostFX", m_tod.TimeOfDayPostFxProfile.TimeOfDayPostFXData.m_useShadowToning, helpEnabled);
            if (m_tod.TimeOfDayPostFxProfile.TimeOfDayPostFXData.m_useShadowToning)
            {
                m_tod.TimeOfDayPostFxProfile.TimeOfDayPostFXData.m_shadows = GradientField("STShadows", m_tod.TimeOfDayPostFxProfile.TimeOfDayPostFXData.m_shadows, false, helpEnabled);
                m_tod.TimeOfDayPostFxProfile.TimeOfDayPostFXData.m_highlights = GradientField("STHighlights", m_tod.TimeOfDayPostFxProfile.TimeOfDayPostFXData.m_highlights, false, helpEnabled);
                m_tod.TimeOfDayPostFxProfile.TimeOfDayPostFXData.m_shadowBalance = CurveField("STBalance", m_tod.TimeOfDayPostFxProfile.TimeOfDayPostFXData.m_shadowBalance, helpEnabled);
            }
        }
        private void VignetteSettings(bool helpEnabled)
        {
            m_tod.TimeOfDayPostFxProfile.TimeOfDayPostFXData.m_useVignette = m_editorUtils.Toggle("UsePostFX", m_tod.TimeOfDayPostFxProfile.TimeOfDayPostFXData.m_useVignette, helpEnabled);
            if (m_tod.TimeOfDayPostFxProfile.TimeOfDayPostFXData.m_useVignette)
            {
                m_tod.TimeOfDayPostFxProfile.TimeOfDayPostFXData.m_vignetteColor = GradientField("VignetteColor", m_tod.TimeOfDayPostFxProfile.TimeOfDayPostFXData.m_vignetteColor, false, helpEnabled);
                m_tod.TimeOfDayPostFxProfile.TimeOfDayPostFXData.m_vignetteIntensity = CurveField("VignetteIntensity", m_tod.TimeOfDayPostFxProfile.TimeOfDayPostFXData.m_vignetteIntensity, helpEnabled);
                m_tod.TimeOfDayPostFxProfile.TimeOfDayPostFXData.m_vignetteSmoothness = CurveField("VignetteSmoothness", m_tod.TimeOfDayPostFxProfile.TimeOfDayPostFXData.m_vignetteSmoothness, helpEnabled);
            }
        }
        private void RayTracingSettings(bool helpEnabled)
        {
            m_editorUtils.InlineHelp("RayTracingHelp", true);

            EditorGUI.BeginChangeCheck();
            bool useRayTracing = m_tod.UseRayTracing;
            useRayTracing = m_editorUtils.Toggle("UseRayTracing", useRayTracing, helpEnabled);
            if (EditorGUI.EndChangeCheck())
            {
                if (useRayTracing != m_tod.UseRayTracing)
                {
                    m_tod.UseRayTracing = useRayTracing;
                    if (useRayTracing)
                    {
                        SetScriptDefine("RAY_TRACING_ENABLED", true);
                    }
                    else
                    {
                        SetScriptDefine("RAY_TRACING_ENABLED", false);
                    }
                }
                EditorUtility.SetDirty(m_tod);
            }
#if HDRPTIMEOFDAY
            if (useRayTracing)
            {
                EditorGUI.indentLevel++;
                EditorGUI.BeginChangeCheck();
#if HDRPTIMEOFDAY
                m_tod.RayTracingLightingModel = (RayTracingLightingModel)m_editorUtils.EnumPopup("RayTracingLightingModel", m_tod.RayTracingLightingModel, helpEnabled);
#endif
                if (EditorGUI.EndChangeCheck())
                {
                    EditorUtility.SetDirty(m_tod);
                }
                EditorGUI.BeginChangeCheck();
                bool renderInEdit = m_tod.TimeOfDayProfile.RayTracingProfile.RayTracingSettings.m_renderInEditMode;
                renderInEdit = m_editorUtils.Toggle("RenderInEditMode", renderInEdit, helpEnabled);
                if (renderInEdit != m_tod.TimeOfDayProfile.RayTracingProfile.RayTracingSettings.m_renderInEditMode)
                {
                    m_tod.TimeOfDayProfile.RayTracingProfile.RayTracingSettings.m_renderInEditMode = renderInEdit;
                    EditorUtility.SetDirty(m_tod.TimeOfDayProfile);
                    HDRPTimeOfDayRayTracingUtils.ApplyRayTracingSettings(m_tod.GetTODComponents(), m_tod.TimeOfDayProfile.RayTracingProfile.RayTracingSettings, m_tod);
                    m_tod.RefreshRTVolumeState();
                    EditorGUIUtility.ExitGUI();
                }
                m_tod.TimeOfDayProfile.RayTracingProfile.RayTracingSettings.m_overrideQuality = m_editorUtils.Toggle("UseOverrideQuality", m_tod.TimeOfDayProfile.RayTracingProfile.RayTracingSettings.m_overrideQuality, helpEnabled);
                if (m_tod.TimeOfDayProfile.RayTracingProfile.RayTracingSettings.m_overrideQuality)
                {
                    EditorGUI.indentLevel++;
                    m_tod.TimeOfDayProfile.RayTracingProfile.RayTracingSettings.m_overrideQualityValue = (GeneralQuality)m_editorUtils.EnumPopup("OverrideQuality", m_tod.TimeOfDayProfile.RayTracingProfile.RayTracingSettings.m_overrideQualityValue, helpEnabled);
                    EditorGUI.indentLevel--;
                }

                if (EditorGUI.EndChangeCheck())
                {
                    EditorUtility.SetDirty(m_tod.TimeOfDayProfile);
                    HDRPTimeOfDayRayTracingUtils.ApplyOverrideRayTracingQuality(m_tod.GetTODComponents(), m_tod.TimeOfDayProfile.RayTracingProfile.RayTracingSettings);
                    m_tod.RefreshRTVolumeState();
                }

                EditorGUI.BeginChangeCheck();
                //SSGI
                m_tod.TimeOfDayProfile.RayTracingProfile.RayTracingSettings.m_rayTraceSSGI = m_editorUtils.Toggle("RayTraceSSGI", m_tod.TimeOfDayProfile.RayTracingProfile.RayTracingSettings.m_rayTraceSSGI, helpEnabled);
                if (m_tod.TimeOfDayProfile.RayTracingProfile.RayTracingSettings.m_rayTraceSSGI)
                {
                    EditorGUI.indentLevel++;
                    if (!m_tod.TimeOfDayProfile.RayTracingProfile.RayTracingSettings.m_overrideQuality)
                    {
                        m_tod.TimeOfDayProfile.RayTracingProfile.RayTracingSettings.m_ssgiQuality = (GeneralQuality)m_editorUtils.EnumPopup("Quality", m_tod.TimeOfDayProfile.RayTracingProfile.RayTracingSettings.m_ssgiQuality, helpEnabled);
                    }
                    EditorGUI.indentLevel--;
                }
                //SSR
                m_tod.TimeOfDayProfile.RayTracingProfile.RayTracingSettings.m_rayTraceSSR = m_editorUtils.Toggle("RayTraceSSR", m_tod.TimeOfDayProfile.RayTracingProfile.RayTracingSettings.m_rayTraceSSR, helpEnabled);
                if (m_tod.TimeOfDayProfile.RayTracingProfile.RayTracingSettings.m_rayTraceSSR)
                {
                    EditorGUI.indentLevel++;
                    if (!m_tod.TimeOfDayProfile.RayTracingProfile.RayTracingSettings.m_overrideQuality)
                    {
                        m_tod.TimeOfDayProfile.RayTracingProfile.RayTracingSettings.m_ssrQuality = (GeneralQuality)m_editorUtils.EnumPopup("Quality", m_tod.TimeOfDayProfile.RayTracingProfile.RayTracingSettings.m_ssrQuality, helpEnabled);
                    }
                    EditorGUI.indentLevel--;
                }
                //AO
                m_tod.TimeOfDayProfile.RayTracingProfile.RayTracingSettings.m_rayTraceAmbientOcclusion = m_editorUtils.Toggle("RayTraceAmbientOcclusion", m_tod.TimeOfDayProfile.RayTracingProfile.RayTracingSettings.m_rayTraceAmbientOcclusion, helpEnabled);
                if (m_tod.TimeOfDayProfile.RayTracingProfile.RayTracingSettings.m_rayTraceAmbientOcclusion)
                {
                    EditorGUI.indentLevel++;
                    if (!m_tod.TimeOfDayProfile.RayTracingProfile.RayTracingSettings.m_overrideQuality)
                    {
                        m_tod.TimeOfDayProfile.RayTracingProfile.RayTracingSettings.m_aoQuality = (GeneralQuality)m_editorUtils.EnumPopup("Quality", m_tod.TimeOfDayProfile.RayTracingProfile.RayTracingSettings.m_aoQuality, helpEnabled);
                    }
                    EditorGUI.indentLevel--;
                }
                //Recursive Rendering
                m_tod.TimeOfDayProfile.RayTracingProfile.RayTracingSettings.m_recursiveRendering = m_editorUtils.Toggle("RecursiveRendering", m_tod.TimeOfDayProfile.RayTracingProfile.RayTracingSettings.m_recursiveRendering, helpEnabled);
                if (m_tod.TimeOfDayProfile.RayTracingProfile.RayTracingSettings.m_recursiveRendering)
                {
                    EditorGUI.indentLevel++;
                    if (!m_tod.TimeOfDayProfile.RayTracingProfile.RayTracingSettings.m_overrideQuality)
                    {
                        m_tod.TimeOfDayProfile.RayTracingProfile.RayTracingSettings.m_recursiveRenderingQuality = (GeneralQuality)m_editorUtils.EnumPopup("Quality", m_tod.TimeOfDayProfile.RayTracingProfile.RayTracingSettings.m_recursiveRenderingQuality, helpEnabled);
                    }
                    EditorGUI.indentLevel--;
                }
                //SSS
                m_tod.TimeOfDayProfile.RayTracingProfile.RayTracingSettings.m_rayTraceSubSurfaceScattering = m_editorUtils.Toggle("RayTraceSubSurfaceScattering", m_tod.TimeOfDayProfile.RayTracingProfile.RayTracingSettings.m_rayTraceSubSurfaceScattering, helpEnabled);
                if (m_tod.TimeOfDayProfile.RayTracingProfile.RayTracingSettings.m_rayTraceSubSurfaceScattering)
                {
                    EditorGUI.indentLevel++;
                    if (!m_tod.TimeOfDayProfile.RayTracingProfile.RayTracingSettings.m_overrideQuality)
                    {
                        m_tod.TimeOfDayProfile.RayTracingProfile.RayTracingSettings.m_sssQuality = (GeneralQuality)m_editorUtils.EnumPopup("Quality", m_tod.TimeOfDayProfile.RayTracingProfile.RayTracingSettings.m_sssQuality, helpEnabled);
                    }
                    EditorGUI.indentLevel--;
                }
                //Shadows
                m_tod.TimeOfDayProfile.RayTracingProfile.RayTracingSettings.m_rayTraceShadows = m_editorUtils.Toggle("RayTraceShadows", m_tod.TimeOfDayProfile.RayTracingProfile.RayTracingSettings.m_rayTraceShadows, helpEnabled);
                if (m_tod.TimeOfDayProfile.RayTracingProfile.RayTracingSettings.m_rayTraceShadows)
                {
                    EditorGUI.indentLevel++;
                    if (!m_tod.TimeOfDayProfile.RayTracingProfile.RayTracingSettings.m_overrideQuality)
                    {
                        m_tod.TimeOfDayProfile.RayTracingProfile.RayTracingSettings.m_shadowsQuality = (GeneralQuality)m_editorUtils.EnumPopup("Quality", m_tod.TimeOfDayProfile.RayTracingProfile.RayTracingSettings.m_shadowsQuality, helpEnabled);
                    }
                    m_tod.TimeOfDayProfile.RayTracingProfile.RayTracingSettings.m_rayTraceContactShadows = m_editorUtils.Toggle("RayTraceContactShadows", m_tod.TimeOfDayProfile.RayTracingProfile.RayTracingSettings.m_rayTraceContactShadows, helpEnabled);
                    m_tod.TimeOfDayProfile.RayTracingProfile.RayTracingSettings.m_colorShadows = m_editorUtils.Toggle("ColorShadows", m_tod.TimeOfDayProfile.RayTracingProfile.RayTracingSettings.m_colorShadows, helpEnabled);
                    m_tod.TimeOfDayProfile.RayTracingProfile.RayTracingSettings.m_denoiseShadows = m_editorUtils.Toggle("DenoiseShadows", m_tod.TimeOfDayProfile.RayTracingProfile.RayTracingSettings.m_denoiseShadows, helpEnabled);
                    EditorGUI.indentLevel--;
                }
                EditorGUI.indentLevel--;
                if (EditorGUI.EndChangeCheck())
                {
                    EditorUtility.SetDirty(m_tod.TimeOfDayProfile);
                    HDRPTimeOfDayRayTracingUtils.ApplyRayTracingSettings(m_tod.GetTODComponents(), m_tod.TimeOfDayProfile.RayTracingProfile.RayTracingSettings, m_tod);
                    m_tod.RefreshRTVolumeState();
                }
            }
#endif
        }
        private void LightProbePanel(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();
            m_tod.m_lightProbeSettings.m_processingMode = (ProcessingMode)m_editorUtils.EnumPopup("LPProcessMode", m_tod.m_lightProbeSettings.m_processingMode, helpEnabled);
            m_tod.m_lightProbeSettings.m_generateMode = (LightProbeGenerateMode)m_editorUtils.EnumPopup("LPGenerateMode", m_tod.m_lightProbeSettings.m_generateMode, helpEnabled);
            if (m_tod.m_lightProbeSettings.m_generateMode != LightProbeGenerateMode.Volumes)
            {
                EditorGUI.indentLevel++;
                m_tod.m_lightProbeSettings.m_sphereOffset = m_editorUtils.FloatField("LPSphereOffset", m_tod.m_lightProbeSettings.m_sphereOffset, helpEnabled);
                m_tod.m_lightProbeSettings.m_addHeightOffset = m_editorUtils.Toggle("LPUseHeightOffset", m_tod.m_lightProbeSettings.m_addHeightOffset, helpEnabled);
                if (m_tod.m_lightProbeSettings.m_addHeightOffset)
                {
                    EditorGUI.indentLevel++;
                    m_tod.m_lightProbeSettings.m_heightOffset = m_editorUtils.FloatField("LPHeightOffset", m_tod.m_lightProbeSettings.m_heightOffset, helpEnabled);
                    EditorGUI.indentLevel--;
                }
                EditorGUI.indentLevel--;
            }

            if (m_editorUtils.Button("GenerateLightProbes"))
            {
                HDRPTimeOfDayLightProbeGeneratorUtils.GenerateLightProbes(m_tod.m_lightProbeSettings);
            }

            if (m_editorUtils.Button("RefreshAllVolumes"))
            {
                HDRPTimeOfDayLightProbeVolume.RefreshAllVolumes();
            }

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(m_tod);
            }
        }
        private void SetScriptDefine(string define, bool add)
        {
            bool updateScripting = false;
            string symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            
            if (add)
            {
                if (!symbols.Contains(define))
                {
                    updateScripting = true;
                    if (symbols.Length < 1)
                    {
                        symbols += define;
                    }
                    else
                    {
                        symbols += ";" + define;
                    }
                }
            }
            else
            {
                if (symbols.Contains(define))
                {
                    updateScripting = true;
                    symbols = symbols.Replace(define, "");
                }
            }

            if (updateScripting)
            {
                PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, symbols);
            }
        }
        /// <summary>
        /// Ambient Audio
        /// </summary>
        private void AmbientAudio(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();
            m_tod.AudioProfile = (HDRPTimeOfDayAmbientProfile)m_editorUtils.ObjectField("AmbientProfile", m_tod.AudioProfile, typeof(HDRPTimeOfDayAmbientProfile), false, helpEnabled);
            if (m_tod.AudioProfile != null)
            {
                m_tod.m_interiorControllerData.m_exteriorReverbPreset = (AudioReverbPreset)m_editorUtils.EnumPopup("ExteriorReverbPreset", m_tod.m_interiorControllerData.m_exteriorReverbPreset, helpEnabled);
                m_tod.UseAmbientAudioTracker = m_editorUtils.Toggle("UseAmbientAudioTracker", m_tod.UseAmbientAudio, helpEnabled);
                if (m_tod.UseAmbientAudioTracker)
                {
                    EditorGUI.indentLevel++;
                    m_tod.AudioProfile.m_audioTrackingRange = m_editorUtils.FloatField("TrackingRange", m_tod.AudioProfile.m_audioTrackingRange, helpEnabled);
                    m_tod.AmbientAudioTrackerGameObject = (GameObject)m_editorUtils.ObjectField("TrackerGameObject", m_tod.AmbientAudioTrackerGameObject, typeof(GameObject), true, helpEnabled);
                    EditorGUI.indentLevel--;
                }
                m_tod.AudioProfile.m_morningAmbient = (AudioClip)m_editorUtils.ObjectField("MorningAmbient", m_tod.AudioProfile.m_morningAmbient, typeof(AudioClip), false, helpEnabled);
                m_tod.AudioProfile.m_afternoonAmbient = (AudioClip)m_editorUtils.ObjectField("AfternoonAmbient", m_tod.AudioProfile.m_afternoonAmbient, typeof(AudioClip), false, helpEnabled);
                m_tod.AudioProfile.m_eveningAmbient = (AudioClip)m_editorUtils.ObjectField("EveningAmbient", m_tod.AudioProfile.m_eveningAmbient, typeof(AudioClip), false, helpEnabled);
                m_tod.AudioProfile.m_nightAmbient = (AudioClip)m_editorUtils.ObjectField("NightAmbient", m_tod.AudioProfile.m_nightAmbient, typeof(AudioClip), false, helpEnabled);
                m_tod.AudioProfile.m_masterVolume = m_editorUtils.Slider("MaxVolume", m_tod.AudioProfile.m_masterVolume, 0f, 1f, helpEnabled);
                m_tod.AudioProfile.m_weatherVolumeMultiplier = m_editorUtils.Slider("WeatherVolumeMultiplier", m_tod.AudioProfile.m_weatherVolumeMultiplier, 0f, 1f, helpEnabled);
                m_tod.AudioProfile.m_timeOfDayIntervals = m_editorUtils.Vector4Field("TimeOfDayIntervals", m_tod.AudioProfile.m_timeOfDayIntervals, helpEnabled);
            }

            if (EditorGUI.EndChangeCheck())
            {
                if (m_tod.AudioProfile != null)
                {
                    EditorUtility.SetDirty(m_tod.AudioProfile);
                }
            }
        }
        /// <summary>
        /// Underwater
        /// </summary>
        private void UnderwaterOverridesSettings(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();
            m_tod.TimeOfDayProfile.UnderwaterOverrideData.m_useOverrides = m_editorUtils.Toggle("UseUnderwaterOverrides", m_tod.TimeOfDayProfile.UnderwaterOverrideData.m_useOverrides, helpEnabled);
            if (m_tod.TimeOfDayProfile.UnderwaterOverrideData.m_useOverrides)
            {
                EditorGUI.indentLevel++;
                m_tod.TimeOfDayProfile.UnderwaterOverrideData.m_systemType = (UnderwaterOverrideSystemType)m_editorUtils.EnumPopup("SystemSyncType", m_tod.TimeOfDayProfile.UnderwaterOverrideData.m_systemType, helpEnabled);
                if (m_tod.TimeOfDayProfile.UnderwaterOverrideData.m_systemType == UnderwaterOverrideSystemType.HDRPTimeOfDay)
                {
                    m_tod.TimeOfDayProfile.UnderwaterOverrideData.m_seaLevel = m_editorUtils.FloatField("SeaLevel", m_tod.TimeOfDayProfile.UnderwaterOverrideData.m_seaLevel, helpEnabled);
                    m_tod.TimeOfDayProfile.UnderwaterOverrideData.m_useUnderwaterReverb = m_editorUtils.Toggle("UseUnderwaterReverb", m_tod.TimeOfDayProfile.UnderwaterOverrideData.m_useUnderwaterReverb, helpEnabled);
                }
                EditorGUI.indentLevel--;
            }
            if (EditorGUI.EndChangeCheck())
            {
                if (m_tod.TimeOfDayProfile != null)
                {
                    EditorUtility.SetDirty(m_tod.TimeOfDayProfile);
                }

                if (m_tod.WeatherProfiles.Count > 0)
                {
                    foreach (HDRPTimeOfDayWeatherProfile profile in m_tod.WeatherProfiles)
                    {
                        profile.UnderwaterOverrideData.m_systemType = m_tod.TimeOfDayProfile.UnderwaterOverrideData.m_systemType;
                        profile.UnderwaterOverrideData.m_seaLevel = m_tod.TimeOfDayProfile.UnderwaterOverrideData.m_seaLevel;
                        profile.UnderwaterOverrideData.m_useUnderwaterReverb = m_tod.TimeOfDayProfile.UnderwaterOverrideData.m_useUnderwaterReverb;
                    }
                }
            }
        }

        #endregion
        #region Utils

        /// <summary>
        /// Checks to see if realtime/baked gi is baked
        /// </summary>
        private void CheckBakedRealtimeGIState()
        {
            if (m_tod != null)
            {
                if (Lightmapping.lightingDataAsset != null)
                {
                    if (Lightmapping.lightingSettings.bakedGI && Lightmapping.lightingSettings.realtimeGI &&
                        Lightmapping.lightingSettings.realtimeEnvironmentLighting)
                    {
                        if (m_tod.TimeOfDayProfile != null && !m_tod.m_ignoreRealtimeGICheck)
                        {
                            if (!m_tod.TimeOfDayProfile.TimeOfDayData.m_useSSGI)
                            {
                                return;
                            }

                            int result = EditorUtility.DisplayDialogComplex("Realtime GI Detected",
                                "Realtime GI / Environment Lighting has been detected in the scene. It seems that you have SSGI enabled and this is not required when using baked realtime GI (Enlighten), we recommend disabling SSGI. Would you like to disable this effect?",
                                "Yes", "Ignore", "No");

                            switch (result)
                            {
                                case 0:
                                {
                                    m_tod.TimeOfDayProfile.TimeOfDayData.m_useSSGI = false;
                                    m_tod.m_ignoreRealtimeGICheck = true;
                                    break;
                                }
                                case 1:
                                {
                                    m_tod.m_ignoreRealtimeGICheck = true;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Handy layer mask interface - creates a EditorGUILayout Mask field
        /// </summary>
        /// <param name="label"></param>
        /// <param name="layerMask"></param>
        /// <returns></returns>
        public static LayerMask LayerMaskField(EditorUtils editorUtils, string label, LayerMask layerMask, bool helpEnabled)
        {
            List<string> layers = new List<string>();
            List<int> layerNumbers = new List<int>();

            for (int i = 0; i < 32; i++)
            {
                string layerName = LayerMask.LayerToName(i);
                if (layerName != "")
                {
                    layers.Add(layerName);
                    layerNumbers.Add(i);
                }
            }
            int maskWithoutEmpty = 0;
            for (int i = 0; i < layerNumbers.Count; i++)
            {
                if (((1 << layerNumbers[i]) & layerMask.value) > 0)
                    maskWithoutEmpty |= (1 << i);
            }
            maskWithoutEmpty = EditorGUILayout.MaskField(new GUIContent(editorUtils.GetTextValue(label), editorUtils.GetTooltip(label)), maskWithoutEmpty, layers.ToArray());
            editorUtils.InlineHelp(label, helpEnabled);
            int mask = 0;
            for (int i = 0; i < layerNumbers.Count; i++)
            {
                if ((maskWithoutEmpty & (1 << i)) > 0)
                    mask |= (1 << layerNumbers[i]);
            }
            layerMask.value = mask;
            return layerMask;
        }
        private void RepaintSceneView()
        {
            if (m_sceneView == null)
            {
                m_sceneView = SceneView.lastActiveSceneView;
            }

            if (m_sceneView != null)
            {
                m_sceneView.Repaint();
            }
        }
        private void SimulateUpdate()
        {
            if (m_tod == null || !m_tod.DebugSettings.m_simulate)
            {
                return;
            }

            m_tod.TimeOfDay += Time.deltaTime * m_tod.DebugSettings.m_simulationSpeed;
            if (m_tod.TimeOfDay >= 24f)
            {
                m_tod.TimeOfDay = 0f;
            }
        }
        /// <summary>
        /// Custom Indicator Line
        /// </summary>
        /// <param name="percentage">percentage to draw the indicator line.</param>
        /// <param name="color">color of the indicator line.</param>
        /// <param name="width">width of the indicator line.</param>
        private void DrawIndicatorLine(float percentage, Color? color = null, float? width = null)
        {
            Rect rect = GUILayoutUtility.GetLastRect();
            float markerXPos = rect.x + EditorGUIUtility.labelWidth + (rect.width - EditorGUIUtility.labelWidth) * percentage;
            EditorGUI.DrawRect(new Rect(markerXPos, rect.y, width ?? m_defaultIndicatorWidth, EditorGUIUtility.singleLineHeight), color ?? m_defaultIndicatorColor);
        }
        /// <summary>
        /// Custom gradient field
        /// </summary>
        /// <param name="key"></param>
        /// <param name="gradient"></param>
        /// <param name="hdr"></param>
        /// <param name="helpEnabled"></param>
        /// <returns></returns>
        private Gradient GradientField(string key, Gradient gradient, bool hdr, bool helpEnabled)
        {
            gradient = EditorGUILayout.GradientField(new GUIContent(m_editorUtils.GetTextValue(key), m_editorUtils.GetTooltip(key)), gradient, true);
            DrawIndicatorLine(TODPercentage);
            m_editorUtils.InlineHelp(key, helpEnabled);
            return gradient;
        }
        /// <summary>
        /// Custom Curve Field
        /// </summary>
        /// <param name="key"></param>
        /// <param name="curve"></param>
        /// <param name="helpEnabled"></param>
        /// <returns></returns>
        private AnimationCurve CurveField(string key, AnimationCurve curve, bool helpEnabled)
        {
           AnimationCurve value = m_editorUtils.CurveField(key, curve, helpEnabled);
           DrawIndicatorLine(TODPercentage);
           return value;
        }

        #endregion
        #region Menu Items

        //[MenuItem("Window/Procedural Worlds/HDRP Time Of Day/Add HDRP TOD")]
        [MenuItem("Window/" + PWConst.COMMON_MENU + "/HDRP Time Of Day/Add HDRP Time Of Day...", false, 40)]
        public static void AddHDRPTODToScene()
        {
            HDRPTimeOfDay.CreateTimeOfDay(null);
        }
        //[MenuItem("Window/Procedural Worlds/HDRP Time Of Day/Remove HDRP TOD")]
        [MenuItem("Window/" + PWConst.COMMON_MENU + "/HDRP Time Of Day/Remove HDRP Time Of Day...", false, 40)]
        public static void RemoveHDRPTODToScene()
        {
            HDRPTimeOfDay.RemoveTimeOfDay();
        }
        [MenuItem("Assets/Create/Procedural Worlds/HDRP Time Of Day/Create Empty Post FX Profile")]
        public static void CreateHDRPTimeOfDayPostFXProfile()
        {
            HDRPTimeOfDayPostFXProfile asset = ScriptableObject.CreateInstance<HDRPTimeOfDayPostFXProfile>();

            AssetDatabase.CreateAsset(asset, "Assets/HDRP Time Of Day Post FX Profile.asset");
            AssetDatabase.SaveAssets();

            EditorUtility.FocusProjectWindow();

            Selection.activeObject = asset;
        }
        [MenuItem("Assets/Create/Procedural Worlds/HDRP Time Of Day/Create Empty Profile")]
        public static void CreateHDRPTimeOfDayProfile()
        {
            HDRPTimeOfDayProfile asset = ScriptableObject.CreateInstance<HDRPTimeOfDayProfile>();

            AssetDatabase.CreateAsset(asset, "Assets/HDRP Time Of Day Profile.asset");
            AssetDatabase.SaveAssets();

            EditorUtility.FocusProjectWindow();

            Selection.activeObject = asset;
        }
        [MenuItem("Assets/Create/Procedural Worlds/HDRP Time Of Day/Create Empty Weather Profile")]
        public static void CreateHDRPTimeOfDayWeatherProfile()
        {
            HDRPTimeOfDayWeatherProfile asset = ScriptableObject.CreateInstance<HDRPTimeOfDayWeatherProfile>();

            AssetDatabase.CreateAsset(asset, "Assets/HDRP Time Of Day Weather Profile.asset");
            AssetDatabase.SaveAssets();

            EditorUtility.FocusProjectWindow();

            Selection.activeObject = asset;
        }
        [MenuItem("Assets/Create/Procedural Worlds/HDRP Time Of Day/Create Empty Audio Profile")]
        public static void CreateHDRPTimeOfDayAmbientProfile()
        {
            HDRPTimeOfDayAmbientProfile asset = ScriptableObject.CreateInstance<HDRPTimeOfDayAmbientProfile>();

            AssetDatabase.CreateAsset(asset, "Assets/HDRP Time Of Day Ambient Profile.asset");
            AssetDatabase.SaveAssets();

            EditorUtility.FocusProjectWindow();

            Selection.activeObject = asset;
        }
        [MenuItem("Assets/Create/Procedural Worlds/HDRP Time Of Day/Create Empty Defaults Profile")]
        public static void CreateHDRPTimeOfDayDefaultsProfile()
        {
            HDRPDefaultsProfile asset = ScriptableObject.CreateInstance<HDRPDefaultsProfile>();

            AssetDatabase.CreateAsset(asset, "Assets/HDRP Time Of Day Defaults Profile.asset");
            AssetDatabase.SaveAssets();

            EditorUtility.FocusProjectWindow();

            Selection.activeObject = asset;
        }
        [MenuItem("Assets/Create/Procedural Worlds/HDRP Time Of Day/Create Empty Reflection Probe Profile")]
        public static void CreateHDRPTimeOfDayReflectionProbeProfile()
        {
            HDRPTimeOfDayReflectionProbeProfile asset = ScriptableObject.CreateInstance<HDRPTimeOfDayReflectionProbeProfile>();

            AssetDatabase.CreateAsset(asset, "Assets/HDRP Time Of Day Reflection Probe Profile.asset");
            AssetDatabase.SaveAssets();

            EditorUtility.FocusProjectWindow();

            Selection.activeObject = asset;
        }
        [MenuItem("Assets/Create/Procedural Worlds/HDRP Time Of Day/Create Season Profile")]
        public static void CreateHDRPTimeOfDaySeasonProfile()
        {
            HDRPTimeOfDaySeasonProfile asset = ScriptableObject.CreateInstance<HDRPTimeOfDaySeasonProfile>();

            AssetDatabase.CreateAsset(asset, "Assets/HDRP Time Of Day Season Profile.asset");
            AssetDatabase.SaveAssets();

            EditorUtility.FocusProjectWindow();

            Selection.activeObject = asset;
        }
        [MenuItem("Assets/Create/Procedural Worlds/HDRP Time Of Day/Create Preset Profile")]
        public static void CreateHDRPTimeOfDayPresetProfile()
        {
            HDRPTimeOfDayPresetProfile asset = ScriptableObject.CreateInstance<HDRPTimeOfDayPresetProfile>();

            AssetDatabase.CreateAsset(asset, "Assets/HDRP Time Of Day Preset Profile.asset");
            AssetDatabase.SaveAssets();

            EditorUtility.FocusProjectWindow();

            Selection.activeObject = asset;
        }

        #endregion
    }
}
#endif