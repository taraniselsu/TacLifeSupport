/**
 * ButtonWrapper.cs
 * 
 * Thunder Aerospace Corporation's library for the Kerbal Space Program, by Taranis Elsu
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
    public class ButtonWrapper
    {
        private Icon<ButtonWrapper> icon = null;
        private ToolbarButton button = null;

        public bool Visible
        {
            get
            {
                if (button != null)
                {
                    return button.Visible;
                }
                else if (icon != null)
                {
                    return icon.Visible;
                }
                else
                {
                    return false;
                }
            }
            set
            {
                if (button != null)
                {
                    button.Visible = value;
                }
                if (icon != null)
                {
                    icon.Visible = value;
                }
            }
        }

        public ButtonWrapper(Rect defaultPosition, string imageFilename, string noImageText,
            string tooltip, Action onClickHandler, string configNodeName = "Icon")
        {
            button = ToolbarButton.Create(imageFilename, noImageText, tooltip, onClickHandler);
            if (button == null)
            {
                this.Log("Failed to create the toolbar button, using my Icon instead.");
                icon = new Icon<ButtonWrapper>(defaultPosition, imageFilename, noImageText, tooltip,
                    onClickHandler, configNodeName);
            }
        }

        public void OnGUI()
        {
            icon?.OnGUI();
        }

        public void Load(ConfigNode node)
        {
            if (icon != null)
            {
                icon.Load(node);
            }
        }

        public void Save(ConfigNode node)
        {
            if (icon != null)
            {
                icon.Save(node);
            }
        }

        public void Destroy()
        {
            if (button != null)
            {
                button.Destroy();
            }
            if (icon != null)
            {
                icon.Visible = false;
            }
        }
    }
}
