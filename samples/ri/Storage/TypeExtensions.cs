using System;
using Domain.Interfaces.Entities;

namespace Storage
{
    public static class TypeExtensions
    {
        public static bool IsComplexStorageType(this Type type)
        {
            if (type == typeof(string)
                || type.IsEnum || type.IsNullableEnum()
                || type == typeof(DateTime) || type == typeof(DateTime?)
                || type == typeof(DateTimeOffset) || type == typeof(DateTimeOffset?)
                || type == typeof(bool) || type == typeof(bool?)
                || type == typeof(int) || type == typeof(int?)
                || type == typeof(long) || type == typeof(long?)
                || type == typeof(double) || type == typeof(double?)
                || type == typeof(byte[])
                || type == typeof(Guid) || type == typeof(Guid?)
                || typeof(IPersistableValueObject).IsAssignableFrom(type)
            )
            {
                return false;
            }

            return true;
        }

        public static bool IsNullableEnum(this Type type)
        {
            return Nullable.GetUnderlyingType(type)?.IsEnum == true;
        }

        public static object ParseNullable(this Type type, string value)
        {
            var enumType = Nullable.GetUnderlyingType(type);
            if (enumType == null)
            {
                throw new ArgumentNullException();
            }
            return Enum.Parse(enumType, value, true);
        }
    }
}