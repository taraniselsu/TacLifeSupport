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
        private TacGameSettings gameSettings;
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

                var crew = HighLogic.CurrentGame.CrewRoster.Crew;
                var knownCrew = gameSettings.knownCrew;
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

            var vesselsToDelete = new List<Guid>();//vessels that no longer exist
            var homelessCrew = new List<String>();//Crew who's last know ship was deleted (but could show up elsewhere)
            List<String> homedCrew = new List<String>();//crew that have a home already

            foreach (var entry in knownVessels)
            {
                Guid vesselId = entry.Key;
                VesselInfo vesselInfo = entry.Value;
                Vessel vessel = allVessels.Find(v => v.id == vesselId);

                if (vessel == null)
                {
                    this.Log("Deleting vessel " + vesselInfo.vesselName + " - vessel does not exist anymore");
                    vesselsToDelete.Add(vesselId);
                    List<String> crewToDelete = gameSettings.knownCrew.Where(e => e.Value.vesselId == vesselId)
                        .Select(e => e.Key)
                        .Where(name => !homedCrew.Contains(name)).ToList();
                    //If this crewmember is't found in a different vessel he will be deleted
                    homelessCrew.AddRange(crewToDelete);
                    continue;
                }

                if (vessel.loaded)
                {
                    int crewCapacity = UpdateVesselInfoLoaded(vesselInfo, vessel);

                    if (crewCapacity == 0)
                    {
                        this.Log("Deleting vessel " + vesselInfo.vesselName + " - no crew parts anymore");
                        vesselsToDelete.Add(vesselId);
                        continue;
                    }

                    foreach (String crewName in vesselInfo.crew.Keys)
                    {
                        homelessCrew.Remove(crewName);
                        homedCrew.Add(crewName);
                    }
                }
                else
                {
                    UpdateVesselInfoUnloaded(vesselInfo, vessel);
                }



                //What resources get used theis period?
                //copy the current resources
                Dictionary<int, ResourceLimits> resources = new Dictionary<int, ResourceLimits>();
                foreach (KeyValuePair<int, ResourceLimits> resAvail in vesselInfo.resourceLimits) {
                    resources.Add(resAvail.Key, resAvail.Value.clone());
                }
                //LogResources("last known", vesselInfo.lastKnownAmounts);
                //LogResources("before kerbals", resources);

                //TODO remove MaxDeltaTime?
                double deltaTime = Math.Min(currentTime - vesselInfo.lastUpdate, globalSettings.MaxDeltaTime);

                //Crew needs come first
                HashSet<CrewMemberInfo> vesselCrew=new HashSet<CrewMemberInfo>();
                
                int numActiveCrew = 0;
                foreach (ProtoCrewMember crew in vessel.GetVesselCrew()) {
                    String crewName = crew.name;
                    CrewMemberInfo info = gameSettings.knownCrew[crewName];
                    vesselCrew.Add(info);
                    if (!info.hibernating) numActiveCrew++;
                }

                //Calculate crew resource consumption
                crewConsumption(resources, deltaTime, numActiveCrew);
				//TODO perCapsule and differnt EVA electicity consumption
                //LogResources("after kerbals", resources);

                //calcuate power generation as best we can when unloaded
                if (!vessel.loaded)
                {
                    calculatePowerGeneration(vessel, vesselInfo, resources, deltaTime);
                }


                //now the converters
                foreach (TacGenericConverter converter in vesselInfo.converters)
                {
                    converter.recycle(resources, deltaTime);
                }

                //LogResources("after recycle", resources);
                //If on kerbin atmosphere the crew can get any shortfall of oxygen from the atmosphere
                if (!NeedOxygen(vessel, vesselInfo))
                {
                    resources[globalSettings.OxygenId].available = Math.Max(resources[globalSettings.OxygenId].available, 
                        vesselInfo.resourceLimits[globalSettings.OxygenId].available);
                }

                Dictionary<int, double> rates = new Dictionary<int, double>();
                foreach (int resource in resources.Keys)
                {
                    rates[resource] = resources[resource].available - vesselInfo.resourceLimits[resource].available;
                }
                //LogResources("rates", rates);

                calculateCrewReserves(vessel, resources, numActiveCrew);

                //Wake up hibernating kerbals that have full reserves
                wakeUpHibernaters(vessel);

                //update estimates
                UpdateDepletionEstimates(currentTime, vesselInfo, vessel, resources, deltaTime, rates);

                if (vessel.loaded)
                {
                    ConsumeResources(vessel, vesselInfo, resources);
                }
                else
                {
                    vesselInfo.loaded = false;
                }
                vesselInfo.resourceLimits = resources;
                vesselInfo.lastUpdate = currentTime;

            }

            vesselsToDelete.ForEach(id => knownVessels.Remove(id));
            foreach (String homeless in homelessCrew)
            {
                this.Log(homeless + " is homelesss");
            }
            homelessCrew.ForEach(crewName => gameSettings.knownCrew.Remove(crewName));

            foreach (Vessel vessel in allVessels.Where(v => v.loaded))
            {
                if (!knownVessels.ContainsKey(vessel.id) && vessel.parts.Any(p => p.protoModuleCrew.Count > 0) && IsLaunched(vessel))
                {
                    this.Log("New vessel: " + vessel.vesselName + " (" + vessel.id + ")");
                    var knownCrew = gameSettings.knownCrew;

                    if (vessel.isEVA)
                    {
                        ProtoCrewMember crewMember = vessel.GetVesselCrew().FirstOrDefault();
                        if (crewMember != null && !knownCrew.ContainsKey(crewMember.name))
                        {
                            FillRescueEvaSuit(vessel);
                        }
                    }

                    VesselInfo vesselInfo = new VesselInfo(vessel.vesselName, currentTime);
                    vesselInfo.loaded = true;//true says that the actual resources are corect and we should use real converters
                    knownVessels[vessel.id] = vesselInfo;

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

        private void UpdateDepletionEstimates(double currentTime, VesselInfo vesselInfo, Vessel vessel, 
            Dictionary<int, ResourceLimits> resources, double deltaTime, Dictionary<int, double> originalRates)
        {
            foreach (int resource in globalSettings.kerbalRequirements)
            {
                double produced = originalRates[resource];
                if (resources[resource].available > 1e-7)
                {
                    if (produced >= 0)//net production of resource
                    {
                        vesselInfo.depletionEstimates[resource] = double.PositiveInfinity;
                    }
                    else
                    {
                        vesselInfo.depletionEstimates[resource] = currentTime + resources[resource].available / (-produced / deltaTime);
                    }
                }
                else
                {
                    var activeCrew = vesselInfo.crew.Where(c => !c.Value.hibernating);
                    double minKerbalReserves = activeCrew.Min(c => c.Value.reserves[resource].available);
                    int activeCount = activeCrew.Count();
                    //we are into kerbl reserves
                    if (produced < 0)
                    {
                        //and the reserves are going down
                        double timeToLive = minKerbalReserves / -produced * activeCount * deltaTime;
                        vesselInfo.depletionEstimates[resource] = currentTime + timeToLive - globalSettings.kerbalStarvationTimes[resource];
                    }
                    else
                    {
                        //Reserves are being filled up
                        double space = globalSettings.kerbalStarvationTimes[resource] * - globalSettings.kerbalProductionRates[resource] - minKerbalReserves;
                        double timeToFill = space / produced * activeCount * deltaTime;
                        vesselInfo.depletionEstimates[resource] = currentTime - timeToFill; 
                    }
                }
            }
            ShowWarnings(vessel, vesselInfo, currentTime);
        }

        private void crewConsumption(Dictionary<int, ResourceLimits> resources, double deltaTime, int numActiveCrew)
        {
            if (numActiveCrew > 0)
            {

                foreach (KeyValuePair<int, double> resourceRate in globalSettings.kerbalProductionRates)
                {
                    double amount = resourceRate.Value * numActiveCrew * deltaTime;
                    resources[resourceRate.Key].add(amount);
                }
            }
        }

        private void calculatePowerGeneration(Vessel vessel, VesselInfo vesselInfo, Dictionary<int, ResourceLimits> resources, double deltaTime)
        {
            foreach (IProtoElecComponent elecComp in vesselInfo.elecProtoComponents)
            {
                elecComp.generate(resources, deltaTime, vessel);
            }
        }

        private void calculateCrewReserves(Vessel vessel, Dictionary<int, ResourceLimits> resources, int numActiveCrew)
        {
            //Deduct shortfalls from kerbals reserves
            //kill/hibernate the first kerbal who runs out
            foreach (int resource in globalSettings.kerbalRequirements)
            {
                if (resources[resource].available < 0)
                {
                    int crewToGo = numActiveCrew;
                    foreach (ProtoCrewMember crew in vessel.GetVesselCrew())
                    {
                        CrewMemberInfo crewInfo = gameSettings.knownCrew[crew.name];
                        if (crewInfo.hibernating)
                        {
                            continue;
                        }
                        double share = resources[resource].available / crewToGo--;//this crew members share of the shortfall
                        ResourceLimits reserve = crewInfo.reserves[resource];
                        share = Math.Max(share, -reserve.available);//cant remove more that the reserves he has
                        reserve.available += share;
                        String name=PartResourceLibrary.Instance.GetDefinition(resource).name;
                        if (reserve.available <= 0)
                        {
                            //reserves has run out - this kerbal dies/hibernates
                            //they die at the start of the tick and dont use this resource this tick
                            resources[resource].available -= globalSettings.kerbalProductionRates[resource];
                            if (gameSettings.HibernateInsteadOfKill)
                            {
                                this.Log(crew.name + " is going into hibernation");
                                crewInfo.hibernating = true;
                            }
                            else
                            {
                                KillCrewMember(crew, globalSettings.deathCauses[resource], vessel);
                            }
                        }
                        else
                        {
                            //a share of the deficit is taken from this kerbal
                            resources[resource].available += share;
                        }
                    }
                }
                else
                {
                    //We have a surplus do any kerbals need topping up?
                    int crewCount = vessel.GetVesselCrew().Count;
                    foreach (ProtoCrewMember crew in vessel.GetVesselCrew())
                    {
                        CrewMemberInfo crewInfo = gameSettings.knownCrew[crew.name];
                        double share = resources[resource].available / (crewCount--);//share remaining reserves between remaining crew
                        share = Math.Min(share, crewInfo.reserves[resource].getSpace());
                        crewInfo.reserves[resource].available += share;
                        resources[resource].available -= share;
                    }
                }
            }
        }

        private void wakeUpHibernaters(Vessel vessel)
        {
            foreach (ProtoCrewMember crew in vessel.GetVesselCrew())
            {
                CrewMemberInfo crewInfo = gameSettings.knownCrew[crew.name];
                if (crewInfo.hibernating)
                {
                    Boolean full = true;
                    foreach (int resource in globalSettings.kerbalRequirements)
                    {
                        if (crewInfo.reserves[resource].available<(crewInfo.reserves[resource].maximum*.5))//wake up again when reserves are half used
                        {
                            full = false;
                        }
                    }
                    if (full)
                    {
                        this.Log(crew.name + " is waking up!");
                        crewInfo.hibernating = false;
                    }
                }
            }
        }

        private void ConsumeResources(Vessel vessel, VesselInfo vesselInfo, Dictionary<int, ResourceLimits> after)
        {
            foreach (KeyValuePair<int, ResourceLimits> resource in after)
            {
                double produced;
                produced = resource.Value.available - vesselInfo.lastKnownAmounts[resource.Key];
                vesselInfo.lastKnownAmounts[resource.Key] = resource.Value.available;
                if (Math.Abs(produced) > 1e-8)
                {
                    //TODO better part target for consumption?
                    double taken = vessel.rootPart.TakeResource(resource.Key, -produced);
                }
            }

            vesselInfo.vesselName = vessel.vesselName;
            vesselInfo.vesselType = vessel.vesselType;
            vesselInfo.loaded = true;
        }    
            

        private int UpdateVesselInfoLoaded(VesselInfo vesselInfo, Vessel vessel)
        {
            if (vesselInfo.loaded)
            {
                //if the vessel isn't already loaded then the estimates are more up to date than the actual
                //otherwise get the actual amounts available
                vesselInfo.ClearAmounts();
                foreach (Part part in vessel.parts)
                {
                    foreach (PartResource resource in part.Resources)
                    {
                        if (resource.flowState)
                        {
                            if (!vesselInfo.resourceLimits.ContainsKey(resource.info.id))
                            {
                                vesselInfo.resourceLimits[resource.info.id] = new ResourceLimits(0, 0); ;
                            }
                            ResourceLimits limits = vesselInfo.resourceLimits[resource.info.id];
                            limits.available += resource.amount;
                            limits.maximum += resource.maxAmount;
                        }
                    }
                } 
                foreach (var resource in vesselInfo.resourceLimits)
                {
                    vesselInfo.lastKnownAmounts[resource.Key] = resource.Value.available;
                }
            }
            
            
            //Refresh converter information - docking might have changed it since last frame
            vesselInfo.converters=null;
            vesselInfo.elecProtoComponents = null;

            List<TacGenericConverter> converters = vessel.FindPartModulesImplementing<TacGenericConverter>();
            vesselInfo.converters = converters;

            //refresh crew informtion
            int crewCapacity = 0;
            vesselInfo.crew = new Dictionary<string, CrewMemberInfo>();
            foreach (Part part in vessel.parts)
            {
                crewCapacity += part.CrewCapacity;
                if (part.protoModuleCrew.Count > 0)
                {
                    foreach (ProtoCrewMember crew in part.protoModuleCrew)
                    {
                        if (!vesselInfo.crew.ContainsKey(crew.name))
                        {
                            vesselInfo.crew[crew.name] = loadCrewMember(vessel, crew);
                        }
                    }
                    ++vesselInfo.numOccupiedParts;
                }
            }
            vesselInfo.crewCapacity = crewCapacity;
            return crewCapacity;
        }

        private CrewMemberInfo loadCrewMember(Vessel vessel, ProtoCrewMember crew)
        {
            CrewMemberInfo crewMemberInfo;
            if (gameSettings.knownCrew.ContainsKey(crew.name))
            {
                //a kerbal we already know about - likely has just transfered from another ship
                crewMemberInfo = gameSettings.knownCrew[crew.name];
                crewMemberInfo.vesselId = vessel.id;
                crewMemberInfo.vesselName = vessel.vesselName;
            }
            else
            {
                crewMemberInfo = new CrewMemberInfo(crew.name, vessel.vesselName, vessel.id, 0);
            }
            return crewMemberInfo;
        }

        private int UpdateVesselInfoUnloaded(VesselInfo vesselInfo, Vessel vessel)
        {
            if (vesselInfo.loaded || vesselInfo.converters==null)
            {
                generateProxies(vesselInfo, vessel);

            }
            return vesselInfo.crewCapacity;
        }

        private void generateProxies(VesselInfo vesselInfo, Vessel vessel)
        {
            List<TacGenericConverter> converters = new List<TacGenericConverter>();
            List<IProtoElecComponent> elecComponents = new List<IProtoElecComponent>();
            List<ProtoPartSnapshot> parts = vessel.protoVessel.protoPartSnapshots;
            int crewCapacity = 0;
            foreach (ProtoPartSnapshot snap in parts)
            {
                foreach (ProtoPartModuleSnapshot mod in snap.modules) {
                    foreach (ProtoCrewMember crew in snap.protoModuleCrew)
                    {
                        vesselInfo.crew[crew.name] = loadCrewMember(vessel, crew);
                    }
                    Part p = snap.partInfo.partPrefab;
                    crewCapacity += p.CrewCapacity;
                    if (mod.moduleName == "TacGenericConverter")
                    {
                        TacGenericConverter converter = ((TacGenericConverter)p.Modules["TacGenericConverter"]).Clone();
                        converter.OnLoad(mod.moduleValues);
                        converters.Add(converter);
                    }
                    else if (mod.moduleName == "ModuleGenerator")
                    {
                        ModuleGenerator gen = (ModuleGenerator)p.Modules["ModuleGenerator"];
                        IProtoElecComponent protoGen = new ProtoModuleGenerator(gen);
                        protoGen.OnLoad(mod.moduleValues);
                        elecComponents.Add(protoGen);
                    }
                    else if (mod.moduleName == "ModuleDeployableSolarPanel")
                    {
                        ModuleDeployableSolarPanel sp = (ModuleDeployableSolarPanel)p.Modules["ModuleDeployableSolarPanel"];
                        IProtoElecComponent protoPanel = new ProtoSolarPanel(sp);
                        protoPanel.OnLoad(mod.moduleValues);
                        elecComponents.Add(protoPanel);
                    }
                }
            }

            vesselInfo.converters = converters;
            vesselInfo.elecProtoComponents = elecComponents;
            vesselInfo.crewCapacity = crewCapacity;


        }

        private void ShowWarnings(Vessel vessel, VesselInfo vesselInfo, double currentTime)
        {
            if (vesselInfo.crew.Count == 0) return;
            foreach (int resource in globalSettings.kerbalRequirements)
            {
                double estimatedTime = vesselInfo.depletionEstimates[resource] - currentTime;
                VesselInfo.Status status;
                vesselInfo.resourceStatuses.TryGetValue(resource, out status);
                String resourceName = PartResourceLibrary.Instance.GetDefinition(resource).name;
                if (estimatedTime<1)//less than 1 second
                {
                    if (status != VesselInfo.Status.CRITICAL)
                    {
                        ScreenMessages.PostScreenMessage(vessel.vesselName + " - " + resourceName + " depleted!", 10.0f, ScreenMessageStyle.UPPER_CENTER);
                        this.Log(vessel.vesselName + " - " + resourceName + " depleted!");
                        status = VesselInfo.Status.CRITICAL;
                        TimeWarp.SetRate(0, false);
                    }
                }
                else if ((vesselInfo.resourceLimits[resource].available / vesselInfo.resourceLimits[resource].maximum) < 0.1)
                {
                    if (status != VesselInfo.Status.LOW && status!= VesselInfo.Status.CRITICAL)
                    {
                        ScreenMessages.PostScreenMessage(vessel.vesselName + " - " + resourceName + " is running out!", 10.0f, ScreenMessageStyle.UPPER_CENTER);
                        this.Log(vessel.vesselName + " - " + resourceName + " is running out!");
                        TimeWarp.SetRate(0, false);
                    }
                    status = VesselInfo.Status.LOW;
                }
                else
                {
                    status = VesselInfo.Status.GOOD;
                }
                vesselInfo.resourceStatuses[resource] = status;
            }
        }

        /*private double CalculateElectricityConsumptionRate(Vessel vessel, VesselInfo vesselInfo)
        {
			//TODO
            if (!vessel.isEVA)
            {
                return (globalSettings.ElectricityConsumptionRate * vesselInfo.numCrew) + (globalSettings.BaseElectricityConsumptionRate * vesselInfo.numOccupiedParts);
            }
            else
            {
                return globalSettings.EvaElectricityConsumptionRate;
            }
        }*/

        private void FillEvaSuit(Part oldPart, Part newPart)
        {
            Dictionary<int, double> desired = new Dictionary<int, double>();
            foreach (int resource in globalSettings.kerbalRequirements) {
                desired[resource] = -globalSettings.kerbalProductionRates[resource] * globalSettings.EvaDefaultResourceAmount;
            }
            //electicity is special
            desired[globalSettings.ElectricityId] =  globalSettings.EvaElectricityConsumptionRate * globalSettings.EvaDefaultResourceAmount;

            Vessel lastVessel = oldPart.vessel;
            VesselInfo lastVesselInfo;
            if (!gameSettings.knownVessels.TryGetValue(lastVessel.id, out lastVesselInfo))
            {
                this.Log("Unknown vessel: " + lastVessel.vesselName + " (" + lastVessel.id + ")");
                lastVesselInfo = new VesselInfo(lastVessel.vesselName, Planetarium.GetUniversalTime());
            }

            UpdateVesselInfoLoaded(lastVesselInfo, lastVessel);
            int numCrew = lastVesselInfo.crew.Count;
            Dictionary<int, double> obtained = new Dictionary<int,double>();
            foreach (int resource in globalSettings.kerbalRequirements) {
                double toTake = Math.Min(desired[resource], lastVesselInfo.resourceLimits[resource].available / numCrew);
                obtained[resource] = oldPart.TakeResource(resource, toTake);
                String resourceName = PartResourceLibrary.Instance.GetDefinition(resource).name;
                double max = newPart.Resources.Get(resource).maxAmount;
                this.Log("Filling EVA with " + resourceName + " amount " + obtained[resource] + " max="+max);
                newPart.TakeResource(resource, -obtained[resource]);
            }
        }

        private void FillRescueEvaSuit(Vessel vessel)
        {
            this.Log("Rescue mission EVA: " + vessel.vesselName);
            Part part = vessel.rootPart;

            // Only fill the suit to 10-90% full
            double fillAmount = UnityEngine.Random.Range(0.1f, 0.9f);
            foreach (int resource in globalSettings.kerbalRequirements)
            {
                part.TakeResource(resource, fillAmount * globalSettings.kerbalProductionRates[resource] * globalSettings.EvaDefaultResourceAmount);
            }
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
            foreach (int resource in globalSettings.kerbalProductionRates.Keys)
            {
                newPart.TakeResource(resource, -lastVesselInfo.resourceLimits[resource].available);

            }
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
                if (vessel.staticPressure > 0.5)
                {
                    // air pressure is high enough so they can open a window
                    return false;
                }
                //TODO re-integrate this
               /* else if (vessel.staticPressure > 0.2 && vesselInfo.remainingElectricity > vesselInfo.estimatedElectricityConsumptionRate)
                {
                    // air pressure is high enough & have electricity to run vents
                    return false;
                }*/
            }

            return true;
        }

        private bool NeedElectricity(Vessel vessel, VesselInfo vesselInfo)
        {
            //TODO re-integrate this
            
            // Need electricity to survive unless:
            // 1) on Kerbin below a reasonable altitude, so they can open a hatch or window or vent
            if (vessel.mainBody == FlightGlobals.Bodies[1])
            {
                // On or above Kerbin
                if (vessel.staticPressure > 0.5)
                {
                    // air pressure is high enough so they can open a window
                    return false;
                }
            }

            return true;
        }

        private void LogResources(String prefix, Dictionary<int, ResourceLimits> resources)
        {
            StringBuilder build = new StringBuilder(prefix+" ");
            foreach (var resource in resources)
            {
                String resourceName = PartResourceLibrary.Instance.GetDefinition(resource.Key).name;
                build.Append(resourceName);
                build.Append(":");
                build.Append(resource.Value.available);
                build.Append(" ");
            }
            this.Log(build.ToString());
        }

        private void LogResources(String prefix, Dictionary<int, double> resources)
        {
            StringBuilder build = new StringBuilder(prefix + " ");
            foreach (var resource in resources)
            {
                String resourceName = PartResourceLibrary.Instance.GetDefinition(resource.Key).name;
                build.Append(resourceName);
                build.Append(":");
                build.Append(resource.Value);
                build.Append(" ");
            }
            this.Log(build.ToString());
        }
    }
}
