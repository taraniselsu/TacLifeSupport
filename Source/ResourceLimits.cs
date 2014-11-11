using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tac
{
    public class ResourceLimits
    {
        public double available;
        public double maximum;

        public ResourceLimits(double available, double maximum)
        {
            this.available = available;
            this.maximum = maximum;
        }

        public ResourceLimits clone()
        {
            return new ResourceLimits(available, maximum);
        }

        internal void add(double amount)
        {
            available = Math.Min(available+amount, maximum);
        }

        public double getSpace()
        {
            return maximum - available;
        }

        public override string ToString()
        {
            return "avail:" + available + " max:" + maximum;
        }
    }
}
