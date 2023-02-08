using System.Reflection;

namespace TechFlurry.Blazor.MVVM.Utils.Extensions;
public static class BindingExtensions
{
    //extension method to get all public properties of the type
    internal static PropertyInfo[ ] GetPublicProperties(this Type type)
    {
        return type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
    }

}
