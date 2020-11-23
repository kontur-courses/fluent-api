using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public static class ObjectPrinter
    {
        public static string PrintToString<TOwner>(TOwner obj,
            Func<PrintingConfig<TOwner>, PrintingConfig<TOwner>> config)
        {
            return PrintToString(obj, config(new PrintingConfig<TOwner>()), 0);
        }

        private static string PrintToString<TOwner>(object obj, PrintingConfig<TOwner> config, int nestingLevel)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            var type = obj.GetType();
            if (type.IsPrimitive || config.FinalTypes.Contains(type))
                return GetSerializedObject(obj, config);
            if (config.VisitedObjects.Contains(obj))
                return "cycle" + Environment.NewLine;
            if (type.IsClass)
                config.VisitedObjects.Add(obj);

            return typeof(ICollection).IsAssignableFrom(type)
                ? GetSerializedCollection(obj, nestingLevel, config)
                : GetSerializedMembers(obj, nestingLevel, config);
        }

        private static string GetSerializedMembers<TOwner>(object obj, int nestingLevel, PrintingConfig<TOwner> config)
        {
            var resultString = new StringBuilder().AppendLine(obj.GetType().Name);
            var currentIndentation = string.Concat(Enumerable.Repeat(config.Indentation, nestingLevel + 1));

            foreach (var member in obj.GetType().GetProperties().Cast<MemberInfo>()
                .Concat(obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public))
                .Where(x => !config.ExcludingMembers.Contains(x)
                            && (x is PropertyInfo propertyInfo &&
                                !config.ExcludingTypes.Contains(propertyInfo.PropertyType)
                                || x is FieldInfo fieldInfo && !config.ExcludingTypes.Contains(fieldInfo.FieldType))))
                resultString.Append(currentIndentation + member.Name + $" {config.SeparatorBetweenNameAndValue} " +
                                    GetSerializedMember(member, obj, nestingLevel, config));

            return resultString.ToString();
        }

        private static string GetSerializedCollection<TOwner>(object obj, int nestingLevel,
            PrintingConfig<TOwner> config)
        {
            var resultString = new StringBuilder().AppendLine(obj.GetType().Name);
            var currentIndentation = string.Concat(Enumerable.Repeat(config.Indentation, nestingLevel + 1));

            foreach (var e in (IEnumerable) obj)
                resultString.Append(currentIndentation + PrintToString(e, config, nestingLevel + 1));
            return resultString.ToString();
        }

        private static string GetSerializedObject<TOwner>(object obj, PrintingConfig<TOwner> config)
        {
            return config.CultureTypes.ContainsKey(obj.GetType())
                ? string.Format(config.CultureTypes[obj.GetType()], "{0}" + Environment.NewLine, obj)
                : obj + Environment.NewLine;
        }

        private static string GetSerializedMember<TOwner>(MemberInfo member, object obj, int nestingLevel,
            PrintingConfig<TOwner> config)
        {
            var memberValue = member is PropertyInfo info
                ? info.GetValue(obj)
                : ((FieldInfo) member).GetValue(obj);
            var memberType = member is PropertyInfo propertyInfo
                ? propertyInfo.PropertyType
                : ((FieldInfo) member).FieldType;

            if (config.PrintingFunctionsForMembers.ContainsKey(member))
                return config.PrintingFunctionsForMembers[member].DynamicInvoke(memberValue)?.ToString();
            if (config.PrintingFunctionsForTypes.ContainsKey(memberType))
                return config.PrintingFunctionsForTypes[memberType].DynamicInvoke(memberValue)?.ToString();
            return PrintToString(memberValue, config, nestingLevel + 1);
        }
    }
}