using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TechFlurry.Blazor.MVVM.ViewModels
{
    public interface IViewModelBase : INotifyPropertyChanged, IDisposable, IAsyncDisposable, IEquatable<IViewModelBase>
    {
    }

    //implement disposable pattern and implement INotifyPropertyChanged and generic type to get the Model type and inject the Model type
    public abstract class ViewModelBase : IViewModelBase
    {
        public ViewModelBase()
        {
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public void Dispose()
        {
            //Model.Dispose ();
        }
        //method to raise the PropertyChanged event
        internal void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        //method to assign the value to the property and raise the PropertyChanged event
        protected void SetMember<T>(ref T storage, T value, [CallerMemberName] string memberName = null)
        {
            if (!EqualityComparer<T>.Default.Equals(storage, value))
            {
                storage = value;
                RaisePropertyChanged(memberName);
            }
        }
        public ValueTask DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }
        public abstract bool Equals(IViewModelBase? other);
        public override bool Equals(object? obj)
        {
            return obj is IViewModelBase ? Equals(obj as IViewModelBase) : base.Equals(obj);
        }
    }
}
