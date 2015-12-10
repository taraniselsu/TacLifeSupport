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
        private readonly TacGameSettings gameSettings;
        private readonly RosterWindow rosterWindow;
        private readonly string version;

        private GUIStyle labelStyle;
        private GUIStyle warningStyle;
        private GUIStyle criticalStyle;
        private GUIStyle headerStyle;
        private GUIStyle scrollStyle;
        private GUIStyle versionStyle;
        private Vector2 scrollPosition = Vector2.zero;

        public LifeSupportMonitoringWindow(LifeSupportController controller, GlobalSettings globalSettings, TacGameSettings gameSettings, RosterWindow rosterWindow)
            : base("Life Support Monitoring", 300, 300)
        {
            this.gameSettings = gameSettings;
            this.rosterWindow = rosterWindow;
            version = Utilities.GetDllVersion(this);

            windowPos.y = 75;
            SetVisible(true);
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

                scrollStyle = new GUIStyle(GUI.skin.scrollView);

                versionStyle = Utilities.GetVersionStyle();
            }
        }

        protected override void DrawWindowContents(int windowId)
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, scrollStyle);
            GUILayout.BeginVertical();
            GUILayout.Space(4);

            double currentTime = Planetarium.GetUniversalTime();
            var vesselsCopy = new List<KeyValuePair<Guid, VesselInfo>>(gameSettings.knownVessels);
            vesselsCopy.Sort(new VesselSorter(FlightGlobals.ActiveVessel));

            if (FlightGlobals.ready)
            {
                // Draw the active vessel first, if any
                Vessel activeVessel = FlightGlobals.ActiveVessel;
                int skipCount = 0;
                if (activeVessel != null && vesselsCopy.Count > 0)
                {
                    var vessel = vesselsCopy[0];
                    if (FlightGlobals.ActiveVessel.id == vessel.Key)
                    {
                        DrawVesselInfo(vessel.Value, currentTime);

                        // Skip the active vessel later when drawing the rest of the vessels
                        skipCount = 1;
                    }
                    else
                    {
                        // No info cached about the active vessel -- either it has not launched yet, or there is no crew
                        int numCrew = activeVessel.GetCrewCount();
                        GUILayout.Label(activeVessel.vesselName + " (" + numCrew + " crew) [" + activeVessel.vesselType + "]", headerStyle);
                        if (numCrew > 0)
                        {
                            GUILayout.Label("  Prelaunch", labelStyle);
                        }
                        else
                        {
                            GUILayout.Label("  No Crew", labelStyle);
                        }
                    }
                    GUILayout.Space(10);
                }

                foreach (var vessel in vesselsCopy.Skip(skipCount))
                {
                    DrawVesselInfo(vessel.Value, currentTime);
                    GUILayout.Space(10);
                }
            }
            else
            {
                foreach (var vessel in vesselsCopy)
                {
                    DrawVesselInfo(vessel.Value, currentTime);
                    GUILayout.Space(10);
                }
            }

            GUILayout.EndVertical();
            GUILayout.EndScrollView();

            GUILayout.Space(8);

            if (GUI.Button(new Rect(windowPos.width - 46, 4, 20, 20), "R", closeButtonStyle))
            {
                rosterWindow.SetVisible(true);
            }

            GUI.Label(new Rect(4, windowPos.height - 13, windowPos.width - 20, 12), "TAC Life Support v" + version, versionStyle);
        }

        private void DrawVesselInfo(VesselInfo vesselInfo, double currentTime)
        {
            GUILayout.Label(vesselInfo.vesselName + " (" + vesselInfo.numCrew + " crew) [" + vesselInfo.vesselType + "]", headerStyle);
            GUILayout.Label("  Last updated:          " + Utilities.FormatTime(currentTime - vesselInfo.lastUpdate), labelStyle);
            if (vesselInfo.numCrew > 0)
            {
                GUILayout.Label("  Food remaining:        " + Utilities.FormatTime(vesselInfo.estimatedTimeFoodDepleted - currentTime), getStyle(vesselInfo.foodStatus));
                GUILayout.Label("  Water remaining:       " + Utilities.FormatTime(vesselInfo.estimatedTimeWaterDepleted - currentTime), getStyle(vesselInfo.waterStatus));
                GUILayout.Label("  Oxygen remaining:      " + Utilities.FormatTime(vesselInfo.estimatedTimeOxygenDepleted - currentTime), getStyle(vesselInfo.oxygenStatus));
                GUILayout.Label("  Electricity remaining: " + Utilities.FormatTime(vesselInfo.estimatedTimeElectricityDepleted - currentTime), getStyle(vesselInfo.electricityStatus));
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

        private class VesselSorter : IComparer<KeyValuePair<Guid, VesselInfo>>
        {
            private Vessel activeVessel;

            public VesselSorter(Vessel activeVessel)
            {
                this.activeVessel = activeVessel;
            }

            public int Compare(KeyValuePair<Guid, VesselInfo> left, KeyValuePair<Guid, VesselInfo> right)
            {
                // Put the active vessel at the top of the list
                if (activeVessel != null)
                {
                    if (left.Key.Equals(activeVessel.id))
                    {
                        if (right.Key.Equals(activeVessel.id))
                        {
                            // Both sides are the active vessel (i.e. the same vessel)
                            return 0;
                        }
                        else
                        {
                            return -1;
                        }
                    }
                    else if (right.Key.Equals(activeVessel.id))
                    {
                        return 1;
                    }
                }

                // then sort by the shortest time until a resource is depleted
                double leftShortestTime = Math.Min(left.Value.estimatedTimeFoodDepleted, Math.Min(left.Value.estimatedTimeWaterDepleted, left.Value.estimatedTimeOxygenDepleted));
                double rightShortestTime = Math.Min(right.Value.estimatedTimeFoodDepleted, Math.Min(right.Value.estimatedTimeWaterDepleted, right.Value.estimatedTimeOxygenDepleted));

                return leftShortestTime.CompareTo(rightShortestTime);
            }
        }
    }
}
