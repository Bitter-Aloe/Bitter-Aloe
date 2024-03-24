#if HDPipeline && UNITY_2021_2_OR_NEWER
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralWorlds.HDRPTOD
{
    [ExecuteAlways]
    public class HDRPTimeOfDayLightProbeVolume : MonoBehaviour
    {
        public bool m_isEnabled = true;

        public int DensityX
        {
            get { return m_densityX; }
            set
            {
                if (m_densityX != value)
                {
                    m_densityX = Mathf.Clamp(value, 2, int.MaxValue);
                }
            }
        }
        [SerializeField] private int m_densityX = 16;

        public int DensityY
        {
            get { return m_densityY; }
            set
            {
                if (m_densityY != value)
                {
                    m_densityY = Mathf.Clamp(value, 2, int.MaxValue);
                }
            }
        }
        [SerializeField] private int m_densityY = 8;

        public int DensityZ
        {
            get { return m_densityZ; }
            set
            {
                if (m_densityZ != value)
                {
                    m_densityZ = Mathf.Clamp(value, 2, int.MaxValue);
                }
            }
        }
        [SerializeField] private int m_densityZ = 16;

        public bool m_useJitter = true;
        public float m_jitterAmount = 1f;

        public bool m_excludeMeshRendererBounds = true;
        public bool m_useIncludeVolumes = true;
        public bool m_testSeaLevel = true;
        public float m_seaLevel = 0f;
        public bool m_drawGizmos = true;
        public bool m_drawProbePositionGizmos = false;
        public Color m_boundsGizmoColor = new Color(0f, 0f, 1f, 0.25f);
        public Color m_probePositionGizmoColor = new Color(1f, 0f, 0f, 1f);
        public float m_probeGizmoSize = 0.1f;

        [SerializeField] private GameObject m_lastGeneratedProbes;
        [SerializeField] private float m_jitterValue;
        [SerializeField] private List<Vector3> m_probePositions = new List<Vector3>();
        [SerializeField] private List<HDRPTimeOfDayLightProbeIncludeVolume> m_includeVolumes = new List<HDRPTimeOfDayLightProbeIncludeVolume>();
        private List<MeshRenderer> m_renderers = new List<MeshRenderer>();
        private Vector3 m_lastPosition;

        private void OnEnable()
        {
            if (HDRPTimeOfDay.Instance != null)
            {
                HDRPTimeOfDay.Instance.AddLightProbeVolume(this);
            }
        }
        private void OnDisable()
        {
            if (HDRPTimeOfDay.Instance != null)
            {
                HDRPTimeOfDay.Instance.RemoveLightProbeVolume(this);
            }
        }
        /// <summary>
        /// Draws gizmos when the object is selected
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            if (!m_isEnabled || !m_drawGizmos)
            {
                return;
            }

            if (m_lastPosition != transform.position)
            {
                m_lastPosition = transform.position;
                BuildCachedPosition();
            }

            GetTransformSettings(out Vector3 pos, out Vector3 scale, out Quaternion rot);
            Gizmos.color = m_boundsGizmoColor;
            Gizmos.matrix = Matrix4x4.TRS(pos, rot, scale);
            Gizmos.DrawCube(Vector3.zero, Vector3.one);

            if (m_drawProbePositionGizmos)
            {
                Gizmos.color = m_probePositionGizmoColor;
                if (m_useIncludeVolumes && m_includeVolumes.Count > 0)
                {
                    foreach (HDRPTimeOfDayLightProbeIncludeVolume ignoreVolume in m_includeVolumes)
                    {
                        if (ignoreVolume == null)
                        {
                            continue;
                        }
                        ignoreVolume.RefreshBounds();
                    }
                }
                foreach (Vector3 position in m_probePositions)
                {
                    DrawGizmoProbePosition(position, scale, rot, m_probeGizmoSize);
                }
            }
        }

        /// <summary>
        /// Builds the light probes volume
        /// </summary>
        public void BuildVolumeProbes()
        {
#if UNITY_EDITOR
            if (m_isEnabled)
            {
                Refresh();
                if (m_lastGeneratedProbes != null)
                {
                    DestroyImmediate(m_lastGeneratedProbes);
                }

                if (m_probePositions.Count > 0)
                {
                    GameObject probeVolume = new GameObject(gameObject.name + " Probe Volume");
                    LightProbeGroup lightProbeGroup = probeVolume.AddComponent<LightProbeGroup>();
                    lightProbeGroup.dering = true;
                    lightProbeGroup.probePositions = m_probePositions.ToArray();
                    lightProbeGroup.transform.SetParent(GetParent());
                    m_lastGeneratedProbes = probeVolume;
                }
            }
#endif
        }
        /// <summary>
        /// Refreshes the ignore volumes
        /// </summary>
        public void Refresh()
        {
            BuildMeshRenderersInVolume();
            BuildCachedPosition();
        }
        /// <summary>
        /// Builds the cached positions
        /// </summary>
        public void BuildCachedPosition()
        {
            RefreshIncludeVolumes();
            GetTransformSettings(out Vector3 pos, out Vector3 scale, out Quaternion rot);
            m_jitterValue = UnityEngine.Random.Range(0f, m_jitterAmount);
            m_probePositions.Clear();
            m_probePositions = BuildProbePositions(pos, scale, true);
        }
        /// <summary>
        /// Adds the include volume
        /// </summary>
        /// <param name="volume"></param>
        public void AddIncludeVolume(HDRPTimeOfDayLightProbeIncludeVolume volume)
        {
            m_includeVolumes.Add(volume);
            Refresh();
        }
        /// <summary>
        /// Removes the include volume
        /// </summary>
        /// <param name="volume"></param>
        public void RemoveIncludeVolume(HDRPTimeOfDayLightProbeIncludeVolume volume)
        {
            m_includeVolumes.Remove(volume);
            Refresh();
        }
        /// <summary>
        /// Refreshes all the volumes in the scene
        /// </summary>
        public static void RefreshAllVolumes()
        {
            HDRPTimeOfDayLightProbeVolume[] volumes = GameObject.FindObjectsOfType<HDRPTimeOfDayLightProbeVolume>();
            foreach (HDRPTimeOfDayLightProbeVolume volume in volumes)
            {
                if (volume == null)
                {
                    continue;
                }
                volume.Refresh();
            }
        }
        /// <summary>
        /// Gets the parent transform
        /// </summary>
        /// <param name="parentName"></param>
        /// <returns></returns>
        public static Transform GetParent(string parentName = "Generated Light Probes")
        {
            GameObject lightProbe = GameObject.Find(parentName);
            if (lightProbe == null)
            {
                lightProbe = new GameObject(parentName);
            }

            return lightProbe.transform;
        }

        /// <summary>
        /// Draw probe position
        /// Only can be drawn in draw gizmos
        /// </summary>
        /// <param name="position"></param>
        /// <param name="scale"></param>
        /// <param name="rotation"></param>
        /// <param name="radius"></param>
        private void DrawGizmoProbePosition(Vector3 position, Vector3 scale, Quaternion rotation, float radius = 1f)
        {
            Gizmos.matrix = Matrix4x4.TRS(position, rotation, Vector3.one);
            Gizmos.DrawSphere(Vector3.zero, radius);
        }
        /// <summary>
        /// Collects the mesh renderers within the bounds
        /// </summary>
        private void BuildMeshRenderersInVolume()
        {
            m_renderers.Clear();
            if (m_excludeMeshRendererBounds)
            {
                Collider[] colliders = Physics.OverlapBox(transform.position, transform.localScale / 2f, transform.rotation);
                if (colliders.Length > 0)
                {
                    foreach (Collider col in colliders)
                    {
                        if (col == null)
                        {
                            continue;
                        }
                        MeshRenderer[] colRenderers = col.GetComponentsInChildren<MeshRenderer>();
                        if (colRenderers.Length > 0)
                        {
                            m_renderers.AddRange(colRenderers);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Builds the ignore volumes
        /// </summary>
        private void RefreshIncludeVolumes()
        {
            m_includeVolumes.Clear();
            if (m_useIncludeVolumes && HDRPTimeOfDay.Instance != null)
            {
                m_includeVolumes.AddRange(HDRPTimeOfDay.Instance.m_lightProbeIncludeVolumes);
                foreach (HDRPTimeOfDayLightProbeIncludeVolume volume in m_includeVolumes)
                {
                    if (volume == null)
                    {
                        continue;
                    }
                    volume.RefreshBounds();
                }
            }
        }
        /// <summary>
        /// Builds the volume probes positions
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="scale"></param>
        /// <returns></returns>
        private List<Vector3> BuildProbePositions(Vector3 pos, Vector3 scale, bool processJitter = false)
        {
            Vector3 currentPos = GetStartPosition();
            List<Vector3> positions = new List<Vector3>();
            for (int x = 0; x < m_densityX; x++)
            {
                for (int y = 0; y < m_densityY; y++)
                {
                    for (int z = 0; z < m_densityZ; z++)
                    {
                        currentPos.x = ((x + 1) * scale.x / m_densityX) - scale.x / m_densityX / 2f + pos.x - (scale.x / 2f);
                        currentPos.y = ((y + 1) * scale.y / m_densityY) - scale.y / m_densityY / 2f + pos.y - (scale.y / 2f);
                        currentPos.z = ((z + 1) * scale.z / m_densityZ) - scale.z / m_densityZ / 2f + pos.z - (scale.z / 2f);
                        if (processJitter)
                        {
                            ApplyJitter(ref currentPos);
                        }

                        bool addProbe = true;
                        if (m_testSeaLevel)
                        {
                            if (currentPos.y <= m_seaLevel)
                            {
                                addProbe = false;
                            }
                        }

                        if (m_excludeMeshRendererBounds)
                        {
                            foreach (MeshRenderer meshRenderer in m_renderers)
                            {
                                if (meshRenderer == null)
                                {
                                    continue;
                                }
                                Vector3 checkPos = meshRenderer.transform.InverseTransformPoint(currentPos);
                                if(meshRenderer.localBounds.Contains(checkPos))
                                {
                                    addProbe = false;
                                    break;
                                }
                            }
                        }

                        if (m_useIncludeVolumes)
                        {
                            foreach (HDRPTimeOfDayLightProbeIncludeVolume volume in m_includeVolumes)
                            {
                                if (volume == null)
                                {
                                    continue;
                                }
                                if (volume.PositionInBounds(currentPos, out bool isIgnore, true))
                                {
                                    if (isIgnore)
                                    {
                                        addProbe = false;
                                    }
                                    else
                                    {
                                        addProbe = true;
                                    }
                                    break;
                                }
                            }
                        }

                        if (addProbe)
                        {
                            positions.Add(currentPos);
                        }
                    }
                }
            }

            return positions;
        }
        /// <summary>
        /// Applies jitter to the position
        /// </summary>
        /// <param name="position"></param>
        private void ApplyJitter(ref Vector3 position)
        {
            if (m_useJitter)
            {
                bool xIsPositive = UnityEngine.Random.Range(0f, 100f) > 60f;
                bool zIsPositive = UnityEngine.Random.Range(0f, 100f) > 60f;
                //X
                if (xIsPositive)
                {
                    position.x += m_jitterValue;
                }
                else
                {
                    position.x -= m_jitterValue;
                }
                //Z
                if (zIsPositive)
                {
                    position.z += m_jitterValue;
                }
                else
                {
                    position.z -= m_jitterValue;
                }
            }
        }
        /// <summary>
        /// Gets the transform settings
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="scale"></param>
        /// <param name="rot"></param>
        private void GetTransformSettings(out Vector3 pos, out Vector3 scale, out Quaternion rot)
        {
            pos = transform.position;
            scale = transform.localScale;
            rot = transform.rotation;
        }
        /// <summary>
        /// Gets the start position
        /// </summary>
        /// <returns></returns>
        private Vector3 GetStartPosition()
        {
            GetTransformSettings(out Vector3 pos, out Vector3 scale, out Quaternion rot);
            Vector3 startPosition = pos;
            startPosition.x = pos.x - (scale.x / 2f);
            startPosition.y = pos.y - (scale.y / 2f);
            startPosition.z = pos.z - (scale.z / 2f);
            return startPosition;
        }
    }
}
#endif