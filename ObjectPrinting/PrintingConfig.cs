using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private static readonly Type[] finalTypes = new[]
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan)
        };
        
        private ConfigInfo configInfo;

        public PrintingConfig()
        {
            configInfo = new ConfigInfo();
        }
        
        public PrintingConfig(ConfigInfo configInfo)
        {
            this.configInfo = configInfo;
        }
        
        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        public PrintingConfig<TOwner> Exclude<TProperty>()
        {
            return new PrintingConfig<TOwner>(configInfo.AddToExcluding(typeof(TProperty)));
        }
        
        public PrintingConfig<TOwner> Exclude<TProperty>(
            Expression<Func<TOwner, TProperty>> selectorExpression)
        {
            var propertyToExclude = (PropertyInfo)((MemberExpression) selectorExpression.Body).Member;
            return new PrintingConfig<TOwner>(configInfo.AddToExcluding(propertyToExclude));
        }

        public PropertyPrintingConfig<TOwner, TProperty> Printing<TProperty>()
        {
            return new PropertyPrintingConfig<TOwner, TProperty>(this);
        }

        public PropertyPrintingConfig<TOwner, TProperty> Printing<TProperty>(
            Expression<Func<TOwner, TProperty>> selectorExpression)
        {
            var propertyToExclude = (PropertyInfo)((MemberExpression) selectorExpression.Body).Member;
            return new PropertyPrintingConfig<TOwner, TProperty>(this, propertyToExclude);
        }
        
        // Я сделал данный метод internal для того, чтобы его не было видно вне проекта.
        // Для того, чтобы его использовать, необходимо сперва вызвать Printing, затем Using (или что-то другое)
        // Разработчику, который будет использовать этот класс в дальнейшем, не стоит показывать лишние методы
        internal PrintingConfig<TOwner> SetCustomSerialization<TProperty>(Func<TProperty, string> serialization,
            PropertyInfo propertyToCustomize)
        {
            if (propertyToCustomize != null)
                return new PrintingConfig<TOwner>(configInfo
                    .SetCustomSerialization(serialization, propertyToCustomize));
            return new PrintingConfig<TOwner>(configInfo
                .SetCustomSerialization(serialization));
        }
        
        private string PrintToString(object obj, int nestingLevel)
        {
            //TODO apply configurations
            if (obj == null)
                return "null" + Environment.NewLine;
            
            if (finalTypes.Contains(obj.GetType()))
                return obj + Environment.NewLine;

            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                if (configInfo.ShouldToExclude(propertyInfo))
                    continue;
                
                var serialization = configInfo.GetSpecialSerialization(propertyInfo);

                sb.Append(identation + propertyInfo.Name + " = ");
                
                if (serialization != null)
                    sb.Append(serialization
                        .DynamicInvoke(propertyInfo.GetValue(obj)) + Environment.NewLine);
                else
                    sb.Append(PrintToString(propertyInfo.GetValue(obj),
                                  nestingLevel + 1));
            }
            return sb.ToString();
        }
    }
}