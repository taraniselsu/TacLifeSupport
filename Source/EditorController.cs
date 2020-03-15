/**
 * Thunder Aerospace Corporation's Life Support for Kerbal Space Program.
 * Originally Written by Taranis Elsu.
 * This version written and maintained by JPLRepo (Jamie Leighton)
 * 
 * (C) Copyright 2013, Taranis Elsu
 * (C) Copyright 2016, Jamie Leighton
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


using KSP.UI.Screens;
using RSTUtils;
using UnityEngine;

namespace Tac
{
    class EditorController : MonoBehaviour, Savable
    {
        private BuildAidWindow window;
        internal AppLauncherToolBar TACMenuAppLToolBar;
        private const string lockName = "TACLS_EditorLock";
        private const ControlTypes desiredLock = ControlTypes.EDITOR_SOFT_LOCK | ControlTypes.EDITOR_UI | ControlTypes.EDITOR_LAUNCH;

        void Awake()
        {
            this.Log("Awake");
            TACMenuAppLToolBar = new AppLauncherToolBar("TACLifeSupport", "TAC Life Support",
                Textures.PathToolbarIconsPath + "/TACgreenIconTB",
                ApplicationLauncher.AppScenes.SPH | ApplicationLauncher.AppScenes.VAB,
                (Texture)Textures.GrnApplauncherIcon, (Texture)Textures.GrnApplauncherIcon,
                GameScenes.EDITOR);
            window = new BuildAidWindow(TACMenuAppLToolBar, TacStartOnce.Instance.globalSettings);
        }

        void Start()
        {
            this.Log("Start");
            //If Settings wants to use ToolBar mod, check it is installed and available. If not set the Setting to use Stock.
            if (!ToolbarManager.ToolbarAvailable && !HighLogic.CurrentGame.Parameters.CustomParams<TAC_SettingsParms>().UseAppLToolbar)
            {
                HighLogic.CurrentGame.Parameters.CustomParams<TAC_SettingsParms>().UseAppLToolbar = true;
            }

            TACMenuAppLToolBar.Start(HighLogic.CurrentGame.Parameters.CustomParams<TAC_SettingsParms>().UseAppLToolbar);

            RSTUtils.Utilities.setScaledScreen();

        }

        void Update()
        {
            if (window.IsVisible() && Event.current != null && window.Contains(Event.current.mousePosition))
            {
                if (InputLockManager.GetControlLock(lockName) != desiredLock)
                {
                    InputLockManager.SetControlLock(desiredLock, lockName);
                }
            }
            else
            {
                if (InputLockManager.GetControlLock(lockName) == desiredLock)
                {
                    InputLockManager.RemoveControlLock(lockName);
                }
            }
        }

        public void Load(ConfigNode globalNode)
        {
            window.Load(globalNode);
        }

        public void Save(ConfigNode globalNode)
        {
            window.Save(globalNode);
        }

        void OnGUI()
        {
            window.SetVisible(TACMenuAppLToolBar.GuiVisible);
            window?.OnGUI();
        }

        void OnDestroy()
        {
            this.Log("OnDestroy");
            TACMenuAppLToolBar.Destroy();

            // Make sure we remove our locks
            if (InputLockManager.GetControlLock(lockName) == desiredLock)
            {
                InputLockManager.RemoveControlLock(lockName);
            }
        }
    }
}
