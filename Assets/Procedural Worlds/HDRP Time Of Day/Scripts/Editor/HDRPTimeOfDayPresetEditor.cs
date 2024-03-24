#if HDPipeline && UNITY_2021_2_OR_NEWER
using UnityEditor;
using UnityEngine;

namespace ProceduralWorlds.HDRPTOD
{
    [CustomEditor(typeof(HDRPTimeOfDayPresetManager))]
    public class HDRPTimeOfDayPresetEditor : Editor
    {
        private HDRPTimeOfDayPresetManager m_editor;
        private HDRPTimeOfDay m_timeOfDay;
        private string m_newName;
        private Texture2D m_newImage;
        private GUIStyle m_imageButtonStyle;
        private GUIStyle m_textButtonStyle;
        private GUIStyle m_boxStyle;

        private void OnEnable()
        {
            SetupEditor(false);
            m_newName = "";
            m_newImage = null;
        }

        public override void OnInspectorGUI()
        {
            SetupEditor(true);
            EditorGUI.BeginChangeCheck();
            GUIContent askBeforeContent = new GUIContent("Ask Before Applying", "If enabled it will display a popup asking if you are sure you want to apply as applying can not be undone.");
            m_editor.AskBeforeApplying = EditorGUILayout.Toggle(askBeforeContent, m_editor.AskBeforeApplying);
            m_editor.PresetProfile = (HDRPTimeOfDayPresetProfile)EditorGUILayout.ObjectField("Preset Profile", m_editor.PresetProfile, typeof(HDRPTimeOfDayPresetProfile), false);

            if (m_editor.PresetProfile != null)
            {
                if (m_editor.PresetProfile.TimeOfDayPresets.Count > 0 && m_timeOfDay)
                {
                    for (int i = 0; i < m_editor.PresetProfile.TimeOfDayPresets.Count; i++)
                    {
                        EditorGUILayout.BeginVertical(m_boxStyle);
                        DrawPresetPreview(m_editor.PresetProfile.TimeOfDayPresets[i]);
                        EditorGUILayout.EndVertical();
                        EditorGUILayout.Space();
                    }
                }
                else
                {
                    if (GUILayout.Button("Create Preset"))
                    {
                        m_editor.PresetProfile.TimeOfDayPresets.Add(new TimeOfDayPreset());
                    }
                }
            }
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(m_editor);
            }
        }

