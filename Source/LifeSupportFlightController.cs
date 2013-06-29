using KSP.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Tac
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    class LifeSupportFlightController : MonoBehaviour
    {
        private const int SECONDS_PER_DAY = 24 * 60 * 60;

        private Settings settings;
        private LifeSupportMonitoringWindow monitoringWindow;
        private Icon<LifeSupportFlightController> icon;
        private string configFilename;

        public double LastUpdateTime { get; private set; }

        public double RemainingFood { get; private set; }
        public double RemainingWater { get; private set; }
        public double RemainingOxygen { get; private set; }

        public bool FoodCritical { get; private set; }
        public bool WaterCritical { get; private set; }
        public bool OxygenCritical { get; private set; }
        
        public Vessel currentVessel { get; private set; }

        private bool alerted;

        void Awake()
        {
            Debug.Log("TAC Life Support (LifeSupportFlightController) [" + this.GetInstanceID().ToString("X") + "][" + Time.time + "]: Awake");
        }

        void Start()
        {
            Debug.Log("TAC Life Support (LifeSupportFlightController) [" + this.GetInstanceID().ToString("X") + "][" + Time.time + "]: Start");

            settings = LifeSupportController.Instance.settings;
            monitoringWindow = new LifeSupportMonitoringWindow(this, settings);

            icon = new Icon<LifeSupportFlightController>(new Rect(Screen.width * 0.75f, 0, 32, 32), "icon.png",
                "Click to show the Life Support Monitoring Window", OnIconClicked);

            Load();
            icon.SetVisible(true);

            Reset();
        }

        void OnDestroy()
        {
            Debug.Log("TAC Life Support (LifeSupportFlightController) [" + this.GetInstanceID().ToString("X") + "][" + Time.time + "]: OnDestroy");
            icon.SetVisible(false);
            Save();
        }

        private void Load()
        {
            if (File.Exists<LifeSupportFlightController>(configFilename))
            {
                ConfigNode config = ConfigNode.Load(configFilename);
                icon.Load(config);
                monitoringWindow.Load(config);
            }
        }

        private void Save()
        {
            ConfigNode config = new ConfigNode();
            icon.Save(config);
            monitoringWindow.Save(config);
            settings.Save(config);

            config.Save(configFilename);
        }

        private void OnIconClicked()
        {
            monitoringWindow.ToggleVisible();
        }

        void FixedUpdate()
        {
            if (!FlightGlobals.ready || FlightGlobals.ActiveVessel == null)
            {
                Debug.Log("TAC Life Support (LifeSupportFlightController) [" + this.GetInstanceID().ToString("X") + "][" + Time.time +
                    "]: Flight Globals are not ready or no active vessel yet.");
                return;
            }

            Vessel vessel = FlightGlobals.fetch.activeVessel;

            if (vessel.GetCrewCapacity() < 1)
            {
                icon.SetVisible(false);
                monitoringWindow.SetVisible(false);
                return;
            }
            else if (!icon.IsVisible())
            {
                icon.SetVisible(true);
            }

            if (vessel != currentVessel)
            {
                Reset();
                currentVessel = vessel;
            }

            CheckResourceLevels(vessel);

            if (vessel.missionTime < 1)
            {
                // Wait until after launch
            }
            else if (LastUpdateTime == -1)
            {
                LastUpdateTime = Planetarium.GetUniversalTime();
                // Wait until the second update
            }
        }

        private void Reset()
        {
            LastUpdateTime = -1;

            FoodCritical = false;
            WaterCritical = false;
            OxygenCritical = false;

            alerted = false;
        }

        private void CheckResourceLevels(Vessel vessel)
        {
            RemainingFood = 0.0;
            RemainingWater = 0.0;
            RemainingOxygen = 0.0;
            double maxFood = 0.0;
            double maxWater = 0.0;
            double maxOxygen = 0.0;

            foreach (Part part in vessel.parts)
            {
                foreach (PartResource resource in part.Resources)
                {
                    if (resource.info.id == settings.FoodId)
                    {
                        RemainingFood += resource.amount;
                        maxFood += resource.maxAmount;
                    }
                    else if (resource.info.id == settings.WaterId)
                    {
                        RemainingWater += resource.amount;
                        maxWater += resource.maxAmount;
                    }
                    else if (resource.info.id == settings.OxygenId)
                    {
                        RemainingOxygen += resource.amount;
                        maxOxygen += resource.maxAmount;
                    }
                }
            }

            FoodCritical = (RemainingFood < (maxFood * 0.10));
            WaterCritical = (RemainingWater < (maxWater * 0.10));
            OxygenCritical = (RemainingOxygen < (maxOxygen * 0.10));

            if (FoodCritical || WaterCritical || OxygenCritical)
            {
                if (!alerted)
                {
                    TimeWarp.SetRate(0, true);
                    ScreenMessages.PostScreenMessage(this.currentVessel + " - LIFE SUPPORT CRITICAL!", 10.0f, ScreenMessageStyle.UPPER_CENTER);
                    Debug.Log("TAC Life Support (LifeSupportFlightController) [" + this.GetInstanceID().ToString("X") + "][" + Time.time + "]: "
                        + this.currentVessel + " - LIFE SUPPORT CRITICAL!");
                    alerted = true;
                }
            }
            else
            {
                alerted = false;
            }
        }
    }
}
