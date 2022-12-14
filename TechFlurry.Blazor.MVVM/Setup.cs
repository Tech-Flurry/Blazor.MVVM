using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using TechFlurry.Blazor.MVVM.ViewModels;

namespace TechFlurry.Blazor.MVVM;
public static class Setup
{
    public static void AddBlazorMVVM(this IServiceCollection services)
    {
        //get all interfaces from the calling assembly and of type IViewModelBase
        var viewModelInterfaces = Assembly.GetCallingAssembly()
                                            .GetTypes()
                                            .Where(t => t.IsInterface
                                            && t.GetInterfaces().Any(x=>x.Name.Contains(nameof(IViewModelBase)))).ToList();
        foreach (var viewModelInterface in viewModelInterfaces)
        {
            //register the viewModelType with their respective implementation
            var implementableClass = viewModelInterface.GetImplementableClasses().FirstOrDefault();
            if (implementableClass is not null)
            {
                services.AddTransient(viewModelInterface, implementableClass);
            }
        }
    }

    internal static IEnumerable<Type> GetImplementableClasses(this Type viewModelType)
    {
        return AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes()).Where(p => viewModelType.IsAssignableFrom(p) && p.IsClass);
    }

}
