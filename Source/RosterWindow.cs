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
    class RosterWindow : Window<RosterWindow>
    {
        private readonly GlobalSettings globalSettings;
        private readonly TacGameSettings gameSettings;

        private GUIStyle labelStyle;
        private GUIStyle warningStyle;
        private GUIStyle criticalStyle;
        private GUIStyle headerStyle;
        private Vector2 scrollPosition;

        public RosterWindow(GlobalSettings globalSettings, TacGameSettings gameSettings)
            : base("Life Support Crew Roster", 320, 200)
        {
            this.globalSettings = globalSettings;
            this.gameSettings = gameSettings;

            SetVisible(true);
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

                warningStyle = new GUIStyle(labelStyle);
                warningStyle.normal.textColor = Color.yellow;

                criticalStyle = new GUIStyle(labelStyle);
                criticalStyle.normal.textColor = Color.red;

                headerStyle = new GUIStyle(labelStyle);
                headerStyle.fontStyle = FontStyle.Bold;
            }
        }

        protected override void DrawWindowContents(int windowID)
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            GUILayout.BeginVertical();
            GUILayout.Space(4);

            double currentTime = Planetarium.GetUniversalTime();

            foreach (CrewMemberInfo crewInfo in gameSettings.knownCrew.Values)
            {
                GUILayout.Label(crewInfo.name + " (" + crewInfo.vesselName + ")", headerStyle);
                GUILayout.Label("  Last updated: " + Utilities.FormatTime(currentTime - crewInfo.lastUpdate), labelStyle);
                GUILayout.Label("  Last food: " + Utilities.FormatTime(currentTime - crewInfo.lastFood), getStyle(crewInfo.lastUpdate, crewInfo.lastFood, globalSettings.MaxTimeWithoutFood));
                GUILayout.Label("  Last water: " + Utilities.FormatTime(currentTime - crewInfo.lastWater), getStyle(crewInfo.lastUpdate, crewInfo.lastWater, globalSettings.MaxTimeWithoutWater));
                if (gameSettings.HibernateInsteadOfKill)
                {
                    GUILayout.Label("  Hibernating: " + crewInfo.hibernating, labelStyle);
                }
                GUILayout.Space(10);
            }

            GUILayout.EndVertical();
            GUILayout.EndScrollView();

            GUILayout.Space(8);
        }

        private GUIStyle getStyle(double lastUpdate, double lastConsumption, double maxTime)
        {
            if (lastUpdate > (lastConsumption + maxTime / 10))
            {
                return criticalStyle;
            }
            else if (lastUpdate > lastConsumption)
            {
                return warningStyle;
            }
            else
            {
                return labelStyle;
            }
        }
    }
}
