using Microsoft.Extensions.DependencyInjection;
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