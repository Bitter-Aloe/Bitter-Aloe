#if HDPipeline
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

namespace ProceduralWorlds.HDRPTOD
{
    public enum InteriorParticleRenderMode { BoxCollision, Disable }
    public enum GizmoRenderShapeMode { Box, Sphere, None }

    [Serializable]
    public class InteriorControllerData
    {
        public bool m_isEnabled = true;
        public InteriorParticleRenderMode m_onEnterRenderMode = InteriorParticleRenderMode.BoxCollision;
        public AudioReverbPreset m_interiorReverbPreset = AudioReverbPreset.Room;
        public bool m_useBoundsSettings = true;
        public float m_boundsMultiplier = 2.5f;
        public Vector4 m_particleCollisionSettings = new Vector4(1f, 1f, 1f, 0f);
        public bool m_blendFog = false;
        public float m_fogFadeAmount = 0.05f;
    }
    [Serializable]
    public class InteriorControllerManagerData
    {
        public bool m_useInteriorControllers = true;
        public bool m_refreshControllerSystemsOnWeatherStart = true;
        public bool m_useAudioReverb = true;
        public AudioReverbPreset m_exteriorReverbPreset = AudioReverbPreset.Forest;
        public List<HDRPTimeOfDayInteriorController> m_controllers = new List<HDRPTimeOfDayInteriorController>();
        public List<VisualEffect> m_visualEffects = new List<VisualEffect>();
        public AudioReverbFilter m_reverbFilter;
        public Vector4 m_defaultParticleCollisionSettings = new Vector4(0f, 0f, 0f, 0f);

        public const string CollisionSettingsID = "CollisionSettings";
        public const string CollisionPositionSettingsID = "CollisionPositionSettings";

