/**
 * LifeSupportModule.cs
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Tac
{
    public class LifeSupportModule : PartModule
    {
        private const int SECONDS_PER_DAY = 24 * 60 * 60;

        private Settings settings;

        public double LastUpdateTime { get; private set; }

        public double TimeFoodRanOut { get; private set; }
        public double TimeWaterRanOut { get; private set; }
        public double TimeOxygenRanOut { get; private set; }
        public double TimeElectricityRanOut { get; private set; }

        public override void OnAwake()
        {
            Debug.Log("TAC Life Support (LifeSupportModule) [" + this.GetInstanceID().ToString("X") + "][" + Time.time + "]: OnAwake");
            base.OnAwake();

            LastUpdateTime = -1;

            TimeFoodRanOut = -1;
            TimeWaterRanOut = -1;
            TimeOxygenRanOut = -1;
            TimeElectricityRanOut = -1;
        }

        public override void OnStart(PartModule.StartState state)
        {
            Debug.Log("TAC Life Support (LifeSupportModule) [" + this.GetInstanceID().ToString("X") + "][" + Time.time + "]: OnStart: " + state);
            base.OnStart(state);

            settings = LifeSupportController.Instance.settings;

            if (state != StartState.Editor)
            {
                part.force_activate();
            }
        }

        public override void OnLoad(ConfigNode node)
        {
            Debug.Log("TAC Life Support (LifeSupportModule) [" + this.GetInstanceID().ToString("X") + "][" + Time.time + "]: OnLoad");
            base.OnLoad(node);

            LastUpdateTime = Utilities.GetValue(node, "LastUpdateTime", LastUpdateTime);
        }

        public override void OnSave(ConfigNode node)
        {
            Debug.Log("TAC Life Support (LifeSupportModule) [" + this.GetInstanceID().ToString("X") + "][" + Time.time + "]: OnSave");

            node.AddValue("LastUpdateTime", LastUpdateTime);

            base.OnSave(node);
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
        }

        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();

            if (vessel.missionTime < 1)
            {
                // Wait until after launch
            }
            else if (LastUpdateTime == -1)
            {
                LastUpdateTime = Planetarium.GetUniversalTime();
                // Wait until the second update
            }
            else
            {
                double currentTime = Planetarium.GetUniversalTime();
                double timeDelta = currentTime - LastUpdateTime;
                LastUpdateTime = currentTime;

                int numCrew = part.protoModuleCrew.Count;
                if (numCrew > 0)
                {
                    DoUpdate(numCrew, currentTime, timeDelta);
                }
            }
        }

        private void DoUpdate(int numCrew, double currentTime, double timeDelta)
        {
            // Food
            double desiredFood = numCrew * timeDelta * settings.FoodConsumptionRate / SECONDS_PER_DAY;
            double foodObtained = part.RequestResource(settings.FoodId, desiredFood);
            if (foodObtained < (desiredFood * 0.98))
            {
                if (TimeFoodRanOut == -1)
                {
                    TimeWarp.SetRate(0, true);
                    ScreenMessages.PostScreenMessage(vessel.vesselName + " - LIFE SUPPORT CRITICAL: FOOD DEPLETED!", 10.0f, ScreenMessageStyle.UPPER_CENTER);
                    Debug.Log("TAC Life Support (LifeSupportModule) [" + this.GetInstanceID().ToString("X") + "][" + Time.time + "]: " + vessel.vesselName + " - LIFE SUPPORT CRITICAL: FOOD DEPLETED!");
                    TimeFoodRanOut = currentTime + RandomRespite() - ((desiredFood - foodObtained) / settings.FoodConsumptionRate / numCrew * SECONDS_PER_DAY);
                }
                else if ((currentTime - TimeFoodRanOut) > settings.MaxTimeWithoutFood)
                {
                    KillCrewMember("starvation");
                    TimeFoodRanOut += RandomRespite();
                }
            }
            else
            {
                TimeFoodRanOut = -1;
            }

            // Water
            double desiredWater = numCrew * timeDelta * settings.WaterConsumptionRate / SECONDS_PER_DAY;
            double waterObtained = part.RequestResource(settings.WaterId, desiredWater);
            if (waterObtained < (desiredWater * 0.98))
            {
                if (TimeWaterRanOut == -1)
                {
                    TimeWarp.SetRate(0, true);
                    ScreenMessages.PostScreenMessage(vessel.vesselName + " - LIFE SUPPORT CRITICAL: WATER DEPLETED!", 10.0f, ScreenMessageStyle.UPPER_CENTER);
                    Debug.Log("TAC Life Support (LifeSupportModule) [" + this.GetInstanceID().ToString("X") + "][" + Time.time + "]: " + vessel.vesselName + " - LIFE SUPPORT CRITICAL: WATER DEPLETED!");
                    TimeWaterRanOut = currentTime + RandomRespite() - ((desiredWater - waterObtained) / settings.WaterConsumptionRate / numCrew * SECONDS_PER_DAY);
                }
                else if ((currentTime - TimeWaterRanOut) > settings.MaxTimeWithoutWater)
                {
                    KillCrewMember("dehydration");
                    TimeWaterRanOut += RandomRespite();
                }
            }
            else
            {
                TimeWaterRanOut = -1;
            }

            if (!vessel.orbit.referenceBody.atmosphereContainsOxygen || FlightGlobals.getStaticPressure() < 0.2)
            {
                // Oxygen
                double desiredOxygen = numCrew * timeDelta * settings.OxygenConsumptionRate / SECONDS_PER_DAY;
                double oxygenObtained = part.RequestResource(settings.OxygenId, desiredOxygen);
                if (oxygenObtained < (desiredOxygen * 0.98))
                {
                    if (TimeOxygenRanOut == -1)
                    {
                        TimeWarp.SetRate(0, true);
                        ScreenMessages.PostScreenMessage(vessel.vesselName + " - LIFE SUPPORT CRITICAL: OXYGEN DEPLETED!", 10.0f, ScreenMessageStyle.UPPER_CENTER);
                        Debug.Log("TAC Life Support (LifeSupportModule) [" + this.GetInstanceID().ToString("X") + "][" + Time.time + "]: " + vessel.vesselName + " - LIFE SUPPORT CRITICAL: OXYGEN DEPLETED!");
                        TimeOxygenRanOut = currentTime + RandomRespite() - ((desiredOxygen - oxygenObtained) / settings.OxygenConsumptionRate / numCrew * SECONDS_PER_DAY);
                    }
                    else if ((currentTime - TimeOxygenRanOut) > settings.MaxTimeWithoutOxygen)
                    {
                        KillCrewMember("oxygen deprivation");
                        TimeOxygenRanOut += RandomRespite();
                    }
                }
                else
                {
                    TimeOxygenRanOut = -1;
                }

                // CO2
                double co2Production = oxygenObtained * settings.CO2ProductionRate / settings.OxygenConsumptionRate;
                part.RequestResource(settings.CO2Id, -co2Production);
            }

            // Electricity
            double desiredElectricity = ((numCrew * settings.ElectricityConsumptionRate) + settings.BaseElectricityConsumptionRate) / SECONDS_PER_DAY * timeDelta;
            double electricityObtained = part.RequestResource(settings.ElectricityId, desiredElectricity);
            if (electricityObtained < (desiredElectricity * 0.98))
            {
                if (TimeElectricityRanOut == -1)
                {
                    TimeWarp.SetRate(0, true);
                    ScreenMessages.PostScreenMessage(vessel.vesselName + " - LIFE SUPPORT CRITICAL: ELECTRIC CHARGE DEPLETED!", 10.0f, ScreenMessageStyle.UPPER_CENTER);
                    Debug.Log("TAC Life Support (LifeSupportModule) [" + this.GetInstanceID().ToString("X") + "][" + Time.time + "]: " + vessel.vesselName + " - LIFE SUPPORT CRITICAL: ELECTRIC CHARGE DEPLETED!");
                    TimeElectricityRanOut = currentTime + RandomRespite();// -((desiredElectricity - electricityObtained) / (settings.ElectricityConsumptionRate * numCrew + settings.BaseElectricityConsumptionRate) * SECONDS_PER_DAY);
                }
                else if ((currentTime - TimeElectricityRanOut) > settings.MaxTimeWithoutElectricity)
                {
                    KillCrewMember("heat/cold/air stagnation");
                    TimeElectricityRanOut += RandomRespite();
                }
            }
            else
            {
                TimeElectricityRanOut = -1;
            }

            // Waste
            double wasteProduced = foodObtained * settings.WasteProductionRate / settings.FoodConsumptionRate;
            double wasteWaterProduced = waterObtained * settings.WasteWaterProductionRate / settings.WaterConsumptionRate;
            part.RequestResource(settings.WasteId, -wasteProduced);
            part.RequestResource(settings.WasteWaterId, -wasteWaterProduced);
        }

        private static int RandomRespite()
        {
            return UnityEngine.Random.Range(1, 30); // 30, 120);
        }

        private void KillCrewMember(string causeOfDeath)
        {
            TimeWarp.SetRate(0, true);
            if (CameraManager.Instance.currentCameraMode == CameraManager.CameraMode.IVA)
            {
                CameraManager.Instance.SetCameraFlight();
            }

            List<ProtoCrewMember> crew = part.protoModuleCrew;
            int crewMemberIndex = UnityEngine.Random.Range(0, crew.Count - 1);
            ProtoCrewMember crewMember = crew[crewMemberIndex];
            crewMember.Die();
            crewMember.rosterStatus = ProtoCrewMember.RosterStatus.DEAD;

            ScreenMessages.PostScreenMessage(vessel.vesselName + " - " + crewMember.name + " died of " + causeOfDeath + "!", 30.0f, ScreenMessageStyle.UPPER_CENTER);
            Debug.Log(vessel.vesselName + " - " + crewMember.name + " died of " + causeOfDeath + "!");

            part.RemoveCrewmember(crewMember);
        }
    }
}
