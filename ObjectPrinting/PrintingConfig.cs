using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting;

public abstract class PrintingConfig<TOwner> : IPrintingConfig<TOwner>, IInternalPrintingConfig<TOwner>
{
    private readonly PrintingConfig<TOwner>? parentConfig;

    protected PrintingConfig(PrintingConfig<TOwner>? parentConfig)
    {
        this.parentConfig = parentConfig;
    }

    PrintingConfig<TOwner>? IInternalPrintingConfig<TOwner>.ParentConfig => parentConfig;


    RootPrintingConfig<TOwner> IInternalPrintingConfig<TOwner>.GetRoot()
    {
        var reference = this;
        while (true)
        {
            if (reference!.parentConfig is null) return (RootPrintingConfig<TOwner>)reference;
            reference = reference.parentConfig;
        }
    }

    public string PrintToString(TOwner obj)
    {
        var stringBuilder = new StringBuilder();
        if (InternalPrintingConfigExtensions.TryReturnNull(obj, out var stringValue))
        {
            stringBuilder.Append(stringValue);
        }
        else
        {
            var cyclicInheritanceIgnoredObjects = new HashSet<object>();
            ((IInternalPrintingConfig<TOwner>)this).GetRoot()
                .PrintToString(obj!, 0, stringBuilder, cyclicInheritanceIgnoredObjects);
        }

        return stringBuilder.ToString();
    }

    public PrintingConfig<TOwner> Excluding<TType>()
    {
        ((IInternalPrintingConfig<TOwner>)this).GetRoot().TypeExcluding.Add(typeof(TType));
        return this;
    }

    public PrintingConfig<TOwner> Excluding<TType>(
        Expression<Func<TOwner, TType>> expression)
    {
        var body = (MemberExpression)expression.Body;
        var memberInfo = body.Member;
        var valid = memberInfo is PropertyInfo || memberInfo is FieldInfo;
        if (!valid)
            throw new ArgumentException("invalid expression", nameof(body));
        ((IInternalPrintingConfig<TOwner>)this).GetRoot().MemberExcluding.Add(memberInfo);
        return this;
    }


    public TypePrintingConfig<TOwner, TType> Printing<TType>()
    {
        return new(((IInternalPrintingConfig<TOwner>)this).GetRoot());
    }

    public PropertyPrintingConfig<TOwner, TType> Printing<TType>(
        Expression<Func<TOwner, TType>> expression)
    {
        return new(expression, ((IInternalPrintingConfig<TOwner>)this).GetRoot());
    }

    // //1. Исключить из сериализации свойства определенного типа
    // .Excluding<Guid>()
    // //2. Указать альтернативный способ сериализации для определенного типа
    // .Printing<int>().Using(i => i.ToString("X"))
    // //3. Для числовых типов указать культуру
    // .Printing<double>().Using(CultureInfo.InvariantCulture)
    //     //4. Настроить сериализацию конкретного свойства
    //     //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
    //     .Printing(p => p.Name).TrimmedToLength(10)
    //     //6. Исключить из сериализации конкретного свойства
    //     .Excluding(p => p.Age);
}