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

        public Settings settings { get; private set; }
        private LifeSupportMonitoringWindow monitoringWindow;
        private SettingsWindow settingsWindow;
        private RosterWindow rosterWindow;
        private Icon<LifeSupportController> icon;
        private string configFilename;

        void Awake()
        {
            Debug.Log("TAC Life Support (LifeSupportController) [" + this.GetInstanceID().ToString("X") + "][" + Time.time.ToString("0.00") + "]: Awake");
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
            Debug.Log("TAC Life Support (LifeSupportController) [" + this.GetInstanceID().ToString("X") + "][" + Time.time.ToString("0.00") + "]: Start");
            Load();
            icon.SetVisible(true);

            GameEvents.onCrewOnEva.Add(OnCrewOnEva);
            GameEvents.onCrewBoardVessel.Add(OnCrewBoardVessel);
        }

        void OnDestroy()
        {
            Debug.Log("TAC Life Support (LifeSupportController) [" + this.GetInstanceID().ToString("X") + "][" + Time.time.ToString("0.00") + "]: OnDestroy");
            Save();

            GameEvents.onCrewOnEva.Remove(OnCrewOnEva);
            GameEvents.onCrewBoardVessel.Remove(OnCrewBoardVessel);
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
                    if (vessel.missionTime > 0.05)
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

                    ConsumeOxygen(currentTime, vessel, vesselInfo, crewMember, crewMemberInfo, part);
                    ConsumeWater(currentTime, vessel, vesselInfo, crewMember, crewMemberInfo, part);
                    ConsumeFood(currentTime, vessel, vesselInfo, crewMember, crewMemberInfo, part);

                    crewMemberInfo.lastUpdate = currentTime;
                    crewMemberInfo.vesselId = vessel.id;
                    crewMemberInfo.isEVA = vessel.isEVA;
                }
                else
                {
                    Debug.Log("TAC Life Support (LifeSupportController) [" + this.GetInstanceID().ToString("X") + "][" + Time.time.ToString("0.00") + "]: Unknown crew member: " + crewMember.name);
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

            vesselInfo.ClearAmounts();

            foreach (Part part in vessel.parts)
            {
                vesselInfo.numCrew += part.protoModuleCrew.Count;

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

            ShowWarnings(vessel, vesselInfo.remainingFood, vesselInfo.maxFood, settings.FoodConsumptionRate * vesselInfo.numCrew, "Food", ref vesselInfo.foodStatus);
            ShowWarnings(vessel, vesselInfo.remainingWater, vesselInfo.maxWater, settings.WaterConsumptionRate * vesselInfo.numCrew, "Water", ref vesselInfo.waterStatus);
            ShowWarnings(vessel, vesselInfo.remainingOxygen, vesselInfo.maxOxygen, settings.OxygenConsumptionRate * vesselInfo.numCrew, "Oxygen", ref vesselInfo.oxygenStatus);
            ShowWarnings(vessel, vesselInfo.remainingElectricity, vesselInfo.maxElectricity, CalculateElectricityConsumptionRate(vessel, vesselInfo), "Electric Charge", ref vesselInfo.electricityStatus);

            return vesselInfo;
        }

        private void ShowWarnings(Vessel vessel, double resourceRemaining, double max, double consumptionRate, string resourceName, ref VesselInfo.Status status)
        {
            double criticalLevel = consumptionRate; // 1 second
            double warningLevel = max * 0.10; // 10%

            if (resourceRemaining < criticalLevel)
            {
                if (status != VesselInfo.Status.CRITICAL)
                {
                    ScreenMessages.PostScreenMessage(vessel.vesselName + " - " + resourceName + " depleted!", 15.0f, ScreenMessageStyle.UPPER_CENTER);
                    Debug.Log("TAC Life Support (LifeSupportController) [" + this.GetInstanceID().ToString("X") + "][" + Time.time.ToString("0.00") + "]: " + vessel.vesselName + " - " + resourceName + " depleted!");
                    status = VesselInfo.Status.CRITICAL;
                    TimeWarp.SetRate(0, false);
                }
            }
            else
            {
                if (resourceRemaining < warningLevel)
                {
                    if (status == VesselInfo.Status.CRITICAL)
                    {
                        status = VesselInfo.Status.LOW;
                    }
                    else if (status != VesselInfo.Status.LOW)
                    {
                        ScreenMessages.PostScreenMessage(vessel.vesselName + " - " + resourceName + " is running out!", 15.0f, ScreenMessageStyle.UPPER_CENTER);
                        Debug.Log("TAC Life Support (LifeSupportController) [" + this.GetInstanceID().ToString("X") + "][" + Time.time.ToString("0.00") + "]: " + vessel.vesselName + " - " + resourceName + " is running out!");
                        status = VesselInfo.Status.LOW;
                        TimeWarp.SetRate(0, false);
                    }
                }
                else
                {
                    status = VesselInfo.Status.GOOD;
                }
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

        private void FillEvaSuit(Part oldPart, Part newPart)
        {
            double desiredFood = settings.FoodConsumptionRate * settings.EvaDefaultResourceAmount;
            double desiredWater = settings.WaterConsumptionRate * settings.EvaDefaultResourceAmount;
            double desiredOxygen = settings.OxygenConsumptionRate * settings.EvaDefaultResourceAmount;
            double desiredElectricity = settings.EvaElectricityConsumptionRate * settings.EvaDefaultResourceAmount;

            VesselInfo lastVesselInfo = GetVesselInfo(oldPart.vessel, Planetarium.GetUniversalTime());
            int numCrew = lastVesselInfo.numCrew + 1;

            double foodObtained = oldPart.TakeResource(settings.FoodId, Min(desiredFood, lastVesselInfo.remainingFood / numCrew));
            double waterObtained = oldPart.TakeResource(settings.WaterId, Min(desiredWater, lastVesselInfo.remainingWater / numCrew));
            double oxygenObtained = oldPart.TakeResource(settings.OxygenId, Min(desiredOxygen, lastVesselInfo.remainingOxygen / numCrew));
            double electricityObtained = oldPart.TakeResource(settings.ElectricityId, Min(desiredElectricity, lastVesselInfo.remainingElectricity / numCrew));

            newPart.TakeResource(settings.FoodId, -foodObtained);
            newPart.TakeResource(settings.WaterId, -waterObtained);
            newPart.TakeResource(settings.OxygenId, -oxygenObtained);
            newPart.TakeResource(settings.ElectricityId, -electricityObtained);
        }

        private void EmptyEvaSuit(Part oldPart, Part newPart)
        {
            VesselInfo lastVesselInfo = knownVessels[oldPart.vessel.id];
            newPart.TakeResource(settings.FoodId, -lastVesselInfo.remainingFood);
            newPart.TakeResource(settings.WaterId, -lastVesselInfo.remainingWater);
            newPart.TakeResource(settings.OxygenId, -lastVesselInfo.remainingOxygen);
            newPart.TakeResource(settings.ElectricityId, -lastVesselInfo.remainingElectricity);
            newPart.TakeResource(settings.CO2Id, -lastVesselInfo.remainingCO2);
            newPart.TakeResource(settings.WasteId, -lastVesselInfo.remainingWaste);
            newPart.TakeResource(settings.WasteWaterId, -lastVesselInfo.remainingWasteWater);
        }

        private double RequestResource(string resourceName, double requestedAmount, Part part)
        {
            return part.TakeResource(resourceName, requestedAmount);
        }

        private void KillCrewMember(ProtoCrewMember crewMember, string causeOfDeath, Vessel vessel)
        {
            TimeWarp.SetRate(0, false);
            if (CameraManager.Instance.currentCameraMode == CameraManager.CameraMode.IVA)
            {
                CameraManager.Instance.SetCameraFlight();
            }

            string vesselName = (!vessel.isEVA) ? vessel.vesselName + " - " : "";
            ScreenMessages.PostScreenMessage(vesselName + crewMember.name + " died of " + causeOfDeath + "!", 30.0f, ScreenMessageStyle.UPPER_CENTER);
            Debug.Log("TAC Life Support (LifeSupportController) [" + this.GetInstanceID().ToString("X") + "][" + Time.time.ToString("0.00") + "]: " + vessel.vesselName + " - " + crewMember.name + " died of " + causeOfDeath + "!");

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

        private void OnCrewOnEva(GameEvents.FromToAction<Part, Part> action)
        {
            Debug.Log("TAC Life Support (LifeSupportController) [" + this.GetInstanceID().ToString("X")
                + "][" + Time.time.ToString("0.00") + "]: OnCrewOnEva: from=" + action.from.partInfo.title + "(" + action.from.vessel.vesselName + ")"
                + ", to=" + action.to.partInfo.title + "(" + action.to.vessel.vesselName + ")");
            FillEvaSuit(action.from, action.to);
        }

        private void OnCrewBoardVessel(GameEvents.FromToAction<Part, Part> action)
        {
            Debug.Log("TAC Life Support (LifeSupportController) [" + this.GetInstanceID().ToString("X")
                + "][" + Time.time.ToString("0.00") + "]: OnCrewBoardVessel: from=" + action.from.partInfo.title + "(" + action.from.vessel.vesselName + ")"
                + ", to=" + action.to.partInfo.title + "(" + action.to.vessel.vesselName + ")");
            EmptyEvaSuit(action.from, action.to);
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
