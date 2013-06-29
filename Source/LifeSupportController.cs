/**
 * LifeSupportController.cs
 * 
 * Thunder Aerospace Corporation's Life Support for the Kerbal Space Program, by Taranis Elsu
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Tac
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class LifeSupportController : MonoBehaviour
    {
        public static LifeSupportController Instance { get; private set; }
        public Settings settings { get; private set; }

        private string configFilename;

        void Awake()
        {
            Debug.Log("TAC Life Support (LifeSupportController) [" + this.GetInstanceID().ToString("X") + "][" + Time.time + "]: Awake");
            Instance = this;
            settings = new Settings();

            configFilename = IOUtils.GetFilePathFor(this.GetType(), "LifeSupport.cfg");

            DontDestroyOnLoad(this);
        }

        void Start()
        {
            Debug.Log("TAC Life Support (LifeSupportController) [" + this.GetInstanceID().ToString("X") + "][" + Time.time + "]: Start");
            Load();
        }

        void OnDestroy()
        {
            Debug.Log("TAC Life Support (LifeSupportController) [" + this.GetInstanceID().ToString("X") + "][" + Time.time + "]: OnDestroy");
        }

        private void Load()
        {
            if (File.Exists<LifeSupportController>(configFilename))
            {
                ConfigNode config = ConfigNode.Load(configFilename);
                settings.Load(config);
            }
        }
    }
}
