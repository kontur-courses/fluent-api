using System;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        public SelectedProperty<TOwner, PropertyConfigurator<TProperty>, TProperty> Choose<TProperty>()
        {
            return new SelectedProperty<TOwner, PropertyConfigurator<TProperty>, TProperty>();
        }

        public SelectedProperty<TOwner, PropertyConfigurator<TProperty>, TProperty> Choose<TProperty>(
            Expression<Func<TOwner, TProperty>> selector)
        {
            return new SelectedProperty<TOwner, PropertyConfigurator<TProperty>, TProperty>();
        }

        public SelectedProperty<TOwner, StringPropertyConfigurator, string> Choose(
            Expression<Func<TOwner, string>> selector)
        {
            return new SelectedProperty<TOwner, StringPropertyConfigurator, string>();
        }
        public SelectedProperty<TOwner, NumberPropertyConfigurator, int> Choose(
            Expression<Func<TOwner, int>> selector)
        {
            return new SelectedProperty<TOwner, NumberPropertyConfigurator, int>();
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

    public class SelectedProperty<TOwner, TConfigurator, TProperty>
        where TConfigurator : PropertyConfigurator<TProperty>
    {
        public PrintingConfig<TOwner> Parent { get; }

        public PrintingConfig<TOwner> Using(Action<TConfigurator> configurator)
        {
            return Parent;
        }

        public PrintingConfig<TOwner> Exclude()
        {
            throw new NotImplementedException();
        }

        public PrintingConfig<TOwner> UseSerializer(Func<object, string> func)
        {
            throw new NotImplementedException();
        }

        public PrintingConfig<TOwner> SetCulture(CultureInfo currentCulture)
        {
            throw new NotImplementedException();
        }

        public PrintingConfig<TOwner> Trim(int i)
        {
            throw new NotImplementedException();
        }
    }

    public class PropertyConfigurator<TProperty>
    {
        public void Exclude()
        {
        }

        public PropertyConfigurator<TProperty> UseSerializer(Func<TProperty, string> serializer)
        {
            return this;
        }
    }

    public class StringPropertyConfigurator : PropertyConfigurator<string>
    {
        public StringPropertyConfigurator Substring()
        {
            return this;
        }
    }

    public class NumberPropertyConfigurator : PropertyConfigurator<int>
    {
        public NumberPropertyConfigurator SetCulture(CultureInfo cultureInfo)
        {
            return this;
        }
    }

    public class PersonExtensions
    {
        
    }
}