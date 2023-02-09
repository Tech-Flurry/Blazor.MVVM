using TechFlurry.Blazor.MVVM.Attributes;
using TechFlurry.Blazor.MVVM.ViewModels;

namespace Blazor.MVVM.Tests.SetupTests;


public interface IViewModelA : IViewModelBase
{
    [BroadcastState]
    int A { get; set; }
    (string name, int val) UpdatedProperty { get; }

    [BroadcastPropertyState(nameof(A))]
    void ChangePropA(int value);
}
public class ViewModelA : ViewModelBase, IViewModelA
{
    public (string name, int val) UpdatedProperty { get; private set; }
    public ViewModelA() : base() { }

    public int A { get; set; }

    public void ChangePropA(int value) => A = value;
    protected override async Task GetUpdateAsync(string propertyName)
    {
        UpdatedProperty = (propertyName, A);
    }
}

public interface IViewModelB : IViewModelBase { }

public class ViewModelB : ViewModelBase, IViewModelB
{
    public ViewModelB() : base() { }

    protected override Task GetUpdateAsync(string propertyName)
    {
        throw new NotImplementedException();
    }
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

    protected override Task GetUpdateAsync(string propertyName)
    {
        throw new NotImplementedException();
    }
}
