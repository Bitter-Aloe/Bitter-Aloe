#if HDPipeline && UNITY_2021_2_OR_NEWER
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
#if TOD_CINEMACHINE
using Cinemachine;
#endif
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.VFX;

namespace ProceduralWorlds.HDRPTOD
{
    [System.Serializable]
    public class HDRPTimeOfDayComponents
    {
        //Global
        public GameObject m_componentsObject;
        //Lighting
        public GameObject m_sunRotationObject;
        public Light m_sunLight;
        public HDAdditionalLightData m_sunLightData;
        public Light m_moonLight;
        public HDAdditionalLightData m_moonLightData;
        public Light m_twilightLight;
        public HDAdditionalLightData m_twilightLightData;
        public Volume m_timeOfDayVolume;
        public VolumeProfile m_timeOfDayVolumeProfile;
        public HDRPTimeOfDayReflectionProbeManager m_reflectionProbeManager;
        public WindZone m_windZone;
        public LocalVolumetricFog m_localVolumetricFog;
        public HDRPTimeOfDayVolumeComponenets m_timeOfDayVolumeComponenets = new HDRPTimeOfDayVolumeComponenets();
        //Post FX
        public Volume m_timeOfDayPostFXVolume;
        public VolumeProfile m_timeOfDayPostFXVolumeProfile;
        public ColorAdjustments m_colorAdjustments;
        public WhiteBalance m_whiteBalance;
        public Bloom m_bloom;
        public SplitToning m_splitToning;
        public Vignette m_vignette;
#if UNITY_2022_2_OR_NEWER
        public ScreenSpaceAmbientOcclusion m_ambientOcclusion;
#else
        public AmbientOcclusion m_ambientOcclusion;
#endif
        public DepthOfField m_depthOfField;
        //Advanced
        public LensFlareComponentSRP m_sunLensFlare;
        public LensFlareComponentSRP m_moonLensFlare;
        //Camera
        public Camera m_camera;
        public HDAdditionalCameraData m_cameraData;
#if TOD_CINEMACHINE
        public List<CinemachineVirtualCamera> m_cinemachineVirtualCameras = new List<CinemachineVirtualCamera>();
#endif
        //Weather 2.0
        public Volume m_weatherBlendVolume;
        public VolumeProfile m_weatherBlendProfile;
#if UNITY_2022_2_OR_NEWER
        public HDRPTimeOfDayVolumeComponenets m_weatherVolumeComponenets = new HDRPTimeOfDayVolumeComponenets();
#endif
        //Ray tracing
        public Volume m_rayTracingVolume;
#if RAY_TRACING_ENABLED
        public GlobalIllumination m_rayTracedGlobalIllumination;
        public ScreenSpaceReflection m_rayTracedScreenSpaceReflection;
#if UNITY_2022_2_OR_NEWER
        public ScreenSpaceAmbientOcclusion m_rayTracedAmbientOcclusion;
#else
        public AmbientOcclusion m_rayTracedAmbientOcclusion;
#endif
        public RayTracingSettings m_rayTracedSettings;
        public RecursiveRendering m_rayTracedRecursiveRendering;
        public SubSurfaceScattering m_rayTracedSubSurfaceScattering;
        public ContactShadows m_rayTracedContactShadows;
        public PathTracing m_pathTracing;
        public RayTracingSettings m_rayTracingSettings;
        public Exposure m_rayTracingExposure;
#endif
        /// <summary>
        /// Validates if any componenets are missing
        /// </summary>
        /// <param name="failedObject"></param>
        /// <returns></returns>
        public bool Validated(out string failedObject)
        {
            failedObject = "Unknown";
            if (m_camera == null)
            {
                failedObject = "m_camera";
                return false;
            }
            if (m_cameraData == null)
            {
                failedObject = "m_cameraData";
                return false;
            }
            if (m_windZone == null)
            {
                failedObject = "m_windZone";
                return false;
            }
            if (m_reflectionProbeManager == null)
            {
                failedObject = "m_reflectionProbeManager";
                return false;
            }
            if (m_sunRotationObject == null)
            {
                failedObject = "m_sunRotationObject";
                return false;
            }
            if (m_sunLight == null)
            {
                failedObject = "m_sunLight";
                return false;
            }
            if (m_sunLightData == null)
            {
                failedObject = "m_sunLightData";
                return false;
            }
            if (m_moonLight == null)
            {
                failedObject = "m_moonLight";
                return false;
            }
            if (m_moonLightData == null)
            {
                failedObject = "m_moonLightData";
                return false;
            }
            if (m_twilightLight == null)
            {
                failedObject = "m_twilightLight";
                return false;
            }
            if (m_twilightLightData == null)
            {
                failedObject = "m_twilightLightData";
                return false;
            }
            if (m_localVolumetricFog == null)
            {
                failedObject = "m_localVolumetricFog";
                return false;
            }
            if (m_timeOfDayVolume == null)
            {
                failedObject = "m_timeOfDayVolume";
                return false;
            }
            if (m_timeOfDayVolumeProfile == null)
            {
                failedObject = "m_timeOfDayVolumeProfile";
                return false;
            }
            if (!m_timeOfDayVolumeComponenets.Validated(out failedObject))
            {
                return false;
            }
            if (m_timeOfDayPostFXVolume == null)
            {
                failedObject = "m_timeOfDayPostFXVolume";
                return false;
            }
            if (m_timeOfDayPostFXVolumeProfile == null)
            {
                failedObject = "m_timeOfDayPostFXVolumeProfile";
                return false;
            }
            if (m_colorAdjustments == null)
            {
                failedObject = "m_colorAdjustments";
                return false;
            }
            if (m_whiteBalance == null)
            {
                failedObject = "m_whiteBalance";
                return false;
            }
            if (m_bloom == null)
            {
                failedObject = "m_bloom";
                return false;
            }
            if (m_splitToning == null)
            {
                failedObject = "m_splitToning";
                return false;
            }
            if (m_vignette == null)
            {
                failedObject = "m_vignette";
                return false;
            }
            if (m_ambientOcclusion == null)
            {
                failedObject = "m_ambientOcclusion";
                return false;
            }

            if (m_depthOfField == null)
            {
                failedObject = "m_depthOfField";
                return false;
            }
            if (m_sunLensFlare == null)
            {
                failedObject = "m_sunLensFlare";
                return false;
            }
            if (m_moonLensFlare == null)
            {
                failedObject = "m_moonLensFlare";
                return false;
            }
            if (m_weatherBlendVolume == null)
            {
                failedObject = "m_weatherBlendVolume";
                return false;
            }
#if UNITY_2022_2_OR_NEWER
            if (m_weatherBlendProfile == null)
            {
                failedObject = "m_weatherBlendProfile";
                return false;
            }
            if (!m_weatherVolumeComponenets.Validated(out failedObject))
            {
                return false;
            }
#endif
#if RAY_TRACING_ENABLED
            if (m_rayTracingVolume == null)
            {
                failedObject = "m_rayTracingVolume";
                return false;
            }
            if (m_rayTracedGlobalIllumination == null)
            {
                failedObject = "m_rayTracedGlobalIllumination";
                return false;
            }
            if (m_rayTracedScreenSpaceReflection == null)
            {
                failedObject = "m_rayTracedScreenSpaceReflection";
                return false;
            }
            if (m_rayTracedAmbientOcclusion == null)
            {
                failedObject = "m_rayTracedAmbientOcclusion";
                return false;
            }
            if (m_rayTracedSettings == null)
            {
                failedObject = "m_rayTracedSettings";
                return false;
            }
            if (m_rayTracedRecursiveRendering == null)
            {
                failedObject = "m_rayTracedRecursiveRendering";
                return false;
            }
            if (m_rayTracedSubSurfaceScattering == null)
            {
                failedObject = "m_rayTracedSubSurfaceScattering";
                return false;
            }
            if (m_rayTracedContactShadows == null)
            {
                failedObject = "m_rayTracedContactShadows";
                return false;
            }
            if (m_pathTracing == null)
            {
                failedObject = "m_pathTracing";
                return false;
            }
            if (m_rayTracingSettings == null)
            {
                failedObject = "m_rayTracingSettings";
                return false;
            }
            if (m_rayTracingExposure == null)
            {
                failedObject = "m_rayTracingExposure";
                return false;
            }
#endif

            failedObject = null;
            return true;
        }
        /// <summary>
        /// Sets the sun/moon state based on isDay value
        /// </summary>
        /// <param name="isDay"></param>
        public void SetSunMoonState(bool isDay, bool isInInterior)
        {
            if (isInInterior)
            {
                m_sunLight.enabled = false;
                m_sunLightData.enabled = false;
                m_moonLight.enabled = false;
                m_moonLightData.enabled = false;
                return;
            }

            if (isDay)
            {
                m_sunLight.enabled = true;
                m_sunLightData.enabled = true;
                m_moonLight.enabled = false;
                m_moonLightData.enabled = false;
            }
            else
            {
                m_sunLight.enabled = false;
                m_sunLightData.enabled = false;
                m_moonLight.enabled = true;
                m_moonLightData.enabled = true;
            }
        }
    }
    [System.Serializable]
    public class HDRPTimeOfDayVolumeComponenets
    {
        public VisualEnvironment m_visualEnvironment;
        public PhysicallyBasedSky m_physicallyBasedSky;
        public CloudLayer m_cloudLayer;
        public VolumetricClouds m_volumetricClouds;
        public GlobalIllumination m_globalIllumination;
        public Fog m_fog;
        public Exposure m_exposure;
        public ScreenSpaceReflection m_screenSpaceReflection;
        public ScreenSpaceRefraction m_screenSpaceRefraction;
        public ContactShadows m_contactShadows;
        public MicroShadowing m_microShadowing;
        public IndirectLightingController m_indirectLightingController;
        public HDShadowSettings m_shadows;

        /// <summary>
        /// Validates if any componenets are missing
        /// </summary>
        /// <param name="failedObject"></param>
        /// <returns></returns>
        public bool Validated(out string failedObject)
        {
            failedObject = "Unknown";
            if (m_visualEnvironment == null)
            {
                failedObject = "m_visualEnvironment";
                return false;
            }
            if (m_physicallyBasedSky == null)
            {
                failedObject = "m_physicallyBasedSky";
                return false;
            }
            if (m_cloudLayer == null)
            {
                failedObject = "m_cloudLayer";
                return false;
            }
            if (m_volumetricClouds == null)
            {
                failedObject = "m_volumetricClouds";
                return false;
            }
            if (m_globalIllumination == null)
            {
                failedObject = "m_globalIllumination";
                return false;
            }
            if (m_fog == null)
            {
                failedObject = "m_fog";
                return false;
            }
            if (m_exposure == null)
            {
                failedObject = "m_exposure";
                return false;
            }
            if (m_screenSpaceReflection == null)
            {
                failedObject = "m_screenSpaceReflection";
                return false;
            }
            if (m_screenSpaceRefraction == null)
            {
                failedObject = "m_screenSpaceRefraction";
                return false;
            }
            if (m_contactShadows == null)
            {
                failedObject = "m_contactShadows";
                return false;
            }
            if (m_microShadowing == null)
            {
                failedObject = "m_microShadowing";
                return false;
            }
            if (m_shadows == null)
            {
                failedObject = "m_shadows";
                return false;
            }
            if (m_indirectLightingController == null)
            {
                failedObject = "m_indirectLightingController";
                return false;
            }

            failedObject = null;
            return true;
        }
        /// <summary>
        /// Assigns all the componenets from the volume profile
        /// </summary>
        /// <param name="profile"></param>
        public void AssignComponents(VolumeProfile profile)
        {
            if (profile != null)
            {
                profile.TryGet(out m_visualEnvironment);
                profile.TryGet(out m_physicallyBasedSky);
                profile.TryGet(out m_cloudLayer);
                profile.TryGet(out m_volumetricClouds);
                profile.TryGet(out m_globalIllumination);
                profile.TryGet(out m_fog);
                profile.TryGet(out m_exposure);
                profile.TryGet(out m_screenSpaceReflection);
                profile.TryGet(out m_screenSpaceRefraction);
                profile.TryGet(out m_contactShadows);
                profile.TryGet(out m_microShadowing);
                profile.TryGet(out m_indirectLightingController);
                profile.TryGet(out m_shadows);
            }
        }
    }

    [ExecuteAlways]
    public class HDRPTimeOfDay : MonoBehaviour
    {
        #region Properties

        public static HDRPTimeOfDay Instance
        {
            get { return m_instance; }
        }
        [SerializeField] private static HDRPTimeOfDay m_instance;

        public Transform Player
        {
            get { return m_player; }
            set
            {
                m_player = value;
                UpdatePlayerTransform();
            }
        }
        [SerializeField] private Transform m_player;

        public Transform VolumeOverrideTransform
        {
            get { return m_volumeOverrideTransform; }
            set
            {
                if (m_volumeOverrideTransform != value)
                {
                    m_volumeOverrideTransform = value;
                    if (m_useOverrideVolumes)
                    {
                        m_overrideVolumeData.m_isInVolue = CheckOverrideVolumes();
                    }
                }
            }
        }
        [SerializeField] private Transform m_volumeOverrideTransform;

        public HDRPTimeOfDayProfile TimeOfDayProfile
        {
            get { return m_timeOfDayProfile; }
            set
            {
                if (m_timeOfDayProfile != value)
                {
                    m_timeOfDayProfile = value;
                    ProcessTimeOfDay();
                }
            }
        }
        [SerializeField] private HDRPTimeOfDayProfile m_timeOfDayProfile;

        public bool IncrementalUpdate
        {
            get { return m_incrementalUpdate; }
            set
            {
                if (m_incrementalUpdate != value)
                {
                    m_incrementalUpdate = value;
                    m_currentIncrementalFrameCount = 0;
                }
            }
        }
        [SerializeField] private bool m_incrementalUpdate = false;

        public int IncrementalFrameCount
        {
            get { return m_incrementalFrameCount; }
            set
            {
                m_incrementalFrameCount = Mathf.Clamp(value, 1, int.MaxValue);
            }
        }
        [SerializeField] private int m_incrementalFrameCount = 25;

        public bool UsePostFX
        {
            get { return m_usePostFX; }
            set
            {
                if (m_usePostFX != value)
                {
                    m_usePostFX = value;
                    ProcessTimeOfDay();
                    SetPostProcessingActive(value);
                }
            }
        }
        [SerializeField]
        private bool m_usePostFX = true;

        public HDRPTimeOfDayPostFXProfile TimeOfDayPostFxProfile
        {
            get { return m_timeOfDayPostFxProfile; }
            set
            {
                if (m_timeOfDayPostFxProfile != value)
                {
                    m_timeOfDayPostFxProfile = value;
                    ProcessTimeOfDay();
                }
            }
        }
        [SerializeField] private HDRPTimeOfDayPostFXProfile m_timeOfDayPostFxProfile;

        public TimeOfDayDebugSettings DebugSettings
        {
            get { return m_debugSettings; }
            set
            {
                if (m_debugSettings != value)
                {
                    m_debugSettings = value;
                }
            }
        }
        [SerializeField] private TimeOfDayDebugSettings m_debugSettings = new TimeOfDayDebugSettings();

        public HDRPTimeOfDayReflectionProbeProfile ReflectionProbeProfile
        {
            get { return m_reflectionProbeProfile; }
            set
            {
                if (m_reflectionProbeProfile != value)
                {
                    m_reflectionProbeProfile = value;
                    SetReflectionProbeProfile();
                }
            }
        }
        [SerializeField] private HDRPTimeOfDayReflectionProbeProfile m_reflectionProbeProfile;

        public float TimeOfDay
        {
            get { return m_timeOfDay; }
            set
            {
                if (m_timeOfDay != value)
                {
                    if (value > 24f)
                    {
                        UpdateDate();
                    }

                    m_timeOfDay = Mathf.Clamp(value, 0f, 24f);
                    ProcessTimeOfDay();
                    ProcessLightSyncComponenets(false);
                    if (UseRayTracing)
                    {
#if HDRPTIMEOFDAY
                        if (RayTracingLightingModel == HDRPTOD.RayTracingLightingModel.PathTraced)
                        {
                            TimeOfDayProfile.RayTracingProfile.RayTracingSettings.AdvancedRTGlobalSettings.ApplyPathTracing(Components, this);
                        }
#endif
                    }

                    if (!DebugSettings.m_simulate)
                    {
                        m_savedTODTime = value;
                    }
                }
            }
        }
        [SerializeField] private float m_timeOfDay = 11f;

        public float DirectionY
        {
            get { return m_directionY; }
            set
            {
                if (m_directionY != value)
                {
                    m_directionY = Mathf.Clamp(value, 0f, 360f);
                    UpdateSunRotation(CurrentTime);
                    if (UseRayTracing)
                    {
#if HDRPTIMEOFDAY
                        if (RayTracingLightingModel == HDRPTOD.RayTracingLightingModel.PathTraced)
                        {
                            TimeOfDayProfile.RayTracingProfile.RayTracingSettings.AdvancedRTGlobalSettings.ApplyPathTracing(Components, this);
                        }
#endif
                    }
                    onTimeOfDayChanged?.Invoke(CurrentTime);
                }
            }
        }
        [SerializeField] private float m_directionY = 0f;

        public bool IsInInterior
        {
            get { return m_isInInterior; }
            set
            {
                if (m_isInInterior != value)
                {
                    m_isInInterior = value;
                    ProcessTimeOfDay();
                }
            }
        }
        [SerializeField] private bool m_isInInterior = false;

        public bool UseRayTracing
        {
            get { return m_useRayTracing; }
            set
            {
                if (m_useRayTracing != value)
                {
                    m_useRayTracing = value;
                    m_rayTracingSetup = SetupRayTracing(value);
                    HDRPTimeOfDayAPI.ValidateRayTracingEnabled();
                }
            }
        }
        [SerializeField] private bool m_useRayTracing = false;

        public bool RefreshOverrideVolume
        {
            get { return m_refreshOverrideVolume; }
            set
            {
                if (value)
                {
                    m_overrideVolumeData.m_isInVolue = CheckOverrideVolumes();
                    m_overrideVolumeData.m_transitionTime = 0f;
                    RefreshTimeOfDay();
                }
            }
        }
        [SerializeField] private bool m_refreshOverrideVolume = false;

        public bool UseOverrideVolumes
        {
            get { return m_useOverrideVolumes; }
            set
            {
                if (m_useOverrideVolumes != value)
                {
                    m_useOverrideVolumes = value;
                    if (value)
                    {
                        HDRPTimeOfDayOverrideVolumeController controller = GetComponent<HDRPTimeOfDayOverrideVolumeController>();
                        if (controller == null)
                        {
                            controller = gameObject.AddComponent<HDRPTimeOfDayOverrideVolumeController>();
                        }
                        RefreshTimeOfDay();
                        SetupAllOverrideVolumes();
                        controller.CheckState(true);
                    }
                    else
                    {
                        m_overrideVolumeData.m_transitionTime = 0f;
                        m_overrideVolumeData.m_isInVolue = false;
                        HDRPTimeOfDayOverrideVolume[] volumes = FindObjectsOfType<HDRPTimeOfDayOverrideVolume>();
                        if (volumes.Length > 0)
                        {
                            m_overrideVolumeData.m_isInVolue = false;
                            m_overrideVolumeData.m_settings = null;
                            for (int i = 0; i < volumes.Length; i++)
                            {
                                volumes[i].RemoveLocalFogVolume();
                            }
                        }
                    }
                }
            }
        }
        [SerializeField] private bool m_useOverrideVolumes = false;

