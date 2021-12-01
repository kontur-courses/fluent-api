using System;
using System.Collections;
using System.Collections.Generic;
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

        public string PrintToString(object obj, int nestingLevel = 0)
        {
            if (obj == null)
                return "null" + Environment.NewLine;
            var type = obj.GetType();
            return GetSerializedObject(obj, nestingLevel, type);
        }

        private string GetSerializedObject(object obj, int nestingLevel, Type type)
        {
            var identation = new string('\t', nestingLevel + 1);
            
            if (finalTypes.Contains(type))
                return GetLineWithEnd(obj.ToString());

            var sb = new StringBuilder();
            sb.AppendLine(type.Name);
            if (complexObjects.Contains(obj))
                return GetLineWithEnd("The reference is cyclical");
            complexObjects.Add(obj);

            if (obj is IEnumerable enumerable)
                AppendEnumerable(enumerable, nestingLevel, identation, sb);
            else
                AppendProperties(obj, nestingLevel, type, identation, sb);
            return sb.ToString();
        }
        
        private void AppendEnumerable
            (IEnumerable enumerable, int nestingLevel, string identation, StringBuilder sb)
        {
            var count = 0;
            foreach (var element in enumerable)
            {
                count++;
                var serialized = GetSerializedObject(element, nestingLevel + 2, element.GetType());
                sb.Append(identation);
                sb.Append(serialized);
                if (count <= 100) 
                    continue;
                sb.Append(GetLineWithEnd
                    ("IEnumerable probably endless! Serializing for this enumerable stopped."));
                return;
            }
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
            sb.Append($"{identation}{memberInfo.Name} = ");
            if (config.AlternativeMemberSerializations.ContainsKey(memberInfo))
                sb.Append(config.AlternativeMemberSerializations[memberInfo]
                    .Invoke(value) + Environment.NewLine);
            else
                sb.Append(PrintToString(value, nestingLevel + 1));
        }

        private string GetLineWithEnd(string serialized)
            => $"{serialized}{Environment.NewLine}";
    }
}