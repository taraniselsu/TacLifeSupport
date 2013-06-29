/**
 * AddLifeSupport.cs
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
    public class AddLifeSupport : MonoBehaviour
    {
        private static bool initialized = false;
        private Settings settings;

        void Awake()
        {
            Debug.Log("TAC Life Support (AddLifeSupport) [" + this.GetInstanceID().ToString("X") + "][" + Time.time + "]: Awake");
        }

        void Start()
        {
            Debug.Log("TAC Life Support (AddLifeSupport) [" + this.GetInstanceID().ToString("X") + "][" + Time.time + "]: Start");
        }

        void OnDestroy()
        {
            Debug.Log("TAC Life Support (AddLifeSupport) [" + this.GetInstanceID().ToString("X") + "][" + Time.time + "]: OnDestroy");
        }

        void Update()
        {
            if (!initialized)
            {
                initialized = true;
                settings = LifeSupportController.Instance.settings;

                List<AvailablePart> parts = PartLoader.LoadedPartsList;
                foreach (AvailablePart part in parts)
                {
                    try
                    {
                        Part prefabPart = part.partPrefab;
                        if (prefabPart.CrewCapacity > 0 && !part.name.Equals("kerbalEVA"))
                        {
                            Debug.Log("TAC Life Support (AddLifeSupport) [" + this.GetInstanceID().ToString("X") + "][" + Time.time
                                + "]: Adding Life Support to " + prefabPart.partName + " (" + prefabPart.partInfo.title + ")");

                            AddPartModule(prefabPart);

                            AddResource(prefabPart, settings.FoodId, settings.Food, settings.FoodConsumptionRate, true);
                            AddResource(prefabPart, settings.WaterId, settings.Water, settings.WaterConsumptionRate, true);
                            AddResource(prefabPart, settings.OxygenId, settings.Oxygen, settings.OxygenConsumptionRate, true);
                            AddResource(prefabPart, settings.CO2Id, settings.CO2, settings.CO2ProductionRate, false);
                            AddResource(prefabPart, settings.WasteId, settings.Waste, settings.WasteProductionRate, false);
                            AddResource(prefabPart, settings.WasteWaterId, settings.WasteWater, settings.WasteWaterProductionRate, false);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError("TAC Life Support (AddLifeSupport) [" + this.GetInstanceID().ToString("X") + "][" + Time.time
                            + "]: Failed to add Life Support to " + part + ":\n" + ex.Message + "\n" + ex.StackTrace);
                    }
                }

                Destroy(this);
            }
        }

        private void AddPartModule(Part part)
        {
            try
            {
                if (!part.Modules.Contains("LifeSupportModule"))
                {
                    Debug.Log("TAC missing!");

                    ConfigNode node = new ConfigNode("MODULE");
                    node.AddValue("name", "LifeSupportModule");

                    part.AddModule(node);
                }
                else
                {
                    Debug.Log("TAC already there!");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("TAC Life Support (AddLifeSupport) [" + this.GetInstanceID().ToString("X") + "][" + Time.time
                    + "]: Failed to add the part module: " + ex.Message + "\n" + ex.StackTrace);
            }
        }

        private void AddResource(Part part, int id, string name, double rate, bool full)
        {
            try
            {
                if (!part.Resources.Contains(id))
                {
                    double max = part.CrewCapacity * rate * settings.DaysWorthOfResources;
                    ConfigNode node = new ConfigNode("RESOURCE");
                    node.AddValue("name", name);
                    node.AddValue("maxAmount", max);

                    if (full)
                    {
                        node.AddValue("amount", max);
                    }
                    else
                    {
                        node.AddValue("amount", 0);
                    }

                    part.AddResource(node);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("TAC Life Support (AddLifeSupport) [" + this.GetInstanceID().ToString("X") + "][" + Time.time
                    + "]: Failed to add resource " + name + " to the part: " + ex.Message + "\n" + ex.StackTrace);
            }
        }
    }
}
