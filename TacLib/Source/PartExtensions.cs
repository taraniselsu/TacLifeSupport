/**
 * PartExtensions.cs
 * 
 * Thunder Aerospace Corporation's library for the Kerbal Space Program, by Taranis Elsu
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
    public static class PartExtensions
    {
        public static double TakeResource(this Part part, string resourceName, double demand)
        {
            PartResourceDefinition resource = PartResourceLibrary.Instance.GetDefinition(resourceName);
            return TakeResource(part, resource, demand);
        }

        public static double TakeResource(this Part part, int resourceId, double demand)
        {
            PartResourceDefinition resource = PartResourceLibrary.Instance.GetDefinition(resourceId);
            return TakeResource(part, resource, demand);
        }

        public static double TakeResource(this Part part, PartResourceDefinition resource, double demand)
        {
            if (resource == null)
            {
                Debug.LogError("Tac.PartExtensions.TakeResource: resource is null");
                return 0.0;
            }

            switch (resource.resourceFlowMode)
            {
                case ResourceFlowMode.NO_FLOW:
                    return TakeResource_NoFlow(part, resource, demand);
                case ResourceFlowMode.ALL_VESSEL:
                    return TakeResource_AllVessel(part, resource, demand);
                case ResourceFlowMode.STACK_PRIORITY_SEARCH:
                    return TakeResource_StackPriority(part, resource, demand);
                case ResourceFlowMode.STAGE_PRIORITY_FLOW:
                    Debug.LogWarning("Tac.PartExtensions.TakeResource: ResourceFlowMode.STAGE_PRIORITY_FLOW is not supported yet.");
                    return part.RequestResource(resource.id, demand);
                default:
                    Debug.LogWarning("Tac.PartExtensions.TakeResource: Unknown ResourceFlowMode = " + resource.resourceFlowMode.ToString());
                    return part.RequestResource(resource.id, demand);
            }
        }

        public static double IsResourceAvailable(this Part part, string resourceName, double demand)
        {
            PartResourceDefinition resource = PartResourceLibrary.Instance.GetDefinition(resourceName);
            return IsResourceAvailable(part, resource, demand);
        }

        public static double IsResourceAvailable(this Part part, int resourceId, double demand)
        {
            PartResourceDefinition resource = PartResourceLibrary.Instance.GetDefinition(resourceId);
            return IsResourceAvailable(part, resource, demand);
        }

        public static double IsResourceAvailable(this Part part, PartResourceDefinition resource, double demand)
        {
            if (resource == null)
            {
                Debug.LogError("Tac.PartExtensions.IsResourceAvailable: resource is null");
                return 0.0;
            }

            switch (resource.resourceFlowMode)
            {
                case ResourceFlowMode.NO_FLOW:
                    return IsResourceAvailable_NoFlow(part, resource, demand);
                case ResourceFlowMode.ALL_VESSEL:
                    return IsResourceAvailable_AllVessel(part, resource, demand);
                case ResourceFlowMode.STACK_PRIORITY_SEARCH:
                    return IsResourceAvailable_StackPriority(part, resource, demand);
                case ResourceFlowMode.STAGE_PRIORITY_FLOW:
                    Debug.LogWarning("Tac.PartExtensions.IsResourceAvailable: ResourceFlowMode.STAGE_PRIORITY_FLOW is not supported yet.");
                    return IsResourceAvailable_AllVessel(part, resource, demand);
                default:
                    Debug.LogWarning("Tac.PartExtensions.IsResourceAvailable: Unknown ResourceFlowMode = " + resource.resourceFlowMode.ToString());
                    return IsResourceAvailable_AllVessel(part, resource, demand);
            }
        }

        private static double TakeResource_NoFlow(Part part, PartResourceDefinition resource, double demand)
        {
            // ignoring PartResourceDefinition.ResourceTransferMode

            PartResource partResource = part.Resources.Get(resource.id);
            if (partResource != null)
            {
                if (partResource.flowMode == PartResource.FlowMode.None)
                {
                    Debug.LogWarning("Tac.PartExtensions.TakeResource_NoFlow: cannot take resource from a part where FlowMode is None.");
                    return 0.0;
                }
                else if (!partResource.flowState)
                {
                    // Resource flow was shut off -- no warning needed
                    return 0.0;
                }
                else if (demand >= 0.0)
                {
                    if (partResource.flowMode == PartResource.FlowMode.In)
                    {
                        Debug.LogWarning("Tac.PartExtensions.TakeResource_NoFlow: cannot take resource from a part where FlowMode is In.");
                        return 0.0;
                    }

                    double taken = Math.Min(partResource.amount, demand);
                    partResource.amount -= taken;
                    return taken;
                }
                else
                {
                    if (partResource.flowMode == PartResource.FlowMode.Out)
                    {
                        Debug.LogWarning("Tac.PartExtensions.TakeResource_NoFlow: cannot give resource to a part where FlowMode is Out.");
                        return 0.0;
                    }

                    double given = Math.Min(partResource.maxAmount - partResource.amount, -demand);
                    partResource.amount += given;
                    return -given;
                }
            }
            else
            {
                return 0.0;
            }
        }

        private static double TakeResource_AllVessel(Part part, PartResourceDefinition resource, double demand)
        {
            if (demand >= 0.0)
            {
                double leftOver = demand;

                // Takes an equal percentage from each part (rather than an equal amount from each part)
                List<PartResource> partResources = GetAllPartResources(part.vessel, resource, true);
                double totalAmount = 0.0;
                foreach (PartResource partResource in partResources)
                {
                    totalAmount += partResource.amount;
                }

                if (totalAmount > 0.0)
                {
                    double percentage = Math.Min(leftOver / totalAmount, 1.0);

                    foreach (PartResource partResource in partResources)
                    {
                        double taken = partResource.amount * percentage;
                        partResource.amount -= taken;
                        leftOver -= taken;
                    }
                }

                return demand - leftOver;
            }
            else
            {
                double leftOver = -demand;

                List<PartResource> partResources = GetAllPartResources(part.vessel, resource, false);
                double totalSpace = 0.0;
                foreach (PartResource partResource in partResources)
                {
                    totalSpace += partResource.maxAmount - partResource.amount;
                }

                if (totalSpace > 0.0)
                {
                    double percentage = Math.Min(leftOver / totalSpace, 1.0);

                    foreach (PartResource partResource in partResources)
                    {
                        double space = partResource.maxAmount - partResource.amount;
                        double given = space * percentage;
                        partResource.amount += given;
                        leftOver -= given;
                    }
                }

                return demand + leftOver;
            }
        }

        private static double TakeResource_StackPriority(Part part, PartResourceDefinition resource, double demand)
        {
            // FIXME finish implementing
            return part.RequestResource(resource.id, demand);
        }

        private static double IsResourceAvailable_NoFlow(Part part, PartResourceDefinition resource, double demand)
        {
            PartResource partResource = part.Resources.Get(resource.id);
            if (partResource != null)
            {
                if (partResource.flowMode == PartResource.FlowMode.None || partResource.flowState == false)
                {
                    return 0.0;
                }
                else if (demand > 0.0)
                {
                    if (partResource.flowMode != PartResource.FlowMode.In)
                    {
                        return Math.Min(partResource.amount, demand);
                    }
                }
                else
                {
                    if (partResource.flowMode != PartResource.FlowMode.Out)
                    {
                        return -Math.Min((partResource.maxAmount - partResource.amount), -demand);
                    }
                }
            }

            return 0.0;
        }

        private static double IsResourceAvailable_AllVessel(Part part, PartResourceDefinition resource, double demand)
        {
            if (demand >= 0.0)
            {
                double amountAvailable = 0.0;

                foreach (Part p in part.vessel.parts)
                {
                    PartResource partResource = p.Resources.Get(resource.id);
                    if (partResource != null)
                    {
                        if (partResource.flowState && partResource.flowMode != PartResource.FlowMode.None && partResource.flowMode != PartResource.FlowMode.In)
                        {
                            amountAvailable += partResource.amount;

                            if (amountAvailable >= demand)
                            {
                                return demand;
                            }
                        }
                    }
                }

                return amountAvailable;
            }
            else
            {
                double availableSpace = 0.0;
                double demandedSpace = -demand;

                foreach (Part p in part.vessel.parts)
                {
                    PartResource partResource = p.Resources.Get(resource.id);
                    if (partResource != null)
                    {
                        if (partResource.flowState && partResource.flowMode != PartResource.FlowMode.None && partResource.flowMode != PartResource.FlowMode.Out)
                        {
                            availableSpace += (partResource.maxAmount - partResource.amount);

                            if (availableSpace >= demandedSpace)
                            {
                                return demand;
                            }
                        }
                    }
                }

                return -availableSpace;
            }
        }

        private static double IsResourceAvailable_StackPriority(Part part, PartResourceDefinition resource, double demand)
        {
            // FIXME finish implementing
            return IsResourceAvailable_AllVessel(part, resource, demand);
        }

        private static List<PartResource> GetAllPartResources(Vessel vessel, PartResourceDefinition resource, bool consuming)
        {
            // ignoring PartResourceDefinition.ResourceTransferMode
            List<PartResource> resources = new List<PartResource>();

            foreach (Part p in vessel.parts)
            {
                PartResource partResource = p.Resources.Get(resource.id);
                if (partResource != null)
                {
                    if (partResource.flowState && partResource.flowMode != PartResource.FlowMode.None)
                    {
                        if (consuming)
                        {
                            if (partResource.flowMode != PartResource.FlowMode.In && partResource.amount > 0.0)
                            {
                                resources.Add(partResource);
                            }
                        }
                        else
                        {
                            if (partResource.flowMode != PartResource.FlowMode.Out && partResource.amount < partResource.maxAmount)
                            {
                                resources.Add(partResource);
                            }
                        }
                    }
                }
            }

            return resources;
        }
    }
}
