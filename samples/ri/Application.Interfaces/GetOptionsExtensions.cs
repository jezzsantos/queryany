﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Common;

namespace Application.Interfaces
{
    public static class GetOptionsExtensions
    {
        internal static List<string> ReferencesToNames<TResource>(
            this Expression<Func<TResource, object>>[] propertyReferences)
        {
            return propertyReferences.Safe().Select(ToResourceReference)
                .ToList();
        }

        internal static string ToResourceReference<TResource>(Expression<Func<TResource, object>> propertyReference)
        {
            var propertyName = Reflector<TResource>.GetPropertyName(propertyReference);

            return $"{typeof(TResource).Name}.{propertyName}".ToLower();
        }
    }
}