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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using KSP.Localization;

namespace Tac
{
    public class TAC_SettingsParms : GameParameters.CustomParameterNode

    {
        public override string Title { get { return Localizer.Format("#autoLOC_TACLS_00067"); } }
        public override GameParameters.GameMode GameMode { get { return GameParameters.GameMode.ANY; } }
        public override bool HasPresets { get { return false; } }
        public override string Section { get { return "TAC Life Support"; } }
        public override string DisplaySection { get { return Localizer.Format("#autoLOC_TACLS_00037"); } }
        public override int SectionOrder { get { return 1; } }

        [GameParameters.CustomStringParameterUI("Test String UI", autoPersistance = false, lines = 4, title = "", toolTip = "#autoLOC_TACLS_00052")] //#autoLOC_TACLS_00052 = You cannot change TAC LS settings in Flight!
        public string CBstring = "#autoLOC_TACLS_00052"; //#autoLOC_TACLS_00052 = You cannot change TAC LS settings in Flight!

        [GameParameters.CustomParameterUI("#autoLOC_TACLS_00053", autoPersistance = true, toolTip = "#autoLOC_TACLS_00054")] //#autoLOC_TACLS_00053 - Enabled  //#autoLOC_TACLS_00054 = If on, TAC/LS is enabled in this save,\nIf off, it's not enabled in this save.
        public bool enabled = true;

        [GameParameters.CustomParameterUI("#autoLOC_TACLS_00253", autoPersistance = true, toolTip = "#autoLOC_TACLS_00254")] //#autoLOC_TACLS_00253 = Unloaded Vessel Processing  //#autoLOC_TACLS_00254 = If enabled TAC LS will process resources on unloaded vessels. If disabled, it won't and play the catchup and estimation game.
        public bool backgroundresources = true;

        [GameParameters.CustomParameterUI("#autoLOC_TACLS_00055", autoPersistance = true, toolTip = "#autoLOC_TACLS_00056")] //#autoLOC_TACLS_00055 = Use Stock App Launcher Icon //# autoLOC_TACLS_00056 = If on, the Stock Application launcher will be used,\nif off will use Blizzy Toolbar if installed.
        public bool UseAppLToolbar = true;

        [GameParameters.CustomParameterUI("#autoLOC_TACLS_00057", autoPersistance = true, toolTip = "#autoLOC_TACLS_00058")] //#autoLOC_TACLS_00057 = Editor Filter //# autoLOC_TACLS_00058 = Turn the TAC/LS Editor filter Category on and off.
        public bool EditorFilter = true;

        [GameParameters.CustomParameterUI("#autoLOC_TACLS_00059", autoPersistance = false, toolTip = "#autoLOC_TACLS_00060")] //#autoLOC_TACLS_00059 = When resources run out, kerbals.. //# autoLOC_TACLS_00060 = When LS resources run out,\nyou can select that kerbals die or hibernate.
        public string displayhibernate = Localizer.Format("#autoLOC_TACLS_00065"); //#autoLOC_TACLS_00065 = Die
        public string hibernate = "";
        [GameParameters.CustomIntParameterUI("#autoLOC_TACLS_00061", minValue = 300, maxValue = 100000, stepSize = 300, autoPersistance = true, toolTip = "#autoLOC_TACLS_00062")] //#autoLOC_TACLS_00061 = Respawn delay (seconds) //# autoLOC_TACLS_00062 = The time in seconds a kerbal is comatose\n if fatal EC / Heat option is off
        public int respawnDelay = 300;

        [GameParameters.CustomIntParameterUI("#autoLOC_TACLS_00063", minValue = 1, maxValue = 30, stepSize = 1, autoPersistance = true, toolTip = "#autoLOC_TACLS_00064")] //#autoLOC_TACLS_00063 = Vessel List Update delay (Minutes) //# autoLOC_TACLS_00064 = The Minutes between List Sorting for the Vessel List.\nThe list is sorted from least resources to most and this is costly performance-wise.
        public int vesselUpdateList = 5;

        public override IList ValidValues(MemberInfo member)
        {
            if (member.Name == "displayhibernate")
            {
                List<string> myList = new List<string>();
                myList.Add(Localizer.Format("#autoLOC_TACLS_00065")); //#autoLOC_TACLS_00065 = Die
                myList.Add(Localizer.Format("#autoLOC_TACLS_00066")); //#autoLOC_TACLS_00066 = Hibernate
                IList myIlist = myList;
                return myIlist;
            }
            else
            {
                return null;
            }
        }

