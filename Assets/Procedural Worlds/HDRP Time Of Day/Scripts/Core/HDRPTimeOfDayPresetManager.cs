using UnityEngine;

namespace ProceduralWorlds.HDRPTOD
{
    [ExecuteAlways]
    public class HDRPTimeOfDayPresetManager : MonoBehaviour
    {
        public static HDRPTimeOfDayPresetManager Instance
        {
            get { return m_instance; }
            set
            {
                if (m_instance != value)
                {
                    m_instance = value;
                }
            }
        }
        [SerializeField] private static HDRPTimeOfDayPresetManager m_instance;

        public bool AskBeforeApplying = true;
#if HDPipeline && UNITY_2021_2_OR_NEWER
        public HDRPTimeOfDayPresetProfile PresetProfile;
#endif

        private void OnEnable()
        {
            m_instance = this;
        }
    }
}