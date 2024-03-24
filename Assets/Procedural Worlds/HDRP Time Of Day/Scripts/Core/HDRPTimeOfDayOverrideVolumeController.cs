#if HDPipeline && UNITY_2021_2_OR_NEWER
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ProceduralWorlds.HDRPTOD
{
    [ExecuteAlways]
    public class HDRPTimeOfDayOverrideVolumeController : MonoBehaviour
    {
        public static HDRPTimeOfDayOverrideVolumeController Instance
        {
            get { return m_instance; }
        }
        [SerializeField] private static HDRPTimeOfDayOverrideVolumeController m_instance;

        public List<HDRPTimeOfDayOverrideVolume> m_dayTimeOverrideVolumes = new List<HDRPTimeOfDayOverrideVolume>();
        public List<HDRPTimeOfDayOverrideVolume> m_nightTimeOverrideVolumes = new List<HDRPTimeOfDayOverrideVolume>();

        [SerializeField, HideInInspector] private bool m_lastIsDayValue = false;

        private void OnEnable()
        {
            m_instance = this;
            CheckState(true);
#if UNITY_EDITOR
            if (Application.isPlaying)
            {
                EditorApplication.update -= EditorUpdate;
            }
            else
            {
                EditorApplication.update -= EditorUpdate;
                EditorApplication.update += EditorUpdate;
            }
#endif
        }
        private void OnDisable()
        {
#if UNITY_EDITOR
            EditorApplication.update -= EditorUpdate;
#endif
        }
        private void OnDestroy()
        {
#if UNITY_EDITOR
            EditorApplication.update -= EditorUpdate;
#endif
        }
        private void LateUpdate()
        {
            if (Application.isPlaying)
            {
                CheckState();
            }
        }

        public void AddOverrideVolume(HDRPTimeOfDayOverrideVolume volume, OverrideTODType type, bool cleanUpMissing = true)
        {
            if (volume != null)
            {
                switch (type)
                {
                    case OverrideTODType.Day:
                    {
                        if (!m_dayTimeOverrideVolumes.Contains(volume))
                        {
                            m_dayTimeOverrideVolumes.Add(volume);
                        }
                        if (m_nightTimeOverrideVolumes.Contains(volume))
                        {
                            m_nightTimeOverrideVolumes.Remove(volume);
                        }
                        break;
                    }
                    case OverrideTODType.Night:
                    {
                        if (!m_nightTimeOverrideVolumes.Contains(volume))
                        {
                            m_nightTimeOverrideVolumes.Add(volume);
                        }
                        if (m_dayTimeOverrideVolumes.Contains(volume))
                        {
                            m_dayTimeOverrideVolumes.Remove(volume);
                        }
                        break;
                    }
                }

                if (cleanUpMissing)
                {
                    CleanUpMissingVolumes(type, true);
                }
                CheckState(true);
            }
        }
        public void RemoveOverrideVolume(HDRPTimeOfDayOverrideVolume volume, OverrideTODType type, bool cleanUpMissing = true)
        {
            if (volume != null)
            {
                switch (type)
                {
                    case OverrideTODType.Day:
                    {
                        if (m_dayTimeOverrideVolumes.Contains(volume))
                        {
                            m_dayTimeOverrideVolumes.Remove(volume);
                        }
                        break;
                    }
                    case OverrideTODType.Night:
                    {
                        if (m_nightTimeOverrideVolumes.Contains(volume))
                        {
                            m_nightTimeOverrideVolumes.Remove(volume);
                        }
                        break;
                    }
                }
                if (cleanUpMissing)
                {
                    CleanUpMissingVolumes(type, true);
                }
                CheckState(true);
            }
        }
        public void CheckState(bool overrideApply = false)
        {
            HDRPTimeOfDay timeOfDay = HDRPTimeOfDay.Instance;
            if (timeOfDay != null)
            {
                if (!timeOfDay.UseOverrideVolumes)
                {
                    return;
                }

                bool isDay = timeOfDay.IsDayTime();
                if (overrideApply)
                {
                    m_lastIsDayValue = isDay;
                    ProcessVolumes(isDay);
                    timeOfDay.ResetOverrideVolumeBlendTime(true);
                }
                else
                {
                    if (isDay != m_lastIsDayValue)
                    {
                        m_lastIsDayValue = isDay;
                        ProcessVolumes(isDay);
                        timeOfDay.ResetOverrideVolumeBlendTime(true);
                    }
                }
            }
        }

        private void SetupOverrideVolumes(HDRPTimeOfDayOverrideVolume volumeObject)
        {
            if (volumeObject != null)
            {
                volumeObject.LoadVolume();
            }
        }

        private void CleanUpMissingVolumes(OverrideTODType type, bool cleanAll = false)
        {
            if (!cleanAll)
            {
                switch (type)
                {
                    case OverrideTODType.Day:
                    {
                        for (int i = m_dayTimeOverrideVolumes.Count; i --> 0;)
                        {
                            if (m_dayTimeOverrideVolumes[i] == null)
                            {
                                m_dayTimeOverrideVolumes.RemoveAt(i);
                            }
                        }
                        break;
                    }
                    case OverrideTODType.Night:
                    {
                        for (int i = m_nightTimeOverrideVolumes.Count; i --> 0;)
                        {
                            if (m_nightTimeOverrideVolumes[i] == null)
                            {
                                m_nightTimeOverrideVolumes.RemoveAt(i);
                            }
                        }
                        break;
                    }
                }
            }
            else
            {
                //Day
                for (int i = m_dayTimeOverrideVolumes.Count; i --> 0;)
                {
                    if (m_dayTimeOverrideVolumes[i] == null)
                    {
                        m_dayTimeOverrideVolumes.RemoveAt(i);
                    }
                }
                //Night
                for (int i = m_nightTimeOverrideVolumes.Count; i --> 0;)
                {
                    if (m_nightTimeOverrideVolumes[i] == null)
                    {
                        m_nightTimeOverrideVolumes.RemoveAt(i);
                    }
                }
            }
        }
        private void ProcessVolumes(bool isDay)
        {
            if (isDay)
            {
                if (m_dayTimeOverrideVolumes.Count > 0)
                {
                    foreach (HDRPTimeOfDayOverrideVolume overrideVolumeDay in m_dayTimeOverrideVolumes)
                    {
                        if (overrideVolumeDay != null)
                        {
                            overrideVolumeDay.enabled = true;
                            SetupOverrideVolumes(overrideVolumeDay);
                            overrideVolumeDay.ApplyLocalFogVolume(true);
                        }
                    }
                }
                if (m_nightTimeOverrideVolumes.Count > 0)
                {
                    foreach (HDRPTimeOfDayOverrideVolume overrideVolumeNight in m_nightTimeOverrideVolumes)
                    {
                        if (overrideVolumeNight != null)
                        {
                            overrideVolumeNight.enabled = false;
                            overrideVolumeNight.ApplyLocalFogVolume(true);
                        }
                    }
                }
            }
            else
            {
                if (m_dayTimeOverrideVolumes.Count > 0)
                {
                    foreach (HDRPTimeOfDayOverrideVolume overrideVolumeDay in m_dayTimeOverrideVolumes)
                    {
                        if (overrideVolumeDay != null)
                        {
                            overrideVolumeDay.enabled = false;
                            overrideVolumeDay.ApplyLocalFogVolume(true);
                        }
                    }
                }
                if (m_nightTimeOverrideVolumes.Count > 0)
                {
                    foreach (HDRPTimeOfDayOverrideVolume overrideVolumeNight in m_nightTimeOverrideVolumes)
                    {
                        if (overrideVolumeNight != null)
                        {
                            overrideVolumeNight.enabled = true;
                            SetupOverrideVolumes(overrideVolumeNight);
                            overrideVolumeNight.ApplyLocalFogVolume(true);
                        }
                    }
                }
            }
        }
        private void EditorUpdate()
        {
            CheckState();
        }
    }
}
#endif