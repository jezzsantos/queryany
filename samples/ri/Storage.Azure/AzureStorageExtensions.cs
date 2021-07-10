using System;
using System.Text.RegularExpressions;
using ServiceStack;
using Storage.Azure.Properties;

namespace Storage.Azure
{
    public static class AzureStorageConstants
    {
        public const string StorageNameValidationExpression = @"^[a-z0-9\-]{3,63}$";
        public const string CosmosContainerIdValidationExpression = @"^([a-zA-Z0-9 ]|[^\\\/\#\?]){1,255}$";
    }

    public static class AzureStorageExtensions
    {
        public static string SanitiseAndValidateStorageName(this string name)
        {
            var lowercased = name.ToLowerInvariant();
            ValidateStorageName(lowercased);

            return lowercased;
        }

        private static void ValidateStorageName(string name)
        {
            if (!Regex.IsMatch(name, AzureStorageConstants.StorageNameValidationExpression))
            {
                throw new ArgumentOutOfRangeException(
                    Resources.AzureQueueStorageRepository_InvalidStorageName.Fmt(name));
            }
        }

        public static void ValidateCosmosContainerId(this string name)
        {
            if (!Regex.IsMatch(name, AzureStorageConstants.CosmosContainerIdValidationExpression))
            {
                throw new ArgumentOutOfRangeException(
                    Resources.AzureQueueStorageRepository_InvalidCosmosContainerName.Fmt(name));
            }
        }
    }
}