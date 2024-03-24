#if HDPipeline && UNITY_2021_2_OR_NEWER
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralWorlds.HDRPTOD
{
    [Serializable]
    public class TreePrototypeBoundsScale
    {
        public int m_prototypeID;
        public float m_yScale;
    }

    public class HDRPTimeOfDayLightProbeGeneratorUtils
    {

        private static List<TreePrototypeBoundsScale> m_treePototypeBoundsData = new List<TreePrototypeBoundsScale>();
        private static LightProbeGeneratorSettings m_settings = new LightProbeGeneratorSettings();

        /// <summary>
        /// Generates all the light probes for all the generated mode on the terrain provided
        /// </summary>
        /// <param name="settings"></param>
        public static void GenerateLightProbes(LightProbeGeneratorSettings settings)
        {
            m_settings.CopySettings(settings);
            Terrain[] terrains = Terrain.activeTerrains;
            if (settings.m_processingMode == ProcessingMode.Advanced)
            {
                settings.m_processingMode = ProcessingMode.Simple;
                Debug.Log("Advanced mode is not yet supported");
            }

            switch (settings.m_generateMode)
            {
                case LightProbeGenerateMode.Trees:
                {
                    ProcessTrees(terrains);
                    break;
                }
                case LightProbeGenerateMode.TreesAdvanced:
                {
                    ProcessTreesAdvanced(terrains);
                    break;
                }
                case LightProbeGenerateMode.MeshRenderers:
                {
                    ProcessMeshRenderers();
                    break;
                }
                case LightProbeGenerateMode.Volumes:
                {
                    ProcessVolumes();
                    break;
                }
                case LightProbeGenerateMode.All:
                {
                    ProcessTrees(terrains);
                    ProcessTreesAdvanced(terrains);
                    ProcessVolumes();
                    ProcessMeshRenderers();
                    break;
                }
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
        /// Processes the terrain trees
        /// </summary>
        /// <param name="terrains"></param>
        private static void ProcessTrees(Terrain[] terrains)
        {
            foreach (Terrain terrain in terrains)
            {
                TreeInstance[] treeInstances = terrain.terrainData.treeInstances;
                if (treeInstances.Length > 0)
                {
                    m_treePototypeBoundsData.Clear();
                    GameObject lightProbeGroupObject = new GameObject("Tree Lights Probe Group");
                    LightProbeGroup group = lightProbeGroupObject.AddComponent<LightProbeGroup>();
                    List<Vector3> probePositions = new List<Vector3>();
                    for (int i = 0; i < treeInstances.Length; i++)
                    {
                        TreeInstance instance = treeInstances[i];
                        Vector3 convertedPosition = new Vector3(
                            (instance.position.x * terrain.terrainData.size.x) + terrain.transform.position.x,
                            instance.position.y * terrain.terrainData.size.y,
                            (instance.position.z * terrain.terrainData.size.z) + +terrain.transform.position.z);
                        float y = terrain.SampleHeight(convertedPosition);
                        y += (GetYTreeScale(terrain, instance.prototypeIndex) * instance.heightScale) - 0.5f;
                        if (m_settings.m_addHeightOffset)
                        {
                            y += m_settings.m_heightOffset;
                        }

                        Vector3 currentPos = new Vector3(convertedPosition.x, y + instance.heightScale + 0.1f,
                            convertedPosition.z);
                        if (ProbePositionDoesNotExists(probePositions, currentPos))
                        {
                            probePositions.Add(currentPos);
                        }
                    }

                    group.dering = true;
                    group.probePositions = probePositions.ToArray();
                    group.transform.SetParent(GetParent());
                }
            }
        }
        /// <summary>
        /// Processes the terrain trees
        /// </summary>
        /// <param name="terrains"></param>
        private static void ProcessTreesAdvanced(Terrain[] terrains)
        {
            foreach (Terrain terrain in terrains)
            {
                TreeInstance[] treeInstances = terrain.terrainData.treeInstances;
                if (treeInstances.Length > 0)
                {
                    m_treePototypeBoundsData.Clear();
                    GameObject lightProbeGroupObject = new GameObject("Details Lights Probe Group");
                    LightProbeGroup group = lightProbeGroupObject.AddComponent<LightProbeGroup>();
                    List<Vector3> probePositions = new List<Vector3>();
                    for (int i = 0; i < treeInstances.Length; i++)
                    {
                        TreeInstance instance = treeInstances[i];
                        Vector3 convertedPosition = new Vector3(
                            (instance.position.x * terrain.terrainData.size.x) + terrain.transform.position.x,
                            instance.position.y * terrain.terrainData.size.y,
                            (instance.position.z * terrain.terrainData.size.z) + +terrain.transform.position.z);
                        float y = terrain.SampleHeight(convertedPosition);
                        y += (GetYTreeScale(terrain, instance.prototypeIndex) * instance.heightScale) - 0.5f;
                        if (m_settings.m_addHeightOffset)
                        {
                            y += m_settings.m_heightOffset;
                        }

                        //Vector3 currentPos = new Vector3(convertedPosition.x, y + instance.heightScale + 0.1f, convertedPosition.z);
                        Vector3 currentPos = new Vector3(convertedPosition.x, y + (instance.heightScale / 4),
                            convertedPosition.z);
                        if (ProbePositionDoesNotExists(probePositions, currentPos))
                        {
                            probePositions.Add(currentPos);
                            probePositions.Add(currentPos + (Vector3.forward * m_settings.m_sphereOffset));
                            probePositions.Add(currentPos + (Vector3.right * m_settings.m_sphereOffset));
                            probePositions.Add(currentPos + (Vector3.left * m_settings.m_sphereOffset));
                            probePositions.Add(currentPos + (Vector3.back * m_settings.m_sphereOffset));
                            probePositions.Add(currentPos + (Vector3.up * m_settings.m_sphereOffset));
                            if (m_settings.m_spawnProbeUnderDetaial)
                            {
                                probePositions.Add(currentPos + (Vector3.down * m_settings.m_sphereOffset));
                            }

                            probePositions.Add(currentPos + ((Vector3.forward + Vector3.right) * m_settings.m_sphereOffset));
                            probePositions.Add(currentPos + ((Vector3.forward + Vector3.left) * m_settings.m_sphereOffset));
                            probePositions.Add(currentPos + ((Vector3.back + Vector3.right) * m_settings.m_sphereOffset));
                            probePositions.Add(currentPos + ((Vector3.back + Vector3.left) * m_settings.m_sphereOffset));
                        }
                    }

                    group.dering = true;
                    group.probePositions = probePositions.ToArray();
                    group.transform.SetParent(GetParent());
                }
            }
        }
        /// <summary>
        /// Processes all the probe volumes in the scene
        /// </summary>
        private static void ProcessVolumes()
        {
            HDRPTimeOfDayLightProbeVolume[] probeVolumes = GameObject.FindObjectsOfType<HDRPTimeOfDayLightProbeVolume>();
            if (probeVolumes.Length > 0)
            {
                foreach (HDRPTimeOfDayLightProbeVolume volume in probeVolumes)
                {
                    volume.BuildVolumeProbes();
                }
            }
        }
        /// <summary>
        /// Processes all the mesh renderers in the scene
        /// </summary>
        private static void ProcessMeshRenderers()
        {
            MeshRenderer[] renderers = GetAllSceneMeshRenderers();
            if (renderers.Length > 0)
            {
                GameObject lightProbeGroupObject = new GameObject("Mesh Renderers Light Probe Group");
                LightProbeGroup group = lightProbeGroupObject.AddComponent<LightProbeGroup>();
                List<Vector3> probePositions = new List<Vector3>();
                for (int i = 0; i < renderers.Length; i++)
                {
                    Bounds bounds = renderers[i].bounds;
                    Vector3 position = renderers[i].transform.position;
                    if (m_settings.m_addHeightOffset)
                    {
                        position.y += m_settings.m_heightOffset;
                    }

                    Vector3 currentPos = new Vector3(position.x, position.y + bounds.size.y + 0.1f, position.z);
                    if (ProbePositionDoesNotExists(probePositions, currentPos))
                    {
                        probePositions.Add(currentPos);
                    }
                }

                group.dering = true;
                group.probePositions = probePositions.ToArray();
                group.transform.SetParent(GetParent());
            }
        }
        /// <summary>
        /// Gets the 3 scale from the bounds of the prototype prefab and stores it for fast access while going through the tree instances
        /// This is reset when you re-generate the probes
        /// </summary>
        /// <param name="terrain"></param>
        /// <param name="treeID"></param>
        /// <returns></returns>
        private static float GetYTreeScale(Terrain terrain, int treeID)
        {
            if (terrain != null)
            {
                foreach (TreePrototypeBoundsScale data in m_treePototypeBoundsData)
                {
                    if (data.m_prototypeID == treeID)
                    {
                        return data.m_yScale;
                    }
                }

                TreePrototype prototype = terrain.terrainData.treePrototypes[treeID];
                if (prototype != null)
                {
                    GameObject prefab = prototype.prefab;
                    if (prefab != null)
                    {
                        MeshRenderer renderer = prefab.GetComponent<MeshRenderer>();
                        if (renderer == null)
                        {
                            renderer = prefab.GetComponentInChildren<MeshRenderer>();
                        }

                        if (renderer != null)
                        {
                            Bounds bounds = renderer.bounds;
                            m_treePototypeBoundsData.Add(new TreePrototypeBoundsScale
                            {
                                m_prototypeID = treeID,
                                m_yScale = bounds.size.y
                            });
                            return bounds.size.y;
                        }
                    }
                }
            }

            return 0f;
        }
        /// <summary>
        /// Gets all the mesh renderers in the scene
        /// </summary>
        /// <returns></returns>
        private static MeshRenderer[] GetAllSceneMeshRenderers()
        {
            return GameObject.FindObjectsOfType<MeshRenderer>();
        }
        /// <summary>
        /// Checks to see if the position does not exist
        /// </summary>
        /// <param name="currentPositions"></param>
        /// <param name="checkPosition"></param>
        /// <returns></returns>
        private static bool ProbePositionDoesNotExists(List<Vector3> currentPositions, Vector3 checkPosition)
        {
            if (currentPositions.Count < 1)
            {
                return true;
            }
            else
            {
                bool DoesNotExists = true;
                foreach (Vector3 currentPosition in currentPositions)
                {
                    if (currentPosition == checkPosition)
                    {
                        DoesNotExists = false;
                        break;
                    }
                }

                return DoesNotExists;
            }
        }
    }
}
#endif