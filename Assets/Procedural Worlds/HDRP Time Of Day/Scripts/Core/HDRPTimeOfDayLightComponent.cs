#if HDPipeline
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace ProceduralWorlds.HDRPTOD
{
    public enum LightSyncRenderMode { NightOnly, AlwaysOn, AlwaysOnOptimized }

    [Serializable]
    public class RTLightOptimization : IComparable<RTLightOptimization>
    {
        public HDRPTimeOfDayLightComponent Component;
        public float Distance { get; set; }
        public bool RTCheckEnabled;

        public int CompareTo(RTLightOptimization other)
        {
            if (other == null)
            {
                return 1;
            }
            else
            {
                return this.Distance.CompareTo(other.Distance);
            }
        }
    }

    [ExecuteAlways]
    public class HDRPTimeOfDayLightComponent : MonoBehaviour
    {
        public bool m_addLightToTimeOfDay = true;
        public LightSyncRenderMode m_renderMode = LightSyncRenderMode.NightOnly;
        public bool m_useRenderDistance = true;
        public float m_renderDistance = 150f;
        public bool m_turnOffDelay = true;
        public bool m_includeForRTOptimizationCheck = true;
        public float m_delayTimer = 2f;
        public Light m_lightSource;
        public HDAdditionalLightData m_lightData;
        public HDRPTimeOfDay m_timeOfDay;
        private bool m_validated = false;

        public void OnEnable()
        {
            if (m_lightSource == null)
            {
                m_lightSource = GetComponent<Light>();
            }

            if (m_lightSource != null)
            {
                if (m_lightData == null)
                {
                    m_lightData = m_lightSource.GetComponent<HDAdditionalLightData>();
                }

                if (m_lightData != null)
                {
                    m_validated = true;
                }
                else
                {
                    m_validated = false;
                }
            }
            else
            {
                m_validated = false;
            }

            m_timeOfDay = HDRPTimeOfDay.Instance;
            if (m_timeOfDay != null)
            {
                m_timeOfDay.AddLightSyncComponent(this);
                SetRenderState(!m_timeOfDay.IsDayTime());

                if (m_validated)
                {
                    if (m_addLightToTimeOfDay)
                    {
                        m_timeOfDay.AddAdditionalLight(m_lightData);
                    }
                    else
                    {
                        m_timeOfDay.RemoveAdditionalLight(m_lightData);
                    }
                }
            }
        }
        public void OnDisable()
        {
            if (m_timeOfDay != null)
            {
                m_timeOfDay.RemoveLightSyncComponent(this);
                if (m_validated)
                {
                    m_timeOfDay.RemoveAdditionalLight(m_lightData);
                }
            }
        }

        /// <summary>
        /// Refreshes the system
        /// </summary>
        public void Refresh()
        {
            if (m_timeOfDay != null)
            {
                SetRenderState(!m_timeOfDay.IsDayTime());
            }
        }
        /// <summary>
        /// Refreshes the light souce
        /// </summary>
        public void RefreshLightSource()
        {
            m_lightData = null;
            OnEnable();
        }
        /// <summary>
        /// Sets the render state
        /// </summary>
        /// <param name="value"></param>
        public void SetRenderState(bool value)
        {
            if (m_validated)
            {
                switch (m_renderMode)
                {
                    case LightSyncRenderMode.NightOnly:
                    {
                        if (!m_turnOffDelay)
                        {
                            m_lightSource.enabled = value;
                            m_lightData.enabled = value;
                        }
                        else
                        {
                            if (!value)
                            {
                                StopCoroutine(TurnOffDelay(value));
                                if (Application.isPlaying)
                                {
                                    StartCoroutine(TurnOffDelay(value));
                                }
                                else
                                {
                                    m_lightSource.enabled = value;
                                    m_lightData.enabled = value;
                                }
                            }
                            else
                            {
                                StopCoroutine(TurnOffDelay(value));
                                m_lightSource.enabled = value;
                                m_lightData.enabled = value;
                            }
                        }
                        break;
                    }
                    case LightSyncRenderMode.AlwaysOnOptimized:
                    {
                        m_lightSource.enabled = true;
                        m_lightData.enabled = true;
                        break;
                    }
                    default:
                    {
                        m_lightSource.enabled = true;
                        m_lightData.enabled = true;
                        break;
                    }
                }
            }
        }
        /// <summary>
        /// Sets the culling state if it's enabled
        /// </summary>
        /// <param name="value"></param>
        /// <param name="player"></param>
        public RTLightOptimization SetCullingState(bool value, Transform player, float lodBias)
        {
            if (m_useRenderDistance && m_validated || m_renderMode == LightSyncRenderMode.AlwaysOnOptimized && m_validated)
            {
                if (m_renderMode == LightSyncRenderMode.NightOnly && value)
                {
                    m_lightSource.enabled = false;
                    m_lightData.enabled = false;
                    return null;
                }

                if (player != null)
                {
                    float distance = Vector3.Distance(transform.position, player.position);
                    distance /= lodBias;
                    if (distance > m_renderDistance)
                    {
                        m_lightSource.enabled = false;
                        m_lightData.enabled = false;
                        return null;
                    }
                    else
                    {
                        m_lightSource.enabled = true;
                        m_lightData.enabled = true;
                        if (m_includeForRTOptimizationCheck)
                        {
                            return new RTLightOptimization { Component = this, Distance = distance, RTCheckEnabled = m_includeForRTOptimizationCheck };
                        }
                    }
                }
            }

            return null;
        }
        /// <summary>
        /// Sets the include for ray tracing bool for this light source
        /// </summary>
        /// <param name="value"></param>
        public void SetLightIncludeForRT(bool value, bool rtShadows)
        {
            if (m_lightData != null)
            {
                m_lightData.includeForRayTracing = value;
                m_lightData.useRayTracedShadows = value ? rtShadows : false;
            }
        }
        /// <summary>
        /// Updates the RT lights to optimize the ray tracing on light sources like point and spot lights
        /// </summary>
        /// <param name="optimizationProcesses"></param>
        /// <param name="maxRayTracedLightCount"></param>
#if HDRPTIMEOFDAY
        public static void UpdateRTOptimization(List<RTLightOptimization> optimizationProcesses, RTGlobalQualitySettings rtSettings, RayTraceSettings rtGlobalSettings)
        {
            if (optimizationProcesses.Count > 0)
            {
                //optimizationProcesses.RemoveAll(x => x.Distance > rtSettings.MaxRenderDistanceCheck);
                if (rtSettings.SortClosesLightSources)
                {
                    optimizationProcesses.Sort();
                }

                int currentCount = 0;
                for (int i = 0; i < optimizationProcesses.Count; i++)
                {
                    bool allowRTLight = currentCount < rtSettings.MaxRTAdditionalLightSources;
                    if (allowRTLight)
                    {
                        allowRTLight = optimizationProcesses[i].Distance <= rtSettings.MaxRenderDistanceCheck;
                    }

                    if (allowRTLight)
                    {
                        currentCount++;
                    }

                    optimizationProcesses[i].Component.SetLightIncludeForRT(allowRTLight, rtGlobalSettings.m_rayTraceShadows);
                }
            }
        }
#endif
        /// <summary>
        /// Turn off delay allows the lights to remin on for x seconds before turning off
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private IEnumerator TurnOffDelay(bool value)
        {
            yield return new WaitForSeconds(m_delayTimer);
            m_lightSource.enabled = value;
            m_lightData.enabled = value;
        }
    }
}
#endif