        public bool AutoOrganizeOverrideVolumes
        {
            get { return m_autoOrganizeOverrideVolumes; }
            set
            {
                if (m_autoOrganizeOverrideVolumes != value)
                {
                    m_autoOrganizeOverrideVolumes = value;
                    if (value)
                    {
                        SortAllOverrideVolumes();
                    }
                }
            }
        }
        [SerializeField] private bool m_autoOrganizeOverrideVolumes = false;

        public bool UseWeatherFX
        {
            get { return m_useWeatherFX; }
            set
            {
                if (m_useWeatherFX != value)
                {
                    m_useWeatherFX = value;
                    if (value)
                    {
                        CheckCloudSettingsForWeather();
                    }
                    else
                    {
                        if (m_weatherIsActive)
                        {
                            if (m_selectedActiveWeatherProfile != -1 && m_selectedActiveWeatherProfile < WeatherProfiles.Count - 1)
                            {
                                StopCurrentWeatherVFX(IsDay, ConvertTimeOfDay(), true);
                            }
                        }
                    }
                }
            }
        }
        [SerializeField] private bool m_useWeatherFX = false;

        public List<HDRPTimeOfDayWeatherProfile> WeatherProfiles
        {
            get { return m_weatherProfiles; }
            set
            {
                if (m_weatherProfiles != value)
                {
                    m_weatherProfiles = value;
                }
            }
        }
        [SerializeField] private List<HDRPTimeOfDayWeatherProfile> m_weatherProfiles = new List<HDRPTimeOfDayWeatherProfile>();

        public bool UseAmbientAudio
        {
            get { return m_useAmbientAudio; }
            set
            {
                if (m_useAmbientAudio != value)
                {
                    m_useAmbientAudio = value;
                    SetupAmbientAudio();
                }
            }
        }
        [SerializeField] private bool m_useAmbientAudio = true;

        public HDRPTimeOfDayAmbientProfile AudioProfile
        {
            get { return m_audioProfile; }
            set
            {
                if (m_audioProfile != value)
                {
                    m_audioProfile = value;
                    SetupAmbientAudio();
                }
            }
        }
        [SerializeField] private HDRPTimeOfDayAmbientProfile m_audioProfile;

        public bool UseAmbientAudioTracker
        {
            get { return m_useAmbientAudioTracker; }
            set
            {
                if (m_useAmbientAudioTracker != value)
                {
                    m_useAmbientAudioTracker = value;
                    if (AudioProfile != null)
                    {
                        AudioProfile.UpdateObjectTracking(value, AmbientAudioTrackerGameObject);
                    }
                }
            }
        }
        [SerializeField] private bool m_useAmbientAudioTracker = false;

        public GameObject AmbientAudioTrackerGameObject
        {
            get { return m_ambientAudioTrackerGameObject; }
            set
            {
                if (m_ambientAudioTrackerGameObject != value)
                {
                    m_ambientAudioTrackerGameObject = value;
                    if (AudioProfile != null)
                    {
                        AudioProfile.UpdateObjectTracking(m_useAmbientAudioTracker, value);
                    }
                }
            }
        }
        [SerializeField] private GameObject m_ambientAudioTrackerGameObject;

        public bool EnableReflectionProbeSync
        {
            get { return m_enableReflectionProbeSync; }
            set
            {
                if (m_enableReflectionProbeSync != value)
                {
                    m_enableReflectionProbeSync = value;
                    SetReflectionProbeSystem();
                    foreach (HDRPTimeOfDayAdditionalProbe additionalProbe in m_additionalProbes)
                    {
                        if (value)
                        {
                            additionalProbe.EnableProbe();
                        }
                        else
                        {
                            additionalProbe.DisableProbe();
                        }
                    }
                }
            }
        }
        [SerializeField] private bool m_enableReflectionProbeSync = true;

        public bool UseAdditionalReflectionProbes
        {
            get { return m_useAdditionalReflectionProbes; }
            set
            {
                if (m_useAdditionalReflectionProbes != value)
                {
                    m_useAdditionalReflectionProbes = value;
                    foreach (HDRPTimeOfDayAdditionalProbe additionalProbe in m_additionalProbes)
                    {
                        if (value)
                        {
                            additionalProbe.EnableProbe();
                        }
                        else
                        {
                            additionalProbe.DisableProbe();
                        }
                    }
                }
            }
        }
        [SerializeField] private bool m_useAdditionalReflectionProbes = true;

        public ProbeRenderMode ProbeRenderMode
        {
            get { return m_probeRenderMode; }
            set
            {
                if (m_probeRenderMode != value)
                {
                    m_probeRenderMode = value;
                    SetReflectionProbeSystem();
                }
            }
        }
        [SerializeField] private ProbeRenderMode m_probeRenderMode = ProbeRenderMode.Sky;

        public float ProbeRenderDistance
        {
            get { return m_probeRenderDistance; }
            set
            {
                if (m_probeRenderDistance != value)
                {
                    m_probeRenderDistance = value;
                    SetReflectionProbeSystem();
                }
            }
        }
        [SerializeField] private float m_probeRenderDistance = 5000f;

        public HDRPTimeOfDaySeasonProfile SeasonProfile
        {
            get { return m_seasonProfile; }
            set
            {
                if (m_seasonProfile != value)
                {
                    m_seasonProfile = value;
                    UpdateSeasons();
                }
            }
        }
        [SerializeField] private HDRPTimeOfDaySeasonProfile m_seasonProfile;

        public bool UsingCinemachine
        {
            get { return m_usingCinemachine; }
            set
            {
                if (m_usingCinemachine != value)
                {
                    m_usingCinemachine = value;
                    BuildCinemachine();
                }
            }
        }
        [SerializeField] private bool m_usingCinemachine = true;

        public bool UseMaterialSync
        {
            get { return m_useMaterialSync; }
            set
            {
                if (m_useMaterialSync != value)
                {
                    m_useMaterialSync = value;
                    BuildMaterialSync(true);
                }
            }
        }
        [SerializeField] private bool m_useMaterialSync = true;

        public bool UseLightSync
        {
            get { return m_useLightSync; }
            set
            {
                if (m_useLightSync != value)
                {
                    m_useLightSync = value;
                    if (!value)
                    {
                        EnableAllLightSyncSources();
                    }

                    ProcessLightSyncComponenets(true);
                }
            }
        }
        [SerializeField] private bool m_useLightSync = true;

        public HDRPTimeOfDayPresetProfile PresetProfile
        {
            get { return m_presetProfile; }
            set
            {
                if (m_presetProfile != value)
                {
                    m_presetProfile = value;
                    ApplyPresetProfile(value);
                }
            }
        }
        [SerializeField] private HDRPTimeOfDayPresetProfile m_presetProfile;

        //Ray tracing
#if HDRPTIMEOFDAY
        public RayTracingLightingModel RayTracingLightingModel
        {
            get { return m_rayTracingLightingModel; }
            set
            {
                if (m_rayTracingLightingModel != value)
                {
                    m_rayTracingLightingModel = value;
                    HDRPTimeOfDayRayTracingUtils.ApplyRayTracingSettings(Components, TimeOfDayProfile.RayTracingProfile.RayTracingSettings, this);
                }
            }
        }
        [SerializeField] private RayTracingLightingModel m_rayTracingLightingModel = RayTracingLightingModel.RayTraced;
#endif

        #endregion
        #region Variables

        //Global
        public bool m_reflectionProbeSettings = false;
        public bool m_lightSyncSettings = false;
        public bool m_overrideVolumeSettings = false;
        public bool m_lightSourceOverride = false;
        public bool m_enableSeasons = true;
        public bool m_enableTimeOfDaySystem = false;
        public float m_timeOfDayMultiplier = 1f;
        public bool m_enableAutoWeather = true;
        public Vector2 m_randomWeatherTimer = new Vector2(120f, 400f);
        public Dictionary<int, HDRPTimeOfDayOverrideVolume> m_overrideVolumes = new Dictionary<int, HDRPTimeOfDayOverrideVolume>();
        public int m_overrideVolumeCount = -1;
        public bool m_resetWeatherShaderProperty = true;
        public bool m_instantWeatherStop = true;
        public bool m_avoidSameRandomWeather = true;
        public CameraSettings m_cameraSettings = new CameraSettings();
        public bool m_ignoreRealtimeGICheck = false;
#if HDRPTIMEOFDAY
        public List<HDRPTimeOfDayInteriorLightingManager> m_interiorLightingManagers = new List<HDRPTimeOfDayInteriorLightingManager>();
        public List<HDRPTimeOfDayLightController> m_lightControllers = new List<HDRPTimeOfDayLightController>();
#endif
        public List<HDAdditionalLightData> AdditionalLights = new List<HDAdditionalLightData>();

        [SerializeField] private float m_savedTODTime = 8f;
        [SerializeField] private List<GameObject> m_disableItems = new List<GameObject>();
        [SerializeField] private bool m_rayTracingSetup = false;
        [SerializeField] private HDRPTimeOfDayComponents Components = new HDRPTimeOfDayComponents();
        [SerializeField] private Color m_currentFogColor = Color.white;
        [SerializeField] private float m_currentLocalFogDistance = 100f;
        private int m_currentIncrementalFrameCount = 0;

        //Day/Month/Year
        public int DateDay = 20;
        public int DateMonth = 3;
        public int DateYear = 2023;

        //Auto Exposure
        private float m_autoExposureLerpTimer;
        private bool m_autoExposureLerpEnabled = false;
        private float m_autoExposureStartMinValue;
        private float m_autoExposureEndMinValue;
        private float m_autoExposureStartMaxValue;
        private float m_autoExposureEndMaxValue;
        private float m_autoExposureTransitionDuration;

        //Interior
        public InteriorControllerManagerData m_interiorControllerData = new InteriorControllerManagerData();
        private HDRPTimeOfDayInteriorController m_currentInteriorController;

        //Light Sync
        public float m_lightLODBias = 1f;
        public List<HDRPTimeOfDayLightComponent> m_lightComponenets = new List<HDRPTimeOfDayLightComponent>();
        private bool m_lastIsNightValue = false;
        private List<RTLightOptimization> m_rtLightOptimizations = new List<RTLightOptimization>();

        //Material Sync
        public List<HDRPTimeOfDayMaterialSync> m_materialSyncs = new List<HDRPTimeOfDayMaterialSync>();

        //Additional Reflection Probes
        public List<HDRPTimeOfDayAdditionalProbe> m_additionalProbes = new List<HDRPTimeOfDayAdditionalProbe>();

        //Light probe settings
        public LightProbeGeneratorSettings m_lightProbeSettings = new LightProbeGeneratorSettings();
        public List<HDRPTimeOfDayLightProbeVolume> m_lightProbeVolumes = new List<HDRPTimeOfDayLightProbeVolume>();
        public List<HDRPTimeOfDayLightProbeIncludeVolume> m_lightProbeIncludeVolumes = new List<HDRPTimeOfDayLightProbeIncludeVolume>();

        //Audio
        [SerializeField] private AudioSource m_ambientSourceA;
        [SerializeField] private AudioSource m_ambientSourceB;
        private bool m_hasBeenSetupCorrectly = false;
        private bool m_validating = false;
        private HDRPTimeOfDayOverrideVolume m_lastOverrideVolume;
        private OverrideDataInfo m_overrideVolumeData = new OverrideDataInfo();
        private bool m_audioInitilized = false;

        //Utils
        [SerializeField] private GameObject m_volumeMasterParent;
        [SerializeField] private GameObject m_volumeDayParent;
        [SerializeField] private GameObject m_volumeNightParent;

        //Weather VFX
        public List<IHDRPWeatherVFX> m_additionalWeatherVFX = new List<IHDRPWeatherVFX>();
        public IHDRPWeatherVFX m_weatherVFX;
        private bool m_weatherIsActive = false;
        private float m_currentRandomWeatherTimer;
        private float m_weatherDurationTimer;
        private float m_currentWeatherTransitionDuration;
        private int m_selectedActiveWeatherProfile = -1;
        private HDRPTimeOfDayWeatherProfile m_selectedWeatherProfileAsset;
        private int m_lastSelectedWeatherProfile = -1;
        private bool m_resetDuration = false;
        private float m_lastCloudLayerOpacity;
        private int m_currentValidateCheckerFrames = 0;
        private float m_audioBlendTimer;
        private bool m_weatherVFXStillPlaying = false;
        private bool m_resetAdditionalSystems = false;
        private bool m_isUnderwater;
        private List<VisualEffect> m_underwaterVisualEffects = new List<VisualEffect>();
        private bool m_weatherFadingOut = false;

        //Important Current Values
        [SerializeField] private float CurrentTime;
        [SerializeField] private bool IsDay;

        //Constants
        private const string ComponentsPrefabName = "Time Of Day Components.prefab";
        private const string TimeOfDayDefaultsProfileName = "Defaults Profile.asset";
        private const int ValidateCheckerFrameLimit = 500;

        #endregion
        #region Events

        /// <summary>
        /// Event used when weather is starting up
        /// </summary>
        /// <param name="progress"></param>
        public delegate void OnWeatherStart(float progress);
        public OnWeatherStart onWeatherStarting;

        /// <summary>
        /// Event used when weather effect is finishing
        /// </summary>
        /// <param name="progress"></param>
        public delegate void OnWeatherEnd(float progress);
        public OnWeatherStart onWeatherEnding;

        /// <summary>
        /// Event called when ever time of day or the direction is changed
        /// </summary>
        /// <param name="timeOfDay"></param>
        public delegate void OnTimeOfDayChanged(float timeOfDay);
        public OnTimeOfDayChanged onTimeOfDayChanged;

        /// <summary>
        /// Event called when ever time of day or the direction is changed
        /// </summary>
        /// <param name="timeOfDay"></param>
        public delegate void OnRayTracingUpdated(bool rtActive);
        public OnRayTracingUpdated onRayTracingUpdated;

        #endregion
        #region Unity Functions

        /// <summary>
        /// Called moment this script wakes up
        /// </summary>
        private void Awake()
        {
            m_instance = this;
        }
        /// <summary>
        /// Executes on enabled
        /// </summary>
        private void OnEnable()
        {
            Enable();
        }
        /// <summary>
        /// Executes on disable
        /// </summary>
        private void OnDisable()
        {
            Disable();
        }
        /// <summary>
        /// Executes every frame
        /// </summary>
        private void Update()
        {
            OnUpdate();
        }

        #endregion
        #region Public Functions

