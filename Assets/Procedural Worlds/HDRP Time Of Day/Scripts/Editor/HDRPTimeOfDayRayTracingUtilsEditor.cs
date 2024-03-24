#if HDPipeline
using System.Collections;
using System.Collections.Generic;
using PWCommon5;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace ProceduralWorlds.HDRPTOD
{
    public class HDRPTimeOfDayRayTracingUtilsEditor : EditorWindow
    {
        public static HDRPTimeOfDayRayTracingUtilsProfile m_profile;
        private GUIStyle gpanel;

        // Add menu named "My Window" to the Window menu
        [MenuItem("Window/" + PWConst.COMMON_MENU + "/HDRP Time Of Day/Open Ray Tracing Utils...", false, 40)]
        public static void OpenWindow()
        {
            // Get existing open window or if none, make a new one:
            HDRPTimeOfDayRayTracingUtilsEditor window = (HDRPTimeOfDayRayTracingUtilsEditor)EditorWindow.GetWindow(typeof(HDRPTimeOfDayRayTracingUtilsEditor));
            window.Show();
        }

        private void OnEnable()
        {
            m_profile = AssetDatabase.LoadAssetAtPath<HDRPTimeOfDayRayTracingUtilsProfile>("Assets/Procedural Worlds/HDRP Time Of Day/Content Resources/Settings/HDRP Time Of Day RTX Utils Profile.asset");
        }
        private void OnGUI()
        {
            if (gpanel == null)
            {
                gpanel = new GUIStyle(GUI.skin.box)
                {
                    normal =
                    {
                        textColor = GUI.skin.label.normal.textColor
                    },
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.UpperLeft
                };
            }

            m_profile = (HDRPTimeOfDayRayTracingUtilsProfile)EditorGUILayout.ObjectField("Profile", m_profile, typeof(HDRPTimeOfDayRayTracingUtilsProfile), false);
            if (m_profile != null)
            {
                SettingsGUI();
                DrawPrefabGUI();
                DrawSelectedGameObjects();
            }
        }

        /// <summary>
        /// Handle drop area for new objects
        /// </summary>
        private bool DrawPrefabGUI()
        {
            // Ok - set up for drag and drop
            Event evt = Event.current;
            Rect dropArea = GUILayoutUtility.GetRect(0.0f, 50.0f, GUILayout.ExpandWidth(true));
            string dropMsg = "Drag and drop prefabs";
            GUI.Box(dropArea, dropMsg, gpanel);
            if (evt.type == EventType.DragPerform || evt.type == EventType.DragUpdated)
            {
                if (!dropArea.Contains(evt.mousePosition))
                    return false;
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();
                    // Handle game objects / prefabs
                    foreach (Object draggedObject in DragAndDrop.objectReferences)
                    {
                        if (draggedObject is GameObject go)
                        {
                            if (PrefabUtility.IsPartOfPrefabAsset(go))
                            {
                                m_profile.AddGameObject(go);
                            }
                        }
                    }

                    foreach (string path in DragAndDrop.paths)
                    {
                        //Add folder stupports
                    }

                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Draws the selected gameobjects list
        /// </summary>
        private void DrawSelectedGameObjects()
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Optimize Assets"))
            {
                if (m_profile != null)
                {
                    Undo.RecordObjects(m_profile.m_selectedGameObjects.ToArray(), "RTX Scene Optimize");
                    for (int i = 0; i < m_profile.m_selectedGameObjects.Count; i++)
                    {
                        string path = AssetDatabase.GetAssetPath(m_profile.m_selectedGameObjects[i]);
                        GameObject prefabContents = PrefabUtility.LoadPrefabContents(path);
#if HDRPTIMEOFDAY
                        if (m_profile.Process(prefabContents, m_profile.m_optimizationData))
                        {
                            PrefabUtility.SaveAsPrefabAsset(prefabContents, path);
                        }
#endif
                    }
                }
            }

            EditorGUI.BeginChangeCheck();
            if (GUILayout.Button("Clear Current List"))
            {
                m_profile.Clear();
                EditorUtility.SetDirty(m_profile);
                EditorGUIUtility.ExitGUI();
            }
            EditorGUILayout.EndHorizontal();

            if (m_profile.m_selectedGameObjects.Count > 0)
            {
                for (int i = 0; i < m_profile.m_selectedGameObjects.Count; i++)
                {
                    GameObject selected = m_profile.m_selectedGameObjects[i];
                    if (selected == null)
                    {
                        continue;
                    }

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(selected.name);
                    if (GUILayout.Button("Select"))
                    {
                        Selection.activeObject = selected;
                    }

                    if (GUILayout.Button("Remove"))
                    {
                        m_profile.RemoveGameObject(selected);
                        EditorUtility.SetDirty(m_profile);
                        EditorGUIUtility.ExitGUI();
                    }

                    EditorGUILayout.EndHorizontal();
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(m_profile);
            }
        }
        /// <summary>
        /// Settings gui
        /// </summary>
        private void SettingsGUI()
        {
            EditorGUI.BeginChangeCheck();
#if HDRPTIMEOFDAY
            m_profile.m_optimizationData.m_ignoreIfAnimatorFound = EditorGUILayout.Toggle("Ignore If Animator Found", m_profile.m_optimizationData.m_ignoreIfAnimatorFound);
            m_profile.m_optimizationData.m_setStaticShadowCasting = EditorGUILayout.Toggle("Static Shadow Casting", m_profile.m_optimizationData.m_setStaticShadowCasting);
            m_profile.m_optimizationData.m_setObjectLayer = EditorGUILayout.Toggle("Set Object Layer", m_profile.m_optimizationData.m_setObjectLayer);
            m_profile.m_optimizationData.m_nonRayTracedObjectScale = EditorGUILayout.FloatField("Non RTX Object Scale", m_profile.m_optimizationData.m_nonRayTracedObjectScale);
            m_profile.m_optimizationData.m_nonRayTracedLayer = EditorGUILayout.TextField("Non RTX Layer", m_profile.m_optimizationData.m_nonRayTracedLayer);
            m_profile.m_optimizationData.m_staticRayTracingMode = (RayTracingMode)EditorGUILayout.EnumPopup("Static Object RTX Mode", m_profile.m_optimizationData.m_staticRayTracingMode);
            m_profile.m_optimizationData.m_dynamicRayTracingMode = (RayTracingMode)EditorGUILayout.EnumPopup("Dynamic Object RTX Mode", m_profile.m_optimizationData.m_dynamicRayTracingMode);
#endif
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(m_profile);
            }
        }

        [MenuItem("Assets/Create/Procedural Worlds/HDRP Time Of Day/RTX Utils Profile")]
        public static void CreateHDRPTimeOfDayRTXUtilsProfile()
        {
            HDRPTimeOfDayRayTracingUtilsProfile asset = ScriptableObject.CreateInstance<HDRPTimeOfDayRayTracingUtilsProfile>();

            AssetDatabase.CreateAsset(asset, "Assets/HDRP Time Of Day RTX Utils Profile.asset");
            AssetDatabase.SaveAssets();

            EditorUtility.FocusProjectWindow();

            Selection.activeObject = asset;
        }
    }
}
#endif