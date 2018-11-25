using System;
using System.Linq.Expressions;
using System.Reflection;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly ConfigsContainer configsContainer = new ConfigsContainer();

        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            configsContainer.TypesToExclude.Add(typeof(TPropType));
            return this;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(
            Expression<Func<TOwner, TPropType>> propertySelector)
        {
            var propertyInfo = ExtractPropertyInfo(propertySelector);
            configsContainer.PropertiesToExclude.Add(propertyInfo.Name);
            return this;
        }

        private PropertyInfo ExtractPropertyInfo<TPropType>(
            Expression<Func<TOwner, TPropType>> propertySelector)
        {
            var memberExpression = propertySelector.Body as MemberExpression;
            if (memberExpression == null)
                throw new ArgumentException("Delegate is not a member selector");
            var propertyInfo = memberExpression.Member as PropertyInfo;
            if (propertyInfo == null)
                throw new ArgumentException("Delegate selects not a property");
            return propertyInfo;
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            var printingConfig = new PropertyPrintingConfig<TOwner, TPropType>(this);
            configsContainer.PrintersForTypes[typeof(TPropType)] =
                obj => (printingConfig as IPropertyPrintingConfig<TOwner, TPropType>).PrintingFunction((TPropType) obj);
            return printingConfig;
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(
            Expression<Func<TOwner, TPropType>> propertySelector)
        {
            var printingConfig = new PropertyPrintingConfig<TOwner, TPropType>(this);
            var propertyInfo = ExtractPropertyInfo(propertySelector);
            configsContainer.PrintersForProperties[propertyInfo.Name] = 
                obj => ((IPropertyPrintingConfig<TOwner, TPropType>) printingConfig).PrintingFunction((TPropType)obj);

            return printingConfig;
        }

        public string PrintToString(TOwner obj)
        {
            return new PrinterToString(configsContainer).PrintToString(obj);
        }
    }
}