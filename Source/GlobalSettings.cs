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


        //amount of resource produced by 1 kerbal in 1 second - negative means this resource is used
        public Dictionary<int, double> kerbalProductionRates = new Dictionary<int, double>();
        public double BaseElectricityConsumptionRate { get; set; }
        public double EvaElectricityConsumptionRate { get; set; }
        public double EvaDefaultResourceAmount { get; set; }

        //for each resource, how long can a kerbal go without
        public Dictionary<int, double> kerbalStarvationTimes = new Dictionary<int, double>();
        public int[] kerbalRequirements;
        public int[] kerbalProduction;

        public Dictionary<int, String> deathCauses = new Dictionary<int, string>();

        public int FoodId { get; private set; }
        public int WaterId { get; private set; }
        public int OxygenId { get; private set; }
        public int ElectricityId { get; private set; }
        public int CO2Id { get; private set; }
        public int WasteId { get; private set; }
        public int WasteWaterId { get; private set; }

        const int SECONDS_PER_MINUTE = 60;
        const int SECONDS_PER_HOUR = 60 * SECONDS_PER_MINUTE;
        const int SECONDS_PER_KERBIN_DAY = 6 * SECONDS_PER_HOUR;

        public GlobalSettings()
        {

            
            Food = "Food";
            Water = "Water";
            Oxygen = "Oxygen";
            Electricity = "ElectricCharge";
            CO2 = "CarbonDioxide";
            Waste = "Waste";
            WasteWater = "WasteWater";

            // Consumption rates in units per Earth second
            // See the TacResources.cfg for conversions between units and metric tons.
            // Defaults are scaled from NASA's numbers for Human consumption
            // For the math behind the numbers, see
            // https://docs.google.com/spreadsheet/ccc?key=0Aioc9ek3XAvwdGNsRlh3OVhlbTFBR3M4RW0zLUNTRFE&usp=sharing
            kerbalProductionRates[FoodId] = -0.000016927083333;
            kerbalProductionRates[WaterId] = -0.000011188078704;
            kerbalProductionRates[OxygenId] = -0.001713537562385;
            kerbalProductionRates[CO2Id] = 0.00148012889876;
            kerbalProductionRates[WasteId] = 0.000001539351852;
            kerbalProductionRates[WasteWaterId] = 0.000014247685185;
            kerbalProductionRates[ElectricityId] = -0.014166666666667;

            BaseElectricityConsumptionRate = 0.02125; // 76.5 per hour or 1.275 per minute, about 75% of a stock probe core's consumption (1.7 per min)
            EvaElectricityConsumptionRate = 0.00425; // 91.8 per 6 hours (1 Kerbin day), 15.3 per hour, 15% of a probe core or 12% compared to in a pod


            kerbalStarvationTimes[FoodId] = 360.0 * SECONDS_PER_HOUR; // 360 hours, 60 Kerbin days, 15 Earth days
            kerbalStarvationTimes[WaterId] = 36.0 * SECONDS_PER_HOUR; // 36 hours, 6 Kerbin days, 1.5 Earth days
            kerbalStarvationTimes[OxygenId] = 2.0 * SECONDS_PER_HOUR; // 2 hours
            kerbalStarvationTimes[ElectricityId] = 2.0 * SECONDS_PER_HOUR; // 2 hours
            kerbalRequirements = new int[] { FoodId, WaterId, OxygenId, ElectricityId };
            kerbalProduction = new int[] { CO2Id, WaterId, WasteWaterId };
            
        }

        public void Load(ConfigNode node)
        {
            if (node.HasNode(configNodeName))
            {
                ConfigNode settingsNode = node.GetNode(configNodeName);
                MaxDeltaTime = SECONDS_PER_HOUR * 24; // max 24 hours (86,400 seconds) per physics update, or 2,160,000 seconds per second (for the default of 0.04 seconds per physics update)
                ElectricityMaxDeltaTime = 1; // max 1 second per physics update

                MaxDeltaTime = Utilities.GetValue(settingsNode, "MaxDeltaTime", MaxDeltaTime);
                ElectricityMaxDeltaTime = Utilities.GetValue(settingsNode, "ElectricityMaxDeltaTime", ElectricityMaxDeltaTime);

                //TODO this will get in a bit of a mess if you change the resources - treating the default resources and the new ones seperately
                Food = Utilities.GetValue(settingsNode, "FoodResource", Food);
                Water = Utilities.GetValue(settingsNode, "WaterResource", Water);
                Oxygen = Utilities.GetValue(settingsNode, "OxygenResource", Oxygen);
                CO2 = Utilities.GetValue(settingsNode, "CarbonDioxideResource", CO2);
                Waste = Utilities.GetValue(settingsNode, "WasteResource", Waste);
                WasteWater = Utilities.GetValue(settingsNode, "WasteWaterResource", WasteWater);

                deathCauses[FoodId] = "starvation";
                deathCauses[WaterId] = "dehydration";
                deathCauses[OxygenId] = "oxygen deprivation";
                deathCauses[ElectricityId] = "air toxicity";


                kerbalRequirements = new int[] { FoodId, WaterId, OxygenId, ElectricityId };

                // Amount of resources to load crewable parts with, in seconds
                EvaDefaultResourceAmount = 1.0 * SECONDS_PER_KERBIN_DAY; // 1 Kerbin day, 6 hours

                // Maximum amount of time in seconds that a Kerbal can go without the resource

                foreach (int resource in new int[]{FoodId, WaterId, OxygenId, CO2Id, WasteId, WasteWaterId, ElectricityId}) {
                    String resourceName =  PartResourceLibrary.Instance.GetDefinition(resource).name;
                    //Load up either a prodcution or a consumption - consuption rates need to be negated
                    kerbalProductionRates[resource] =  -Utilities.GetValue(settingsNode, resourceName+"ConsumptionRate", -kerbalProductionRates[resource]);
                    kerbalProductionRates[resource] = Utilities.GetValue(settingsNode, resourceName + "ProductionRateRate", kerbalProductionRates[resource]);
                }
                BaseElectricityConsumptionRate = Utilities.GetValue(settingsNode, "BaseElectricityConsumptionRate", BaseElectricityConsumptionRate);
                EvaElectricityConsumptionRate = Utilities.GetValue(settingsNode, "EvaElectricityConsumptionRate", EvaElectricityConsumptionRate);

                EvaDefaultResourceAmount = Utilities.GetValue(settingsNode, "EvaDefaultResourceAmount", EvaDefaultResourceAmount);
                foreach (int resource in kerbalRequirements) {
                    String resourceName =  PartResourceLibrary.Instance.GetDefinition(resource).name;
                    kerbalStarvationTimes[resource] = Utilities.GetValue(settingsNode, "MaxTimeWithout"+resourceName,kerbalStarvationTimes[resource]);
                }
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

            foreach (int resource in new int[] { FoodId, WaterId, OxygenId, CO2Id, WasteId, WasteWaterId, ElectricityId })
            {
                String resourceName = PartResourceLibrary.Instance.GetDefinition(resource).name;
                settingsNode.AddValue(resourceName + "ProductionRateRate", kerbalProductionRates[resource]);
            }
            settingsNode.AddValue("BaseElectricityConsumptionRate", BaseElectricityConsumptionRate);
            settingsNode.AddValue("EvaElectricityConsumptionRate", EvaElectricityConsumptionRate);

            settingsNode.AddValue("EvaDefaultResourceAmount", EvaDefaultResourceAmount);

            foreach (int resource in kerbalRequirements)
            {
                String resourceName = PartResourceLibrary.Instance.GetDefinition(resource).name;
                settingsNode.AddValue("MaxTimeWithout" + resourceName, kerbalStarvationTimes[resource]);
            }
        }
    }
}
