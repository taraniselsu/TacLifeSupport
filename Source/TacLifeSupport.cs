/**
 * Thunder Aerospace Corporation's Life Support for Kerbal Space Program.
 * Originally Written by Taranis Elsu.
 * This version written and maintained by JPLRepo (Jamie Leighton)
 * 
 * (C) Copyright 2013, Taranis Elsu
 * (C) Copyright 2016, Jamie Leighton
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
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Tac
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class TacStartOnce : MonoBehaviour
    {
        public static GlobalSettings globalSettings { get; set; }
        public static TacStartOnce Instance;
        
        public void Awake()
        {
            Instance = this;
            DontDestroyOnLoad(this);

            globalSettings = new GlobalSettings();
            ConfigNode[] globalNodes;
            globalNodes = GameDatabase.Instance.GetConfigNodes("TACLSGlobalSettings");
            if (globalNodes != null)
            {
                foreach (ConfigNode node in globalNodes)
                {
                    globalSettings.Load(node);
                }
            }
            else
            {
                this.LogError("Could not find TACLSGlobalSettings node!");
            }
        }

        public void Start()
        {
            Textures.LoadIconAssets();
        }
    }

    [KSPScenario(ScenarioCreationOptions.AddToAllGames, GameScenes.SPACECENTER, GameScenes.EDITOR, GameScenes.FLIGHT, GameScenes.TRACKSTATION)]
    public class TacLifeSupport : ScenarioModule
    {
        public static TacLifeSupport Instance { get; private set; }

        public TacGameSettings gameSettings { get; set; }
        
        private ConfigNode[] globalNodes;
        internal bool globalConfigChanged = false;

        private readonly List<Component> children = new List<Component>();

        
        public TacLifeSupport()
        {
            //this.Log("Constructor");
            Instance = this;
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
            
            gameSettings = new TacGameSettings();
            gameSettings.Load(gameNode);
            if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
            {
                TACEditorFilter.Instance.Setup();
            }
            foreach (Savable s in children.Where(c => c is Savable))
            {
                s.Load(gameNode);
            }

            this.Log("OnLoad: " + gameNode);
        }

        public override void OnSave(ConfigNode gameNode)
        {
            base.OnSave(gameNode);
            gameSettings.Save(gameNode);
            foreach (Savable s in children.Where(c => c is Savable))
            {
                s.Save(gameNode);
            }
            
            this.Log("OnSave: " + gameNode);
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
        void Load(ConfigNode gameNode);
        void Save(ConfigNode gameNode);
    }
}
