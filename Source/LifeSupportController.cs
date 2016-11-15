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
using RSTKSPGameEvents;

namespace Tac
{
    class LifeSupportController : MonoBehaviour, Savable
    {
        private LifeSupportMonitoringWindow monitoringWindow;
        private RosterWindow rosterWindow;
        internal AppLauncherToolBar TACMenuAppLToolBar;
        private bool loadingNewScene = false;
        private double seaLevelPressure = 101.325;
        private GlobalSettings globalsettings;
        private TacGameSettings gameSettings;
        private float VesselSortCounter = 0f;
        private bool VesselSortCountervslChgFlag = false;
        public static LifeSupportController Instance;
        private bool checkedDictionaries = false;

        private TAC_SettingsParms settings_sec1;
        internal List<KeyValuePair<Guid, VesselInfo>> knownVesselsList;

        void Awake()
        {
            this.Log("Awake");
            Instance = this;
            globalsettings = TacStartOnce.Instance.globalSettings;
            VesselSortCounter = Time.time;
            settings_sec1 = HighLogic.CurrentGame.Parameters.CustomParams<TAC_SettingsParms>();
            if (!settings_sec1.enabled)
            {
                Destroy(this);
            }

            TACMenuAppLToolBar = new AppLauncherToolBar("TACLifeSupport", "TAC Life Support",
                Textures.PathToolbarIconsPath + "/TACgreenIconTB",
                ApplicationLauncher.AppScenes.TRACKSTATION | ApplicationLauncher.AppScenes.FLIGHT | ApplicationLauncher.AppScenes.SPACECENTER,
                (Texture)Textures.GrnApplauncherIcon, (Texture)Textures.GrnApplauncherIcon,
                GameScenes.TRACKSTATION , GameScenes.FLIGHT, GameScenes.SPACECENTER);
        }

        void Start()
        {
            this.Log("Start");
            gameSettings = TacLifeSupport.Instance.gameSettings;
            knownVesselsList = new List<KeyValuePair<Guid, VesselInfo>>(gameSettings.knownVessels);
            resetVesselList(FlightGlobals.fetch != null ? FlightGlobals.ActiveVessel : null);
            if (rosterWindow == null)
                rosterWindow = new RosterWindow(TACMenuAppLToolBar, globalsettings, gameSettings);
            if (monitoringWindow == null)
                monitoringWindow = new LifeSupportMonitoringWindow(TACMenuAppLToolBar, rosterWindow);

            if (!ToolbarManager.ToolbarAvailable && !settings_sec1.UseAppLToolbar)
            {
                settings_sec1.UseAppLToolbar = true;
            }
            
            TACMenuAppLToolBar.Start(settings_sec1.UseAppLToolbar);

            RSTUtils.Utilities.setScaledScreen();

            GameEvents.onCrewOnEva.Add(OnCrewOnEva);
            GameEvents.onCrewBoardVessel.Add(OnCrewBoardVessel);
            GameEvents.onGameSceneLoadRequested.Add(OnGameSceneLoadRequested);
            GameEvents.onVesselSwitching.Add(onVesselSwitching);
            GameEvents.onVesselSwitchingToUnloaded.Add(onVesselSwitching);
            GameEvents.onVesselCrewWasModified.Add(changeVesselCrew);
            GameEvents.onVesselWillDestroy.Add(onVesselWillDestroy);
            GameEvents.onVesselRecovered.Add(onVesselrecovered);
            GameEvents.onVesselTerminated.Add(onVesselTerminated);
            GameEvents.onVesselCreate.Add(onVesselCreate);
            GameEvents.onVesselWasModified.Add(onVesselWasModified);
            GameEvents.onVesselSituationChange.Add(onVesselSituationChange);
            GameEvents.onLevelWasLoaded.Add(onLevelWasLoaded);
            RSTEvents.onKerbalFrozen.Add(onKerbalFrozen);
            RSTEvents.onKerbalThaw.Add(onKerbalThaw);
            RSTEvents.onFrozenKerbalDied.Add(onFrozenKerbalDie);
            
            // Double check that we have the right sea level pressure for Kerbin
            seaLevelPressure = FlightGlobals.Bodies[1].GetPressure(0);
            
        }

        void OnDestroy()
        {
            this.Log("OnDestroy");
            TACMenuAppLToolBar.Destroy();
            GameEvents.onCrewOnEva.Remove(OnCrewOnEva);
            GameEvents.onCrewBoardVessel.Remove(OnCrewBoardVessel);
            GameEvents.onGameSceneLoadRequested.Remove(OnGameSceneLoadRequested);
            GameEvents.onVesselSwitching.Remove(onVesselSwitching);
            GameEvents.onVesselSwitchingToUnloaded.Remove(onVesselSwitching);
            GameEvents.onVesselCrewWasModified.Remove(changeVesselCrew);
            GameEvents.onVesselWillDestroy.Remove(onVesselWillDestroy);
            GameEvents.onVesselRecovered.Remove(onVesselrecovered);
            GameEvents.onVesselTerminated.Remove(onVesselTerminated);
            GameEvents.onVesselCreate.Remove(onVesselCreate);
            GameEvents.onVesselWasModified.Remove(onVesselWasModified);
            GameEvents.onVesselSituationChange.Remove(onVesselSituationChange);
            GameEvents.onLevelWasLoaded.Remove(onLevelWasLoaded);
            RSTEvents.onKerbalFrozen.Remove(onKerbalFrozen);
            RSTEvents.onKerbalThaw.Remove(onKerbalThaw);
            RSTEvents.onFrozenKerbalDied.Remove(onFrozenKerbalDie);
        }

        void OnGUI()
        {
            if (settings_sec1.enabled)
            {
                monitoringWindow.SetVisible(TACMenuAppLToolBar.GuiVisible);
                if (!TACMenuAppLToolBar.GuiVisible && rosterWindow.IsVisible())
                {
                    rosterWindow.SetVisible(false);
                }
                rosterWindow?.OnGUI();
                monitoringWindow?.OnGUI();
            }
        }

