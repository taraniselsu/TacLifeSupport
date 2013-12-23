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
        private GlobalSettings globalSettings;
        private GameSettings gameSettings;
        private LifeSupportMonitoringWindow monitoringWindow;
        private RosterWindow rosterWindow;
        private ButtonWrapper button;
        private string configFilename;
        private bool loadingNewScene = false;

        void Awake()
        {
            this.Log("Awake");
            globalSettings = TacLifeSupport.Instance.globalSettings;
            gameSettings = TacLifeSupport.Instance.gameSettings;
            rosterWindow = new RosterWindow(globalSettings, gameSettings);
            monitoringWindow = new LifeSupportMonitoringWindow(this, globalSettings, gameSettings, rosterWindow);

            button = new ButtonWrapper(new Rect(Screen.width * 0.75f, 0, 32, 32), "ThunderAerospace/TacLifeSupport/Textures/greenIcon",
                "LS", "TAC Life Support Monitoring Window", OnIconClicked, "FlightIcon");

            configFilename = IOUtils.GetFilePathFor(this.GetType(), "LifeSupport.cfg");
        }

        void Start()
        {
            this.Log("Start");
            if (gameSettings.Enabled)
            {
                button.Visible = true;

                CrewRoster crewRoster = HighLogic.CurrentGame.CrewRoster;
                var knownCrew = gameSettings.knownCrew;
                foreach (ProtoCrewMember crewMember in crewRoster)
                {
                    if (crewMember.rosterStatus != ProtoCrewMember.RosterStatus.ASSIGNED && knownCrew.ContainsKey(crewMember.name))
                    {
                        this.Log("Deleting crew member: " + crewMember.name);
                        knownCrew.Remove(crewMember.name);
                    }
                }

                GameEvents.onCrewOnEva.Add(OnCrewOnEva);
                GameEvents.onCrewBoardVessel.Add(OnCrewBoardVessel);
                GameEvents.onGameSceneLoadRequested.Add(OnGameSceneLoadRequested);
            }
            else
            {
                button.Visible = false;
                monitoringWindow.SetVisible(false);
                Destroy(this);
            }
        }

        void OnDestroy()
        {
            this.Log("OnDestroy");
            button.Destroy();

            GameEvents.onCrewOnEva.Remove(OnCrewOnEva);
            GameEvents.onCrewBoardVessel.Remove(OnCrewBoardVessel);
            GameEvents.onGameSceneLoadRequested.Remove(OnGameSceneLoadRequested);
        }

        void FixedUpdate()
        {
            if (Time.timeSinceLevelLoad < 1.0f || !FlightGlobals.ready || loadingNewScene)
            {
                return;
            }

            double currentTime = Planetarium.GetUniversalTime();
            var allVessels = FlightGlobals.Vessels;
            var knownVessels = gameSettings.knownVessels;

            var vesselsToDelete = new List<Guid>();
            foreach (var entry in knownVessels)
            {
                Guid vesselId = entry.Key;
                VesselInfo vesselInfo = entry.Value;
                Vessel vessel = allVessels.Find(v => v.id == vesselId);

                if (vessel == null)
                {
                    this.Log("Deleting vessel " + vesselInfo.vesselName + " - vessel does not exist anymore");
                    vesselsToDelete.Add(vesselId);
                    var crewToDelete = gameSettings.knownCrew.Where(e => e.Value.vesselId == vesselId).Select(e => e.Key).ToList();
                    foreach (String name in crewToDelete)
                    {
                        this.Log("Deleting crew member: " + name);
                        gameSettings.knownCrew.Remove(name);
                    }
                    continue;
                }

                if (vessel.loaded)
                {
                    int crewCapacity = UpdateVesselInfo(vesselInfo, vessel);

                    if (crewCapacity == 0)
                    {
                        this.Log("Deleting vessel " + vesselInfo.vesselName + " - no crew parts anymore");
                        vesselsToDelete.Add(vesselId);
                        continue;
                    }
                }

                if (vesselInfo.numCrew > 0)
                {
                    double foodRate = globalSettings.FoodConsumptionRate * vesselInfo.numCrew;
                    vesselInfo.estimatedTimeFoodDepleted = vesselInfo.lastFood + (vesselInfo.remainingFood / foodRate);
                    double estimatedFood = vesselInfo.remainingFood - ((currentTime - vesselInfo.lastFood) * foodRate);
                    ShowWarnings(vessel, estimatedFood, vesselInfo.maxFood, foodRate, globalSettings.Food, ref vesselInfo.foodStatus);

                    double waterRate = globalSettings.WaterConsumptionRate * vesselInfo.numCrew;
                    vesselInfo.estimatedTimeWaterDepleted = vesselInfo.lastWater + (vesselInfo.remainingWater / waterRate);
                    double estimatedWater = vesselInfo.remainingWater - ((currentTime - vesselInfo.lastWater) * waterRate);
                    ShowWarnings(vessel, estimatedWater, vesselInfo.maxWater, waterRate, globalSettings.Water, ref vesselInfo.waterStatus);

                    double oxygenRate = globalSettings.OxygenConsumptionRate * vesselInfo.numCrew;
                    vesselInfo.estimatedTimeOxygenDepleted = vesselInfo.lastOxygen + (vesselInfo.remainingOxygen / oxygenRate);
                    double estimatedOxygen = vesselInfo.remainingOxygen - ((currentTime - vesselInfo.lastOxygen) * oxygenRate);
                    ShowWarnings(vessel, estimatedOxygen, vesselInfo.maxOxygen, oxygenRate, globalSettings.Oxygen, ref vesselInfo.oxygenStatus);

                    vesselInfo.estimatedTimeElectricityDepleted = vesselInfo.lastElectricity + (vesselInfo.remainingElectricity / vesselInfo.estimatedElectricityConsumptionRate);
                    if (vessel.loaded)
                    {
                        ShowWarnings(vessel, vesselInfo.remainingElectricity, vesselInfo.maxElectricity, vesselInfo.estimatedElectricityConsumptionRate, globalSettings.Electricity, ref vesselInfo.electricityStatus);
                    }
                }

                if (vessel.loaded)
                {
                    ConsumeResources(currentTime, vessel, vesselInfo);
                }
            }

            vesselsToDelete.ForEach(id => knownVessels.Remove(id));

            foreach (Vessel vessel in allVessels.Where(v => v.loaded))
            {
                if (!knownVessels.ContainsKey(vessel.id) && vessel.parts.Any(p => p.protoModuleCrew.Any()) && IsLaunched(vessel))
                {
                    this.Log("New vessel: " + vessel.vesselName + " (" + vessel.id + ")");
                    VesselInfo vesselInfo = new VesselInfo(vessel.vesselName, currentTime);
                    knownVessels[vessel.id] = vesselInfo;
                    UpdateVesselInfo(vesselInfo, vessel);

                    var knownCrew = gameSettings.knownCrew;
                    foreach (ProtoCrewMember crewMember in vessel.GetVesselCrew())
                    {
                        if (knownCrew.ContainsKey(crewMember.name))
                        {
                            CrewMemberInfo crewMemberInfo = knownCrew[crewMember.name];
                            crewMemberInfo.vesselId = vessel.id;
                            crewMemberInfo.vesselName = vessel.vesselName;
                        }
                        else
                        {
                            this.Log("New crew member: " + crewMember.name);
                            knownCrew[crewMember.name] = new CrewMemberInfo(crewMember.name, vessel.vesselName, vessel.id, currentTime);
                        }
                    }
                }
            }
        }

        private void ConsumeResources(double currentTime, Vessel vessel, VesselInfo vesselInfo)
        {
            ConsumeElectricity(currentTime, vessel, vesselInfo);
            ConsumeOxygen(currentTime, vessel, vesselInfo);

            vesselInfo.lastFood = currentTime;
            vesselInfo.lastWater = currentTime;
            vesselInfo.hibernating = false;

            List<ProtoCrewMember> crew = vessel.GetVesselCrew();
            var knownCrew = gameSettings.knownCrew;
            foreach (ProtoCrewMember crewMember in crew)
            {
                if (knownCrew.ContainsKey(crewMember.name))
                {
                    CrewMemberInfo crewMemberInfo = knownCrew[crewMember.name];
                    Part part = (crewMember.KerbalRef != null) ? crewMember.KerbalRef.InPart : vessel.rootPart;

                    ConsumeFood(currentTime, vessel, vesselInfo, crewMember, crewMemberInfo, part);
                    ConsumeWater(currentTime, vessel, vesselInfo, crewMember, crewMemberInfo, part);

                    crewMemberInfo.lastUpdate = currentTime;
                    crewMemberInfo.vesselId = vessel.id;
                    crewMemberInfo.vesselName = (!vessel.isEVA) ? vessel.vesselName : "EVA";

                    if (vesselInfo.lastFood > crewMemberInfo.lastFood)
                    {
                        vesselInfo.lastFood = crewMemberInfo.lastFood;
                    }
                    if (vesselInfo.lastWater > crewMemberInfo.lastWater)
                    {
                        vesselInfo.lastWater = crewMemberInfo.lastWater;
                    }
                    if (crewMemberInfo.hibernating)
                    {
                        vesselInfo.hibernating = true;
                    }
                }
                else
                {
                    this.LogWarning("Unknown crew member: " + crewMember.name);
                    knownCrew[crewMember.name] = new CrewMemberInfo(crewMember.name, vessel.vesselName, vessel.id, currentTime);
                }
            }

            vesselInfo.lastUpdate = currentTime;
            vesselInfo.vesselName = vessel.vesselName;
            vesselInfo.vesselType = vessel.vesselType.ToString();
        }

        private void ConsumeFood(double currentTime, Vessel vessel, VesselInfo vesselInfo, ProtoCrewMember crewMember, CrewMemberInfo crewMemberInfo, Part part)
        {
            if (vesselInfo.remainingFood >= globalSettings.FoodConsumptionRate)
            {
                double deltaTime = Math.Min(currentTime - crewMemberInfo.lastFood, globalSettings.MaxDeltaTime);
                double desiredFood = globalSettings.FoodConsumptionRate * deltaTime;
                double foodObtained = part.TakeResource(globalSettings.FoodId, Math.Min(desiredFood, vesselInfo.remainingFood / vesselInfo.numCrew));

                double wasteProduced = foodObtained * globalSettings.WasteProductionRate / globalSettings.FoodConsumptionRate;
                part.TakeResource(globalSettings.WasteId, -wasteProduced);

                crewMemberInfo.lastFood += deltaTime - ((desiredFood - foodObtained) / globalSettings.FoodConsumptionRate);
                crewMemberInfo.hibernating = false;
            }
            else
            {
                double timeWithoutFood = currentTime - crewMemberInfo.lastFood;
                if (timeWithoutFood > (globalSettings.MaxTimeWithoutFood + crewMemberInfo.respite))
                {
                    if (!gameSettings.HibernateInsteadOfKill)
                    {
                        KillCrewMember(crewMember, "starvation", vessel);
                    }
                    else
                    {
                        crewMemberInfo.hibernating = true;
                    }
                }
            }
        }

        private void ConsumeWater(double currentTime, Vessel vessel, VesselInfo vesselInfo, ProtoCrewMember crewMember, CrewMemberInfo crewMemberInfo, Part part)
        {
            if (vesselInfo.remainingWater >= globalSettings.WaterConsumptionRate)
            {
                double deltaTime = Math.Min(currentTime - crewMemberInfo.lastWater, globalSettings.MaxDeltaTime);
                double desiredWater = globalSettings.WaterConsumptionRate * deltaTime;
                double waterObtained = part.TakeResource(globalSettings.WaterId, Math.Min(desiredWater, vesselInfo.remainingWater / vesselInfo.numCrew));

                double wasteWaterProduced = waterObtained * globalSettings.WasteWaterProductionRate / globalSettings.WaterConsumptionRate;
                part.TakeResource(globalSettings.WasteWaterId, -wasteWaterProduced);

                crewMemberInfo.lastWater += deltaTime - ((desiredWater - waterObtained) / globalSettings.WaterConsumptionRate);
                crewMemberInfo.hibernating = false;
            }
            else
            {
                double timeWithoutWater = currentTime - crewMemberInfo.lastWater;
                if (timeWithoutWater > (globalSettings.MaxTimeWithoutWater + crewMemberInfo.respite))
                {
                    if (!gameSettings.HibernateInsteadOfKill)
                    {
                        KillCrewMember(crewMember, "dehydration", vessel);
                    }
                    else
                    {
                        crewMemberInfo.hibernating = true;
                    }
                }
            }
        }

        private void ConsumeOxygen(double currentTime, Vessel vessel, VesselInfo vesselInfo)
        {
            if (NeedOxygen(vessel, vesselInfo))
            {
                if (vesselInfo.remainingOxygen >= globalSettings.OxygenConsumptionRate)
                {
                    double deltaTime = Math.Min(currentTime - vesselInfo.lastOxygen, globalSettings.MaxDeltaTime);
                    if (vesselInfo.numCrew > 0)
                    {
                        double rate = globalSettings.OxygenConsumptionRate * vesselInfo.numCrew;
                        double desiredOxygen = rate * deltaTime;
                        double oxygenObtained = vessel.rootPart.TakeResource(globalSettings.OxygenId, desiredOxygen);

                        double co2Production = oxygenObtained * globalSettings.CO2ProductionRate / globalSettings.OxygenConsumptionRate;
                        vessel.rootPart.TakeResource(globalSettings.CO2Id, -co2Production);

                        vesselInfo.lastOxygen += deltaTime - ((desiredOxygen - oxygenObtained) / rate);
                    }
                    else
                    {
                        vesselInfo.lastOxygen += currentTime - vesselInfo.lastUpdate;
                    }
                }
                else
                {
                    double timeWithoutOxygen = currentTime - vesselInfo.lastOxygen;
                    if (timeWithoutOxygen > globalSettings.MaxTimeWithoutOxygen)
                    {
                        List<ProtoCrewMember> crew = vessel.GetVesselCrew();
                        int crewMemberIndex = UnityEngine.Random.Range(0, crew.Count - 1);
                        KillCrewMember(crew[crewMemberIndex], "oxygen deprivation", vessel);

                        vesselInfo.lastOxygen += UnityEngine.Random.Range(60, 600);
                    }
                }
            }
            else
            {
                vesselInfo.lastOxygen = currentTime;
            }
        }

        private void ConsumeElectricity(double currentTime, Vessel vessel, VesselInfo vesselInfo)
        {
            double rate = vesselInfo.estimatedElectricityConsumptionRate = CalculateElectricityConsumptionRate(vessel, vesselInfo);
            if (rate > 0.0)
            {
                if (vesselInfo.remainingElectricity >= rate)
                {
                    double deltaTime = Math.Min(currentTime - vesselInfo.lastElectricity, globalSettings.ElectricityMaxDeltaTime);
                    double desiredElectricity = rate * deltaTime;
                    double electricityObtained = vessel.rootPart.TakeResource(globalSettings.ElectricityId, desiredElectricity);

                    vesselInfo.lastElectricity = currentTime - ((desiredElectricity - electricityObtained) / rate);
                }
                else if (NeedElectricity(vessel, vesselInfo))
                {
                    double timeWithoutElectricity = currentTime - vesselInfo.lastElectricity;
                    if (timeWithoutElectricity > globalSettings.MaxTimeWithoutElectricity)
                    {
                        List<ProtoCrewMember> crew = vessel.GetVesselCrew();
                        int crewMemberIndex = UnityEngine.Random.Range(0, crew.Count - 1);
                        KillCrewMember(crew[crewMemberIndex], "air toxicity", vessel);

                        vesselInfo.lastElectricity += UnityEngine.Random.Range(60, 600);
                    }
                }
            }
            else
            {
                vesselInfo.lastElectricity += currentTime - vesselInfo.lastUpdate;
            }
        }

        private int UpdateVesselInfo(VesselInfo vesselInfo, Vessel vessel)
        {
            int crewCapacity = 0;
            vesselInfo.ClearAmounts();

            foreach (Part part in vessel.parts)
            {
                crewCapacity += part.CrewCapacity;
                if (part.protoModuleCrew.Any())
                {
                    vesselInfo.numCrew += part.protoModuleCrew.Count;
                    ++vesselInfo.numOccupiedParts;
                }

                foreach (PartResource resource in part.Resources)
                {
                    if (resource.flowState)
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
            }

            return crewCapacity;
        }

        private void ShowWarnings(Vessel vessel, double resourceRemaining, double max, double rate, string resourceName, ref VesselInfo.Status status)
        {
            double criticalLevel = rate; // 1 second of resources
            double warningLevel = max * 0.10; // 10% full

            if (resourceRemaining < criticalLevel)
            {
                if (status != VesselInfo.Status.CRITICAL)
                {
                    ScreenMessages.PostScreenMessage(vessel.vesselName + " - " + resourceName + " depleted!", 15.0f, ScreenMessageStyle.UPPER_CENTER);
                    this.Log(vessel.vesselName + " - " + resourceName + " depleted!");
                    status = VesselInfo.Status.CRITICAL;
                    TimeWarp.SetRate(0, false);
                }
            }
            else if (resourceRemaining < warningLevel)
            {
                if (status == VesselInfo.Status.CRITICAL)
                {
                    status = VesselInfo.Status.LOW;
                }
                else if (status != VesselInfo.Status.LOW)
                {
                    ScreenMessages.PostScreenMessage(vessel.vesselName + " - " + resourceName + " is running out!", 15.0f, ScreenMessageStyle.UPPER_CENTER);
                    this.Log(vessel.vesselName + " - " + resourceName + " is running out!");
                    status = VesselInfo.Status.LOW;
                    TimeWarp.SetRate(0, false);
                }
            }
            else
            {
                status = VesselInfo.Status.GOOD;
            }
        }

        private double CalculateElectricityConsumptionRate(Vessel vessel, VesselInfo vesselInfo)
        {
            if (!vessel.isEVA)
            {
                return (globalSettings.ElectricityConsumptionRate * vesselInfo.numCrew) + (globalSettings.BaseElectricityConsumptionRate * vesselInfo.numOccupiedParts);
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

            Vessel lastVessel = oldPart.vessel;
            VesselInfo lastVesselInfo;
            if (!gameSettings.knownVessels.TryGetValue(lastVessel.id, out lastVesselInfo))
            {
                lastVesselInfo = new VesselInfo(lastVessel.vesselName, Planetarium.GetUniversalTime());
            }

            UpdateVesselInfo(lastVesselInfo, lastVessel);
            int numCrew = lastVesselInfo.numCrew + 1;

            double foodObtained = oldPart.TakeResource(globalSettings.FoodId, Math.Min(desiredFood, lastVesselInfo.remainingFood / numCrew));
            double waterObtained = oldPart.TakeResource(globalSettings.WaterId, Math.Min(desiredWater, lastVesselInfo.remainingWater / numCrew));
            double oxygenObtained = oldPart.TakeResource(globalSettings.OxygenId, Math.Min(desiredOxygen, lastVesselInfo.remainingOxygen / numCrew));
            double electricityObtained = oldPart.TakeResource(globalSettings.ElectricityId, Math.Min(desiredElectricity, lastVesselInfo.remainingElectricity / numCrew));

            newPart.TakeResource(globalSettings.FoodId, -foodObtained);
            newPart.TakeResource(globalSettings.WaterId, -waterObtained);
            newPart.TakeResource(globalSettings.OxygenId, -oxygenObtained);
            newPart.TakeResource(globalSettings.ElectricityId, -electricityObtained);
        }

        private void EmptyEvaSuit(Part oldPart, Part newPart)
        {
            Vessel lastVessel = oldPart.vessel;
            VesselInfo lastVesselInfo;
            if (!gameSettings.knownVessels.TryGetValue(lastVessel.id, out lastVesselInfo))
            {
                ScreenMessages.PostScreenMessage("Error - EmptyEvaSuit - Cannot find VesselInfo for " + oldPart.vessel.id, 10.0f, ScreenMessageStyle.UPPER_CENTER);
                this.LogError("EmptyEvaSuit - Cannot find VesselInfo for " + oldPart.vessel.id);
                return;
            }

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
            this.Log(vessel.vesselName + " - " + crewMember.name + " died of " + causeOfDeath + "!");

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

            gameSettings.knownCrew.Remove(crewMember.name);
        }

        public void Load(ConfigNode globalNode)
        {
            button.Load(globalNode);
            monitoringWindow.Load(globalNode);
            rosterWindow.Load(globalNode);
        }

        public void Save(ConfigNode globalNode)
        {
            button.Save(globalNode);
            monitoringWindow.Save(globalNode);
            rosterWindow.Save(globalNode);
        }

        private void OnIconClicked()
        {
            monitoringWindow.ToggleVisible();
        }

        private void OnCrewOnEva(GameEvents.FromToAction<Part, Part> action)
        {
            this.Log("OnCrewOnEva: from=" + action.from.partInfo.title + "(" + action.from.vessel.vesselName + ")" + ", to=" + action.to.partInfo.title + "(" + action.to.vessel.vesselName + ")");
            FillEvaSuit(action.from, action.to);
        }

        private void OnCrewBoardVessel(GameEvents.FromToAction<Part, Part> action)
        {
            this.Log("OnCrewBoardVessel: from=" + action.from.partInfo.title + "(" + action.from.vessel.vesselName + ")" + ", to=" + action.to.partInfo.title + "(" + action.to.vessel.vesselName + ")");
            EmptyEvaSuit(action.from, action.to);
        }

        private void OnGameSceneLoadRequested(GameScenes gameScene)
        {
            this.Log("Game scene load requested: " + gameScene);

            // Disable this instance becuase a new instance will be created after the new scene is loaded
            loadingNewScene = true;
        }

        private bool IsLaunched(Vessel vessel)
        {
            return vessel.missionTime > 0.01 || (Time.timeSinceLevelLoad > 5.0f && vessel.srf_velocity.magnitude > 2.0);
        }

        private bool NeedOxygen(Vessel vessel, VesselInfo vesselInfo)
        {
            // Need oxygen unless:
            // 1) landed or splashed down on Kerbin below a reasonable altitude, so they can open a hatch
            // 2) flying on Kerbin with electricity for the vents, below a reasonable altitude
            if (vessel.mainBody == FlightGlobals.Bodies[1])
            {
                // On or above Kerbin
                if (vessel.staticPressure > 0.2 && vesselInfo.remainingElectricity > vesselInfo.estimatedElectricityConsumptionRate)
                {
                    // air pressure is high enough & have electricity to run vents
                    return false;
                }
                else if (vessel.staticPressure > 0.5 && (vessel.situation == Vessel.Situations.LANDED || vessel.situation == Vessel.Situations.SPLASHED || vessel.situation == Vessel.Situations.PRELAUNCH))
                {
                    // air pressure is high enough & landed/spashed so they can open the hatch
                    return false;
                }
            }

            return true;
        }

        private bool NeedElectricity(Vessel vessel, VesselInfo vesselInfo)
        {
            // Need electricity to survive unless:
            // 1) landed or splashed down on Kerbin below a reasonable altitude, so they can open a hatch
            if (vessel.mainBody == FlightGlobals.Bodies[1])
            {
                // On or above Kerbin
                if (vessel.staticPressure > 0.5 && (vessel.situation == Vessel.Situations.LANDED || vessel.situation == Vessel.Situations.SPLASHED || vessel.situation == Vessel.Situations.PRELAUNCH))
                {
                    // air pressure is high enough & landed/spashed so they can open the hatch
                    return false;
                }
            }

            return true;
        }
    }
}
