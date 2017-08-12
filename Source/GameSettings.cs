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
using System.Collections.Generic;
using System.Reflection;

namespace Tac
{
    public class TacGameSettings
    {
        private const string configNodeName = "SavedGameSettings";

        public bool IsNewSave;
        public const int lastCompatibleMajor = 0;
        public const int lastCompatibleMinor = 13;
        public const int lastCompatibleRev = 5;
        public bool compatible;

        public string file_full_version;
        public int file_version_major;
        public int file_version_minor;
        public int file_version_revision;  

        public DictionaryValueList<string, CrewMemberInfo> knownCrew { get; private set; }
        public DictionaryValueList<Guid, VesselInfo> knownVessels { get; private set; }
        
        public TacGameSettings()
        {
            IsNewSave = true;
            compatible = false;
            file_full_version = "";
            knownCrew = new DictionaryValueList<string, CrewMemberInfo>();
            knownVessels = new DictionaryValueList<Guid, VesselInfo>();
        }

        public void Load(ConfigNode node)
        {
            if (node.HasNode(configNodeName))
            {
                ConfigNode settingsNode = node.GetNode(configNodeName);
                settingsNode.TryGetValue("Version", ref file_full_version);
                settingsNode.TryGetValue("IsNewSave", ref IsNewSave);
                if (IsNewSave)
                {
                    GetNewFileVersion(); 
                    compatible = true;
                }
                else
                {
                    CheckFileVersion();
                }               
                
                knownVessels.Clear();
                var vesselNodes = settingsNode.GetNodes(VesselInfo.ConfigNodeName);
                foreach (ConfigNode vesselNode in vesselNodes)
                {
                    if (vesselNode.HasValue("Guid"))
                    {
                        Guid id = new Guid(vesselNode.GetValue("Guid"));
                        VesselInfo vesselInfo = VesselInfo.Load(vesselNode);
                        knownVessels[id] = vesselInfo;
                    }
                }

                knownCrew.Clear();
                var crewNodes = settingsNode.GetNodes(CrewMemberInfo.ConfigNodeName);
                foreach (ConfigNode crewNode in crewNodes)
                {
                    CrewMemberInfo crewMemberInfo = CrewMemberInfo.Load(crewNode);
                    knownCrew[crewMemberInfo.name] = crewMemberInfo;
                    if (knownVessels.Contains(crewMemberInfo.vesselId))
                    {
                        knownVessels[crewMemberInfo.vesselId].CrewInVessel.Add(crewMemberInfo);
                    }
                }
            }
            
            if (!compatible)
            {
                ProcessSaveUpgrade();
            }
        }

