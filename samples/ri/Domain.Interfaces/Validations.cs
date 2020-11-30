using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using QueryAny.Primitives;

namespace Domain.Interfaces
{
    public static class Validations
    {
        public static readonly ValidationFormat Email =
            new ValidationFormat(
                @"^(?:[\w\!\#\$\%\&\'\*\+\-\/\=\?\^\`\{\|\}\~]+\.)*[\w\!\#\$\%\&\'\*\+\-\/\=\?\^\`\{\|\}\~]+@(?:(?:(?:[a-zA-Z0-9](?:[a-zA-Z0-9\-](?!\.)){0,61}[a-zA-Z0-9]?\.)+[a-zA-Z0-9](?:[a-zA-Z0-9\-](?!$)){0,61}[a-zA-Z0-9]?)|(?:\[(?:(?:[01]?\d{1,2}|2[0-4]\d|25[0-5])\.){3}(?:[01]?\d{1,2}|2[0-4]\d|25[0-5])\]))$");

        public static bool IsMatchedWith(this ValidationFormat format, string value)
        {
            if (format.Expression.HasValue())
            {
                return Regex.IsMatch(value, format.Expression);
            }

            if (format.Function != null)
            {
                return format.Function(value);
            }

            return false;
        }

        public static ValidationFormat DescriptiveName(int min = 1, int max = 100)
        {
            return
                new ValidationFormat(@"^[\d\w\`\#\(\)\-\'\,\.\/ ]{{{0},{1}}}$".Format(min,
                    max), min, max);
        }
    }

    public class ValidationFormat
    {
        public ValidationFormat(string expression, int minLength = 0, int maxLength = 0,
            IEnumerable<string> substitutions = null)
        {
            Expression = expression;
            MinLength = minLength;
            MaxLength = maxLength;
            Substitutions = substitutions ?? new List<string>();
        }

        public ValidationFormat(Func<string, bool> predicate) : this(null, 0)
        {
            Function = predicate;
        }

        public Func<string, bool> Function { get; }

        public string Expression { get; }

        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        private int MaxLength { get; }

        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        private int MinLength { get; }

        private IEnumerable<string> Substitutions { get; }

        /// <summary>
        ///     Substitutes the given name/values into the expression.
        /// </summary>
        private string Substitute(IDictionary<string, string> values)
        {
            values.GuardAgainstNull(nameof(values));

            var expression = Expression;
            values.ToList()
                .ForEach(val =>
                {
                    if (Substitutions.Contains(val.Key))
                    {
                        expression = expression.Replace(val.Key, val.Value);
                    }
                });

            return expression;
        }

        /// <summary>
        ///     Substitutes the given values into the expression.
        /// </summary>
        /// <remarks>
        ///     Substitutions are performed by index
        /// </remarks>
        public string Substitute(IEnumerable<string> values)
        {
            return Substitute(InitializeSubstitutions(values));
        }

        private IDictionary<string, string> InitializeSubstitutions(IEnumerable<string> values)
        {
            values.GuardAgainstNull(nameof(values));

            var result = new Dictionary<string, string>();

            var substitutions = Substitutions.ToList();
            var counter = 0;
            values.ToList().ForEach(val =>
            {
                if (substitutions.Count > counter)
                {
                    result.Add(substitutions[counter], val);

                    counter++;
                }
            });

            return result;
        }
    }
}