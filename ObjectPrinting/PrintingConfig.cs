using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly HashSet<Type> excludedTypes = new HashSet<Type>();
        private readonly HashSet<MemberInfo> excludedMembers = new HashSet<MemberInfo>();
        internal readonly Dictionary<Type, Func<object, string>> TypeCustomPrintings =
            new Dictionary<Type, Func<object, string>>();
        internal readonly Dictionary<MemberInfo, Func<object, string>> MemberCustomPrinting =
            new Dictionary<MemberInfo, Func<object, string>>();
        
        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var member  = GetMemberInfo(memberSelector);
            return new PropertyPrintingConfig<TOwner, TPropType>(this, member);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var member  = GetMemberInfo(memberSelector);
            excludedMembers.Add(member);
            return this;
        }

        private static MemberInfo GetMemberInfo<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var expression = (MemberExpression)memberSelector.Body;
            return expression.Member;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            excludedTypes.Add(typeof(TPropType));
            return this;
        }

        public string PrintToString(TOwner obj)
        {
            var sb = new StringBuilder();
            PrintToString(obj, 0, new Dictionary<Type, Dictionary<object, int>>(), sb);
            return sb.ToString();
        }

        private void PrintToString(object obj, int nestingLevel, 
            Dictionary<Type, Dictionary<object, int>> printedObjects, StringBuilder currentPrint)
        {
            if (obj is null)
            {
                currentPrint.Append("null");
                return;
            }
            var type = obj.GetType();
            if (TypeCustomPrintings.ContainsKey(type))
            {
                currentPrint.Append(TypeCustomPrintings[type](obj));
                return;
            }
            if (!(obj is string) && obj is IEnumerable collection)
            {
                PrintCollection(collection, nestingLevel, printedObjects, currentPrint);
                return;
            }
            if (!(obj is string) && GetAllMembers(type, obj).Any())
            {
                PrintClassWithFields(obj, nestingLevel, printedObjects, currentPrint);
                return;
            }
            currentPrint.Append(obj);
        }

        private void PrintCollection(IEnumerable collection, int nestingLevel,
            Dictionary<Type, Dictionary<object, int>> printedObjects, StringBuilder currentPrint)
        {
            var indentation = new string('\t', nestingLevel + 1);
            currentPrint.AppendLine("{");
            var first = true;
            foreach (var element in collection)
            {
                if (first) first = false;
                else currentPrint.AppendLine(",");
                currentPrint.Append(indentation);
                PrintToString(element, nestingLevel + 1, printedObjects, currentPrint);
            }
            currentPrint.Append("}");
        }

        private void PrintClassWithFields(object obj, int nestingLevel, 
            Dictionary<Type, Dictionary<object, int>> printedObjects, StringBuilder currentPrint)
        {
            var indentation = new string('\t', nestingLevel + 1);
            var type = obj.GetType();
            if (!obj.GetType().IsValueType)
            {
                if (!printedObjects.ContainsKey(type))
                    printedObjects[type] = new Dictionary<object, int>();
                if (printedObjects[type].ContainsKey(obj))
                {
                    currentPrint.Append(type.Name).Append(" ").Append(printedObjects[type][obj].ToString());
                    return;
                }
                printedObjects[type][obj] = printedObjects[type].Count;
                currentPrint.Append(type.Name).Append(" ").AppendLine(printedObjects[type][obj].ToString());
            }
            else
                currentPrint.AppendLine(type.Name);

            var first = true;
            foreach (var (member, value) in GetAllMembers(type, obj))
            {
                if (first) first = false;
                else currentPrint.AppendLine();
                currentPrint.Append(indentation + member.Name + " = ");
                if (MemberCustomPrinting.ContainsKey(member))
                    currentPrint.Append(MemberCustomPrinting[member](value));
                else 
                    PrintToString(value, nestingLevel + 1, printedObjects, currentPrint);
            }
        }

        private IEnumerable<(MemberInfo member,  object value)> GetAllMembers(Type type, object obj) =>
            type.GetProperties()
                .Where(p => !p.GetAccessors().Any(x => x.IsStatic) && !excludedTypes.Contains(p.PropertyType))
                .Select(p => ((MemberInfo)p,  p.GetValue(obj)))
                .Concat(type.GetFields().Where(f => !f.IsStatic && f.IsPublic && !excludedTypes.Contains(f.FieldType))
                    .Select(f => ((MemberInfo)f,  f.GetValue(obj))))
                .Where(m =>  !excludedMembers.Contains(m.Item1));
    }
}