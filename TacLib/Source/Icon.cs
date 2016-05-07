/**
 * Icon.cs
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

using KSP.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Tac
{
    public class Icon<T>
    {
        private string configNodeName;
        private bool mouseDown = false;
        private bool mouseWasDragged = false;
        private int iconId;
        private Rect iconPos;
        private Action onClick;
        private GUIContent content;
        private GUIStyle iconStyle;

        public bool Visible { get; set; }

        public Icon(Rect defaultPosition, string imageFilename, string noImageText, string tooltip, Action onClickHandler, string configNodeName = "Icon")
        {
            this.configNodeName = configNodeName;
            this.Log("Constructor: " + imageFilename);
            this.iconId = imageFilename.GetHashCode();
            this.iconPos = defaultPosition;
            this.onClick = onClickHandler;

            if (GameDatabase.Instance.ExistsTexture(imageFilename))
            {
                Texture2D texture = GameDatabase.Instance.GetTexture(imageFilename, false);
                content = new GUIContent(texture, tooltip);
            }
            else
            {
                content = new GUIContent(noImageText, tooltip);
            }
        }
		
		private void OnGUI()
		{
			if (this.Visible)
			{
                DrawIcon();
			}
		}

        private void DrawIcon()
        {
            GUI.skin = HighLogic.Skin;
            ConfigureStyles();

            GUI.Label(iconPos, content, iconStyle);
            HandleIconEvents();
        }

        public void Load(ConfigNode node)
        {
            if (node.HasNode(configNodeName))
            {
                ConfigNode iconNode = node.GetNode(configNodeName);

                iconPos.x = Utilities.GetValue(iconNode, "xPos", iconPos.x);
                iconPos.y = Utilities.GetValue(iconNode, "yPos", iconPos.y);

                iconPos = Utilities.EnsureVisible(iconPos, Math.Min(iconPos.width, iconPos.height));
            }
        }

        public void Save(ConfigNode node)
        {
            ConfigNode iconNode;
            if (node.HasNode(configNodeName))
            {
                iconNode = node.GetNode(configNodeName);
                iconNode.ClearData();
            }
            else
            {
                iconNode = node.AddNode(configNodeName);
            }

            iconNode.AddValue("xPos", iconPos.x);
            iconNode.AddValue("yPos", iconPos.y);
        }

        private void ConfigureStyles()
        {
            if (iconStyle == null)
            {
                iconStyle = new GUIStyle(GUI.skin.button);
                iconStyle.alignment = TextAnchor.MiddleCenter;
                iconStyle.padding = new RectOffset(1, 1, 1, 1);
            }
        }

        private void HandleIconEvents()
        {
            var theEvent = Event.current;
            if (theEvent != null)
            {
                if (!mouseDown)
                {
                    if (theEvent.type == EventType.MouseDown && theEvent.button == 0 && iconPos.Contains(theEvent.mousePosition))
                    {
                        mouseDown = true;
                        theEvent.Use();
                    }
                }
                else if (theEvent.type != EventType.Layout)
                {
                    if (Input.GetMouseButton(0))
                    {
                        // Flip the mouse Y so that 0 is at the top
                        float mouseY = Screen.height - Input.mousePosition.y;

                        if (mouseWasDragged)
                        {
                            iconPos.x = Mathf.Clamp(Input.mousePosition.x - (iconPos.width / 2), 0, Screen.width - iconPos.width);
                            iconPos.y = Mathf.Clamp(mouseY - (iconPos.height / 2), 0, Screen.height - iconPos.height);
                        }
                        else if (Mathf.Abs(iconPos.x - Input.mousePosition.x) > iconPos.width || Mathf.Abs(iconPos.y - mouseY) > iconPos.height)
                        {
                            mouseWasDragged = true;
                        }
                    }
                    else
                    {
                        if (!mouseWasDragged)
                        {
                            onClick();
                        }

                        mouseDown = false;
                        mouseWasDragged = false;
                    }
                }
            }
        }
    }
}
