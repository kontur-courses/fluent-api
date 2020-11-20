using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using ObjectPrinting.Configuration;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        public static PrintingConfig<TOwner> Default => new PrintingConfig<TOwner>();

        public SelectedPropertyGroup<TOwner, TProperty> Choose<TProperty>()
        {
            return new SelectedPropertyGroup<TOwner, TProperty>(this);
        }

        public SelectedProperty<TOwner, TProperty> Choose<TProperty>(
            Expression<Func<TOwner, TProperty>> selector)
        {
            var memberInfo = selector.Body as MemberExpression;
            var propertyInfo = memberInfo?.Member as PropertyInfo;
            if (propertyInfo == null)
                throw new ArgumentException($"{nameof(selector)} must point on a Property");

            return new SelectedProperty<TOwner, TProperty>(propertyInfo, this);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            //TODO apply configurations
            if (obj == null)
                return "null" + Environment.NewLine;

            var finalTypes = new[]
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan)
            };
            if (finalTypes.Contains(obj.GetType()))
                return obj + Environment.NewLine;

            var indentation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                sb.Append(indentation + propertyInfo.Name + " = " +
                          PrintToString(propertyInfo.GetValue(obj),
                              nestingLevel + 1));
            }

            return sb.ToString();
        }
    }
}