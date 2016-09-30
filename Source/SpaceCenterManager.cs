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
    class SpaceCenterManager : MonoBehaviour//, Savable
    {
        public SpaceCenterManager()
        {
            //this.Log("Constructor");
        }

        void Awake()
        {
            this.Log("Awake");
        }

        void Start()
        {
            this.Log("Start, new game = " + TacLifeSupport.Instance.gameSettings.IsNewSave);
            
            if (TacLifeSupport.Instance.gameSettings.IsNewSave)
            {
                this.Log("New save detected!");
                //TACMenuAppLToolBar.onAppLaunchToggle();
                Vector2 anchormin = new Vector2(0.5f, 0.5f);
                Vector2 anchormax = new Vector2(0.5f, 0.5f);
                string msg = "TAC LS Config Settings are now available via the KSP Settings - Difficulty Options Window.";
                string title = "TAC Life Support";
                UISkinDef skin = HighLogic.UISkin;
                DialogGUIBase[] dialogGUIBase = new DialogGUIBase[1];
                dialogGUIBase[0] = new DialogGUIButton("Ok", delegate{});
                PopupDialog.SpawnPopupDialog(anchormin, anchormax, new MultiOptionDialog(msg, title, skin, dialogGUIBase), false, HighLogic.UISkin, true, string.Empty);
                TacLifeSupport.Instance.gameSettings.IsNewSave = false;
            }
            
            AddLifeSupport als = new AddLifeSupport();
            als.run();

            var crew = HighLogic.CurrentGame.CrewRoster.Crew;
            var knownCrew = TacLifeSupport.Instance.gameSettings.knownCrew;
            foreach (ProtoCrewMember crewMember in crew)
            {
                if (crewMember.rosterStatus != ProtoCrewMember.RosterStatus.Assigned && knownCrew.ContainsKey(crewMember.name))
                {
                    this.Log("Deleting crew member: " + crewMember.name);
                    knownCrew.Remove(crewMember.name);
                }
            }
        }
        
        void OnDestroy()
        {
            this.Log("OnDestroy");
        }
    }
}