        public override bool Enabled(MemberInfo member, GameParameters parameters)
        {
            if (HighLogic.fetch != null)
            {
                if (member.Name == "CBstring")
                {
                    if (HighLogic.LoadedScene == GameScenes.FLIGHT)
                        return true;
                    else
                        return false;
                }

                if (member.Name == "backgroundresources")
                {
                    if (RSTUtils.Utilities.IsModInstalled("BackgroundResources"))
                        return true;
                    else
                        return false;
                }

                if (HighLogic.LoadedScene == GameScenes.MAINMENU || HighLogic.LoadedScene == GameScenes.SETTINGS || HighLogic.LoadedScene == GameScenes.SPACECENTER)
                {
                    return true;
                }
            }
            return false;
        }

        public override bool Interactible(MemberInfo member, GameParameters parameters)
        {
            if (member.Name == "enabled")
            {
                if (HighLogic.LoadedScene == GameScenes.MAINMENU || HighLogic.LoadedScene == GameScenes.SETTINGS ||
                    HighLogic.LoadedScene == GameScenes.SPACECENTER)
                    return true;
            }
            if (member.Name == "respawnDelay")
            {
                UpdateHibernateField();
                if (enabled && hibernate != "Die")
                    return true;
                return false;
            }
            if (member.Name == "UseAppLToolbar")
            {
                if (RSTUtils.ToolbarManager.ToolbarAvailable)
                    return true;
                return false;
            }
            if (enabled)
                return true;
            return false;
        }
        public override void OnLoad(ConfigNode node)
        {            
            if (node.HasValue("hibernate"))
            {
                hibernate = node.GetValue("hibernate");
                if (hibernate == "Hibernate" || hibernate == Localizer.Format("#autoLOC_TACLS_00066"))
                {
                    displayhibernate = Localizer.Format("#autoLOC_TACLS_00066"); //#autoLOC_TACLS_00066 = Hibernate
                }
                else
                {
                    displayhibernate = Localizer.Format("#autoLOC_TACLS_00065"); //#autoLOC_TACLS_00065 = Die
                }
            }
        }
        public override void OnSave(ConfigNode node)
        {
            UpdateHibernateField();
            node.SetValue("hibernate", hibernate);            
        }

        private void UpdateHibernateField()
        {
            if (displayhibernate == Localizer.Format("#autoLOC_TACLS_00065")) //#autoLOC_TACLS_00065 = Die
            {
                hibernate = "Die";
            }
            else
            {
                hibernate = "Hibernate";                
            }
        }
    }

    public class TAC_SettingsParms_Sec2 : GameParameters.CustomParameterNode
    {
        public override string Title { get { return Localizer.Format("#autoLOC_TACLS_00068"); } } //#autoLOC_TACLS_00068 = TAC LS Consumption/Production
        public override GameParameters.GameMode GameMode { get { return GameParameters.GameMode.ANY; }}
        public override bool HasPresets { get { return true; }}
        public override string Section { get { return "TAC Life Support"; }}
        public override string DisplaySection { get { return Localizer.Format("#autoLOC_TACLS_00037"); }}
        public override int SectionOrder { get { return 2; }}
        public bool upgrade135 = false;

        [GameParameters.CustomStringParameterUI("Test String UI", autoPersistance = false, lines = 4, title = "", toolTip = "#autoLOC_TACLS_00069")] //#autoLOC_TACLS_00069 = You have been warned!
        public string CBstring = "#autoLOC_TACLS_00070"; //#autoLOC_TACLS_00070 = Changing these values in an active game will impact Active Kerbals. It is recommended you restart KSP. Even then Kerbals on EVA will not be updated.

        [GameParameters.CustomFloatParameterUI("#autoLOC_TACLS_00071", //#autoLOC_TACLS_00071 = Food Consumption Rate p/d
            toolTip = "#autoLOC_TACLS_00072", minValue = 0.01f, maxValue = 6f, displayFormat = "F6", stepCount = 400)] //#autoLOC_TACLS_00072 = Amt of food consumed per Kerbal (units per Day).
        public float displayFoodConsumptionRate = 0.365625f;
        public double FoodConsumptionRate = 0.000016927083333f;

