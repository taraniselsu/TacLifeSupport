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

namespace Tac
{
    class LifeSupportMonitoringWindow : Window<LifeSupportMonitoringWindow>
    {        
        private readonly RosterWindow rosterWindow;
        private readonly string version;
        private readonly bool DeepFreezeInstalled = false;
        
        private GUIStyle labelStyle;
        private GUIStyle warningStyle;
        private GUIStyle criticalStyle;
        private GUIStyle headerStyle;
        private GUIStyle scrollStyle;
        private GUIStyle versionStyle;
        private GUIStyle frozenStyle;
        private Vector2 scrollPosition = Vector2.zero;

        #region Localization Tag cache

        private static string cacheautoLOC_TACLS_00002;
        private static string cacheautoLOC_TACLS_00003;
        private static string cacheautoLOC_TACLS_00004;
        private static string cacheautoLOC_TACLS_00005;
        private static string cacheautoLOC_TACLS_00006;
        private static string cacheautoLOC_TACLS_00007;
        private static string cacheautoLOC_TACLS_00008;
        private static string cacheautoLOC_TACLS_00009;
        private static string cacheautoLOC_TACLS_00010;
        private static string cacheautoLOC_TACLS_00011;
        private static string cacheautoLOC_TACLS_00012;
        private static string cacheautoLOC_TACLS_00233;
        private void cacheLocalStrings()
        {
            cacheautoLOC_TACLS_00002 = Localizer.Format("#autoLOC_TACLS_00002"); // cacheautoLOC_TACLS_00002 = No Vessels.
            cacheautoLOC_TACLS_00003 = Localizer.Format("#autoLOC_TACLS_00003"); // cacheautoLOC_TACLS_00003 = R
            cacheautoLOC_TACLS_00004 = Localizer.Format("#autoLOC_TACLS_00004", version); // cacheautoLOC_TACLS_00004 = TAC Life Support v<<1>>
            cacheautoLOC_TACLS_00005 = Localizer.Format("#autoLOC_TACLS_00005"); // cacheautoLOC_TACLS_00005 = crew
            cacheautoLOC_TACLS_00006 = Localizer.Format("#autoLOC_TACLS_00006"); // cacheautoLOC_TACLS_00006 = \u0020\u0020Prelaunch Vessel
            cacheautoLOC_TACLS_00007 = Localizer.Format("#autoLOC_TACLS_00007"); // cacheautoLOC_TACLS_00007 = \u0020\u0020Rescue Vessel
            cacheautoLOC_TACLS_00008 = Localizer.Format("#autoLOC_TACLS_00008"); // cacheautoLOC_TACLS_00008 = \u0020\u0020Last updated:
            cacheautoLOC_TACLS_00009 = Localizer.Format("#autoLOC_TACLS_00009"); // cacheautoLOC_TACLS_00009 = \u0020\u0020Food remaining:
            cacheautoLOC_TACLS_00010 = Localizer.Format("#autoLOC_TACLS_00010"); // cacheautoLOC_TACLS_00010 = \u0020\u0020Water remaining:
            cacheautoLOC_TACLS_00011 = Localizer.Format("#autoLOC_TACLS_00011"); // cacheautoLOC_TACLS_00011 = \u0020\u0020Oxygen remaining:
            cacheautoLOC_TACLS_00012 = Localizer.Format("#autoLOC_TACLS_00012"); // cacheautoLOC_TACLS_00012 = \u0020\u0020Electricity remaining:
            cacheautoLOC_TACLS_00233 = Localizer.Format("#autoLOC_TACLS_00233"); // cacheautoLOC_TACLS_00012 = \u0020\u0020Out of EC, Windows are Open
        }

        #endregion

        public LifeSupportMonitoringWindow(AppLauncherToolBar TACMenuAppLToolBar,  RosterWindow rosterWindow)
            : base(TACMenuAppLToolBar, Localizer.Format("#autoLOC_TACLS_00001"), 300, 300)
        {                      
            this.rosterWindow = rosterWindow;
            version = Utilities.GetDllVersion(this);
            DeepFreezeInstalled = RSTUtils.Utilities.IsModInstalled("DeepFreeze");
            cacheLocalStrings();
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

                frozenStyle = new GUIStyle(labelStyle);
                frozenStyle.normal.textColor = Color.cyan;
                frozenStyle.fontStyle = FontStyle.Bold;

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
            
            for (int i = 0; i < LifeSupportController.Instance.knownVesselsList.Count; i++)
            { 
                DrawVesselInfo(LifeSupportController.Instance.knownVesselsList[i].Value, currentTime);
                GUILayout.Space(10);
            }
            if (LifeSupportController.Instance.knownVesselsList.Count == 0)
            {
                GUILayout.Label(cacheautoLOC_TACLS_00002, headerStyle);  // cacheautoLOC_TACLS_00002 = No Vessels.
            }

            GUILayout.EndVertical();
            GUILayout.EndScrollView();

            GUILayout.Space(8);

            if (GUI.Button(new Rect(windowPos.width - 46, 4, 20, 20), cacheautoLOC_TACLS_00003, closeButtonStyle)) // cacheautoLOC_TACLS_00003 = R
            {
                rosterWindow.SetVisible(!rosterWindow.IsVisible());
            }

            GUI.Label(new Rect(4, windowPos.height - 13, windowPos.width - 20, 12), cacheautoLOC_TACLS_00004, versionStyle); // cacheautoLOC_TACLS_00004 = TAC Life Support v<<1>> where 1 = version
        }

