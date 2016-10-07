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

using System;
using System.Collections.Generic;
using System.Linq;
using KSP.UI.Screens;
using RSTUtils;
using UnityEngine;
using TacDFWrapper;

namespace Tac
{
    class LifeSupportController : MonoBehaviour, Savable
    {
        private LifeSupportMonitoringWindow monitoringWindow;
        private RosterWindow rosterWindow;
        internal AppLauncherToolBar TACMenuAppLToolBar;
        private bool loadingNewScene = false;
        private double seaLevelPressure = 101.325;
        private bool IsDFInstalled = false;
        private GlobalSettings globalsettings;
        TAC_SettingsParms settings_sec1 = HighLogic.CurrentGame.Parameters.CustomParams<TAC_SettingsParms>();
        TAC_SettingsParms_Sec2 settings_sec2 = HighLogic.CurrentGame.Parameters.CustomParams<TAC_SettingsParms_Sec2>();
        TAC_SettingsParms_Sec3 settings_sec3 = HighLogic.CurrentGame.Parameters.CustomParams<TAC_SettingsParms_Sec3>();

        void Awake()
        {
            this.Log("Awake");
            globalsettings = TacStartOnce.globalSettings;
            TACMenuAppLToolBar = new AppLauncherToolBar("TACLifeSupport", "TAC Life Support",
                Textures.PathToolbarIconsPath + "/TACgreenIconTB",
                ApplicationLauncher.AppScenes.TRACKSTATION | ApplicationLauncher.AppScenes.FLIGHT,
                (Texture)Textures.GrnApplauncherIcon, (Texture)Textures.GrnApplauncherIcon,
                GameScenes.TRACKSTATION , GameScenes.FLIGHT);

            
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
            if (rosterWindow == null)
                rosterWindow = new RosterWindow(TACMenuAppLToolBar, globalsettings, TacLifeSupport.Instance.gameSettings);
            if (monitoringWindow == null)
                monitoringWindow = new LifeSupportMonitoringWindow(TACMenuAppLToolBar, this, globalsettings, TacLifeSupport.Instance.gameSettings, rosterWindow);

            if (settings_sec1.enabled)
            {
                if (!ToolbarManager.ToolbarAvailable && !settings_sec1.UseAppLToolbar)
                {
                    settings_sec1.UseAppLToolbar = true;
                }

                TACMenuAppLToolBar.Start(settings_sec1.UseAppLToolbar);

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
            var loadedVessels = FlightGlobals.VesselsLoaded;
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
                        ShowWarnings(vessel.vesselName, vesselInfo.remainingElectricity, vesselInfo.maxElectricity, vesselInfo.estimatedElectricityConsumptionRate, globalsettings.Electricity, ref vesselInfo.electricityStatus);
                    }
                }

                if (vesselInfo.numCrew > 0)
                {
                    double foodRate = settings_sec2.FoodConsumptionRate * vesselInfo.numCrew;
                    vesselInfo.estimatedTimeFoodDepleted = vesselInfo.lastFood + (vesselInfo.remainingFood / foodRate);
                    double estimatedFood = vesselInfo.remainingFood - ((currentTime - vesselInfo.lastFood) * foodRate);
                    ShowWarnings(vesselInfo.vesselName, estimatedFood, vesselInfo.maxFood, foodRate, globalsettings.Food, ref vesselInfo.foodStatus);

                    double waterRate = settings_sec2.WaterConsumptionRate * vesselInfo.numCrew;
                    vesselInfo.estimatedTimeWaterDepleted = vesselInfo.lastWater + (vesselInfo.remainingWater / waterRate);
                    double estimatedWater = vesselInfo.remainingWater - ((currentTime - vesselInfo.lastWater) * waterRate);
                    ShowWarnings(vesselInfo.vesselName, estimatedWater, vesselInfo.maxWater, waterRate, globalsettings.Water, ref vesselInfo.waterStatus);

                    double oxygenRate = settings_sec2.OxygenConsumptionRate * vesselInfo.numCrew;
                    vesselInfo.estimatedTimeOxygenDepleted = vesselInfo.lastOxygen + (vesselInfo.remainingOxygen / oxygenRate);
                    double estimatedOxygen = vesselInfo.remainingOxygen - ((currentTime - vesselInfo.lastOxygen) * oxygenRate);
                    ShowWarnings(vesselInfo.vesselName, estimatedOxygen, vesselInfo.maxOxygen, oxygenRate, globalsettings.Oxygen, ref vesselInfo.oxygenStatus);

                    vesselInfo.estimatedTimeElectricityDepleted = vesselInfo.lastElectricity + (vesselInfo.remainingElectricity / vesselInfo.estimatedElectricityConsumptionRate);
                }

            }

