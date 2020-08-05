﻿using System;
using Domain.Interfaces.Entities;

namespace Storage
{
    public static class TypeExtensions
    {
        public static bool IsComplexStorageType(this Type type)
        {
            if (type == typeof(string)
                || type == typeof(DateTime)
                || type == typeof(DateTimeOffset)
                || type == typeof(bool)
                || type == typeof(int)
                || type == typeof(long)
                || type == typeof(double)
                || type == typeof(byte[])
                || type == typeof(Guid)
                || typeof(IPersistableValueType).IsAssignableFrom(type)
            )
            {
                return false;
            }

            return true;
        }
    }
}