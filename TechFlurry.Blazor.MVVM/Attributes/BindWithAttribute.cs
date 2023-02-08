namespace TechFlurry.Blazor.MVVM.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class BindWithAttribute : Attribute
{
    public BindWithAttribute(string propertyName)
    {
        PropertyName = propertyName;
    }

    public string PropertyName { get; }
}
