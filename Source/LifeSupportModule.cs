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
    [KSPModule("TAC Life Support")]
    class LifeSupportModule : PartModule, IResourceConsumer
    {
        public override void OnAwake()
        {
            this.Log("OnAwake");
            base.OnAwake();
        }

        public override void OnStart(PartModule.StartState state)
        {
            this.Log("OnStart: " + state);
            base.OnStart(state);
        }

        public override string GetInfo()
        {
            return base.GetInfo() + "\nEquipped with the Thunder Aerospace Corporation (TAC) Life Support System.\n";
        }

        public List<PartResourceDefinition> GetConsumedResources()
        {
            return new List<PartResourceDefinition>() {
                PartResourceLibrary.Instance.GetDefinition("Food"),
                PartResourceLibrary.Instance.GetDefinition("Water"),
                PartResourceLibrary.Instance.GetDefinition("Oxygen"),
                PartResourceLibrary.Instance.GetDefinition("ElectricCharge")
            };
        }
    }
}
