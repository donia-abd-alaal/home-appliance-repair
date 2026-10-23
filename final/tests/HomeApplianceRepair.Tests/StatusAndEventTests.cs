using System;
using System.Collections.Generic;
using HomeApplianceRepair.Events;
using HomeApplianceRepair.Models;
using HomeApplianceRepair.Services;
using Xunit;

namespace HomeApplianceRepair.Tests
{
    public class StatusAndEventTests
    {
        // ----- Status transitions -----

        [Fact]
        public void ValidSequence_Succeeds()
        {
            Refrigerator fridge = TestData.NewFridge();

            fridge.ChangeStatus(RepairStatus.UnderMaintenance);
            fridge.ChangeStatus(RepairStatus.Repaired);
            fridge.ChangeStatus(RepairStatus.Delivered);

            Assert.Equal(RepairStatus.Delivered, fridge.Status);
            Assert.NotNull(fridge.RepairedDate);
        }

        [Theory]
        [InlineData(RepairStatus.Delivered)]
        [InlineData(RepairStatus.Repaired)]
        [InlineData(RepairStatus.Received)]
        public void InvalidTransitionFromReceived_IsRejected(RepairStatus target)
        {
            Washer_Rejects(TestData.NewWasher(), target);
        }

        private static void Washer_Rejects(WashingMachine washer, RepairStatus target)
        {
            Assert.False(washer.CanChangeStatusTo(target));
            Assert.Throws<InvalidOperationException>(() => washer.ChangeStatus(target));
            Assert.Equal(RepairStatus.Received, washer.Status);
        }

        [Fact]
        public void CannotDeliver_UnrepairedAppliance()
        {
            AirConditioner ac = TestData.NewAc();
            ac.ChangeStatus(RepairStatus.UnderMaintenance);

            Assert.Throws<InvalidOperationException>(() => ac.ChangeStatus(RepairStatus.Delivered));
        }

        [Fact]
        public void CannotMoveBackwards_OrRepeatSameStatus()
        {
            Refrigerator fridge = TestData.NewFridge();
            fridge.ChangeStatus(RepairStatus.UnderMaintenance);

            Assert.Throws<InvalidOperationException>(() => fridge.ChangeStatus(RepairStatus.UnderMaintenance));
            Assert.Throws<InvalidOperationException>(() => fridge.ChangeStatus(RepairStatus.Received));
        }

        // ----- Events -----

        [Fact]
        public void StatusChangedEvent_IsRaisedOncePerTransition_WithCorrectData()
        {
            RepairCenter center = new RepairCenter();
            Refrigerator fridge = TestData.NewFridge();
            center.Add(fridge);
            List<ApplianceStatusChangedEventArgs> received = new List<ApplianceStatusChangedEventArgs>();
            center.ApplianceStatusChanged += (sender, e) => received.Add(e);

            fridge.ChangeStatus(RepairStatus.UnderMaintenance);
            fridge.ChangeStatus(RepairStatus.Repaired);

            Assert.Equal(2, received.Count);
            Assert.Equal(RepairStatus.UnderMaintenance, received[1].OldStatus);
            Assert.Equal(RepairStatus.Repaired, received[1].NewStatus);
            Assert.Same(fridge, received[1].Appliance);
        }

        [Fact]
        public void ApplianceStatusChangedEvent_CanBeSubscribedDirectly_AndCarriesOldAndNewStatus()
        {
            Refrigerator fridge = TestData.NewFridge();
            ApplianceStatusChangedEventArgs first = null;
            int count = 0;
            fridge.StatusChanged += (sender, e) => { count++; first = first ?? e; };

            fridge.ChangeStatus(RepairStatus.UnderMaintenance);

            Assert.Equal(1, count);
            Assert.Same(fridge, first.Appliance);
            Assert.Equal(RepairStatus.Received, first.OldStatus);
            Assert.Equal(RepairStatus.UnderMaintenance, first.NewStatus);
        }

        [Fact]
        public void StatusChangedEvent_IsNotRaised_WhenTransitionIsRejected()
        {
            RepairCenter center = new RepairCenter();
            WashingMachine washer = TestData.NewWasher();
            center.Add(washer);
            int count = 0;
            center.ApplianceStatusChanged += (sender, e) => count++;

            Assert.Throws<InvalidOperationException>(() => washer.ChangeStatus(RepairStatus.Delivered));

            Assert.Equal(0, count);
        }

        [Fact]
        public void DeliveryOverdue_IsRaisedOnce_WhenLimitExceeded()
        {
            RepairCenter center = new RepairCenter();
            Refrigerator fridge = TestData.NewFridge();
            center.Add(fridge);
            fridge.ChangeStatus(RepairStatus.UnderMaintenance);
            fridge.ChangeStatus(RepairStatus.Repaired);
            List<DeliveryOverdueEventArgs> received = new List<DeliveryOverdueEventArgs>();
            center.DeliveryOverdue += (sender, e) => received.Add(e);
            DateTime later = fridge.RepairedDate.Value.AddDays(5);

            Assert.Equal(1, center.CheckOverdueDeliveries(later, 3));
            Assert.Equal(0, center.CheckOverdueDeliveries(later, 3)); // no repeated event
            Assert.Single(received);
            Assert.Equal(5, received[0].DaysSinceRepair);
        }

