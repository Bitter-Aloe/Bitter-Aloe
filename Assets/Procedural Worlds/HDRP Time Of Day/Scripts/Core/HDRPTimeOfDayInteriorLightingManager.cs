#if HDRPTIMEOFDAY && HDPipeline
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace ProceduralWorlds.HDRPTOD
{
    public class HDRPTimeOfDayInteriorLightingManager : MonoBehaviour
    {
        public bool Render
        {
            get { return m_render; }
            set
            {
                if (m_render != value)
                {
                    m_render = value;
                    UpdateRender(value);
                }
            }
        }
        [SerializeField] private bool m_render = true;

        public HDRPTimeOfDayInteriorLightingProfile Profile
        {
            get { return m_profile; }
            set
            {
                if (m_profile != value)
                {
                    m_profile = value;
                    UpdateInterior(true);
                }
            }
        }
        [SerializeField] private HDRPTimeOfDayInteriorLightingProfile m_profile;

        public Vector3 InteriorBounds
        {
            get { return m_interiorBounds; }
            set
            {
                if (m_interiorBounds != value)
                {
                    m_interiorBounds = value;
                    RefreshBounds(value);
                }
            }
        }
        [SerializeField] private Vector3 m_interiorBounds = new Vector3(10f, 10f, 10f);

        public AudioReverbPreset InteriorReverbPreset
        {
            get { return m_interiorReverbPreset; }
            set
            {
                if (m_interiorReverbPreset != value)
                {
                    m_interiorReverbPreset = value;
                    UpdateReverbPreset(value);
                }
            }
        }
        [SerializeField] private AudioReverbPreset m_interiorReverbPreset = AudioReverbPreset.Room;

        [SerializeField] private BoxCollider m_boxCollider;
        [SerializeField] private BoxCollider m_boxColliderRTX;
        [SerializeField] private LocalVolumetricFog m_localFog;
        [SerializeField] private Volume m_volume;
        [SerializeField] private GameObject m_volumeRTXGameObject;
        [SerializeField] private Volume m_volumeRTX;
        [SerializeField] private ReflectionProbe m_reflectionProbe;
        [SerializeField] private HDAdditionalReflectionData m_reflectionProbeData;
        [SerializeField] private Light m_fakeSunLight;
        [SerializeField] private HDAdditionalLightData m_fakeSunLightData;
        [SerializeField] private HDRPTimeOfDayInteriorController m_interiorController;

        private void OnEnable()
        {
            if (HDRPTimeOfDay.Instance != null)
            {
                HDRPTimeOfDay.Instance.m_interiorLightingManagers.Add(this);
            }
        }
        private void OnDisable()
        {
            if (HDRPTimeOfDay.Instance != null)
            {
                HDRPTimeOfDay.Instance.m_interiorLightingManagers.Remove(this);
            }
        }

        /// <summary>
        /// Builds the interior
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="targetVolumeSpace"></param>
        /// <param name="boundsSize"></param>
        /// <param name="forceUpdate"></param>
        public void BuildInterior(HDRPTimeOfDayInteriorLightingProfile profile, GameObject targetVolumeSpace, Vector3 boundsSize, Vector3 targetPosition, AudioReverbPreset interiorReverbPreset = AudioReverbPreset.Room, bool forceUpdate = false)
        {
            if (targetVolumeSpace == null)
            {
                Debug.LogError("Target Volume Gameobject is null, please make sure you pass in a valid gameobject.");
                return;
            }

            //Setup the components
            m_interiorBounds = boundsSize;
            SetupComponents(targetVolumeSpace, m_interiorBounds, interiorReverbPreset);

            //Updates the targets position
            targetVolumeSpace.transform.position = targetPosition;

            //Assign profile
            if (profile == null)
            {
                Debug.LogError("Lighting profile that was passed is was null, unable to finish the setup of the interior lighting system.");
                return;
            }
            Profile = profile;
            if (forceUpdate)
            {
                UpdateInterior(true);
            }
        }
        /// <summary>
        /// Refreshes the interior settings
        /// </summary>
        public void Refresh()
        {
            UpdateInterior(true);
        }
        /// <summary>
        /// Refreshes the ray tracing for the interior lighting
        /// </summary>
        /// <param name="value"></param>
        public void RefreshRayTracing(bool value)
        {
            UpdateRayTracing(value);
        }
        /// <summary>
        /// Safely removes the magaer system from the gameobject
        /// Also removes all the created components linked to the system if removeComponentsToo set to true
        /// </summary>
        /// <param name="removeComponentsToo"></param>
        public void RemoveManager(bool removeComponentsToo = true)
        {
            if (removeComponentsToo)
            {
                SafeDestroy(m_boxCollider);
                SafeDestroy(m_localFog);
                SafeDestroy(m_volume);
                SafeDestroy(m_reflectionProbeData);
                SafeDestroy(m_reflectionProbe);
            }

            SafeDestroy(this);
        }

        /// <summary>
        /// Updates the rendering of the interior lighting
        /// </summary>
        /// <param name="value"></param>
        private void UpdateRender(bool value)
        {
            if (IsValidated())
            {
                m_boxCollider.enabled = value;
                m_boxColliderRTX.enabled = value;
                m_localFog.enabled = value;
                m_volume.enabled = value;
                m_reflectionProbe.enabled = value;
                m_reflectionProbeData.enabled = value;
                m_volumeRTX.enabled = HDRPTimeOfDayAPI.RayTracingActive();
                m_interiorController.enabled = value;
                m_fakeSunLight.enabled = value;
                m_fakeSunLightData.enabled = value;
            }
        }
        /// <summary>
        /// Sets up and assigns the components for the system to work
        /// </summary>
        /// <param name="targetVolumeSpace"></param>
        /// <param name="boundsSize"></param>
        private void SetupComponents(GameObject targetVolumeSpace, Vector3 boundsSize, AudioReverbPreset interiorReverbPreset)
        {
            //Box collider
            m_boxCollider = targetVolumeSpace.GetComponent<BoxCollider>();
            if (m_boxCollider == null)
            {
                m_boxCollider = targetVolumeSpace.AddComponent<BoxCollider>();
            }
            
            //Local fog
            m_localFog = targetVolumeSpace.GetComponent<LocalVolumetricFog>();
            if (m_localFog == null)
            {
                m_localFog = targetVolumeSpace.AddComponent<LocalVolumetricFog>();
            }

            //Volume
            m_volume = targetVolumeSpace.GetComponent<Volume>();
            if (m_volume == null)
            {
                m_volume = targetVolumeSpace.AddComponent<Volume>();
            }
            //Volume RTX GameObject
            if (m_volumeRTXGameObject == null)
            {
                m_volumeRTXGameObject = new GameObject("RTX Volume Object");
                m_volumeRTXGameObject.transform.SetParent(targetVolumeSpace.transform, false);
            }
            //Box collider RTX
            m_boxColliderRTX = m_volumeRTXGameObject.GetComponent<BoxCollider>();
            if (m_boxColliderRTX == null)
            {
                m_boxColliderRTX = m_volumeRTXGameObject.AddComponent<BoxCollider>();
            }
            //Volume RTX
            m_volumeRTX = m_volumeRTXGameObject.GetComponent<Volume>();
            if (m_volumeRTX == null)
            {
                m_volumeRTX = m_volumeRTXGameObject.AddComponent<Volume>();
            }
            //Reflection probe
            m_reflectionProbe = targetVolumeSpace.GetComponent<ReflectionProbe>();
            if (m_reflectionProbe == null)
            {
                m_reflectionProbe = targetVolumeSpace.AddComponent<ReflectionProbe>();
            }
            m_reflectionProbeData = m_reflectionProbe.GetComponent<HDAdditionalReflectionData>();
            if (m_reflectionProbeData == null)
            {
                m_reflectionProbeData = m_reflectionProbe.gameObject.AddComponent<HDAdditionalReflectionData>();
            }
            //Fake Sun
            GameObject sun = new GameObject("Fake Sun Light");
            sun.transform.SetParent(targetVolumeSpace.transform, false);
            sun.transform.localEulerAngles = new Vector3(30f, 0f, 0f);
            m_fakeSunLight = sun.AddComponent<Light>();
            m_fakeSunLight.type = LightType.Directional;
            m_fakeSunLightData = sun.GetComponent<HDAdditionalLightData>();
            if (m_fakeSunLightData == null)
            {
                m_fakeSunLightData = sun.gameObject.AddComponent<HDAdditionalLightData>();
            }

            //Interior Controller
            m_interiorController = targetVolumeSpace.GetComponent<HDRPTimeOfDayInteriorController>();
            if (m_interiorController == null)
            {
                m_interiorController = targetVolumeSpace.gameObject.AddComponent<HDRPTimeOfDayInteriorController>();
            }

            m_interiorReverbPreset = interiorReverbPreset;
            m_interiorController.Collider = m_boxCollider;
            m_interiorController.Priority = 99;
            m_interiorController.m_controllerData.m_interiorReverbPreset = InteriorReverbPreset;
            m_interiorController.Refresh();

            //Assigns the bounds
            RefreshBounds(boundsSize);
        }
        /// <summary>
        /// Updates the controller audio preset
        /// </summary>
        /// <param name="preset"></param>
        private void UpdateReverbPreset(AudioReverbPreset preset)
        {
            m_interiorController.m_controllerData.m_interiorReverbPreset = preset;
            m_interiorController.Refresh();
        }
        /// <summary>
        /// Checks to make sure it's validated
        /// </summary>
        /// <returns></returns>
        private bool IsValidated()
        {
            if (m_boxCollider == null)
            {
                return false;
            }
            if (m_boxColliderRTX == null)
            {
                return false;
            }
            if (m_localFog == null)
            {
                return false;
            }
            if (m_volume == null)
            {
                return false;
            }
            if (m_volumeRTX == null)
            {
                return false;
            }
            if (m_reflectionProbe == null)
            {
                return false;
            }
            if (m_reflectionProbeData == null)
            {
                return false;
            }

            return true;
        }
        /// <summary>
        /// Updates the interior settings
        /// </summary>
        private void UpdateInterior(bool refreshRayTracing, bool validate = true)
        {
            if (Profile != null)
            {
                if (validate)
                {
                    if (!IsValidated())
                    {
                        SetupComponents(gameObject, m_interiorBounds, InteriorReverbPreset);
                        if (!IsValidated())
                        {
                            Debug.LogWarning("Components were null so interior lighting can't be updated. We tried to setup the components on the gameobject this script is attached to. We highly recommend you use the BuildInterior() function to setup this interior correctly.");
                            return;
                        }
                    }
                }

                bool RTXOn = HDRPTimeOfDayAPI.RayTracingActive();

                Profile.ApplyVolume(m_volume);
                Profile.ApplyVolumeRTX(m_volumeRTX);
                Profile.ApplyLocalFog(m_localFog);
                Profile.ApplyReflectionProbe(m_reflectionProbeData, RTXOn);
                Profile.ApplyFakeSun(m_fakeSunLightData, RTXOn);

                if (refreshRayTracing)
                {
                    UpdateRayTracing(RTXOn);
                }
            }
        }
        /// <summary>
        /// Refreshes the bounds size scale on all the systems to make sure they fit the bounds
        /// </summary>
        /// <param name="boundsSize"></param>
        /// <param name="validate"></param>
        private void RefreshBounds(Vector3 boundsSize, bool validate = true)
        {
            if (validate)
            {
                if (!IsValidated())
                {
                    SetupComponents(gameObject, m_interiorBounds, InteriorReverbPreset);
                    Debug.LogWarning("Components were null so interior lighting can't be updated. We tried to setup the components on the gameobject this script is attached to. We highly recommend you use the BuildInterior() function to setup this interior correctly.");
                    return;
                }
            }

            //Box Collider
            m_boxCollider.isTrigger = true;
            m_boxCollider.size = boundsSize;
            //Box Collider RTX
            m_boxColliderRTX.isTrigger = true;
            m_boxColliderRTX.size = boundsSize;
            //Local fog
            m_localFog.parameters.size = boundsSize;
            //Volume
            m_volume.isGlobal = false;
            m_volume.priority = 999f;
            //Volume RTX
            m_volumeRTX.isGlobal = false;
            m_volumeRTX.priority = 9999f;
            //Reflection probe
            m_reflectionProbeData.influenceVolume.boxSize = boundsSize;
            //Controller
            m_interiorController.Refresh(true);
        }
        /// <summary>
        /// Safely destoys a component based on if you are playing or in editor
        /// </summary>
        /// <param name="component"></param>
        private void SafeDestroy(Object component)
        {
            if (Application.isPlaying)
            {
                Destroy(component);
            }
            else
            {
                DestroyImmediate(component);
            }
        }
        /// <summary>
        /// Upadtes the RTX volume to enable it on or off
        /// </summary>
        /// <param name="RTXOn"></param>
        private void UpdateRayTracing(bool RTXOn)
        {
            if (m_volumeRTX != null)
            {
                m_volumeRTX.enabled = RTXOn;
            }

            Profile.ApplyReflectionProbe(m_reflectionProbeData, RTXOn);
        }
    }
}
#endif