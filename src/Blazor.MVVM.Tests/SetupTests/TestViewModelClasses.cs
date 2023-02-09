﻿using TechFlurry.Blazor.MVVM.Attributes;
using TechFlurry.Blazor.MVVM.ViewModels;

namespace Blazor.MVVM.Tests.SetupTests;


public interface IViewModelA : IViewModelBase
{
    [BroadcastState]
    int A { get; set; }

    [BroadcastPropertyState(nameof(A))]
    void ChangePropA(int value);
}
public class ViewModelA : ViewModelBase, IViewModelA
{
    public ViewModelA() : base() { }

    public int A { get; set; }

    public void ChangePropA(int value) => A = value;
}

public interface IViewModelB : IViewModelBase { }

public class ViewModelB : ViewModelBase, IViewModelB
{
    public ViewModelB() : base() { }
}

public interface IViewModelC : IViewModelBase
{
    IViewModelA ViewModelA { get; }
}

public class ViewModelC : ViewModelBase, IViewModelC
{

    public ViewModelC(IViewModelA viewModelA)
    {
        ViewModelA = viewModelA;
    }

    public virtual IViewModelA ViewModelA { get; }
}