        [GameParameters.CustomFloatParameterUI("#autoLOC_TACLS_00073", //#autoLOC_TACLS_00073 = Water Consumption Rate p/d
            toolTip = "#autoLOC_TACLS_00074", minValue = 0.01f, maxValue = 4f, displayFormat = "F6", stepCount = 400)] //#autoLOC_TACLS_00074 = Amt of water consumed per Kerbal (units per Day).
        public float displayWaterConsumptionRate = 0.2416625f;
        public double WaterConsumptionRate = 0.000011188078704f;

        [GameParameters.CustomFloatParameterUI("#autoLOC_TACLS_00075", //#autoLOC_TACLS_00075 = Oxygen Consumption Rate p/d
            toolTip = "#autoLOC_TACLS_00076", minValue = 10f, maxValue = 600f, displayFormat = "F5", stepCount = 400)] //#autoLOC_TACLS_00076 = Amt of oxygen consumed per Kerbal (units per Day).
        public float displayOxygenConsumptionRate = 37.01241f;
        public double OxygenConsumptionRate = 0.001713537562385f;

        [GameParameters.CustomFloatParameterUI("#autoLOC_TACLS_00077", //#autoLOC_TACLS_00077 = Base Electricity Rate p/d
            toolTip = "#autoLOC_TACLS_00078", minValue = 300f, maxValue = 1500f, displayFormat = "F1", stepCount = 1000)] //#autoLOC_TACLS_00078 = Base Electricity Consumption Rate (units per Day).
        public float displayBaseElectricityConsumptionRate = 459f;
        public double BaseElectricityConsumptionRate = 0.02125f;

        [GameParameters.CustomFloatParameterUI("#autoLOC_TACLS_00079", //#autoLOC_TACLS_00079 = Kerbal Electricity Rate p/d
            toolTip = "#autoLOC_TACLS_00080", minValue = 100f, maxValue = 700f, displayFormat = "F1", stepCount = 1000)] //#autoLOC_TACLS_00080 = Per Kerbal Electricity Consumption Rate (units per Day).
        public float displayElectricityConsumptionRate = 306f;
        public double ElectricityConsumptionRate = 0.014166666666667f;

        [GameParameters.CustomFloatParameterUI("#autoLOC_TACLS_00081", //#autoLOC_TACLS_00081 = EVA Electricity Rate p/m
            toolTip = "#autoLOC_TACLS_00082", minValue = 0.01f, maxValue = 150f, displayFormat = "F3", stepCount = 400)]
        public float displayEvaElectricityConsumptionRate = 0.255f;
        public double EvaElectricityConsumptionRate = 0.00425f;

        [GameParameters.CustomFloatParameterUI("#autoLOC_TACLS_00083", //#autoLOC_TACLS_00083 = EVA Lamp Electricity Rate p/m
            toolTip = "#autoLOC_TACLS_00084", minValue = 0.00f, maxValue = 50f, displayFormat = "F3", stepCount = 400)] //#autoLOC_TACLS_00082 = EVA Electricity Consumption Rate (units per Minute).
        public float displayEvaLampElectricityConsumptionRate = 0.1278f;
        public double EvaLampElectricityConsumptionRate = 0.00213f;

        [GameParameters.CustomFloatParameterUI("#autoLOC_TACLS_00085", //#autoLOC_TACLS_00085 = CO2 Production Rate p/d
            toolTip = "#autoLOC_TACLS_00086", minValue = 0.1f, maxValue = 600f, displayFormat = "F5", stepCount = 600)] //#autoLOC_TACLS_00084 = EVA Lamp Electricity Consumption Rate (units per Minute).
        public float displayCO2ProductionRate = 31.97978f;
        public double CO2ProductionRate = 0.00148012889876f;

        [GameParameters.CustomFloatParameterUI("#autoLOC_TACLS_00087", //#autoLOC_TACLS_00087 = Waste Production Rate p/d
            toolTip = "#autoLOC_TACLS_00088", minValue = 0.01f, maxValue = 2f, displayFormat = "F5", stepCount = 400)] //#autoLOC_TACLS_00086 = Per Kerbal CarbonDioxide Production Rate (units per Day).
        public float displayWasteProductionRate = 0.03325f;
        public double WasteProductionRate = 0.000001539351852f;

