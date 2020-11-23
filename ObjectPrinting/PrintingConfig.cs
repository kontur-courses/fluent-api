using System;
using System.Linq.Expressions;
using System.Reflection;
using ObjectPrinting.Contexts;
using ObjectPrinting.Contracts;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner> : IPrintingConfig
    {
        private PrintExcluder Excluder { get; }
        private AlternativePrinter AlternativePrinter { get; }
        AlternativePrinter IPrintingConfig.AlternativePrinter => AlternativePrinter;
        PrintExcluder IPrintingConfig.PrintExcluder => Excluder;

        public PrintingConfig()
        {
            Excluder = new PrintExcluder();
            AlternativePrinter = new AlternativePrinter();
        }

        private PrintingConfig(PrintExcluder excluder, AlternativePrinter alternativePrinter)
        {
            Excluder = excluder;
            AlternativePrinter = alternativePrinter;
        }

        IPrintingConfig IPrintingConfig.AddAlternativePrintingFor<TContext>(TContext entity, Func<object, string> print)
        {
            return entity switch
            {
                Type type => new PrintingConfig<TOwner>(Excluder, AlternativePrinter.AddOrUpdate(type, print)),
                PropertyInfo prop => new PrintingConfig<TOwner>(Excluder, AlternativePrinter.AddOrUpdate(prop, print)),
                FieldInfo field => new PrintingConfig<TOwner>(Excluder, AlternativePrinter.AddOrUpdate(field, print)),
                _ => throw new ArgumentException($"{nameof(entity)} should be Type, PropertyInfo or FieldInfo!")
            };
        }

        public TypePrintingConfig<TOwner, TPropType> Printing<TPropType>() =>
            new TypePrintingConfig<TOwner, TPropType>(this);

        public IContextPrintingConfig<TOwner, TContext> Printing<TContext>(
            Expression<Func<TOwner, TContext>> memberSelector)
        {
            var property = ((MemberExpression) memberSelector.Body).Member as PropertyInfo;
            var field = ((MemberExpression) memberSelector.Body).Member as FieldInfo;

            if (property is null && field is null)
                throw new ArgumentException($"{memberSelector} should return field of property {nameof(TOwner)}");

            return property is not null
                ? new PropertyPrintingConfig<TOwner, TContext>(this, property) as
                    IContextPrintingConfig<TOwner, TContext>
                : new FieldPrintingConfig<TOwner, TContext>(this, field);
        }

        public PrintingConfig<TOwner> Excluding<TContext>(Expression<Func<TOwner, TContext>> memberSelector)
        {
            var property = ((MemberExpression) memberSelector.Body).Member as PropertyInfo;
            var field = ((MemberExpression) memberSelector.Body).Member as FieldInfo;

            if (property is null && field is null)
                throw new ArgumentException($"{memberSelector} should return field of property {nameof(TOwner)}");

            return property is not null
                ? new PrintingConfig<TOwner>(Excluder.Exclude(property), AlternativePrinter)
                : new PrintingConfig<TOwner>(Excluder.Exclude(field), AlternativePrinter);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>() =>
            new PrintingConfig<TOwner>(Excluder.Exclude(typeof(TPropType)), AlternativePrinter);

        public string PrintToString(TOwner obj)
        {
            var serializer = new Serializer(this);
            return serializer.PrintToString(obj, 0);
        }
    }
}