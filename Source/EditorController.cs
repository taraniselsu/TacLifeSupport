/**
 * Thunder Aerospace Corporation's Life Support for Kerbal Space Program.
 * Written by Taranis Elsu.
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

using KSP.UI.Screens;
using UnityEngine;

namespace Tac
{
    class EditorController : MonoBehaviour, Savable
    {
        ApplicationLauncherButton _appLauncherButton;
        BuildAidWindow window;
        const string lockName = "TACLS_EditorLock";
        const ControlTypes desiredLock = ControlTypes.EDITOR_SOFT_LOCK | ControlTypes.EDITOR_UI | ControlTypes.EDITOR_LAUNCH;

        void Awake()
        {
            this.Log("Awake");
            window = new BuildAidWindow(TacLifeSupport.Instance.globalSettings);
            GameEvents.onGUIApplicationLauncherReady.Add(OnGUIAppLauncherReady);
            OnGUIAppLauncherReady();
        }

        void Start()
        {
            this.Log("Start");
        }

        void Update()
        {
            if (window.IsVisible() && window.Contains(Event.current.mousePosition))
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

        void OnGUIAppLauncherReady()
        {
            if (ApplicationLauncher.Ready && _appLauncherButton == null)
            {
                _appLauncherButton = ApplicationLauncher.Instance.AddModApplication(
                    () => window.SetVisible(true),
                    () => window.SetVisible(false),
                    () => { },
                    () => { },
                    () => { },
                    () => { },
                    ApplicationLauncher.AppScenes.SPH | ApplicationLauncher.AppScenes.VAB,
                    GameDatabase.Instance.GetTexture("ThunderAerospace/TacLifeSupport/Textures/greenIcon", false));
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
            window?.OnGUI();
        }

        void OnDestroy()
        {
            this.Log("OnDestroy");

            GameEvents.onGUIApplicationLauncherReady.Remove(OnGUIAppLauncherReady);
            ApplicationLauncher.Instance.RemoveModApplication(_appLauncherButton);

            // Make sure we remove our locks
            if (InputLockManager.GetControlLock(lockName) == desiredLock)
            {
                InputLockManager.RemoveControlLock(lockName);
            }
        }
    }
}
