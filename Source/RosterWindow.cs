/**
 * RosterWindow.cs
 * 
 * Thunder Aerospace Corporation's Fuel Balancer for the Kerbal Space Program, by Taranis Elsu
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
    class RosterWindow : Window<RosterWindow>
    {
        private GUIStyle labelStyle;
        private GUIStyle headerStyle;

        private Vector2 scrollPosition;

        public RosterWindow()
            : base("TAC Life Support Crew Roster", 320, 200)
        {
        }

        protected override void ConfigureStyles()
        {
            base.ConfigureStyles();

            if (labelStyle == null)
            {
                labelStyle = new GUIStyle(GUI.skin.label);
                labelStyle.fontStyle = FontStyle.Normal;
                labelStyle.margin.top = 0;
                labelStyle.margin.bottom = 0;
                labelStyle.padding.top = 0;
                labelStyle.padding.bottom = 0;
                labelStyle.normal.textColor = Color.white;
                labelStyle.wordWrap = false;

                headerStyle = new GUIStyle(labelStyle);
                headerStyle.fontStyle = FontStyle.Bold;
            }
        }

        protected override void DrawWindowContents(int windowID)
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            GUILayout.BeginVertical();

            double currentTime = Planetarium.GetUniversalTime();
            CrewRoster crewRoster = HighLogic.CurrentGame.CrewRoster;

            GUILayout.Label("Number of crew: " + crewRoster.GetList().Count, headerStyle);

            foreach (ProtoCrewMember crewMember in crewRoster)
            {
                string respawnTime = "";
                if (crewMember.rosterStatus == ProtoCrewMember.RosterStatus.MISSING)
                {
                    respawnTime = ", Respawn in " + Utilities.FormatTime(crewMember.UTaR - currentTime);
                }

                string vessel = (crewMember.KerbalRef != null) ? crewMember.KerbalRef.InVessel.vesselName : "Unknown";
                string part = (crewMember.KerbalRef != null) ? crewMember.KerbalRef.InPart.ToString() : "Unknown";
                GUILayout.Label(crewMember.name + ", " + crewMember.rosterStatus + ", " + vessel + ", " + part + respawnTime, labelStyle);
            }

            GUILayout.EndVertical();
            GUILayout.EndScrollView();

            GUILayout.Space(8);
        }
    }
}
