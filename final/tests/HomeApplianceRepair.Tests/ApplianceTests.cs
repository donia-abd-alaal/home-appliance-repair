using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HomeApplianceRepair.Models;
using HomeApplianceRepair.Services;
using HomeApplianceRepair.Utilities;
using Xunit;

namespace HomeApplianceRepair.Tests
{
    public class ApplianceTests
    {
        // ----- Abstraction / inheritance -----

        [Fact]
        public void Appliance_IsAbstract_AndImplementsInterface()
        {
            Assert.True(typeof(Appliance).IsAbstract);
            Assert.True(typeof(HomeApplianceRepair.Interfaces.IAppliance).IsAssignableFrom(typeof(Appliance)));
        }

        [Fact]
        public void EachType_CanBeCreated_AndStartsAsReceived()
        {
            Appliance[] all = { TestData.NewFridge(), TestData.NewWasher(), TestData.NewAc() };

            Assert.All(all, a => Assert.Equal(RepairStatus.Received, a.Status));
        }

        // ----- Cost calculation per type -----

        [Fact]
        public void Refrigerator_Cost_IncludesCompressorFault()
        {
            Refrigerator fridge = TestData.NewFridge(compressorFault: true);
            fridge.PartsCost = 45m;
            fridge.LaborHours = 3.5;

            Assert.Equal(237.5m, fridge.CalculateRepairCost()); // 20 + 45 + 52.5 + 120
        }

        [Fact]
        public void WashingMachine_Cost_IncludesFrontLoaderSurcharge()
        {
            WashingMachine washer = TestData.NewWasher(frontLoader: true);
            washer.PartsCost = 30m;
            washer.LaborHours = 2;

            Assert.Equal(94m, washer.CalculateRepairCost()); // 15 + 30 + 24 + 25
        }

        [Fact]
        public void AirConditioner_Cost_IncludesGasRefill()
        {
            AirConditioner ac = TestData.NewAc(gas: true);
            ac.PartsCost = 20m;
            ac.LaborHours = 1.5;

            Assert.Equal(137m, ac.CalculateRepairCost()); // 30 + 20 + 27 + 60
        }

        [Fact]
        public void Cost_WithNoLaborAndNoParts_IsOnlyTheBaseFee()
        {
            Assert.Equal(20m, TestData.NewFridge().CalculateRepairCost());
            Assert.Equal(15m, TestData.NewWasher().CalculateRepairCost());
            Assert.Equal(30m, TestData.NewAc().CalculateRepairCost());
        }

        [Fact]
        public void Refrigerator_LargeCapacityBoundary_AddsSurcharge()
        {
            Assert.Equal(20m, TestData.NewFridge(liters: 499).CalculateRepairCost());
            Assert.Equal(50m, TestData.NewFridge(liters: 500).CalculateRepairCost());
        }

        // ----- Polymorphism -----

        [Fact]
        public void SameCall_ProducesDifferentCosts_ForDifferentTypes()
        {
            List<Appliance> list = new List<Appliance> { TestData.NewFridge(), TestData.NewWasher(), TestData.NewAc() };
            foreach (Appliance a in list)
            {
                a.LaborHours = 2; // identical inputs for every type
            }

            List<decimal> costs = list.Select(a => a.CalculateRepairCost()).ToList();

            Assert.Equal(new List<decimal> { 50m, 39m, 66m }, costs);
        }

        [Fact]
        public void RepairCenter_Total_UsesPolymorphicCosts()
        {
            RepairCenter center = new RepairCenter();
            Refrigerator fridge = TestData.NewFridge();
            WashingMachine washer = TestData.NewWasher();
            AirConditioner ac = TestData.NewAc();
            center.Add(fridge);
            center.Add(washer);
            center.Add(ac);

            Assert.Equal(20m + 15m + 30m, center.CalculateTotalRepairCost());
        }

        // ----- Encapsulation -----

        [Fact]
        public void SerialNumber_And_ReceivedDate_AreReadOnly()
        {
            Assert.False(typeof(Appliance).GetProperty("SerialNumber").CanWrite);
            Assert.False(typeof(Appliance).GetProperty("ReceivedDate").CanWrite);
            Assert.False(typeof(Appliance).GetProperty("Status").CanWrite);
        }

        [Theory]
        [InlineData(typeof(Appliance))]
        [InlineData(typeof(Refrigerator))]
        [InlineData(typeof(WashingMachine))]
        [InlineData(typeof(AirConditioner))]
        [InlineData(typeof(RepairCenter))]
        [InlineData(typeof(CustomerNotifier))]
        [InlineData(typeof(ApplianceValidator))]
        [InlineData(typeof(DisplayFormat))]
        public void AllFields_ArePrivate(Type type)
        {
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public |
                                 BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

            Assert.All(type.GetFields(flags), f => Assert.True(f.IsPrivate, type.Name + "." + f.Name + " must be private"));
        }

        [Fact]
        public void CostInputs_AreLocked_AfterRepair()
        {
            Refrigerator fridge = TestData.NewFridge();
            fridge.ChangeStatus(RepairStatus.UnderMaintenance);
            fridge.ChangeStatus(RepairStatus.Repaired);

            Assert.Throws<InvalidOperationException>(() => fridge.PartsCost = 10m);
            Assert.Throws<InvalidOperationException>(() => fridge.LaborHours = 1);
            Assert.Throws<InvalidOperationException>(() => fridge.HasCompressorFault = true);
        }