        private void CheckFileVersion()
        {
            compatible = false;

            if (!string.IsNullOrEmpty(file_full_version))            
            {
                string[] subversions = file_full_version.Split('.');
                if (subversions.Length == 3)
                {
                    file_version_major = int.Parse(subversions[0]);
                    file_version_minor = int.Parse(subversions[1]);
                    file_version_revision = int.Parse(subversions[2]);
                    if (file_version_major > lastCompatibleMajor) //If Major is higher must be ok.
                    {
                        compatible = true;       
                    }
                    else
                    {
                        if (file_version_major == lastCompatibleMajor) //If Major is equal, check next level.
                        {
                            if (file_version_minor > lastCompatibleMinor)  //If Minor is higher must be ok.
                            {
                                compatible = true;
                            }
                            else
                            {
                                if (file_version_minor == lastCompatibleMinor) //If Minor is equal, check next level.
                                {
                                    if (file_version_revision >= lastCompatibleRev)  //If Revision is greater or equal. Must be ok.
                                    {
                                        compatible = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void ProcessSaveUpgrade()
        {
            //If Null then the version is prior to 0.13.5, which means we have to adjust the settings.
            if (string.IsNullOrEmpty(file_full_version)) 
            {
                if (HighLogic.CurrentGame.Parameters.CustomParams<TAC_SettingsParms_Sec2>().FoodConsumptionRate > 0.1f)
                {
                    HighLogic.CurrentGame.Parameters.CustomParams<TAC_SettingsParms_Sec2>().upgrade135 = true;
                    
                    float hoursDay = GameSettings.KERBIN_TIME ? 6f : 24f;
                    HighLogic.CurrentGame.Parameters.CustomParams<TAC_SettingsParms_Sec2>().FoodConsumptionRate = HighLogic.CurrentGame.Parameters.CustomParams<TAC_SettingsParms_Sec2>().FoodConsumptionRate / 60f / 60f / hoursDay;
                    HighLogic.CurrentGame.Parameters.CustomParams<TAC_SettingsParms_Sec2>().WaterConsumptionRate = HighLogic.CurrentGame.Parameters.CustomParams<TAC_SettingsParms_Sec2>().WaterConsumptionRate / 60f / 60f / hoursDay;
                    HighLogic.CurrentGame.Parameters.CustomParams<TAC_SettingsParms_Sec2>().OxygenConsumptionRate = HighLogic.CurrentGame.Parameters.CustomParams<TAC_SettingsParms_Sec2>().OxygenConsumptionRate / 60f / 60f / hoursDay;
                    HighLogic.CurrentGame.Parameters.CustomParams<TAC_SettingsParms_Sec2>().BaseElectricityConsumptionRate = HighLogic.CurrentGame.Parameters.CustomParams<TAC_SettingsParms_Sec2>().BaseElectricityConsumptionRate / 60f / 60f / hoursDay;
                    HighLogic.CurrentGame.Parameters.CustomParams<TAC_SettingsParms_Sec2>().ElectricityConsumptionRate = HighLogic.CurrentGame.Parameters.CustomParams<TAC_SettingsParms_Sec2>().ElectricityConsumptionRate / 60f / 60f / hoursDay;
                    HighLogic.CurrentGame.Parameters.CustomParams<TAC_SettingsParms_Sec2>().EvaElectricityConsumptionRate = HighLogic.CurrentGame.Parameters.CustomParams<TAC_SettingsParms_Sec2>().EvaElectricityConsumptionRate / 60f;
                    HighLogic.CurrentGame.Parameters.CustomParams<TAC_SettingsParms_Sec2>().EvaLampElectricityConsumptionRate = HighLogic.CurrentGame.Parameters.CustomParams<TAC_SettingsParms_Sec2>().EvaLampElectricityConsumptionRate / 60f;
                    HighLogic.CurrentGame.Parameters.CustomParams<TAC_SettingsParms_Sec2>().CO2ProductionRate = HighLogic.CurrentGame.Parameters.CustomParams<TAC_SettingsParms_Sec2>().CO2ProductionRate / 60f / 60f / hoursDay;
                    HighLogic.CurrentGame.Parameters.CustomParams<TAC_SettingsParms_Sec2>().WasteProductionRate = HighLogic.CurrentGame.Parameters.CustomParams<TAC_SettingsParms_Sec2>().WasteProductionRate / 60f / 60f / hoursDay;
                    HighLogic.CurrentGame.Parameters.CustomParams<TAC_SettingsParms_Sec2>().WasteWaterProductionRate = HighLogic.CurrentGame.Parameters.CustomParams<TAC_SettingsParms_Sec2>().WasteWaterProductionRate / 60f / 60f / hoursDay;
                }
                if (HighLogic.CurrentGame.Parameters.CustomParams<TAC_SettingsParms_Sec3>().MaxTimeWithoutFood < 400f)
                {
                    HighLogic.CurrentGame.Parameters.CustomParams<TAC_SettingsParms_Sec3>().upgrade135 = true;
                    HighLogic.CurrentGame.Parameters.CustomParams<TAC_SettingsParms_Sec3>().MaxTimeWithoutFood = (HighLogic.CurrentGame.Parameters.CustomParams<TAC_SettingsParms_Sec3>().MaxTimeWithoutFood * 60f * 60f);
                    if (HighLogic.CurrentGame.Parameters.CustomParams<TAC_SettingsParms_Sec3>().MaxTimeWithoutFood == 0)
                    {
                        HighLogic.CurrentGame.Parameters.CustomParams<TAC_SettingsParms_Sec3>().MaxTimeWithoutFood = TacStartOnce.Instance.globalSettings.MaxTimeWithoutFood;
                    }                    
                    HighLogic.CurrentGame.Parameters.CustomParams<TAC_SettingsParms_Sec3>().MaxTimeWithoutOxygen = (HighLogic.CurrentGame.Parameters.CustomParams<TAC_SettingsParms_Sec3>().MaxTimeWithoutOxygen * 60f);
                    if (HighLogic.CurrentGame.Parameters.CustomParams<TAC_SettingsParms_Sec3>().MaxTimeWithoutOxygen == 0)
                    {
                        HighLogic.CurrentGame.Parameters.CustomParams<TAC_SettingsParms_Sec3>().MaxTimeWithoutOxygen = TacStartOnce.Instance.globalSettings.MaxTimeWithoutOxygen;
                    }
                    HighLogic.CurrentGame.Parameters.CustomParams<TAC_SettingsParms_Sec3>().MaxTimeWithoutWater = (HighLogic.CurrentGame.Parameters.CustomParams<TAC_SettingsParms_Sec3>().MaxTimeWithoutWater * 60f * 60f);
                    if (HighLogic.CurrentGame.Parameters.CustomParams<TAC_SettingsParms_Sec3>().MaxTimeWithoutWater == 0)
                    {
                        HighLogic.CurrentGame.Parameters.CustomParams<TAC_SettingsParms_Sec3>().MaxTimeWithoutWater = TacStartOnce.Instance.globalSettings.MaxTimeWithoutWater;
                    }
                    HighLogic.CurrentGame.Parameters.CustomParams<TAC_SettingsParms_Sec3>().MaxTimeWithoutElectricity = (HighLogic.CurrentGame.Parameters.CustomParams<TAC_SettingsParms_Sec3>().MaxTimeWithoutElectricity * 60f);
                    if (HighLogic.CurrentGame.Parameters.CustomParams<TAC_SettingsParms_Sec3>().MaxTimeWithoutElectricity == 0)
                    {
                        HighLogic.CurrentGame.Parameters.CustomParams<TAC_SettingsParms_Sec3>().MaxTimeWithoutElectricity = TacStartOnce.Instance.globalSettings.MaxTimeWithoutElectricity;
                    }

                }
            }
            GetNewFileVersion();
        }

        private void GetNewFileVersion()
        {
            Version executingVersion = Assembly.GetExecutingAssembly().GetName().Version;
            file_full_version = executingVersion.Major + "." + executingVersion.Minor + "." + executingVersion.Build;
            file_version_major = executingVersion.Major;
            file_version_minor = executingVersion.Minor;
            file_version_revision = executingVersion.Build;
        }

        public void Save(ConfigNode node)
        {
            ConfigNode settingsNode;
            if (node.HasNode(configNodeName))
            {
                settingsNode = node.GetNode(configNodeName);
            }
            else
            {
                settingsNode = node.AddNode(configNodeName);
            }

            settingsNode.AddValue("Version", file_full_version);
            settingsNode.AddValue("IsNewSave", IsNewSave);

            Dictionary<string, CrewMemberInfo>.Enumerator crewenumerator = knownCrew.GetDictEnumerator();
            while (crewenumerator.MoveNext())
            {
                crewenumerator.Current.Value.Save(settingsNode);
            }
            
            Dictionary<Guid, VesselInfo>.Enumerator vslenumerator = knownVessels.GetDictEnumerator();
            while (vslenumerator.MoveNext())
            {
                ConfigNode vesselNode = vslenumerator.Current.Value.Save(settingsNode);
                vesselNode.AddValue("Guid", vslenumerator.Current.Key);
            }            
        }
    }
}
