namespace HomeApplianceRepair.Events
{
    /// <summary>Delegate used to notify that an appliance changed its repair status.</summary>
    public delegate void ApplianceStatusChangedHandler(object sender, ApplianceStatusChangedEventArgs e);

    /// <summary>Delegate used to notify that a repaired appliance has not been delivered in time.</summary>
    public delegate void DeliveryOverdueHandler(object sender, DeliveryOverdueEventArgs e);
}
