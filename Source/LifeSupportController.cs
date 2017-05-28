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
using System.Collections;
using System.Collections.Generic;
using Contracts.Templates;
using KSP.UI.Screens;
using RSTUtils;
using UnityEngine;
using KSP.Localization;
using BackgroundResources;

namespace Tac
{
    class LifeSupportController : MonoBehaviour, Savable
    {
        public static LifeSupportController Instance;

        #region Private Variables
        private LifeSupportMonitoringWindow monitoringWindow;
        private RosterWindow rosterWindow;
        internal AppLauncherToolBar TACMenuAppLToolBar;
        private bool loadingNewScene = false;
        private double seaLevelPressure = 101.325;
        private GlobalSettings globalsettings;
        private TacGameSettings gameSettings;
        private float VesselSortCounter = 0f;
        private bool VesselSortCountervslChgFlag = false;        
        private bool checkedDictionaries = false;
        private List<Guid> vesselstoDelete;
        private TAC_SettingsParms settings_sec1;
        internal List<KeyValuePair<Guid, VesselInfo>> knownVesselsList;
        private EventData<Part, ProtoCrewMember> onKerbalFrozenEvent;
        private EventData<Part, ProtoCrewMember> onKerbalThawEvent;
        private EventData<ProtoCrewMember> onFrozenKerbalDiedEvent;
        private List<Vessel> loadedVessels;
        private List<Vessel> unloadedVessels;
        private List<Vessel> allVessels;
        private VesselInfo.Status overallLifeSupportStatus;
        private VesselSorter vesselSorter;

        #endregion

        #region Mono methods
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
            vesselstoDelete = new List<Guid>();
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
            allVessels = new List<Vessel>();
            loadedVessels = new List<Vessel>();
            unloadedVessels = new List<Vessel>();
            knownVesselsList = new List<KeyValuePair<Guid, VesselInfo>>();            
            Dictionary<Guid, VesselInfo>.Enumerator vslenumerator = gameSettings.knownVessels.GetDictEnumerator();
            while (vslenumerator.MoveNext())
            {
                knownVesselsList.Add(vslenumerator.Current);
            }
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
            GameEvents.Contract.onCancelled.Add(onContractCancelled);
            GameEvents.onPartUndock.Add(onPartUndock);

            onKerbalFrozenEvent = GameEvents.FindEvent<EventData<Part, ProtoCrewMember>>("onKerbalFrozen");
            if (onKerbalFrozenEvent != null)
            {
                onKerbalFrozenEvent.Add(onKerbalFrozen);
            }
            onKerbalThawEvent = GameEvents.FindEvent<EventData<Part, ProtoCrewMember>>("onKerbalThaw");
            if (onKerbalThawEvent != null)
            {
                onKerbalThawEvent.Add(onKerbalThaw);
            }
            onFrozenKerbalDiedEvent = GameEvents.FindEvent<EventData<ProtoCrewMember>>("onFrozenKerbalDied");
            if (onFrozenKerbalDiedEvent != null)
            {
                onFrozenKerbalDiedEvent.Add(onFrozenKerbalDie);
            }
            
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
            GameEvents.Contract.onCancelled.Remove(onContractCancelled);
            GameEvents.onPartUndock.Remove(onPartUndock);
            if (onKerbalFrozenEvent != null)
            {
                onKerbalFrozenEvent.Remove(onKerbalFrozen);
            }
            if (onKerbalThawEvent != null)
            {
                onKerbalThawEvent.Remove(onKerbalThaw);
            }
            if (onFrozenKerbalDiedEvent != null)
            {
                onFrozenKerbalDiedEvent.Remove(onFrozenKerbalDie);
            }
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
            if (Time.timeSinceLevelLoad < 2.0f || loadingNewScene)
            {
                return;
            }
            
            double currentTime = Planetarium.GetUniversalTime();
            loadedVessels = FlightGlobals.VesselsLoaded;
            unloadedVessels = FlightGlobals.VesselsUnloaded;
            if (vesselstoDelete.Count > 0)
            {
                vesselstoDelete.Clear();
            }

            //Iterate the knownVessels dictionary

