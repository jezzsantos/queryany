using System;
using System.Globalization;

namespace QueryAny
{
    /// <summary>
    /// Copied from Microsoft.Azure.Cosmos.Table.TableQuery
    /// </summary>
    public class AzureCosmosTableQuery
    {
        public static readonly DateTime MinAzureDateTime = new DateTime(1780, 1, 1, 0, 0, 0, DateTimeKind.Utc);


        public static string CombineFilters(string filterA, string operatorString, string filterB)
        {
            return string.Format(CultureInfo.InvariantCulture, "({0}) {1} ({2})", filterA, operatorString, filterB);
        }

        public static string GenerateFilterCondition(string propertyName, string operation, string givenValue)
        {
            givenValue = (givenValue ?? string.Empty);
            return GenerateFilterCondition(propertyName, operation, givenValue, EdmType.String);
        }

        public static string GenerateFilterConditionForDate(string propertyName, string operation, DateTimeOffset givenValue)
        {
            return GenerateFilterCondition(propertyName, operation, givenValue.UtcDateTime.ToString("o", CultureInfo.InvariantCulture), EdmType.DateTime);
        }

        private static string GenerateFilterCondition(string propertyName, string operation, string givenValue, EdmType edmType)
        {
            string text = null;
            switch (edmType)
            {
                case EdmType.Boolean:
                case EdmType.Int32:
                    text = givenValue;
                    break;
                case EdmType.Double:
                    {
                        text = (int.TryParse(givenValue, out var _) ? string.Format(CultureInfo.InvariantCulture, "{0}.0", givenValue) : givenValue);
                        break;
                    }
                case EdmType.Int64:
                    text = string.Format(CultureInfo.InvariantCulture, "{0}L", givenValue);
                    break;
                case EdmType.DateTime:
                    text = string.Format(CultureInfo.InvariantCulture, "datetime'{0}'", givenValue);
                    break;
                case EdmType.Guid:
                    text = string.Format(CultureInfo.InvariantCulture, "guid'{0}'", givenValue);
                    break;
                case EdmType.Binary:
                    text = string.Format(CultureInfo.InvariantCulture, "X'{0}'", givenValue);
                    break;
                default:
                    text = string.Format(CultureInfo.InvariantCulture, "'{0}'", givenValue.Replace("'", "''"));
                    break;
            }
            return string.Format(CultureInfo.InvariantCulture, "{0} {1} {2}", propertyName, operation, text);
        }
    }

    public enum EdmType
    {
        String,
        Binary,
        Boolean,
        DateTime,
        Double,
        Guid,
        Int32,
        Int64
    }
}
