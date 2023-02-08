using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TechFlurry.Blazor.MVVM.ViewModels
{
    public interface IViewModelBase : INotifyPropertyChanged, IDisposable, IAsyncDisposable
    {
    }

    //implement disposable pattern and implement INotifyPropertyChanged and generic type to get the Model type and inject the Model type
    public abstract class ViewModelBase : IViewModelBase
    {
        public ViewModelBase()
        {
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public virtual void Dispose()
        {
            GC.SuppressFinalize(this);
            //Model.Dispose ();
        }
        //method to raise the PropertyChanged event
        internal void RaisePropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public virtual ValueTask DisposeAsync()
        {
            GC.SuppressFinalize(this);
            return ValueTask.CompletedTask;
        }
    }
}
