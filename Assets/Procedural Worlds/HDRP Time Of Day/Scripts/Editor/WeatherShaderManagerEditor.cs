using UnityEditor;

namespace ProceduralWorlds.HDRPTOD
{
    [CustomEditor(typeof(WeatherShaderManager))]
    public class WeatherShaderManagerEditor : Editor
    {
        private WeatherShaderManager m_editor;

        private void OnEnable()
        {
            m_editor = (WeatherShaderManager)target;
            if (m_editor != null)
            {
                m_editor.m_shaderData = WeatherShaderManager.GetAllShaderValues();
            }
        }
        private void OnDestroy()
        {
            WeatherShaderManager.ResetAllWeatherPowerValues(m_editor.m_shaderData);
        }

        public override void OnInspectorGUI()
        {
            m_editor = (WeatherShaderManager)target;

            EditorGUI.BeginChangeCheck();

            DrawDefaultInspector();

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(m_editor);
                WeatherShaderManager.AllowGlobalShaderUpdates = true;
                WeatherShaderManager.ApplyAllShaderValues(m_editor.m_shaderData);
            }
        }
    }
}