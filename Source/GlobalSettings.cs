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
            const int SECONDS_PER_MINUTE = 60;
            const int SECONDS_PER_HOUR = 60 * SECONDS_PER_MINUTE;
            const int SECONDS_PER_KERBIN_DAY = 6 * SECONDS_PER_HOUR;

            MaxDeltaTime = SECONDS_PER_HOUR * 24; // max 24 hours (86,400 seconds) per physics update, or 2,160,000 seconds per second (for the default of 0.04 seconds per physics update)
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
            // For the math behind the numbers, see
            // https://docs.google.com/spreadsheet/ccc?key=0Aioc9ek3XAvwdGNsRlh3OVhlbTFBR3M4RW0zLUNTRFE&usp=sharing
            FoodConsumptionRate = 0.000042560952886;
            WaterConsumptionRate = 0.000020807576966;
            OxygenConsumptionRate = 0.003521591846326;
            BaseElectricityConsumptionRate = 100.0 / SECONDS_PER_HOUR; // 100 per hour or 0.02777778 per second, same as a stock probe core
            ElectricityConsumptionRate = BaseElectricityConsumptionRate / 2.0; // 0.01388889 per second, half as much as a stock probe core
            EvaElectricityConsumptionRate = 20.0 / 6.0 / SECONDS_PER_HOUR; // 20 per 6 hours (1 Kerbin day), 0.000925 per second
            CO2ProductionRate = 0.003029853129847;
            WasteProductionRate = 0.000076255040587;
            WasteWaterProductionRate = 0.000022876512176;

            // Amount of resources to load crewable parts with, in seconds
            EvaDefaultResourceAmount = 1.0 * SECONDS_PER_KERBIN_DAY; // 1 Kerbin day, 6 hours

            // Maximum amount of time in seconds that a Kerbal can go without the resource
            MaxTimeWithoutFood = 30.0 * SECONDS_PER_KERBIN_DAY; // 30 Kerbin days, 180 hours
            MaxTimeWithoutWater = 3.0 * SECONDS_PER_KERBIN_DAY; // 3 Kerbin days, 18 hours
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

                FoodConsumptionRate = Utilities.GetValue(settingsNode, "FoodConsumptionRate", FoodConsumptionRate);
                WaterConsumptionRate = Utilities.GetValue(settingsNode, "WaterConsumptionRate", WaterConsumptionRate);
                OxygenConsumptionRate = Utilities.GetValue(settingsNode, "OxygenConsumptionRate", OxygenConsumptionRate);
                ElectricityConsumptionRate = Utilities.GetValue(settingsNode, "ElectricityConsumptionRate", ElectricityConsumptionRate);
                BaseElectricityConsumptionRate = Utilities.GetValue(settingsNode, "BaseElectricityConsumptionRate", BaseElectricityConsumptionRate);
                EvaElectricityConsumptionRate = Utilities.GetValue(settingsNode, "EvaElectricityConsumptionRate", EvaElectricityConsumptionRate);
                CO2ProductionRate = Utilities.GetValue(settingsNode, "CO2ProductionRate", CO2ProductionRate);
                WasteProductionRate = Utilities.GetValue(settingsNode, "WasteProductionRate", WasteProductionRate);
                WasteWaterProductionRate = Utilities.GetValue(settingsNode, "WasteWaterProductionRate", WasteWaterProductionRate);

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
        }
    }
}
