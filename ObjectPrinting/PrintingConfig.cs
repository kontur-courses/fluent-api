using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner> : IPrintingConfig<TOwner>
    {
        private readonly HashSet<Type> FinalTypes = new()
        {
            typeof(int),
            typeof(double),
            typeof(float),
            typeof(string),
            typeof(DateTime),
            typeof(TimeSpan)
        };
        
        private readonly HashSet<Type> excludedTypes = new();
        private readonly HashSet<MemberInfo> excludedMembers = new();
        private readonly Dictionary<Type, Delegate> typesSerializers = new();
        private readonly Dictionary<MemberInfo, Delegate> membersSerializers = new();
        private readonly HashSet<object> visited = new();
        private bool checkCyclicReference = false;

        public IPrintingConfig<TOwner> Exclude<TExcluding>()
        {
            excludedTypes.Add(typeof(TExcluding));
            return this;
        }

        public IPrintingConfig<TOwner> Exclude<TExcluding>(Expression<Func<TOwner, TExcluding>> memberSelector)
        {
            var memberExpression = (MemberExpression) memberSelector.Body;
            excludedMembers.Add(memberExpression.Member);
            return this;
        }

        public IMemberPrintingConfig<TOwner, TMember> Printing<TMember>()
        {
            return new MemberPrintingConfig<TMember>(this);
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        public IPrintingConfig<TOwner> ThrowIfCyclicReferences()
        {
            checkCyclicReference = true;
            return this;
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            if (typesSerializers.ContainsKey(obj.GetType()))
                return (string) typesSerializers[obj.GetType()].DynamicInvoke(obj) + Environment.NewLine;
            
            if (FinalTypes.Contains(obj.GetType()))
                return obj + Environment.NewLine;
            
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);

            if (visited.Contains(obj))
            {
                if (checkCyclicReference)
                {
                    throw new ArgumentException($"Cyclic reference on {type.Name}");
                }
                
                return $"Cyclic reference on {type.Name}" + Environment.NewLine;
            }
            
            visited.Add(obj);
            
            var indentation = new string('\t', nestingLevel + 1);
            
            switch (obj)
            {
                case IDictionary dictionary:
                    sb.Append(PrintDictionary(dictionary, nestingLevel + 1));
                    break;
                case IList list:
                    sb.Append(PrintList(list, nestingLevel + 1));
                    break;
                default:
                {
                    foreach (var memberInfo in GetTypeFieldsAndProperties(type))
                    {
                        if (TryPrintFieldOrProperty(obj, memberInfo, nestingLevel, out var printing))
                            sb.Append(indentation + memberInfo.Name + " = " + printing);
                    }

                    break;
                }
            }

            visited.Remove(obj);
            
            return sb.ToString();
        }

        private string PrintList(IList list,
            int nestingLevel)
        {
            var result = new StringBuilder();
            var indentation = new string('\t', nestingLevel + 1);
            
            for (var i = 0; i < list.Count; ++i)
            {
                result
                    .Append(indentation)
                    .Append(i.ToString())
                    .Append(": ")
                    .Append(PrintToString(list[i], nestingLevel + 1));
            }
        
            return result.ToString();
        }
        
        private string PrintDictionary(IDictionary dictionary,
            int nestingLevel)
        {
            var result = new StringBuilder();
            var indentation = new string('\t', nestingLevel + 1);

            foreach (var key in dictionary.Keys)
            {
                result.Append(indentation)
                    .Append("Key: ")
                    .Append(Environment.NewLine)
                    .Append(indentation + "\t")
                    .Append(PrintToString(key, nestingLevel + 1))
                    .Append(indentation)
                    .Append("Value: ")
                    .Append(Environment.NewLine)
                    .Append(indentation + "\t")
                    .Append(PrintToString(dictionary[key], nestingLevel + 1));
            }
        
            return result.ToString();
        }

        private object GetObjectMemberValue(object obj, MemberInfo memberInfo) =>
            memberInfo.MemberType switch
            {
                MemberTypes.Field => ((FieldInfo) memberInfo).GetValue(obj),
                MemberTypes.Property => ((PropertyInfo) memberInfo).GetValue(obj),
                _ => throw new ArgumentException("Unexpected argument")
            };

        private Type GetMemberType(MemberInfo memberInfo) =>
            memberInfo.MemberType switch
            {
                MemberTypes.Field => ((FieldInfo) memberInfo).FieldType,
                MemberTypes.Property => ((PropertyInfo) memberInfo).PropertyType,
                _ => throw new ArgumentException("Unexpected argument")
            };

        private bool TryPrintFieldOrProperty(object obj, MemberInfo memberInfo,
            int nestingLevel, out string printing)
        {
            printing = null;
            var type = GetMemberType(memberInfo);
            
            if (excludedMembers.Contains(memberInfo) || excludedTypes.Contains(type)) return false;

            var value = GetObjectMemberValue(obj, memberInfo);

            if (value is null)
            {
                printing = "null" + Environment.NewLine;
                return true;
            }

            printing = PrintToString(membersSerializers.ContainsKey(memberInfo) ?
            membersSerializers[memberInfo].DynamicInvoke(value) : value, nestingLevel + 1);

            return true;
        }

        private static IEnumerable<MemberInfo> GetTypeFieldsAndProperties(Type type)
        {
            var bindingFlags = BindingFlags.Public | BindingFlags.Instance;
            
            return type.GetFields(bindingFlags)
                .Cast<MemberInfo>()
                .Concat(type.GetProperties(bindingFlags));
        }
        
        private class MemberPrintingConfig<TMember> : IMemberPrintingConfig<TOwner, TMember>
        {
            private readonly PrintingConfig<TOwner> parentConfig;

            public MemberPrintingConfig(PrintingConfig<TOwner> parentConfig)
            {
                this.parentConfig = parentConfig;
            }

            public IPrintingConfig<TOwner> Using(Func<TMember, string> alternativeSerializer)
            {
                parentConfig.typesSerializers[typeof(TMember)] = alternativeSerializer;

                return parentConfig;
            }

            public IPrintingConfig<TOwner> Using(Expression<Func<TOwner, TMember>> memberSelector,
                Func<TMember, string> alternativeSerializer)
            {
                var memberExpr = (MemberExpression) memberSelector.Body;
                parentConfig.membersSerializers[memberExpr.Member] = alternativeSerializer;

                return parentConfig;
            }
        }
    }
}