        [Fact]
        public void DeliveryOverdue_IsNotRaised_WithinLimit_OrAtTheBoundary_OrWhenDelivered()
        {
            RepairCenter center = new RepairCenter();
            Refrigerator fridge = TestData.NewFridge();
            AirConditioner ac = TestData.NewAc();
            center.Add(fridge);
            center.Add(ac);
            fridge.ChangeStatus(RepairStatus.UnderMaintenance);
            fridge.ChangeStatus(RepairStatus.Repaired);
            ac.ChangeStatus(RepairStatus.UnderMaintenance);
            ac.ChangeStatus(RepairStatus.Repaired);
            ac.ChangeStatus(RepairStatus.Delivered);
            int count = 0;
            center.DeliveryOverdue += (sender, e) => count++;

            Assert.Equal(0, center.CheckOverdueDeliveries(fridge.RepairedDate.Value.AddDays(2), 3));
            Assert.Equal(0, center.CheckOverdueDeliveries(fridge.RepairedDate.Value.AddDays(3), 3));
            Assert.Equal(0, count);
        }

        [Fact]
        public void CheckOverdue_RejectsNegativeLimit()
        {
            RepairCenter center = new RepairCenter();

            Assert.Throws<ArgumentOutOfRangeException>(() => center.CheckOverdueDeliveries(DateTime.Now, -1));
        }

        [Fact]
        public void Notifier_ReactsToEvents()
        {
            RepairCenter center = new RepairCenter();
            CustomerNotifier notifier = new CustomerNotifier();
            notifier.Subscribe(center);
            Refrigerator fridge = TestData.NewFridge();
            center.Add(fridge);

            fridge.ChangeStatus(RepairStatus.UnderMaintenance);
            fridge.ChangeStatus(RepairStatus.Repaired);

            Assert.Equal(2, notifier.SentNotifications.Count);
            Assert.Contains("repaired", notifier.SentNotifications[1]);
        }

        [Fact]
        public void Notifier_SendsDeliveredAndOverdueMessages()
        {
            RepairCenter center = new RepairCenter();
            CustomerNotifier notifier = new CustomerNotifier();
            notifier.Subscribe(center);
            AirConditioner ac = TestData.NewAc("AC-300777");
            WashingMachine washer = TestData.NewWasher("WM-200411");
            center.Add(ac);
            center.Add(washer);
            ac.ChangeStatus(RepairStatus.UnderMaintenance);
            ac.ChangeStatus(RepairStatus.Repaired);
            ac.ChangeStatus(RepairStatus.Delivered);
            washer.ChangeStatus(RepairStatus.UnderMaintenance);
            washer.ChangeStatus(RepairStatus.Repaired);

            center.CheckOverdueDeliveries(DateTime.Now.AddDays(5), 3);

            Assert.Equal(6, notifier.SentNotifications.Count);
            Assert.Contains("delivered", notifier.SentNotifications[2]);
            Assert.Contains("Please collect it", notifier.SentNotifications[5]);
        }

        [Fact]
        public void Notifier_StopsReceiving_AfterUnsubscribe()
        {
            RepairCenter center = new RepairCenter();
            CustomerNotifier notifier = new CustomerNotifier();
            notifier.Subscribe(center);
            notifier.Unsubscribe(center);
            WashingMachine washer = TestData.NewWasher();
            center.Add(washer);

            washer.ChangeStatus(RepairStatus.UnderMaintenance);

            Assert.Empty(notifier.SentNotifications);
        }

        // ----- RepairCenter list handling -----

        [Fact]
        public void Center_RejectsNullAndDuplicates()
        {
            RepairCenter center = new RepairCenter();
            center.Add(TestData.NewFridge("RF-100234"));

            Assert.Throws<ArgumentNullException>(() => center.Add(null));
            Assert.Throws<InvalidOperationException>(() => center.Add(TestData.NewFridge("RF-100234")));
        }

        [Fact]
        public void Center_FindsAndFiltersByStatus()
        {
            RepairCenter center = new RepairCenter();
            center.Add(TestData.NewFridge("RF-100234"));
            center.Add(TestData.NewWasher("WM-200411"));
            center.ChangeStatus("WM-200411", RepairStatus.UnderMaintenance);

            Assert.NotNull(center.FindBySerial("RF-100234"));
            Assert.Null(center.FindBySerial("RF-999999"));
            Assert.Single(center.GetByStatus(RepairStatus.UnderMaintenance));
            Assert.Single(center.GetByStatus(RepairStatus.Received));
            Assert.Equal(15m, center.CalculateTotalRepairCost(RepairStatus.UnderMaintenance));
        }

        [Fact]
        public void Center_ChangeStatus_WithUnknownSerial_Throws()
        {
            RepairCenter center = new RepairCenter();

            Assert.Throws<KeyNotFoundException>(() => center.ChangeStatus("RF-999999", RepairStatus.UnderMaintenance));
        }
    }
}
