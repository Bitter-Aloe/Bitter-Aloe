#if HDPipeline && UNITY_2021_2_OR_NEWER
using UnityEditor;

namespace ProceduralWorlds.HDRPTOD
{
    [CustomEditor(typeof(HDRPTimeOfDayWeatherProfile))]
    public class HDRPTimeOfDayWeatherProfileEditor : Editor
    {
        private HDRPTimeOfDayWeatherProfile m_profile;

        public override void OnInspectorGUI()
        {
            m_profile = (HDRPTimeOfDayWeatherProfile)target;

            EditorGUI.BeginChangeCheck();
            DrawDefaultInspector();
            if (EditorGUI.EndChangeCheck())
            {
                HDRPTimeOfDay timeOfDay = HDRPTimeOfDay.Instance;
                if (timeOfDay != null)
                {
                    if (timeOfDay.WeatherActive(out HDRPTimeOfDayWeatherProfile asset))
                    {
                        if (asset != null)
                        {
                            if (asset == m_profile)
                            {
                                WeatherShaderManager.AllowGlobalShaderUpdates = true;
                                WeatherShaderManager.ApplyAllShaderValues(m_profile.WeatherShaderData);
                            }
                        }
                    }
                }
            }
        }
    }
}
#endif