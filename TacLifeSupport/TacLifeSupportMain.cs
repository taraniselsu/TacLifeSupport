/**
 * TacLifeSupportMain.cs
 * 
 * Thunder Aerospace Corporation's Atomic Clock for the Kerbal Space Program, by Taranis Elsu
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
using System.Threading.Tasks;
using UnityEngine;

class TacLifeSupportMain : MonoBehaviour
{
    private static double SECONDS_PER_DAY = 24 * 60 * 60;
    private string windowTitle;
    private int windowId;
    private Rect windowPos;
    private bool windowVisible;

    private double lastUpdateTime;

    private double foodConsumptionRate;
    private double waterConsumptionRate;
    private double oxygenConsumptionRate;
    private double co2ProductionRate;
    private double liquidWasteProductionRate;
    private double solidWasteProductionRate;

    private int foodResourceId;
    private int waterResourceId;
    private int oxygenResourceId;
    private int co2ResourceId;
    private int liquidWasteResourceId;
    private int solidWasteResourceId;

    private bool foodCritical;
    private bool waterCritical;
    private bool oxygenCritical;

    private double timeFoodRanOut;
    private double timeWaterRanOut;
    private double timeOxygenRanOut;

    private double maxTimeNoFood;
    private double maxTimeNoWater;
    private double maxTimeNoOxygen;

    private bool alerted;

    public static TacLifeSupportMain GetInstance()
    {
        GameObject obj = GameObject.Find("TacLifeSupportMain");
        if (!obj)
        {
            obj = new GameObject("TacLifeSupportMain", typeof(TacLifeSupportMain));
        }

        return obj.GetComponent<TacLifeSupportMain>();
    }

    void Awake()
    {
        windowTitle = "Life Support Monitoring";
        windowId = windowTitle.GetHashCode();
        windowPos = new Rect(10, 10, 100, 100);
        windowVisible = false;

        lastUpdateTime = -1;

        // consumption rates in kg per Earth day (24-hour)
        foodConsumptionRate = 0.62;
        waterConsumptionRate = 3.52;
        oxygenConsumptionRate = 0.84;
        co2ProductionRate = 1.00;
        liquidWasteProductionRate = 3.87;
        solidWasteProductionRate = 0.11;

        foodCritical = false;
        waterCritical = false;
        oxygenCritical = false;

        timeFoodRanOut = -1.0;
        timeWaterRanOut = -1.0;
        timeOxygenRanOut = -1.0;

        maxTimeNoFood = 30 * 24 * 60 * 60; // 30 days
        maxTimeNoWater = 3 * 24 * 60 * 60; // 3 days
        maxTimeNoOxygen = 2 * 60 * 60; // 2 hours

        alerted = false;
    }

    void FixedUpdate()
    {
        if (HighLogic.LoadedSceneIsFlight)
        {
            Vessel vessel = FlightGlobals.ActiveVessel;
            if (vessel == null)
            {
                Debug.Log("TAC vessel is null");
                return;
            }

            if (lastUpdateTime == -1)
            {
                if (vessel.missionTime > 0)
                {
                    lastUpdateTime = Planetarium.GetUniversalTime();
                }
            }
            else
            {
                double currentTime = Planetarium.GetUniversalTime();
                double timeDelta = currentTime - lastUpdateTime;
                lastUpdateTime = currentTime;

                CheckResourceLevels(vessel);

                int numCrew = vessel.GetCrewCount();
                Part part = vessel.rootPart;

                // Food
                double desiredFood = numCrew * timeDelta * foodConsumptionRate / SECONDS_PER_DAY;
                if (foodCritical)
                {
                    desiredFood /= 2.0;
                }
                double foodObtained = part.RequestResource(foodResourceId, desiredFood);
                if (foodObtained < (desiredFood * 0.99))
                {
                    if (timeFoodRanOut == -1)
                    {
                        TimeWarp.SetRate(0, true);
                        ScreenMessages.PostScreenMessage("LIFE SUPPORT CRITICAL: FOOD DEPLETED!", 5.0f, ScreenMessageStyle.UPPER_CENTER);
                        timeFoodRanOut = currentTime - ((desiredFood - foodObtained) / foodConsumptionRate * SECONDS_PER_DAY);
                    }
                    else if ((currentTime - timeFoodRanOut) > maxTimeNoFood)
                    {
                        timeFoodRanOut += KillCrewMember(vessel, "malnutrition");
                    }
                }
                else
                {
                    timeFoodRanOut = -1;
                }

                // Water
                double desiredWater = numCrew * timeDelta * waterConsumptionRate / SECONDS_PER_DAY;
                if (waterCritical)
                {
                    desiredWater /= 2.0;
                }
                double waterObtained = part.RequestResource(waterResourceId, desiredWater);
                if (waterObtained < (desiredWater * 0.99))
                {
                    if (timeWaterRanOut == -1)
                    {
                        TimeWarp.SetRate(0, true);
                        ScreenMessages.PostScreenMessage("LIFE SUPPORT CRITICAL: WATER DEPLETED!", 5.0f, ScreenMessageStyle.UPPER_CENTER);
                        timeWaterRanOut = currentTime;
                    }
                    else if ((currentTime - timeWaterRanOut) > maxTimeNoWater)
                    {
                        timeWaterRanOut += KillCrewMember(vessel, "dehydration");
                    }
                }
                else
                {
                    timeWaterRanOut = -1;
                }

                if (!vessel.orbit.referenceBody.atmosphereContainsOxygen || FlightGlobals.getStaticPressure() < 0.2)
                {
                    // Oxygen
                    double desiredOxygen = numCrew * timeDelta * oxygenConsumptionRate / SECONDS_PER_DAY;
                    double oxygenObtained = part.RequestResource(oxygenResourceId, desiredOxygen);
                    if (oxygenObtained < (desiredOxygen * 0.99))
                    {
                        if (timeOxygenRanOut == -1)
                        {
                            TimeWarp.SetRate(0, true);
                            ScreenMessages.PostScreenMessage("LIFE SUPPORT CRITICAL: OXYGEN DEPLETED!", 5.0f, ScreenMessageStyle.UPPER_CENTER);
                            timeOxygenRanOut = currentTime;
                        }
                        else if ((currentTime - timeOxygenRanOut) > maxTimeNoOxygen)
                        {
                            timeOxygenRanOut += KillCrewMember(vessel, "oxygen deprivation");
                        }
                    }
                    else
                    {
                        timeOxygenRanOut = -1;
                    }

                    // CO2
                    double co2Production = oxygenObtained * co2ProductionRate / oxygenConsumptionRate;
                    part.RequestResource(co2ResourceId, -co2Production);
                }
                else
                {
                    part.RequestResource(oxygenResourceId, timeDelta * -1.0);
                    part.RequestResource(co2ResourceId, timeDelta * 1.0);
                }

                // Waste
                double liquidWasteProduced = waterObtained * liquidWasteProductionRate / waterConsumptionRate;
                double solidWasteProduced = foodObtained * solidWasteProductionRate / foodConsumptionRate;
                part.RequestResource(liquidWasteResourceId, -liquidWasteProduced);
                part.RequestResource(solidWasteResourceId, -solidWasteProduced);
            }
        }
    }

    private void CheckResourceLevels(Vessel vessel)
    {
        double totalFood = 0.0;
        double maxFood = 0.0;
        double totalWater = 0.0;
        double maxWater = 0.0;
        double totalOxygen = 0.0;
        double maxOxygen = 0.0;

        foreach (Part part in vessel.parts)
        {
            foreach (PartResource resource in part.Resources)
            {
                if (resource.info.id == foodResourceId)
                {
                    totalFood += resource.amount;
                    maxFood += resource.maxAmount;
                }
                else if (resource.info.id == waterResourceId)
                {
                    totalWater += resource.amount;
                    maxWater += resource.maxAmount;
                }
                else if (resource.info.id == oxygenResourceId)
                {
                    totalOxygen += resource.amount;
                    maxOxygen += resource.maxAmount;
                }
            }
        }

        foodCritical = (totalFood < (maxFood * 0.10));
        waterCritical = (totalWater < (maxWater * 0.10));
        oxygenCritical = (totalOxygen < (maxOxygen * 0.10));

        if (foodCritical || waterCritical || oxygenCritical)
        {
            if (!alerted)
            {
                TimeWarp.SetRate(0, true);
                ScreenMessages.PostScreenMessage("LIFE SUPPORT CRITICAL!", 5.0f, ScreenMessageStyle.UPPER_CENTER);
                alerted = true;
            }
        }
        else
        {
            alerted = false;
        }
    }

    void OnGUI()
    {
        if (HighLogic.LoadedSceneIsFlight && !windowVisible)
        {
            RenderingManager.AddToPostDrawQueue(3, CreateWindow);
            windowVisible = true;
        }
    }

    void CreateWindow()
    {
        bool paused;
        try
        {
            paused = PauseMenu.isOpen;
        }
        catch (Exception)
        {
            // assume it is not open
            paused = false;
        }

        if (!paused)
        {
            GUI.skin = HighLogic.Skin;
            windowPos = GUILayout.Window(windowId, windowPos, DrawWindow, windowTitle);
        }
    }

    void DrawWindow(int windowId)
    {
        GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.wordWrap = false;
        labelStyle.fontStyle = FontStyle.Normal;
        labelStyle.normal.textColor = Color.white;

        GUIStyle warningStyle = new GUIStyle(labelStyle);
        warningStyle.normal.textColor = Color.red;

        string vesselName = "unknown";
        string vesselType = "unknown";
        string numCrew = "unknown";
        Vessel vessel = FlightGlobals.ActiveVessel;
        if (vessel != null)
        {
            vesselName = vessel.vesselName;
            vesselType = vessel.vesselType.ToString();
            numCrew = vessel.GetCrewCount().ToString();
        }

        GUILayout.BeginVertical();

        GUILayout.BeginHorizontal();
        
        GUILayout.BeginVertical();
        GUILayout.Label("Vessel:", labelStyle);
        GUILayout.Label("Type:", labelStyle);
        GUILayout.Label("Kerbals:", labelStyle);
        GUILayout.Label("Last update time:", labelStyle);
        GUILayout.EndVertical();
        
        GUILayout.BeginVertical();
        GUILayout.Label(vesselName, labelStyle);
        GUILayout.Label(vesselType, labelStyle);
        GUILayout.Label(numCrew, labelStyle);
        GUILayout.Label(lastUpdateTime.ToString("#,#"), labelStyle);
        GUILayout.EndVertical();

        GUILayout.EndHorizontal();

        double currentTime = Planetarium.GetUniversalTime();

        if (timeFoodRanOut != -1)
        {
            GUILayout.Label("Out of food! " + FormatTime(maxTimeNoFood - (currentTime - timeFoodRanOut)), warningStyle);
        }
        else if (foodCritical)
        {
            GUILayout.Label("Food level critical!", warningStyle);
        }

        if (timeWaterRanOut != -1)
        {
            GUILayout.Label("Out of Water! " + FormatTime(maxTimeNoWater - (currentTime - timeWaterRanOut)), warningStyle);
        }
        else if (waterCritical)
        {
            GUILayout.Label("Water level critical!", warningStyle);
        }

        if (timeOxygenRanOut != -1)
        {
            GUILayout.Label("Out of Oxygen! " + FormatTime(maxTimeNoOxygen - (currentTime - timeOxygenRanOut)), warningStyle);
        }
        else if (oxygenCritical)
        {
            GUILayout.Label("Oxygen level critical!", warningStyle);
        }

        GUILayout.EndVertical();

        GUI.DragWindow();

        if (GUI.changed)
        {
            windowPos.width = 100;
            windowPos.height = 100;
        }
    }

    public void Load(ConfigNode node)
    {
        Debug.Log("TAC LifeSupport Loading " + node);
        GetValue(node, "lastUpdateTime", out lastUpdateTime, -1);
        GetValue(node, "timeFoodRanOut", out timeFoodRanOut, -1);
        GetValue(node, "timeWaterRanOut", out timeWaterRanOut, -1);
        GetValue(node, "timeOxygenRanOut", out timeOxygenRanOut, -1);

        GetResourceIds();
    }

    public void Save(ConfigNode node)
    {
        node.AddValue("lastUpdateTime", lastUpdateTime);
        node.AddValue("timeFoodRanOut", timeFoodRanOut);
        node.AddValue("timeWaterRanOut", timeWaterRanOut);
        node.AddValue("timeOxygenRanOut", timeOxygenRanOut);
        Debug.Log("TAC LifeSupport Saving " + node);
    }

    private void GetResourceIds()
    {
        PartResourceLibrary resourceLibrary = PartResourceLibrary.Instance;
        foodResourceId = resourceLibrary.GetDefinition("TAC_Food").id;
        waterResourceId = resourceLibrary.GetDefinition("TAC_Water").id;
        oxygenResourceId = resourceLibrary.GetDefinition("TAC_Oxygen").id;
        co2ResourceId = resourceLibrary.GetDefinition("TAC_CO2").id;
        liquidWasteResourceId = resourceLibrary.GetDefinition("TAC_LiquidWaste").id;
        solidWasteResourceId = resourceLibrary.GetDefinition("TAC_SolidWaste").id;
    }

    private static void GetValue(ConfigNode config, string name, out double value, double defaultValue)
    {
        double newValue;
        if (config.HasValue(name) && double.TryParse(config.GetValue(name), out newValue))
        {
            value = newValue;
        }
        else
        {
            value = defaultValue;
        }
    }

    private double KillCrewMember(Vessel vessel, string causeOfDeath)
    {
        List<ProtoCrewMember> crew = vessel.GetVesselCrew();
        if (crew.Count > 0)
        {
            int crewMemberIndex = UnityEngine.Random.Range(0, crew.Count - 1);
            ProtoCrewMember crewMember = crew[crewMemberIndex];

            TimeWarp.SetRate(0, true);
            crewMember.Die();
            ScreenMessages.PostScreenMessage(crewMember.name + " died of " + causeOfDeath + "!", 10.0f, ScreenMessageStyle.UPPER_CENTER);

            return UnityEngine.Random.Range(1.0f, 60.0f);
        }

        return 0.0;
    }

    private string FormatTime(double time)
    {
        int hours = (int)(time / 3600);
        time -= hours * 3600;

        int minutes = (int)(time / 60);
        time -= minutes * 60;

        int seconds = (int)time;

        return hours.ToString("#0") + ":" + minutes.ToString("00") + ":" + seconds.ToString("00");
    }
}
