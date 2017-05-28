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

using System.Collections.Generic;
using UnityEngine;

namespace Tac
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class TacStartOnce : MonoBehaviour
    {
        public GlobalSettings globalSettings { get; set; }
        public static TacStartOnce Instance;
        
        public void Awake()
        {
            Instance = this;
            DontDestroyOnLoad(this);

            globalSettings = new GlobalSettings();
            LoadGlobalSettings();
        }

        public void Start()
        {
            GameEvents.OnGameSettingsApplied.Add(ApplySettings);
            GameEvents.onLevelWasLoaded.Add(LevelLoaded);
            Textures.LoadIconAssets();
        }

        public void LoadGlobalSettings()
        {
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

        public void OnDestroy()
        {
            GameEvents.OnGameSettingsApplied.Remove(ApplySettings);
            GameEvents.onLevelWasLoaded.Remove(LevelLoaded);
        }

        public void ApplySettings()
        {
            // If TAC LS is enabled re-apply TACLS custom Part filter and if it is not, turn off the TACLS custom Part filter.
            if (HighLogic.CurrentGame != null & HighLogic.CurrentGame.Parameters.CustomParams<TAC_SettingsParms>() != null)
            {
                if (HighLogic.CurrentGame.Parameters.CustomParams<TAC_SettingsParms>().enabled)
                {
                    if (TacLifeSupport.Instance != null)
                    {
                        if (TacLifeSupport.Instance.gameSettings == null)
                        {
                            TacLifeSupport.Instance.gameSettings = new TacGameSettings();
                        }
                    }
                    if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
                    {
                        TACEditorFilter.Instance.Setup();
                    }
                }
                else
                {
                    HighLogic.CurrentGame.Parameters.CustomParams<TAC_SettingsParms>().EditorFilter = false;
                    TACEditorFilter.Instance.Setup();
                }
            }
        }

        //If the MainMenu is loaded we reset the AddLifeSupport.initialized bool so the resources get re-added to the kerbal EVA prefabs.
        public void LevelLoaded(GameScenes scene)
        {
            if (scene == GameScenes.MAINMENU)
            {
                AddLifeSupport.initialized = false;
            }
        }
    }

    [KSPScenario(ScenarioCreationOptions.AddToAllGames, GameScenes.SPACECENTER, GameScenes.EDITOR, GameScenes.FLIGHT, GameScenes.TRACKSTATION)]
    public class TacLifeSupport : ScenarioModule
    {
        public static TacLifeSupport Instance;

        public TacGameSettings gameSettings { get; set; }
        
        private ConfigNode[] globalNodes;
        internal bool globalConfigChanged = false;

        private readonly List<Component> children = new List<Component>();

        public bool Enabled
        {
            get
            {
                if (HighLogic.CurrentGame != null)
                {
                    return HighLogic.CurrentGame.Parameters.CustomParams<TAC_SettingsParms>().enabled;
                }
                return true;
            }
        }

        public double BaseElectricityConsumptionRate
        {
            get { return TacStartOnce.Instance.globalSettings.BaseElectricityConsumptionRate; }
        }

        public double ElectricityConsumptionRate
        {
            get { return TacStartOnce.Instance.globalSettings.ElectricityConsumptionRate; }
        }

        public TacLifeSupport()
        {
            //this.Log("Constructor");
            Instance = this;
        }

        public override void OnAwake()
        {
            this.Log("OnAwake in " + HighLogic.LoadedScene);
            base.OnAwake();
            if (Enabled)
            {
                GameEvents.onGameSceneLoadRequested.Add(OnGameSceneLoadRequested);

                if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
                {
                    this.Log("Adding SpaceCenterManager");
                    var c = gameObject.AddComponent<SpaceCenterManager>();
                    children.Add(c);
                    this.Log("Adding LifeSupportController");
                    var d = gameObject.AddComponent<LifeSupportController>();
                    children.Add(d);
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
        }

        public override void OnLoad(ConfigNode gameNode)
        {
            base.OnLoad(gameNode);
            if (Enabled)
            {
                gameSettings = new TacGameSettings();
                gameSettings.Load(gameNode);
                if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
                {
                    TACEditorFilter.Instance.Setup();
                }
                for (int i = 0; i < children.Count; ++i)
                {
                    if (children[i] is Savable)
                    {
                        var child = children[i] as Savable;
                        child.Load(gameNode);
                    }
                }
                //this.Log("OnLoad: " + gameNode);
            }
        }

        public override void OnSave(ConfigNode gameNode)
        {
            base.OnSave(gameNode);
            if (Enabled)
            {
                gameSettings.Save(gameNode);
                for (int i = 0; i < children.Count; ++i)
                {
                    if (children[i] is Savable)
                    {
                        var child = children[i] as Savable;
                        child.Save(gameNode);
                    }
                }
                //this.Log("OnSave: " + gameNode);
            }
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
