using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tac
{
    class ProtoModuleGenerator : IProtoElecComponent
    {
        private float efficiency;
        private bool isActive;
        private List<ModuleGenerator.GeneratorResource> inputList;
        private bool isAlwaysActive;
        private List<ModuleGenerator.GeneratorResource> outputList;

        public ProtoModuleGenerator(ModuleGenerator gen)
        {
            efficiency = gen.efficiency;
            isActive = gen.generatorIsActive;
            inputList = gen.inputList;
            isAlwaysActive = gen.isAlwaysActive;
            outputList = gen.outputList;
            //TODO a couple of other fields
        }

        public void OnLoad(ConfigNode configNode)
        {
            //TODO load some stuff
        }
        
        public void generate(IDictionary<int, ResourceLimits> resources, double deltaTime, Vessel vessel)
        {
            if (isActive || isAlwaysActive)
            {
                //TODO this module can be more sophisticated but this works for RTGs
                foreach (var output in outputList) {
                    resources[output.id].add(deltaTime * output.rate);
                }
            }
        }
    }

    
}
