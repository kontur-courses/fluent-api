using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner> : IPrintingConfig<TOwner>
    {
        private readonly HashSet<Type> excludedMemberTypes = new HashSet<Type>();
        private readonly HashSet<string> excludedMemberNames = new HashSet<string>();
        private readonly Dictionary<object, int> usedObjects = new Dictionary<object, int>();
        private readonly Dictionary<string, int> membersToTrim = new Dictionary<string, int>();
        private readonly Dictionary<Type, CultureInfo> typesWithCulture = new Dictionary<Type, CultureInfo>();
        private readonly Dictionary<Type, Delegate> typesWithSpecificPrint = new Dictionary<Type, Delegate>();
        private readonly Dictionary<string, Delegate> namesWithSpecificPrint = new Dictionary<string, Delegate>();
        private readonly char indent;

        public PrintingConfig(char indent = '\t')
        {
            this.indent = indent;
        }

        public MemberPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new MemberPrintingConfig<TOwner, TPropType>(this);
        }

        public MemberPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var propertyName = ((MemberExpression)memberSelector.Body).Member.Name;
            return new MemberPrintingConfig<TOwner, TPropType>(this, propertyName);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            excludedMemberNames.Add(((MemberExpression)memberSelector.Body).Member.Name);
            return this;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            excludedMemberTypes.Add(typeof(TPropType));
            return this;
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            foreach (var e in usedObjects.Where(kvp => kvp.Value >= nestingLevel).ToList())
                usedObjects.Remove(e.Key);

            if (obj == null)
                return "null" + Environment.NewLine;

            if (obj.IsSimpleType())
                return obj + Environment.NewLine;

            var indentation = new string(indent, nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);

            if (obj is ICollection collection)
                return PrintCollection(collection, sb, nestingLevel);

            if (usedObjects.ContainsKey(obj))
            {
                if (usedObjects[obj] != nestingLevel)
                {
                    sb.Append($"{indentation}...{Environment.NewLine}");
                    return sb.ToString();
                }
            }
            else
                usedObjects.Add(obj, nestingLevel);

            return PrintMembers(sb, obj, nestingLevel);
        }

        private string GetIndent(int nestingLevel)
        {
            return new string(indent, nestingLevel);
        }

        private string PrintMembers(StringBuilder sb, object obj, int nestingLevel)
        {
            var propertyInfos = obj.GetType().GetProperties().Where(p => !excludedMemberTypes.Contains(p.PropertyType)
                                                                && !excludedMemberNames.Contains(p.Name));
            var fieldInfos = obj.GetType().GetFields().Where(f => !excludedMemberTypes.Contains(f.FieldType)
                                                         && !excludedMemberNames.Contains(f.Name));

            var memberInfos = propertyInfos.Cast<MemberInfo>().Concat(fieldInfos);


            foreach (var memberInfo in memberInfos)
            {
                var memberValue = GetMemberValue(memberInfo, obj);

                if (memberValue.IsSimpleType())
                    memberValue = ApplyConfig(memberValue, memberInfo.Name);
                sb.Append($"{GetIndent(nestingLevel + 1)}{memberInfo.Name} = " +
                          PrintToString(memberValue, nestingLevel + 1));
            }
            return sb.ToString();
        }

        private string PrintCollection(ICollection collection, StringBuilder sb, int nestingLevel)
        {
            var count = 0;
            var indentation = new string(indent, nestingLevel + 1);
            foreach (var element in collection)
            {
                if (count > 2)
                {
                    sb.Append($"{indentation}...{Environment.NewLine}");
                    break;
                }

                var elementToString = element.GetType().Namespace == nameof(System) ? element : element.GetType().Name;
                sb.Append($"{indentation}[{ count}] = {elementToString}{Environment.NewLine}");
                count++;
            }

            return sb.ToString();
        }

        private object GetMemberValue(MemberInfo memberInfo, object obj)
        {
            if (memberInfo is PropertyInfo propertyInfo)
                return propertyInfo.GetValue(obj);
            if (memberInfo is FieldInfo fieldInfo)
                return fieldInfo.GetValue(obj);
            throw new ArgumentException("Member is not Field or Property");
        }

        private string Trim(string memberName, string memberValue)
        {
            if (memberValue == null)
                return null;
            if (membersToTrim.ContainsKey(memberName))
            {
                var requiredLength = membersToTrim[memberName];
                var resultLength = Math.Min(requiredLength, memberValue.Length);
                return memberValue.Substring(0, resultLength);
            }

            return memberValue;
        }

        private object ApplyCulture(object obj)
        {
            if (typesWithCulture.ContainsKey(obj.GetType()))
                obj = string.Format(typesWithCulture[obj.GetType()], "{0}", obj);
            return obj;
        }

        private object ApplySpecificPrint(object obj, string memberName)
        {
            if (namesWithSpecificPrint.ContainsKey(memberName))
            {
                var func = namesWithSpecificPrint[memberName];
                return func.DynamicInvoke(obj);
            }

            if (typesWithSpecificPrint.ContainsKey(obj.GetType()))
            {
                var func = typesWithSpecificPrint[obj.GetType()];
                return func.DynamicInvoke(obj);
            }

            return obj;
        }

        private string ApplyConfig(object obj, string name)
        {
            if (obj == null)
                return "null";
            object result;
            result = ApplySpecificPrint(obj, name);
            result = ApplyCulture(result);
            result = Trim(name, result.ToString());

            return result.ToString();
        }

        Dictionary<string, int> IPrintingConfig<TOwner>.PropertiesToTrim => membersToTrim;
        Dictionary<Type, CultureInfo> IPrintingConfig<TOwner>.TypesWithCulture => typesWithCulture;
        Dictionary<Type, Delegate> IPrintingConfig<TOwner>.TypesWithSpecificPrint => typesWithSpecificPrint;
        Dictionary<string, Delegate> IPrintingConfig<TOwner>.NamesWithSpecificPrint => namesWithSpecificPrint;
    }

    public interface IPrintingConfig<TOwner>
    {
        Dictionary<string, int> PropertiesToTrim { get; }
        Dictionary<Type, CultureInfo> TypesWithCulture { get; }
        Dictionary<Type, Delegate> TypesWithSpecificPrint { get; }
        Dictionary<string, Delegate> NamesWithSpecificPrint { get; }
    }
}