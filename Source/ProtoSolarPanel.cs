using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tac
{
    class ProtoSolarPanel : IProtoElecComponent
    {
        private int resource;
        private float chargeRate;
        private ModuleDeployableSolarPanel.panelStates panelState;
        private FloatCurve powerCurve;
        private bool sunTracking;
        private bool enabled;

        public ProtoSolarPanel(ModuleDeployableSolarPanel sp)
        {
            resource = PartResourceLibrary.Instance.GetDefinition(sp.resourceName).id;
            chargeRate = sp.chargeRate;
            powerCurve=sp.powerCurve;
            sunTracking = sp.sunTracking;
        }

        public void OnLoad(ConfigNode configNode)
        {
            String panelStateString = Utilities.GetValue(configNode, "stateString", "RETRACTED");
            panelState = (ModuleDeployableSolarPanel.panelStates)
                Enum.Parse(typeof(ModuleDeployableSolarPanel.panelStates),
                panelStateString);
        }

        public void generate(IDictionary<int, ResourceLimits> resources, double deltaTime, Vessel vessel)
        {
            //!suntracking seems to be the best way to tell the OX-STAT apart
            if (panelState==ModuleDeployableSolarPanel.panelStates.EXTENDED || !sunTracking)
            {
                //Reduce by 50% if landed and 30% or something is orbiting
                CelestialBody sun = vessel.mainBody;
                while (sun.referenceBody != null && sun.referenceBody != sun)//the sun references itself
                {
                    sun = sun.referenceBody;
                }
                Vector3d vesPos = vessel.GetWorldPos3D();
                Vector3d sunPos = sun.position;
                Vector3d relPos = vesPos + sunPos;
                double distance = relPos.magnitude;
                double intensity = powerCurve.Evaluate((float)distance);
                resources[resource].add(intensity * chargeRate * deltaTime);
            }
        }
    }
}