            Dictionary<Guid, VesselInfo>.Enumerator vslenumerator = gameSettings.knownVessels.GetDictEnumerator();
            while (vslenumerator.MoveNext())
            {
                //If vessel is a recovery vessel check if it's EVA (Kerbal) or not. If it isn't we skip it completely.
                //If it is EVA (rescue kerbal) we continue processing.
                if (vslenumerator.Current.Value.recoveryvessel)
                {
                    Vessel vsl = null;
                    allVessels = FlightGlobals.Vessels;
                    bool isOwned = false;
                    vsl = FlightGlobals.FindVessel(vslenumerator.Current.Key);
                    if (vsl != null)
                    {
                        // Once a rescue vessel becomes Owned (Loaded by coming in range of active vessel or switched to the vessel)
                        // It is no longer a rescue vessel. It is filled with random resource values and is processed like any other vessel.
                        if (vsl.DiscoveryInfo.Level == DiscoveryLevels.Owned)
                        {
                            isOwned = true;
                            vslenumerator.Current.Value.recoveryvessel = false;
                            Dictionary<string, CrewMemberInfo>.Enumerator crewenumerator = gameSettings.knownCrew.GetDictEnumerator();
                            while (crewenumerator.MoveNext())
                            {
                                if (crewenumerator.Current.Value.vesselId == vslenumerator.Current.Key)
                                {
                                    crewenumerator.Current.Value.recoverykerbal = false;
                                }
                            }
                            if (!vsl.rootPart.Resources.Contains(TacStartOnce.Instance.globalSettings.FoodId))
                            {
                                this.LogWarning("FillRescueVessel: new Rescue Vessel has no Resources. Adding some.");
                                FillRescuePart(vsl);
                            }
                        }
                    }
                    //If it's not Owned we skip the rescue vessel.
                    if (!isOwned)
                    {
                        updateVslCrewCurrentTime(vsl, vslenumerator.Current, currentTime);
                        continue;
                    }
                }

                //Find the vessel.
                Vessel vessel = null;
                vessel = FlightGlobals.FindVessel(vslenumerator.Current.Key);
                if (vessel != null)  //We found it.
                {
                    if (vessel.loaded)  //Process Loaded Vessel
                    {
                        //If vessel is loaded we update crewCapacity and if the vessel is NOT PRELAUNCH we consume resources and show warnings.
                        int crewCapacity = UpdateVesselInfo(vslenumerator.Current.Value, vessel);
                        if (crewCapacity == 0)
                        {
                            StartCoroutine(checkDictionaries());
                            continue;
                        }
                        //If vessel is PRELAUNCH
                        if (vessel.situation == Vessel.Situations.PRELAUNCH)
                        {
                            updateVslCrewCurrentTime(vessel, vslenumerator.Current, currentTime, true);
                            vslenumerator.Current.Value.estimatedTimeElectricityDepleted = vslenumerator.Current.Value.lastElectricity + (vslenumerator.Current.Value.remainingElectricity / CalculateElectricityConsumptionRate(vessel, vslenumerator.Current.Value));
                        }
                        //Vessel is NOT PRELAUNCH
                        else
                        {
                            ConsumeResources(currentTime, vessel, vslenumerator.Current.Value);
                        }
                    }
                    else  //Process Unloaded Vessel                   
                    {                        
                        //vslenumerator.Current.Value.numCrew = vessel.GetCrewCount();
                        //vslenumerator.Current.Value.numFrozenCrew = vslenumerator.Current.Value.numFrozenCrew;
                        //vslenumerator.Current.Value.numOccupiedParts = vessel.crewedParts;
                        int numCrew = UpdateUnloadedVesselInfo(vslenumerator.Current.Value, vessel);
                        //If vessel is PRELAUNCH
                        if (vessel.situation == Vessel.Situations.PRELAUNCH)
                        {
                            updateVslCrewCurrentTime(vessel, vslenumerator.Current, currentTime, true);
                            vslenumerator.Current.Value.estimatedTimeElectricityDepleted = vslenumerator.Current.Value.lastElectricity + (vslenumerator.Current.Value.remainingElectricity / CalculateElectricityConsumptionRate(vessel, vslenumerator.Current.Value));
                        }
                        //Vessel is NOT PRELAUNCH
                        else
                        {
                            if (settings_sec1.backgroundresources)
                            {
                                ConsumeResources(currentTime, vessel, vslenumerator.Current.Value);
                            }
                        }

                    }
                    //If there are no longer any crew stop tracking vessel.
                    if (vslenumerator.Current.Value.numCrew == 0 && vslenumerator.Current.Value.numFrozenCrew == 0)
                    {
                        vesselstoDelete.Add(vessel.id);
                    }
                }
                else //The vessel was not loaded and was not unloaded. It's gone (through docking probably or destroyed)
                {
                    vesselstoDelete.Add(vslenumerator.Current.Key);
                }

                //Do warning processing on all vessels except PreLaunch.
                if (vessel.situation != Vessel.Situations.PRELAUNCH)
                {
                    doWarningProcessing(vslenumerator.Current.Value, currentTime);
                }
            }
            for (int i = 0; i < vesselstoDelete.Count; i++)
            {
                RemoveVesselTracking(vesselstoDelete[i]);
            }

            if (gameSettings.knownVessels.Count == 0)
            {
                overallLifeSupportStatus = VesselInfo.Status.GOOD;
            }
            SetAppIconColor(overallLifeSupportStatus);

            //Will re-create and sort the knownVesselsList that is used by the GUI.
            //It does this when a Dictionary event occurs or every settings_sec1.vesselUpdateList minutes.
            //The second part is because the sort order is ActiveVessel followed by all other vessels based on remaining resources.
            //It WAS doing this on every onGUI loop.
            //todo once the event driven changes are made look at this again and if it can be event driven as well
            if (Time.time - VesselSortCounter > settings_sec1.vesselUpdateList * 60 || VesselSortCountervslChgFlag)
            {
                resetVesselList(FlightGlobals.fetch != null ? FlightGlobals.ActiveVessel : null);
            }            
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

        #endregion