        void FixedUpdate()
        {
            if (Time.timeSinceLevelLoad < 1.0f || loadingNewScene)
            {
                return;
            }
            
            double currentTime = Planetarium.GetUniversalTime();
            var loadedVessels = FlightGlobals.VesselsLoaded;
            //Iterate the knownVessels dictionary
            foreach (var entry in gameSettings.knownVessels)
            {
                //If vessel is a recovery vessel check if it's EVA (Kerbal) or not. If it isn't we skip it completely.
                //If it is EVA (rescue kerbal) we continue processing.
                if (entry.Value.recoveryvessel)
                {
                    Vessel vsl = null;
                    var allvessels = FlightGlobals.Vessels;
                    bool isEVA = false;
                    for (int i = 0; i < allvessels.Count; ++i)
                    {
                        if (allvessels[i].id == entry.Key)
                        {
                            vsl = allvessels[i];
                            if (allvessels[i].isEVA)
                            {
                                isEVA = true;
                            }
                            break;
                        }
                    }
                    //If it's not EVA we skip.
                    if (!isEVA)
                    {
                        updateVslCrewCurrentTime(vsl, entry, currentTime);
                        continue;
                    }
                }
                Profiler.BeginSample("processVessel");
                //Loaded vessels first. Process Loaded vessel.
                Vessel loadedvessel = null;
                for (int i = 0; i < loadedVessels.Count; ++i)
                {
                    if (loadedVessels[i].id == entry.Key)
                    {
                        loadedvessel = loadedVessels[i];
                        break;
                    }
                }
                if (loadedvessel != null)
                {

                    //If vessel is loaded we update crewCapacity and if the vessel is NOT PRELAUNCH we consume resources and show warnings.
                    int crewCapacity = UpdateVesselInfo(entry.Value, loadedvessel);
                    if (crewCapacity == 0)
                    {
                        checkDictionaries();
                        continue;
                    }
                    //If vessel is PRELAUNCH
                    if (loadedvessel.situation == Vessel.Situations.PRELAUNCH)
                    {
                        updateVslCrewCurrentTime(loadedvessel, entry, currentTime, true);
                    }
                    //Vessel is NOT PRELAUNCH
                    else
                    {
                        ConsumeResources(currentTime, loadedvessel, entry.Value);
                        if (entry.Value.numCrew > 0)
                        {
                            ShowWarnings(loadedvessel.vesselName, entry.Value.remainingElectricity, entry.Value.maxElectricity, entry.Value.estimatedElectricityConsumptionRate, globalsettings.Electricity, ref entry.Value.electricityStatus);
                        }
                    }
                }

                //Unloaded vessels processing happens here. In a future release.
                //todo unloaded vessels processing.
                else
                {
                    
                }
                Profiler.EndSample();
                //Do warning processing on all vessels.
                Profiler.BeginSample("doWarningProcessing");
                doWarningProcessing(entry.Value, currentTime);
                Profiler.EndSample();
            }

            //Will re-create and sort the knownVesselsList that is used by the GUI.
            //It does this when a Dictionary event occurs or every settings_sec1.vesselUpdateList minutes.
            //The second part is because the sort order is ActiveVessel followed by all other vessels based on remaining resources.
            //It WAS doing this on every onGUI loop.
            //todo once the event driven changes are made look at this again and if it can be event driven as well
            Profiler.BeginSample("resetVesselList");
            if (Time.time - VesselSortCounter > settings_sec1.vesselUpdateList * 60 || VesselSortCountervslChgFlag)
            {
                resetVesselList(FlightGlobals.fetch != null ? FlightGlobals.ActiveVessel : null);
            }
            Profiler.EndSample();
        }

        /// <summary>
        /// Updates the knownVessel and all it's knownCrew lastFood, Oxygen,Water,Electricy and lastUpdate fields to the current time.
        /// Used for PreLaunch and Rescue Kerbal vessels.
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="currentTime"></param>
        /// <param name="PreLaunch"></param>
        private void updateVslCrewCurrentTime(Vessel vessel, KeyValuePair<Guid, VesselInfo> entry, double currentTime, bool PreLaunch = false)
        {
            entry.Value.lastFood = currentTime;
            entry.Value.lastOxygen = currentTime;
            entry.Value.lastWater = currentTime;
            entry.Value.lastElectricity = currentTime;
            entry.Value.lastUpdate = currentTime;
            if (vessel != null)
            {
                entry.Value.numCrew = vessel.GetCrewCount();
                entry.Value.numOccupiedParts = vessel.crewedParts;
            }

            if (entry.Value.numCrew > 0)
            {
                foreach (var crew in TacLifeSupport.Instance.gameSettings.knownCrew)
                {
                    if (crew.Value.vesselId == entry.Key)
                    {
                        crew.Value.lastFood = currentTime;
                        crew.Value.lastWater = currentTime;
                        crew.Value.lastUpdate = currentTime;
                        crew.Value.vesselIsPreLaunch = PreLaunch;
                    }
                }
            }
            else
            {
                entry.Value.recoveryvessel = false;
            }
        }

