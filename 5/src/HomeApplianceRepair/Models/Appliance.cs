using System;
using HomeApplianceRepair.Events;
using HomeApplianceRepair.Interfaces;
using HomeApplianceRepair.Utilities;

namespace HomeApplianceRepair.Models
{
    /// <summary>
    /// Base class of every appliance. It holds the shared data, enforces the status
    /// life cycle, raises <see cref="StatusChanged"/> and forces each subclass to
    /// define its own repair cost.
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
        private DateTime? _repairedDate;
        private decimal _recordedRepairCost;

        /// <summary>Raised once for every real status transition.</summary>
        public event ApplianceStatusChangedHandler StatusChanged;

        /// <summary>Number of appliances currently under maintenance (all instances).</summary>
        public static int UnderMaintenanceCount { get; private set; }

        /// <summary>Sum of the repair costs recorded when appliances reached the Repaired status.</summary>
        public static decimal TotalRecordedRepairCost { get; private set; }

        protected Appliance(string serialNumber, string model, string customerName, DateTime receivedDate)
        {
            if (!ApplianceValidator.IsValidSerialNumber(serialNumber))
            {
                throw new ArgumentException(
                    "Serial number must look like 'RF-100234' (2-3 capital letters, a dash, 4-8 digits).",
                    nameof(serialNumber));
            }

            if (!ApplianceValidator.IsValidReceivedDate(receivedDate))
            {
                throw new ArgumentException(
                    "Received date must be between 2000-01-01 and today.",
                    nameof(receivedDate));
            }

            _serialNumber = serialNumber;
            _receivedDate = receivedDate.Date;
            _model = RequireText(model, nameof(model));
            _customerName = RequireText(customerName, nameof(customerName));
            _status = RepairStatus.Received;
        }

        // ----- Read-only data (cannot change after creation) -----

        public string SerialNumber => _serialNumber;
        public DateTime ReceivedDate => _receivedDate;

        // ----- Read-only data maintained by the status life cycle -----

        public RepairStatus Status => _status;
        public DateTime? RepairedDate => _repairedDate;
        public decimal RecordedRepairCost => _recordedRepairCost;

        // ----- Validated, editable data -----

        public string Model
        {
            get => _model;
            set => _model = RequireText(value, nameof(Model));
        }

        public string CustomerName
        {
            get => _customerName;
            set => _customerName = RequireText(value, nameof(CustomerName));
        }

        /// <summary>Cost of the spare parts used. Locked once the repair is completed.</summary>
        public decimal PartsCost
        {
            get => _partsCost;
            set
            {
                EnsureCostInputsEditable();
                if (!ApplianceValidator.IsValidCost(value))
                {
                    throw new ArgumentOutOfRangeException(nameof(PartsCost), value,
                        "Parts cost must be between 0 and " + ApplianceValidator.MaxCost + ".");
                }
                _partsCost = value;
            }
        }

        /// <summary>Technician hours spent on the repair. Locked once the repair is completed.</summary>
        public double LaborHours
        {
            get => _laborHours;
            set
            {
                EnsureCostInputsEditable();
                _laborHours = RequireInRange(value, 0, 100, nameof(LaborHours), "Labor hours");
            }
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
                   " | Received: " + DisplayFormat.Date(_receivedDate) +
                   " | Status: " + _status +
                   " | Cost: " + DisplayFormat.Money(CalculateRepairCost());
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

            RepairStatus oldStatus = _status;
            _status = newStatus;

            // Keep the static statistics in sync with real transitions.
            if (newStatus == RepairStatus.UnderMaintenance)
            {
                UnderMaintenanceCount++;
            }

            if (oldStatus == RepairStatus.UnderMaintenance)
            {
                UnderMaintenanceCount--;
            }

            if (newStatus == RepairStatus.Repaired)
            {
                _repairedDate = DateTime.Now;
                _recordedRepairCost = CalculateRepairCost();
                TotalRecordedRepairCost += _recordedRepairCost;
            }

            OnStatusChanged(oldStatus, newStatus);
        }

        // ----- Helpers for subclasses -----

        /// <summary>Parts cost plus labor at the given hourly rate (shared part of every cost formula).</summary>
        protected decimal CalculateLaborAndParts(decimal hourlyRate)
        {
            return _partsCost + hourlyRate * (decimal)_laborHours;
        }

        /// <summary>Subclasses call this in setters of properties that affect the repair cost.</summary>
        protected void EnsureCostInputsEditable()
        {
            if (_status == RepairStatus.Repaired || _status == RepairStatus.Delivered)
            {
                throw new InvalidOperationException(
                    "Repair cost inputs of " + _serialNumber + " are locked after the repair is completed.");
            }
        }

        protected virtual void OnStatusChanged(RepairStatus oldStatus, RepairStatus newStatus)
        {
            StatusChanged?.Invoke(this, new ApplianceStatusChangedEventArgs(this, oldStatus, newStatus));
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

        /// <summary>
        /// Shared range check used by the subclasses for their capacity properties.
        /// Returns the value when it is inside [min, max]; otherwise throws.
        /// </summary>
        protected static double RequireInRange(double value, double min, double max, string paramName, string description)
        {
            if (!ApplianceValidator.IsInRange(value, min, max))
            {
                throw new ArgumentOutOfRangeException(paramName, value,
                    description + " must be between " + min + " and " + max + ".");
            }

            return value;
        }

        private static string RequireText(string value, string paramName)
        {
            if (!ApplianceValidator.IsNonEmptyText(value))
            {
                throw new ArgumentException(paramName + " must be non-empty text (max 60 characters).", paramName);
            }

            return value.Trim();
        }
    }
}