        private void DrawVesselInfo(VesselInfo vesselInfo, double currentTime)
        {
            if (DeepFreezeInstalled & vesselInfo.numFrozenCrew > 0)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(vesselInfo.vesselName + " (" + vesselInfo.numCrew + "/", headerStyle);
                GUILayout.Label(vesselInfo.numFrozenCrew.ToString(), frozenStyle);
                GUILayout.Label(" " + cacheautoLOC_TACLS_00005 + ") [" + vesselInfo.vesselType + "]", headerStyle); //cacheautoLOC_TACLS_00005 = crew

                GUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.Label(vesselInfo.vesselName + " (" + vesselInfo.numCrew + cacheautoLOC_TACLS_00005 + ") [" +vesselInfo.vesselType + "]", headerStyle); //cacheautoLOC_TACLS_00005 = crew
            }
            if (vesselInfo.vesselIsPreLaunch)
            {
                GUILayout.Label(cacheautoLOC_TACLS_00006, labelStyle); // cacheautoLOC_TACLS_00006 = \u0020\u0020Prelaunch Vessel
                if (vesselInfo.numCrew > 0)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(cacheautoLOC_TACLS_00012, getStyle(vesselInfo.electricityStatus), GUILayout.Width(150)); // cacheautoLOC_TACLS_00012 = \u0020\u0020Electricity remaining:
                    GUILayout.Label(Utilities.FormatTime(vesselInfo.estimatedTimeElectricityDepleted - currentTime), getStyle(vesselInfo.electricityStatus));
                    GUILayout.EndHorizontal();
                    if (vesselInfo.windowOpen)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Label(cacheautoLOC_TACLS_00233, getStyle(VesselInfo.Status.CRITICAL), GUILayout.Width(150)); // #autoLOC_TACLS_00233 = \u0020\u0020Out of EC, Windows are Open
                        GUILayout.EndHorizontal();
                    }
                }
            }
            else if (vesselInfo.recoveryvessel)
            {
                GUILayout.Label(cacheautoLOC_TACLS_00007, labelStyle); // cacheautoLOC_TACLS_00007 = \u0020\u0020Rescue Vessel
            }
            else
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(cacheautoLOC_TACLS_00008, getStyle(vesselInfo.foodStatus), GUILayout.Width(150)); // cacheautoLOC_TACLS_00008 = \u0020\u0020Last updated:
                GUILayout.Label(Utilities.FormatTime(currentTime - vesselInfo.lastUpdate), labelStyle);
                GUILayout.EndHorizontal();
                if (vesselInfo.numCrew > 0)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(cacheautoLOC_TACLS_00009, getStyle(vesselInfo.foodStatus), GUILayout.Width(150)); // cacheautoLOC_TACLS_00009 = \u0020\u0020Food remaining:
                    GUILayout.Label(Utilities.FormatTime(vesselInfo.estimatedTimeFoodDepleted - currentTime),getStyle(vesselInfo.foodStatus));
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(cacheautoLOC_TACLS_00010, getStyle(vesselInfo.waterStatus), GUILayout.Width(150)); // cacheautoLOC_TACLS_00010 = \u0020\u0020Water remaining:
                    GUILayout.Label(Utilities.FormatTime(vesselInfo.estimatedTimeWaterDepleted - currentTime),getStyle(vesselInfo.waterStatus));
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(cacheautoLOC_TACLS_00011, getStyle(vesselInfo.oxygenStatus), GUILayout.Width(150)); // cacheautoLOC_TACLS_00011 = \u0020\u0020Oxygen remaining:
                    GUILayout.Label(Utilities.FormatTime(vesselInfo.estimatedTimeOxygenDepleted - currentTime),getStyle(vesselInfo.oxygenStatus));
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(cacheautoLOC_TACLS_00012, getStyle(vesselInfo.electricityStatus),GUILayout.Width(150)); // cacheautoLOC_TACLS_00012 = \u0020\u0020Electricity remaining:
                    GUILayout.Label(Utilities.FormatTime(vesselInfo.estimatedTimeElectricityDepleted - currentTime),getStyle(vesselInfo.electricityStatus));
                    GUILayout.EndHorizontal();
                    if (vesselInfo.windowOpen)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Label(cacheautoLOC_TACLS_00233, getStyle(VesselInfo.Status.CRITICAL), GUILayout.Width(150)); // #autoLOC_TACLS_00233 = \u0020\u0020Out of EC, Windows are Open
                        GUILayout.EndHorizontal();
                    }
                }
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
