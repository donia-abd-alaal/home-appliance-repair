using System;
using HomeApplianceRepair.Interfaces;
using HomeApplianceRepair.Models;

namespace HomeApplianceRepair
{
    internal static class Program
    {
        // Stage 1 only defines the abstractions, so there is nothing to instantiate yet.
        // We simply confirm the design with reflection.
        private static void Main()
        {
            Console.WriteLine("Stage 1: Abstraction");
            Console.WriteLine("  IAppliance is an interface:        " + typeof(IAppliance).IsInterface);
            Console.WriteLine("  Appliance is an abstract class:    " + typeof(Appliance).IsAbstract);
            Console.WriteLine("  Appliance implements IAppliance:   " + typeof(IAppliance).IsAssignableFrom(typeof(Appliance)));
            Console.WriteLine("  Statuses: " + string.Join(" -> ", Enum.GetNames(typeof(RepairStatus))));
        }
    }
}
