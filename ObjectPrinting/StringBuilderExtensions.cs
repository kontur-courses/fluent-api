//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace ObjectPrinting
//{
//    public static class StringBuilderExtensions
//    {
//        public static void AppendObjectProperties(this StringBuilder sb, object printable, PrintingConfig)
//        {
//            var printableType = printable.GetType();
//            foreach (var propertyInfo in printableType.GetProperties())
//            {
//                if (!PrintingConfig.excludedTypes.Contains(propertyInfo.PropertyType))
//                {
//                    if (propertyConfigs.TryGetValue(propertyInfo.Name, out var propertyConfig) && propertyConfig.Func != null)
//                    {
//                        sb.Append(identation + propertyInfo.Name + " = " +
//                                propertyConfig.Func.Invoke(propertyInfo.GetValue(printable)) + Environment.NewLine);
//                    }
//                    else if (!excludedPropertyNames.Contains(propertyInfo.Name))
//                    {
//                        sb.Append(identation + propertyInfo.Name + " = " +
//                              PrintToString(propertyInfo.GetValue(printable), nestingLevel + 1));
//                    }
//                }
//            }
//        }
//    }
//}
