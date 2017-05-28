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
using UnityEngine;
using KSP.Localization;
using System.Text;

namespace Tac
{
    public static class Utilities
    {
        #region strings cache

        private static string cacheautoLOC_6002321; //#autoLOC_6002321 = y
        private static string cacheautoLOC_6002320; //#autoLOC_6002320 = d 
        private static string cacheautoLOC_6002319; //#autoLOC_6002319 = h
        private static string cacheautoLOC_6002318; //#autoLOC_6002318 = m
        private static string cacheautoLOC_6002317; //#autoLOC_6002317 = s
        private static bool stringsCached = false;
        
        private static void CacheStrings()
        {
            cacheautoLOC_6002321 = Localizer.Format("#autoLOC_6002321");
            cacheautoLOC_6002320 = Localizer.Format("#autoLOC_6002320");
            cacheautoLOC_6002319 = Localizer.Format("#autoLOC_6002319");
            cacheautoLOC_6002318 = Localizer.Format("#autoLOC_6002318");
            cacheautoLOC_6002317 = Localizer.Format("#autoLOC_6002317");
        }

        #endregion

        public static Rect EnsureVisible(Rect pos, float min = 16.0f)
        {
            float xMin = min - pos.width;
            float xMax = Screen.width - min;
            float yMin = min - pos.height;
            float yMax = Screen.height - min;

            pos.x = Mathf.Clamp(pos.x, xMin, xMax);
            pos.y = Mathf.Clamp(pos.y, yMin, yMax);

            return pos;
        }

        public static Rect EnsureCompletelyVisible(Rect pos)
        {
            float xMin = 0;
            float xMax = Screen.width - pos.width;
            float yMin = 0;
            float yMax = Screen.height - pos.height;

            pos.x = Mathf.Clamp(pos.x, xMin, xMax);
            pos.y = Mathf.Clamp(pos.y, yMin, yMax);

            return pos;
        }

        public static bool GetValue(ConfigNode config, string name, bool currentValue)
        {
            bool newValue;
            if (config.HasValue(name) && bool.TryParse(config.GetValue(name), out newValue))
            {
                return newValue;
            }
            else
            {
                return currentValue;
            }
        }

        public static int GetValue(ConfigNode config, string name, int currentValue)
        {
            int newValue;
            if (config.HasValue(name) && int.TryParse(config.GetValue(name), out newValue))
            {
                return newValue;
            }
            else
            {
                return currentValue;
            }
        }

        public static float GetValue(ConfigNode config, string name, float currentValue)
        {
            float newValue;
            if (config.HasValue(name) && float.TryParse(config.GetValue(name), out newValue))
            {
                return newValue;
            }
            else
            {
                return currentValue;
            }
        }

        public static double GetValue(ConfigNode config, string name, double currentValue)
        {
            double newValue;
            if (config.HasValue(name) && double.TryParse(config.GetValue(name), out newValue))
            {
                return newValue;
            }
            else
            {
                return currentValue;
            }
        }

        public static string GetValue(ConfigNode config, string name, string currentValue)
        {
            if (config.HasValue(name))
            {
                return config.GetValue(name);
            }
            else
            {
                return currentValue;
            }
        }

        public static T GetValue<T>(ConfigNode config, string name, T currentValue) where T : IComparable, IFormattable, IConvertible
        {
            if (config.HasValue(name))
            {
                string stringValue = config.GetValue(name);
                if (Enum.IsDefined(typeof(T), stringValue))
                {
                    return (T)Enum.Parse(typeof(T), stringValue);
                }
            }

            return currentValue;
        }

        public static string FormatTime(double value, int numDecimals = 0)
        {
            const double SECONDS_PER_MINUTE = 60.0;
            const double MINUTES_PER_HOUR = 60.0;
            double HOURS_PER_DAY = (GameSettings.KERBIN_TIME) ? 6.0 : 24.0;
            double DAYS_PER_YEAR = (GameSettings.KERBIN_TIME) ? 426.0 : 365.0;

            if (!stringsCached)
            {
                CacheStrings();
                stringsCached = true;
            }
            string sign = "";
            if (value < 0.0)
            {
                sign = "-";
                value = -value;
            }

            double seconds = value;

            long minutes = (long)(seconds / SECONDS_PER_MINUTE);
            seconds -= (long)(minutes * SECONDS_PER_MINUTE);

            long hours = (long)(minutes / MINUTES_PER_HOUR);
            minutes -= (long)(hours * MINUTES_PER_HOUR);

            long days = (long)(hours / HOURS_PER_DAY);
            hours -= (long)(days * HOURS_PER_DAY);

            long years = (long)(days / DAYS_PER_YEAR);
            days -= (long)(years * DAYS_PER_YEAR);

            if (years > 0)
            {

                return sign + years.ToString("#0") + cacheautoLOC_6002321 + " "
                       + days.ToString("##0") + cacheautoLOC_6002320 + " "
                       + hours.ToString("00") + cacheautoLOC_6002319 + " "
                       + minutes.ToString("00") + cacheautoLOC_6002318;
            }
            if (days > 0)
            {
                return sign + days.ToString("#0") + cacheautoLOC_6002320 + " "
                       + hours.ToString("00") + cacheautoLOC_6002319 + " "
                       + minutes.ToString("00") + cacheautoLOC_6002318;
            }
            else if (hours > 0)
            {
                return sign + hours.ToString("#0") + cacheautoLOC_6002319 + " "
                       + minutes.ToString("00") + cacheautoLOC_6002318;
            }
            else
            {
                string secondsString;
                if (numDecimals > 0)
                {
                    // ToString always rounds and we want to truncate, so format with an
                    // extra decimal place and then lop it off
                    string format = "00." + new String('0', numDecimals + 1);
                    secondsString = seconds.ToString(format);
                    secondsString = secondsString.Substring(0, secondsString.Length - 1);
                }
                else
                {
                    secondsString = Math.Floor(seconds).ToString("00");
                }

                return sign + minutes.ToString("#0") + cacheautoLOC_6002318 + " "
                    + secondsString + cacheautoLOC_6002317;
            }
        }
        public static string GetDllVersion<T>(T t)
        {
            System.Reflection.Assembly assembly = t.GetType().Assembly;
            System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
            return fvi.FileVersion;
        }

        public static GUIStyle GetVersionStyle()
        {
            GUIStyle versionStyle = new GUIStyle(GUI.skin.label);
            versionStyle.alignment = TextAnchor.MiddleLeft;
            versionStyle.fontSize = 10;
            versionStyle.fontStyle = FontStyle.Normal;
            versionStyle.normal.textColor = Color.white;
            versionStyle.margin.top = 0;
            versionStyle.margin.bottom = 0;
            versionStyle.padding.top = 0;
            versionStyle.padding.bottom = 0;
            versionStyle.wordWrap = false;
            return versionStyle;
        }
        
    }
}
