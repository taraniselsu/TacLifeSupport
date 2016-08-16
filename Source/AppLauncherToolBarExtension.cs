/*
 * AppLauncherToolBar.cs
 * (C) Copyright 2016, Jamie Leighton (JPLRepo)
 * REPOSoft Technologies 
 * Kerbal Space Program is Copyright (C) 2013 Squad. See http://kerbalspaceprogram.com/. This
 * project is in no way associated with nor endorsed by Squad.
 *
 *  This file is part of RST Utils. My attempt at creating my own KSP Mod base Architecture.
 *
 *  RST Utils is free software: you can redistribute it and/or modify
 *  it under the terms of the MIT License 
 *
 *  RST Utils is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  
 *
 *  You should have received a copy of the MIT License
 *  along with RST Utils.  If not, see <http://opensource.org/licenses/MIT>.
 *
 */
using System;
using KSP.UI.Screens;
using UnityEngine;

namespace RSTUtils
{
    public class AppLauncherToolBar
    {
        public static AppLauncherToolBar Instance { get; private set; }

        private bool usingToolbar = false; //Set to true if user is using ToolBar
        private IButton button1;  //ToolBar button
        private string toolBarName;  //set to Name for button on ToolBar (modname)
        private string toolBarToolTip; // set tooltip for ToolBar button
        private string toolBarTexturePath; //The TooBar formatted Texture Path
        private GameScenesVisibility toolBarGameScenes; //GameScenes toolbar button can be seen in

        private bool usingStock = false; //Set to true if user is using Stock AppLauncher
        private ApplicationLauncherButton stockToolbarButton; // Stock Toolbar Button
        private ApplicationLauncher.AppScenes VisibleinScenes; //What scenes is the applauncher button seen in
        private UnityEngine.Texture appbtnTexON; //Texture for AppLauncher button when ON
        private UnityEngine.Texture appbtnTexOFF; //Texture for AppLauncher button when OFF
        private bool showHoverText = false; //Whether to show AppLauncher Hover Text or not.

        private bool _gamePaused;
        public Boolean gamePaused
        {
            get { return _gamePaused; }
            private set
            {
                _gamePaused = value;      //Set the private variable
            }
        }

        private bool _hideUI;
        public Boolean hideUI
        {
            get { return _hideUI; }
            private set
            {
                _hideUI = value;      //Set the private variable
            }
        }

        public bool StockButtonNotNull
        {
            get { return stockToolbarButton != null; }
        }

        public bool ToolBarButtonNotNull
        {
            get { return button1 != null;  }
        }

        public bool usingToolBar
        {
            get { return usingToolbar; }
        }

        public bool usingAppLauncher
        {
            get { return usingStock; }
        }

        //GuiVisibility
        private bool _Visible;
        public Boolean GuiVisible
        {
            get { return _Visible; }
            set
            {
                _Visible = value;      //Set the private variable
            }
        }

        public bool ShowHoverText
        {
            get { return showHoverText; }
        }

        private void GamePaused()
        {
            gamePaused = true;
        }

        private void GameUnPaused()
        {
            gamePaused = false;
        }

        private void onHideUI()
        {
            hideUI = true;
        }

