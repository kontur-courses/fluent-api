using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>: ISettings
    {
        private readonly Settings settings;
        Settings ISettings.Settings => settings;

        public PrintingConfig()
        {
            settings = new Settings();
        }

        public TypePrintingConfig<TOwner, TPropType> Serialize<TPropType>()
        {
            return new TypePrintingConfig<TOwner, TPropType>(this);
        }

        public PropertySerializationConfig<TOwner, TPropType> Serialize<TPropType>(Expression<Func<TOwner, TPropType>> propertyFunc)
        {
            var memberExpression = (MemberExpression) propertyFunc.Body;
            var result = new PropertySerializationConfig<TOwner, TPropType>(this, memberExpression.Member.Name);

            return result;
        }

        public PrintingConfig<TOwner> Exclude<TPropType>()
        {
            settings.AddTypeToExclude(typeof(TPropType));

            return this;
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
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

            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                sb.Append(identation + propertyInfo.Name + " = " +
                          PrintToString(propertyInfo.GetValue(obj),
                              nestingLevel + 1));
            }
            return sb.ToString();
        }
    }
}