            vesselsToDelete.ForEach(id => knownVessels.Remove(id));

            foreach (Vessel vessel in loadedVessels)
            {
                if (!knownVessels.ContainsKey(vessel.id) && vessel.GetVesselCrew().Count > 0 && IsLaunched(vessel))
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

        private bool CheckFrozenKerbals(string kerbalName)
        {
            try
            {
                if (DFWrapper.DeepFreezeAPI.FrozenKerbals.ContainsKey(kerbalName))
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                this.Log("Error attempting to check for FrozenKerbal: " + kerbalName + " in DeepFreeze");
                this.Log(ex.Message);
                return false;
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
            if (vesselInfo.remainingFood >= settings_sec2.FoodConsumptionRate)
            {
                double deltaTime = Math.Min(currentTime - crewMemberInfo.lastFood, settings_sec3.MaxDeltaTime);
                double desiredFood = settings_sec2.FoodConsumptionRate * deltaTime;
                double foodObtained, foodSpace = 0;
                RSTUtils.Utilities.requireResourceID(vessel, globalsettings.FoodId,
                    Math.Min(desiredFood, vesselInfo.remainingFood/vesselInfo.numCrew), true, true, false, out foodObtained, out foodSpace);
                
                double wasteProduced = foodObtained * settings_sec2.WasteProductionRate / settings_sec2.FoodConsumptionRate;
                double wasteObtained, wasteSpace = 0;
                RSTUtils.Utilities.requireResourceID(vessel, globalsettings.WasteId,
                    -wasteProduced, true, false, false, out wasteObtained, out wasteSpace);
                
                crewMemberInfo.lastFood += deltaTime - ((desiredFood - foodObtained) / settings_sec2.FoodConsumptionRate);

                if (crewMemberInfo.hibernating)
                {
                    this.LogWarning("Removing hibernation status from crew member: " + crewMemberInfo.name);
                    ProtoCrewMember kerbal = HighLogic.CurrentGame.CrewRoster[crewMemberInfo.name]; 
                    if (kerbal != null && kerbal.type != crewMemberInfo.crewType)
                    {
                        if (IsDFInstalled && DFWrapper.APIReady)
                        {
                            if (!CheckFrozenKerbals(kerbal.name))
                            {
                                kerbal.type = crewMemberInfo.crewType;
                                kerbal.RegisterExperienceTraits(part);
                            }
                        }
                        else
                        {
                            kerbal.type = crewMemberInfo.crewType;
                            kerbal.RegisterExperienceTraits(part);
                        }
                    }
                }

                crewMemberInfo.hibernating = false;
            }
            else
            {
                double timeWithoutFood = currentTime - crewMemberInfo.lastFood;
                if (timeWithoutFood > (settings_sec3.MaxTimeWithoutFood + crewMemberInfo.respite))
                {
                    if (settings_sec1.hibernate == "Die")
                    {
                        KillCrewMember(crewMember, "starvation", vessel);
                    }
                    else
                    {
                        crewMemberInfo.hibernating = true;
                        ProtoCrewMember kerbal = HighLogic.CurrentGame.CrewRoster[crewMemberInfo.name]; 
                        if (kerbal != null)
                        {
                            kerbal.type = ProtoCrewMember.KerbalType.Tourist;
                            kerbal.UnregisterExperienceTraits(part);
                        }

                    }
                }
            }
        }

        private void ConsumeWater(double currentTime, Vessel vessel, VesselInfo vesselInfo, ProtoCrewMember crewMember, CrewMemberInfo crewMemberInfo, Part part)
        {
            if (vesselInfo.remainingWater >= settings_sec2.WaterConsumptionRate)
            {
                double deltaTime = Math.Min(currentTime - crewMemberInfo.lastWater, settings_sec3.MaxDeltaTime);
                double desiredWater = settings_sec2.WaterConsumptionRate * deltaTime;
                double waterObtained, waterSpace = 0;
                RSTUtils.Utilities.requireResourceID(vessel, globalsettings.WaterId,
                    Math.Min(desiredWater, vesselInfo.remainingWater / vesselInfo.numCrew), true, true, false, out waterObtained, out waterSpace);
                
                double wasteWaterProduced = waterObtained * settings_sec2.WasteWaterProductionRate / settings_sec2.WaterConsumptionRate;
                double wasteWaterObtained, wasteWaterSpace = 0;
                RSTUtils.Utilities.requireResourceID(vessel, globalsettings.WasteWaterId,
                    -wasteWaterProduced, true, false, false, out wasteWaterObtained, out wasteWaterSpace);
                
                crewMemberInfo.lastWater += deltaTime - ((desiredWater - waterObtained) / settings_sec2.WaterConsumptionRate);
                if (crewMemberInfo.hibernating)
                {
                    this.LogWarning("Removing hibernation status from crew member: " + crewMemberInfo.name);
                    ProtoCrewMember kerbal = HighLogic.CurrentGame.CrewRoster[crewMemberInfo.name];
                    if (kerbal != null && kerbal.type != crewMemberInfo.crewType)
                    {
                        if (IsDFInstalled && DFWrapper.APIReady)
                        {
                            if (!CheckFrozenKerbals(kerbal.name))
                            {
                                kerbal.type = crewMemberInfo.crewType;
                                kerbal.RegisterExperienceTraits(part);
                            }
                        }
                        else
                        {
                            kerbal.type = crewMemberInfo.crewType;
                            kerbal.RegisterExperienceTraits(part);
                        }
                    }
                }

                crewMemberInfo.hibernating = false;
            }
            else
            {
                double timeWithoutWater = currentTime - crewMemberInfo.lastWater;
                if (timeWithoutWater > (settings_sec3.MaxTimeWithoutWater + crewMemberInfo.respite))
                {
                    if (settings_sec1.hibernate == "Die")
                    {
                        KillCrewMember(crewMember, "dehydration", vessel);
                    }
                    else
                    {
                        crewMemberInfo.hibernating = true;
                        ProtoCrewMember kerbal = HighLogic.CurrentGame.CrewRoster[crewMemberInfo.name];
                        if (kerbal != null)
                        {
                            kerbal.type = ProtoCrewMember.KerbalType.Tourist;
                            kerbal.UnregisterExperienceTraits(part);
                        }
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
                    if (vesselInfo.remainingOxygen >= settings_sec2.OxygenConsumptionRate)
                    {
                        double deltaTime = Math.Min(currentTime - vesselInfo.lastOxygen, settings_sec3.MaxDeltaTime);
                        double rate = settings_sec2.OxygenConsumptionRate * vesselInfo.numCrew;
                        double desiredOxygen = rate * deltaTime;
                        double oxygenObtained, oxygenSpace = 0;
                        RSTUtils.Utilities.requireResourceID(vessel, globalsettings.OxygenId,
                            desiredOxygen, true, true, false, out oxygenObtained, out oxygenSpace);
                        
                        double co2Production = oxygenObtained * settings_sec2.CO2ProductionRate / settings_sec2.OxygenConsumptionRate;
                        double co2Obtained, co2Space = 0;
                        RSTUtils.Utilities.requireResourceID(vessel, globalsettings.CO2Id,
                            -co2Production, true, false, false, out co2Obtained, out co2Space);
                        
                        vesselInfo.lastOxygen += deltaTime - ((desiredOxygen - oxygenObtained) / rate);
                    }
                    else
                    {
                        double timeWithoutOxygen = currentTime - vesselInfo.lastOxygen;
                        if (timeWithoutOxygen > settings_sec3.MaxTimeWithoutOxygen)
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
                    double deltaTime = Math.Min(currentTime - vesselInfo.lastElectricity, Math.Max(settings_sec3.ElectricityMaxDeltaTime, TimeWarp.fixedDeltaTime));
                    double desiredElectricity = rate * deltaTime;
                    double electricityObtained, electricitySpace = 0;
                    RSTUtils.Utilities.requireResourceID(vessel, globalsettings.ElectricityId,
                        desiredElectricity, true, true, false, out electricityObtained, out electricitySpace);
                    
                    vesselInfo.lastElectricity = currentTime - ((desiredElectricity - electricityObtained) / rate);
                }
                else if (NeedElectricity(vessel, vesselInfo))
                {
                    double timeWithoutElectricity = currentTime - vesselInfo.lastElectricity;
                    if (timeWithoutElectricity > settings_sec3.MaxTimeWithoutElectricity)
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

            crewCapacity = vessel.GetCrewCapacity();
            vesselInfo.numCrew = vessel.GetCrewCount();

            for (int i = 0; i < vessel.parts.Count; i++)
            {
                if (vessel.parts[i].protoModuleCrew.Count > 0)
                {
                    ++vesselInfo.numOccupiedParts;
                }
            }
            vessel.GetConnectedResourceTotals(globalsettings.FoodId, out vesselInfo.remainingFood, out vesselInfo.maxFood);
            vessel.GetConnectedResourceTotals(globalsettings.WaterId, out vesselInfo.remainingWater, out vesselInfo.maxWater);
            vessel.GetConnectedResourceTotals(globalsettings.OxygenId, out vesselInfo.remainingOxygen, out vesselInfo.maxOxygen);
            vessel.GetConnectedResourceTotals(globalsettings.ElectricityId, out vesselInfo.remainingElectricity, out vesselInfo.maxElectricity);
            double maxCO2 = 0f;
            double maxWaste = 0f;
            double maxWasteWater = 0f;
            vessel.GetConnectedResourceTotals(globalsettings.CO2Id, out vesselInfo.remainingCO2, out maxCO2);
            vessel.GetConnectedResourceTotals(globalsettings.WasteId, out vesselInfo.remainingWaste, out maxWaste);
            vessel.GetConnectedResourceTotals(globalsettings.WasteWaterId, out vesselInfo.remainingWasteWater, out maxWasteWater);

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
                return (settings_sec2.ElectricityConsumptionRate * vesselInfo.numCrew) + (settings_sec2.BaseElectricityConsumptionRate * vesselInfo.numOccupiedParts);
            }
            else
            {
                return settings_sec2.EvaElectricityConsumptionRate;
            }
        }

        private void FillEvaSuit(Part oldPart, Part newPart)
        {
            if (!newPart.Resources.Contains(TacStartOnce.globalSettings.FoodId))
            {
                this.LogError("FillEvaSuit: new part does not have room for a Food resource.");
            }

            double desiredFood = settings_sec2.FoodConsumptionRate * settings_sec3.EvaDefaultResourceAmount;
            double desiredWater = settings_sec2.WaterConsumptionRate * settings_sec3.EvaDefaultResourceAmount;
            double desiredOxygen = settings_sec2.OxygenConsumptionRate * settings_sec3.EvaDefaultResourceAmount;
            double desiredElectricity = settings_sec2.EvaElectricityConsumptionRate * settings_sec3.EvaDefaultResourceAmount;

            Vessel lastVessel = oldPart.vessel;
            Vessel newVessel = newPart.vessel;
            VesselInfo lastVesselInfo;
            if (!TacLifeSupport.Instance.gameSettings.knownVessels.TryGetValue(lastVessel.id, out lastVesselInfo))
            {
                this.Log("FillEvaSuit: Unknown vessel: " + lastVessel.vesselName + " (" + lastVessel.id + ")");
                lastVesselInfo = new VesselInfo(lastVessel.vesselName, Planetarium.GetUniversalTime());
            }

            UpdateVesselInfo(lastVesselInfo, lastVessel);
            int numCrew = lastVesselInfo.numCrew + 1;

            double foodObtained, waterObtained, oxygenObtained, electricityObtained, foodGiven, waterGiven, oxygenGiven, electricityGiven = 0;
            double foodSpace, waterSpace, oxygenSpace, electricitySpace = 0;
            RSTUtils.Utilities.requireResourceID(lastVessel, globalsettings.FoodId, Math.Min(desiredFood, lastVesselInfo.remainingFood / numCrew), true, true, false, out foodObtained, out foodSpace);
            RSTUtils.Utilities.requireResourceID(lastVessel, globalsettings.WaterId, Math.Min(desiredWater, lastVesselInfo.remainingWater / numCrew), true, true, false, out waterObtained, out waterSpace);
            RSTUtils.Utilities.requireResourceID(lastVessel, globalsettings.OxygenId, Math.Min(desiredOxygen, lastVesselInfo.remainingOxygen / numCrew), true, true, false, out oxygenObtained, out oxygenSpace);
            RSTUtils.Utilities.requireResourceID(lastVessel, globalsettings.ElectricityId, Math.Min(desiredElectricity, lastVesselInfo.remainingElectricity / numCrew), true, true, false, out electricityObtained, out electricitySpace);

            RSTUtils.Utilities.requireResourceID(newVessel, globalsettings.FoodId, -foodObtained, true, false, false, out foodGiven, out foodSpace);
            RSTUtils.Utilities.requireResourceID(newVessel, globalsettings.WaterId, -waterObtained, true, false, false, out waterGiven, out waterSpace);
            RSTUtils.Utilities.requireResourceID(newVessel, globalsettings.OxygenId, -oxygenObtained, true, false, false, out oxygenGiven, out oxygenSpace);
            RSTUtils.Utilities.requireResourceID(newVessel, globalsettings.ElectricityId, -electricityObtained, true, false, false, out electricityGiven, out electricitySpace);
        }

        private void FillRescueEvaSuit(Vessel vessel)
        {
            this.Log("FillRescueEvaSuit: Rescue mission EVA: " + vessel.vesselName);
            Part part = vessel.rootPart;

            // Only fill the suit to 30-90% full
            double fillAmount = UnityEngine.Random.Range(0.3f, 0.9f);
            part.AddResource(PartResourceLibrary.Instance.GetDefinition(globalsettings.ElectricityId).Config);
            part.AddResource(PartResourceLibrary.Instance.GetDefinition(globalsettings.FoodId).Config);
            part.AddResource(PartResourceLibrary.Instance.GetDefinition(globalsettings.WaterId).Config);
            part.AddResource(PartResourceLibrary.Instance.GetDefinition(globalsettings.OxygenId).Config);

            double foodObtained, waterObtained, oxygenObtained, electricityObtained = 0;
            double foodSpace, waterSpace, oxygenSpace, electricitySpace = 0;
            RSTUtils.Utilities.requireResourceID(vessel, globalsettings.ElectricityId, -fillAmount * settings_sec2.EvaElectricityConsumptionRate * settings_sec3.EvaDefaultResourceAmount, true, false, false, out electricityObtained, out electricitySpace);
            RSTUtils.Utilities.requireResourceID(vessel, globalsettings.FoodId, -fillAmount * settings_sec2.FoodConsumptionRate * settings_sec3.EvaDefaultResourceAmount, true, false, false, out foodObtained, out foodSpace);
            RSTUtils.Utilities.requireResourceID(vessel, globalsettings.WaterId, -fillAmount * settings_sec2.WaterConsumptionRate * settings_sec3.EvaDefaultResourceAmount, true, false, false, out waterObtained, out waterSpace);
            RSTUtils.Utilities.requireResourceID(vessel, globalsettings.OxygenId, -fillAmount * settings_sec2.OxygenConsumptionRate * settings_sec3.EvaDefaultResourceAmount, true, false, false, out oxygenObtained, out oxygenSpace);
            
        }

        private void EmptyEvaSuit(Part oldPart, Part newPart)
        {
            Vessel lastVessel = oldPart.vessel;
            Vessel newVessel = newPart.vessel;
            VesselInfo lastVesselInfo;
            if (!TacLifeSupport.Instance.gameSettings.knownVessels.TryGetValue(lastVessel.id, out lastVesselInfo))
            {
                ScreenMessages.PostScreenMessage("Error - EmptyEvaSuit - Cannot find VesselInfo for " + oldPart.vessel.id, 10.0f, ScreenMessageStyle.UPPER_CENTER);
                this.LogError("EmptyEvaSuit - Cannot find VesselInfo for " + oldPart.vessel.id);
                return;
            }
            double foodObtained, waterObtained, oxygenObtained, electricityObtained, co2Obtained, wasteObtained, wastewaterObtained = 0;
            double foodSpace, waterSpace, oxygenSpace, electricitySpace = 0;
            RSTUtils.Utilities.requireResourceID(newVessel, globalsettings.ElectricityId, -lastVesselInfo.remainingElectricity, true, false, false, out electricityObtained, out electricitySpace);
            RSTUtils.Utilities.requireResourceID(newVessel, globalsettings.FoodId, -lastVesselInfo.remainingFood, true, false, false, out foodObtained, out foodSpace);
            RSTUtils.Utilities.requireResourceID(newVessel, globalsettings.WaterId, -lastVesselInfo.remainingWater, true, false, false, out waterObtained, out waterSpace);
            RSTUtils.Utilities.requireResourceID(newVessel, globalsettings.OxygenId, -lastVesselInfo.remainingOxygen, true, false, false, out oxygenObtained, out oxygenSpace);

            RSTUtils.Utilities.requireResourceID(newVessel, globalsettings.CO2Id, -lastVesselInfo.remainingCO2, true, false, false, out co2Obtained, out oxygenSpace);
            RSTUtils.Utilities.requireResourceID(newVessel, globalsettings.WasteId, -lastVesselInfo.remainingWaste, true, false, false, out wasteObtained, out foodSpace);
            RSTUtils.Utilities.requireResourceID(newVessel, globalsettings.WasteWaterId, -lastVesselInfo.remainingWasteWater, true, false, false, out wastewaterObtained, out waterSpace);
            
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
                        crewMember.StartRespawnPeriod(settings_sec1.respawnDelay);
                    }
                }
            }
            else
            {
                vessel.rootPart.Die();

                if (HighLogic.CurrentGame.Parameters.Difficulty.MissingCrewsRespawn)
                {
                    crewMember.StartRespawnPeriod(settings_sec1.respawnDelay);
                }
            }

            TacLifeSupport.Instance.gameSettings.knownCrew.Remove(crewMember.name);
        }

        public void Load(ConfigNode globalNode)
        {
            if (rosterWindow == null)
                rosterWindow = new RosterWindow(TACMenuAppLToolBar, globalsettings, TacLifeSupport.Instance.gameSettings);
            if (monitoringWindow == null)
                monitoringWindow = new LifeSupportMonitoringWindow(TACMenuAppLToolBar, this, globalsettings, TacLifeSupport.Instance.gameSettings, rosterWindow);

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
            //action.to.gameObject.AddComponent<LifeSupportModule>();
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
