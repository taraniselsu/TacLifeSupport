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
using KSP.Localization;

namespace Tac
{
    public class GlobalSettings
    {
        private const string configNodeName = "TACLSGlobalSettings";

        public int MaxDeltaTime;
        public int ElectricityMaxDeltaTime; 
        
        public string Food;  //English name/key
        public string Water; //English name/key
        public string Oxygen; //English name/key
        public string Electricity; //English name/key
        public string CO2; //English name/key
        public string Waste; //English name/key
        public string WasteWater; //English name/key
        public string displayFood; //Localized name for UI
        public string displayWater; //Localized name for UI
        public string displayOxygen; //Localized name for UI
        public string displayElectricity; //Localized name for UI
        public string displayCO2; //Localized name for UI
        public string displayWaste; //Localized name for UI
        public string displayWasteWater; //Localized name for UI
        public double FoodConsumptionRate;
        public double WaterConsumptionRate;
        public double OxygenConsumptionRate;
        public double ElectricityConsumptionRate;
        public double BaseElectricityConsumptionRate;
        public double EvaElectricityConsumptionRate;
        public double EvaLampElectricityConsumptionRate;
        public double CO2ProductionRate;
        public double WasteProductionRate;
        public double WasteWaterProductionRate;
        public double EvaDefaultResourceAmount;
        public double MaxTimeWithoutFood;
        public double MaxTimeWithoutWater;
        public double MaxTimeWithoutOxygen;
        public double MaxTimeWithoutElectricity; 
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
            displayFood = Localizer.Format("#autoLOC_TACLS_00016");
            Water = "Water";
            displayWater = Localizer.Format("#autoLOC_TACLS_00017");
            Oxygen = "Oxygen";
            displayOxygen = Localizer.Format("#autoLOC_TACLS_00018");
            Electricity = "ElectricCharge";
            displayElectricity = Localizer.Format("#autoLOC_TACLS_00019");
            CO2 = "CarbonDioxide";
            displayCO2 = Localizer.Format("#autoLOC_TACLS_00022");
            Waste = "Waste";
            displayWaste = Localizer.Format("#autoLOC_TACLS_00020");
            WasteWater = "WasteWater";
            displayWasteWater = Localizer.Format("#autoLOC_TACLS_00021");
            FoodId = PartResourceLibrary.Instance.GetDefinition(Food).id;
            WaterId = PartResourceLibrary.Instance.GetDefinition(Water).id;
            OxygenId = PartResourceLibrary.Instance.GetDefinition(Oxygen).id;
            ElectricityId = PartResourceLibrary.Instance.GetDefinition(Electricity).id;
            CO2Id = PartResourceLibrary.Instance.GetDefinition(CO2).id;
            WasteId = PartResourceLibrary.Instance.GetDefinition(Waste).id;
            WasteWaterId = PartResourceLibrary.Instance.GetDefinition(WasteWater).id;

            // Consumption rates in units per Earth second
            // See the TacResources.cfg for conversions between units and metric tons.
            // Defaults are scaled from NASA's numbers for Human consumption
            // For the math behind the numbers, see
            // https://docs.google.com/spreadsheet/ccc?key=0Aioc9ek3XAvwdGNsRlh3OVhlbTFBR3M4RW0zLUNTRFE&usp=sharing
            FoodConsumptionRate = 0.000016927083333;
            WaterConsumptionRate = 0.000011188078704;
            OxygenConsumptionRate = 0.001713537562385;
            CO2ProductionRate = 0.00148012889876;
            WasteProductionRate = 0.000001539351852;
            WasteWaterProductionRate = 0.000014247685185;

            BaseElectricityConsumptionRate = 0.02125; // 76.5 per hour or 1.275 per minute, about 75% of a stock probe core's consumption (1.7 per min)
            ElectricityConsumptionRate = 0.014166666666667; // 51 per hour or 0.85 per minute, about 50% of a stock probe core's consumption
            EvaElectricityConsumptionRate = 0.00425; // 91.8 per 6 hours (1 Kerbin day), 15.3 per hour, 15% of a probe core or 12% compared to in a pod
            EvaLampElectricityConsumptionRate = 0.00213;// 45.9 per 6 hours (1 Kerbin day), 7.65 per hour
            // Amount of resources to load crewable parts with, in seconds
            EvaDefaultResourceAmount = 1.0 * SECONDS_PER_KERBIN_DAY; // 1 Kerbin day, 6 hours

            // Maximum amount of time in seconds that a Kerbal can go without the resource
            MaxTimeWithoutFood = 360.0 * SECONDS_PER_HOUR; // 360 hours, 60 Kerbin days, 15 Earth days
            MaxTimeWithoutWater = 36.0 * SECONDS_PER_HOUR; // 36 hours, 6 Kerbin days, 1.5 Earth days
            MaxTimeWithoutOxygen = 2.0 * SECONDS_PER_HOUR; // 2 hours
            MaxTimeWithoutElectricity = 2.0 * SECONDS_PER_HOUR; // 2 hours
        }

        public void Load(ConfigNode node)
        {
            //if (node.HasNode(configNodeName))
            //{
            //ConfigNode settingsNode = node.GetNode(configNodeName);
            //ConfigNode TACLSsettingsNode = new ConfigNode();
            //if (!node.TryGetNode(configNodeName, ref TACLSsettingsNode)) return;
            node.TryGetValue("MaxDeltaTime", ref MaxDeltaTime);
            node.TryGetValue("ElectricityMaxDeltaTime", ref ElectricityMaxDeltaTime);
            node.TryGetValue("FoodResource", ref Food);
            node.TryGetValue("WaterResource", ref Water);
            node.TryGetValue("OxygenResource", ref Oxygen);
            node.TryGetValue("CarbonDioxideResource", ref CO2);
            node.TryGetValue("WasteResource", ref Waste);
            node.TryGetValue("WasteWaterResource", ref WasteWater);
            node.TryGetValue("FoodConsumptionRate", ref FoodConsumptionRate);
            node.TryGetValue("WaterConsumptionRate", ref WaterConsumptionRate);
            node.TryGetValue("OxygenConsumptionRate", ref OxygenConsumptionRate);
            node.TryGetValue("ElectricityConsumptionRate", ref ElectricityConsumptionRate);
            node.TryGetValue("BaseElectricityConsumptionRate", ref BaseElectricityConsumptionRate);
            node.TryGetValue("EvaElectricityConsumptionRate", ref EvaElectricityConsumptionRate);
            node.TryGetValue("EvaLampElectricityConsumptionRate", ref EvaLampElectricityConsumptionRate);
            node.TryGetValue("CO2ProductionRate", ref CO2ProductionRate);
            node.TryGetValue("WasteProductionRate", ref WasteProductionRate);
            node.TryGetValue("WasteWaterProductionRate", ref WasteWaterProductionRate);
            node.TryGetValue("EvaDefaultResourceAmount", ref EvaDefaultResourceAmount);
            node.TryGetValue("MaxTimeWithoutFood", ref MaxTimeWithoutFood);
            node.TryGetValue("MaxTimeWithoutWater", ref MaxTimeWithoutWater);
            node.TryGetValue("MaxTimeWithoutOxygen", ref MaxTimeWithoutOxygen);
            node.TryGetValue("MaxTimeWithoutElectricity", ref MaxTimeWithoutElectricity);
            //}
        }
        
    }
}
