using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class Printer<TOwner>
    {
        private readonly HashSet<ICollection> serializedCollections;

        private readonly PrintingConfig<TOwner> config;
        public Printer(PrintingConfig<TOwner> config)
        {
            this.config = config;
            serializedCollections = new HashSet<ICollection>();
        }

        public string PrintToString(object printable, int nestingLevel)
        {
            if (printable == null)
                return "null" + Environment.NewLine;

            var printableType = printable.GetType();
            if (TypeHaveDifferentSerialization(printableType, out var typeConfig))
                return typeConfig.Func.Invoke(printable) + Environment.NewLine;

            if (config.finalTypes.Contains(printableType))
                return printable + Environment.NewLine;


            if (config.PrintedObjects.Contains(printable))
                return "Cyclic reference found with " + printable.GetType().Name + Environment.NewLine;

            config.PrintedObjects.Add(printable);
            return ToStringComplexObject(printable, nestingLevel);
        }

        private bool TypeHaveDifferentSerialization(Type printableType, out TypeConfig<TOwner> typeConfig)
        {
            return config.typeConfigs.TryGetValue(printableType, out typeConfig) && typeConfig.Func != null;
        }

        private string ToStringComplexObject(object printable, int nestingLevel)
        {
            var printableType = printable.GetType();
            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder(printableType.Name + Environment.NewLine);
            foreach (var propertyInfo in printableType.GetProperties()
                .Where(x => !config.excludedTypes.Contains(x.PropertyType))
                .Where(x => !config.excludedPropertyNames.Contains(x.Name)))
            {
                if (PropertyHaveDifferentSerialization(propertyInfo, out var propertyConfig))
                {
                    sb.Append(ApplyAlternativeSerialization(printable, identation, propertyInfo, propertyConfig));
                }
                else
                {
                    sb.Append(ApplyDefaultSerialization(printable, nestingLevel, identation, propertyInfo));
                }
            }

            return sb.ToString();
        }

        private string ApplyDefaultSerialization(object printable, int nestingLevel, string identation, PropertyInfo propertyInfo)
        {
            var SB = new StringBuilder();
            if (printable is ICollection)
            {
                SB.Append(ApplyCollectionSerialization(printable, nestingLevel + 1));
            }
            else
            {
                SB.Append(identation + propertyInfo.Name + " = " +
                                                  PrintToString(propertyInfo.GetValue(printable, null), nestingLevel + 1));
            }
            return SB.ToString();
        }

        private string ApplyCollectionSerialization(object printable, int nestingLevel)
        {
            if (serializedCollections.Contains(printable))
            {
                return "";
            }

            var identation = new string('\t', nestingLevel);
            var SB = new StringBuilder();
            SB.Append(identation + "Elements:" + Environment.NewLine);
            foreach (var e in (ICollection)printable)
            {
                SB.Append(identation + "\t" + PrintToString(e, nestingLevel + 1));
            }
            serializedCollections.Add((ICollection)printable);
            return SB.ToString();
        }

        private static string ApplyAlternativeSerialization(object printable, string identation, PropertyInfo propertyInfo, IPropertyConfig propertyConfig)
        {
            return identation + propertyInfo.Name + " = " +
                                        propertyConfig.Func.Invoke(propertyInfo.GetValue(printable)) + Environment.NewLine;
        }

        private bool PropertyHaveDifferentSerialization(PropertyInfo propertyInfo, out IPropertyConfig propertyConfig)
        {
            return config.propertyConfigs.TryGetValue(propertyInfo.Name, out propertyConfig) && propertyConfig.Func != null;
        }
    }
}
