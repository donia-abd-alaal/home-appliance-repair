using System;
using HomeApplianceRepair.Models;
using HomeApplianceRepair.Utilities;
using Xunit;

namespace HomeApplianceRepair.Tests
{
    public class StaticMembersTests
    {
        // ----- Static class: ApplianceValidator -----

        [Theory]
        [InlineData("RF-100234")]
        [InlineData("WM-2004")]
        [InlineData("AC-12345678")]
        [InlineData("ABC-1234")]
        public void ValidSerialNumbers_AreAccepted(string serial)
        {
            Assert.True(ApplianceValidator.IsValidSerialNumber(serial));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("rf-100234")]
        [InlineData("RF-123")]
        [InlineData("RF-1234\n")]
        [InlineData("RF-12a4")]
        public void InvalidSerialNumbers_AreRejected(string serial)
        {
            Assert.False(ApplianceValidator.IsValidSerialNumber(serial));
        }

        [Fact]
        public void ReceivedDate_Boundaries()
        {
            DateTime today = new DateTime(2026, 10, 13);

            Assert.True(ApplianceValidator.IsValidReceivedDate(today, today));
            Assert.False(ApplianceValidator.IsValidReceivedDate(today.AddDays(1), today));
            Assert.True(ApplianceValidator.IsValidReceivedDate(new DateTime(2000, 1, 1), today));
            Assert.False(ApplianceValidator.IsValidReceivedDate(new DateTime(1999, 12, 31), today));
        }

        [Fact]
        public void Cost_Boundaries()
        {
            Assert.True(ApplianceValidator.IsValidCost(0m));
            Assert.False(ApplianceValidator.IsValidCost(-0.01m));
            Assert.True(ApplianceValidator.IsValidCost(ApplianceValidator.MaxCost));
            Assert.False(ApplianceValidator.IsValidCost(ApplianceValidator.MaxCost + 0.01m));
        }

        [Fact]
        public void Text_And_Range_Rules()
        {
            Assert.True(ApplianceValidator.IsNonEmptyText("LG"));
            Assert.False(ApplianceValidator.IsNonEmptyText(null));
            Assert.False(ApplianceValidator.IsNonEmptyText("   "));
            Assert.False(ApplianceValidator.IsNonEmptyText(new string('x', 61)));
            Assert.True(ApplianceValidator.IsInRange(5, 5, 10));
            Assert.False(ApplianceValidator.IsInRange(double.NaN, 0, 10));
        }

        [Fact]
        public void Validator_IsStaticClass()
        {
            Type type = typeof(ApplianceValidator);

            Assert.True(type.IsAbstract && type.IsSealed); // how C# represents a static class
        }

        // ----- Static properties (compared as differences, so other tests do not matter) -----

        [Fact]
        public void UnderMaintenanceCount_FollowsRealTransitions()
        {
            int before = Appliance.UnderMaintenanceCount;
            Refrigerator fridge = TestData.NewFridge();

            fridge.ChangeStatus(RepairStatus.UnderMaintenance);
            Assert.Equal(before + 1, Appliance.UnderMaintenanceCount);

            fridge.ChangeStatus(RepairStatus.Repaired);
            Assert.Equal(before, Appliance.UnderMaintenanceCount);
        }

        [Fact]
        public void TotalRecordedRepairCost_AddsCostWhenRepaired()
        {
            decimal before = Appliance.TotalRecordedRepairCost;
            Refrigerator fridge = TestData.NewFridge(compressorFault: true);
            fridge.PartsCost = 45m;
            fridge.LaborHours = 3.5;

            fridge.ChangeStatus(RepairStatus.UnderMaintenance);
            Assert.Equal(before, Appliance.TotalRecordedRepairCost); // not yet repaired

            fridge.ChangeStatus(RepairStatus.Repaired);
            Assert.Equal(before + 237.5m, Appliance.TotalRecordedRepairCost);

            fridge.ChangeStatus(RepairStatus.Delivered);
            Assert.Equal(before + 237.5m, Appliance.TotalRecordedRepairCost); // delivery adds nothing
            Assert.Equal(237.5m, fridge.RecordedRepairCost);
        }

        [Fact]
        public void RejectedTransition_DoesNotChangeStatistics()
        {
            int countBefore = Appliance.UnderMaintenanceCount;
            decimal totalBefore = Appliance.TotalRecordedRepairCost;
            WashingMachine washer = TestData.NewWasher();

            Assert.Throws<InvalidOperationException>(() => washer.ChangeStatus(RepairStatus.Delivered));

            Assert.Equal(countBefore, Appliance.UnderMaintenanceCount);
            Assert.Equal(totalBefore, Appliance.TotalRecordedRepairCost);
        }
    }
}