        /// <summary>
        /// Calculates remaining time for resources on vessel and displays warnings as appropriate.
        /// </summary>
        /// <param name="vesselInfo"></param>
        /// <param name="currentTime"></param>
        private void doWarningProcessing(VesselInfo vesselInfo, double currentTime)
        {
            double foodRate = globalsettings.FoodConsumptionRate * vesselInfo.numCrew;
            vesselInfo.estimatedTimeFoodDepleted = vesselInfo.lastFood + (vesselInfo.remainingFood / foodRate);
            double estimatedFood = vesselInfo.remainingFood - ((currentTime - vesselInfo.lastFood) * foodRate);
            ShowWarnings(vesselInfo.vesselName, estimatedFood, vesselInfo.maxFood, foodRate, globalsettings.Food, ref vesselInfo.foodStatus);

            double waterRate = globalsettings.WaterConsumptionRate * vesselInfo.numCrew;
            vesselInfo.estimatedTimeWaterDepleted = (vesselInfo.lastWater + vesselInfo.remainingWater / waterRate);
            double estimatedWater = vesselInfo.remainingWater - ((currentTime - vesselInfo.lastWater) * waterRate);
            ShowWarnings(vesselInfo.vesselName, estimatedWater, vesselInfo.maxWater, waterRate, globalsettings.Water, ref vesselInfo.waterStatus);

            double oxygenRate = globalsettings.OxygenConsumptionRate * vesselInfo.numCrew;
            vesselInfo.estimatedTimeOxygenDepleted = vesselInfo.lastOxygen + (vesselInfo.remainingOxygen / oxygenRate);
            double estimatedOxygen = vesselInfo.remainingOxygen - ((currentTime - vesselInfo.lastOxygen) * oxygenRate);
            ShowWarnings(vesselInfo.vesselName, estimatedOxygen, vesselInfo.maxOxygen, oxygenRate, globalsettings.Oxygen, ref vesselInfo.oxygenStatus);

            vesselInfo.estimatedTimeElectricityDepleted = vesselInfo.lastElectricity + (vesselInfo.remainingElectricity / vesselInfo.estimatedElectricityConsumptionRate);

        }
        /// <summary>
        /// Recreate the knownVesselsList which is used in the GUI display.
        /// A sort is done as well if the passed in vessel is not null.
        /// </summary>
        /// <param name="vessel">vessel to sort to top of list or null</param>
        private void resetVesselList(Vessel vessel)
        {
            if (gameSettings != null && knownVesselsList != null)
            {
                knownVesselsList.Clear();
                knownVesselsList = gameSettings.knownVessels.ToList();
                if (vessel != null)
                    knownVesselsList.Sort(new VesselSorter(vessel));
                VesselSortCounter = Time.time;
                VesselSortCountervslChgFlag = false;
            }
        }

        /// <summary>
        /// Fires on Vessel crew was modified or new vessel found.
        /// Updates the knownCrew dictionary.
        /// Special handling is done if contracts system is active to check if there is a rescue contract for each kerbal.
        /// If there is the kerbal and their vessel is marked as a recoveryvessel/kerbal.
        /// </summary>
        /// <param name="vessel">vessel crew we need to update</param>
        private void changeVesselCrew(Vessel vessel)
        {
            double currentTime = Planetarium.GetUniversalTime();
            var vslCrew = vessel.GetVesselCrew();
            for (int i = 0; i < vslCrew.Count; i++)
            { 
                if (gameSettings.knownCrew.ContainsKey(vslCrew[i].name))
                {
                    CrewMemberInfo crewMemberInfo = gameSettings.knownCrew[vslCrew[i].name];
                    crewMemberInfo.vesselId = vessel.id;
                    crewMemberInfo.vesselName = vessel.vesselName;
                    crewMemberInfo.vesselIsPreLaunch = vessel.SituationString == "PRELAUNCH";
                    //If rescue kerbal set their last use to current time and turn off their recoverkerbal flag.
                    //IE: We now start consuming resources.
                    if (crewMemberInfo.recoverykerbal)
                    {
                        crewMemberInfo.lastFood = crewMemberInfo.lastWater = crewMemberInfo.lastUpdate = currentTime;
                    }
                }
                else
                {
                    this.Log("New crew member: " + vslCrew[i].name);
                    //Check Contracts for Rescue Vessel/Kerbal
                    bool rescuekerbal = false;
                    if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER)
                    {
                        rescuekerbal = checkContractsForRescue(vslCrew[i]);
                    }
                    //Create new knownCrew Info.
                    var cmi = new CrewMemberInfo(vslCrew[i].name, vessel.vesselName, vessel.id, currentTime);
                    if (rescuekerbal) //Set RescueKerbal fields.
                    {
                        cmi.recoverykerbal = true;
                        cmi.vesselIsPreLaunch = false;
                        gameSettings.knownVessels[vessel.id].recoveryvessel = true;
                    }
                    //save knownCrew record.
                    gameSettings.knownCrew[vslCrew[i].name] = cmi;
                    //Increase number of crew on knownVessel record and if numOccupiedParts is 0 make it 1.
                    if (gameSettings.knownVessels.ContainsKey(vessel.id))
                    {
                        gameSettings.knownVessels[vessel.id].numCrew++;
                        if (gameSettings.knownVessels[vessel.id].numOccupiedParts == 0)
                            gameSettings.knownVessels[vessel.id].numOccupiedParts++;
                    }
                    resetVesselList(vessel);
                }
            }
        }

        /// <summary>
        /// Fires from RSTKSPGameEvents from DeepFreeze mod when a frozen kerbal dies.
        /// Remove them from knownCrew tracking.
        /// </summary>
        /// <param name="part"></param>
        /// <param name="crew"></param>
        private void onFrozenKerbalDie(ProtoCrewMember crew)
        {
            this.Log("Frozen crew member Died, Removing: " + crew.name);
            gameSettings.knownCrew.Remove(crew.name);
        }

        /// <summary>
        /// Fires from RSTKSPGameEvents from DeepFreeze mod when a Kerbal is Frozen.
        /// Changes knownCrew DFfrozen to true so TAC LS won't consume resources for frozen kerbal.
        /// Increases the knownVessel numFrozenCrew count as well so we don't stop tracking the vessel and don't consume O2 and EC for the vessel.
        /// </summary>
        /// <param name="part"></param>
        /// <param name="crew"></param>
        private void onKerbalFrozen(Part part, ProtoCrewMember crew)
        {
            if (gameSettings.knownCrew.ContainsKey(crew.name))
            {
                this.Log("Frozen crew member: " + crew.name);
                var knowncrew = gameSettings.knownCrew[crew.name];
                knowncrew.DFfrozen = true;
                if (gameSettings.knownVessels.ContainsKey(knowncrew.vesselId))
                {
                    var knownvessel = gameSettings.knownVessels[knowncrew.vesselId];
                    knownvessel.numFrozenCrew++;
                }
            }
        }