        [GameParameters.CustomFloatParameterUI("#autoLOC_TACLS_00089", //#autoLOC_TACLS_00089 = Waste Water Production Rate p/d
            toolTip = "#autoLOC_TACLS_00090", minValue = 0.1f, maxValue = 5f, displayFormat = "F5", stepCount = 400)] //#autoLOC_TACLS_00088 = Per Kerbal Waste Production Rate (units per Day).
        public float displayWasteWaterProductionRate = 0.30775f;
        public double WasteWaterProductionRate = 0.000014247685185f;

        /// <summary>
        /// If the user selects of the default difficulty preset buttons.
        /// TAC LS does not change the settings for different difficulty presets.
        /// So when the user clicks one it will reload the global settings effectively setting them back to the global settings, 
        /// unless they select custom in which case it leaves the settings as-is.
        /// </summary>
        /// <param name="preset"></param>
        public override void SetDifficultyPreset(GameParameters.Preset preset)
        {
            Debug.Log("Setting difficulty preset");            
            switch (preset)
            {
                case GameParameters.Preset.Easy:                    
                    SetDefaults(preset);
                    break;
                case GameParameters.Preset.Normal:
                    SetDefaults(preset);
                    break;
                case GameParameters.Preset.Moderate:
                    SetDefaults(preset);
                    break;
                case GameParameters.Preset.Hard:
                    SetDefaults(preset);
                    break;
                case GameParameters.Preset.Custom:                    
                    break;
            }
        }

        public override bool Enabled(MemberInfo member, GameParameters parameters)
        {
            if (HighLogic.fetch != null)
            {
                if (member.Name == "CBstring")
                {
                    if (HighLogic.LoadedScene == GameScenes.MAINMENU || HighLogic.LoadedScene == GameScenes.SETTINGS)
                    {
                        return false;
                    }
                }
                if (HighLogic.LoadedScene == GameScenes.MAINMENU || HighLogic.LoadedScene == GameScenes.SETTINGS ||
                    HighLogic.LoadedScene == GameScenes.SPACECENTER)
                {
                    return true;
                }
            }
            return false;
        }

        public override bool Interactible(MemberInfo member, GameParameters parameters)
        {
            if (parameters.CustomParams<TAC_SettingsParms>().enabled)
                return true;
            return false;
        }

        public void SetDefaults(GameParameters.Preset preset)
        {            
            if (HighLogic.LoadedScene == GameScenes.MAINMENU)
            {
                if (TacStartOnce.Instance.globalSettings == null)
                {
                    TacStartOnce.Instance.Awake();
                }
                float hoursDay = GameSettings.KERBIN_TIME ? 6f : 24f;
                FoodConsumptionRate = (TacStartOnce.Instance.globalSettings.FoodConsumptionRate);
                displayFoodConsumptionRate = (float) (TacStartOnce.Instance.globalSettings.FoodConsumptionRate*60f*60f*hoursDay);
                WaterConsumptionRate = (TacStartOnce.Instance.globalSettings.WaterConsumptionRate);
                displayWaterConsumptionRate = (float) (TacStartOnce.Instance.globalSettings.WaterConsumptionRate*60f*60f*hoursDay);
                OxygenConsumptionRate = (TacStartOnce.Instance.globalSettings.OxygenConsumptionRate);
                displayOxygenConsumptionRate = (float) (TacStartOnce.Instance.globalSettings.OxygenConsumptionRate*60f*60f*hoursDay);
                BaseElectricityConsumptionRate = (TacStartOnce.Instance.globalSettings.BaseElectricityConsumptionRate);
                displayBaseElectricityConsumptionRate = (float) (TacStartOnce.Instance.globalSettings.BaseElectricityConsumptionRate*60f*60f*hoursDay);
                ElectricityConsumptionRate = (TacStartOnce.Instance.globalSettings.ElectricityConsumptionRate);
                displayElectricityConsumptionRate = (float) (TacStartOnce.Instance.globalSettings.ElectricityConsumptionRate*60f*60f*hoursDay);
                EvaElectricityConsumptionRate = (TacStartOnce.Instance.globalSettings.EvaElectricityConsumptionRate);
                displayEvaElectricityConsumptionRate = (float) (TacStartOnce.Instance.globalSettings.EvaElectricityConsumptionRate*60f);
                EvaLampElectricityConsumptionRate = (TacStartOnce.Instance.globalSettings.EvaLampElectricityConsumptionRate);
                displayEvaLampElectricityConsumptionRate = (float)(TacStartOnce.Instance.globalSettings.EvaLampElectricityConsumptionRate * 60f);
                CO2ProductionRate = (TacStartOnce.Instance.globalSettings.CO2ProductionRate);
                displayCO2ProductionRate = (float) (TacStartOnce.Instance.globalSettings.CO2ProductionRate*60f*60f*hoursDay);
                WasteProductionRate = (TacStartOnce.Instance.globalSettings.WasteProductionRate);
                displayWasteProductionRate = (float) (TacStartOnce.Instance.globalSettings.WasteProductionRate*60f*60f*hoursDay);
                WasteWaterProductionRate = (TacStartOnce.Instance.globalSettings.WasteWaterProductionRate);
                displayWasteWaterProductionRate = (float) (TacStartOnce.Instance.globalSettings.WasteWaterProductionRate*60f*60f*hoursDay);                
            }
        }

