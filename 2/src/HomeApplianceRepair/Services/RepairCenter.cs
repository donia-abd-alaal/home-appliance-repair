using System;
using System.Collections.Generic;
using System.Linq;
using HomeApplianceRepair.Models;

namespace HomeApplianceRepair.Services
{
    /// <summary>
    /// Main class of the system. It stores every appliance in one list of the common
    /// base type and processes them polymorphically. It also relays appliance events
    /// so that subscribers only need to know the repair center.
    /// </summary>
    public class RepairCenter
    {
        private readonly List<Appliance> _appliances = new List<Appliance>();

        public IReadOnlyList<Appliance> Appliances => _appliances.AsReadOnly();
        public int Count => _appliances.Count;

        public void Add(Appliance appliance)
        {
            if (appliance == null)
            {
                throw new ArgumentNullException(nameof(appliance));
            }

            if (FindBySerial(appliance.SerialNumber) != null)
            {
                throw new InvalidOperationException(
                    "An appliance with serial number " + appliance.SerialNumber + " already exists.");
            }

            _appliances.Add(appliance);
        }

        public Appliance FindBySerial(string serialNumber)
        {
            return _appliances.FirstOrDefault(a => a.SerialNumber == serialNumber);
        }

        public List<Appliance> GetByStatus(RepairStatus status)
        {
            return _appliances.Where(a => a.Status == status).ToList();
        }

        /// <summary>Changes the status of the appliance with the given serial number.</summary>
        public void ChangeStatus(string serialNumber, RepairStatus newStatus)
        {
            Appliance appliance = FindBySerial(serialNumber);
            if (appliance == null)
            {
                throw new KeyNotFoundException("No appliance with serial number " + serialNumber + ".");
            }

            appliance.ChangeStatus(newStatus);
        }

        /// <summary>Polymorphic total: each appliance calculates its own cost.</summary>
        public decimal CalculateTotalRepairCost(RepairStatus? status = null)
        {
            return _appliances
                .Where(a => status == null || a.Status == status.Value)
                .Sum(a => a.CalculateRepairCost());
        }

        /// <summary>Polymorphic display: each appliance describes itself.</summary>
        public List<string> GetSummaries(RepairStatus? status = null)
        {
            return _appliances
                .Where(a => status == null || a.Status == status.Value)
                .Select(a => a.GetSummary())
                .ToList();
        }
    }
}
