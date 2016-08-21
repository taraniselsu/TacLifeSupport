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

using System.Linq;
using KSP.UI;
using RSTUtils;
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
        private string food = "";
        private string water = "";
        private string oxygen = "";
        private string electricity = "";
        private string maxWaste = "";
        private string maxWasteWater = "";
        private string maxCarbonDioxide = "";
        private string foodDuration = "";
        private string waterDuration = "";
        private string oxygenDuration = "";
        private string electricityDuration = "";
        private string wasteRoom = "";
        private string wasteWaterRoom = "";
        private string carbonDioxideRoom = "";
        private string foodDurationMaxCrew = "";
        private string waterDurationMaxCrew = "";
        private string oxygenDurationMaxCrew = "";
        private string electricityDurationMaxCrew = "";
        private string wasteRoomMaxCrew = "";
        private string wasteWaterRoomMaxCrew = "";
        private string carbonDioxideRoomMaxCrew = "";

        public BuildAidWindow(AppLauncherToolBar TACMenuAppLToolBar, GlobalSettings globalSettings)
            : base(TACMenuAppLToolBar, "Life Support Build Aid", 300, 180)
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
            GUILayout.Space(10f);
            GUILayout.Label("Waste", labelStyle);
            GUILayout.Label("Waste Water", labelStyle);
            GUILayout.Label("Carbon Dioxide", labelStyle);
            GUILayout.EndVertical();

            GUILayout.BeginVertical();
            GUILayout.Label("Amount", valueStyle);
            GUILayout.Label(food, valueStyle);
            GUILayout.Label(water, valueStyle);
            GUILayout.Label(oxygen, valueStyle);
            GUILayout.Label(electricity, valueStyle);
            GUILayout.Space(10f);
            GUILayout.Label(maxWaste, valueStyle);
            GUILayout.Label(maxWasteWater, valueStyle);
            GUILayout.Label(maxCarbonDioxide, valueStyle);
            GUILayout.EndVertical();

            GUILayout.BeginVertical();
            GUILayout.Label("With Current Crew", valueStyle);
            GUILayout.Label(foodDuration, valueStyle);
            GUILayout.Label(waterDuration, valueStyle);
            GUILayout.Label(oxygenDuration, valueStyle);
            GUILayout.Label(electricityDuration, valueStyle);
            GUILayout.Space(10f);
            GUILayout.Label(wasteRoom, valueStyle);
            GUILayout.Label(wasteWaterRoom, valueStyle);
            GUILayout.Label(carbonDioxideRoom, valueStyle);
            GUILayout.EndVertical();

            GUILayout.BeginVertical();
            GUILayout.Label("With Max Crew", valueStyle);
            GUILayout.Label(foodDurationMaxCrew, valueStyle);
            GUILayout.Label(waterDurationMaxCrew, valueStyle);
            GUILayout.Label(oxygenDurationMaxCrew, valueStyle);
            GUILayout.Label(electricityDurationMaxCrew, valueStyle);
            GUILayout.Space(10f);
            GUILayout.Label(wasteRoomMaxCrew, valueStyle);
            GUILayout.Label(wasteWaterRoomMaxCrew, valueStyle);
            GUILayout.Label(carbonDioxideRoomMaxCrew, valueStyle);
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
                double foodValue = 0;
                double waterValue = 0;
                double oxygenValue = 0;
                double electricityValue = 0;
                double wasteValue = 0;
                double wasteWaterValue = 0;
                double carbonDioxideValue = 0;

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
                            foodValue += partResource.amount;
                        }
                        else if (partResource.info.id == globalSettings.WaterId)
                        {
                            waterValue += partResource.amount;
                        }
                        else if (partResource.info.id == globalSettings.OxygenId)
                        {
                            oxygenValue += partResource.amount;
                        }
                        else if (partResource.info.id == globalSettings.ElectricityId)
                        {
                            electricityValue += partResource.amount;
                        }
                        else if (partResource.info.id == globalSettings.WasteId)
                        {
                            wasteValue += partResource.maxAmount;
                        }
                        else if (partResource.info.id == globalSettings.WasteWaterId)
                        {
                            wasteWaterValue += partResource.maxAmount;
                        }
                        else if (partResource.info.id == globalSettings.CO2Id)
                        {
                            carbonDioxideValue += partResource.maxAmount;
                        }
                    }
                }

                CrewAssignmentDialog dialog = CrewAssignmentDialog.Instance;
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

                food = foodValue.ToString("#,##0.00");
                water = waterValue.ToString("#,##0.00");
                oxygen = oxygenValue.ToString("#,##0.00");
                electricity = electricityValue.ToString("#,##0.00");
                maxWaste = wasteValue.ToString("#,##0.00");
                maxWasteWater = wasteWaterValue.ToString("#,##0.00");
                maxCarbonDioxide = carbonDioxideValue.ToString("#,##0.00");

                if (numCrew > 0)
                {
                    foodDuration = Utilities.FormatTime(foodValue / globalSettings.FoodConsumptionRate / numCrew);
                    waterDuration = Utilities.FormatTime(waterValue / globalSettings.WaterConsumptionRate / numCrew);
                    oxygenDuration = Utilities.FormatTime(oxygenValue / globalSettings.OxygenConsumptionRate / numCrew);
                    electricityDuration = Utilities.FormatTime(electricityValue / CalculateElectricityConsumptionRate(numCrew, numOccupiedParts));
                    wasteRoom = Utilities.FormatTime(wasteValue / globalSettings.WasteProductionRate / numCrew);
                    wasteWaterRoom = Utilities.FormatTime(wasteWaterValue / globalSettings.WasteWaterProductionRate / numCrew);
                    carbonDioxideRoom = Utilities.FormatTime(carbonDioxideValue / globalSettings.CO2ProductionRate / numCrew);
                }
                else
                {
                    foodDuration = "-";
                    waterDuration = "-";
                    oxygenDuration = "-";
                    electricityDuration = "-";
                    wasteRoom = "-";
                    wasteWaterRoom = "-";
                    carbonDioxideRoom = "-";
                }

                if (maxCrew > 0)
                {
                    foodDurationMaxCrew = Utilities.FormatTime(foodValue / globalSettings.FoodConsumptionRate / maxCrew);
                    waterDurationMaxCrew = Utilities.FormatTime(waterValue / globalSettings.WaterConsumptionRate / maxCrew);
                    oxygenDurationMaxCrew = Utilities.FormatTime(oxygenValue / globalSettings.OxygenConsumptionRate / maxCrew);
                    electricityDurationMaxCrew = Utilities.FormatTime(electricityValue / CalculateElectricityConsumptionRate(maxCrew, numOccupiableParts));
                    wasteRoomMaxCrew = Utilities.FormatTime(wasteValue / globalSettings.WasteProductionRate / maxCrew);
                    wasteWaterRoomMaxCrew = Utilities.FormatTime(wasteWaterValue / globalSettings.WasteWaterProductionRate / maxCrew);
                    carbonDioxideRoomMaxCrew = Utilities.FormatTime(carbonDioxideValue / globalSettings.CO2ProductionRate / maxCrew);
                }
                else
                {
                    foodDurationMaxCrew = "-";
                    waterDurationMaxCrew = "-";
                    oxygenDurationMaxCrew = "-";
                    electricityDurationMaxCrew = "-";
                    wasteRoomMaxCrew = "-";
                    wasteWaterRoomMaxCrew = "-";
                    carbonDioxideRoomMaxCrew = "-";
                }
            }
        }

        private double CalculateElectricityConsumptionRate(int numCrew, int numParts)
        {
            return (globalSettings.ElectricityConsumptionRate * numCrew) + (globalSettings.BaseElectricityConsumptionRate * numParts);
        }
    }
}
