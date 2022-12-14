using Microsoft.AspNetCore.Components;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using TechFlurry.Blazor.MVVM.Utils.Extensions;
using TechFlurry.Blazor.MVVM.ViewModels;

namespace TechFlurry.Blazor.MVVM.Views
{
    //Add generic type to the base class to get the ViewModel type and implement disposable pattern
    public abstract class ViewBase<TViewModel> : ComponentBase, IDisposable, IAsyncDisposable where TViewModel : IViewModelBase
    {
        private readonly Type _viewModelType, _viewType;
        private readonly Dictionary<string, bool> _propertyFlags = new Dictionary<string, bool>();

        public ViewBase()
        {
            _viewModelType = typeof(TViewModel);
            _viewType = GetType();
        }


        [Inject]
        public TViewModel? ViewModelContext { get; set; }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            //intercept the property setters of the ViewModelContext
            //_viewModelType.InterceptPropertySetters();
            BindEvents();
        }

        public void Dispose()
        {
            UnBindEvents();
        }

        protected virtual void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //checks whether the property change call should not be generated from this view
            if (!_propertyFlags.ContainsKey(e.PropertyName) || !_propertyFlags[e.PropertyName])
            {
                HandlePropertyChanged(e.PropertyName);
                StateHasChanged();
            }
            else if (_propertyFlags.ContainsKey(e.PropertyName))
            {
                _propertyFlags[e.PropertyName] = false;
            }
        }

        //method to bind the property of the view model to the property of the view
        private void Bind(string viewModelPropertyName, string viewPropertyName)
        {
            //get the property of the view model
            var viewModelProperty = _viewModelType.GetProperty(viewModelPropertyName);
            //get the property of the view
            var viewProperty = _viewType.GetProperty(viewPropertyName);
            //get the value of the property of the view model
            var viewPropertyValue = viewProperty.GetValue(this);
            //set the value of the property of the view
            var setter = viewModelProperty.GetSetMethod();
            setter.Invoke(ViewModelContext, new[ ] { viewPropertyValue });
        }

        //private method to bind all the properties of the view model to the properties of the view with the same names
        private void BindAll()
        {
            var viewModelProperties = _viewModelType.GetPublicProperties();
            var viewProperties = _viewType.GetPublicProperties();
            foreach (var viewModelProperty in viewModelProperties)
            {
                var viewProperty = viewProperties.FirstOrDefault(x => x.Name == viewModelProperty.Name);
                if (viewProperty != null)
                {
                    Bind(viewModelProperty.Name, viewProperty.Name);
                }
            }
        }

        protected void BindSetterWithViewModel([CallerMemberName] string propertyName = null)
        {
            if (!string.IsNullOrEmpty(propertyName))
            {
                //adds a flag to give the information that the event call has been generated from the view
                if (!_propertyFlags.TryAdd(propertyName, true))
                {
                    _propertyFlags[propertyName] = true;
                }
                Bind(propertyName, propertyName);
            }
        }

        //method to handle the property changed event of the view model
        protected void HandlePropertyChanged(string propertyName)
        {
            //get the property of the view model
            var viewModelProperty = _viewModelType.GetProperty(propertyName);
            //get the value of the property of the view model
            var viewModelPropertyValue = viewModelProperty.GetValue(ViewModelContext);
            //get the property of the view
            var viewProperty = _viewType.GetProperty(propertyName);
            //set the value of the property of the view
            viewProperty.SetValue(this, viewModelPropertyValue);
        }
        //method to bind events
        protected virtual void BindEvents()
        {
            ViewModelContext.PropertyChanged += OnPropertyChanged;
        }
        //method to unbind events
        protected virtual void UnBindEvents()
        {
            ViewModelContext.PropertyChanged -= OnPropertyChanged;
        }

        public virtual ValueTask DisposeAsync()
        {
            return new ValueTask(Task.CompletedTask);
        }
    }
}
