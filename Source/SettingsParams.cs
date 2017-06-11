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

        [GameParameters.CustomStringParameterUI("Test String UI", lines = 4, title = "", toolTip = "#autoLOC_TACLS_00052")] //#autoLOC_TACLS_00052 = You cannot change TAC LS settings in Flight!
        public string CBstring = "#autoLOC_TACLS_00052"; //#autoLOC_TACLS_00052 = You cannot change TAC LS settings in Flight!

        [GameParameters.CustomParameterUI("#autoLOC_TACLS_00053", autoPersistance = true, toolTip = "#autoLOC_TACLS_00054")] //#autoLOC_TACLS_00053 - Enabled  //#autoLOC_TACLS_00054 = If on, TAC/LS is enabled in this save,\nIf off, it's not enabled in this save.
        public bool enabled = true;

        [GameParameters.CustomParameterUI("#autoLOC_TACLS_00253", autoPersistance = true, toolTip = "#autoLOC_TACLS_00254")] //#autoLOC_TACLS_00253 = Unloaded Vessel Processing  //#autoLOC_TACLS_00254 = If enabled TAC LS will process resources on unloaded vessels. If disabled, it won't and play the catchup and estimation game.
        public bool backgroundresources = true;

        [GameParameters.CustomParameterUI("#autoLOC_TACLS_00055", toolTip = "#autoLOC_TACLS_00056")] //#autoLOC_TACLS_00055 = Use Stock App Launcher Icon //# autoLOC_TACLS_00056 = If on, the Stock Application launcher will be used,\nif off will use Blizzy Toolbar if installed.
        public bool UseAppLToolbar = true;

        [GameParameters.CustomParameterUI("#autoLOC_TACLS_00057", autoPersistance = true, toolTip = "#autoLOC_TACLS_00058")] //#autoLOC_TACLS_00057 = Editor Filter //# autoLOC_TACLS_00058 = Turn the TAC/LS Editor filter Category on and off.
        public bool EditorFilter = true;

        [GameParameters.CustomParameterUI("#autoLOC_TACLS_00059", toolTip = "#autoLOC_TACLS_00060")] //#autoLOC_TACLS_00059 = When resources run out, kerbals.. //# autoLOC_TACLS_00060 = When LS resources run out,\nyou can select that kerbals die or hibernate.
        public string hibernate = Localizer.Format("#autoLOC_TACLS_00065"); //#autoLOC_TACLS_00065 = Die

        [GameParameters.CustomIntParameterUI("#autoLOC_TACLS_00061", minValue = 300, maxValue = 100000, stepSize = 300, autoPersistance = true, toolTip = "#autoLOC_TACLS_00062")] //#autoLOC_TACLS_00061 = Respawn delay (seconds) //# autoLOC_TACLS_00062 = The time in seconds a kerbal is comatose\n if fatal EC / Heat option is off
        public int respawnDelay = 300;

        [GameParameters.CustomIntParameterUI("#autoLOC_TACLS_00063", minValue = 1, maxValue = 30, stepSize = 1, autoPersistance = true, toolTip = "#autoLOC_TACLS_00064")] //#autoLOC_TACLS_00063 = Vessel List Update delay (Minutes) //# autoLOC_TACLS_00064 = The Minutes between List Sorting for the Vessel List.\nThe list is sorted from least resources to most and this is costly performance-wise.
        public int vesselUpdateList = 5;

        public override IList ValidValues(MemberInfo member)
        {
            if (member.Name == "hibernate")
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
                if (enabled && hibernate != "Die")
                    return true;
                return false;
            }
            if (enabled)
                return true;
            return false;
        }
        public override void OnLoad(ConfigNode node)
        {
            hibernate = Localizer.Format("#autoLOC_TACLS_00066"); //#autoLOC_TACLS_00066 = Hibernate
            if (node.HasValue("hibernate"))
            {
                string value = node.GetValue("hibernate");
                if (value == "Die")
                {
                    hibernate = Localizer.Format("#autoLOC_TACLS_00065"); //#autoLOC_TACLS_00065 = Die
                }                
            }
        }
        public override void OnSave(ConfigNode node)
        {
            if (hibernate == Localizer.Format("#autoLOC_TACLS_00065")) //#autoLOC_TACLS_00065 = Die
            {
                node.SetValue("hibernate", "Die");
            }
            else
            {
                node.SetValue("hibernate", "Hibernate");
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
        private bool FirstRun = true;
        [GameParameters.CustomStringParameterUI("Test String UI", lines = 4, title = "", toolTip = "#autoLOC_TACLS_00069")] //#autoLOC_TACLS_00069 = You have been warned!
        public string CBstring = "#autoLOC_TACLS_00070"; //#autoLOC_TACLS_00070 = Changing these values in an active game will impact Active Kerbals. It is recommended you restart KSP. Even then Kerbals on EVA will not be updated.

        [GameParameters.CustomFloatParameterUI("#autoLOC_TACLS_00071", //#autoLOC_TACLS_00071 = Food Consumption Rate p/d
            toolTip = "#autoLOC_TACLS_00072", minValue = 0.01f, maxValue = 6f, displayFormat = "F6", stepCount = 400)] //#autoLOC_TACLS_00072 = Amt of food consumed per Kerbal (units per Day).
        public float FoodConsumptionRate = 0.365625f;

        [GameParameters.CustomFloatParameterUI("#autoLOC_TACLS_00073", //#autoLOC_TACLS_00073 = Water Consumption Rate p/d
            toolTip = "#autoLOC_TACLS_00074", minValue = 0.01f, maxValue = 4f, displayFormat = "F6", stepCount = 400)] //#autoLOC_TACLS_00074 = Amt of water consumed per Kerbal (units per Day).
        public float WaterConsumptionRate = 0.2416625f;

        [GameParameters.CustomFloatParameterUI("#autoLOC_TACLS_00075", //#autoLOC_TACLS_00075 = Oxygen Consumption Rate p/d
            toolTip = "#autoLOC_TACLS_00076", minValue = 10f, maxValue = 600f, displayFormat = "F5", stepCount = 400)] //#autoLOC_TACLS_00076 = Amt of oxygen consumed per Kerbal (units per Day).
        public float OxygenConsumptionRate = 37.01241f;

        [GameParameters.CustomFloatParameterUI("#autoLOC_TACLS_00077", //#autoLOC_TACLS_00077 = Base Electricity Rate p/d
            toolTip = "#autoLOC_TACLS_00078", minValue = 300f, maxValue = 1500f, displayFormat = "F1", stepCount = 1000)] //#autoLOC_TACLS_00078 = Base Electricity Consumption Rate (units per Day).
        public float BaseElectricityConsumptionRate = 459f;

        [GameParameters.CustomFloatParameterUI("#autoLOC_TACLS_00079", //#autoLOC_TACLS_00079 = Kerbal Electricity Rate p/d
            toolTip = "#autoLOC_TACLS_00080", minValue = 100f, maxValue = 700f, displayFormat = "F1", stepCount = 1000)] //#autoLOC_TACLS_00080 = Per Kerbal Electricity Consumption Rate (units per Day).
        public float ElectricityConsumptionRate = 306f;

        [GameParameters.CustomFloatParameterUI("#autoLOC_TACLS_00081", //#autoLOC_TACLS_00081 = EVA Electricity Rate p/m
            toolTip = "#autoLOC_TACLS_00082", minValue = 0.01f, maxValue = 150f, displayFormat = "F3", stepCount = 400)]
        public float EvaElectricityConsumptionRate = 0.255f;

        [GameParameters.CustomFloatParameterUI("#autoLOC_TACLS_00083", //#autoLOC_TACLS_00083 = EVA Lamp Electricity Rate p/m
            toolTip = "#autoLOC_TACLS_00084", minValue = 0.00f, maxValue = 50f, displayFormat = "F3", stepCount = 400)] //#autoLOC_TACLS_00082 = EVA Electricity Consumption Rate (units per Minute).
        public float EvaLampElectricityConsumptionRate = 0.1278f;

        [GameParameters.CustomFloatParameterUI("#autoLOC_TACLS_00085", //#autoLOC_TACLS_00085 = CO2 Production Rate p/d
            toolTip = "#autoLOC_TACLS_00086", minValue = 0.1f, maxValue = 600f, displayFormat = "F5", stepCount = 600)] //#autoLOC_TACLS_00084 = EVA Lamp Electricity Consumption Rate (units per Minute).
        public float CO2ProductionRate = 31.97978f;

        [GameParameters.CustomFloatParameterUI("#autoLOC_TACLS_00087", //#autoLOC_TACLS_00087 = Waste Production Rate p/d
            toolTip = "#autoLOC_TACLS_00088", minValue = 0.01f, maxValue = 2f, displayFormat = "F5", stepCount = 400)] //#autoLOC_TACLS_00086 = Per Kerbal CarbonDioxide Production Rate (units per Day).
        public float WasteProductionRate = 0.03325f;

        [GameParameters.CustomFloatParameterUI("#autoLOC_TACLS_00089", //#autoLOC_TACLS_00089 = Waste Water Production Rate p/d
            toolTip = "#autoLOC_TACLS_00090", minValue = 0.1f, maxValue = 5f, displayFormat = "F5", stepCount = 400)] //#autoLOC_TACLS_00088 = Per Kerbal Waste Production Rate (units per Day).
        public float WasteWaterProductionRate = 0.30775f;

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
                    TacStartOnce.Instance.LoadGlobalSettings();
                    this.OnLoad(node);
                    break;
                case GameParameters.Preset.Normal:
                    TacStartOnce.Instance.LoadGlobalSettings();
                    this.OnLoad(node);
                    break;
                case GameParameters.Preset.Moderate:
                    TacStartOnce.Instance.LoadGlobalSettings();
                    this.OnLoad(node);
                    break;
                case GameParameters.Preset.Hard:
                    TacStartOnce.Instance.LoadGlobalSettings();
                    this.OnLoad(node);
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

        public override void OnLoad(ConfigNode node)
        {
            if (HighLogic.LoadedScene == GameScenes.MAINMENU)
            {
                if (TacStartOnce.Instance.globalSettings == null)
                {
                    TacStartOnce.Instance.Awake();
                }
                var hoursDay = GameSettings.KERBIN_TIME ? 6 : 24;
                FoodConsumptionRate = (float) (TacStartOnce.Instance.globalSettings.FoodConsumptionRate*60*60*hoursDay);
                WaterConsumptionRate = (float) (TacStartOnce.Instance.globalSettings.WaterConsumptionRate*60*60*hoursDay);
                OxygenConsumptionRate = (float) (TacStartOnce.Instance.globalSettings.OxygenConsumptionRate*60*60*hoursDay);
                BaseElectricityConsumptionRate =
                    (float) (TacStartOnce.Instance.globalSettings.BaseElectricityConsumptionRate*60*60*hoursDay);
                ElectricityConsumptionRate =
                    (float) (TacStartOnce.Instance.globalSettings.ElectricityConsumptionRate*60*60*hoursDay);
                EvaElectricityConsumptionRate =
                    (float) (TacStartOnce.Instance.globalSettings.EvaElectricityConsumptionRate*60);
                EvaLampElectricityConsumptionRate =
                    (float)(TacStartOnce.Instance.globalSettings.EvaLampElectricityConsumptionRate * 60);
                CO2ProductionRate = (float) (TacStartOnce.Instance.globalSettings.CO2ProductionRate*60*60*hoursDay);
                WasteProductionRate = (float) (TacStartOnce.Instance.globalSettings.WasteProductionRate*60*60*hoursDay);
                
                WasteWaterProductionRate = (float) (TacStartOnce.Instance.globalSettings.WasteWaterProductionRate*60*60*hoursDay);
            }
        }

        public override void OnSave(ConfigNode node)
        {
            if (FirstRun)
            {
                FirstRun = false;
                return;
            }
            else
            {
                var hoursDay = GameSettings.KERBIN_TIME ? 6 : 24;
                TacStartOnce.Instance.globalSettings.FoodConsumptionRate = FoodConsumptionRate/60/60/hoursDay;
                TacStartOnce.Instance.globalSettings.WaterConsumptionRate = WaterConsumptionRate/60/60/hoursDay;
                TacStartOnce.Instance.globalSettings.OxygenConsumptionRate = OxygenConsumptionRate/60/60/hoursDay;
                TacStartOnce.Instance.globalSettings.BaseElectricityConsumptionRate = BaseElectricityConsumptionRate/60/60/hoursDay;
                TacStartOnce.Instance.globalSettings.ElectricityConsumptionRate = ElectricityConsumptionRate/60/60/hoursDay;
                TacStartOnce.Instance.globalSettings.EvaElectricityConsumptionRate = EvaElectricityConsumptionRate/60;
                TacStartOnce.Instance.globalSettings.EvaLampElectricityConsumptionRate = EvaLampElectricityConsumptionRate / 60;
                TacStartOnce.Instance.globalSettings.CO2ProductionRate = CO2ProductionRate/60/60/hoursDay;
                TacStartOnce.Instance.globalSettings.WasteProductionRate = WasteProductionRate/60/60/hoursDay;
                TacStartOnce.Instance.globalSettings.WasteWaterProductionRate = WasteWaterProductionRate/60/60/hoursDay;
                
                //Change EVA resource values
                AddLifeSupport als = new AddLifeSupport();
                als.ChangeValues();
            }
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
        private bool FirstRun = true;
        [GameParameters.CustomStringParameterUI("Test String UI", lines = 4, title = "", toolTip = "#autoLOC_TACLS_00069")] //#autoLOC_TACLS_00069 = You have been warned!
        public string CBstring = "#autoLOC_TACLS_00070"; //Changing these values in an active game will impact Active Kerbals. It is recommended you restart KSP. Even then Kerbals on EVA will not be updated.

        [GameParameters.CustomIntParameterUI("#autoLOC_TACLS_00092", toolTip = "#autoLOC_TACLS_00093", minValue = 3000, maxValue = 200000)] //#autoLOC_TACLS_00092 = Max Delta Time //# autoLOC_TACLS_00093 = This is the maximum time multiplier used between resource calculations.
        public int MaxDeltaTime = 86400;

        [GameParameters.CustomIntParameterUI("#autoLOC_TACLS_00094", toolTip = "#autoLOC_TACLS_00095", minValue = 1, maxValue = 5)] //#autoLOC_TACLS_00094 = Max Delta Time (Electricity) //# autoLOC_TACLS_00095 = This is the maximum time multiplier used between Electricity calculations.
        public int ElectricityMaxDeltaTime = 1;

        [GameParameters.CustomIntParameterUI("#autoLOC_TACLS_00096", toolTip = "#autoLOC_TACLS_00097", minValue = 72, maxValue = 900)] //#autoLOC_TACLS_00096 = Max time without Food (hrs) //# autoLOC_TACLS_00097 = The maximum amount of time a Kerbal can go without food (in hours).
        public int MaxTimeWithoutFood = 360;

        [GameParameters.CustomIntParameterUI("#autoLOC_TACLS_00098", toolTip = "#autoLOC_TACLS_00099", minValue = 24, maxValue = 200)] //#autoLOC_TACLS_00098 = Max time without Water (hrs) //# autoLOC_TACLS_00099 = The maximum amount of time a Kerbal can go without water (in hours).
        public int MaxTimeWithoutWater = 36;

        [GameParameters.CustomIntParameterUI("#autoLOC_TACLS_00100", toolTip = "#autoLOC_TACLS_00101", minValue = 1, maxValue = 200)] //#autoLOC_TACLS_00100 = Max time without Oxygen (min) //# autoLOC_TACLS_00101 = The maximum amount of time a Kerbal can go without oxygen (in minutes).
        public int MaxTimeWithoutOxygen = 120;

        [GameParameters.CustomIntParameterUI("#autoLOC_TACLS_00102", toolTip = "#autoLOC_TACLS_00103", minValue = 2, maxValue = 200)] //#autoLOC_TACLS_00102 = Max time without Electricity (min) //# autoLOC_TACLS_00103 = The maximum amount of time a Kerbal can go without electricity (in minutes).
        public int MaxTimeWithoutElectricity = 120;

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
                    TacStartOnce.Instance.LoadGlobalSettings();
                    this.OnLoad(node);
                    break;
                case GameParameters.Preset.Normal:
                    TacStartOnce.Instance.LoadGlobalSettings();
                    this.OnLoad(node);
                    break;
                case GameParameters.Preset.Moderate:
                    TacStartOnce.Instance.LoadGlobalSettings();
                    this.OnLoad(node);
                    break;
                case GameParameters.Preset.Hard:
                    TacStartOnce.Instance.LoadGlobalSettings();
                    this.OnLoad(node);
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

        public override void OnLoad(ConfigNode node)
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
                MaxTimeWithoutFood = (int) TacStartOnce.Instance.globalSettings.MaxTimeWithoutFood/60/60;
                MaxTimeWithoutOxygen = (int) TacStartOnce.Instance.globalSettings.MaxTimeWithoutOxygen/60;
                MaxTimeWithoutWater = (int) TacStartOnce.Instance.globalSettings.MaxTimeWithoutWater/60/60;
                MaxTimeWithoutElectricity = (int) TacStartOnce.Instance.globalSettings.MaxTimeWithoutElectricity/60;
                EvaDefaultResourceAmount = (int) TacStartOnce.Instance.globalSettings.EvaDefaultResourceAmount;
            }
        }

        public override void OnSave(ConfigNode node)
        {
            if (FirstRun)
            {
                FirstRun = false;
                return;
            }
            else
            {
                //Convert Hourly amounts from UI back to per second.
                TacStartOnce.Instance.globalSettings.MaxDeltaTime = MaxDeltaTime;
                TacStartOnce.Instance.globalSettings.ElectricityMaxDeltaTime = ElectricityMaxDeltaTime;
                TacStartOnce.Instance.globalSettings.MaxTimeWithoutFood = MaxTimeWithoutFood*60*60;
                TacStartOnce.Instance.globalSettings.MaxTimeWithoutOxygen = MaxTimeWithoutOxygen*60;
                TacStartOnce.Instance.globalSettings.MaxTimeWithoutWater = MaxTimeWithoutWater*60*60;
                TacStartOnce.Instance.globalSettings.MaxTimeWithoutElectricity = MaxTimeWithoutElectricity*60;
                double TOLERANCE = 1;
                if (Math.Abs(TacStartOnce.Instance.globalSettings.EvaDefaultResourceAmount - EvaDefaultResourceAmount) > TOLERANCE)
                {
                    TacStartOnce.Instance.globalSettings.EvaDefaultResourceAmount = EvaDefaultResourceAmount;
                    //Change EVA resource values
                    AddLifeSupport als = new AddLifeSupport();
                    als.ChangeValues();
                }

            }
        }
    }
}
