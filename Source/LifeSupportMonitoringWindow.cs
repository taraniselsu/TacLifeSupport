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
        private const int SECONDS_PER_MINUTE = 60;
        private const int SECONDS_PER_HOUR = 3600;
        private const int SECONDS_PER_DAY = 24 * SECONDS_PER_HOUR;

        private readonly LifeSupportFlightController controller;
        private readonly Settings settings;

        public LifeSupportMonitoringWindow(LifeSupportFlightController controller, Settings settings)
            : base("Life Support Monitoring")
        {
            this.controller = controller;
            this.settings = settings;
        }

        protected override void DrawWindowContents(int windowId)
        {
            GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.fontStyle = FontStyle.Normal;
            labelStyle.margin.top = 1;
            labelStyle.margin.bottom = 1;
            labelStyle.normal.textColor = Color.white;
            labelStyle.wordWrap = false;

            GUIStyle warningStyle = new GUIStyle(labelStyle);
            warningStyle.normal.textColor = Color.red;

            string vesselName = "Unknown";
            string vesselType = "Unknown";
            int numCrew = -1;
            string numCrewString = "Unknown";
            Vessel vessel = controller.currentVessel;
            if (vessel != null)
            {
                vesselName = vessel.vesselName;
                vesselType = vessel.vesselType.ToString();
                numCrew = vessel.GetVesselCrew().Count(crew => crew.rosterStatus != ProtoCrewMember.RosterStatus.DEAD
                    && crew.rosterStatus != ProtoCrewMember.RosterStatus.RESPAWN);
                numCrewString = numCrew.ToString();
            }

            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical();
            GUILayout.Label("Vessel:", labelStyle);
            GUILayout.Label("Type:", labelStyle);
            GUILayout.Label("Kerbals:", labelStyle);
            GUILayout.Label("Last update time:", labelStyle);
            GUILayout.EndVertical();

            GUILayout.BeginVertical();
            GUILayout.Label(vesselName, labelStyle);
            GUILayout.Label(vesselType, labelStyle);
            GUILayout.Label(numCrewString, labelStyle);
            GUILayout.Label(controller.LastUpdateTime.ToString("#,#"), labelStyle);
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();

            double currentTime = Planetarium.GetUniversalTime();

            if (controller.FoodCritical)
            {
                string remaining = "";
                if (numCrew >= 1)
                {
                    remaining = " Remaining: " + FormatTime(controller.RemainingFood / settings.FoodConsumptionRate / numCrew * SECONDS_PER_DAY * 2);
                }
                GUILayout.Label("Food level critical!" + remaining, warningStyle);
            }
            else if (numCrew >= 1)
            {
                GUILayout.Label("Remaining Food: " + FormatTime(controller.RemainingFood / settings.FoodConsumptionRate / numCrew * SECONDS_PER_DAY), labelStyle);
            }

            if (controller.WaterCritical)
            {
                string remaining = "";
                if (numCrew >= 1)
                {
                    remaining = " Remaining: " + FormatTime(controller.RemainingWater / settings.WaterConsumptionRate / numCrew * SECONDS_PER_DAY * 2);
                }
                GUILayout.Label("Water level critical!" + remaining, warningStyle);
            }
            else if (numCrew >= 1)
            {
                GUILayout.Label("Remaining Water: " + FormatTime(controller.RemainingWater / settings.WaterConsumptionRate / numCrew * SECONDS_PER_DAY), labelStyle);
            }

            if (controller.OxygenCritical)
            {
                string remaining = "";
                if (numCrew >= 1)
                {
                    remaining = " Remaining: " + FormatTime(controller.RemainingOxygen / settings.OxygenConsumptionRate / numCrew * SECONDS_PER_DAY);
                }
                GUILayout.Label("Oxygen level critical!" + remaining, warningStyle);
            }
            else if (numCrew >= 1)
            {
                GUILayout.Label("Remaining Oxygen: " + FormatTime(controller.RemainingOxygen / settings.OxygenConsumptionRate / numCrew * SECONDS_PER_DAY), labelStyle);
            }

            GUILayout.EndVertical();
        }

        private static string FormatTime(double time)
        {
            if (time < 0)
            {
                return "";
            }

            int days = (int)(time / SECONDS_PER_DAY);
            time -= days * SECONDS_PER_DAY;

            int hours = (int)(time / SECONDS_PER_HOUR);
            time -= hours * SECONDS_PER_HOUR;

            int minutes = (int)(time / SECONDS_PER_MINUTE);
            time -= minutes * SECONDS_PER_MINUTE;

            int seconds = (int)time;

            string result = "";
            if (days > 0)
            {
                result += days.ToString("#0") + ":";
            }
            result += hours.ToString("00") + ":" + minutes.ToString("00") + ":" + seconds.ToString("00");
            return result;
        }
    }
}
