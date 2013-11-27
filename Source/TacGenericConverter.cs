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
     *       converterName = Water Purifier
     *       conversionRate = 8
     *       inputResources = WasteWater, 1, ElectricCharge, 1000
     *       outputResources = Water, 0.9, false, Waste, 6.382, true
     *    }
     */
    public class TacGenericConverter : PartModule
    {
        private static char[] delimiters = { ' ', ',', '\t', ';' };
        private const int SECONDS_PER_DAY = 24 * 60 * 60;

        [KSPField]
        public string converterName = "TAC Generic Converter";

        [KSPField(guiActive = true, guiName = "Converter Status")]
        public string converterStatus = "Unknown";

        [KSPField(isPersistant = true)]
        public bool converterEnabled = false;

        [KSPField]
        public float conversionRate = 0.001f;

        [KSPField]
        public string inputResources = "";

        [KSPField]
        public string outputResources = "";

        [KSPField]
        public bool requiresOxygenAtmo = false;

        private double lastUpdateTime = 0.0f;
        private int maxDeltaTime;

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
                maxDeltaTime = TacLifeSupport.Instance.globalSettings.MaxDeltaTime;
            }
        }

        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();

            if (lastUpdateTime == 0.0f)
            {
                // Just started running
                lastUpdateTime = Planetarium.GetUniversalTime();
                return;
            }

            double deltaTime = Planetarium.GetUniversalTime() - lastUpdateTime;
            if (deltaTime > maxDeltaTime)
            {
                deltaTime = maxDeltaTime;
            }
            lastUpdateTime += deltaTime;

            if (converterEnabled)
            {
                if (requiresOxygenAtmo && !vessel.mainBody.atmosphereContainsOxygen)
                {
                    converterStatus = "Atmo lacks oxygen.";
                    return;
                }

                double desiredAmount = conversionRate / SECONDS_PER_DAY * deltaTime;

                // Limit the resource amounts so that we do not produce more than we have room for, nor consume more than is available
                foreach (ResourceRatio output in outputResourceList)
                {
                    if (!output.allowExtra)
                    {
                        double availableSpace = -part.IsResourceAvailable(output.resource, -desiredAmount * output.ratio);
                        desiredAmount = availableSpace / output.ratio;

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
                    double amountAvailable = part.IsResourceAvailable(input.resource, desiredAmount * input.ratio);
                    desiredAmount = amountAvailable / input.ratio;

                    if (desiredAmount <= 0.000000001)
                    {
                        // Not enough input resources
                        converterStatus = "Not enough " + input.resource.name;
                        return;
                    }
                }

                foreach (ResourceRatio input in inputResourceList)
                {
                    double desired = desiredAmount * input.ratio;
                    double actual = part.TakeResource(input.resource, desired);

                    if (actual < (desired * 0.999))
                    {
                        this.LogWarning("OnFixedUpdate: obtained less " + input.resource.name + " than expected: " + desired.ToString("0.000000") + "/" + actual.ToString("0.000000"));
                    }
                }

                foreach (ResourceRatio output in outputResourceList)
                {
                    double desired = desiredAmount * output.ratio;
                    double actual = -part.TakeResource(output.resource.id, -desired);

                    if (actual < (desired * 0.999) && !output.allowExtra)
                    {
                        this.LogWarning("OnFixedUpdate: put less " + output.resource.name + " than expected: " + desired.ToString("0.000000") + "/" + actual.ToString("0.000000"));
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
            sb.Append(base.GetInfo());
            sb.Append("\nContains the ");
            sb.Append(converterName);
            sb.Append(" module\n  Inputs: ");
            sb.Append(String.Join(", ", inputResourceList.Select(value => value.resource.name + ", " + value.ratio).ToArray()));
            sb.Append("\n  Outputs: ");
            sb.Append(String.Join(", ", outputResourceList.Select(value => value.resource.name + ", " + value.ratio).ToArray()));
            sb.Append("\n  Conversion Rate: ");
            sb.Append(conversionRate);
            if (requiresOxygenAtmo)
            {
                sb.Append("\nRequires an atmosphere containing Oxygen.");
            }
            sb.Append("\n");

            return sb.ToString();
        }

        [KSPEvent(active = false, guiActive = true, guiName = "Activate Converter")]
        public void ActivateConverter()
        {
            converterEnabled = true;
            UpdateEvents();
        }

        [KSPEvent(active = false, guiActive = true, guiName = "Deactivate Converter")]
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
            Events["ActivateConverter"].active = !converterEnabled;
            Events["DeactivateConverter"].active = converterEnabled;

            if (!converterEnabled)
            {
                converterStatus = "Inactive";
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

            parseInputResourceString(inputResources, inputResourceList);
            parseOutputResourceString(outputResources, outputResourceList);

            Events["ActivateConverter"].guiName = "Activate " + converterName;
            Events["DeactivateConverter"].guiName = "Deactivate " + converterName;
            Actions["ToggleConverter"].guiName = "Toggle " + converterName;
            Fields["converterStatus"].guiName = converterName;
        }

        private void parseInputResourceString(string resourceString, List<ResourceRatio> resources)
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

        private void parseOutputResourceString(string resourceString, List<ResourceRatio> resources)
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
