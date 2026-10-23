using System;
using System.Globalization;

namespace HomeApplianceRepair.Utilities
{
    /// <summary>Culture-independent formatting helpers so output is identical on every machine.</summary>
    public static class DisplayFormat
    {
        public static string Money(decimal amount)
        {
            return amount.ToString("F2", CultureInfo.InvariantCulture);
        }

        public static string Number(double value)
        {
            return value.ToString("0.0", CultureInfo.InvariantCulture);
        }

        public static string Date(DateTime date)
        {
            return date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }

        public static string YesNo(bool value)
        {
            return value ? "Yes" : "No";
        }
    }
}