        #region Manage Dictionaries
        /// <summary>
        /// check the knownVessels and knownCrew dictionaries for entries that should be removed and remove them.
        /// Check all vessels in game that have crew and if we are not tracking them add them to vessel tracking.
        /// </summary>
        private IEnumerator checkDictionaries()
        {
            List<Guid> vesselsToDelete = new List<Guid>();
            Dictionary<Guid, VesselInfo>.Enumerator vslenumerator = gameSettings.knownVessels.GetDictEnumerator();
            while (vslenumerator.MoveNext())
            {
                Vessel gamevessel = FlightGlobals.FindVessel(vslenumerator.Current.Key);
                if (gamevessel == null)
                {
                    this.Log("Deleting vessel [" + vslenumerator.Current.Key + "] " + vslenumerator.Current.Value.vesselName + " - vessel does not exist anymore");
                    vesselsToDelete.Add(vslenumerator.Current.Key);
                }
                else
                {
                    if (gamevessel.crewedParts == 0 && vslenumerator.Current.Value.numFrozenCrew == 0)
                    {
                        this.Log("Deleting vessel [" + vslenumerator.Current.Key + "] " + vslenumerator.Current.Value.vesselName + " - no crewed parts anymore");
                        vesselsToDelete.Add(vslenumerator.Current.Key);
                    }
                }
            }

            //Delete any vessels (and their crew) that are no longer in the game but in our knownVessels list.
            List<string> crewtoDelete = new List<string>();
            for (int i = 0; i < vesselsToDelete.Count; i++)
            {
                //Check vessel does not contain frozen crew.
                if (gameSettings.knownVessels[vesselsToDelete[i]].numFrozenCrew == 0)
                {
                    Dictionary<string, CrewMemberInfo>.Enumerator crewenumerator = gameSettings.knownCrew.GetDictEnumerator();
                    while (crewenumerator.MoveNext())
                    {
                        if (crewenumerator.Current.Value.vesselId == vesselstoDelete[i])
                        {
                            crewtoDelete.Add(crewenumerator.Current.Key);
                        }
                    }
                    gameSettings.knownVessels.Remove(vesselsToDelete[i]);
                    VesselSortCountervslChgFlag = true;
                }
            }
            for (int i = 0; i < crewtoDelete.Count; ++i)
            {
                this.Log("Deleting crew member: " + crewtoDelete[i]);
                gameSettings.knownCrew.Remove(crewtoDelete[i]);
            }


            //Now check the knownCrew dictionary. any entries where a crew member is not RosterStatus of Assigned is removed.

            DictionaryValueList<string, CrewMemberInfo> knownCrew = gameSettings.knownCrew;
            IEnumerator<ProtoCrewMember> enumerator = HighLogic.CurrentGame.CrewRoster.Kerbals(ProtoCrewMember.KerbalType.Crew,
                new ProtoCrewMember.RosterStatus[]
            {
                ProtoCrewMember.RosterStatus.Available,
                ProtoCrewMember.RosterStatus.Dead,
                ProtoCrewMember.RosterStatus.Missing
            }).GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (knownCrew.Contains(enumerator.Current.name))
                {
                    this.Log("CrewMember is NOT assigned roster status. Deleting crew member: " + enumerator.Current.name);
                    knownCrew.Remove(enumerator.Current.name);
                    VesselSortCountervslChgFlag = true;
                }
            }
            //Now check the knownCrew dictionary. any entries where a tourist is not RosterStatus of Assigned is removed.
            IEnumerator<ProtoCrewMember> enumerator2 = HighLogic.CurrentGame.CrewRoster.Kerbals(ProtoCrewMember.KerbalType.Tourist,
                new ProtoCrewMember.RosterStatus[]
            {
                ProtoCrewMember.RosterStatus.Available,
                ProtoCrewMember.RosterStatus.Dead,
                ProtoCrewMember.RosterStatus.Missing
            }).GetEnumerator();
            while (enumerator2.MoveNext())
            {
                if (knownCrew.Contains(enumerator2.Current.name))
                {
                    this.Log("Tourist is NOT assigned roster status. Deleting crew member: " + enumerator2.Current.name);
                    knownCrew.Remove(enumerator2.Current.name);
                    VesselSortCountervslChgFlag = true;
                }
            }
            //If Contracts system is active. Wait for it to finish loading contracts first.
            if (Contracts.ContractSystem.Instance != null)
            {
                if (!Contracts.ContractSystem.loaded)
                {
                    yield return null;
                }
            }
            //Check All Vessels, if they have crew and we are not tracking the vessel add it to tracking.
            if (FlightGlobals.fetch)
            {
                List<Vessel> allVessels = FlightGlobals.Vessels;
                for (int i = 0; i < allVessels.Count; ++i)
                {
                    if (!gameSettings.knownVessels.Contains(allVessels[i].id) && allVessels[i].GetVesselCrew().Count > 0)
                    {
                        CreateVesselEntry(allVessels[i]);
                    }
                }
            }
            checkedDictionaries = true;
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
                Dictionary<string, CrewMemberInfo>.Enumerator crewenumerator = TacLifeSupport.Instance.gameSettings.knownCrew.GetDictEnumerator();
                while (crewenumerator.MoveNext())
                {
                    if (crewenumerator.Current.Value.vesselId == entry.Key)
                    {
                        crewenumerator.Current.Value.lastFood = currentTime;
                        crewenumerator.Current.Value.lastWater = currentTime;
                        crewenumerator.Current.Value.lastUpdate = currentTime;
                        crewenumerator.Current.Value.vesselIsPreLaunch = PreLaunch;
                    }
                }                
            }
            else
            {
                if (entry.Value.recoveryvessel)
                    this.Log("Recovery Vessel no longer has any crew, changing it's status to normal vessel: " + vessel.vesselName);
                entry.Value.recoveryvessel = false;
            }
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
                Dictionary<Guid, VesselInfo>.Enumerator vslenumerator = gameSettings.knownVessels.GetDictEnumerator();
                while (vslenumerator.MoveNext())
                {
                    knownVesselsList.Add(vslenumerator.Current);
                } 
                if (vesselSorter == null)
                {
                    vesselSorter = new VesselSorter(vessel);
                }
                vesselSorter.ChangeActiveVessel(vessel);
                knownVesselsList.Sort(vesselSorter);
                
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
            List<ProtoCrewMember> vslCrew = vessel.GetVesselCrew();
            for (int i = 0; i < vslCrew.Count; i++)
            {
                if (gameSettings.knownCrew.Contains(vslCrew[i].name))
                {
                    CrewMemberInfo crewMemberInfo = gameSettings.knownCrew[vslCrew[i].name];
                    crewMemberInfo.vesselId = vessel.id;
                    crewMemberInfo.vesselName = vessel.vesselName;
                    crewMemberInfo.crewType = vslCrew[i].type;
                    crewMemberInfo.vesselIsPreLaunch = vessel.SituationString == "PRELAUNCH";
                    //If rescue kerbal set their last use to current time.
                    //IE: We will now start consuming resources. When the EVA processing completes.
                    if (crewMemberInfo.recoverykerbal)
                    {
                        crewMemberInfo.lastFood = crewMemberInfo.lastWater = crewMemberInfo.lastUpdate = currentTime;
                    }
                }
                else
                {
                    this.Log("New crew member: " + vslCrew[i].name);
                    //Check Contracts for Rescue Vessel/Kerbal and set temp bool.
                    bool rescuekerbal = false;
                    if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER)
                    {
                        rescuekerbal = checkContractsForRescue(vslCrew[i]);
                    }
                    //Create new knownCrew Info.
                    CrewMemberInfo cmi = new CrewMemberInfo(vslCrew[i].name, vessel.vesselName, vessel.id, currentTime);
                    if (rescuekerbal) //If Rescue Kerbal, Set RescueKerbal fields.
                    {
                        cmi.recoverykerbal = true;
                        cmi.vesselIsPreLaunch = false;
                        CreateVesselEntry(vessel);
                        gameSettings.knownVessels[vessel.id].recoveryvessel = true;
                        FillRescuePart(vessel);
                    }
                    //save knownCrew record.
                    gameSettings.knownCrew[vslCrew[i].name] = cmi;
                    VesselSortCountervslChgFlag = true;
                }
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
            vesselInfo.vesselIsPreLaunch = vessel.situation == Vessel.Situations.PRELAUNCH;
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
        /// Updates vessel info for an unloaded vessel to be stored in the knownVessels dictionary.
        /// </summary>
        /// <param name="vesselInfo"></param>
        /// <param name="vessel"></param>
        /// <returns></returns>
        private int UpdateUnloadedVesselInfo(VesselInfo vesselInfo, Vessel vessel)
        {
            int crewCapacity = 0;
            //vesselInfo.ClearAmounts();                     
            crewCapacity = vessel.GetCrewCapacity();
            crewCapacity += vesselInfo.numFrozenCrew;
            vesselInfo.numCrew = vessel.GetCrewCount();
            vesselInfo.numOccupiedParts = vessel.crewedParts;
            vesselInfo.vesselSituation = vessel.situation;
            vesselInfo.vesselIsPreLaunch = vessel.situation == Vessel.Situations.PRELAUNCH;
            if (settings_sec1.enabled)
            {
                UnloadedResourceProcessing.GetResourceTotals(vessel.protoVessel, globalsettings.Food, out vesselInfo.remainingFood, out vesselInfo.maxFood);
                UnloadedResourceProcessing.GetResourceTotals(vessel.protoVessel, globalsettings.Water, out vesselInfo.remainingWater, out vesselInfo.maxWater);
                UnloadedResourceProcessing.GetResourceTotals(vessel.protoVessel, globalsettings.Oxygen, out vesselInfo.remainingOxygen, out vesselInfo.maxOxygen);
                UnloadedResourceProcessing.GetResourceTotals(vessel.protoVessel, globalsettings.Electricity, out vesselInfo.remainingElectricity, out vesselInfo.maxElectricity);
                double maxCO2 = 0f;
                double maxWaste = 0f;
                double maxWasteWater = 0f;
                UnloadedResourceProcessing.GetResourceTotals(vessel.protoVessel, globalsettings.CO2, out vesselInfo.remainingCO2, out maxCO2);
                UnloadedResourceProcessing.GetResourceTotals(vessel.protoVessel, globalsettings.Waste, out vesselInfo.remainingWaste, out maxWaste);
                UnloadedResourceProcessing.GetResourceTotals(vessel.protoVessel, globalsettings.WasteWater, out vesselInfo.remainingWasteWater, out maxWasteWater);
            }
            return crewCapacity;
        }

