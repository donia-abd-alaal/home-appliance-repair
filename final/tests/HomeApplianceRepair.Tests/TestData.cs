using System;
using HomeApplianceRepair.Models;

namespace HomeApplianceRepair.Tests
{
    /// <summary>Factory methods that create valid appliances for the tests.</summary>
    internal static class TestData
    {
        public static Refrigerator NewFridge(string serial = "RF-100234", int liters = 400, bool compressorFault = false)
        {
            return new Refrigerator(serial, "Test Fridge", "Test Customer", DateTime.Today.AddDays(-3), liters, compressorFault);
        }

        public static WashingMachine NewWasher(string serial = "WM-200411", bool frontLoader = false, bool drum = false)
        {
            return new WashingMachine(serial, "Test Washer", "Test Customer", DateTime.Today.AddDays(-3), 8, frontLoader, drum);
        }

        public static AirConditioner NewAc(string serial = "AC-300777", int btu = 12000, bool gas = false)
        {
            return new AirConditioner(serial, "Test AC", "Test Customer", DateTime.Today.AddDays(-3), btu, gas);
        }
    }
}
