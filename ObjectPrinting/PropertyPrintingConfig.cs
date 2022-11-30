using System;
using System.Collections.Generic;
using System.Globalization;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TParentType, TPropType>
    {
        internal readonly PrintingConfig<TParentType> Parent;
        private string specialSerializationPropertyName;

        public PropertyPrintingConfig(PrintingConfig<TParentType> parent, string specialSerializationPropertyName)
        {
            this.Parent = parent;
            this.specialSerializationPropertyName = specialSerializationPropertyName;
        }

        public PropertyPrintingConfig(PrintingConfig<TParentType> parent) : this(parent, null)
        {
        }

        public string PrintToString(TParentType obj)
        {
            return Parent.PrintToString(obj);
        }

        public PrintingConfig<TParentType> Using(Func<TPropType, string> function)
        {
            if (specialSerializationPropertyName == null)
                Parent.SpecialSerializationForTypes.Add(typeof(TPropType), el => function.Invoke((TPropType) el));
            else
                Parent.SpecialSerializationForNames.Add(specialSerializationPropertyName,
                    el => function.Invoke((TPropType) el));
            return Parent;
        }

        public PrintingConfig<TParentType> Using(CultureInfo cultureInfo)
        {
            Parent.CultureInfos.Add(typeof(TPropType), cultureInfo);
            return Parent;
        }
    }
}