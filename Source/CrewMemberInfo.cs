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

namespace Tac
{
    public class CrewMemberInfo
    {
        public const string ConfigNodeName = "CrewMemberInfo";

        public readonly string name;
        public double lastUpdate;

        public Dictionary<int, ResourceLimits> reserves = new Dictionary<int, ResourceLimits>();
        public string vesselName;
        public Guid vesselId;
        public bool hibernating;
        public readonly double respite = UnityEngine.Random.Range(60, 600);

        public CrewMemberInfo(string crewMemberName, string vesselName, Guid vesselId, double currentTime)
        {
            GlobalSettings globalSettings = TacLifeSupport.Instance.globalSettings;
            name = crewMemberName;
            lastUpdate = currentTime;
            foreach (int resource in globalSettings.kerbalRequirements) {
                double maxAmount = - globalSettings.kerbalProductionRates[resource] * globalSettings.kerbalStarvationTimes[resource];
                reserves[resource] = new ResourceLimits(maxAmount,maxAmount);
            }
            this.vesselName = vesselName;
            this.vesselId = vesselId;
            hibernating = false;
        }

        public static CrewMemberInfo Load(ConfigNode node)
        {
            string name = Utilities.GetValue(node, "name", "Unknown");
            double lastUpdate = Utilities.GetValue(node, "lastUpdate", 0.0);
            string vesselName = Utilities.GetValue(node, "vesselName", "Unknown");
            GlobalSettings globalSettings = TacLifeSupport.Instance.globalSettings;
            Guid vesselId;
            if (node.HasValue("vesselId"))
            {
                vesselId = new Guid(node.GetValue("vesselId"));
            }
            else
            {
                vesselId = Guid.Empty;
            }

            CrewMemberInfo info = new CrewMemberInfo(name, vesselName, vesselId, lastUpdate);
            //load up old style lastfood config
            if (node.HasValue("lastFood"))
            {
                foreach (int resource in globalSettings.kerbalRequirements)
                {
                    String resourceName = PartResourceLibrary.Instance.GetDefinition(resource).name;
                    double timeWithout = lastUpdate - Utilities.GetValue(node, "last"+resourceName, lastUpdate);
                    double amount = -globalSettings.kerbalProductionRates[resource] * (globalSettings.kerbalStarvationTimes[resource] - timeWithout);
                    double maxAmount = - globalSettings.kerbalProductionRates[resource] * globalSettings.kerbalStarvationTimes[resource];
                    info.reserves[resource] = new ResourceLimits(amount, maxAmount);
                }
            }
            else
            {
                foreach (int resource in globalSettings.kerbalRequirements)
                {
                    String resourceName = PartResourceLibrary.Instance.GetDefinition(resource).name;
                    double maxAmount = -globalSettings.kerbalProductionRates[resource] * globalSettings.kerbalStarvationTimes[resource];
                    double amount = Utilities.GetValue(node, resourceName + "Reserves", maxAmount);
                    info.reserves[resource] = new ResourceLimits(amount, maxAmount);
                }
            }
            info.hibernating = Utilities.GetValue(node, "hibernating", false);

            return info;
        }

        public ConfigNode Save(ConfigNode config)
        {
            this.Log("saving crewinfo for " + name);
            GlobalSettings globalSettings = TacLifeSupport.Instance.globalSettings;
            ConfigNode node = config.AddNode(ConfigNodeName);
            node.AddValue("name", name);
            node.AddValue("lastUpdate", lastUpdate);
            foreach (int resource in globalSettings.kerbalRequirements)
            {
                String resourceName = PartResourceLibrary.Instance.GetDefinition(resource).name;
                node.AddValue(resourceName+"Reserves", reserves[resource].available);
            }
            node.AddValue("vesselName", vesselName);
            node.AddValue("vesselId", vesselId);
            node.AddValue("hibernating", hibernating);
            return node;
        }
    }
}
