#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace ProceduralWorlds.HDRPTOD
{
    [ExecuteAlways]
    public class HDRPTimeOfDayTransformTracker : MonoBehaviour
    {
        [SerializeField, HideInInspector] private Transform Player;

        private void OnEnable()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                EditorApplication.update -= EditorUpdate;
                EditorApplication.update += EditorUpdate;
            }
            else
            {
                EditorApplication.update -= EditorUpdate;
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
            if (!Application.isPlaying)
            {
                return;
            }

            UpdateTransform(Player);
        }

        /// <summary>
        /// Update used for editor application
        /// </summary>
        private void EditorUpdate()
        {
#if UNITY_EDITOR
            SceneView sceneView = SceneView.lastActiveSceneView;
            if (sceneView != null)
            {
                Camera sceneCamera = sceneView.camera;
                if (sceneCamera != null)
                {
                    UpdateTransform(sceneCamera.transform);
                }
            }
#endif
        }
        /// <summary>
        /// Updates the transforms position to the player postion
        /// </summary>
        /// <param name="player"></param>
        private void UpdateTransform(Transform player)
        {
            if (player != null)
            {
                gameObject.transform.SetPositionAndRotation(player.position, Quaternion.identity);
            }
        }
        /// <summary>
        /// Sets the new player transform
        /// </summary>
        /// <param name="player"></param>
        public void SetNewPlayer(Transform player)
        {
            Player = player;
        }
    }
}