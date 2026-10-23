using System;
using System.Text.RegularExpressions;

namespace HomeApplianceRepair.Utilities
{
    /// <summary>
    /// Static helper holding the validation rules for appliance data.
    /// It keeps no state, so a static class is the right choice.
    /// </summary>
    public static class ApplianceValidator
    {
        /// <summary>Largest cost value accepted for any cost field.</summary>
        public static decimal MaxCost { get; } = 1000000m;

        // Example of a valid serial number: RF-100234 (2-3 capital letters, dash, 4-8 digits).
        private static readonly Regex SerialPattern =
            new Regex(@"^[A-Z]{2,3}-[0-9]{4,8}\z", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        /// <summary>Earliest date accepted as a reception date.</summary>
        public static DateTime EarliestAllowedDate { get; } = new DateTime(2000, 1, 1);

        public static bool IsValidSerialNumber(string serialNumber)
        {
            return !string.IsNullOrWhiteSpace(serialNumber) && SerialPattern.IsMatch(serialNumber);
        }

        /// <summary>A reception date must be between 2000-01-01 and today (or the given reference date).</summary>
        public static bool IsValidReceivedDate(DateTime date, DateTime? referenceDate = null)
        {
            DateTime today = (referenceDate ?? DateTime.Today).Date;
            return date.Date >= EarliestAllowedDate && date.Date <= today;
        }

        public static bool IsNonEmptyText(string text, int maxLength = 60)
        {
            return !string.IsNullOrWhiteSpace(text) && text.Trim().Length <= maxLength;
        }

        public static bool IsValidCost(decimal cost)
        {
            return cost >= 0m && cost <= MaxCost;
        }

        /// <summary>Returns true when min &lt;= value &lt;= max (false for NaN).</summary>
        public static bool IsInRange(double value, double min, double max)
        {
            return value >= min && value <= max;
        }
    }
}
