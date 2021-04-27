namespace Domain.Interfaces
{
    public static class ObjectExtensions
    {
        public static bool IsNull(this object instance)
        {
            return instance == null;
        }

        public static bool IsNotNull(this object instance)
        {
            return instance != null;
        }

        public static bool Exists(this object instance)
        {
            return instance.IsNotNull();
        }

        public static bool NotExists(this object instance)
        {
            return instance.IsNull();
        }
    }
}