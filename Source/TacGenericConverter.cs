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
     *       // [Optional] Sets the converter so that it is enabled initially
     *       // so that you do not need to worry about forgeting to turn it on
     *       // before launching.
     *       // converterEnabled = false
     *
     *       // [Optional] When set to true, the converter cannot be disabled
     *       // (or turned off)
     *       // alwaysOn = false
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
     *
     *       // [Optional] When set to true, the converter will not run unless
     *       // on a planet that has oxygen in the atmosphere (only Kerbin or
     *       // Laythe).
     *       requiresOxygenAtmo = false
     *    }
     * or
     *    MODULE
     *    {
     *       name = TacGenericConverter
     *       converterName = Water Purifier
     *       conversionRate = 8
     *       inputResources = WasteWater, 1, ElectricCharge, 1000
     *       outputResources = Water, 0.9, false, Waste, 6.382, true
     *    }
     */
    [KSPModule("TAC Converter")]
    public class TacGenericConverter : PartModule
    {
        private static char[] delimiters = { ' ', ',', '\t', ';' };

        [KSPField]
        public string converterName = "TAC Generic Converter";

        [KSPField(guiActive = true, guiName = "Converter Status")]
        public string converterStatus = "Unknown";

        [KSPField(isPersistant = true)]
        public bool converterEnabled = false;

        [KSPField]
        public bool alwaysOn = false;

        [KSPField]
        public float conversionRate = 1.0f;

        [KSPField]
        public string inputResources = "";

        [KSPField]
        public string outputResources = "";

        [KSPField]
        public bool requiresOxygenAtmo = false;

        private double lastUpdateTime = 0.0f;

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

            UpdateEvents();
        }

        void FixedUpdate()
        {
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

            if (converterEnabled)
            {
                if (requiresOxygenAtmo && !vessel.mainBody.atmosphereContainsOxygen)
                {
                    converterStatus = "Atmo lacks oxygen.";
                    return;
                }

                double desiredAmount = conversionRate * deltaTime;
                double maxElectricityDesired = Math.Min(desiredAmount, conversionRate * Math.Max(globalSettings.ElectricityMaxDeltaTime, TimeWarp.fixedDeltaTime)); // Limit the max electricity consumed when reloading a vessel

                // Limit the resource amounts so that we do not produce more than we have room for, nor consume more than is available
                foreach (ResourceRatio output in outputResourceList)
                {
                    if (!output.allowExtra)
                    {
                        if (output.resource.id == globalSettings.ElectricityId && desiredAmount > maxElectricityDesired)
                        {
                            // Special handling for electricity
                            double desiredElectricity = maxElectricityDesired * output.ratio;
                            double availableSpace = -part.IsResourceAvailable(output.resource, -desiredElectricity);
                            desiredAmount = desiredAmount * (availableSpace / desiredElectricity);
                        }
                        else
                        {
                            double availableSpace = -part.IsResourceAvailable(output.resource, -desiredAmount * output.ratio);
                            desiredAmount = availableSpace / output.ratio;
                        }

                        if (desiredAmount <= 0.000000001)
                        {
                            // Out of space, so no need to run
                            converterStatus = "No space for more " + output.resource.name;
                            return;
                        }
                    }
                }

                foreach (ResourceRatio input in inputResourceList)
                {
                    if (input.resource.id == globalSettings.ElectricityId && desiredAmount > maxElectricityDesired)
                    {
                        // Special handling for electricity
                        double desiredElectricity = maxElectricityDesired * input.ratio;
                        double amountAvailable = part.IsResourceAvailable(input.resource, desiredElectricity);
                        desiredAmount = desiredAmount * (amountAvailable / desiredElectricity);
                    }
                    else
                    {
                        double amountAvailable = part.IsResourceAvailable(input.resource, desiredAmount * input.ratio);
                        desiredAmount = amountAvailable / input.ratio;
                    }

                    if (desiredAmount <= 0.000000001)
                    {
                        // Not enough input resources
                        converterStatus = "Not enough " + input.resource.name;
                        return;
                    }
                }

                foreach (ResourceRatio input in inputResourceList)
                {
                    double desired;
                    if (input.resource.id == globalSettings.ElectricityId)
                    {
                        desired = Math.Min(desiredAmount, maxElectricityDesired) * input.ratio;
                    }
                    else
                    {
                        desired = desiredAmount * input.ratio;
                    }

                    double actual = part.TakeResource(input.resource, desired);

                    if (actual < (desired * 0.999))
                    {
                        this.LogWarning("OnFixedUpdate: obtained less " + input.resource.name + " than expected: " + desired.ToString("0.000000000") + "/" + actual.ToString("0.000000000"));
                    }
                }

                foreach (ResourceRatio output in outputResourceList)
                {
                    double desired;
                    if (output.resource.id == globalSettings.ElectricityId)
                    {
                        desired = Math.Min(desiredAmount, maxElectricityDesired) * output.ratio;
                    }
                    else
                    {
                        desired = desiredAmount * output.ratio;
                    }

                    double actual = -part.TakeResource(output.resource.id, -desired);

                    if (actual < (desired * 0.999) && !output.allowExtra)
                    {
                        this.LogWarning("OnFixedUpdate: put less " + output.resource.name + " than expected: " + desired.ToString("0.000000000") + "/" + actual.ToString("0.000000000"));
                    }
                }

                converterStatus = "Running";
            }
        }

        public override void OnLoad(ConfigNode node)
        {
            this.Log("OnLoad: " + node);
            base.OnLoad(node);
            lastUpdateTime = Utilities.GetValue(node, "lastUpdateTime", lastUpdateTime);

            UpdateResourceLists();
            UpdateEvents();
        }

        public override void OnSave(ConfigNode node)
        {
            node.AddValue("lastUpdateTime", lastUpdateTime);
            this.Log("OnSave: " + node);
        }

        public override string GetInfo()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(converterName);
            sb.Append("\n\nInputs:");
            foreach (var input in inputResourceList)
            {
                double ratio = input.ratio * conversionRate;
                sb.Append("\n - ").Append(input.resource.name).Append(": ").Append(Utilities.FormatValue(ratio, 3)).Append("U/sec");
            }
            sb.Append("\n\nOutputs: ");
            foreach (var output in outputResourceList)
            {
                double ratio = output.ratio * conversionRate;
                sb.Append("\n - ").Append(output.resource.name).Append(": ").Append(Utilities.FormatValue(ratio, 3)).Append("U/sec");
            }
            sb.Append("\n");
            if (requiresOxygenAtmo)
            {
                sb.Append("\nRequires an atmosphere containing Oxygen.");
            }
            if (alwaysOn)
            {
                sb.Append("\nCannot be turned off.");
            }

            return sb.ToString();
        }

        [KSPEvent(active = false, guiActive = true, guiActiveEditor = true, guiName = "Activate Converter")]
        public void ActivateConverter()
        {
            converterEnabled = true;
            UpdateEvents();
        }

        [KSPEvent(active = false, guiActive = true, guiActiveEditor = true, guiName = "Deactivate Converter")]
        public void DeactivateConverter()
        {
            converterEnabled = false;
            UpdateEvents();
        }

        [KSPAction("Toggle Converter")]
        public void ToggleConverter(KSPActionParam param)
        {
            converterEnabled = !converterEnabled;
            UpdateEvents();
        }

        private void UpdateEvents()
        {
            if (alwaysOn)
            {
                Events["ActivateConverter"].active = false;
                Events["DeactivateConverter"].active = false;
                converterEnabled = true;
            }
            else
            {
                Events["ActivateConverter"].active = !converterEnabled;
                Events["DeactivateConverter"].active = converterEnabled;

                if (!converterEnabled)
                {
                    converterStatus = "Inactive";
                }
            }
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

            ParseInputResourceString(inputResources, inputResourceList);
            ParseOutputResourceString(outputResources, outputResourceList);

            Events["ActivateConverter"].guiName = "Activate " + converterName;
            Events["DeactivateConverter"].guiName = "Deactivate " + converterName;
            Actions["ToggleConverter"].guiName = "Toggle " + converterName;
            Fields["converterStatus"].guiName = converterName;
        }

        private void ParseInputResourceString(string resourceString, List<ResourceRatio> resources)
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
            this.Log("Input resources parsed: " + ratios + "\nfrom " + resourceString);
        }

        private void ParseOutputResourceString(string resourceString, List<ResourceRatio> resources)
        {
            resources.Clear();

            string[] tokens = resourceString.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < (tokens.Length - 2); i += 3)
            {
                PartResourceDefinition resource = PartResourceLibrary.Instance.GetDefinition(tokens[i]);
                double ratio;
                bool allowExtra;
                if (resource != null && double.TryParse(tokens[i + 1], out ratio) && bool.TryParse(tokens[i + 2], out allowExtra))
                {
                    resources.Add(new ResourceRatio(resource, ratio, allowExtra));
                }
                else
                {
                    this.Log("Cannot parse \"" + resourceString + "\", something went wrong.");
                }
            }

            var ratios = resources.Aggregate("", (result, value) => result + value.resource.name + ", " + value.ratio + ", ");
            this.Log("Output resources parsed: " + ratios + "\nfrom " + resourceString);
        }
    }

    public class ResourceRatio
    {
        public PartResourceDefinition resource;
        public double ratio;
        public bool allowExtra;

        public ResourceRatio(PartResourceDefinition resource, double ratio, bool allowExtra = false)
        {
            this.resource = resource;
            this.ratio = ratio;
            this.allowExtra = allowExtra;
        }
    }
}
