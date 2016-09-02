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
using KSP.UI.Screens;
using RSTUtils;
using UnityEngine;
using TacDFWrapper;

namespace Tac
{
    class LifeSupportController : MonoBehaviour, Savable
    {
       // private TacLifeSupport.Instance.globalSettings TacLifeSupport.Instance.globalSettings;
       // private TacTacLifeSupport.Instance.gameSettings TacLifeSupport.Instance.gameSettings;
        private LifeSupportMonitoringWindow monitoringWindow;
        private RosterWindow rosterWindow;
        internal AppLauncherToolBar TACMenuAppLToolBar;
        private bool loadingNewScene = false;
        private double seaLevelPressure = 101.325;
        private bool IsDFInstalled = false;

        void Awake()
        {
            this.Log("Awake");
            //globalSettings = TacLifeSupport.Instance.TacLifeSupport.Instance.globalSettings;
            //gameSettings = TacLifeSupport.Instance.TacLifeSupport.Instance.gameSettings;
            
            TACMenuAppLToolBar = new AppLauncherToolBar("TACLifeSupport", "TAC Life Support",
                Textures.PathToolbarIconsPath + "/TACgreenIconTB",
                ApplicationLauncher.AppScenes.TRACKSTATION | ApplicationLauncher.AppScenes.FLIGHT,
                (Texture)Textures.GrnApplauncherIcon, (Texture)Textures.GrnApplauncherIcon,
                GameScenes.TRACKSTATION , GameScenes.FLIGHT);

            rosterWindow = new RosterWindow(TACMenuAppLToolBar, TacLifeSupport.Instance.globalSettings, TacLifeSupport.Instance.gameSettings);
            monitoringWindow = new LifeSupportMonitoringWindow(TACMenuAppLToolBar, this, TacLifeSupport.Instance.globalSettings, TacLifeSupport.Instance.gameSettings, rosterWindow);
            
            //Check if DeepFreeze is installed and set bool.
            var DeepFreezeassembly = (from a in AppDomain.CurrentDomain.GetAssemblies()
                    where a.FullName.StartsWith("DeepFreeze")
                    select a).FirstOrDefault();
            if (DeepFreezeassembly != null)
            {
                IsDFInstalled = true;
            }
            else
            {
                IsDFInstalled = false;
            }
        }

        void Start()
        {
            this.Log("Start");
            if (TacLifeSupport.Instance.gameSettings.Enabled)
            {
                if (!ToolbarManager.ToolbarAvailable && !TacLifeSupport.Instance.gameSettings.UseAppLauncher)
                {
                    TacLifeSupport.Instance.gameSettings.UseAppLauncher = true;
                }

                TACMenuAppLToolBar.Start(TacLifeSupport.Instance.gameSettings.UseAppLauncher);

                RSTUtils.Utilities.setScaledScreen();

                var crew = HighLogic.CurrentGame.CrewRoster.Crew;
                var knownCrew = TacLifeSupport.Instance.gameSettings.knownCrew;
                foreach (ProtoCrewMember crewMember in crew)
                {
                    if (crewMember.rosterStatus != ProtoCrewMember.RosterStatus.Assigned && knownCrew.ContainsKey(crewMember.name))
                    {
                        this.Log("Deleting crew member: " + crewMember.name);
                        knownCrew.Remove(crewMember.name);
                    }
                }

                GameEvents.onCrewOnEva.Add(OnCrewOnEva);
                GameEvents.onCrewBoardVessel.Add(OnCrewBoardVessel);
                GameEvents.onGameSceneLoadRequested.Add(OnGameSceneLoadRequested);

                // Double check that we have the right sea level pressure for Kerbin
                seaLevelPressure = FlightGlobals.Bodies[1].GetPressure(0);
            }
            else
            {
                TACMenuAppLToolBar.Destroy();
                monitoringWindow.SetVisible(false);
                Destroy(this);
            }
        }

        void OnDestroy()
        {
            this.Log("OnDestroy");
            TACMenuAppLToolBar.Destroy();
            GameEvents.onCrewOnEva.Remove(OnCrewOnEva);
            GameEvents.onCrewBoardVessel.Remove(OnCrewBoardVessel);
            GameEvents.onGameSceneLoadRequested.Remove(OnGameSceneLoadRequested);
        }

