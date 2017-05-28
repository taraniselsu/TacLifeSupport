using KSP.UI.Screens;
using PreFlightTests;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KSP.Localization;

namespace Tac
{
    public class CheckSupplies : DesignConcernBase
    {
        #region Cache Strings

        private static string cacheautoLOC_TACLS_00255;
        private static string cacheautoLOC_TACLS_00256;

        private static void CacheLocalStrings()
        {
            cacheautoLOC_TACLS_00255 = Localizer.Format("#autoLOC_TACLS_00255");
            cacheautoLOC_TACLS_00256 = Localizer.Format("#autoLOC_TACLS_00256");
        }

        #endregion

        public CheckSupplies()
        {
            this.Log("constructor");
            CacheLocalStrings();
        }

        public override string GetConcernDescription()
        {
            return cacheautoLOC_TACLS_00256;
        }

        public override string GetConcernTitle()
        {
            return cacheautoLOC_TACLS_00255;
        }

        public override bool TestCondition()
        {
            var parts = EditorLogic.fetch.ship.parts;
            return !parts.Any(p => p.CrewCapacity > 0);
        }

        public override DesignConcernSeverity GetSeverity()
        {
            return DesignConcernSeverity.NOTICE;
        }

        public override List<Part> GetAffectedParts()
        {
            var parts = EditorLogic.fetch.ship.parts;
            return parts.Where(p => p.CrewCapacity > 0).ToList();
        }
    }

    [KSPAddon(KSPAddon.Startup.EditorAny, false)]
    class EngineersReportLifeSupportTests : MonoBehaviour
    {
        void Awake()
        {
            this.Log("Awake");
            GameEvents.onGUIEngineersReportReady.Add(OnGUIEngineersReportReady);
            GameEvents.onGUIEngineersReportDestroy.Add(OnGUIEngineersReportDestroy);
        }

        void Start()
        {
            this.Log("Start");
        }

        void OnDestroy()
        {
            this.Log("OnDestroy");
            GameEvents.onGUIEngineersReportReady.Remove(OnGUIEngineersReportReady);
            GameEvents.onGUIEngineersReportDestroy.Remove(OnGUIEngineersReportDestroy);
        }

        void OnGUIEngineersReportReady()
        {
            this.Log("OnGUIEngineersReportReady");
            EngineersReport.Instance.AddTest(new CheckSupplies());
        }

        void OnGUIEngineersReportDestroy()
        {
        }
    }
}
