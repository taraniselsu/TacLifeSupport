/**
 * SettingsWindow.cs
 * 
 * Thunder Aerospace Corporation's Fuel Balancer for the Kerbal Space Program, by Taranis Elsu
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
    class SettingsWindow : Window<SettingsWindow>
    {
        private Settings settings;
        private GUIStyle labelStyle;
        private GUIStyle editStyle;
        private GUIStyle headerStyle;
        private GUIStyle buttonStyle;

        //private Vector2 scrollPosition;

        public SettingsWindow(Settings settings)
            : base("TAC Life Support Settings", 330, 120)//540, 600)
        {
            this.settings = settings;
        }

        protected override void ConfigureStyles()
        {
            base.ConfigureStyles();

            if (labelStyle == null)
            {
                labelStyle = new GUIStyle(GUI.skin.label);
                labelStyle.alignment = TextAnchor.MiddleLeft;
                labelStyle.fontStyle = FontStyle.Normal;
                //labelStyle.margin.top = 0;
                //labelStyle.margin.bottom = 0;
                //labelStyle.padding.top = 0;
                //labelStyle.padding.bottom = 0;
                labelStyle.normal.textColor = Color.white;
                labelStyle.wordWrap = false;

                editStyle = new GUIStyle(GUI.skin.textField);
                //editStyle.margin.top = 1;
                //editStyle.margin.bottom = 1;
                //editStyle.padding.top = 0;
                //editStyle.padding.bottom = 0;

                buttonStyle = new GUIStyle(GUI.skin.button);

                headerStyle = new GUIStyle(labelStyle);
                headerStyle.fontStyle = FontStyle.Bold;
            }
        }

        protected override void DrawWindowContents(int windowID)
        {
//            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            GUILayout.BeginVertical();
/*
            GUILayout.BeginHorizontal();
            GUILayout.Label("Food Consumption Rate", labelStyle);
            GUILayout.FlexibleSpace();
            settings.FoodConsumptionRate = Utilities.ShowTextField(settings.FoodConsumptionRate, 30, editStyle, GUILayout.MinWidth(150));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Water Consumption Rate", labelStyle);
            GUILayout.FlexibleSpace();
            settings.WaterConsumptionRate = Utilities.ShowTextField(settings.WaterConsumptionRate, 30, editStyle, GUILayout.MinWidth(150));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Oxygen Consumption Rate", labelStyle);
            GUILayout.FlexibleSpace();
            settings.OxygenConsumptionRate = Utilities.ShowTextField(settings.OxygenConsumptionRate, 30, editStyle, GUILayout.MinWidth(150));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Base Electricity Consumption Rate", labelStyle);
            GUILayout.FlexibleSpace();
            settings.BaseElectricityConsumptionRate = Utilities.ShowTextField(settings.BaseElectricityConsumptionRate, 30, editStyle, GUILayout.MinWidth(150));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Per Kerbal Electricity Consumption Rate", labelStyle);
            GUILayout.FlexibleSpace();
            settings.ElectricityConsumptionRate = Utilities.ShowTextField(settings.ElectricityConsumptionRate, 30, editStyle, GUILayout.MinWidth(150));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("EVA Electricity Consumption Rate", labelStyle);
            GUILayout.FlexibleSpace();
            settings.EvaElectricityConsumptionRate = Utilities.ShowTextField(settings.EvaElectricityConsumptionRate, 30, editStyle, GUILayout.MinWidth(150));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("CO2 Production Rate", labelStyle);
            GUILayout.FlexibleSpace();
            settings.CO2ProductionRate = Utilities.ShowTextField(settings.CO2ProductionRate, 30, editStyle, GUILayout.MinWidth(150));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Waste Production Rate", labelStyle);
            GUILayout.FlexibleSpace();
            settings.WasteProductionRate = Utilities.ShowTextField(settings.WasteProductionRate, 30, editStyle, GUILayout.MinWidth(150));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Waste Water Production Rate", labelStyle);
            GUILayout.FlexibleSpace();
            settings.WasteWaterProductionRate = Utilities.ShowTextField(settings.WasteWaterProductionRate, 30, editStyle, GUILayout.MinWidth(150));
            GUILayout.EndHorizontal();

            GUILayout.Space(20);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Max time without Food", labelStyle);
            GUILayout.FlexibleSpace();
            settings.MaxTimeWithoutFood = Utilities.ShowTextField(settings.MaxTimeWithoutFood, 20, editStyle, GUILayout.MinWidth(150));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Max time without Water", labelStyle);
            GUILayout.FlexibleSpace();
            settings.MaxTimeWithoutWater = Utilities.ShowTextField(settings.MaxTimeWithoutWater, 20, editStyle, GUILayout.MinWidth(150));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Max time without Oxygen", labelStyle);
            GUILayout.FlexibleSpace();
            settings.MaxTimeWithoutOxygen = Utilities.ShowTextField(settings.MaxTimeWithoutOxygen, 20, editStyle, GUILayout.MinWidth(150));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Max time without Electricity", labelStyle);
            GUILayout.FlexibleSpace();
            settings.MaxTimeWithoutElectricity = Utilities.ShowTextField(settings.MaxTimeWithoutElectricity, 20, editStyle, GUILayout.MinWidth(150));
            GUILayout.EndHorizontal();

            GUILayout.Space(20);
*/
            GUILayout.BeginHorizontal();
            GUILayout.Label("Allow Crew Respawn", labelStyle);
            GUILayout.FlexibleSpace();
            settings.AllowCrewRespawn = GUILayout.Toggle(settings.AllowCrewRespawn, "");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Crew Respawn Delay", labelStyle);
            GUILayout.FlexibleSpace();
            settings.RespawnDelay = Utilities.ShowTextField(settings.RespawnDelay, 20, editStyle, GUILayout.MinWidth(150));
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
//            GUILayout.EndScrollView();

            GUILayout.Space(8);
        }
    }
}
