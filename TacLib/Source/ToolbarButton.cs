/**
 * ToolbarButton.cs
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
    public class ToolbarButton
    {
        private static bool typeInfoIsLoaded = false;
        private static Assembly assembly = null;
        private static Type buttonType = null;
        private static PropertyInfo toolTipProperty = null;
        private static PropertyInfo textProperty = null;
        private static PropertyInfo texturePathProperty = null;
        private static PropertyInfo visibleProperty = null;
        private static MethodInfo destroyMethod = null;

        private static EventInfo onClickEvent = null;
        private static Type clickEventType = null;
        private static MethodInfo clickEventAddMethod = null;

        private object buttonInstance = null;

        public bool Visible
        {
            get
            {
                return (bool)visibleProperty.GetValue(buttonInstance, null);
            }
            set
            {
                visibleProperty.SetValue(buttonInstance, value, null);
            }
        }

        public string ToolTip
        {
            set
            {
                toolTipProperty.SetValue(buttonInstance, value, null);
            }
        }

        public string TexturePath
        {
            set
            {
                texturePathProperty.SetValue(buttonInstance, value, null);
            }
        }

        public string Text
        {
            set
            {
                textProperty.SetValue(buttonInstance, value, null);
            }
        }

        private ToolbarButton(object buttonInstance)
        {
            this.buttonInstance = buttonInstance;
        }

        public void AddOnClickHandler(Action<object> handler)
        {
            Delegate d = Delegate.CreateDelegate(clickEventType, handler.Target, handler.Method);
            clickEventAddMethod.Invoke(buttonInstance, new object[] { d });
        }

        public void Destroy()
        {
            destroyMethod.Invoke(buttonInstance, null);
        }

        public static ToolbarButton Create(string imageFilename, string noImageText,
            string tooltip, Action onClickHandler)
        {
            try
            {
                if (!typeInfoIsLoaded && !LoadTypeInfo())
                {
                    // The error was already logged in LoadTypeInfo
                    return null;
                }

                var b = ToolbarWrapper.AddButton("Tac", noImageText);
                if (b == null)
                {
                    LogWarningS("Failed to create the Toolbar Button");
                    return null;
                }

                ToolbarButton button = new ToolbarButton(b);
                button.ToolTip = tooltip;
                button.AddOnClickHandler(e => onClickHandler());

                if (GameDatabase.Instance.ExistsTexture(imageFilename))
                {
                    button.TexturePath = imageFilename;
                }
                else
                {
                    button.Text = noImageText;
                }

                button.Log("Create successful.");
                return button;
            }
            catch (Exception ex)
            {
                LogWarningS("Exception while creating the Toolbar button: " + ex.Message);
                return null;
            }
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

            buttonType = assembly.GetExportedTypes().FirstOrDefault(t => t.FullName == "Toolbar.IButton");
            if (buttonType == null)
            {
                LogWarningS("Could not find the Toolbar.IButton type.");
                return false;
            }

            toolTipProperty = buttonType.GetProperty("ToolTip");
            if (toolTipProperty == null)
            {
                LogWarningS("Could not find the ToolTip property.");
                return false;
            }

            textProperty = buttonType.GetProperty("Text");
            if (textProperty == null)
            {
                LogWarningS("Could not find the Text property.");
                return false;
            }

            texturePathProperty = buttonType.GetProperty("TexturePath");
            if (texturePathProperty == null)
            {
                LogWarningS("Could not find the TexturePath property.");
                return false;
            }

            visibleProperty = buttonType.GetProperty("Visible");
            if (visibleProperty == null)
            {
                LogWarningS("Could not get the Visible property.");
                return false;
            }

            destroyMethod = buttonType.GetMethod("Destroy");
            if (destroyMethod == null)
            {
                LogWarningS("Could not find the Destroy method.");
                return false;
            }

            onClickEvent = buttonType.GetEvent("OnClick");
            if (onClickEvent == null)
            {
                LogWarningS("Could not find the OnClick event.");
                return false;
            }

            clickEventType = onClickEvent.EventHandlerType;
            if (clickEventType == null)
            {
                LogWarningS("Could not get the OnClick event handler type.");
                return false;
            }

            clickEventAddMethod = onClickEvent.GetAddMethod();
            if (clickEventAddMethod == null)
            {
                LogWarningS("Could not get the add method for the OnClick event.");
                return false;
            }

            typeInfoIsLoaded = true;
            return true;
        }

        private static void LogWarningS(String message)
        {
            Logging.Log("Tac.ToolbarButton", message);
        }
    }
}
