/**
 * CrewMemberInfo.cs
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

namespace Tac
{
    public class CrewMemberInfo
    {
        public string name;
        public Guid vesselId;
        public double lastUpdate;
        public double lastFood;
        public double lastWater;
        public double lastOxygen;
        public readonly double respite = UnityEngine.Random.Range(30, 150);

        public CrewMemberInfo(string crewMemberName, Guid vesselId, double currentTime)
        {
            name = crewMemberName;
            this.vesselId = vesselId;
            lastUpdate = currentTime;
            lastFood = currentTime;
            lastWater = currentTime;
            lastOxygen = currentTime;
        }

        public CrewMemberInfo(ConfigNode node)
        {
            name = Utilities.GetValue(node, "name", name);

            if (node.HasValue("vesselId"))
            {
                vesselId = new Guid(node.GetValue("vesselId"));
            }

            lastUpdate = Utilities.GetValue(node, "lastUpdate", lastUpdate);
            lastFood = Utilities.GetValue(node, "lastFood", lastFood);
            lastWater = Utilities.GetValue(node, "lastWater", lastWater);
            lastOxygen = Utilities.GetValue(node, "lastOxygen", lastOxygen);
        }

        public void Save(ConfigNode config)
        {
            ConfigNode node = new ConfigNode("CrewMemberInfo");
            node.AddValue("name", name);
            node.AddValue("vesselId", vesselId);
            node.AddValue("lastUpdate", lastUpdate);
            node.AddValue("lastFood", lastFood);
            node.AddValue("lastWater", lastWater);
            node.AddValue("lastOxygen", lastOxygen);
            config.AddNode(node);
        }
    }
}
