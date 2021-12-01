using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using ObjectPrinting.Configs;
using ObjectPrinting.Extensions;

namespace ObjectPrinting
{
    internal class Printer
    {
        private readonly GlobalConfig config;
        private readonly Type[] finalTypes;
        private readonly List<object> complexObjects;

        public Printer(GlobalConfig config)
        {
            this.config = config;
            finalTypes = config.FinalTypes;
            complexObjects = new List<object>();
        }

        public string PrintToString(object obj, MemberInfo member = null, int nestingLevel = 0)
        {
            if (obj == null)
                return "null" + Environment.NewLine;
            var type = obj.GetType();
            return GetSerializedObject(obj, member, nestingLevel, type);
        }

        private string GetSerializedObject(object obj, MemberInfo member, int nestingLevel, Type type)
        {
            var identation = new string('\t', nestingLevel + 1);

            if (finalTypes.Contains(type))
                return GetFinal(obj, type, member);

            var sb = new StringBuilder();
            sb.AppendLine(type.Name);
            if (complexObjects.Contains(obj))
                sb.Append(identation + "The reference is cyclical" + Environment.NewLine);
            else
            {
                complexObjects.Add(obj);
                if (obj is IEnumerable enumerable)
                    AppendCollection(enumerable, nestingLevel, identation, sb);
                else
                    AppendProperties(obj, nestingLevel, type, identation, sb);
            }
            return sb.ToString();
        }

        private string GetFinal(object obj, Type type, MemberInfo member)
        {
            if (member == null) 
                return obj + Environment.NewLine;
            var culture = config.DefaultCulture;
            if (config.Cultures.ContainsKey(member))
                return GetFinalIFormattable(obj, config.Cultures[member]);
            if (culture != null && obj is IFormattable)
                return GetFinalIFormattable(obj, culture);

            if (type == typeof(string)) 
                return GetFinalString(obj, member);
            return obj + Environment.NewLine;
        }

        private string GetFinalIFormattable(object obj, CultureInfo culture)
            => (obj as IFormattable)?.ToString(null, culture) ?? 
               throw new ArgumentException("You try to Get Final IFormattable with no IFormattable");

        private string GetFinalString(object obj, MemberInfo member)
        {
            var final = obj.ToString();
            var cutLength = config.CutLengths.ContainsKey(member) ?
                config.CutLengths[member] : config.DefaultCutLength;
            return final.Substring(0, cutLength > final.Length ? final.Length : cutLength)
                   + Environment.NewLine;
        }

        private void AppendCollection
            (IEnumerable collection, int nestingLevel, string identation, StringBuilder sb)
        {
            foreach (var element in collection)
                sb.Append(identation + "\t" +
                          GetSerializedObject(element, null, nestingLevel + 2, element.GetType()));
        }

        private void AppendProperties(object obj,
            int nestingLevel, 
            Type type, 
            string identation,
            StringBuilder sb)
        {
            foreach (var memberInfo in type.GetSerializedMembers()
                .Where(m => !config.ExcludedMembers.Contains(m)))
            {
                AppendMember(obj, nestingLevel, identation, sb, memberInfo);
            }
        }

        private void AppendMember(object obj,
            int nestingLevel,
            string identation,
            StringBuilder sb,
            MemberInfo memberInfo)
        {
            var value = memberInfo.GetValue(obj);
            sb.Append(identation + memberInfo.Name + " = ");
            if (config.AlternativeMemberSerializations.ContainsKey(memberInfo))
                sb.Append(config.AlternativeMemberSerializations[memberInfo]
                    .Invoke(value) + Environment.NewLine);
            else
                sb.Append(PrintToString(value, memberInfo, nestingLevel + 1));
        }
    }
}