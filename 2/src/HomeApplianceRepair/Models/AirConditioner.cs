using System;

namespace HomeApplianceRepair.Models
{
    /// <summary>Air conditioner: cost includes a site visit, gas refill and a large-unit surcharge.</summary>
    public class AirConditioner : Appliance
    {
        private const decimal SiteVisitFee = 30m;
        private const decimal LaborRatePerHour = 18m;
        private const decimal GasRefillFee = 60m;
        private const decimal LargeUnitSurcharge = 40m;
        private const int LargeUnitBtu = 24000;

        private int _coolingCapacityBtu;
        private bool _needsGasRefill;

        public AirConditioner(string serialNumber, string model, string customerName, DateTime receivedDate,
                              int coolingCapacityBtu, bool needsGasRefill = false)
            : base(serialNumber, model, customerName, receivedDate)
        {
            _coolingCapacityBtu = coolingCapacityBtu;
            _needsGasRefill = needsGasRefill;
        }

        public int CoolingCapacityBtu
        {
            get => _coolingCapacityBtu;
            set
            {
                _coolingCapacityBtu = value;
            }
        }

        public bool NeedsGasRefill
        {
            get => _needsGasRefill;
            set
            {
                _needsGasRefill = value;
            }
        }

        public override decimal CalculateRepairCost()
        {
            decimal cost = SiteVisitFee + CalculateLaborAndParts(LaborRatePerHour);

            if (_needsGasRefill)
            {
                cost += GasRefillFee;
            }

            if (_coolingCapacityBtu >= LargeUnitBtu)
            {
                cost += LargeUnitSurcharge;
            }

            return cost;
        }

        public override string GetSummary()
        {
            return base.GetSummary() +
                   " | Capacity: " + _coolingCapacityBtu + " BTU" +
                   " | Gas refill: " + (_needsGasRefill ? "Yes" : "No");
        }
    }
}
