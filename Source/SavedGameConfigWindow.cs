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
    class SavedGameConfigWindow : Window<SavedGameConfigWindow>
    {
        private GlobalSettings globalSettings;
        private TacGameSettings gameSettings;
        private GUIStyle labelStyle;
        private GUIStyle editStyle;
        private GUIStyle headerStyle;
        private GUIStyle headerStyle2;
        private GUIStyle warningStyle;
        private GUIStyle buttonStyle;

        private bool showConsumptionRates = false;
        private bool showMaxTimeWithout = false;
        private bool showDefaultResourceAmounts = false;

        private readonly string version;

        public SavedGameConfigWindow(GlobalSettings globalSettings, TacGameSettings gameSettings)
            : base("TAC Life Support Settings", 400, 300)
        {
            base.Resizable = false;
            this.globalSettings = globalSettings;
            this.gameSettings = gameSettings;

            version = Utilities.GetDllVersion(this);
        }

        protected override void ConfigureStyles()
        {
            base.ConfigureStyles();

            if (labelStyle == null)
            {
                labelStyle = new GUIStyle(GUI.skin.label);
                labelStyle.alignment = TextAnchor.MiddleLeft;
                labelStyle.fontStyle = FontStyle.Normal;
                labelStyle.normal.textColor = Color.white;
                labelStyle.wordWrap = false;

                editStyle = new GUIStyle(GUI.skin.textField);
                editStyle.alignment = TextAnchor.MiddleRight;

                headerStyle = new GUIStyle(labelStyle);
                headerStyle.fontStyle = FontStyle.Bold;

                headerStyle2 = new GUIStyle(headerStyle);
                headerStyle2.wordWrap = true;

                buttonStyle = new GUIStyle(GUI.skin.button);

                warningStyle = new GUIStyle(headerStyle2);
                warningStyle.normal.textColor = new Color(0.88f, 0.20f, 0.20f, 1.0f);
            }
        }

        protected override void DrawWindowContents(int windowId)
        {
            GUILayout.Label("Version: " + version, labelStyle);
            GUILayout.Label("Configure TAC Life Support for use with this saved game.", headerStyle);
            gameSettings.Enabled = GUILayout.Toggle(gameSettings.Enabled, "Enabled");

            if (gameSettings.Enabled)
            {
                GUILayout.Space(10);

                //string[] killOptions = { "Die", "Hibernate" };
                //int oldValue = (gameSettings.HibernateInsteadOfKill) ? 1 : 0;

                //GUILayout.BeginHorizontal();
                //GUILayout.Label("When resources run out, Kerbals ", labelStyle);
                //int newValue = GUILayout.SelectionGrid(oldValue, killOptions, 2);
                //GUILayout.EndHorizontal();
                //gameSettings.HibernateInsteadOfKill = (newValue == 1);

                if (!gameSettings.HibernateInsteadOfKill && HighLogic.CurrentGame.Parameters.Difficulty.MissingCrewsRespawn)
                {
                    gameSettings.RespawnDelay = Utilities.ShowTextField("Respawn delay (seconds)", labelStyle,
                        gameSettings.RespawnDelay, 30, editStyle, GUILayout.MinWidth(100));
                }

                GUILayout.Space(10);
                ConsumptionRates();
                GUILayout.Space(10);
                MaxTimeWithout();
                GUILayout.Space(10);
                DefaultResourceAmounts();
            }

            if (GUI.changed)
            {
                SetSize(10, 10);
            }
        }

        private void ConsumptionRates()
        {
            showConsumptionRates = GUILayout.Toggle(showConsumptionRates, "Resource Consumption Rates", buttonStyle);

            if (showConsumptionRates)
            {
                GUILayout.BeginVertical(GUI.skin.box);
                GUILayout.Label("These settings affect all saves. Restart KSP for changes to take effect.", warningStyle);
                GUILayout.Label("The following values are in units per second.", headerStyle);
                GUILayout.Label("See the TacResources.cfg for conversions between units and metric tons.", headerStyle);

                globalSettings.FoodConsumptionRate = Utilities.ShowTextField("Food Consumption Rate", labelStyle,
                    globalSettings.FoodConsumptionRate, 30, editStyle, GUILayout.MinWidth(150));
                globalSettings.WaterConsumptionRate = Utilities.ShowTextField("Water Consumption Rate", labelStyle,
                    globalSettings.WaterConsumptionRate, 30, editStyle, GUILayout.MinWidth(150));
                globalSettings.OxygenConsumptionRate = Utilities.ShowTextField("Oxygen Consumption Rate", labelStyle,
                    globalSettings.OxygenConsumptionRate, 30, editStyle, GUILayout.MinWidth(150));

                GUILayout.Space(5);

                globalSettings.BaseElectricityConsumptionRate = Utilities.ShowTextField("Base Electricity Consumption Rate", labelStyle,
                    globalSettings.BaseElectricityConsumptionRate, 30, editStyle, GUILayout.MinWidth(150));
                globalSettings.ElectricityConsumptionRate = Utilities.ShowTextField("Per Kerbal Electricity Consumption Rate", labelStyle,
                    globalSettings.ElectricityConsumptionRate, 30, editStyle, GUILayout.MinWidth(150));
                globalSettings.EvaElectricityConsumptionRate = Utilities.ShowTextField("EVA Electricity Consumption Rate", labelStyle,
                    globalSettings.EvaElectricityConsumptionRate, 30, editStyle, GUILayout.MinWidth(150));

                GUILayout.Space(5);

                globalSettings.CO2ProductionRate = Utilities.ShowTextField("CarbonDioxide Production Rate", labelStyle,
                    globalSettings.CO2ProductionRate, 30, editStyle, GUILayout.MinWidth(150));
                globalSettings.WasteProductionRate = Utilities.ShowTextField("Waste Production Rate", labelStyle,
                    globalSettings.WasteProductionRate, 30, editStyle, GUILayout.MinWidth(150));
                globalSettings.WasteWaterProductionRate = Utilities.ShowTextField("Waste Water Production Rate", labelStyle,
                    globalSettings.WasteWaterProductionRate, 30, editStyle, GUILayout.MinWidth(150));

                GUILayout.Space(5);

                globalSettings.MaxDeltaTime = (int)Utilities.ShowTextField("Max Delta Time", labelStyle, globalSettings.MaxDeltaTime,
                    30, editStyle, GUILayout.MinWidth(150));
                globalSettings.ElectricityMaxDeltaTime = (int)Utilities.ShowTextField("Max Delta Time (Electricity)", labelStyle,
                    globalSettings.ElectricityMaxDeltaTime, 30, editStyle, GUILayout.MinWidth(150));

                GUILayout.EndVertical();
            }
        }

        private void MaxTimeWithout()
        {
            showMaxTimeWithout = GUILayout.Toggle(showMaxTimeWithout, "Maximum time without resources", buttonStyle);

            if (showMaxTimeWithout)
            {
                GUILayout.BeginVertical(GUI.skin.box);
                GUILayout.Label("These settings affect all saves. Restart KSP for changes to take effect.", warningStyle);
                GUILayout.Label("The following values are in seconds.", headerStyle);

                globalSettings.MaxTimeWithoutFood = Utilities.ShowTextField("Max time without Food", labelStyle,
                    globalSettings.MaxTimeWithoutFood, 20, editStyle, GUILayout.MinWidth(150));
                globalSettings.MaxTimeWithoutWater = Utilities.ShowTextField("Max time without Water", labelStyle,
                    globalSettings.MaxTimeWithoutWater, 20, editStyle, GUILayout.MinWidth(150));
                globalSettings.MaxTimeWithoutOxygen = Utilities.ShowTextField("Max time without Oxygen", labelStyle,
                    globalSettings.MaxTimeWithoutOxygen, 20, editStyle, GUILayout.MinWidth(150));
                globalSettings.MaxTimeWithoutElectricity = Utilities.ShowTextField("Max time without Electricity", labelStyle,
                    globalSettings.MaxTimeWithoutElectricity, 20, editStyle, GUILayout.MinWidth(150));
                GUILayout.EndVertical();
            }
        }

        private void DefaultResourceAmounts()
        {
            showDefaultResourceAmounts = GUILayout.Toggle(showDefaultResourceAmounts, "Default resource amounts", buttonStyle);

            if (showDefaultResourceAmounts)
            {
                GUILayout.BeginVertical(GUI.skin.box);
                GUILayout.Label("These settings affect all saves. Restart KSP for changes to take effect.", warningStyle);
                GUILayout.Label("Will automatically stock EVA suits with the following amount of resources (in seconds).", headerStyle2);

                globalSettings.EvaDefaultResourceAmount = Utilities.ShowTextField("Default amount for EVA suits", labelStyle,
                    globalSettings.EvaDefaultResourceAmount, 20, editStyle, GUILayout.MinWidth(150));
                GUILayout.EndVertical();
            }
        }
    }
}
