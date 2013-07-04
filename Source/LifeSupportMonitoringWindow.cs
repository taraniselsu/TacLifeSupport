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
        private readonly Settings settings;

        private GUIStyle labelStyle;
        private GUIStyle warningStyle;
        private GUIStyle criticalStyle;
        private GUIStyle headerStyle;
        private Vector2 scrollPosition;

        public LifeSupportMonitoringWindow(LifeSupportController controller, Settings settings)
            : base("Life Support Monitoring", 240, 400)
        {
            this.controller = controller;
            this.settings = settings;
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

                foreach (var entry in controller.knownVessels)
                {
                    Vessel vessel = FlightGlobals.Vessels.Find(v => v.id.Equals(entry.Key));
                    VesselInfo vesselInfo = entry.Value;

                    GUILayout.Label("Vessel: " + vessel.vesselName + " (" + vessel.vesselType + ")", headerStyle);
                    GUILayout.Label("Crew: " + vesselInfo.numCrew, headerStyle);

                    var crew = vessel.GetVesselCrew().Select(crewMember => controller.knownCrew[crewMember.name]);
                    foreach (CrewMemberInfo crewMemberInfo in crew)
                    {
                        GUIStyle style = labelStyle;
                        StringBuilder text = new StringBuilder(crewMemberInfo.name);
                        if ((currentTime - crewMemberInfo.lastFood) > 1)
                        {
                            text.Append("  Food=").Append(FormatTime(currentTime - crewMemberInfo.lastFood));
                            style = criticalStyle;
                        }
                        if ((currentTime - crewMemberInfo.lastWater) > 1)
                        {
                            text.Append("  Water=").Append(FormatTime(currentTime - crewMemberInfo.lastWater));
                            style = criticalStyle;
                        }
                        if ((currentTime - crewMemberInfo.lastOxygen) > 1)
                        {
                            text.Append("  Oxygen=").Append(FormatTime(currentTime - crewMemberInfo.lastOxygen));
                            style = criticalStyle;
                        }

                        GUILayout.Label(text.ToString(), style);
                    }

                    GUILayout.Space(5);

                    // Electricity
                    if (vesselInfo.electricityStatus == VesselInfo.Status.CRITICAL)
                    {
                        GUILayout.Label("Electric Charge depleted!  " + FormatTime(vesselInfo.lastElectricity - currentTime), criticalStyle);
                    }
                    else
                    {
                        GUIStyle style = labelStyle;
                        if (vesselInfo.electricityStatus == VesselInfo.Status.LOW)
                        {
                            style = warningStyle;
                        }

                        double electricityConsumptionRate = controller.CalculateElectricityConsumptionRate(vessel, vesselInfo);
                        GUILayout.Label("Remaining Electricity: " + FormatTime(vesselInfo.remainingElectricity / electricityConsumptionRate)/* + " (" + vesselInfo.remainingElectricity.ToString("0.000000") + ")"*/, style);
                    }

                    // Food
                    if (vesselInfo.foodStatus == VesselInfo.Status.CRITICAL)
                    {
                        CrewMemberInfo crewMemberInfo = crew.OrderBy(cmi => cmi.lastFood).First();
                        GUILayout.Label("Food depleted! " + FormatTime(crewMemberInfo.lastFood - currentTime), criticalStyle);
                    }
                    else
                    {
                        GUIStyle style = labelStyle;
                        if (vesselInfo.foodStatus == VesselInfo.Status.LOW)
                        {
                            style = warningStyle;
                        }

                        GUILayout.Label("Remaining Food: " + FormatTime(vesselInfo.remainingFood / settings.FoodConsumptionRate / vesselInfo.numCrew)/* + " (" + vesselInfo.remainingFood.ToString("0.000000") + ")"*/, style);
                    }

                    // Water
                    if (vesselInfo.waterStatus == VesselInfo.Status.CRITICAL)
                    {
                        CrewMemberInfo crewMemberInfo = crew.OrderBy(cmi => cmi.lastWater).First();
                        GUILayout.Label("Water depleted! " + FormatTime(crewMemberInfo.lastWater - currentTime), criticalStyle);
                    }
                    else
                    {
                        GUIStyle style = labelStyle;
                        if (vesselInfo.waterStatus == VesselInfo.Status.LOW)
                        {
                            style = warningStyle;
                        }

                        GUILayout.Label("Remaining Water: " + FormatTime(vesselInfo.remainingWater / settings.WaterConsumptionRate / vesselInfo.numCrew)/* + " (" + vesselInfo.remainingWater.ToString("0.000000") + ")"*/, style);
                    }

                    // Oxygen
                    if (vesselInfo.oxygenStatus == VesselInfo.Status.CRITICAL)
                    {
                        CrewMemberInfo crewMemberInfo = crew.OrderBy(cmi => cmi.lastOxygen).First();
                        GUILayout.Label("Oxygen depleted! " + FormatTime(crewMemberInfo.lastOxygen - currentTime), criticalStyle);
                    }
                    else
                    {
                        GUIStyle style = labelStyle;
                        if (vesselInfo.oxygenStatus == VesselInfo.Status.LOW)
                        {
                            style = warningStyle;
                        }

                        GUILayout.Label("Remaining Oxygen: " + FormatTime(vesselInfo.remainingOxygen / settings.OxygenConsumptionRate / vesselInfo.numCrew)/* + " (" + vesselInfo.remainingOxygen.ToString("0.000000") + ")"*/, style);
                    }

                    GUILayout.Space(20);
                }

                //List<ProtoCrewMember> crewRoster = KerbalCrewRoster.CrewRoster;
                //GUILayout.Label("All crew: " + crewRoster.Count, headerStyle);
                //foreach (ProtoCrewMember crewMember in crewRoster)
                //{
                //    string part = (crewMember.KerbalRef != null) ? crewMember.KerbalRef.InPart.ToString() : "Unknown";
                //    string vessel = (crewMember.KerbalRef != null) ? crewMember.KerbalRef.InVessel.vesselName : "Unknown";
                //    GUILayout.Label(crewMember.name + ", " + crewMember.rosterStatus + ", " + vessel + ", " + part, labelStyle);
                //}
            }

            GUILayout.EndVertical();
            GUILayout.EndScrollView();

            GUILayout.Space(8);
        }

        private static string FormatTime(double time)
        {
            const int SECONDS_PER_MINUTE = 60;
            const int SECONDS_PER_HOUR = 3600;
            const int SECONDS_PER_DAY = 24 * SECONDS_PER_HOUR;

            time = (int)time;

            string result = "";
            if (time < 0)
            {
                result += "-";
                time = -time;
            }

            int days = (int)(time / SECONDS_PER_DAY);
            time -= days * SECONDS_PER_DAY;

            int hours = (int)(time / SECONDS_PER_HOUR);
            time -= hours * SECONDS_PER_HOUR;

            int minutes = (int)(time / SECONDS_PER_MINUTE);
            time -= minutes * SECONDS_PER_MINUTE;

            int seconds = (int)time;

            if (days > 0)
            {
                result += days.ToString("#0") + ":";
            }
            result += hours.ToString("00") + ":" + minutes.ToString("00") + ":" + seconds.ToString("00");
            return result;
        }
    }
}
