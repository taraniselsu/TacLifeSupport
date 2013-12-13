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
        private string foodDuration = "";
        private string waterDuration = "";
        private string oxygenDuration = "";
        private string electricityDuration = "";
        private string foodDurationMaxCrew = "";
        private string waterDurationMaxCrew = "";
        private string oxygenDurationMaxCrew = "";
        private string electricityDurationMaxCrew = "";

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
            GUILayout.Label("Food", labelStyle);
            GUILayout.Label("Water", labelStyle);
            GUILayout.Label("Oxygen", labelStyle);
            GUILayout.Label("Electricity", labelStyle);
            GUILayout.EndVertical();

            GUILayout.BeginVertical();
            GUILayout.Label("With Current Crew", valueStyle);
            GUILayout.Label(foodDuration, valueStyle);
            GUILayout.Label(waterDuration, valueStyle);
            GUILayout.Label(oxygenDuration, valueStyle);
            GUILayout.Label(electricityDuration, valueStyle);
            GUILayout.EndVertical();

            GUILayout.BeginVertical();
            GUILayout.Label("With Max Crew", valueStyle);
            GUILayout.Label(foodDurationMaxCrew, valueStyle);
            GUILayout.Label(waterDurationMaxCrew, valueStyle);
            GUILayout.Label(oxygenDurationMaxCrew, valueStyle);
            GUILayout.Label(electricityDurationMaxCrew, valueStyle);
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
                double food = 0;
                double water = 0;
                double oxygen = 0;
                double electricity = 0;

                foreach (Part part in EditorLogic.fetch.ship.parts)
                {
                    if (part.CrewCapacity > 0)
                    {
                        ++numOccupiableParts;
                        maxCrew += part.CrewCapacity;
                    }

                    foreach (PartResource partResource in part.Resources)
                    {
                        if (partResource.info.id == globalSettings.FoodId)
                        {
                            food += partResource.amount;
                        }
                        else if (partResource.info.id == globalSettings.WaterId)
                        {
                            water += partResource.amount;
                        }
                        else if (partResource.info.id == globalSettings.OxygenId)
                        {
                            oxygen += partResource.amount;
                        }
                        else if (partResource.info.id == globalSettings.ElectricityId)
                        {
                            electricity += partResource.amount;
                        }
                    }
                }

                CMAssignmentDialog dialog = EditorLogic.fetch.CrewAssignmentDialog;
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

                if (numCrew > 0)
                {
                    foodDuration = Utilities.FormatTime(food / globalSettings.FoodConsumptionRate / numCrew);
                    waterDuration = Utilities.FormatTime(water / globalSettings.WaterConsumptionRate / numCrew);
                    oxygenDuration = Utilities.FormatTime(oxygen / globalSettings.OxygenConsumptionRate / numCrew);
                    electricityDuration = Utilities.FormatTime(electricity / CalculateElectricityConsumptionRate(numCrew, numOccupiedParts));
                }
                else
                {
                    foodDuration = "-";
                    waterDuration = "-";
                    oxygenDuration = "-";
                    electricityDuration = "-";
                }

                if (maxCrew > 0)
                {
                    foodDurationMaxCrew = Utilities.FormatTime(food / globalSettings.FoodConsumptionRate / maxCrew);
                    waterDurationMaxCrew = Utilities.FormatTime(water / globalSettings.WaterConsumptionRate / maxCrew);
                    oxygenDurationMaxCrew = Utilities.FormatTime(oxygen / globalSettings.OxygenConsumptionRate / maxCrew);
                    electricityDurationMaxCrew = Utilities.FormatTime(electricity / CalculateElectricityConsumptionRate(maxCrew, numOccupiableParts));
                }
                else
                {
                    foodDurationMaxCrew = "-";
                    waterDurationMaxCrew = "-";
                    oxygenDurationMaxCrew = "-";
                    electricityDurationMaxCrew = "-";
                }
            }
        }

        private double CalculateElectricityConsumptionRate(int numCrew, int numParts)
        {
            return (globalSettings.ElectricityConsumptionRate * numCrew) + (globalSettings.BaseElectricityConsumptionRate * numParts);
        }
    }
}
