#if HDPipeline && UNITY_2021_2_OR_NEWER
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralWorlds.HDRPTOD
{
    public enum MaterialSyncPropertyType { Float, Color, Int }
    public enum MaterialApplyType { TimeOfDaySync }

    [Serializable]
    public class TODMaterialSyncData
    {
        [Header("Global Settings")]
        public string m_name;
        public bool m_isDayMaterialData = true;
        public MaterialSyncPropertyType m_propertyType = MaterialSyncPropertyType.Float;

        [Header("Property Values")]
        public bool m_isSettingHDRPEmission = false;
        public string m_propertyName;
        public float m_floatValue;
        public int m_intValue;
        public Color m_colorValue;

        [Header("Materials")]
        public List<Material> m_materials = new List<Material>();

        /// <summary>
        /// Applies the material settings
        /// </summary>
        public void Apply()
        {
            if (string.IsNullOrEmpty(m_propertyName))
            {
                return;
            }

            foreach (Material material in m_materials)
            {
                if (!material.HasProperty(m_propertyName))
                {
                    continue;
                }

                switch (m_propertyType)
                {
                    case MaterialSyncPropertyType.Float:
                    {
                        material.SetFloat(m_propertyName, m_floatValue);
                        break;
                    }
                    case MaterialSyncPropertyType.Color:
                    {
                        if (m_isSettingHDRPEmission)
                        {
                            material.SetColor(m_propertyName, m_colorValue * m_floatValue);
                        }
                        else
                        {
                            material.SetColor(m_propertyName, m_colorValue);
                        }
                        break;
                    }
                    case MaterialSyncPropertyType.Int:
                    {
                        material.SetInt(m_propertyName, m_intValue);
                        break;
                    }
                }
            }
        }
    }

    [ExecuteAlways]
    public class HDRPTimeOfDayMaterialSync : MonoBehaviour
    {
        [Header("Data Settings")]
        public MaterialApplyType m_applyType = MaterialApplyType.TimeOfDaySync;
        public List<TODMaterialSyncData> m_materialData = new List<TODMaterialSyncData>();

        private bool m_lastIsDayValue = false;

        private void OnEnable()
        {
            if (HDRPTimeOfDay.Instance != null)
            {
                m_lastIsDayValue = HDRPTimeOfDay.Instance.AddMaterialSync(this);
            }
        }
        private void OnDisable()
        {
            if (HDRPTimeOfDay.Instance != null)
            {
                HDRPTimeOfDay.Instance.RemoveMaterialSync(this);
            }
        }

        /// <summary>
        /// Checks and processes to see if the materials need updating
        /// </summary>
        /// <param name="isDay"></param>
        /// <param name="forceApply"></param>
        public void Process(bool isDay, bool forceApply = false)
        {
            if (!forceApply)
            {
                if (isDay != m_lastIsDayValue)
                {
                    ApplyMaterials(isDay);
                }
            }
            else
            {
                ApplyMaterials(isDay);
            }
        }

        /// <summary>
        /// Applies the materials
        /// </summary>
        /// <param name="isDay"></param>
        private void ApplyMaterials(bool isDay)
        {
            m_lastIsDayValue = isDay;
            if (m_applyType == MaterialApplyType.TimeOfDaySync)
            {
                foreach (TODMaterialSyncData data in m_materialData)
                {
                    if (data.m_isDayMaterialData == isDay)
                    {
                        data.Apply();
                    }
                }
            }
        }
    }
}
#endif