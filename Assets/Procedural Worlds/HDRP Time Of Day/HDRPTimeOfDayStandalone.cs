#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;

namespace ProceduralWorlds.HDRPTOD
{
    [InitializeOnLoad]
    public static class HDRPTimeOfDayStandalone
    {
        public static string Version = "1.1.0";
        private const string GaiaHDRPTODPath = "Assets/Procedural Worlds/Gaia/Gaia Pro/HDRP Time Of Day";
        private const string GaiaLocalizationPath = "Assets/Procedural Worlds/Gaia/Localization";
        private const string HDRPTimeOfDayPackagePath = "Assets/Procedural Worlds/HDRP Time Of Day/HDRP Time Of Day.unitypackage";
        private const string DevUtilsPath = "Assets/Dev Utilities";
        private const string InstallMenuItem = "Window/Procedural Worlds/HDRP Time Of Day/Install Latest version";
        private const string CleanUpGaiaMenuItem = "Window/Procedural Worlds/HDRP Time Of Day/Clean Up Gaia HDRP Time Of Day";
        private static List<string> GaiaHDRPTODLocalizationFiles = new List<string> { "HDRPTimeOfDayEditor", "HDRPTimeOfDayOverrideVolumeEditor" };
        private static bool ShowGaiaRuntimeMessage = false;

        static HDRPTimeOfDayStandalone()
        {
            StartProcess();
        }
        /// <summary>
        /// Menu item that allows manual installation of latest versions
        /// </summary>
        [MenuItem(InstallMenuItem)]
        public static void InstallLatestMenuItem()
        {
            if (!File.Exists(HDRPTimeOfDayPackagePath))
            {
                EditorUtility.DisplayDialog("Missing Package", "HDRP Time Of Day package was not found, this package is normally located at " + HDRPTimeOfDayPackagePath + ". Please make sure the package is there before trying to 'Install Latest Version'.", "Ok");
                return;
            }

#if !GAIA_PRO_PRESENT
            PerformImport(true, false, true);
#else
            StartProcess();
#endif
        }
        [MenuItem(CleanUpGaiaMenuItem)]
        public static void CleanUpGaiaHDRPTimeOfDayMenuItem()
        {
            if (Directory.Exists(GaiaHDRPTODPath))
            {
                CleanUpGaiaHDRPTODLocalizationFiles();
                CleanUpGaiaHDRPTODDirectory();
            }
        }
        private static void StartProcess()
        {
#if GAIA_PRO_PRESENT
            //If Gaia is installed
            if (!Directory.Exists(DevUtilsPath))
            {
                if (Directory.Exists(GaiaHDRPTODPath))
                {
                    if (File.Exists(HDRPTimeOfDayPackagePath))
                    {
                        //Ask if you want to install standalone version
                        if (EditorUtility.DisplayDialog("Gaia Pro Detected",
                                "A Gaia Pro install has been detected, would you like to use the standalone version of HDRP Time Of Day?\r\n\r\n" +
                                "This will remove the older embedded HDRP Time Of Day version from Gaia and install the newer standalone version. \r\n\r\n If you have custom files (e.g. lighting profiles) in the this directory " +
                                GaiaHDRPTODPath + " please make sure you move them out of this directory before proceeding. Deletion of any user-created files cannot be undone!",
                                "Yes", "No"))
                        {
                            ShowGaiaRuntimeMessage = true;
                            PerformImport(false, true);
                        }
                    }
                }
                else
                {
                    ShowGaiaRuntimeMessage = false;
                    PerformImport(true, false, true);
                }
            }
#else
            ShowGaiaRuntimeMessage = false;
            PerformImport(true);
#endif
        }
        /// <summary>
        /// Performs the import of the standalone HDRP TOD
        /// </summary>
        /// <param name="cleanupGaiaVersion"></param>
        private static void PerformImport(bool interactiveImport, bool cleanupGaiaVersion = false, bool forceDeletePackageFile = false)
        {
            //Import standalone package if it exists
            if (!Directory.Exists(DevUtilsPath))
            {
                //Delete the HDRP Time of day directory in Gaia
                if (cleanupGaiaVersion)
                {
                    CleanUpGaiaHDRPTODLocalizationFiles();
                    CleanUpGaiaHDRPTODDirectory();
                }

#if HDRPTIMEOFDAY && HDPipeline
                HDRPTimeOfDayAPI.RefreshTimeOfDay();
#elif GAIA_PRO_PRESENT && HDPipeline
                HDRPTimeOfDayAPI.RefreshTimeOfDay();
#endif
                if (!File.Exists(HDRPTimeOfDayPackagePath))
                {
                    return;
                }

                AssetDatabase.Refresh();
                AssetDatabase.importPackageCompleted -= ImportComplete;
                AssetDatabase.importPackageCompleted += ImportComplete;
                AssetDatabase.ImportPackage(HDRPTimeOfDayPackagePath, interactiveImport);

                if (forceDeletePackageFile)
                {
                    ImportComplete("");
                }
            }
        }
        /// <summary>
        /// Removes the HDRP TOD localization files from Gaia
        /// </summary>
        private static void CleanUpGaiaHDRPTODLocalizationFiles()
        {
            //Get localization files
            string[] localizationFiles = Directory.GetFiles(GaiaLocalizationPath, "*.pwlpk", SearchOption.AllDirectories);
            if (localizationFiles.Length > 0)
            {
                foreach (string file in localizationFiles)
                {
                    foreach (string localizationFile in GaiaHDRPTODLocalizationFiles)
                    {
                        //Delete if file contains name of the files we want to remove
                        if (file.Contains(localizationFile))
                        {
                            AssetDatabase.DeleteAssets(new List<string> { file + ".meta", file }.ToArray(), new List<string>());
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Cleans up the HDRP TOD folder directory in Gaia Pro
        /// </summary>
        /// <param name="refreshAssetDatabase"></param>
        private static void CleanUpGaiaHDRPTODDirectory()
        {
            //Clean up HDRP TOD in Gaia directory
            AssetDatabase.DeleteAsset(GaiaHDRPTODPath);
        }
        /// <summary>
        /// Code executed when the package has successfully imported into your project
        /// </summary>
        /// <param name="packageName"></param>
        private static void ImportComplete(string packageName)
        {
            //Delete the unity package after import
            if (File.Exists(HDRPTimeOfDayPackagePath))
            {
                AssetDatabase.DeleteAssets(new List<string> {HDRPTimeOfDayPackagePath, HDRPTimeOfDayPackagePath +".meta"}.ToArray(), new List<string>());

                if (ShowGaiaRuntimeMessage)
                {
                    EditorUtility.DisplayDialog("Gaia Runtime Notice",
                        "You now have the standalone version of HDRP Time Of Day installed. " +
                        "Any current scenes that are using the Gaia version of HDRP Time Of Day will need to be refreshed.\r\n\r\n" +
                        "You can do this by going to the 'Gaia Manager' and clicking the Add/Update Runtime button to update all the runtime components in the scene. " +
                        "Or you can add the HDRP Time Of Day from the window menu at 'Window/Procedural Worlds/HDRP Time Of Day/Add Time Of Day' to refresh time of day.",
                        "Ok");
                    ShowGaiaRuntimeMessage = false;
                }
            }
        }
    }
}
#endif