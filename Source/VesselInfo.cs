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


namespace Tac
{

    public class VesselInfo
    {
        public const string ConfigNodeName = "VesselInfo";

        public string vesselName;  //The Vessel Name
        public VesselType vesselType = VesselType.Unknown;  //The Vessel Type
        public Vessel.Situations vesselSituation = Vessel.Situations.PRELAUNCH; //The Vessel Situation
        public bool vesselIsPreLaunch = true;  //True if vessel is PreLaunch
        public int numCrew;  //The number of crew on board
        public int numOccupiedParts; //The number of occupied parts
        public int numFrozenCrew; //The number of frozen crew on board (DeepFreeze compatibility)

        public double lastUpdate;  //The last time the vesselinfo was updated
        public double lastFood;    //The last time food was consumed
        public double lastWater;   //The last time water was consumed
        public double lastOxygen;  //The last time oxygen was consumed
        public double lastElectricity; //The last time electricity was consumed

        public Status overallStatus = Status.GOOD;
        public Status foodStatus = Status.GOOD;
        public Status waterStatus = Status.GOOD;
        public Status oxygenStatus = Status.GOOD;
        public Status electricityStatus = Status.GOOD;

        public double remainingFood;
        public double remainingWater;
        public double remainingOxygen;
        public double remainingElectricity;
        public double remainingCO2;
        public double remainingWaste;
        public double remainingWasteWater;

        public double maxFood;
        public double maxWater;
        public double maxOxygen;
        public double maxElectricity;

        public double estimatedTimeFoodDepleted;
        public double estimatedTimeWaterDepleted;
        public double estimatedTimeOxygenDepleted;
        public double estimatedTimeElectricityDepleted;

        public double estimatedElectricityConsumptionRate;
        public bool hibernating;
        public bool recoveryvessel;  //True if Vessel is a Recover Kerbal/Part Contract Vessel.
        public bool windowOpen; //True if we are out of EC, but in atmosphere so have the windows open.

        public VesselInfo(string vesselName, Vessel.Situations situation, VesselType vesseltype, double currentTime)
        {
            this.vesselName = vesselName;
            lastUpdate = currentTime;
            lastFood = currentTime;
            lastWater = currentTime;
            lastOxygen = currentTime;
            lastElectricity = currentTime;
            hibernating = false;
            numFrozenCrew = 0;
            vesselSituation = situation;
            vesselIsPreLaunch = situation == Vessel.Situations.PRELAUNCH;
            vesselType = vesseltype;
            recoveryvessel = false;
            windowOpen = false;
        }

