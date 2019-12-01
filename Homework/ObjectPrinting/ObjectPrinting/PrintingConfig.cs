using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner> : PrintingConfigBase
    {
        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            return this;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            return this;
        }

        public string PrintToString(TOwner printedObject)
        {
            return PrintToString(printedObject, 0);
        }

        private string PrintToString(object printedObject, int nestingLevel)
        {
            //TODO apply configurations
            if (printedObject is null)
                return NullRepresentation + Environment.NewLine;

            var objectRuntimeType = printedObject.GetType();
            
            if (FinalTypes.Contains(objectRuntimeType))
                return printedObject + Environment.NewLine;
            
            var objectRepresentationBuilder = new StringBuilder();
            
            objectRepresentationBuilder.AppendLine(objectRuntimeType.Name);
            
            var indentation = new string(Indentation, nestingLevel + 1);
            
            foreach (var propertyInfo in objectRuntimeType.GetProperties())
            {
                var propertyObjectRepresentation = PrintToString(propertyInfo.GetValue(printedObject),
                                                                 nestingLevel + 1);
                var propertyRepresentation = string.Concat(
                    indentation, 
                    propertyInfo.Name, 
                    " = ", 
                    propertyObjectRepresentation);
                
                objectRepresentationBuilder.Append(propertyRepresentation);
            }
            
            return objectRepresentationBuilder.ToString();
        }
    }
}