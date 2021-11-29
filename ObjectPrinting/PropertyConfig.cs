using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace ObjectPrinting
{
    public interface IPropertyConfig
    {
        Func<object, string> func { get; set; }
        public IPrintingConfig father { get; set; }
        public Type type { get; set; }
        public string propertyName { get; set; }
    }
    public class PropertyConfig<TOwner, T> : IPropertyConfig
    {
        public string propertyName { get; set; }
        public IPrintingConfig father { get; set; }
        public Type type { get; set; }
        public Func<object, string> func { get; set; }

        public PrintingConfig<TOwner> Using(Func<object, string> func)
        {
            this.func = func;
            return (PrintingConfig<TOwner>)father;
        }

        public PropertyConfig(PrintingConfig<TOwner> father)
        {
            this.father = father;
            this.type = typeof(T);
        }

        public PropertyConfig(PrintingConfig<TOwner> father, Expression<Func<TOwner, T>> expression)
        {
            this.father = father;
            this.type = typeof(T);
            var strExpressionSplitted = expression.Body.ToString().Split('.');
            this.propertyName = strExpressionSplitted[strExpressionSplitted.Length - 1];
        }
    }
}