        [Fact]
        public void CostInputs_OfEverySubtype_AreLocked_AfterRepair_AndAfterDelivery()
        {
            WashingMachine washer = TestData.NewWasher();
            AirConditioner ac = TestData.NewAc();
            foreach (Appliance a in new Appliance[] { washer, ac })
            {
                a.ChangeStatus(RepairStatus.UnderMaintenance);
                a.ChangeStatus(RepairStatus.Repaired);
            }

            Assert.Throws<InvalidOperationException>(() => washer.NeedsDrumReplacement = true);
            Assert.Throws<InvalidOperationException>(() => washer.IsFrontLoader = true);
            Assert.Throws<InvalidOperationException>(() => ac.NeedsGasRefill = true);
            Assert.Throws<InvalidOperationException>(() => ac.CoolingCapacityBtu = 30000);

            ac.ChangeStatus(RepairStatus.Delivered);
            Assert.Throws<InvalidOperationException>(() => ac.PartsCost = 5m);
        }

        [Fact]
        public void CostInputs_AreEditable_BeforeRepair_AndChangeTheCost()
        {
            AirConditioner ac = TestData.NewAc();
            ac.ChangeStatus(RepairStatus.UnderMaintenance);

            ac.NeedsGasRefill = true;
            ac.CoolingCapacityBtu = 24000;

            Assert.Equal(30m + 60m + 40m, ac.CalculateRepairCost());
        }

        [Fact]
        public void ModelAndCustomerName_AreTrimmed_WhenStored()
        {
            Refrigerator fridge = TestData.NewFridge();

            fridge.Model = "  Samsung RT38  ";
            fridge.CustomerName = " Ahmad ";

            Assert.Equal("Samsung RT38", fridge.Model);
            Assert.Equal("Ahmad", fridge.CustomerName);
        }

        // ----- Polymorphic summaries -----

        [Fact]
        public void GetSummaries_UsesEachTypeOwnDescription()
        {
            RepairCenter center = new RepairCenter();
            center.Add(TestData.NewFridge());
            center.Add(TestData.NewWasher());
            center.Add(TestData.NewAc());

            List<string> summaries = center.GetSummaries();

            Assert.Equal(3, summaries.Count);
            Assert.Contains("[Refrigerator]", summaries[0]);
            Assert.Contains("Compressor fault", summaries[0]);
            Assert.Contains("[WashingMachine]", summaries[1]);
            Assert.Contains("Front loader", summaries[1]);
            Assert.Contains("[AirConditioner]", summaries[2]);
            Assert.Contains("BTU", summaries[2]);
        }

        [Fact]
        public void DisplayFormat_IsCultureIndependent()
        {
            Assert.Equal("237.50", DisplayFormat.Money(237.5m));
            Assert.Equal("8.5", DisplayFormat.Number(8.5));
            Assert.Equal("2026-10-13", DisplayFormat.Date(new DateTime(2026, 10, 13)));
            Assert.Equal("Yes", DisplayFormat.YesNo(true));
            Assert.Equal("No", DisplayFormat.YesNo(false));
        }

        // ----- Validation on creation and modification -----

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("rf-100234")]
        [InlineData("RF100234")]
        [InlineData("R-1234")]
        [InlineData("RF-123")]
        [InlineData("RF-123456789")]
        [InlineData("RFFF-1234")]
        public void InvalidSerialNumber_IsRejected(string serial)
        {
            Assert.Throws<ArgumentException>(() => TestData.NewFridge(serial));
        }

        [Fact]
        public void FutureReceivedDate_IsRejected()
        {
            Assert.Throws<ArgumentException>(() =>
                new AirConditioner("AC-300777", "Test", "Test", DateTime.Today.AddDays(1), 12000));
        }

        [Fact]
        public void EmptyModelOrCustomer_IsRejected()
        {
            Assert.Throws<ArgumentException>(() =>
                new Refrigerator("RF-100234", "", "Customer", DateTime.Today, 300));
            Assert.Throws<ArgumentException>(() => TestData.NewWasher().CustomerName = " ");
        }

        [Fact]
        public void NegativeOrHugeCost_IsRejected()
        {
            Refrigerator fridge = TestData.NewFridge();

            Assert.Throws<ArgumentOutOfRangeException>(() => fridge.PartsCost = -1m);
            Assert.Throws<ArgumentOutOfRangeException>(() => fridge.PartsCost = 1000000.01m);
            Assert.Throws<ArgumentOutOfRangeException>(() => fridge.LaborHours = -0.5);
            Assert.Throws<ArgumentOutOfRangeException>(() => fridge.LaborHours = double.NaN);
        }

        [Fact]
        public void ZeroCost_And_MaxCost_AreAccepted()
        {
            Refrigerator fridge = TestData.NewFridge();
            fridge.PartsCost = 0m;
            fridge.PartsCost = 1000000m;

            Assert.Equal(1000000m, fridge.PartsCost);
        }

        [Fact]
        public void CapacityOutOfRange_IsRejected()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => TestData.NewFridge(liters: 10));
            Assert.Throws<ArgumentOutOfRangeException>(() => TestData.NewAc(btu: 100000));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new WashingMachine("WM-200411", "Test", "Test", DateTime.Today, 1));
        }
    }
}
