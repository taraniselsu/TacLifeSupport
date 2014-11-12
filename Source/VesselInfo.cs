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
    public class VesselInfo
    {
        public const string ConfigNodeName = "VesselInfo";

        public string vesselName;
        public VesselType vesselType = VesselType.Unknown;

        public double lastUpdate = 0;
        public Boolean loaded;

        public Dictionary<int, Status> resourceStatuses = new Dictionary<int, Status>();
        public Dictionary<int, ResourceLimits> resourceLimits = new Dictionary<int, ResourceLimits>();
        public Dictionary<int, double> lastKnownAmounts = new Dictionary<int, double>();
        
        public Dictionary<String, CrewMemberInfo> crew = new Dictionary<String, CrewMemberInfo>();
        public int numOccupiedParts;
        public int crewCapacity;

        public Dictionary<int, double> depletionEstimates = new Dictionary<int, double>();

        public List<TacGenericConverter> converters;
        public List<IProtoElecComponent> elecProtoComponents;

        public VesselInfo(string vesselName, double currentTime)
        {
            this.vesselName = vesselName;
            lastUpdate = currentTime;
        }

        public static VesselInfo Load(ConfigNode node)
        {
            string vesselName = Utilities.GetValue(node, "vesselName", "Unknown");
            double lastUpdate = Utilities.GetValue(node, "lastUpdate", 0.0);

            VesselInfo info = new VesselInfo(vesselName, lastUpdate);
            info.vesselType = Utilities.GetValue(node, "vesselType", VesselType.Unknown);
            info.numOccupiedParts = Utilities.GetValue(node, "numOccupiedParts", 0);

            //TODO There isn't a better way to get the names and valued that all start with "remaining"?
            ConfigNode.ValueList values = node.values;
            foreach (String valueName in values.DistinctNames()) {
                if (valueName.StartsWith("remaining"))
                {
                    String resourceName = valueName.Substring(9);
                    double remaining = Utilities.GetValue(node, "remaining" + resourceName, 0.0);
                    double max = Utilities.GetValue(node, "max" + resourceName, 0.0);
                    double lastKnown = Utilities.GetValue(node, "lastKnown" + resourceName, remaining);

                    int resourceId = getResourceId(resourceName);
                    ResourceLimits loadedLimits = new ResourceLimits(remaining, max);
                    info.resourceLimits.Add(resourceId, loadedLimits);
                    info.lastKnownAmounts[resourceId] = lastKnown;
                }
            }

            return info;
        }

        private static int getResourceId(String resourceName) {
            if (resourceName=="Electricity") resourceName = "ElectricCharge";
            if (resourceName == "CO2") resourceName = "CarbonDioxide";
            return PartResourceLibrary.Instance.GetDefinition(resourceName).id;
        }

        public ConfigNode Save(ConfigNode config)
        {
            ConfigNode node = config.AddNode(ConfigNodeName);
            node.AddValue("vesselName", vesselName);
            node.AddValue("vesselType", vesselType.ToString());
            node.AddValue("numOccupiedParts", numOccupiedParts);

            node.AddValue("lastUpdate", lastUpdate);

            foreach (KeyValuePair<int, ResourceLimits> resources in resourceLimits) {
                String resourceName = PartResourceLibrary.Instance.GetDefinition(resources.Key).name;
                node.AddValue("remaining" + resourceName, resources.Value.available);
                node.AddValue("max" + resourceName, resources.Value.maximum);
                node.AddValue("lastKnown" + resourceName, lastKnownAmounts[resources.Key]);
            }
            return node;
        }

        public void ClearAmounts()
        {
            numOccupiedParts = 0;
            resourceLimits.Clear();
        }

        public static double GetResourceQuantity(Vessel vessel, int resource)
        {
            double actualAvailable = 0;
            foreach (Part part in vessel.parts)
            {
                if (part.Resources.Contains(resource))
                {
                    actualAvailable += part.Resources.Get(resource).amount;
                }
            }
            return actualAvailable;
        }

        public enum Status
        {
            GOOD,
            LOW,
            CRITICAL
        }
    }
}
