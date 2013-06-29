/**
 * AddEvaLifeSupport.cs
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
    [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
    class AddEvaLifeSupport : MonoBehaviour
    {
        private static bool initialized = false;
        private Settings settings;

        void Awake()
        {
            Debug.Log("TAC Life Support (AddEvaLifeSupport) [" + this.GetInstanceID().ToString("X") + "][" + Time.time + "]: Awake");
        }

        void Start()
        {
            Debug.Log("TAC Life Support (AddEvaLifeSupport) [" + this.GetInstanceID().ToString("X") + "][" + Time.time + "]: Start");
        }

        void OnDestroy()
        {
            Debug.Log("TAC Life Support (AddEvaLifeSupport) [" + this.GetInstanceID().ToString("X") + "][" + Time.time + "]: OnDestroy");
        }

        void Update()
        {
            if (!initialized)
            {
                initialized = true;
                settings = LifeSupportController.Instance.settings;

                AvailablePart part = PartLoader.getPartInfoByName("kerbalEVA");
                Part prefabPart = part.partPrefab;

                Debug.Log("TAC Life Support (AddEvaLifeSupport) [" + this.GetInstanceID().ToString("X") + "][" + Time.time
                    + "]: Adding resources to " + prefabPart.partName + " (" + prefabPart.partInfo.title + ")");

                AddPartModule(prefabPart);

                AddResource(prefabPart, settings.ElectricityConsumptionRate, "ElectricCharge", true);
                AddResource(prefabPart, settings.FoodConsumptionRate, settings.Food, false);
                AddResource(prefabPart, settings.WaterConsumptionRate, settings.Water, true);
                AddResource(prefabPart, settings.OxygenConsumptionRate, settings.Oxygen, true);
                AddResource(prefabPart, settings.CO2ProductionRate, settings.CO2, false);
                AddResource(prefabPart, settings.WasteProductionRate, settings.Waste, false);
                AddResource(prefabPart, settings.WasteWaterProductionRate, settings.WasteWater, false);

                Destroy(this);
            }
        }

        private void AddPartModule(Part part)
        {
            try
            {
                ConfigNode node = new ConfigNode("MODULE");
                node.AddValue("name", "EvaLifeSupportModule");

                part.AddModule(node);

            }
            catch (Exception ex)
            {
                Debug.LogError("TAC Life Support (AddEvaLifeSupport) [" + this.GetInstanceID().ToString("X") + "][" + Time.time
                    + "]: Failed to add the part module to EVA: " + ex.Message + "\n" + ex.StackTrace);
            }

        }

        private void AddResource(Part part, double rate, string name, bool full)
        {
            try
            {
                double max = rate * settings.EvaDaysWorthOfResources;
                PartResource resource = part.gameObject.AddComponent<PartResource>();
                resource.SetInfo(PartResourceLibrary.Instance.resourceDefinitions[name]);
                resource.maxAmount = max;

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
                Debug.LogError("TAC Life Support (AddEvaLifeSupport) [" + this.GetInstanceID().ToString("X") + "][" + Time.time
                    + "]: Failed to add resource " + name + " to EVA: " + ex.Message + "\n" + ex.StackTrace);
            }
        }
    }
}
