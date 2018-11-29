using System;
using System.Globalization;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace ObjectPrinting.Solved
{
    public class PropertyPrintingConfig<TOwner, TPropType> : IPrintingConfig<TOwner, TPropType>,
        IMemberSelector<TOwner, TPropType>
    {
        private readonly Expression<Func<TOwner, TPropType>> memberSelectorExp;
        private readonly PrintingConfig<TOwner> printingConfig;

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig,
            Expression<Func<TOwner, TPropType>> memberSelector)
        {
            this.printingConfig = printingConfig;
            memberSelectorExp = memberSelector;
        }

        Expression<Func<TOwner, TPropType>> IMemberSelector<TOwner, TPropType>.memberSelector
        {
            get { return memberSelectorExp; }
        }

        PrintingConfig<TOwner> IPrintingConfig<TOwner, TPropType>.ParentConfig
        {
            get { return printingConfig; }
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
        {
            var settingsHolder = printingConfig as IPrintingConfigurationHolder;
            var matchCollection = new Regex(string.Format("{0}.(\\w*)", memberSelectorExp.Parameters[0].Name));
            var serializedProperty = matchCollection.Match(memberSelectorExp.Body.ToString()).Value
                .Replace(memberSelectorExp.Parameters[0].Name + ".", "");
            settingsHolder.propertySerializers.Add(serializedProperty, print);
            return printingConfig;
        }

        public PrintingConfig<TOwner> Using(CultureInfo culture)
        {
            return printingConfig;
        }
    }

    public interface IMemberSelector<TOwner, TPropType>
    {
        Expression<Func<TOwner, TPropType>> memberSelector { get; }
    }
}