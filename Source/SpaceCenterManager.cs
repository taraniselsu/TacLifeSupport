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

using KSP.IO;
using KSP.UI.Screens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Tac
{
    class SpaceCenterManager : MonoBehaviour, Savable
    {
        ApplicationLauncherButton _appLauncherButton;
        GlobalSettings globalSettings;
        private TacGameSettings gameSettings;
        SavedGameConfigWindow configWindow;
        const string lockName = "TACLS_SpaceCenterLock";
        const ControlTypes desiredLock = ControlTypes.KSC_FACILITIES;

        public SpaceCenterManager()
        {
            this.Log("Constructor");
            globalSettings = TacLifeSupport.Instance.globalSettings;
            gameSettings = TacLifeSupport.Instance.gameSettings;
        }

        void Awake()
        {
            this.Log("Awake");
            configWindow = new SavedGameConfigWindow(globalSettings, gameSettings);
            GameEvents.onGUIApplicationLauncherReady.Add(OnGUIAppLauncherReady);
            OnGUIAppLauncherReady();
        }

        void Start()
        {
            this.Log("Start, new game = " + gameSettings.IsNewSave);

            if (gameSettings.IsNewSave)
            {
                this.Log("New save detected!");
                configWindow.SetVisible(true);
                gameSettings.IsNewSave = false;
            }

            AddLifeSupport als = new AddLifeSupport(globalSettings);
            als.run();

            var crew = HighLogic.CurrentGame.CrewRoster.Crew;
            var knownCrew = gameSettings.knownCrew;
            foreach (ProtoCrewMember crewMember in crew)
            {
                if (crewMember.rosterStatus != ProtoCrewMember.RosterStatus.Assigned && knownCrew.ContainsKey(crewMember.name))
                {
                    this.Log("Deleting crew member: " + crewMember.name);
                    knownCrew.Remove(crewMember.name);
                }
            }
        }

        void OnGUIAppLauncherReady()
        {
            if (ApplicationLauncher.Ready && _appLauncherButton == null)
            {
                _appLauncherButton = ApplicationLauncher.Instance.AddModApplication(
                    () => { configWindow.SetVisible(true); this.Log("set true"); },
                    () => { configWindow.SetVisible(false); this.Log("set false"); },
                    () => { },
                    () => { },
                    () => { },
                    () => { },
                    ApplicationLauncher.AppScenes.SPACECENTER,
                    GameDatabase.Instance.GetTexture("ThunderAerospace/TacLifeSupport/Textures/greenIcon", false));
            }
        }

        void Update()
        {
            if (configWindow.IsVisible() && configWindow.Contains(Event.current.mousePosition))
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
            configWindow.Load(globalNode);
        }

        public void Save(ConfigNode globalNode)
        {
            configWindow.Save(globalNode);
        }

        void OnGUI()
        {
            configWindow?.OnGUI();
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
