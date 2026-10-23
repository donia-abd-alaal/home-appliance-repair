using HomeApplianceRepair.Models;

namespace HomeApplianceRepair.Interfaces
{
    /// <summary>
    /// Core operations shared by every appliance handled by the repair center.
    /// </summary>
    public interface IAppliance
    {
        string SerialNumber { get; }
        string Model { get; }
        string CustomerName { get; }
        RepairStatus Status { get; }

        /// <summary>Calculates the repair cost according to the appliance type.</summary>
        decimal CalculateRepairCost();

        /// <summary>Tells whether the appliance may move from its current status to <paramref name="newStatus"/>.</summary>
        bool CanChangeStatusTo(RepairStatus newStatus);

        /// <summary>Moves the appliance to <paramref name="newStatus"/> if the transition is allowed.</summary>
        void ChangeStatus(RepairStatus newStatus);

        /// <summary>Returns a one-line description of the appliance.</summary>
        string GetSummary();
    }
}
