/**
 * LifeSupportController.cs
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

using KSP.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Tac
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    class LifeSupportController : MonoBehaviour
    {
        public static LifeSupportController Instance { get; private set; }

        public Dictionary<string, CrewMemberInfo> knownCrew { get; private set; }
        public Dictionary<Guid, VesselInfo> knownVessels { get; private set; }

        private Settings settings;
        private LifeSupportMonitoringWindow monitoringWindow;
        private SettingsWindow settingsWindow;
        private RosterWindow rosterWindow;
        private Icon<LifeSupportController> icon;
        private string configFilename;

        void Awake()
        {
            Debug.Log("TAC Life Support (LifeSupportController) [" + this.GetInstanceID().ToString("X") + "][" + Time.time + "]: Awake");
            Instance = this;

            knownCrew = new Dictionary<string, CrewMemberInfo>();
            knownVessels = new Dictionary<Guid, VesselInfo>();

            settings = new Settings();
            settingsWindow = new SettingsWindow(settings);
            rosterWindow = new RosterWindow();
            monitoringWindow = new LifeSupportMonitoringWindow(this, settings, settingsWindow, rosterWindow);

            icon = new Icon<LifeSupportController>(new Rect(Screen.width * 0.75f, 0, 32, 32), "icon.png", "LS",
                "Click to show the Life Support Monitoring Window", OnIconClicked);

            configFilename = IOUtils.GetFilePathFor(this.GetType(), "LifeSupport.cfg");
        }

        void Start()
        {
            Debug.Log("TAC Life Support (LifeSupportController) [" + this.GetInstanceID().ToString("X") + "][" + Time.time + "]: Start");
            Load();
            icon.SetVisible(true);
        }

        void OnDestroy()
        {
            Debug.Log("TAC Life Support (LifeSupportController) [" + this.GetInstanceID().ToString("X") + "][" + Time.time + "]: OnDestroy");
            Save();
        }

        void FixedUpdate()
        {
            if (!FlightGlobals.ready)
            {
                return;
            }

            double currentTime = Planetarium.GetUniversalTime();
            var vessels = FlightGlobals.Vessels.Where(v => v.loaded && v.GetCrewCount() > 0);
            foreach (Vessel vessel in vessels)
            {
                if (!vessel.isEVA)
                {
                    VesselInfo vesselInfo = GetVesselInfo(vessel, currentTime);
                    if (vessel.missionTime < 0.05)
                    {
                        // The vessel has not been launched yet
                        foreach (ProtoCrewMember crewMember in vessel.GetVesselCrew())
                        {
                            knownCrew[crewMember.name] = new CrewMemberInfo(crewMember.name, vessel, currentTime);
                        }
                    }
                    else
                    {
                        ConsumeResources(currentTime, vessel, vesselInfo);
                    }
                }
                else
                {
                    if (vessel.missionTime < 0.05)
                    {
                        // It is a new EVA
                        FillEvaSuit(vessel);
                    }
                    else
                    {
                        VesselInfo vesselInfo = GetVesselInfo(vessel, currentTime);
                        ConsumeResources(currentTime, vessel, vesselInfo);
                    }
                }
            }

            // Clean up the mapping of known vessels, removing ones that are not in the list anymore
            var oldVessels = knownVessels.Keys.Where(key => !vessels.Any(v => v.id.Equals(key))).ToList();
            foreach (Guid id in oldVessels)
            {
                knownVessels.Remove(id);
            }
        }

        private void ConsumeResources(double currentTime, Vessel vessel, VesselInfo vesselInfo)
        {
            // Electricity
            ConsumeElectricity(currentTime, vessel, vesselInfo);

            List<ProtoCrewMember> crew = vessel.GetVesselCrew();
            foreach (ProtoCrewMember crewMember in crew)
            {
                if (knownCrew.ContainsKey(crewMember.name))
                {
                    CrewMemberInfo crewMemberInfo = knownCrew[crewMember.name];
                    var part = (crewMember.KerbalRef != null) ? crewMember.KerbalRef.InPart : vessel.rootPart;

                    if (crewMemberInfo.vesselId != vessel.id && crewMemberInfo.isEVA)
                    {
                        // The crewmember came back inside after EVA, return the remaining resources out of the suit
                        EmptyEvaSuit(crewMemberInfo, part);
                    }

                    // Oxygen
                    ConsumeOxygen(currentTime, vessel, vesselInfo, crewMember, crewMemberInfo, part);

                    // Water
                    ConsumeWater(currentTime, vessel, vesselInfo, crewMember, crewMemberInfo, part);

                    // Food
                    ConsumeFood(currentTime, vessel, vesselInfo, crewMember, crewMemberInfo, part);

                    crewMemberInfo.lastUpdate = currentTime;
                    crewMemberInfo.vesselId = vessel.id;
                    crewMemberInfo.isEVA = vessel.isEVA;
                }
                else
                {
                    Debug.Log("TAC Life Support (LifeSupportController) [" + this.GetInstanceID().ToString("X") + "][" + Time.time + "]: Unknown crew member: " + crewMember.name);
                    knownCrew[crewMember.name] = new CrewMemberInfo(crewMember.name, vessel, currentTime);
                }
            }

            vesselInfo.lastUpdate = currentTime;
        }

        private void ConsumeFood(double currentTime, Vessel vessel, VesselInfo vesselInfo, ProtoCrewMember crewMember, CrewMemberInfo crewMemberInfo, Part part)
        {
            if (vesselInfo.remainingFood >= settings.FoodConsumptionRate)
            {
                double desiredFood = settings.FoodConsumptionRate * (currentTime - crewMemberInfo.lastUpdate);
                double foodObtained = RequestResource(settings.Food, Min(desiredFood, vesselInfo.remainingFood / vesselInfo.numCrew, vesselInfo.remainingFood * 0.95), part);

                double wasteProduced = foodObtained * settings.WasteProductionRate / settings.FoodConsumptionRate;
                RequestResource(settings.Waste, -wasteProduced, part);

                crewMemberInfo.lastFood = currentTime - ((desiredFood - foodObtained) / settings.FoodConsumptionRate);
            }
            else
            {
                double timeWithoutFood = currentTime - crewMemberInfo.lastFood;
                if (timeWithoutFood > (settings.MaxTimeWithoutFood + crewMemberInfo.respite))
                {
                    KillCrewMember(crewMember, "starvation", vessel);
                }
            }
        }

        private void ConsumeWater(double currentTime, Vessel vessel, VesselInfo vesselInfo, ProtoCrewMember crewMember, CrewMemberInfo crewMemberInfo, Part part)
        {
            if (vesselInfo.remainingWater >= settings.WaterConsumptionRate)
            {
                double desiredWater = settings.WaterConsumptionRate * (currentTime - crewMemberInfo.lastUpdate);
                double waterObtained = RequestResource(settings.Water, Min(desiredWater, vesselInfo.remainingWater / vesselInfo.numCrew, vesselInfo.remainingWater * 0.95), part);

                double wasteWaterProduced = waterObtained * settings.WasteWaterProductionRate / settings.WaterConsumptionRate;
                RequestResource(settings.WasteWater, -wasteWaterProduced, part);

                crewMemberInfo.lastWater = currentTime - ((desiredWater - waterObtained) / settings.WaterConsumptionRate);
            }
            else
            {
                double timeWithoutWater = currentTime - crewMemberInfo.lastWater;
                if (timeWithoutWater > (settings.MaxTimeWithoutWater + crewMemberInfo.respite))
                {
                    KillCrewMember(crewMember, "dehydration", vessel);
                }
            }
        }

        private void ConsumeOxygen(double currentTime, Vessel vessel, VesselInfo vesselInfo, ProtoCrewMember crewMember, CrewMemberInfo crewMemberInfo, Part part)
        {
            if (!vessel.orbit.referenceBody.atmosphereContainsOxygen || FlightGlobals.getStaticPressure() < 0.2)
            {
                if (vesselInfo.remainingOxygen >= settings.OxygenConsumptionRate)
                {
                    double desiredOxygen = settings.OxygenConsumptionRate * (currentTime - crewMemberInfo.lastUpdate);
                    double oxygenObtained = RequestResource(settings.Oxygen, Min(desiredOxygen, vesselInfo.remainingOxygen / vesselInfo.numCrew, vesselInfo.remainingOxygen * 0.95), part);

                    double co2Production = oxygenObtained * settings.CO2ProductionRate / settings.OxygenConsumptionRate;
                    RequestResource(settings.CO2, -co2Production, part);

                    crewMemberInfo.lastOxygen = currentTime - ((desiredOxygen - oxygenObtained) / settings.OxygenConsumptionRate);
                }
                else
                {
                    double timeWithoutOxygen = currentTime - crewMemberInfo.lastOxygen;
                    if (timeWithoutOxygen > (settings.MaxTimeWithoutOxygen + crewMemberInfo.respite))
                    {
                        KillCrewMember(crewMember, "oxygen deprivation", vessel);
                    }
                }
            }
            else
            {
                crewMemberInfo.lastOxygen = currentTime;
            }
        }

        private void ConsumeElectricity(double currentTime, Vessel vessel, VesselInfo vesselInfo)
        {
            double rate = CalculateElectricityConsumptionRate(vessel, vesselInfo);
            if (vesselInfo.remainingElectricity >= rate)
            {
                double desiredElectricity = rate * TimeWarp.fixedDeltaTime;
                double electricityObtained = RequestResource(settings.Electricity, Min(desiredElectricity, vesselInfo.remainingElectricity * 0.95), vessel.rootPart);

                vesselInfo.lastElectricity = currentTime - ((desiredElectricity - electricityObtained) / rate);
            }
            else
            {
                double timeWithoutElectricity = currentTime - vesselInfo.lastElectricity;
                if (timeWithoutElectricity > settings.MaxTimeWithoutElectricity)
                {
                    List<ProtoCrewMember> crew = vessel.GetVesselCrew();
                    int crewMemberIndex = UnityEngine.Random.Range(0, crew.Count - 1);
                    KillCrewMember(crew[crewMemberIndex], "heat/cold/air stagnation", vessel);

                    vesselInfo.lastElectricity += UnityEngine.Random.Range(60, 180);
                }
            }
        }

        private VesselInfo GetVesselInfo(Vessel vessel, double currentTime)
        {
            VesselInfo vesselInfo;
            if (knownVessels.ContainsKey(vessel.id))
            {
                vesselInfo = knownVessels[vessel.id];
            }
            else
            {
                vesselInfo = new VesselInfo(currentTime);
                knownVessels[vessel.id] = vesselInfo;
            }

            vesselInfo.numCrew = vessel.GetCrewCount();
            vesselInfo.ClearResourceAmounts();

            foreach (Part part in vessel.parts)
            {
                foreach (PartResource resource in part.Resources)
                {
                    if (resource.info.id == settings.FoodId)
                    {
                        vesselInfo.remainingFood += resource.amount;
                        vesselInfo.maxFood += resource.maxAmount;
                    }
                    else if (resource.info.id == settings.WaterId)
                    {
                        vesselInfo.remainingWater += resource.amount;
                        vesselInfo.maxWater += resource.maxAmount;
                    }
                    else if (resource.info.id == settings.OxygenId)
                    {
                        vesselInfo.remainingOxygen += resource.amount;
                        vesselInfo.maxOxygen += resource.maxAmount;
                    }
                    else if (resource.info.id == settings.ElectricityId)
                    {
                        vesselInfo.remainingElectricity += resource.amount;
                        vesselInfo.maxElectricity += resource.maxAmount;
                    }
                    else if (resource.info.id == settings.CO2Id)
                    {
                        vesselInfo.remainingCO2 += resource.amount;
                    }
                    else if (resource.info.id == settings.WasteId)
                    {
                        vesselInfo.remainingWaste += resource.amount;
                    }
                    else if (resource.info.id == settings.WasteWaterId)
                    {
                        vesselInfo.remainingWasteWater += resource.amount;
                    }
                }
            }

            ShowWarnings(vessel, vesselInfo.remainingFood, vesselInfo.maxFood, settings.FoodConsumptionRate / vesselInfo.numCrew, "Food", ref vesselInfo.foodStatus);
            ShowWarnings(vessel, vesselInfo.remainingWater, vesselInfo.maxWater, settings.WaterConsumptionRate / vesselInfo.numCrew, "Water", ref vesselInfo.waterStatus);
            ShowWarnings(vessel, vesselInfo.remainingOxygen, vesselInfo.maxOxygen, settings.OxygenConsumptionRate / vesselInfo.numCrew, "Oxygen", ref vesselInfo.oxygenStatus);
            ShowWarnings(vessel, vesselInfo.remainingElectricity, vesselInfo.maxElectricity, CalculateElectricityConsumptionRate(vessel, vesselInfo), "Electric Charge", ref vesselInfo.electricityStatus);

            return vesselInfo;
        }

        private void ShowWarnings(Vessel vessel, double resourceRemaining, double max, double consumptionRate, string resourceName, ref VesselInfo.Status status)
        {
            const double multiplier = 1.1;
            const double warningLevel = 0.10;

            int currentWarpRateIndex = TimeWarp.CurrentRateIndex;
            float currentWarpRate = TimeWarp.fetch.warpRates[currentWarpRateIndex];

            if ((resourceRemaining / consumptionRate) < (currentWarpRate * multiplier))
            {
                if (status != VesselInfo.Status.CRITICAL)
                {
                    if (currentWarpRateIndex > 0)
                    {
                        Debug.Log("TAC Life Support (LifeSupportController) [" + this.GetInstanceID().ToString("X") + "][" + Time.time + "]: @CRITICAL, warp rate: " + currentWarpRate);
                        TimeWarp.SetRate(currentWarpRateIndex - 1, false);
                    }
                    else
                    {
                        ScreenMessages.PostScreenMessage(vessel.vesselName + " - " + resourceName + " depleted!", 15.0f, ScreenMessageStyle.UPPER_CENTER);
                        Debug.Log("TAC Life Support (LifeSupportController) [" + this.GetInstanceID().ToString("X") + "][" + Time.time + "]: " + vessel.vesselName + " - " + resourceName + " depleted!");
                        status = VesselInfo.Status.CRITICAL;
                    }
                }
            }
            else if (resourceRemaining < (max * warningLevel))
            {
                if (status != VesselInfo.Status.LOW)
                {
                    if (currentWarpRateIndex > 0)
                    {
                        Debug.Log("TAC Life Support (LifeSupportController) [" + this.GetInstanceID().ToString("X") + "][" + Time.time + "]: @LOW, warp rate: " + currentWarpRate);
                        TimeWarp.SetRate(currentWarpRateIndex - 1, false);
                    }
                    else
                    {
                        ScreenMessages.PostScreenMessage(vessel.vesselName + " - " + resourceName + " is running out!", 15.0f, ScreenMessageStyle.UPPER_CENTER);
                        Debug.Log("TAC Life Support (LifeSupportController) [" + this.GetInstanceID().ToString("X") + "][" + Time.time + "]: " + vessel.vesselName + " - " + resourceName + " is running out!");
                        status = VesselInfo.Status.LOW;
                    }
                }
            }
            else
            {
                status = VesselInfo.Status.GOOD;
            }
        }

        public double CalculateElectricityConsumptionRate(Vessel vessel, VesselInfo vesselInfo)
        {
            if (!vessel.isEVA)
            {
                int numOccupiedParts = vessel.Parts.Count(p => p.protoModuleCrew.Count > 0);
                return (settings.ElectricityConsumptionRate * vesselInfo.numCrew) + (settings.BaseElectricityConsumptionRate * numOccupiedParts);
            }
            else
            {
                return settings.EvaElectricityConsumptionRate;
            }
        }

        private void FillEvaSuit(Vessel evaVessel)
        {
            ProtoCrewMember crewMember = evaVessel.GetVesselCrew()[0];
            CrewMemberInfo crewMemberInfo = knownCrew[crewMember.name];

            if (crewMemberInfo.vesselId != evaVessel.id)
            {
                Debug.Log("TAC Life Support (LifeSupportController) [" + this.GetInstanceID().ToString("X") + "][" + Time.time + "]: Filling EVA suit for " + crewMember.name);

                Vessel lastVessel = FlightGlobals.Vessels.Find(v => v.id.Equals(crewMemberInfo.vesselId));
                VesselInfo lastVesselInfo = knownVessels[crewMemberInfo.vesselId];

                double desiredFood = settings.FoodConsumptionRate * settings.EvaDefaultResourceAmount;
                double desiredWater = settings.WaterConsumptionRate * settings.EvaDefaultResourceAmount;
                double desiredOxygen = settings.OxygenConsumptionRate * settings.EvaDefaultResourceAmount;
                double desiredElectricity = settings.EvaElectricityConsumptionRate * settings.EvaDefaultResourceAmount;

                Part oldPart = lastVessel.rootPart;
                int numCrew = lastVessel.GetCrewCount() + 1;

                double foodObtained = RequestResource(settings.Food, Min(desiredFood, lastVesselInfo.remainingFood / numCrew, lastVesselInfo.remainingFood * 0.95), oldPart);
                double waterObtained = RequestResource(settings.Water, Min(desiredWater, lastVesselInfo.remainingWater / numCrew, lastVesselInfo.remainingWater * 0.95), oldPart);
                double oxygenObtained = RequestResource(settings.Oxygen, Min(desiredOxygen, lastVesselInfo.remainingOxygen / numCrew, lastVesselInfo.remainingOxygen * 0.95), oldPart);
                double electricityObtained = RequestResource(settings.Electricity, Min(desiredElectricity, lastVesselInfo.remainingElectricity / numCrew, lastVesselInfo.remainingElectricity * 0.95), oldPart);

                Part newPart = evaVessel.rootPart;
                RequestResource(settings.Food, -foodObtained, newPart);
                RequestResource(settings.Water, -waterObtained, newPart);
                RequestResource(settings.Oxygen, -oxygenObtained, newPart);
                RequestResource(settings.Electricity, -electricityObtained, newPart);

                crewMemberInfo.vesselId = evaVessel.id;
                crewMemberInfo.isEVA = true;
            }
            else
            {
                Debug.Log("TAC Life Support (LifeSupportController) [" + this.GetInstanceID().ToString("X") + "][" + Time.time + "]: EVA suit for " + crewMember.name + " has already been filled.");
            }
        }

        private void EmptyEvaSuit(CrewMemberInfo crewMemberInfo, Part part)
        {
            VesselInfo lastVesselInfo = knownVessels[crewMemberInfo.vesselId];
            RequestResource(settings.Food, -lastVesselInfo.remainingFood, part);
            RequestResource(settings.Water, -lastVesselInfo.remainingWater, part);
            RequestResource(settings.Oxygen, -lastVesselInfo.remainingOxygen, part);
            RequestResource(settings.Electricity, -lastVesselInfo.remainingElectricity, part);
            RequestResource(settings.CO2, -lastVesselInfo.remainingCO2, part);
            RequestResource(settings.Waste, -lastVesselInfo.remainingWaste, part);
            RequestResource(settings.WasteWater, -lastVesselInfo.remainingWasteWater, part);
        }

        private double RequestResource(string resourceName, double requestedAmount, Part part)
        {
            if (part.vessel.isEVA)
            {
                double amount;
                if (requestedAmount >= 0)
                {
                    amount = Math.Min(requestedAmount, part.Resources[resourceName].amount);
                }
                else
                {
                    double remainingCapacity = part.Resources[resourceName].maxAmount - part.Resources[resourceName].amount;
                    amount = -Math.Min(-requestedAmount, remainingCapacity);
                }
                part.Resources[resourceName].amount -= amount;
                return amount;
            }
            else
            {
                return part.RequestResource(resourceName, requestedAmount);
            }
        }

        private void KillCrewMember(ProtoCrewMember crewMember, string causeOfDeath, Vessel vessel)
        {
            TimeWarp.SetRate(0, false);
            if (CameraManager.Instance.currentCameraMode == CameraManager.CameraMode.IVA)
            {
                CameraManager.Instance.SetCameraFlight();
            }

            ScreenMessages.PostScreenMessage(vessel.vesselName + " - " + crewMember.name + " died of " + causeOfDeath + "!", 30.0f, ScreenMessageStyle.UPPER_CENTER);
            Debug.Log("TAC Life Support (LifeSupportController) [" + this.GetInstanceID().ToString("X") + "][" + Time.time + "]: " + vessel.vesselName + " - " + crewMember.name + " died of " + causeOfDeath + "!");

            if (!vessel.isEVA)
            {
                Part part = vessel.Parts.Find(p => p.protoModuleCrew.Contains(crewMember));
                if (part != null)
                {
                    part.RemoveCrewmember(crewMember);
                    crewMember.Die();

                    if (settings.AllowCrewRespawn)
                    {
                        crewMember.StartRespawnPeriod(settings.RespawnDelay);
                    }
                }
            }
            else
            {
                vessel.rootPart.Die();

                if (settings.AllowCrewRespawn)
                {
                    crewMember.StartRespawnPeriod(settings.RespawnDelay);
                }
            }
        }

        public void Load()
        {
            if (File.Exists<LifeSupportController>(configFilename))
            {
                ConfigNode config = ConfigNode.Load(configFilename);
                settings.Load(config);
                icon.Load(config);
                monitoringWindow.Load(config);
                settingsWindow.Load(config);
                rosterWindow.Load(config);
            }
        }

        public void Save()
        {
            ConfigNode config = new ConfigNode();
            settings.Save(config);
            icon.Save(config);
            monitoringWindow.Save(config);
            settingsWindow.Save(config);
            rosterWindow.Save(config);

            config.Save(configFilename);
        }

        private void OnIconClicked()
        {
            monitoringWindow.ToggleVisible();
        }

        private static double Min(double value1, double value2)
        {
            return Math.Min(value1, value2);
        }

        private static double Min(double value1, double value2, double value3)
        {
            return Math.Min(value1, Math.Min(value2, value3));
        }
    }
}