        /// <summary>
        /// Assigns the visual effects
        /// </summary>
        /// <param name="visualEffects"></param>
        public void AssignVisualEffects(List<VisualEffect> visualEffects)
        {
            m_visualEffects.Clear();
            m_visualEffects.AddRange(visualEffects);
        }
        /// <summary>
        /// Applies the interior systems
        /// </summary>
        /// <param name="value"></param>
        /// <param name="controller"></param>
        public bool Apply(bool value, bool inMainBounds, HDRPTimeOfDayInteriorController controller, HDRPTimeOfDayComponents components, bool weatherActive, UnderwaterOverrideData underwaterData, bool isUnderwater)
        {
            SetReverbFilter(inMainBounds, controller, underwaterData, isUnderwater);
            if (weatherActive)
            {
                SetFogInvert(inMainBounds, components, controller);
                if (controller != null)
                {
                    switch (controller.m_controllerData.m_onEnterRenderMode)
                    {
                        case InteriorParticleRenderMode.Disable:
                        {
                            SetVisualEffectsState(value, controller);
                            break;
                        }
                        case InteriorParticleRenderMode.BoxCollision:
                        {
                            SetWeatherParticleCollisions(value, controller);
                            break;
                        }
                    }
                }
                else
                {
                    SetVisualEffectsState(value, controller);
                    SetWeatherParticleCollisions(value, controller);
                }
            }

            return value;
        }
        /// <summary>
        /// Checks if you are in the bounds of a controller
        /// </summary>
        /// <returns></returns>
        public bool CheckControllers(out bool inMainBounds, out HDRPTimeOfDayInteriorController controller)
        {
            inMainBounds = false;
            controller = null;
            if (m_useInteriorControllers)
            {
                int lastPriority = -1;
                foreach (HDRPTimeOfDayInteriorController currentController in m_controllers)
                {
                    if (currentController.IsInBounds())
                    {
                        if (currentController.Priority > lastPriority)
                        {
                            lastPriority = currentController.Priority;
                            controller = currentController;
                            inMainBounds = controller.IsInBounds(true);
                        }
                    }
                }

                return controller != null;
            }

            return false;
        }
        /// <summary>
        /// Gets all the controllers
        /// This should not be called every frame rather on start/awake
        /// </summary>
        public void GetAllControllers()
        {
            m_controllers.Clear();
            m_controllers.AddRange(GameObject.FindObjectsOfType<HDRPTimeOfDayInteriorController>());
        }
        /// <summary>
        /// Adds the controller to the list
        /// </summary>
        /// <param name="controller"></param>
        public void AddController(HDRPTimeOfDayInteriorController controller)
        {
            if (!m_controllers.Contains(controller))
            {
                m_controllers.Add(controller);
            }
        }
        /// <summary>
        /// Removes the controller from the list
        /// </summary>
        /// <param name="controller"></param>
        public void RemoveController(HDRPTimeOfDayInteriorController controller)
        {
            if (m_controllers.Contains(controller))
            {
                m_controllers.Remove(controller);
            }
        }
        /// <summary>
        /// Sets up the audio reverb filter
        /// </summary>
        /// <param name="camera"></param>
        public void SetupAudioReverb(Transform camera)
        {
            if (camera != null)
            {
                AudioReverbFilter reverb = camera.GetComponent<AudioReverbFilter>();
                if (m_useAudioReverb)
                {
                    if (reverb == null)
                    {
                        reverb = camera.gameObject.AddComponent<AudioReverbFilter>();
                    }

                    reverb.reverbPreset = m_exteriorReverbPreset;
                    m_reverbFilter = reverb;
                    return;
                }
                else
                {
                    if (reverb != null)
                    {
                        GameObject.DestroyImmediate(reverb);
                    }
                }
            }
        }
        /// <summary>
        /// Applies the invert fog effect
        /// </summary>
        /// <param name="state"></param>
        /// <param name="components"></param>
        public void SetFogInvert(bool value, HDRPTimeOfDayComponents components, HDRPTimeOfDayInteriorController controller)
        {
            if (components.m_localVolumetricFog != null)
            {
                if (controller != null && controller.m_controllerData.m_blendFog)
                {
                    components.m_localVolumetricFog.parameters.invertFade = value;
                    if (value)
                    {
                        Vector3 fadeDistance = GetLocalFogBlendRange(controller.Bounds.size, controller.m_controllerData.m_fogFadeAmount, components);
                        components.m_localVolumetricFog.parameters.negativeFade = value ? fadeDistance : new Vector3(0.005f, 0.005f, 0.005f);
                        components.m_localVolumetricFog.parameters.positiveFade = value ? fadeDistance : new Vector3(0.005f, 0.005f, 0.005f);
                    }
                    else
                    {
                        Vector3 fadeDistance = GetLocalFogBlendRange(Vector3.one, 1f, components);
                        components.m_localVolumetricFog.parameters.negativeFade = fadeDistance;
                        components.m_localVolumetricFog.parameters.positiveFade = fadeDistance;
                    }
                }
                else
                {
                    Vector3 fadeDistance = GetLocalFogBlendRange(Vector3.one, 1f, components);
                    components.m_localVolumetricFog.parameters.invertFade = false;
                    components.m_localVolumetricFog.parameters.negativeFade = fadeDistance;
                    components.m_localVolumetricFog.parameters.positiveFade = fadeDistance;
                }
            }
        }
        /// <summary>
        /// Sets the render state of the weather effect
        /// </summary>
        /// <param name="value"></param>
        public void SetVisualEffectsState(bool value, HDRPTimeOfDayInteriorController controller)
        {
            foreach (VisualEffect effect in m_visualEffects)
            {
                if (controller != null && effect != null)
                {
                    switch (controller.m_controllerData.m_onEnterRenderMode)
                    {
                        case InteriorParticleRenderMode.Disable:
                        {
                            if (value)
                            {
                                effect.Stop();
                                effect.enabled = false;
                            }
                            else
                            {
                                effect.Play();
                                effect.enabled = true;
                            }
                            break;
                        }
                    }
                }
                else
                {
                    effect.enabled = true;
                    effect.Play();
                }
            }
        }
        /// <summary>
        /// Assigns the current reverb filter
        /// </summary>
        /// <param name="value"></param>
        /// <param name="controller"></param>
        public void SetReverbFilter(bool value, HDRPTimeOfDayInteriorController controller, UnderwaterOverrideData underwaterData, bool isUnderwater)
        {
            if (!m_useAudioReverb)
            {
                return;
            }

            if (m_reverbFilter != null)
            {
                if (controller != null)
                {
                    m_reverbFilter.reverbPreset = value ? controller.m_controllerData.m_interiorReverbPreset : m_exteriorReverbPreset;
                }
                else
                {
                    m_reverbFilter.reverbPreset = m_exteriorReverbPreset;
                }

                if (underwaterData.m_useUnderwaterReverb)
                {
                    if (isUnderwater)
                    {
                        m_reverbFilter.reverbPreset = AudioReverbPreset.Underwater;
                    }
                }
            }
        }
        /// <summary>
        /// Sets the weather particles collision settings
        /// </summary>
        /// <param name="value"></param>
        /// <param name="controller"></param>
        public void SetWeatherParticleCollisions(bool value, HDRPTimeOfDayInteriorController controller)
        {
            foreach (VisualEffect visualEffect in m_visualEffects)
            {
                visualEffect.enabled = true;
                visualEffect.Play();
                if (visualEffect.HasVector4(CollisionSettingsID))
                {
                    if (controller != null)
                    {
                        if (value)
                        {
                            if (controller.m_controllerData.m_useBoundsSettings)
                            {
                                visualEffect.SetVector4(CollisionSettingsID, controller.Bounds.size);
                            }
                            else
                            {
                                visualEffect.SetVector4(CollisionSettingsID, controller.m_controllerData.m_particleCollisionSettings);
                            }
                        }
                        else
                        {
                            visualEffect.SetVector4(CollisionSettingsID, m_defaultParticleCollisionSettings);
                        }
                    }
                    else
                    {
                        visualEffect.SetVector4(CollisionSettingsID, m_defaultParticleCollisionSettings);
                    }
                }

                if (visualEffect.HasVector3(CollisionPositionSettingsID))
                {
                    if (controller != null)
                    {
                        if (value)
                        {
                            if (controller.m_controllerData.m_useBoundsSettings)
                            {
                                visualEffect.SetVector3(CollisionPositionSettingsID, controller.transform.position);
                            }
                            else
                            {
                                visualEffect.SetVector3(CollisionPositionSettingsID, controller.MainCamera.position);
                            }
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Gets the fade blend range
        /// </summary>
        /// <param name="controlerSize"></param>
        /// <param name="components"></param>
        /// <returns></returns>
        public Vector3 GetLocalFogBlendRange(Vector3 controlerSize, float multiplier, HDRPTimeOfDayComponents components)
        {
            return new Vector3(controlerSize.x / components.m_localVolumetricFog.parameters.size.x, controlerSize.y / components.m_localVolumetricFog.parameters.size.y, controlerSize.z / components.m_localVolumetricFog.parameters.size.z) / multiplier;
        }
    }
    [Serializable]
    public class InteriorControllerGizmoSettings
    {
        public bool m_drawGizmo = true;
        public bool m_drawGizmoSelectedOnly = true;
        public Color m_gizmoColor = new Color(0f, 1f, 0f, 0.25f);
        public Color m_gizmoColorFog = new Color(0f, 0f, 1f, 0.25f);
        public GizmoRenderShapeMode m_gizmoRenderMode = GizmoRenderShapeMode.Box;

        /// <summary>
        /// Assigns the shape render mode
        /// </summary>
        /// <param name="collider"></param>
        public void SetGizmoRenderMode(Collider collider)
        {
            if (collider == null)
            {
                m_gizmoRenderMode = GizmoRenderShapeMode.None;
                return;
            }

            if (collider.GetType() == typeof(CapsuleCollider) || collider.GetType() == typeof(SphereCollider))
            {
                m_gizmoRenderMode = GizmoRenderShapeMode.Sphere;
                return;
            }
            if (collider.GetType() == typeof(BoxCollider))
            {
                m_gizmoRenderMode = GizmoRenderShapeMode.Box;
                return;
            }
            else
            {
                m_gizmoRenderMode = GizmoRenderShapeMode.None;
            }

        }
        /// <summary>
        /// Draws the gizmos
        /// Should only be called using: OnDrawGizmos or OnDrawGizmosSelected
        /// </summary>
        /// <param name="objectTransform"></param>
        /// <param name="bounds"></param>
        /// <param name="isEnabled"></param>
        /// <param name="isWireFrame"></param>
        public void DrawGizmo(Transform objectTransform, Bounds bounds, bool isEnabled, bool isWireFrame = false, bool isFogBounds = false)
        {
            if (m_drawGizmo && objectTransform != null)
            {
                Gizmos.matrix = Matrix4x4.TRS(objectTransform.position, objectTransform.rotation, bounds.size);
                if (!isFogBounds)
                {
                    Gizmos.color = isEnabled ? m_gizmoColor : m_gizmoColor / 2.5f;
                }
                else
                {
                    Gizmos.color = isEnabled ? m_gizmoColorFog : m_gizmoColorFog / 2.5f;
                }

                switch (m_gizmoRenderMode)
                {
                    case GizmoRenderShapeMode.Sphere:
                    {
                        if (isWireFrame)
                        {
                            Gizmos.DrawWireSphere(Vector3.zero, 1f);
                        }
                        else
                        {
                            Gizmos.DrawSphere(Vector3.zero, 1f);
                        }
                        break;
                    }
                    case GizmoRenderShapeMode.Box:
                    {
                        if (isWireFrame)
                        {
                            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
                        }
                        else
                        {
                            Gizmos.DrawCube(Vector3.zero, Vector3.one);
                        }
                        break;
                    }
                }
            }
        }
    }

    [ExecuteAlways]
    public class HDRPTimeOfDayInteriorController : MonoBehaviour
    {
        public Bounds Bounds
        {
            get { return m_bounds; }
        }
        [SerializeField] private Bounds m_bounds;
        [SerializeField] private Bounds m_blendBounds;
        public Collider Collider
        {
            get { return m_collider; }
            set
            {
                if (m_collider != value)
                {
                    m_collider = value;
                    Refresh();
                }
            }
        }
        [SerializeField] private Collider m_collider;
        public Transform MainCamera
        {
            get { return m_mainCamera; }
            set
            {
                if (m_mainCamera != value)
                {
                    m_mainCamera = value;
                    Refresh();
                }
            }
        }
        [SerializeField] private Transform m_mainCamera;
        public int Priority
        {
            get { return m_priority; }
            set
            {
                if (m_priority != value)
                {
                    m_priority = Mathf.Clamp(value, 0, int.MaxValue);
                }
            }
        }
        [SerializeField] private int m_priority = 0;

        public InteriorControllerData m_controllerData = new InteriorControllerData();
        public InteriorControllerGizmoSettings m_gizmoSettings = new InteriorControllerGizmoSettings();
        public string m_colliderLayer = "Ignore Raycast";
        [SerializeField] private bool m_validated = false;

        #region Unity Functions

        private void OnEnable()
        {
            Refresh(false);
            AssignController(true);
        }
        private void OnDestroy()
        {
            AssignController(false);
        }
        private void OnDrawGizmos()
        {
            if (!m_gizmoSettings.m_drawGizmoSelectedOnly)
            {
                m_gizmoSettings.DrawGizmo(transform, Bounds, m_controllerData.m_isEnabled);
                m_gizmoSettings.DrawGizmo(transform, m_blendBounds, m_controllerData.m_isEnabled, true);
                if (HDRPTimeOfDay.Instance != null)
                {
                    m_gizmoSettings.DrawGizmo(m_mainCamera, new Bounds(m_mainCamera.position, HDRPTimeOfDayAPI.CreateVector3FromFloat(Mathf.Lerp(0f, HDRPTimeOfDay.Instance.GetLocalFogSize() / 2f, m_controllerData.m_fogFadeAmount))), m_controllerData.m_isEnabled, true, true);
                }
            }
        }
        private void OnDrawGizmosSelected()
        {
            if (m_gizmoSettings.m_drawGizmoSelectedOnly)
            {
                m_gizmoSettings.DrawGizmo(transform, Bounds, m_controllerData.m_isEnabled);
                m_gizmoSettings.DrawGizmo(transform, m_blendBounds, m_controllerData.m_isEnabled, true);
                if (HDRPTimeOfDay.Instance != null)
                {
                    m_gizmoSettings.DrawGizmo(m_mainCamera, new Bounds(m_mainCamera.position, HDRPTimeOfDayAPI.CreateVector3FromFloat(Mathf.Lerp(0f, HDRPTimeOfDay.Instance.GetLocalFogSize() / 2f, m_controllerData.m_fogFadeAmount))), m_controllerData.m_isEnabled, true, true);
                }
            }
        }

        #endregion
        #region Public Functions

        /// <summary>
        /// Function is used to check this controller if the player/camera is within the bounds
        /// </summary>
        /// <param name="checkMainBounds"></param>
        /// <returns></returns>
        public bool IsInBounds(bool checkMainBounds = false)
        {
            if (checkMainBounds)
            {
                return CheckIfInMainBounds(m_mainCamera);
            }
            else
            {
                return CheckIfInBounds(m_mainCamera);
            }
        }
        /// <summary>
        /// Refreshes the system
        /// </summary>
        public void Refresh(bool updateBoundsOnly = true)
        {
            if (updateBoundsOnly)
            {
                UpdateBounds();
                m_gizmoSettings.SetGizmoRenderMode(m_collider);
            }
            else
            {
                UpdateBounds();
                m_gizmoSettings.SetGizmoRenderMode(m_collider);
                if (m_mainCamera == null)
                {
                    m_mainCamera = HDRPTimeOfDayAPI.GetCamera();
                }

                if (m_mainCamera != null && Collider != null && Bounds != null)
                {
                    m_validated = true;
                }
                else
                {
                    m_validated = false;
                }
            }
        }

        #endregion
        #region Private Functions

        /// <summary>
        /// Used to add or remove the controller to the active list
        /// </summary>
        /// <param name="remove"></param>
        private void AssignController(bool add)
        {
#if UNITY_2021_2_OR_NEWER && HDPipeline
            HDRPTimeOfDay timeOfDay = HDRPTimeOfDay.Instance;
            if (timeOfDay != null)
            {
                if (add)
                {
                    timeOfDay.AddInteriorController(this);
                }
                else
                {
                    timeOfDay.RemoveInteriorController(this);
                }
            }
#endif
        }
        /// <summary>
        /// Checks to see if the player transform is in the bounds
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        private bool CheckIfInMainBounds(Transform player)
        {
            if (m_validated && m_controllerData.m_isEnabled)
            {
                return m_bounds.Contains(player.position);
            }

            return false;
        }
        /// <summary>
        /// Checks to see if the player transform is in the bounds
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        private bool CheckIfInBounds(Transform player)
        {
            if (m_validated && m_controllerData.m_isEnabled)
            {
                switch (m_controllerData.m_onEnterRenderMode)
                {
                    case InteriorParticleRenderMode.BoxCollision:
                    {
                        if (m_controllerData.m_useBoundsSettings)
                        {
                            return m_blendBounds.Contains(player.position);
                        }
                        else
                        {
                            return m_bounds.Contains(player.position);
                        }
                    }
                    case InteriorParticleRenderMode.Disable:
                    {
                        return m_bounds.Contains(player.position);
                    }
                }
            }

            return false;
        }
        /// <summary>
        /// Updates the bounds for the controller
        /// </summary>
        private void UpdateBounds()
        {
            if (m_collider == null)
            {
                m_collider = GetComponent<Collider>();
            }

            if (m_collider != null)
            {
                m_collider.gameObject.layer = LayerMask.NameToLayer(m_colliderLayer);
            }

            m_bounds = GetBounds(m_collider);
            m_blendBounds = new Bounds(transform.position, m_bounds.size * m_controllerData.m_boundsMultiplier);
        }
        /// <summary>
        /// Gets the bounds based on the colider
        /// </summary>
        /// <param name="collider"></param>
        /// <returns></returns>
        private Bounds GetBounds(Collider collider)
        {
            if (m_controllerData.m_useBoundsSettings)
            {
                if (collider == null)
                {
                    return new Bounds(transform.position, Vector3.zero);
                }

                collider.isTrigger = true;
                BoxCollider boxCollider = collider.GetComponent<BoxCollider>();
                if (boxCollider != null)
                {
                    return new Bounds(transform.position, new Vector3(boxCollider.size.x * transform.lossyScale.x, boxCollider.size.y * transform.lossyScale.y, boxCollider.size.z * transform.lossyScale.z));
                }
                SphereCollider sphereCollider = collider.GetComponent<SphereCollider>();
                if (sphereCollider != null)
                {
                    return new Bounds(transform.position, new Vector3(sphereCollider.radius * transform.lossyScale.x, sphereCollider.radius * transform.lossyScale.y, sphereCollider.radius* transform.lossyScale.z));
                }
                CapsuleCollider capsuleCollider = collider.GetComponent<CapsuleCollider>();
                if (capsuleCollider != null)
                {
                    return new Bounds(transform.position, new Vector3(capsuleCollider.radius * transform.lossyScale.x, capsuleCollider.height * transform.lossyScale.y, capsuleCollider.radius * transform.lossyScale.z));
                }
                MeshCollider meshCollider = collider.GetComponent<MeshCollider>();
                if (meshCollider != null)
                {
                    return new Bounds(meshCollider.bounds.center, meshCollider.bounds.size);
                }
            }
            else
            {
                return new Bounds(transform.position, new Vector3(m_controllerData.m_particleCollisionSettings.x * transform.lossyScale.x, m_controllerData.m_particleCollisionSettings.y * transform.lossyScale.y, m_controllerData.m_particleCollisionSettings.z * transform.lossyScale.z));
            }

            return new Bounds(transform.position, Vector3.zero);
        }

        #endregion
    }
}
#endif