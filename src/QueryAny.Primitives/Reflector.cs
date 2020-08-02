using System;
using System.Linq.Expressions;
using System.Reflection;
using QueryAny.Primitives.Properties;

namespace QueryAny.Primitives
{
    public static class Reflector<TTarget>
    {
        /// <summary>
        ///     Returns whether the name of the property represented by the lambda expression is valid
        /// </summary>
        public static bool IsValidPropertyName<TResult>(Expression<Func<TTarget, TResult>> property)
        {
            LambdaExpression expression = property;
            if (property.Body.NodeType == ExpressionType.Convert && property.Body.Type == typeof(object))
            {
                expression = Expression.Lambda(((UnaryExpression) property.Body).Operand, property.Parameters);
            }

            if (expression.Body.NodeType != ExpressionType.MemberAccess)
            {
                return false;
            }

            var info = GetMemberInfo(expression) as PropertyInfo;

            return info != null;
        }

        /// <summary>
        ///     Gets the name of the property represented by the lambda expression.
        /// </summary>
        /// <param name="property"> An expression that accesses a property. </param>
        /// <exception cref="ArgumentNullException"> The <paramref name="property" /> is null. </exception>
        /// <exception cref="ArgumentException">
        ///     The <paramref name="property" /> is not a lambda expression or it does not
        ///     represent a property access.
        /// </exception>
        /// <returns> The property info. </returns>
        public static string GetPropertyName<TResult>(Expression<Func<TTarget, TResult>> property)
        {
            return GetProperty(property).Name;
        }

        private static PropertyInfo GetProperty<TResult>(Expression<Func<TTarget, TResult>> property)
        {
            var info = GetMemberInfo(property) as PropertyInfo;
            if (info == null)
            {
                throw new ArgumentException(Resources.Reflector_ErrorNotProperty);
            }

            return info;
        }

        private static MemberInfo GetMemberInfo(LambdaExpression lambda)
        {
            lambda.GuardAgainstNull(nameof(lambda));

            if (lambda.Body.NodeType == ExpressionType.Convert && lambda.Body.Type == typeof(object))
            {
                var expression = Expression.Lambda(((UnaryExpression) lambda.Body).Operand, lambda.Parameters);
                if (expression.NodeType == ExpressionType.Lambda)
                {
                    return ((MemberExpression) expression.Body).Member;
                }
            }

            if (lambda.Body.NodeType == ExpressionType.MemberAccess)
            {
                return ((MemberExpression) lambda.Body).Member;
            }

            throw new ArgumentException(Resources.Reflector_ErrorNotMemberAccess, nameof(lambda));
        }
    }
}