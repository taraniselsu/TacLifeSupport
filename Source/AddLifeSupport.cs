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

using KSP.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Tac
{
    class AddLifeSupport
    {
        private static bool initialized = false;
        private GlobalSettings globalSettings;

        public AddLifeSupport(GlobalSettings globalSettings)
        {
            this.Log("Constructor");
            this.globalSettings = globalSettings;
        }

        public void run()
        {
            if (!initialized)
            {
                this.Log("run");
                initialized = true;

                try
                {
                    var evaParts = PartLoader.LoadedPartsList.Where(p => p.name.Equals("kerbalEVA") || p.name.Equals("kerbalEVAfemale"));
                    foreach (var evaPart in evaParts)
                    {
                        EvaAddLifeSupport(evaPart);
                    }
                }
                catch (Exception ex)
                {
                    this.LogError("Failed to add Life Support to the EVA.\n" + ex.Message + "\n" + ex.StackTrace);
                }
            }
        }

        private void EvaAddLifeSupport(AvailablePart part)
        {
            Part prefabPart = part.partPrefab;

            this.Log("Adding resources to " + part.name + "/" + prefabPart.partInfo.title);

            EvaAddPartModule(prefabPart);

            EvaAddResource(prefabPart, globalSettings.EvaElectricityConsumptionRate, globalSettings.Electricity, false);
            EvaAddResource(prefabPart, globalSettings.FoodConsumptionRate, globalSettings.Food, false);
            EvaAddResource(prefabPart, globalSettings.WaterConsumptionRate, globalSettings.Water, false);
            EvaAddResource(prefabPart, globalSettings.OxygenConsumptionRate, globalSettings.Oxygen, false);
            EvaAddResource(prefabPart, globalSettings.CO2ProductionRate, globalSettings.CO2, false);
            EvaAddResource(prefabPart, globalSettings.WasteProductionRate, globalSettings.Waste, false);
            EvaAddResource(prefabPart, globalSettings.WasteWaterProductionRate, globalSettings.WasteWater, false);
        }

        private void EvaAddPartModule(Part part)
        {
            try
            {
                ConfigNode node = new ConfigNode("MODULE");
                node.AddValue("name", "LifeSupportModule");

                part.AddModule(node);

                this.LogWarning("The expected exception did not happen when adding the Life Support part module to the EVA!");
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Object reference not set"))
                {
                    this.Log("Adding life support to the EVA part succeeded as expected.");
                }
                else
                {
                    this.LogError("Unexpected error while adding the Life Support part module to the EVA: " + ex.Message + "\n" + ex.StackTrace);
                }
            }
        }

        private void EvaAddResource(Part part, double rate, string name, bool full)
        {
            try
            {
                double max = rate * globalSettings.EvaDefaultResourceAmount;
                PartResource resource = part.gameObject.AddComponent<PartResource>();
                resource.SetInfo(PartResourceLibrary.Instance.resourceDefinitions[name]);
                resource.maxAmount = max;
                resource.flowState = true;
                resource.flowMode = PartResource.FlowMode.Both;
                resource.part = part;

                if (full)
                {
                    resource.amount = max;
                }
                else
                {
                    resource.amount = 0;
                }

                part.Resources.list.Add(resource);
            }
            catch (Exception ex)
            {
                if (!ex.Message.Contains("Object reference not set"))
                {
                    this.LogError("Unexpected error while adding resource " + name + " to the EVA: " + ex.Message + "\n" + ex.StackTrace);
                }
            }
        }
    }
}