        public static VesselInfo Load(ConfigNode node)
        {
            string vesselName = Utilities.GetValue(node, "vesselName", "Unknown");
            double lastUpdate = Utilities.GetValue(node, "lastUpdate", 0.0);
            Vessel.Situations vesselSituation = Utilities.GetValue(node, "vesselSituation", Vessel.Situations.PRELAUNCH);
            VesselType vesselType = Utilities.GetValue(node, "vesselType", VesselType.Unknown);

            VesselInfo info = new VesselInfo(vesselName, vesselSituation, vesselType, lastUpdate);

            
            info.vesselIsPreLaunch = Utilities.GetValue(node, "vesselIsPreLaunch", true);
            if (info.vesselIsPreLaunch && !(vesselSituation == Vessel.Situations.PRELAUNCH))
            {
                Logging.LogError("VesselInfo.Load",
                    "Mismatch between VesselSituation and vesselIsPreLaunch, setting to Prelaunch");
                info.vesselIsPreLaunch = true;
                info.vesselSituation = Vessel.Situations.PRELAUNCH;
            }

            info.numCrew = Utilities.GetValue(node, "numCrew", 0);
            info.numFrozenCrew = Utilities.GetValue(node, "numFrozenCrew", 0);
            info.numOccupiedParts = Utilities.GetValue(node, "numOccupiedParts", 0);

            info.lastFood = Utilities.GetValue(node, "lastFood", lastUpdate);
            info.lastWater = Utilities.GetValue(node, "lastWater", lastUpdate);
            info.lastOxygen = Utilities.GetValue(node, "lastOxygen", lastUpdate);
            info.lastElectricity = Utilities.GetValue(node, "lastElectricity", lastUpdate);

            info.remainingFood = Utilities.GetValue(node, "remainingFood", 0.0);
            info.remainingWater = Utilities.GetValue(node, "remainingWater", 0.0);
            info.remainingOxygen = Utilities.GetValue(node, "remainingOxygen", 0.0);
            info.remainingElectricity = Utilities.GetValue(node, "remainingElectricity", 0.0);
            info.remainingCO2 = Utilities.GetValue(node, "remainingCO2", 0.0);
            info.remainingWaste = Utilities.GetValue(node, "remainingWaste", 0.0);
            info.remainingWasteWater = Utilities.GetValue(node, "remainingWasteWater", 0.0);

            info.maxFood = Utilities.GetValue(node, "maxFood", 0.0);
            info.maxWater = Utilities.GetValue(node, "maxWater", 0.0);
            info.maxOxygen = Utilities.GetValue(node, "maxOxygen", 0.0);
            info.maxElectricity = Utilities.GetValue(node, "maxElectricity", 0.0);

            info.estimatedElectricityConsumptionRate = Utilities.GetValue(node, "estimatedElectricityConsumptionRate", 0.0);

            info.hibernating = Utilities.GetValue(node, "hibernating", false);
            info.recoveryvessel = Utilities.GetValue(node, "recoveryvessel", false);
            info.windowOpen = Utilities.GetValue(node, "windowOpen", false);           

            return info;
        }

        public ConfigNode Save(ConfigNode config)
        {
            ConfigNode node = config.AddNode(ConfigNodeName);
            node.AddValue("vesselName", vesselName);
            node.AddValue("vesselType", vesselType.ToString());
            node.AddValue("vesselSituation", vesselSituation.ToString());
            node.AddValue("vesselIsPreLaunch", vesselIsPreLaunch);
            node.AddValue("numCrew", numCrew);
            node.AddValue("numFrozenCrew", numFrozenCrew);
            node.AddValue("numOccupiedParts", numOccupiedParts);

            node.AddValue("lastUpdate", lastUpdate);
            node.AddValue("lastFood", lastFood);
            node.AddValue("lastWater", lastWater);
            node.AddValue("lastOxygen", lastOxygen);
            node.AddValue("lastElectricity", lastElectricity);

            node.AddValue("remainingFood", remainingFood);
            node.AddValue("remainingWater", remainingWater);
            node.AddValue("remainingOxygen", remainingOxygen);
            node.AddValue("remainingElectricity", remainingElectricity);
            node.AddValue("remainingCO2", remainingCO2);
            node.AddValue("remainingWaste", remainingWaste);
            node.AddValue("remainingWasteWater", remainingWasteWater);

            node.AddValue("maxFood", maxFood);
            node.AddValue("maxWater", maxWater);
            node.AddValue("maxOxygen", maxOxygen);
            node.AddValue("maxElectricity", maxElectricity);

            node.AddValue("estimatedElectricityConsumptionRate", estimatedElectricityConsumptionRate);

            node.AddValue("hibernating", hibernating);
            node.AddValue("recoveryvessel", recoveryvessel);
            node.AddValue("windowOpen", windowOpen);

            return node;
        }

        /// <summary>
        /// Clears the Amounts in VesselInfo as they are about to be recalculated
        /// Includes the number of crew, remaining counters, max counters
        /// </summary>
        public void ClearAmounts()
        {
            numCrew = 0;
            numOccupiedParts = 0;
            remainingFood = 0.0;
            remainingWater = 0.0;
            remainingOxygen = 0.0;
            remainingElectricity = 0.0;
            remainingCO2 = 0.0;
            remainingWaste = 0.0;
            remainingWasteWater = 0.0;
            maxFood = 0.0;
            maxWater = 0.0;
            maxOxygen = 0.0;
            maxElectricity = 0.0;
        }

        public enum Status 
        {
            GOOD = 1,
            LOW = 2,
            CRITICAL = 3
        }
    }
}
