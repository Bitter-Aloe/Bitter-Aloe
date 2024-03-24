#if HDRPTIMEOFDAY && HDPipeline
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralWorlds.HDRPTOD
{
    /// <summary>
    /// This can be used on objects to disable scripts, animators or game objects when path tracing is enabled as some things are not allowed in path tracing
    /// </summary>
    public class HDRPTimeOfDayPathTracingObjectHandler : MonoBehaviour
    {
        #region Public Functions

        public List<MonoBehaviour> ManagableMonoBehaviours = new List<MonoBehaviour>();
        public List<Animator> ManagableAnimators = new List<Animator>();
        public List<GameObject> ManagableGameObjects = new List<GameObject>();
        public bool AllowUpdateKeyToEnableManageables = true;
        public KeyCode EnableManageablesKey = KeyCode.Mouse1;

        #endregion
        #region Private Variables

        private HDRPTimeOfDay m_timeOfDay;
        private bool m_stateChangedToPathTracing = false;

        #endregion
        #region Unity Functions

        /// <summary>
        /// Called when this script enables
        /// </summary>
        private void OnEnable()
        {
            if (m_timeOfDay == null)
            {
                m_timeOfDay = HDRPTimeOfDay.Instance;
            }

            if (m_timeOfDay != null)
            {
                m_timeOfDay.onRayTracingUpdated -= RayTracingUpdated;
                m_timeOfDay.onRayTracingUpdated += RayTracingUpdated;
            }
        }
        /// <summary>
        /// Called when this script disables
        /// </summary>
        private void OnDisable()
        {
            if (m_timeOfDay != null)
            {
                m_timeOfDay.onRayTracingUpdated -= RayTracingUpdated;
            }
        }
        /// <summary>
        /// Called at the end of the frame
        /// </summary>
        private void LateUpdate()
        {
            if (m_stateChangedToPathTracing && AllowUpdateKeyToEnableManageables)
            {
                if (Input.GetKey(EnableManageablesKey))
                {
                    SetManageableState(true);
                }
                else
                {
                    SetManageableState(false);
                }
            }
        }

        #endregion
        #region Private Functions

        /// <summary>
        /// Callback called when ray tracing is updated in time of day
        /// </summary>
        /// <param name="rtActive"></param>
        private void RayTracingUpdated(bool rtActive)
        {
            if (m_timeOfDay != null)
            {
                if (rtActive)
                {
                    if (m_timeOfDay.IsPathTracing())
                    {
                        SetManageableState(false);
                        m_stateChangedToPathTracing = true;
                    }
                    else
                    {
                        SetManageableState(true);
                        m_stateChangedToPathTracing = false;
                    }
                }
                else
                {
                    SetManageableState(true);
                    m_stateChangedToPathTracing = false;
                }
            }
        }
        /// <summary>
        /// Sets the render state of the monobehaviours and gameobjects
        /// </summary>
        /// <param name="state"></param>
        private void SetManageableState(bool state)
        {
            for (int i = 0; i < ManagableMonoBehaviours.Count; i++)
            {
                ManagableMonoBehaviours[i].enabled = state;
            }
            for (int i = 0; i < ManagableGameObjects.Count; i++)
            {
                ManagableGameObjects[i].SetActive(state);
            }
            for (int i = 0; i < ManagableAnimators.Count; i++)
            {
                ManagableAnimators[i].enabled = state;
            }
        }

        #endregion
    }
}
#endif