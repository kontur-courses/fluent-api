using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace ObjectPrinting;

internal class PropertyPath
{
    private static readonly EqualityComparer<object> _referenceComparer = EqualityComparer<object>
        .Create(ReferenceEquals, obj => obj.GetHashCode());

    private readonly ImmutableHashSet<object> _values;
    private readonly int _hashCode;

    public PropertyPath(PropertyValue propertyValue, PropertyPath? previous = null)
    {
        PropertyValue = propertyValue;
        Previous = previous;
        _values = previous?._values ?? ImmutableHashSet.Create<object>(_referenceComparer);

        if (propertyValue.Value != null && !propertyValue.Value.GetType().IsValueType)
        {
            _values = _values.Add(propertyValue.Value);
        }

        _hashCode = HashCode.Combine(propertyValue.DeclaringType, propertyValue.Name, previous);
    }

    public PropertyValue? PropertyValue { get; }

    public PropertyPath? Previous { get; }

    public override bool Equals(object? obj)
    {
        return obj is PropertyPath propertyPath
            && AreEqualPropertyValues(PropertyValue, propertyPath.PropertyValue)
            && (Previous == propertyPath.Previous
                || Previous != null && Previous.Equals(propertyPath.Previous));
    }

    public override int GetHashCode()
    {
        return _hashCode;
    }

    public bool Contains(object? obj)
    {
        return obj != null && _values.Contains(obj);
    }

    public static bool TryGetPropertyPath<TOwner, TPropType>(
        Expression<Func<TOwner, TPropType>> propertySelector,
        [MaybeNullWhen(false)] out PropertyPath path)
    {
        var propertyNames = propertySelector.Body.ToString().Split('.');
        var type = typeof(TOwner);
        path = default;

        if (propertyNames.Length >= 1 && propertyNames[0] == propertySelector.Parameters.Single().Name)
        {
            path = new PropertyPath(new PropertyValue(null, null), null);
        }
        else
        {
            path = default;
            return false;
        }

        for (var i = 1; i < propertyNames.Length; i++) 
        {
            var property = type.GetProperty(propertyNames[i]);

            if (property == null)
            {
                path = default;
                return false;
            }

            type = property.PropertyType;
            path = new PropertyPath(new PropertyValue(property, null), path);
        }

        return true;
    }

    private static bool AreEqualPropertyValues(PropertyValue? property1, PropertyValue? property2)
    {
        return property1 == property2
            || property1 != null && property2 != null
                && property1.DeclaringType == property2.DeclaringType
                && property1.Name == property2.Name;
    }
}
