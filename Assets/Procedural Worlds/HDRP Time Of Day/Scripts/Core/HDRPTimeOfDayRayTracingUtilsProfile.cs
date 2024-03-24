#if HDPipeline && UNITY_2021_2_OR_NEWER
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralWorlds.HDRPTOD
{
    public class HDRPTimeOfDayRayTracingUtilsProfile : ScriptableObject
    {
#if HDRPTIMEOFDAY
        public RayTracingOptimizationData m_optimizationData = new RayTracingOptimizationData(true);
#endif
        public List<GameObject> m_selectedGameObjects = new List<GameObject>();

#if HDRPTIMEOFDAY
        public bool Process(GameObject prefabObject, RayTracingOptimizationData rtxData)
        {
            if (prefabObject != null)
            {
                MeshRenderer[] meshRenderers = prefabObject.GetComponentsInChildren<MeshRenderer>();
                if (meshRenderers.Length > 0)
                {
                    HDRPTimeOfDayRayTracingUtils.OptimizeMeshesForRayTracing(rtxData, meshRenderers);
                    return true;
                }
            }

            return false;
        }
#endif
        public void AddGameObject(GameObject prefabObject)
        {
            if (!m_selectedGameObjects.Contains(prefabObject))
            {
                m_selectedGameObjects.Add(prefabObject);
            }
        }
        public void RemoveGameObject(GameObject prefabObject)
        {
            if (m_selectedGameObjects.Contains(prefabObject))
            {
                m_selectedGameObjects.Remove(prefabObject);
            }
        }
        public void Clear()
        {
            m_selectedGameObjects.Clear();
        }
    }
}
#endif