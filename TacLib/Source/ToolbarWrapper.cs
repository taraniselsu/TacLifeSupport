/**
 * ToolbarWrapper.cs
 * 
 * Thunder Aerospace Corporation's library for the Kerbal Space Program, by Taranis Elsu
 * 
 * (C) Copyright 2013, Taranis Elsu
 * 
 * Kerbal Space Program is Copyright (C) 2013 Squad. See http://kerbalspaceprogram.com/. This
 * project is in no way associated with nor endorsed by Squad.
 * 
 * This code is licensed under the Attribution-NonCommercial-ShareAlike 3.0 (CC BY-NC-SA 3.0)
 * creative commons license. See <http://creativecommons.org/licenses/by-nc-sa/3.0/legalcode>
 * for full details.
 * 
 * Attribution — You are free to modify this code, so long as you mention that the resulting
 * work is based upon or adapted from this code.
 * 
 * Non-commercial - You may not use this work for commercial purposes.
 * 
 * Share Alike — If you alter, transform, or build upon this work, you may distribute the
 * resulting work only under the same or similar license to the CC BY-NC-SA 3.0 license.
 * 
 * Note that Thunder Aerospace Corporation is a ficticious entity created for entertainment
 * purposes. It is in no way meant to represent a real entity. Any similarity to a real entity
 * is purely coincidental.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Tac
{
    public class ToolbarWrapper
    {
        private static bool typeInfoIsLoaded = false;
        private static Assembly assembly = null;
        private static Type toolbarManagerType = null;
        private static PropertyInfo instanceProperty = null;
        private static MethodInfo addMethod = null;
        private static object toolbarManagerInstance = null;

        public static object AddButton(string ns, string id)
        {
            if (toolbarManagerInstance == null)
            {
                if (!typeInfoIsLoaded && !LoadTypeInfo())
                {
                    // The error was already logged in LoadTypeInfo
                    return null;
                }

                toolbarManagerInstance = instanceProperty.GetValue(null, null);
                if (toolbarManagerInstance == null)
                {
                    LogWarningS("Could not get the ToolbarManager instance.");
                    return null;
                }
            }

            return addMethod.Invoke(toolbarManagerInstance, new object[] { ns, id });
        }

        private static bool LoadTypeInfo()
        {
            AssemblyLoader.LoadedAssembly loadedAssembly = AssemblyLoader.loadedAssemblies.FirstOrDefault(a => a.dllName == "Toolbar");
            if (loadedAssembly == null)
            {
                LogWarningS("Could not find Toolbar.dll.");
                return false;
            }
            assembly = loadedAssembly.assembly;

            toolbarManagerType = assembly.GetExportedTypes().FirstOrDefault(t => t.FullName == "Toolbar.ToolbarManager");
            if (toolbarManagerType == null)
            {
                LogWarningS("Could not find the Toolbar.ToolbarManager type.");
                return false;
            }

            instanceProperty = toolbarManagerType.GetProperty("Instance");
            if (instanceProperty == null)
            {
                LogWarningS("Could not find the ToolbarManager.Instance property.");
                return false;
            }

            addMethod = toolbarManagerType.GetMethod("add");
            if (addMethod == null)
            {
                LogWarningS("Could not find the ToolbarManager.add method.");
                return false;
            }

            typeInfoIsLoaded = true;
            return true;
        }

        private static void LogWarningS(String message)
        {
            Logging.LogWarning("Tac.ToolbarWrapper", message);
        }
    }
}
