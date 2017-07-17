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

namespace Tac
{
    public class CrewMemberInfo
    {
        public const string ConfigNodeName = "CrewMemberInfo";

        public readonly string name;    //The Kerbals name
        public double lastUpdate;       //Last time Kerbal was updated
        public double lastFood;         //Last time Kerbal consumed food
        public double lastWater;        //Last time Kerbal consumed water
        public double lastO2;           //Last time Kerbal consumed O2
        public double lastEC;           //Last time Kerbal consumed EC
        public string vesselName;       //Name of the vessel the Kerbal is in
        public Guid vesselId;           //Guid of he vessel the Kerbal is in
        public bool vesselIsPreLaunch;  //True if the vessel this kerbal is on is PreLaunch status
        public bool hibernating;        //True if Kerbal is hibernating (rather than dead)
        public bool lackofO2;           //True if Kerbal lacks O2
        public bool lackofEC;           //True if Kerbal lacks EC
        public bool lackofFood;         //True if Kerbal lacks Food
        public bool lackofWater;        //True if Kerbal lacks Water
        public bool DFfrozen;           //True if DeepFreeze Mod is installed and this kerbal is frozen
        public bool recoverykerbal;     //True if this kerbal is part of a Recover Contract
        public ProtoCrewMember.KerbalType crewType; //The Crew Type
        public readonly double respite = UnityEngine.Random.Range(60, 600);  //Random number respoite kerbal is given - how long they can go without.

        public CrewMemberInfo(string crewMemberName, string vesselName, Guid vesselId, double currentTime)
        {
            name = crewMemberName;
            lastUpdate = currentTime;
            lastFood = currentTime;
            lastWater = currentTime;
            lastO2 = currentTime;
            lastEC = currentTime;
            this.vesselName = vesselName;
            this.vesselId = vesselId;
            this.vesselIsPreLaunch = true;
            hibernating = false;
            lackofEC = false;
            lackofO2 = false;
            lackofFood = false;
            lackofWater = false;
            DFfrozen = false;
            recoverykerbal = false;
            crewType = ProtoCrewMember.KerbalType.Crew;
            if (HighLogic.CurrentGame != null)
            {
                if (HighLogic.CurrentGame.CrewRoster.Exists(name))
                {
                    ProtoCrewMember kerbal = HighLogic.CurrentGame.CrewRoster[name];
                    if (kerbal != null)
                    {
                        crewType = kerbal.type;
                    }
                }
            }
        }

        public static CrewMemberInfo Load(ConfigNode node)
        {
            string name = Utilities.GetValue(node, "name", "Unknown");
            double lastUpdate = Utilities.GetValue(node, "lastUpdate", 0.0);
            string vesselName = Utilities.GetValue(node, "vesselName", "Unknown");
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
            info.vesselIsPreLaunch = Utilities.GetValue(node, "vesselIsPreLaunch", true);
            info.lastFood = Utilities.GetValue(node, "lastFood", lastUpdate);
            info.lastWater = Utilities.GetValue(node, "lastWater", lastUpdate);
            info.lastO2 = Utilities.GetValue(node, "lastO2", lastUpdate);
            info.lastEC = Utilities.GetValue(node, "lastEC", lastUpdate);
            info.hibernating = Utilities.GetValue(node, "hibernating", false);
            info.lackofEC = Utilities.GetValue(node, "lackofEC", false);
            info.lackofO2 = Utilities.GetValue(node, "lackofO2", false);
            info.lackofFood = Utilities.GetValue(node, "lackofFood", false);
            info.lackofWater = Utilities.GetValue(node, "lackofWater", false);
            info.DFfrozen = Utilities.GetValue(node, "DFFrozen", false);
            info.recoverykerbal = Utilities.GetValue(node, "recoverykerbal", false);
            info.crewType = Utilities.GetValue(node, "crewType", info.crewType);
            return info;
        }

        public ConfigNode Save(ConfigNode config)
        {
            ConfigNode node = config.AddNode(ConfigNodeName);
            node.AddValue("name", name);
            node.AddValue("lastUpdate", lastUpdate);
            node.AddValue("lastO2", lastO2);
            node.AddValue("lastEC", lastEC);
            node.AddValue("lastFood", lastFood);
            node.AddValue("lastWater", lastWater);
            node.AddValue("vesselName", vesselName);
            node.AddValue("vesselId", vesselId);
            node.AddValue("vesselIsPreLaunch", vesselIsPreLaunch);
            node.AddValue("hibernating", hibernating);
            node.AddValue("lackofEC", lackofEC);
            node.AddValue("lackofO2", lackofO2);
            node.AddValue("lackofFood", lackofFood);
            node.AddValue("lackofWater", lackofWater);
            node.AddValue("DFFrozen", DFfrozen);
            node.AddValue("recoverykerbal", recoverykerbal);
            node.AddValue("crewType", crewType);
            return node;
        }
    }
}
