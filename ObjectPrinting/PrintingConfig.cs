using System;
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

            var indentation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);

            if (checkCyclicReference) CheckForCyclicReference(obj);
            
            foreach (var memberInfo in GetTypeFieldsAndProperties(type))
            {
                if (excludedMembers.Contains(memberInfo)) continue;
                
                var nameValueType = GetMemberNameValueType(obj, memberInfo);
                
                if (excludedTypes.Contains(nameValueType.type)) continue;

                if (membersSerializers.ContainsKey(memberInfo))
                {
                    sb.Append(PrintFieldOrProperty(nameValueType.name, 
                        membersSerializers[memberInfo].DynamicInvoke(nameValueType.value),
                              indentation, nestingLevel));
                }
                else
                {
                    sb.Append(PrintFieldOrProperty(nameValueType.name,
                        nameValueType.value,
                        indentation, nestingLevel));
                }
            }
            return sb.ToString();
        }

        private void CheckForCyclicReference(object obj)
        {
            if (visited.Contains(obj))
            {
                throw new ArgumentException("Cyclic reference");
            }

            visited.Add(obj);
        }

        private (string name, object value, Type type) GetMemberNameValueType(object obj, MemberInfo memberInfo)
        {
            switch (memberInfo.MemberType)
            {
                case MemberTypes.Field:
                {
                    var fieldInfo = (FieldInfo) memberInfo;
                    return (fieldInfo.Name, fieldInfo.GetValue(obj), fieldInfo.FieldType);
                }
                case MemberTypes.Property:
                {
                    var propertyInfo = (PropertyInfo) memberInfo;
                    return (propertyInfo.Name, propertyInfo.GetValue(obj), propertyInfo.PropertyType);
                }
                default:
                    throw new ArgumentException("Unexpected argument");
            }
        }

        private string PrintFieldOrProperty(string memberName,
            object value,
            string indentation,
            int nestingLevel)
        {
            return indentation + memberName + " = " +
                   PrintToString(value,
                       nestingLevel + 1);
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