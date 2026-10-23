using System;
using HomeApplianceRepair.Interfaces;

namespace HomeApplianceRepair.Models
{
    /// <summary>
    /// Base class of every appliance. It holds the shared data, enforces the status
    /// life cycle and forces each subclass to define its own repair cost.
    /// </summary>
    public abstract class Appliance : IAppliance
    {
        private readonly string _serialNumber;
        private readonly DateTime _receivedDate;
        private string _model;
        private string _customerName;
        private decimal _partsCost;
        private double _laborHours;
        private RepairStatus _status;

        protected Appliance(string serialNumber, string model, string customerName, DateTime receivedDate)
        {
            _serialNumber = serialNumber;
            _receivedDate = receivedDate.Date;
            _model = model;
            _customerName = customerName;
            _status = RepairStatus.Received;
        }

        // ----- Shared data -----

        public string SerialNumber => _serialNumber;
        public DateTime ReceivedDate => _receivedDate;
        public RepairStatus Status => _status;

        public string Model
        {
            get => _model;
            set => _model = value;
        }

        public string CustomerName
        {
            get => _customerName;
            set => _customerName = value;
        }

        /// <summary>Cost of the spare parts used.</summary>
        public decimal PartsCost
        {
            get => _partsCost;
            set => _partsCost = value;
        }

        /// <summary>Technician hours spent on the repair.</summary>
        public double LaborHours
        {
            get => _laborHours;
            set => _laborHours = value;
        }

        // ----- Polymorphic behavior -----

        /// <summary>Each appliance type defines its own repair cost.</summary>
        public abstract decimal CalculateRepairCost();

        /// <summary>Describes the appliance; subclasses extend it with their own details.</summary>
        public virtual string GetSummary()
        {
            return "[" + GetType().Name + "] " + _serialNumber +
                   " | " + _model +
                   " | Customer: " + _customerName +
                   " | Received: " + _receivedDate.ToString("yyyy-MM-dd") +
                   " | Status: " + _status +
                   " | Cost: " + CalculateRepairCost().ToString("F2");
        }

        // ----- Status life cycle -----

        public bool CanChangeStatusTo(RepairStatus newStatus)
        {
            return IsAllowedTransition(_status, newStatus);
        }

        public void ChangeStatus(RepairStatus newStatus)
        {
            if (!CanChangeStatusTo(newStatus))
            {
                throw new InvalidOperationException(
                    _serialNumber + ": cannot change status from " + _status + " to " + newStatus + ".");
            }

            _status = newStatus;
        }

        // ----- Helpers for subclasses -----

        /// <summary>Parts cost plus labor at the given hourly rate (shared part of every cost formula).</summary>
        protected decimal CalculateLaborAndParts(decimal hourlyRate)
        {
            return _partsCost + hourlyRate * (decimal)_laborHours;
        }

        private static bool IsAllowedTransition(RepairStatus current, RepairStatus next)
        {
            switch (current)
            {
                case RepairStatus.Received:
                    return next == RepairStatus.UnderMaintenance;
                case RepairStatus.UnderMaintenance:
                    return next == RepairStatus.Repaired;
                case RepairStatus.Repaired:
                    return next == RepairStatus.Delivered;
                default:
                    return false;
            }
        }
    }
}
