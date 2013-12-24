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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Tac
{
    /*
     * Example config file:
     *    MODULE
     *    {
     *       name = TacGenericConverter
     *
     *       // Displayed when right clicking the part
     *       converterName = Carbon Extractor
     *
     *       // Number of units to convert per day (24 hours)
     *       conversionRate = 8
     *
     *       // A comma separated list of resources to use as inputs.
     *       // For each resource, list the resource name and the amount (which
     *       // is multiplied by the conversionRate)
     *       inputResources = CarbonDioxide, 1, ElectricCharge, 1000
     *
     *       // A comma separated list of resources to output. Same as above
     *       // but also specify whether it should keep converting if the
     *       // resource is full (generating excess that will be thrown away).
     *       outputResources = Oxygen, 0.9, false, Waste, 2.218, true
     *    }
     * or
     *    MODULE
     *    {
     *       name = TacGenericConverter
     *       converterName = Greenhouse
     *       conversionRate = 1
     *       inputResources = Water, Waste, CarbonDioxide, ElectricCharge,
     *       outputResources = Oxygen, WasteWater
     *    }
     */
    public class TacGreenhouse : PartModule
    {
        private static char[] delimiters = { ' ', ',', '\t', ';' };
        private const int SECONDS_PER_DAY = 24 * 60 * 60;

        [KSPField]
        public string converterName = "TAC Greenhouse";

        [KSPField(guiActive = true, guiName = "Greenhouse Status")]
        public string converterStatus = "Unplanted";

        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true),
         UI_FloatRange(minValue = 0.1f, maxValue = 10.0f, stepIncrement = 0.1f)]
        public float conversionRate = 1.0f;

        [KSPField]
        public string inputResources = "";

        [KSPField]
        public string outputResources = "";

        [KSPField(guiActive = true, guiActiveEditor = true)]
        public string biomassName = "PlantBiomass";

        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true),
         UI_FloatRange(minValue = 0, maxValue = 10, stepIncrement = 1)]
        public float requiredCrew = 1;

        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true),
         UI_FloatRange(minValue = 0.01f, maxValue = 0.25f, stepIncrement = 0.01f)]
        public float biomassGrowthRate = 0.05f;

        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true),
         UI_FloatRange(minValue = 0.01f, maxValue = 0.25f, stepIncrement = 0.01f)]
        public float biomassDieOffRate = 0.10f;

        [KSPField(guiActive = true, guiFormat = "F3")]
        private double lastUpdateTime = 0.0;

        private List<ResourceRatio> inputResourceList;
        private List<ResourceRatio> outputResourceList;

        public override void OnAwake()
        {
            this.Log("OnAwake");
            base.OnAwake();
            UpdateResourceLists();
        }

        public override void OnStart(PartModule.StartState state)
        {
            this.Log("OnStart: " + state);
            base.OnStart(state);

            if (state != StartState.Editor)
            {
                part.force_activate();
            }
        }

        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();

            if (Time.timeSinceLevelLoad < 1.0f || !FlightGlobals.ready)
            {
                return;
            }

            if (lastUpdateTime == 0.0f)
            {
                // Just started running
                lastUpdateTime = Planetarium.GetUniversalTime();
                return;
            }

            GlobalSettings globalSettings = TacLifeSupport.Instance.globalSettings;

            double deltaTime = Math.Min(Planetarium.GetUniversalTime() - lastUpdateTime, globalSettings.MaxDeltaTime);
            lastUpdateTime += deltaTime;

            if (!part.Resources.Contains(biomassName))
            {
                this.Log("Unknown resource " + biomassName);
                return;
            }

            PartResource biomassResource = part.Resources[biomassName];
            double currentBiomass = biomassResource.amount;
            double maxBiomass = biomassResource.maxAmount;

            if (currentBiomass > 0.0)
            {
                double maxGrowth = Math.Max(currentBiomass * biomassGrowthRate, maxBiomass * 0.01) / SECONDS_PER_DAY * deltaTime;
                double maxDieOff = Math.Max(currentBiomass * biomassDieOffRate, maxBiomass * 0.01) / SECONDS_PER_DAY * deltaTime;

                // Start off with the assumption that the plants have plenty of everything and are going to grow
                double growth = maxGrowth;

                // Determine if there is enough crew to care for the plants
                double adjustedMaxBiomass;
                int numCrew = vessel.GetCrewCount();
                if (requiredCrew > 0 && numCrew < requiredCrew)
                {
                    adjustedMaxBiomass = maxBiomass * numCrew / requiredCrew;
                }
                else
                {
                    adjustedMaxBiomass = maxBiomass;
                }

                if (currentBiomass > adjustedMaxBiomass)
                {
                    double amountOverMax = currentBiomass - adjustedMaxBiomass;
                    growth = Math.Min(-Math.Min(amountOverMax, maxDieOff), growth);
                }
                else
                {
                    double amountUnderMax = adjustedMaxBiomass - currentBiomass;
                    growth = Math.Min(Math.Min(amountUnderMax, maxGrowth), growth);
                }

                // Consume the inputs
                double desiredAmount = conversionRate / SECONDS_PER_DAY * deltaTime * (currentBiomass / maxBiomass);
                double percentAvailable = 1.0f;

                foreach (ResourceRatio input in inputResourceList)
                {
                    double desired = desiredAmount * input.ratio;
                    double actual = part.TakeResource(input.resource, desired);

                    double actualPercent = actual / desired;
                    if (actualPercent < percentAvailable)
                    {
                        percentAvailable = actualPercent;
                    }
                }

                if (percentAvailable > 0.95)
                {
                    // no change, it will grow or die according to the adjusted max biomass calculations above
                }
                else if (percentAvailable < 0.8)
                {
                    // not enough of the inputs, so the plants are dying
                    growth = -maxDieOff;
                }
                else
                {
                    // not enough of the inputs, so the plants are stagnating. No new growth, but not
                    // enough short that they start dying.
                    growth = Math.Min(growth, 0.0);
                }

                // Produce the outputs
                foreach (ResourceRatio output in outputResourceList)
                {
                    double desired = desiredAmount * output.ratio;
                    part.TakeResource(output.resource.id, -desired);
                }

                biomassResource.amount = clamp(currentBiomass + growth, 0.0, maxBiomass);

                // Update the status
                if (growth > 0.0)
                {
                    converterStatus = "Growing";
                }
                else if (growth < 0.0)
                {
                    converterStatus = "Dying";
                }
                else
                {
                    converterStatus = "Stagnant";
                }
            }
            else if (converterStatus != "Unplanted")
            {
                converterStatus = "Dead";
            }
        }

        public override void OnLoad(ConfigNode node)
        {
            this.Log("OnLoad: " + node);
            base.OnLoad(node);
            lastUpdateTime = Utilities.GetValue(node, "lastUpdateTime", lastUpdateTime);

            UpdateResourceLists();
        }

        public override void OnSave(ConfigNode node)
        {
            base.OnSave(node);
            node.AddValue("lastUpdateTime", lastUpdateTime);
            this.Log("OnSave: " + node);
        }

        public override string GetInfo()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(base.GetInfo());
            sb.Append("\nContains the ");
            sb.Append(converterName);
            sb.Append(" module\n  Inputs: ");
            sb.Append(String.Join(", ", inputResourceList.Select(value => value.resource.name + ", " + value.ratio).ToArray()));
            sb.Append("\n  Outputs: ");
            sb.Append(String.Join(", ", outputResourceList.Select(value => value.resource.name + ", " + value.ratio).ToArray()));
            sb.Append("\n  Conversion Rate: ");
            sb.Append(conversionRate);
            sb.Append("\n");

            return sb.ToString();
        }

        [KSPEvent(active = true, guiActive = true, guiName = "Replant")]
        public void ReplantEvent()
        {
            // Only replant if it does not require any crew or there is at least one Kerbal on board
            if (requiredCrew > 0 && !vessel.parts.Any(p => p.protoModuleCrew.Any()))
            {
                ScreenMessages.PostScreenMessage("Need crew to replant!", 5.0f, ScreenMessageStyle.UPPER_CENTER);
                return;
            }

            PartResource biomassResource = part.Resources[biomassName];
            biomassResource.amount = Math.Max(biomassResource.maxAmount * 0.05f, biomassResource.amount);
            converterStatus = "Replanted";
        }

        [KSPAction("Replant")]
        public void ReplantAction(KSPActionParam param)
        {
            // Only replant if it does not require any crew or there is at least one Kerbal on board
            if (requiredCrew > 0 && !vessel.parts.Any(p => p.protoModuleCrew.Any()))
            {
                ScreenMessages.PostScreenMessage("Need crew to replant!", 5.0f, ScreenMessageStyle.UPPER_CENTER);
                return;
            }

            PartResource biomassResource = part.Resources[biomassName];
            biomassResource.amount = Math.Max(biomassResource.maxAmount * 0.05f, biomassResource.amount);
            converterStatus = "Replanted";
        }

        private void UpdateResourceLists()
        {
            if (inputResourceList == null)
            {
                inputResourceList = new List<ResourceRatio>();
            }
            if (outputResourceList == null)
            {
                outputResourceList = new List<ResourceRatio>();
            }

            ParseResourceString(inputResources, inputResourceList);
            ParseResourceString(outputResources, outputResourceList);

            Fields["converterStatus"].guiName = converterName;
        }

        private void ParseResourceString(string resourceString, List<ResourceRatio> resources)
        {
            resources.Clear();

            string[] tokens = resourceString.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < (tokens.Length - 1); i += 2)
            {
                PartResourceDefinition resource = PartResourceLibrary.Instance.GetDefinition(tokens[i]);
                double ratio;
                if (resource != null && double.TryParse(tokens[i + 1], out ratio))
                {
                    resources.Add(new ResourceRatio(resource, ratio));
                }
                else
                {
                    this.Log("Cannot parse \"" + resourceString + "\", something went wrong.");
                }
            }

            var ratios = resources.Aggregate("", (result, value) => result + value.resource.name + ", " + value.ratio + ", ");
            this.Log("Resources parsed: " + ratios + "\nfrom " + resourceString);
        }

        private static double clamp(double value, double minValue, double maxValue)
        {
            return Math.Max(minValue, Math.Min(value, maxValue));
        }
    }
}