        /// <summary>
        /// Checks to see if path tracing is enabled
        /// </summary>
        /// <returns></returns>
        public bool IsPathTracing()
        {
            if (m_hasBeenSetupCorrectly)
            {
#if HDRPTIMEOFDAY
                HDRPTimeOfDayRayTracingProfile profile = TimeOfDayProfile.RayTracingProfile;
                if (profile != null)
                {
                    if (UseRayTracing && profile.IsRTActive() &&
                        RayTracingLightingModel == RayTracingLightingModel.PathTraced)
                    {
                        return true;
                    }
                }
#endif
            }

            return false;
        }
        /// <summary>
        /// Applies and assigns the volumetric cloud preset
        /// </summary>
        /// <param name="preset"></param>
#if HDRPTIMEOFDAY
        public void ApplyVolumetricCloudPreset(HDRPTimeOfDayVolumetricCloudPreset preset)
        {
            if (TimeOfDayProfile != null && m_hasBeenSetupCorrectly)
            {
                TimeOfDayProfile.TimeOfDayData.VolumetricCloudPreset = preset;
                preset.Apply(Components.m_timeOfDayVolumeComponenets.m_volumetricClouds);
            }
        }
#endif
        /// <summary>
        /// Returns the current light rotation and shadow distance that can be used for shader calulations
        /// </summary>
        /// <param name="lightRotation"></param>
        /// <param name="shadowDistance"></param>
        public bool GetTimeOfDayLightingVariables(out Vector4 lightRotation, out Color lightColor, out float shadowDistance)
        {
            lightRotation = Vector3.zero;
            lightColor = Color.white;
            shadowDistance = 0;

            if (m_hasBeenSetupCorrectly)
            {
                lightRotation = IsDay ? Components.m_sunLight.transform.rotation * Vector3.forward : Components.m_moonLight.transform.rotation * Vector3.forward;
                lightColor = IsDay ? Mathf.CorrelatedColorTemperatureToRGB(Components.m_sunLight.colorTemperature) : Mathf.CorrelatedColorTemperatureToRGB(Components.m_moonLight.colorTemperature);
                shadowDistance = Components.m_timeOfDayVolumeComponenets.m_shadows.maxShadowDistance.value;
                return true;
            }

            return false;
        }
        /// <summary>
        /// Adds a additional light this for now is used in advanced ray tracing for cinematic/film
        /// </summary>
        /// <param name="lightData"></param>
        public void AddAdditionalLight(HDAdditionalLightData lightData)
        {
            if (lightData != null)
            {
                if (!AdditionalLights.Contains(lightData))
                {
                    AdditionalLights.Add(lightData);
                }
            }
        }
        /// <summary>
        /// Removes a additional light this for now is used in advanced ray tracing for cinematic/film
        /// </summary>
        /// <param name="lightData"></param>
        public void RemoveAdditionalLight(HDAdditionalLightData lightData)
        {
            if (lightData != null)
            {
                if (AdditionalLights.Contains(lightData))
                {
                    AdditionalLights.Remove(lightData);
                }
            }
        }
        /// <summary>
        /// Adds a light controller
        /// </summary>
        /// <param name="lightController"></param>
#if HDRPTIMEOFDAY
        public void AddLightController(HDRPTimeOfDayLightController lightController)
        {
            if (!m_lightControllers.Contains(lightController))
            {
                m_lightControllers.Add(lightController);
            }
        }
        /// <summary>
        /// Removes a light controller
        /// </summary>
        /// <param name="lightController"></param>
        public void RemoveLightController(HDRPTimeOfDayLightController lightController)
        {
            if (m_lightControllers.Contains(lightController))
            {
                m_lightControllers.Remove(lightController);
            }
        }
        /// <summary>
        /// Sets a new volumetric cloud preset
        /// </summary>
        /// <param name="preset"></param>
        public void ChangeVolumetricCloudPreset(HDRPTimeOfDayVolumetricCloudPreset preset)
        {
            if (preset != null && TimeOfDayProfile != null)
            {
                TimeOfDayProfile.TimeOfDayData.VolumetricCloudPreset = preset;
                ProcessTimeOfDay();
            }
        }
#endif
        /// <summary>
        /// Refreshes the interior lighting
        /// </summary>
        /// <param name="RTXOn"></param>
        public void RefreshInteriorLighting(bool RTXOn)
        {
#if HDRPTIMEOFDAY
            foreach (HDRPTimeOfDayInteriorLightingManager interiorLighting in m_interiorLightingManagers)
            {
                if (interiorLighting == null)
                {
                    continue;
                }

                interiorLighting.Refresh();
            }
#endif
        }
        /// <summary>
        /// Refreshes the RT volume state
        /// </summary>
        public void RefreshRTVolumeState()
        {
#if HDRPTIMEOFDAY
            if (!TimeOfDayProfile.RayTracingProfile.IsRTActive() && Application.isPlaying || !TimeOfDayProfile.RayTracingProfile.RayTracingSettings.m_renderInEditMode && !Application.isPlaying)
            {
                DisableRayTraceVolume();
            }
            else
            {
                EnableRayTraceVolume();
            }
#endif
        }
        /// <summary>
        /// Gets the transform that is then used in override volume calculations
        /// </summary>
        /// <returns></returns>
        public Transform GetOverrideVolumeTransform()
        {
            Transform transformToUse = Player;
            if (VolumeOverrideTransform != null)
            {
                transformToUse = VolumeOverrideTransform;
            }

            return transformToUse;
        }
        /// <summary>
        /// Starts the exposure lerp to update the min value for smoother lighting transition
        /// </summary>
        /// <param name="components"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="duration"></param>
        public void StartExposureMinLerp(float startMin, float endMin, float startMax, float endMax, float duration, bool overrideApply = false)
        {
            if (!m_autoExposureLerpEnabled || overrideApply)
            {
                m_autoExposureLerpTimer = 0f;
                m_autoExposureStartMinValue = startMin;
                m_autoExposureStartMaxValue = startMax;
                m_autoExposureEndMinValue = endMin;
                m_autoExposureEndMaxValue = endMax;
                m_autoExposureTransitionDuration = duration;
                m_autoExposureLerpEnabled = true;
            }
        }
        /// <summary>
        /// Creates and saves a new preset
        /// </summary>
        public void SaveCurrentAsPreset()
        {
            if (PresetProfile == null)
            {
                HDRPTimeOfDayPresetManager manager = HDRPTimeOfDayPresetManager.Instance;
                if (manager != null)
                {
                    PresetProfile = manager.PresetProfile;
                }
            }

            if (PresetProfile != null)
            {
                PresetProfile.CreatePreset(this);
            }
        }
        /// <summary>
        /// Cleans up the scene map ID that can sometimes be broken or missing componenets when upgrading to HDRP project in existing scenes.
        /// The check count is how many times it's goign to seach for the object
        /// </summary>
        /// <param name="checkCount"></param>
        public void CleanupSceneMapID(int checkCount = 2, string mapIDObjectName = "SceneIDMap")
        {
            for (int i = 0; i < checkCount; i++)
            {
                GameObject sceneMapID = GameObject.Find(mapIDObjectName);
                if (sceneMapID != null)
                {
                    Debug.Log(sceneMapID.name + " was successfully removed from the scene");
                    DestroyImmediate(sceneMapID);
                }
            }   
        }
        /// <summary>
        /// Sets the sea level
        /// </summary>
        /// <param name="value"></param>
        public void SetSeaLevel(float value)
        {
            if (TimeOfDayProfile != null)
            {
                TimeOfDayProfile.UnderwaterOverrideData.m_seaLevel = value;
            }
        }
        /// <summary>
        /// Adds the light probe volume
        /// </summary>
        /// <param name="volume"></param>
        public void AddLightProbeVolume(HDRPTimeOfDayLightProbeVolume volume)
        {
            m_lightProbeVolumes.Add(volume);
        }
        /// <summary>
        /// Removes the light probe volume
        /// </summary>
        /// <param name="volume"></param>
        public void RemoveLightProbeVolume(HDRPTimeOfDayLightProbeVolume volume)
        {
            m_lightProbeVolumes.Remove(volume);
        }
        /// <summary>
        /// Adds include volume to all probe volumes
        /// </summary>
        /// <param name="volume"></param>
        public void AddLightProbeIncludeVolume(HDRPTimeOfDayLightProbeIncludeVolume volume)
        {
            m_lightProbeIncludeVolumes.Add(volume);
        }
        /// <summary>
        /// Remove include volume to all probe volumes
        /// </summary>
        /// <param name="volume"></param>
        public void RemoveLightProbeIncludeVolume(HDRPTimeOfDayLightProbeIncludeVolume volume)
        {
            m_lightProbeIncludeVolumes.Remove(volume);
        }
        /// <summary>
        /// Sets the sky ground color
        /// </summary>
        /// <param name="color"></param>
        public void SetSkyGroundColor(Color color)
        {
            if (TimeOfDayProfile != null)
            {
                TimeOfDayProfile.TimeOfDayData.m_skyboxGroundColor = color;
                SetSkySettings(TimeOfDayProfile.TimeOfDayData.m_skyboxExposure, TimeOfDayProfile.TimeOfDayData.m_skyboxGroundColor);
                RefreshTimeOfDay();
            }
        }
        /// <summary>
        /// Sets the cloud render quality
        /// Higher quality means more GPU usage
        /// </summary>
        /// <param name="cloudQuality"></param>
        public void SetCloudQuality(CloudResolutionQuality cloudQuality)
        {
            if (!m_hasBeenSetupCorrectly)
            {
                return;
            }

            //Time Of Day Volumetric/Procedural
            switch (cloudQuality)
            {
                case CloudResolutionQuality.Low:
                {
                    Components.m_timeOfDayVolumeComponenets.m_volumetricClouds.numPrimarySteps.value = 32;
                    Components.m_timeOfDayVolumeComponenets.m_volumetricClouds.numLightSteps.value = 4;
                    Components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerA.steps.value = 2;
                    Components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerB.steps.value = 2;
                    break;
                }
                case CloudResolutionQuality.Medium:
                {
                    Components.m_timeOfDayVolumeComponenets.m_volumetricClouds.numPrimarySteps.value = 64;
                    Components.m_timeOfDayVolumeComponenets.m_volumetricClouds.numLightSteps.value = 8;
                    Components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerA.steps.value = 4;
                    Components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerB.steps.value = 4;
                    break;
                }
                case CloudResolutionQuality.High:
                {
                    Components.m_timeOfDayVolumeComponenets.m_volumetricClouds.numPrimarySteps.value = 128;
                    Components.m_timeOfDayVolumeComponenets.m_volumetricClouds.numLightSteps.value = 16;
                    Components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerA.steps.value = 8;
                    Components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerB.steps.value = 8;
                    break;
                }
                case CloudResolutionQuality.VeryHigh:
                {
                    Components.m_timeOfDayVolumeComponenets.m_volumetricClouds.numPrimarySteps.value = 256;
                    Components.m_timeOfDayVolumeComponenets.m_volumetricClouds.numLightSteps.value = 24;
                    Components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerA.steps.value = 12;
                    Components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerB.steps.value = 12;
                    break;
                }
                case CloudResolutionQuality.Ultra:
                {
                    Components.m_timeOfDayVolumeComponenets.m_volumetricClouds.numPrimarySteps.value = 512;
                    Components.m_timeOfDayVolumeComponenets.m_volumetricClouds.numLightSteps.value = 24;
                    Components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerA.steps.value = 16;
                    Components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerB.steps.value = 16;
                    break;
                }
                case CloudResolutionQuality.Cinematic:
                {
                    Components.m_timeOfDayVolumeComponenets.m_volumetricClouds.numPrimarySteps.value = 1024;
                    Components.m_timeOfDayVolumeComponenets.m_volumetricClouds.numLightSteps.value = 32;
                    Components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerA.steps.value = 32;
                    Components.m_timeOfDayVolumeComponenets.m_cloudLayer.layerB.steps.value = 32;
                    break;
                }
            }
            TimeOfDayProfile.TimeOfDayData.m_cloudResolution = (CloudResolution)cloudQuality;

            //Weather Volumetric/Procedural
#if UNITY_2022_2_OR_NEWER
            switch (cloudQuality)
            {
                case CloudResolutionQuality.Low:
                {
                    Components.m_weatherVolumeComponenets.m_volumetricClouds.numPrimarySteps.value = 32;
                    Components.m_weatherVolumeComponenets.m_volumetricClouds.numLightSteps.value = 4;
                    Components.m_weatherVolumeComponenets.m_cloudLayer.layerA.steps.value = 2;
                    Components.m_weatherVolumeComponenets.m_cloudLayer.layerB.steps.value = 2;
                    break;
                }
                case CloudResolutionQuality.Medium:
                {
                    Components.m_weatherVolumeComponenets.m_volumetricClouds.numPrimarySteps.value = 64;
                    Components.m_weatherVolumeComponenets.m_volumetricClouds.numLightSteps.value = 8;
                    Components.m_weatherVolumeComponenets.m_cloudLayer.layerA.steps.value = 4;
                    Components.m_weatherVolumeComponenets.m_cloudLayer.layerB.steps.value = 4;
                    break;
                }
                case CloudResolutionQuality.High:
                {
                    Components.m_weatherVolumeComponenets.m_volumetricClouds.numPrimarySteps.value = 128;
                    Components.m_weatherVolumeComponenets.m_volumetricClouds.numLightSteps.value = 16;
                    Components.m_weatherVolumeComponenets.m_cloudLayer.layerA.steps.value = 8;
                    Components.m_weatherVolumeComponenets.m_cloudLayer.layerB.steps.value = 8;
                    break;
                }
                case CloudResolutionQuality.VeryHigh:
                {
                    Components.m_weatherVolumeComponenets.m_volumetricClouds.numPrimarySteps.value = 256;
                    Components.m_weatherVolumeComponenets.m_volumetricClouds.numLightSteps.value = 24;
                    Components.m_weatherVolumeComponenets.m_cloudLayer.layerA.steps.value = 12;
                    Components.m_weatherVolumeComponenets.m_cloudLayer.layerB.steps.value = 12;
                    break;
                }
                case CloudResolutionQuality.Ultra:
                {
                    Components.m_weatherVolumeComponenets.m_volumetricClouds.numPrimarySteps.value = 512;
                    Components.m_weatherVolumeComponenets.m_volumetricClouds.numLightSteps.value = 24;
                    Components.m_weatherVolumeComponenets.m_cloudLayer.layerA.steps.value = 16;
                    Components.m_weatherVolumeComponenets.m_cloudLayer.layerB.steps.value = 16;
                    break;
                }
                case CloudResolutionQuality.Cinematic:
                {
                    Components.m_weatherVolumeComponenets.m_volumetricClouds.numPrimarySteps.value = 1024;
                    Components.m_weatherVolumeComponenets.m_volumetricClouds.numLightSteps.value = 32;
                    Components.m_weatherVolumeComponenets.m_cloudLayer.layerA.steps.value = 32;
                    Components.m_weatherVolumeComponenets.m_cloudLayer.layerB.steps.value = 32;
                    break;
                }
            }
#endif
            TimeOfDayProfile.TimeOfDayData.m_cloudResolution = (CloudResolution)cloudQuality;
            RefreshTimeOfDay();
        }
        /// <summary>
        /// Sets the reflection probe profile to the probe manager
        /// </summary>
        public void SetReflectionProbeProfile()
        {
            if (Components.m_reflectionProbeManager != null)
            {
                Components.m_reflectionProbeManager.Profile = ReflectionProbeProfile;
                Components.m_reflectionProbeManager.Refresh(HDRPTimeOfDayAPI.RayTracingSSGIActive());
            }
        }
        /// <summary>
        /// Sets the reflection probe profile to the probe manager
        /// </summary>
        public void SetReflectionProbeProfile(HDRPTimeOfDayReflectionProbeProfile profile)
        {
            if (Components.m_reflectionProbeManager != null)
            {
                ReflectionProbeProfile = profile;
            }
        }
        /// <summary>
        /// Gets the sun and moon componenets
        /// </summary>
        /// <param name="sun"></param>
        /// <param name="moon"></param>
        public void GetMainLightSources(out Light sun, out Light moon)
        {
            if (Components == null)
            {
                sun = null;
                moon = null;
                return;
            }

            sun = Components.m_sunLight;
            moon = Components.m_moonLight;
        }
        /// <summary>
        /// Gets the main camera
        /// </summary>
        /// <returns></returns>
        public Camera GetMainCamera()
        {
            return Components.m_camera;
        }
        /// <summary>
        /// Gets the main camera
        /// </summary>
        /// <returns></returns>
        public Camera GetMainCamera(out HDAdditionalCameraData data)
        {
            data = Components.m_cameraData;
            return Components.m_camera;
        }
        /// <summary>
        /// Sets the planet position.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="yOverride"></param>
        public void SetPlanetPosition(Vector3 position, float yOverride = -6378100f)
        {
            if (Components.m_timeOfDayVolumeComponenets.m_physicallyBasedSky != null)
            {
                Vector3 newPos = position;
                newPos.y = yOverride;
                Components.m_timeOfDayVolumeComponenets.m_physicallyBasedSky.planetCenterPosition.value = newPos;
            }
        }
        /// <summary>
        /// Adds a material sync system
        /// </summary>
        /// <param name="sync"></param>
        /// <returns></returns>
        public bool AddMaterialSync(HDRPTimeOfDayMaterialSync sync)
        {
            m_materialSyncs.Add(sync);
            return IsDayTime();
        }
        /// <summary>
        /// Removes a material sync system
        /// </summary>
        /// <param name="sync"></param>
        public void RemoveMaterialSync(HDRPTimeOfDayMaterialSync sync)
        {
            m_materialSyncs.Remove(sync);
        }
        /// <summary>
        /// Checks if SSGI is enabled
        /// </summary>
        /// <returns></returns>
        public bool IsSSGIEnabled()
        {
#if HDRPTIMEOFDAY
            if (m_timeOfDayProfile.RayTracingProfile.RayTracingSettings.m_rayTraceSSGI || m_timeOfDayProfile.TimeOfDayData.m_useSSGI)
            {
                return true;
            }
            else
            {
                return false;
            }
#else
            return false;
#endif
        }
        /// <summary>
        /// Sets the wind speed
        /// </summary>
        /// <param name="value"></param>
        public void SetWindSpeed(float value)
        {
            if (Components.m_windZone != null)
            {
                Components.m_windZone.windMain = value;
            }
        }
        /// <summary>
        /// Gets the wind speed
        /// </summary>
        /// <returns></returns>
        public float GetWindSpeed()
        {
            if (Components.m_windZone != null)
            {
                return Components.m_windZone.windMain;
            }
            return 0f;
        }
        /// <summary>
        /// Sets camera custom frame override
        /// </summary>
        /// <param name="frameSettings"></param>
        /// <param name="value"></param>
        public void SetCameraFrameOverride(FrameSettingsField frameSettings, bool value)
        {
            if (m_hasBeenSetupCorrectly)
            {
                Components.m_cameraData.customRenderingSettings = true;
                Components.m_cameraData.renderingPathCustomFrameSettingsOverrideMask.mask[(uint)frameSettings] = true;
                Components.m_cameraData.renderingPathCustomFrameSettings.SetEnabled(frameSettings, value);
            }
        }
        /// <summary>
        /// Sets camera custom frame override
        /// </summary>
        /// <param name="frameSettings"></param>
        /// <param name="value"></param>
        public void SetMaterialQuality(FrameSettingsField frameSettings, int value)
        {
            if (m_hasBeenSetupCorrectly)
            {
                Components.m_cameraData.customRenderingSettings = true;
                Components.m_cameraData.renderingPathCustomFrameSettingsOverrideMask.mask[(uint)frameSettings] = true;
                switch (value)
                {
                    case 0:
                    {
                        Components.m_cameraData.renderingPathCustomFrameSettings.materialQuality = MaterialQuality.Low;
                        break;
                    }
                    case 1:
                    {
                        Components.m_cameraData.renderingPathCustomFrameSettings.materialQuality = MaterialQuality.Medium;
                        break;
                    }
                    case 2:
                    {
                        Components.m_cameraData.renderingPathCustomFrameSettings.materialQuality = MaterialQuality.High;
                        break;
                    }
                }
            }
        }
        /// <summary>
        /// Sets the light shadow quality
        /// </summary>
        /// <param name="shadowResolution"></param>
        public void SetLightShadowQuality(LightShadowResolutionQuality shadowResolution)
        {
            int shadowRes = (int)shadowResolution;
            Components.m_sunLightData.shadowResolution.level = shadowRes;
            shadowRes = -1;
            if (shadowRes < 0)
            {
                shadowRes = 0;
            }
            Components.m_moonLightData.shadowResolution.level = shadowRes;
        }
        /// <summary>
        /// Allows you to override the lod bias on the camera
        /// </summary>
        /// <param name="value"></param>
        public void SetLODBiasOverride(float value)
        {
            if (Components.m_cameraData != null)
            {
                Components.m_cameraData.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.LODBias, true);
                Components.m_cameraData.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.LODBiasMode, true);
                Components.m_cameraData.renderingPathCustomFrameSettings.lodBiasMode = LODBiasMode.ScaleQualitySettings;
                Components.m_cameraData.renderingPathCustomFrameSettings.lodBias = value;
            }
        }
        /// <summary>
        /// Applies the camera settings
        /// </summary>
        public void ApplyCameraSettings()
        {
            m_cameraSettings.ApplyCameraSettings(Components.m_cameraData, Components.m_camera);
        }
        /// <summary>
        /// Gets the bool if raytacing is setup
        /// </summary>
        /// <returns></returns>
        public bool IsRayTracingSetup()
        {
            return m_rayTracingSetup;
        }
        /// <summary>
        /// Adds the componenet to be processed by the light sync
        /// </summary>
        /// <param name="component"></param>
        public void AddLightSyncComponent(HDRPTimeOfDayLightComponent component)
        {
            m_lightComponenets.Add(component);
        }
        /// <summary>
        /// Removes the componenet from being processed by the light sync
        /// </summary>
        /// <param name="component"></param>
        public void RemoveLightSyncComponent(HDRPTimeOfDayLightComponent component)
        {
            m_lightComponenets.Remove(component);
        }
        /// <summary>
        /// Sets the current incrimental frame value this is useful when you want to override the frame value
        /// </summary>
        /// <param name="value"></param>
        public void SetCurrentIncrimentalFrameValue(int value)
        {
            m_currentIncrementalFrameCount = value;
        }
        /// <summary>
        /// Shows the debug log if it's enabled
        /// </summary>
        /// <param name="message"></param>
        /// <param name="type"></param>
        public void ShowDebugLog(string message, DebugLogType type)
        {
            if (!DebugSettings.m_showDebugLogs)
            {
                return;
            }

            switch (type)
            {
                case DebugLogType.Log:
                {
                    Debug.Log(message);
                    break;
                }
                case DebugLogType.Warning:
                {
                    Debug.LogWarning(message);
                    break;
                }
                case DebugLogType.Error:
                {
                    Debug.LogError(message);
                    break;
                }
            }
        }
        /// <summary>
        /// Gets the TOD components data block
        /// </summary>
        /// <returns></returns>
        public HDRPTimeOfDayComponents GetTODComponents()
        {
            return Components;
        }
        /// <summary>
        /// Sets the probe system to refresh this checks to see if the baked probe needs to be changed
        /// </summary>
        public void UpdateReflectionProbeSystem()
        {
            if (!m_enableReflectionProbeSync)
            {
                return;
            }

            Components.m_reflectionProbeManager.Refresh(HDRPTimeOfDayAPI.RayTracingSSGIActive());

            if (m_useAdditionalReflectionProbes)
            {
                foreach (HDRPTimeOfDayAdditionalProbe additionalProbe in m_additionalProbes)
                {
                    additionalProbe.Refresh(HDRPTimeOfDayAPI.RayTracingSSGIActive());
                }
            }
        }
        /// <summary>
        /// Updates and applies the material sync system
        /// </summary>
        public void UpdateMaterialSyncSystem()
        {
            if (UseMaterialSync)
            {
                if (m_materialSyncs.Count > 0)
                {
                    foreach (HDRPTimeOfDayMaterialSync sync in m_materialSyncs)
                    {
                        sync.Process(IsDay);
                    }
                }
            }
        }
        /// <summary>
        /// Adds the additional probe
        /// </summary>
        /// <param name="probe"></param>
        public void AddAdditionalProbe(HDRPTimeOfDayAdditionalProbe probe)
        {
            m_additionalProbes.Add(probe);
        }
        /// <summary>
        /// Removes the additional probe
        /// </summary>
        /// <param name="probe"></param>
        public void RemoveAdditionalProbe(HDRPTimeOfDayAdditionalProbe probe)
        {
            m_additionalProbes.Remove(probe);
        }
        /// <summary>
        /// Returns the bool if weather is currently active
        /// </summary>
        /// <returns></returns>
        public bool WeatherActive()
        {
            return m_weatherIsActive;
        }
        /// <summary>
        /// Returns the bool if weather is currently active
        /// Also outs the active profile asset
        /// </summary>
        /// <param name="activeWeatherProfileAsset"></param>
        /// <returns></returns>
        public bool WeatherActive(out HDRPTimeOfDayWeatherProfile activeWeatherProfileAsset)
        {
            activeWeatherProfileAsset = null;
            if (m_weatherIsActive)
            {
                activeWeatherProfileAsset = m_selectedWeatherProfileAsset;
            }

            return m_weatherIsActive;
        }
        /// <summary>
        /// Starts weather effect with the weather profile index selected
        /// </summary>
        /// <param name="weatherProfile"></param>
        public void StartWeather(int weatherProfile)
        {
            if (m_weatherIsActive)
            {
                Debug.Log("A weather system is currently active, weather blending from one to another is not yet supported. Please call StopWeather() and then start a new weather effect once the weather effect has finished.");
                return;
            }

            m_selectedActiveWeatherProfile = weatherProfile;
            m_selectedWeatherProfileAsset = m_weatherProfiles[m_selectedActiveWeatherProfile];
            m_currentRandomWeatherTimer = 0f;
            m_currentWeatherTransitionDuration = 0f;
            if (weatherProfile < 0 || weatherProfile <= m_weatherProfiles.Count - 1)
            {
                if (m_selectedWeatherProfileAsset == null)
                {
                    ShowDebugLog("Weather profile at " + weatherProfile + " is null.", DebugLogType.Error);
                    return;
                }

                //Assign cached data
#if HDRPTIMEOFDAY
                m_selectedWeatherProfileAsset.WeatherData.m_weatherData.VolumetricCloudPreset.SetCachedCloudSettings(Components.m_timeOfDayVolumeComponenets.m_volumetricClouds);
                m_selectedWeatherProfileAsset.WeatherData.m_weatherData.ProceduralCloudPreset.SetCachedCloudSettings(Components.m_timeOfDayVolumeComponenets.m_cloudLayer, TimeOfDayProfile.TimeOfDayData.ProceduralCloudPreset.LayerAChannel, TimeOfDayProfile.TimeOfDayData.ProceduralCloudPreset.LayerBChannel);
#endif

                m_weatherFadingOut = false;
                m_weatherVFXStillPlaying = true;
                m_weatherIsActive = true;
                m_weatherDurationTimer = UnityEngine.Random.Range(m_selectedWeatherProfileAsset.WeatherData.m_weatherDuration.x, m_selectedWeatherProfileAsset.WeatherData.m_weatherDuration.y);
                WeatherEffectsData data = m_selectedWeatherProfileAsset.WeatherFXData;
                m_additionalWeatherVFX.Clear();
                if (data.m_weatherEffect != null)
                {                   
                    m_weatherVFX = HDRPTimeOfDayWeatherProfile.GetInterface(Instantiate(data.m_weatherEffect));
                    data.SetupAdditionalEffectsCopy();
                    if (data.m_additionalEffectsCopy.Count > 0)
                    {
                        if (data.m_randomizeAdditionalEffects)
                        {
                            data.RandomizeEffects();
                        }

                        foreach (HDRPWeatherAdditionalEffects additionalEffect in data.m_additionalEffectsCopy)
                        {
                            if (additionalEffect.m_active)
                            {
                                if (data.ValidateAdditionalEffect(additionalEffect))
                                {
                                    GameObject additionalGameObject = Instantiate(additionalEffect.m_effect);
                                    additionalEffect.ApplyGlobalAudioEffect(additionalGameObject);
                                    IHDRPWeatherVFX additionalEffectInterface =
                                        HDRPTimeOfDayWeatherProfile.GetInterface(additionalGameObject);
                                    if (additionalEffectInterface != null)
                                    {
                                        additionalEffectInterface.StartWeatherFX(m_selectedWeatherProfileAsset);
                                        m_additionalWeatherVFX.Add(additionalEffectInterface);
                                    }
                                }
                            }
                        }
                    }
                }

                TimeOfDayProfileData todData = new TimeOfDayProfileData();
                TimeOfDayProfileData.CopySettings(todData, m_timeOfDayProfile.TimeOfDayData);
                m_selectedWeatherProfileAsset.WeatherData.SetupStartingSettings(todData, CurrentTime);
                if (m_weatherVFX != null)
                {
                    m_weatherVFX.StartWeatherFX(m_selectedWeatherProfileAsset);
                    List<VisualEffect> vfxToAdd = m_weatherVFX.CanBeControlledByUnderwater();
                    foreach (IHDRPWeatherVFX weatherVfx in m_additionalWeatherVFX)
                    {
                        vfxToAdd.AddRange(weatherVfx.CanBeControlledByUnderwater());
                    }

                    m_interiorControllerData.AssignVisualEffects(vfxToAdd);
                    if (m_interiorControllerData.m_refreshControllerSystemsOnWeatherStart)
                    {
                        SetupInteriorSystems();
                    }

                    m_underwaterVisualEffects.Clear();
                    m_underwaterVisualEffects.AddRange(vfxToAdd);
                }

                m_resetDuration = true;
            }
        }
        /// <summary>
        /// Stops the current weather effect
        /// </summary>
        public void StopWeather(bool instant = false)
        {
            m_weatherFadingOut = true;
            if (instant)
            {
                StopCurrentWeatherVFX(IsDay, CurrentTime, instant);
            }
            else
            {
                m_weatherDurationTimer = -1f;
            }
        }
        /// <summary>
        /// Gets the local fog size/scale of the volume that is applied as vector 3
        /// </summary>
        /// <returns></returns>
        public float GetLocalFogSize()
        {
            if (TimeOfDayProfile != null)
            {
                return TimeOfDayProfile.TimeOfDayData.m_localFogSize;
            }
                
            return 200f;
        }
        /// <summary>
        /// This will refresh all the override voluems and sort them according to being day or night;
        /// </summary>
        public void SortAllOverrideVolumes()
        {
            HDRPTimeOfDayOverrideVolume[] volumes = FindObjectsOfType<HDRPTimeOfDayOverrideVolume>();
            if (volumes.Length < 1)
            {
                return;
            }

            SetupOverrideVolumeOrganize();
            foreach (HDRPTimeOfDayOverrideVolume overrideVolume in volumes)
            {
                ParentOverrideVolume(overrideVolume, false);
            }
        }
        /// <summary>
        /// Parents the override volume to day or night gameobject based on it's type
        /// </summary>
        /// <param name="volume"></param>
        /// <param name="checkSetup"></param>
        public void ParentOverrideVolume(HDRPTimeOfDayOverrideVolume volume, bool checkSetup = true)
        {
            if (checkSetup)
            {
                SetupOverrideVolumeOrganize();
            }

            if (volume != null)
            {
                switch (volume.m_volumeSettings.m_volumeType)
                {
                    case OverrideTODType.Day:
                    {
                        if (m_volumeDayParent != null)
                        {
                            volume.transform.SetParent(m_volumeDayParent.transform);
                        }
                        break;
                    }
                    case OverrideTODType.Night:
                    {
                        if (m_volumeNightParent != null)
                        {
                            volume.transform.SetParent(m_volumeNightParent.transform);
                        }
                        break;
                    }
                }
            }
        }
        /// <summary>
        /// Assigns the controller into the active controllers if it's not there
        /// </summary>
        /// <param name="controller"></param>
        public void AddInteriorController(HDRPTimeOfDayInteriorController controller)
        {
            if (controller != null)
            {
                m_interiorControllerData.AddController(controller);
            }
        }
        /// <summary>
        /// Removes the controller from the active controllers if it is there
        /// </summary>
        /// <param name="controller"></param>
        public void RemoveInteriorController(HDRPTimeOfDayInteriorController controller)
        {
            if (controller != null)
            {
                m_interiorControllerData.RemoveController(controller);
            }
        }
        /// <summary>
        /// Gets the current fog color value
        /// </summary>
        /// <returns></returns>
        public Color GetCurrentFogColor()
        {
            return m_currentFogColor;
        }
        /// <summary>
        /// Gets the current local fog distance value
        /// </summary>
        /// <returns></returns>
        public float GetCurrentLocalFogDistance()
        {
            return m_currentLocalFogDistance;
        }
        /// <summary>
        /// Sets the local fog distance that is used in override volumes
        /// </summary>
        /// <param name="value"></param>
        public void SetCurrentLocalFogDistance(float value)
        {
            m_currentLocalFogDistance = value;
        }
        /// <summary>
        /// Resets the override volume blend time
        /// </summary>
        public void ResetOverrideVolumeBlendTime(bool resetIsInVolume)
        {
            m_overrideVolumeData.m_transitionTime = 0f;
            if (resetIsInVolume)
            {
                m_overrideVolumeData.m_isInVolue = false;
                m_overrideVolumeData.m_settings = null;
                m_lastOverrideVolume = null;
            }
        }
        /// <summary>
        /// Sets up time of day
        /// </summary>
        public bool SetupHDRPTimeOfDay()
        {
            m_currentRandomWeatherTimer = UnityEngine.Random.Range(m_randomWeatherTimer.x, m_randomWeatherTimer.y);

            if (m_timeOfDayProfile == null)
            {
                return false;
            }

            bool successful = BuildVolumesAndCollectComponents();
            if (successful && Components.m_weatherBlendVolume != null)
            {
                Components.m_weatherBlendVolume.weight = 0f;
            }
            UpdatePlayerTransform();
            SetupVisualEnvironment();
            return successful;
        }
        /// <summary>
        /// The function that processes the time of day
        /// </summary>
        /// <param name="hasBeenSetup"></param>
        public void ProcessTimeOfDay()
        {
            if (!m_hasBeenSetupCorrectly)
            {
                return;
            }

            if (!Application.isPlaying)
            {
                if (Components == null)
                {
                    ShowDebugLog("HDRP Time Of Day components is null.", DebugLogType.Error);
                    return;
                }
                else
                {
                    if (!Components.Validated(out string component))
                    {
                        if (!string.IsNullOrEmpty(component))
                        {
                            ShowDebugLog("HDRP Time Of Day components validate failed because " + component + " was null. We recommend you refresh the time of day componenets by going to Debug Settings Panel and pressing Refresh Time Of Day Componenets.", DebugLogType.Error);
                        }
                        else
                        {
                            ShowDebugLog("HDRP Time Of Day components validate failed.", DebugLogType.Error);
                        }

                        return;
                    }
                }
            }
            else
            {
                if (IncrementalUpdate && m_enableTimeOfDaySystem)
                {
                    if (m_currentIncrementalFrameCount >= m_incrementalFrameCount)
                    {
                        m_currentIncrementalFrameCount = 0;
                    }
                    else
                    {
                        m_currentIncrementalFrameCount++;
                        return;
                    }
                }
            }

            CurrentTime = ConvertTimeOfDay();
            //This is used to evaluate systems that can range from 0-1
            UpdateSunRotation(CurrentTime);

            IsDay = IsDayTime();
            if (!m_weatherIsActive)
            {
                //Process TOD
                UpdateAdvancedLighting(CurrentTime, IsDay, m_timeOfDayProfile.TimeOfDayData);
                UpdateSun(CurrentTime, IsDay, m_lightSourceOverride, m_timeOfDayProfile.TimeOfDayData);
                UpdateFog(CurrentTime, m_timeOfDayProfile.TimeOfDayData);
                UpdateShadows(IsDay, CurrentTime, m_timeOfDayProfile.TimeOfDayData);
                UpdateClouds(CurrentTime, m_timeOfDayProfile.TimeOfDayData);
                UpdateLensFlare(CurrentTime, m_timeOfDayProfile.TimeOfDayData, IsDay);
            }
            else
            {
                ProcessWeather();
            }

            //Process Post FX
            if (UsePostFX)
            {
                if (TimeOfDayPostFxProfile != null)
                {
                    UpdateAmbientOcclusion(CurrentTime, TimeOfDayPostFxProfile.TimeOfDayPostFXData);
                    UpdateColorGrading(CurrentTime, TimeOfDayPostFxProfile.TimeOfDayPostFXData);
                    UpdateBloom(CurrentTime, TimeOfDayPostFxProfile.TimeOfDayPostFXData);
                    UpdateShadowToning(CurrentTime, TimeOfDayPostFxProfile.TimeOfDayPostFXData);
                    UpdateVignette(CurrentTime, TimeOfDayPostFxProfile.TimeOfDayPostFXData);
                    UpdateDepthOfField(CurrentTime, TimeOfDayPostFxProfile.TimeOfDayPostFXData);
                }
            }

            if (!m_weatherIsActive)
            {
                UpdateReflectionProbeSystem();
            }

            UpdateMaterialSyncSystem();
            SetCameraFrameSettings();

            onTimeOfDayChanged?.Invoke(TimeOfDay);
        }
        /// <summary>
        /// Refreshes the ray tracing settings
        /// </summary>
        public void RefreshRayTracing()
        {
#if HDRPTIMEOFDAY
            HDRPTimeOfDayRayTracingUtils.ApplyRayTracingSettings(Components, TimeOfDayProfile.RayTracingProfile.RayTracingSettings, this);
#endif
        }
        /// <summary>
        /// Processes the active weather profile
        /// </summary>
        public void ProcessWeather(bool overrideApply = false)
        {
            if (m_selectedWeatherProfileAsset == null)
            {
                return;
            }

            float duration = m_currentWeatherTransitionDuration;
            if (m_weatherVFX != null)
            {
                m_currentWeatherTransitionDuration = m_weatherVFX.GetCurrentDuration();
            }

            Components.SetSunMoonState(IsDay, IsInInterior);
            if (m_weatherDurationTimer > 0f)
            {
                if (m_selectedWeatherProfileAsset.WeatherData.ApplyWeather(Components, IsDay, m_lightSourceOverride, CurrentTime, duration))
                {
                    m_weatherDurationTimer -= Time.deltaTime;
#if UNITY_2022_2_OR_NEWER
                    if (m_enableTimeOfDaySystem || overrideApply)
                    {
                        if (WeatherShaderManager.AllowGlobalShaderUpdates)
                        {
                            WeatherShaderManager.ApplyAllShaderValues(m_selectedWeatherProfileAsset.WeatherShaderData);
                        }
                    }
#else
                    TimeOfDayProfileData WeatherData = m_selectedWeatherProfileAsset.WeatherData.m_weatherData;
                    if (m_enableTimeOfDaySystem || overrideApply)
                    {
                        UpdateAdvancedLighting(CurrentTime, IsDay, WeatherData);
                        UpdateSun(CurrentTime, IsDay, m_lightSourceOverride, WeatherData);
                        UpdateFog(CurrentTime, WeatherData);
                        UpdateShadows(IsDay, CurrentTime, WeatherData);
                        UpdateClouds(CurrentTime, WeatherData);
                        UpdateLensFlare(CurrentTime, WeatherData, IsDay);
                        if (WeatherShaderManager.AllowGlobalShaderUpdates)
                        {
                            WeatherShaderManager.ApplyAllShaderValues(m_selectedWeatherProfileAsset.WeatherShaderData);
                        }
                    }
                    else
                    {
                        UpdateSun(CurrentTime, IsDay, m_lightSourceOverride, WeatherData);
                    }
#endif
                }

                if (!m_enableTimeOfDaySystem)
                {
                    TimeOfDayProfile.TimeOfDayData.ApplyExposure(Components, IsDay, CurrentTime, 0f, GetCurrentOverrideData());
                }

                UpdateReflectionProbeSystem();

#if UNITY_2022_2_OR_NEWER
                if (WeatherActive())
                {
                    m_currentLocalFogDistance = Mathf.Clamp(m_currentLocalFogDistance, m_selectedWeatherProfileAsset.WeatherData.m_weatherData.m_fogDensity.Evaluate(CurrentTime) / 2f, float.MaxValue);
                }
#endif
            }
            else
            {
                WeatherShaderManager.AllowGlobalShaderUpdates = false;
                StopCurrentWeatherVFX(IsDay, CurrentTime);
            }
        }
        /// <summary>
        /// This function calls update all light values on either the sun or moon depending if it's day or night time
        /// </summary>
        public void RefreshLights(bool refreshIsDayCheck = false)
        {
            if (refreshIsDayCheck)
            {
                IsDay = IsDayTime();
            }
            StartCoroutine(RefreshSunSettings());
        }
        /// <summary>
        /// Returns true if the duration value for current vfx is >= 1
        /// </summary>
        /// <returns></returns>
        public bool HasWeatherTransitionCompleted()
        {
            if (m_weatherIsActive)
            {
                return m_weatherVFX.GetCurrentDuration() >= 1f;
            }
            return false;
        }
        /// <summary>
        /// Returns true if the duration value for current vfx is >= 1
        /// Returns 10 steps where you can add effects, apply settings in steps.
        /// Example of this is used in the reflection probe system to make a transition using weather intenisty multiplier
        /// </summary>
        /// <returns></returns>
        public int HasWeatherTransitionCompletedSteps(out bool isPlayingVFX)
        {
            isPlayingVFX = m_weatherVFXStillPlaying;
            int stepID = -1;
            if (m_weatherIsActive)
            {
                float value = m_weatherVFX.GetCurrentDuration();
                if (value >= 0f && value <= 0.1f)
                {
                    stepID = 0;
                }
                else if (value >= 0.1f && value <= 0.2f)
                {
                    stepID = 1;
                }
                else if (value >= 0.2f && value <= 0.3f)
                {
                    stepID = 2;
                }
                else if (value >= 0.3f && value <= 0.4f)
                {
                    stepID = 3;
                }
                else if (value >= 0.4f && value <= 0.5f)
                {
                    stepID = 4;
                }
                else if (value >= 0.5f && value <= 0.6f)
                {
                    stepID = 5;
                }
                else if (value >= 0.6f && value <= 0.7f)
                {
                    stepID = 6;
                }
                else if (value >= 0.7f && value <= 0.8f)
                {
                    stepID = 7;
                }
                else if (value >= 0.8f && value <= 0.9f)
                {
                    stepID = 8;
                }
                else
                {
                    stepID = 9;
                }
            }

            return stepID;
        }
        /// <summary>
        /// Logs what the time value is on animation curves and gradient fields
        /// This is to help you fine tune the times of day
        /// </summary>
        public void GetDebugInformation()
        {
            float currentTime = ConvertTimeOfDay();
            if (DebugSettings.m_roundUp)
            {
                Debug.Log("Animation Curve time is ranged from 0-1 and the current value at " + TimeOfDay +
                          " Time Of Day is " + currentTime.ToString("n2"));
                Debug.Log("Gradients time is ranged from 0-100% and the current value at " + TimeOfDay +
                          " Time Of Day is " + Mathf.FloorToInt(currentTime * 100f) + "%");
            }
            else
            {
                Debug.Log("Animation Curve time is ranged from 0-1 and the current value at " + TimeOfDay +
                          " Time Of Day is " + currentTime);
                Debug.Log("Gradients time is ranged from 0-100% and the current value at " + TimeOfDay +
                          " Time Of Day is " + currentTime * 100f + "%");
            }
        }
        /// <summary>
        /// Checks to see if the components and systems have been setup correctly
        /// </summary>
        /// <returns></returns>
        public bool HasBeenSetup()
        {
            return m_hasBeenSetupCorrectly;
        }
        /// <summary>
        /// Manually set the systems has been setup with the bool value
        /// </summary>
        /// <param name="value"></param>
        public void SetHasBeenSetup(bool value)
        {
            m_hasBeenSetupCorrectly = value;
        }
        /// <summary>
        /// Refreshes time of day settings and if you set check setup to true
        /// It will process all the components to make sure everythign is setup correctly
        /// </summary>
        /// <param name="checkSetup"></param>
        public void RefreshTimeOfDay(bool checkSetup = false, bool refreshCameraSettings = true)
        {
            if (checkSetup)
            {
                m_hasBeenSetupCorrectly = SetupHDRPTimeOfDay();
            }

            if (!m_hasBeenSetupCorrectly)
            {
                return;
            }

            if (refreshCameraSettings)
            {
                ApplyCameraSettings();
            }

            ProcessTimeOfDay();
        }
        /// <summary>
        /// Checks to see if it's day time if retuns false then it's night time
        /// </summary>
        /// <param name="time"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool IsDayTime()
        {
            return CalculateHorizon();
        }
        /// <summary>
        /// Applies visual environment settings
        /// </summary>
        public void SetupVisualEnvironment()
        {
            if (Components.m_timeOfDayVolumeComponenets.m_visualEnvironment != null)
            {
                Components.m_timeOfDayVolumeComponenets.m_visualEnvironment.skyAmbientMode.value = SkyAmbientMode.Dynamic;
                if (Components.m_timeOfDayVolumeComponenets.m_volumetricClouds != null && Components.m_timeOfDayVolumeComponenets.m_cloudLayer != null)
                {
                    switch (m_timeOfDayProfile.TimeOfDayData.m_globalCloudType)
                    {
                        case GlobalCloudType.Volumetric:
                        {
                            Components.m_timeOfDayVolumeComponenets.m_visualEnvironment.cloudType.value = 0;
                            Components.m_timeOfDayVolumeComponenets.m_volumetricClouds.enable.value = true;
                            Components.m_timeOfDayVolumeComponenets.m_cloudLayer.opacity.value = 0f;
                            break;
                        }
                        case GlobalCloudType.Procedural:
                        {
                            Components.m_timeOfDayVolumeComponenets.m_visualEnvironment.cloudType.value = (int) CloudType.CloudLayer;
                            Components.m_timeOfDayVolumeComponenets.m_volumetricClouds.enable.value = false;
                            Components.m_timeOfDayVolumeComponenets.m_cloudLayer.opacity.value = m_lastCloudLayerOpacity;
                            break;
                        }
                        case GlobalCloudType.Both:
                        {
                            Components.m_timeOfDayVolumeComponenets.m_visualEnvironment.cloudType.value = (int) CloudType.CloudLayer;
                            Components.m_timeOfDayVolumeComponenets.m_volumetricClouds.enable.value = true;
                            Components.m_timeOfDayVolumeComponenets.m_cloudLayer.opacity.value = m_lastCloudLayerOpacity;
                            break;
                        }
                        case GlobalCloudType.None:
                        {
                            Components.m_timeOfDayVolumeComponenets.m_visualEnvironment.cloudType.value = 0;
                            Components.m_timeOfDayVolumeComponenets.m_volumetricClouds.enable.value = false;
                            Components.m_timeOfDayVolumeComponenets.m_cloudLayer.opacity.value = 0f;
                            break;
                        }
                    }
                }

                if (Components.m_timeOfDayVolumeComponenets.m_physicallyBasedSky != null)
                {
                    switch (m_timeOfDayProfile.TimeOfDayData.m_skyMode)
                    {
                        case TimeOfDaySkyMode.PhysicallyBased:
                        {
                            Components.m_timeOfDayVolumeComponenets.m_visualEnvironment.skyType.value = (int)SkyType.PhysicallyBased;
                            Components.m_timeOfDayVolumeComponenets.m_physicallyBasedSky.active = true;
                            break;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Sets the skybox exposure value
        /// </summary>
        /// <param name="value"></param>
        public void SetSkySettings(float value, Color color)
        {
            if (Components.m_timeOfDayVolumeComponenets.m_physicallyBasedSky != null)
            {
                Components.m_timeOfDayVolumeComponenets.m_physicallyBasedSky.exposure.value = value;
                Components.m_timeOfDayVolumeComponenets.m_physicallyBasedSky.groundTint.value = color;
            }
        }
        /// <summary>
        /// Refreshes the physically based or gradient sky to update the to the current lighting
        /// </summary>
        public void SetRefreshMode()
        {
            Components.m_timeOfDayVolumeComponenets.m_physicallyBasedSky.updateMode.value = EnvironmentUpdateMode.OnChanged;
        }
        /// <summary>
        /// Sets the static singleton instance
        /// </summary>
        /// <param name="timeOfDay"></param>
        public void SetStaticInstance(HDRPTimeOfDay timeOfDay)
        {
            m_instance = timeOfDay;
        }
        /// <summary>
        /// Adds a gameobject item that has been disabled by this system
        /// </summary>
        /// <param name="disableObject"></param>
        public bool AddDisabledItem(GameObject disableObject)
        {
            if (!m_disableItems.Contains(disableObject))
            {
                if (IsNotTODVolume(disableObject))
                {
                    m_disableItems.Add(disableObject);
                    return true;
                }
            }

            return false;
        }
        /// <summary>
        /// Re-enables all the disable objects that have been added to the system
        /// </summary>
        public void EnableAllDisabledItems()
        {
            if (m_disableItems.Count > 0)
            {
                foreach (GameObject disableItem in m_disableItems)
                {
                    if (disableItem != null)
                    {
                        disableItem.SetActive(true);
                    }
                }
            }
        }
        /// <summary>
        /// Adds an override volume
        /// </summary>
        /// <param name="id"></param>
        /// <param name="volume"></param>
        public bool AddOverrideVolume(int id, HDRPTimeOfDayOverrideVolume volume)
        {
            if (!m_overrideVolumes.ContainsKey(id))
            {
                m_overrideVolumes.Add(id, volume);
            }

            return true;
        }
        /// <summary>
        /// Removes an override volume
        /// </summary>
        /// <param name="id"></param>
        public bool RemoveOverrideVolume(int id)
        {
            if (m_overrideVolumes.ContainsKey(id))
            {
                m_overrideVolumes.Remove(id);
            }

            return false;
        }
        /// <summary>
        /// Sets up the override volumes in the scene
        /// </summary>
        public void SetupAllOverrideVolumes()
        {
            HDRPTimeOfDayOverrideVolume[] volumes = FindObjectsOfType<HDRPTimeOfDayOverrideVolume>();
            if (volumes.Length > 0)
            {
                m_overrideVolumeData.m_isInVolue = false;
                m_overrideVolumeData.m_settings = null;
                for (int i = 0; i < volumes.Length; i++)
                {
                    volumes[i].Setup(i);
                    volumes[i].SetupVolumeTypeToController();
                    volumes[i].ApplyLocalFogVolume(false);
                }
            }
        }
        /// <summary>
        /// Gets the current override data
        /// </summary>
        /// <returns></returns>
        public OverrideDataInfo GetCurrentOverrideData()
        {
            return m_overrideVolumeData;
        }
        /// <summary>
        /// Overrides the current light source settings this can be used to simulate lighting for example
        /// </summary>
        /// <param name="temperature"></param>
        /// <param name="tint"></param>
        /// <param name="intensity"></param>
        /// <param name="reset"></param>
        public void OverrideLightSource(float temperature, Color tint, float intensity, LightShadows shadows, bool reset = true, bool resetOnly = false)
        {
            if (resetOnly && reset)
            {
                m_lightSourceOverride = false;
                if (m_weatherIsActive)
                {
                    ProcessWeather();
                }
                else
                {
                    ProcessTimeOfDay();
                }
                return;
            }

            m_lightSourceOverride = true;
            bool isDay = IsDayTime();
            if (isDay)
            {
                if (Components.m_sunLightData != null)
                {
                    Components.m_sunLightData.SetColor(tint, temperature);
                    Components.m_sunLightData.SetIntensity(intensity);
                    Components.m_sunLight.shadows = shadows;
                }
            }
            else
            {
                if (Components.m_moonLightData != null)
                {
                    Components.m_moonLightData.SetColor(tint, temperature);
                    Components.m_moonLightData.SetIntensity(intensity);
                    Components.m_moonLight.shadows = shadows;
                }
            }

            if (reset)
            {
                m_lightSourceOverride = false;
                if (m_weatherIsActive)
                {
                    ProcessWeather();
                }
                else
                {
                    ProcessTimeOfDay();
                }
            }
        }
        /// <summary>
        /// Converts the time of day float from 24 = 0-1
        /// </summary>
        /// <returns></returns>
        public float ConvertTimeOfDay()
        {
            return TimeOfDay / 24f;
        }
        /// <summary>
        /// Plays an audio effect
        /// </summary>
        /// <param name="clip"></param>
        /// <param name="source"></param>
        /// <param name="oneShot"></param>
        /// <param name="volume"></param>
        public void PlaySoundFX(AudioClip clip, AudioSource source, bool oneShot, float volume)
        {
            if (source != null && clip != null)
            {
                source.clip = clip;
                source.volume = volume;
                if (oneShot)
                {
                    source.PlayOneShot(clip, volume);
                }
                else
                {
                    if (!source.isPlaying)
                    {
                        source.Play();
                    }
                }
            }
        }
        /// <summary>
        /// Stops the weather effect
        /// If instant is set to true it will disable right away
        /// </summary>
        /// <param name="weatherProfile"></param>
        /// <param name="isDay"></param>
        /// <param name="currentTime"></param>
        /// <param name="instant"></param>
        public void StopCurrentWeatherVFX(bool isDay, float currentTime, bool instant = false)
        {
            m_weatherVFXStillPlaying = false;
            if (m_resetDuration)
            {
                if (m_currentWeatherTransitionDuration >= 1f)
                {
                    m_currentWeatherTransitionDuration = 0f;
                }
                else
                {
                    m_currentWeatherTransitionDuration = 1f - m_currentWeatherTransitionDuration;
                }
                m_resetDuration = false;
                TimeOfDayProfileData todData = new TimeOfDayProfileData();
                TimeOfDayProfileData.CopySettings(todData, m_selectedWeatherProfileAsset.WeatherData.m_weatherData);
                m_timeOfDayProfile.TimeOfDayData.SetupStartingSettings(todData, ConvertTimeOfDay());
                if (m_weatherVFX != null)
                {
                    m_weatherVFX.StopWeatherFX(m_currentWeatherTransitionDuration);
                    m_weatherVFX.SetVFXPlayState(false);
                }

                if (m_additionalWeatherVFX.Count > 0)
                {
                    foreach (IHDRPWeatherVFX weatherVfx in m_additionalWeatherVFX)
                    {
                        if (weatherVfx != null)
                        {
                            weatherVfx.StopWeatherFX(m_currentWeatherTransitionDuration);
                            weatherVfx.SetVFXPlayState(false);
                        }
                    }
                }
                
            }

            //Assign cached data
#if HDRPTIMEOFDAY
            m_selectedWeatherProfileAsset.WeatherData.m_weatherData.VolumetricCloudPreset.SetCachedCloudSettings(Components.m_timeOfDayVolumeComponenets.m_volumetricClouds);
            m_selectedWeatherProfileAsset.WeatherData.m_weatherData.ProceduralCloudPreset.SetCachedCloudSettings(Components.m_timeOfDayVolumeComponenets.m_cloudLayer, TimeOfDayProfile.TimeOfDayData.ProceduralCloudPreset.LayerAChannel, TimeOfDayProfile.TimeOfDayData.ProceduralCloudPreset.LayerBChannel);
#endif

            bool destroyVFXInstant = false;
            if (instant)
            {
                if (m_weatherVFX != null)
                {
                    m_weatherVFX.SetDuration(1f);
                }

                m_currentWeatherTransitionDuration = 1f;
                destroyVFXInstant = true;
            }

            float duration = m_currentWeatherTransitionDuration;
            if (m_weatherVFX != null)
            {
                m_weatherVFX.GetCurrentDuration();
            }

            if (m_timeOfDayProfile.TimeOfDayData.ReturnFromWeather(Components, isDay, m_lightSourceOverride, currentTime, duration))
            {
                m_selectedWeatherProfileAsset.WeatherData.Reset();
                if (destroyVFXInstant)
                {
                    if (m_weatherVFX != null)
                    {
                        m_weatherVFX.DestroyInstantly();
                    }

                    if (m_additionalWeatherVFX.Count > 0)
                    {
                        foreach (IHDRPWeatherVFX vfx in m_additionalWeatherVFX)
                        {
                            if (vfx != null)
                            {
                                vfx.DestroyInstantly();
                            }
                        }
                    }
                }
                else
                {
                    if (m_weatherVFX != null)
                    {
                        m_weatherVFX.DestroyVFX();
                    }

                    if (m_additionalWeatherVFX.Count > 0)
                    {
                        foreach (IHDRPWeatherVFX vfx in m_additionalWeatherVFX)
                        {
                            if (vfx != null)
                            {
                                vfx.DestroyVFX();
                            }
                        }
                    }
                }

                m_currentRandomWeatherTimer = UnityEngine.Random.Range(m_randomWeatherTimer.x, m_randomWeatherTimer.y);
                m_weatherIsActive = false;
                if (!m_enableTimeOfDaySystem)
                {
                    ProcessTimeOfDay();
                }
            }
            else
            {
                UpdateReflectionProbeSystem();
            }
        }
        /// <summary>
        /// Fixes the camera render distance
        /// Helpful when you are using local volumetric clouds
        /// </summary>
        public void FixCameraRenderDistance()
        {
            if (m_timeOfDayProfile != null)
            {
                if (!m_timeOfDayProfile.TimeOfDayData.m_useLocalClouds)
                {
                    return;
                }
            }
            bool hasBeenSet = false;
#if UNITY_EDITOR
            SceneView sceneView = SceneView.lastActiveSceneView;
            if (sceneView != null)
            {
                if (sceneView.camera != null && sceneView.cameraSettings is { farClip: < 20000f })
                {
                    hasBeenSet = true;
                    sceneView.cameraSettings.farClip = m_cameraSettings.m_farClipPlane;
                    sceneView.camera.farClipPlane = m_cameraSettings.m_farClipPlane;
                }
            }
#endif
            if (hasBeenSet)
            {
                ShowDebugLog("Camera render distance was set to " + m_cameraSettings.m_farClipPlane + " as you are using local clouds", DebugLogType.Log);
            }
        }
        /// <summary>
        /// Refreshes the exposure
        /// </summary>
        public void RefreshExposure()
        {
            StopCoroutine(ExposureRefreshFunction());
            StartCoroutine(ExposureRefreshFunction());
        }
        /// <summary>
        /// Function used to setup the time of day system and refreshes the components by removing them and re-adding the latest
        /// </summary>
        public void RefreshTimeOfDayComponents()
        {
            if (Components.m_componentsObject != null)
            {
                DestroyImmediate(Components.m_componentsObject);
                m_hasBeenSetupCorrectly = SetupHDRPTimeOfDay();
                ProcessTimeOfDay();
            }
        }
        /// <summary>
        /// Stops the simulation to see how the TOD transition looks within the editor
        /// </summary>
        /// <param name="stop"></param>
        public void StopSimulate()
        {
            TimeOfDay = m_savedTODTime;
            DebugSettings.m_simulate = false;
        }
        /// <summary>
        /// Starts the simulation by saving the TOD value and setting to bool to true
        /// </summary>
        public void StartSimulate()
        {
            m_savedTODTime = TimeOfDay;
            DebugSettings.m_simulate = true;
        }
        /// <summary>
        /// Sets the audio transition timer if you set it to 1 there will be no blend
        /// </summary>
        /// <param name="value"></param>
        public void SetAudioTransitionTime(float value, bool audioInitilized)
        {
            m_audioBlendTimer = value;
            m_audioInitilized = audioInitilized;
        }

#endregion
        #region Private Functions

        /// <summary>
        /// OnEnable function
        /// </summary>
        private void Enable()
        {
            if (m_lightProbeVolumes.Count > 0)
            {
                m_lightProbeVolumes.Clear();
            }
            m_lightProbeVolumes.AddRange(FindObjectsOfType<HDRPTimeOfDayLightProbeVolume>());
            m_lightProbeIncludeVolumes.Clear();
            m_lightProbeIncludeVolumes.AddRange(FindObjectsOfType<HDRPTimeOfDayLightProbeIncludeVolume>());

            m_resetAdditionalSystems = false;
            m_lastIsNightValue = !m_lastIsNightValue;
            m_currentWeatherTransitionDuration = 0f;
            m_hasBeenSetupCorrectly = false;
            if (Instance == null)
            {
                m_instance = this;
            }

            HDRPTimeOfDayRayTracingUtils.CheckRayTracingQualityExists(this);
            StopSimulate();
            FixCameraRenderDistance();
            SetupAmbientAudio();
            ResetShaderSettings();
            BuildCinemachine();
            ApplyPresetProfile(PresetProfile);
            m_lightSourceOverride = false;
            m_hasBeenSetupCorrectly = SetupHDRPTimeOfDay();
            BuildMaterialSync(true);
            if (UseRayTracing)
            {
                m_rayTracingSetup = SetupRayTracing(UseRayTracing);
#if HDRPTIMEOFDAY
                HDRPTimeOfDayRayTracingUtils.ApplyRayTracingSettings(Components, TimeOfDayProfile.RayTracingProfile.RayTracingSettings, this);
#endif
                RefreshRTVolumeState();
            }
            else
            {
                DisableRayTraceVolume();
            }
            if (UseOverrideVolumes)
            {
                SetupAllOverrideVolumes();
            }

            if (m_hasBeenSetupCorrectly)
            {
                m_cameraSettings.ApplyCameraSettings(Components.m_cameraData, Components.m_camera);
                ResetStarsRotaion(m_timeOfDayProfile.TimeOfDayData);
                SetRefreshMode();
                ProcessTimeOfDay();
            }

            if (TimeOfDayPostFxProfile != null)
            {
                TimeOfDayPostFxProfile.TimeOfDayPostFXData.SetDepthOfFieldQuality(Components.m_depthOfField);
            }

            if (m_seasonProfile != null)
            {
                m_seasonProfile.Setup();
            }

            SetPlanetPosition(Vector3.zero);
            SetupInteriorSystems();
            ProcessLightSyncComponenets(true);

            m_additionalProbes.Clear();
            m_additionalProbes.AddRange(FindObjectsOfType<HDRPTimeOfDayAdditionalProbe>());

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
        /// <summary>
        /// OnDisable function
        /// </summary>
        private void Disable()
        {
            if (m_seasonProfile != null)
            {
                m_seasonProfile.Reset();
            }

#if UNITY_EDITOR
            EditorApplication.update -= EditorUpdate;
#endif

            ResetShaderSettings();
        }
        /// <summary>
        /// OnUpdate function
        /// </summary>
        private void OnUpdate()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (!m_hasBeenSetupCorrectly && !m_validating)
            {
                m_currentValidateCheckerFrames = 0;
                m_hasBeenSetupCorrectly = SetupHDRPTimeOfDay();
                StartCoroutine(ValidationChecker());
            }

            IsDay = IsDayTime();
            if (m_enableTimeOfDaySystem)
            {
                UpdateTime(IsDay, m_timeOfDayProfile.TimeOfDayData);
            }

            CurrentTime = ConvertTimeOfDay();
            if (UseAmbientAudio && m_audioProfile != null)
            {
                if (m_audioBlendTimer < 1f && m_audioInitilized)
                {
                    m_audioBlendTimer += Time.deltaTime / 3f;
                }
                else
                {
                    m_audioBlendTimer = 1f;
                }

                if (m_audioProfile.ProcessAmbientAudio(m_ambientSourceA, m_ambientSourceB, CurrentTime, m_audioBlendTimer, m_audioInitilized, this))
                {
                    m_audioBlendTimer = 0f;
                }

                m_audioInitilized = true;
            }

            if (m_weatherIsActive)
            {
                ProcessWeather();
                if (m_weatherVFX != null)
                {
                    m_currentWeatherTransitionDuration = Mathf.Clamp01(m_weatherVFX.GetCurrentDuration());
                    if (m_currentWeatherTransitionDuration < 1f)
                    {
                        if (m_currentWeatherTransitionDuration >= 0.998f)
                        {
                            m_currentWeatherTransitionDuration = 1f;
                        }

                        if (m_weatherFadingOut)
                        {
                            onWeatherEnding?.Invoke(m_currentWeatherTransitionDuration);
                        }
                        else
                        {
                            onWeatherStarting?.Invoke(m_currentWeatherTransitionDuration);
                        }
                    }
                }
                else
                {
                    m_currentWeatherTransitionDuration += Time.deltaTime / m_selectedWeatherProfileAsset.WeatherData.m_transitionDuration;
                }
            }
            else
            {
                if (UseWeatherFX)
                {
                    CheckAutoWeather();
                }

                if (UseOverrideVolumes)
                {
                    m_overrideVolumeData.m_isInVolue = CheckOverrideVolumes();
                    if (m_overrideVolumeData.m_transitionTime < 1f)
                    {
                        if (m_lastOverrideVolume != null)
                        {
                            m_overrideVolumeData.m_transitionTime += Time.deltaTime / m_lastOverrideVolume.m_volumeSettings.m_blendTime;
                        }
                        else
                        {
                            m_overrideVolumeData.m_transitionTime += Time.deltaTime / 3f;
                        }
                    }
                }
                else
                {
                    if (m_overrideVolumeData.m_transitionTime < 1f)
                    {
                        m_overrideVolumeData.m_transitionTime += Time.deltaTime / 3f;
                    }
                }

                if (m_overrideVolumeData.m_transitionTime < 1f)
                {
                    ProcessTimeOfDay();
                }
            }

            UpdateUnderwater();
            UpdateStars(IsDay, CurrentTime, m_timeOfDayProfile.TimeOfDayData);
            UpdateSeasons();
            CheckInteriorControllers();
            ProcessLightSyncOcclusion();
            LerpAutoExposureMin();
            UpdateTwilight();

            m_timeOfDayProfile.TimeOfDayData.ApplyContactShadows(Components, CurrentTime, IsDay);
            m_timeOfDayProfile.TimeOfDayData.ApplyVolumetricCloudShadows(Components, CurrentTime);
        }
        /// <summary>
        /// Updates the ambient twilight source
        /// </summary>
        private void UpdateTwilight()
        {
            TimeOfDayProfile.TimeOfDayData.ApplyTwilight(Components, CalculateTwilightHorizon(), CurrentTime, IsInInterior);
        }
        /// <summary>
        /// Updates the date
        /// This is only simple and basic
        /// </summary>
        private void UpdateDate()
        {
            DateDay++;
            if (DateDay > 28)
            {
                DateDay = 1;
                DateMonth++;
                if (DateMonth > 12)
                {
                    DateMonth = 1;
                    DateYear++;
                }
            }
        }
        /// <summary>
        /// Enables all the light sync componenets
        /// </summary>
        private void EnableAllLightSyncSources()
        {
            if (m_lightComponenets.Count > 0)
            {
                for (int i = 0; i < m_lightComponenets.Count; i++)
                {
                    m_lightComponenets[i].SetRenderState(true);
                    m_lightComponenets[i].SetLightIncludeForRT(true, false);
                }
            }
        }
        /// <summary>
        /// Coroutine used to update the exposure min value to lerp it smoothly
        /// </summary>
        /// <param name="components"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        private void LerpAutoExposureMin()
        {
            if (m_autoExposureLerpEnabled)
            {
                m_autoExposureLerpTimer += Time.deltaTime / m_autoExposureTransitionDuration;
                if (m_autoExposureLerpTimer >= 1f)
                {
                    m_autoExposureLerpTimer = 1f;
                }
                Components.m_timeOfDayVolumeComponenets.m_exposure.limitMin.value = Mathf.Lerp(m_autoExposureStartMinValue, m_autoExposureEndMinValue, m_autoExposureLerpTimer);
                Components.m_timeOfDayVolumeComponenets.m_exposure.limitMax.value = Mathf.Lerp(m_autoExposureStartMaxValue, m_autoExposureEndMaxValue, m_autoExposureLerpTimer);
                if (Components.m_timeOfDayVolumeComponenets.m_exposure.limitMin.value == m_autoExposureEndMinValue && Components.m_timeOfDayVolumeComponenets.m_exposure.limitMax.value == m_autoExposureEndMaxValue)
                {
                    m_autoExposureLerpEnabled = false;
                }
            }
        }
        /// <summary>
        /// Applies the preset profile
        /// </summary>
        /// <param name="profile"></param>
        private void ApplyPresetProfile(HDRPTimeOfDayPresetProfile profile)
        {
            if (HDRPTimeOfDayPresetManager.Instance != null && profile != null)
            {
                HDRPTimeOfDayPresetManager.Instance.PresetProfile = profile;
            }
        }
        /// <summary>
        /// Sets the camera frame settings to disable feautes that are not enabled to save performance
        /// </summary>
        private void SetCameraFrameSettings()
        {
            if (m_hasBeenSetupCorrectly)
            {
                SetCameraFrameOverride(FrameSettingsField.SSGI, m_timeOfDayProfile.TimeOfDayData.m_useSSGI);
                SetCameraFrameOverride(FrameSettingsField.SSR, m_timeOfDayProfile.TimeOfDayData.m_useSSR);
                SetCameraFrameOverride(FrameSettingsField.SubsurfaceScattering, m_timeOfDayProfile.TimeOfDayData.m_useSSS);
                SetCameraFrameOverride(FrameSettingsField.ContactShadows, m_timeOfDayProfile.TimeOfDayData.m_useContactShadows);
                SetCameraFrameOverride(FrameSettingsField.Transmission, m_timeOfDayProfile.TimeOfDayData.m_useSSS);
                SetCameraFrameOverride(FrameSettingsField.Volumetrics, m_timeOfDayProfile.TimeOfDayData.m_enableVolumetricFog);
                SetCameraFrameOverride(FrameSettingsField.AtmosphericScattering, m_timeOfDayProfile.TimeOfDayData.m_useFog);
                SetCameraFrameOverride(FrameSettingsField.SSAO, m_timeOfDayPostFxProfile.TimeOfDayPostFXData.m_useAmbientOcclusion);
            }
        }
        /// <summary>
        /// Builds the material syncs system
        /// </summary>
        /// <param name="forceUpdate"></param>
        private void BuildMaterialSync(bool forceUpdate)
        {
            m_materialSyncs.Clear();
            HDRPTimeOfDayMaterialSync[] syncs = FindObjectsOfType<HDRPTimeOfDayMaterialSync>();
            if (syncs.Length > 0)
            {
                m_materialSyncs.AddRange(syncs);
            }

            if (m_materialSyncs.Count > 0)
            {
                foreach (HDRPTimeOfDayMaterialSync sync in m_materialSyncs)
                {
                    sync.Process(IsDayTime(), forceUpdate);
                }
            }
        }
        /// <summary>
        /// Builds cinemachine componenets
        /// </summary>
        private void BuildCinemachine()
        {
#if TOD_CINEMACHINE
            CinemachineBrain cinemachineBrain = FindObjectOfType<CinemachineBrain>();
            UsingCinemachine = cinemachineBrain != null;
            if (UsingCinemachine)
            {
                CinemachineVirtualCamera[] virtualCameras = FindObjectsOfType<CinemachineVirtualCamera>();
                if (virtualCameras.Length > 0)
                {
                    Components.m_cinemachineVirtualCameras.Clear();
                    Components.m_cinemachineVirtualCameras.AddRange(virtualCameras);
                }
            }
#endif
        }
        /// <summary>
        /// Processes the light sync setup this is to turn lights on/off
        /// </summary>
        private void ProcessLightSyncComponenets(bool overrideApply)
        {
            if (m_useLightSync && m_hasBeenSetupCorrectly)
            {
                if (overrideApply)
                {
                    m_lastIsNightValue = IsDayTime();
                    foreach (HDRPTimeOfDayLightComponent component in m_lightComponenets)
                    {
                        if (component != null)
                        {
                            component.SetRenderState(!m_lastIsNightValue);
                        }
                    }
                }
                else
                {
                    bool currentValue = IsDayTime();
                    if (currentValue != m_lastIsNightValue)
                    {
                        m_lastIsNightValue = currentValue;
                        foreach (HDRPTimeOfDayLightComponent component in m_lightComponenets)
                        {
                            if (component != null)
                            {
                                component.SetRenderState(!currentValue);
                            }
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Exposure co routine that refreshes it to help when switching time of day from day to night etc.
        /// </summary>
        /// <returns></returns>
        private IEnumerator ExposureRefreshFunction()
        {
            if (!m_enableReflectionProbeSync && Components.m_timeOfDayVolumeComponenets.m_exposure != null && TimeOfDayProfile != null)
            {
                for (int i = 0; i < 10; i++)
                {
                    Components.m_timeOfDayVolumeComponenets.m_exposure.fixedExposure.value = TimeOfDayProfile.TimeOfDayData.m_generalExposure.Evaluate(m_timeOfDay) - 0.1f;
                    yield return new WaitForEndOfFrame();
                    Components.m_timeOfDayVolumeComponenets.m_exposure.fixedExposure.value = TimeOfDayProfile.TimeOfDayData.m_generalExposure.Evaluate(m_timeOfDay);
                }
            }

            StopCoroutine(ExposureRefreshFunction());
        }
        /// <summary>
        /// Sets up the interior systems for time of day
        /// </summary>
        private void SetupInteriorSystems()
        {
            m_interiorControllerData.GetAllControllers();
            m_interiorControllerData.SetupAudioReverb(Player);
            if (m_timeOfDayProfile != null)
            {
                m_interiorControllerData.SetReverbFilter(false, m_currentInteriorController, m_timeOfDayProfile.UnderwaterOverrideData, false);
            }
        }
        /// <summary>
        /// Checks the interior controller state
        /// </summary>
        private bool CheckInteriorControllers()
        {
            bool inController = m_interiorControllerData.Apply(m_interiorControllerData.CheckControllers(out bool isInMainBounds, out m_currentInteriorController), 
                isInMainBounds, m_currentInteriorController, 
                Components, 
                m_weatherIsActive, m_weatherIsActive ? m_selectedWeatherProfileAsset.UnderwaterOverrideData : m_timeOfDayProfile.UnderwaterOverrideData, 
                m_isUnderwater);
            if (inController && m_currentInteriorController != null)
            {
                //Do blending instead here
            }

            return false;
        }
        /// <summary>
        /// Checks to see if the gameobject is not the components volume for TOD
        /// </summary>
        /// <param name="volume"></param>
        /// <returns></returns>
        private bool IsNotTODVolume(GameObject volume)
        {
            if (volume.GetComponent<HDRPTimeOfDayComponentType>())
            {
                return false;
            }

            return true;
        }
        /// <summary>
        /// Updates the seasonal settings
        /// </summary>
        private void UpdateSeasons()
        {
            if (m_seasonProfile != null)
            {
                m_seasonProfile.Apply(m_enableSeasons, Application.isPlaying);
            }
        }
        /// <summary>
        /// Sets up the gameobjects that are used for parenting
        /// </summary>
        private void SetupOverrideVolumeOrganize()
        {
            if (m_volumeMasterParent == null)
            {
                m_volumeMasterParent = GameObject.Find("Override Volumes");
                if (m_volumeMasterParent == null)
                {
                    m_volumeMasterParent = new GameObject("Override Volumes");
                }
            }

            if (m_volumeDayParent == null)
            {
                m_volumeDayParent = GameObject.Find("Day Volumes");
                if (m_volumeDayParent == null)
                {
                    m_volumeDayParent = new GameObject("Day Volumes");
                }
            }

            if (m_volumeNightParent == null)
            {
                m_volumeNightParent = GameObject.Find("Night Volumes");
                if (m_volumeNightParent == null)
                {
                    m_volumeNightParent = new GameObject("Night Volumes");
                }
            }

            m_volumeDayParent.transform.SetParent(m_volumeMasterParent.transform);
            m_volumeNightParent.transform.SetParent(m_volumeMasterParent.transform);
        }
        /// <summary>
        /// Calculates if the sun is below the horizon + offset value
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="horizonMin"></param>
        /// <param name="horizonMax"></param>
        /// <returns></returns>
        private void SetReflectionProbeSystem()
        {
            if (Components.m_reflectionProbeManager.Profile != null)
            {
                if (m_enableReflectionProbeSync)
                {
                    Components.m_reflectionProbeManager.Profile.m_renderMode = ProbeRenderMode;
                    Components.m_reflectionProbeManager.RenderDistance = ProbeRenderDistance;
                }
                else
                {
                    Components.m_reflectionProbeManager.Profile.m_renderMode = ProbeRenderMode.None;
                }

                Components.m_reflectionProbeManager.Refresh(HDRPTimeOfDayAPI.RayTracingSSGIActive());
            }
        }
        /// <summary>
        /// Calculates the horizon
        /// True = sun above the horizon, False = sun below the horizon +- the offset
        /// </summary>
        /// <returns></returns>
        private bool CalculateHorizon()
        {
            if (Components.m_sunRotationObject != null && m_timeOfDayProfile.TimeOfDayData != null)
            {
                return Components.m_sunRotationObject.transform.up.y < m_timeOfDayProfile.TimeOfDayData.m_horizonOffset;
            }

            return true;
        }
        /// <summary>
        /// Calculates the horizon
        /// True = sun above the horizon, False = sun below the horizon +- the offset
        /// </summary>
        /// <returns></returns>
        private bool CalculateTwilightHorizon()
        {
            if (Components.m_sunRotationObject != null && m_timeOfDayProfile.TimeOfDayData != null)
            {
                float horizon = m_timeOfDayProfile.TimeOfDayData.m_horizonOffset;
                horizon /= 1.7f;
                return Components.m_sunRotationObject.transform.up.y < horizon;
            }

            return true;
        }
        /// <summary>
        /// Setup the ambient audio system
        /// </summary>
        private void SetupAmbientAudio()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (UseAmbientAudio)
            {
                if (m_ambientSourceA == null)
                {
                    GameObject source = new GameObject("Ambient Audio Source A");
                    m_ambientSourceA = source.AddComponent<AudioSource>();
                    m_ambientSourceA.loop = true;
                    m_ambientSourceA.volume = 0f;
                    m_ambientSourceA.playOnAwake = false;
                    m_ambientSourceA.maxDistance = 10000f;
                    m_ambientSourceA.transform.SetParent(transform);
                }
                if (m_ambientSourceB == null)
                {
                    GameObject source = new GameObject("Ambient Audio Source B");
                    m_ambientSourceB = source.AddComponent<AudioSource>();
                    m_ambientSourceB.loop = true;
                    m_ambientSourceB.volume = 0f;
                    m_ambientSourceB.playOnAwake = false;
                    m_ambientSourceB.maxDistance = 10000f;
                    m_ambientSourceB.transform.SetParent(transform);
                }
            }
            else
            {
                if (m_ambientSourceA != null)
                {
                    HDRPTimeOfDayAPI.SafeDestroy(m_ambientSourceA.gameObject);
                }
                if (m_ambientSourceB != null)
                {
                    HDRPTimeOfDayAPI.SafeDestroy(m_ambientSourceB.gameObject);
                }
            }
        }
        /// <summary>
        /// Resets the power values for weather
        /// </summary>
        private void ResetShaderSettings()
        {
            if (m_weatherProfiles.Count > 0 && m_resetWeatherShaderProperty)
            {
                foreach (HDRPTimeOfDayWeatherProfile profile in m_weatherProfiles)
                {
#if GTS_PRESENT
                    if (GTS.GTSTerrain.activeTerrains.Length > 0)
                    {
                        if (profile.WeatherData.m_weatherName.Contains("Sand"))
                        {
                            WeatherShaderManager.ResetAllWeatherPowerValues(profile.WeatherShaderData);
                        }
                    }
                    else
                    {
                        WeatherShaderManager.ResetAllWeatherPowerValues(profile.WeatherShaderData);
                    }
#else
                    WeatherShaderManager.ResetAllWeatherPowerValues(profile.WeatherShaderData);
#endif
                }
            }
        }
        /// <summary>
        /// Validation checker to check the TOD is setup correctly
        /// </summary>
        /// <returns></returns>
        private IEnumerator ValidationChecker()
        {
            while (true)
            {
                m_validating = true;
                yield return new WaitForEndOfFrame();
                m_currentValidateCheckerFrames++;
                if (m_hasBeenSetupCorrectly)
                {
                    m_validating = false;
                    StopAllCoroutines();
                }
                m_hasBeenSetupCorrectly = SetupHDRPTimeOfDay();
                if (m_currentValidateCheckerFrames >= ValidateCheckerFrameLimit || m_hasBeenSetupCorrectly)
                {
                    ProcessTimeOfDay();
                    m_validating = false;
                    StopAllCoroutines();
                }
            }
        }
        /// <summary>
        /// Editor update for scene view previewing
        /// </summary>
        private void EditorUpdate()
        {
            if (!m_hasBeenSetupCorrectly || Application.isPlaying)
            {
                return;
            }

#if UNITY_EDITOR
            if (EditorApplication.isUpdating)
            {
                return;
            }
#endif

            if (!m_resetAdditionalSystems)
            {
                m_lightComponenets.Clear();
                HDRPTimeOfDayLightComponent[] lightComponents = FindObjectsOfType<HDRPTimeOfDayLightComponent>();
                foreach (HDRPTimeOfDayLightComponent hdrpTimeOfDayLightComponent in lightComponents)
                {
                    hdrpTimeOfDayLightComponent.OnEnable();
                    hdrpTimeOfDayLightComponent.SetRenderState(!IsDayTime());
                }
                m_additionalProbes.Clear();
                HDRPTimeOfDayAdditionalProbe[] additionalProbes = FindObjectsOfType<HDRPTimeOfDayAdditionalProbe>();
                foreach (HDRPTimeOfDayAdditionalProbe additionalProbe in additionalProbes)
                {
                    additionalProbe.OnEnable();
                    additionalProbe.Refresh(HDRPTimeOfDayAPI.RayTracingSSGIActive());
                }

                m_resetAdditionalSystems = true;
            }
            if (UseOverrideVolumes)
            {
                m_overrideVolumeData.m_isInVolue = CheckOverrideVolumes();
                if (m_overrideVolumeData.m_transitionTime < 1f && Components.Validated(out string failedObject))
                {
                    if (m_lastOverrideVolume != null)
                    {
                        m_overrideVolumeData.m_transitionTime += Time.deltaTime / m_lastOverrideVolume.m_volumeSettings.m_blendTime;
                    }
                    else
                    {
                        m_overrideVolumeData.m_transitionTime += Time.deltaTime / 3f;
                    }

                    ProcessTimeOfDay();
                }
            }
            else
            {
                if (m_overrideVolumeData.m_transitionTime < 1f && Components.Validated(out string failedObject))
                {
                    m_overrideVolumeData.m_transitionTime += Time.deltaTime / 3f;
                    ProcessTimeOfDay();
                }
            }

            UpdateUnderwater();
            UpdateSeasons();
            ProcessLightSyncOcclusion();
            LerpAutoExposureMin();
        }
        /// <summary>
        /// Processes the light sync occlusion render state
        /// </summary>
        private void ProcessLightSyncOcclusion()
        {
            if (m_useLightSync)
            {
                Transform player = Player;
                if (!Application.isPlaying)
                {
#if UNITY_EDITOR
                    SceneView sceneView = SceneView.lastActiveSceneView;
                    if (sceneView != null)
                    {
                        player = sceneView.camera.transform;
                    }
#endif
                }

                m_rtLightOptimizations.Clear();
#if HDRPTIMEOFDAY
                bool rtActive = TimeOfDayProfile.RayTracingProfile.IsRTActive();
#else
                bool rtActive = false;
#endif
                foreach (HDRPTimeOfDayLightComponent lightComponent in m_lightComponenets)
                {
                    if (lightComponent == null)
                    {
                        continue;
                    }

                    RTLightOptimization rtLightOptimization = lightComponent.SetCullingState(IsDay, player, m_lightLODBias);
                    if (rtActive && rtLightOptimization != null)
                    {
                        m_rtLightOptimizations.Add(rtLightOptimization);
                    }
                }

#if HDRPTIMEOFDAY
                RTGlobalQualitySettings rtSettings = TimeOfDayProfile.RayTracingProfile.RayTracingSettings.RTGlobalQualitySettings;
                if (rtSettings.UseRTAdditionalLightOptimization && rtActive)
                {
                    HDRPTimeOfDayLightComponent.UpdateRTOptimization(m_rtLightOptimizations, rtSettings, TimeOfDayProfile.RayTracingProfile.RayTracingSettings);
                }
#endif
            }
        }
        /// <summary>
        /// Updates the sun settings by rotating the sun and then back to refresh the light
        /// </summary>
        /// <returns></returns>
        private IEnumerator RefreshSunSettings()
        {
            Light light = Components.m_sunLight;
            HDAdditionalLightData data = Components.m_sunLightData;
            if (!IsDay)
            {
                light = Components.m_moonLight;
                data = Components.m_moonLightData;
            }

            Vector3 rotation = light.transform.eulerAngles;
            rotation.z += 1f;
            light.transform.eulerAngles = rotation;
            yield return new WaitForSeconds(0.1f);
            rotation.z -= 1f;
            light.transform.eulerAngles = rotation;
            data.UpdateAllLightValues();
            StopCoroutine(RefreshSunSettings());
        }
        /// <summary>
        /// Checks the setup for use of weather
        /// </summary>
        private void CheckCloudSettingsForWeather()
        {
            if (m_timeOfDayProfile != null)
            {
                if (!m_timeOfDayProfile.TimeOfDayData.m_useLocalClouds)
                {
#if UNITY_EDITOR
                    if (EditorUtility.DisplayDialog("Weather Enabled",
                        "You have enabled weather but local volumetric clouds is disabled. We highly recommend using local clouds for a better experience. Will also need to increase your camera far clip plane view distance, would you like us to set this up for you?",
                        "Yes", "No"))
                    {
                        FixCameraRenderDistance();
                        m_timeOfDayProfile.TimeOfDayData.m_useLocalClouds = true;
                        ProcessTimeOfDay();
                    }
#endif
                }
            }
        }
        /// <summary>
        /// Checks to see if you are in an override volume
        /// </summary>
        /// <returns></returns>
        private bool CheckOverrideVolumes()
        {
            if (m_overrideVolumes.Count < 1)
            {
                if (m_lastOverrideVolume != null)
                {
                    m_overrideVolumeData.m_transitionTime = 0f;
                    m_lastOverrideVolume = null;
                }
                return false;
            }
            else
            {
                HDRPTimeOfDayOverrideVolume volume = m_overrideVolumes.Values.Last();
                if (volume != null)
                {
                    if (!volume.enabled || !volume.gameObject.activeInHierarchy)
                    {
                        return false;
                    }
                    else if (WeatherActive() && !volume.m_volumeSettings.m_volumeActiveInWeather)
                    {
                        return false;
                    }

                    if (m_lastOverrideVolume == null || volume != m_lastOverrideVolume)
                    {
                        m_lastOverrideVolume = volume;
                        m_overrideVolumeData = new OverrideDataInfo
                        {
                            m_isInVolue = volume.m_volumeSettings.IsAnyOverrideEnabled(),
                            m_transitionTime = 0f,
                            m_settings = volume.m_volumeSettings
                        };

                        return true;
                    }

                    return volume.m_volumeSettings.IsAnyOverrideEnabled();
                }

                if (m_lastOverrideVolume != null)
                {
                    m_overrideVolumeData.m_transitionTime = 0f;
                    m_lastOverrideVolume = null;
                }
                return false;
            }
        }
        /// <summary>
        /// Sets up the ray tracing components
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private bool SetupRayTracing(bool value)
        {
#if RAY_TRACING_ENABLED
            HDRPTimeOfDayComponentType[] componentTypes = gameObject.GetComponentsInChildren<HDRPTimeOfDayComponentType>();
            if (componentTypes.Length > 0)
            {
                foreach (HDRPTimeOfDayComponentType type in componentTypes)
                {
                    if (type.m_componentType == TimeOfDayComponentType.RayTracedVolume)
                    {
                        Volume volume = type.GetComponent<Volume>();
                        if (volume != null)
                        {
                            Components.m_rayTracingVolume = volume;
                            Components.m_rayTracingVolume.weight = 0f;
                            if (volume.sharedProfile != null)
                            {
                                volume.sharedProfile.TryGet(out Components.m_rayTracedGlobalIllumination);
                                volume.sharedProfile.TryGet(out Components.m_rayTracedScreenSpaceReflection);
                                volume.sharedProfile.TryGet(out Components.m_rayTracedAmbientOcclusion);
                                volume.sharedProfile.TryGet(out Components.m_rayTracedSettings);
                                volume.sharedProfile.TryGet(out Components.m_rayTracedRecursiveRendering);
                                volume.sharedProfile.TryGet(out Components.m_rayTracedSubSurfaceScattering);
                                volume.sharedProfile.TryGet(out Components.m_rayTracedContactShadows);
                                volume.sharedProfile.TryGet(out Components.m_pathTracing);
                                volume.sharedProfile.TryGet(out Components.m_rayTracingSettings);
                                volume.sharedProfile.TryGet(out Components.m_rayTracingExposure);
                            }

                            if (Components.m_rayTracingVolume == null)
                            {
                                return false;
                            }
                            if (Components.m_rayTracedGlobalIllumination == null)
                            {
                                return false;
                            }
                            if (Components.m_rayTracedScreenSpaceReflection == null)
                            {
                                return false;
                            }
                            if (Components.m_rayTracedAmbientOcclusion == null)
                            {
                                return false;
                            }
                            if (Components.m_rayTracedSettings == null)
                            {
                                return false;
                            }
                            if (Components.m_rayTracedRecursiveRendering == null)
                            {
                                return false;
                            }
                            if (Components.m_rayTracedSubSurfaceScattering == null)
                            {
                                return false;
                            }
                            if (Components.m_rayTracedContactShadows == null)
                            {
                                return false;
                            }
                            if (Components.m_pathTracing == null)
                            {
                                return false;
                            }

                            if (value)
                            {
                                Components.m_rayTracingVolume.weight = 1f;
                                return true;
                            }
                            else
                            {
                                Components.m_rayTracingVolume.weight = 0f;
                                return false;
                            }
                        }
                    }
                }
            }
            RefreshInteriorLighting(value);
#endif
            return false;
        }
        /// <summary>
        /// Disables the volume
        /// </summary>
        private void DisableRayTraceVolume()
        {
            if (Components.m_rayTracingVolume == null)
            {
                HDRPTimeOfDayComponentType[] componentTypes = gameObject.GetComponentsInChildren<HDRPTimeOfDayComponentType>();
                foreach (HDRPTimeOfDayComponentType type in componentTypes)
                {
                    if (type.m_componentType == TimeOfDayComponentType.RayTracedVolume)
                    {
                        Components.m_rayTracingVolume = type.GetVolume();
                    }
                }
            }

            if (Components.m_rayTracingVolume != null)
            {
                Components.m_rayTracingVolume.weight = 0f;
            }

            onRayTracingUpdated?.Invoke(false);
        }
        /// <summary>
        /// Disables the volume
        /// </summary>
        private void EnableRayTraceVolume()
        {
            if (Components.m_rayTracingVolume == null)
            {
                HDRPTimeOfDayComponentType[] componentTypes = gameObject.GetComponentsInChildren<HDRPTimeOfDayComponentType>();
                foreach (HDRPTimeOfDayComponentType type in componentTypes)
                {
                    if (type.m_componentType == TimeOfDayComponentType.RayTracedVolume)
                    {
                        Components.m_rayTracingVolume = type.GetVolume();
                    }
                }
            }

            if (Components.m_rayTracingVolume != null)
            {
                Components.m_rayTracingVolume.weight = 1f;
            }

            onRayTracingUpdated?.Invoke(true);
        }
        /// <summary>
        /// Updates the time of day by adding time to it if it's enabled
        /// </summary>
        /// <param name="isDay"></param>
        /// <param name="data"></param>
        private void UpdateTime(bool isDay, TimeOfDayProfileData data)
        {
            if (!Application.isPlaying || !m_enableTimeOfDaySystem)
            {
                return;
            }

            if (isDay)
            {
                TimeOfDay += (Time.deltaTime * m_timeOfDayMultiplier) / data.m_dayDuration;
            }
            else
            {
                TimeOfDay += (Time.deltaTime * m_timeOfDayMultiplier) / data.m_nightDuration;
            }
            if (TimeOfDay >= 24f)
            {
                TimeOfDay = 0f;
            }
        }
        /// <summary>
        /// Updates the sun/moon position and rotation
        /// </summary>
        /// <param name="time"></param>
        /// <param name="isDay"></param>
        /// <param name="data"></param>
        private void UpdateSun(float time, bool isDay, bool overrideSource, TimeOfDayProfileData data)
        {
            if (data.ValidateSun())
            {
                Components.SetSunMoonState(isDay, IsInInterior);

                //Apply Settings
                HDAdditionalLightData lightData = Components.m_sunLightData;
                if (!isDay)
                {
                    lightData = Components.m_moonLightData;
                }

                data.ApplySunSettings(lightData, time, isDay, overrideSource, m_overrideVolumeData);
            }
        }
        /// <summary>
        /// Sets the sun rotation
        /// </summary>
        private void UpdateSunRotation(float time)
        {
            //Set rotation
            if (!m_hasBeenSetupCorrectly)
            {
                return;
            }
            Components.m_sunRotationObject.transform.rotation = Quaternion.Euler(new Vector3(Mathf.Lerp(0f, 360f, time), DirectionY, 0f));
        }
        /// <summary>
        /// Sets the star intensity
        /// </summary>
        /// <param name="isDay"></param>
        /// <param name="time"></param>
        /// <param name="data"></param>
        private void UpdateStars(bool isDay, float time, TimeOfDayProfileData data)
        {
            if (!isDay && data.ValidateStars())
            {
                data.ApplyStarSettings(Components, time);
            }
        }
        /// <summary>
        /// Updates the underwater effects, reverb setup
        /// </summary>
        private void UpdateUnderwater()
        {
            if (m_weatherIsActive)
            {
                m_isUnderwater = m_selectedWeatherProfileAsset.UnderwaterOverrideData.ApplySettings(CurrentTime, Player, m_underwaterVisualEffects);
            }
            else
            {
                m_isUnderwater = m_timeOfDayProfile.UnderwaterOverrideData.ApplySettings(CurrentTime, Player, m_underwaterVisualEffects);
            }
        }
        /// <summary>
        /// Resets the stars rotation back to 0,0,0
        /// </summary>
        private void ResetStarsRotaion(TimeOfDayProfileData data)
        {
            if (data.m_resetStarsRotationOnEnable)
            {
                Components.m_timeOfDayVolumeComponenets.m_physicallyBasedSky.spaceRotation.value = Vector3.zero;
            }
        }
        /// <summary>
        /// Updates the fog settings
        /// </summary>
        /// <param name="time"></param>
        /// <param name="data"></param>
        private void UpdateFog(float time, TimeOfDayProfileData data)
        {
            if (data.ValidateFog())
            {
                data.ApplyFogSettings(Components, time, out m_currentFogColor, out m_currentLocalFogDistance);

                if (WeatherActive())
                {
                    m_currentLocalFogDistance = Mathf.Clamp(m_currentLocalFogDistance, data.m_fogDensity.Evaluate(time) / 2f, float.MaxValue);
                }
            }
        }
        /// <summary>
        /// Updates the shadow settings
        /// </summary>
        /// <param name="time"></param>
        /// <param name="data"></param>
        private void UpdateShadows(bool isDay, float time, TimeOfDayProfileData data)
        {
            if (data.ValidateShadows())
            {
                data.ApplyShadowSettings(Components, time, m_overrideVolumeData, isDay);
            }
        }
        /// <summary>
        /// Updates advanced lighting settings
        /// </summary>
        /// <param name="time"></param>
        /// <param name="data"></param>
        private void UpdateAdvancedLighting(float time, bool isDay, TimeOfDayProfileData data)
        {
            if (data.ValidateAdvancedLighting())
            {
#if HDRPTIMEOFDAY
                data.ApplyAdvancedLighting(Components, UseRayTracing, TimeOfDayProfile.RayTracingProfile.RayTracingSettings, isDay, time, m_overrideVolumeData);
#endif
            }
        }
        /// <summary>
        /// Updates cloud settings
        /// </summary>
        /// <param name="time"></param>
        /// <param name="data"></param>
        private void UpdateClouds(float time, TimeOfDayProfileData data)
        {
            if (data.ValidateClouds())
            {
                m_lastCloudLayerOpacity = data.ApplyCloudSettings(Components, time);
            }
        }
        /// <summary>
        /// Updates ambient occlusion Settings
        /// </summary>
        /// <param name="time"></param>
        /// <param name="data"></param>
        private void UpdateAmbientOcclusion(float time, TimeOfDayPostFXProfileData data)
        {
            if (data.ValidateAmbientOcclusion())
            {
                data.ApplyAmbientOcclusion(Components.m_ambientOcclusion, time);
            }
        }
        /// <summary>
        /// Updates color grading settings
        /// </summary>
        /// <param name="time"></param>
        /// <param name="data"></param>
        private void UpdateColorGrading(float time, TimeOfDayPostFXProfileData data)
        {
            if (data.ValidateColorGrading())
            {
                data.ApplyColorGradingSettings(Components.m_colorAdjustments, Components.m_whiteBalance, time);
            }
        }
        /// <summary>
        /// Updates bloom settings
        /// </summary>
        /// <param name="time"></param>
        /// <param name="data"></param>
        private void UpdateBloom(float time, TimeOfDayPostFXProfileData data)
        {
            if (data.ValidateBloom())
            {
                data.ApplyBloomSettings(Components.m_bloom, time);
            }
        }
        /// <summary>
        /// Updates shadow toning settings
        /// </summary>
        /// <param name="time"></param>
        /// <param name="data"></param>
        private void UpdateShadowToning(float time, TimeOfDayPostFXProfileData data)
        {
            if (data.ValidateShadowToning())
            {
                data.ApplyShadowToningSettings(Components.m_splitToning, time);
            }
        }
        /// <summary>
        /// Updates vignette settings
        /// </summary>
        /// <param name="time"></param>
        /// <param name="data"></param>
        private void UpdateVignette(float time, TimeOfDayPostFXProfileData data)
        {
            if (data.ValidateVignette())
            {
                data.ApplyVignetteSettings(Components.m_vignette, time);
            }
        }
        /// <summary>
        /// Updates depth of field settings
        /// </summary>
        /// <param name="time"></param>
        /// <param name="data"></param>
        private void UpdateDepthOfField(float time, TimeOfDayPostFXProfileData data)
        {
            if (data.ValidateDepthOfField())
            {
                data.ApplyDepthOfField(Components.m_depthOfField, time);
            }
        }
        /// <summary>
        /// Updates lens flare settings
        /// </summary>
        /// <param name="time"></param>
        /// <param name="data"></param>
        /// <param name="isDay"></param>
        private void UpdateLensFlare(float time, TimeOfDayProfileData data, bool isDay)
        {
            if (data.ValidateSunLensFlare() && data.ValidateMoonLensFlare())
            {
                data.ApplyLensFlare(Components, time, isDay);
            }
        }
        /// <summary>
        /// Sets post processing state
        /// </summary>
        /// <param name="isActive"></param>
        private void SetPostProcessingActive(bool isActive)
        {
            if (Components.m_timeOfDayPostFXVolume != null)
            {
                Components.m_timeOfDayPostFXVolume.enabled = isActive;
            }
        }
        /// <summary>
        /// Checks if a weather profile is going to be active
        /// </summary>
        private void CheckAutoWeather()
        {
            if (!m_useWeatherFX || m_weatherIsActive || !Application.isPlaying)
            {
                return;
            }

#if UNITY_2022_2_OR_NEWER
            Components.m_weatherBlendVolume.weight = 0f;
#endif
            if (m_enableAutoWeather)
            {
                m_currentRandomWeatherTimer -= Time.deltaTime;
            }

            if (m_currentRandomWeatherTimer <= 0f)
            {
                m_selectedActiveWeatherProfile = UnityEngine.Random.Range(0, m_weatherProfiles.Count - 1);
                //Try avoid same profile twice
                if (m_avoidSameRandomWeather)
                {
                    if (m_lastSelectedWeatherProfile == -1)
                    {
                        m_lastSelectedWeatherProfile = m_selectedActiveWeatherProfile;
                    }
                    else
                    {
                        if (m_selectedActiveWeatherProfile == m_lastSelectedWeatherProfile)
                        {
                            m_selectedActiveWeatherProfile++;
                            if (m_selectedActiveWeatherProfile > m_weatherProfiles.Count - 1)
                            {
                                m_selectedActiveWeatherProfile = 0;
                            }
                        }

                        m_lastSelectedWeatherProfile = m_selectedActiveWeatherProfile;
                    }
                }

                StartWeather(m_selectedActiveWeatherProfile);
            }
        }
        /// <summary>
        /// Sets up the time of day prefab and volume
        /// </summary>
        private bool BuildVolumesAndCollectComponents()
        {
            if (!gameObject.activeInHierarchy)
            {
                return false;
            }

            if (!SetupComponentsPrefab())
            {
                return false;
            }

            if (Components.Validated(out string failedObject))
            {
                return true;
            }

            if (Components.m_reflectionProbeManager == null)
            {
                Components.m_reflectionProbeManager = HDRPTimeOfDayReflectionProbeManager.Instance;
                if (Components.m_reflectionProbeManager == null)
                {
                    Components.m_reflectionProbeManager = FindObjectOfType<HDRPTimeOfDayReflectionProbeManager>();
                }
            }

            HDRPTimeOfDayComponentType[] componentType = FindObjectsOfType<HDRPTimeOfDayComponentType>();
            if (Components.m_weatherBlendVolume == null)
            {
                if (componentType.Length > 0)
                {
                    foreach (HDRPTimeOfDayComponentType type in componentType)
                    {
                        if (type.m_componentType == TimeOfDayComponentType.WeatherBlendVolume)
                        {
                            if (Components.m_weatherBlendVolume == null || Components.m_weatherBlendProfile == null)
                            {
                                Components.m_weatherBlendVolume = type.GetComponent<Volume>();
                                if (Components.m_weatherBlendVolume != null)
                                {
                                    Components.m_weatherBlendProfile = Components.m_weatherBlendVolume.sharedProfile;
                                }
                            }
                            break;
                        }
                    }
                }
            }

            if (Components.m_weatherBlendVolume == null)
            {
                return false;
            }

            if (Components.m_camera == null)
            {
                Transform cameraTransform = HDRPTimeOfDayAPI.GetCamera();
                Camera camera = null;
                HDAdditionalCameraData data = null;
                if (cameraTransform != null)
                {
                    camera = cameraTransform.GetComponent<Camera>();
                    if (camera != null)
                    {
                        data = camera.GetComponent<HDAdditionalCameraData>();
                        if (data == null)
                        {
                            data = camera.gameObject.AddComponent<HDAdditionalCameraData>();
                        }
                    }
                }

                Components.m_camera = camera;
                Components.m_cameraData = data;
            }

            if (Components.m_cameraData == null)
            {
                if (Components.m_camera != null)
                {
                    HDAdditionalCameraData data = Components.m_camera.GetComponent<HDAdditionalCameraData>();
                    if (data == null)
                    {
                        data = Components.m_camera.gameObject.AddComponent<HDAdditionalCameraData>();
                    }

                    Components.m_cameraData = data;
                }
            }

            if (componentType.Length > 0)
            {
                foreach (HDRPTimeOfDayComponentType type in componentType)
                {
                    if (type.m_componentType == TimeOfDayComponentType.PostProcessing)
                    {
                        if (Components.m_timeOfDayPostFXVolumeProfile == null || Components.m_timeOfDayPostFXVolume == null)
                        {
                            Components.m_timeOfDayPostFXVolume = type.GetComponent<Volume>();
                            if (Components.m_timeOfDayPostFXVolume != null)
                            {
                                Components.m_timeOfDayPostFXVolumeProfile = Components.m_timeOfDayPostFXVolume.sharedProfile;
                            }
                        }
                    }
                    else if (type.m_componentType == TimeOfDayComponentType.SunRotationObject)
                    {
                        Components.m_sunRotationObject = type.gameObject;
                    }
                    else if (type.m_componentType == TimeOfDayComponentType.WindZone)
                    {
                        Components.m_windZone = type.GetComponent<WindZone>();
                    }
                }
            }

            if (m_player == null || !m_player.gameObject.activeSelf)
            {
                m_player = HDRPTimeOfDayAPI.GetCamera();
            }

            if (m_usePostFX && Components.m_timeOfDayPostFXVolume != null)
            {
                Components.m_timeOfDayPostFXVolume.isGlobal = true;
                Components.m_timeOfDayPostFXVolume.priority = 50;
                if (TimeOfDayPostFxProfile == null)
                {
                    Components.m_timeOfDayPostFXVolume.weight = 0f;
                }
                else
                {
                    Components.m_timeOfDayPostFXVolume.weight = 1f;
                }
            }

            if (UsePostFX)
            {
                if (Components.m_timeOfDayPostFXVolumeProfile == null)
                {
                    return false;
                }

                if (!Components.m_timeOfDayPostFXVolumeProfile.TryGet(out Components.m_colorAdjustments))
                {
                    return false;
                }
                if (!Components.m_timeOfDayPostFXVolumeProfile.TryGet(out Components.m_whiteBalance))
                {
                    return false;
                }
                if (!Components.m_timeOfDayPostFXVolumeProfile.TryGet(out Components.m_bloom))
                {
                    return false;
                }
                if (!Components.m_timeOfDayPostFXVolumeProfile.TryGet(out Components.m_splitToning))
                {
                    return false;
                }
                if (!Components.m_timeOfDayPostFXVolumeProfile.TryGet(out Components.m_vignette))
                {
                    return false;
                }
                if (!Components.m_timeOfDayPostFXVolumeProfile.TryGet(out Components.m_ambientOcclusion))
                {
                    return false;
                }
                if (!Components.m_timeOfDayPostFXVolumeProfile.TryGet(out Components.m_depthOfField))
                {
                    return false;
                }
            }

            if (Components.m_timeOfDayVolume == null)
            {
                return false;
            }

            Components.m_timeOfDayVolume.isGlobal = true;
            Components.m_timeOfDayVolume.priority = 50;
            Components.m_timeOfDayVolumeProfile = Components.m_timeOfDayVolume.sharedProfile;

            if (Components.m_timeOfDayVolumeProfile == null)
            {
                return false;
            }
            else
            {
                Components.m_timeOfDayVolumeComponenets.AssignComponents(Components.m_timeOfDayVolumeProfile);
            }

#if UNITY_2022_2_OR_NEWER
            if (Components.m_weatherBlendProfile == null)
            {
                return false;
            }
            else
            {
                Components.m_weatherVolumeComponenets.AssignComponents(Components.m_weatherBlendProfile);
            }
#endif

            Components.m_localVolumetricFog = Components.m_timeOfDayVolume.gameObject.GetComponentInChildren<LocalVolumetricFog>();
            if (Components.m_localVolumetricFog == null)
            {
                return false;
            }

            Light[] lights = Components.m_timeOfDayVolume.gameObject.GetComponentsInChildren<Light>();
            if (lights.Length < 1)
            {
                ShowDebugLog("Sun and moon light could not be found", DebugLogType.Error);
                return false;
            }
            else
            {
                foreach (Light light in lights)
                {
                    HDRPTimeOfDayComponentType lightType = light.GetComponent<HDRPTimeOfDayComponentType>();
                    if (lightType != null)
                    {
                        if (lightType.m_componentType == TimeOfDayComponentType.Sun)
                        {
                            Components.m_sunLight = light;
                        }
                        else if (lightType.m_componentType == TimeOfDayComponentType.Moon)
                        {
                            Components.m_moonLight = light;
                        }
                        else if (lightType.m_componentType == TimeOfDayComponentType.TwilightLight)
                        {
                            Components.m_twilightLight = light;
                        }
                    }
                }
            }

            if (Components.m_sunLight == null)
            {
                return false;
            }
            else
            {
                if (Components.m_sunLightData == null)
                {
                    Components.m_sunLightData = Components.m_sunLight.GetComponent<HDAdditionalLightData>();
                    if (Components.m_sunLightData == null)
                    {
                        Components.m_sunLightData = Components.m_sunLight.gameObject.AddComponent<HDAdditionalLightData>();
                    }
                }
            }
            if (Components.m_moonLight == null)
            {
                return false;
            }
            else
            {
                if (Components.m_moonLightData == null)
                {
                    Components.m_moonLightData = Components.m_moonLight.GetComponent<HDAdditionalLightData>();
                    if (Components.m_moonLightData == null)
                    {
                        Components.m_moonLightData = Components.m_moonLight.gameObject.AddComponent<HDAdditionalLightData>();
                    }
                }
            }
            if (Components.m_twilightLight == null)
            {
                return false;
            }
            else
            {
                if (Components.m_twilightLightData == null)
                {
                    Components.m_twilightLightData = Components.m_twilightLight.GetComponent<HDAdditionalLightData>();
                    if (Components.m_twilightLightData == null)
                    {
                        Components.m_twilightLightData = Components.m_twilightLight.gameObject.AddComponent<HDAdditionalLightData>();
                    }
                }
            }

            Light[] directionalLights = FindObjectsOfType<Light>();
            if (directionalLights.Length > 0)
            {
                foreach (Light directionalLight in directionalLights)
                {
                    if (directionalLight.type == LightType.Directional)
                    {
                        if (directionalLight != Components.m_sunLight && directionalLight != Components.m_moonLight && directionalLight != Components.m_twilightLight)
                        {
                            if (directionalLight.enabled)
                            {
                                if (AddDisabledItem(directionalLight.gameObject))
                                {
                                    directionalLight.gameObject.SetActive(false);
                                }
                                Debug.Log(directionalLight.name + " was disabled as it conflicts with HDRP Time Of Day. If you ever remove time of day system you can just re-enable this light source.");
                            }
                        }
                    }
                }
            }

            //Sun lens flare
            Components.m_sunLensFlare = Components.m_sunLight.GetComponent<LensFlareComponentSRP>();
            if (Components.m_sunLensFlare == null)
            {
                Components.m_sunLensFlare = Components.m_sunLight.gameObject.AddComponent<LensFlareComponentSRP>();
            }
            if (Components.m_sunLensFlare == null)
            {
                return false;
            }

            //Moon lens flare
            Components.m_moonLensFlare = Components.m_moonLight.GetComponent<LensFlareComponentSRP>();
            if (Components.m_moonLensFlare == null)
            {
                Components.m_moonLensFlare = Components.m_moonLight.gameObject.AddComponent<LensFlareComponentSRP>();
            }
            if (Components.m_moonLensFlare == null)
            {
                return false;
            }

            if (Components.m_sunRotationObject == null)
            {
                return false;
            }

            SetSkySettings(m_timeOfDayProfile.TimeOfDayData.m_skyboxExposure, m_timeOfDayProfile.TimeOfDayData.m_skyboxGroundColor);
            SetupRayTracing(UseRayTracing);

            return true;
        }
        /// <summary>
        /// Sets up the component prefab prefab and spawns if it's null/empty 
        /// </summary>
        /// <returns></returns>
        private bool SetupComponentsPrefab()
        {
            if (Components.m_timeOfDayVolume == null || Components.m_componentsObject == null)
            {
                GameObject timeOfDayVolume = GameObject.Find("Time Of Day Components");
                if (timeOfDayVolume == null)
                {
#if UNITY_EDITOR
                    GameObject componentsPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(GetAssetPath(ComponentsPrefabName));
                    if (componentsPrefab == null)
                    {
                        Debug.LogError("Time Of Day Components Prefab is missing and could not be found in the project. It's normally found in 'HDRP Time Of Day/Resources' folder");
                        return false;
                    }

                    timeOfDayVolume = Instantiate(componentsPrefab);
                    timeOfDayVolume.name = "Time Of Day Components";
#endif
                }

                if (timeOfDayVolume.transform.parent != transform)
                {
                    timeOfDayVolume.transform.SetParent(transform);
                }

                Components.m_timeOfDayVolume = timeOfDayVolume.GetComponent<Volume>();
                Components.m_componentsObject = timeOfDayVolume;
            }

            return true;
        }
        /// <summary>
        /// Sets the new player transform
        /// </summary>
        private void UpdatePlayerTransform()
        {
            if (Player == null)
            {
                return;
            }

            HDRPTimeOfDayTransformTracker[] tackers = FindObjectsOfType<HDRPTimeOfDayTransformTracker>();
            if (tackers.Length > 0)
            {
                foreach (HDRPTimeOfDayTransformTracker tracker in tackers)
                {
                    tracker.SetNewPlayer(Player);
                }
            }
        }

        #endregion
        #region Public Static Functions

        /// <summary>
        /// Adds time of day to the scene
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="selection"></param>
        public static void CreateTimeOfDay(GameObject parent, bool selection = true, bool removeExisting = false)
        {
            if (removeExisting)
            {
                RemoveTimeOfDay(true);
            }

            HDRPTimeOfDay timeOfDay = Instance;
            if (timeOfDay == null)
            {
                GameObject timeOfDayGameObject = new GameObject("HDRP Time Of Day");
                timeOfDay = timeOfDayGameObject.AddComponent<HDRPTimeOfDay>();
               
#if UNITY_EDITOR
                HDRPDefaultsProfile defaults = null;
                defaults = AssetDatabase.LoadAssetAtPath<HDRPDefaultsProfile>(GetAssetPath(TimeOfDayDefaultsProfileName));
                if (defaults != null)
                {
                    defaults.ApplyDefaultsToTimeOfDay(timeOfDay);
                }
#endif
                timeOfDay.SetHasBeenSetup(timeOfDay.SetupHDRPTimeOfDay());
                timeOfDay.SetStaticInstance(timeOfDay);
                SetupDefaultWeatherProfiles(timeOfDay);

                Volume[] localVolumes = FindObjectsOfType<Volume>();
                if (localVolumes.Length > 0)
                {
                    bool localFound = false;
                    List<Volume> allLocalVolumes = new List<Volume>();
                    foreach (Volume localVolume in localVolumes)
                    {
                        localFound = true;
                        allLocalVolumes.Add(localVolume);
                    }

                    if (parent != null)
                    {
                        timeOfDay.transform.SetParent(parent.transform);
                    }
#if UNITY_EDITOR
                    if (localFound)
                    {
                        if (EditorUtility.DisplayDialog("Local/Global Volumes Found",
                                "We have detected local/global volumes in your scene, this could affect the lighting quality of the time of day system as it might override some important settings. Would you like us to disable these volumes?",
                                "Yes", "No"))
                        {
                            foreach (Volume volume in allLocalVolumes)
                            {
                                if (volume != null)
                                {
                                    if (timeOfDay.AddDisabledItem(volume.gameObject))
                                    {
                                        volume.gameObject.SetActive(false);
                                        Debug.Log(volume.name + " has been disabled");
                                    }
                                }
                            }
                        }
                    }

                    if (selection)
                    {
                        Selection.activeObject = timeOfDay;
                    }

                    UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
#endif
                    if (localFound)
                    {
                        //Removes warning
                    }
                    timeOfDay.SetReflectionProbeProfile();
                }
            }

        }
        /// <summary>
        /// Removes time of day from the scene
        /// </summary>
        public static void RemoveTimeOfDay(bool isCreatingTOD = false)
        {
            HDRPTimeOfDay timeOfDay = Instance;
            if (timeOfDay != null)
            {
#if UNITY_EDITOR
                if (!isCreatingTOD)
                {
                    if (EditorUtility.DisplayDialog("Re-enable disabled gameobjects",
                            "HDRP Time Of Day has been removed would you like to activate all the disable objects that this system disabled?",
                            "Yes", "No"))
                    {
                        timeOfDay.EnableAllDisabledItems();
                    }
                }
#endif
                DestroyImmediate(timeOfDay.gameObject);
            }
            else
            {
                GameObject timeOfDayGameObject = GameObject.Find("HDRP Time Of Day");
                if (timeOfDayGameObject != null)
                {
                    DestroyImmediate(timeOfDayGameObject);
                }
            }
#if UNITY_EDITOR
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
#endif
        }
        /// <summary>
        /// Get the asset path of the first thing that matches the name
        /// </summary>
        /// <param name="fileName">File name to search for</param>
        /// <returns></returns>
        public static string GetAssetPath(string fileName)
        {
#if UNITY_EDITOR
            string fName = Path.GetFileNameWithoutExtension(fileName);
            string[] assets = UnityEditor.AssetDatabase.FindAssets(fName, null);
            for (int idx = 0; idx < assets.Length; idx++)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(assets[idx]);
                if (Path.GetFileName(path) == fileName)
                {
                    return path;
                }
            }
#endif
            return "";
        }
        /// <summary>
        /// Sets up the default weather profiles
        /// </summary>
        /// <param name="timeOfDay"></param>
        public static void SetupDefaultWeatherProfiles(HDRPTimeOfDay timeOfDay)
        {
            if (timeOfDay != null)
            {
#if UNITY_EDITOR
                if (EditorUtility.DisplayDialog("Enable Weather Effects", "Would you like to enable weather effects?", "Yes", "No"))
                {
                    timeOfDay.UseWeatherFX = true;
                }
                else
                {
                    timeOfDay.UseWeatherFX = false;
                }
#endif
            }
        }

        #endregion
    }
}
#endif