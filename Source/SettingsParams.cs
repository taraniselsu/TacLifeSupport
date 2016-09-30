using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Tac
{
    public class TAC_SettingsParms : GameParameters.CustomParameterNode

    {
        public override string Title { get { return "TAC LS Options"; } }
        public override GameParameters.GameMode GameMode { get { return GameParameters.GameMode.ANY; } }
        public override bool HasPresets { get { return true; } }
        public override string Section { get { return "TAC Life Support"; } }
        public override int SectionOrder { get { return 1; } }

        [GameParameters.CustomParameterUI("Enabled", autoPersistance = true, toolTip = "If on, TAC/LS is enabled in this save,\nIf off, it's not enabled in this save.")]
        public bool enabled = true;

        [GameParameters.CustomParameterUI("Use Stock App Launcher Icon", toolTip = "If on, the Stock Application launcher will be used,\nif off will use Blizzy Toolbar if installed.")]
        public bool UseAppLToolbar = true;

        [GameParameters.CustomParameterUI("Editor Filter", autoPersistance = true, toolTip = "Turn the TAC/LS Editor filter Category on and off.")]
        public bool EditorFilter = true;

        [GameParameters.CustomParameterUI("When resources run out, kerbals..", toolTip = "When LS resources run out,\nyou can select that kerbals die or hibernate.")]
        public string hibernate = "Die";

        [GameParameters.CustomIntParameterUI("Respawn delay (seconds)", minValue = 300, maxValue = 100000, stepSize = 300, autoPersistance = true, toolTip = "The time in seconds a kerbal is comatose\n if fatal EC / Heat option is off")]
        public int respawnDelay = 300;
        

        public override void SetDifficultyPreset(GameParameters.Preset preset)
        {
            Debug.Log("Setting difficulty preset");
            switch (preset)
            {
                case GameParameters.Preset.Easy:
                    
                    break;
                case GameParameters.Preset.Normal:
                    
                    break;
                case GameParameters.Preset.Moderate:
                    
                    break;
                case GameParameters.Preset.Hard:
                    
                    break;
                case GameParameters.Preset.Custom:
                    break;
            }
        }

        public override IList ValidValues(MemberInfo member)
        {
            if (member.Name == "hibernate")
            {
                List<string> myList = new List<string>();
                myList.Add("Die");
                myList.Add("Hibernate");
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
                if (HighLogic.LoadedScene == GameScenes.MAINMENU || HighLogic.LoadedScene == GameScenes.SETTINGS || HighLogic.LoadedScene == GameScenes.SPACECENTER)
                {
                    //if (member.Name != "TempinKelvin" && member.Name != "StripLightsActive" && member.Name != "ToolTips" &&
                    //    member.Name != "UseAppLToolbar" && member.Name != "DebugLogging")
                        return true;
                }
            }
            return false;
        }

        public override bool Interactible(MemberInfo member, GameParameters parameters)
        {
            if (member.Name == "enabled")
                return true;
            if (member.Name == "respawnDelay")
            {
                if (enabled && hibernate != "Die")
                    return true;
                else
                    return false;
            }
            if (enabled)
                return true;
            return false;
        }
    }

    public class TAC_SettingsParms_Sec2 : GameParameters.CustomParameterNode

    {
        public override string Title
        {
            get { return "TAC LS Consumption/Production"; }
        }

        public override GameParameters.GameMode GameMode
        {
            get { return GameParameters.GameMode.ANY; }
        }

        public override bool HasPresets
        {
            get { return true; }
        }

        public override string Section
        {
            get { return "TAC Life Support"; }
        }

        public override int SectionOrder
        {
            get { return 2; }
        }

        [GameParameters.CustomFloatParameterUI("Food Consumption Rate",
            toolTip = "Amt of food consumed per Kerbal (units per second).", minValue = 0.00000001f, maxValue = 0.001f, displayFormat = "F11", stepCount = 10000)]
        public float FoodConsumptionRate = 0.000016927083333f;

        [GameParameters.CustomFloatParameterUI("Water Consumption Rate",
            toolTip = "Amt of water consumed per Kerbal (units per second).", minValue = 0.000001f, maxValue = 0.001f, displayFormat = "F11", stepCount = 10000)]
        public float WaterConsumptionRate = 0.000011188078704f;

        [GameParameters.CustomFloatParameterUI("Oxygen Consumption Rate",
            toolTip = "Amt of oxygen consumed per Kerbal (units per second).", minValue = 0.0001f, maxValue = 0.100f, displayFormat = "F11", stepCount = 10000)]
        public float OxygenConsumptionRate = 0.001713537562385f;

        [GameParameters.CustomFloatParameterUI("Base Electricity Rate",
            toolTip = "Base Electricity Consumption Rate (units per second).", minValue = 0.0001f, maxValue = 0.100f, displayFormat = "F11", stepCount = 10000)]
        public float BaseElectricityConsumptionRate = 0.02125f;

        [GameParameters.CustomFloatParameterUI("Kerbal Electricity Rate",
            toolTip = "Per Kerbal Electricity Consumption Rate (units per second).", minValue = 0.0001f, maxValue = 0.100f, displayFormat = "F11", stepCount = 10000)]
        public float ElectricityConsumptionRate = 0.014166666666667f;

        [GameParameters.CustomFloatParameterUI("EVA Electricity Rate",
            toolTip = "EVA Electricity Consumption Rate (units per second).", minValue = 0.0001f, maxValue = 0.100f, displayFormat = "F11", stepCount = 10000)]
        public float EvaElectricityConsumptionRate = 0.00425f;

        [GameParameters.CustomFloatParameterUI("CarbonDioxide Production Rate",
            toolTip = "Per Kerbal CarbonDioxide Production Rate (units per second).", minValue = 0.0001f, maxValue = 0.100f, displayFormat = "F11", stepCount = 10000)]
        public float CO2ProductionRate = 0.00148012889876f;

        [GameParameters.CustomFloatParameterUI("Waste Production Rate",
            toolTip = "Per Kerbal Waste Production Rate (units per second).", minValue = 0.00000001f, maxValue = 0.001f, displayFormat = "F11", stepCount = 10000)]
        public float WasteProductionRate = 0.000001539351852f;

        [GameParameters.CustomFloatParameterUI("Waste Water Production Rate",
            toolTip = "Per Kerbal Waste Water Production Rate (units per second).", minValue = 0.00000001f, maxValue = 0.001f, displayFormat = "F11", stepCount = 10000)]
        public float WasteWaterProductionRate = 0.000014247685185f;

        public override void SetDifficultyPreset(GameParameters.Preset preset)
        {
            Debug.Log("Setting difficulty preset");
            switch (preset)
            {
                case GameParameters.Preset.Easy:

                    break;
                case GameParameters.Preset.Normal:

                    break;
                case GameParameters.Preset.Moderate:

                    break;
                case GameParameters.Preset.Hard:

                    break;
                case GameParameters.Preset.Custom:
                    break;
            }
        }

        public override bool Enabled(MemberInfo member, GameParameters parameters)
        {
            if (HighLogic.fetch != null)
            {
                if (HighLogic.LoadedScene == GameScenes.MAINMENU || HighLogic.LoadedScene == GameScenes.SETTINGS ||
                    HighLogic.LoadedScene == GameScenes.SPACECENTER)
                {
                    //if (member.Name != "TempinKelvin" && member.Name != "StripLightsActive" && member.Name != "ToolTips" &&
                    //    member.Name != "UseAppLToolbar" && member.Name != "DebugLogging")
                    return true;
                }
            }
            return false;
        }

        public override bool Interactible(MemberInfo member, GameParameters parameters)
        {
            if (parameters.CustomParams<TAC_SettingsParms>().enabled)
                return true;
            else
                return false;
        }

        public override void OnLoad(ConfigNode node)
        {
            if (TacStartOnce.globalSettings == null)
            {
                TacStartOnce.Instance.Awake();
            }
            FoodConsumptionRate = (float)TacStartOnce.globalSettings.FoodConsumptionRate;
            WaterConsumptionRate = (float) TacStartOnce.globalSettings.WaterConsumptionRate;
            OxygenConsumptionRate = (float) TacStartOnce.globalSettings.OxygenConsumptionRate;
            BaseElectricityConsumptionRate = (float) TacStartOnce.globalSettings.BaseElectricityConsumptionRate;
            ElectricityConsumptionRate = (float) TacStartOnce.globalSettings.ElectricityConsumptionRate;
            EvaElectricityConsumptionRate = (float) TacStartOnce.globalSettings.EvaElectricityConsumptionRate;
            CO2ProductionRate = (float) TacStartOnce.globalSettings.CO2ProductionRate;
            WasteProductionRate = (float) TacStartOnce.globalSettings.WasteProductionRate;
            WasteWaterProductionRate = (float) TacStartOnce.globalSettings.WasteWaterProductionRate;
        }
    }

    public class TAC_SettingsParms_Sec3 : GameParameters.CustomParameterNode

    {
        public override string Title { get { return "TAC LS Limits"; } }
        public override GameParameters.GameMode GameMode { get { return GameParameters.GameMode.ANY; } }
        public override bool HasPresets { get { return true; } }
        public override string Section { get { return "TAC Life Support"; } }
        public override int SectionOrder { get { return 3; } }
        
        [GameParameters.CustomFloatParameterUI("Max Delta Time", toolTip = "This is the maximum time multiplier used between resource calculations.", minValue = 10000, maxValue = 300000, stepCount = 10000)]
        public float MaxDeltaTime = 86400f;

        [GameParameters.CustomFloatParameterUI("Max Delta Time (Electricity)", toolTip = "This is the maximum time multiplier used between Electricity calculations.", minValue = 1, maxValue = 5)]
        public float ElectricityMaxDeltaTime = 1f;

        [GameParameters.CustomFloatParameterUI("Max time without Food", toolTip = "The maximum amount of time a Kerbal can go without food (in secs).", minValue = 0, maxValue = 2000000, stepCount = 10000)]
        public float MaxTimeWithoutFood = 1296000f;

        [GameParameters.CustomFloatParameterUI("Max time without Water", toolTip = "The maximum amount of time a Kerbal can go without water (in secs).", minValue = 0, maxValue = 500000, stepCount = 10000)]
        public float MaxTimeWithoutWater = 129600f;

        [GameParameters.CustomFloatParameterUI("Max time without Oxygen", toolTip = "The maximum amount of time a Kerbal can go without oxygen (in secs).", minValue = 0, maxValue = 30000, stepCount = 1000)]
        public float MaxTimeWithoutOxygen = 7200f;

        [GameParameters.CustomFloatParameterUI("Max time without Electricity", toolTip = "The maximum amount of time a Kerbal can go without electricity (in secs).", minValue = 0, maxValue = 30000, stepCount = 1000)]
        public float MaxTimeWithoutElectricity = 7200f;

        [GameParameters.CustomFloatParameterUI("Default amount for EVA suits", toolTip = "The amount of each resource EVA suits will take with them (in units).", minValue = 0, maxValue = 100000, stepCount = 10000)]
        public float EvaDefaultResourceAmount = 21600f;

        

        public override void SetDifficultyPreset(GameParameters.Preset preset)
        {
            Debug.Log("Setting difficulty preset");
            switch (preset)
            {
                case GameParameters.Preset.Easy:

                    break;
                case GameParameters.Preset.Normal:

                    break;
                case GameParameters.Preset.Moderate:

                    break;
                case GameParameters.Preset.Hard:

                    break;
                case GameParameters.Preset.Custom:
                    break;
            }
        }
        
        public override bool Enabled(MemberInfo member, GameParameters parameters)
        {
            if (HighLogic.fetch != null)
            {
                if (HighLogic.LoadedScene == GameScenes.MAINMENU || HighLogic.LoadedScene == GameScenes.SETTINGS || HighLogic.LoadedScene == GameScenes.SPACECENTER)
                {
                    //if (member.Name != "TempinKelvin" && member.Name != "StripLightsActive" && member.Name != "ToolTips" &&
                    //    member.Name != "UseAppLToolbar" && member.Name != "DebugLogging")
                    return true;
                }
            }
            return false;
        }

        public override bool Interactible(MemberInfo member, GameParameters parameters)
        {
            if (parameters.CustomParams<TAC_SettingsParms>().enabled)
                return true;
            else
                return false;
        }

        public override void OnLoad(ConfigNode node)
        {
            if (TacStartOnce.globalSettings == null)
            {
                TacStartOnce.Instance.Awake();
            }

            MaxDeltaTime = (float)TacStartOnce.globalSettings.MaxDeltaTime;
            ElectricityMaxDeltaTime = (float) TacStartOnce.globalSettings.ElectricityMaxDeltaTime;
            MaxTimeWithoutFood = (float) TacStartOnce.globalSettings.MaxTimeWithoutFood;
            MaxTimeWithoutOxygen = (float) TacStartOnce.globalSettings.MaxTimeWithoutOxygen;
            MaxTimeWithoutWater = (float) TacStartOnce.globalSettings.MaxTimeWithoutWater;
            MaxTimeWithoutElectricity = (float) TacStartOnce.globalSettings.MaxTimeWithoutElectricity;
            EvaDefaultResourceAmount = (float) TacStartOnce.globalSettings.EvaDefaultResourceAmount;
        }
    }


}