        /// <summary>
        /// Will remove a vessel ID from the knownVessels dictionary and find all Crew assigned to that vessel ID and remove
        /// then from the knownCrew dictionary.
        /// </summary>
        /// <param name="vesselID"></param>
        private void RemoveVesselTracking(Guid vesselID)
        {
            if (gameSettings.knownVessels.Contains(vesselID))
            {
                this.Log("Deleting vessel " + vesselID + " - vessel does not exist anymore");
                bool rescuevessel = gameSettings.knownVessels[vesselID].recoveryvessel;
                gameSettings.knownVessels.Remove(vesselID);
                VesselSortCountervslChgFlag = true;

                //We do not stop tracking crew from Rescue Vessels.
                if (rescuevessel) return;

                //Stop tracking associated Crew.
                List<String> crewToDelete = new List<string>();
                var enumerator = gameSettings.knownCrew.GetDictEnumerator();
                while (enumerator.MoveNext())
                {
                    if (enumerator.Current.Value.vesselId == vesselID)
                    {
                        crewToDelete.Add(enumerator.Current.Key);
                    }
                }
                for (int i = 0; i < crewToDelete.Count; ++i)
                {
                    this.Log("Deleting crew member: " + crewToDelete[i]);
                    gameSettings.knownCrew.Remove(crewToDelete[i]);
                }
            }
        }
        
