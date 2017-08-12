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

using System.Collections.Generic;
using KSP.UI;
using RSTUtils;
using UnityEngine;
using KSP.Localization;

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

        #region Localization Tag cache
                
        private static string cacheautoLOC_TACLS_00014;
        private static string cacheautoLOC_TACLS_00015;
        private static string cacheautoLOC_TACLS_00016;
        private static string cacheautoLOC_TACLS_00017;
        private static string cacheautoLOC_TACLS_00018;
        private static string cacheautoLOC_TACLS_00019;
        private static string cacheautoLOC_TACLS_00020;
        private static string cacheautoLOC_TACLS_00021;
        private static string cacheautoLOC_TACLS_00022;
        private static string cacheautoLOC_TACLS_00023;
        private static string cacheautoLOC_TACLS_00024;
        private static string cacheautoLOC_TACLS_00025;
        private void cacheLocalStrings()
        {
            cacheautoLOC_TACLS_00014 = Localizer.Format("#autoLOC_TACLS_00014"); // cacheautoLOC_TACLS_00014 = Current crew
            cacheautoLOC_TACLS_00015 = Localizer.Format("#autoLOC_TACLS_00015"); // cacheautoLOC_TACLS_00015 = Maximum crew
            cacheautoLOC_TACLS_00016 = Localizer.Format("#autoLOC_TACLS_00016"); // cacheautoLOC_TACLS_00016 = Food
            cacheautoLOC_TACLS_00017 = Localizer.Format("#autoLOC_TACLS_00017"); // cacheautoLOC_TACLS_00017 = Water
            cacheautoLOC_TACLS_00018 = Localizer.Format("#autoLOC_TACLS_00018"); // cacheautoLOC_TACLS_00018 = Oxygen
            cacheautoLOC_TACLS_00019 = Localizer.Format("#autoLOC_TACLS_00019"); // cacheautoLOC_TACLS_00019 = Electricity
            cacheautoLOC_TACLS_00020 = Localizer.Format("#autoLOC_TACLS_00020"); // cacheautoLOC_TACLS_00020 = Waste
            cacheautoLOC_TACLS_00021 = Localizer.Format("#autoLOC_TACLS_00021"); // cacheautoLOC_TACLS_00021 = Waste Water
            cacheautoLOC_TACLS_00022 = Localizer.Format("#autoLOC_TACLS_00022"); // cacheautoLOC_TACLS_00022 = Carbon Dioxide
            cacheautoLOC_TACLS_00023 = Localizer.Format("#autoLOC_TACLS_00023"); // cacheautoLOC_TACLS_00023 = Amount
            cacheautoLOC_TACLS_00024 = Localizer.Format("#autoLOC_TACLS_00024"); // cacheautoLOC_TACLS_00024 = With Current Crew
            cacheautoLOC_TACLS_00025 = Localizer.Format("#autoLOC_TACLS_00025"); // cacheautoLOC_TACLS_00025 = With Max Crew
        }

        #endregion
        public BuildAidWindow(AppLauncherToolBar TACMenuAppLToolBar, GlobalSettings globalSettings)
            : base(TACMenuAppLToolBar, Localizer.Format("#autoLOC_TACLS_00013"), 300, 180) //#autoLOC_TACLS_00013 = Life Support Build Aid
        {
            this.globalSettings = globalSettings;
            cacheLocalStrings();
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
            GUILayout.Label(cacheautoLOC_TACLS_00014, labelStyle); // cacheautoLOC_TACLS_00014 = Current Crew
            GUILayout.Label(cacheautoLOC_TACLS_00015, labelStyle); // cacheautoLOC_TACLS_00015 = Maximum Crew
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
            GUILayout.Label(cacheautoLOC_TACLS_00016, labelStyle); // cacheautoLOC_TACLS_00016 = Food
            GUILayout.Label(cacheautoLOC_TACLS_00017, labelStyle); // cacheautoLOC_TACLS_00017 = Water
            GUILayout.Label(cacheautoLOC_TACLS_00018, labelStyle); // cacheautoLOC_TACLS_00018 = Oxygen
            GUILayout.Label(cacheautoLOC_TACLS_00019, labelStyle); // cacheautoLOC_TACLS_00019 = Electricity
            GUILayout.Space(10f);
            GUILayout.Label(cacheautoLOC_TACLS_00020, labelStyle); // cacheautoLOC_TACLS_00020 = Waste
            GUILayout.Label(cacheautoLOC_TACLS_00021, labelStyle); // cacheautoLOC_TACLS_00021 = Waste Water
            GUILayout.Label(cacheautoLOC_TACLS_00022, labelStyle); // cacheautoLOC_TACLS_00022 = Carbon Dioxide
            GUILayout.EndVertical();

            GUILayout.BeginVertical();
            GUILayout.Label(cacheautoLOC_TACLS_00023, valueStyle); // cacheautoLOC_TACLS_00023 = Amount
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
            GUILayout.Label(cacheautoLOC_TACLS_00024, valueStyle); // cacheautoLOC_TACLS_00024 = With Current Crew
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
            GUILayout.Label(cacheautoLOC_TACLS_00025, valueStyle); // cacheautoLOC_TACLS_00025 = With Max Crew
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

                for (int i = 0; i < EditorLogic.fetch.ship.parts.Count; i++)
                {                 
                    if (EditorLogic.fetch.ship.parts[i].CrewCapacity > 0)
                    {
                        ++numOccupiableParts;
                        maxCrew += EditorLogic.fetch.ship.parts[i].CrewCapacity;
                    }

                    for (int j = 0; j < EditorLogic.fetch.ship.parts[i].Resources.Count; j++)
                    { 
                        if (EditorLogic.fetch.ship.parts[i].Resources[j].info.id == globalSettings.FoodId)
                        {
                            foodValue += EditorLogic.fetch.ship.parts[i].Resources[j].amount;
                        }
                        else if (EditorLogic.fetch.ship.parts[i].Resources[j].info.id == globalSettings.WaterId)
                        {
                            waterValue += EditorLogic.fetch.ship.parts[i].Resources[j].amount;
                        }
                        else if (EditorLogic.fetch.ship.parts[i].Resources[j].info.id == globalSettings.OxygenId)
                        {
                            oxygenValue += EditorLogic.fetch.ship.parts[i].Resources[j].amount;
                        }
                        else if (EditorLogic.fetch.ship.parts[i].Resources[j].info.id == globalSettings.ElectricityId)
                        {
                            electricityValue += EditorLogic.fetch.ship.parts[i].Resources[j].amount;
                        }
                        else if (EditorLogic.fetch.ship.parts[i].Resources[j].info.id == globalSettings.WasteId)
                        {
                            wasteValue += EditorLogic.fetch.ship.parts[i].Resources[j].maxAmount;
                        }
                        else if (EditorLogic.fetch.ship.parts[i].Resources[j].info.id == globalSettings.WasteWaterId)
                        {
                            wasteWaterValue += EditorLogic.fetch.ship.parts[i].Resources[j].maxAmount;
                        }
                        else if (EditorLogic.fetch.ship.parts[i].Resources[j].info.id == globalSettings.CO2Id)
                        {
                            carbonDioxideValue += EditorLogic.fetch.ship.parts[i].Resources[j].maxAmount;
                        }
                    }
                }

                CrewAssignmentDialog dialog = CrewAssignmentDialog.Instance;
                if (dialog != null)
                {
                    VesselCrewManifest manifest = dialog.GetManifest();
                    if (manifest != null)
                    {
                        List<PartCrewManifest> manifests = manifest.GetCrewableParts();
                        for (int i = 0; i < manifests.Count; i++)
                        {
                            var partCrew = manifests[i].GetPartCrew();
                            int partCrewCount = 0;
                            for (int j = 0; j < partCrew.Length; ++j)
                            {
                                if (partCrew[j] != null)
                                    ++partCrewCount;
                            }
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
                    foodDuration = Utilities.FormatTime(foodValue / HighLogic.CurrentGame.Parameters.CustomParams<TAC_SettingsParms_Sec2>().FoodConsumptionRate / numCrew);
                    waterDuration = Utilities.FormatTime(waterValue / HighLogic.CurrentGame.Parameters.CustomParams<TAC_SettingsParms_Sec2>().WaterConsumptionRate / numCrew);
                    oxygenDuration = Utilities.FormatTime(oxygenValue / HighLogic.CurrentGame.Parameters.CustomParams<TAC_SettingsParms_Sec2>().OxygenConsumptionRate / numCrew);
                    electricityDuration = Utilities.FormatTime(electricityValue / CalculateElectricityConsumptionRate(numCrew, numOccupiedParts));
                    wasteRoom = Utilities.FormatTime(wasteValue / HighLogic.CurrentGame.Parameters.CustomParams<TAC_SettingsParms_Sec2>().WasteProductionRate / numCrew);
                    wasteWaterRoom = Utilities.FormatTime(wasteWaterValue / HighLogic.CurrentGame.Parameters.CustomParams<TAC_SettingsParms_Sec2>().WasteWaterProductionRate / numCrew);
                    carbonDioxideRoom = Utilities.FormatTime(carbonDioxideValue / HighLogic.CurrentGame.Parameters.CustomParams<TAC_SettingsParms_Sec2>().CO2ProductionRate / numCrew);
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
                    foodDurationMaxCrew = Utilities.FormatTime(foodValue / HighLogic.CurrentGame.Parameters.CustomParams<TAC_SettingsParms_Sec2>().FoodConsumptionRate / maxCrew);
                    waterDurationMaxCrew = Utilities.FormatTime(waterValue / HighLogic.CurrentGame.Parameters.CustomParams<TAC_SettingsParms_Sec2>().WaterConsumptionRate / maxCrew);
                    oxygenDurationMaxCrew = Utilities.FormatTime(oxygenValue / HighLogic.CurrentGame.Parameters.CustomParams<TAC_SettingsParms_Sec2>().OxygenConsumptionRate / maxCrew);
                    electricityDurationMaxCrew = Utilities.FormatTime(electricityValue / CalculateElectricityConsumptionRate(maxCrew, numOccupiableParts));
                    wasteRoomMaxCrew = Utilities.FormatTime(wasteValue / HighLogic.CurrentGame.Parameters.CustomParams<TAC_SettingsParms_Sec2>().WasteProductionRate / maxCrew);
                    wasteWaterRoomMaxCrew = Utilities.FormatTime(wasteWaterValue / HighLogic.CurrentGame.Parameters.CustomParams<TAC_SettingsParms_Sec2>().WasteWaterProductionRate / maxCrew);
                    carbonDioxideRoomMaxCrew = Utilities.FormatTime(carbonDioxideValue / HighLogic.CurrentGame.Parameters.CustomParams<TAC_SettingsParms_Sec2>().CO2ProductionRate / maxCrew);
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
            return (HighLogic.CurrentGame.Parameters.CustomParams<TAC_SettingsParms_Sec2>().ElectricityConsumptionRate * numCrew) + 
                (HighLogic.CurrentGame.Parameters.CustomParams<TAC_SettingsParms_Sec2>().BaseElectricityConsumptionRate * numParts);
        }
    }
}