        /// <summary>
        /// Fires from RSTKSPGameEvents from DeepFreeze mod when a Kerbal is Thawed.
        /// Changes knownCrew DFfrozen to false and sets last consumed resources time to now so TAC LS will start consuming resources again from now.
        /// Decreases the knownVessel numFrozenCrew count as well
        /// </summary>
        /// <param name="part"></param>
        /// <param name="crew"></param>
        private void onKerbalThaw(Part part, ProtoCrewMember crew)
        {
            if (gameSettings.knownCrew.ContainsKey(crew.name))
            {
                this.Log("Thawed crew member: " + crew.name);
                double currentTime = Planetarium.GetUniversalTime();
                var knowncrew = gameSettings.knownCrew[crew.name];
                knowncrew.DFfrozen = false;
                knowncrew.lastFood = currentTime;
                knowncrew.lastWater = currentTime;
                knowncrew.lastUpdate = currentTime;
                if (gameSettings.knownVessels.ContainsKey(knowncrew.vesselId))
                {
                    var knownvessel = gameSettings.knownVessels[knowncrew.vesselId];
                    knownvessel.numFrozenCrew--;
                }
            }
        }
        
        /// <summary>
        /// Consume Life Support resources for a vessel. Called from FixedUpdate.
        /// </summary>
        /// <param name="currentTime"></param>
        /// <param name="vessel"></param>
        /// <param name="vesselInfo"></param>
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
                    crewMemberInfo.vesselIsPreLaunch = false;

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

