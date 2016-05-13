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

using System.Globalization;
using UnityEngine;

namespace Tac
{
    public static class Logging
    {
        const string Info = "INFO";
        const string Warning = "WARNING";
        const string Error = "ERROR";

        public static void Log(this UnityEngine.Object obj, string message)
        {
            Debug.Log(GenerateLogMessage(Info, obj, message), obj);
        }

        public static void LogWarning(this UnityEngine.Object obj, string message)
        {
            Debug.LogWarning(GenerateLogMessage(Warning, obj, message), obj);
        }

        public static void LogError(this UnityEngine.Object obj, string message)
        {
            Debug.LogError(GenerateLogMessage(Error, obj, message), obj);
        }

        public static void Log(this System.Object obj, string message)
        {
            Debug.Log(GenerateLogMessage(Info, obj, message));
        }

        public static void LogWarning(this System.Object obj, string message)
        {
            Debug.LogWarning(GenerateLogMessage(Warning, obj, message));
        }

        public static void LogError(this System.Object obj, string message)
        {
            Debug.LogError(GenerateLogMessage(Error, obj, message));
        }

        public static void Log(string context, string message)
        {
            Debug.Log(GenerateLogMessage(Info, context, message));
        }

        public static void LogWarning(string context, string message)
        {
            Debug.LogWarning(GenerateLogMessage(Warning, context, message));
        }

        public static void LogError(string context, string message)
        {
            Debug.LogError(GenerateLogMessage(Error, context, message));
        }

        static string GenerateLogMessage(string type, System.Object obj, string message)
        {
            return GenerateLogMessage(type, "{0}][{1}".FormatInvarient(obj.GetType().FullName, obj.GetHashCode().ToString("X")), message);
        }

        static string GenerateLogMessage(string type, string context, string message)
        {
            return "[TLS-{0}][{1}][{2}][{2}]: {3}".FormatInvarient(type, context, Time.time.ToString("0.00"), message);
        }

        public static string FormatInvarient(this string formater, params object[] arguments)
        {
            return string.Format(CultureInfo.InvariantCulture, formater, arguments);
        }
    }
}