        /// <summary>
        /// Scans the active contracts for any that contain the Crew Member and returns true or false.
        /// </summary>
        /// <param name="crew">PCM of the crew member</param>
        /// <returns>True if contract with crew member, false if not.</returns>
        private bool checkContractsForRescue(ProtoCrewMember crew)
        {
            if (Contracts.ContractSystem.Instance != null)
            {
                var contracts = Contracts.ContractSystem.Instance.Contracts;
                for (int i = 0; i < contracts.Count; ++i)
                {
                    if (contracts[i].Title.Contains(FinePrint.Utilities.StringUtilities.ShortKerbalName(crew.name)))
                    {
                        return true;
                    }
                }
            }
            return false;
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
                if (!gameSettings.knownVessels.Contains(vessel.id) && vessel.GetVesselCrew().Count > 0)
                {
                    this.Log("New vessel: " + vessel.vesselName + " (" + vessel.id + ")");

                    VesselInfo vesselInfo = new VesselInfo(vessel.vesselName, vessel.situation, vessel.vesselType, Planetarium.GetUniversalTime());

                    gameSettings.knownVessels[vessel.id] = vesselInfo;
                    if (vessel.loaded)
                        UpdateVesselInfo(vesselInfo, vessel);
                    VesselSortCountervslChgFlag = true;
                    changeVesselCrew(vessel);
                }
            }
        }
        
        private void checkVesselHasCrew(Vessel vessel)
        {
            if (vessel.GetCrewCount() == 0 && gameSettings.knownVessels.Contains(vessel.id))
            {
                if (gameSettings.knownVessels[vessel.id].numFrozenCrew == 0)
                    RemoveVesselTracking(vessel.id);
            }
        }

        #endregion

        #region DeepFreeze Mod Support
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
            if (gameSettings.knownCrew.Contains(crew.name))
            {
                this.Log("Frozen crew member: " + crew.name);
                var knowncrew = gameSettings.knownCrew[crew.name];
                knowncrew.DFfrozen = true;
                knowncrew.crewType = crew.type;
                if (gameSettings.knownVessels.Contains(knowncrew.vesselId))
                {
                    var knownvessel = gameSettings.knownVessels[knowncrew.vesselId];
                    knownvessel.numFrozenCrew++;
                }
                changeVesselCrew(part.vessel);
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
            if (gameSettings.knownCrew.Contains(crew.name))
            {
                this.Log("Thawed crew member: " + crew.name);
                double currentTime = Planetarium.GetUniversalTime();
                var knowncrew = gameSettings.knownCrew[crew.name];
                knowncrew.DFfrozen = false;
                knowncrew.lastFood = currentTime;
                knowncrew.lastWater = currentTime;
                knowncrew.lastUpdate = currentTime;
                knowncrew.crewType = crew.type;
                if (gameSettings.knownVessels.Contains(knowncrew.vesselId))
                {
                    var knownvessel = gameSettings.knownVessels[knowncrew.vesselId];
                    knownvessel.numFrozenCrew--;
                }
                changeVesselCrew(part.vessel);
            }
        }

        #endregion

        #region Manage Life Support Resources
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
            DictionaryValueList<string, CrewMemberInfo> knownCrew = TacLifeSupport.Instance.gameSettings.knownCrew;
            for (int i = 0; i < crew.Count; ++i)            
            {
                ProtoCrewMember crewMember = crew[i];
                if (knownCrew.Contains(crewMember.name))
                {
                    CrewMemberInfo crewMemberInfo = knownCrew[crewMember.name];
                    Part part = null;
                    if (vessel.loaded)
                    {
                        part = (crewMember.KerbalRef != null) ? crewMember.KerbalRef.InPart : vessel.rootPart;
                    }                    
                    
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

            //If Deepfreeze and vessel has frozen crew. Process them.
            //todo: this is terrible. We need a List of frozen Kerbals not iterate the whole dictionary of knownKerbals.
            // Fix this next version.
            if (vesselInfo.numFrozenCrew > 0)
            {

                Dictionary<string, CrewMemberInfo>.Enumerator crewenumerator = knownCrew.GetDictEnumerator();
                while (crewenumerator.MoveNext())
                {
                    if (crewenumerator.Current.Value.DFfrozen && crewenumerator.Current.Value.vesselId == vessel.id)
                    {
                        crewenumerator.Current.Value.lastUpdate = currentTime;
                        crewenumerator.Current.Value.lastFood = currentTime;
                        crewenumerator.Current.Value.lastWater = currentTime;
                        crewenumerator.Current.Value.vesselId = vessel.id;
                        crewenumerator.Current.Value.vesselName = (!vessel.isEVA) ? vessel.vesselName : "EVA";
                        crewenumerator.Current.Value.vesselIsPreLaunch = false;
                    }
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
                double foodObtained = 0;
                double foodSpace = 0;
                if (vessel.loaded)
                {
                    RSTUtils.Utilities.requireResourceID(vessel, globalsettings.FoodId,
                        Math.Min(desiredFood, vesselInfo.remainingFood / vesselInfo.numCrew), true, true, false, out foodObtained, out foodSpace);
                }
                else
                {
                    UnloadedResourceProcessing.RequestResource(vessel.protoVessel, globalsettings.Food, desiredFood, out foodObtained);
                }

                double wasteProduced = foodObtained * globalsettings.WasteProductionRate / globalsettings.FoodConsumptionRate;
                double wasteObtained = 0;
                double wasteSpace = 0;
                if (vessel.loaded)
                {
                    RSTUtils.Utilities.requireResourceID(vessel, globalsettings.WasteId,
                        -wasteProduced, true, false, false, out wasteObtained, out wasteSpace);
                }
                else
                {
                    UnloadedResourceProcessing.RequestResource(vessel.protoVessel, globalsettings.Waste, wasteProduced, out wasteObtained, true);
                }
                
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
                            if (part != null)
                            {
                                kerbal.RegisterExperienceTraits(part);
                            }
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
                        KillCrewMember(crewMember, Localizer.Format("#autoLOC_TACLS_00042"), vessel); //#autoLOC_TACLS_00042 = starvation
                    }
                    else
                    {
                        crewMemberInfo.hibernating = true;
                        ProtoCrewMember kerbal = HighLogic.CurrentGame.CrewRoster[crewMemberInfo.name]; 
                        if (kerbal != null)
                        {
                            kerbal.type = ProtoCrewMember.KerbalType.Tourist;
                            if (part != null)
                            {
                                kerbal.UnregisterExperienceTraits(part);
                            }
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
                double waterObtained = 0;
                double waterSpace = 0;
                if (vessel.loaded)
                {
                    RSTUtils.Utilities.requireResourceID(vessel, globalsettings.WaterId,
                        Math.Min(desiredWater, vesselInfo.remainingWater / vesselInfo.numCrew), true, true, false, out waterObtained, out waterSpace);
                }
                else
                {
                    UnloadedResourceProcessing.RequestResource(vessel.protoVessel, globalsettings.Water, desiredWater, out waterObtained);
                }
                
                double wasteWaterProduced = waterObtained * globalsettings.WasteWaterProductionRate / globalsettings.WaterConsumptionRate;
                double wasteWaterObtained = 0;
                double wasteWaterSpace = 0;
                if (vessel.loaded)
                {
                    RSTUtils.Utilities.requireResourceID(vessel, globalsettings.WasteWaterId,
                        -wasteWaterProduced, true, false, false, out wasteWaterObtained, out wasteWaterSpace);
                }
                else
                {
                    UnloadedResourceProcessing.RequestResource(vessel.protoVessel, globalsettings.WasteWater, wasteWaterProduced, out wasteWaterObtained, true);
                }
                
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
                            if (part != null)
                            {
                                kerbal.RegisterExperienceTraits(part);
                            }
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
                        KillCrewMember(crewMember, Localizer.Format("#autoLOC_TACLS_00043"), vessel); //#autoLOC_TACLS_00043 = dehydration
                    }
                    else
                    {
                        crewMemberInfo.hibernating = true;
                        ProtoCrewMember kerbal = HighLogic.CurrentGame.CrewRoster[crewMemberInfo.name];
                        if (kerbal != null)
                        {
                            kerbal.type = ProtoCrewMember.KerbalType.Tourist;
                            if (part != null)
                            {
                                kerbal.UnregisterExperienceTraits(part);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Consumes Oxygen. Oxygen consumption is calculated per vessel. If oxygen runs out, checks if they have exceeded the no oxygen limit.
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
                        double oxygenObtained = 0;
                        double oxygenSpace = 0;
                        if (vessel.loaded)
                        {
                            RSTUtils.Utilities.requireResourceID(vessel, globalsettings.OxygenId,
                                desiredOxygen, true, true, false, out oxygenObtained, out oxygenSpace);
                        }
                        else
                        {
                            UnloadedResourceProcessing.RequestResource(vessel.protoVessel, globalsettings.Oxygen, desiredOxygen, out oxygenObtained);
                        }
                        
                        double co2Production = oxygenObtained * globalsettings.CO2ProductionRate / globalsettings.OxygenConsumptionRate;
                        double co2Obtained = 0;
                        double co2Space = 0;
                        if (vessel.loaded)
                        {
                            RSTUtils.Utilities.requireResourceID(vessel, globalsettings.CO2Id,
                                -co2Production, true, false, false, out co2Obtained, out co2Space);
                        }
                        else
                        {
                            UnloadedResourceProcessing.RequestResource(vessel.protoVessel, globalsettings.CO2, co2Production, out co2Obtained, true);
                        }
                        
                        vesselInfo.lastOxygen += deltaTime - ((desiredOxygen - oxygenObtained) / rate);
                    }
                    else
                    {
                        double timeWithoutOxygen = currentTime - vesselInfo.lastOxygen;
                        if (timeWithoutOxygen > globalsettings.MaxTimeWithoutOxygen)
                        {
                            List<ProtoCrewMember> crew = vessel.GetVesselCrew();
                            int crewMemberIndex = UnityEngine.Random.Range(0, crew.Count - 1);
                            KillCrewMember(crew[crewMemberIndex], Localizer.Format("#autoLOC_TACLS_00044"), vessel); //#autoLOC_TACLS_00044 = oxygen deprivation

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
            vesselInfo.windowOpen = false; //Close the windows (temporarily if already open).
            //Calculate the rate of EC we need based on number of occupied parts and kerbals on board.
            //The rate is per second.
            double rate = CalculateElectricityConsumptionRate(vessel, vesselInfo);
            vesselInfo.estimatedElectricityConsumptionRate = rate;
            if (rate > 0.0)  //If the rate > zero we have a job to do.
            {
                if (vesselInfo.remainingElectricity >= rate)  //If we have enough EC stored we process
                {
                    //The delta Time is the minimum of the amount of time since we last took EC and the minimum of EC max delta time or currented fixed delta time.
                    double deltaTime = Math.Min(currentTime - vesselInfo.lastElectricity, Math.Min(globalsettings.ElectricityMaxDeltaTime, TimeWarp.fixedDeltaTime));
                    double desiredElectricity = rate * deltaTime;  //We need the rate x delta time.                    
                    double electricityObtained = 0;
                    double electricitySpace = 0;
                    if (vessel.loaded)
                    {
                        RSTUtils.Utilities.requireResourceID(vessel, globalsettings.ElectricityId,
                            desiredElectricity, true, true, false, out electricityObtained, out electricitySpace);
                    }
                    else
                    {
                        UnloadedResourceProcessing.RequestResource(vessel.protoVessel, globalsettings.Electricity, desiredElectricity, out electricityObtained);
                    }

                    vesselInfo.lastElectricity += deltaTime - ((desiredElectricity - electricityObtained) / rate);
                }
                else
                {
                    if (NeedElectricity(vessel, vesselInfo))
                    {
                        double timeWithoutElectricity = currentTime - vesselInfo.lastElectricity;
                        if (timeWithoutElectricity > globalsettings.MaxTimeWithoutElectricity)
                        {
                            List<ProtoCrewMember> crew = vessel.GetVesselCrew();
                            int crewMemberIndex = UnityEngine.Random.Range(0, crew.Count - 1);
                            KillCrewMember(crew[crewMemberIndex], Localizer.Format("#autoLOC_TACLS_00045"), vessel); //#autoLOC_TACLS_00045 = air toxicity

                            vesselInfo.lastElectricity += UnityEngine.Random.Range(60, 600);
                        }                        
                    }
                    else  //We are out of EC, but we can open the windows. so put that in the GUI
                    {
                        vesselInfo.windowOpen = true;
                    }
                }
            }
            else  //Otherwise nothing to do...
            {
                vesselInfo.lastElectricity += currentTime - vesselInfo.lastUpdate;
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
                if (vesselInfo.numCrew == 0)
                {
                    return 0;
                }
                else
                {
                    return (globalsettings.ElectricityConsumptionRate * vesselInfo.numCrew) + (globalsettings.BaseElectricityConsumptionRate * vesselInfo.numOccupiedParts);
                }
            }
            else
            {
                return globalsettings.EvaElectricityConsumptionRate + ConsumeEVALightEC(vessel);
            }
        }

        /// <summary>
        /// Call ONLY if vessel is an EVA vessel.
        /// Finds if the kerbalEVA has their lampOn and if it is returns the EVAlampEC rate.
        /// Otherwise returns zero.
        /// </summary>
        /// <param name="vessel">The vessel</param>
        /// <returns>Returns the global setting for EVA Lamp EC consumption or zero</returns>
        private double ConsumeEVALightEC(Vessel vessel)
        {
            double returnAmount = 0;
            if (vessel.isEVA && vessel.loaded)
            {
                KerbalEVA kerbalEVA = vessel.FindPartModuleImplementing<KerbalEVA>();
                if (kerbalEVA != null)
                {
                    if (kerbalEVA.lampOn) //Ok so if their lamp is on, consume EC
                    {
                        returnAmount = globalsettings.EvaLampElectricityConsumptionRate;
                    }
                }
            }
            return returnAmount;
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
            if (vessel.mainBody == FlightGlobals.Bodies[1] || vessel.mainBody.atmosphereContainsOxygen)
            {
                // On or above Kerbin or a Planet that has an atmosphere that contains oxygen.
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
            if (vessel.mainBody == FlightGlobals.Bodies[1] || vessel.mainBody.atmosphereContainsOxygen)
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

        #endregion

        #region Warnings
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
            ShowWarnings(vesselInfo.vesselName, estimatedFood, vesselInfo.maxFood, foodRate, globalsettings.displayFood, ref vesselInfo.foodStatus);

            double waterRate = globalsettings.WaterConsumptionRate * vesselInfo.numCrew;
            vesselInfo.estimatedTimeWaterDepleted = (vesselInfo.lastWater + vesselInfo.remainingWater / waterRate);
            double estimatedWater = vesselInfo.remainingWater - ((currentTime - vesselInfo.lastWater) * waterRate);
            ShowWarnings(vesselInfo.vesselName, estimatedWater, vesselInfo.maxWater, waterRate, globalsettings.displayWater, ref vesselInfo.waterStatus);

            double oxygenRate = globalsettings.OxygenConsumptionRate * vesselInfo.numCrew;
            vesselInfo.estimatedTimeOxygenDepleted = vesselInfo.lastOxygen + (vesselInfo.remainingOxygen / oxygenRate);
            double estimatedOxygen = vesselInfo.remainingOxygen - ((currentTime - vesselInfo.lastOxygen) * oxygenRate);
            ShowWarnings(vesselInfo.vesselName, estimatedOxygen, vesselInfo.maxOxygen, oxygenRate, globalsettings.displayOxygen, ref vesselInfo.oxygenStatus);

            vesselInfo.estimatedTimeElectricityDepleted = vesselInfo.lastElectricity + (vesselInfo.remainingElectricity / vesselInfo.estimatedElectricityConsumptionRate);
            if (vesselInfo.numCrew > 0 || vesselInfo.numFrozenCrew > 0)  //Only show EC warning if there are crew on board.
            {
                double estimatedElectricity = vesselInfo.remainingElectricity - ((currentTime - vesselInfo.lastElectricity) * vesselInfo.estimatedElectricityConsumptionRate);
                ShowWarnings(vesselInfo.vesselName, estimatedElectricity, vesselInfo.maxElectricity, vesselInfo.estimatedElectricityConsumptionRate, globalsettings.displayElectricity, ref vesselInfo.electricityStatus);
            }

            vesselInfo.overallStatus = vesselInfo.foodStatus | vesselInfo.oxygenStatus | vesselInfo.waterStatus | vesselInfo.electricityStatus;
            overallLifeSupportStatus = overallLifeSupportStatus | vesselInfo.overallStatus;
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
            double criticalLevel = max * 0.03; // 3% full
            double warningLevel = max * 0.10; // 10% full

            if (resourceRemaining < criticalLevel)
            {
                if (status != VesselInfo.Status.CRITICAL)
                {
                    TimeWarp.SetRate(0, false);
                    ScreenMessages.PostScreenMessage(Localizer.Format("#autoLOC_TACLS_00038", vesselName, resourceName), 15.0f, ScreenMessageStyle.UPPER_CENTER); //#autoLOC_TACLS_00038 = <<1>> - <<2>> depleted!
                    this.Log(vesselName + " - " + resourceName + " depleted!");
                    status = VesselInfo.Status.CRITICAL;                    
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
                    TimeWarp.SetRate(0, false);
                    ScreenMessages.PostScreenMessage(Localizer.Format("#autoLOC_TACLS_00039", vesselName, resourceName), 15.0f, ScreenMessageStyle.UPPER_CENTER); //#autoLOC_TACLS_00039 = <<1>> - <<2>> is running out!
                    this.Log(vesselName + " - " + resourceName + " is running out!");
                    status = VesselInfo.Status.LOW;                    
                }
            }
            else
            {
                status = VesselInfo.Status.GOOD;
            }
        }

        /// <summary>
        /// Sets the Stock App ToolBar Icon or Blizzy ToolBar Icon color as per the passed in status.
        /// </summary>
        /// <param name="status"></param>
        private void SetAppIconColor(VesselInfo.Status status)
        {
            //Set Icon Colour            
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

        #endregion

        #region EVA and Rescue
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
        /// Fills a rescue Part. The part is randomly filled to between 30% - 90% capacity.
        /// </summary>
        /// <param name="vessel"></param>
        private void FillRescuePart(Vessel vessel)
        {
            this.Log("FillRescuePart: Rescue mission: " + vessel.vesselName);
            Part part = vessel.rootPart;

            // Only fill the suit to 30-90% full
            double fillAmount = UnityEngine.Random.Range(0.3f, 0.9f);
            RescuePartAddResource(part, globalsettings.Electricity,fillAmount * (globalsettings.BaseElectricityConsumptionRate + globalsettings.ElectricityConsumptionRate) * globalsettings.EvaDefaultResourceAmount);
            RescuePartAddResource(part, globalsettings.Food, fillAmount * globalsettings.FoodConsumptionRate * globalsettings.EvaDefaultResourceAmount);
            RescuePartAddResource(part, globalsettings.Water, fillAmount * globalsettings.WaterConsumptionRate * globalsettings.EvaDefaultResourceAmount);
            RescuePartAddResource(part, globalsettings.Oxygen, fillAmount * globalsettings.OxygenConsumptionRate * globalsettings.EvaDefaultResourceAmount);
            vessel.UpdateResourceSets();
        }

        /// <summary>
        /// Adds TAC LS Resource definition and amount to Rescue Part 
        /// </summary>
        /// <param name="part">the part</param>
        /// <param name="name">resource name</param>
        /// <param name="fillAmount">max amount</param>
        public void RescuePartAddResource(Part part, string name, double fillAmount)
        {
            try
            {
                ConfigNode resourceNode = new ConfigNode("RESOURCE");
                resourceNode.AddValue("name", name);
                resourceNode.AddValue("maxAmount", fillAmount);
                resourceNode.AddValue("amount", fillAmount);
                resourceNode.AddValue("isTweakable", false);
                //Check part doesn't have resource already. If it does remove it first, then re-add it.
                if (part.Resources.Contains(name))
                {
                    part.Resources.Remove(name);
                }
                PartResource resource = part.AddResource(resourceNode);
                resource.flowState = true;
                resource.flowMode = PartResource.FlowMode.Both;
            }
            catch (Exception ex)
            {
                if (!ex.Message.Contains("Object reference not set"))
                {
                    this.LogError("Unexpected error while adding resource " + name + " to the RescuePart: " + ex.Message + "\n" + ex.StackTrace);
                }
            }
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
                ScreenMessages.PostScreenMessage(Localizer.Format("#autoLOC_TACLS_00040", oldPart.vessel.id), 10.0f, ScreenMessageStyle.UPPER_CENTER); //#autoLOC_TACLS_00040 = Error - EmptyEvaSuit - Cannot find Vessel Info for <<1>>
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

        #endregion

        #region GameEvents

        /// <summary>
        /// called on Part undock. but we are using it here for when a kerbal leaves an external seat.
        /// If the kerbal is exiting a CommandSeat will go through their Life Support resources and fill them up
        /// from the vessel the CommandSeat is a part of. Will also leave behind waste resources.
        /// </summary>
        /// <param name="kerbal"></param>
        /// <param name="entering"></param>
        private void onPartUndock(Part part)
        {
            // Find partModule KerbalEVA on part. If not there we are not interested.
            PartModule kerbalEvaModule = part.FindModuleImplementing<KerbalEVA>();
            if (kerbalEvaModule == null)
            {
                return;
            }
            KerbalEVA kerbalEVA = kerbalEvaModule as KerbalEVA;

            //Create oldVsl PartSet and newVsl PartSet to move resouces around.
            HashSet<Part> oldParts = new HashSet<Part>();
            for (int j = 0; j < part.vessel.Parts.Count; j++)
            {
                if (part.vessel.Parts[j] != part)
                {
                    oldParts.Add(part.vessel.Parts[j]);
                }
            }
            PartSet oldVslPartSet = new PartSet(oldParts);
            HashSet<Part> NewParts = new HashSet<Part>();
            NewParts.Add(part);
            PartSet newVslPartSet = new PartSet(NewParts);

            //loop through kerbal part Resources to top them up.
            for (int i = 0; i < kerbalEVA.part.Resources.Count; i++)
            {
                //Set the resourceID
                int resourceID = 0;
                if (kerbalEVA.part.Resources[i].resourceName == globalsettings.Food)
                {
                    resourceID = globalsettings.FoodId;
                }
                if (kerbalEVA.part.Resources[i].resourceName == globalsettings.Oxygen)
                {
                    resourceID = globalsettings.OxygenId;
                }
                if (kerbalEVA.part.Resources[i].resourceName == globalsettings.Water)
                {
                    resourceID = globalsettings.WaterId;
                }
                if (kerbalEVA.part.Resources[i].resourceName == globalsettings.Electricity)
                {
                    resourceID = globalsettings.ElectricityId;
                }
                if (kerbalEVA.part.Resources[i].resourceName == globalsettings.WasteWater)
                {
                    resourceID = globalsettings.WasteWaterId;
                }
                if (kerbalEVA.part.Resources[i].resourceName == globalsettings.Waste)
                {
                    resourceID = globalsettings.WasteId;
                }
                if (kerbalEVA.part.Resources[i].resourceName == globalsettings.CO2)
                {
                    resourceID = globalsettings.CO2Id;
                }

                if (resourceID == globalsettings.FoodId ||
                    resourceID == globalsettings.WaterId ||
                    resourceID == globalsettings.OxygenId ||
                    resourceID == globalsettings.ElectricityId)
                {
                    //If it's a TAC LS resource and we have room. Top it up.
                    double missingAmount = kerbalEVA.part.Resources[i].maxAmount - kerbalEVA.part.Resources[i].amount;
                    if (missingAmount > 0)
                    {

                        //Get resource from oldVslPartSet
                        double amtReceived = oldVslPartSet.RequestResource(part.vessel.rootPart, resourceID, missingAmount, true);
                        //Top up the newVslPartSet
                        double amtPut = newVslPartSet.RequestResource(part, resourceID, -amtReceived, true);
                    }
                }

                if (resourceID == globalsettings.CO2Id ||
                    resourceID == globalsettings.WasteId ||
                    resourceID == globalsettings.WasteWaterId)
                {
                    //If it's a TAC LS resource Leave it behind.
                    if (kerbalEVA.part.Resources[i].amount > 0)
                    {
                        //Get resource from newVslPartSet
                        double amtReceived = newVslPartSet.RequestResource(part, resourceID, kerbalEVA.part.Resources[i].amount, true);
                        //Top up the oldVslPartSet
                        double amtPut = oldVslPartSet.RequestResource(part.vessel.rootPart, resourceID, -amtReceived, true);
                    }
                }
            }
        }
        
        /// <summary>
        /// Called when GameEvent onLevelWasLoaded is fired.
        /// If the scene is FlightScene check the Dictionaries for errant entries.
        /// </summary>
        private void onLevelWasLoaded(GameScenes scene)
        {
            if (scene == GameScenes.FLIGHT && !checkedDictionaries)
            {
                StartCoroutine(checkDictionaries());
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
            if (gameSettings.knownVessels.Contains(lastVessel.id))
            {
                if (gameSettings.knownVessels[lastVessel.id].recoveryvessel)
                {
                    this.Log("EVA from Recovery Vessel, Remove Recovery Vessel from Tracking");
                    RemoveVesselTracking(lastVessel.id);
                    //Skip FillEvaSuit as recovery vessel Kerbals will have FillRescueSuit called from CreateVesselEntry method.
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
                //resetVesselList(from);
            }
            else
            {
                this.Log("TAC LS Vessel Change Flagged to: " + to.vesselName);
                checkVesselHasCrew(from);
                CreateVesselEntry(to);
                //resetVesselList(to);
            }
            VesselSortCountervslChgFlag = true;
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

        /// <summary>
        /// When a contract is cancelled this method will check if it is a Recover contract and then search for
        /// any Kerbals and their associated vessels and remove them from TAC LS tracking.
        /// </summary>
        /// <param name="contract"></param>
        private void onContractCancelled(Contracts.Contract contract)
        {
            if (contract.GetType() == typeof(RecoverAsset)) //If a RecoverAsset Contract
            {
                if (contract.Title.Contains("Rescue ")) //And the title starts with Rescue
                {
                    //Construct the Kerbals Name and search if we are tracking them and remove them and their vessel
                    string[] words = contract.Title.Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries);
                    if (words.Length >= 3)
                    {
                        string kerbalName = words[1] + " " + words[2];
                        if (gameSettings.knownCrew.Contains(kerbalName))
                        {
                            this.Log("Rescue Contract cancelled for crew member: " + kerbalName);
                            var knowncrew = gameSettings.knownCrew[kerbalName];
                            if (gameSettings.knownVessels.Contains(knowncrew.vesselId))
                            {
                                RemoveVesselTracking(knowncrew.vesselId);
                            }
                            gameSettings.knownCrew.Remove(kerbalName);
                        }
                    }
                }
            }
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
            if (vessel.GetCrewCount() > 0)
            {
                //If we have an entry already, update the tracked crew dictionary.
                if (gameSettings.knownVessels.Contains(vessel.id))
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

        private void onVesselSituationChange(GameEvents.HostedFromToAction<Vessel, Vessel.Situations> evt)
        {
            this.Log("Vessel situation change");
        }

        #endregion

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

            public void ChangeActiveVessel(Vessel activeVessel)
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
            ScreenMessages.PostScreenMessage(Localizer.Format("#autoLOC_TACLS_00041", vesselName, crewMember.name, causeOfDeath), 15.0f, ScreenMessageStyle.UPPER_CENTER); // #autoLOC_TACLS_00041 = <<1>> <<2>> died of <<3>>!
            this.Log(vessel.vesselName + " - " + crewMember.name + " died of " + causeOfDeath + "!");

            if (!vessel.isEVA)
            {
                if (vessel.loaded)
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
                    ProtoPartSnapshot part = vessel.protoVessel.protoPartSnapshots.Find(p => p.protoModuleCrew.Contains(crewMember));
                    if (part != null)
                    {
                        part.RemoveCrew(crewMember);
                        crewMember.Die();

                        if (HighLogic.CurrentGame.Parameters.Difficulty.MissingCrewsRespawn)
                        {
                            crewMember.StartRespawnPeriod(settings_sec1.respawnDelay);
                        }
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

    }
}
