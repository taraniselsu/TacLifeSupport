/**
 * LifeSupportMonitoringWindow.cs
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Tac
{
    class LifeSupportMonitoringWindow : Window<LifeSupportMonitoringWindow>
    {
        private readonly LifeSupportController controller;
        private readonly GlobalSettings globalSettings;
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
            this.controller = controller;
            this.globalSettings = globalSettings;
            this.gameSettings = gameSettings;
            this.rosterWindow = rosterWindow;

            windowPos.y = 20;
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
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            GUILayout.BeginVertical();
            GUILayout.Space(4);

            if (FlightGlobals.ready)
            {
                double currentTime = Planetarium.GetUniversalTime();

                foreach (var entry in gameSettings.knownVessels)
                {
                    Vessel vessel = FlightGlobals.Vessels.FirstOrDefault(v => v.id.Equals(entry.Key));
                    VesselInfo vesselInfo = entry.Value;

                    if (vessel == null || vesselInfo.numCrew < 1)
                    {
                        continue;
                    }

                    GUILayout.Label("Vessel: " + vessel.vesselName + " (" + vessel.vesselType + ")", headerStyle);
                    GUILayout.Label("Crew: " + vesselInfo.numCrew, headerStyle);

                    var crew = vessel.GetVesselCrew().Select(crewMember => gameSettings.knownCrew[crewMember.name]);
                    foreach (CrewMemberInfo crewMemberInfo in crew)
                    {
                        GUIStyle style = labelStyle;
                        StringBuilder text = new StringBuilder(crewMemberInfo.name);
                        if ((currentTime - crewMemberInfo.lastFood) > 1)
                        {
                            text.Append("  Food=").Append(Utilities.FormatTime(currentTime - crewMemberInfo.lastFood));
                            style = criticalStyle;
                        }
                        if ((currentTime - crewMemberInfo.lastWater) > 1)
                        {
                            text.Append("  Water=").Append(Utilities.FormatTime(currentTime - crewMemberInfo.lastWater));
                            style = criticalStyle;
                        }
                        if ((currentTime - crewMemberInfo.lastOxygen) > 1)
                        {
                            text.Append("  Oxygen=").Append(Utilities.FormatTime(currentTime - crewMemberInfo.lastOxygen));
                            style = criticalStyle;
                        }

                        GUILayout.Label(text.ToString(), style);
                    }

                    GUILayout.Space(5);

                    // Electricity
                    if (vesselInfo.electricityStatus == VesselInfo.Status.CRITICAL)
                    {
                        GUILayout.Label("Electric Charge depleted!  " + Utilities.FormatTime(vesselInfo.lastElectricity - currentTime), criticalStyle);
                    }
                    else
                    {
                        GUIStyle style = labelStyle;
                        if (vesselInfo.electricityStatus == VesselInfo.Status.LOW)
                        {
                            style = warningStyle;
                        }

                        double electricityConsumptionRate = controller.CalculateElectricityConsumptionRate(vessel, vesselInfo);
                        GUILayout.Label("Remaining Electricity: " + Utilities.FormatTime(vesselInfo.remainingElectricity / electricityConsumptionRate)/* + " (" + vesselInfo.remainingElectricity.ToString("0.000000") + ")"*/, style);
                    }

                    // Food
                    if (vesselInfo.foodStatus == VesselInfo.Status.CRITICAL)
                    {
                        CrewMemberInfo crewMemberInfo = crew.OrderBy(cmi => cmi.lastFood).First();
                        GUILayout.Label("Food depleted! " + Utilities.FormatTime(crewMemberInfo.lastFood - currentTime), criticalStyle);
                    }
                    else
                    {
                        GUIStyle style = labelStyle;
                        if (vesselInfo.foodStatus == VesselInfo.Status.LOW)
                        {
                            style = warningStyle;
                        }

                        GUILayout.Label("Remaining Food: " + Utilities.FormatTime(vesselInfo.remainingFood / globalSettings.FoodConsumptionRate / vesselInfo.numCrew)/* + " (" + vesselInfo.remainingFood.ToString("0.000000") + ")"*/, style);
                    }

                    // Water
                    if (vesselInfo.waterStatus == VesselInfo.Status.CRITICAL)
                    {
                        CrewMemberInfo crewMemberInfo = crew.OrderBy(cmi => cmi.lastWater).First();
                        GUILayout.Label("Water depleted! " + Utilities.FormatTime(crewMemberInfo.lastWater - currentTime), criticalStyle);
                    }
                    else
                    {
                        GUIStyle style = labelStyle;
                        if (vesselInfo.waterStatus == VesselInfo.Status.LOW)
                        {
                            style = warningStyle;
                        }

                        GUILayout.Label("Remaining Water: " + Utilities.FormatTime(vesselInfo.remainingWater / globalSettings.WaterConsumptionRate / vesselInfo.numCrew)/* + " (" + vesselInfo.remainingWater.ToString("0.000000") + ")"*/, style);
                    }

                    // Oxygen
                    if (vesselInfo.oxygenStatus == VesselInfo.Status.CRITICAL)
                    {
                        CrewMemberInfo crewMemberInfo = crew.OrderBy(cmi => cmi.lastOxygen).First();
                        GUILayout.Label("Oxygen depleted! " + Utilities.FormatTime(crewMemberInfo.lastOxygen - currentTime), criticalStyle);
                    }
                    else
                    {
                        GUIStyle style = labelStyle;
                        if (vesselInfo.oxygenStatus == VesselInfo.Status.LOW)
                        {
                            style = warningStyle;
                        }

                        GUILayout.Label("Remaining Oxygen: " + Utilities.FormatTime(vesselInfo.remainingOxygen / globalSettings.OxygenConsumptionRate / vesselInfo.numCrew)/* + " (" + vesselInfo.remainingOxygen.ToString("0.000000") + ")"*/, style);
                    }

                    GUILayout.Space(20);
                }
            }

            GUILayout.EndVertical();
            GUILayout.EndScrollView();

            GUILayout.Space(8);

            if (GUI.Button(new Rect(windowPos.width - 46, 4, 20, 20), "R", closeButtonStyle))
            {
                rosterWindow.SetVisible(true);
            }
        }
    }
}
