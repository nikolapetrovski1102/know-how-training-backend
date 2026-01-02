using System;
using Microsoft.Data.SqlClient;

namespace Core.Application.Helpers
{
    public static class SqlDataReaderExtensions
    {
        public static string? GetSafeString(this SqlDataReader reader, string columnName)
        {
            try
            {
                var ordinal = reader.GetOrdinal(columnName);
                return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
            }
            catch (IndexOutOfRangeException)
            {
                return null;
            }
        }

        public static int GetInt32(this SqlDataReader reader, string columnName)
        {
            var ordinal = reader.GetOrdinal(columnName);
            return reader.GetInt32(ordinal);
        }

        public static bool GetBoolean(this SqlDataReader reader, string columnName)
        {
            var ordinal = reader.GetOrdinal(columnName);
            return reader.IsDBNull(ordinal) ? false : reader.GetBoolean(ordinal);
        }

        public static bool IsDBNull(this SqlDataReader reader, string columnName)
        {
            try
            {
                var ordinal = reader.GetOrdinal(columnName);
                return reader.IsDBNull(ordinal);
            }
            catch (IndexOutOfRangeException)
            {
                return true;
            }
        }

        // ✅ NEW - Safe nullable int
        public static int? GetSafeInt32(this SqlDataReader reader, string columnName)
        {
            try
            {
                var ordinal = reader.GetOrdinal(columnName);
                return reader.IsDBNull(ordinal) ? null : reader.GetInt32(ordinal);
            }
            catch (IndexOutOfRangeException)
            {
                return null;
            }
        }

        // ✅ NEW - Safe nullable decimal
        public static decimal? GetSafeDecimal(this SqlDataReader reader, string columnName)
        {
            try
            {
                var ordinal = reader.GetOrdinal(columnName);
                return reader.IsDBNull(ordinal) ? null : reader.GetDecimal(ordinal);
            }
            catch (IndexOutOfRangeException)
            {
                return null;
            }
        }

        // Keep existing generic method for other types
        public static T? GetSafeValue<T>(this SqlDataReader reader, string columnName) where T : struct
        {
            try
            {
                var ordinal = reader.GetOrdinal(columnName);
                if (reader.IsDBNull(ordinal)) return null;

                var value = reader.GetValue(ordinal);

                // Handle conversions
                if (value is IConvertible)
                {
                    return (T)Convert.ChangeType(value, typeof(T));
                }

                return (T)value;
            }
            catch (IndexOutOfRangeException)
            {
                return null;
            }
            catch
            {
                return null;
            }
        }
    }
}
