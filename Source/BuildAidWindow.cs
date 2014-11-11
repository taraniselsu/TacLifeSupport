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
    class BuildAidWindow : Window<BuildAidWindow>
    {
        private readonly GlobalSettings globalSettings;

        private float lastUpdateTime = 0.0f;
        private float updateInterval = 1.0f;

        private GUIStyle labelStyle;
        private GUIStyle valueStyle;

        private int numCrew;
        private int maxCrew;
        private Dictionary<int, String> amounts = new Dictionary<int, String>();
        private Dictionary<int, String> durations = new Dictionary<int,string>();
        private Dictionary<int, String> maxCrewDurations = new Dictionary<int, string>();


        public BuildAidWindow(GlobalSettings globalSettings)
            : base("Life Support Build Aid", 300, 180)
        {
            this.globalSettings = globalSettings;
        }

        protected override void ConfigureStyles()
        {
            base.ConfigureStyles();

            if (labelStyle == null)
            {
                labelStyle = new GUIStyle(GUI.skin.label);
                labelStyle.fontStyle = FontStyle.Normal;
                labelStyle.normal.textColor = Color.white;
                labelStyle.margin.top = 0;
                labelStyle.margin.bottom = 0;
                labelStyle.padding.top = 0;
                labelStyle.padding.bottom = 1;
                labelStyle.wordWrap = false;

                valueStyle = new GUIStyle(labelStyle);
                valueStyle.alignment = TextAnchor.MiddleRight;
                valueStyle.stretchWidth = true;
            }
        }

        protected override void DrawWindowContents(int windowId)
        {
            float now = Time.time;
            if ((now - lastUpdateTime) > updateInterval)
            {
                lastUpdateTime = now;
                UpdateValues();
            }

            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical();
            GUILayout.Label("Current crew", labelStyle);
            GUILayout.Label("Maximum crew", labelStyle);
            GUILayout.EndVertical();

            GUILayout.BeginVertical();
            GUILayout.Label(numCrew.ToString(), valueStyle);
            GUILayout.Label(maxCrew.ToString(), valueStyle);
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();

            GUILayout.Space(5f);

            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical();
            GUILayout.Label("  ", labelStyle);
            //TODO load resource names
            foreach (int resource in globalSettings.kerbalRequirements)
            {
                String resourceName = PartResourceLibrary.Instance.GetDefinition(resource).name;
                GUILayout.Label(resourceName, labelStyle);
            }
            GUILayout.Space(10f);
            foreach (int resource in globalSettings.kerbalProduction)
            {
                String resourceName = PartResourceLibrary.Instance.GetDefinition(resource).name;
                GUILayout.Label(resourceName, labelStyle);
            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical();
            GUILayout.Label("Amount", valueStyle);
            foreach (int resource in globalSettings.kerbalRequirements)
            {
                GUILayout.Label(amounts[resource], valueStyle);
            }
            GUILayout.Space(10f);
            foreach (int resource in globalSettings.kerbalProduction)
            {
                GUILayout.Label(amounts[resource], valueStyle);
            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical();
            GUILayout.Label("With Current Crew", valueStyle);
            foreach (int resource in globalSettings.kerbalRequirements)
            {
                GUILayout.Label(durations[resource], valueStyle);
            }
            GUILayout.Space(10f);
            foreach (int resource in globalSettings.kerbalProduction)
            {
                GUILayout.Label(durations[resource], valueStyle);
            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical();
            GUILayout.Label("With Max Crew", valueStyle);
            foreach (int resource in globalSettings.kerbalRequirements)
            {
                GUILayout.Label(maxCrewDurations[resource], valueStyle);
            }
            GUILayout.Space(10f);
            foreach (int resource in globalSettings.kerbalProduction)
            {
                GUILayout.Label(maxCrewDurations[resource], valueStyle);
            }
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        private void UpdateValues()
        {
            if (EditorLogic.fetch != null)
            {
                numCrew = 0;
                maxCrew = 0;
                int numOccupiedParts = 0;
                int numOccupiableParts = 0;
                Dictionary<int, ResourceLimits> resources = new Dictionary<int,ResourceLimits>();

                foreach (int resource in globalSettings.kerbalProductionRates.Keys)
                {
                    resources[resource] = new ResourceLimits(0, 0);
                }
                foreach (Part part in EditorLogic.fetch.ship.parts)
                {
                    if (part.CrewCapacity > 0)
                    {
                        ++numOccupiableParts;
                        maxCrew += part.CrewCapacity;
                    }

                    foreach (PartResource partResource in part.Resources)
                    {
                        if (!resources.ContainsKey(partResource.info.id))
                        {
                            resources[partResource.info.id] = new ResourceLimits(0, 0);
                        }
                        ResourceLimits limits = resources[partResource.info.id];
                        limits.available += partResource.amount;
                        limits.maximum += partResource.maxAmount;
                    }
                }

                CMAssignmentDialog dialog = CMAssignmentDialog.Instance;
                if (dialog != null)
                {
                    VesselCrewManifest manifest = dialog.GetManifest();
                    if (manifest != null)
                    {
                        foreach (PartCrewManifest pcm in manifest)
                        {
                            int partCrewCount = pcm.GetPartCrew().Count(c => c != null);
                            if (partCrewCount > 0)
                            {
                                ++numOccupiedParts;
                                numCrew += partCrewCount;
                            }
                        }
                    }
                }

                foreach (KeyValuePair<int, ResourceLimits> resource in resources)
                {
                    if (resource.Value.available > 0)
                    {
                        amounts[resource.Key] = resource.Value.available.ToString("#,##0.00");
                    }
                    else
                    {
                        amounts[resource.Key] = (resource.Value.maximum - resource.Value.available).ToString("#,##0.00");
                    }
                }

                foreach (KeyValuePair<int, double> rates in globalSettings.kerbalProductionRates) {
                    if (rates.Value<0)//this resource is used
                    {
                        durations[rates.Key] = formatTime(resources[rates.Key].available / -rates.Value, numCrew);
                        maxCrewDurations[rates.Key] = formatTime(resources[rates.Key].available / -rates.Value, maxCrew);
                    }
                    else if (rates.Value>0)//this resource is produced
                    {
                        durations[rates.Key] = formatTime((resources[rates.Key].maximum - resources[rates.Key].available) / rates.Value, numCrew);
                        maxCrewDurations[rates.Key] = formatTime((resources[rates.Key].maximum - resources[rates.Key].available) / rates.Value, maxCrew);
                    }
                }
            }
        }

        private String formatTime(double time, int crew)
        {
            if (crew == 0) return "-";
            return Utilities.FormatTime(time/crew);
        }
    }
}
