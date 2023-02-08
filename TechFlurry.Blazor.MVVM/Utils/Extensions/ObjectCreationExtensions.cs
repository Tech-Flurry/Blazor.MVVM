using Castle.DynamicProxy;
using TechFlurry.Blazor.MVVM.Infrastructure;
using TechFlurry.Blazor.MVVM.ViewModels;

namespace TechFlurry.Blazor.MVVM.Utils.Extensions;
public static class ObjectCreationExtensions
{
    internal static object CreateProxy<T>(this T instance, Type @interface) => new ProxyGenerator().CreateInterfaceProxyWithTargetInterface(@interface, instance, new ViewModelInterceptor(instance as ViewModelBase));

    internal static IEnumerable<Type> GetImplementableClasses(this Type viewModelType) => AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes()).Where(p => viewModelType.IsAssignableFrom(p) && p.IsClass);
}