        private void onShowUI()
        {
            hideUI = false;
        }
        /// <summary>
        /// Constructor for AppLauncherToolBar. You need to construct one of these for your Mod Menu/GUI environment.
        /// </summary>
        /// <param name="toolBarName">A string passed into ToolBar indicating the Name of the Mod</param>
        /// <param name="toolBarToolTip">A string passed into ToolBar to use for the Icon ToolTip</param>
        /// <param name="toolBarTexturePath">A string in ToolBar expected format of the TexturePath for the ToolBarIcon</param>
        /// <param name="VisibleinScenes">ApplicationLauncher.AppScenes list - logically OR'd</param>
        /// <param name="appbtnTexON">Texture reference for the AppLauncher ON Icon</param>
        /// <param name="appbtnTexOFF">Texture reference for the AppLauncher OFF Icon</param>
        /// <param name="gameScenes">A list of GameScenes use for ToolBar icon visiblity</param>
        public AppLauncherToolBar(string toolBarName, string toolBarToolTip, string toolBarTexturePath,  
            ApplicationLauncher.AppScenes VisibleinScenes, UnityEngine.Texture appbtnTexON, UnityEngine.Texture appbtnTexOFF, params GameScenes[] gameScenes)
        {
            Instance = this;
            if (ToolbarManager.ToolbarAvailable)
            {
                this.toolBarName = toolBarName;
                this.toolBarToolTip = toolBarToolTip;
                this.toolBarTexturePath = toolBarTexturePath;
                this.toolBarGameScenes = new GameScenesVisibility(gameScenes);
            }
            this.VisibleinScenes = VisibleinScenes;
            this.appbtnTexON = appbtnTexON;
            this.appbtnTexOFF = appbtnTexOFF;
        }

        private void OnGUIAppLauncherReady()
        {
            Utilities.Log_Debug("OnGUIAppLauncherReady");
            if (ApplicationLauncher.Ready)
            {
                Utilities.Log_Debug("Adding AppLauncherButton");
                stockToolbarButton = ApplicationLauncher.Instance.AddModApplication(
                    onAppLaunchToggle,
                    onAppLaunchToggle,
                    onHoverOn,
                    onHoverOff,
                    DummyVoid,
                    DummyVoid,
                    VisibleinScenes,
                    appbtnTexOFF);
            }
        }

        private void DummyVoid()
        {
        }

        private void onHoverOn()
        {
            showHoverText = true;
        }
        private void onHoverOff()
        {
            showHoverText = false;
        }

        public void onAppLaunchToggle()
        {
            GuiVisible = !GuiVisible;
            if (stockToolbarButton != null)
            {
                stockToolbarButton.SetTexture(GuiVisible ? appbtnTexON : appbtnTexOFF);
            }
        }

        private void DestroyToolBar()
        {
            if (ToolbarManager.ToolbarAvailable)
            {
                if (button1 != null)
                    button1.Destroy();
            }
        }

        private void CreateToolBar()
        {
            if (ToolbarManager.ToolbarAvailable)
            {
                button1 = ToolbarManager.Instance.add(toolBarName, "button1");
                button1.TexturePath = toolBarTexturePath;
                button1.ToolTip = toolBarToolTip;
                button1.Visibility = toolBarGameScenes;
                button1.OnClick += e => GuiVisible = !GuiVisible;
            }
        }

        private void DestroyStockButton()
        {
            GameEvents.onGUIApplicationLauncherReady.Remove(OnGUIAppLauncherReady);
            if (stockToolbarButton != null)
            {
                ApplicationLauncher.Instance.RemoveModApplication(stockToolbarButton);
                stockToolbarButton = null;
            }
        }

        private void CreateStockButton()
        {
            Utilities.Log_Debug("Adding onGUIAppLauncher callbacks");
            if (ApplicationLauncher.Ready)
            {
                if (stockToolbarButton == null)
                    OnGUIAppLauncherReady();
            }
            else
                GameEvents.onGUIApplicationLauncherReady.Add(OnGUIAppLauncherReady);
        }

        private void OnGameSceneLoadRequestedForAppLauncher(GameScenes SceneToLoad)
        {
            if (stockToolbarButton != null)
            {
                ApplicationLauncherButton[] lstButtons = UnityEngine.Object.FindObjectsOfType<ApplicationLauncherButton>();
                Utilities.Log_Debug("TSTMenu AppLauncher: Destroying Button-Button Count:" + lstButtons.Length);
                ApplicationLauncher.Instance.RemoveModApplication(stockToolbarButton);
                stockToolbarButton = null;
            }
        }

