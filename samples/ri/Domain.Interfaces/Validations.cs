using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Common;
using TimeZoneConverter;

namespace Domain.Interfaces
{
    public static class Validations
    {
        private static readonly string FreeFormTextAllowedCharacters =
            @"\d\w\`\~\!\@\#\$\%\:\&\*\(\)\-\+\=\[\]\{{\}}\:\;\'\""\<\,\>\.\?\|\/ \r\n";
        private static readonly string EmojiCharacters =
            "😀😁😂😃😉😋😎😍😗🤗🤔😣😫😴😌🤓😛😜😠😇😷😈👻😺😸😹😻😼😽🙀🙈🙉🙊👼👮🕵💂👳🎅👸👰👲🙍🙇🚶🏃💃⛷🏂🏌🏄🚣🏊⛹🏋🚴👫💪👈👉👆🖕👇🖖🤘🖐👌👍👎✊👊👏🙌🙏🐵🐶🐇🐥🐸🐌🐛🐜🐝🍉🍄🍔🍤🍨🍪🎂🍰🍾🍷🍸🍺🌍🚑⏰🌙🌝🌞⭐🌟🌠🌨🌩⛄🔥🎄🎈🎉🎊🎁🎗🏀🏈🎲🔇🔈📣🔔🎵🎷💰🖊📅✅❎💯";
        public static readonly ValidationFormat Email =
            new ValidationFormat(
                @"^(?:[\w\!\#\$\%\&\'\*\+\-\/\=\?\^\`\{\|\}\~]+\.)*[\w\!\#\$\%\&\'\*\+\-\/\=\?\^\`\{\|\}\~]+@(?:(?:(?:[a-zA-Z0-9](?:[a-zA-Z0-9\-](?!\.)){0,61}[a-zA-Z0-9]?\.)+[a-zA-Z0-9](?:[a-zA-Z0-9\-](?!$)){0,61}[a-zA-Z0-9]?)|(?:\[(?:(?:[01]?\d{1,2}|2[0-4]\d|25[0-5])\.){3}(?:[01]?\d{1,2}|2[0-4]\d|25[0-5])\]))$");
        public static readonly ValidationFormat OrganisationName = DescriptiveName(2);
        public static readonly ValidationFormat Timezone = new ValidationFormat(timezone =>
        {
#if TESTINGONLY
            if (timezone == Timezones.Test)
            {
                return true;
            }
#endif
            return TZConvert.KnownIanaTimeZoneNames.FirstOrDefault(tz => tz.EqualsIgnoreCase(timezone)).Exists();
        });

        public static readonly ValidationFormat Url =
            new ValidationFormat(s => Uri.IsWellFormedUriString(s, UriKind.Absolute));
        public static readonly ValidationFormat Identifier =
            new ValidationFormat(@"^[\w]{1,20}_[\d\w]{10,22}$", 12, 43);
        public static readonly ValidationFormat IdentifierPrefix = new ValidationFormat(@"^[^\W_]*$", 1, 20);

        public static bool Matches<TValue>(this ValidationFormat<TValue> format, TValue value)
        {
            if (format.Function.Exists())
            {
                return format.Function(value);
            }

            if (IsInvalidLength(format, value))
            {
                return false;
            }

            return Regex.IsMatch(value.ToString(), format.Expression);
        }

        public static ValidationFormat DescriptiveName(int min = 1, int max = 100)
        {
            return new ValidationFormat(@"^[\d\w\`\!\@\#\$\%\&\(\)\-\:\;\'\,\.\?\/ ]*$", min, max);
        }

        public static ValidationFormat FreeformText(int min = 1, int max = 1000)
        {
            return
                new ValidationFormat(
                    @$"^[${FreeFormTextAllowedCharacters}]*$", min, max);
        }

        public static ValidationFormat Markdown(int min = 1, int max = 1000)
        {
            return
                new ValidationFormat(
                    $@"^[${FreeFormTextAllowedCharacters}${EmojiCharacters}]*$", min, max);
        }

        public static ValidationFormat Anything(int min = 1, int max = 100)
        {
            return new ValidationFormat(@".*", min, max);
        }

        private static bool IsInvalidLength<TValue>(ValidationFormat<TValue> format, TValue value)
        {
            if (format.MinLength.HasValue && value.ToString().Length < format.MinLength.Value)
            {
                return true;
            }

            if (format.MaxLength.HasValue && value.ToString().Length > format.MaxLength.Value)
            {
                return true;
            }

            return false;
        }

        public static class Password
        {
            public static int MinLength = 8;
            public static int MaxLength = 200;

            /// <summary>
            ///     Strict policy requires that the password contains at least 3 of the 4 character classes, and matches length
            ///     requirements.
            ///     The three character classes are:
            ///     1. at least one uppercase character (including unicode)
            ///     2. at least one lowercase character (including unicode)
            ///     3. at least one number character (ie. 0123456789 )
            ///     4. at least one special character (ie: <![CDATA[`~!@#$%^&*()-_=+[{]}\;:'",<.>/?]]> )
            /// </summary>
            public static readonly ValidationFormat Strict = new ValidationFormat(password =>
            {
                if (!password.HasValue())
                {
                    return false;
                }

                if (password.Length < MinLength)
                {
                    return false;
                }
                if (password.Length > MaxLength)
                {
                    return false;
                }

                var characterClassCount = 0;
                if (Regex.IsMatch(password, @"[\d]{1,}"))
                {
                    characterClassCount++;
                }
                if (Regex.IsMatch(password, @"[\p{Ll}]{1,}"))
                {
                    characterClassCount++;
                }
                if (Regex.IsMatch(password, @"[\p{Lu}]{1,}"))
                {
                    characterClassCount++;
                }
                if (Regex.IsMatch(password, @"[ \!""\#\$\%\&\'\(\)\*\+\,\-\.\/\:\;\<\=\>\?\@\[\]\^_\`\{\|\}\~]{1,}"))
                {
                    characterClassCount++;
                }

                return characterClassCount >= 3;
            });

            /// <summary>
            ///     Loose policy requires that the password contains any character, and matches length
            ///     requirements.
            /// </summary>
            public static readonly ValidationFormat Loose = new ValidationFormat(
                @"^[\w\d \!""\#\$\%\&\'\(\)\*\+\,\-\.\/\:\;\<\=\>\?\@\[\]\^_\`\{\|\}\~]*$", MinLength, MaxLength);
        }
    }

    public class ValidationFormat : ValidationFormat<string>
    {
        public ValidationFormat(string expression, int? minLength = null, int? maxLength = null,
            IEnumerable<string> substitutions = null) : base(expression, minLength, maxLength, substitutions)
        {
        }

        public ValidationFormat(Func<string, bool> predicate) : base(predicate)
        {
        }
    }

    public class ValidationFormat<TValue>
    {
        public ValidationFormat(string expression, int? minLength = null, int? maxLength = null,
            IEnumerable<string> substitutions = null)
        {
            expression.GuardAgainstNull(nameof(expression));

            Expression = expression;
            MinLength = minLength;
            MaxLength = maxLength;
            Substitutions = substitutions ?? new List<string>();
        }

        public ValidationFormat(Func<TValue, bool> predicate)
        {
            predicate.GuardAgainstNull(nameof(predicate));

            Function = predicate;
            Expression = null;
            MinLength = null;
            MaxLength = null;
            Substitutions = new List<string>();
        }

        public Func<TValue, bool> Function { get; }

        public string Expression { get; }

        public int? MaxLength { get; }

        public int? MinLength { get; }

        private IEnumerable<string> Substitutions { get; }

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