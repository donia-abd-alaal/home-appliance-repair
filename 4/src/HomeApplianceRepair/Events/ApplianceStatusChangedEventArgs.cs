using System;
using HomeApplianceRepair.Interfaces;
using HomeApplianceRepair.Models;

namespace HomeApplianceRepair.Events
{
    /// <summary>Data sent to subscribers when an appliance changes status.</summary>
    public class ApplianceStatusChangedEventArgs : EventArgs
    {
        public ApplianceStatusChangedEventArgs(IAppliance appliance, RepairStatus oldStatus, RepairStatus newStatus)
        {
            Appliance = appliance ?? throw new ArgumentNullException(nameof(appliance));
            OldStatus = oldStatus;
            NewStatus = newStatus;
        }

        public IAppliance Appliance { get; }
        public RepairStatus OldStatus { get; }
        public RepairStatus NewStatus { get; }
    }
}
