using System;
using System.Linq.Expressions;

namespace ObjectPrinting
{
    public interface IPropertyConfig
    {
        Func<object, string> Func { get; set; }
        public IPrintingConfig Father { get; }
        public Type Type { get; }
        public string PropertyName { get; }
    }
    public class PropertyConfig<TOwner, T> : IPropertyConfig
    {
        public string PropertyName { get; }
        public IPrintingConfig Father { get; }
        public Type Type { get; }
        public Func<object, string> Func { get; set; }

        public PrintingConfig<TOwner> Using(Func<object, string> func)
        {
            this.Func = func;
            return (PrintingConfig<TOwner>)Father;
        }

        public PropertyConfig(PrintingConfig<TOwner> father)
        {
            this.Father = father;
            this.Type = typeof(T);
        }

        public PropertyConfig(PrintingConfig<TOwner> father, Expression<Func<TOwner, T>> expression)
        {
            this.Father = father;
            this.Type = typeof(T);
            var strExpressionSplitted = expression.Body.ToString().Split('.');
            this.PropertyName = strExpressionSplitted[^1];
        }
    }
}
