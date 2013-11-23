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
    class LifeSupportMonitoringWindow : Window<LifeSupportMonitoringWindow>
    {
        private readonly GameSettings gameSettings;
        private readonly RosterWindow rosterWindow;

        private GUIStyle labelStyle;
        private GUIStyle warningStyle;
        private GUIStyle criticalStyle;
        private GUIStyle headerStyle;
        private Vector2 scrollPosition;

        public LifeSupportMonitoringWindow(LifeSupportController controller, GlobalSettings globalSettings, GameSettings gameSettings, RosterWindow rosterWindow)
            : base("Life Support Monitoring", 300, 300)
        {
            this.gameSettings = gameSettings;
            this.rosterWindow = rosterWindow;

            windowPos.y = 50;
        }

        public override void SetVisible(bool newValue)
        {
            base.SetVisible(newValue);

            if (newValue == false)
            {
                rosterWindow.SetVisible(false);
            }
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

        protected override void DrawWindowContents(int windowId)
        {
            if (FlightGlobals.ready)
            {
                scrollPosition = GUILayout.BeginScrollView(scrollPosition);
                GUILayout.BeginVertical();
                GUILayout.Space(4);

                double currentTime = Planetarium.GetUniversalTime();

                foreach (var entry in gameSettings.knownVessels)
                {
                    Guid vesselId = entry.Key;
                    VesselInfo vesselInfo = entry.Value;

                    GUILayout.Label(vesselInfo.vesselName + " (" + vesselInfo.numCrew + " crew) [" + vesselInfo.vesselType + "]", headerStyle);
                    GUILayout.Label("  Last updated:          " + Utilities.FormatTime(currentTime - vesselInfo.lastUpdate), labelStyle);
                    if (vesselInfo.numCrew > 0)
                    {
                        GUILayout.Label("  Food remaining:        " + Utilities.FormatTime(vesselInfo.estimatedTimeFoodDepleted - currentTime), getStyle(vesselInfo.foodStatus));
                        GUILayout.Label("  Water remaining:       " + Utilities.FormatTime(vesselInfo.estimatedTimeWaterDepleted - currentTime), getStyle(vesselInfo.waterStatus));
                        GUILayout.Label("  Oxygen remaining:      " + Utilities.FormatTime(vesselInfo.estimatedTimeOxygenDepleted - currentTime), getStyle(vesselInfo.oxygenStatus));
                        GUILayout.Label("  Electricity remaining: " + Utilities.FormatTime(vesselInfo.estimatedTimeElectricityDepleted - currentTime), getStyle(vesselInfo.electricityStatus));
                    }

                    GUILayout.Space(10);
                }

                GUILayout.EndVertical();
                GUILayout.EndScrollView();
            }

            GUILayout.Space(8);

            if (GUI.Button(new Rect(windowPos.width - 46, 4, 20, 20), "R", closeButtonStyle))
            {
                rosterWindow.SetVisible(true);
            }
        }

        private GUIStyle getStyle(VesselInfo.Status status)
        {
            if (status == VesselInfo.Status.CRITICAL)
            {
                return criticalStyle;
            }
            else if (status == VesselInfo.Status.LOW)
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
