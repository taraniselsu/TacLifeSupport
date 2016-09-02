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

namespace Tac
{
    public class TacGameSettings
    {
        private const string configNodeName = "SavedGameSettings";

        public bool IsNewSave;
        public bool Enabled;
        public bool UseAppLauncher;
        public bool UseEditorFilter;
        public bool HibernateInsteadOfKill;
        public double RespawnDelay;

        public int MaxDeltaTime;
        public int ElectricityMaxDeltaTime;
        public double FoodConsumptionRate;
        public double WaterConsumptionRate;
        public double OxygenConsumptionRate;
        public double ElectricityConsumptionRate;
        public double BaseElectricityConsumptionRate;
        public double EvaElectricityConsumptionRate;
        public double CO2ProductionRate;
        public double WasteProductionRate;
        public double WasteWaterProductionRate;
        public double EvaDefaultResourceAmount;
        public double MaxTimeWithoutFood;
        public double MaxTimeWithoutWater;
        public double MaxTimeWithoutOxygen;
        public double MaxTimeWithoutElectricity; 

        public Dictionary<string, CrewMemberInfo> knownCrew { get; private set; }
        public Dictionary<Guid, VesselInfo> knownVessels { get; private set; }

        public TacGameSettings()
        {
            IsNewSave = true;
            Enabled = true;
            UseAppLauncher = true;
            UseEditorFilter = true;
            HibernateInsteadOfKill = false;
            RespawnDelay = 9203545.0; // 1 Kerbin year (the game's default is too short at only 36 minutes)

            MaxDeltaTime = TacLifeSupport.Instance.globalSettings.MaxDeltaTime;
            ElectricityMaxDeltaTime = TacLifeSupport.Instance.globalSettings.ElectricityMaxDeltaTime;
            FoodConsumptionRate = TacLifeSupport.Instance.globalSettings.FoodConsumptionRate;
            WaterConsumptionRate = TacLifeSupport.Instance.globalSettings.WaterConsumptionRate;
            OxygenConsumptionRate = TacLifeSupport.Instance.globalSettings.OxygenConsumptionRate;
            ElectricityConsumptionRate = TacLifeSupport.Instance.globalSettings.ElectricityConsumptionRate;
            BaseElectricityConsumptionRate = TacLifeSupport.Instance.globalSettings.BaseElectricityConsumptionRate;
            EvaElectricityConsumptionRate = TacLifeSupport.Instance.globalSettings.EvaElectricityConsumptionRate;
            CO2ProductionRate = TacLifeSupport.Instance.globalSettings.CO2ProductionRate;
            WasteProductionRate = TacLifeSupport.Instance.globalSettings.WasteProductionRate;
            WasteWaterProductionRate = TacLifeSupport.Instance.globalSettings.WasteWaterProductionRate;
            EvaDefaultResourceAmount = TacLifeSupport.Instance.globalSettings.EvaDefaultResourceAmount;
            MaxTimeWithoutFood = TacLifeSupport.Instance.globalSettings.MaxTimeWithoutFood;
            MaxTimeWithoutWater = TacLifeSupport.Instance.globalSettings.MaxTimeWithoutWater;
            MaxTimeWithoutOxygen = TacLifeSupport.Instance.globalSettings.MaxTimeWithoutOxygen;
            MaxTimeWithoutElectricity = TacLifeSupport.Instance.globalSettings.MaxTimeWithoutElectricity;

            knownCrew = new Dictionary<string, CrewMemberInfo>();
            knownVessels = new Dictionary<Guid, VesselInfo>();
        }

