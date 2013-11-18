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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Tac
{
    class SpaceCenterManager : MonoBehaviour, Savable
    {
        private GlobalSettings globalSettings;
        private GameSettings gameSettings;
        private Icon<SpaceCenterManager> icon;
        private SavedGameConfigWindow configWindow;

        public SpaceCenterManager()
        {
            Debug.Log("TAC Life Support (SpaceCenterManager) [" + this.GetInstanceID().ToString("X") + "][" + Time.time.ToString("0.00") + "]: Constructor");
            globalSettings = TacLifeSupport.Instance.globalSettings;
            gameSettings = TacLifeSupport.Instance.gameSettings;
            icon = new Icon<SpaceCenterManager>(new Rect(Screen.width * 0.75f, 0, 32, 32), "icon.png", "LS",
                "Click to show the Life Support configuration window", OnIconClicked);
            configWindow = new SavedGameConfigWindow(globalSettings, gameSettings);
        }

        void Awake()
        {
            Debug.Log("TAC Life Support (SpaceCenterManager) [" + this.GetInstanceID().ToString("X") + "][" + Time.time.ToString("0.00") + "]: Awake, new game = " + gameSettings.IsNewSave);
        }

        void Start()
        {
            Debug.Log("TAC Life Support (SpaceCenterManager) [" + this.GetInstanceID().ToString("X") + "][" + Time.time.ToString("0.00") + "]: Start, new game = " + gameSettings.IsNewSave);
            icon.SetVisible(true);

            if (gameSettings.IsNewSave)
            {
                Debug.Log("TAC Life Support (SpaceCenterManager) [" + this.GetInstanceID().ToString("X") + "][" + Time.time.ToString("0.00") + "]: New save detected!");
                configWindow.SetVisible(true);
                gameSettings.IsNewSave = false;
            }

            AddLifeSupport als = new AddLifeSupport(globalSettings);
            als.run();
        }

        public void Load(ConfigNode globalNode)
        {
            icon.Load(globalNode);
            configWindow.Load(globalNode);
        }

        public void Save(ConfigNode globalNode)
        {
            icon.Save(globalNode);
            configWindow.Save(globalNode);
        }

        void OnDestroy()
        {
            Debug.Log("TAC Life Support (SpaceCenterManager) [" + this.GetInstanceID().ToString("X") + "][" + Time.time.ToString("0.00") + "]: OnDestroy");
        }

        private void OnIconClicked()
        {
            configWindow.ToggleVisible();
        }
    }
}
