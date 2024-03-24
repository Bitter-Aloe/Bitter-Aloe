#if HDRPTIMEOFDAY && HDPipeline
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace ProceduralWorlds.HDRPTOD
{
    [CustomEditor(typeof(HDRPTimeOfDayInteriorLightingManager))]
    public class HDRPTimeOfDayInteriorLightingManagerEditor : Editor
    {
        private HDRPTimeOfDayInteriorLightingManager m_manager;
        private GUIStyle m_boxStyle;

        public override void OnInspectorGUI()
        {
            m_manager = (HDRPTimeOfDayInteriorLightingManager)target;
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

            EditorGUILayout.BeginVertical(m_boxStyle);
            m_manager.Render = EditorGUILayout.Toggle("Render", m_manager.Render);
            if (m_manager.Render)
            {
                EditorGUI.indentLevel++;
                m_manager.InteriorBounds = EditorGUILayout.Vector3Field("Interior Bounds", m_manager.InteriorBounds);
                EditorGUILayout.BeginHorizontal();
                m_manager.Profile = (HDRPTimeOfDayInteriorLightingProfile)EditorGUILayout.ObjectField("Lighting Profile", m_manager.Profile, typeof(HDRPTimeOfDayInteriorLightingProfile), false);
                if (GUILayout.Button("New", GUILayout.MaxWidth(40f)))
                {
                    m_manager.Profile = CreateNewProfile();
                    m_manager.Refresh();
                }
                EditorGUILayout.EndHorizontal();

                if (m_manager.Profile != null)
                {
                    DrawProfileSettings(m_manager.Profile);
                }

                EditorGUI.indentLevel--;
                if (GUILayout.Button("Refresh"))
                {
                    m_manager.Refresh();
                }
                if (GUILayout.Button("Remove Manager"))
                {
                    m_manager.RemoveManager();
                }
            }
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Draws the profile settings editor
        /// </summary>
        /// <param name="profile"></param>
        private void DrawProfileSettings(HDRPTimeOfDayInteriorLightingProfile profile)
        {
            if (profile != null)
            {
                EditorGUI.BeginChangeCheck();

                EditorGUI.indentLevel++;
                //Volume
                EditorGUILayout.BeginVertical(m_boxStyle);
                profile.m_volumeFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(profile.m_volumeFoldout, "Volume Settings");
                if (profile.m_volumeFoldout)
                {
                    m_manager.InteriorReverbPreset = (AudioReverbPreset)EditorGUILayout.EnumPopup("Interior Reverb Preset", m_manager.InteriorReverbPreset);
                    profile.m_volumeProfile = (VolumeProfile)EditorGUILayout.ObjectField("Volume Profile", profile.m_volumeProfile, typeof(VolumeProfile), false);
                    profile.m_volumeProfileRTX = (VolumeProfile)EditorGUILayout.ObjectField("Volume Profile RTX", profile.m_volumeProfileRTX, typeof(VolumeProfile), false);
                }
                EditorGUILayout.EndFoldoutHeaderGroup();
                EditorGUILayout.EndVertical();
                //Fog
                EditorGUILayout.BeginVertical(m_boxStyle);
                profile.m_fogFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(profile.m_fogFoldout, "Fog Settings");
                if (profile.m_fogFoldout)
                {
                    profile.m_fogColor = EditorGUILayout.ColorField("Color", profile.m_fogColor);
                    profile.m_fogDistance = Mathf.Clamp(EditorGUILayout.FloatField("Distance", profile.m_fogDistance), 0.05f, float.PositiveInfinity);
#if UNITY_2022_3_OR_NEWER
                    profile.m_fogBlendingMode = (LocalVolumetricFogBlendingMode)EditorGUILayout.EnumPopup("Blending Mode", profile.m_fogBlendingMode);
#endif
                    profile.m_fogPriority = EditorGUILayout.IntField("Priority", profile.m_fogPriority);
                    profile.m_fogScrollSpeed = EditorGUILayout.Vector3Field("Scroll Speed", profile.m_fogScrollSpeed);
                    profile.m_fogTiling = EditorGUILayout.Vector3Field("Tiling", profile.m_fogTiling);
                    profile.m_fogTexture = (Texture3D)EditorGUILayout.ObjectField("Texture (3D)", profile.m_fogTexture, typeof(Texture3D), false, GUILayout.MaxHeight(16f));
                }
                EditorGUILayout.EndFoldoutHeaderGroup();
                EditorGUILayout.EndVertical();
                //Sun
                EditorGUILayout.BeginVertical(m_boxStyle);
                profile.m_sunFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(profile.m_sunFoldout, "Sun Settings");
                if (profile.m_sunFoldout)
                {
                    profile.m_sunIntensity = EditorGUILayout.Slider("Intensity", profile.m_sunIntensity, 0f, 120000f);
                    profile.m_sunIntensityRTX = EditorGUILayout.Slider("Intensity RTX", profile.m_sunIntensityRTX, 0f, 120000f);
                    profile.m_sunTemperature = EditorGUILayout.Slider("Temperature", profile.m_sunTemperature, 0f, 20000f);
                }
                EditorGUILayout.EndFoldoutHeaderGroup();
                EditorGUILayout.EndVertical();
                //Probe
                EditorGUILayout.BeginVertical(m_boxStyle);
                profile.m_probeFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(profile.m_probeFoldout, "Reflection Probe Settings");
                if (profile.m_probeFoldout)
                {
                    profile.m_probeCubemap = (Cubemap)EditorGUILayout.ObjectField("Cubemap", profile.m_probeCubemap, typeof(Cubemap), false, GUILayout.MaxHeight(16f));
                    profile.m_probeIntensity = Mathf.Clamp(EditorGUILayout.FloatField("Intensity", profile.m_probeIntensity), 0f, float.PositiveInfinity);
                    profile.m_probeIntensityRTX = Mathf.Clamp(EditorGUILayout.FloatField("Intensity RTX", profile.m_probeIntensityRTX), 0f, float.PositiveInfinity);
                }
                EditorGUILayout.EndFoldoutHeaderGroup();
                EditorGUILayout.EndVertical();
                EditorGUI.indentLevel--;

                if (EditorGUI.EndChangeCheck())
                {
                    EditorUtility.SetDirty(profile);
                    if (m_manager != null)
                    {
                        m_manager.Refresh();
                        EditorUtility.SetDirty(m_manager);
                    }
                }
            }
        }
        /// <summary>
        /// Creates a new profile
        /// </summary>
        /// <returns></returns>
        private HDRPTimeOfDayInteriorLightingProfile CreateNewProfile()
        {
            HDRPTimeOfDayInteriorLightingProfile asset = ScriptableObject.CreateInstance<HDRPTimeOfDayInteriorLightingProfile>();
            string date = string.Format("{0}_{1}", DateTime.Now.Date.ToShortDateString(), DateTime.Now.ToShortTimeString());
            date = date.Replace("/", "_");
            date = date.Replace(":", "_");
            string path = "Assets/Interior Lighting Profile_" + date + ".asset";
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            return AssetDatabase.LoadAssetAtPath<HDRPTimeOfDayInteriorLightingProfile>(path);
        }
    }
}
#endif