        public override void OnLoad(ConfigNode node)
        {
            //Set the display values from the saved settings
            float hoursDay = GameSettings.KERBIN_TIME ? 6f : 24f;
            displayFoodConsumptionRate = (float)(FoodConsumptionRate * 60f * 60f * hoursDay);
            displayWaterConsumptionRate = (float)WaterConsumptionRate * 60f * 60f * hoursDay;
            displayOxygenConsumptionRate = (float)OxygenConsumptionRate * 60f * 60f * hoursDay;
            displayBaseElectricityConsumptionRate = (float)BaseElectricityConsumptionRate * 60f * 60f * hoursDay;
            displayElectricityConsumptionRate = (float)ElectricityConsumptionRate * 60f * 60f * hoursDay;
            displayEvaElectricityConsumptionRate = (float)EvaElectricityConsumptionRate * 60f;
            displayEvaLampElectricityConsumptionRate = (float)EvaLampElectricityConsumptionRate * 60f;
            displayCO2ProductionRate = (float)CO2ProductionRate * 60f * 60f * hoursDay;
            displayWasteProductionRate = (float)WasteProductionRate * 60f * 60f * hoursDay;
            displayWasteWaterProductionRate = (float)WasteWaterProductionRate * 60f * 60f * hoursDay;
        }

