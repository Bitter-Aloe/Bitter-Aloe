#if HDPipeline && UNITY_2021_2_OR_NEWER
using UnityEngine;

namespace ProceduralWorlds.HDRPTOD
{
    [ExecuteAlways]
    public class HDRPTimeOfDayLightProbeIncludeVolume : MonoBehaviour
    {
        public bool m_useAsIgnoreVolume = false;
        public Color m_gizmoColor = new Color(0f, 1f, 0f, 0.5f);

        private Bounds m_bounds;

        private void OnEnable()
        {
            if (HDRPTimeOfDay.Instance != null)
            {
                HDRPTimeOfDay.Instance.AddLightProbeIncludeVolume(this);
            }
        }
        private void OnDisable()
        {
            if (HDRPTimeOfDay.Instance != null)
            {
                HDRPTimeOfDay.Instance.RemoveLightProbeIncludeVolume(this);
            }
        }
        private void OnDrawGizmosSelected()
        {
            RefreshBounds();
            Gizmos.color = m_gizmoColor;
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.localScale);
            Gizmos.DrawCube(Vector3.zero, Vector3.one);
        }

        /// <summary>
        /// Checks to see if the position is within the bounds of this ignore volume
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public bool PositionInBounds(Vector3 position, out bool isIgnore, bool refreshBounds = false)
        {
            isIgnore = m_useAsIgnoreVolume;
            if (refreshBounds)
            {
                RefreshBounds();
            }

            Vector3 checkPos = transform.InverseTransformPoint(position);
            return m_bounds.Contains(checkPos);
        }
        /// <summary>
        /// Refreshes the bounds
        /// </summary>
        public void RefreshBounds()
        {
            m_bounds = new Bounds(Vector3.zero, Vector3.one);
        }
    }
}
#endif