using Castle.DynamicProxy;
using TechFlurry.Blazor.MVVM.Attributes;
using TechFlurry.Blazor.MVVM.ViewModels;

namespace TechFlurry.Blazor.MVVM.Infrastructure;
internal class ViewModelInterceptor : IInterceptor
{
    private readonly ViewModelBase _viewModel;

    public ViewModelInterceptor(ViewModelBase viewModel)
    {
        _viewModel = viewModel;
    }

    public void Intercept(IInvocation invocation)
    {
        invocation.Proceed();
        var property = invocation.Method.DeclaringType?.GetProperty(invocation.Method.Name[4..]);
        if (property is not null)
        {
            var attrs = property?.GetCustomAttributes(typeof(BroadcastStateAttribute), true).Cast<BroadcastStateAttribute>();

            if (attrs.Any())
            {
                _viewModel.RaisePropertyChanged(property?.Name);
            }
        }
        else
        {
            var method = invocation.Method.DeclaringType?.GetMethod(invocation.Method.Name);
            var attrs = method?.GetCustomAttributes(typeof(BroadcastPropertyStateAttribute), true).Cast<BroadcastPropertyStateAttribute>();
            if (attrs.Any())
            {
                foreach (var attr in attrs)
                {
                    _viewModel.RaisePropertyChanged(attr.PropertyName);
                }
            }
        }
    }
}
