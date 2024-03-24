#if HDRPTIMEOFDAY && HDPipeline
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace ProceduralWorlds.HDRPTOD
{
    [ExecuteAlways]
    public class HDRPTimeOfDayRealtimeProbe : MonoBehaviour
    {
        public Transform m_rootUpdateTransform;

        [SerializeField] private ReflectionProbe m_probe;
        [SerializeField] private HDAdditionalReflectionData m_probeData;

        /// <summary>
        /// Called when this script enables
        /// </summary>
        private void OnEnable()
        {
            GetProbe();
            if (HDRPTimeOfDay.Instance != null)
            {
                HDRPTimeOfDay.Instance.onTimeOfDayChanged -= UpdateProbe;
                HDRPTimeOfDay.Instance.onTimeOfDayChanged += UpdateProbe;
                UpdateProbe(HDRPTimeOfDay.Instance.TimeOfDay);
            }
        }
        /// <summary>
        /// Called when this script is disabled
        /// </summary>
        private void OnDisable()
        {
            if (HDRPTimeOfDay.Instance != null)
            {
                HDRPTimeOfDay.Instance.onTimeOfDayChanged -= UpdateProbe;
            }
        }
        /// <summary>
        /// Called at the end of the frame
        /// </summary>
        private void LateUpdate()
        {
            if (m_rootUpdateTransform != null)
            {
                if (m_rootUpdateTransform.hasChanged && HDRPTimeOfDay.Instance != null)
                {
                    UpdateProbe(HDRPTimeOfDay.Instance.TimeOfDay);
                }
            }
        }

        /// <summary>
        /// Refresh the probe and request an update
        /// </summary>
        /// <param name="time"></param>
        private void UpdateProbe(float time)
        {
            if (m_probeData != null)
            {
                StartCoroutine(DelayedUpdate());
                if (m_rootUpdateTransform != null)
                {
                    m_rootUpdateTransform.hasChanged = false;
                }
            }
        }
        /// <summary>
        /// Gets the probe components
        /// </summary>
        private void GetProbe()
        {
            if (m_probe == null)
            {
                m_probe = GetComponentInChildren<ReflectionProbe>();
            }

            if (m_probe != null)
            {
                m_probeData = m_probe.GetComponent<HDAdditionalReflectionData>();
                SetupProbeSettings();
            }
        }
        /// <summary>
        /// Applies the setup of the probe and also bakes it at it's current position
        /// </summary>
        private void SetupProbeSettings()
        {
            if (m_probeData != null)
            {
                m_probeData.mode = ProbeSettings.Mode.Realtime;
                m_probeData.realtimeMode = ProbeSettings.RealtimeMode.OnDemand;
#if UNITY_2022_3_OR_NEWER
                m_probeData.timeSlicing = true;
#endif
                if (HDRPTimeOfDay.Instance != null)
                {
                    UpdateProbe(HDRPTimeOfDay.Instance.TimeOfDay);
                }
            }
        }
        /// <summary>
        /// Waits 1/10 of a second before requesting update
        /// </summary>
        /// <returns></returns>
        private IEnumerator DelayedUpdate()
        {
            yield return new WaitForSeconds(0.1f);
            m_probeData.RequestRenderNextUpdate();
        }
    }
}
#endif