        /// <summary>
        /// Consumes Food for a Kerbal. If food runs out, checks if they have exceeded the no food limit.
        /// If they have they will enter hibernation or Die.
        /// </summary>
        /// <param name="currentTime"></param>
        /// <param name="vessel"></param>
        /// <param name="vesselInfo"></param>
        /// <param name="crewMember"></param>
        /// <param name="crewMemberInfo"></param>
        /// <param name="part"></param>
        private void ConsumeFood(double currentTime, Vessel vessel, VesselInfo vesselInfo, ProtoCrewMember crewMember, CrewMemberInfo crewMemberInfo, Part part)
        {
            if (vesselInfo.remainingFood >= globalsettings.FoodConsumptionRate)
            {
                double deltaTime = Math.Min(currentTime - crewMemberInfo.lastFood, globalsettings.MaxDeltaTime);
                double desiredFood = globalsettings.FoodConsumptionRate * deltaTime;
                double foodObtained, foodSpace = 0;
                RSTUtils.Utilities.requireResourceID(vessel, globalsettings.FoodId,
                    Math.Min(desiredFood, vesselInfo.remainingFood/vesselInfo.numCrew), true, true, false, out foodObtained, out foodSpace);
                
                double wasteProduced = foodObtained * globalsettings.WasteProductionRate / globalsettings.FoodConsumptionRate;
                double wasteObtained, wasteSpace = 0;
                RSTUtils.Utilities.requireResourceID(vessel, globalsettings.WasteId,
                    -wasteProduced, true, false, false, out wasteObtained, out wasteSpace);
                
                crewMemberInfo.lastFood += deltaTime - ((desiredFood - foodObtained) / globalsettings.FoodConsumptionRate);

                if (crewMemberInfo.hibernating)
                {
                    this.LogWarning("Removing hibernation status from crew member: " + crewMemberInfo.name);
                    ProtoCrewMember kerbal = HighLogic.CurrentGame.CrewRoster[crewMemberInfo.name]; 
                    if (kerbal != null && kerbal.type != crewMemberInfo.crewType)
                    {
                        if (!crewMemberInfo.DFfrozen)
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
                if (timeWithoutFood > (globalsettings.MaxTimeWithoutFood + crewMemberInfo.respite))
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

        /// <summary>
        /// Consumes Water for a Kerbal. If water runs out, checks if they have exceeded the no water limit.
        /// If they have they will enter hibernation or Die.
        /// </summary>
        /// <param name="currentTime"></param>
        /// <param name="vessel"></param>
        /// <param name="vesselInfo"></param>
        /// <param name="crewMember"></param>
        /// <param name="crewMemberInfo"></param>
        /// <param name="part"></param>
        private void ConsumeWater(double currentTime, Vessel vessel, VesselInfo vesselInfo, ProtoCrewMember crewMember, CrewMemberInfo crewMemberInfo, Part part)
        {
            if (vesselInfo.remainingWater >= globalsettings.WaterConsumptionRate)
            {
                double deltaTime = Math.Min(currentTime - crewMemberInfo.lastWater, globalsettings.MaxDeltaTime);
                double desiredWater = globalsettings.WaterConsumptionRate * deltaTime;
                double waterObtained, waterSpace = 0;
                RSTUtils.Utilities.requireResourceID(vessel, globalsettings.WaterId,
                    Math.Min(desiredWater, vesselInfo.remainingWater / vesselInfo.numCrew), true, true, false, out waterObtained, out waterSpace);
                
                double wasteWaterProduced = waterObtained * globalsettings.WasteWaterProductionRate / globalsettings.WaterConsumptionRate;
                double wasteWaterObtained, wasteWaterSpace = 0;
                RSTUtils.Utilities.requireResourceID(vessel, globalsettings.WasteWaterId,
                    -wasteWaterProduced, true, false, false, out wasteWaterObtained, out wasteWaterSpace);
                
                crewMemberInfo.lastWater += deltaTime - ((desiredWater - waterObtained) / globalsettings.WaterConsumptionRate);
                if (crewMemberInfo.hibernating)
                {
                    this.LogWarning("Removing hibernation status from crew member: " + crewMemberInfo.name);
                    ProtoCrewMember kerbal = HighLogic.CurrentGame.CrewRoster[crewMemberInfo.name];
                    if (kerbal != null && kerbal.type != crewMemberInfo.crewType)
                    {
                        if (!crewMemberInfo.DFfrozen)
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
                if (timeWithoutWater > (globalsettings.MaxTimeWithoutWater + crewMemberInfo.respite))
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

        /// <summary>
        /// Consumes Oxygen for a Kerbal. If oxygen runs out, checks if they have exceeded the no oxygen limit.
        /// If they have they will enter hibernation or Die.
        /// </summary>
        /// <param name="currentTime"></param>
        /// <param name="vessel"></param>
        /// <param name="vesselInfo"></param>
        private void ConsumeOxygen(double currentTime, Vessel vessel, VesselInfo vesselInfo)
        {
            if (NeedOxygen(vessel, vesselInfo))
            {
                if (vesselInfo.numCrew > 0)
                {
                    if (vesselInfo.remainingOxygen >= globalsettings.OxygenConsumptionRate)
                    {
                        double deltaTime = Math.Min(currentTime - vesselInfo.lastOxygen, globalsettings.MaxDeltaTime);
                        double rate = globalsettings.OxygenConsumptionRate * vesselInfo.numCrew;
                        double desiredOxygen = rate * deltaTime;
                        double oxygenObtained, oxygenSpace = 0;
                        RSTUtils.Utilities.requireResourceID(vessel, globalsettings.OxygenId,
                            desiredOxygen, true, true, false, out oxygenObtained, out oxygenSpace);
                        
                        double co2Production = oxygenObtained * globalsettings.CO2ProductionRate / globalsettings.OxygenConsumptionRate;
                        double co2Obtained, co2Space = 0;
                        RSTUtils.Utilities.requireResourceID(vessel, globalsettings.CO2Id,
                            -co2Production, true, false, false, out co2Obtained, out co2Space);
                        
                        vesselInfo.lastOxygen += deltaTime - ((desiredOxygen - oxygenObtained) / rate);
                    }
                    else
                    {
                        double timeWithoutOxygen = currentTime - vesselInfo.lastOxygen;
                        if (timeWithoutOxygen > globalsettings.MaxTimeWithoutOxygen)
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

        /// <summary>
        /// Consumes EC for a Vessel. EC consumption is vessel based. If EC runs out, checks if the vessel has exceeded the no EC limit.
        /// If it has all the kerbals on-board will enter hibernation or Die.
        /// </summary>
        /// <param name="currentTime"></param>
        /// <param name="vessel"></param>
        /// <param name="vesselInfo"></param>
        private void ConsumeElectricity(double currentTime, Vessel vessel, VesselInfo vesselInfo)
        {
            double rate = vesselInfo.estimatedElectricityConsumptionRate = CalculateElectricityConsumptionRate(vessel, vesselInfo);
            if (rate > 0.0)
            {
                if (vesselInfo.remainingElectricity >= rate)
                {
                    double deltaTime = Math.Min(currentTime - vesselInfo.lastElectricity, Math.Max(globalsettings.ElectricityMaxDeltaTime, TimeWarp.fixedDeltaTime));
                    double desiredElectricity = rate * deltaTime;
                    double electricityObtained, electricitySpace = 0;
                    RSTUtils.Utilities.requireResourceID(vessel, globalsettings.ElectricityId,
                        desiredElectricity, true, true, false, out electricityObtained, out electricitySpace);
                    
                    vesselInfo.lastElectricity = currentTime - ((desiredElectricity - electricityObtained) / rate);
                }
                else if (NeedElectricity(vessel, vesselInfo))
                {
                    double timeWithoutElectricity = currentTime - vesselInfo.lastElectricity;
                    if (timeWithoutElectricity > globalsettings.MaxTimeWithoutElectricity)
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

        /// <summary>
        /// Updates vessel info for a vessel to be stored in the knownVessels dictionary.
        /// </summary>
        /// <param name="vesselInfo"></param>
        /// <param name="vessel"></param>
        /// <returns></returns>
        private int UpdateVesselInfo(VesselInfo vesselInfo, Vessel vessel)
        {
            int crewCapacity = 0;
            vesselInfo.ClearAmounts();
            crewCapacity = vessel.GetCrewCapacity();
            crewCapacity += vesselInfo.numFrozenCrew;
            vesselInfo.numCrew = vessel.GetCrewCount();
            vesselInfo.numOccupiedParts = vessel.crewedParts;
            vesselInfo.vesselSituation = vessel.situation;
            vesselInfo.vesselIsPreLaunch = vessel.SituationString == "PRELAUNCH";
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

        /// <summary>
        /// Calculates and displays warnings for low resources where appropriate.
        /// </summary>
        /// <param name="vesselName"></param>
        /// <param name="resourceRemaining"></param>
        /// <param name="max"></param>
        /// <param name="rate"></param>
        /// <param name="resourceName"></param>
        /// <param name="status"></param>
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
                    iconToSet = "/TACyellowIconTB";
                }
                else
                {
                    if (status == VesselInfo.Status.CRITICAL)
                    {
                        iconToSet = "/TACredIconTB";
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

        /// <summary>
        /// Calculates and returns the EC consumption for a vessel
        /// </summary>
        /// <param name="vessel"></param>
        /// <param name="vesselInfo"></param>
        /// <returns></returns>
        private double CalculateElectricityConsumptionRate(Vessel vessel, VesselInfo vesselInfo)
        {
            if (!vessel.isEVA)
            {
                return (globalsettings.ElectricityConsumptionRate * vesselInfo.numCrew) + (globalsettings.BaseElectricityConsumptionRate * vesselInfo.numOccupiedParts);
            }
            else
            {
                return globalsettings.EvaElectricityConsumptionRate;
            }
        }

        /// <summary>
        /// Fills an EVA suit when a kerbal goes EVA with life support resources from the host vessel.
        /// </summary>
        /// <param name="oldPart"></param>
        /// <param name="newPart"></param>
        private void FillEvaSuit(Part oldPart, Part newPart)
        {
            if (!newPart.Resources.Contains(TacStartOnce.Instance.globalSettings.FoodId))
            {
                this.LogError("FillEvaSuit: new part does not have room for a Food resource.");
            }

            double desiredFood = globalsettings.FoodConsumptionRate * globalsettings.EvaDefaultResourceAmount;
            double desiredWater = globalsettings.WaterConsumptionRate * globalsettings.EvaDefaultResourceAmount;
            double desiredOxygen = globalsettings.OxygenConsumptionRate * globalsettings.EvaDefaultResourceAmount;
            double desiredElectricity = globalsettings.EvaElectricityConsumptionRate * globalsettings.EvaDefaultResourceAmount;

            Vessel lastVessel = oldPart.vessel;
            Vessel newVessel = newPart.vessel;
            VesselInfo lastVesselInfo;
            if (!TacLifeSupport.Instance.gameSettings.knownVessels.TryGetValue(lastVessel.id, out lastVesselInfo))
            {
                this.Log("FillEvaSuit: Unknown vessel: " + lastVessel.vesselName + " (" + lastVessel.id + ")");
                lastVesselInfo = new VesselInfo(lastVessel.vesselName, lastVessel.situation, lastVessel.vesselType, Planetarium.GetUniversalTime());
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

        /// <summary>
        /// Fills a rescue kerbal EVA suit. The suit is randomly filled to between 30% - 90% capacity.
        /// </summary>
        /// <param name="vessel"></param>
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
            RSTUtils.Utilities.requireResourceID(vessel, globalsettings.ElectricityId, -fillAmount * globalsettings.EvaElectricityConsumptionRate * globalsettings.EvaDefaultResourceAmount, true, false, false, out electricityObtained, out electricitySpace);
            RSTUtils.Utilities.requireResourceID(vessel, globalsettings.FoodId, -fillAmount * globalsettings.FoodConsumptionRate * globalsettings.EvaDefaultResourceAmount, true, false, false, out foodObtained, out foodSpace);
            RSTUtils.Utilities.requireResourceID(vessel, globalsettings.WaterId, -fillAmount * globalsettings.WaterConsumptionRate * globalsettings.EvaDefaultResourceAmount, true, false, false, out waterObtained, out waterSpace);
            RSTUtils.Utilities.requireResourceID(vessel, globalsettings.OxygenId, -fillAmount * globalsettings.OxygenConsumptionRate * globalsettings.EvaDefaultResourceAmount, true, false, false, out oxygenObtained, out oxygenSpace);
            
        }

        /// <summary>
        /// Empties an EVA suit of life support resources and puts them into the vessel they have boarded
        /// </summary>
        /// <param name="oldPart"></param>
        /// <param name="newPart"></param>
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
        
        /// <summary>
        /// Handles a kerbal death
        /// </summary>
        /// <param name="crewMember"></param>
        /// <param name="causeOfDeath"></param>
        /// <param name="vessel"></param>
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
                monitoringWindow = new LifeSupportMonitoringWindow(TACMenuAppLToolBar, rosterWindow);

            monitoringWindow.Load(globalNode);
            rosterWindow.Load(globalNode);
        }

        public void Save(ConfigNode globalNode)
        {
            monitoringWindow.Save(globalNode);
            rosterWindow.Save(globalNode);
        }

        /// <summary>
        /// Called when GameEvent onLevelWasLoaded is fired.
        /// If the scene is FlightScene check the Dictionaries for errant entries.
        /// </summary>
        private void onLevelWasLoaded(GameScenes scene)
        {
            if (scene == GameScenes.FLIGHT && !checkedDictionaries)
            {
                checkDictionaries();
                checkedDictionaries = true;
            }
        }

        /// <summary>
        /// check the knownVessels and knownCrew dictionaries for entries that should be removed and remove them.
        /// </summary>
        private void checkDictionaries()
        {
            var vesselsToDelete = new List<Guid>();
            foreach (var vessel in gameSettings.knownVessels)
            {
                var gamevessel = FlightGlobals.Vessels.Find(a => a.id == vessel.Key);
                if (gamevessel == null)
                {
                    this.Log("Deleting vessel [" + vessel.Key + "] " + vessel.Value.vesselName + " - vessel does not exist anymore");
                    vesselsToDelete.Add(vessel.Key);
                }
                else
                {
                    if (gamevessel.crewedParts == 0 && vessel.Value.numFrozenCrew == 0)
                    {
                        this.Log("Deleting vessel [" + vessel.Key + "] " + vessel.Value.vesselName + " - no crewed parts anymore");
                        vesselsToDelete.Add(vessel.Key);
                    }
                }
            }

            //Delete any vessels (and their crew) that are no longer in the game but in our knownVessels list.
            for (int i = 0; i < vesselsToDelete.Count; i++)
            {
                //Check vessel does not contain frozen crew.
                if (gameSettings.knownVessels[vesselsToDelete[i]].numFrozenCrew == 0)
                {
                    var crewToDelete =
                        gameSettings.knownCrew.Where(e => e.Value.vesselId == vesselsToDelete[i])
                            .Select(e => e.Key)
                            .ToList();
                    foreach (String name in crewToDelete)
                    {
                        this.Log("Deleting crew member: " + name);
                        gameSettings.knownCrew.Remove(name);
                    }
                    gameSettings.knownVessels.Remove(vesselsToDelete[i]);
                    VesselSortCountervslChgFlag = true;
                }
            }

            //Now check the knownCrew dictionary. any entries where that crewmember is not RosterStatus of Assigned is removed.
            var knownCrew = gameSettings.knownCrew;
            IEnumerator<ProtoCrewMember> enumerator = HighLogic.CurrentGame.CrewRoster.Kerbals(ProtoCrewMember.KerbalType.Crew, 
                new ProtoCrewMember.RosterStatus[]
            {
                ProtoCrewMember.RosterStatus.Available,
                ProtoCrewMember.RosterStatus.Dead,
                ProtoCrewMember.RosterStatus.Missing       
            }).GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (knownCrew.ContainsKey(enumerator.Current.name))
                {
                    this.Log("CrewMember is NOT assigned roster status. Deleting crew member: " + enumerator.Current.name);
                    knownCrew.Remove(enumerator.Current.name);
                    VesselSortCountervslChgFlag = true;
                }
            }
            //Now check the knownCrew dictionary. any entries where that tourist is not RosterStatus of Assigned is removed.
            IEnumerator<ProtoCrewMember> enumerator2 = HighLogic.CurrentGame.CrewRoster.Kerbals(ProtoCrewMember.KerbalType.Tourist,
                new ProtoCrewMember.RosterStatus[]
            {
                ProtoCrewMember.RosterStatus.Available,
                ProtoCrewMember.RosterStatus.Dead,
                ProtoCrewMember.RosterStatus.Missing
            }).GetEnumerator();
            while (enumerator2.MoveNext())
            {
                if (knownCrew.ContainsKey(enumerator2.Current.name))
                {
                    this.Log("Tourist is NOT assigned roster status. Deleting crew member: " + enumerator2.Current.name);
                    knownCrew.Remove(enumerator2.Current.name);
                    VesselSortCountervslChgFlag = true;
                }
            }
            //Check All Vessels
            if (FlightGlobals.fetch)
            {
                var allVessels = FlightGlobals.Vessels;
                for (int i = 0; i < allVessels.Count; ++i)
                {
                    if (!gameSettings.knownVessels.ContainsKey(allVessels[i].id) && allVessels[i].GetVesselCrew().Count > 0)
                    {
                        CreateVesselEntry(allVessels[i]);
                    }
                }
            }
        }

        /// <summary>
        /// Called when GameEvent OnCrewOnEva is fired.
        /// </summary>
        /// <param name="action"></param>
        private void OnCrewOnEva(GameEvents.FromToAction<Part, Part> action)
        {
            this.Log("OnCrewOnEva: from=" + action.from.partInfo.title + "(" + action.from.vessel.vesselName + ")" + ", to=" + action.to.partInfo.title + "(" + action.to.vessel.vesselName + ")");
            //action.to.gameObject.AddComponent<LifeSupportModule>();
            Vessel lastVessel = action.from.vessel;
            if (gameSettings.knownVessels.ContainsKey(lastVessel.id))
            {
                if (gameSettings.knownVessels[lastVessel.id].recoveryvessel)
                {
                    this.Log("EVA from Recovery Vessel, Remove Recovery Vessel from Tracking");
                    RemoveVesselTracking(lastVessel.id);
                    return;    
                }
            }
            FillEvaSuit(action.from, action.to);
        }

        /// <summary>
        /// Called when GameEvent OnrewboardVessel is fired.
        /// </summary>
        /// <param name="action"></param>
        private void OnCrewBoardVessel(GameEvents.FromToAction<Part, Part> action)
        {
            this.Log("OnCrewBoardVessel: from=" + action.from.partInfo.title + "(" + action.from.vessel.vesselName + ")" + ", to=" + action.to.partInfo.title + "(" + action.to.vessel.vesselName + ")");
            EmptyEvaSuit(action.from, action.to);
        }

        /// <summary>
        /// Called when GameEvent OnGameSceneLoadRequested is fired. Will halt TAC LS processing until the scene changes.
        /// </summary>
        /// <param name="gameScene"></param>
        private void OnGameSceneLoadRequested(GameScenes gameScene)
        {
            this.Log("Game scene load requested: " + gameScene);

            // Disable this instance because a new instance will be created after the new scene is loaded
            loadingNewScene = true;
            // Reset DeepFreeze Reflection wrapper on scene change
            //resetDFonSceneChange = true;
        }

        /// <summary>
        /// Called when GameEvent onVesselSwitching occurs. Primarily to resort the Vessel List for the GUI display.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        private void onVesselSwitching(Vessel from, Vessel to)
        {
            if (to == null)
            {
                this.Log("TAC LS Vessel Change Flagged to: " + from.vesselName);
                CreateVesselEntry(from);
                resetVesselList(from);
            }
            else
            {
                this.Log("TAC LS Vessel Change Flagged to: " + to.vesselName);
                checkVesselHasCrew(from);
                CreateVesselEntry(to);
                resetVesselList(to);
            }
        }

        /// <summary>
        /// Called when GameEvent onVesselTermined occurs. Removes vessel and crew tracking.
        /// </summary>
        /// <param name="vessel"></param>
        private void onVesselTerminated(ProtoVessel vessel)
        {
            this.Log("Vessel Terminated [" + vessel.vesselID + "] " + vessel.vesselName);
            RemoveVesselTracking(vessel.vesselID);
        }

        /// <summary>
        /// Called when GameEvent onVesselDestroy occurs. Removes vessel and crew tracking.
        /// </summary>
        /// <param name="vessel"></param>
        private void onVesselWillDestroy(Vessel vessel)
        {
            this.Log("Vessel Destroyed [" + vessel.id + "] " + vessel.vesselName);
            RemoveVesselTracking(vessel.id);
        }

        /// <summary>
        /// Called when GameEvent on Vesselrecovered occurs. Removes vessel and crew tracking.
        /// </summary>
        /// <param name="vessel"></param>
        /// <param name="quick"></param>
        private void onVesselrecovered(ProtoVessel vessel, bool quick)
        {
            this.Log("Vessel Recovered [" + vessel.vesselID + "] " + vessel.vesselName);
            RemoveVesselTracking(vessel.vesselID);
        }

        /// <summary>
        /// Will remove a vessel ID from the knownVessels dictionary and find all Crew assigned to that vessel ID and remove
        /// then from the knownCrew dictionary.
        /// </summary>
        /// <param name="vesselID"></param>
        private void RemoveVesselTracking(Guid vesselID)
        {
            if (gameSettings.knownVessels.ContainsKey(vesselID))
            {
                this.Log("Deleting vessel " + vesselID + " - vessel does not exist anymore");
                gameSettings.knownVessels.Remove(vesselID);

                var crewToDelete =
                    gameSettings.knownCrew.Where(e => e.Value.vesselId == vesselID).Select(e => e.Key).ToList();
                foreach (String name in crewToDelete)
                {
                    this.Log("Deleting crew member: " + name);
                    gameSettings.knownCrew.Remove(name);
                }
                VesselSortCountervslChgFlag = true;
            }
        }

        /// <summary>
        /// When a vessel is created will add tracking of that Vessel to the knownVessel dictionary and will add tracking of any crew on board.
        /// </summary>
        /// <param name="vessel"></param>
        private void onVesselCreate(Vessel vessel)
        {
            if (RSTUtils.Utilities.ValidVslType(vessel))
            {
                CreateVesselEntry(vessel);
            }
        }

        //todo May need to check DeepFreeze Freezer Module on-board and if frozen kerbals on-board.
        /// <summary>
        /// Will check if there is a knownVessels entry or not. If not, it will create one only if the vessel has crew on-board. 
        /// </summary>
        /// <param name="vessel"></param>
        private void CreateVesselEntry(Vessel vessel)
        {
            if (gameSettings != null)
            {
                if (!gameSettings.knownVessels.ContainsKey(vessel.id) && vessel.GetVesselCrew().Count > 0)
                {
                    this.Log("New vessel: " + vessel.vesselName + " (" + vessel.id + ")");
                    if (vessel.isEVA)
                    {
                        ProtoCrewMember crewMember = vessel.GetVesselCrew().FirstOrDefault();
                        if (crewMember != null)// && gameSettings.knownCrew.ContainsKey(crewMember.name))
                        {
                            CrewMemberInfo value;
                            if (gameSettings.knownCrew.TryGetValue(crewMember.name, out value))
                            {
                                if (value == null)
                                {
                                    this.Log("Critical Error, failed to get EVA CrewMember Info");
                                }
                                else
                                {
                                    if (value.recoverykerbal)
                                    {
                                        //The vessel the Recovery Kerbal EVA'd from is removed from TACLS tracking in OnCrewOnEva when it is fired.
                                        FillRescueEvaSuit(vessel);
                                        value.recoverykerbal = false;
                                    }
                                }
                            }
                        }
                    }

                    VesselInfo vesselInfo = new VesselInfo(vessel.vesselName, vessel.situation, vessel.vesselType,Planetarium.GetUniversalTime());
                    
                    gameSettings.knownVessels[vessel.id] = vesselInfo;
                    if (vessel.loaded)
                        UpdateVesselInfo(vesselInfo, vessel);
                    VesselSortCountervslChgFlag = true;
                    changeVesselCrew(vessel);
                }
            }
        }

        private bool checkContractsForRescue(ProtoCrewMember crew)
        {
            var contracts = Contracts.ContractSystem.Instance.Contracts;
            for (int i = 0; i < contracts.Count; ++i)
            {
                if (contracts[i].Title.Contains(crew.name))
                {
                    return true;
                }
            }
            return false;
        }
        
        /// <summary>
        /// Called when GameEvent onVesselWasModified occurs. Checks if vessel has crew or not. IF it has crew we update the
        /// vessel dictionary (or add a new entry) and the crew dictionary. If there is no crew checks if we have a dictionary entry and
        /// removes it.
        /// </summary>
        /// <param name="vessel"></param>
        private void onVesselWasModified(Vessel vessel)
        {
            this.Log("Vessel Modified: " + vessel.vesselName + " (" + vessel.id + ")");
            //If vessel has crew we want to track it.
            if (vessel.GetVesselCrew().Count > 0)
            {
                //If we have an entry already, update the tracked crew dictionary.
                if (gameSettings.knownVessels.ContainsKey(vessel.id))
                {
                    changeVesselCrew(vessel);
                    VesselSortCountervslChgFlag = true;
                }
                //We didn't find it so create it.
                else
                {
                    this.Log("Couldn't find vessel, creating: " + vessel.vesselName + " (" + vessel.id + ")");
                    onVesselCreate(vessel);
                }
            }
            //If there is no crew (including check for frozen), check we don't have an entry, if we do, we want to stop tracking it.
            else
            {
                checkVesselHasCrew(vessel);
            }
        }

        private void checkVesselHasCrew(Vessel vessel)
        {
            if (vessel.GetVesselCrew().Count == 0 && gameSettings.knownVessels.ContainsKey(vessel.id))
            {
                if (gameSettings.knownVessels[vessel.id].numFrozenCrew == 0)
                    RemoveVesselTracking(vessel.id);
            }
        }

        private void onVesselSituationChange(GameEvents.HostedFromToAction<Vessel, Vessel.Situations> evt)
        {
            this.Log("Vessel situation change");
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
        /// <summary>
        /// Determine if vessel actually needs oxygen or can use outside air.
        /// </summary>
        /// <param name="vessel"></param>
        /// <param name="vesselInfo"></param>
        /// <returns>True if vessel can use outside air, otherwise returns false.</returns>
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

        /// <summary>
        /// Determine if vessel can open a window or not if out of EC.
        /// </summary>
        /// <param name="vessel"></param>
        /// <param name="vesselInfo"></param>
        /// <returns></returns>
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

        /// <summary>
        /// This class is used to sort the knownVessels list for the GUI.
        /// </summary>
        private class VesselSorter : IComparer<KeyValuePair<Guid, VesselInfo>>
        {
            private Vessel activeVessel;

            public VesselSorter(Vessel activeVessel)
            {
                this.activeVessel = activeVessel;
            }

            public int Compare(KeyValuePair<Guid, VesselInfo> left, KeyValuePair<Guid, VesselInfo> right)
            {
                // Put the active vessel at the top of the list
                if (activeVessel != null)
                {
                    if (left.Key.Equals(activeVessel.id))
                    {
                        if (right.Key.Equals(activeVessel.id))
                        {
                            // Both sides are the active vessel (i.e. the same vessel)
                            return 0;
                        }
                        else
                        {
                            return -1;
                        }
                    }
                    else if (right.Key.Equals(activeVessel.id))
                    {
                        return 1;
                    }
                }

                // then sort by the shortest time until a resource is depleted
                double leftShortestTime = Math.Min(left.Value.estimatedTimeFoodDepleted, Math.Min(left.Value.estimatedTimeWaterDepleted, left.Value.estimatedTimeOxygenDepleted));
                double rightShortestTime = Math.Min(right.Value.estimatedTimeFoodDepleted, Math.Min(right.Value.estimatedTimeWaterDepleted, right.Value.estimatedTimeOxygenDepleted));

                return leftShortestTime.CompareTo(rightShortestTime);
            }
        }
    }
}
