using Domain.Interfaces;
using PhoneNumbers;

namespace PersonsDomain
{
    public static class Validations
    {
        public static class Person
        {
            public static readonly ValidationFormat Name = Domain.Interfaces.Validations.DescriptiveName();

            public static readonly ValidationFormat PhoneNumber = new ValidationFormat(value =>
            {
                if (!value.HasValue())
                {
                    return false;
                }

                if (!value.StartsWith("+"))
                {
                    return false;
                }

                var util = PhoneNumberUtil.GetInstance();
                try
                {
                    var number = util.Parse(value, null);
                    return util.IsValidNumber(number);
                }
                catch (NumberParseException)
                {
                    return false;
                }
            });
        }
    }
}