using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TechFlurry.Blazor.MVVM.ViewModels;

public interface IViewModelBase : INotifyPropertyChanged, IDisposable, IAsyncDisposable
{
}

public abstract class ViewModelBase : IViewModelBase
{
    public ViewModelBase()
    {
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    public virtual void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    internal protected void RaisePropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    internal protected abstract Task GetUpdateAsync(string propertyName);

    public virtual async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
    }
}
