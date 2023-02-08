using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using TechFlurry.Blazor.MVVM.Utils.Extensions;
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
                                            && t.GetInterfaces().Any(x => x.Name.Contains(nameof(IViewModelBase)))).ToList();


        foreach (var viewModelInterface in viewModelInterfaces)
        {
            //register the viewModelType with their respective implementation
            var implementableClass = viewModelInterface.GetImplementableClasses().FirstOrDefault();
            if (implementableClass is not null)
            {
                services.AddTransient(viewModelInterface, x =>
                {
                    // Resolve any dependencies of the ViewModel
                    var requiredServices = implementableClass.GetConstructors().First().GetParameters();

                    var dependencies = requiredServices.Select(p => x.GetService(p.ParameterType)).ToArray();

                    // Create an instance of the ViewModel and return it

                    var instance = dependencies.Any()
                        ? Activator.CreateInstance(implementableClass, dependencies)
                        : Activator.CreateInstance(implementableClass);

                    return instance.CreateProxy(viewModelInterface);
                });
            }
            else
            {
                viewModelInterfaces.Remove(viewModelInterface);
            }
        }
    }
}
