using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using NUnit.Framework.Constraints;

namespace ObjectPrinting
{
    public class TypePrintingConfig<TOwner, TPropType> : ITypePrintingConfig<TOwner>
    {
        PrintingConfig<TOwner> ITypePrintingConfig<TOwner>.PrintingConfig => printConfig;
        string ITypePrintingConfig<TOwner>.TypeName => typeName;

        private readonly PrintingConfig<TOwner> printConfig;
        private string typeName;


        public TypePrintingConfig(PrintingConfig<TOwner> printConfig, Type type)
        {
            this.typeName = type.FullName;
            this.printConfig = printConfig;
        }

        public TypePrintingConfig(PrintingConfig<TOwner> printConfig, Expression<Func<TOwner, TPropType>> func)
        {
            typeName = ((MemberExpression) func.Body).Member.Name;
            this.printConfig = printConfig;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> serializationFunc)
        {
            (printConfig as IPrintingConfig).PropertySerializer.Add(typeName, serializationFunc);
            return printConfig;
        }
    }
}
