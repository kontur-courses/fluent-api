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
        private readonly Type[] finalTypes;

        public PrintingConfig()
        {
            finalTypes = new[]
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan), typeof(Guid)
            };
        }

        public string PrintToString(TOwner obj) => PrintToString(obj, 0);

        public SelectedPropertyGroup<TOwner, TProperty> Choose<TProperty>()
        {
            const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance;
            var targetsEnumerable = typeof(TOwner)
                .GetFields(bindingFlags)
                .Select(f => new SerializationTarget(f))
                .Union(typeof(TOwner)
                    .GetProperties(bindingFlags)
                    .Select(p => new SerializationTarget(p)));
            return new SelectedPropertyGroup<TOwner, TProperty>(targetsEnumerable, this);
        }

        public SelectedProperty<TOwner, TProperty> Choose<TProperty>(Expression<Func<TOwner, TProperty>> selector)
        {
            var memberInfo = (selector.Body as MemberExpression)?.Member;

            var target = memberInfo switch
            {
                PropertyInfo propertyInfo => new SerializationTarget(propertyInfo),
                FieldInfo fieldInfo => new SerializationTarget(fieldInfo),
                _ => throw new ArgumentException($"{nameof(selector)} must point on a Property")
            };

            return new SelectedProperty<TOwner, TProperty>(target, this);
        }

        private string PrintToString(object? obj, int nestingLevel)
        {
            //TODO apply configurations
            if (obj == null)
                return "null" + Environment.NewLine;

            var type = obj.GetType();
            if (finalTypes.Contains(type))
                return obj.ToString() + Environment.NewLine;

            var indentation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                sb.Append(indentation + propertyInfo.Name + " = " +
                          PrintToString(propertyInfo.GetValue(obj), nestingLevel + 1));
            }

            return sb.ToString();
        }

        public static PrintingConfig<TOwner> Default => new PrintingConfig<TOwner>();
    }
}