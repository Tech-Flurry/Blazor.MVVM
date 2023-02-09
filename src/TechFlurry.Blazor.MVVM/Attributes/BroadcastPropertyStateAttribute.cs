namespace TechFlurry.Blazor.MVVM.Attributes;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
public class BroadcastPropertyStateAttribute : Attribute
{
    private readonly string _propertyName;

    public BroadcastPropertyStateAttribute(string propertyName)
    {
        _propertyName = propertyName;
    }

    internal string PropertyName => _propertyName;
}
