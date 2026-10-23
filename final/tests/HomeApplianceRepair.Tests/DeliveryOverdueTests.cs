using System;
using HomeApplianceRepair.Models;
using HomeApplianceRepair.Services;
using Xunit;

namespace HomeApplianceRepair.Tests
{
    public class DeliveryOverdueTests
    {
        [Fact]
        public void CheckOverdue_UsesCompletedDaysAndRaisesAfterAllowedDaysAreExceeded()
        {
            RepairCenter center = new RepairCenter();
            Refrigerator fridge = TestData.NewFridge();
            center.Add(fridge);
            fridge.ChangeStatus(RepairStatus.UnderMaintenance);
            fridge.ChangeStatus(RepairStatus.Repaired);
            DateTime exactLimit = fridge.RepairedDate.Value.AddDays(3);
            int raised = 0;
            center.DeliveryOverdue += (sender, e) => raised++;

            Assert.Equal(0, center.CheckOverdueDeliveries(exactLimit, 3));
            Assert.Equal(0, raised);
            Assert.Equal(0, center.CheckOverdueDeliveries(exactLimit.AddTicks(1), 3));
            Assert.Equal(0, raised);
            Assert.Equal(1, center.CheckOverdueDeliveries(fridge.RepairedDate.Value.AddDays(4), 3));
            Assert.Equal(1, raised);
        }

        [Fact]
        public void CheckOverdue_DoesNotRaiseWhenReferenceTimeIsBeforeRepair()
        {
            RepairCenter center = new RepairCenter();
            Refrigerator fridge = TestData.NewFridge();
            center.Add(fridge);
            fridge.ChangeStatus(RepairStatus.UnderMaintenance);
            fridge.ChangeStatus(RepairStatus.Repaired);

            Assert.Equal(0, center.CheckOverdueDeliveries(fridge.RepairedDate.Value.AddTicks(-1), 0));
        }
    }
}
