/**
 * Based on the InstallChecker from the Kethane mod for Kerbal Space Program.
 * https://github.com/Majiir/Kethane/blob/b93b1171ec42b4be6c44b257ad31c7efd7ea1702/Plugin/InstallChecker.cs
 * 
 * Original is (C) Copyright Majiir.
 * CC0 Public Domain (http://creativecommons.org/publicdomain/zero/1.0/)
 * http://forum.kerbalspaceprogram.com/threads/65395-CompatibilityChecker-Discussion-Thread?p=899895&viewfull=1#post899895
 * 
 * This file has been modified extensively and is released under the same license.
 */
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Tac
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    internal class InstallChecker : MonoBehaviour
    {
        private const string modName = "TAC Life Support";
        private const string expectedPath = "ThunderAerospace/TacLifeSupport";

        protected void Start()
        {
            // Log some information that might be of interest when debugging
            this.Log(modName + " - KSPUtil.ApplicationRootPath = " + KSPUtil.ApplicationRootPath);
            this.Log(modName + " - GameDatabase.Instance.PluginDataFolder = " + GameDatabase.Instance.PluginDataFolder);
            this.Log(modName + " - Assembly.GetExecutingAssembly().Location = " + Assembly.GetExecutingAssembly().Location);
            this.Log(modName + " - Using 64-bit? " + (IntPtr.Size == 8));

            // Search for this mod's DLL existing in the wrong location. This will also detect duplicate copies because only one can be in the right place.
            var assemblies = AssemblyLoader.loadedAssemblies.Where(a => a.assembly.GetName().Name == Assembly.GetExecutingAssembly().GetName().Name).Where(a => a.url != expectedPath);
            if (assemblies.Any())
            {
                var badPaths = assemblies.Select(a => a.path).Select(p => Uri.UnescapeDataString(new Uri(Path.GetFullPath(KSPUtil.ApplicationRootPath)).MakeRelativeUri(new Uri(p)).ToString().Replace('/', Path.DirectorySeparatorChar)));
                string badPathsString = String.Join("\n", badPaths.ToArray());
                this.Log(modName + " - Incorrectly installed, bad paths:\n" + badPathsString);
                PopupDialog.SpawnPopupDialog("Incorrect " + modName + " Installation",
                    modName + " has been installed incorrectly and will not function properly. All files should be located in KSP/GameData/" + expectedPath + ". Do not move any files from inside that folder.\n\nIncorrect path(s):\n" + badPathsString,
                    "OK", false, HighLogic.Skin);
            }

            // Check for Module Manager
            if (!AssemblyLoader.loadedAssemblies.Any(a => a.assembly.GetName().Name.StartsWith("ModuleManager") && a.url == ""))
            {
                this.Log(modName + " - Missing or incorrectly installed ModuleManager.");
                PopupDialog.SpawnPopupDialog("Missing Module Manager",
                    modName + " requires the Module Manager mod in order to function properly.\n\nPlease download from http://forum.kerbalspaceprogram.com/threads/55219 and copy to the KSP/GameData/ directory.",
                    "OK", false, HighLogic.Skin);
            }

            CleanupOldVersions();
        }

        /*
         * Tries to fix the install if it was installed over the top of a previous version
         */
        void CleanupOldVersions()
        {
            try
            {
                // Upgrading 0.8 -> 0.9
                // StockPartChanges.cfg was split into multiple files with different names
                if (File.Exists(KSPUtil.ApplicationRootPath + "GameData/ThunderAerospace/TacLifeSupport/StockPartChanges.cfg"))
                {
                    this.Log(modName + " - deleting the old StockPartChanges.cfg.");
                    File.Delete(KSPUtil.ApplicationRootPath + "GameData/ThunderAerospace/TacLifeSupport/StockPartChanges.cfg");
                }
            }
            catch (Exception ex)
            {
                this.LogError("Exception caught while cleaning up old files.\n" + ex.Message + "\n" + ex.StackTrace);
            }
        }
    }
}
