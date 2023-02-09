using Blazor.MVVM.Tests.SetupTests;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.ComponentModel;
using TechFlurry.Blazor.MVVM;
using TechFlurry.Blazor.MVVM.ViewModels;
using TechFlurry.Blazor.MVVM.Views;

namespace Blazor.MVVM.Tests.ViewTests;

[TestFixture]
public class ViewBaseTests
{
    private Bunit.TestContext _ctx;
    private IRenderedComponent<TestViewBase> _viewBase;

    [SetUp]
    public void SetUp()
    {
        _ctx = new Bunit.TestContext();
        _ctx.Services.AddBlazorMVVM();
        _viewBase = _ctx.RenderComponent<TestViewBase>();
    }

    [Test]
    public void TestConstruction()
    {
        // Assert
        Assert.That(_viewBase, Is.Not.Null);
    }

    [Test]
    public void ViewBase_ShouldCreateInstanceWithGivenTViewModel()
    {
        // Assert
        Assert.That(_viewBase.Instance.ViewModelContext, Is.InstanceOf<IViewModelA>());
    }

    [Test]
    public void ViewBase_OnInitialized_ShouldCallBindEvents()
    {
        // Assert
        Assert.That(_viewBase.Instance.BindEventsCalled, Is.True);
    }

    [Test]
    public void ViewBase_OnPropertyChanged_ShouldCallStateHasChanged()
    {
        //Arrange
        _viewBase.Instance.ViewRendered = false;

        // Act
        var viewModel = _viewBase.Instance.ViewModelContext;
        //viewModel.A = 10;
        viewModel.ChangePropA(10);
        // Assert
        Assert.That(_viewBase.Instance.ViewRendered, Is.True);
    }

    [Test]
    public void ViewBase_BindEvents_ShouldBindEvents()
    {
        // Arrange
        var viewModelPropValue = 15;
        var viewModel = _viewBase.Instance.ViewModelContext;
        _viewBase.Instance.ViewRendered = false;
        // Act
        viewModel.A = 15;

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(_viewBase.Instance.ViewRendered, Is.True);
            Assert.That(_viewBase.Instance.ViewModelContext.A, Is.EqualTo(viewModelPropValue));
        });
    }

    private class TestViewBase : ViewBase<IViewModelA>
    {
        public bool BindEventsCalled { get; set; } = false;
        public bool ViewRendered { get; set; } = false;

        public int A { get => ViewModelContext.A; set => ViewModelContext.A = value; }
        protected override void BindEvents()
        {
            base.BindEvents();
            BindEventsCalled = true;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            ViewRendered = true;
        }
    }

}
