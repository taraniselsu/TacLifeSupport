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
    class LifeSupportController : MonoBehaviour, Savable
    {
        public static LifeSupportController Instance { get; private set; }

        private GlobalSettings globalSettings;
        private GameSettings gameSettings;
        private LifeSupportMonitoringWindow monitoringWindow;
        private RosterWindow rosterWindow;
        private Icon<LifeSupportController> icon;
        private string configFilename;

        void Awake()
        {
            Debug.Log("TAC Life Support (LifeSupportController) [" + this.GetInstanceID().ToString("X") + "][" + Time.time.ToString("0.00") + "]: Awake");
            Instance = this;

            globalSettings = TacLifeSupport.Instance.globalSettings;
            gameSettings = TacLifeSupport.Instance.gameSettings;
            rosterWindow = new RosterWindow();
            monitoringWindow = new LifeSupportMonitoringWindow(this, globalSettings, gameSettings, rosterWindow);

            icon = new Icon<LifeSupportController>(new Rect(Screen.width * 0.75f, 0, 32, 32), "icon.png", "LS",
                "Click to show the Life Support Monitoring Window", OnIconClicked);

            configFilename = IOUtils.GetFilePathFor(this.GetType(), "LifeSupport.cfg");
        }

        void Start()
        {
            Debug.Log("TAC Life Support (LifeSupportController) [" + this.GetInstanceID().ToString("X") + "][" + Time.time.ToString("0.00") + "]: Start");
            if (gameSettings.Enabled)
            {
                icon.SetVisible(true);

                GameEvents.onCrewOnEva.Add(OnCrewOnEva);
                GameEvents.onCrewBoardVessel.Add(OnCrewBoardVessel);
            }
            else
            {
                icon.SetVisible(false);
                monitoringWindow.SetVisible(false);
                Destroy(this);
            }
        }

        void OnDestroy()
        {
            Debug.Log("TAC Life Support (LifeSupportController) [" + this.GetInstanceID().ToString("X") + "][" + Time.time.ToString("0.00") + "]: OnDestroy");

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
                            gameSettings.knownCrew[crewMember.name] = new CrewMemberInfo(crewMember.name, vessel.id, currentTime);
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
            var oldVessels = gameSettings.knownVessels.Keys.Where(key => !vessels.Any(v => v.id.Equals(key))).ToList();
            foreach (Guid id in oldVessels)
            {
                gameSettings.knownVessels.Remove(id);
            }
        }

        private void ConsumeResources(double currentTime, Vessel vessel, VesselInfo vesselInfo)
        {
            // Electricity
            ConsumeElectricity(currentTime, vessel, vesselInfo);

            List<ProtoCrewMember> crew = vessel.GetVesselCrew();
            foreach (ProtoCrewMember crewMember in crew)
            {
                if (gameSettings.knownCrew.ContainsKey(crewMember.name))
                {
                    CrewMemberInfo crewMemberInfo = gameSettings.knownCrew[crewMember.name];
                    var part = (crewMember.KerbalRef != null) ? crewMember.KerbalRef.InPart : vessel.rootPart;

                    ConsumeOxygen(currentTime, vessel, vesselInfo, crewMember, crewMemberInfo, part);
                    ConsumeWater(currentTime, vessel, vesselInfo, crewMember, crewMemberInfo, part);
                    ConsumeFood(currentTime, vessel, vesselInfo, crewMember, crewMemberInfo, part);

                    crewMemberInfo.lastUpdate = currentTime;
                    crewMemberInfo.vesselId = vessel.id;
                }
                else
                {
                    Debug.Log("TAC Life Support (LifeSupportController) [" + this.GetInstanceID().ToString("X") + "][" + Time.time.ToString("0.00") + "]: Unknown crew member: " + crewMember.name);
                    gameSettings.knownCrew[crewMember.name] = new CrewMemberInfo(crewMember.name, vessel.id, currentTime);
                }
            }

            vesselInfo.lastUpdate = currentTime;
        }

        private void ConsumeFood(double currentTime, Vessel vessel, VesselInfo vesselInfo, ProtoCrewMember crewMember, CrewMemberInfo crewMemberInfo, Part part)
        {
            if (vesselInfo.remainingFood >= globalSettings.FoodConsumptionRate)
            {
                double desiredFood = globalSettings.FoodConsumptionRate * (currentTime - crewMemberInfo.lastUpdate);
                double foodObtained = RequestResource(globalSettings.Food, Min(desiredFood, vesselInfo.remainingFood / vesselInfo.numCrew, vesselInfo.remainingFood * 0.95), part);

                double wasteProduced = foodObtained * globalSettings.WasteProductionRate / globalSettings.FoodConsumptionRate;
                RequestResource(globalSettings.Waste, -wasteProduced, part);

                crewMemberInfo.lastFood = currentTime - ((desiredFood - foodObtained) / globalSettings.FoodConsumptionRate);
            }
            else
            {
                double timeWithoutFood = currentTime - crewMemberInfo.lastFood;
                if (timeWithoutFood > (globalSettings.MaxTimeWithoutFood + crewMemberInfo.respite))
                {
                    KillCrewMember(crewMember, "starvation", vessel);
                }
            }
        }

        private void ConsumeWater(double currentTime, Vessel vessel, VesselInfo vesselInfo, ProtoCrewMember crewMember, CrewMemberInfo crewMemberInfo, Part part)
        {
            if (vesselInfo.remainingWater >= globalSettings.WaterConsumptionRate)
            {
                double desiredWater = globalSettings.WaterConsumptionRate * (currentTime - crewMemberInfo.lastUpdate);
                double waterObtained = RequestResource(globalSettings.Water, Min(desiredWater, vesselInfo.remainingWater / vesselInfo.numCrew, vesselInfo.remainingWater * 0.95), part);

                double wasteWaterProduced = waterObtained * globalSettings.WasteWaterProductionRate / globalSettings.WaterConsumptionRate;
                RequestResource(globalSettings.WasteWater, -wasteWaterProduced, part);

                crewMemberInfo.lastWater = currentTime - ((desiredWater - waterObtained) / globalSettings.WaterConsumptionRate);
            }
            else
            {
                double timeWithoutWater = currentTime - crewMemberInfo.lastWater;
                if (timeWithoutWater > (globalSettings.MaxTimeWithoutWater + crewMemberInfo.respite))
                {
                    KillCrewMember(crewMember, "dehydration", vessel);
                }
            }
        }

        private void ConsumeOxygen(double currentTime, Vessel vessel, VesselInfo vesselInfo, ProtoCrewMember crewMember, CrewMemberInfo crewMemberInfo, Part part)
        {
            if (!vessel.orbit.referenceBody.atmosphereContainsOxygen || FlightGlobals.getStaticPressure() < 0.2)
            {
                if (vesselInfo.remainingOxygen >= globalSettings.OxygenConsumptionRate)
                {
                    double desiredOxygen = globalSettings.OxygenConsumptionRate * (currentTime - crewMemberInfo.lastUpdate);
                    double oxygenObtained = RequestResource(globalSettings.Oxygen, Min(desiredOxygen, vesselInfo.remainingOxygen / vesselInfo.numCrew, vesselInfo.remainingOxygen * 0.95), part);

                    double co2Production = oxygenObtained * globalSettings.CO2ProductionRate / globalSettings.OxygenConsumptionRate;
                    RequestResource(globalSettings.CO2, -co2Production, part);

                    crewMemberInfo.lastOxygen = currentTime - ((desiredOxygen - oxygenObtained) / globalSettings.OxygenConsumptionRate);
                }
                else
                {
                    double timeWithoutOxygen = currentTime - crewMemberInfo.lastOxygen;
                    if (timeWithoutOxygen > (globalSettings.MaxTimeWithoutOxygen + crewMemberInfo.respite))
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
                double electricityObtained = RequestResource(globalSettings.Electricity, Min(desiredElectricity, vesselInfo.remainingElectricity * 0.95), vessel.rootPart);

                vesselInfo.lastElectricity = currentTime - ((desiredElectricity - electricityObtained) / rate);
            }
            else
            {
                double timeWithoutElectricity = currentTime - vesselInfo.lastElectricity;
                if (timeWithoutElectricity > globalSettings.MaxTimeWithoutElectricity)
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
            if (gameSettings.knownVessels.ContainsKey(vessel.id))
            {
                vesselInfo = gameSettings.knownVessels[vessel.id];
            }
            else
            {
                vesselInfo = new VesselInfo(currentTime);
                gameSettings.knownVessels[vessel.id] = vesselInfo;
            }

            vesselInfo.ClearAmounts();

            foreach (Part part in vessel.parts)
            {
                vesselInfo.numCrew += part.protoModuleCrew.Count;

                foreach (PartResource resource in part.Resources)
                {
                    if (resource.info.id == globalSettings.FoodId)
                    {
                        vesselInfo.remainingFood += resource.amount;
                        vesselInfo.maxFood += resource.maxAmount;
                    }
                    else if (resource.info.id == globalSettings.WaterId)
                    {
                        vesselInfo.remainingWater += resource.amount;
                        vesselInfo.maxWater += resource.maxAmount;
                    }
                    else if (resource.info.id == globalSettings.OxygenId)
                    {
                        vesselInfo.remainingOxygen += resource.amount;
                        vesselInfo.maxOxygen += resource.maxAmount;
                    }
                    else if (resource.info.id == globalSettings.ElectricityId)
                    {
                        vesselInfo.remainingElectricity += resource.amount;
                        vesselInfo.maxElectricity += resource.maxAmount;
                    }
                    else if (resource.info.id == globalSettings.CO2Id)
                    {
                        vesselInfo.remainingCO2 += resource.amount;
                    }
                    else if (resource.info.id == globalSettings.WasteId)
                    {
                        vesselInfo.remainingWaste += resource.amount;
                    }
                    else if (resource.info.id == globalSettings.WasteWaterId)
                    {
                        vesselInfo.remainingWasteWater += resource.amount;
                    }
                }
            }

            ShowWarnings(vessel, vesselInfo.remainingFood, vesselInfo.maxFood, globalSettings.FoodConsumptionRate * vesselInfo.numCrew, "Food", ref vesselInfo.foodStatus);
            ShowWarnings(vessel, vesselInfo.remainingWater, vesselInfo.maxWater, globalSettings.WaterConsumptionRate * vesselInfo.numCrew, "Water", ref vesselInfo.waterStatus);
            ShowWarnings(vessel, vesselInfo.remainingOxygen, vesselInfo.maxOxygen, globalSettings.OxygenConsumptionRate * vesselInfo.numCrew, "Oxygen", ref vesselInfo.oxygenStatus);
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
                return (globalSettings.ElectricityConsumptionRate * vesselInfo.numCrew) + (globalSettings.BaseElectricityConsumptionRate * numOccupiedParts);
            }
            else
            {
                return globalSettings.EvaElectricityConsumptionRate;
            }
        }

        private void FillEvaSuit(Part oldPart, Part newPart)
        {
            double desiredFood = globalSettings.FoodConsumptionRate * globalSettings.EvaDefaultResourceAmount;
            double desiredWater = globalSettings.WaterConsumptionRate * globalSettings.EvaDefaultResourceAmount;
            double desiredOxygen = globalSettings.OxygenConsumptionRate * globalSettings.EvaDefaultResourceAmount;
            double desiredElectricity = globalSettings.EvaElectricityConsumptionRate * globalSettings.EvaDefaultResourceAmount;

            VesselInfo lastVesselInfo = GetVesselInfo(oldPart.vessel, Planetarium.GetUniversalTime());
            int numCrew = lastVesselInfo.numCrew + 1;

            double foodObtained = oldPart.TakeResource(globalSettings.FoodId, Min(desiredFood, lastVesselInfo.remainingFood / numCrew));
            double waterObtained = oldPart.TakeResource(globalSettings.WaterId, Min(desiredWater, lastVesselInfo.remainingWater / numCrew));
            double oxygenObtained = oldPart.TakeResource(globalSettings.OxygenId, Min(desiredOxygen, lastVesselInfo.remainingOxygen / numCrew));
            double electricityObtained = oldPart.TakeResource(globalSettings.ElectricityId, Min(desiredElectricity, lastVesselInfo.remainingElectricity / numCrew));

            newPart.TakeResource(globalSettings.FoodId, -foodObtained);
            newPart.TakeResource(globalSettings.WaterId, -waterObtained);
            newPart.TakeResource(globalSettings.OxygenId, -oxygenObtained);
            newPart.TakeResource(globalSettings.ElectricityId, -electricityObtained);
        }

        private void EmptyEvaSuit(Part oldPart, Part newPart)
        {
            VesselInfo lastVesselInfo = gameSettings.knownVessels[oldPart.vessel.id];
            newPart.TakeResource(globalSettings.FoodId, -lastVesselInfo.remainingFood);
            newPart.TakeResource(globalSettings.WaterId, -lastVesselInfo.remainingWater);
            newPart.TakeResource(globalSettings.OxygenId, -lastVesselInfo.remainingOxygen);
            newPart.TakeResource(globalSettings.ElectricityId, -lastVesselInfo.remainingElectricity);
            newPart.TakeResource(globalSettings.CO2Id, -lastVesselInfo.remainingCO2);
            newPart.TakeResource(globalSettings.WasteId, -lastVesselInfo.remainingWaste);
            newPart.TakeResource(globalSettings.WasteWaterId, -lastVesselInfo.remainingWasteWater);
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

                    if (gameSettings.AllowCrewRespawn)
                    {
                        crewMember.StartRespawnPeriod(gameSettings.RespawnDelay);
                    }
                }
            }
            else
            {
                vessel.rootPart.Die();

                if (gameSettings.AllowCrewRespawn)
                {
                    crewMember.StartRespawnPeriod(gameSettings.RespawnDelay);
                }
            }
        }

        public void Load(ConfigNode globalNode)
        {
            icon.Load(globalNode);
            monitoringWindow.Load(globalNode);
            rosterWindow.Load(globalNode);
        }

        public void Save(ConfigNode globalNode)
        {
            icon.Save(globalNode);
            monitoringWindow.Save(globalNode);
            rosterWindow.Save(globalNode);
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
