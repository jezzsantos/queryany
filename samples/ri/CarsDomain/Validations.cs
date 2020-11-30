using Domain.Interfaces;

namespace CarsDomain
{
    public static class Validations
    {
        public static class Car
        {
            public static readonly ValidationFormat Jurisdiction = new ValidationFormat(@"^[\d\w\-\. ]{1,50}$", 1, 50);
            public static readonly ValidationFormat Number = new ValidationFormat(@"^[\d\w ]{1,15}$", 1, 15);
        }
    }
}