        public void Load(ConfigNode node)
        {
            if (node.HasNode(configNodeName))
            {
                ConfigNode settingsNode = node.GetNode(configNodeName);

                settingsNode.TryGetValue("IsNewSave", ref IsNewSave);
                settingsNode.TryGetValue("Enabled", ref Enabled);
                settingsNode.TryGetValue("UseAppLauncher", ref UseAppLauncher);
                settingsNode.TryGetValue("UseEditorFilter", ref UseEditorFilter);
                settingsNode.TryGetValue("HibernateInsteadOfKill", ref HibernateInsteadOfKill);
                settingsNode.TryGetValue("RespawnDelay", ref RespawnDelay);
                settingsNode.TryGetValue("MaxDeltaTime", ref MaxDeltaTime);
                settingsNode.TryGetValue("ElectricityMaxDeltaTime", ref ElectricityMaxDeltaTime);
                settingsNode.TryGetValue("FoodConsumptionRate", ref FoodConsumptionRate);
                settingsNode.TryGetValue("WaterConsumptionRate", ref WaterConsumptionRate);
                settingsNode.TryGetValue("OxygenConsumptionRate", ref OxygenConsumptionRate);
                settingsNode.TryGetValue("ElectricityConsumptionRate", ref ElectricityConsumptionRate);
                settingsNode.TryGetValue("BaseElectricityConsumptionRate", ref BaseElectricityConsumptionRate);
                settingsNode.TryGetValue("EvaElectricityConsumptionRate", ref EvaElectricityConsumptionRate);
                settingsNode.TryGetValue("CO2ProductionRate", ref CO2ProductionRate);
                settingsNode.TryGetValue("WasteProductionRate", ref WasteProductionRate);
                settingsNode.TryGetValue("WasteWaterProductionRate", ref WasteWaterProductionRate);
                settingsNode.TryGetValue("EvaDefaultResourceAmount", ref EvaDefaultResourceAmount);
                settingsNode.TryGetValue("MaxTimeWithoutFood", ref MaxTimeWithoutFood);
                settingsNode.TryGetValue("MaxTimeWithoutWater", ref MaxTimeWithoutWater);
                settingsNode.TryGetValue("MaxTimeWithoutOxygen", ref MaxTimeWithoutOxygen);
                settingsNode.TryGetValue("MaxTimeWithoutElectricity", ref MaxTimeWithoutElectricity);

                TacLifeSupport.Instance.globalSettings.MaxDeltaTime = MaxDeltaTime;
                TacLifeSupport.Instance.globalSettings.ElectricityMaxDeltaTime = ElectricityMaxDeltaTime;
                TacLifeSupport.Instance.globalSettings.FoodConsumptionRate = FoodConsumptionRate;
                TacLifeSupport.Instance.globalSettings.WaterConsumptionRate = WaterConsumptionRate;
                TacLifeSupport.Instance.globalSettings.OxygenConsumptionRate = OxygenConsumptionRate;
                TacLifeSupport.Instance.globalSettings.ElectricityConsumptionRate = ElectricityConsumptionRate;
                TacLifeSupport.Instance.globalSettings.BaseElectricityConsumptionRate = BaseElectricityConsumptionRate;
                TacLifeSupport.Instance.globalSettings.EvaElectricityConsumptionRate = EvaElectricityConsumptionRate;
                TacLifeSupport.Instance.globalSettings.CO2ProductionRate = CO2ProductionRate;
                TacLifeSupport.Instance.globalSettings.WasteProductionRate = WasteProductionRate;
                TacLifeSupport.Instance.globalSettings.WasteWaterProductionRate = WasteWaterProductionRate;
                TacLifeSupport.Instance.globalSettings.EvaDefaultResourceAmount = EvaDefaultResourceAmount;
                TacLifeSupport.Instance.globalSettings.MaxTimeWithoutFood = MaxTimeWithoutFood;
                TacLifeSupport.Instance.globalSettings.MaxTimeWithoutWater = MaxTimeWithoutWater;
                TacLifeSupport.Instance.globalSettings.MaxTimeWithoutOxygen = MaxTimeWithoutOxygen;
                TacLifeSupport.Instance.globalSettings.MaxTimeWithoutElectricity = MaxTimeWithoutElectricity;

                knownCrew.Clear();
                var crewNodes = settingsNode.GetNodes(CrewMemberInfo.ConfigNodeName);
                foreach (ConfigNode crewNode in crewNodes)
                {
                    CrewMemberInfo crewMemberInfo = CrewMemberInfo.Load(crewNode);
                    knownCrew[crewMemberInfo.name] = crewMemberInfo;
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
            }
        }

        public void Save(ConfigNode node)
        {
            MaxDeltaTime = TacLifeSupport.Instance.globalSettings.MaxDeltaTime;
            ElectricityMaxDeltaTime = TacLifeSupport.Instance.globalSettings.ElectricityMaxDeltaTime;
            FoodConsumptionRate = TacLifeSupport.Instance.globalSettings.FoodConsumptionRate;
            WaterConsumptionRate = TacLifeSupport.Instance.globalSettings.WaterConsumptionRate;
            OxygenConsumptionRate = TacLifeSupport.Instance.globalSettings.OxygenConsumptionRate;
            ElectricityConsumptionRate = TacLifeSupport.Instance.globalSettings.ElectricityConsumptionRate;
            BaseElectricityConsumptionRate = TacLifeSupport.Instance.globalSettings.BaseElectricityConsumptionRate;
            EvaElectricityConsumptionRate = TacLifeSupport.Instance.globalSettings.EvaElectricityConsumptionRate;
            CO2ProductionRate = TacLifeSupport.Instance.globalSettings.CO2ProductionRate;
            WasteProductionRate = TacLifeSupport.Instance.globalSettings.WasteProductionRate;
            WasteWaterProductionRate = TacLifeSupport.Instance.globalSettings.WasteWaterProductionRate;
            EvaDefaultResourceAmount = TacLifeSupport.Instance.globalSettings.EvaDefaultResourceAmount;
            MaxTimeWithoutFood = TacLifeSupport.Instance.globalSettings.MaxTimeWithoutFood;
            MaxTimeWithoutWater = TacLifeSupport.Instance.globalSettings.MaxTimeWithoutWater;
            MaxTimeWithoutOxygen = TacLifeSupport.Instance.globalSettings.MaxTimeWithoutOxygen;
            MaxTimeWithoutElectricity = TacLifeSupport.Instance.globalSettings.MaxTimeWithoutElectricity;
            
            ConfigNode settingsNode;
            if (node.HasNode(configNodeName))
            {
                settingsNode = node.GetNode(configNodeName);
            }
            else
            {
                settingsNode = node.AddNode(configNodeName);
            }

            settingsNode.AddValue("IsNewSave", IsNewSave);
            settingsNode.AddValue("Enabled", Enabled);
            settingsNode.AddValue("UseAppLauncher", UseAppLauncher);
            settingsNode.AddValue("UseEditorFilter", UseEditorFilter);
            settingsNode.AddValue("HibernateInsteadOfKill", HibernateInsteadOfKill);
            settingsNode.AddValue("RespawnDelay", RespawnDelay);
            settingsNode.AddValue("MaxDeltaTime", MaxDeltaTime);
            settingsNode.AddValue("ElectricityMaxDeltaTime", ElectricityMaxDeltaTime);
            settingsNode.AddValue("FoodConsumptionRate", FoodConsumptionRate);
            settingsNode.AddValue("WaterConsumptionRate", WaterConsumptionRate);
            settingsNode.AddValue("OxygenConsumptionRate", OxygenConsumptionRate);
            settingsNode.AddValue("ElectricityConsumptionRate", ElectricityConsumptionRate);
            settingsNode.AddValue("BaseElectricityConsumptionRate", BaseElectricityConsumptionRate);
            settingsNode.AddValue("EvaElectricityConsumptionRate", EvaElectricityConsumptionRate);
            settingsNode.AddValue("CO2ProductionRate", CO2ProductionRate);
            settingsNode.AddValue("WasteProductionRate", WasteProductionRate);
            settingsNode.AddValue("WasteWaterProductionRate", WasteWaterProductionRate);
            settingsNode.AddValue("EvaDefaultResourceAmount", EvaDefaultResourceAmount);
            settingsNode.AddValue("MaxTimeWithoutFood", MaxTimeWithoutFood);
            settingsNode.AddValue("MaxTimeWithoutWater", MaxTimeWithoutWater);
            settingsNode.AddValue("MaxTimeWithoutOxygen", MaxTimeWithoutOxygen);
            settingsNode.AddValue("MaxTimeWithoutElectricity", MaxTimeWithoutElectricity);

            foreach (CrewMemberInfo crewMemberInfo in knownCrew.Values)
            {
                crewMemberInfo.Save(settingsNode);
            }

            foreach (var entry in knownVessels)
            {
                ConfigNode vesselNode = entry.Value.Save(settingsNode);
                vesselNode.AddValue("Guid", entry.Key);
            }
        }
    }
}
