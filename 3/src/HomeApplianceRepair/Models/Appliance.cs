using System;
using System.Text.RegularExpressions;
using HomeApplianceRepair.Interfaces;

namespace HomeApplianceRepair.Models
{
    /// <summary>
    /// Base class of every appliance. It holds the shared data, enforces the status
    /// life cycle and forces each subclass to define its own repair cost.
    /// </summary>
    public abstract class Appliance : IAppliance
    {
        private const decimal MaxCost = 1000000m;

        // Example of a valid serial number: RF-100234 (2-3 capital letters, dash, 4-8 digits).
        private static readonly Regex SerialPattern =
            new Regex(@"^[A-Z]{2,3}-[0-9]{4,8}\z", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private readonly string _serialNumber;
        private readonly DateTime _receivedDate;
        private string _model;
        private string _customerName;
        private decimal _partsCost;
        private double _laborHours;
        private RepairStatus _status;

        protected Appliance(string serialNumber, string model, string customerName, DateTime receivedDate)
        {
            if (string.IsNullOrWhiteSpace(serialNumber) || !SerialPattern.IsMatch(serialNumber))
            {
                throw new ArgumentException(
                    "Serial number must look like 'RF-100234' (2-3 capital letters, a dash, 4-8 digits).",
                    nameof(serialNumber));
            }

            if (receivedDate.Date < new DateTime(2000, 1, 1) || receivedDate.Date > DateTime.Today)
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
                if (value < 0m || value > MaxCost)
                {
                    throw new ArgumentOutOfRangeException(nameof(PartsCost), value,
                        "Parts cost must be between 0 and " + MaxCost + ".");
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

        /// <summary>Subclasses call this in setters of properties that affect the repair cost.</summary>
        protected void EnsureCostInputsEditable()
        {
            if (_status == RepairStatus.Repaired || _status == RepairStatus.Delivered)
            {
                throw new InvalidOperationException(
                    "Repair cost inputs of " + _serialNumber + " are locked after the repair is completed.");
            }
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
            if (value < min || value > max)
            {
                throw new ArgumentOutOfRangeException(paramName, value,
                    description + " must be between " + min + " and " + max + ".");
            }

            return value;
        }

        private static string RequireText(string value, string paramName)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Trim().Length > 60)
            {
                throw new ArgumentException(paramName + " must be non-empty text (max 60 characters).", paramName);
            }

            return value.Trim();
        }
    }
}
