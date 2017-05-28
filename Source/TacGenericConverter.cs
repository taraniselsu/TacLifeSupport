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

using System;
using System.Text;
using KSP.Localization;

namespace Tac
{
    
    [KSPModule("TAC Converter")]
    public class TacGenericConverter : ModuleResourceConverter
    {
        [KSPField] public string converterName = "TAC Generic Converter";

        [KSPField(isPersistant = true)] public bool converterEnabled = false;

        [KSPField] public bool alwaysActive = false;

        [KSPField] public bool requiresOxygenAtmo = false;

        [KSPField] public float conversionRate = 1f;

        #region Localization Tag cache

        private static string cacheautoLOC_TACLS_00234;
        private static string cacheautoLOC_TACLS_00235;
        private static string cacheautoLOC_TACLS_00236;
        
        private static void cacheLocalStrings()
        {
            cacheautoLOC_TACLS_00234 = Localizer.Format("#autoLOC_TACLS_00234"); //#autoLOC_TACLS_00234 = Atmo lacks oxygen.
            cacheautoLOC_TACLS_00235 = Localizer.Format("#autoLOC_TACLS_00235"); //#autoLOC_TACLS_00235 = \nRequires an atmosphere containing Oxygen.
            cacheautoLOC_TACLS_00236 = Localizer.Format("#autoLOC_TACLS_00236"); //#autoLOC_TACLS_00236 = \nCannot be turned off.
        }

        #endregion

        public override void OnAwake()
        {
            this.Log("OnAwake");
            base.OnAwake();
            cacheLocalStrings();
        }

        public override void OnStart(PartModule.StartState state)
        {
            this.Log("OnStart: " + state);
            base.OnStart(state);
        }

        protected override void PreProcessing()
        {
            if (HighLogic.LoadedScene == GameScenes.FLIGHT)
            {
                if (requiresOxygenAtmo && !vessel.mainBody.atmosphereContainsOxygen)
                {

                    IsActivated = false;
                    converterEnabled = false;
                    status = cacheautoLOC_TACLS_00234;
                }
            }
        }

        protected override void PostProcess(ConverterResults result, double deltaTime)
        {
            var diff = Math.Abs(deltaTime - result.TimeFactor);
            converterEnabled = diff < 0.00001f;
        }

        protected override ConversionRecipe LoadRecipe()
        {
            var r = new ConversionRecipe();
            try
            {
                //conversionRate must be > 0, otherwise set to default = 1.
                if (conversionRate < 0)
                    conversionRate = 1f;
                //if conversionRate is not equal to 1 multiply all Inputs, Outputs and Requirements resource Ratios
                // by the value of conversionRate.
                if (conversionRate != 1f)
                {
                    for (int i = 0; i < inputList.Count; i++)
                    {
                        double tmpRate = inputList[i].Ratio * conversionRate;
                        ResourceRatio tmpRat = new ResourceRatio(inputList[i].ResourceName, tmpRate,
                            inputList[i].DumpExcess) {FlowMode = inputList[i].FlowMode};
                        r.Inputs.Add(tmpRat);
                    }
                    for (int i = 0; i < outputList.Count; i++)
                    {
                        double tmpRate = outputList[i].Ratio * conversionRate;
                        ResourceRatio tmpRat = new ResourceRatio(outputList[i].ResourceName, tmpRate,
                            outputList[i].DumpExcess)
                        { FlowMode = outputList[i].FlowMode };
                        r.Outputs.Add(tmpRat);
                    }
                    for (int i = 0; i < reqList.Count; i++)
                    {
                        double tmpRate = reqList[i].Ratio * conversionRate;
                        ResourceRatio tmpRat = new ResourceRatio(reqList[i].ResourceName, tmpRate,
                            reqList[i].DumpExcess)
                        { FlowMode = reqList[i].FlowMode };
                        r.Requirements.Add(tmpRat);
                    }
                }
                // else, conversion rate is 1. We just use the values from the cfg file.
                else
                {

                    r.Inputs.AddRange(inputList);
                    r.Outputs.AddRange(outputList);
                    r.Requirements.AddRange(reqList);
                }

                // if CovertByMass then convert Recipe to Units.
                if (ConvertByMass)
                    ConvertRecipeToUnits(r);
            }
            catch (Exception)
            {
                this.LogError("[TACGenericConverter] Error creating recipe");
            }
            return r;
        }

        public override string GetInfo()
        {
            if (string.IsNullOrEmpty(cacheautoLOC_TACLS_00235))
            {
                cacheLocalStrings();
            }
            StringBuilder sb = new StringBuilder();
            sb.Append(base.GetInfo());
            sb.Append("\n");
            if (requiresOxygenAtmo)
            {
                sb.Append(cacheautoLOC_TACLS_00235);
            }
            if (alwaysActive)
            {
                sb.Append(cacheautoLOC_TACLS_00236);
            }

            return sb.ToString();
        }
    }
}