        public override void OnSave(ConfigNode node)
        {
            if (upgrade135)
            {
                //Save the actual per second values that TAC LS uses.
                node.SetValue("FoodConsumptionRate", FoodConsumptionRate);
                node.SetValue("WaterConsumptionRate", WaterConsumptionRate);
                node.SetValue("OxygenConsumptionRate", OxygenConsumptionRate);
                node.SetValue("BaseElectricityConsumptionRate", BaseElectricityConsumptionRate);
                node.SetValue("ElectricityConsumptionRate", ElectricityConsumptionRate);
                node.SetValue("EvaElectricityConsumptionRate", EvaElectricityConsumptionRate);
                node.SetValue("EvaLampElectricityConsumptionRate", EvaLampElectricityConsumptionRate);
                node.SetValue("CO2ProductionRate", CO2ProductionRate);
                node.SetValue("WasteProductionRate", WasteProductionRate);
                node.SetValue("WasteWaterProductionRate", WasteWaterProductionRate);
                OnLoad(node);
                node.SetValue("displayFoodConsumptionRate", displayFoodConsumptionRate);
                node.SetValue("displayWaterConsumptionRate", displayWaterConsumptionRate);
                node.SetValue("displayOxygenConsumptionRate", displayOxygenConsumptionRate);
                node.SetValue("displayBaseElectricityConsumptionRate", displayBaseElectricityConsumptionRate);
                node.SetValue("displayElectricityConsumptionRate", displayElectricityConsumptionRate);
                node.SetValue("displayEvaElectricityConsumptionRate", displayEvaElectricityConsumptionRate);
                node.SetValue("displayEvaLampElectricityConsumptionRate", displayEvaLampElectricityConsumptionRate);
                node.SetValue("displayCO2ProductionRate", displayCO2ProductionRate);
                node.SetValue("displayWasteProductionRate", displayWasteProductionRate);
                node.SetValue("displayWasteWaterProductionRate", displayWasteWaterProductionRate);
                upgrade135 = false;
                node.SetValue("upgrade135", false);
                return;
            }
            if (HighLogic.LoadedScene != GameScenes.MAINMENU)
            {   //Change EVA resource values
                AddLifeSupport als = new AddLifeSupport();
                als.ChangeValues();
            }
            //Set the actual values from the display values at per second values.
            float hoursDay = GameSettings.KERBIN_TIME ? 6f : 24f;
            FoodConsumptionRate = displayFoodConsumptionRate / 60f / 60f / hoursDay;            
            WaterConsumptionRate = displayWaterConsumptionRate / 60f / 60f / hoursDay;            
            OxygenConsumptionRate = displayOxygenConsumptionRate / 60f / 60f / hoursDay;            
            BaseElectricityConsumptionRate = displayBaseElectricityConsumptionRate / 60f / 60f / hoursDay;            
            ElectricityConsumptionRate = displayElectricityConsumptionRate / 60f / 60f / hoursDay;            
            EvaElectricityConsumptionRate = displayEvaElectricityConsumptionRate / 60f;           
            EvaLampElectricityConsumptionRate = displayEvaLampElectricityConsumptionRate / 60f;           
            CO2ProductionRate = displayCO2ProductionRate / 60f / 60f / hoursDay;           
            WasteProductionRate = displayWasteProductionRate / 60f / 60f / hoursDay;            
            WasteWaterProductionRate = displayWasteWaterProductionRate / 60 / 60 / hoursDay;
            //Save the actual per second values that TAC LS uses.
            node.SetValue("FoodConsumptionRate", FoodConsumptionRate);
            node.SetValue("WaterConsumptionRate", WaterConsumptionRate);
            node.SetValue("OxygenConsumptionRate", OxygenConsumptionRate);            
            node.SetValue("BaseElectricityConsumptionRate", BaseElectricityConsumptionRate);            
            node.SetValue("ElectricityConsumptionRate", ElectricityConsumptionRate);            
            node.SetValue("EvaElectricityConsumptionRate", EvaElectricityConsumptionRate);            
            node.SetValue("EvaLampElectricityConsumptionRate", EvaLampElectricityConsumptionRate);            
            node.SetValue("CO2ProductionRate", CO2ProductionRate);
            node.SetValue("WasteProductionRate", WasteProductionRate);            
            node.SetValue("WasteWaterProductionRate", WasteWaterProductionRate);            
        }
    }

    public class TAC_SettingsParms_Sec3 : GameParameters.CustomParameterNode

    {
        public override string Title { get { return Localizer.Format("#autoLOC_TACLS_00091"); } } //autoLOC_TACLS_00091 = TAC LS Limits
        public override GameParameters.GameMode GameMode { get { return GameParameters.GameMode.ANY; } }
        public override bool HasPresets { get { return true; } }
        public override string Section { get { return "TAC Life Support"; } }
        public override string DisplaySection { get { return Localizer.Format("#autoLOC_TACLS_00037"); } }
        public override int SectionOrder { get { return 3; } }
        public bool upgrade135 = false;
        
        [GameParameters.CustomStringParameterUI("Test String UI", autoPersistance = false, lines = 4, title = "", toolTip = "#autoLOC_TACLS_00069")] //#autoLOC_TACLS_00069 = You have been warned!
        public string CBstring = "#autoLOC_TACLS_00070"; //Changing these values in an active game will impact Active Kerbals. It is recommended you restart KSP. Even then Kerbals on EVA will not be updated.

        [GameParameters.CustomIntParameterUI("#autoLOC_TACLS_00092", toolTip = "#autoLOC_TACLS_00093", minValue = 3000, maxValue = 200000)] //#autoLOC_TACLS_00092 = Max Delta Time //# autoLOC_TACLS_00093 = This is the maximum time multiplier used between resource calculations.
        public int MaxDeltaTime = 86400;

