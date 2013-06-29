/**
 * EvaLifeSupportModule.cs
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
    public class EvaLifeSupportModule : PartModule
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
            Debug.Log("TAC Life Support (EvaLifeSupportModule) [" + this.GetInstanceID().ToString("X") + "][" + Time.time + "]: OnAwake");
            base.OnAwake();

            LastUpdateTime = -1;

            TimeFoodRanOut = -1;
            TimeWaterRanOut = -1;
            TimeOxygenRanOut = -1;
            TimeElectricityRanOut = -1;
        }

        public override void OnStart(PartModule.StartState state)
        {
            Debug.Log("TAC Life Support (EvaLifeSupportModule) [" + this.GetInstanceID().ToString("X") + "][" + Time.time + "]: OnStart: " + state);
            base.OnStart(state);

            settings = LifeSupportController.Instance.settings;
        }

        public override void OnLoad(ConfigNode node)
        {
            Debug.Log("TAC Life Support (EvaLifeSupportModule) [" + this.GetInstanceID().ToString("X") + "][" + Time.time + "]: OnLoad");
            base.OnLoad(node);

            LastUpdateTime = Utilities.GetValue(node, "LastUpdateTime", LastUpdateTime);
        }

        public override void OnSave(ConfigNode node)
        {
            Debug.Log("TAC Life Support (EvaLifeSupportModule) [" + this.GetInstanceID().ToString("X") + "][" + Time.time + "]: OnSave");

            node.AddValue("LastUpdateTime", LastUpdateTime);

            base.OnSave(node);
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

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

                if (timeDelta > 0)
                {
                    LastUpdateTime = currentTime;

                    DoUpdate(currentTime, timeDelta);
                }
            }
        }

        private void DoUpdate(double currentTime, double timeDelta)
        {
            // Food
            PartResource food = part.Resources[settings.Food];
            double desiredFood = timeDelta * settings.FoodConsumptionRate / SECONDS_PER_DAY;
            double foodObtained = Math.Min(desiredFood, food.amount);
            food.amount -= foodObtained;
            if (foodObtained < (desiredFood * 0.98))
            {
                if (TimeFoodRanOut == -1)
                {
                    TimeWarp.SetRate(0, true);
                    ScreenMessages.PostScreenMessage(vessel.vesselName + " - LIFE SUPPORT CRITICAL: FOOD DEPLETED!", 10.0f, ScreenMessageStyle.UPPER_CENTER);
                    Debug.Log("TAC Life Support (EvaLifeSupportModule) [" + this.GetInstanceID().ToString("X") + "][" + Time.time + "]: " + vessel.vesselName + " - LIFE SUPPORT CRITICAL: FOOD DEPLETED!");
                    TimeFoodRanOut = currentTime + RandomRespite() - ((desiredFood - foodObtained) / settings.FoodConsumptionRate * SECONDS_PER_DAY);
                }
                else if ((currentTime - TimeFoodRanOut) > settings.MaxTimeWithoutFood)
                {
                    KillCrewMember("starvation");
                }
            }
            else
            {
                TimeFoodRanOut = -1;
            }

            // Water
            PartResource water = part.Resources[settings.Water];
            double desiredWater = timeDelta * settings.WaterConsumptionRate / SECONDS_PER_DAY;
            double waterObtained = Math.Min(desiredWater, water.amount);
            water.amount -= waterObtained;
            if (waterObtained < (desiredWater * 0.98))
            {
                if (TimeWaterRanOut == -1)
                {
                    TimeWarp.SetRate(0, true);
                    ScreenMessages.PostScreenMessage(vessel.vesselName + " - LIFE SUPPORT CRITICAL: WATER DEPLETED!", 10.0f, ScreenMessageStyle.UPPER_CENTER);
                    Debug.Log("TAC Life Support (EvaLifeSupportModule) [" + this.GetInstanceID().ToString("X") + "][" + Time.time + "]: " + vessel.vesselName + " - LIFE SUPPORT CRITICAL: WATER DEPLETED!");
                    TimeWaterRanOut = currentTime + RandomRespite() - ((desiredWater - waterObtained) / settings.WaterConsumptionRate * SECONDS_PER_DAY);
                }
                else if ((currentTime - TimeWaterRanOut) > settings.MaxTimeWithoutWater)
                {
                    KillCrewMember("dehydration");
                }
            }
            else
            {
                TimeWaterRanOut = -1;
            }

            //if (!vessel.orbit.referenceBody.atmosphereContainsOxygen || FlightGlobals.getStaticPressure() < 0.2)
            {
                // Oxygen
                PartResource oxygen = part.Resources[settings.Oxygen];
                double desiredOxygen = timeDelta * settings.OxygenConsumptionRate / SECONDS_PER_DAY;
                double oxygenObtained = Math.Min(desiredOxygen, oxygen.amount);
                oxygen.amount -= oxygenObtained;
                if (oxygenObtained < (desiredOxygen * 0.98))
                {
                    if (TimeOxygenRanOut == -1)
                    {
                        TimeWarp.SetRate(0, true);
                        ScreenMessages.PostScreenMessage(vessel.vesselName + " - LIFE SUPPORT CRITICAL: OXYGEN DEPLETED!", 10.0f, ScreenMessageStyle.UPPER_CENTER);
                        Debug.Log("TAC Life Support (EvaLifeSupportModule) [" + this.GetInstanceID().ToString("X") + "][" + Time.time + "]: " + vessel.vesselName + " - LIFE SUPPORT CRITICAL: OXYGEN DEPLETED!");
                        TimeOxygenRanOut = currentTime + RandomRespite() - ((desiredOxygen - oxygenObtained) / settings.OxygenConsumptionRate * SECONDS_PER_DAY);
                    }
                    else if ((currentTime - TimeOxygenRanOut) > settings.MaxTimeWithoutOxygen)
                    {
                        KillCrewMember("oxygen deprivation");
                    }
                }
                else
                {
                    TimeOxygenRanOut = -1;
                }

                // CO2
                PartResource co2 = part.Resources[settings.CO2];
                double co2Production = oxygenObtained * settings.CO2ProductionRate / settings.OxygenConsumptionRate;
                co2.amount += Math.Min(co2Production, co2.maxAmount - co2.amount);
            }

            // Electricity
            PartResource electricCharge = part.Resources["ElectricCharge"];
            double desiredElectricity = settings.ElectricityConsumptionRate / SECONDS_PER_DAY * timeDelta;
            double electricityObtained = Math.Min(desiredElectricity, electricCharge.amount);
            electricCharge.amount -= electricityObtained;
            if (electricityObtained < (desiredElectricity * 0.98))
            {
                if (TimeElectricityRanOut == -1)
                {
                    TimeWarp.SetRate(0, true);
                    ScreenMessages.PostScreenMessage(vessel.vesselName + " - LIFE SUPPORT CRITICAL: ELECTRIC CHARGE DEPLETED!", 10.0f, ScreenMessageStyle.UPPER_CENTER);
                    Debug.Log("TAC Life Support (EvaLifeSupportModule) [" + this.GetInstanceID().ToString("X") + "][" + Time.time + "]: " + vessel.vesselName + " - LIFE SUPPORT CRITICAL: ELECTRIC CHARGE DEPLETED!");
                    TimeElectricityRanOut = currentTime + RandomRespite() - ((desiredElectricity - electricityObtained) / settings.ElectricityConsumptionRate * SECONDS_PER_DAY);
                }
                else if ((currentTime - TimeElectricityRanOut) > settings.MaxTimeWithoutElectricity)
                {
                    KillCrewMember("heat/cold/air stagnation");
                }
            }
            else
            {
                TimeElectricityRanOut = -1;
            }

            // Waste
            PartResource waste = part.Resources[settings.Waste];
            double wasteProduced = foodObtained * settings.WasteProductionRate / settings.FoodConsumptionRate;
            waste.amount += Math.Min(wasteProduced, waste.maxAmount - waste.amount);

            PartResource wasteWater = part.Resources[settings.WasteWater];
            double wasteWaterProduced = waterObtained * settings.WasteWaterProductionRate / settings.WaterConsumptionRate;
            wasteWater.amount += Math.Min(wasteWaterProduced, wasteWater.maxAmount - wasteWater.amount);
        }

        private static int RandomRespite()
        {
            return UnityEngine.Random.Range(30, 120);
        }

        private void KillCrewMember(string causeOfDeath)
        {
            TimeWarp.SetRate(0, true);

            ProtoCrewMember crewMember = part.protoModuleCrew[0];
            crewMember.Die();
            crewMember.rosterStatus = ProtoCrewMember.RosterStatus.DEAD;

            ScreenMessages.PostScreenMessage(vessel.vesselName + " - " + crewMember.name + " died of " + causeOfDeath + "!", 30.0f, ScreenMessageStyle.UPPER_CENTER);
            Debug.Log(vessel.vesselName + " - " + crewMember.name + " died of " + causeOfDeath + "!");

            part.explode();
        }
    }
}
