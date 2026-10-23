namespace HomeApplianceRepair.Models
{
    /// <summary>
    /// Life cycle of an appliance inside the repair center.
    /// Allowed path: Received -> UnderMaintenance -> Repaired -> Delivered.
    /// </summary>
    public enum RepairStatus
    {
        Received = 0,
        UnderMaintenance = 1,
        Repaired = 2,
        Delivered = 3
    }
}
