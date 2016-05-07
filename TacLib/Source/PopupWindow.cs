/**
 * PopupWindow.cs
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
using UnityEngine;

namespace Tac
{
    public class PopupWindow : MonoBehaviour
    {
        private static GameObject go;
        private static PopupWindow instance;
        private readonly int windowId;
        private bool showPopup;
        private Rect popupPos;
        private Func<int, object, bool> callback;
        private object parameter;

        private static PopupWindow GetInstance()
        {
            if (go == null)
            {
                go = new GameObject("TacPopupWindow");
                instance = go.AddComponent<PopupWindow>();
            }
            return instance;
        }

        PopupWindow()
        {
            windowId = "Tac.PopupWindow".GetHashCode();
        }

        void Awake()
        {
            showPopup = false;
        }

        void OnGUI()
        {
            if (showPopup)
            {
                GUI.skin = HighLogic.Skin;
                popupPos = Utilities.EnsureCompletelyVisible(popupPos);
                popupPos = GUILayout.Window(windowId, popupPos, DrawPopupContents, "");
            }
        }

        private void DrawPopupContents(int windowId)
        {
            GUI.BringWindowToFront(windowId);

            var pos = popupPos;
            var c = callback;

            bool shouldClose = callback(windowId, parameter);

            if (shouldClose && c == callback)
            {
                showPopup = false;
            }

            // Close the popup window if clicked somewhere outside it
            if (c == callback && (Input.GetMouseButton(0) || Input.GetMouseButton(1) || Input.GetMouseButton(2)))
            {
                var mousePos = new Vector3(Input.mousePosition.x, Screen.height - Input.mousePosition.y, Input.mousePosition.z);
                if (!pos.Contains(mousePos))
                {
                    showPopup = false;
                }
            }
        }

        public static void Draw(string buttonText, Rect windowPos, Func<int, object, bool> popupDrawCallback, GUIStyle buttonStyle, object parameter, params GUILayoutOption[] options)
        {
            PopupWindow pw = PopupWindow.GetInstance();

            var content = new GUIContent(buttonText);
            var rect = GUILayoutUtility.GetRect(content, buttonStyle, options);
            if (GUI.Button(rect, content, buttonStyle))
            {
                pw.showPopup = true;

                var mouse = Input.mousePosition;
                pw.popupPos = new Rect(mouse.x - 10, Screen.height - mouse.y - 10, 10, 10);

                pw.callback = popupDrawCallback;
                pw.parameter = parameter;
            }
        }
    }
}