        [GameParameters.CustomIntParameterUI("#autoLOC_TACLS_00094", toolTip = "#autoLOC_TACLS_00095", minValue = 1, maxValue = 5)] //#autoLOC_TACLS_00094 = Max Delta Time (Electricity) //# autoLOC_TACLS_00095 = This is the maximum time multiplier used between Electricity calculations.
        public int ElectricityMaxDeltaTime = 1;

        [GameParameters.CustomIntParameterUI("#autoLOC_TACLS_00096", toolTip = "#autoLOC_TACLS_00097", minValue = 72, maxValue = 900)] //#autoLOC_TACLS_00096 = Max time without Food (hrs) //# autoLOC_TACLS_00097 = The maximum amount of time a Kerbal can go without food (in hours).
        public int displayMaxTimeWithoutFood = 360;
        public double MaxTimeWithoutFood = 1296000;

        [GameParameters.CustomIntParameterUI("#autoLOC_TACLS_00098", toolTip = "#autoLOC_TACLS_00099", minValue = 24, maxValue = 200)] //#autoLOC_TACLS_00098 = Max time without Water (hrs) //# autoLOC_TACLS_00099 = The maximum amount of time a Kerbal can go without water (in hours).
        public int displayMaxTimeWithoutWater = 36;
        public double MaxTimeWithoutWater = 129600;

        [GameParameters.CustomIntParameterUI("#autoLOC_TACLS_00100", toolTip = "#autoLOC_TACLS_00101", minValue = 1, maxValue = 200)] //#autoLOC_TACLS_00100 = Max time without Oxygen (min) //# autoLOC_TACLS_00101 = The maximum amount of time a Kerbal can go without oxygen (in minutes).
        public int displayMaxTimeWithoutOxygen = 120;
        public double MaxTimeWithoutOxygen = 7200;

        [GameParameters.CustomIntParameterUI("#autoLOC_TACLS_00102", toolTip = "#autoLOC_TACLS_00103", minValue = 2, maxValue = 200)] //#autoLOC_TACLS_00102 = Max time without Electricity (min) //# autoLOC_TACLS_00103 = The maximum amount of time a Kerbal can go without electricity (in minutes).
        public int displayMaxTimeWithoutElectricity = 120;
        public double MaxTimeWithoutElectricity = 7200;

        [GameParameters.CustomIntParameterUI("#autoLOC_TACLS_00104", toolTip = "#autoLOC_TACLS_00105", minValue = 0, maxValue = 90000)] //#autoLOC_TACLS_00104 = Default Units amount for EVA suits //# autoLOC_TACLS_00105 = The amount of each resource EVA suits will take with them (in units).
        public int EvaDefaultResourceAmount = 21600;
        
        /// <summary>
        /// If the user selects of the default difficulty preset buttons.
        /// TAC LS does not change the settings for different difficulty presets.
        /// So when the user clicks one it will reload the global settings effectively setting them back to the global settings, 
        /// unless they select custom in which case it leaves the settings as-is.
        /// </summary>
        /// <param name="preset"></param>
        public override void SetDifficultyPreset(GameParameters.Preset preset)
        {
            Debug.Log("Setting difficulty preset");
            ConfigNode node = new ConfigNode();
            switch (preset)
            {
                case GameParameters.Preset.Easy:
                    SetDefaults(preset);
                    break;
                case GameParameters.Preset.Normal:
                    SetDefaults(preset);
                    break;
                case GameParameters.Preset.Moderate:
                    SetDefaults(preset);
                    break;
                case GameParameters.Preset.Hard:
                    SetDefaults(preset);
                    break;
                case GameParameters.Preset.Custom:                    
                    break;
            }
        }

        public override bool Enabled(MemberInfo member, GameParameters parameters)
        {
            if (HighLogic.fetch != null)
            {
                if (member.Name == "CBstring")
                {
                    if (HighLogic.LoadedScene == GameScenes.MAINMENU || HighLogic.LoadedScene == GameScenes.SETTINGS)
                    {
                        return false;
                    }
                }
                if (HighLogic.LoadedScene == GameScenes.MAINMENU || HighLogic.LoadedScene == GameScenes.SETTINGS || HighLogic.LoadedScene == GameScenes.SPACECENTER)
                {
                    return true;
                }
            }
            return false;
        }

