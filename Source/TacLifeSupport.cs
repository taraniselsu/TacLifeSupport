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
    [KSPScenario(ScenarioCreationOptions.AddToAllGames, GameScenes.SPACECENTER, GameScenes.EDITOR, GameScenes.FLIGHT, GameScenes.TRACKSTATION)]
    public class TacLifeSupport : ScenarioModule
    {
        public static TacLifeSupport Instance { get; private set; }

        public TacGameSettings gameSettings { get; private set; }
        public GlobalSettings globalSettings { get; private set; }

        private readonly string globalConfigFilename;
        private ConfigNode globalNode = new ConfigNode();

        private readonly List<Component> children = new List<Component>();

        public TacLifeSupport()
        {
            this.Log("Constructor");
            Instance = this;
            gameSettings = new TacGameSettings();
            globalSettings = new GlobalSettings();

            globalConfigFilename = IOUtils.GetFilePathFor(this.GetType(), "LifeSupport.cfg");
        }

        public override void OnAwake()
        {
            this.Log("OnAwake in " + HighLogic.LoadedScene);
            base.OnAwake();

            GameEvents.onGameSceneLoadRequested.Add(OnGameSceneLoadRequested);

            if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
            {
                this.Log("Adding SpaceCenterManager");
                var c = gameObject.AddComponent<SpaceCenterManager>();
                children.Add(c);
                /* This does show the window, but only shows one button in the toolbar
                this.Log("Adding LifeSupportController");
                var d = gameObject.AddComponent<LifeSupportController>();
                children.Add(d);
                */
            }
            else if (HighLogic.LoadedScene == GameScenes.TRACKSTATION)
            {
                this.Log("Adding LifeSupportController");
                var c = gameObject.AddComponent<LifeSupportController>();
                children.Add(c);
            }
            else if (HighLogic.LoadedScene == GameScenes.FLIGHT)
            {
                this.Log("Adding LifeSupportController");
                var c = gameObject.AddComponent<LifeSupportController>();
                children.Add(c);
            }
            else if (HighLogic.LoadedScene == GameScenes.EDITOR)
            {
                this.Log("Adding EditorController");
                var c = gameObject.AddComponent<EditorController>();
                children.Add(c);
            }
        }

        public override void OnLoad(ConfigNode gameNode)
        {
            base.OnLoad(gameNode);
            gameSettings.Load(gameNode);

            // Load the global settings
            if (File.Exists<TacLifeSupport>(globalConfigFilename))
            {
                globalNode = ConfigNode.Load(globalConfigFilename);
                globalSettings.Load(globalNode);
                foreach (Savable s in children.Where(c => c is Savable))
                {
                    s.Load(globalNode);
                }
            }

            this.Log("OnLoad: " + gameNode + "\n" + globalNode);
        }

        public override void OnSave(ConfigNode gameNode)
        {
            base.OnSave(gameNode);
            gameSettings.Save(gameNode);

            // Save the global settings
            globalSettings.Save(globalNode);
            foreach (Savable s in children.Where(c => c is Savable))
            {
                s.Save(globalNode);
            }
            globalNode.Save(globalConfigFilename);

            this.Log("OnSave: " + gameNode + "\n" + globalNode);
        }

        private void OnGameSceneLoadRequested(GameScenes gameScene)
        {
            this.Log("Game scene load requested: " + gameScene);
        }

        void OnDestroy()
        {
            this.Log("OnDestroy");
            foreach (Component c in children)
            {
                Destroy(c);
            }
            children.Clear();
        }
    }

    interface Savable
    {
        void Load(ConfigNode globalNode);
        void Save(ConfigNode globalNode);
    }
}
