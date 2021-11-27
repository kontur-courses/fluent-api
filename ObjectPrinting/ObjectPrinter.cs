using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text;
using ObjectPrinting.Contexts;
using ObjectPrinting.PrintingMembers;

namespace ObjectPrinting
{
    public static class ObjectPrinter
    {
        public static ConfigPrintingContext<T> For<T>() => new(new PrintingConfig
        {
            FinalTypes = ImmutableList.CreateRange(new[]
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan), typeof(Guid)
            })
        });
    }

    public class ObjectPrinter<TOwner>
    {
        private readonly PrintingConfig config;

        public ObjectPrinter(PrintingConfig config)
        {
            this.config = config;
        }

        public string PrintToString(TOwner obj) => BuildObjectString(obj, 0).ToString();

        private StringBuilder BuildObjectString(object obj, int nestingLevel) =>
            TryGetTypeString(obj, out var typeString)
                ? new StringBuilder(typeString)
                : BuildComplexObjectString(obj, nestingLevel);

        private StringBuilder BuildComplexObjectString(object obj, int nestingLevel)
        {
            var builder = new StringBuilder();
            var type = obj.GetType();
            builder.AppendLine(type.Name);

            builder.Append(GetPropertiesBuilder(obj, nestingLevel));

            return builder;
        }

        private StringBuilder GetPropertiesBuilder(object obj, int nestingLevel)
        {
            var builder = new StringBuilder();
            var indentation = new string('\t', nestingLevel + 1);
            foreach (var objMember in GetSuitableObjectMembers(obj))
            {
                builder.Append(indentation);
                builder.Append(BuildMemberString(obj, objMember, nestingLevel));
            }

            return builder;
        }

        private IEnumerable<PrintingMember> GetSuitableObjectMembers(object obj)
        {
            var type = obj.GetType();
            var members = type.GetMembers()
                .Where(config.PrintingMemberFactory.CanConvert)
                .Select(config.PrintingMemberFactory.Convert);

            return members
                .Where(objMember => !config.ExcludingTypes.Contains(objMember.Type))
                .Where(objMember => !config.ExcludingMembers.Contains(objMember.MemberInfo));
        }

        private StringBuilder BuildMemberString(object obj, PrintingMember objMember, int nestingLevel)
        {
            if (TryGetMemberString(objMember.MemberInfo, out var propString))
                return new StringBuilder(propString);

            var builder = new StringBuilder(objMember.Name + " = ");
            builder.Append(BuildValueString(obj, objMember, nestingLevel));
            return builder;
        }

        private bool TryGetMemberString(MemberInfo propertyInfo, out string propString)
        {
            if (config.MemberPrinting.TryGetValue(propertyInfo, out var printProperty))
            {
                propString = printProperty(propertyInfo) + Environment.NewLine;
                return true;
            }

            propString = null;
            return false;
        }

        private StringBuilder BuildValueString(object obj, PrintingMember objMember, int nestingLevel)
        {
            if (TryGetTypeString(obj, out var typeString))
                return new StringBuilder(typeString);

            if (nestingLevel >= 100)
                return new StringBuilder("...");

            return BuildObjectString(objMember.GetValue(obj), nestingLevel + 1);
        }

        private bool TryGetTypeString(object obj, out string typeString)
        {
            typeString = null;

            if (obj == null)
                typeString = "null";
            else if (!TrySerializeType(obj, obj.GetType(), out typeString))
                return false;

            typeString += Environment.NewLine;
            return true;
        }

        private bool TrySerializeType(object obj, Type type, out string typeString)
        {
            typeString = null;
            if (config.TypePrinting.TryGetValue(type, out var printType))
                typeString = printType(obj);
            else if (config.FinalTypes.Contains(type))
                typeString = obj.ToString();

            return typeString != null;
        }
    }
}