/**
 * TacGenericConverter.cs
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
    class TacGenericConverter : PartModule
    {
        private static char[] delimiters = { ' ', ',', '\t', ';' };

        [KSPField(isPersistant = true)]
        public string converterName = "TAC Generic Converter";

        [KSPField(isPersistant = false, guiActive = true, guiName = "Converter Status")]
        public string converterStatus = "Unknown";

        [KSPField(isPersistant = true)]
        public bool converterEnabled = false;

        [KSPField(isPersistant = true)]
        public float conversionRate = 0.001f;

        [KSPField(isPersistant = true)]
        public string inputResources = "";

        [KSPField(isPersistant = true)]
        public string outputResources = "";

        private LifeSupportController controller;
        private Settings settings;

        private List<ResourceRatio> inputResourceList;
        private List<ResourceRatio> outputResourceList;

        public override void OnAwake()
        {
            Debug.Log("TAC Converter [" + this.GetInstanceID().ToString("X") + "][" + Time.time + "]: OnAwake");
            base.OnAwake();
            UpdateResourceLists();
        }

        public override void OnStart(PartModule.StartState state)
        {
            Debug.Log("TAC Converter [" + this.GetInstanceID().ToString("X") + "][" + Time.time + "]: OnStart: " + state);
            base.OnStart(state);

            if (state != StartState.Editor)
            {
                controller = LifeSupportController.Instance;
                settings = controller.settings;
            }
        }

        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();

            if (converterEnabled)
            {
                StringBuilder sb = new StringBuilder();

                // Limit the resource amounts so that we do not produce more than we have room for, nor consume more than is available
                double desiredAmount = conversionRate * Time.fixedDeltaTime;
                foreach (ResourceRatio output in outputResourceList)
                {
                    double availableSpace = AvailableSpace(output.resource.id, desiredAmount * output.ratio);
                    desiredAmount = availableSpace / output.ratio;

                    if (desiredAmount <= 0.000001)
                    {
                        // Out of space, so no need to run
                        converterStatus = "Idle: no space for more " + output.resource.name;
                        return;
                    }
                }

                foreach (ResourceRatio input in inputResourceList)
                {
                    double amountAvailable = AmountAvailable(input.resource.id, desiredAmount * input.ratio);
                    desiredAmount = amountAvailable / input.ratio;

                    if (desiredAmount <= 0.000001)
                    {
                        // Not enough input resources
                        converterStatus = "Not enough " + input.resource.name + "!";
                        return;
                    }
                }

                sb.Append("Inputs: ");
                foreach (ResourceRatio input in inputResourceList)
                {
                    double desired = desiredAmount * input.ratio;
                    double actual = part.RequestResource(input.resource.id, desired);
                    sb.Append(input.resource.name + "(" + desired.ToString("0.000000") + "/" + actual.ToString("0.000000") + "), ");
                }

                sb.Append("Outputs: ");
                foreach (ResourceRatio output in outputResourceList)
                {
                    double desired = desiredAmount * output.ratio;
                    double actual = part.RequestResource(output.resource.id, -desired);
                    sb.Append(output.resource.name + "(" + desired.ToString("0.000000") + "/" + actual.ToString("0.000000") + "), ");
                }

                Debug.Log("TAC Converter [" + this.GetInstanceID().ToString("X") + "][" + Time.time + "]: OnFixedUpdate: " + sb);
                converterStatus = "Running";
            }
            else
            {
                converterStatus = "Inactive";
            }
        }

        public override void OnLoad(ConfigNode node)
        {
            Debug.Log("TAC Converter [" + this.GetInstanceID().ToString("X") + "][" + Time.time + "]: OnLoad: " + node);
            base.OnLoad(node);

            Events["ActivateConverter"].guiName = "Activate " + converterName;
            Events["DeactivateConverter"].guiName = "Deactivate " + converterName;
            Fields["converterStatus"].guiName = converterName;
            UpdateEvents();
            UpdateResourceLists();
        }

        public override void OnSave(ConfigNode node)
        {
            Debug.Log("TAC Converter [" + this.GetInstanceID().ToString("X") + "][" + Time.time + "]: OnSave: " + node);
            base.OnSave(node);
        }

        public override string GetInfo()
        {
            var inputs = inputResourceList.Select(input => input.resource.name + ", " + input.ratio).Aggregate("", (total, input) => total + ", " + input);
            var outputs = outputResourceList.Select(output => output.resource.name + ", " + output.ratio).Aggregate("", (total, output) => total + ", " + output);
            return base.GetInfo() + "\n" + converterName + " module added!\n  Inputs: " + inputs + "\n  Outputs: " + outputs + "\n";
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

        private void UpdateEvents()
        {
            Events["ActivateConverter"].active = !converterEnabled;
            Events["DeactivateConverter"].active = converterEnabled;
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

            parseResourceString(inputResources, inputResourceList);
            parseResourceString(outputResources, outputResourceList);
        }

        private void parseResourceString(string resourceString, List<ResourceRatio> resources)
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
                    Debug.Log("TAC Converter [" + this.GetHashCode().ToString("X") + "][" + Time.time + "]: Cannot parse \"" + resourceString + "\", something went wrong.");
                }
            }

            var ratios = resources.Select(ratio => ratio.resource.name + ", " + ratio.ratio).Aggregate("", (total, ratio) => total + ", " + ratio);
            Debug.Log("TAC Converter [" + this.GetHashCode().ToString("X") + "][" + Time.time + "]: ResourceList loaded " + ratios + "\nfrom " + resourceString);
        }

        private double AvailableSpace(int resourceId, double desiredSpace)
        {
            double availableSpace = 0.0;

            List<PartResource> connectedResources = new List<PartResource>();
            part.GetConnectedResources(resourceId, connectedResources);

            string resourceName = PartResourceLibrary.Instance.GetDefinition(resourceId).name;
            string connectedParts = connectedResources.Aggregate("", (str, partResource) => str + partResource.part.partInfo.title + "(" + partResource.amount.ToString("0.00") + "), ");
            Debug.Log("TAC Converter [" + this.GetInstanceID().ToString("X") + "][" + Time.time + "]: AvailableSpace connectedParts with " + resourceName + ": " + connectedParts);

            foreach (PartResource partResource in connectedResources)
            {
                availableSpace += (partResource.maxAmount - partResource.amount);

                if ((availableSpace * 0.95) > desiredSpace)
                {
                    return desiredSpace;
                }
            }

            return availableSpace;
        }

        private double AmountAvailable(int resourceId, double desiredAmount)
        {
            double amountAvailable = 0.0;

            List<PartResource> connectedResources = new List<PartResource>();
            part.GetConnectedResources(resourceId, connectedResources);

            string resourceName = PartResourceLibrary.Instance.GetDefinition(resourceId).name;
            string connectedParts = connectedResources.Aggregate("", (str, partResource) => str + partResource.part.partInfo.title + "(" + partResource.amount.ToString("0.00") + "), ");
            Debug.Log("TAC Converter [" + this.GetInstanceID().ToString("X") + "][" + Time.time + "]: AmountAvailable connectedParts with " + resourceName + ": " + connectedParts);

            foreach (PartResource partResource in connectedResources)
            {
                amountAvailable += partResource.amount;

                if ((amountAvailable * 0.95) > desiredAmount)
                {
                    return desiredAmount;
                }
            }

            return amountAvailable;
        }
    }

    public class ResourceRatio
    {
        public PartResourceDefinition resource;
        public double ratio;

        public ResourceRatio(PartResourceDefinition resource, double ratio)
        {
            this.resource = resource;
            this.ratio = ratio;
        }
    }
}
