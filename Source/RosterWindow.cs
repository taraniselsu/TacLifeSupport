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

using RSTUtils;
using UnityEngine;
using KSP.Localization;
using System.Collections.Generic;

namespace Tac
{
    class RosterWindow : Window<RosterWindow>
    {
        private readonly GlobalSettings globalSettings;
        private readonly TacGameSettings gameSettings;

        private GUIStyle labelStyle;
        private GUIStyle warningStyle;
        private GUIStyle criticalStyle;
        private GUIStyle frozenStyle;
        private GUIStyle headerStyle;
        private Vector2 scrollPosition;
        #region Localization Tag cache

        private static string cacheautoLOC_TACLS_00027;
        private static string cacheautoLOC_TACLS_00028;
        private static string cacheautoLOC_TACLS_00029;
        private static string cacheautoLOC_TACLS_00030;
        private static string cacheautoLOC_TACLS_00031;
        private static string cacheautoLOC_TACLS_00032;
        private static string cacheautoLOC_TACLS_00033;
        private static string cacheautoLOC_TACLS_00034;
        private static string cacheautoLOC_TACLS_00035;
        
        private void cacheLocalStrings()
        {
            cacheautoLOC_TACLS_00027 = Localizer.Format("#autoLOC_TACLS_00027"); // cacheautoLOC_TACLS_00027 = \u0020\u0020Prelaunch - Frozen
            cacheautoLOC_TACLS_00028 = Localizer.Format("#autoLOC_TACLS_00028"); // cacheautoLOC_TACLS_00028 = \u0020\u0020Frozen
            cacheautoLOC_TACLS_00029 = Localizer.Format("#autoLOC_TACLS_00029"); // cacheautoLOC_TACLS_00029 = \u0020\u0020Prelaunch
            cacheautoLOC_TACLS_00030 = Localizer.Format("#autoLOC_TACLS_00030"); // cacheautoLOC_TACLS_00030 = \u0020\u0020Rescue Me!
            cacheautoLOC_TACLS_00031 = Localizer.Format("#autoLOC_TACLS_00031"); // cacheautoLOC_TACLS_00031 = \u0020\u0020Last updated:\u0020
            cacheautoLOC_TACLS_00032 = Localizer.Format("#autoLOC_TACLS_00032"); // cacheautoLOC_TACLS_00032 = \u0020\u0020Last food:\u0020
            cacheautoLOC_TACLS_00033 = Localizer.Format("#autoLOC_TACLS_00033"); // cacheautoLOC_TACLS_00033 = \u0020\u0020Last water:\u0020
            cacheautoLOC_TACLS_00034 = Localizer.Format("#autoLOC_TACLS_00034"); // cacheautoLOC_TACLS_00034 = \u0020\u0020Hibernating:\u0020
            cacheautoLOC_TACLS_00035 = Localizer.Format("#autoLOC_TACLS_00035"); // cacheautoLOC_TACLS_00035 = No Crew.
        }

        #endregion
        public RosterWindow(AppLauncherToolBar TACMenuAppLToolBar, GlobalSettings globalSettings, TacGameSettings gameSettings)
            : base(TACMenuAppLToolBar, Localizer.Format("#autoLOC_TACLS_00026"), 370, 200) // #autoLOC_TACLS_00026 = Life Support Crew Roster
        {
            this.globalSettings = globalSettings;
            this.gameSettings = gameSettings;
            cacheLocalStrings();
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

                frozenStyle = new GUIStyle(labelStyle);
                frozenStyle.normal.textColor = Color.cyan;

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

            Dictionary<string, CrewMemberInfo>.Enumerator crewenumerator = gameSettings.knownCrew.GetDictEnumerator();
            while (crewenumerator.MoveNext())
            {
                CrewMemberInfo crewInfo = crewenumerator.Current.Value;
                GUILayout.Label(crewInfo.name + " (" + crewInfo.vesselName + ")", headerStyle);
                if (crewInfo.DFfrozen)
                {
                    if (crewInfo.vesselIsPreLaunch)
                    {
                        GUILayout.Label(cacheautoLOC_TACLS_00027, frozenStyle); // cacheautoLOC_TACLS_00027 = \u0020\u0020Prelaunch - Frozen
                    }
                    else
                    {
                        GUILayout.Label(cacheautoLOC_TACLS_00028, frozenStyle); // cacheautoLOC_TACLS_00028 = \u0020\u0020Frozen
                    }
                }
                else if (crewInfo.vesselIsPreLaunch)
                {
                    GUILayout.Label(cacheautoLOC_TACLS_00029, labelStyle); // cacheautoLOC_TACLS_00029 = \u0020\u0020Prelaunch
                }
                else if (crewInfo.recoverykerbal)
                {
                    GUILayout.Label(cacheautoLOC_TACLS_00030, labelStyle); // cacheautoLOC_TACLS_00030 = \u0020\u0020Rescue Me!
                }
                else
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(cacheautoLOC_TACLS_00031, labelStyle, GUILayout.Width(150)); // cacheautoLOC_TACLS_00031 = \u0020\u0020Last updated:\u0020
                    GUILayout.Label(Utilities.FormatTime(currentTime - crewInfo.lastUpdate), labelStyle);
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(cacheautoLOC_TACLS_00032, labelStyle, GUILayout.Width(150)); // cacheautoLOC_TACLS_00032 = \u0020\u0020Last food:\u0020
                    GUILayout.Label(Utilities.FormatTime(currentTime - crewInfo.lastFood),
                        getStyle(crewInfo.lastUpdate, crewInfo.lastFood, HighLogic.CurrentGame.Parameters.CustomParams<TAC_SettingsParms_Sec3>().MaxTimeWithoutFood));
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(cacheautoLOC_TACLS_00033, labelStyle, GUILayout.Width(150)); // cacheautoLOC_TACLS_00033 = \u0020\u0020Last water:\u0020
                    GUILayout.Label(Utilities.FormatTime(currentTime - crewInfo.lastWater),
                        getStyle(crewInfo.lastUpdate, crewInfo.lastWater, HighLogic.CurrentGame.Parameters.CustomParams<TAC_SettingsParms_Sec3>().MaxTimeWithoutWater));
                    GUILayout.EndHorizontal();
                    if (HighLogic.CurrentGame.Parameters.CustomParams<TAC_SettingsParms>().hibernate != "Die" ||
                        crewInfo.hibernating)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Label(cacheautoLOC_TACLS_00034, labelStyle, GUILayout.Width(150));  // cacheautoLOC_TACLS_00034 = \u0020\u0020Hibernating:\u0020
                        GUILayout.Label(crewInfo.hibernating.ToString(), labelStyle);
                        GUILayout.EndHorizontal();
                    }
                }
                GUILayout.Space(10);
            }
            if (gameSettings.knownCrew.Count == 0)
            {
                GUILayout.Label(cacheautoLOC_TACLS_00035, headerStyle); // cacheautoLOC_TACLS_00035 = No Crew.
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