        void OnGUI()
        {
            //rosterWindow.SetVisible(TACMenuAppLToolBar.GuiVisible);
            monitoringWindow.SetVisible(TACMenuAppLToolBar.GuiVisible);
            if (!TACMenuAppLToolBar.GuiVisible && rosterWindow.IsVisible())
            {
                rosterWindow.SetVisible(false);
            }
            rosterWindow?.OnGUI();
            monitoringWindow?.OnGUI();
        }

        void FixedUpdate()
        {
            if (Time.timeSinceLevelLoad < 1.0f || loadingNewScene)
            {
                return;
            }

            // If DeepFreeze is installed do DeepFreeze processing to remove frozen kerbals from our list.
            if (IsDFInstalled)
            {
                if (!DFWrapper.InstanceExists)  // Check if DFWrapper has been initialized or not. If not try to initialize.
                {
                    DFWrapper.InitDFWrapper();
                }
                if (DFWrapper.APIReady)
                {
                    //Check if the DeepFreeze Dictionary contains any Frozen Kerbals in the current Game.
                    //If it does process them.
                    if (DFWrapper.DeepFreezeAPI.FrozenKerbals.Count > 0)
                    {
                        //Remove any Frozen Kerbals from TAC LS tracking.
                        RemoveFrozenKerbals();
                    }
                }
            }

            double currentTime = Planetarium.GetUniversalTime();
            var allVessels = FlightGlobals.Vessels;
            var knownVessels = TacLifeSupport.Instance.gameSettings.knownVessels;

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
                    var crewToDelete = TacLifeSupport.Instance.gameSettings.knownCrew.Where(e => e.Value.vesselId == vesselId).Select(e => e.Key).ToList();
                    foreach (String name in crewToDelete)
                    {
                        this.Log("Deleting crew member: " + name);
                        TacLifeSupport.Instance.gameSettings.knownCrew.Remove(name);
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

                    ConsumeResources(currentTime, vessel, vesselInfo);

                    if (vesselInfo.numCrew > 0)
                    {
                        ShowWarnings(vessel.vesselName, vesselInfo.remainingElectricity, vesselInfo.maxElectricity, vesselInfo.estimatedElectricityConsumptionRate, TacLifeSupport.Instance.globalSettings.Electricity, ref vesselInfo.electricityStatus);
                    }
                }

                if (vesselInfo.numCrew > 0)
                {
                    double foodRate = TacLifeSupport.Instance.globalSettings.FoodConsumptionRate * vesselInfo.numCrew;
                    vesselInfo.estimatedTimeFoodDepleted = vesselInfo.lastFood + (vesselInfo.remainingFood / foodRate);
                    double estimatedFood = vesselInfo.remainingFood - ((currentTime - vesselInfo.lastFood) * foodRate);
                    ShowWarnings(vesselInfo.vesselName, estimatedFood, vesselInfo.maxFood, foodRate, TacLifeSupport.Instance.globalSettings.Food, ref vesselInfo.foodStatus);

                    double waterRate = TacLifeSupport.Instance.globalSettings.WaterConsumptionRate * vesselInfo.numCrew;
                    vesselInfo.estimatedTimeWaterDepleted = vesselInfo.lastWater + (vesselInfo.remainingWater / waterRate);
                    double estimatedWater = vesselInfo.remainingWater - ((currentTime - vesselInfo.lastWater) * waterRate);
                    ShowWarnings(vesselInfo.vesselName, estimatedWater, vesselInfo.maxWater, waterRate, TacLifeSupport.Instance.globalSettings.Water, ref vesselInfo.waterStatus);

                    double oxygenRate = TacLifeSupport.Instance.globalSettings.OxygenConsumptionRate * vesselInfo.numCrew;
                    vesselInfo.estimatedTimeOxygenDepleted = vesselInfo.lastOxygen + (vesselInfo.remainingOxygen / oxygenRate);
                    double estimatedOxygen = vesselInfo.remainingOxygen - ((currentTime - vesselInfo.lastOxygen) * oxygenRate);
                    ShowWarnings(vesselInfo.vesselName, estimatedOxygen, vesselInfo.maxOxygen, oxygenRate, TacLifeSupport.Instance.globalSettings.Oxygen, ref vesselInfo.oxygenStatus);

                    vesselInfo.estimatedTimeElectricityDepleted = vesselInfo.lastElectricity + (vesselInfo.remainingElectricity / vesselInfo.estimatedElectricityConsumptionRate);
                }

            }

            vesselsToDelete.ForEach(id => knownVessels.Remove(id));

