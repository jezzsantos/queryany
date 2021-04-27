using System.Reflection;

namespace Api.Common
{
    public static class ReflectionExtensions
    {
        public static string GetAssemblyFileVersion(this object instance)
        {
            var assembly = instance.GetType().Assembly;
            var version = assembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version;
            return version;
        }
    }
}