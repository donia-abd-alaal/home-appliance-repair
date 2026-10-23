using System;
using HomeApplianceRepair.Interfaces;

namespace HomeApplianceRepair.Events
{
    /// <summary>Data sent to subscribers when a repaired appliance is not collected in time.</summary>
    public class DeliveryOverdueEventArgs : EventArgs
    {
        public DeliveryOverdueEventArgs(IAppliance appliance, int daysSinceRepair)
        {
            Appliance = appliance ?? throw new ArgumentNullException(nameof(appliance));
            DaysSinceRepair = daysSinceRepair;
        }

        public IAppliance Appliance { get; }
        public int DaysSinceRepair { get; }
    }
}