        public override bool Interactible(MemberInfo member, GameParameters parameters)
        {
            if (parameters.CustomParams<TAC_SettingsParms>().enabled)
                return true;
            return false;
        }

        public void SetDefaults(GameParameters.Preset preset)
        {
            if (HighLogic.LoadedScene == GameScenes.MAINMENU)
            {
                if (TacStartOnce.Instance.globalSettings == null)
                {
                    TacStartOnce.Instance.Awake();
                }
                //set values, converted from per second to per Day for UI.
                MaxDeltaTime = TacStartOnce.Instance.globalSettings.MaxDeltaTime;
                ElectricityMaxDeltaTime = TacStartOnce.Instance.globalSettings.ElectricityMaxDeltaTime;
                displayMaxTimeWithoutFood = (int) (TacStartOnce.Instance.globalSettings.MaxTimeWithoutFood / 60f / 60f);
                MaxTimeWithoutFood = TacStartOnce.Instance.globalSettings.MaxTimeWithoutFood; 
                displayMaxTimeWithoutOxygen = (int) (TacStartOnce.Instance.globalSettings.MaxTimeWithoutOxygen / 60f);
                MaxTimeWithoutOxygen = TacStartOnce.Instance.globalSettings.MaxTimeWithoutOxygen;
                displayMaxTimeWithoutWater = (int) (TacStartOnce.Instance.globalSettings.MaxTimeWithoutWater / 60f / 60f);
                MaxTimeWithoutWater = TacStartOnce.Instance.globalSettings.MaxTimeWithoutWater;
                displayMaxTimeWithoutElectricity = (int) (TacStartOnce.Instance.globalSettings.MaxTimeWithoutElectricity / 60f);
                MaxTimeWithoutElectricity = TacStartOnce.Instance.globalSettings.MaxTimeWithoutElectricity;
                EvaDefaultResourceAmount = (int) TacStartOnce.Instance.globalSettings.EvaDefaultResourceAmount;
            }
        }

        public override void OnLoad(ConfigNode node)
        {
            displayMaxTimeWithoutFood = (int)(MaxTimeWithoutFood / 60f / 60f);
            displayMaxTimeWithoutOxygen = (int)(MaxTimeWithoutOxygen / 60f);
            displayMaxTimeWithoutWater = (int)(MaxTimeWithoutWater / 60f / 60f);
            displayMaxTimeWithoutElectricity = (int)(MaxTimeWithoutElectricity / 60f);            
        }

        public override void OnSave(ConfigNode node)
        {
            if (upgrade135)
            {
                //Set the actual values from the display values at per second values.                
                node.SetValue("MaxTimeWithoutFood", MaxTimeWithoutFood);                
                node.SetValue("MaxTimeWithoutOxygen", MaxTimeWithoutOxygen);                
                node.SetValue("MaxTimeWithoutWater", MaxTimeWithoutWater);                
                node.SetValue("MaxTimeWithoutElectricity", MaxTimeWithoutElectricity);
                OnLoad(node);
                node.SetValue("displayMaxTimeWithoutFood", displayMaxTimeWithoutFood);
                node.SetValue("displayMaxTimeWithoutOxygen", displayMaxTimeWithoutOxygen);
                node.SetValue("displayMaxTimeWithoutWater", displayMaxTimeWithoutWater);
                node.SetValue("displayMaxTimeWithoutElectricity", displayMaxTimeWithoutElectricity);
                upgrade135 = false;
                node.SetValue("upgrade135", false);
                return;
            }
            //Set the actual values from the display values at per second values.
            MaxTimeWithoutFood = (double)displayMaxTimeWithoutFood * 60f * 60f;
            node.SetValue("MaxTimeWithoutFood", MaxTimeWithoutFood);
            MaxTimeWithoutOxygen = (double)displayMaxTimeWithoutOxygen * 60f;
            node.SetValue("MaxTimeWithoutOxygen", MaxTimeWithoutOxygen);
            MaxTimeWithoutWater = (double)displayMaxTimeWithoutWater * 60f * 60f;
            node.SetValue("MaxTimeWithoutWater", MaxTimeWithoutWater);
            MaxTimeWithoutElectricity = (double)displayMaxTimeWithoutElectricity * 60f;
            node.SetValue("MaxTimeWithoutElectricity", MaxTimeWithoutElectricity);
        }
    }
}
