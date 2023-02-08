using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechFlurry.Blazor.MVVM;
using TechFlurry.Blazor.MVVM.ViewModels;

namespace Blazor.MVVM.Tests.SetupTests;

[TestFixture]
public class AddBlazorMVVMTests
{

    private IServiceCollection _services;

    [SetUp]
    public void SetUp()
    {
        _services = new ServiceCollection();
    }

    [Test]
    public void TestAddBlazorMVVM_Positive()
    {
        // Arrange
        var definedViewModelsCount = 3;

        // Act
        _services.AddBlazorMVVM();

        // Assert
        Assert.AreEqual(definedViewModelsCount, _services.Count);
    }
    [Test]
    public void AddBlazorMVVM_DependenciesAreCorrectlyResolved()
    {
        // Arrange
        IViewModelA expectedService;

        // Act
        _services.AddBlazorMVVM();
        var serviceCalled = _services.BuildServiceProvider().GetService<IViewModelC>();

        // Assert
        Assert.True(serviceCalled.ViewModelA is IViewModelA);
    }
    [Test]
    public void AddBlazorMVVM_WhenCalledWithValidAssembly_ShouldRegisterAllViewModelInterfaces()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddBlazorMVVM();
        services.BuildServiceProvider();

        // Assert
        var registeredInterfaces = services.Where(x => x.ServiceType.IsInterface
                                                      && x.ServiceType.GetInterfaces()
                                                      .Any(y => y.Name.Contains(nameof(IViewModelBase)))).ToList();
        Assert.IsNotEmpty(registeredInterfaces);
    }
}

public interface IViewModelA : IViewModelBase { }

public class ViewModelA : ViewModelBase, IViewModelA
{
    public ViewModelA() : base() { }

    public override bool Equals(IViewModelBase? other)
    {
        throw new NotImplementedException();
    }
}

public interface IViewModelB : IViewModelBase { }

public class ViewModelB : ViewModelBase, IViewModelB
{
    public ViewModelB() : base() { }

    public override bool Equals(IViewModelBase? other)
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

    public override bool Equals(IViewModelBase? other)
    {
        throw new NotImplementedException();
    }
}
