﻿using System;
using System.Linq.Expressions;
using System.Reflection;

namespace QueryAny
{
    public static class Reflector<TTarget>
    {
        /// <summary>
        /// Gets the method represented by the lambda expression.
        /// </summary>
        /// <param name="method"> An expression that invokes a method. </param>
        /// <exception cref="ArgumentNullException"> The <paramref name="method" /> is null. </exception>
        /// <exception cref="ArgumentException"> The <paramref name="method" /> is not a lambda expression or it does not represent a method invocation. </exception>
        /// <returns> The method info. </returns>
        public static MethodInfo GetMethod(Expression<Action<TTarget>> method)
        {
            return GetMethodInfo(method);
        }

        /// <summary>
        /// Gets the method represented by the lambda expression.
        /// </summary>
        /// <param name="method"> An expression that invokes a method. </param>
        /// <typeparam name="T1"> Type of the first argument. </typeparam>
        /// <exception cref="ArgumentNullException"> The <paramref name="method" /> is null. </exception>
        /// <exception cref="ArgumentException"> The <paramref name="method" /> is not a lambda expression or it does not represent a method invocation. </exception>
        /// <returns> The method info. </returns>
        public static MethodInfo GetMethod<T1>(Expression<Action<TTarget, T1>> method)
        {
            return GetMethodInfo(method);
        }

        /// <summary>
        /// Gets the method represented by the lambda expression.
        /// </summary>
        /// <param name="method"> An expression that invokes a method. </param>
        /// <typeparam name="T1"> Type of the first argument. </typeparam>
        /// <typeparam name="T2"> Type of the second argument. </typeparam>
        /// <exception cref="ArgumentNullException"> The <paramref name="method" /> is null. </exception>
        /// <exception cref="ArgumentException"> The <paramref name="method" /> is not a lambda expression or it does not represent a method invocation. </exception>
        /// <returns> The method info. </returns>
        public static MethodInfo GetMethod<T1, T2>(Expression<Action<TTarget, T1, T2>> method)
        {
            return GetMethodInfo(method);
        }

        /// <summary>
        /// Gets the method represented by the lambda expression.
        /// </summary>
        /// <param name="method"> An expression that invokes a method. </param>
        /// <typeparam name="T1"> Type of the first argument. </typeparam>
        /// <typeparam name="T2"> Type of the second argument. </typeparam>
        /// <typeparam name="T3"> Type of the third argument. </typeparam>
        /// <exception cref="ArgumentNullException"> The <paramref name="method" /> is null. </exception>
        /// <exception cref="ArgumentException"> The <paramref name="method" /> is not a lambda expression or it does not represent a method invocation. </exception>
        /// <returns> The method info. </returns>
        public static MethodInfo GetMethod<T1, T2, T3>(Expression<Action<TTarget, T1, T2, T3>> method)
        {
            return GetMethodInfo(method);
        }

        /// <summary>
        /// Gets the name of the property represented by the lambda expression.
        /// </summary>
        /// <param name="property"> An expression that accesses a property. </param>
        /// <exception cref="ArgumentNullException"> The <paramref name="property" /> is null. </exception>
        /// <exception cref="ArgumentException"> The <paramref name="property" /> is not a lambda expression or it does not represent a property access. </exception>
        /// <returns> The property info. </returns>
        public static string GetPropertyName<TResult>(Expression<Func<TTarget, TResult>> property)
        {
            return GetProperty(property).Name;
        }

        /// <summary>
        /// Gets the property represented by the lambda expression.
        /// </summary>
        /// <param name="property"> An expression that accesses a property. </param>
        /// <exception cref="ArgumentNullException"> The <paramref name="property" /> is null. </exception>
        /// <exception cref="ArgumentException"> The <paramref name="property" /> is not a lambda expression or it does not represent a property access. </exception>
        /// <returns> The property info. </returns>
        public static PropertyInfo GetProperty<TResult>(Expression<Func<TTarget, TResult>> property)
        {
            var info = GetMemberInfo(property) as PropertyInfo;
            if (info == null)
            {
                throw new ArgumentException(Properties.Resources.Reflector_ErrorNotProperty);
            }

            return info;
        }

        /// <summary>
        /// Gets the field represented by the lambda expression.
        /// </summary>
        /// <param name="field"> An expression that accesses a field. </param>
        /// <exception cref="ArgumentNullException"> The <paramref name="field" /> is null. </exception>
        /// <exception cref="ArgumentException"> The <paramref name="field" /> is not a lambda expression or it does not represent a field access. </exception>
        /// <returns> The field info. </returns>
        public static FieldInfo GetField<TResult>(Expression<Func<TTarget, TResult>> field)
        {
            var info = GetMemberInfo(field) as FieldInfo;
            if (info == null)
            {
                throw new ArgumentException(Properties.Resources.Reflector_ErrorNotField);
            }

            return info;
        }

        private static MethodInfo GetMethodInfo(LambdaExpression lambda)
        {
            Guard.AgainstNull(() => lambda, lambda);

            if (lambda.Body.NodeType != ExpressionType.Call)
            {
                throw new ArgumentException(Properties.Resources.Reflector_ErrorNotMethodCall, "lambda");
            }

            return ((MethodCallExpression)lambda.Body).Method;
        }

        private static MemberInfo GetMemberInfo(LambdaExpression lambda)
        {
            Guard.AgainstNull(() => lambda, lambda);

            if (lambda.Body.NodeType == ExpressionType.MemberAccess)
            {
                return ((MemberExpression)lambda.Body).Member;
            }

            throw new ArgumentException(Properties.Resources.Reflector_ErrorNotMemberAccess, nameof(lambda));
        }
    }
}
