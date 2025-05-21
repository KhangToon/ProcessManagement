using System.Globalization;

namespace ProcessManagement.Commons
{
    public static class DateTimeHelper
    {
        private const string SQLDateFormat = "yyyy-MM-dd HH:mm:ss"; // Base format for SQL without milliseconds

        public static DateTime? ExTractFromSQLDateFormat(object? inputSQLDate)
        {
            if (inputSQLDate == null)
            {
                return null; // Return null if inputSQLDate is null
            }

            string? dateString = inputSQLDate.ToString();
            if (string.IsNullOrWhiteSpace(dateString))
            {
                return null; // Return null if the string representation is empty or whitespace
            }

            // Define the possible formats
            string[] formats = {
                        "yyyy-MM-dd HH:mm:ss",      // Format without milliseconds
                        "yyyy-MM-dd HH:mm:ss.fff"    // Format with milliseconds
            };

            // Try parsing the date
            if (DateTime.TryParseExact(dateString, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
            {
                return result; // Return the parsed DateTime
            }

            return null; // Return null if parsing fails
        }

        public static DateTime? ConvertToSQLDatetimeFormat(DateTime inputDate)
        {
            string formattedDate = inputDate.ToString(SQLDateFormat, CultureInfo.InvariantCulture);
            return DateTime.ParseExact(formattedDate, SQLDateFormat, CultureInfo.InvariantCulture);
        }

    }
}
