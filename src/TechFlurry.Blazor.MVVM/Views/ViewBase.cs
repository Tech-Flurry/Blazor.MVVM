﻿using Microsoft.AspNetCore.Components;
using System.ComponentModel;
using TechFlurry.Blazor.MVVM.ViewModels;

namespace TechFlurry.Blazor.MVVM.Views;

public abstract class ViewBase<TViewModel> : ComponentBase, IDisposable, IAsyncDisposable where TViewModel : IViewModelBase
{
    public ViewBase()
    {
    }

    [Inject]
    public TViewModel ViewModelContext { get; set; }

    protected override void OnInitialized()
    {
        BindEvents();
        base.OnInitialized();
    }

    protected virtual async void OnPropertyChangedAsync(object sender, PropertyChangedEventArgs e) => await InvokeAsync(StateHasChanged);

    protected virtual void BindEvents() => ViewModelContext.PropertyChanged += OnPropertyChangedAsync;

    protected virtual void UnBindEvents() => ViewModelContext.PropertyChanged -= OnPropertyChangedAsync;

    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        await DisposeAsyncCore();
        GC.SuppressFinalize(this);
    }
    void IDisposable.Dispose()
    {
        UnBindEvents();
        ViewModelContext.Dispose();
        GC.SuppressFinalize(this);
    }

    protected virtual async ValueTask DisposeAsyncCore() => await ViewModelContext.DisposeAsync().ConfigureAwait(false);
}
