/**
 * VesselInfo.cs
 * 
 * Thunder Aerospace Corporation's Life Support for the Kerbal Space Program, by Taranis Elsu
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

namespace Tac
{
    public class VesselInfo
    {
        public Status foodStatus = Status.GOOD;
        public Status waterStatus = Status.GOOD;
        public Status oxygenStatus = Status.GOOD;
        public Status electricityStatus = Status.GOOD;

        public int numCrew;

        public double remainingFood;
        public double remainingWater;
        public double remainingOxygen;
        public double remainingElectricity;
        public double remainingCO2;
        public double remainingWaste;
        public double remainingWasteWater;

        public double maxFood;
        public double maxWater;
        public double maxOxygen;
        public double maxElectricity;

        public double lastElectricity;
        public double lastUpdate;

        public VesselInfo(double currentTime)
        {
            lastElectricity = currentTime;
            lastUpdate = currentTime;
        }

        public void ClearAmounts()
        {
            numCrew = 0;
            remainingFood = 0.0;
            remainingWater = 0.0;
            remainingOxygen = 0.0;
            remainingElectricity = 0.0;
            remainingCO2 = 0.0;
            remainingWaste = 0.0;
            remainingWasteWater = 0.0;
            maxFood = 0.0;
            maxWater = 0.0;
            maxOxygen = 0.0;
            maxElectricity = 0.0;
        }

        public enum Status
        {
            GOOD,
            LOW,
            CRITICAL
        }
    }
}
