# Home Appliance Repair Center Management System

Course assignment **TIC_IPG203_F25** (Object-Oriented Programming and Design, Syrian Virtual University).
A C# console application that manages a home appliance repair center and demonstrates the core OOP principles.

## Requirements

- .NET SDK 8.0 or later (https://dotnet.microsoft.com/download)

## Run

```bash
dotnet run --project src/HomeApplianceRepair
```

## Test

```bash
dotnet test HomeApplianceRepair.sln
```

The test project needs the xUnit NuGet packages, so the first run requires an internet connection.

## Build

```bash
dotnet build HomeApplianceRepair.sln
```

The solution file `HomeApplianceRepair.sln` is in the repository root and contains the application and the test project.

## Project structure

```
src/HomeApplianceRepair/
├── Interfaces/IAppliance.cs              Interface with the shared operations
├── Models/
│   ├── Appliance.cs                      Abstract class (implements IAppliance, raises StatusChanged)
│   ├── Refrigerator.cs                   Subclass
│   ├── WashingMachine.cs                 Subclass
│   ├── AirConditioner.cs                 Subclass
│   └── RepairStatus.cs                   Enum: Received, UnderMaintenance, Repaired, Delivered
├── Events/                               Delegates and event argument classes
├── Services/
│   ├── RepairCenter.cs                   Main class: List<Appliance>, polymorphic processing, event relay
│   └── CustomerNotifier.cs               Event subscriber
├── Utilities/
│   ├── ApplianceValidator.cs             Static validation class
│   └── DisplayFormat.cs                  Static formatting helpers
└── Program.cs                            Console demo
tests/HomeApplianceRepair.Tests/          xUnit automated tests
docs/                                     UML source (Graphviz .dot) with SVG and PNG exports
HomeApplianceRepair.sln                   Solution file
```

## UML diagram

Edit `docs/UML-class-diagram.dot` as the source and regenerate the exports with `dot -Tpng` and `dot -Tsvg`.

## OOP principles and where to find them

| Principle          | Where                                                                                                                |
| ------------------ | -------------------------------------------------------------------------------------------------------------------- |
| Abstraction        | `IAppliance`, `Appliance.CalculateRepairCost()` (abstract)                                                           |
| Inheritance        | `Refrigerator`, `WashingMachine`, `AirConditioner` inherit `Appliance`                                               |
| Polymorphism       | `RepairCenter.CalculateTotalRepairCost()` and `GetSummaries()` over `List<Appliance>`                                |
| Encapsulation      | private fields, validated properties, read-only `SerialNumber` and `ReceivedDate`                                    |
| Delegates & Events | `ApplianceStatusChangedHandler`, `DeliveryOverdueHandler`, `Appliance.StatusChanged`, `RepairCenter.DeliveryOverdue` |
| Static members     | `ApplianceValidator` (static class), `Appliance.UnderMaintenanceCount`, `Appliance.TotalRecordedRepairCost`          |

## Status life cycle

`Received -> UnderMaintenance -> Repaired -> Delivered`. Any other transition is rejected
(for example, an appliance that was not repaired cannot be delivered).
