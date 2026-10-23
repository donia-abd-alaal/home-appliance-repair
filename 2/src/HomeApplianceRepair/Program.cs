using System;
using System.Globalization;
using HomeApplianceRepair.Models;
using HomeApplianceRepair.Services;

namespace HomeApplianceRepair
{
    /// <summary>Console demo that exercises every requirement of the assignment.</summary>
    internal static class Program
    {
        private static void Main()
        {
            RepairCenter center = new RepairCenter();

            PrintHeader("1. Creating appliances of different types");
            Refrigerator fridge = new Refrigerator("RF-100234", "Samsung RT38", "Ahmad Khaled",
                DateTime.Today.AddDays(-6), 420, hasCompressorFault: true);
            WashingMachine washer = new WashingMachine("WM-200411", "LG F4V5", "Lina Haddad",
                DateTime.Today.AddDays(-5), 8.5, isFrontLoader: true);
            AirConditioner ac1 = new AirConditioner("AC-300777", "Gree Split 18K", "Omar Nasser",
                DateTime.Today.AddDays(-4), 18000, needsGasRefill: true);
            AirConditioner ac2 = new AirConditioner("AC-300812", "Midea Inverter 24K", "Sara Mansour",
                DateTime.Today.AddDays(-2), 24000);

            fridge.PartsCost = 45m;
            fridge.LaborHours = 3.5;
            washer.PartsCost = 30m;
            washer.LaborHours = 2;
            ac1.PartsCost = 20m;
            ac1.LaborHours = 1.5;
            ac2.PartsCost = 85m;
            ac2.LaborHours = 4;

            // All appliances are stored in one list of the common base type.
            center.Add(fridge);
            center.Add(washer);
            center.Add(ac1);
            center.Add(ac2);
            Console.WriteLine("  Appliances registered: " + center.Count);

            PrintHeader("2. Polymorphism: every appliance describes itself and prices itself");
            foreach (string line in center.GetSummaries())
            {
                Console.WriteLine("  " + line);
            }

            Console.WriteLine("  Total repair cost (all appliances): " + Money(center.CalculateTotalRepairCost()));

            PrintHeader("3. Changing status along the life cycle");
            center.ChangeStatus("RF-100234", RepairStatus.UnderMaintenance);
            center.ChangeStatus("RF-100234", RepairStatus.Repaired);
            center.ChangeStatus("WM-200411", RepairStatus.UnderMaintenance);
            center.ChangeStatus("AC-300777", RepairStatus.UnderMaintenance);
            center.ChangeStatus("AC-300777", RepairStatus.Repaired);
            center.ChangeStatus("AC-300777", RepairStatus.Delivered);

            PrintHeader("4. Invalid transitions are blocked");
            TryAction("Deliver an appliance that was never repaired", () => ac2.ChangeStatus(RepairStatus.Delivered));

            PrintHeader("5. Appliances by status");
            foreach (RepairStatus status in Enum.GetValues(typeof(RepairStatus)))
            {
                Console.WriteLine("  " + status + " (total cost: " + Money(center.CalculateTotalRepairCost(status)) + ")");
                foreach (string line in center.GetSummaries(status))
                {
                    Console.WriteLine("    " + line);
                }
            }
        }

        private static string Money(decimal amount)
        {
            return amount.ToString("F2", CultureInfo.InvariantCulture);
        }

        private static void PrintHeader(string title)
        {
            Console.WriteLine();
            Console.WriteLine("=== " + title + " ===");
        }

        // Runs an action that is expected to be rejected and prints the reason.
        private static void TryAction(string description, Action action)
        {
            try
            {
                action();
                Console.WriteLine("  [UNEXPECTED] " + description + " was accepted.");
            }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
            {
                Console.WriteLine("  [Rejected] " + description + ": " + ex.Message);
            }
        }
    }
}