            foreach (Vessel vessel in allVessels.Where(v => v.loaded))
            {
                if (!knownVessels.ContainsKey(vessel.id) && vessel.parts.Any(p => p.protoModuleCrew.Count > 0) && IsLaunched(vessel))
                {
                    this.Log("New vessel: " + vessel.vesselName + " (" + vessel.id + ")");
                    var knownCrew = TacLifeSupport.Instance.gameSettings.knownCrew;

                    if (vessel.isEVA)
                    {
                        ProtoCrewMember crewMember = vessel.GetVesselCrew().FirstOrDefault();
                        if (crewMember != null && !knownCrew.ContainsKey(crewMember.name))
                        {
                            FillRescueEvaSuit(vessel);
                        }
                    }

                    VesselInfo vesselInfo = new VesselInfo(vessel.vesselName, currentTime);
                    knownVessels[vessel.id] = vesselInfo;
                    UpdateVesselInfo(vesselInfo, vessel);

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

        private void RemoveFrozenKerbals()
        {
            try
            {
                foreach (KeyValuePair<string, DFWrapper.KerbalInfo> frznCrew in DFWrapper.DeepFreezeAPI.FrozenKerbals)
                {
                    if (TacLifeSupport.Instance.gameSettings.knownCrew.ContainsKey(frznCrew.Key))
                    {
                        this.Log("Deleting Frozen crew member: " + frznCrew.Key);
                        TacLifeSupport.Instance.gameSettings.knownCrew.Remove(frznCrew.Key);
                    }
                }
            }
            catch (Exception ex)
            {
                this.Log("Error attempting to check DeepFreeze for FrozenKerbals");
                this.Log(ex.Message);
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
            var knownCrew = TacLifeSupport.Instance.gameSettings.knownCrew;
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
            vesselInfo.vesselType = vessel.vesselType;
        }

        private void ConsumeFood(double currentTime, Vessel vessel, VesselInfo vesselInfo, ProtoCrewMember crewMember, CrewMemberInfo crewMemberInfo, Part part)
        {
            if (vesselInfo.remainingFood >= TacLifeSupport.Instance.globalSettings.FoodConsumptionRate)
            {
                double deltaTime = Math.Min(currentTime - crewMemberInfo.lastFood, TacLifeSupport.Instance.globalSettings.MaxDeltaTime);
                double desiredFood = TacLifeSupport.Instance.globalSettings.FoodConsumptionRate * deltaTime;
                double foodObtained = part.TakeResource(TacLifeSupport.Instance.globalSettings.FoodId, Math.Min(desiredFood, vesselInfo.remainingFood / vesselInfo.numCrew));

                double wasteProduced = foodObtained * TacLifeSupport.Instance.globalSettings.WasteProductionRate / TacLifeSupport.Instance.globalSettings.FoodConsumptionRate;
                part.TakeResource(TacLifeSupport.Instance.globalSettings.WasteId, -wasteProduced);

                crewMemberInfo.lastFood += deltaTime - ((desiredFood - foodObtained) / TacLifeSupport.Instance.globalSettings.FoodConsumptionRate);
                crewMemberInfo.hibernating = false;
            }
            else
            {
                double timeWithoutFood = currentTime - crewMemberInfo.lastFood;
                if (timeWithoutFood > (TacLifeSupport.Instance.globalSettings.MaxTimeWithoutFood + crewMemberInfo.respite))
                {
                    if (!TacLifeSupport.Instance.gameSettings.HibernateInsteadOfKill)
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
            if (vesselInfo.remainingWater >= TacLifeSupport.Instance.globalSettings.WaterConsumptionRate)
            {
                double deltaTime = Math.Min(currentTime - crewMemberInfo.lastWater, TacLifeSupport.Instance.globalSettings.MaxDeltaTime);
                double desiredWater = TacLifeSupport.Instance.globalSettings.WaterConsumptionRate * deltaTime;
                double waterObtained = part.TakeResource(TacLifeSupport.Instance.globalSettings.WaterId, Math.Min(desiredWater, vesselInfo.remainingWater / vesselInfo.numCrew));

                double wasteWaterProduced = waterObtained * TacLifeSupport.Instance.globalSettings.WasteWaterProductionRate / TacLifeSupport.Instance.globalSettings.WaterConsumptionRate;
                part.TakeResource(TacLifeSupport.Instance.globalSettings.WasteWaterId, -wasteWaterProduced);

                crewMemberInfo.lastWater += deltaTime - ((desiredWater - waterObtained) / TacLifeSupport.Instance.globalSettings.WaterConsumptionRate);
                crewMemberInfo.hibernating = false;
            }
            else
            {
                double timeWithoutWater = currentTime - crewMemberInfo.lastWater;
                if (timeWithoutWater > (TacLifeSupport.Instance.globalSettings.MaxTimeWithoutWater + crewMemberInfo.respite))
                {
                    if (!TacLifeSupport.Instance.gameSettings.HibernateInsteadOfKill)
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
                if (vesselInfo.numCrew > 0)
                {
                    if (vesselInfo.remainingOxygen >= TacLifeSupport.Instance.globalSettings.OxygenConsumptionRate)
                    {
                        double deltaTime = Math.Min(currentTime - vesselInfo.lastOxygen, TacLifeSupport.Instance.globalSettings.MaxDeltaTime);
                        double rate = TacLifeSupport.Instance.globalSettings.OxygenConsumptionRate * vesselInfo.numCrew;
                        double desiredOxygen = rate * deltaTime;
                        double oxygenObtained = vessel.rootPart.TakeResource(TacLifeSupport.Instance.globalSettings.OxygenId, desiredOxygen);

                        double co2Production = oxygenObtained * TacLifeSupport.Instance.globalSettings.CO2ProductionRate / TacLifeSupport.Instance.globalSettings.OxygenConsumptionRate;
                        vessel.rootPart.TakeResource(TacLifeSupport.Instance.globalSettings.CO2Id, -co2Production);

                        vesselInfo.lastOxygen += deltaTime - ((desiredOxygen - oxygenObtained) / rate);
                    }
                    else
                    {
                        double timeWithoutOxygen = currentTime - vesselInfo.lastOxygen;
                        if (timeWithoutOxygen > TacLifeSupport.Instance.globalSettings.MaxTimeWithoutOxygen)
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
                    vesselInfo.lastOxygen += currentTime - vesselInfo.lastUpdate;
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
                    double deltaTime = Math.Min(currentTime - vesselInfo.lastElectricity, Math.Max(TacLifeSupport.Instance.globalSettings.ElectricityMaxDeltaTime, TimeWarp.fixedDeltaTime));
                    double desiredElectricity = rate * deltaTime;
                    double electricityObtained = vessel.rootPart.TakeResource(TacLifeSupport.Instance.globalSettings.ElectricityId, desiredElectricity);

                    vesselInfo.lastElectricity = currentTime - ((desiredElectricity - electricityObtained) / rate);
                }
                else if (NeedElectricity(vessel, vesselInfo))
                {
                    double timeWithoutElectricity = currentTime - vesselInfo.lastElectricity;
                    if (timeWithoutElectricity > TacLifeSupport.Instance.globalSettings.MaxTimeWithoutElectricity)
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
                if (part.protoModuleCrew.Count > 0)
                {
                    vesselInfo.numCrew += part.protoModuleCrew.Count;
                    ++vesselInfo.numOccupiedParts;
                }

                foreach (PartResource resource in part.Resources)
                {
                    if (resource.flowState)
                    {
                        if (resource.info.id == TacLifeSupport.Instance.globalSettings.FoodId)
                        {
                            vesselInfo.remainingFood += resource.amount;
                            vesselInfo.maxFood += resource.maxAmount;
                        }
                        else if (resource.info.id == TacLifeSupport.Instance.globalSettings.WaterId)
                        {
                            vesselInfo.remainingWater += resource.amount;
                            vesselInfo.maxWater += resource.maxAmount;
                        }
                        else if (resource.info.id == TacLifeSupport.Instance.globalSettings.OxygenId)
                        {
                            vesselInfo.remainingOxygen += resource.amount;
                            vesselInfo.maxOxygen += resource.maxAmount;
                        }
                        else if (resource.info.id == TacLifeSupport.Instance.globalSettings.ElectricityId)
                        {
                            vesselInfo.remainingElectricity += resource.amount;
                            vesselInfo.maxElectricity += resource.maxAmount;
                        }
                        else if (resource.info.id == TacLifeSupport.Instance.globalSettings.CO2Id)
                        {
                            vesselInfo.remainingCO2 += resource.amount;
                        }
                        else if (resource.info.id == TacLifeSupport.Instance.globalSettings.WasteId)
                        {
                            vesselInfo.remainingWaste += resource.amount;
                        }
                        else if (resource.info.id == TacLifeSupport.Instance.globalSettings.WasteWaterId)
                        {
                            vesselInfo.remainingWasteWater += resource.amount;
                        }
                    }
                }
            }

            return crewCapacity;
        }

        private void ShowWarnings(string vesselName, double resourceRemaining, double max, double rate, string resourceName, ref VesselInfo.Status status)
        {
            double criticalLevel = rate; // 1 second of resources
            double warningLevel = max * 0.10; // 10% full

            if (resourceRemaining < criticalLevel)
            {
                if (status != VesselInfo.Status.CRITICAL)
                {
                    ScreenMessages.PostScreenMessage(vesselName + " - " + resourceName + " depleted!", 10.0f, ScreenMessageStyle.UPPER_CENTER);
                    this.Log(vesselName + " - " + resourceName + " depleted!");
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
                    ScreenMessages.PostScreenMessage(vesselName + " - " + resourceName + " is running out!", 10.0f, ScreenMessageStyle.UPPER_CENTER);
                    this.Log(vesselName + " - " + resourceName + " is running out!");
                    status = VesselInfo.Status.LOW;
                    TimeWarp.SetRate(0, false);
                }
            }
            else
            {
                status = VesselInfo.Status.GOOD;
            }
            //Set Icon Colour
            //if toolbar
            if (TACMenuAppLToolBar.usingToolBar)
            {
                string iconToSet = "/TACgreenIconTB";
                if (status == VesselInfo.Status.LOW)
                {
                    iconToSet = "TACyellowIconTB";
                }
                else
                {
                    if (status == VesselInfo.Status.CRITICAL)
                    {
                        iconToSet = "TACredIconTB";
                    }
                }
                TACMenuAppLToolBar.setToolBarTexturePath(Textures.PathToolbarIconsPath + iconToSet);
            }
            else
            {
                if (TACMenuAppLToolBar.StockButtonNotNull)
                {
                    Texture iconToSet = Textures.GrnApplauncherIcon;
                    if (status == VesselInfo.Status.LOW)
                    {
                        iconToSet = Textures.YlwApplauncherIcon;
                    }
                    else
                    {
                        if (status == VesselInfo.Status.CRITICAL)
                        {
                            iconToSet = Textures.RedApplauncherIcon;
                        }
                    }

                    TACMenuAppLToolBar.setAppLauncherTexture(iconToSet);
                }
            }
        }

        private double CalculateElectricityConsumptionRate(Vessel vessel, VesselInfo vesselInfo)
        {
            if (!vessel.isEVA)
            {
                return (TacLifeSupport.Instance.globalSettings.ElectricityConsumptionRate * vesselInfo.numCrew) + (TacLifeSupport.Instance.globalSettings.BaseElectricityConsumptionRate * vesselInfo.numOccupiedParts);
            }
            else
            {
                return TacLifeSupport.Instance.globalSettings.EvaElectricityConsumptionRate;
            }
        }

        private void FillEvaSuit(Part oldPart, Part newPart)
        {
            if (!newPart.Resources.Contains(TacLifeSupport.Instance.globalSettings.FoodId))
            {
                this.LogError("FillEvaSuit: new part does not have room for a Food resource.");
            }

            double desiredFood = TacLifeSupport.Instance.globalSettings.FoodConsumptionRate * TacLifeSupport.Instance.globalSettings.EvaDefaultResourceAmount;
            double desiredWater = TacLifeSupport.Instance.globalSettings.WaterConsumptionRate * TacLifeSupport.Instance.globalSettings.EvaDefaultResourceAmount;
            double desiredOxygen = TacLifeSupport.Instance.globalSettings.OxygenConsumptionRate * TacLifeSupport.Instance.globalSettings.EvaDefaultResourceAmount;
            double desiredElectricity = TacLifeSupport.Instance.globalSettings.EvaElectricityConsumptionRate * TacLifeSupport.Instance.globalSettings.EvaDefaultResourceAmount;

            Vessel lastVessel = oldPart.vessel;
            VesselInfo lastVesselInfo;
            if (!TacLifeSupport.Instance.gameSettings.knownVessels.TryGetValue(lastVessel.id, out lastVesselInfo))
            {
                this.Log("FillEvaSuit: Unknown vessel: " + lastVessel.vesselName + " (" + lastVessel.id + ")");
                lastVesselInfo = new VesselInfo(lastVessel.vesselName, Planetarium.GetUniversalTime());
            }

            UpdateVesselInfo(lastVesselInfo, lastVessel);
            int numCrew = lastVesselInfo.numCrew + 1;

            double foodObtained = oldPart.TakeResource(TacLifeSupport.Instance.globalSettings.FoodId, Math.Min(desiredFood, lastVesselInfo.remainingFood / numCrew));
            double waterObtained = oldPart.TakeResource(TacLifeSupport.Instance.globalSettings.WaterId, Math.Min(desiredWater, lastVesselInfo.remainingWater / numCrew));
            double oxygenObtained = oldPart.TakeResource(TacLifeSupport.Instance.globalSettings.OxygenId, Math.Min(desiredOxygen, lastVesselInfo.remainingOxygen / numCrew));
            double electricityObtained = oldPart.TakeResource(TacLifeSupport.Instance.globalSettings.ElectricityId, Math.Min(desiredElectricity, lastVesselInfo.remainingElectricity / numCrew));

            newPart.TakeResource(TacLifeSupport.Instance.globalSettings.FoodId, -foodObtained);
            newPart.TakeResource(TacLifeSupport.Instance.globalSettings.WaterId, -waterObtained);
            newPart.TakeResource(TacLifeSupport.Instance.globalSettings.OxygenId, -oxygenObtained);
            newPart.TakeResource(TacLifeSupport.Instance.globalSettings.ElectricityId, -electricityObtained);
        }

        private void FillRescueEvaSuit(Vessel vessel)
        {
            this.Log("FillRescueEvaSuit: Rescue mission EVA: " + vessel.vesselName);
            Part part = vessel.rootPart;

            // Only fill the suit to 30-90% full
            double fillAmount = UnityEngine.Random.Range(0.3f, 0.9f);
            part.AddResource(PartResourceLibrary.Instance.GetDefinition(TacLifeSupport.Instance.globalSettings.ElectricityId).Config);
            part.AddResource(PartResourceLibrary.Instance.GetDefinition(TacLifeSupport.Instance.globalSettings.FoodId).Config);
            part.AddResource(PartResourceLibrary.Instance.GetDefinition(TacLifeSupport.Instance.globalSettings.WaterId).Config);
            part.AddResource(PartResourceLibrary.Instance.GetDefinition(TacLifeSupport.Instance.globalSettings.OxygenId).Config);
            
            part.TakeResource(TacLifeSupport.Instance.globalSettings.ElectricityId, -fillAmount * TacLifeSupport.Instance.globalSettings.EvaElectricityConsumptionRate * TacLifeSupport.Instance.globalSettings.EvaDefaultResourceAmount);
            part.TakeResource(TacLifeSupport.Instance.globalSettings.FoodId, -fillAmount * TacLifeSupport.Instance.globalSettings.FoodConsumptionRate * TacLifeSupport.Instance.globalSettings.EvaDefaultResourceAmount);
            part.TakeResource(TacLifeSupport.Instance.globalSettings.WaterId, -fillAmount * TacLifeSupport.Instance.globalSettings.WaterConsumptionRate * TacLifeSupport.Instance.globalSettings.EvaDefaultResourceAmount);
            part.TakeResource(TacLifeSupport.Instance.globalSettings.OxygenId, -fillAmount * TacLifeSupport.Instance.globalSettings.OxygenConsumptionRate * TacLifeSupport.Instance.globalSettings.EvaDefaultResourceAmount);
        }

        private void EmptyEvaSuit(Part oldPart, Part newPart)
        {
            Vessel lastVessel = oldPart.vessel;
            VesselInfo lastVesselInfo;
            if (!TacLifeSupport.Instance.gameSettings.knownVessels.TryGetValue(lastVessel.id, out lastVesselInfo))
            {
                ScreenMessages.PostScreenMessage("Error - EmptyEvaSuit - Cannot find VesselInfo for " + oldPart.vessel.id, 10.0f, ScreenMessageStyle.UPPER_CENTER);
                this.LogError("EmptyEvaSuit - Cannot find VesselInfo for " + oldPart.vessel.id);
                return;
            }

            newPart.TakeResource(TacLifeSupport.Instance.globalSettings.FoodId, -lastVesselInfo.remainingFood);
            newPart.TakeResource(TacLifeSupport.Instance.globalSettings.WaterId, -lastVesselInfo.remainingWater);
            newPart.TakeResource(TacLifeSupport.Instance.globalSettings.OxygenId, -lastVesselInfo.remainingOxygen);
            newPart.TakeResource(TacLifeSupport.Instance.globalSettings.ElectricityId, -lastVesselInfo.remainingElectricity);
            newPart.TakeResource(TacLifeSupport.Instance.globalSettings.CO2Id, -lastVesselInfo.remainingCO2);
            newPart.TakeResource(TacLifeSupport.Instance.globalSettings.WasteId, -lastVesselInfo.remainingWaste);
            newPart.TakeResource(TacLifeSupport.Instance.globalSettings.WasteWaterId, -lastVesselInfo.remainingWasteWater);
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
            ScreenMessages.PostScreenMessage(vesselName + crewMember.name + " died of " + causeOfDeath + "!", 15.0f, ScreenMessageStyle.UPPER_CENTER);
            this.Log(vessel.vesselName + " - " + crewMember.name + " died of " + causeOfDeath + "!");

            if (!vessel.isEVA)
            {
                Part part = vessel.Parts.Find(p => p.protoModuleCrew.Contains(crewMember));
                if (part != null)
                {
                    part.RemoveCrewmember(crewMember);
                    crewMember.Die();

                    if (HighLogic.CurrentGame.Parameters.Difficulty.MissingCrewsRespawn)
                    {
                        crewMember.StartRespawnPeriod(TacLifeSupport.Instance.gameSettings.RespawnDelay);
                    }
                }
            }
            else
            {
                vessel.rootPart.Die();

                if (HighLogic.CurrentGame.Parameters.Difficulty.MissingCrewsRespawn)
                {
                    crewMember.StartRespawnPeriod(TacLifeSupport.Instance.gameSettings.RespawnDelay);
                }
            }

            TacLifeSupport.Instance.gameSettings.knownCrew.Remove(crewMember.name);
        }

        public void Load(ConfigNode globalNode)
        {
            monitoringWindow.Load(globalNode);
            rosterWindow.Load(globalNode);
        }

        public void Save(ConfigNode globalNode)
        {
            monitoringWindow.Save(globalNode);
            rosterWindow.Save(globalNode);
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

            // Disable this instance because a new instance will be created after the new scene is loaded
            loadingNewScene = true;
        }

        private bool IsLaunched(Vessel vessel)
        {
            return vessel.missionTime > 0.01 || (Time.timeSinceLevelLoad > 5.0f && vessel.srf_velocity.magnitude > 2.0);
        }

        /*
         * Notes:
         *  Mt Everest is at 8,848 meters (29,029 ft). The air pressure is ~0.33 atm.
         *  Everest Base Camp is at ~5,000 m (16,000 ft), with an air pressure of ~0.5 atm.
         *  0.2 atm is around 12,500 m (41,010.5 ft), close to the maximum altitude most airliners fly.
         * References:
         *  http://en.wikipedia.org/wiki/Mount_Everest
         *  http://en.wikipedia.org/wiki/Effects_of_high_altitude_on_humans
         *  http://www.altitude.org/air_pressure.php
         */
        private bool NeedOxygen(Vessel vessel, VesselInfo vesselInfo)
        {
            // Need oxygen unless:
            // 1) on Kerbin below a reasonable altitude, so they can open a hatch or window or vent
            // 2) flying on Kerbin with electricity for the vents, below a reasonable altitude
            if (vessel.mainBody == FlightGlobals.Bodies[1])
            {
                // On or above Kerbin
                if ((vessel.staticPressurekPa / seaLevelPressure) > 0.5)
                {
                    // air pressure is high enough so they can open a window
                    return false;
                }
                else if ((vessel.staticPressurekPa / seaLevelPressure) > 0.2 && vesselInfo.remainingElectricity > vesselInfo.estimatedElectricityConsumptionRate)
                {
                    // air pressure is high enough & have electricity to run vents
                    return false;
                }
            }

            return true;
        }

        private bool NeedElectricity(Vessel vessel, VesselInfo vesselInfo)
        {
            // Need electricity to survive unless:
            // 1) on Kerbin below a reasonable altitude, so they can open a hatch or window or vent
            if (vessel.mainBody == FlightGlobals.Bodies[1])
            {
                // On or above Kerbin
                if ((vessel.staticPressurekPa / seaLevelPressure) > 0.5)
                {
                    // air pressure is high enough so they can open a window
                    return false;
                }
            }

            return true;
        }
    }
}
