using System;
using System.Collections.Generic;
using HomeApplianceRepair.Events;
using HomeApplianceRepair.Models;
using HomeApplianceRepair.Utilities;

namespace HomeApplianceRepair.Services
{
    /// <summary>
    /// Subscriber: reacts to the repair center events and "notifies the customer"
    /// by printing and storing a message. The repair center does not know this class.
    /// </summary>
    public class CustomerNotifier
    {
        private readonly List<string> _sentNotifications = new List<string>();

        public IReadOnlyList<string> SentNotifications => _sentNotifications.AsReadOnly();

        public void Subscribe(RepairCenter center)
        {
            if (center == null)
            {
                throw new ArgumentNullException(nameof(center));
            }

            center.ApplianceStatusChanged += OnStatusChanged;
            center.DeliveryOverdue += OnDeliveryOverdue;
        }

        public void Unsubscribe(RepairCenter center)
        {
            if (center == null)
            {
                throw new ArgumentNullException(nameof(center));
            }

            center.ApplianceStatusChanged -= OnStatusChanged;
            center.DeliveryOverdue -= OnDeliveryOverdue;
        }

        private void OnStatusChanged(object sender, ApplianceStatusChangedEventArgs e)
        {
            string who = e.Appliance.CustomerName + " - " + e.Appliance.Model + " (" + e.Appliance.SerialNumber + ")";
            string message;

            switch (e.NewStatus)
            {
                case RepairStatus.UnderMaintenance:
                    message = "Dear " + who + ": maintenance has started.";
                    break;
                case RepairStatus.Repaired:
                    message = "Dear " + who + ": your appliance has been repaired. Total cost: " +
                              DisplayFormat.Money(e.Appliance.CalculateRepairCost()) + ".";
                    break;
                case RepairStatus.Delivered:
                    message = "Dear " + who + ": your appliance has been delivered. Thank you.";
                    break;
                default:
                    return;
            }

            Send(message);
        }

        private void OnDeliveryOverdue(object sender, DeliveryOverdueEventArgs e)
        {
            Send("Reminder to " + e.Appliance.CustomerName + ": " + e.Appliance.Model + " (" +
                 e.Appliance.SerialNumber + ") has been ready for " + e.DaysSinceRepair +
                 " days. Please collect it.");
        }

        private void Send(string message)
        {
            _sentNotifications.Add(message);
            Console.WriteLine("  [Notification] " + message);
        }
    }
}
