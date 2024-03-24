#if HDRPTIMEOFDAY && HDPipeline
using UnityEditor;
using UnityEngine;

namespace ProceduralWorlds.HDRPTOD
{
    [CustomEditor(typeof(HDRPTimeOfDayLightController))]
    public class HDRPTimeOfDayLightControllerEditor : Editor
    {
        private HDRPTimeOfDayLightController m_editor;
        private GUIStyle m_boxStyle;

        public override void OnInspectorGUI()
        {
            m_editor = (HDRPTimeOfDayLightController)target;
            //Set up the box style
            if (m_boxStyle == null)
            {
                m_boxStyle = new GUIStyle(GUI.skin.box)
                {
                    normal = {textColor = GUI.skin.label.normal.textColor},
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.UpperLeft
                };
            }

            HDRPTimeOfDayLightControllerProfile profile = m_editor.LightInformation;

            EditorGUILayout.BeginVertical(m_boxStyle);
            profile = (HDRPTimeOfDayLightControllerProfile)EditorGUILayout.ObjectField(new GUIContent("Light Info Profile", "Profile containing all the controllable information for this."), profile, typeof(HDRPTimeOfDayLightControllerProfile), false);
            if (profile != m_editor.LightInformation)
            {
                m_editor.LightInformation = profile;
                m_editor.Refresh();
                EditorUtility.SetDirty(m_editor);
            }
            if (m_editor.LightInformation != null)
            {
                EditorGUI.BeginChangeCheck();
                EditorGUI.indentLevel++;
                m_editor.LightInformation.Intensity = EditorGUILayout.FloatField(new GUIContent("Light Intensity", "Sets the light intensity that will be used with the curve."), m_editor.LightInformation.Intensity);
                m_editor.LightInformation.IntensityMultiplierAnimationCurve = EditorGUILayout.CurveField(new GUIContent("Intensity Multiplier Curve", "Sets the multiplier curve for the time of day that will be applied to the light."), m_editor.LightInformation.IntensityMultiplierAnimationCurve);
                m_editor.LightInformation.CastShadows = EditorGUILayout.Toggle(new GUIContent("Cast Shadows", "Enables shadows on this light source."), m_editor.LightInformation.CastShadows);
                m_editor.LightInformation.LightShadowResolutionQuality = (LightShadowResolutionQuality)EditorGUILayout.EnumPopup(new GUIContent("Shadow Resolution", "Sets the shadow resolution on this light source."), m_editor.LightInformation.LightShadowResolutionQuality);
                EditorGUI.indentLevel--;
                if (EditorGUI.EndChangeCheck())
                {
                    EditorUtility.SetDirty(m_editor);
                }
            }
            EditorGUILayout.EndVertical();
        }
    }
}
#endif