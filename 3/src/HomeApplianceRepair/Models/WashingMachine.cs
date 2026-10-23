using System;
using System.Globalization;

namespace HomeApplianceRepair.Models
{
    /// <summary>Washing machine: cost depends on drum replacement and on the loading type.</summary>
    public class WashingMachine : Appliance
    {
        private const decimal InspectionFee = 15m;
        private const decimal LaborRatePerHour = 12m;
        private const decimal DrumReplacementFee = 90m;
        private const decimal FrontLoaderSurcharge = 25m;

        private double _loadCapacityKg;
        private bool _isFrontLoader;
        private bool _needsDrumReplacement;

        public WashingMachine(string serialNumber, string model, string customerName, DateTime receivedDate,
                              double loadCapacityKg, bool isFrontLoader = false, bool needsDrumReplacement = false)
            : base(serialNumber, model, customerName, receivedDate)
        {
            _loadCapacityKg = RequireCapacity(loadCapacityKg);
            _isFrontLoader = isFrontLoader;
            _needsDrumReplacement = needsDrumReplacement;
        }

        public double LoadCapacityKg
        {
            get => _loadCapacityKg;
            set
            {
                EnsureCostInputsEditable();
                _loadCapacityKg = RequireCapacity(value);
            }
        }

        public bool IsFrontLoader
        {
            get => _isFrontLoader;
            set
            {
                EnsureCostInputsEditable();
                _isFrontLoader = value;
            }
        }

        public bool NeedsDrumReplacement
        {
            get => _needsDrumReplacement;
            set
            {
                EnsureCostInputsEditable();
                _needsDrumReplacement = value;
            }
        }

        public override decimal CalculateRepairCost()
        {
            decimal cost = InspectionFee + CalculateLaborAndParts(LaborRatePerHour);

            if (_needsDrumReplacement)
            {
                cost += DrumReplacementFee;
            }

            if (_isFrontLoader)
            {
                cost += FrontLoaderSurcharge;
            }

            return cost;
        }

        public override string GetSummary()
        {
            return base.GetSummary() +
                   " | Load: " + _loadCapacityKg.ToString("0.0", CultureInfo.InvariantCulture) + " kg" +
                   " | Front loader: " + (_isFrontLoader ? "Yes" : "No") +
                   " | Drum replacement: " + (_needsDrumReplacement ? "Yes" : "No");
        }

        private static double RequireCapacity(double kg)
        {
            return RequireInRange(kg, 3, 20, nameof(LoadCapacityKg), "Washing machine load capacity (kg)");
        }
    }
}
