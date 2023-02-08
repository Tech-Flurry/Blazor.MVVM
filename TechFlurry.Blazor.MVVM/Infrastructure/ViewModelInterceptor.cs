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
        var property = invocation.Method.DeclaringType?.GetProperty(invocation.Method.Name[4..]);
        var attrs = property?.GetCustomAttributes(typeof(BroadcastStateAttribute), true);

        if (attrs?.Length > 0)
        {
            _viewModel.RaisePropertyChanged(property?.Name);
        }

        invocation.Proceed();
    }
}
