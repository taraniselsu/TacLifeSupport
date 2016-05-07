/**
 * Utilities.cs
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
    public static class Logging
    {
        public static void Log(this UnityEngine.Object obj, string message)
        {
            Debug.Log("-INFO- " + obj.GetType().FullName + "[" + obj.GetInstanceID().ToString("X") + "][" + Time.time.ToString("0.00") + "]: " + message, obj);
        }

        public static void LogWarning(this UnityEngine.Object obj, string message)
        {
            Debug.LogWarning("-WARNING- " + obj.GetType().FullName + "[" + obj.GetInstanceID().ToString("X") + "][" + Time.time.ToString("0.00") + "]: " + message, obj);
        }

        public static void LogError(this UnityEngine.Object obj, string message)
        {
            Debug.LogError("-ERROR- " + obj.GetType().FullName + "[" + obj.GetInstanceID().ToString("X") + "][" + Time.time.ToString("0.00") + "]: " + message, obj);
        }

        public static void Log(this System.Object obj, string message)
        {
            Debug.Log("-INFO- " + obj.GetType().FullName + "[" + obj.GetHashCode().ToString("X") + "][" + Time.time.ToString("0.00") + "]: " + message);
        }

        public static void LogWarning(this System.Object obj, string message)
        {
            Debug.LogWarning("-WARNING- " + obj.GetType().FullName + "[" + obj.GetHashCode().ToString("X") + "][" + Time.time.ToString("0.00") + "]: " + message);
        }

        public static void LogError(this System.Object obj, string message)
        {
            Debug.LogError("-ERROR- " + obj.GetType().FullName + "[" + obj.GetHashCode().ToString("X") + "][" + Time.time.ToString("0.00") + "]: " + message);
        }

        public static void Log(string context, string message)
        {
            Debug.Log("-INFO- " + context + "[][" + Time.time.ToString("0.00") + "]: " + message);
        }

        public static void LogWarning(string context, string message)
        {
            Debug.LogWarning("-WARNING- " + context + "[][" + Time.time.ToString("0.00") + "]: " + message);
        }

        public static void LogError(string context, string message)
        {
            Debug.LogError("-ERROR- " + context + "[][" + Time.time.ToString("0.00") + "]: " + message);
        }
    }
}
