using PWCommon5;
using UnityEditor;
using UnityEngine.Rendering;

namespace ProceduralWorlds.HDRPTOD
{
    [InitializeOnLoad]
    public static class HDRPTimeOfDayScriptDefines
    {
        static HDRPTimeOfDayScriptDefines()
        {
            CheckScriptDefine();
        }

        /// <summary>
        /// Processes the check for the script define setup
        /// </summary>
        private static void CheckScriptDefine()
        {
#if !GAIA_2_PRESENT
            bool updateScripting = false;
            var symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            if (IsHDRP())
            {
                if (!symbols.Contains("HDPipeline"))
                {
                    updateScripting = true;
                    if (symbols.Length > 0)
                    {
                        symbols += ";HDPipeline";
                    }
                    else
                    {
                        symbols += "HDPipeline";
                    }
                }

                if (!symbols.Contains("HDRPTIMEOFDAY"))
                {
                    updateScripting = true;
                    if (symbols.Length > 0)
                    {
                        symbols += ";HDRPTIMEOFDAY";
                    }
                    else
                    {
                        symbols += "HDRPTIMEOFDAY";
                    }
                }
            }
            else
            {
                if (symbols.Contains("HDPipeline"))
                {
                    updateScripting = true;
                    symbols = symbols.Replace("HDPipeline", "");
                }

                if (symbols.Contains("HDRPTIMEOFDAY"))
                {
                    updateScripting = true;
                    symbols = symbols.Replace("HDRPTIMEOFDAY", "");
                }
            }

            if (updateScripting && EditorUserBuildSettings.selectedBuildTargetGroup != BuildTargetGroup.Unknown)
            {
                PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, symbols);
            }
#else
            bool updateScripting = false;
            var symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            if (IsHDRP())
            {
                if (!symbols.Contains("HDRPTIMEOFDAY"))
                {
                    updateScripting = true;
                    if (symbols.Length > 0)
                    {
                        symbols += ";HDRPTIMEOFDAY";
                    }
                    else
                    {
                        symbols += "HDRPTIMEOFDAY";
                    }
                }
            }
            else
            {
                if (symbols.Contains("HDRPTIMEOFDAY"))
                {
                    updateScripting = true;
                    symbols = symbols.Replace("HDRPTIMEOFDAY", "");
                }
            }

            if (updateScripting && EditorUserBuildSettings.selectedBuildTargetGroup != BuildTargetGroup.Unknown)
            {
                PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, symbols);
            }
#endif
        }
        /// <summary>
        /// Adds or Removes cinemachine script define if it's missing
        /// </summary>
        public static void AddCinemachineDefine(bool add)
        {
            bool updateScripting = false;
            var symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);

            if (add)
            {
                if (!symbols.Contains("TOD_CINEMACHINE"))
                {
                    updateScripting = true;
                    if (symbols.Length > 0)
                    {
                        symbols += ";TOD_CINEMACHINE";
                    }
                    else
                    {
                        symbols += "TOD_CINEMACHINE";
                    }
                }
            }
            else
            {
                if (symbols.Contains(";TOD_CINEMACHINE"))
                {
                    updateScripting = true;
                    symbols = symbols.Replace(";TOD_CINEMACHINE", "");
                }
                else if (symbols.Contains("TOD_CINEMACHINE"))
                {
                    updateScripting = true;
                    symbols = symbols.Replace("TOD_CINEMACHINE", "");
                }
            }

            if (updateScripting && EditorUserBuildSettings.selectedBuildTargetGroup != BuildTargetGroup.Unknown)
            {
                PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, symbols);
            }
        }
        /// <summary>
        /// Checks to see if HDRP is currently being used
        /// Returns false if it's built-in or URP pipeline
        /// </summary>
        /// <returns></returns>
        private static bool IsHDRP()
        {
            if (GraphicsSettings.renderPipelineAsset != null)
            {
                return GraphicsSettings.renderPipelineAsset.GetType().ToString().Contains("HDRenderPipelineAsset");
            }

            return false;
        }

        [MenuItem("Window/" + PWConst.COMMON_MENU + "/HDRP Time Of Day/Refresh Script Define...", false, 40)]
        public static void RefreshScriptDefineCheck()
        {
            CheckScriptDefine();
        }
    }
}