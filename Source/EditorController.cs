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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Tac
{
    class EditorController : MonoBehaviour, Savable
    {
        private ButtonWrapper button;
        private BuildAidWindow window;
        private const string lockName = "TACLS_EditorLock";
        private const ControlTypes desiredLock = ControlTypes.EDITOR_SOFT_LOCK | ControlTypes.EDITOR_UI | ControlTypes.EDITOR_LAUNCH;

        void Awake()
        {
            this.Log("Awake");
            button = new ButtonWrapper(new Rect(Screen.width * 0.275f, 0, 32, 32), "ThunderAerospace/TacLifeSupport/Textures/greenIcon",
                "LS", "TAC Life Support Build Aid", OnIconClicked, "EditorIcon");
            window = new BuildAidWindow(TacLifeSupport.Instance.globalSettings);
        }

        void Start()
        {
            this.Log("Start");
            button.Visible = true;
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

        public void Load(ConfigNode globalNode)
        {
            button.Load(globalNode);
            window.Load(globalNode);
        }

        public void Save(ConfigNode globalNode)
        {
            button.Save(globalNode);
            window.Save(globalNode);
        }

        void OnGUI()
        {
            window?.OnGUI();
            button?.OnGUI();
        }

        void OnDestroy()
        {
            this.Log("OnDestroy");
            button.Destroy();

            // Make sure we remove our locks
            if (InputLockManager.GetControlLock(lockName) == desiredLock)
            {
                InputLockManager.RemoveControlLock(lockName);
            }
        }

        private void OnIconClicked()
        {
            window.ToggleVisible();
        }
    }
}
