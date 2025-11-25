using System;
using System.Linq.Expressions;

namespace ObjectPrinting;

internal interface IPropertyPrintingConfigAccessor<TOwner, TPropType>
{
    PrintingConfig<TOwner> ParentConfig { get; }
    Expression<Func<TOwner, TPropType>>? PropertySelector { get; }
    PropertyPath PropertyPath { get; }
}

public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfigAccessor<TOwner, TPropType>
{
    private readonly PrintingConfig<TOwner> config;
    private readonly Expression<Func<TOwner, TPropType>>? selector;
    private readonly PropertyPath propertyPath;

    public PropertyPrintingConfig(PrintingConfig<TOwner> config, Expression<Func<TOwner, TPropType>>? selector)
    {
        this.config = config;
        this.selector = ValidateExpression(selector);
        this.propertyPath = PropertyPath.ConvertFromSelector(selector!); // Не null т.к. валидируем это
    }

    private Expression<Func<TOwner, TPropType>> ValidateExpression(Expression<Func<TOwner, TPropType>>? validatingSelector)
    {
        if (validatingSelector == null)
            throw new ArgumentNullException(nameof(validatingSelector));
        if (validatingSelector.Body is not MemberExpression memberExpression)
            throw new ArgumentException($"{nameof(validatingSelector)} must select property or field");
        if (!IsDirectPropertyAccess(memberExpression, validatingSelector.Parameters[0]))
            throw new ArgumentException("Property selector should only access properties of the parameter, not external variables");
        return validatingSelector;
    }
    
    private bool IsDirectPropertyAccess(Expression? expression, ParameterExpression parameter)
    {
        while (expression is MemberExpression member)
            expression = member.Expression;
        return expression == parameter;
    }
    
    PrintingConfig<TOwner> IPropertyPrintingConfigAccessor<TOwner, TPropType>.ParentConfig => config;

    Expression<Func<TOwner, TPropType>>? IPropertyPrintingConfigAccessor<TOwner, TPropType>.PropertySelector => selector;

    PropertyPath IPropertyPrintingConfigAccessor<TOwner, TPropType>.PropertyPath =>
        propertyPath;
}