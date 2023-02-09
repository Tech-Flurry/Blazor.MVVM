using Blazor.MVVM.Tests.SetupTests;
using Microsoft.AspNetCore.Components;
using TechFlurry.Blazor.MVVM;
using TechFlurry.Blazor.MVVM.ViewModels;
using TechFlurry.Blazor.MVVM.Views;

namespace Blazor.MVVM.Tests.ViewModelTests;

[TestFixture]
public class ViewModelBaseTests
{
    [Test]
    public void Construction_Test()
    {
        var viewModelBase = new TestViewModelBase();
        Assert.That(viewModelBase, Is.Not.Null);
    }

    [Test]
    public void PropertyChanged_Raised_Test()
    {
        var viewModelBase = new TestViewModelBase();
        var propertyChangedRaised = false;
        viewModelBase.PropertyChanged += (sender, args) => propertyChangedRaised = true;

        viewModelBase.InvokeRaisePropertyChanged();

        Assert.That(propertyChangedRaised, Is.True);
    }

    [Test]
    public void Dispose_Test()
    {
        var viewModelBase = new TestViewModelBase();
        viewModelBase.Dispose();
    }

    [Test]
    public async Task DisposeAsync_Test()
    {
        var viewModelBase = new TestViewModelBase();
        await viewModelBase.DisposeAsync();
    }


    [Test]
    public void PropertyChanged_Correct_Property_Name_Test()
    {
        var viewModelBase = new TestViewModelBase();
        var propertyName = string.Empty;
        const string PROPERTY_NAME = "Property";

        viewModelBase.PropertyChanged += (sender, args) => propertyName = args.PropertyName;

        viewModelBase.InvokeRaisePropertyChanged(PROPERTY_NAME);

        Assert.That(propertyName, Is.EqualTo(PROPERTY_NAME));
    }

    [Test]
    public void View_To_ViewModel_Binding()
    {
        //arrange
        var ctx = new Bunit.TestContext();
        ctx.Services.AddBlazorMVVM();
        const int PROPERTY_VALUE = 20;


        //act
        var viewBase = ctx.RenderComponent<TestViewBase>();
        var viewModelBase = viewBase.Instance.ViewModelContext;

        viewBase.Instance.PropertyA = PROPERTY_VALUE;

        //assert
        Assert.Multiple(() =>
        {
            Assert.That(viewModelBase.UpdatedProperty.name, Is.EqualTo(nameof(IViewModelA.A)));
            Assert.That(viewModelBase.A, Is.EqualTo(PROPERTY_VALUE));
        });
    }

    [Test]
    public void View_ToViewModel_Binding_UsingParameter()
    {
        //arrange
        var ctx = new Bunit.TestContext();
        ctx.Services.AddBlazorMVVM();
        const int PROPERTY_VALUE = 20;

        //act
        var viewBase = ctx.RenderComponent<TestViewBase>(param => param.Add(p => p.PropertyParam, PROPERTY_VALUE.ToString()));
        var viewModelBase = viewBase.Instance.ViewModelContext;

        //assert
        Assert.Multiple(() =>
        {
            Assert.That(viewModelBase.UpdatedProperty.name, Is.EqualTo(nameof(IViewModelA.A)));
            Assert.That(viewModelBase.A, Is.EqualTo(PROPERTY_VALUE));
        });
    }

    private class TestViewModelBase : ViewModelBase
    {
        public void InvokeRaisePropertyChanged(string propertyName = "")
        {
            RaisePropertyChanged(propertyName);
        }

        protected override Task GetUpdateAsync(string propertyName)
        {
            throw new NotImplementedException();
        }
    }

    private class TestViewBase : ViewBase<IViewModelA>
    {
        [Parameter]
        public string PropertyParam { get => ViewModelContext.A.ToString(); set => ViewModelContext.A = Convert.ToInt32(value); }

        public int PropertyA { get => ViewModelContext.A; set => ViewModelContext.A = value; }
    }
}
