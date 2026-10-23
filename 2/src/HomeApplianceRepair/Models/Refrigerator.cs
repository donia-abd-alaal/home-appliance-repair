using System;

namespace HomeApplianceRepair.Models
{
    /// <summary>Refrigerator: cost depends on a compressor fault and on the unit size.</summary>
    public class Refrigerator : Appliance
    {
        private const decimal InspectionFee = 20m;
        private const decimal LaborRatePerHour = 15m;
        private const decimal CompressorReplacementFee = 120m;
        private const decimal LargeCapacitySurcharge = 30m;
        private const int LargeCapacityLiters = 500;

        private int _capacityLiters;
        private bool _hasCompressorFault;

        public Refrigerator(string serialNumber, string model, string customerName, DateTime receivedDate,
                            int capacityLiters, bool hasCompressorFault = false)
            : base(serialNumber, model, customerName, receivedDate)
        {
            _capacityLiters = capacityLiters;
            _hasCompressorFault = hasCompressorFault;
        }

        public int CapacityLiters
        {
            get => _capacityLiters;
            set
            {
                _capacityLiters = value;
            }
        }

        public bool HasCompressorFault
        {
            get => _hasCompressorFault;
            set
            {
                _hasCompressorFault = value;
            }
        }

        public override decimal CalculateRepairCost()
        {
            decimal cost = InspectionFee + CalculateLaborAndParts(LaborRatePerHour);

            if (_hasCompressorFault)
            {
                cost += CompressorReplacementFee;
            }

            if (_capacityLiters >= LargeCapacityLiters)
            {
                cost += LargeCapacitySurcharge;
            }

            return cost;
        }

        public override string GetSummary()
        {
            return base.GetSummary() +
                   " | Capacity: " + _capacityLiters + " L" +
                   " | Compressor fault: " + (_hasCompressorFault ? "Yes" : "No");
        }
    }
}
