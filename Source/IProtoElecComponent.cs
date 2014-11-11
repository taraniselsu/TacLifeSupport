using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tac
{
    public interface IProtoElecComponent
    {
        void generate(IDictionary<int, ResourceLimits> resources, double deltaTime, Vessel vessel);

        void OnLoad(ConfigNode configNode);
    }
}
