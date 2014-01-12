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
    public class GlobalSettings
    {
        private const string configNodeName = "GlobalSettings";
        private const int SECONDS_PER_MINUTE = 60;
        private const int SECONDS_PER_HOUR = 60 * SECONDS_PER_MINUTE;
        private const int SECONDS_PER_DAY = 24 * SECONDS_PER_HOUR;

        public int MaxDeltaTime { get; set; }
        public int ElectricityMaxDeltaTime { get; set; }

        private string food;
        private string water;
        private string oxygen;
        private string electricity;
        private string co2;
        private string waste;
        private string wasteWater;

        public string Food
        {
            get { return food; }
            private set
            {
                food = value;
                FoodId = PartResourceLibrary.Instance.GetDefinition(food).id;
            }
        }
        public string Water
        {
            get { return water; }
            private set
            {
                water = value;
                WaterId = PartResourceLibrary.Instance.GetDefinition(water).id;
            }
        }
        public string Oxygen
        {
            get { return oxygen; }
            private set
            {
                oxygen = value;
                OxygenId = PartResourceLibrary.Instance.GetDefinition(oxygen).id;
            }
        }
        public string Electricity
        {
            get { return electricity; }
            private set
            {
                electricity = value;
                ElectricityId = PartResourceLibrary.Instance.GetDefinition(electricity).id;
            }
        }
        public string CO2
        {
            get { return co2; }
            private set
            {
                co2 = value;
                CO2Id = PartResourceLibrary.Instance.GetDefinition(co2).id;
            }
        }
        public string Waste
        {
            get { return waste; }
            private set
            {
                waste = value;
                WasteId = PartResourceLibrary.Instance.GetDefinition(waste).id;
            }
        }
        public string WasteWater
        {
            get { return wasteWater; }
            private set
            {
                wasteWater = value;
                WasteWaterId = PartResourceLibrary.Instance.GetDefinition(wasteWater).id;
            }
        }

        public double FoodConsumptionRate { get; set; }
        public double WaterConsumptionRate { get; set; }
        public double OxygenConsumptionRate { get; set; }
        public double ElectricityConsumptionRate { get; set; }
        public double BaseElectricityConsumptionRate { get; set; }
        public double EvaElectricityConsumptionRate { get; set; }
        public double CO2ProductionRate { get; set; }
        public double WasteProductionRate { get; set; }
        public double WasteWaterProductionRate { get; set; }

        public double DefaultResourceAmount { get; set; }
        public double EvaDefaultResourceAmount { get; set; }

        public double MaxTimeWithoutFood { get; set; }
        public double MaxTimeWithoutWater { get; set; }
        public double MaxTimeWithoutOxygen { get; set; }
        public double MaxTimeWithoutElectricity { get; set; }

        public int FoodId { get; private set; }
        public int WaterId { get; private set; }
        public int OxygenId { get; private set; }
        public int ElectricityId { get; private set; }
        public int CO2Id { get; private set; }
        public int WasteId { get; private set; }
        public int WasteWaterId { get; private set; }

        public GlobalSettings()
        {
            MaxDeltaTime = SECONDS_PER_DAY; // max 1 day (24 hour) per physics update, or 50 days (4,320,000 seconds) per second
            ElectricityMaxDeltaTime = 1; // max 1 second per physics update

            Food = "Food";
            Water = "Water";
            Oxygen = "Oxygen";
            Electricity = "ElectricCharge";
            CO2 = "CarbonDioxide";
            Waste = "Waste";
            WasteWater = "WasteWater";

            // Consumption rates in units per Earth second
            // See the TacResources.cfg for conversions between units and metric tons.
            // Defaults are based on 50% of NASA's numbers for Human consumption
            FoodConsumptionRate = 1.0 / SECONDS_PER_DAY;
            WaterConsumptionRate = 1.0 / SECONDS_PER_DAY;
            OxygenConsumptionRate = 1.0 / SECONDS_PER_DAY;
            ElectricityConsumptionRate = 1200.0 / SECONDS_PER_DAY;
            BaseElectricityConsumptionRate = 2400.0 / SECONDS_PER_DAY;
            EvaElectricityConsumptionRate = 100.0 / (SECONDS_PER_DAY / 2.0); // 100 per 12 hours (1/2 day)
            CO2ProductionRate = 1.0 / SECONDS_PER_DAY;
            WasteProductionRate = 1.0 / SECONDS_PER_DAY;
            WasteWaterProductionRate = 1.0 / SECONDS_PER_DAY;

            // Amount of resources to load crewable parts with, in seconds
            DefaultResourceAmount = 1.0 * SECONDS_PER_DAY; // 1 day (24 hours)
            EvaDefaultResourceAmount = 0.5 * SECONDS_PER_DAY; // 1/2 day (12 hours)

            // Maximum amount of time in seconds that a Kerbal can go without the resource
            MaxTimeWithoutFood = 30.0 * SECONDS_PER_DAY; // 30 days
            MaxTimeWithoutWater = 3.0 * SECONDS_PER_DAY; // 3 days
            MaxTimeWithoutOxygen = 2.0 * SECONDS_PER_HOUR; // 2 hours
            MaxTimeWithoutElectricity = 2.0 * SECONDS_PER_HOUR; // 2 hours
        }

        public void Load(ConfigNode node)
        {
            if (node.HasNode(configNodeName))
            {
                ConfigNode settingsNode = node.GetNode(configNodeName);

                MaxDeltaTime = Utilities.GetValue(settingsNode, "MaxDeltaTime", MaxDeltaTime);
                ElectricityMaxDeltaTime = Utilities.GetValue(settingsNode, "ElectricityMaxDeltaTime", ElectricityMaxDeltaTime);

                Food = Utilities.GetValue(settingsNode, "FoodResource", Food);
                Water = Utilities.GetValue(settingsNode, "WaterResource", Water);
                Oxygen = Utilities.GetValue(settingsNode, "OxygenResource", Oxygen);
                CO2 = Utilities.GetValue(settingsNode, "CarbonDioxideResource", CO2);
                Waste = Utilities.GetValue(settingsNode, "WasteResource", Waste);
                WasteWater = Utilities.GetValue(settingsNode, "WasteWaterResource", WasteWater);

                FoodConsumptionRate = Utilities.GetValue(settingsNode, "FoodConsumptionRate", FoodConsumptionRate) / SECONDS_PER_DAY;
                WaterConsumptionRate = Utilities.GetValue(settingsNode, "WaterConsumptionRate", WaterConsumptionRate) / SECONDS_PER_DAY;
                OxygenConsumptionRate = Utilities.GetValue(settingsNode, "OxygenConsumptionRate", OxygenConsumptionRate) / SECONDS_PER_DAY;
                ElectricityConsumptionRate = Utilities.GetValue(settingsNode, "ElectricityConsumptionRate", ElectricityConsumptionRate) / SECONDS_PER_DAY;
                BaseElectricityConsumptionRate = Utilities.GetValue(settingsNode, "BaseElectricityConsumptionRate", BaseElectricityConsumptionRate) / SECONDS_PER_DAY;
                EvaElectricityConsumptionRate = Utilities.GetValue(settingsNode, "EvaElectricityConsumptionRate", EvaElectricityConsumptionRate) / SECONDS_PER_DAY;
                CO2ProductionRate = Utilities.GetValue(settingsNode, "CO2ProductionRate", CO2ProductionRate) / SECONDS_PER_DAY;
                WasteProductionRate = Utilities.GetValue(settingsNode, "WasteProductionRate", WasteProductionRate) / SECONDS_PER_DAY;
                WasteWaterProductionRate = Utilities.GetValue(settingsNode, "WasteWaterProductionRate", WasteWaterProductionRate) / SECONDS_PER_DAY;

                DefaultResourceAmount = Utilities.GetValue(settingsNode, "DefaultResourceAmount", DefaultResourceAmount);
                EvaDefaultResourceAmount = Utilities.GetValue(settingsNode, "EvaDefaultResourceAmount", EvaDefaultResourceAmount);

                MaxTimeWithoutFood = Utilities.GetValue(settingsNode, "MaxTimeWithoutFood", MaxTimeWithoutFood);
                MaxTimeWithoutWater = Utilities.GetValue(settingsNode, "MaxTimeWithoutWater", MaxTimeWithoutWater);
                MaxTimeWithoutOxygen = Utilities.GetValue(settingsNode, "MaxTimeWithoutOxygen", MaxTimeWithoutOxygen);
                MaxTimeWithoutElectricity = Utilities.GetValue(settingsNode, "MaxTimeWithoutElectricity", MaxTimeWithoutElectricity);
            }
        }

        public void Save(ConfigNode node)
        {
            ConfigNode settingsNode;
            if (node.HasNode(configNodeName))
            {
                settingsNode = node.GetNode(configNodeName);
                settingsNode.ClearData();
            }
            else
            {
                settingsNode = node.AddNode(configNodeName);
            }

            settingsNode.AddValue("MaxDeltaTime", MaxDeltaTime);
            settingsNode.AddValue("ElectricityMaxDeltaTime", ElectricityMaxDeltaTime);

            settingsNode.AddValue("FoodResource", Food);
            settingsNode.AddValue("WaterResource", Water);
            settingsNode.AddValue("OxygenResource", Oxygen);
            settingsNode.AddValue("CarbonDioxideResource", CO2);
            settingsNode.AddValue("WasteResource", Waste);
            settingsNode.AddValue("WasteWaterResource", WasteWater);

            settingsNode.AddValue("FoodConsumptionRate", FoodConsumptionRate * SECONDS_PER_DAY);
            settingsNode.AddValue("WaterConsumptionRate", WaterConsumptionRate * SECONDS_PER_DAY);
            settingsNode.AddValue("OxygenConsumptionRate", OxygenConsumptionRate * SECONDS_PER_DAY);
            settingsNode.AddValue("ElectricityConsumptionRate", ElectricityConsumptionRate * SECONDS_PER_DAY);
            settingsNode.AddValue("BaseElectricityConsumptionRate", BaseElectricityConsumptionRate * SECONDS_PER_DAY);
            settingsNode.AddValue("EvaElectricityConsumptionRate", EvaElectricityConsumptionRate * SECONDS_PER_DAY);
            settingsNode.AddValue("CO2ProductionRate", CO2ProductionRate * SECONDS_PER_DAY);
            settingsNode.AddValue("WasteProductionRate", WasteProductionRate * SECONDS_PER_DAY);
            settingsNode.AddValue("WasteWaterProductionRate", WasteWaterProductionRate * SECONDS_PER_DAY);

            settingsNode.AddValue("DefaultResourceAmount", DefaultResourceAmount);
            settingsNode.AddValue("EvaDefaultResourceAmount", EvaDefaultResourceAmount);

            settingsNode.AddValue("MaxTimeWithoutFood", MaxTimeWithoutFood);
            settingsNode.AddValue("MaxTimeWithoutWater", MaxTimeWithoutWater);
            settingsNode.AddValue("MaxTimeWithoutOxygen", MaxTimeWithoutOxygen);
            settingsNode.AddValue("MaxTimeWithoutElectricity", MaxTimeWithoutElectricity);
        }
    }
}