        private void DrawPresetPreview(TimeOfDayPreset preset, bool editable = false)
        {
            if (editable)
            {
                if (!string.IsNullOrEmpty(preset.PresetName))
                {
                    EditorGUILayout.BeginHorizontal();
                    if (!preset.RenameEnabled)
                    {
                        EditorGUILayout.LabelField("Preset: " + preset.PresetName);
                        if (GUILayout.Button("Rename"))
                        {
                            m_newName = preset.PresetName;
                            preset.RenameEnabled = true;
                        }
                    }
                    else
                    {
                        m_newName = EditorGUILayout.TextField("New Preset Name", m_newName);
                        if (GUILayout.Button("Save"))
                        {
                            preset.PresetName = m_newName;
                            m_newName = "";
                            preset.RenameEnabled = false;
                        }
                    }

                    EditorGUILayout.EndHorizontal();
                    if (preset.PresetImage != null)
                    {
                        if (!preset.ReplaceImageEnabled)
                        {
                            if (GUILayout.Button("Change Image"))
                            {
                                m_newImage = preset.PresetImage;
                                preset.ReplaceImageEnabled = true;
                            }

                            GUIContent guiContent = new GUIContent(preset.PresetImage, "Click this to apply the preset to your scene.");
                            Vector2 buttonSize = new Vector2(preset.PresetImage.width, preset.PresetImage.height);
                            buttonSize.x = Mathf.Clamp(buttonSize.x, EditorGUIUtility.currentViewWidth / 2f, EditorGUIUtility.currentViewWidth / 1.05f);
                            buttonSize.y = Mathf.Clamp(buttonSize.y, EditorGUIUtility.currentViewWidth / 4f, EditorGUIUtility.currentViewWidth / 2f);
                            if (GUILayout.Button(guiContent, GUILayout.MaxWidth(buttonSize.x), GUILayout.MaxHeight(buttonSize.y)))
                            {
                                if (EditorUtility.DisplayDialog("Applying Preset",
                                        "You are about to apply a preset, this can not be undone. Are you sure you want to proceed?",
                                        "Yes", "No"))
                                {
                                    m_editor.PresetProfile.ApplyPreset(m_timeOfDay, preset.PresetName);
                                    EditorGUIUtility.ExitGUI();
                                }
                            }
                        }
                        else
                        {
                            EditorGUILayout.BeginHorizontal();
                            GUIContent presetImageContent = new GUIContent("Preset Image", "Assigns a preset image to be displayed");
                            m_newImage = (Texture2D)EditorGUILayout.ObjectField(presetImageContent, m_newImage, typeof(Texture2D), false, GUILayout.MaxHeight(16f));
                            if (GUILayout.Button("Save Image"))
                            {
                                preset.PresetImage = m_newImage;
                                preset.ReplaceImageEnabled = false;
                            }
                            EditorGUILayout.EndHorizontal();
                        }
                    }
                    else
                    {
                        GUIContent presetImageContent = new GUIContent("Preset Image", "Assigns a preset image to be displayed");
                        preset.PresetImage = (Texture2D)EditorGUILayout.ObjectField(presetImageContent, preset.PresetImage, typeof(Texture2D), false, GUILayout.MaxHeight(16f));
                    }
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(preset.PresetName))
                {
                    EditorGUILayout.LabelField("Preset: " + preset.PresetName, m_textButtonStyle);
                    if (preset.PresetImage != null)
                    {
                        GUIContent guiContent = new GUIContent("", "Click this to apply the preset to your scene.");
                        Vector2 buttonSize = new Vector2(preset.PresetImage.width, preset.PresetImage.height);
                        buttonSize.x = Mathf.Clamp(buttonSize.x, EditorGUIUtility.currentViewWidth / 2f, EditorGUIUtility.currentViewWidth / 1.05f);
                        buttonSize.y = Mathf.Clamp(buttonSize.y, EditorGUIUtility.currentViewWidth / 4f, EditorGUIUtility.currentViewWidth / 2f);
                        m_imageButtonStyle.fixedWidth = buttonSize.x;
                        m_imageButtonStyle.fixedHeight = buttonSize.y;
                        m_imageButtonStyle.normal.background = preset.PresetImage;
                        if (GUILayout.Button(guiContent, m_imageButtonStyle, GUILayout.MaxWidth(buttonSize.x), GUILayout.MaxHeight(buttonSize.y)))
                        {
                            if (m_editor.AskBeforeApplying)
                            {
                                if (EditorUtility.DisplayDialog("Applying Preset",
                                        "You are about to apply a preset, this can not be undone. Are you sure you want to proceed?",
                                        "Yes", "No"))
                                {
                                    m_editor.PresetProfile.ApplyPreset(m_timeOfDay, preset.PresetName);
                                }
                            }
                            else
                            {
                                m_editor.PresetProfile.ApplyPreset(m_timeOfDay, preset.PresetName);
                            }

                            EditorGUIUtility.ExitGUI();
                        }
                    }
                }
            }

        }
        private void SetupEditor(bool guiCall)
        {
            if (m_editor == null)
            {
                m_editor = (HDRPTimeOfDayPresetManager)target;
            }

            if (m_timeOfDay == null)
            {
                m_timeOfDay = HDRPTimeOfDay.Instance;
            }

            if (guiCall)
            {
                if (m_imageButtonStyle == null)
                {
                    m_imageButtonStyle = new GUIStyle(GUI.skin.button)
                    {
                        normal =
                        {
                            background = null
                        }
                    };
                }

                if (m_textButtonStyle == null)
                {
                    m_textButtonStyle = new GUIStyle(EditorStyles.boldLabel)
                    {
                        fontSize = 14
                    };
                }

                if (m_boxStyle == null)
                {
                    m_boxStyle = new GUIStyle(GUI.skin.box)
                    {
                        normal = {textColor = GUI.skin.label.normal.textColor},
                        fontStyle = FontStyle.Bold,
                        alignment = TextAnchor.UpperLeft
                    };
                }
            }
        }
    }
}
#endif