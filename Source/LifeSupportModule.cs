/**
 * LifeSupportModule.cs
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
    class LifeSupportModule : PartModule
    {
        public override void OnAwake()
        {
            Debug.Log("TAC Life Support (LifeSupportModule) [" + this.GetInstanceID().ToString("X") + "][" + Time.time + "]: OnAwake");
            base.OnAwake();
        }

        public override void OnStart(PartModule.StartState state)
        {
            Debug.Log("TAC Life Support (LifeSupportModule) [" + this.GetInstanceID().ToString("X") + "][" + Time.time + "]: OnStart: " + state);
            base.OnStart(state);
        }

        public override void OnLoad(ConfigNode node)
        {
            Debug.Log("TAC Life Support (LifeSupportModule) [" + this.GetInstanceID().ToString("X") + "][" + Time.time + "]: OnLoad");
            base.OnLoad(node);

            if (LifeSupportController.Instance != null)
            {
                Dictionary<string, CrewMemberInfo> knownCrew = LifeSupportController.Instance.knownCrew;
                var crewNodes = node.GetNodes("CrewMemberInfo");
                foreach (ConfigNode crewNode in crewNodes)
                {
                    CrewMemberInfo crewMemberInfo = new CrewMemberInfo(crewNode, vessel);
                    knownCrew[crewMemberInfo.name] = crewMemberInfo;
                }
            }
        }

        public override void OnSave(ConfigNode node)
        {
            Debug.Log("TAC Life Support (LifeSupportModule) [" + this.GetInstanceID().ToString("X") + "][" + Time.time + "]: OnSave");
            base.OnSave(node);

            if (LifeSupportController.Instance != null)
            {
                Dictionary<string, CrewMemberInfo> knownCrew = LifeSupportController.Instance.knownCrew;
                foreach (ProtoCrewMember crewMember in part.protoModuleCrew)
                {
                    CrewMemberInfo crewMemberInfo = knownCrew[crewMember.name];
                    if (crewMemberInfo != null)
                    {
                        crewMemberInfo.Save(node);
                    }
                }
            }
        }

        public override string GetInfo()
        {
            return base.GetInfo() + "\nTAC Life Support added!\n";
        }
    }
}
