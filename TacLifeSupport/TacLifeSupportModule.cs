/**
 * TacLifeSupportModule.cs
 * 
 * Thunder Aerospace Corporation's Atomic Clock for the Kerbal Space Program, by Taranis Elsu
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

public class TacLifeSupportModule : PartModule
{/*
    private TacLifeSupportMain main;

    public override void OnAwake()
    {
        base.OnAwake();
        main = TacLifeSupportMain.GetInstance();

        NotificationManager.OnVesselChange_Add(OnVesselChange);
        NotificationManager.OnVesselDock_Add(OnVesselDock);
    }

    //public override void OnStart(PartModule.StartState state)
    //{
    //    base.OnStart(state);
    //    if (state != StartState.Editor)
    //    {
    //        part.OnJustAboutToBeDestroyed += CleanUp;
    //        vessel.OnJustAboutToBeDestroyed += CleanUp;
    //    }
    //}

    public override void OnLoad(ConfigNode node)
    {
        base.OnLoad(node);
        main.Load(node);
    }

    public override void OnSave(ConfigNode node)
    {
        base.OnSave(node);
        main.Save(node);
    }

    public void OnVesselChange(Vessel v)
    {
        string message = "TAC LifeSupport -- Vessel changed! ";
        if (v != null)
        {
            message += v.vesselName;
        }
        Debug.Log(message);
    }

    public void OnVesselDock(Vessel v)
    {
        string message = "TAC LifeSupport -- Vessel docked! ";
        if (v != null)
        {
            message += v.vesselName;
        }
        Debug.Log(message);
    }

    //private void CleanUp()
    //{
    //}*/
}
