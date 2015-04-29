using PreFlightTests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Tac
{
    public class CheckSupplies : DesignConcernBase
    {
        public CheckSupplies()
        {
            this.Log("constructor");
        }

        public override string GetConcernDescription()
        {
            return "Did you remember to check your life support supplies to ensure that you have enough?";
        }

        public override string GetConcernTitle()
        {
            return "Check Life Support.";
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
