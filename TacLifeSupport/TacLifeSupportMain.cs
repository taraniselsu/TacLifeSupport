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
    private int numCrew;

    private double foodConsumptionRate;
    private double waterConsumptionRate;
    private double oxygenConsumptionRate;

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
        numCrew = -1;

        // consumption rates in kg per Earth day (24-hour)
        foodConsumptionRate = 0.62;
        waterConsumptionRate = 3.52;
        oxygenConsumptionRate = 0.84;
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
                    lastUpdateTime = Planetarium.GetUniversalTime();// vessel.launchTime;
                }
            }
            else
            {
                double currentTime = Planetarium.GetUniversalTime();
                numCrew = vessel.GetCrewCount();

                double timeDelta = currentTime - lastUpdateTime;
                lastUpdateTime = currentTime;

                double food = vessel.rootPart.RequestResource("TAC_Food", numCrew * timeDelta * foodConsumptionRate / SECONDS_PER_DAY);
                double water = vessel.rootPart.RequestResource("TAC_Water", numCrew * timeDelta * waterConsumptionRate / SECONDS_PER_DAY);

                if (!vessel.orbit.referenceBody.atmosphereContainsOxygen || FlightGlobals.getStaticPressure() < 0.2)
                {
                    double oxygen = vessel.rootPart.RequestResource("TAC_Oxygen", numCrew * timeDelta * oxygenConsumptionRate / SECONDS_PER_DAY);
                }
            }
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
        if (!PauseMenu.isOpen)
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

        GUILayout.BeginHorizontal();

        GUILayout.BeginVertical();
        GUILayout.Label("Last update time:", labelStyle);
        GUILayout.Label("Kerbals:", labelStyle);
        GUILayout.EndVertical();

        GUILayout.BeginVertical();
        GUILayout.Label(lastUpdateTime.ToString(), labelStyle);
        GUILayout.Label(numCrew.ToString(), labelStyle);
        GUILayout.EndVertical();

        GUILayout.EndHorizontal();

        GUI.DragWindow();
    }

    public void Load(ConfigNode node)
    {
        double newDouble;
        if (node.HasValue("TACLS_lastUpdateTime") && double.TryParse(node.GetValue("TACLS_lastUpdateTime"), out newDouble))
        {
            lastUpdateTime = newDouble;
        }
        else
        {
            lastUpdateTime = -1;
        }
    }

    public void Save(ConfigNode node)
    {
        node.AddValue("TACLS_lastUpdateTime", lastUpdateTime);
    }
}
