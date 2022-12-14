using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using TechFlurry.Blazor.MVVM.Utils.ReflectionsHandlers;
using TechFlurry.Blazor.MVVM.ViewModels;

namespace TechFlurry.Blazor.MVVM.Utils.Extensions;
public static class BindingExtensions
{
    //extension method to get all public properties of the type
    internal static PropertyInfo[ ] GetPublicProperties(this Type type)
    {
        return type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
    }
    //extension method to bind Add method of List with propertychanged
    public static void Add<T, TParent>(this List<IViewModelBase> list, T item, TParent parent) where T : IViewModelBase where TParent : ViewModelBase
    {
        list.Add(item);
        item.PropertyChanged += (sender, args) => parent.RaisePropertyChanged(args.PropertyName);

        //get name of the calling property
        var callingPropertyName = new StackTrace()?.GetFrame(1)?.GetMethod()?.Name.Replace("set_", string.Empty);

        //call the RaisePropertyChanged method of the parent view model
        parent.RaisePropertyChanged(callingPropertyName);
    }
    //extension method to bind Remove method of List with propertychanged
    public static void Remove<T, TParent>(this List<IViewModelBase> list, IViewModelBase item, TParent parent) where T : IViewModelBase where TParent : ViewModelBase
    {
        item.PropertyChanged -= (sender, args) => parent.RaisePropertyChanged(args.PropertyName);
        list.Remove(item);

        //get name of the calling property
        var callingPropertyName = new StackTrace()?.GetFrame(1)?.GetMethod()?.Name.Replace("set_", string.Empty);

        //call the RaisePropertyChanged method of the parent view model
        parent.RaisePropertyChanged(callingPropertyName);
    }
    //private method to get the type builder for view model class
    private static TypeBuilder GetViewModelTypeBuilder(this Type type)
    {
        AssemblyName assemblyName = type.Assembly.GetName();
        //get name of the type
        var typeName = type.Name;
        //define a module builder with the name of the assembly
        var module = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(assemblyName.Name), AssemblyBuilderAccess.Run).DefineDynamicModule((string?)assemblyName.Name);
        //define a type builder with the name of the type
        var typeBuilder = module.DefineType(typeName, TypeAttributes.Public);
        return typeBuilder;
    }

    //private method to intercept the property setter and raise the PropertyChanged event using type builder
    internal static void InterceptPropertySetters(this Type type)
    {
        //add ViewModelBase type builder
        TypeBuilder viewModelBaseTypeBuilder = type.GetViewModelTypeBuilder();
        var properties = type.GetPublicProperties();
        foreach (var property in properties)
        {
            //get backing field for the interface property
            //var backingField = type.GetField($"<{property.Name}>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
            var backingField = property.GetBackingField();
            if (/*backingField != null*/ true)
            {
                PropertyBuilder propertyBuilder = viewModelBaseTypeBuilder.DefineProperty(property.Name, PropertyAttributes.None, property.PropertyType, null);
                var setter = property.GetSetMethod();
                if (setter != null)
                {
                    MethodBuilder setMethodBuilder = viewModelBaseTypeBuilder.DefineMethod($"set_{property.Name}", MethodAttributes.Public | MethodAttributes.Virtual, null, new Type[ ] { property.PropertyType });
                    var setterMethod = new DynamicMethod($"set_{property.Name}", null, new Type[ ] { property.DeclaringType, property.PropertyType }, property.DeclaringType.Module, true);
                    var il = setMethodBuilder.GetILGenerator();
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldarg_1);
                    il.Emit(OpCodes.Stfld, backingField);
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldstr, property.Name);
                    il.Emit(OpCodes.Callvirt, type.GetMethod(nameof(ViewModelBase.RaisePropertyChanged), BindingFlags.NonPublic | BindingFlags.Instance));
                    il.Emit(OpCodes.Ret);
                    propertyBuilder.SetSetMethod(setMethodBuilder);
                }
            }
        }
    }

}