        /// <summary>
        /// This Class is not using MonoBehaviour but has a Start Method that must be called.
        /// Call this in your Start Method for a Mod GUI/Menu Class.
        /// </summary>
        /// <param name="stock">True if we are to use the Stock Applauncher, False to use ToolBar mod</param>
        public void Start(bool stock)
        {
            DestroyToolBar();
            if (ToolbarManager.ToolbarAvailable && !stock)
            {
                // Set up ToolBar button
                CreateToolBar();
                usingToolbar = true;
                usingStock = false;
            }
            else
            {
                // Set up the stock toolbar
                CreateStockButton();
                usingToolbar = false;
                usingStock = true;
            }
            GameEvents.onGameSceneLoadRequested.Add(OnGameSceneLoadRequestedForAppLauncher);
            GameEvents.onGamePause.Add(GamePaused);
            GameEvents.onGameUnpause.Add(GameUnPaused);
            GameEvents.onHideUI.Add(onHideUI);
            GameEvents.onShowUI.Add(onShowUI);
        }

        /// <summary>
        /// This Class is not using MonoBehaviour but has a Destroy Method that must be called.
        /// Call this in your OnDestroy Method for a Mod GUI/Menu Class.
        /// </summary>
        public void Destroy()
        {
            DestroyToolBar();

            // Stock toolbar
            Utilities.Log_Debug("Removing onGUIAppLauncher callbacks");
            
            DestroyStockButton();

            if (GuiVisible) GuiVisible = !GuiVisible;
            GameEvents.onGameSceneLoadRequested.Remove(OnGameSceneLoadRequestedForAppLauncher);
            GameEvents.onGamePause.Remove(GamePaused);
            GameEvents.onGameUnpause.Remove(GameUnPaused);
            GameEvents.onHideUI.Remove(onHideUI);
            GameEvents.onShowUI.Remove(onShowUI);
        }

        /// <summary>
        /// Sets the ToolBar Icon visible or not. To be extended in future to not require calling from Mod.
        /// Currently it is because I haven't incorporated the mod's Setting for Whether the user wants to use Stock AppLauncher or Toolbar.
        /// </summary>
        /// <param name="visible">True if set to visible, false will turn it off</param>
        public void setToolBarBtnVisibility(bool visible)
        {
            button1.Visible = visible;
        }

        /// <summary>
        /// Sets the Applauncher Icon visible or not. To be extended in future to not require calling from Mod.
        /// Currently it is because I haven't incorporated the mod's Setting for Whether the user wants to use Stock AppLauncher or Toolbar.
        /// </summary>
        /// <param name="visible">True if set to visible, false will turn it off</param>
        public void setAppLSceneVisibility(ApplicationLauncher.AppScenes visibleinScenes)
        {
            VisibleinScenes = visibleinScenes;
            stockToolbarButton.VisibleInScenes = VisibleinScenes;
        }

        /// <summary>
        /// Call this to change from AppLauncher to Toobar or vice-versa.
        /// Will Destroy ToolBar or AppLauncher Icon and create a new one.
        /// </summary>
        /// <param name="stock">True if using AppLauncher, False if using ToolBar</param>
        public void chgAppIconStockToolBar(bool stock)
        {
            if (!stock && ToolbarManager.ToolbarAvailable)
            {
                DestroyStockButton();
                DestroyToolBar();
                CreateToolBar();
                usingToolbar = true;
                usingStock = false;

            }
            else
            {
                DestroyToolBar();
                DestroyStockButton();
                CreateStockButton();
                usingToolbar = false;
                usingStock = true;
            }
        }

        /// <summary>
        /// Change the ToolBar TexturePath - to say change the Icon
        /// </summary>
        /// <param name="icontoSet">string in ToolBar TexturePath format</param>
        public void setToolBarTexturePath(string icontoSet)
        {
            button1.TexturePath = icontoSet;
        }

        /// <summary>
        /// Change the AppLauncher Icon Texture - to say change the Icon
        /// </summary>
        /// <param name="icontoSet">Texture to set Icon to</param>
        public void setAppLauncherTexture(Texture icontoSet)
        {
            stockToolbarButton.SetTexture(icontoSet);
        }
    }
}
