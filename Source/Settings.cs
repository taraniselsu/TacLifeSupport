/**
 * Settings.cs
 * 
 * Thunder Aerospace Corporation's Life Support for the Kerbal Space Program, by Taranis Elsu
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
    public class Settings
    {
        private const int SECONDS_PER_MINUTE = 60;
        private const int SECONDS_PER_HOUR = 60 * SECONDS_PER_MINUTE;
        private const int SECONDS_PER_DAY = 24 * SECONDS_PER_HOUR;
        private const int SECONDS_PER_KERBIN_DAY = 6 * SECONDS_PER_HOUR;

        public string Food { get; private set; }
        public string Water { get; private set; }
        public string Oxygen { get; private set; }
        public string Electricity { get { return "ElectricCharge"; } }
        public string CO2 { get; private set; }
        public string Waste { get; private set; }
        public string WasteWater { get; private set; }

        public double FoodConsumptionRate { get; set; }
        public double WaterConsumptionRate { get; set; }
        public double OxygenConsumptionRate { get; set; }
        public double ElectricityConsumptionRate { get; set; }
        public double BaseElectricityConsumptionRate { get; set; }
        public double EvaElectricityConsumptionRate { get; set; }
        public double CO2ProductionRate { get; set; }
        public double WasteProductionRate { get; set; }
        public double WasteWaterProductionRate { get; set; }

        public double MaxTimeWithoutFood { get; set; }
        public double MaxTimeWithoutWater { get; set; }
        public double MaxTimeWithoutOxygen { get; set; }
        public double MaxTimeWithoutElectricity { get; set; }

        public double DefaultResourceAmount { get; set; }
        public double EvaDefaultResourceAmount { get; set; }

        public int FoodId
        {
            get
            {
                return PartResourceLibrary.Instance.GetDefinition(Food).id;
            }
        }
        public int WaterId
        {
            get
            {
                return PartResourceLibrary.Instance.GetDefinition(Water).id;
            }
        }
        public int OxygenId
        {
            get
            {
                return PartResourceLibrary.Instance.GetDefinition(Oxygen).id;
            }
        }
        public int ElectricityId
        {
            get
            {
                return PartResourceLibrary.Instance.GetDefinition(Electricity).id;
            }
        }
        public int CO2Id
        {
            get
            {
                return PartResourceLibrary.Instance.GetDefinition(CO2).id;
            }
        }
        public int WasteId
        {
            get
            {
                return PartResourceLibrary.Instance.GetDefinition(Waste).id;
            }
        }
        public int WasteWaterId
        {
            get
            {
                return PartResourceLibrary.Instance.GetDefinition(WasteWater).id;
            }
        }

        public bool AllowCrewRespawn
        {
            get
            {
                return HighLogic.CurrentGame.Parameters.Difficulty.MissingCrewsRespawn;
            }
            set
            {
                HighLogic.CurrentGame.Parameters.Difficulty.MissingCrewsRespawn = value;
            }
        }
        public double RespawnDelay { get; set; }

        public Settings()
        {
            Food = "Food";
            Water = "Water";
            Oxygen = "Oxygen";
            CO2 = "CarbonDioxide";
            Waste = "Waste";
            WasteWater = "WasteWater";

            // Consumption rates in units per Earth second
            // See the TacResources.cfg for conversions between units and kg.
            // FIXME add my calculations and references
            FoodConsumptionRate = 1.0 / SECONDS_PER_KERBIN_DAY;
            WaterConsumptionRate = 1.0 / SECONDS_PER_KERBIN_DAY;
            OxygenConsumptionRate = 1.0 / SECONDS_PER_KERBIN_DAY;
            ElectricityConsumptionRate = 1200.0 / SECONDS_PER_DAY;
            BaseElectricityConsumptionRate = 2400.0 / SECONDS_PER_DAY;
            EvaElectricityConsumptionRate = 100.0 / (SECONDS_PER_DAY / 2.0); // 100 per 12 hours (1/2 day)
            CO2ProductionRate = 1.0 / SECONDS_PER_KERBIN_DAY;
            WasteProductionRate = 1.0 / SECONDS_PER_KERBIN_DAY;
            WasteWaterProductionRate = 1.0 / SECONDS_PER_KERBIN_DAY;

            MaxTimeWithoutFood = 30.0 * SECONDS_PER_KERBIN_DAY; // 30 days
            MaxTimeWithoutWater = 3.0 * SECONDS_PER_KERBIN_DAY; // 3 days
            MaxTimeWithoutOxygen = 2.0 * SECONDS_PER_HOUR; // 2 hours
            MaxTimeWithoutElectricity = 2.0 * SECONDS_PER_HOUR; // 2 hours

            // Amount of resources to load crewable parts with, in seconds
            DefaultResourceAmount = 1.0 * SECONDS_PER_KERBIN_DAY; // 1 Kerbin day (~6 hours)
            EvaDefaultResourceAmount = 0.5 * SECONDS_PER_KERBIN_DAY; // 1/2 Kerbin day (~3 hours)

            RespawnDelay = 9203545.0; // 1 Kerbin year (default is too short at only 36 minutes)
        }

        public void Load(ConfigNode config)
        {
            Food = Utilities.GetValue(config, "FoodResource", Food);
            Water = Utilities.GetValue(config, "WaterResource", Water);
            Oxygen = Utilities.GetValue(config, "OxygenResource", Oxygen);
            CO2 = Utilities.GetValue(config, "CarbonDioxideResource", CO2);
            Waste = Utilities.GetValue(config, "WasteResource", Waste);
            WasteWater = Utilities.GetValue(config, "WasteWaterResource", WasteWater);

            FoodConsumptionRate = Utilities.GetValue(config, "FoodConsumptionRate", FoodConsumptionRate) / SECONDS_PER_KERBIN_DAY;
            WaterConsumptionRate = Utilities.GetValue(config, "WaterConsumptionRate", WaterConsumptionRate) / SECONDS_PER_KERBIN_DAY;
            OxygenConsumptionRate = Utilities.GetValue(config, "OxygenConsumptionRate", OxygenConsumptionRate) / SECONDS_PER_KERBIN_DAY;
            ElectricityConsumptionRate = Utilities.GetValue(config, "ElectricityConsumptionRate", ElectricityConsumptionRate) / SECONDS_PER_KERBIN_DAY;
            BaseElectricityConsumptionRate = Utilities.GetValue(config, "BaseElectricityConsumptionRate", BaseElectricityConsumptionRate) / SECONDS_PER_KERBIN_DAY;
            EvaElectricityConsumptionRate = Utilities.GetValue(config, "EvaElectricityConsumptionRate", EvaElectricityConsumptionRate) / SECONDS_PER_KERBIN_DAY;
            CO2ProductionRate = Utilities.GetValue(config, "CO2ProductionRate", CO2ProductionRate) / SECONDS_PER_KERBIN_DAY;
            WasteProductionRate = Utilities.GetValue(config, "WasteProductionRate", WasteProductionRate) / SECONDS_PER_KERBIN_DAY;
            WasteWaterProductionRate = Utilities.GetValue(config, "WasteWaterProductionRate", WasteWaterProductionRate) / SECONDS_PER_KERBIN_DAY;

            MaxTimeWithoutFood = Utilities.GetValue(config, "MaxTimeWithoutFood", MaxTimeWithoutFood);
            MaxTimeWithoutWater = Utilities.GetValue(config, "MaxTimeWithoutWater", MaxTimeWithoutWater);
            MaxTimeWithoutOxygen = Utilities.GetValue(config, "MaxTimeWithoutOxygen", MaxTimeWithoutOxygen);
            MaxTimeWithoutElectricity = Utilities.GetValue(config, "MaxTimeWithoutElectricity", MaxTimeWithoutElectricity);

            DefaultResourceAmount = Utilities.GetValue(config, "DefaultResourceAmount", DefaultResourceAmount);
            EvaDefaultResourceAmount = Utilities.GetValue(config, "EvaDefaultResourceAmount", EvaDefaultResourceAmount);

            RespawnDelay = Utilities.GetValue(config, "RespawnDelay", RespawnDelay);
        }

        public void Save(ConfigNode config)
        {
            config.AddValue("FoodResource", Food);
            config.AddValue("WaterResource", Water);
            config.AddValue("OxygenResource", Oxygen);
            config.AddValue("CarbonDioxideResource", CO2);
            config.AddValue("WasteResource", Waste);
            config.AddValue("WasteWaterResource", WasteWater);

            config.AddValue("FoodConsumptionRate", FoodConsumptionRate * SECONDS_PER_KERBIN_DAY);
            config.AddValue("WaterConsumptionRate", WaterConsumptionRate * SECONDS_PER_KERBIN_DAY);
            config.AddValue("OxygenConsumptionRate", OxygenConsumptionRate * SECONDS_PER_KERBIN_DAY);
            config.AddValue("ElectricityConsumptionRate", ElectricityConsumptionRate * SECONDS_PER_KERBIN_DAY);
            config.AddValue("BaseElectricityConsumptionRate", BaseElectricityConsumptionRate * SECONDS_PER_KERBIN_DAY);
            config.AddValue("EvaElectricityConsumptionRate", EvaElectricityConsumptionRate * SECONDS_PER_KERBIN_DAY);
            config.AddValue("CO2ProductionRate", CO2ProductionRate * SECONDS_PER_KERBIN_DAY);
            config.AddValue("WasteProductionRate", WasteProductionRate * SECONDS_PER_KERBIN_DAY);
            config.AddValue("WasteWaterProductionRate", WasteWaterProductionRate * SECONDS_PER_KERBIN_DAY);

            config.AddValue("MaxTimeWithoutFood", MaxTimeWithoutFood);
            config.AddValue("MaxTimeWithoutWater", MaxTimeWithoutWater);
            config.AddValue("MaxTimeWithoutOxygen", MaxTimeWithoutOxygen);
            config.AddValue("MaxTimeWithoutElectricity", MaxTimeWithoutElectricity);

            config.AddValue("DefaultResourceAmount", DefaultResourceAmount);
            config.AddValue("EvaDefaultResourceAmount", EvaDefaultResourceAmount);

            config.AddValue("RespawnDelay", RespawnDelay);
        }
    }
}
